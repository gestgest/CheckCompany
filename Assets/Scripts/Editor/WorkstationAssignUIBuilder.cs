#if UNITY_EDITOR

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 직원 배정 UI를 손으로 조립하지 않고 메뉴 한 번으로 만들어준다.
///
/// 씬이 둘로 나뉘어 있어서(UI는 GamePlay, 배치 시스템은 MyCompany/DebugCompany) 메뉴도 둘이다.
/// 두 메뉴 모두 여러 번 눌러도 안전하다 - 이미 있으면 새로 만들지 않고 참조만 다시 이어준다.
/// </summary>
public static class WorkstationAssignUIBuilder
{
    private const string TapEventPath = "Assets/ScriptableObjects/Event/Object/TapPlaceableObjectEventSO.asset";
    private const string ElementPrefabPath = "Assets/Prefab/UI/WorkstationEmployeeElement.prefab";

    [MenuItem("Tools/CheckCompany/1. 직원 배정 팝업 만들기 (GamePlay 씬)")]
    private static void BuildPopup()
    {
        PlaceableObjectEventChannelSO tapEvent = GetOrCreateTapEvent();
        GameObject elementPrefab = GetOrCreateElementPrefab();

        //팝업을 어디에 붙일지. DeleteConfirmPopup 옆에 두면 캔버스/정렬 순서가 자동으로 맞는다.
        DeleteConfirmPopup sibling = FindInScene<DeleteConfirmPopup>();
        Transform parent;
        BoolEventChannelSO isHandlingEvent = null;

        if (sibling != null)
        {
            parent = sibling.transform.parent;

            //PlaceSystem이 쓰는 것과 반드시 같은 채널이어야 한다. 옆집 것을 그대로 베낀다.
            SerializedObject so = new SerializedObject(sibling);
            isHandlingEvent = so.FindProperty("_isHandlingEvent").objectReferenceValue as BoolEventChannelSO;
        }
        else
        {
            Canvas canvas = FindInScene<Canvas>();

            if (canvas == null)
            {
                Debug.LogError("[배정UI] 씬에서 Canvas를 찾지 못했습니다. GamePlay 씬을 열고 다시 실행하세요.");
                return;
            }

            parent = canvas.transform;
            Debug.LogWarning("[배정UI] DeleteConfirmPopup을 못 찾아 _isHandlingEvent는 비워둡니다. 인스펙터에서 직접 넣어주세요.");
        }

        WorkstationAssignPopup existing = FindInScene<WorkstationAssignPopup>();
        GameObject popupObject;

        if (existing != null)
        {
            popupObject = existing.gameObject;
            Debug.Log("[배정UI] 이미 있는 팝업을 찾아 참조만 다시 잇습니다.");
        }
        else
        {
            popupObject = CreatePopupHierarchy(parent);
        }

        WirePopup(popupObject, tapEvent, elementPrefab, isHandlingEvent);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = popupObject;

        Debug.Log("[배정UI] 완료. 씬을 저장(Ctrl+S)하세요.", popupObject);
    }

    [MenuItem("Tools/CheckCompany/2. 탭 입력 연결하기 (MyCompany / DebugCompany 씬)")]
    private static void WireInput()
    {
        PlaceSystem placeSystem = FindInScene<PlaceSystem>();

        if (placeSystem == null)
        {
            Debug.LogError("[배정UI] 씬에서 PlaceSystem을 찾지 못했습니다. MyCompany 또는 DebugCompany 씬을 열고 실행하세요.");
            return;
        }

        //RequireComponent가 붙어 있어도, 이미 저장된 씬에는 컴포넌트가 없을 수 있다
        PlacedObjectInput input = placeSystem.GetComponent<PlacedObjectInput>();

        if (input == null)
        {
            input = Undo.AddComponent<PlacedObjectInput>(placeSystem.gameObject);
        }

        SerializedObject so = new SerializedObject(input);
        so.FindProperty("_tapEvent").objectReferenceValue = GetOrCreateTapEvent();
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = placeSystem.gameObject;

        Debug.Log($"[배정UI] '{placeSystem.name}'에 PlacedObjectInput 연결 완료. 씬을 저장(Ctrl+S)하세요.", placeSystem);
    }

    #region 팝업 조립

    private static GameObject CreatePopupHierarchy(Transform parent)
    {
        GameObject root = NewUIObject("WorkstationAssignPopup", parent);
        Stretch(root);
        root.AddComponent<WorkstationAssignPopup>();

        //실제로 켜고 끄이는 부분. Awake의 Close()가 꺼주지만 에디터에서도 꺼둬야 시야를 안 가린다.
        GameObject dim = NewUIObject("Dim", root.transform);
        Stretch(dim);
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject window = NewUIObject("Window", dim.transform);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.anchoredPosition = Vector2.zero;
        windowRect.sizeDelta = new Vector2(800f, 1000f);
        window.AddComponent<Image>().color = Color.white;

        GameObject title = NewUIObject("Title", window.transform);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-40f, 90f);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "비어 있는 자리";
        titleText.fontSize = 48f;
        titleText.color = Color.black;
        titleText.alignment = TextAlignmentOptions.Center;

        GameObject closeButton = CreateButton("CloseButton", window.transform, "닫기", new Vector2(-150f, 40f));
        GameObject releaseButton = CreateButton("ReleaseButton", window.transform, "자리 비우기", new Vector2(150f, 40f));

        GameObject content = CreateScrollView(window.transform);

        //조립이 끝난 뒤에 끈다. 켜져 있는 동안 만들어야 레이아웃이 한 번 계산된다.
        dim.SetActive(false);

        Undo.RegisterCreatedObjectUndo(root, "직원 배정 팝업 생성");

        //WirePopup에서 이름으로 다시 찾으므로 여기서 따로 넘기지 않는다
        return root;
    }

    private static GameObject CreateScrollView(Transform parent)
    {
        GameObject scrollView = NewUIObject("ScrollView", parent);
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(20f, 110f); //아래 버튼 자리를 비운다
        scrollRect.offsetMax = new Vector2(-20f, -120f); //위 제목 자리를 비운다

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        GameObject viewport = NewUIObject("Viewport", scrollView.transform);
        Stretch(viewport);
        viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        GameObject content = NewUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 8f;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;

        return content;
    }

    private static GameObject CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject button = NewUIObject(name, parent);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(260f, 90f);

        button.AddComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f);
        button.AddComponent<Button>();

        GameObject text = NewUIObject("Text", button.transform);
        Stretch(text);
        TextMeshProUGUI tmp = text.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36f;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;

        return button;
    }

    /// <summary>인스펙터 슬롯을 채우고 버튼 OnClick을 잇는다. 이미 있던 팝업에도 그대로 쓴다.</summary>
    private static void WirePopup(
        GameObject popupObject,
        PlaceableObjectEventChannelSO tapEvent,
        GameObject elementPrefab,
        BoolEventChannelSO isHandlingEvent)
    {
        WorkstationAssignPopup popup = popupObject.GetComponent<WorkstationAssignPopup>();

        if (popup == null)
        {
            popup = popupObject.AddComponent<WorkstationAssignPopup>();
        }

        Transform dim = popupObject.transform.Find("Dim");
        Transform title = popupObject.transform.Find("Dim/Window/Title");
        Transform content = popupObject.transform.Find("Dim/Window/ScrollView/Viewport/Content");
        Transform closeButton = popupObject.transform.Find("Dim/Window/CloseButton");
        Transform releaseButton = popupObject.transform.Find("Dim/Window/ReleaseButton");

        SerializedObject so = new SerializedObject(popup);

        SetRef(so, "_root", dim != null ? dim.gameObject : null);
        SetRef(so, "_titleText", title != null ? title.GetComponent<TextMeshProUGUI>() : null);
        SetRef(so, "_elementParent", content);
        SetRef(so, "_elementPrefab", elementPrefab);
        SetRef(so, "_releaseButton", releaseButton != null ? releaseButton.gameObject : null);
        SetRef(so, "_workstationManagerSO", FindAsset<WorkstationManagerSO>());
        SetRef(so, "_employeeManagerSO", FindAsset<EmployeeManagerSO>());
        SetRef(so, "_tapEvent", tapEvent);

        if (isHandlingEvent != null)
        {
            SetRef(so, "_isHandlingEvent", isHandlingEvent);
        }

        so.ApplyModifiedProperties();

        BindClick(closeButton, popup.Close);
        BindClick(releaseButton, popup.Release);
    }

    private static void BindClick(Transform buttonTransform, UnityAction action)
    {
        if (buttonTransform == null)
        {
            return;
        }

        Button button = buttonTransform.GetComponent<Button>();

        if (button == null)
        {
            return;
        }

        //여러 번 실행해도 같은 호출이 쌓이지 않도록 비우고 새로 건다
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }

        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    #endregion

    #region 에셋

    private static PlaceableObjectEventChannelSO GetOrCreateTapEvent()
    {
        PlaceableObjectEventChannelSO existing = FindAsset<PlaceableObjectEventChannelSO>();

        if (existing != null)
        {
            return existing;
        }

        EnsureFolder(Path.GetDirectoryName(TapEventPath));

        PlaceableObjectEventChannelSO created = ScriptableObject.CreateInstance<PlaceableObjectEventChannelSO>();
        AssetDatabase.CreateAsset(created, TapEventPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[배정UI] 탭 이벤트 에셋 생성 : {TapEventPath}", created);
        return created;
    }

    private static GameObject GetOrCreateElementPrefab()
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ElementPrefabPath);

        if (existing != null)
        {
            return existing;
        }

        EnsureFolder(Path.GetDirectoryName(ElementPrefabPath));

        GameObject row = NewUIObject("WorkstationEmployeeElement", null);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(700f, 120f);
        row.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);
        row.AddComponent<Button>();

        LayoutElement layoutElement = row.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 120f;

        GameObject icon = NewUIObject("Icon", row.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(20f, 0f);
        iconRect.sizeDelta = new Vector2(90f, 90f);
        icon.AddComponent<Image>();

        GameObject nameText = NewUIObject("NameText", row.transform);
        RectTransform nameRect = nameText.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(130f, 0f);
        nameRect.offsetMax = new Vector2(-160f, 0f);
        TextMeshProUGUI tmp = nameText.AddComponent<TextMeshProUGUI>();
        tmp.text = "이름";
        tmp.fontSize = 40f;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        //"이 자리에 앉아있음"/"다른 자리에 앉아있음" 표시. 두 조건이 겹칠 일이 없어서
        //오브젝트 하나에 텍스트만 바꿔 쓴다 (WorkstationEmployeeElement.Init() 참고)
        GameObject statusMark = NewUIObject("StatusMark", row.transform);
        RectTransform markRect = statusMark.GetComponent<RectTransform>();
        markRect.anchorMin = new Vector2(1f, 0.5f);
        markRect.anchorMax = new Vector2(1f, 0.5f);
        markRect.pivot = new Vector2(1f, 0.5f);
        markRect.anchoredPosition = new Vector2(-20f, 0f);
        markRect.sizeDelta = new Vector2(120f, 60f);
        TextMeshProUGUI markText = statusMark.AddComponent<TextMeshProUGUI>();
        markText.text = "근무중";
        markText.fontSize = 30f;
        markText.color = new Color(0.1f, 0.5f, 0.1f);
        markText.alignment = TextAlignmentOptions.Center;

        WorkstationEmployeeElement element = row.AddComponent<WorkstationEmployeeElement>();

        SerializedObject so = new SerializedObject(element);
        SetRef(so, "_icon", icon.GetComponent<Image>());
        SetRef(so, "_nameText", tmp);
        SetRef(so, "_statusMark", statusMark);
        SetRef(so, "_statusMarkText", markText);
        so.ApplyModifiedProperties();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row, ElementPrefabPath);
        Object.DestroyImmediate(row);

        Debug.Log($"[배정UI] 직원 줄 프리팹 생성 : {ElementPrefabPath}", prefab);
        return prefab;
    }

    private static void EnsureFolder(string assetFolder)
    {
        if (string.IsNullOrEmpty(assetFolder) || AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        Directory.CreateDirectory(assetFolder);
        AssetDatabase.Refresh();
    }

    #endregion

    #region 잡일

    private static GameObject NewUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));

        if (parent != null)
        {
            obj.transform.SetParent(parent, false);
        }

        return obj;
    }

    private static void Stretch(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRef(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty property = so.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"[배정UI] '{propertyName}' 필드를 찾지 못했습니다.");
            return;
        }

        property.objectReferenceValue = value;
    }

    /// <summary>꺼져 있는 오브젝트에 붙은 것도 찾는다 (팝업은 평소에 꺼져 있다).</summary>
    private static T FindInScene<T>() where T : Object
    {
        T[] found = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return found.Length > 0 ? found[0] : null;
    }

    private static T FindAsset<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

        if (guids.Length == 0)
        {
            Debug.LogWarning($"[배정UI] {typeof(T).Name} 에셋을 찾지 못했습니다. 인스펙터에서 직접 넣어주세요.");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    #endregion
}

#endif

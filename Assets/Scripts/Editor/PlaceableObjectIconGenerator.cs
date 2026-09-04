#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 프리팹 목록을 받아 프로젝트 창에 뜨는 썸네일을 PNG로 뽑고 스프라이트로 임포트한다.
/// 그 프리팹을 쓰는 PlaceableObjectSO를 찾으면 icon 칸까지 자동으로 채워준다.
///
/// 미리보기는 유니티가 백그라운드에서 렌더링하기 때문에 부른 즉시 나오지 않는다.
/// 그래서 버튼을 누르면 큐에 쌓아두고, EditorApplication.update에서 준비된 것부터 하나씩 저장한다.
/// </summary>
public class PlaceableObjectIconGenerator : EditorWindow
{
    //유니티가 만들어주는 썸네일 원본 크기. 이보다 크게 저장해도 늘어날 뿐 선명해지지 않는다.
    private const int PreviewSize = 128;

    //GetAssetPreview는 렌더링이 끝나기 전까지 null을 준다. 요청 직후에는
    //IsLoadingAssetPreview조차 false일 수 있어서, 없다고 바로 포기하면 안 된다.
    private const int MaxWaitTicks = 300;

    [SerializeField] private List<GameObject> _prefabs = new List<GameObject>();
    [SerializeField] private string _outputFolder = "Assets/Resources/UI/Icon/PlaceableObject";
    [SerializeField] private int _size = PreviewSize;
    [SerializeField] private bool _assignToSO = true;

    private SerializedObject _serialized;
    private Vector2 _scroll;

    private readonly Queue<GameObject> _queue = new Queue<GameObject>();
    private int _waitTicks;
    private int _doneCount;
    private int _failCount;
    private int _totalCount;
    private string _log = "";

    [MenuItem("Tools/CheckCompany/프리팹 아이콘 만들기")]
    private static void Open()
    {
        PlaceableObjectIconGenerator window = GetWindow<PlaceableObjectIconGenerator>("프리팹 아이콘");
        window.minSize = new Vector2(400f, 460f);
    }

    private void OnEnable()
    {
        //도메인 리로드 후에는 예전 SerializedObject가 죽은 참조를 들고 있다
        _serialized = null;
    }

    private void OnDisable()
    {
        //창을 닫으면 돌던 작업도 같이 멈춘다
        EditorApplication.update -= Tick;
    }

    private void OnGUI()
    {
        if (_serialized == null)
        {
            _serialized = new SerializedObject(this);
        }

        _serialized.Update();

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("대상 프리팹", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_serialized.FindProperty("_prefabs"), new GUIContent("프리팹 목록"), true);

        EditorGUILayout.Space();

        bool addSelection;
        bool addAll;
        bool clear;

        using (new EditorGUILayout.HorizontalScope())
        {
            addSelection = GUILayout.Button("선택한 것 담기");
            addAll = GUILayout.Button("PlaceableObject 전부 담기");
            clear = GUILayout.Button("비우기", GUILayout.Width(60f));
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_serialized.FindProperty("_outputFolder"), new GUIContent("저장 폴더"));
        EditorGUILayout.PropertyField(_serialized.FindProperty("_size"), new GUIContent("저장 크기(px)"));
        EditorGUILayout.PropertyField(
            _serialized.FindProperty("_assignToSO"),
            new GUIContent("SO의 icon에 자동 연결", "그 프리팹을 쓰는 PlaceableObjectSO를 찾아 icon을 채웁니다."));

        EditorGUILayout.HelpBox(
            $"유니티 썸네일 원본은 {PreviewSize}x{PreviewSize}입니다. " +
            "더 크게 저장하면 늘려서 저장될 뿐 선명해지지는 않습니다.",
            MessageType.Info);

        EditorGUILayout.Space();

        bool isRunning = _queue.Count > 0;

        using (new EditorGUI.DisabledScope(isRunning))
        {
            if (GUILayout.Button(isRunning ? "만드는 중..." : "아이콘 만들기", GUILayout.Height(32f)))
            {
                StartGenerate();
            }
        }

        if (isRunning)
        {
            EditorGUILayout.LabelField($"진행 : {_totalCount - _queue.Count} / {_totalCount}");
        }

        if (!string.IsNullOrEmpty(_log))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("결과", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_log, GUILayout.MinHeight(120f));
        }

        EditorGUILayout.EndScrollView();

        //ApplyModifiedProperties가 인스펙터에서 고친 값을 필드로 써준다.
        //목록을 직접 건드리는 버튼들은 이 뒤에 처리해야 방금 한 수정이 되돌려지지 않는다.
        _serialized.ApplyModifiedProperties();

        if (addSelection)
        {
            AddSelection();
            _serialized.Update();
        }

        if (addAll)
        {
            AddAllPlaceableObjects();
            _serialized.Update();
        }

        if (clear)
        {
            _prefabs.Clear();
            _serialized.Update();
        }
    }

    #region 목록 채우기

    private void AddSelection()
    {
        GameObject[] selected = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        for (int i = 0; i < selected.Length; i++)
        {
            if (!_prefabs.Contains(selected[i]))
            {
                _prefabs.Add(selected[i]);
            }
        }
    }

    /// <summary>프로젝트 전체에서 PlaceableObject가 붙은 프리팹을 전부 찾아 담는다.</summary>
    private void AddAllPlaceableObjects()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null || prefab.GetComponent<PlaceableObject>() == null)
            {
                continue;
            }

            if (!_prefabs.Contains(prefab))
            {
                _prefabs.Add(prefab);
            }
        }
    }

    #endregion

    #region Generation

    private void StartGenerate()
    {
        _queue.Clear();
        _doneCount = 0;
        _failCount = 0;
        _waitTicks = 0;
        _log = "";

        for (int i = 0; i < _prefabs.Count; i++)
        {
            if (_prefabs[i] != null)
            {
                _queue.Enqueue(_prefabs[i]);
            }
        }

        _totalCount = _queue.Count;

        if (_totalCount == 0)
        {
            _log = "대상 프리팹이 없습니다.";
            return;
        }

        EnsureFolder(_outputFolder);

        //여러 개를 연달아 뽑으면 앞에서 만든 미리보기가 캐시에서 밀려난다
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(64, _totalCount * 2));

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private void Tick()
    {
        if (_queue.Count == 0)
        {
            Finish();
            return;
        }

        GameObject prefab = _queue.Peek();

        //목록에 넣은 뒤 지워진 경우
        if (prefab == null)
        {
            _queue.Dequeue();
            _waitTicks = 0;
            return;
        }

        Texture2D preview = AssetPreview.GetAssetPreview(prefab);

        if (preview == null)
        {
            _waitTicks++;

            //아직 렌더링 중이다. 충분히 기다렸는데도 안 나오면 그때 포기한다.
            if (_waitTicks < MaxWaitTicks)
            {
                return;
            }

            _log += $"[실패] {prefab.name} : 미리보기를 만들지 못했습니다.\n";
            _failCount++;
            _queue.Dequeue();
            _waitTicks = 0;
            return;
        }

        _queue.Dequeue();
        _waitTicks = 0;

        Save(prefab, preview);
        Repaint();
    }

    private void Finish()
    {
        EditorApplication.update -= Tick;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _log += $"\n완료 : 성공 {_doneCount}개, 실패 {_failCount}개";
        Repaint();
    }

    private void Save(GameObject prefab, Texture2D preview)
    {
        int size = Mathf.Clamp(_size, 16, 1024);

        Texture2D readable = CopyToReadable(preview, size);
        byte[] png = readable.EncodeToPNG();
        DestroyImmediate(readable);

        string path = $"{_outputFolder}/{prefab.name}.png";
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        //PNG는 기본이 Default 타입이라 그대로 두면 Sprite 칸에 못 넣는다
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        _doneCount++;
        _log += $"[생성] {path}\n";

        if (!_assignToSO)
        {
            return;
        }

        AssignIcon(prefab, AssetDatabase.LoadAssetAtPath<Sprite>(path));
    }

    /// <summary>
    /// AssetPreview가 준 텍스처는 읽기 전용이라 EncodeToPNG가 바로 안 된다.
    /// RenderTexture에 한 번 그린 뒤 픽셀을 읽어 새 텍스처로 복사한다.
    /// </summary>
    private static Texture2D CopyToReadable(Texture2D source, int size)
    {
        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = rt;

        //빌려온 RenderTexture에는 이전에 쓰던 그림이 남아있을 수 있다
        GL.Clear(true, true, Color.clear);
        Graphics.Blit(source, rt);

        Texture2D copy = new Texture2D(size, size, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0f, 0f, size, size), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return copy;
    }

    /// <summary>그 프리팹을 prefab 칸에 물고 있는 PlaceableObjectSO를 찾아 icon을 채운다.</summary>
    private void AssignIcon(GameObject prefab, Sprite sprite)
    {
        if (sprite == null)
        {
            _log += "  └ 스프라이트로 임포트되지 않아 연결하지 못했습니다.\n";
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:PlaceableObjectSO");

        for (int i = 0; i < guids.Length; i++)
        {
            string soPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            PlaceableObjectSO so = AssetDatabase.LoadAssetAtPath<PlaceableObjectSO>(soPath);

            if (so == null || so.GetPrefab() != prefab)
            {
                continue;
            }

            //icon은 private이라 SerializedObject로 넣어야 한다
            SerializedObject serialized = new SerializedObject(so);
            SerializedProperty iconProperty = serialized.FindProperty("icon");

            if (iconProperty == null)
            {
                _log += $"  └ '{so.name}'에 icon 필드를 찾지 못했습니다.\n";
                return;
            }

            iconProperty.objectReferenceValue = sprite;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(so);

            _log += $"  └ {so.name}.icon 연결 완료\n";
            return;
        }

        _log += "  └ 이 프리팹을 쓰는 PlaceableObjectSO가 없어 연결은 건너뜁니다.\n";
    }

    private static void EnsureFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
    }

    #endregion
}

#endif

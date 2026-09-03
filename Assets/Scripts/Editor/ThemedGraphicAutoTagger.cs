#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 선택한 오브젝트 아래의 모든 Graphic(Image, TextMeshProUGUI ...)에 ThemedGraphic을 붙이고
/// 역할(UIRole)을 추측해서 채워준다. 로그인 씬처럼 손대야 할 그래픽이 수십 개일 때
/// 하나하나 컴포넌트를 붙이고 역할을 고르는 걸 대신해준다.
///
/// 추측일 뿐이다 - 다 붙인 다음 인스펙터에서 눈으로 확인하고 틀린 역할은 고쳐야 한다.
/// 이미 ThemedGraphic이 붙어있는 오브젝트는 건드리지 않는다 (이미 손으로 고쳐둔 역할을
/// 다시 돌릴 때마다 덮어쓰면 안 되니까).
///
/// 사용법 : Hierarchy에서 Canvas(또는 그 아래 원하는 루트)를 고르고 메뉴 실행.
/// </summary>
public static class ThemedGraphicAutoTagger
{
    private const string MenuPath = "Tools/CheckCompany/UI 역할 자동 태깅 (ThemedGraphic)";

    //TMP 기본 텍스트가 알파 0.5로 들어가 있으면 플레이스홀더로 본다 (로그인 씬 관찰 결과 그대로).
    private const float PlaceholderAlphaThreshold = 0.9f;

    [MenuItem(MenuPath)]
    private static void Tag()
    {
        GameObject[] roots = Selection.gameObjects;

        if (roots == null || roots.Length == 0)
        {
            Debug.LogWarning("[ThemedGraphicAutoTagger] 먼저 Hierarchy에서 Canvas나 루트 오브젝트를 선택해주세요.");
            return;
        }

        int tagged = 0;
        int skipped = 0;

        foreach (GameObject root in roots)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);

            foreach (Graphic graphic in graphics)
            {
                if (graphic.GetComponent<ThemedGraphic>() != null)
                {
                    skipped++;
                    continue;
                }

                UIRole role = GuessRole(graphic);

                ThemedGraphic themed = Undo.AddComponent<ThemedGraphic>(graphic.gameObject);
                SerializedObject serialized = new SerializedObject(themed);
                serialized.FindProperty("_role").enumValueIndex = (int)role;
                serialized.ApplyModifiedProperties();

                EditorUtility.SetDirty(graphic.gameObject);
                tagged++;
            }
        }

        Debug.Log($"[ThemedGraphicAutoTagger] {tagged}개에 역할을 붙였습니다 (이미 있어서 건너뛴 것 {skipped}개). " +
                   "역할이 맞는지 하나씩 확인해주세요.");
    }

    [MenuItem(MenuPath, true)]
    private static bool TagValidate()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    /// <summary>
    /// 로그인 씬에서 실제로 관찰된 패턴 기반 추측:
    /// - Canvas 바로 밑에 붙어 화면 전체를 덮는 배경 하나 → Backdrop
    /// - 텍스트인데 알파가 낮게(반투명) 잡혀 있으면 → PlaceholderText, 진하면 → PrimaryText
    /// - Button 컴포넌트가 달린 오브젝트(또는 그 자식) 자신의 배경 → Button
    /// - 그 외 나머지 Image → Surface (입력창 배경 등)
    /// </summary>
    private static UIRole GuessRole(Graphic graphic)
    {
        if (graphic is TMP_Text tmpText)
        {
            return tmpText.color.a < PlaceholderAlphaThreshold ? UIRole.PlaceholderText : UIRole.PrimaryText;
        }

        //Canvas 바로 밑에 있는 Image = 화면 전체 배경. 그 밑(자식)에 있는 건 카드/버튼이니 여기 안 걸린다.
        Transform parent = graphic.transform.parent;
        if (parent != null && parent.GetComponent<Canvas>() != null)
        {
            return UIRole.Backdrop;
        }

        if (graphic.GetComponent<Button>() != null)
        {
            return UIRole.Button;
        }

        return UIRole.Surface;
    }
}

#endif

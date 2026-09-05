using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택한 오브젝트(프리팹)의 BoxCollider를 자식 메쉬 전체 크기에 맞춰준다.
///
/// Unity가 BoxCollider를 자동으로 맞춰주는 것은 같은 GameObject에 MeshFilter가 있을 때뿐이다.
/// FBX를 통째로 가져와 루트에 콜라이더만 붙이는 경우(루트에는 메쉬가 없다) 크기가
/// 기본값 1x1x1 / 중심 0 그대로 남는데, PlaceableObject는 이 콜라이더로 차지하는 칸 수와
/// 위에 물건을 올릴 높이를 계산하므로 실제 모양과 어긋나면 배치가 전부 어긋난다.
///
/// 사용법 : Hierarchy나 Project에서 프리팹을 고르고 메뉴 실행.
/// </summary>
public static class BoxColliderFitter
{
    private const string MenuPath = "Tools/CheckCompany/3. 선택한 오브젝트의 BoxCollider를 메쉬에 맞추기";

    [MenuItem(MenuPath)]
    private static void Fit()
    {
        GameObject[] targets = Selection.gameObjects;

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("[BoxColliderFitter] 맞출 오브젝트를 먼저 선택해주세요.");
            return;
        }

        int fitted = 0;

        foreach (GameObject target in targets)
        {
            if (FitOne(target))
            {
                fitted++;
            }
        }

        Debug.Log($"[BoxColliderFitter] {fitted}개의 BoxCollider를 메쉬 크기에 맞췄습니다.");
    }

    [MenuItem(MenuPath, true)]
    private static bool FitValidate()
    {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    private static bool FitOne(GameObject target)
    {
        BoxCollider box = target.GetComponent<BoxCollider>();

        if (box == null)
        {
            Debug.LogWarning($"[BoxColliderFitter] '{target.name}' : BoxCollider가 없습니다.", target);
            return false;
        }

        MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>();

        if (filters.Length == 0)
        {
            Debug.LogWarning($"[BoxColliderFitter] '{target.name}' : 자식에 메쉬가 없습니다.", target);
            return false;
        }

        //메쉬의 바운드 상자를 꼭짓점 단위로 이 오브젝트의 '로컬' 공간에 옮겨서 감싼다.
        //
        //예전에는 Renderer.bounds(월드 축 기준 AABB)를 모아 lossyScale로 나눠 box.size에 넣었다.
        //그런데 box.size는 로컬 축 기준으로 해석되므로, 루트에 회전이 걸린 프리팹에서는
        //월드 축으로 잰 길이가 엉뚱한 로컬 축에 들어간다.
        //(문: 루트가 -90도라 로컬 Y가 월드 -Z를 향하는데, 월드에서 잰 높이 2.14가
        // 그대로 로컬 Y에 박혀서 메쉬는 서 있는데 콜라이더만 바닥에 눕는 모양이 됐다)
        Transform t = target.transform;
        Bounds local = default;
        bool started = false;

        foreach (MeshFilter filter in filters)
        {
            Mesh mesh = filter.sharedMesh;

            if (mesh == null)
            {
                continue;
            }

            Bounds meshBounds = mesh.bounds;

            for (int i = 0; i < CornerSigns.Length; i++)
            {
                Vector3 corner = meshBounds.center + Vector3.Scale(meshBounds.extents, CornerSigns[i]);

                //메쉬 로컬 -> 월드 -> 대상의 로컬. 중간에 회전이 몇 번 끼어 있어도 정확히 따라간다.
                Vector3 localPoint = t.InverseTransformPoint(filter.transform.TransformPoint(corner));

                if (started)
                {
                    local.Encapsulate(localPoint);
                }
                else
                {
                    local = new Bounds(localPoint, Vector3.zero);
                    started = true;
                }
            }
        }

        if (!started)
        {
            Debug.LogWarning($"[BoxColliderFitter] '{target.name}' : 메쉬가 비어 있습니다.", target);
            return false;
        }

        Undo.RecordObject(box, "Fit BoxCollider");
        box.center = local.center;
        box.size = local.size;
        EditorUtility.SetDirty(box);

        Debug.Log(
            $"[BoxColliderFitter] '{target.name}' : size {local.size}, center {local.center}",
            target);

        return true;
    }

    //바운드 상자의 여덟 꼭짓점 부호
    private static readonly Vector3[] CornerSigns =
    {
        new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f),
        new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f),
        new Vector3(-1f, -1f, 1f), new Vector3(1f, -1f, 1f),
        new Vector3(-1f, 1f, 1f), new Vector3(1f, 1f, 1f)
    };
}

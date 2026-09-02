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

        MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[BoxColliderFitter] '{target.name}' : 자식에 MeshRenderer가 없습니다.", target);
            return false;
        }

        //월드 기준 바운드를 모은 뒤 로컬로 되돌린다.
        //Renderer.bounds는 월드 기준이라 그대로 넣으면 회전/스케일이 있는 프리팹에서 어긋난다.
        Bounds world = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            world.Encapsulate(renderers[i].bounds);
        }

        Transform t = target.transform;
        Vector3 localCenter = t.InverseTransformPoint(world.center);

        Vector3 lossy = t.lossyScale;
        Vector3 localSize = new Vector3(
            SafeDivide(world.size.x, lossy.x),
            SafeDivide(world.size.y, lossy.y),
            SafeDivide(world.size.z, lossy.z));

        Undo.RecordObject(box, "Fit BoxCollider");
        box.center = localCenter;
        box.size = localSize;
        EditorUtility.SetDirty(box);

        Debug.Log(
            $"[BoxColliderFitter] '{target.name}' : size {localSize}, center {localCenter}",
            target);

        return true;
    }

    private static float SafeDivide(float value, float scale)
    {
        return Mathf.Approximately(scale, 0f) ? value : value / Mathf.Abs(scale);
    }
}

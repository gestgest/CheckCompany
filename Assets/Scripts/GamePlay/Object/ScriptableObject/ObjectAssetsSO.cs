using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에서 파는 오브젝트 전부. 카테고리별로 배열을 나눠 갖지 않고 여기 한 곳에만 모아둔다.
/// 탭이 늘어나도 채워 넣을 곳은 여전히 이 배열 하나뿐이고,
/// 카테고리는 각 ObjectSO가 들고 있는 ObjectType으로 갈린다.
/// </summary>
[CreateAssetMenu(fileName = "ObjectAssetsSO", menuName = "ScriptableObject/ObjectAssetsSO")]
public class ObjectAssetsSO : ScriptableObject
{
    [SerializeField] private ObjectSO[] _objects;

    /// <summary>그 카테고리의 오브젝트만 골라준다. 순서는 배열에 넣은 순서 그대로다.</summary>
    public List<ObjectSO> GetObjects(ObjectType type)
    {
        List<ObjectSO> result = new List<ObjectSO>();

        for (int i = 0; i < _objects.Length; i++)
        {
            ObjectSO obj = _objects[i];

            //배열에 빈 칸을 남겨둔 경우. 여기서 걸러야 쓰는 쪽에서 NRE가 안 난다.
            if (obj == null)
            {
                Debug.LogWarning($"[ObjectAssetsSO] '{name}' : _objects[{i}]가 비어 있습니다.", this);
                continue;
            }

            if (obj.GetObjectType() == type)
            {
                result.Add(obj);
            }
        }

        return result;
    }
}

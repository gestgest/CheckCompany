using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에서 파는 오브젝트 전부. 타입마다 그룹을 따로 두고, 그 안에서 id % 100으로 바로 인덱싱한다.
///
/// id 규칙 : (타입 블록 * 100) + 그룹 안에서의 순번.
///   예) Desk 블록 0  -> id 1, 2, 3
///       Chair 블록 1 -> id 101, 102, 103
/// 0번 칸은 비워두고 1번부터 채운다 (이미 그렇게 만들어 둔 id를 그대로 쓰기 위함).
///
/// _objects[i]의 id % 100은 반드시 i와 같아야 한다 - 인스펙터에서 순서를 바꾸면 이 규칙이 깨지므로
/// OnValidate가 저장할 때마다 확인해서 어긋나면 에러로 알려준다.
/// </summary>
[CreateAssetMenu(fileName = "PlaceableObjectAssetsSO", menuName = "ScriptableObject/PlaceableObjectAssetsSO")]
public class PlaceableObjectAssetsSO : ScriptableObject
{
    
    //인덱스 == id % 100. 순번이 비어도 되지만(0번 칸 등), 채운 칸의 위치는 반드시 id와 맞아야 한다.
    [SerializeField] private PlaceableObjectSO[] _objects;
    
    [Serializable]
    private class ObjectGroup
    {
        [SerializeField] private ObjectType _type;

        //id를 100으로 나눈 몫. 이 그룹에 속한 모든 SO의 id는 반드시 (_block * 100 + 배열 인덱스)여야 한다.
        [SerializeField] private int _block;

        //인덱스 == id % 100. 순번이 비어도 되지만(0번 칸 등), 채운 칸의 위치는 반드시 id와 맞아야 한다.
        [SerializeField] private PlaceableObjectSO[] _objects;

        public ObjectType Type => _type;
        public int Block => _block;
        public PlaceableObjectSO[] Objects => _objects;

        public PlaceableObjectSO GetByLocalIndex(int localIndex)
        {
            if (_objects == null || localIndex < 0 || localIndex >= _objects.Length)
            {
                return null;
            }

            return _objects[localIndex];
        }
    }

    [SerializeField] private ObjectGroup[] _groups;

    /// <summary>서버에서 받은 property_id로 SO를 찾는다. id % 100으로 바로 인덱싱하므로 O(1)이다.</summary>
    public PlaceableObjectSO GetObject(int id)
    {
        int block = id / 100;
        int localIndex = id % 100;

        if (_groups == null)
        {
            return null;
        }

        for (int i = 0; i < _groups.Length; i++)
        {
            if (_groups[i].Block != block)
            {
                continue;
            }

            PlaceableObjectSO result = _groups[i].GetByLocalIndex(localIndex);

            if (result == null)
            {
                Debug.LogError(
                    $"[PlaceableObjectAssetsSO] '{name}' : id {id}(블록 {block}, 순번 {localIndex})에 " +
                    "해당하는 오브젝트가 없습니다.",
                    this);
            }

            return result;
        }

        Debug.LogError(
            $"[PlaceableObjectAssetsSO] '{name}' : id {id} - 블록 {block}을 담당하는 그룹이 없습니다.",
            this);

        return null;
    }

    /// <summary>그 카테고리의 오브젝트만 골라준다. 순서는 그룹 안 배열 순서 그대로다.</summary>
    public List<PlaceableObjectSO> GetObjects(ObjectType type)
    {
        List<PlaceableObjectSO> result = new List<PlaceableObjectSO>();

        if (_groups == null)
        {
            return result;
        }

        for (int i = 0; i < _groups.Length; i++)
        {
            if (_groups[i].Type != type)
            {
                continue;
            }

            PlaceableObjectSO[] objects = _groups[i].Objects;

            if (objects == null)
            {
                continue;
            }

            for (int j = 0; j < objects.Length; j++)
            {
                if (objects[j] != null)
                {
                    result.Add(objects[j]);
                }
            }
        }

        return result;
    }

#if UNITY_EDITOR
    /// <summary>
    /// id % 100 == 배열 인덱스 규칙이 깨졌는지 저장할 때마다 확인한다.
    /// 이걸 안 하면 인스펙터에서 드래그로 순서 한 번만 바꿔도 GetObject()가 조용히 엉뚱한 물건을
    /// 돌려주는데, 그걸 원인 찾기가 매우 어렵다 - 그래서 여기서 바로 크게 알린다.
    /// </summary>
    private void OnValidate()
    {
        if (_groups == null)
        {
            return;
        }

        HashSet<int> blockOwners = new HashSet<int>();
        Dictionary<GameObject, PlaceableObjectSO> prefabOwners = new Dictionary<GameObject, PlaceableObjectSO>();

        for (int g = 0; g < _groups.Length; g++)
        {
            ObjectGroup group = _groups[g];

            if (!blockOwners.Add(group.Block))
            {
                Debug.LogError(
                    $"[PlaceableObjectAssetsSO] '{name}' : 블록 {group.Block}을 담당하는 그룹이 두 개 이상입니다.",
                    this);
            }

            PlaceableObjectSO[] objects = group.Objects;

            if (objects == null)
            {
                continue;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                PlaceableObjectSO obj = objects[i];

                if (obj == null)
                {
                    continue;
                }

                int expectedId = group.Block * 100 + i;

                if (obj.GetID() != expectedId)
                {
                    Debug.LogError(
                        $"[PlaceableObjectAssetsSO] '{name}' : {group.Type} 그룹의 {i}번 칸에는 " +
                        $"id {expectedId}인 SO가 있어야 하는데 '{obj.name}'(id {obj.GetID()})이 들어있습니다. " +
                        "드래그로 순서가 바뀌었는지 확인하세요.",
                        obj);
                }

                GameObject prefab = obj.GetPrefab();

                if (prefab == null)
                {
                    continue;
                }

                if (prefabOwners.TryGetValue(prefab, out PlaceableObjectSO owner))
                {
                    Debug.LogWarning(
                        $"[PlaceableObjectAssetsSO] '{obj.name}'와 '{owner.name}'가 같은 프리팹" +
                        $"('{prefab.name}')을 가리킵니다. 복사한 뒤 prefab을 안 바꾼 게 아닌지 확인하세요.",
                        obj);
                }
                else
                {
                    prefabOwners.Add(prefab, obj);
                }
            }
        }
    }
#endif
}

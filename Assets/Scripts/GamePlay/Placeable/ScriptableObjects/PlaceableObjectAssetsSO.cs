using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상점에서 파는 오브젝트 전부. 카테고리별로 배열을 나눠 갖지 않고 여기 한 곳에만 모아둔다.
/// 탭이 늘어나도 채워 넣을 곳은 여전히 이 배열 하나뿐이고,
/// 카테고리는 각 PlaceableObjectSO가 들고 있는 ObjectType으로 갈린다.
///
/// 서버는 오브젝트 종류를 property_id(숫자)로만 기억하므로 id -> SO 변환도 여기서 맡는다.
/// 배열의 몇 번째 칸인지가 아니라 각 SO에 박아둔 id로 찾기 때문에,
/// 인스펙터에서 순서를 바꾸거나 중간에 하나를 끼워 넣어도 저장된 데이터가 안 틀어진다.
/// </summary>
[CreateAssetMenu(fileName = "PlaceableObjectAssetsSO", menuName = "ScriptableObject/PlaceableObjectAssetsSO")]
public class PlaceableObjectAssetsSO : ScriptableObject
{
    //인스펙터에서 채우는 원본. 유니티는 Dictionary를 직렬화하지 못하므로 이쪽은 반드시 배열이어야 한다.
    [SerializeField] private PlaceableObjectSO[] _objects;

    //id -> SO. _objects에서 만들어내는 값이라 직렬화하지 않는다.
    //여기에 직접 뭔가를 넣는 코드가 생기면 원본이 두 개가 되므로, 오직 BuildLookup()만 채운다.
    private Dictionary<int, PlaceableObjectSO> _lookup;

    /// <summary>서버에서 받은 property_id로 SO를 찾는다. 목록에 없으면 null.</summary>
    public PlaceableObjectSO GetObject(int id)
    {
        //에디터에서는 개별 SO의 id를 아무 때나 고칠 수 있는데, 그때 이 에셋의 OnValidate는 불리지 않아
        //캐시가 낡는다. 플레이 중에는 id가 바뀔 일이 없으니 그때만 캐시하고 에디터에서는 매번 새로 만든다.
        if (_lookup == null || !Application.isPlaying)
        {
            BuildLookup();
        }

        if (_lookup.TryGetValue(id, out PlaceableObjectSO result))
        {
            return result;
        }

        //서버에는 있는데 목록에서 빠진 경우. 그 오브젝트만 화면에 안 나온다.
        Debug.LogError(
            $"[PlaceableObjectAssetsSO] '{name}' : id {id}인 오브젝트가 목록에 없습니다. " +
            "_objects에 넣었는지, 그 SO의 id 값이 맞는지 확인하세요.",
            this);

        return null;
    }

    /// <summary>그 카테고리의 오브젝트만 골라준다. 순서는 배열에 넣은 순서 그대로다.</summary>
    public List<PlaceableObjectSO> GetObjects(ObjectType type)
    {
        List<PlaceableObjectSO> result = new List<PlaceableObjectSO>();

        if (_objects == null)
        {
            return result;
        }

        for (int i = 0; i < _objects.Length; i++)
        {
            PlaceableObjectSO obj = _objects[i];

            //배열에 빈 칸을 남겨둔 경우. 여기서 걸러야 쓰는 쪽에서 NRE가 안 난다.
            if (obj == null)
            {
                Debug.LogWarning($"[PlaceableObjectAssetsSO] '{name}' : _objects[{i}]가 비어 있습니다.", this);
                continue;
            }

            if (obj.GetObjectType() == type)
            {
                result.Add(obj);
            }
        }

        return result;
    }

    /// <summary>
    /// id -> SO 표를 만든다.
    ///
    /// 여기서 id 중복과 prefab 중복을 같이 잡는다. 둘 다 그냥 두면 에러 없이 조용히 넘어가고,
    /// 나중에 "가끔 다른 물건이 나온다"로만 나타나서 원인을 찾기가 매우 어렵다.
    /// </summary>
    private void BuildLookup()
    {
        _lookup = new Dictionary<int, PlaceableObjectSO>();

        if (_objects == null)
        {
            return;
        }

        //prefab 중복 검사용. id와 달리 표를 만드는 것만으로는 안 걸리는 실수다
        //(SO를 복사해놓고 prefab 칸만 안 바꾼 경우).
        Dictionary<GameObject, PlaceableObjectSO> prefabOwners = new Dictionary<GameObject, PlaceableObjectSO>();

        for (int i = 0; i < _objects.Length; i++)
        {
            PlaceableObjectSO obj = _objects[i];

            if (obj == null)
            {
                continue;
            }

            int id = obj.GetID();

            //id가 겹치면 어느 쪽을 써야 할지 알 수 없다
            if (_lookup.ContainsKey(id))
            {
                Debug.LogError(
                    $"[PlaceableObjectAssetsSO] '{name}' : id {id}가 겹칩니다. " +
                    $"'{obj.name}'와 '{_lookup[id].name}' 중 하나의 id를 바꿔주세요.",
                    obj);
                continue;
            }

            _lookup.Add(id, obj);

            GameObject prefab = obj.GetPrefab();

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[PlaceableObjectAssetsSO] '{obj.name}' : prefab이 비어 있어 놓을 수 없습니다.",
                    obj);
                continue;
            }

            if (prefabOwners.TryGetValue(prefab, out PlaceableObjectSO owner))
            {
                Debug.LogWarning(
                    $"[PlaceableObjectAssetsSO] '{obj.name}'와 '{owner.name}'가 같은 프리팹" +
                    $"('{prefab.name}')을 가리킵니다. 복사한 뒤 prefab을 안 바꾼 게 아닌지 확인하세요.",
                    obj);
                continue;
            }

            prefabOwners.Add(prefab, obj);
        }
    }
}

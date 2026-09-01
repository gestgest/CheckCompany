using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 한 카테고리(의자, 책상 ...)에서 파는 오브젝트 목록. 에셋 하나가 타입 하나를 맡는다.
///
/// id 규칙 : (블록 * 100) + 배열 인덱스.
///   예) 책상 에셋 -> id 1, 2, 3
///       의자 에셋 -> id 101, 102, 103
/// 0번 칸은 비워두고 1번부터 채운다.
///
/// 배열 인덱스가 곧 id의 뒤 두 자리라서 GetObject()는 id % 100으로 바로 꺼낸다.
/// </summary>
[CreateAssetMenu(fileName = "PlaceableObjectAssetsSO", menuName = "ScriptableObject/PlaceableObjectAssetsSO")]
public class PlaceableObjectAssetsSO : ScriptableObject
{
    //이 에셋이 맡는 카테고리
    [SerializeField] private ObjectType _type;

    //인덱스 == id % 100
    [SerializeField] private PlaceableObjectSO[] _objects;

    public ObjectType Type => _type;

    /// <summary>id로 바로 꺼낸다. 없으면 null.</summary>
    public PlaceableObjectSO GetObject(int id)
    {
        int index = id % 100;

        if (_objects == null || index < 0 || index >= _objects.Length)
        {
            Debug.LogError($"[PlaceableObjectAssetsSO] '{name}' : id {id}에 해당하는 칸이 없습니다.", this);
            return null;
        }

        return _objects[index];
    }

    /// <summary>이 카테고리의 오브젝트 전부. 비어있는 칸은 빼고 준다.</summary>
    public List<PlaceableObjectSO> GetObjects()
    {
        List<PlaceableObjectSO> result = new List<PlaceableObjectSO>();

        if (_objects == null)
        {
            return result;
        }

        for (int i = 0; i < _objects.Length; i++)
        {
            if (_objects[i] != null)
            {
                result.Add(_objects[i]);
            }
        }

        return result;
    }

}

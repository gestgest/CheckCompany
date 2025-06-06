using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "PlaceSystemSO", menuName = "ScriptableObject/Manager/PlaceSystemSO")]
public class PlacedObjectManager : ScriptableObject
{

    [Header("PlacedObjects")]
    
    
    
    [Header("Broadcasting on Event")]
    //[SerializeField] private GameObjectEventChannelSO _createEvent;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    
    private List<PlaceableObject> _placedObjects;

    private int object_id;

    public void Init()
    {
        _placedObjects = new List<PlaceableObject>();
    }


    /// <summary>
    /// 모든 서버에서 가져온 데이터 설정
    /// </summary>
    /// <param name="data"></param>
    /// <param name="object_id"></param>
    public void SetPlacedObjects(Dictionary<string, object> data, int object_id)
    {
        //map 구조의 data
        SetObjectID(object_id, false);

        if (data == null)
        {
            return;
        }
        //map형태의 recruitments를 list로 변환
        foreach (KeyValuePair<string, object> serverPlaceableObject in data)
        {
            JSONtoPlacedObject(serverPlaceableObject);
        }

    }

    //object_id

    public void SetObjectID(int object_id, bool isServer = true)
    {
        this.object_id = object_id;
        if (!isServer)
        {
            return;
        }
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "placeableObject_id",
            this.object_id
        );
    }

    //
    private void JSONtoPlacedObject(KeyValuePair<string, object> placeableObject)
    {
        int id = Convert.ToInt32(placeableObject.Key);

        Dictionary<string, object> keyValues = (Dictionary<string, object>)placeableObject.Value;
        int index = Convert.ToInt32(keyValues["property_id"]);
        Dictionary<string, object> server_pos = (Dictionary<string, object>)keyValues["startPosition"];

        Vector3 pos = new Vector3(
            Convert.ToSingle(server_pos["x"]),
            Convert.ToSingle(server_pos["y"]),
            Convert.ToSingle(server_pos["z"])
        );
        _placedObjects[_placedObjects.Count - 1].ObjectPosition = pos;

        //BuildingObject(_shopPlaceableObjects[index].gameObject, pos); // 핸들링 안할거

        _placedObjects[_placedObjects.Count - 1].SetObjectID(id);
    }


    /// <summary> 모든 건물 타일 색칠 => false면 색칠no </summary>
    private void SetAllArea(bool isSelected) //
    {
        for (int i = 0; i < _placedObjects.Count; i++)
        {
            PlaceableObject po = _placedObjects[i]; //po는 null 아님, 아마 GetStartPosition 이거 자체가?
            Vector3Int startpos = gridLayout.WorldToCell(po.GetStartPosition());
            TakenArea(startpos, po.Size, isSelected);
        }
    }


}

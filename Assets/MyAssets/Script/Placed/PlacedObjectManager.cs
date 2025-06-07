using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlacedObjectManager", menuName = "ScriptableObject/Manager/PlacedObjectManager")]
public class PlacedObjectManager : ScriptableObject
{
    private List<PlacedObjectData> _placedObjects;

    [Header("Broadcasting on Event")]
    //[SerializeField] private GameObjectEventChannelSO _createEvent;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    

    private int object_id;

    public void Init()
    {
        _placedObjects = new List<PlacedObjectData>();
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
            JSONtoPlacedObjectData(serverPlaceableObject);
        }

    }

    //  ServerToObjectId
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

    private void JSONtoPlacedObjectData(KeyValuePair<string, object> placeableObject)
    {
        int id = Convert.ToInt32(placeableObject.Key);

        Dictionary<string, object> keyValues = (Dictionary<string, object>)placeableObject.Value;
        int property_id = Convert.ToInt32(keyValues["property_id"]);
        Dictionary<string, object> server_pos = (Dictionary<string, object>)keyValues["startPosition"];

        Vector3 pos = new Vector3(
            Convert.ToSingle(server_pos["x"]),
            Convert.ToSingle(server_pos["y"]),
            Convert.ToSingle(server_pos["z"])
        );
        PlacedObjectData pod = new PlacedObjectData(id, property_id, pos);
    }

    public void SendPlaceableObject(PlaceableObject selectedObject)
    {
        //오브젝트 ID, startpos를 전송하는 서버 함수
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "placeableObjects." + selectedObject.GetObjectID(),
            selectedObject.ObjectToJSON()
        );
    }

    public List<PlacedObjectData> GetPlacedObjects()
    {
        return _placedObjects;
    }

    public int GetObjectID()
    {
        return object_id;
    }


}

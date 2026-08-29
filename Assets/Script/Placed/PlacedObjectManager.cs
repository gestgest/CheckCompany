using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlacedObjectManager", menuName = "ScriptableObject/Manager/PlacedObjectManager")]
public class PlacedObjectManager : ScriptableObject
{
    private List<PlacedObjectData> _placedObjects;

    private GameObject _myCamera;
    private GameObject _okButton;
    private GameObject _denyButton;
    private GameObject _deleteButton;
    

    //UIPlaceableObject
    [Header("Broadcasting on Event")]
    [SerializeField] private GameObjectEventChannelSO _createPlaceableObjectEvent;
    [SerializeField] private VoidEventChannelSO _okEvent;
    [SerializeField] private VoidEventChannelSO _denyEvent;
    [SerializeField] private VoidEventChannelSO _deleteEvent;
    [SerializeField] private VoidEventChannelSO _onChangedEvent;
    
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    

    
    
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
        _onChangedEvent.RaiseEvent();
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

        _placedObjects.Add(pod);
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

    /// <summary>
    /// 배치된 오브젝트를 서버와 로컬 목록에서 지운다.
    /// 로컬 목록에서도 빼야 _onChangedEvent로 AllCreatePlacedObjects()가 다시 돌 때 되살아나지 않는다.
    /// </summary>
    public void RemovePlaceableObject(int id)
    {
        if (id == -1)
        {
            Debug.LogWarning("[PlacedObjectManager] id가 없는 오브젝트라 서버에서 지울 수 없습니다.");
            return;
        }

        _deleteFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "placeableObjects." + id
        );

        if (_placedObjects == null)
        {
            return;
        }

        //서버에서 받아온 목록에만 들어있다. 이번 플레이에서 새로 놓은 것은 없을 수도 있다.
        _placedObjects.RemoveAll(placedObject => placedObject.GetID() == id);
    }

    public List<PlacedObjectData> GetPlacedObjects()
    {
        return _placedObjects;
    }

    public int GetObjectID()
    {
        return object_id;
    }

    public void CreatePlaceableObject(GameObject obj)
    {
        _createPlaceableObjectEvent.RaiseEvent(obj);
    }

    public void OkEvent()
    {
        _okEvent.RaiseEvent();
    }

    public void DenyEvent()
    {
        _denyEvent.RaiseEvent();
    }

    //DeleteButton의 OnClick에 연결된다
    public void DeleteEvent()
    {
        _deleteEvent.RaiseEvent();
    }

    public void SetHandlingObjectProperties(
        GameObject camera,
        GameObject okButton,
        GameObject denyButton,
        GameObject deleteButton)
    {
        this._myCamera = camera;
        this._okButton = okButton;
        this._denyButton = denyButton;
        this._deleteButton = deleteButton;
    }
    
    public GameObject GetOkButton()
    {
        return this._okButton;
    }
    public GameObject GetDenyButton()
    {
        return this._denyButton;
    }

    public GameObject GetDeleteButton()
    {
        return this._deleteButton;
    }

    public GameObject GetCamera()
    {
        return this._myCamera;
    }

}

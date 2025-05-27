using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "PlaceSystemSO", menuName = "ScriptableObject/Controller/PlaceSystemSO")]
public class PlaceSystemSO : ScriptableObject
{
    private GridLayout gridLayout;
    private Tilemap mainTilemap;
    private TileBase _takenTile;
    private TileBase _redTile;
    Transform _objectParent;
    
    private Transform _cameraTransform;
    private GameObject _okButton;
    private GameObject _denyButton;

    //selected
    private PlaceableObject selectedObject;
    private Grid grid;

    [Header("PlacedObjects")]
    [SerializeField] private PlaceableObject [] _shopPlaceableObjects;
    
    
    [Header("Event")]
    [SerializeField] private GameObjectEventChannelSO _createEvent;
    [SerializeField] private VoidEventChannelSO _takenAreaEvent;
    [SerializeField] private Vector3TransformChannelSO _gridEvent;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    private List<PlaceableObject> _placedObjects;
    
    private bool isFirst = true; 
    private Vector3Int startpos;
    private Vector3Int object_size;
    private int object_id;

    public void Init
    (
        GridLayout gridLayout,
        Tilemap mainTilemap,
        TileBase takenTile,
        TileBase redTile,
        Transform objectParent,
        Transform cameraTransform,
        GameObject okButton,
        GameObject denyButton
    )
    {
        this.gridLayout = gridLayout;
        this.mainTilemap = mainTilemap;
        this._takenTile = takenTile;
        this._redTile = redTile;
        this._objectParent = objectParent;
        this._cameraTransform = cameraTransform;
        this._okButton = okButton;
        this._denyButton = denyButton;
        
        grid = gridLayout.gameObject.GetComponent<Grid>();
        _placedObjects = new List<PlaceableObject>();
    }

    public void SetPlacedObjects(Dictionary<string, object> data, int object_id)
    {
        //map 구조의 data
        SetObjectID(object_id,false);

        if (data == null)
        {
            return;
        }
        //map형태의 recruitments를 list로 변환
        foreach (KeyValuePair<string, object> serverPlaceableObject in data)
        {
            ServerCreateObject(serverPlaceableObject);
        }
        
    }

    private void OnEnable()
    {
        _createEvent._onEventRaised += CreateObject;
        _takenAreaEvent._onEventRaised += SetArea;
        _gridEvent._onEventRaised += SnapCoordinateToGrid;
    }
    private void OnDisable()
    {
        _createEvent._onEventRaised -= CreateObject;
        _takenAreaEvent._onEventRaised -= SetArea;
        _gridEvent._onEventRaised -= SnapCoordinateToGrid;
    }

    //어차피 안드로이드인데 키보드를 넣을 이유가 있나.
    private void Update()
    {
        if (!selectedObject)
        {
            return;
        }

        //놓는 함수v
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //PutOnObject();
        }
        //해체하는 함수x
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TakeOffObject();
        }
    }

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

    private void ServerCreateObject(KeyValuePair<string, object> placeableObject)
    {
        int id = (int.Parse(placeableObject.Key));
        
        Dictionary<string, object> keyValues = (Dictionary<string, object>)placeableObject.Value;
        int index = Convert.ToInt32(keyValues["property_id"]);
        Dictionary<string, object> server_pos = (Dictionary<string, object>)keyValues["startPosition"];
        
        Vector3 pos = new Vector3(
            Convert.ToSingle(server_pos["x"]),
            Convert.ToSingle(server_pos["y"]),
            Convert.ToSingle(server_pos["z"])
        );
        
        BuildingObject(_shopPlaceableObjects[index].gameObject, pos, false); // 핸들링 안할거
        _placedObjects[_placedObjects.Count - 1].SetObjectID(id);
    }
    
    //건물 만드는 함수
    public void CreateObject(GameObject obj)
    {
        //선택된 오브젝트가 있다면
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }
        BuildingObject(obj, Vector3.zero);
    }

    //건물 놓는 함수
    public void PutOnObject()
    {
        Vector3Int position = gridLayout.WorldToCell(selectedObject.GetStartPosition());

        //겹치는 타일이 없으면
        if (CheckTile(selectedObject, position))
        {
            //놓기 전에 물어보는 함수
            selectedObject.Place();
            selectedObject.SetObjectID(object_id);

            startpos = position;
            ClearArea();
            //TakenArea(startpos, selectedObject.Size);

            TakeOffBuilding();
            
            //오브젝트 ID, startpos를 전송하는 서버 함수
            _sendFirebaseEventChannelSO.RaiseEvent(
                "GamePlayUser",
                GameManager.instance.Nickname, 
                "placeableObjects." + selectedObject.GetObjectID(),
                selectedObject.ObjectToJSON()
            );
            
            SetObjectID(++object_id);
            //배치 하는 순간 조종 권한 제거  
            Destroy(selectedObject.gameObject.GetComponent<HandlingObject>());
            selectedObject = null;
        }
    }

    //안 놓는 함수
    public void TakeOffObject()
    {
        TakeOffBuilding();
        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    private void TakeOffBuilding()
    {
        //타일 지우기
        SetAllArea(false);
        
        //버튼 안 보이게
        _okButton.SetActive(false);
        _denyButton.SetActive(false);
    }

    #region Building Placement  

    /// <summary>
    ///
    /// </summary>
    /// <param name="building"></param>
    /// <param name="position">넣을 값 없으면 Vector3.zero</param>
    private void BuildingObject(GameObject building, Vector3 position, bool isHandling = true)
    {
        isFirst = true;

        //맨 처음 생성할때 0,0,0에 생성  => 마우스 위치에 생성으로 
        position = SnapCoordinateToGrid(position);

        GameObject obj = Instantiate(building, position, Quaternion.identity);

        obj.transform.parent = _objectParent;

        if (isHandling)
        {
            //생성된 오브젝트에 HandlingObject속성 추가  
            obj.AddComponent<HandlingObject>().Init(
                _okButton,
                _denyButton, 
                _cameraTransform,
                _takenAreaEvent,
                _gridEvent
            );
            selectedObject = obj.GetComponent<PlaceableObject>();
            selectedObject.Init();

            SetAllArea(true); //건물 다 색칠
            isFirst = false;
        }

        _placedObjects.Add(selectedObject);
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    //area는 범위, tilemap
    private static TileBase[] GetTileBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y];
        int count = 0;

        foreach (Vector3Int v in area.allPositionsWithin)
        {
            Debug.Log(v);
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[count] = tilemap.GetTile(pos);
            count++;
        }

        return array;
    }

    //타일이 비어있는지  
    public bool CheckTile(PlaceableObject ob, Vector3Int position)
    {
        //타일 베이스 [타일 가져오기]  
        for(int i = 0; i < ob.Size.z;i++)
        {
            for(int j = 0; j < ob.Size.x;j++)
            {
                TileBase b = mainTilemap.GetTile(position + new Vector3Int(j, i, 0));
                //b에 takenTile가 있다면???  
                if (b == _redTile)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary> 타일 채우는 함수</summary>
    /// <param name="startpos"></param>
    /// <param name="size"></param>
    private void TakenArea(Vector3Int startpos, Vector3Int size, bool isSelected)
    {
        this.startpos = startpos;
        this.object_size = size;
        TakenArea(isSelected);
    }
    
    /// <summary>
    /// 영역 설정하는 함수, 근데 너무 비효율적이지 않을까
    /// </summary>
    public void SetArea()
    {
        ClearArea();
        Vector3Int startpos = gridLayout.WorldToCell(selectedObject.GetStartPosition());
        TakenArea(startpos, selectedObject.Size, true);
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

    private void ClearArea()
    {
        if (isFirst)
        {
            isFirst = false;
            return;
        }
        TakenArea(false);
    }

    private void TakenArea(bool isSelected)
    {
        TileBase tile;
        if (isSelected)
        {
            tile = _takenTile;
        }
        else
        {
            tile = null;
        }
        for(int i = 0; i < object_size.z; i++)
        {
            for(int j = 0; j < object_size.x; j++)
            {
                //초록색인데 이미 초록색, 빨간색인 경우 => 빨간색 
                if (isSelected && mainTilemap.GetTile(startpos + new Vector3Int(j, i, 0)) != null)
                {
                    mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), _redTile);
                }
                //비어있는데 이미 빨간색인 경우 
                else if (!isSelected && mainTilemap.GetTile(startpos + new Vector3Int(j, i, 0)) == _redTile)
                {
                    mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), _takenTile);
                }
                else
                {
                    mainTilemap.SetTile(startpos + new Vector3Int(j, i, 0), tile);
                }
            }
        }
    }
    #endregion


}

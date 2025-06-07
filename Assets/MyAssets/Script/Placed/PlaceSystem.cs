
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class PlaceSystem : MonoBehaviour
{
    private List<PlaceableObject> _placedObjects = new List<PlaceableObject>();
    
    private GridLayout gridLayout;
    private Grid grid;

    //selected
    private PlaceableObject selectedObject;

    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private TileBase _takenTile;
    [SerializeField] private TileBase _redTile;
    [SerializeField] private Transform _objectParent;

    //
    private bool isFirst = true;
    private Vector3Int object_size;
    private Vector3Int startPos;

    private GameObject _okButton;
    private GameObject _denyButton;
    private GameObject _camera;
    
    [Space]
    [Header("ShopObjects")]
    [SerializeField] private PlaceableObject[] _shopPlaceableObjects;


    [Space]
    [Header("Manager")]
    [SerializeField] private PlacedObjectManager _placedObjectManager;

    [Space]
    [Header("Listening to Event")]
    [SerializeField] private VoidEventChannelSO _takenAreaEvent;
    [SerializeField] private Vector3TransformChannelSO _gridEvent;
    [SerializeField] private GameObjectEventChannelSO _setOkButtonEvent;
    [SerializeField] private GameObjectEventChannelSO _setDenyButtonEvent;
    [SerializeField] private GameObjectEventChannelSO _setCameraEvent;

    
    private void Start()
    {
        grid = GetComponent<Grid>();
        gridLayout = GetComponent<GridLayout>();

        AllCreatePlacedObjects();
    }

    private void OnEnable()
    {
        //_createEvent._onEventRaised += CreateObject;
        _takenAreaEvent._onEventRaised += SetArea;
        _gridEvent._onEventRaised += SnapCoordinateToGrid;
        
        _setOkButtonEvent._onEventRaised += SetOkButton;
        _setDenyButtonEvent._onEventRaised += SetDenyButton;
        _setCameraEvent._onEventRaised += SetCamera;
    }
    private void OnDisable()
    {
        //_createEvent._onEventRaised -= CreateObject;
        _takenAreaEvent._onEventRaised -= SetArea;
        _gridEvent._onEventRaised -= SnapCoordinateToGrid;
        
        _setOkButtonEvent._onEventRaised -= SetOkButton;
        _setDenyButtonEvent._onEventRaised -= SetDenyButton;
        _setCameraEvent._onEventRaised -= SetCamera;
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

    
    private void AllCreatePlacedObjects()
    {
        foreach(PlacedObjectData obj in _placedObjectManager.GetPlacedObjects())
        {
            PlaceObject(obj);
        }
    }
    
    private void PlaceObject(PlacedObjectData data)
    {
        Vector3Int startPos = gridLayout.WorldToCell(data.GetPosition());
        int id = data.GetID();
        int pid = data.GetPropertyID();
        //생성
        PlaceableObject obj = Instantiate(_shopPlaceableObjects[pid], _objectParent);
        obj.SetPlacedObjectData(data);
        obj.Place();

        _placedObjects.Add(obj);
    }


    
    //오브젝트 버튼 누르면 오브젝트 나오는 함수
    private void StartPlaceMode(GameObject obj)
    {
        //before selected object => current selected object
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }
        SetAllArea(true);
        CreateHandlingObject(obj);
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="building"></param>
    /// <param name="position">넣을 값 없으면 Vector3.zero</param>
    /// <param name="isHandling">손에 들고 있는 오브젝트인지</param>
    private void CreateHandlingObject(GameObject building)
    {
        //isFirst = true;

        //맨 처음 생성할때 0,0,0에 생성  => 마우스 위치에 생성으로 
        Vector3 position = SnapCoordinateToGrid(Vector3.zero);

        GameObject obj = Instantiate(building, position, Quaternion.identity);
        obj.transform.SetParent(_objectParent);

        PlaceableObject tmp = obj.GetComponent<PlaceableObject>();

        PlacedObjectData pod = new PlacedObjectData(
            _placedObjectManager.GetObjectID(),
            tmp.GetPropertyID(),
            position
        );

        tmp.SetPlacedObjectData(pod);

        //생성된 오브젝트에 HandlingObject속성 추가  
        obj.AddComponent<HandlingObject>().Init(
            _okButton,
            _denyButton,
            _camera,
            _takenAreaEvent,
            _gridEvent
        );
        selectedObject = tmp;

        //SetArea(); //지정된 건물만 색칠
        isFirst = false;

        
    }
    

    //핸들링한 건물 놓는 함수
    public void PlaceHandlingObject()
    {
        Vector3Int pos = gridLayout.WorldToCell(selectedObject.GetStartPosition());

        //겹치는 타일이 없으면
        if (CheckTile(selectedObject, pos))
        {
            selectedObject.Place();

            selectedObject.SetPosition(pos);

            startPos = pos;
            ClearArea();

            _placedObjectManager.SendPlaceableObject(selectedObject);
            _placedObjectManager.SetObjectID(selectedObject.GetObjectID() + 1);
            
            //배치 하는 순간 조종 권한 제거  
            Destroy(selectedObject.gameObject.GetComponent<HandlingObject>());
            selectedObject = null;
            
            _placedObjects.Add(selectedObject);
            TakeOffPlaceMode();
        }
        
        //아무일도 없다

    }

    public void TakeOffObject()
    {
        TakeOffPlaceMode();
        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    private void TakeOffPlaceMode()
    {
        //타일 지우기
        SetAllArea(false);

        //버튼 안 보이게
        _okButton.SetActive(false);
        _denyButton.SetActive(false);
    }


    #region TILE
    
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


    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    //area는 범위, tilemap
    // private static TileBase[] GetTileBlock(BoundsInt area, Tilemap tilemap)
    // {
    //     TileBase[] array = new TileBase[area.size.x * area.size.y];
    //     int count = 0;
    //
    //     foreach (Vector3Int v in area.allPositionsWithin)
    //     {
    //         Debug.Log(v);
    //         Vector3Int pos = new Vector3Int(v.x, v.y, 0);
    //         array[count] = tilemap.GetTile(pos);
    //         count++;
    //     }
    //
    //     return array;
    // }

    //타일이 비어있는지  
    public bool CheckTile(PlaceableObject ob, Vector3Int position)
    {
        //타일 베이스 [타일 가져오기]  
        for (int i = 0; i < ob.Size.z; i++)
        {
            for (int j = 0; j < ob.Size.x; j++)
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
        this.startPos = startpos;
        this.object_size = size;
        TakenArea(isSelected);
    }

    /// <summary>
    /// 영역 설정하는 함수, 드래그 할때마다 이 함수가 발동됨
    /// 비효율 적인데?
    /// </summary>
    public void SetArea()
    {
        ClearArea();
        Vector3Int startpos = gridLayout.WorldToCell(selectedObject.GetStartPosition());
        TakenArea(startpos, selectedObject.Size, true);
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
        for (int i = 0; i < object_size.z; i++)
        {
            for (int j = 0; j < object_size.x; j++)
            {
                //초록색인데 이미 초록색, 빨간색인 경우 => 빨간색 
                if (isSelected && mainTilemap.GetTile(startPos + new Vector3Int(j, i, 0)) != null)
                {
                    mainTilemap.SetTile(startPos + new Vector3Int(j, i, 0), _redTile);
                }
                //비어있는데 이미 빨간색인 경우 
                else if (!isSelected && mainTilemap.GetTile(startPos + new Vector3Int(j, i, 0)) == _redTile)
                {
                    mainTilemap.SetTile(startPos + new Vector3Int(j, i, 0), _takenTile);
                }
                else
                {
                    mainTilemap.SetTile(startPos + new Vector3Int(j, i, 0), tile);
                }
            }
        }
    }
    #endregion

    private void SetOkButton(GameObject okButton)
    {
        this._okButton = okButton;
    }

    private void SetDenyButton(GameObject denyButton)
    {
        this._denyButton = denyButton;
    }

    private void SetCamera(GameObject camera)
    {
        this._camera = camera;
    }
}

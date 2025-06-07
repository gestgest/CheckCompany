
using UnityEngine;
using UnityEngine.Tilemaps;


public class PlaceSystem : MonoBehaviour
{
    private GridLayout gridLayout;

    //selected
    private PlaceableObject selectedObject;
    private Grid grid;

    [SerializeField] private Tilemap mainTilemap;
    private TileBase _takenTile;
    private TileBase _redTile;
    Transform _objectParent;

    private Transform _cameraTransform;
    private GameObject _okButton;
    private GameObject _denyButton;

    private bool isFirst = true;
    private Vector3Int startpos;
    private Vector3Int object_size;

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

    private void Start()
    {
        gridLayout = GetComponent<Grid>();
    }

    private void OnEnable()
    {
        //_createEvent._onEventRaised += CreateObject;
        _takenAreaEvent._onEventRaised += SetArea;
        _gridEvent._onEventRaised += SnapCoordinateToGrid;
    }
    private void OnDisable()
    {
        //_createEvent._onEventRaised -= CreateObject;
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



    //오브젝트 버튼 누르면 오브젝트 나오는 함수
    private void StartPlaceMode(GameObject obj)
    {
        //선택된 오브젝트가 있다면
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }
        BuildingObject(obj, Vector3.zero);

        //모든 오브젝트 색칠
        SetAllArea(true);
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
    /// 자체 오브젝트를 생성하는 함수. 초기에 서버에서 가져온 오브젝트와 핸들링한 오브젝트를 설치하는 함수
    /// </summary>
    /// <param name="building"></param>
    /// <param name="position">넣을 값 없으면 Vector3.zero</param>
    /// <param name="isHandling">손에 들고 있는 오브젝트인지</param>
    private void BuildingObject(GameObject building, Vector3 position, bool isHandling = true)
    {
        isFirst = true;

        //맨 처음 생성할때 0,0,0에 생성  => 마우스 위치에 생성으로 
        position = SnapCoordinateToGrid(position);

        GameObject obj = Instantiate(building, position, Quaternion.identity);

        obj.transform.parent = _objectParent;

        PlaceableObject tmp = obj.GetComponent<PlaceableObject>();
        tmp.Init();

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
            selectedObject = tmp;

            //SetArea(); //지정된 건물만 색칠
            isFirst = false;
        }

        _placedObjects.Add(tmp);
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
        this.startpos = startpos;
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

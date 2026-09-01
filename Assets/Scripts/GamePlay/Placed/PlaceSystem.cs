
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;


//입력 감지기는 반드시 같은 오브젝트에 있어야 한다.
//예전에는 없으면 Awake에서 AddComponent로 붙였는데, 그러면 런타임에 생긴 컴포넌트라
//인스펙터에서 채워야 할 참조(_tapEvent 등)가 전부 null인 채로 돌아간다.
//RequireComponent로 바꿔서 항상 씬에 실제로 존재하게 만든다.
[RequireComponent(typeof(PlacedObjectInput))]
public class PlaceSystem : MonoBehaviour
{
    private List<PlaceableObject> _placedObjects = new List<PlaceableObject>();

    //이미 씬에 만들어 놓은 오브젝트의 id.
    //AllCreatePlacedObjects()가 Start()와 _onChangedEvent 양쪽에서 불리기 때문에,
    //서버 데이터를 다시 받아오면(재접속/새로고침) 같은 오브젝트를 한 번 더 만들게 된다.
    //복제본이 생기면 SetAllArea(true)가 같은 칸을 두 번 칠해서 전부 빨간 타일이 되고,
    //그 자리에는 아무것도 놓을 수 없게 된다.
    private readonly HashSet<int> _createdObjectIds = new HashSet<int>();
    
    private GridLayout gridLayout;
    private Grid grid;

    //소유한 사무실 범위. 같은 오브젝트에 붙어있고, 없으면 예전처럼 범위 제한 없이 동작한다.
    private OfficeArea _officeArea;

    //selected
    private PlaceableObject selectedObject;

    [SerializeField] private Tilemap mainTilemap;
    [SerializeField] private TileBase _takenTile;
    [SerializeField] private TileBase _redTile;
    [SerializeField] private Transform _objectParent;

    
    private bool isFirst = true;
    private Vector3Int object_size;
    private Vector3Int startPos;

    //이동(move) 모드 상태. 새로 만드는 배치와 달리, 취소하면 파괴가 아니라 원래 자리로 되돌려야 한다.
    private bool _isMoving;
    private Vector3 _moveOriginPosition;

    //이동 중에 회전까지 했을 수 있으므로 취소하면 각도도 같이 되돌린다
    private int _moveOriginRotation;

    private GameObject _okButton;
    private GameObject _denyButton;
    private GameObject _deleteButton;
    private GameObject _rotateButton;
    private GameObject _camera;
    
    [Space]
    [Header("ShopObjects")]
    [SerializeField] private PlaceableObject[] _shopPlaceableObjects;


    [Space]
    [Header("Manager")]
    [SerializeField] private PlacedObjectManager _placedObjectManager;
    [SerializeField] private WorkstationManagerSO _workstationManagerSO;

    [Space]
    [Header("Listening to Event")]
    [SerializeField] private VoidEventChannelSO _takenAreaEvent;
    [SerializeField] private Vector3TransformChannelSO _gridEvent;
    [SerializeField] private GameObjectEventChannelSO _createPlaceableObjectEvent;

    [SerializeField] private VoidEventChannelSO _onChangedEvent;
    [SerializeField] private VoidEventChannelSO _okEvent;
    [SerializeField] private VoidEventChannelSO _denyEvent;
    [SerializeField] private VoidEventChannelSO _deleteEvent;
    [SerializeField] private VoidEventChannelSO _rotateEvent;

    [Header("Broadcasting on Events")]
    [SerializeField] private BoolEventChannelSO _isHandlingEvent;
    
    private void Awake()
    {
        //PlacedObjectInput과 같은 규칙 - 같은 오브젝트에 있으므로 채널 없이 직접 참조한다
        _officeArea = GetComponent<OfficeArea>();
    }

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

        _createPlaceableObjectEvent._onEventRaised += StartPlaceMode;

        _onChangedEvent._onEventRaised += AllCreatePlacedObjects;

        _okEvent._onEventRaised += PlaceHandlingObject;
        _denyEvent._onEventRaised += TakeOffObject;

        //삭제 채널이 비어있어도 배치/이동 자체는 동작해야 한다
        if (_deleteEvent != null)
        {
            _deleteEvent._onEventRaised += DeleteHandlingObject;
        }

        //회전 채널도 마찬가지 - 없으면 회전만 못 한다
        if (_rotateEvent != null)
        {
            _rotateEvent._onEventRaised += RotateHandlingObject;
        }
    }
    private void OnDisable()
    {
        //_createEvent._onEventRaised -= CreateObject;
        _takenAreaEvent._onEventRaised -= SetArea;
        _gridEvent._onEventRaised -= SnapCoordinateToGrid;

        _createPlaceableObjectEvent._onEventRaised -= StartPlaceMode;

        _onChangedEvent._onEventRaised -= AllCreatePlacedObjects;


        _okEvent._onEventRaised -= PlaceHandlingObject;
        _denyEvent._onEventRaised -= TakeOffObject;

        if (_deleteEvent != null)
        {
            _deleteEvent._onEventRaised -= DeleteHandlingObject;
        }

        if (_rotateEvent != null)
        {
            _rotateEvent._onEventRaised -= RotateHandlingObject;
        }
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
        List<PlacedObjectData> placedObjectDatas = _placedObjectManager.GetPlacedObjects();

        //PlacedObjectManager.Init()보다 먼저 불릴 수 있다
        if (placedObjectDatas == null)
        {
            return;
        }

        foreach(PlacedObjectData obj in placedObjectDatas)
        {
            PlaceObject(obj);
        }
    }
    
    private void PlaceObject(PlacedObjectData data)
    {
        int id = data.GetID();

        //이미 만들어 놓은 오브젝트다. 두 번 만들면 복제본이 겹쳐 쌓인다.
        if (!_createdObjectIds.Add(id))
        {
            return;
        }

        int pid = data.GetPropertyID();
        //생성
        PlaceableObject obj = Instantiate(_shopPlaceableObjects[pid], _objectParent);
        obj.SetPlacedObjectData(data);
        obj.Place();

        _placedObjects.Add(obj);
        _workstationManagerSO.RegisterWorkstation(obj);
    }


    
    //오브젝트 버튼 누르면 오브젝트 나오는 함수
    private void StartPlaceMode(GameObject obj)
    {
        if (!TryFetchHandlingProperties())
        {
            return;
        }

        _isMoving = false;
        _isHandlingEvent.RaiseEvent(true);
        //before selected object => current selected object
        if (selectedObject != null)
        {
            Destroy(selectedObject.gameObject);
            selectedObject = null;
        }
        CreateHandlingObject(obj);
    }
    
    
    /// <summary>지금 무언가를 손에 들고 있는지 (배치 중이거나 이동 중).</summary>
    public bool IsHandling => selectedObject != null;

    /// <summary>
    /// 이미 배치된 오브젝트를 다시 손에 들어 이동시킨다.
    /// 같은 오브젝트에 붙은 PlacedObjectInput이 롱프레스를 감지하면 직접 호출한다.
    /// </summary>
    public void StartMoveMode(PlaceableObject target)
    {
        //이미 무언가 들고 있거나 대상이 없으면 무시
        if (target == null || IsHandling)
        {
            return;
        }

        if (!TryFetchHandlingProperties())
        {
            return;
        }

        _isHandlingEvent.RaiseEvent(true);

        //이동하는 동안에는 배치 목록에서 빼야 한다.
        //안 빼면 SetAllArea(true)가 이 오브젝트의 옛 발자국을 칠하고,
        //CheckTile()이 그 빨간 타일 때문에 제자리에 다시 놓는 것조차 거부한다.
        _placedObjects.Remove(target);
        _workstationManagerSO.UnregisterWorkstation(target);

        //취소했을 때 돌아갈 자리와 각도
        _isMoving = true;
        _moveOriginPosition = target.transform.position;
        _moveOriginRotation = target.GetRotation();

        target.UnPlace();

        target.gameObject.AddComponent<HandlingObject>().Init(
            _okButton,
            _denyButton,
            _deleteButton,
            _rotateButton,
            _camera,
            _takenAreaEvent,
            _gridEvent,
            true //이미 배치된 오브젝트라 삭제할 수 있다
        );

        selectedObject = target;

        //isFirst가 true로 남아있으면 BeforeClearArea()가 계속 no-op이라
        //드래그하는 동안 지나간 타일이 지워지지 않고 자국으로 남는다
        isFirst = false;

        SetArea();
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
            _deleteButton,
            _rotateButton,
            _camera,
            _takenAreaEvent,
            _gridEvent,
            false //아직 놓지도 않은 오브젝트는 취소(deny)가 곧 삭제다
        );
        selectedObject = tmp;

        SetArea();
        isFirst = false;
    }
    

    //핸들링한 건물 놓는 함수 ok
    public void PlaceHandlingObject()
    {
        Vector3Int pos = gridLayout.WorldToCell(selectedObject.GetStartPosition());

        //겹치는 타일이 없으면
        if (CheckTile(selectedObject, pos))
        {
            selectedObject.Place();

            //pos는 CheckTile용 셀 좌표(Vector3Int)다. 여기 그대로 넘기면 Vector3로 암묵 변환되면서
            //"셀 인덱스"가 "월드 좌표"인 것처럼 저장된다 (예: 셀 (3,0,5) → 월드 (3,0,5), 실제로는 (6,0,10)이어야 함).
            //데이터에는 반드시 월드 좌표를 넣어야 한다.
            selectedObject.SetPosition(selectedObject.GetStartPosition());

            startPos = pos;
            BeforeClearArea();

            _placedObjectManager.SendPlaceableObject(selectedObject);

            //이동은 기존 id를 그대로 쓴다. 카운터를 올리면 id가 새고 서버 문서가 고아가 된다.
            //(SendPlaceableObject가 placeableObjects.<id>로 쓰므로 같은 id면 서버가 알아서 덮어쓴다)
            if (!_isMoving)
            {
                _placedObjectManager.SetObjectID(selectedObject.GetObjectID() + 1);

                //이번에 새로 놓은 오브젝트는 서버에만 쓰고 로컬 목록에는 없었다.
                //목록에 넣어야 나중에 데이터를 다시 받아왔을 때 상태가 어긋나지 않고,
                //id를 기억해둬야 그때 AllCreatePlacedObjects()가 이걸 또 만들지 않는다.
                _placedObjectManager.RegisterPlacedObjectData(selectedObject.GetPlacedObjectData());
                _createdObjectIds.Add(selectedObject.GetObjectID());
            }

            _isMoving = false;

            TakeOffPlaceMode();

            //배치 하는 순간 조종 권한 제거
            Destroy(selectedObject.gameObject.GetComponent<HandlingObject>());

            //selectedObject를 null로 비우기 전에 참조를 저장해둬야 한다
            //(이전 코드는 null을 비운 뒤 리스트에 추가해서 항상 null이 쌓이는 버그가 있었음)
            PlaceableObject placed = selectedObject;
            selectedObject = null;

            _placedObjects.Add(placed);
            _workstationManagerSO.RegisterWorkstation(placed);
        }
        
        //아무일도 없다
    }

    // rotate : 손에 든 오브젝트를 90도 돌린다
    public void RotateHandlingObject()
    {
        //_rotateEvent로도 들어오는 경로라 들고 있는 게 없을 수 있다
        if (selectedObject == null)
        {
            return;
        }

        selectedObject.Rotate();

        //돌면 차지하는 칸이 달라진다.
        //SetArea()의 BeforeClearArea()가 아직 "돌기 전" 발자국을 기억하고 있어서 그것부터 지우고 다시 칠한다.
        SetArea();
    }

    // delete : 이동중인(이미 배치돼 있던) 오브젝트를 월드/서버에서 완전히 지운다
    public void DeleteHandlingObject()
    {
        if (selectedObject == null)
        {
            return;
        }

        //이동 모드가 아니면 = 상점에서 막 꺼낸, 서버에 아직 없는 오브젝트다.
        //지울 서버 데이터가 없으므로 취소와 똑같이 처리한다.
        if (!_isMoving)
        {
            TakeOffObject();
            return;
        }

        PlaceableObject target = selectedObject;

        //StartMoveMode에서 이미 _placedObjects와 워크스테이션 풀에서 빼놨지만,
        //자리 배정은 "옮기는 중"일 수도 있어서 일부러 남겨둔 상태다.
        //이번엔 진짜로 없어지는 것이므로 앉아있던 직원을 일으켜 세운다.
        _workstationManagerSO.ReleaseSeatOf(target);

        //여기서는 서버 데이터만 지우고 파괴하면 된다.
        _placedObjectManager.RemovePlaceableObject(target.GetObjectID());
        _createdObjectIds.Remove(target.GetObjectID());

        //들고 있던 자리의 초록/빨강 타일과 버튼 정리
        TakeOffPlaceMode();

        selectedObject = null;
        _isMoving = false;

        Destroy(target.gameObject);
    }

    // deny
    public void TakeOffObject()
    {
        //_denyEvent로도 들어오는 경로라 들고 있는 게 없을 수 있다
        if (selectedObject == null)
        {
            return;
        }

        //이동 취소는 파괴가 아니라 원위치
        if (_isMoving)
        {
            RestoreMovedObject();
            return;
        }

        TakeOffPlaceMode();
        Destroy(selectedObject.gameObject);
        selectedObject = null;
    }

    /// <summary>이동을 취소하고 원래 자리에 다시 배치한다.</summary>
    private void RestoreMovedObject()
    {
        PlaceableObject moved = selectedObject;

        moved.transform.position = _moveOriginPosition;
        moved.SetRotation(_moveOriginRotation);

        TakeOffPlaceMode();

        //Place()가 HandlingObject를 제거하고 Placed를 다시 true로 만든다
        moved.Place();

        _placedObjects.Add(moved);
        _workstationManagerSO.RegisterWorkstation(moved);

        selectedObject = null;
        _isMoving = false;
    }

    private void TakeOffPlaceMode()
    {
        TakenArea(false); //delete handling object tile
        SetAllArea(false); //기존에 있는 타일 지우기

        //버튼 안 보이게
        _okButton.SetActive(false);
        _denyButton.SetActive(false);

        //삭제/회전 버튼은 씬에 연결 안 돼 있을 수도 있다
        if (_deleteButton != null)
        {
            _deleteButton.SetActive(false);
        }

        if (_rotateButton != null)
        {
            _rotateButton.SetActive(false);
        }

        _isHandlingEvent.RaiseEvent(false);
    }


    #region TILE
    /// <summary>
    /// 영역 설정하는 함수, 드래그 할때마다 이 함수가 발동됨
    /// 비효율 적인데?
    /// </summary>
    public void SetArea() //handling object drag
    {
        if (selectedObject == null)
        {
            return;
        }

        BeforeClearArea();
        SetAllArea(false);
        SetAllArea(true);
        
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
        //사무실 밖(아직 못 산 땅)에는 놓을 수 없다.
        //빨간 타일 검사만으로는 못 막는다 - 사무실 밖은 아무것도 안 놓여있어서 타일이 비어있기 때문이다.
        if (_officeArea != null && !_officeArea.Contains(position, ob.Size))
        {
            return false;
        }

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



    private void BeforeClearArea()
    {
        if (isFirst)
        {
            return;
        }
        TakenArea(false);
    }

    //single
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
                Vector3Int cell = startPos + new Vector3Int(j, i, 0);

                //사무실 밖은 CheckTile이 어차피 거부한다. 누르기 전에 빨갛게 보여줘야 왜 안 놓이는지 안다.
                bool isOutside = _officeArea != null && !_officeArea.Contains(cell);

                //초록색인데 이미 초록색, 빨간색인 경우 => 빨간색 
                if (isSelected && (isOutside || mainTilemap.GetTile(cell) != null))
                {
                    mainTilemap.SetTile(cell, _redTile);
                }
                //비어있는데 이미 빨간색인 경우 
                else if (!isSelected && mainTilemap.GetTile(cell) == _redTile)
                {
                    //사무실 밖이라 빨갛던 칸은 밑에 깔린 초록이 없다.
                    //되돌린답시고 초록을 칠하면 아무것도 없는 자리에 유령 타일이 남는다.
                    mainTilemap.SetTile(cell, isOutside ? null : _takenTile);
                }
                else
                {
                    mainTilemap.SetTile(cell, tile);
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// ok/deny 버튼과 카메라는 GamePlay 씬의 UIPlaceableObject가 PlacedObjectManager(SO)에 넣어준다.
    ///
    /// PlacedObjectManager는 ScriptableObject = 에셋이라 플레이를 껐다 켜도 값이 남는데,
    /// 그 값은 이미 파괴된 씬 오브젝트다(MissingReferenceException).
    /// 게다가 PlaceSystem.Start()와 UIPlaceableObject.Start()는 서로 다른 씬에 있어 실행 순서도 보장되지 않는다.
    /// 그래서 Start()에서 한 번 캐싱하면 안 되고, 실제로 쓰기 직전에 매번 다시 받아와야 한다.
    /// </summary>
    private bool TryFetchHandlingProperties()
    {
        SetHandlingPropertys();

        //삭제 버튼은 필수가 아니다 - 없으면 삭제만 못 할 뿐 배치/이동은 그대로 동작해야 한다
        if (_deleteButton == null)
        {
            Debug.LogWarning(
                "[PlaceSystem] 삭제 버튼이 연결되지 않았습니다. " +
                "GamePlay 씬 UIPlaceObject의 UIPlaceableObject에 DeleteObjectButton을 넣어주세요."
            );
        }

        //회전 버튼도 마찬가지
        if (_rotateButton == null)
        {
            Debug.LogWarning(
                "[PlaceSystem] 회전 버튼이 연결되지 않았습니다. " +
                "GamePlay 씬 UIPlaceObject의 UIPlaceableObject에 RotateObjectButton을 넣어주세요."
            );
        }

        //Unity의 == 는 "파괴된 오브젝트"도 null로 쳐주므로 죽은 참조까지 여기서 걸러진다
        if (_okButton == null || _denyButton == null || _camera == null)
        {
            Debug.LogError(
                "[PlaceSystem] ok/deny 버튼 또는 카메라를 아직 받지 못했습니다. " +
                "GamePlay 씬의 UIPlaceableObject가 PlacedObjectManager에 값을 넣어주는지 확인하세요."
            );
            return false;
        }

        return true;
    }

    private void SetHandlingPropertys()
    {
        this._okButton = _placedObjectManager.GetOkButton();
        this._denyButton = _placedObjectManager.GetDenyButton();
        this._deleteButton = _placedObjectManager.GetDeleteButton();
        this._rotateButton = _placedObjectManager.GetRotateButton();
        this._camera = _placedObjectManager.GetCamera();
    }

}

using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// "내 사무실"의 범위를 값 하나로 들고 있고, 바닥/벽/잠긴 타일을 전부 거기서 계산한다.
///
/// - Land   : 바닥 전체 = 최대 확장 범위. 여기서 더는 못 넓힌다
/// - Office : 지금 소유한 범위. 벽 2개가 이 경계에 선다
/// - Locked : Land - Office. 아직 못 산 땅. 평소엔 안 보이고, PlaceSystem이 오브젝트를
///            배치/이동하는 동안(_isHandlingEvent)에만 어두운 타일로 덮어서 "여기까지 넓힐 수 있다"를 보여준다
///            (건물을 만지지도 않았는데 주변이 계속 어두우면 어수선하다 - 만지는 동안만 보여준다)
///
/// 확장은 Office를 키우는 것이고, 바닥/벽/배치 가능 범위는 전부 여기서 파생되므로
/// 씬에서 벽을 손으로 옮길 필요가 없다.
///
/// PlaceSystem과 같은 오브젝트(Grid_PlaceSystem)에 붙는다.
/// LongPressSelector와 같은 방식으로 GetComponent로 직접 참조한다 - 서로 아는 사이에는 이벤트 채널이 낭비다.
/// </summary>
[RequireComponent(typeof(Grid))]
public class OfficeArea : MonoBehaviour
{
    [Header("Land - 바닥 전체(최대 확장 범위), 칸 단위")]
    [SerializeField] private Vector2Int _landOriginCell = new Vector2Int(-10, -10);
    [SerializeField] private Vector2Int _landSizeInTiles = new Vector2Int(20, 20);

    [Header("Office - 지금 소유한 범위, 칸 단위")]
    [SerializeField] private Vector2Int _officeOriginCell = new Vector2Int(-5, -5);
    [SerializeField] private Vector2Int _officeSizeInTiles = new Vector2Int(10, 10);

    [Header("Scene")]
    [SerializeField] private Transform _floor;

    //y로 90도 돌아가 있는 벽. x가 가장 작은 모서리에 서서 z축을 따라 뻗는다.
    [SerializeField] private Transform _wallAlongZ;

    //돌아가지 않은 벽. z가 가장 작은 모서리에 서서 x축을 따라 뻗는다.
    [SerializeField] private Transform _wallAlongX;

    [SerializeField] private float _wallHeight = 11f;
    [SerializeField] private float _wallThickness = 1f;

    [Header("Locked Tiles")]
    //배치용 타일맵(PlaceSystem.mainTilemap)과 반드시 다른 타일맵이어야 한다.
    //배치 중에는 매 프레임 그쪽 타일을 지웠다 다시 칠하기 때문에, 같이 쓰면 잠긴 타일이 같이 지워진다.
    [SerializeField] private Tilemap _lockedTilemap;
    [SerializeField] private TileBase _lockedTile;

    [Header("Listening to Event")]
    //PlaceSystem이 오브젝트를 배치/이동하는 동안 true, 끝나면 false로 쏘는 채널.
    //PlaceSystem 인스펙터에 물린 것과 같은 에셋을 여기도 그대로 물리면 된다.
    [SerializeField] private BoolEventChannelSO _isHandlingEvent;

    //지금 잠긴 타일을 보여주는 중인지. Apply()가 Expand() 도중에도 이 상태를 유지해서 갱신한다.
    private bool _lockedTilesVisible;

    private Grid _grid;

    /// <summary>지금 소유한 칸의 범위. z축은 셀 좌표의 y로 들어간다 (CellSwizzle이 XZY).</summary>
    public RectInt OfficeCells => new RectInt(_officeOriginCell, _officeSizeInTiles);

    /// <summary>바닥 전체(최대 확장 범위)의 칸 범위.</summary>
    public RectInt LandCells => new RectInt(_landOriginCell, _landSizeInTiles);

    private void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    private void OnEnable()
    {
        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised += SetLockedTilesVisible;
        }
    }

    private void OnDisable()
    {
        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised -= SetLockedTilesVisible;
        }
    }

    private void Start()
    {
        //_lockedTilesVisible은 항상 false로 시작하지만, 에디터에서 미리 칠해둔 타일이
        //씬에 남아있을 수 있으니 확실히 지우고 시작한다.
        ClearLockedTiles();
        Apply();
    }

    /// <summary>셀 하나가 소유 범위 안인지.</summary>
    public bool Contains(Vector3Int cell)
    {
        RectInt r = OfficeCells;
        return cell.x >= r.xMin && cell.x < r.xMax
            && cell.y >= r.yMin && cell.y < r.yMax;
    }

    /// <summary>
    /// 시작 모서리 셀에서 size만큼 차지하는 발자국이 통째로 소유 범위 안인지.
    /// size는 PlaceableObject.Size와 같은 규칙 - x가 가로 칸 수, z가 세로 칸 수다.
    /// </summary>
    public bool Contains(Vector3Int startCell, Vector3Int size)
    {
        RectInt r = OfficeCells;
        return startCell.x >= r.xMin && startCell.x + size.x <= r.xMax
            && startCell.y >= r.yMin && startCell.y + size.z <= r.yMax;
    }

    /// <summary>
    /// 소유 범위를 넓힌다. Land를 넘어서면 Land까지만 늘어난다.
    /// 확장 버튼이 붙으면 여기를 부르면 된다.
    /// </summary>
    public void Expand(Vector2Int delta)
    {
        RectInt land = LandCells;

        //원점은 늘어난 만큼 절반씩 뒤로 물러난다 - 사무실이 한쪽으로만 자라면 벽이 한쪽으로 쏠린다
        Vector2Int origin = _officeOriginCell - new Vector2Int(delta.x / 2, delta.y / 2);
        Vector2Int size = _officeSizeInTiles + delta;

        size.x = Mathf.Clamp(size.x, 1, land.width);
        size.y = Mathf.Clamp(size.y, 1, land.height);

        origin.x = Mathf.Clamp(origin.x, land.xMin, land.xMax - size.x);
        origin.y = Mathf.Clamp(origin.y, land.yMin, land.yMax - size.y);

        _officeOriginCell = origin;
        _officeSizeInTiles = size;

        Apply();
    }

    /// <summary>
    /// 바닥, 벽을 지금 값에 맞춘다. 잠긴 타일은 지금 보여주는 중일 때만(SetLockedTilesVisible(true) 상태) 같이 갱신한다.
    /// </summary>
    [ContextMenu("사무실 범위 적용")]
    public void Apply()
    {
        if (_grid == null)
        {
            _grid = GetComponent<Grid>();
        }

        PlaceFloor();
        PlaceWalls();

        if (_lockedTilesVisible)
        {
            PaintLockedTiles();
        }
    }

    /// <summary>
    /// 확장 가능 범위(잠긴 타일)를 보여줄지 정한다. 평소엔 꺼둔 채로 시작한다 -
    /// 아직 건드리지도 않은 땅을 계속 어둡게 깔아두면 어수선하다.
    /// _isHandlingEvent가 이 메서드를 그대로 구독한다 (오브젝트 배치/이동 중일 때만 true).
    /// </summary>
    public void SetLockedTilesVisible(bool visible)
    {
        _lockedTilesVisible = visible;

        if (visible)
        {
            PaintLockedTiles();
        }
        else
        {
            ClearLockedTiles();
        }
    }

    //CellSwizzle이 XZY라 cellSize.y가 월드 z축 길이다
    private float CellWidth => _grid.cellSize.x;
    private float CellDepth => _grid.cellSize.y;

    private void PlaceFloor()
    {
        if (_floor == null)
        {
            return;
        }

        RectInt land = LandCells;

        //y는 건드리지 않는다. 바닥은 두께 1짜리 큐브를 y -0.5에 둬서 윗면이 딱 y=0에 오게 맞춰져 있다
        Vector3 scale = _floor.localScale;
        _floor.localScale = new Vector3(land.width * CellWidth, scale.y, land.height * CellDepth);

        Vector3 pos = _floor.position;
        _floor.position = new Vector3(
            (land.xMin + land.width * 0.5f) * CellWidth,
            pos.y,
            (land.yMin + land.height * 0.5f) * CellDepth);
    }

    private void PlaceWalls()
    {
        RectInt office = OfficeCells;

        float minX = office.xMin * CellWidth;
        float minZ = office.yMin * CellDepth;
        float centerX = (office.xMin + office.width * 0.5f) * CellWidth;
        float centerZ = (office.yMin + office.height * 0.5f) * CellDepth;

        //벽 밑동이 바닥 큐브 속으로 0.5 들어가게 둔다 (기존 씬과 같은 규칙)
        float wallY = _wallHeight * 0.5f - 0.5f;

        if (_wallAlongZ != null)
        {
            //90도 돌아가 있으므로 큐브의 x변(= localScale.x)이 월드 z축을 향한다
            _wallAlongZ.localScale = new Vector3(office.height * CellDepth, _wallHeight, _wallThickness);
            _wallAlongZ.position = new Vector3(minX, wallY, centerZ);
            _wallAlongZ.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        if (_wallAlongX != null)
        {
            _wallAlongX.localScale = new Vector3(office.width * CellWidth, _wallHeight, _wallThickness);
            _wallAlongX.position = new Vector3(centerX, wallY, minZ);
            _wallAlongX.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 아직 못 산 땅을 어두운 타일로 덮는다.
    /// 소유 범위가 커지면 그만큼 타일이 걷히므로, 넓힐 때마다 Land 전체를 다시 칠한다.
    /// </summary>
    private void PaintLockedTiles()
    {
        if (_lockedTilemap == null || _lockedTile == null)
        {
            return;
        }

        RectInt land = LandCells;

        for (int z = land.yMin; z < land.yMax; z++)
        {
            for (int x = land.xMin; x < land.xMax; x++)
            {
                Vector3Int cell = new Vector3Int(x, z, 0);
                _lockedTilemap.SetTile(cell, Contains(cell) ? null : _lockedTile);
            }
        }
    }

    /// <summary>잠긴 타일을 전부 지운다. SetLockedTilesVisible(false)가 부른다.</summary>
    private void ClearLockedTiles()
    {
        if (_lockedTilemap == null)
        {
            return;
        }

        RectInt land = LandCells;

        for (int z = land.yMin; z < land.yMax; z++)
        {
            for (int x = land.xMin; x < land.xMax; x++)
            {
                _lockedTilemap.SetTile(new Vector3Int(x, z, 0), null);
            }
        }
    }
}

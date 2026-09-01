using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//건물의 타일맵 정보를 담는 클래스
//VertexLocalPosition()이 BoxCollider로 타일 크기를 계산하므로 반드시 같이 있어야 한다.
[RequireComponent(typeof(BoxCollider))]
public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    
    private Vector3[] vertices;

    
    //PlacedObjectData 대체
    private int id;
    [SerializeField] private PlaceableObjectSO _placeableObjectSO;
    
    [Header("Workstation")]
    [SerializeField] private bool _isWorkstation;
    [SerializeField] private Transform _seatPoint;

    public bool IsWorkstation => _isWorkstation;

    //상점 가구가 수백 개로 늘어나면 프리팩마다 손으로 끌어다 넣는 것은 불가능하므로,
    //이름 규칙만 지키면 에디터에서 자동으로 채워준다.
    private const string SeatPointName = "SeatPoint";

    //타일 격자에 칸 단위로 맞춰야 하므로 90도 단위로만 돈다.
    private const int RotationStep = 90;

    //차지하는 칸을 가로/세로로 한 칸씩 더 잡는다 (여유 공간).
    private const int TilePadding = 1;

    //타일 한 칸의 월드 크기. Grid의 CellSwizzle이 XZY라서 cellSize.y가 월드 z축 길이다.
    private Vector2 _cellSize = Vector2.zero;

#if UNITY_EDITOR
    //컴포넌트를 처음 붙였을 때 1회
    private void Reset()
    {
        AutoAssignReferences();
    }

    //인스펙터에서 값이 바뀌거나 프리팩을 열 때마다
    private void OnValidate()
    {
        AutoAssignReferences();

        if (_isWorkstation && _seatPoint == null)
        {
            Debug.LogWarning(
                $"[PlaceableObject] '{name}' : 워크스테이션인데 '{SeatPointName}' 자식이 없습니다. " +
                "직원이 오브젝트 중심(NavMesh가 없는 곳)으로 가려고 합니다.",
                this
            );
        }
    }
#endif

    /// <summary>비어있는 참조를 이름/타입으로 찾아 채운다. 이미 들어있는 값은 건드리지 않는다.</summary>
    private void AutoAssignReferences()
    {
        if (_seatPoint == null)
        {
            _seatPoint = transform.Find(SeatPointName);
        }
    }

    /// <summary>직원이 근무할 위치. 지정하지 않으면 오브젝트 자신의 Transform을 사용한다.</summary>
    public Transform GetSeatPoint()
    {
        return _seatPoint != null ? _seatPoint : transform;
    }


    //init와 동일
    public void SetPlacedObjectData(PlacedObjectData placedObjectData)
    {
        id = placedObjectData.GetID();

        transform.position = placedObjectData.GetPosition();

        //회전을 Init()보다 먼저 적용해야 칸 수 계산이 "돌아간 뒤"의 가로/세로를 본다
        transform.rotation = Quaternion.Euler(0f, placedObjectData.GetRotation(), 0f);

        Init();
    }

    /// <summary>현재 y축 회전(도). 항상 0/90/180/270 중 하나다.</summary>
    public int GetRotation()
    {
        return NormalizeDegrees(Mathf.RoundToInt(transform.eulerAngles.y / RotationStep) * RotationStep);
    }

    /// <summary>90도 돌린다. 배치/이동 중 회전 버튼이 부르는 함수.</summary>
    public void Rotate()
    {
        SetRotation(GetRotation() + RotationStep);
    }

    /// <summary>주어진 각도로 돌리고 차지하는 칸 수를 다시 센다.</summary>
    public void SetRotation(int degrees)
    {
        degrees = NormalizeDegrees(degrees);

        transform.rotation = Quaternion.Euler(0f, degrees, 0f);

        //돌면 가로/세로가 뒤바뀌므로 칸 수를 다시 세야 한다
        CalculateTileSize();

        //서버에 보낼 데이터도 같이 맞춰둔다
        //todo 서버
    }

    /// <summary>0 이상 360 미만의 90도 배수로 정리한다. (-90 -> 270, 360 -> 0)</summary>
    private static int NormalizeDegrees(int degrees)
    {
        return ((degrees % 360) + 360) % 360;
    }

    /// <summary>90도나 270도로 돌아가 있어서 가로/세로가 바뀐 상태인지.</summary>
    private bool IsQuarterTurned()
    {
        return GetRotation() % 180 != 0;
    }

    private void Init()
    {
        VertexLocalPosition();
        CalculateTileSize();
    }
    /// <summary>
    /// 배치를 풀고 다시 "손에 든" 상태로 되돌린다 (이동 모드 진입).
    /// Placed를 내려야 PlacedObjectInput이 지금 들고 있는 오브젝트를 또 잡지 않는다.
    /// </summary>
    public void UnPlace()
    {
        Placed = false;
    }

    /// <summary> 손에 있는 selectedObject 제거 </summary>
    public virtual void Place()
    {
        HandlingObject drag = gameObject.GetComponent<HandlingObject>();
        Destroy(drag);

        Placed = true;
    }

    //위치를 Vertex에 넣는 함수  
    public void VertexLocalPosition()
    {
        BoxCollider box = gameObject.GetComponent<BoxCollider>();

        //RequireComponent가 붙기 전에 만든 프리팩은 없을 수 있다 - NRE 대신 원인을 알려준다
        if (box == null)
        {
            Debug.LogError($"[PlaceableObject] '{name}' : BoxCollider가 없어 타일 크기를 계산할 수 없습니다.", this);
            vertices = new Vector3[4];
            return;
        }

        vertices = new Vector3[4];

        //정육면체 아래 면의 사각형의 포지션  0.74, 2
        vertices[0] = new Vector3(-box.size.x, -box.size.y, -box.size.z) * 0.5f + box.center;
        vertices[1] = new Vector3(box.size.x, -box.size.y, -box.size.z) * 0.5f + box.center;
        vertices[2] = new Vector3(box.size.x, -box.size.y, box.size.z) * 0.5f + box.center;
        vertices[3] = new Vector3(-box.size.x, -box.size.y, box.size.z) * 0.5f + box.center;

    }

    /// <summary>
    /// 콜라이더 크기를 "몇 칸"인지로 환산한다.
    ///
    /// vertices는 회전하지 않은 로컬 좌표라 스케일을 곱해야 월드 길이가 되고,
    /// 90/270도로 돌면 콜라이더의 x변이 월드 z축을 향하므로 두 변을 바꿔서 센다.
    /// (타일은 언제나 월드 축 기준으로 칠해진다)
    /// </summary>
    private void CalculateTileSize()
    {
        Vector3 scale = transform.lossyScale;

        float width = Mathf.Abs(vertices[0].x - vertices[1].x) * Mathf.Abs(scale.x);
        float depth = Mathf.Abs(vertices[0].z - vertices[3].z) * Mathf.Abs(scale.z);

        if (IsQuarterTurned())
        {
            float turned = width;
            width = depth;
            depth = turned;
        }

        Vector2 cellSize = GetCellSize();

        //한 칸이라도 걸치면 그 칸은 차지한 것으로 본다.
        //거기에 한 칸씩 더 준다 - 딱 맞게 잡으면 오브젝트끼리 붙어버려서
        //애초부터 프리팹 크기보다 넉넉하게 자리를 잡아주도록 되어 있었다.
        int x = Mathf.Max(1, Mathf.CeilToInt(width / cellSize.x)) + TilePadding;
        int y = Mathf.Max(1, Mathf.CeilToInt(depth / cellSize.y)) + TilePadding;

        Size = new Vector3Int(x, 0, y);
        /*
        Vector3Int[] verticesInt = new Vector3Int[vertices.Length];

        for (int i = 0; i < verticesInt.Length; i++)
        {
            
            Vector3 worldpos = transform.TransformPoint(vertices[i]);
            //Debug.Log(worldpos);
            //타일맵 기준  
            verticesInt[i] = BuildingSystem.instance.gridLayout.WorldToCell(worldpos);
        }

        int x = (int)Mathf.Abs(verticesInt[0].x - verticesInt[1].x);

        //Debug.Log("엄x : " + verticesInt[0].x + " "+ verticesInt[3].x);
        //Debug.Log("엄z : " + verticesInt[0].z + " "+ verticesInt[3].z);
        int y = (int)Mathf.Abs(verticesInt[0].z - verticesInt[3].z);  
        */

    }

    /// <summary>
    /// 타일 한 칸의 월드 크기. 씬에 Grid는 PlaceSystem이 붙은 것 하나뿐이라 거기서 읽는다.
    /// 회전할 때마다 찾지 않도록 한 번 찾으면 캐싱한다.
    /// </summary>
    private Vector2 GetCellSize()
    {
        if (_cellSize != Vector2.zero)
        {
            return _cellSize;
        }

        Grid grid = FindFirstObjectByType<Grid>();

        if (grid == null)
        {
            Debug.LogWarning($"[PlaceableObject] '{name}' : 씬에서 Grid를 찾지 못해 한 칸을 1로 봅니다.", this);
            _cellSize = Vector2.one;
        }
        else
        {
            //CellSwizzle이 XZY라 cellSize.y가 월드 z축 길이다
            _cellSize = new Vector2(grid.cellSize.x, grid.cellSize.y);
        }

        return _cellSize;
    }

    /// <summary>
    /// 타일을 칠하기 시작할 모서리 = 월드 기준으로 x, z가 가장 작은 꼭짓점.
    ///
    /// 칠하는 쪽(PlaceSystem.TakenArea)이 여기서부터 +x, +z 방향으로만 훑기 때문에
    /// 반드시 최소 모서리여야 한다. 회전하면 vertices[0]은 더 이상 그 모서리가 아니라서
    /// 네 꼭짓점을 다 보고 고른다. (회전이 0도면 예전과 똑같이 vertices[0]이 나온다)
    /// </summary>
    public Vector3 GetStartPosition()
    {
        Vector3 start = transform.TransformPoint(vertices[0]);

        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 corner = transform.TransformPoint(vertices[i]);

            start.x = Mathf.Min(start.x, corner.x);
            start.z = Mathf.Min(start.z, corner.z);
        }

        return start;
    }

    public Dictionary<string, object> ObjectToJSON()
    {
        Dictionary<string, object> pos = new Dictionary<string, object>()
        {
            { "x", GetStartPosition().x },
            { "y", GetStartPosition().y },
            { "z", GetStartPosition().z },
            
        };

        Dictionary<string, object> result = new Dictionary<string, object>
        {
            {"startPosition", pos},
            {"property_id", _placeableObjectSO.GetID()},
            {"rotation", GetRotation()},
        };
        return result;
    }
    public int GetObjectID()
    {
        return id;
    }
    public int GetPropertyID()
    {
        return _placeableObjectSO.GetID();
    }
}

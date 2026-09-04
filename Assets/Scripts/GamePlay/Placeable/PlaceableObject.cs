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
    
    /// <summary>
    /// 직원을 배정하는 단위. 자리는 컴퓨터 한 대가 하나다
    /// (컴퓨터가 올라간 책상 + 그 책상에 붙은 의자까지 갖춰져야 실제로 앉을 수 있다 -
    /// WorkstationManagerSO.IsReadyForWork 참고).
    ///
    /// 예전에는 프리팹마다 체크하는 bool이었다. 켜져 있는 건 컴퓨터 하나뿐인데도
    /// 화분까지 전부 그 필드를 달고 다녔고, 쓰는 쪽은 어차피 Type == Computer를 같이 보고 있어서
    /// 둘이 어긋날 여지만 있었다.
    /// </summary>
    public bool IsWorkstation => Type == ObjectType.Computer;

    /// <summary>상점 카테고리이자 배치 규칙의 기준. SO가 안 꽂혀 있으면 Etc로 본다.</summary>
    public ObjectType Type => _placeableObjectSO != null ? _placeableObjectSO.GetObjectType() : ObjectType.Etc;

    //타일 격자에 칸 단위로 맞춰야 하므로 90도 단위로만 돈다.
    private const int RotationStep = 90;


    //타일 한 칸의 월드 크기. Grid의 CellSwizzle이 XZY라서 cellSize.y가 월드 z축 길이다.
    private Vector2 _cellSize = Vector2.zero;

    //씬에 하나뿐인 Grid. 칸 좌표 계산에 매번 필요해서 캐싱한다.
    private Grid _grid;

    //init와 동일
    public void SetPlacedObjectData(PlacedObjectData placedObjectData)
    {
        id = placedObjectData.GetID();

        //회전을 Init()보다 먼저 적용해야 칸 수 계산이 "돌아간 뒤"의 가로/세로를 본다
        transform.rotation = Quaternion.Euler(0f, placedObjectData.GetRotation(), 0f);

        Init();

        //서버에 저장되는 값은 피벗이 아니라 '시작 모서리'(ObjectToJSON이 GetStartPosition을 쓴다).
        //예전에는 그 값을 transform.position에 그대로 넣어서, 새로고침할 때마다 오브젝트가
        //콜라이더 모서리만큼(책상이면 한 칸 가까이) 밀렸다.
        //모서리가 저장된 자리에 오도록 피벗을 역산해서 넣는다.
        transform.position += placedObjectData.GetPosition() - GetStartPosition();
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
        int x = Mathf.Max(1, Mathf.CeilToInt(width / cellSize.x));
        int y = Mathf.Max(1, Mathf.CeilToInt(depth / cellSize.y));

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

        Grid grid = GetGrid();

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

    /// <summary>씬의 Grid는 PlaceSystem이 붙은 것 하나뿐이다. 매번 찾지 않도록 캐싱한다.</summary>
    private Grid GetGrid()
    {
        if (_grid == null)
        {
            _grid = FindFirstObjectByType<Grid>();
        }

        return _grid;
    }

    /// <summary>
    /// 이 오브젝트가 차지하는 칸 범위. 타일맵에 실제로 칠해지는 범위와 같다
    /// (= 시작 모서리에서 Size만큼, Size에는 여유 한 칸이 포함되어 있다).
    ///
    /// 칸 좌표는 Grid의 CellSwizzle이 XZY라 y가 월드 z축이다.
    /// "책상 위인지", "의자가 붙어있는지" 같은 판정은 전부 이 범위로 한다 -
    /// 플레이어가 화면에서 보는 초록 타일과 정확히 같은 범위여야 납득이 되기 때문이다.
    /// </summary>
    public RectInt GetCellRect()
    {
        Grid grid = GetGrid();

        if (grid == null)
        {
            return new RectInt(0, 0, 0, 0);
        }

        Vector3Int start = grid.WorldToCell(GetStartPosition());
        return new RectInt(start.x, start.y, Size.x, Size.z);
    }

    /// <summary>콜라이더 윗면의 월드 Y. 이 위에 다른 오브젝트(컴퓨터)를 올릴 때 기준이 된다.</summary>
    public float GetTopY()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
        {
            return transform.position.y;
        }

        float scaleY = Mathf.Abs(transform.lossyScale.y);
        return transform.position.y + (box.center.y + box.size.y * 0.5f) * scaleY;
    }

    /// <summary>transform.position.y에서 콜라이더 밑면까지의 거리. 윗면에 딱 맞춰 올릴 때 쓴다.</summary>
    public float GetBottomOffset()
    {
        BoxCollider box = GetComponent<BoxCollider>();

        if (box == null)
        {
            return 0f;
        }

        float scaleY = Mathf.Abs(transform.lossyScale.y);
        return (box.center.y - box.size.y * 0.5f) * scaleY;
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

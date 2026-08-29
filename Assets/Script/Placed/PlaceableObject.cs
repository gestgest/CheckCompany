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

    
    [SerializeField] private PlacedObjectData _placedObjectData;
    // private int object_id;
    // [SerializeField] private int property_id;

    [Header("Workstation")]
    [SerializeField] private bool _isWorkstation;
    [SerializeField] private Transform _seatPoint;

    public bool IsWorkstation => _isWorkstation;

    //상점 가구가 수백 개로 늘어나면 프리팩마다 손으로 끌어다 넣는 것은 불가능하므로,
    //이름 규칙만 지키면 에디터에서 자동으로 채워준다.
    private const string SeatPointName = "SeatPoint";

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

    public PlacedObjectData GetPlacedObjectData()
    {
        return _placedObjectData;
    }

    public void SetPlacedObjectData(PlacedObjectData placedObjectData)
    {
        _placedObjectData = placedObjectData;

        transform.position = placedObjectData.GetPosition();
        Init();
    }

    private void Init()
    {
        VertexLocalPosition();
        CalculateTileSize();
    }
    /// <summary>
    /// 배치를 풀고 다시 "손에 든" 상태로 되돌린다 (이동 모드 진입).
    /// Placed를 내려야 LongPressSelector가 지금 들고 있는 오브젝트를 또 잡지 않는다.
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

    //Vertex를 계산해서 타일 사이즈 측정  
    private void CalculateTileSize()
    {

        int x = (int)Mathf.Abs(vertices[0].x - vertices[1].x) * 2 + 1; //셀 사이즈
        int y = (int)Mathf.Abs(vertices[0].z - vertices[3].z) + 1;

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

    public Vector3 GetStartPosition()
    {
        // if (vertices == null)
        //     Init();
        return transform.TransformPoint(vertices[0]); //왜 이게 문제일까
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
            {"property_id", _placedObjectData.GetPropertyID()},
        };
        return result;
    }
    public int GetObjectID()
    {
        if (_placedObjectData == null)
        {
            Debug.LogWarning($"[PlaceableObject] '{name}' : _placedObjectData가 없습니다 (SetPlacedObjectData 호출 필요).", this);
            return -1;
        }

        return _placedObjectData.GetID();
    }
    public int GetPropertyID()
    {
        if (_placedObjectData == null)
        {
            Debug.LogWarning($"[PlaceableObject] '{name}' : _placedObjectData가 없습니다 (SetPlacedObjectData 호출 필요).", this);
            return -1;
        }

        return _placedObjectData.GetPropertyID();
    }
    public void SetPosition(Vector3 position)
    {
        _placedObjectData.SetPosition(position);
    }
}

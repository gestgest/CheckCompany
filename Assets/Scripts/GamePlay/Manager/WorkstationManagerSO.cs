using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배치된 오브젝트(PlaceableObject) 중 IsWorkstation == true 인 것들을 "책상 풀"로 관리하고,
/// 직원(employeeId) 단위로 자리를 배정/반납한다.
///
/// 배정에는 두 갈래가 있다.
/// - RequestSeat  : 직원이 알아서 빈 자리를 집어간다 (EmployeeWorkAI가 쓴다)
/// - AssignEmployee : 플레이어가 "이 자리에 이 직원"을 직접 꽂는다 (자리를 눌러서 뜨는 UI가 쓴다)
/// 둘은 같은 표를 보므로, 손으로 꽂아둔 자리를 다른 직원이 자동으로 집어가는 일은 없다.
/// </summary>
[CreateAssetMenu(fileName = "WorkstationManagerSO", menuName = "ScriptableObject/Manager/WorkstationManagerSO")]
public class WorkstationManagerSO : ScriptableObject
{
    /// <summary>아무도 앉아있지 않은 자리를 나타내는 직원 id.</summary>
    public const int NoEmployee = -1;

    //필드 초기화자로 기본값을 넣어둔다 - Init()이 아직 호출되기 전에(씬/스크립트 실행 순서에 따라
    //EmployeeWorkAI 등이 먼저 접근하는 경우) NullReferenceException이 나지 않도록 하기 위함.
    private List<PlaceableObject> _workstations = new List<PlaceableObject>();

    //배치된 오브젝트 전부(책상/의자/컴퓨터/장식). 워크스테이션만으로는
    //"컴퓨터 밑에 책상이 있는지", "그 책상에 의자가 붙어있는지"를 볼 수 없어서 따로 들고 있는다.
    //PlaceSystem이 Register/Unregister를 부르는 시점이 곧 배치/해제 시점이라 여기서 같이 관리한다.
    private List<PlaceableObject> _placedObjects = new List<PlaceableObject>();

    //employeeId -> 배정된 워크스테이션
    private Dictionary<int, PlaceableObject> _employeeSeats = new Dictionary<int, PlaceableObject>();

    //워크스테이션의 PlacedObjectData id -> 거기 앉은 직원 id.
    //예전에는 HashSet으로 "찼는지"만 봤는데, 자리를 눌렀을 때 누가 앉아있는지 보여주려면
    //반대 방향도 알아야 해서 표로 바꿨다. 중복 배정 방지도 이 표가 겸한다.
    private Dictionary<int, int> _seatOwners = new Dictionary<int, int>();

    //퇴근/출근할 때 걸어나가고 걸어들어올 회사 출입구들. Door 타입 오브젝트도 다른 배치물처럼
    //PlaceSystem이 RegisterWorkstation/UnregisterWorkstation을 불러줄 때 같이 채워진다
    //(워크스테이션 풀과 완전히 같은 방식 - 아래 두 함수 참고).
    private List<PlaceableObject> _doors = new List<PlaceableObject>();

    public void Init()
    {
        _workstations = new List<PlaceableObject>();
        _placedObjects = new List<PlaceableObject>();
        _doors = new List<PlaceableObject>();
        _employeeSeats = new Dictionary<int, PlaceableObject>();
        _seatOwners = new Dictionary<int, int>();
    }

    /// <summary>
    /// 출퇴근길에 오갈 출입구. 놓인 문이 하나도 없으면 null(그 자리에서 그냥 출퇴근 처리 -
    /// EmployeeWorkAI가 이미 이 경우를 예전 방식으로 처리해준다).
    /// 문을 여러 개 놓을 수 있으므로 fromPosition에서 가장 가까운 문을 고른다 -
    /// 직원이 자기가 있는 자리와 동떨어진 반대쪽 문으로 굳이 걸어가지 않게 하기 위함.
    ///
    /// Transform이 아니라 PlaceableObject를 돌려준다. 부르는 쪽에서 목적지(transform.position)뿐 아니라
    /// 여닫을 문(Door.Of)까지 같이 필요하기 때문이다.
    /// 문 앞을 지나는지 보려고 매 프레임 불리는 자리라(EmployeeWorkAI.TickDoor) 할당 없이 훑기만 한다.
    /// </summary>
    public PlaceableObject GetNearestDoor(Vector3 fromPosition)
    {
        PlaceableObject nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (PlaceableObject door in _doors)
        {
            //파괴됐거나(잔여 참조) 아직 배치 확정 전(id 없음)인 문은 목적지로 쓸 수 없다
            if (door == null)
            {
                continue;
            }

            float sqrDistance = (door.transform.position - fromPosition).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearest = door;
                nearestSqrDistance = sqrDistance;
            }
        }

        return nearest;
    }

    /// <summary>지금까지 배치된 책상 수. HUD의 직원 수(n/m)에서 m으로 쓰인다.</summary>
    public int WorkstationCount => _workstations.Count;

    /// <summary>
    /// 배치가 확정된 오브젝트를 등록한다.
    /// 전체 목록에는 무조건 넣고(배치 규칙 판정에 필요), 자리 풀에는 IsWorkstation인 것만,
    /// 출입구 풀에는 Type이 Door인 것만 넣는다.
    /// </summary>
    public void RegisterWorkstation(PlaceableObject workstation)
    {
        if (workstation == null)
        {
            return;
        }

        if (!_placedObjects.Contains(workstation))
        {
            _placedObjects.Add(workstation);
        }

        if (workstation.Type == ObjectType.Door && !_doors.Contains(workstation))
        {
            _doors.Add(workstation);
        }

        if (!workstation.IsWorkstation || _workstations.Contains(workstation))
        {
            return;
        }

        _workstations.Add(workstation);
    }

    /// <summary>지금 배치되어 있는 오브젝트 전부. 배치 규칙 판정용이라 수정하면 안 된다.</summary>
    public IReadOnlyList<PlaceableObject> GetPlacedObjects()
    {
        return _placedObjects;
    }

    /// <summary>
    /// 책상 풀에서 뺀다 (이동하려고 다시 손에 드는 경우).
    ///
    /// 배정은 일부러 남겨둔다 - 책상을 옮긴다고 앉아있던 직원이 잘릴 이유가 없고,
    /// 옮기기를 취소하면 그대로 다시 등록되기 때문이다.
    /// 진짜로 없어질 때(삭제)는 PlaceSystem이 ReleaseSeatOf를 따로 불러준다.
    /// </summary>
    public void UnregisterWorkstation(PlaceableObject workstation)
    {
        if (workstation == null)
        {
            return;
        }

        _workstations.Remove(workstation);
        _doors.Remove(workstation);
        _placedObjects.Remove(workstation);
    }

    /// <summary>
    /// employeeId에게 빈 자리를 배정하고 자리 Transform을 반환한다.
    /// 이미 배정받은 상태면 기존 자리를 그대로 반환. 빈 자리가 없으면 null.
    /// </summary>
    public Transform RequestSeat(int employeeId)
    {
        PlaceableObject assigned = GetAssignedWorkstation(employeeId);

        if (assigned != null)
        {
            return GetSeatPoint(assigned);
        }

        foreach (PlaceableObject workstation in _workstations)
        {
            //파괴됐거나(이전 플레이 세션에서 남은 잔여 참조) 배치 데이터가 없는 자리는 건너뛴다
            if (workstation == null)
            {
                continue;
            }

            int objectId = workstation.GetObjectID();

            if (objectId == -1 || _seatOwners.ContainsKey(objectId))
            {
                continue;
            }

            //책상 위에 없거나 의자가 안 붙어있는 컴퓨터는 아직 앉을 수 없는 자리다
            if (!IsReadyForWork(workstation))
            {
                continue;
            }

            Occupy(employeeId, workstation, objectId);
            return GetSeatPoint(workstation);
        }

        return null;
    }

    /// <summary>
    /// 플레이어가 고른 직원을 이 자리에 앉힌다.
    /// 그 직원이 다른 자리에 앉아 있었다면 거기서 일어나고, 이 자리에 다른 직원이 있었다면 그쪽이 비워진다.
    /// 자리가 워크스테이션이 아니거나 아직 배치가 안 끝났으면 false.
    /// </summary>
    public bool AssignEmployee(int employeeId, PlaceableObject workstation)
    {
        if (workstation == null || !workstation.IsWorkstation)
        {
            return false;
        }

        int objectId = workstation.GetObjectID();

        //배치가 확정되기 전(손에 든 상태)에는 id가 없어서 표에 넣을 수 없다
        if (objectId == -1)
        {
            Debug.LogWarning(
                $"[WorkstationManagerSO] '{workstation.name}' : 아직 배치되지 않아 배정할 수 없습니다.",
                workstation);
            return false;
        }

        //컴퓨터는 책상 위 + 의자가 붙어있어야 근무 자리로 인정된다
        if (!IsReadyForWork(workstation))
        {
            Debug.LogWarning(
                $"[WorkstationManagerSO] '{workstation.name}' : 책상 위에 없거나 의자가 붙어있지 않아 배정할 수 없습니다.",
                workstation);
            return false;
        }

        //이미 그 자리에 앉아 있으면 아무것도 하지 않는다 (표를 지웠다 다시 넣을 이유가 없다)
        if (_seatOwners.TryGetValue(objectId, out int currentOwner) && currentOwner == employeeId)
        {
            return true;
        }

        //한 직원이 두 자리를 차지하면 안 된다
        ReleaseSeat(employeeId);

        //그 자리에 앉아 있던 다른 직원은 일어난다
        ReleaseSeatOf(workstation);

        Occupy(employeeId, workstation, objectId);
        return true;
    }

    /// <summary>배정받은 자리를 반납한다 (직원 퇴사 / 자리에서 빼기).</summary>
    public void ReleaseSeat(int employeeId)
    {
        if (!_employeeSeats.TryGetValue(employeeId, out PlaceableObject workstation))
        {
            return;
        }

        _employeeSeats.Remove(employeeId);

        //오브젝트가 이미 파괴됐어도 표에서는 지워야 한다
        if (workstation != null)
        {
            _seatOwners.Remove(workstation.GetObjectID());
        }
    }

    /// <summary>이 자리에 앉은 직원을 일으켜 세운다. 비어 있으면 아무것도 하지 않는다.</summary>
    public void ReleaseSeatOf(PlaceableObject workstation)
    {
        int employeeId = GetAssignedEmployeeId(workstation);

        if (employeeId == NoEmployee)
        {
            return;
        }

        ReleaseSeat(employeeId);
    }

    /// <summary>이 자리에 앉은 직원의 id. 비어 있으면 NoEmployee(-1).</summary>
    public int GetAssignedEmployeeId(PlaceableObject workstation)
    {
        if (workstation == null)
        {
            return NoEmployee;
        }

        return _seatOwners.TryGetValue(workstation.GetObjectID(), out int employeeId)
            ? employeeId
            : NoEmployee;
    }

    /// <summary>이 직원이 배정받은 자리. 없으면 null.</summary>
    public PlaceableObject GetAssignedWorkstation(int employeeId)
    {
        if (!_employeeSeats.TryGetValue(employeeId, out PlaceableObject workstation))
        {
            return null;
        }

        //배정받은 자리가 삭제됐을 수 있다. 그러면 표를 정리하고 없는 것으로 친다.
        if (workstation == null)
        {
            ReleaseSeat(employeeId);
            return null;
        }

        return workstation;
    }

    /// <summary>이 자리에 누가 앉아 있는지.</summary>
    public bool IsOccupied(PlaceableObject workstation)
    {
        return GetAssignedEmployeeId(workstation) != NoEmployee;
    }

    private void Occupy(int employeeId, PlaceableObject workstation, int objectId)
    {
        _seatOwners[objectId] = employeeId;
        _employeeSeats[employeeId] = workstation;
    }

    #region 배치 규칙 (컴퓨터 - 책상 - 의자)

    /// <summary>
    /// 컴퓨터가 올라가 있는 책상. 바닥에 놓였으면 null.
    ///
    /// 컴퓨터가 차지하는 칸이 책상 칸 안에 온전히 들어가야 "책상 위"로 본다.
    /// 걸쳐만 놓은 것을 허용하면 상판 밖으로 삐져나온 채 놓이기 때문이다.
    /// </summary>
    public PlaceableObject FindDeskUnder(PlaceableObject computer)
    {
        if (computer == null)
        {
            return null;
        }

        RectInt computerRect = computer.GetCellRect();

        for (int i = 0; i < _placedObjects.Count; i++)
        {
            PlaceableObject other = _placedObjects[i];

            if (other == null || other == computer || other.Type != ObjectType.Desk)
            {
                continue;
            }

            if (ContainsRect(other.GetCellRect(), computerRect))
            {
                return other;
            }
        }

        return null;
    }

    /// <summary>
    /// 같은 자리에 이미 다른 컴퓨터가 올라가 있는지.
    ///
    /// 책상 위는 타일맵 겹침 검사를 건너뛰기 때문에, 그것만으로는 한 칸에 컴퓨터가
    /// 여러 대 쌓이는 것을 못 막는다. 그래서 컴퓨터끼리는 따로 본다.
    /// (책상 외의 다른 오브젝트는 애초에 책상과 칸이 겹칠 수 없으므로 검사할 필요가 없다)
    /// </summary>
    public bool IsOverlappedByAnotherComputer(PlaceableObject computer)
    {
        if (computer == null)
        {
            return false;
        }

        RectInt computerRect = computer.GetCellRect();

        for (int i = 0; i < _placedObjects.Count; i++)
        {
            PlaceableObject other = _placedObjects[i];

            if (other == null || other == computer || other.Type != ObjectType.Computer)
            {
                continue;
            }

            if (computerRect.Overlaps(other.GetCellRect()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>책상에 맞닿아 있는 의자. 없으면 null. 모서리만 닿은 대각선은 인정하지 않는다.</summary>
    public PlaceableObject FindChairNextTo(PlaceableObject desk)
    {
        if (desk == null)
        {
            return null;
        }

        RectInt deskRect = desk.GetCellRect();

        for (int i = 0; i < _placedObjects.Count; i++)
        {
            PlaceableObject other = _placedObjects[i];

            if (other == null || other == desk || other.Type != ObjectType.Chair)
            {
                continue;
            }

            if (AreEdgeAdjacent(deskRect, other.GetCellRect()))
            {
                return other;
            }
        }

        return null;
    }

    /// <summary>
    /// 이 컴퓨터가 근무 가능한 자리인지 확인한다.
    /// 조건 : 책상 위에 있을 것 + 그 책상에 의자가 맞닿아 있을 것.
    ///
    /// 통과하면 찾아낸 책상과 의자를 같이 내준다. 직원이 걸어가는 곳은 그 의자다.
    ///
    /// 결과를 컴퓨터에 캐싱해두지 않는다 - 의자나 책상은 언제든 옮기거나 치울 수 있어서
    /// 캐싱하면 그때마다 무효화해줘야 하고, 이 함수를 부르는 쪽(배정 UI, 직원 AI의 판단 주기)은
    /// 프레임마다가 아니라 몇 초에 한 번이라 매번 찾아도 부담이 없다.
    /// </summary>
    public bool TryResolveSeat(PlaceableObject computer, out PlaceableObject desk, out PlaceableObject chair)
    {
        desk = FindDeskUnder(computer);
        chair = desk != null ? FindChairNextTo(desk) : null;

        return desk != null && chair != null;
    }

    /// <summary>
    /// 이 자리에 배정된 직원이 실제로 걸어갈 지점 = 책상에 붙어있는 의자.
    /// 아직 근무 자리가 아니면(책상 위가 아니거나 의자가 없으면) null.
    /// </summary>
    public Transform GetSeatPoint(PlaceableObject workstation)
    {
        return TryResolveSeat(workstation, out _, out PlaceableObject chair) ? Seat.PointOf(chair) : null;
    }

    /// <summary>책상 위에 있고 의자까지 붙어 있어 실제로 직원을 앉힐 수 있는 자리인지.</summary>
    public bool IsReadyForWork(PlaceableObject workstation)
    {
        if (workstation == null || !workstation.IsWorkstation)
        {
            return false;
        }

        return TryResolveSeat(workstation, out _, out _);
    }

    /// <summary>inner가 outer 안에 온전히 들어가는지.</summary>
    private static bool ContainsRect(RectInt outer, RectInt inner)
    {
        if (outer.width <= 0 || outer.height <= 0 || inner.width <= 0 || inner.height <= 0)
        {
            return false;
        }

        return inner.xMin >= outer.xMin && inner.xMax <= outer.xMax
            && inner.yMin >= outer.yMin && inner.yMax <= outer.yMax;
    }

    /// <summary>두 칸 범위가 변끼리 맞닿아 있는지. 겹치거나 떨어져 있거나 대각선이면 false.</summary>
    private static bool AreEdgeAdjacent(RectInt a, RectInt b)
    {
        if (a.width <= 0 || a.height <= 0 || b.width <= 0 || b.height <= 0)
        {
            return false;
        }

        bool xOverlap = a.xMin < b.xMax && b.xMin < a.xMax;
        bool yOverlap = a.yMin < b.yMax && b.yMin < a.yMax;

        bool xTouch = a.xMax == b.xMin || b.xMax == a.xMin;
        bool yTouch = a.yMax == b.yMin || b.yMax == a.yMin;

        return (xOverlap && yTouch) || (yOverlap && xTouch);
    }

    #endregion
}

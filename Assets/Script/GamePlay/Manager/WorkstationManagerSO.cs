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

    //employeeId -> 배정된 워크스테이션
    private Dictionary<int, PlaceableObject> _employeeSeats = new Dictionary<int, PlaceableObject>();

    //워크스테이션의 PlacedObjectData id -> 거기 앉은 직원 id.
    //예전에는 HashSet으로 "찼는지"만 봤는데, 자리를 눌렀을 때 누가 앉아있는지 보여주려면
    //반대 방향도 알아야 해서 표로 바꿨다. 중복 배정 방지도 이 표가 겸한다.
    private Dictionary<int, int> _seatOwners = new Dictionary<int, int>();

    public void Init()
    {
        _workstations = new List<PlaceableObject>();
        _employeeSeats = new Dictionary<int, PlaceableObject>();
        _seatOwners = new Dictionary<int, int>();
    }

    /// <summary>배치가 확정된 오브젝트를 책상 풀에 등록한다. IsWorkstation이 아니면 무시.</summary>
    public void RegisterWorkstation(PlaceableObject workstation)
    {
        if (workstation == null || !workstation.IsWorkstation || _workstations.Contains(workstation))
        {
            return;
        }

        _workstations.Add(workstation);
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
            return assigned.GetSeatPoint();
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

            Occupy(employeeId, workstation, objectId);
            return workstation.GetSeatPoint();
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
}

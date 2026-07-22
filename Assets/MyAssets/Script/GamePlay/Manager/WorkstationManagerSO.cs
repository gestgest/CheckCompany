using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배치된 오브젝트(PlaceableObject) 중 IsWorkstation == true 인 것들을 "책상 풀"로 관리하고,
/// 직원(employeeId) 단위로 빈 책상을 배정/반납한다.
/// </summary>
[CreateAssetMenu(fileName = "WorkstationManagerSO", menuName = "ScriptableObject/Manager/WorkstationManagerSO")]
public class WorkstationManagerSO : ScriptableObject
{
    private List<PlaceableObject> _workstations;

    //employeeId -> 배정된 워크스테이션
    private Dictionary<int, PlaceableObject> _employeeSeats;

    //중복 배정 방지용 (워크스테이션의 PlacedObjectData id 기준)
    private HashSet<int> _occupiedWorkstationIds;

    public void Init()
    {
        _workstations = new List<PlaceableObject>();
        _employeeSeats = new Dictionary<int, PlaceableObject>();
        _occupiedWorkstationIds = new HashSet<int>();
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

    public void UnregisterWorkstation(PlaceableObject workstation)
    {
        if (workstation == null)
        {
            return;
        }

        _workstations.Remove(workstation);
        _occupiedWorkstationIds.Remove(workstation.GetObjectID());
    }

    /// <summary>
    /// employeeId에게 빈 책상을 배정하고 자리 Transform을 반환한다.
    /// 이미 배정받은 상태면 기존 자리를 그대로 반환. 빈 책상이 없으면 null.
    /// </summary>
    public Transform RequestSeat(int employeeId)
    {
        if (_employeeSeats.TryGetValue(employeeId, out PlaceableObject assigned))
        {
            return assigned.GetSeatPoint();
        }

        foreach (PlaceableObject workstation in _workstations)
        {
            int objectId = workstation.GetObjectID();
            if (_occupiedWorkstationIds.Contains(objectId))
            {
                continue;
            }

            _occupiedWorkstationIds.Add(objectId);
            _employeeSeats[employeeId] = workstation;
            return workstation.GetSeatPoint();
        }

        return null;
    }

    /// <summary>배정받은 책상을 반납한다 (직원 퇴사/오브젝트 파괴 시 호출).</summary>
    public void ReleaseSeat(int employeeId)
    {
        if (!_employeeSeats.TryGetValue(employeeId, out PlaceableObject workstation))
        {
            return;
        }

        _occupiedWorkstationIds.Remove(workstation.GetObjectID());
        _employeeSeats.Remove(employeeId);
    }
}

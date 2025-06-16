using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreateMissionManagerSO", menuName = "ScriptableObject/Manager/CreateMissionManagerSO")]
public class CreateMissionManagerSO : ScriptableObject
{
    [Header("Manager")]
    [SerializeField] private EmployeeManagerSO _employeeManager;
    [SerializeField] private MissionManagerSO _missionManager;

    [Header("boardingcast on Events")]
    [SerializeField] private VoidEventChannelSO _ChangeAssignedEmployeeEventChannel; //to AssignMissionPanel

    private int assignableEmployeeSize;
    private List<int> _refEmployeesID;

    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type = 0;
    private int level = 0;


    public void Init()
    {
        if(_refEmployeesID == null)
            _refEmployeesID = new List<int>();
        else
        {
            _refEmployeesID.Clear();
        }
    }

    public void AddRefEmployeeID(int id)
    {
        _refEmployeesID.Add(id);
        _ChangeAssignedEmployeeEventChannel.RaiseEvent();
    }

    public void RemoveRefEmployeeID(int id)
    {
        for (int i = 0; i < _refEmployeesID.Count; i++)
        {
            if (id == _refEmployeesID[i])
                _refEmployeesID.RemoveAt(i);
        }
        _ChangeAssignedEmployeeEventChannel.RaiseEvent();
    }

    public Mission CreateMission(string title, List<Todo_Mission> todo_Missions)
    {
        Mission mission = new Mission(
            _missionManager.GetAndIncrementCount(),
            employee_type,
            title,
            _missionManager.GetIcon(0),
            0, //iconID
            level,
            todo_Missions,
            GetRefEmployeesID()
        );

        return mission;
    }


    #region PERPROTY
    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void SetEmployeeType(int employee_type)
    {
        this.employee_type = employee_type;
    }
    public void SetAssignableEmployeeSize(int size)
    {
        assignableEmployeeSize = size;
    }
    public int GetAssignableEmployeeSize()
    {
        return assignableEmployeeSize;
    }
    public List<int> GetRefEmployeesID()
    {
        return _refEmployeesID;
    }
    #endregion
}

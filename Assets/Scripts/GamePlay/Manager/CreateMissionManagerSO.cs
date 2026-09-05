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

    /// <summary>
    /// 지금 만들고 있는(또는 수정중인) 미션에 붙일 직원 id.
    ///
    /// [SerializeField]를 붙이면 안 된다. ScriptableObject의 직렬화 필드는 에디터에서 플레이를 끝내도
    /// .asset 파일에 그대로 저장돼서, 예전 세션에 골라둔 직원 id가 다음 실행까지 살아남는다.
    /// 그러면 그 직원이 이미 해고됐거나 애초에 다른 계정의 데이터일 때
    /// AssignMissionPanel이 id를 찾지 못해 "할당된 직원 id를 찾을 수 없습니다"를 뱉는다.
    /// 이 값은 패널을 여는 동안만 쓰는 임시 상태라 저장할 이유가 없다.
    /// </summary>
    private List<int> _refEmployeesID;

    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type = 0;
    private int level = 0;


    /// <summary>
    /// 새 미션을 만들기 시작할 때 부른다. 이전에 고른 직원 선택을 지운다.
    ///
    /// 예전에는 리스트가 null일 때만 새로 만들어서, 두 번째 미션을 만들 때
    /// 첫 번째 미션에 붙였던 직원이 그대로 선택된 채로 남아 있었다.
    /// (수정 모드는 이 뒤에 SetRefEmployeesID()로 그 미션의 목록을 다시 넣으므로 영향이 없다)
    /// </summary>
    public void Init()
    {
        if (_refEmployeesID == null)
        {
            _refEmployeesID = new List<int>();
            return;
        }

        _refEmployeesID.Clear();
    }

    public void AddRefEmployeeID(int id)
    {
        GetRefEmployeesID().Add(id);
        _ChangeAssignedEmployeeEventChannel.RaiseEvent();
    }

    public void RemoveRefEmployeeID(int id)
    {
        List<int> ids = GetRefEmployeesID();

        for (int i = 0; i < ids.Count; i++)
        {
            if (id == ids[i])
                ids.RemoveAt(i);
        }
        _ChangeAssignedEmployeeEventChannel.RaiseEvent();
    }

    public Mission CreateMission(int id, string title, List<Todo_Mission> todo_Missions)
    {
        Mission mission = new Mission(
            id,
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
    
    public void SetEmployeeType(int employee_type)
    {
        this.employee_type = employee_type;
    }
    
    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void SetRefEmployeesID(List<int> refEmployeesID)
    {
        _refEmployeesID = refEmployeesID;
    }

    public void SetAssignableEmployeeSize(int size)
    {
        assignableEmployeeSize = size;
    }
    public int GetAssignableEmployeeSize()
    {
        return assignableEmployeeSize;
    }
    /// <summary>
    /// 지금 고른 직원 id 목록. 절대 null을 돌려주지 않는다 -
    /// 더 이상 직렬화하지 않는 필드라 Init()을 거치지 않고 패널이 먼저 열리면 null일 수 있는데,
    /// 부르는 쪽(AssignMissionPanel 등)이 전부 바로 Count를 읽어서 그대로 두면 터진다.
    /// </summary>
    public List<int> GetRefEmployeesID()
    {
        if (_refEmployeesID == null)
        {
            _refEmployeesID = new List<int>();
        }

        return _refEmployeesID;
    }
    #endregion
}

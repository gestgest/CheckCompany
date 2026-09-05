using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//직원 고용 관련 
[CreateAssetMenu(fileName = "EmployeeManagerSO", menuName = "ScriptableObject/Manager/EmployeeManagerSO")]
public class EmployeeManagerSO : ScriptableObject
{
    [Header("Manager")]
    [SerializeField] private RecruitmentManagerSO _recruitmentManager;
    [SerializeField] private EmployeeAssetsSO _employeeAssetsSO;

    [Header("수입")]
    //근무중인 직원 한 명이 게임 1시간 동안 버는 기본 금액. 업무속도 100 / 체력 최대일 때의 값이다.
    //월급 100만원짜리 직원이 하루 9시간 × 30일 = 270시간 일하므로 손익분기점은 시간당 약 3,700원.
    //5,000원이면 체력이 넉넉할 때 흑자, 체력이 바닥나 쉬는 시간이 길어지면 적자로 기운다.
    [SerializeField] private float _incomePerWorkHour = 5000f;

    //정산하고 남은 1원 미만. 게임 1분씩 들어와도 버려지지 않도록 다음 정산으로 넘긴다.
    private float _incomeRemainder;

    [Space]

    [Header("Broadingcast on Events")]
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    [SerializeField] private VoidEventChannelSO _rerollEmployeeStatusEventChannelSO;
    [SerializeField] private BoolEventChannelSO _isChangedEmployeePanelEventChannelSO;
    [SerializeField] private BoolEventChannelSO _isChangedAssignEmployeePanelEventChannelSO;

    //EmployeeObjectSystem
    [SerializeField] private IntEventChannelSO _onChangedCreateEvent;
    [SerializeField] private IntEventChannelSO _onChangedRemoveEvent;
    [SerializeField] private Int2EventChannelSO _onChangeEvent;

    //직원 목록
    //필드 초기화자로 기본값을 넣어둔다 - Init()이 아직 호출되기 전에(씬/스크립트 실행 순서 문제로
    //EmployeeObjectSystem 등이 먼저 접근하는 경우) NullReferenceException이 나지 않도록 하기 위함.
    List<Employee> employees = new List<Employee>();

    private Employee _selectedEmployee;



    public void Init()
    {
        employees = new List<Employee>();
        _incomeRemainder = 0f;
    }


    //제거 함수
    public void RemoveEmployee(int id)
    {
        int index = Search_Employee_Index(id);

        //직원 제거 함수, index가 -1이면 오류
        if (index != -1)
        {
            employees.RemoveAt(index);
            RemoveServerEmployee(id); //서버도 제거
            _isChangedEmployeePanelEventChannelSO.RaiseEvent(true);
            _onChangedRemoveEvent.RaiseEvent(index);
        }
        else
        {
            Debug.LogError("제거 직원 id error : " + id);
        }
    }

    //직원 창 보여주는 기능
    public void ShowEmployeeStatusWindow(int id)
    {
        int index = Search_Employee_Index(id);
        _selectedEmployee = employees[index];

        //EmployeeStatusWindow 호출
        //employeePanel.SwitchingPanel(0);
        List<int> dir = new List<int>();
        dir.Add(0);
        dir.Add(0);
        dir.Add(0);

        PanelManager.instance.SwitchingPanel(dir);
    }

    public void CreateEmployee(Employee e)
    {
        SetServerEmployee(e);
        employees.Add(e);
        SelectionEmployeeSort();
        _isChangedEmployeePanelEventChannelSO.RaiseEvent(true);
        _onChangedCreateEvent.RaiseEvent(employees.Count - 1);
    }

    //월급이 부족해서 지급하지 못했을 때 직원들이 잃는 스태미나/멘탈 양
    private const int UNPAID_SALARY_PENALTY = 20;

    /// <summary>
    /// 업무속도를 올린다(미션 완료 보상). 수입 계산이 WorkSpeed에 비례하므로 곧 돈을 더 버는 것과 같다.
    ///
    /// 상한을 두는 이유: WorkSpeed는 서버에 저장되는 영구 값이라, 미션을 깰 때마다 무한히 오르면
    /// 후반에 수입이 통제 불능이 된다. 지원자 기본값이 80~120(RecruitmentManagerSO)이므로
    /// 상한은 그보다 넉넉히 위에 둔다.
    /// </summary>
    /// <returns>실제로 오른 양. 이미 상한이면 0.</returns>
    public int AddWorkSpeed(int employeeId, int amount, int max)
    {
        Employee employee = GetEmployeeById(employeeId);

        //해고된 직원 등 id가 더 이상 없는 경우
        if (employee == null || amount <= 0)
        {
            return 0;
        }

        int before = employee.WorkSpeed;
        employee.WorkSpeed = Mathf.Min(before + amount, max);

        int gained = employee.WorkSpeed - before;

        if (gained == 0)
        {
            return 0;
        }

        SetServerWorkSpeed(GameManager.instance.Nickname, employeeId, employee.WorkSpeed);

        //직원 목록에 업무속도가 표시되므로 갱신해줘야 바로 보인다
        _isChangedEmployeePanelEventChannelSO.RaiseEvent(true);

        return gained;
    }

    //결제 시도하고 안되면 false
    public bool PayEmployees()
    {
        int sum = 0;
        for (int i = 0; i < employees.Count; i++)
        {
            sum += employees[i].Salary;
        }

        if (sum <= GameManager.instance.Money)
        {
            GameManager.instance.SetMoney(GameManager.instance.Money - sum);
            return true;
        }

        //월급을 못 받은 직원들은 사기가 떨어진다 (스태미나/멘탈 감소)
        for (int i = 0; i < employees.Count; i++)
        {
            employees[i].SetStamina(employees[i].Stamina - UNPAID_SALARY_PENALTY);
            employees[i].SetMental(employees[i].Mental - UNPAID_SALARY_PENALTY);
        }

        return false;
    }

    /// <summary>
    /// 흘러간 게임 시간만큼 근무중인 직원들이 번 돈을 합산해 돌려준다.
    ///
    ///   시간당 수입 = _incomePerWorkHour × (업무속도 / 100) × (체력 / 최대체력)
    ///
    /// 월급(PayEmployees)이 나가기만 하던 흐름에 들어오는 쪽을 붙이는 자리다.
    /// 체력이 0이면 EmployeeWorkAI가 자리에서 일으켜 세우므로(Resting) IsWorking이 꺼지고 수입도 끊긴다.
    /// - 쉬는 동안 못 버는 돈이 곧 체력을 관리할 이유가 된다.
    /// </summary>
    /// <param name="gameMinutes">이번 틱에 흐른 게임 시간(분)</param>
    /// <returns>이번 틱에 번 돈. 1원 미만은 다음 틱으로 넘긴다</returns>
    public long CollectIncome(int gameMinutes)
    {
        if (gameMinutes <= 0)
        {
            return 0;
        }

        float hours = gameMinutes / 60f;

        for (int i = 0; i < employees.Count; i++)
        {
            Employee employee = employees[i];

            if (!employee.IsWorking)
            {
                continue;
            }

            float speedRatio = employee.WorkSpeed / (float)Employee.DEFAULT_WORK_SPEED;

            //max_stamina가 0인 데이터가 들어와도 0으로 나누지 않도록 막는다
            float staminaRatio = employee.Max_Stamina > 0
                ? employee.Stamina / (float)employee.Max_Stamina
                : 0f;

            _incomeRemainder += _incomePerWorkHour * speedRatio * staminaRatio * hours;
        }

        long earned = (long)_incomeRemainder;
        _incomeRemainder -= earned;

        return earned;
    }

    #region PROPERTY

    public Employee GetSelectedEmployee()
    {
        return _selectedEmployee;
    }

    public List<Employee> GetEmployees()
    {
        return employees;
    }

    public Sprite GetIcon(int asset_id)
    {
        return _employeeAssetsSO.GetAsset(asset_id).GetIcon();
    }

    #endregion

    public void ChangedEmployeeStatus()
    {
        //employeestatus is not active
        if(_rerollEmployeeStatusEventChannelSO._onEventRaised == null)
        {
            return;
        }
        _rerollEmployeeStatusEventChannelSO.RaiseEvent();
    }

    public Employee GetEmployee(int index)
    {
        return employees[index];
    }

    /// <summary>id로 직원을 찾는다. 없으면 null.</summary>
    public Employee GetEmployeeById(int id)
    {
        int index = Search_Employee_Index(id);
        return index == -1 ? null : employees[index];
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////서버
    #region SERVER

    //employee
    void SetServerEmployee(Employee e)
    {
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname, 
            "employees." + e.ID,
            e.EmployeeToJSON()
        );
    }

    void RemoveServerEmployee(int id)
    {
        _deleteFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "employees." + id.ToString()
        );
    }

    public void RemoveAllServerMissions(string nickname, int id)
    {
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            nickname,
            "employees." + id.ToString() + ".missions",
            FieldValue.Delete
        );
    }
    public void AddServerMission(Mission m, string nickname, int id)
    {
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            nickname,
            "employees." + id.ToString() + ".missions",
            FieldValue.ArrayUnion(m.MissionToJSON())
        );
    }

    public void SetServerStamina(string nickname, int id, int stamina)
    {
        string em = "employees.";
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            nickname,
            em + id.ToString() + ".stamina",
            stamina
        );
    }

    public void SetServerMental(string nickname, int id, int mental)
    {
        string em = "employees.";
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            nickname,
            em + id.ToString() + ".mental",
            mental
        );
    }

    public void SetServerWorkSpeed(string nickname, int id, int workSpeed)
    {
        string em = "employees.";
        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            nickname,
            em + id.ToString() + ".workSpeed",
            workSpeed
        );
    }


    //고용된 직원 서버 자료들을 인 게임으로 가져오는 함수
    public void JSONToEmployees(Dictionary<string, object> serverEmployees)
    {
        if (this.employees == null)
            this.employees = new List<Employee>();

        if (serverEmployees == null)
        {
            return;
        }

        //map형태의 employees를 list로 변환
        foreach (KeyValuePair<string, object> serverEmployee in serverEmployees)
        {
            Dictionary<string, object> tmp = (Dictionary<string, object>)(serverEmployee.Value);

            EmployeeSO employeeSO = _recruitmentManager.GetEmployeeSO(Convert.ToInt32(tmp["employeeType"]));
            Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO, this, true);

            employee.JSONToEmployee(serverEmployee);
            this.employees.Add(employee);

            //예전엔 이 호출이 loop 밖에서 한 번만 일어나서
            //  1) 직원이 여러 명이면 마지막 직원 오브젝트만 생성되고
            //  2) 직원이 0명이면 RaiseEvent(-1)이 호출되어 EmployeeObjectSystem에서 IndexOutOfRangeException이 났다.
            //직원을 추가할 때마다 즉시 이벤트를 발생시켜 각 직원마다 오브젝트가 생성되도록 수정.
            _onChangedCreateEvent.RaiseEvent(employees.Count - 1);
        }

        //Search_Employee_Index()가 이진탐색이라 리스트가 ID 순으로 정렬돼 있어야 한다.
        //서버가 주는 employees는 Dictionary라 순회 순서가 보장되지 않고, 키가 문자열이라
        //정렬돼 있더라도 "10"이 "2"보다 앞에 온다. 정렬하지 않으면 실제로 있는 직원인데도
        //탐색이 -1을 돌려줘서 해고/상태창/미션 배정이 전부 조용히 실패한다.
        //(CreateEmployee는 추가 직후 이미 이 함수를 부른다 - 여기만 빠져 있었다)
        //오브젝트는 위 루프에서 이미 다 만들어졌고, 스왑마다 _onChangeEvent가 나가서
        //EmployeeObjectSystem의 목록도 같은 순서로 따라온다.
        SelectionEmployeeSort();

        _isChangedEmployeePanelEventChannelSO.RaiseEvent(true);
    }

    #endregion

    #region BINARY_SEARCH

    //이진탐색
    public int Search_Employee_Index(int id)
    {
        int index = Binary_Search_Employee_Index(0, employees.Count - 1, id);

        //이진탐색은 못 찾으면 삽입 위치(0 ~ Count)를 돌려준다.
        //리스트가 비었으면 0, id가 최대값보다 크면 Count가 나오므로 접근 전에 범위를 봐야 한다.
        if (index < 0 || index >= employees.Count)
        {
            return -1;
        }

        return employees[index].ID == id ? index : -1;
    }

    private int Binary_Search_Employee_Index(int start, int end, int id)
    {
        if (start > end)
        {
            return start;
        }
        int mid = (start + end) / 2;
        if (id > employees[mid].ID)
        {
            return Binary_Search_Employee_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Employee_Index(start, mid - 1, id);
        }
    }


    private void SelectionEmployeeSort()
    {
        //O(n^2) => 나중에 다른 정렬로 바꾸지 않을까
        for (int i = 0; i < employees.Count; i++)
        {
            for (int j = i + 1; j < employees.Count; j++)
            {
                if (employees[i].ID > employees[j].ID)
                {
                    Employee tmp = employees[i];
                    employees[i] = employees[j];
                    employees[j] = tmp;

                    _onChangeEvent.RaiseEvent(i, j);
                }
            }
        }
    }

    #endregion
}

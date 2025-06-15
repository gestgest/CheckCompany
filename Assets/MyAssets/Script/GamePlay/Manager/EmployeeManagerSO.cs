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
    List<Employee> employees;

    private Employee _selectedEmployee;



    public void Init()
    {
        employees = new List<Employee>();
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

        return false;
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

    #endregion
    public void AddStamina(int add_value)
    {
        for(int i = 0; i < employees.Count; i++)
        {
            Employee employee = employees[i];
            employee.SetStamina(employee.Stamina + add_value);
        }
    }

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
        }
        _isChangedEmployeePanelEventChannelSO.RaiseEvent(true);
        _onChangedCreateEvent.RaiseEvent(employees.Count - 1);
    }

    #endregion

    #region BINARY_SEARCH

    //이진탐색
    public int Search_Employee_Index(int id)
    {
        int index = Binary_Search_Employee_Index(0, employees.Count - 1, id);
        if (employees[index].ID == id)
        {
            return index;
        }
        return -1;
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

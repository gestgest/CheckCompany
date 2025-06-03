using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

//직원 고용 관련 
[CreateAssetMenu(fileName = "EmployeeManagerSO", menuName = "ScriptableObject/Controller/EmployeeManagerSO")]
public class EmployeeManagerSO : ScriptableObject
{
    [SerializeField] private GameObject employeeElementPrefab;
    [SerializeField] private GameObject employeeObjectPrefab;
    
    [Header("Controller")]
    [SerializeField] private RecruitmentManagerSO recruitmentControllerSo;

    [Header("ServerEvent")]
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    
    //직원 목록
    List<Employee> employees;
    List<GameObject> employeeElementObjects;
    List<GameObject> employeeObjects;

    GameObject element_parent;
    GameObject object_parent;
    EmployeeStatusWindow employeeStatusWindow; //이거를 subPanel로 바꿀 수 없나
    Panel employeePanel;
    
    public void Init(GameObject element_parent,
        GameObject object_parent,
        EmployeeStatusWindow employeeStatusWindow,
        Panel employeePanel)
    {
        this.element_parent = element_parent;
        this.object_parent = object_parent;
        employeeStatusWindow.Init();
        this.employeeStatusWindow = employeeStatusWindow;
        this.employeePanel = employeePanel;
        
        employees = new List<Employee>();
        employeeElementObjects = new List<GameObject>();
        employeeObjects = new List<GameObject>();

        InitEmployeeSet();
    }
    
    //init함수
    private void InitEmployeeSet()
    {
        for (int i = 0; i < employees.Count; i++)
        {
            Employee e = employees[i];
            //오브젝트 생성
            CreateEmployeeObject();
            CreateEmployeeElementUI(e);
        }
    }

    


    //show함수, index를 employees기준으로 하면 안된다. => 나중에 전체 ID로 바꿀 예정
    private void CreateEmployeeElementUI(Employee e)
    {
        GameObject employeeObject = Instantiate(employeeElementPrefab, Vector3.zero, Quaternion.identity);
        EmployeeElement employeeContent = employeeObject.GetComponent<EmployeeElement>();
        Button button = employeeObject.GetComponent<Button>();

        employeeContent.SetEmployee(e._EmployeeSO.GetIcon(), e.Name, e.CareerPeriod, 1, e.Salary, e.ID);
        employeeElementObjects.Add(employeeObject);
        employeeObject.transform.SetParent(element_parent.transform);

        //버튼 추가
        button.onClick.AddListener(() => { ShowEmployeeStatusWindow(e.ID); });
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
            Destroy(employeeElementObjects[index]);
            employeeElementObjects.RemoveAt(index);
        }
        else
        {
            Debug.LogError("제거 직원 id error : " + id);
        }
    }

    //직원 창 보여주는 기능
    private void ShowEmployeeStatusWindow(int id)
    {
        Debug.Log("엄엄");
        //EmployeeStatusWindow 호출
        employeePanel.SwitchingPanel(0);
        //PanelManager.instance.Click_Button_Panel(5, true);

        //Debug.Log("직원 아이디" + id);

        int index = Search_Employee_Index(id);

        //Panel 이동 함수, index가 -1이면 오류
        if(index != -1)
        {
            employeeStatusWindow.SetValue(employees[index]);
        }
        else
        {
            Debug.LogError(id + ", 선택한 직원이 없는데요?");
        }
    }

    public void CreateEmployee(Employee e)
    {
        SetServerEmployee(e);
        employees.Add(e);
        CreateEmployeeElementUI(e);
        SelectionEmployeeSort();
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

    public void AddStamina(int add_value)
    {
        for(int i = 0; i < employees.Count; i++)
        {
            Employee employee = employees[i];
            employee.SetStamina(employee.Stamina + add_value);
        }
    }

    public void SetStaminaBarUI(int employee_id, int value)
    {
        employeeStatusWindow.SetStaminaBarUI(employee_id, value);
    }
    public void SetMentalBarUI(int employee_id, int value)
    {
        employeeStatusWindow.SetMentalBarUI(employee_id, value);
    }

    public void BanCheckTodoMission()
    {
        employeeStatusWindow.BanCheckTodoMission();
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

            EmployeeSO employeeSO = recruitmentControllerSo.GetEmployeeSO(Convert.ToInt32(tmp["employeeType"]));
            Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO, this, true);

            employee.JSONToEmployee(serverEmployee);
            this.employees.Add(employee);
        }
        InitEmployeeSet();
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

                    Transform a = employeeElementObjects[i].transform;
                    int a_index = a.GetSiblingIndex();
                    Transform b = employeeElementObjects[j].transform;
                    int b_index = b.GetSiblingIndex();

                    //Debug.Log("a : " +a_index);
                    //Debug.Log("b : " +b_index);
                    b.SetSiblingIndex(a_index);
                    a.SetSiblingIndex(b_index);

                    GameObject tmp_object = employeeElementObjects[i];
                    employeeElementObjects[i] = employeeElementObjects[j];
                    employeeElementObjects[j] = tmp_object;

                }
            }
        }
    }

    #endregion

    #region OBJECT

    /// <summary> 직원 오브젝트 생성 </summary>
    private void CreateEmployeeObject()
    {
        //GameObject tmp =Instantiate(employeeObjectPrefab, object_parent.transform);
        //employeeObjects.Add(tmp);
    }
    

    #endregion
}

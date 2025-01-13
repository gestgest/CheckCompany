using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//직원 고용 관련 
public class EmployeeController : MonoBehaviour
{
    public static EmployeeController instance;
    //직원 목록
    List<Employee> employees;
    List<GameObject> employeeObjects;

    [SerializeField] private GameObject employeePrefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private EmployeeStatusWindow employeeStatusWindow;
    [SerializeField] private PanelSO employeeStatusPanelSO;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        employees = new List<Employee>();
        employeeObjects = new List<GameObject>();

        InitEmployeeSet();
    }


    //init함수
    private void InitEmployeeSet()
    {
        for (int i = 0; i < employees.Count; i++)
        {
            Employee e = employees[i];
            CreateEmployeeElementUI(e);
        }

    }

    //show함수, index를 employees기준으로 하면 안된다. => 나중에 전체 ID로 바꿀 예정
    private void CreateEmployeeElementUI(Employee e)
    {
        GameObject employeeObject = Instantiate(employeePrefab, Vector3.zero, Quaternion.identity);
        EmployeeElement employeeContent = employeeObject.GetComponent<EmployeeElement>();
        Button button = employeeObject.GetComponent<Button>();

        employeeContent.SetEmployee(e._EmployeeSO.GetIcon(), e.Name, e.CareerPeriod, 1, e.Salary, e.ID);
        employeeObjects.Add(employeeObject);
        employeeObject.transform.SetParent(parent.transform);

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
            RemoveEmployeeToServer(id); //서버도 제거
            Destroy(employeeObjects[index]);
            employeeObjects.RemoveAt(index);
        }
        else
        {
            Debug.LogError("제거 직원 id error : " + id);
        }
    }

    //직원 창 보여주는 기능
    private void ShowEmployeeStatusWindow(int id)
    {
        //EmployeeStatusWindow의 index는 5. 
        PanelManager.instance.Click_Button_Panel(employeeStatusPanelSO.GetIndex(), true);

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
        
        SetEmployeeToServer(e);
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
            employee.SetStamina(employee.Stamina + add_value, false);
        }
    }

    #region SERVER
    void SetEmployeeToServer(Employee e)
    {
        //서버에 
        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname,
            "employees." + e.ID,
            e.SetEmployeeToJSON()
        );
    }

    void RemoveEmployeeToServer(int id)
    {
        FireStoreManager.instance.DeleteFirestoreDataKey(
            "GamePlayUser",
            GameManager.instance.Nickname, 
            "employees." + id.ToString()
        );
    }

    //고용된 직원 서버 자료들을 인 게임으로 가져오는 함수
    public void GetEmployeesFromJSON(Dictionary<string, object> serverEmployees)
    {
        if (this.employees == null)
            this.employees = new List<Employee>();


        //map형태의 employees를 list로 변환
        foreach (KeyValuePair<string, object> serverEmployee in serverEmployees)
        {
            Dictionary<string, object> tmp = (Dictionary<string, object>)(serverEmployee.Value);

            EmployeeSO employeeSO = RecruitmentController.instance.GetEmployeeSO(Convert.ToInt32(tmp["employeeType"]));
            Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO);

            employee.GetEmployeeFromJSON(serverEmployee);
            this.employees.Add(employee);
        }

        InitEmployeeSet();
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

                    Transform a = employeeObjects[i].transform;
                    int a_index = a.GetSiblingIndex();
                    Transform b = employeeObjects[j].transform;
                    int b_index = b.GetSiblingIndex();

                    //Debug.Log("a : " +a_index);
                    //Debug.Log("b : " +b_index);
                    b.SetSiblingIndex(a_index);
                    a.SetSiblingIndex(b_index);

                    GameObject tmp_object = employeeObjects[i];
                    employeeObjects[i] = employeeObjects[j];
                    employeeObjects[j] = tmp_object;

                }
            }
        }
    }

    #endregion
}

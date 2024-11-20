using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//직원 고용 관련 
public class EmployeeController : MonoBehaviour
{
    //직원 목록
    List<IEmployee> employees;
    List<GameObject> employeeObjects;

    [SerializeField] private GameObject employeePrefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private EmployeeStatusWindow employeeStatusWindow;
    [SerializeField] private EmployeeSO debugDevEmployeeType;
    [SerializeField] private PanelSO employeeStatusPanelSO;

    private void Start()
    {
        employees = new List<IEmployee>();
        employeeObjects = new List<GameObject>();

        InitEmployeeSet();
    }


    //init함수
    private void InitEmployeeSet()
    {
        for (int i = 0; i < employees.Count; i++)
        {
            IEmployee e = employees[i];
            CreateEmployeeElementUI(e);
        }

    }

    //show함수, index를 employees기준으로 하면 안된다. => 나중에 전체 ID로 바꿀 예정
    private void CreateEmployeeElementUI(IEmployee e)
    {
        GameObject employeeObject = Instantiate(employeePrefab, Vector3.zero, Quaternion.identity);
        EmployeeElement employeeContent = employeeObject.GetComponent<EmployeeElement>();
        Button button = employeeObject.GetComponent<Button>();


        employeeContent.SetEmployee(e._EmployeeType.GetIcon(), e.Name, e.CareerPeriod, 1, e.Salary);
        employeeObjects.Add(employeeObject);
        employeeObject.transform.SetParent(parent.transform);

        //버튼 추가
        button.onClick.AddListener(() => { ShowEmployeeStatusWindow(e.ID); });
    }

    //직원 창 보여주는 기능, 나중에 index를 전체 id로 바꾸면 이분탐색으로 교체 예정
    private void ShowEmployeeStatusWindow(int index)
    {
        //EmployeeStatusWindow의 index는 5. 
        PanelManager.instance.Click_Button_Panel(employeeStatusPanelSO.GetIndex(),true);

        //Debug.Log(index);
        //나중에 index를 전체 id로 바꾸면 이분탐색으로 교체 예정

        //대충 Panel 이동 함수
        employeeStatusWindow.SetValue(employees[index]);
    }

    public void CreateEmployee()
    {
        IEmployee e = new Development();
        e._EmployeeType = debugDevEmployeeType;
        e.Name = "엄준식";
        e.Age = 10;
        e.CareerPeriod = 10;
        e.Salary = 10;
        e.ID = 0;

        employees.Add(e);
        CreateEmployeeElementUI(e);

    }
}

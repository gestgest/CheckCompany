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

    [SerializeField] List<Sprite> employeeTypeIcons; //아이콘 => RecruitmentController이랑 중첩된다 -> 메모리 공간 차지
    [SerializeField] private GameObject employeePrefab;
    [SerializeField] private GameObject parent;
    //[SerializeField] private GameObject employeeStatusWindow;
    [SerializeField] private UIManager _UIManager;


    private void Start()
    {
        employees = new List<IEmployee>();
        employeeObjects = new List<GameObject>();

        InitEmployeeSet();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            //디버깅용 employee
            IEmployee e = new Development();
            e._EmployeeType = EmployeeType.Developer;
            e.Name = "엄준식";
            e.Age = 10;
            e.Career = 10;
            e.Cost = 10;

            employees.Add(e);
            CreateEmployeeElementUI(e);
        }
    }

    //init함수
    private void InitEmployeeSet()
    {
        for(int i = 0; i < employees.Count; i++)
        {
            IEmployee e = employees[i]; 
            CreateEmployeeElementUI(e);
        }
        
    }

    //show함수
    private void CreateEmployeeElementUI(IEmployee e)
    {
        GameObject employeeObject = Instantiate(employeePrefab, Vector3.zero, Quaternion.identity);
        EmployeeElement employeeContent = employeeObject.GetComponent<EmployeeElement>();
        Button button = employeeObject.GetComponent<Button>();

        employeeContent.SetEmployee(employeeTypeIcons[(int)(e._EmployeeType)],e.Name , e.Career, 1, e.Cost);
        employeeObjects.Add(employeeObject);
        employeeObject.transform.SetParent(parent.transform);

        //버튼 추가
        button.onClick.AddListener(ShowEmployeeStatusWindow);
    }

    //직원 창 보여주는 기능
    private void ShowEmployeeStatusWindow()
    {
        //EmployeeStatusWindow의 index는 3. => 이렇게 하는게 아닌 유동적으로 바꿔야 한다. ★
        _UIManager.ShowWindow(3);

        //대충 직원 내용 윈도우창에 삽입
        
    }
}

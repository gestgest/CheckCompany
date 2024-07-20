using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//직원 고용 관련 
public class EmployeeController : MonoBehaviour
{
    //직원 목록
    List<IEmployee> employees;
    List<GameObject> employeeObjects;

    [SerializeField] List<Sprite> employeeTypeIcons; //아이콘 => RecruitmentController이랑 중첩된다 -> 메모리 공간 차지
    [SerializeField] private GameObject employeePrefab;
    [SerializeField] private GameObject view;

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

        //recruitmentContent.SetRecruitment(employeeTypeIcons[(int)r.GetEmployeeType()], r.GetDay(), r.GetSize(), i);
        employeeContent.SetEmployee(employeeTypeIcons[(int)(e._EmployeeType)],e.Name , e.Career, 1, e.Cost);
        employeeObjects.Add(employeeObject);
        employeeObject.transform.SetParent(view.transform);
    }
}

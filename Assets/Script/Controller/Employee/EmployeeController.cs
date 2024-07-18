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

    private void Start()
    {
        employees = new List<IEmployee>();
        employeeObjects = new List<GameObject>();

        //recruitments 가져오는 함수()
        //디버깅용 employee
        //Employee e = new Employee();
        

        //employees.Add(e);

        InitEmployeeSet();
    }

    //init함수
    private void InitEmployeeSet()
    {
        
    }

    //show함수
    private void ShowEmployeeUI(IEmployee e)
    {
        
    }

}

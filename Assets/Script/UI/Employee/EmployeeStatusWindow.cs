using System.Collections;
using UnityEngine;

public class EmployeeStatusWindow : Window
{
    //EmployeeStatusWindow => 값 받기, panels 관련 애니메이션, panel이동 관련 함수

    //값 받기
    public void SetValue(IEmployee employee)
    {
        Debug.Log(employee.ID);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Employee가 아니라 개발자, 디자이너 이런식으로 해야할 거 같은데

//개발자
public class Development : MonoBehaviour, IEmployee
{
    int id; //식별 번호
    EmployeeType employeeType = EmployeeType.Developer;
    private string employee_name;
    private int age;
    private int career;
    private int cost;


    public EmployeeType _EmployeeType { get { return employeeType; } set { employeeType = value; } }
    public string Name { get { return employee_name; } set {  employee_name = value;} }
    public int Age { get { return age; } set {age = value;} }
    public int Career { get { return career; } set {career = value;} }
    public int Cost { get { return cost; } set { cost = value; } }

}

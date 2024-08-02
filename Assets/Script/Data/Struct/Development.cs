using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Employee가 아니라 개발자, 디자이너 이런식으로 해야할 거 같은데

//개발자
public class Development : MonoBehaviour, IEmployee
{
    int id; //식별 번호 => 우리 회사 따로 ID 또는 전체 ID
    //우리 회사 따로면 그냥 employee[식별번호] 해야함
    //전체 ID면 계속 정렬시키고 이분탐색해야함 [채택]
    EmployeeType employeeType = EmployeeType.Developer;
    private string employee_name;
    private int age;
    private int career;
    private int cost;
    private WorkTime workTime;
    private EmployeeRank rank;


    public int ID { get { return id; } set {id = value;} }
    public EmployeeType _EmployeeType { get { return employeeType; } set { employeeType = value; } }
    public string Name { get { return employee_name; } set {  employee_name = value;} }
    public int Age { get { return age; } set {age = value;} }
    public int Career { get { return career; } set {career = value;} }
    public int Cost { get { return cost; } set { cost = value; } }
    public EmployeeRank _Rank { get { return rank; } set { rank = value; } }
    public WorkTime _WorkTime { get { return workTime; } set { workTime = value; } }

}

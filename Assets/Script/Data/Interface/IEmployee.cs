using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEmployee
{
    int ID { get; set; }
    public EmployeeType _EmployeeType { get; set; }
    string Name { get; set; }
    int Age { get; set; }
    int Career { get; set; } //개월 단위
    EmployeeRank _Rank { get; set; } //단위 (enum
    int Cost { get; set; } //월급 단위
    WorkTime _WorkTime { get; set; } //근무시간

    //기술
    //교육
    //requirement
}

public enum EmployeeType
{
    Product_Manager = 0,
    Developer = 1,
    Designer = 2,
    QA = 3,
}

public enum EmployeeRank
{
    CEO = 0,
    INTERN = 1,
}

public struct WorkTime
{
    public float start;
    public float end;

}
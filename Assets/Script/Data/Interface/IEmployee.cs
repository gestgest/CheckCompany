using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IEmployee
{
    EmployeeType _EmployeeType { get; set; }
    string Name { get; set; }
    int Age { get; set; }
    int Career { get; set; } //개월 단위
    int Cost { get; set; } //월급
    //기술
}

public enum EmployeeType
{
    Product_Manager = 0,
    Developer = 1,
    Designer = 2,
    QA = 3,
}
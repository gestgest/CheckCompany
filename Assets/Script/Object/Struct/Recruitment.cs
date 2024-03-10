using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Recruitment
{
    int day; //Date
    List<IEmployee> employeeList;
    private EmployeeType employeeType;
    public void SetEmployeeType(EmployeeType employeeType)
    {
        this.employeeType = employeeType;
    }
    public EmployeeType GetEmployeeType()
    {
        return employeeType;
    }
    public void SetDay(int day)
    {
        this.day = day;
    }
    public int GetDay()
    {
        return day;
    }
    public int GetSize()
    {
        return employeeList.Count;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Recruitment
{
    int day; //Date
    int id; //모집 구분 id => 이걸로 모집 컴포넌트를 제거 생성해야한다
    int level;
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


    public int GetID()
    {
        return id;
    }
    public void SetID(int id)
    {
        this.id = id;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }
}

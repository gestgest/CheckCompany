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
    private EmployeeSO employeeSO;

    
    public void SetEmployeeSO(EmployeeSO employeeSO)
    {
        this.employeeSO = employeeSO;
    }
    public EmployeeSO GetEmployeeSO()
    {
        return employeeSO;
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

    public Dictionary<string, object> RecruitmentToJSON()
    {

        // 저장할 데이터
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "day", day },
            { "id", id },
            { "level", level },
            //{ "employeeList", employeeList},
            { "employeeType", (int)employeeSO.GetEmployeeType() },
        };
        return data;
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Recruitment
{
    int day; //Date
    int id; //모집 구분 id => 이걸로 모집 컴포넌트를 제거 생성해야한다
    int level;
    List<IEmployee> applicants;
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
        return applicants.Count;
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
            //{ "applicants", GetApplicantsToJson()},
            { "employeeType", (int)employeeSO.GetEmployeeType() },
        };
        //EmployeeToJSON
        return data;
    }


    public Dictionary<int, Dictionary<string, object>> GetApplicantsToJson() //employees를 JSON으로
    {
        Dictionary<int, Dictionary<string, object>> result = new Dictionary<int, Dictionary<string, object>>();

        if(applicants == null)
        {
            return null;
        }

        for(int i = 0; i < applicants.Count; i++)
        {
            result.Add(applicants[i].ID, applicants[i].EmployeeToJSON());
            //result.applicants[i].EmployeeToJSON());
        }

        return result;
    }
}

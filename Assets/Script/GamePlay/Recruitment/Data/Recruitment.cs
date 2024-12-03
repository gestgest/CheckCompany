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
    
    //public Recruitment()
    //{
    //    applicants = new List<IEmployee>();
    //}

    
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
            //{ "id", id },
            { "level", level },
            { "applicants", GetApplicantsToJson()},
            { "employeeType", (int)employeeSO.GetEmployeeType() },
        };
        //EmployeeToJSON
        return data;
    }

    public void JSONToRecruitment(KeyValuePair<string, object> data)
    {
        this.SetID(int.Parse(data.Key));

        Dictionary<string, object> keyValues = (Dictionary<string, object>)data.Value;
        //디버깅
        this.SetDay(Convert.ToInt32(keyValues["day"]));
        this.SetLevel(Convert.ToInt32(keyValues["level"]));
        this.SetEmployeeSO(RecruitmentController.instance.GetEmployeeSO(
            Convert.ToInt32(keyValues["employeeType"]
        )));

        if(applicants == null)
        {
            applicants = new List<IEmployee>();
        }

        foreach (KeyValuePair<string, object> serverApplicant in (Dictionary<string, object>)keyValues["applicants"])
        {
            //0, (age, careerPeriod, name, rank, salary, worktime {start, end})
            //Switch문으로 해결
            IEmployee employee = new Development();
            employee.SetEmployeeWithJson(serverApplicant);
            applicants.Add(employee);
        }
    }


    public Dictionary<string, Dictionary<string, object>> GetApplicantsToJson() //employees를 JSON으로
    {
        Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>();

        if(applicants == null)
        {
            return result;
        }

        for(int i = 0; i < applicants.Count; i++)
        {
            result.Add(applicants[i].ID.ToString(), applicants[i].EmployeeToJSON());
            //result.applicants[i].EmployeeToJSON());
        }

        return result;
    }
}

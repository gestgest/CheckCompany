using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct Recruitment
{
    int day; //Date
    int id; //모집 구분 id => 이걸로 모집 컴포넌트를 제거 생성해야한다
    int level;
    List<Employee> applicants;
    private EmployeeSO employeeSO;

    public UnityAction<int, int> _onDeleteServerApplicant; //서버 지원자 제거 함수
    
    public void Init(UnityAction<int, int> onDeleteServerApplicant)
    {
        if(applicants == null)
            applicants = new List<Employee>();

        _onDeleteServerApplicant += onDeleteServerApplicant;
    }


    #region PROPERTY

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

    public void AddApplicant(Employee applicant)
    {
        applicants.Add(applicant);
    }


    /// <summary> 서버 포함 지원자 제거 함수 </summary>
    /// <param name="applicant_id"></param>
    /// <param name="applicant_index"></param>
    public void RemoveApplicant(int applicant_id, int applicant_index)
    {
        _onDeleteServerApplicant.Invoke(GetID(), applicant_id);
        applicants.RemoveAt(applicant_index);
    }

    public int GetApplicantCount()
    {
        return applicants.Count;
    }
    public Employee GetApplicant(int index)
    {
        return applicants[index];
    }

    #endregion
    #region UI
    public void SwitchApplicant(int i, int j)
    {
        Employee tmp = applicants[i];
        applicants[i] = applicants[j];
        applicants[j] = tmp;
    }

    public int Search_Employee_Index(int id)
    {
        return Binary_Search_Employee_Index(0, GetApplicantCount() - 1, id);
    }

    private int Binary_Search_Employee_Index(int start, int end, int id)
    {
        if (start > end) return -1;
        int mid = (start + end) / 2;

        //아이디가 같다
        if (applicants[mid].ID == id)
        {
            return mid;
        }
        else if (id > applicants[mid].ID)
        {
            return Binary_Search_Employee_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Employee_Index(start, mid - 1, id);
        }

    }
    #endregion

    #region SERVER
    public Dictionary<string, object> RecruitmentToJSON() //recruitment를 
    {

        // 저장할 데이터
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "day", day },
            //{ "id", id },
            { "level", level },
            { "applicants", ApplicantsToJson()},
            { "employeeType", (int)employeeSO.GetEmployeeType() },
        };
        //EmployeeToJSON
        return data;
    }

    //employees를 JSON으로
    public Dictionary<string, Dictionary<string, object>> ApplicantsToJson() 
    {
        Dictionary<string, Dictionary<string, object>> result = new Dictionary<string, Dictionary<string, object>>();

        if (applicants == null)
        {
            return result;
        }

        for (int i = 0; i < applicants.Count; i++)
        {
            result.Add(applicants[i].ID.ToString(), applicants[i].EmployeeToJSON());
            //result.applicants[i].EmployeeToJSON());
        }

        return result;
    }



    //Recruitment 값 입력
    public void JSONToRecruitment(KeyValuePair<string, object> data, RecruitmentControllerSO rsSO, EmployeeControllerSO ecSO)
    {
        this.SetID(int.Parse(data.Key));

        Dictionary<string, object> keyValues = (Dictionary<string, object>)data.Value;
        //디버깅
        this.SetDay(Convert.ToInt32(keyValues["day"]));
        this.SetLevel(Convert.ToInt32(keyValues["level"]));
        this.SetEmployeeSO(rsSO.GetEmployeeSO(
            Convert.ToInt32(keyValues["employeeType"]
        )));

        Init(rsSO.RemoveServerApplicant);

        //지원자
        foreach (KeyValuePair<string, object> serverApplicant in (Dictionary<string, object>)keyValues["applicants"])
        {
            //0, (age, careerPeriod, name, rank, salary, worktime {start, end})
            //Switch문으로 해결 => emploeeSO로 구분
            Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO, ecSO, false);
            

            employee.JSONToEmployee(serverApplicant); //가져오는 함수
            AddApplicant(employee);

            //그러니까 이거를 그려야한다
        }
    }


    #endregion
}

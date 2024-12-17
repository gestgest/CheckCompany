using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEmployee
{
    int ID { get; set; }
    public EmployeeSO _EmployeeSO { get; set; }
    string Name { get; set; }
    int Age { get; set; }
    int CareerPeriod { get; set; } //개월 단위
    EmployeeRank _Rank { get; set; } //단위 (enum
    int Salary { get; set; } //월급단위 (만원)
    WorkTime _WorkTime { get; set; } //근무시간

    //기술
    //미션 => 5개
    MissionSO GetMission(int index);
    bool GetIsClearSmallMission(int index);
    int GetIsClearSmallMissionSize();
    void SetIsClearSmallMission(int index);
    int GetMissionSize();


    void AddMission(MissionSO m);
    void RemoveMission(int index);



    public const int MAX_MISSION_SIZE = 5;
    public const int MAX_SMALL_MISSION_SIZE = 7;
 
    //requirementEmployeeType

   //서버로 넣기 
    public Dictionary<string, object> EmployeeToJSON()
    {
        Dictionary<string, float> _worktime = new Dictionary<string, float>
        {
            { "start", _WorkTime.start },
            { "end", _WorkTime.end },
        };

        Dictionary<string, object> result = new Dictionary<string, object>
        {
            //{ "id", ID },
            { "name", Name },
            { "age", Age },
            { "careerPeriod", CareerPeriod },
            { "rank", _Rank },
            { "salary", Salary },
            { "employeeType", (int)_EmployeeSO.GetEmployeeType() }, //고용하면 채용공고가 없어지기 때문에 넣어야함
            { "worktime", _worktime },
        };
         

        return result;
    }

    //서버에 받기
    public void SetEmployeeFromJson(KeyValuePair<string, object> employee)
    {
        //0, (age, careerPeriod, name, rank, salary, worktime {start, end})
        ID = int.Parse(employee.Key);
        //_EmployeeSO는 그 전에 Recruitment의 JSONToRecruitment함수 에서 함

        Dictionary<string, object> keyValues = (Dictionary<string, object>)employee.Value;
        Age = Convert.ToInt32(keyValues["age"]);
        CareerPeriod = Convert.ToInt32(keyValues["careerPeriod"]);
        Name = keyValues["name"].ToString();
        _Rank = (EmployeeRank)Convert.ToInt32(keyValues["rank"]);
        Salary = Convert.ToInt32(keyValues["salary"]);

        Dictionary<string, object> worktime = (Dictionary<string, object>)keyValues["worktime"];
        _WorkTime = new WorkTime(Convert.ToSingle(worktime["start"]), Convert.ToSingle(worktime["end"]));

    }
}

public enum EmployeeType
{
    PRODUCT_MANAGER = 0,
    DEVELOPER = 1,
    DESIGNER = 2,
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
    public WorkTime(float start, float end)
    {
        this.start = start;
        this.end = end;
    }
}
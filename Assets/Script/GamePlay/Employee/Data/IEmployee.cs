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
            { "workTime", _worktime },
        };
        //{ "employeeType", (int)employeeSO.GetEmployeeType() } => 이거는 Recruitment와 중첩되니 패스


        return result;
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

}
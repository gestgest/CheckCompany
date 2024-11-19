using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEmployee
{
    long ID { get; set; }
    public EmployeeSO _EmployeeType { get; set; }
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
    //requirement
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
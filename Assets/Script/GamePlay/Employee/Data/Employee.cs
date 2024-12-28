using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public interface IEmployee
public class Employee
{
    int id; //식별 번호 => 우리 회사 따로 ID 또는 전체 ID
    //우리 회사 따로면 그냥 employee[식별번호] 해야함
    //전체 ID면 계속 정렬시키고 이분탐색해야함 [채택]
    private EmployeeSO employeeSO;
    private string employee_name;
    private int age;
    private int careerPeriod;
    private int salary;
    private WorkTime workTime;
    private EmployeeRank rank;

    private Mission[] missions; //5개
    private int mission_size = 0;
    //pool => 소미션 
    //private bool [] isClearSmallMission; acievement => Mission의 missions로 대체 

    public const int MAX_MISSION_SIZE = 5;
    public const int MAX_SMALL_MISSION_SIZE = 7; //소미션
    
    public int ID { get { return id; } set { id = value; } }
    public EmployeeSO _EmployeeSO { get { return employeeSO; } set { employeeSO = value; } }
    public string Name { get { return employee_name; } set { employee_name = value; } }
    public int Age { get { return age; } set { age = value; } }
    public int CareerPeriod { get { return careerPeriod; } set { careerPeriod = value; } }
    public int Salary { get { return salary; } set { salary = value; } }
    public EmployeeRank _Rank { get { return rank; } set { rank = value; } }
    public WorkTime _WorkTime { get { return workTime; } set { workTime = value; } }
    public Mission GetMission(int index) { return missions[index]; }
    public bool GetSmallMissionAchievement(int index) {  return missions[0].GetAchievement(index); }
    //public bool GetSmallMissionAchievement(int index) { return false; }

    //기술
    //미션 => 5개
    public Employee()
    {
        missions = new Mission[MAX_MISSION_SIZE];
    }
    
    //소 미션 클리어 갯수 구하기
    public int GetIsClearSmallMissionSize() 
    {
        int count = 0;
        if(mission_size == 0)
            return count;

        Mission mission = GetMission(0);
        for(int i = 0; i < mission.GetMissionSO().GetSmallMissions().Length; i++)
        {
            if(GetSmallMissionAchievement(i))
            {
                count++;
            }
        }
        return count;
    }

    public void SetIsClearSmallMission(int index) 
    { 
        missions[0].SetAchievement(index,!GetSmallMissionAchievement(index));
    }

    public int GetMissionSize() { return mission_size; }

    public void AddMission(Mission m)
    {
        if(mission_size == Employee.MAX_MISSION_SIZE)
            return; 
        missions[mission_size] = m;
        mission_size++;

        if(mission_size == 1)
            missions[0].SetAchievementAllFalse();;
    }
    public void RemoveMission(int index)
    {
        for(int i = index; i < mission_size && i < Employee.MAX_MISSION_SIZE - 1; i++)
        {
            missions[i] = missions[i + 1];
        }
        mission_size--;

        if(mission_size >= 1)
            missions[0].SetAchievementAllFalse();
    }
 
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
    public void GetEmployeeFromJson(KeyValuePair<string, object> employee)
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

        //keyValues["missions"] => List<Mission>
        //   Array로 가져오자 for문
            Mission mission;   
            //mission.GetMissionFromJSON()
            //AddMission
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
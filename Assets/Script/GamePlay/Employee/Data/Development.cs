using System.Collections.Generic;
using UnityEngine;

//Employee가 아니라 개발자, 디자이너 이런식으로 해야할 거 같은데

//개발자
public class Development : IEmployee
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

    private MissionSO[] missions; //5개
    private int mission_size = 0;
    private bool [] isClearSmallMission; //pool

    public Development()
    {
        missions = new MissionSO[IEmployee.MAX_MISSION_SIZE];
        isClearSmallMission = new bool[IEmployee.MAX_SMALL_MISSION_SIZE];
    }

    public int ID { get { return id; } set { id = value; } }
    public EmployeeSO _EmployeeSO { get { return employeeSO; } set { employeeSO = value; } }
    public string Name { get { return employee_name; } set { employee_name = value; } }
    public int Age { get { return age; } set { age = value; } }
    public int CareerPeriod { get { return careerPeriod; } set { careerPeriod = value; } }
    public int Salary { get { return salary; } set { salary = value; } }
    public EmployeeRank _Rank { get { return rank; } set { rank = value; } }
    public WorkTime _WorkTime { get { return workTime; } set { workTime = value; } }
    public MissionSO GetMission(int index) { return missions[index]; }
    public bool GetIsClearSmallMission(int index) { return isClearSmallMission[index]; }

    public int GetIsClearSmallMissionSize() 
    {
        int count = 0;
        if(mission_size == 0)
            return count;

        MissionSO mission = GetMission(0);
        for(int i = 0; i < mission.GetSmallMissions().Length; i++)
        {
            if(isClearSmallMission[i])
            {
                count++;
            }
        }
        return count;
    }

    public void SetIsClearSmallMission(int index) 
    { 
        isClearSmallMission[index] = !isClearSmallMission[index];
    }

    public int GetMissionSize() { return mission_size; }

    public void AddMission(MissionSO m)
    {
        if(mission_size == IEmployee.MAX_MISSION_SIZE)
            return; 
        missions[mission_size] = m;
        mission_size++;

        if(mission_size == 1)
            SetFirstMission();
    }
    public void RemoveMission(int index)
    {
        for(int i = index; i < mission_size && i < IEmployee.MAX_MISSION_SIZE - 1; i++)
        {
            missions[i] = missions[i + 1];
        }
        mission_size--;

        SetFirstMission();
    }
    private void SetFirstMission()
    {
        for(int i = 0; i < IEmployee.MAX_SMALL_MISSION_SIZE; i++)
        {
            isClearSmallMission[i] = false;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;


//public interface IEmployee
public class Employee
{
    int id; //식별 번호 => 전체 ID
    private EmployeeSO employeeSO;
    private string employee_name;
    private int age;

    private int stamina = 100; //체력 => 평균이 100
    private int max_stamina = 100; //최대 체력 => 평균이 100
    private int mental = 100; //멘탈
    private int max_mental = 100; //최대 멘탈
    private int careerPeriod;
    private int salary; //월급
    private WorkTime workTime;
    private EmployeeRank rank;

    private Mission[] missions; //5개
    private int mission_size = 0;

    private bool isEmployee = false;
    //private bool [] isClearSmallMission; acievement => Mission의 missions로 대체 

    public const int MAX_MISSION_SIZE = 5;
    public const int MAX_SMALL_MISSION_SIZE = 7; //소미션

    public int ID { get { return id; } set { id = value; } }
    public EmployeeSO _EmployeeSO { get { return employeeSO; } set { employeeSO = value; } }
    public string Name { get { return employee_name; } set { employee_name = value; } }
    public int Age { get { return age; } set { age = value; } }
    /*
    public int Stamina 
    {
        get { 
            return stamina;
        }
        set
        {
            stamina = value;
            if (stamina > max_stamina)
                stamina = max_stamina;
            SetStaminaToServer(GameManager.instance.Nickname, id);
        }
    }
    */
    public int Stamina => stamina;
    public void SetStamina(int value, bool toServer = true)
    {
        stamina = value;
        if (stamina > max_stamina)
            stamina = max_stamina;
        if(toServer)
            SetStaminaToServer(GameManager.instance.Nickname, id);
    }


    public int Max_Stamina { get { return max_stamina; } set { max_stamina = value; } }
    public int Mental { get { return mental; } set { mental = value; if (mental > max_mental) mental = max_mental; } }
    public int Max_Mental { get { return max_mental; } set { max_mental = value; } }
    public int CareerPeriod { get { return careerPeriod; } set { careerPeriod = value; } }
    public int Salary { get { return salary; } set { salary = value; } }
    public EmployeeRank _Rank { get { return rank; } set { rank = value; } }
    public WorkTime _WorkTime { get { return workTime; } set { workTime = value; } }
    public Mission GetMission(int index) { return missions[index]; }
    public bool Get_SmallMission_Achievement(int index) { return missions[0].GetAchievement(index); }
    public bool IsEmployee { get { return isEmployee; } set { isEmployee = value; } }
    //public bool GetSmallMissionAchievement(int index) { return false; }

    //기술
    //미션 => 5개
    public Employee()
    {
        missions = new Mission[MAX_MISSION_SIZE];
    }

    //소 미션 클리어 갯수 구하기 => Mission 함수의 GetAchievementClearCount를 이용해라 
    /*
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
    */


    public int GetMissionSize() { return mission_size; }

    public void AddMission(Mission m)
    {
        //꽉 차있다면 => 취소
        if (mission_size == Employee.MAX_MISSION_SIZE)
            return;

        missions[mission_size] = m;
        mission_size++;

        //서버에 미션을 넣는다. => 이거 초반에 가져올때 가져오고 넣어짐
        //SetMissionToServer(m, GameManager.instance.Nickname, m.GetMissionID());

        //if (mission_size == 1)
        //missions[0].SetAchievementAllFalse();
    }
    public void RemoveMission(int index)
    {
        string nickName = GameManager.instance.Nickname;
        RemoveAllMissionsToServer(GameManager.instance.Nickname, ID);
        for (int i = index; i < mission_size && i < Employee.MAX_MISSION_SIZE - 1; i++)
        {
            missions[i] = missions[i + 1];
            if (missions[i] != null)
                AddMissionToServer(missions[i], nickName, ID);
        }
        mission_size--;


    }

    //requirementEmployeeType

    #region SERVER

    public void RemoveAllMissionsToServer(string nickname, int id)
    {
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            nickname,
            "employees." + id.ToString() + ".missions",
            FieldValue.Delete
        );
    }

    public void SetAllMissionToServer(string nickname, int id)
    {
        RemoveAllMissionsToServer(nickname, id);
        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i] == null)
            {
                break;
            }
            AddMissionToServer(missions[i], nickname, id);
        }
    }

    public void AddMissionToServer(Mission m, string nickname, int id)
    {
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            nickname,
            "employees." + id.ToString() + ".missions",
            FieldValue.ArrayUnion(m.SetMissionToJSON())
        );
    }
    
    public void SetStaminaToServer(string nickname, int id)
    {
        string em = "employees.";
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            nickname,
            em + id.ToString() + ".stamina",
            stamina
        );
    }

    //JSON으로 만들기
    public Dictionary<string, object> SetEmployeeToJSON()
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
            {"stamina", Stamina },
            {"max_stamina", Max_Stamina },
            {"mental", Mental },
            {"max_mental", Max_Mental },
            { "careerPeriod", CareerPeriod },
            { "rank", _Rank },
            { "salary", Salary },
            { "employeeType", (int)_EmployeeSO.GetEmployeeType() }, //고용하면 채용공고가 없어지기 때문에 넣어야함
            { "worktime", _worktime },
            { "isEmployee", isEmployee },
        };

        //미션은?

        return result;
    }

    //서버에 받기
    public void GetEmployeeFromJSON(KeyValuePair<string, object> employee)
    {
        //0, (age, careerPeriod, name, rank, salary, worktime {start, end})
        ID = int.Parse(employee.Key);
        //_EmployeeSO는 그 전에 Recruitment의 JSONToRecruitment함수 에서 함

        Dictionary<string, object> keyValues = (Dictionary<string, object>)employee.Value;
        Age = Convert.ToInt32(keyValues["age"]);
        SetStamina(Convert.ToInt32(keyValues["stamina"]), false);
        Max_Stamina = Convert.ToInt32(keyValues["max_stamina"]);
        Mental = Convert.ToInt32(keyValues["mental"]);
        Max_Mental = Convert.ToInt32(keyValues["max_mental"]);
        CareerPeriod = Convert.ToInt32(keyValues["careerPeriod"]);
        Name = keyValues["name"].ToString();
        _Rank = (EmployeeRank)Convert.ToInt32(keyValues["rank"]);
        Salary = Convert.ToInt32(keyValues["salary"]);
        //IsEmployee = (bool)keyValues["isEmployee"];

        Dictionary<string, object> worktime = (Dictionary<string, object>)keyValues["worktime"];
        _WorkTime = new WorkTime(Convert.ToSingle(worktime["start"]), Convert.ToSingle(worktime["end"]));

        if (keyValues.TryGetValue("missions", out object output))
        {
            List<object> missions_tmp = output as List<object>;

            for (int i = 0; i < missions_tmp.Count; i++)
            {
                Dictionary<string, object> mission_map = (Dictionary<string, object>)missions_tmp[i];
                Mission mission = new Mission();
                mission.GetMissionFromJSON(mission_map);
                AddMission(mission);
            }
        }
        //세팅?
    }

    #endregion

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
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


//MissionSO를 Todo_Mission으로 교체 => 동적으로 생성가능
public class Mission
{
    private int id;
    private EmployeeType mission_type = EmployeeType.DEVELOPER; //회복, 지능, 기술, 명상 이런식으로 해야하나
    private string missionName;
    private int iconID;
    private Sprite icon;

    private int level; //easy, medium, hard, very hard
    private List<Todo_Mission> todo_missions;

    //private bool isDone = false; //전체 다 완료했는지 => 그냥 GetIsDone
    private Date doneDate;

    public Mission()
    {
        doneDate = new Date();
        this.todo_missions = new List<Todo_Mission>();
    }

    /// <summary> 서버에서 가져오는 ?</summary>
    /// <param name="id"></param>
    /// <param name="_type"></param>
    /// <param name="_name"></param>
    /// <param name="iconID"></param>
    /// <param name="level"></param>
    /// <param name="todo_missions"></param>
    public Mission(int id, int _type, string _name, Sprite icon, int iconID, int level,
        List<Todo_Mission> todo_missions)
    {
        //this.todo_missions = new List<Todo_Mission>();

        this.id = id;
        this.mission_type = (EmployeeType)(_type);
        this.missionName = _name;

        this.icon = icon;
        this.iconID = iconID;
        this.level = level;

        this.todo_missions = todo_missions;
        //for (int i = 0; i < todo_missions.Count; i++)
        //{
        //    this.todo_missions.Add(todo_missions[i]);
        //}
    }

    public int ID
    {
        get => id;
        set => id = value;
    }

    public EmployeeType GetMissionType()
    {
        return mission_type;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public void SetIcon(Sprite icon)
    {
        this.icon = icon;
    }

    public List<Todo_Mission> GetTodoMissions()
    {
        return todo_missions;
    }
    public Todo_Mission GetTodoMission(int index)
    {
        return todo_missions[index];
    }

    /// <summary>todo 리스트들이 다 됐는지 </summary>
    /// <returns></returns>
    public bool GetIsDone()
    {
        for (int i = 0; i < todo_missions.Count; i++)
        {
            if (!todo_missions[i].IsDone)
            {
                return false;
            }
        }

        return true;
    }

    public int GetDoneCount()
    {
        int count = 0;
        for (int i = 0; i < todo_missions.Count; i++)
        {
            if (!todo_missions[i].IsDone)
            {
                count++;
            }
        }

        return count;
    }
    
    private void SetDoneDate(Date date)
    {
        doneDate = date;
    }

    public void Set_TodoMission_IsDone(int index, bool isDone)
    {
        todo_missions[index].IsDone = isDone;

        //완료됐으면 완료된 날짜 넣기 
        if (GetIsDone())
        {
            Date date = new Date();
            date.SetDate(DateTime.Now);

            SetDoneDate(date);
        }
    }

    public string GetName()
    {
        return missionName;
    }

    public int GetLevel()
    {
        return level;
    }

    public Date GetDoneDate()
    {
        return doneDate;
    }

    //서버 보내기
    public Dictionary<string, object> MissionToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            { "type", (int)mission_type },
            { "name", missionName },
            { "icon", iconID },
            { "level", level },
            { "todo_missions", todo_missions }, //배열임
        };

        if (GetIsDone())
        {
            result.Add(
                "doneDate", DateToJSON()
            );
        }

        ;

        return result;
    }

    public Dictionary<string, object> DateToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            { "year", doneDate.Year },
            { "month", doneDate.Month },
            { "day", doneDate.Day },
        };
        return result;
    }

    /// <summary> 서버에서 가져오는 </summary>
    /// <param name="data"></param>
    public void JSONToMission(Dictionary<string, object> data)
    {
        mission_type = (EmployeeType)(Convert.ToInt32(data["type"]));
        missionName = (string)data["name"];
        iconID = Convert.ToInt32(data["icon"]);
        level = Convert.ToInt32(data["level"]);

        List<object> todo_missions = (List<object>)data["todo_missions"];

        for (int i = 0; i < todo_missions.Count; i++)
        {
            Todo_Mission todo_mission = new Todo_Mission();
            todo_mission.Set_Todo_Mission(todo_missions[i]);
            this.todo_missions.Add(todo_mission);
        }

        //완료된 날짜
        if (GetIsDone())
        {
            Dictionary<string, object> doneDate = (Dictionary<string, object>)data["doneDate"];

            this.doneDate.Year = Convert.ToInt32(doneDate["year"]);
            this.doneDate.Month = Convert.ToInt32(doneDate["month"]);
            this.doneDate.Day = Convert.ToInt32(doneDate["day"]);
        }
    }
}

//EmployeeType

/*
public enum MissionType
{
    NONE = 0, //비어있는 상태
    SQL_DEV = 1, //SQL 개발
    CLIENT_DEV = 2, //클라이언트 개발
    CODING_TEST = 3,
    ENGINE_DEV = 4, //엔진개발
    WEB_FRONT_DEV = 5, //프론트엔드
    APP_DEV = 6, //앱개발
    DATA_ANALYSIS = 7, //데이터 분석
}
*/
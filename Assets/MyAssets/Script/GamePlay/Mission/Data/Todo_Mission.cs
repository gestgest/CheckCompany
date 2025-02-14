using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


//MissionSO를 Todo_Mission으로 교체 => 동적으로 생성가능
public class Todo_Mission
{
    private int id;
    private EmployeeType mission_type = EmployeeType.DEVELOPER; //회복, 지능, 기술, 명상 이런식으로 해야하나
    private string missionName;
    private int iconID;
    private int level; //easy, medium, hard, very hard
    private List<string> small_missions;

    public Todo_Mission() 
    {
        this.small_missions = new List<string>();
    }

    public Todo_Mission(int id, int _type, string _name, int iconID, int level, List<string> small_missions)
    {
        this.small_missions = new List<string>();

        this.id = id;
        this.mission_type = (EmployeeType)(_type);
        this.missionName = _name;
        this.iconID = iconID;
        this.level = level;

        for (int i = 0; i < small_missions.Count; i++)
        {
            this.small_missions.Add(small_missions[i]);
        }
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
        return MissionController.instance.GetIcon(iconID);
    }
    public List<string> GetSmallMissions()
    {
        return small_missions;
    }
    public string GetName()
    {
        return missionName;
    }

    public Dictionary<string, object> GetTodoMission_ToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            { "type", (int)mission_type },
            { "name", missionName },
            { "icon", iconID },
            { "level", level },
            { "small_missions", small_missions }, //배열임
        };

        return result;
    }


    public void SetMissionFromJSON(Dictionary<string, object> data)
    {
        mission_type = (EmployeeType)(Convert.ToInt32(data["type"]));
        missionName = (string)data["name"];
        iconID = Convert.ToInt32(data["icon"]);
        level = Convert.ToInt32(data["level"]);
        List<object> sm_tmp = (List<object>)data["small_missions"];
        for (int i = 0; i < sm_tmp.Count; i++) 
        {
            small_missions.Add((string)sm_tmp[i]);
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
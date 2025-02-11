using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


//MissionSO를 Todo_Mission으로 교체 => 동적으로 생성가능
public class Todo_Mission
{
    private int ID;
    private MissionType mission_type = MissionType.NONE;
    private string missionName;
    private int iconID;
    private List<string> small_missions;

    public Todo_Mission(int id, int _type, string _name, int iconID, List<string> small_missions)
    {
        this.small_missions = new List<string>();
        this.ID = id;
        this.mission_type = (MissionType)(_type);
        this.missionName = _name;
        this.iconID = iconID;

        for (int i = 0; i < small_missions.Count; i++)
        {
            this.small_missions.Add(small_missions[i]);
        }
    }


    public MissionType GetMissionType()
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

    public int GetID()
    {
        return ID;
    }

    public Dictionary<string, object> Get_TodoMission_ToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            { "type", (int)mission_type },
            { "name", missionName },
            { "icon", (int)iconID },
            { "small_missions", small_missions }, //배열임
        };

        return result;
    }
}
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

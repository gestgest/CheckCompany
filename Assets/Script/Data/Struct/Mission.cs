using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Mission
{
    MissionType mission_type;
    string[] missions;

    public MissionType GetMissionType() { return mission_type; }
    public void SetMissionType(MissionType mt) { mission_type = mt;}
    public string[] GetMissions() { return missions; }
    public void SetMissions(string[] missions) { this.missions = missions;}
}

public enum MissionType
{
    SQL_DEV = 0, //SQL 개발
}

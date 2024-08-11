using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MissionSO", menuName = "ScriptableObject/MissionSO")]
public class MissionSO : ScriptableObject
{
    [SerializeField] private MissionType mission_type = MissionType.NONE;
    [SerializeField] private string missionName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string[] missions;

    public MissionType GetMissionType()
    {
        return mission_type;
    }
    public Sprite GetIcon()
    {
        return icon;
    }
    public string GetMission(int index)
    {
        return missions[index];
    }
    public string GetName()
    {
        return missionName;
    }
}


public enum MissionType
{
    NONE = 0, //비어있는 상태
    SQL_DEV = 1, //SQL 개발
    CLIENT_DEV = 2, //클라이언트 개발
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MissionSO", menuName = "ScriptableObject/MissionSO")]
public class MissionSO : ScriptableObject
{
    [SerializeField] private int ID;
    [SerializeField] private MissionType mission_type = MissionType.NONE;
    [SerializeField] private string missionName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string[] small_missions;

    public MissionType GetMissionType()
    {
        return mission_type;
    }
    public Sprite GetIcon()
    {
        return icon;
    }
    public string [] GetSmallMissions()
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
}


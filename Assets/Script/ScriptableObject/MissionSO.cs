using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MissionSO", menuName = "ScriptableObject/MissionSO")]
public class MissionSO : ScriptableObject
{
    [SerializeField] private MissionType mission_type;
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
}


public enum MissionType
{
    SQL_DEV = 0, //SQL 개발
    CLIENT_DEV = 1, //클라이언트 개발
}

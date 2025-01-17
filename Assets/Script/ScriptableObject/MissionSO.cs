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

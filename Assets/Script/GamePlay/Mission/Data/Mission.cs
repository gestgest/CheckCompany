using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    private MissionSO m_SO;

    //달성률
    //생각해보니 달성률이 아니라 각각의 미션 달성을 true false 해야할듯
    private bool [] achievementList; //small_missions 사이즈만큼 할당
    private int achievementClearCount;
    
    int mission_id; 
    //어떤 미션을 가지고 있는지 => 나중에 미션 컨트롤러에서 m_SO를 가져오기 위한
    
    /*
    mission
    - achievementList : bool<List>
    - id : number
        
    */

    public Mission(MissionSO missionSO)
    {
        SetMissionSO(missionSO);
    }

    #region SERVER
    //서버에서 가져온 JSON 타입을 Mission에 넣기
    public void GetMissionFromJSON(Dictionary<string, object> mission)
    {
        //achievementList = Convert.ToInt32(mission["achievementList"]);
        int id = Convert.ToInt32(mission["id"]);
        
        //id 탐색
        m_SO = MissionController.instance.GetMission(id);
    }

    //서버 보내기 
    public void SetMissionToServer(string nickname)
    {
        Dictionary<string, object> mission = new Dictionary<string, object>
        {
            {"achievementList", achievementList},
            {"id", mission_id}
        };
        
        //서버에 보내기
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            nickname,
            "mission",
            mission
        );
    }

    #endregion
    #region PROPERTIES

    public MissionSO GetMissionSO()
    {
        return m_SO;
    }

    public void SetMissionSO(MissionSO missionSO)
    {
        m_SO = missionSO;
        achievementList = new bool[missionSO.GetSmallMissions().Length];
        //Debug.Log("achievementList : " + missionSO.GetSmallMissions().Length);
        // 애초에 0 Debug.Log("achievementList : " + achievementList.Count);

        SetAchievementAllFalse();
    }

    public bool GetAchievement(int index)
    {
        return achievementList[index];
    }
    public int GetAchievementClearCount()
    {
        return achievementClearCount;
    }
    
    public void SetAchievement(int index, bool isAchieved)
    {
        //한번에 두번 호출
        //Debug.Log("엄준식" + achievementClearCount);
        achievementList[index] = isAchieved;
        if (isAchieved)
        {
            achievementClearCount++;
        }
        else
        {
            achievementClearCount--;
        }
    }

    public void SetAchievementAllFalse()
    {
        for(int i = 0; i < achievementList.Length; i++)
        {
            achievementList[i] = false;
        }

        achievementClearCount = 0;
    }
    
    #endregion
}
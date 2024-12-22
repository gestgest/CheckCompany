using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    private MissionSO m_SO;

    //달성률
    int goal;
    int mission_id; //어떤 미션을 가지고 있는지 => 나중에 미션 컨트롤러에서 m_SO를 가져오기 위한
    
    /*
    mission
    - goal : number
    - id : number
        
    */

    #region SERVER
    //서버에서 가져온 JSON 타입을 Mission에 넣기
    void GetMissionFromJSON(Dictionary<string, object> mission)
    {
        goal = Convert.ToInt32(mission["goal"]);
        int id = Convert.ToInt32(mission["id"]);
        
        //id 탐색
        m_SO = MissionController.instance.GetMission(id);
    }

    void SetMissionToServer(string nickname)
    {
        Dictionary<string, object> mission = new Dictionary<string, object>
        {
            {"goal", goal},
            {"id", mission_id}
        };
        
        //서버에 보내기
        FireStoreManager.instance.SetFirestoreData("GamePlayUser", nickname,"mission", mission);
    }

    #endregion
    #region PROPERTIES

    public MissionSO GetMissionSO()
    {
        return m_SO;
    }

    public void SetMissionSO(MissionSO mission)
    {
        m_SO = mission;
    }
    
    #endregion
}
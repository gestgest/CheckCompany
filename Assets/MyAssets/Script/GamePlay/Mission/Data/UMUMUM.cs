using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//이거 지울 예정
public class UMUMUM
{
    private Mission todo_mission;

    private MissionControllerSO _missionControllerSo;

    //달성률
    //생각해보니 달성률이 아니라 각각의 미션 달성을 true false 해야할듯
    private bool [] achievementList; //small_missions 사이즈만큼 할당
    private int achievementClearCount;
    
    int mission_id;
    //어떤 미션을 가지고 있는지 => 나중에 미션 컨트롤러에서 todo_mission를 가져오기 위한

    /*
    mission
    - achievementList : bool<List>
    - id : number
        
    */
    public UMUMUM()  { }
    public UMUMUM(Mission missionSO)
    {
        SetTodo_Mission(missionSO);
    }

    #region SERVER
    //서버에서 가져온 JSON 타입을 Mission에 넣기
    public void GetMissionFromJSON(Dictionary<string, object> mission)
    {
        //id 탐색
        int id = Convert.ToInt32(mission["id"]);
        int index = _missionControllerSo.Search_Mission_Index(id);
        SetTodo_Mission(_missionControllerSo.GetMission(index));
        
        List<object> achievementList_tmp = (List<object>)mission["achievementList"];
        for (int i = 0; i < achievementList_tmp.Count; i++)
        {
            achievementList[i] = (bool)achievementList_tmp[i];
            if (achievementList[i])
            {
                achievementClearCount++;
            }
        }
    }

    //JSON으로 만들기 (서버 보내기)
    public Dictionary<string, object> SetMissionToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>
        {
            {"achievementList", achievementList},
            {"id", mission_id}
        };
        return result;
    }

    #endregion
    #region PROPERTIES

    public Mission GetTodo_Mission()
    {
        return todo_mission;
    }

    public void SetTodo_Mission(Mission todo_mission)
    {
        this.todo_mission = todo_mission;
        mission_id = this.todo_mission.ID;
        achievementList = new bool[todo_mission.GetTodoMissions().Count];
        
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

    public int GetMissionID()
    {
        return mission_id;
    }
    
    #endregion
}
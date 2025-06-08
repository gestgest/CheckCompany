using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CompleteMissionPanel : MissionPanel
{
    //start => 
    protected override void OnEnable()
    {
        bool isQuery = _missionManager.GetIsQuery();

        SetMissionCount(0);
        for (int i = 0; i < MISSION_MAX_SIZE; i++)
        {
            missionElementPoolObjects[i].gameObject.SetActive(false);
        }
        
        //base.Start();
        foreach (Mission mission in _missionManager.GetMissions())
        {
            //만약 완료된 미션이 있는 경우(쿼리) => 나중에 여기 if부분만 함수로 나눌 예정
            if (mission.GetIsDone())
            {
                if(Query(mission.GetDoneDate(), isQuery))
                {
                    CreateMissionElementObject(mission);
                }
            }
        }
        
        for (int i = GetMissionCount(); i < MISSION_MAX_SIZE; i++)
        {
            missionElementPoolObjects[i].gameObject.SetActive(false);
        }

        CreateEditPanelIndex();
    }


    private bool Query(Date mission_date, bool isQuery)
    {
        Date queryDate = _missionManager.GetCompleteDate();
        //false면
        if(!isQuery)
        {
            return true;
        }
        if (mission_date.Year != queryDate.Year)
        {
            return false;
        }
        if (mission_date.Month != queryDate.Month)
        {
            return false;
        }
        if (mission_date.Day != queryDate.Day)
        {
            return false;
        }
        return true;
    }
    
}

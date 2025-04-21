using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class CompleteMissionPanel : MissionPanel
{
    private Date queryDate;
    private bool isQuery = false;

    private void Awake()
    {
        queryDate = new Date();
    }

    //start => 
    protected override void OnEnable()
    {
        SetMissionCount(0);
        for (int i = 0; i < MISSION_MAX_SIZE; i++)
        {
            missionElementPoolObjects[i].gameObject.SetActive(false);
        }
        
        //base.Start();
        foreach (Mission mission in MissionController.instance.GetMissions())
        {
            //만약 완료된 미션이 있는 경우(쿼리)
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


    public void SetQueryDate(Date date)
    {
        queryDate.Year = date.Year;
        queryDate.Month = date.Month;
        queryDate.Day = date.Day;

        SetIsQuery(true);
    }

    //버튼용
    public void OffIsQuery()
    {
        this.isQuery = false;
    }

    private void SetIsQuery(bool isQuery)
    {
        this.isQuery = isQuery;
    }

    private bool Query(Date mission_date, bool isQuery)
    {
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


    void DeleteMission()
    {
        
    }
    
    
}

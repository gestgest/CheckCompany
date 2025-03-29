using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CompleteMissionPanel : MissionPanel
{
    //start => 
    protected override void Start()
    {
        //base.Start();
        foreach (Mission mission in MissionController.instance.GetMissions())
        {
            if (mission.GetIsDone())
                CreateMissionElementObject(mission);
        }
        
        for (int i = GetMissionCount(); i < MISSION_MAX_SIZE; i++)
        {
            missionElementPoolObjects[i].gameObject.SetActive(false);
        }

        CreateEditPanelIndex();
    }

    void DeleteMission()
    {
        
    }
    
    
}

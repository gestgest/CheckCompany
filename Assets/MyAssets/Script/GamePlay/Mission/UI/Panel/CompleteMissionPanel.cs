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
                CreateTodoMissionObject(mission);
        }

        CreateEditPanelIndex();
    }

    void DeleteMission()
    {
                
    }
    
    
}

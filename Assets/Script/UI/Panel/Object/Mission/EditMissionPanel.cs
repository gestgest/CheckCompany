using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EditMissionPanel : ManagerMissionPanel
{
    //소미션
    private List<bool> todo_missions_isdone;

    private int mission_id;
    
    private List<int> refEmployees;

    /// <summary> init </summary>
    /// <param name="mission"></param>
    public void SetMission(Mission mission)
    {
        Init();

        
        refEmployees = mission.RefEmployees;
        _createMissionManager.SetRefEmployeesID(refEmployees);
        
        mission_id = mission.ID;
        todo_missions_isdone = new List<bool>();

        //값 넣기
        title.text = mission.GetName();
        
        employeeTypeGroup.SetIndex((int)mission.GetMissionType());
        levelGroup.SetIndex(mission.GetLevel());


        List<Todo_Mission> _todo_missions = mission.GetTodoMissions();
        int _todo_mission_size = _todo_missions.Count;

        //Add todo text
        _todoMissionObjects[0].transform.GetChild(0).GetComponent<TMP_InputField>().text =
            _todo_missions[0].Title;
        todo_missions_isdone.Add(_todo_missions[0].IsDone);

        todo_mission_size = 1;

        for (int i = 1; i < _todo_mission_size; i++)
        {
            todo_missions_isdone.Add(_todo_missions[i].IsDone);

            //오브젝트 생성
            AddTodoMission();
            _todoMissionObjects[i].transform.GetChild(0).GetComponent<TMP_InputField>().text =
                _todo_missions[i].Title;
        }
        
    }

    protected override bool AddIsDone(int i)
    {
        if(i < todo_missions_isdone.Count)
        {
            return todo_missions_isdone[i];
        }

        return false;
    }

    //최종적으로 값을 서버에 수정
    public void EditMission()
    {
        if (title.text == "")
        {
            Debug.LogWarning("Title field is empty!");
            return;
        }
        
        Mission mission = _createMissionManager.CreateMission(mission_id, title.text, GetTodoMissions()); 
        _missionManager.SetServerMission(mission);
        //값을 미션 적용, 이후 모든 오브젝트 초기화
        

        int index = _missionManager.Search_Mission_Index(mission_id);
        _missionManager.SetMission(mission, index);
        //굳이 리롤이 필요할까

        //back 네비 Panel
        PanelManager.instance.Back_Nav_Panel();
    }

    //delete mission on the server
    public void RemoveMission()
    {
        _missionManager.RemoveMission(mission_id);
        PanelManager.instance.Back_Nav_Panel();
    }
}
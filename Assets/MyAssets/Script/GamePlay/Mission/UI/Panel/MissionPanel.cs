using System.Collections.Generic;
using UnityEngine;

public class MissionPanel : Panel
{
    [SerializeField] private GameObject missionPrefab;
    [SerializeField] private Transform missionParent;
    [SerializeField] private EditPanel missionEditPanel;

    private List<int> editPanelIndex;
    
    protected override void Start()
    {
        base.Start();
        foreach (Todo_Mission mission in MissionController.instance.GetMissions())
        {
            CreateTodoMissionObject(mission);
        }
        editPanelIndex = new List<int>();
        editPanelIndex.Add(1);
        editPanelIndex.Add(1);
    }

    private void CreateTodoMissionObject(Todo_Mission todoMission)
    {
        //Todo_Mission 만들기
        GameObject missionObject = Instantiate(missionPrefab, missionParent);

        TodoMissionElementUI missionElementUI = missionObject.GetComponent<TodoMissionElementUI>();
        missionElementUI.SetMission(todoMission);
        
        //Debug.Log(todoMission.ID);
        //editPanel 들어가는 함수를 missionElementUI에 투입
        missionElementUI.AddEventListener(() => { EditPanelOn(todoMission.ID); });
    }

    private void EditPanelOn(int id)
    {
        Debug.Log(id);
        int index = MissionController.instance.Search_Employee_Index(id);
        Debug.Log(index);

        //editPanel에게 값 전달
        missionEditPanel.SetMission(MissionController.instance.GetMission(index));
        GamePanelManager.instance.SwitchingPanel(editPanelIndex);
        //missionEditPanel.gameObject.SetActive(true);
    }
}
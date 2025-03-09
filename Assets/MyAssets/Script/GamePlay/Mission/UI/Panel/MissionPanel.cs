using System.Collections.Generic;
using UnityEngine;

public class MissionPanel : Panel
{
    [SerializeField] private GameObject missionPrefab;
    [SerializeField] private Transform missionParent;
    [SerializeField] private EditPanel missionEditPanel;

    List<GameObject> missionObjects = new List<GameObject>();
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

    public void CreateTodoMissionObject(Todo_Mission todoMission)
    {
        //Todo_Mission 만들기
        GameObject missionObject = Instantiate(missionPrefab, missionParent);
        
        missionObjects.Add(missionObject);
        TodoMissionElement missionElementUI = missionObject.GetComponent<TodoMissionElement>();
        missionElementUI.SetMission(todoMission);
        
        //Debug.Log(todoMission.ID);
        //editPanel 들어가는 함수를 missionElementUI에 투입
        missionElementUI.AddEventListener(() => { EditPanelOn(todoMission.ID); });
    }

    public void RemoveMissionObject(int index)
    {
        Destroy(missionObjects[index]);
        missionObjects.RemoveAt(index);
    }

    private void EditPanelOn(int id)
    {
        int index = MissionController.instance.Search_Employee_Index(id);

        GamePanelManager.instance.SwitchingSubPanel(true, editPanelIndex);

        //editPanel에게 값 전달
        missionEditPanel.SetMission(MissionController.instance.GetMission(index));
        //missionEditPanel.gameObject.SetActive(true);
    }
}
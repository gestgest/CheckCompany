using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MissionPanel : Panel
{
    [SerializeField] private GameObject missionPrefab;
    [SerializeField] private Transform missionParent;
    [SerializeField] private EditMissionPanel missionEditPanel;

    [SerializeField] private MultiLayoutGroup multiLayoutGroup;

    List<MissionElement> missionElementObjects = new List<MissionElement>();
    private List<int> editPanelIndex;

    private static int MISSION_HEIGHT = 100;
    private static int MISSION_SPACE_HEIGHT = 50;

    protected override void Start()
    {
        base.Start();
        foreach (Mission mission in MissionController.instance.GetMissions())
        {
            CreateTodoMissionObject(mission);
        }
        editPanelIndex = new List<int>();
        editPanelIndex.Add(1);
        editPanelIndex.Add(1);
    }

    public void CreateTodoMissionObject(Mission todoMission)
    {
        //Todo_Mission 만들기
        GameObject missionObject = Instantiate(missionPrefab, missionParent);
        
        MissionElement missionElementUI = missionObject.GetComponent<MissionElement>();
        missionElementObjects.Add(missionElementUI);
        missionElementUI.SetMission(todoMission);

        multiLayoutGroup.AddHeight(MISSION_HEIGHT);
        multiLayoutGroup.AddHeight(MISSION_SPACE_HEIGHT);

        //Debug.Log(todoMission.ID);
        //editPanel 들어가는 함수를 missionElementUI에 투입
        missionElementUI.AddEventListener(() => { EditPanelOn(todoMission.ID); });
    }

    public void RemoveMissionObject(int index)
    {
        Destroy(missionElementObjects[index]);
        missionElementObjects.RemoveAt(index);
    }

    /// <summary>
    /// 정해진 미션을 editPanel로 옮기는 함수
    /// </summary>
    /// <param name="id"></param>
    private void EditPanelOn(int id)
    {
        int index = MissionController.instance.Search_Mission_Index(id);

        GamePanelManager.instance.SwitchingSubPanel(true, editPanelIndex);

        //editPanel에게 값 전달
        missionEditPanel.SetMission(MissionController.instance.GetMission(index));
        //missionEditPanel.gameObject.SetActive(true);
    }

    public void SetMissionObject(Mission mission, int index)
    {
        missionElementObjects[index].SetMission(mission, false);
    }


    #region BINARY_SEARCH
    public int Search_MissionObject_Index(int id)
    {
        int index = Binary_Search_MissionObject_Index(0, missionElementObjects.Count - 1, id);
        if (missionElementObjects[index].GetMission().ID == id)
        {
            return index;
        }
        return -1;
    }

    private int Binary_Search_MissionObject_Index(int start, int end, int id)
    {
        if (start > end)
        {
            return start;
        }
        int mid = (start + end) / 2;
        if (id > missionElementObjects[mid].GetMission().ID)
        {
            return Binary_Search_MissionObject_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_MissionObject_Index(start, mid - 1, id);
        }
    }
    #endregion
}
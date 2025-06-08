using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public class MissionPanel : Panel
{
    [SerializeField] private GameObject missionPrefab;
    [SerializeField] private EditMissionPanel missionEditPanel;

    [SerializeField] private MultiLayoutGroup multiLayoutGroup;

    //pool링 예정
    [SerializeField] protected MissionElement [] missionElementPoolObjects = new MissionElement[MISSION_MAX_SIZE];

    [SerializeField] protected MissionManagerSO _missionManager;


    private int mission_count = 0; //현재 미션 카운트
    protected List<int> editPanelIndex; 

    protected const int MISSION_MAX_SIZE = 20;
    private const int MISSION_HEIGHT = 100;
    private const int MISSION_SPACE_HEIGHT = 50;
    
    
    //그냥 활성화할때마다 쿼리해야할듯 => Mission은 다른 Panel에서도 너무 바뀜
    protected virtual void OnEnable()
    {
        RerollPanel();
        //base.Start();
        // List<Mission> missions = missionsSO.GetMissions();
        // for (i = 0; i < missions.Count; i++)
        // {
        //     if (!missions[i].GetIsDone())
        //         CreateMissionElementObject(missions[i]);
        // }
        // for (; i < MISSION_MAX_SIZE; i++)
        // {
        //     missionElementPoolObjects[i].gameObject.SetActive(false);
        // }


    }

    /// <summary> EditPanel index 리스트 추가하는 함수 </summary>
    protected void CreateEditPanelIndex()
    {
        editPanelIndex = new List<int>();
        editPanelIndex.Add(1);
        editPanelIndex.Add(1);
    }
    
    public void CreateMissionElementObject(Mission mission)
    {
        //Todo_Mission 만들기
        //GameObject missionObject = Instantiate(missionPrefab, missionParent);
        
        missionElementPoolObjects[mission_count].gameObject.SetActive(true);
        MissionElement missionElementUI = missionElementPoolObjects[mission_count];
        missionElementUI.SetMission(mission); //미션 지정

        multiLayoutGroup.AddHeight(MISSION_HEIGHT);
        multiLayoutGroup.AddHeight(MISSION_SPACE_HEIGHT);

        //Debug.Log(todoMission.ID);
        //editPanel 들어가는 함수를 missionElementUI에 투입
        missionElementUI.AddEventListener(() => { EditPanelOn(mission.ID); });
        mission_count++;
    }

    public void AddMissionElementObject(Mission mission)
    {
        CreateMissionElementObject(mission);
        SelectionMissionObjectSort(); //정렬
    }
    
    /// <summary> 미션 오브젝트 제거하는 함수 </summary>
    /// <param name="index">인덱스</param>
    public void RemoveMissionObject(int index)
    {
        //한 칸씩 땡기기
        mission_count--;
        for (int i = index; i < mission_count; i++)
        {
            missionElementPoolObjects[i] = missionElementPoolObjects[i + 1];
        }
        missionElementPoolObjects[mission_count].gameObject.SetActive(false); 
        
        multiLayoutGroup.AddHeight(-MISSION_HEIGHT);
        multiLayoutGroup.AddHeight(-MISSION_SPACE_HEIGHT);
        
        //GamePanelManager.instance.SwitchingPanelFromInt(1); //missionPanel로 전환
    }

    
    
    /// <summary> 정해진 미션을 editPanel로 옮기는 함수 </summary>
    /// <param name="id"></param>
    private void EditPanelOn(int id)
    {
        int index = _missionManager.Search_Mission_Index(id);

        GamePanelManager.instance.SwitchingSubPanel(true, editPanelIndex);

        //editPanel에게 값 전달
        missionEditPanel.SetMission(_missionManager.GetMission(index));
        //missionEditPanel.gameObject.SetActive(true);
    }

    public void SetMissionObject(Mission mission, int index)
    {
        missionElementPoolObjects[index].SetMission(mission, false);
    }

    public int GetMissionCount()
    {
        return mission_count;
    }
    public void SetMissionCount(int value)
    {
        mission_count = value;
    }

    #region BINARY_SEARCH
    public int Search_MissionObject_Index(int id)
    {
        int index = Binary_Search_MissionObject_Index(0, missionElementPoolObjects.Length - 1, id);
        if (missionElementPoolObjects[index].GetMission().ID == id)
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
        if (id > missionElementPoolObjects[mid].GetMission().ID)
        {
            return Binary_Search_MissionObject_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_MissionObject_Index(start, mid - 1, id);
        }
    }
    #endregion
    

    private void RerollPanel()
    {
        //Initialize Layout 
        if (mission_count != 0)
            multiLayoutGroup.AddHeight(-multiLayoutGroup.GetHeight());

        mission_count = 0;
        for (int i = 0; i < MISSION_MAX_SIZE; i++)
        {
            missionElementPoolObjects[i].gameObject.SetActive(false);
        }


        foreach (Mission mission in _missionManager.GetMissions())
        {
            //여기서 if문만 따로 함수를 만들면 됨 => 상속버전
            if (!mission.GetIsDone())
                CreateMissionElementObject(mission);
        }

        //for (int i = mission_count; i < MISSION_MAX_SIZE; i++)
        //{
        //    missionElementPoolObjects[i].gameObject.SetActive(false);
        //}
        CreateEditPanelIndex();
    }

    //미션 오브젝트 정렬하는 함수
    
    private void SelectionMissionObjectSort()
    {
        //O(n^2) => 나중에 다른 정렬로 바꾸지 않을까
        for(int i = 0; i < mission_count; i++)
        {
            for (int j = i + 1; j < mission_count; j++)
            {
                if (missionElementPoolObjects[i].GetMission().ID >
                    missionElementPoolObjects[j].GetMission().ID)
                {
                    SwitchMissionElementObject(i, j);

                    // Transform a = missionElementPoolObjects[i].transform;
                    // int a_index = a.GetSiblingIndex();
                    // Transform b = missionElementPoolObjects[j].transform;
                    // int b_index = b.GetSiblingIndex();
                    //
                    // //Debug.Log("a : " +a_index);
                    // //Debug.Log("b : " +b_index);
                    // b.SetSiblingIndex(a_index);
                    // a.SetSiblingIndex(b_index);
                    //
                    // GameObject tmp_object = applicant_objects[i];
                    // applicant_objects[i] = applicant_objects[j];
                    // applicant_objects[j] = tmp_object;
                }
            }
        }
    }
    
    public void SwitchMissionElementObject(int i, int j)
    {
        MissionElement tmp = missionElementPoolObjects[i];
        missionElementPoolObjects[i] = missionElementPoolObjects[j];
        missionElementPoolObjects[j] = tmp;
    }
}
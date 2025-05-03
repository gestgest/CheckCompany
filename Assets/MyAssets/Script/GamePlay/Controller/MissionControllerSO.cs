using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionControllerSO", menuName = "ScriptableObject/Controller/MissionControllerSO")]
public class MissionControllerSO : ScriptableObject
{
    //이미지 리스트?
    [SerializeField] private IconsSO iconsSO;

    //init
    private MissionPanel missionPanel;
    private CompleteMissionPanel completeMissionPanel;


    private List<Mission> missions;
    private int mission_count;


    //나중에 없앨 함수
    public void Init(MissionPanel missionPanel, CompleteMissionPanel completeMissionPanel)
    {
        if (missions == null)
            missions = new List<Mission>();

        this.missionPanel = missionPanel;
        this.completeMissionPanel = completeMissionPanel;

    }

    //서버 가져오는 함수 => 나중에 매개변수에 Dictionary<string, object> todo_missions와 id를 넣을 예정
    public void SetMissionData(Dictionary<string, object> data, int mission_count)
    {
        if (missions == null)
        {
            missions = new List<Mission>();
        }

        this.mission_count = mission_count;

        //null처리
        if (mission_count == 0)
        {
            MissionCountToServer();
        }
        if (data == null)
        {
            return;
        }


        JSONToMissions(data);
    }


    public int GetMissionCount()
    {
        return mission_count;
    }
    public Mission GetMission(int index)
    {
        return missions[index];
    }
    public void SetMission(Mission mission, int index)
    {
        missions[index] = mission;
    }



    public Sprite GetIcon(int index)
    {
        return iconsSO.icons[index];
    }

    public List<Mission> GetMissions()
    {
        return missions;
    }

    public int GetAndIncrementCount()
    {
        return mission_count++;
    }

    public int GetMissionSize()
    {
        return missions.Count;
    }

    public MissionPanel GetMissionPanel()
    {
        return missionPanel;
    }

    public CompleteMissionPanel GetCompleteMissionPanel()
    {
        return completeMissionPanel;
    }


    public void AddMission(Mission mission)
    {
        missions.Add(mission);
    }
    public void RemoveMission(int id)
    {
        int index = Search_Mission_Index(id);
        if (index != -1)
        {
            missions.RemoveAt(index);
            missionPanel.RemoveMissionObject(index);
        }
        PanelManager.instance.Back_Nav_Panel();
    }

    public void Reroll_MissionElement(int index)
    {
        missionPanel.SetMissionObject(missions[index], index);
    }


    /// <summary>미션 서버(json)에서 가져오는 함수</summary>
    /// <param name="data"></param>
    private void JSONToMissions(Dictionary<string, object> data)
    {
        if(data == null)
        { 
            return;
        }

        //id와 id 리스트들
        foreach (KeyValuePair<string, object> todo_mission in data)
        {
            //만약에 속성 한두개가 없다면? => oh...
            Dictionary<string, object> tmp = (Dictionary<string, object>)(todo_mission.Value);

            Mission mission = new Mission();

            mission.ID = Convert.ToInt32(todo_mission.Key);
            mission.JSONToMission(tmp);
            mission.SetIcon(GetIcon(Convert.ToInt32(tmp["icon"])));
            AddMission(mission);


            //여기서 Todo_Mission 생성하고 SetFromJSON 각각
            //EmployeeSO employeeSO = RecruitmentController.instance.GetEmployeeSO(Convert.ToInt32(tmp["employeeType"]));
            //Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO);
        }
    }

    //미션 총합 아이디 서버에 저장
    public void MissionCountToServer()
    {
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "mission_count",
            mission_count
        );
    }


    #region BINARY_SEARCH
    //binary_search
    public int Search_Mission_Index(int id)
    {
        int index = Binary_Search_Mission_Index(0, missions.Count - 1, id);
        if (missions[index].ID == id)
        {
            return index;
        }
        return -1;
    }

    private int Binary_Search_Mission_Index(int start, int end, int id)
    {
        if (start > end)
        {
            return start;
        }
        int mid = (start + end) / 2;
        if (id > missions[mid].ID)
        {
            return Binary_Search_Mission_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Mission_Index(start, mid - 1, id);
        }
    }

    #endregion
}

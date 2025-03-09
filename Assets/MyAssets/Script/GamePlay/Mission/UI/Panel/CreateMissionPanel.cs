using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CreateMissionPanel : Panel
{
    [SerializeField] private MissionPanel missionPanel;

    [SerializeField] private TMP_InputField title_InputField;

    [SerializeField] private GameObject [] smallMissions;
    [SerializeField] private MultiLayoutGroup layoutGroup;
    [SerializeField] private Toggle[] toggles;


    
    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type; //이걸로 dev, QA인지 분류할 수 있지 않을까
    private int level;
    private int smallMission_size;

    void Awake()
    {
        layoutGroup = GetComponent<MultiLayoutGroup>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Init();
    }

    void Init()
    {
        title_InputField.text = "";

        for(int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = true;
        }

        smallMission_size = 7;
        for (int i = 1; i < smallMissions.Length; i++)
        {
            DeleteSmallMission();
        }

        smallMissions[0].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
    }


    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void SetEmployeeType(int employee_type)
    {
        this.employee_type = employee_type;
    }
    
    //소미션 추가하는 버튼 함수 (최대 7번)
    public void AddSmallMission()
    {
        if (smallMission_size == Employee.MAX_SMALL_MISSION_SIZE)
            return;
        smallMissions[smallMission_size].SetActive(true);
        smallMission_size++;
        
        layoutGroup.RerollScreen();
    }
    public void DeleteSmallMission()
    {
        
        if (smallMission_size <= 1)
            return;
        smallMission_size--;
        smallMissions[smallMission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        smallMissions[smallMission_size].SetActive(false);
        
        layoutGroup.RerollScreen();
    }


    //버튼 - 미션 만드는 함수
    public void CreateMission()
    {
        if (title_InputField.text == "")
        {
            Debug.LogWarning("Title field is empty!");
            return;
        }
        
        List<string> small_missions = new List<string>();

        //smallMission_InputFields => small_missions
        for (int i = 0; i < smallMission_size; i++)
        {
            string small_mission = smallMissions[i].transform.GetChild(0)
                .GetComponent<TMP_InputField>().text;
            if (small_mission == "")
            {
                return;
            }
            small_missions.Add(small_mission);
        }

        Todo_Mission todo_mission = new Todo_Mission(
            MissionController.instance.GetAndIncrementCount(),
            employee_type,
            title_InputField.text,
            0, //iconID
            level,
            small_missions
        );
        

        MissionController.instance.Add_TodoMission(todo_mission);
        Mission_Count_ToServer(MissionController.instance.GetMissionCount());
        TodoMission_ToServer(todo_mission.GetTodoMission_ToJSON(), todo_mission.ID);

        missionPanel.CreateTodoMissionObject(todo_mission); //
        //missionPanel.OnPanel(); => 이거 자체가 서브 미션 Panel이라 전환이 안됨

        //초기화
        Init();
        GamePanelManager.instance.SwitchingPanelFromInt(1); //missionPanel로 전환
    }

    //Server
    private void TodoMission_ToServer(Dictionary<string, object> data, int id)
    {
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "todo_missions." + id.ToString(),
            data
        );
    }

    //미션 총합 아이디 서버에 저장
    private void Mission_Count_ToServer(int mission_count)
    {
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "todo_mission_count",
            mission_count
        );
    }

}

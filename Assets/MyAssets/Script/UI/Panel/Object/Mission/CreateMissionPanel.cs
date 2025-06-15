using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CreateMissionPanel : Panel
{
    [Header("Manager")]
    [SerializeField] private MissionManagerSO _missionManager;

    [Space]
    [SerializeField] private MissionPanel missionPanel;

    [SerializeField] private TMP_InputField title_InputField;

    [SerializeField] private GameObject [] todo_Mission_Objects;
    [SerializeField] private MultiLayoutGroup multiLayoutGroup; //제일 아래 Layout => inputList
    [SerializeField] private Toggle[] toggles;

    [Space]
    [Header("Listening to Events")]
    [SerializeField] private IntEventChannelSO _AddRefEmployeeID;  
    [SerializeField] private IntEventChannelSO _RemoveRefEmployeeID;  
    
    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type; //이걸로 dev, QA인지 분류할 수 있지 않을까
    private int level;
    private int todo_mission_size;
    private bool isFirst = true;

    private static int TODO_MISSION_HEIGHT = 30;

    private List<int> refEmployees;


    protected void OnEnable()
    {
        if(!isFirst)
            Init();
        
        _AddRefEmployeeID._onEventRaised += AddRefEmployeeID;
        _RemoveRefEmployeeID._onEventRaised += RemoveRefEmployeeID;
    }

    protected void OnDisable()
    {
        _AddRefEmployeeID._onEventRaised -= AddRefEmployeeID;
        _RemoveRefEmployeeID._onEventRaised -= RemoveRefEmployeeID;
    }

    protected override void Start()
    {
        base.Start();
        Init();
        isFirst = false;
    }

    void Init()
    {
        multiLayoutGroup.Init();
        refEmployees = new List<int>();

        title_InputField.text = "";

        for(int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = true;
        }

        todo_mission_size = 7;
        //Debug.Log(SMALL_MISSION_HEIGHT * smallMission_size - layoutGroup.GetHeight());
        multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT * todo_mission_size - multiLayoutGroup.GetHeight());
        for (int i = 1; i < todo_Mission_Objects.Length; i++)
        {
            DeleteTodoMission();
        }

        todo_Mission_Objects[0].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        
        //layoutGroup.RerollScreen();
        //layoutGroup.SwitchingScreen(true);
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
        if (todo_mission_size == Employee.MAX_TODO_MISSION_SIZE)
            return;
        todo_Mission_Objects[todo_mission_size].SetActive(true);
        todo_mission_size++;
        
        multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT);
        //layoutGroup.RerollScreen();
        //layoutGroup.SwitchingScreen(true);
    }
    public void DeleteTodoMission()
    {
        if (todo_mission_size <= 1)
            return;
        todo_mission_size--;
        todo_Mission_Objects[todo_mission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        todo_Mission_Objects[todo_mission_size].SetActive(false);
        
        multiLayoutGroup.AddHeight(-TODO_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
        //layoutGroup.SwitchingScreen(true);
    }


    //버튼 - 미션 만드는 함수
    public void CreateMission()
    {
        if (title_InputField.text == "")
        {
            Debug.LogWarning("Title field is empty!");
            return;
        }
        
        List<Todo_Mission> todo_Missions = new List<Todo_Mission>();

        //smallMission_InputFields => small_missions
        for (int i = 0; i < todo_mission_size; i++)
        {
            string text = todo_Mission_Objects[i].transform.GetChild(0)
                .GetComponent<TMP_InputField>().text;
            if (text == "")
            {
                return;
            }
            todo_Missions.Add(new Todo_Mission(text));
        }
        
        //대충 

        Mission mission = new Mission(
            _missionManager.GetAndIncrementCount(),
            employee_type,
            title_InputField.text,
            _missionManager.GetIcon(0),
            0, //iconID
            level,
            todo_Missions,
            refEmployees
        );

        _missionManager.AddMission(mission);
        _missionManager.SetServerMissionCount();

        missionPanel.CreateMissionElementObject(mission); //
        //missionPanel.OnPanel(); => 이거 자체가 서브 미션 Panel이라 전환이 안됨

        //초기화
        Init();
        GamePanelManager.instance.SwitchingPanelFromInt(1); //missionPanel로 전환
    }

    public void SwitchingAssignEmployeePanel()
    {
        AssignMissionPanel p = panels[0].GetComponent<AssignMissionPanel>();

        panels[0].OnPanel();
        p.Init();
    }

    private void AddRefEmployeeID(int id)
    {
        refEmployees.Add(id);
    }

    private void RemoveRefEmployeeID(int id)
    {
        for (int i = 0; i < refEmployees.Count; i++)
        {
            if(id == refEmployees[i])
                refEmployees.RemoveAt(i);   
        }
    }

}

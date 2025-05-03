using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CreateMissionPanel : Panel
{
    [FormerlySerializedAs("missionController")]
    [FormerlySerializedAs("missionsSO")]
    [Header("SO")]
    [SerializeField] private MissionControllerSO missionControllerSo;

    [SerializeField] private MissionPanel missionPanel;

    [SerializeField] private TMP_InputField title_InputField;

    [SerializeField] private GameObject [] todo_Mission_Objects;
    [SerializeField] private MultiLayoutGroup multiLayoutGroup; //제일 아래 Layout => inputList
    [SerializeField] private Toggle[] toggles;

    
    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type; //이걸로 dev, QA인지 분류할 수 있지 않을까
    private int level;
    private int todo_mission_size;
    private bool isFirst = true;

    private static int TODO_MISSION_HEIGHT = 30;


    protected void OnEnable()
    {
        if(!isFirst)
            Init();
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
            DeleteSmallMission();
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
    public void DeleteSmallMission()
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

        Mission todo_mission = new Mission(
            missionControllerSo.GetAndIncrementCount(),
            employee_type,
            title_InputField.text,
            missionControllerSo.GetIcon(0),
            0, //iconID
            level,
            todo_Missions
        );


        missionControllerSo.AddMission(todo_mission);
        missionControllerSo.MissionCountToServer();
        
        TodoMission_ToServer(todo_mission.MissionToJSON(), todo_mission.ID);

        missionPanel.CreateMissionElementObject(todo_mission); //
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
            "missions." + id.ToString(),
            data
        );
    }


}

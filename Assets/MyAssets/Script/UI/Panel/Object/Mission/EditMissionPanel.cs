using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EditMissionPanel : Panel
{
    [Header("Controller")]
    [SerializeField] private MissionManagerSO _missionControllerSO;

    
    [SerializeField] private TMP_InputField title;

    
    //라디오버튼 그룹 두개
    //ㄴ 나중에 SetMission때 활용

    [SerializeField] private CustomRadioButtonGroup employeeTypeGroup;
    [SerializeField] private CustomRadioButtonGroup levelGroup;
    [SerializeField] private GameObject[] todo_mission_textObject; //edittext todoMission 오브젝트들

    [SerializeField] private MultiLayoutGroup multiLayoutGroup;
  


    //소미션
    private int todo_mission_size;
    private List<bool> todo_missions_isdone;

    private int mission_id;
    private static int TODO_MISSION_HEIGHT = 30;
    
    private List<int> refEmployees;


    //나중에 직원도 넣을 예정

    /// <summary> init </summary>
    /// <param name="mission"></param>
    public void SetMission(Mission mission)
    {
        multiLayoutGroup.Init();
        mission_id = mission.ID;
        todo_missions_isdone = new List<bool>();

        //값 넣기
        title.text = mission.GetName();
        employeeTypeGroup.SetIndex((int)mission.GetMissionType());
        levelGroup.SetIndex(mission.GetLevel());


        int _todo_mission_size = 7;
        todo_mission_size = _todo_mission_size;

        //height를 210으로 만들어야함
        multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT * todo_mission_size - multiLayoutGroup.GetHeight());

        for (int i = 1; i < _todo_mission_size; i++)
        {
            DeleteTodoMission();
        }

        List<Todo_Mission> _todo_missions = mission.GetTodoMissions();
        _todo_mission_size = _todo_missions.Count;

        //텍스트 넣기
        todo_mission_textObject[0].transform.GetChild(0).GetComponent<TMP_InputField>().text =
            _todo_missions[0].Title;
        todo_missions_isdone.Add(_todo_missions[0].IsDone);

        todo_mission_size = 1;

        for (int i = 1; i < _todo_mission_size; i++)
        {
            todo_missions_isdone.Add(_todo_missions[i].IsDone);

            //오브젝트 생성
            AddTodoMission();
            todo_mission_textObject[i].transform.GetChild(0).GetComponent<TMP_InputField>().text =
                _todo_missions[i].Title;
        }
        
        //refEmployees
        refEmployees = mission.RefEmployees;
    }

    List<Todo_Mission> Get_Todo_Missions()
    {
        List<Todo_Mission> _todoMissions = new List<Todo_Mission>();
        for (int i = 0; i < todo_mission_size; i++)
        {
            bool isDone = false;
            if(i < todo_missions_isdone.Count)
            {
                isDone = todo_missions_isdone[i];
            }
            //여기 isDone 매개변수 자체가 false임
            _todoMissions.Add(
                new Todo_Mission(
                    todo_mission_textObject[i].transform.GetChild(0).GetComponent<TMP_InputField>().text
                    , isDone
                )
            );
        }

        return _todoMissions;
    }

    public void DeleteTodoMission()
    {
        if (todo_mission_size <= 1)
            return;
        todo_mission_size--;
        todo_mission_textObject[todo_mission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        todo_mission_textObject[todo_mission_size].SetActive(false);

        multiLayoutGroup.AddHeight(-TODO_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }

    //
    public void AddTodoMission()
    {
        if (todo_mission_size == Employee.MAX_TODO_MISSION_SIZE)
            return;
        todo_mission_textObject[todo_mission_size].SetActive(true);
        todo_mission_size++;

        multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }

    //최종적으로 값을 서버에 수정
    public void EditMission()
    {
        Mission mission = new Mission(
            id: mission_id,
            employeeTypeGroup.GetIndex(),
            title.text,
            _missionControllerSO.GetIcon(0),
            0, //iconID
            levelGroup.GetIndex(),
            Get_Todo_Missions(),
            refEmployees
        );

        _missionControllerSO.SetServerMission(mission);
        //값을 미션 적용, 이후 모든 오브젝트 초기화
        

        int index = _missionControllerSO.Search_Mission_Index(mission_id);
        _missionControllerSO.SetMission(mission, index);

        //굳이 리롤이 필요할까

        //back 네비 Panel
        PanelManager.instance.Back_Nav_Panel();
    }

    //최종적으로 서버에 있는 미션 삭제
    public void RemoveMission()
    {
        _missionControllerSO.RemoveMission(mission_id);

        PanelManager.instance.Back_Nav_Panel();
    }
}
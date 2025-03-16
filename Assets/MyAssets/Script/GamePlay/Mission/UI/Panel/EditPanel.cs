using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EditPanel : Panel
{
    [SerializeField] private TMP_InputField title;

    //라디오버튼 그룹 두개
    //ㄴ 나중에 SetMission때 활용

    [SerializeField] private CustomRadioButtonGroup employeeTypeGroup;
    [SerializeField] private CustomRadioButtonGroup levelGroup;
    [SerializeField] private GameObject[] smallMissions;

    [SerializeField] private MultiLayoutGroup multiLayoutGroup;

    //소미션
    private int smallMission_size;

    private int mission_id;
    private static int SMALL_MISSION_HEIGHT = 30;

    //나중에 직원도 넣을 예정

    /// <summary> init </summary>
    /// <param name="todoMission"></param>
    public void SetMission(Mission todoMission)
    {
        multiLayoutGroup.Init();
        mission_id = todoMission.ID;
        
        //값 넣기
        title.text = todoMission.GetName();
        employeeTypeGroup.SetIndex((int)todoMission.GetMissionType());
        levelGroup.SetIndex(todoMission.GetLevel());


        int _smallMission_size = 7;
        smallMission_size = _smallMission_size;

        //height를 210으로 만들어야함
        multiLayoutGroup.AddHeight(SMALL_MISSION_HEIGHT * smallMission_size - multiLayoutGroup.GetHeight());

        for (int i = 1; i < _smallMission_size; i++)
        {
            DeleteSmallMission();
        }
        
        List<string> _smallMissions = todoMission.GetSmallMissions();
        _smallMission_size = _smallMissions.Count;
        
        smallMissions[0].transform.GetChild(0).GetComponent<TMP_InputField>().text = _smallMissions[0];
        smallMission_size = 1;
        
        for (int i = 1; i < _smallMission_size; i++)
        {
            //오브젝트 생성
            AddSmallMission();
            smallMissions[i].transform.GetChild(0).GetComponent<TMP_InputField>().text = _smallMissions[i];
        }
    }

    List<string> GetSmallMissions_text()
    {
        List<string> _smallMissions = new List<string>();
        for (int i = 0; i < smallMission_size; i++)
        {
            _smallMissions.Add(smallMissions[i].transform.GetChild(0).GetComponent<TMP_InputField>().text);
        }
        return _smallMissions;
    }

    public void DeleteSmallMission()
    {
        if (smallMission_size <= 1)
            return;
        smallMission_size--;
        smallMissions[smallMission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        smallMissions[smallMission_size].SetActive(false);

        multiLayoutGroup.AddHeight(-SMALL_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }

    //
    public void AddSmallMission()
    {
        if (smallMission_size == Employee.MAX_SMALL_MISSION_SIZE)
            return;
        smallMissions[smallMission_size].SetActive(true);
        smallMission_size++;

        multiLayoutGroup.AddHeight(SMALL_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }

    //최종적으로 값을 서버에 수정
    public void EditMission()
    {
        Mission todo_mission = new Mission(
            id: mission_id,
            employeeTypeGroup.GetIndex(),
            title.text,
            0, //iconID
            levelGroup.GetIndex(),
            GetSmallMissions_text()
        );
        
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "todo_missions." + mission_id.ToString(),
            todo_mission.GetTodoMission_ToJSON()
        );
        
        //back 네비 Panel
    }

    //최종적으로 서버에 있는 미션 삭제
    public void DeleteMission()
    {
        MissionController.instance.Remove_TodoMission(mission_id);
        FireStoreManager.instance.DeleteFirestoreDataKey(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "todo_missions." + mission_id
        );
        
        //back 네비 Panel
    }
}
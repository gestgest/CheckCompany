using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CreateMissionPanel : Panel
{
    [SerializeField] private TMP_InputField title_InputField;
    [SerializeField] private RadioButtonGroup radioGroup;

    [SerializeField] private TMP_InputField smallMission_InputFields;


    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    int mission_type;

    public void SetLevel()
    {
        //switch
    }

    //소미션 추가하는 함수 (최대 7번)
    public void AddSmallMission()
    {

    }    


    //버튼 - 미션 만드는 함수
    public void CreateMission()
    {
        List<string> small_missions = new List<string>();

        //smallMission_InputFields => small_missions

        Todo_Mission todo_mission = new Todo_Mission(
            MissionController.instance.GetAndIncrementID(),
            mission_type,
            title_InputField.text,
            0, //iconID
            small_missions
        );
        //MissionController.instance.AddMission();
    }
}

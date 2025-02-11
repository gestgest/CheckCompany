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
    private int employee_type; //이걸로 dev, QA인지 분류할 수 있지 않을까
    private int level;

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
        
    }


    //버튼 - 미션 만드는 함수
    public void CreateMission()
    {
        List<string> small_missions = new List<string>();
        
        //smallMission_InputFields => small_missions

        Todo_Mission todo_mission = new Todo_Mission(
            MissionController.instance.GetAndIncrementID(),
            employee_type,
            title_InputField.text,
            0, //iconID
            level,
            small_missions
        );
        //MissionController.instance.AddMission();
    }
}

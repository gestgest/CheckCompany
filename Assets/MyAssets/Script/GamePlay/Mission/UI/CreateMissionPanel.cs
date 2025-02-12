using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CreateMissionPanel : Panel
{
    [SerializeField] private TMP_InputField title_InputField;
    [SerializeField] private RadioButtonGroup radioGroup;

    [SerializeField] private GameObject [] smallMissions;


    //이미지? => 정말 나중에 만들 예정 => 지금은 그냥 0으로 default
    private int employee_type; //이걸로 dev, QA인지 분류할 수 있지 않을까
    private int level;
    private int smallMission_size;

    private void OnEnable()
    {
        smallMission_size = 7;
        for (int i = 1; i < smallMissions.Length; i++)
        {
            DeleteSmallMission();
        }
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
        //smallMission_InputFields_parent
    }
    public void DeleteSmallMission()
    {
        if (smallMission_size <= 1)
            return;
        smallMission_size--;
        smallMissions[smallMission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        smallMissions[smallMission_size].SetActive(false);
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
        Debug.Log(smallMission_size);

        //smallMission_InputFields => small_missions
        for (int i = 0; i < smallMission_size; i++)
        {
            string small_mission = smallMissions[i].transform.GetChild(0)
                .GetComponent<TMP_InputField>().text;
            if (small_mission == "")
            {
                Debug.Log("엄준식은 죽었다");
                return;
            }
            small_missions.Add(small_mission);
        }

        Todo_Mission todo_mission = new Todo_Mission(
            MissionController.instance.GetAndIncrementID(),
            employee_type,
            title_InputField.text,
            0, //iconID
            level,
            small_missions
        );
        
        Debug.Log("엄준식은 살아있다");
        MissionController.instance.Add_TodoMission(todo_mission);
    }
}

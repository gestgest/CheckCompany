using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MissionElement : MonoBehaviour
{
    private Mission mission;

    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Image icon;

    [SerializeField] private GameObject down_content; //
    //List TodoMission 리스트

    [SerializeField] private GameObject todoMissionPrefab;
    [SerializeField] private GameObject Dropbox;
    [SerializeField] private Transform parentContent;
    [SerializeField] private Gauge gauge;

    private List<GameObject> todo_mission_objects = new List<GameObject>();

    private MultiLayoutGroup multiLayoutGroup;
    private bool isShowContent = false;

    private static int WIDTH = 800;
    private static int TODO_MISSION_HEIGHT = 100;

    public void LayoutInit()
    {
        multiLayoutGroup = GetComponent<MultiLayoutGroup>();
        multiLayoutGroup.Init();
    }
    
    //<summary> 미션 지정 : init, 미션 edit</summary>
    public void SetMission(Mission mission, bool isInit = true)
    {
        LayoutInit();
        
        this.mission = mission;

        title.text = mission.GetName();
        icon.sprite = mission.GetIcon();
        
        //todo리스트 숨기기
        isShowContent = false;
        multiLayoutGroup.SwitchingScreen(isShowContent, false);
        down_content.SetActive(isShowContent);
        Dropbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        
        multiLayoutGroup.AddOnHeight(-multiLayoutGroup.GetOnHeight()); //오브젝트 초기화
        multiLayoutGroup.AddOnHeight(TODO_MISSION_HEIGHT); //게이지 크기 추가

        int i;
        
        //초기가 아닌 경우 미션 오브젝트들 초기화
        for (i = 0; i < todo_mission_objects.Count; i++)
        {
            Destroy(todo_mission_objects[i]);
        }
        todo_mission_objects.Clear();

        i = 0;
        foreach (Todo_Mission todoMission in mission.GetTodoMissions())
        {
            CreateTodoMissionObject(todoMission, i);
            i++;
        }

        gauge.Init(0, mission.GetTodoMissions().Count, WIDTH);

        //

        //SwitchingDownContent(); //down content 비활성화
        //버튼 이벤트는 따로 놨음
    }


    public void SetEventListener(UnityAction listener)
    {
        Button button = this.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(listener);
    }

    private void CreateTodoMissionObject(Todo_Mission todo_mission, int todo_mission_id)
    {
        GameObject todoMissionObject = Instantiate(todoMissionPrefab, parentContent);
        TodoMissionElement todoMissionElement = todoMissionObject.GetComponent<TodoMissionElement>();


        todo_mission_objects.Add(todoMissionObject);
        todoMissionElement.SetTodoMission(todo_mission, mission.ID, todo_mission_id);
        todoMissionElement.SetGague(gauge);
        multiLayoutGroup.AddOnHeight(TODO_MISSION_HEIGHT); //오브젝트 크기 부여
    }

    public void SwitchingDownContent()
    {
        isShowContent = !isShowContent;
        down_content.SetActive(isShowContent);

        //드롭박스 스위칭
        if (isShowContent)
        {
            Dropbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
        }
        else
        {
            Dropbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        }

        multiLayoutGroup.SwitchingScreen(isShowContent);
    }

    public Mission GetMission()
    {
        return mission;
    }

}

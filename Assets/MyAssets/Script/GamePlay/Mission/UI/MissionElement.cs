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
    //List SmallMission 리스트

    [FormerlySerializedAs("smallMissionPrefab")] [SerializeField] private GameObject todoMissionPrefab;
    [SerializeField] private GameObject Dropbox;
    [SerializeField] private Transform parentContent;
    [SerializeField] private Gauge gauge;

    private List<GameObject> smallMissionObjects = new List<GameObject>();
    
    private MultiLayoutGroup multiLayoutGroup;
    private bool isShowContent = false;

    private static int WIDTH = 800;
    private static int TODO_MISSION_HEIGHT = 100;
    
    //<summary> 미션 지정 : init</summary>
    public void SetMission(Mission mission)
    {
        multiLayoutGroup = GetComponent<MultiLayoutGroup>();
        multiLayoutGroup.Init();
        this.mission = mission;

        title.text = mission.GetName();
        icon.sprite = mission.GetIcon();

        multiLayoutGroup.AddOnHeight(TODO_MISSION_HEIGHT); //게이지 크기 추가
        foreach (string smallMission in mission.GetTodoMissions())
        {
            if (smallMission == null)
            {
                Debug.Log("엄");
            }
            CreateSmallMissionObject(smallMission);
        }
        
        gauge.Init(0, mission.GetTodoMissions().Count, WIDTH);

        isShowContent = false;
        down_content.SetActive(isShowContent);
        Dropbox.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        
        //SwitchingDownContent(); //down content 비활성화
        //버튼 이벤트는 따로 놨음
    }

    public void AddEventListener(UnityAction listener)
    {
        Button button = this.GetComponentInChildren<Button>();
        button.onClick.AddListener(listener);
    }

    private void CreateSmallMissionObject(string smallMission, bool isDone = false)
    {
        GameObject todoMissionObject = Instantiate(todoMissionPrefab, parentContent);
        TodoMissionElement todoMissionElement = todoMissionObject.GetComponent<TodoMissionElement>();
        
        
        smallMissionObjects.Add(todoMissionObject);
        todoMissionElement.SetTodoMission(smallMission, isDone);
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
}

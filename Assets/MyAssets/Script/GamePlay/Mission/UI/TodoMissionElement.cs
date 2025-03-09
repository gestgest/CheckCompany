using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TodoMissionElement : MonoBehaviour
{
    private Todo_Mission todoMission;

    [SerializeField] private TextMeshProUGUI title; 
    [SerializeField] private Image icon;

    [SerializeField] private GameObject down_content; //
    //List SmallMission 리스트

    [SerializeField] private GameObject smallMissionPrefab;
    [SerializeField] private GameObject Dropbox;
    [SerializeField] private Transform parentContent;
    [SerializeField] private Gauge gauge;

    private List<GameObject> smallMissionObjects = new List<GameObject>();
    
    [SerializeField] private MultiLayoutGroup multiLayoutGroup;
    private bool isShowContent = false;
    private static int WIDTH = 800;
    private static int SMALLMISSION_HEIGHT = 100;
    
    //<summary> 미션 지정 : init</summary>
    public void SetMission(Todo_Mission todoMission)
    {
        multiLayoutGroup = GetComponent<MultiLayoutGroup>();
        this.todoMission = todoMission;

        title.text = todoMission.GetName();
        icon.sprite = todoMission.GetIcon();
        
        foreach(string smallMission in todoMission.GetSmallMissions())
        {
            CreateSmallMissionObject(smallMission);
        }
        
        gauge.Init(0, todoMission.GetSmallMissions().Count, WIDTH);
        isShowContent = true;
        SwitchingDownContent(); //down content 비활성화
        //버튼 이벤트는 따로 놨음
    }

    public void AddEventListener(UnityAction listener)
    {
        Button button = this.GetComponentInChildren<Button>();
        button.onClick.AddListener(listener);
    }

    private void CreateSmallMissionObject(string smallMission)
    {
        GameObject smallMissionObject = Instantiate(smallMissionPrefab, parentContent);
        SmallMissionElement smallMissionElement = smallMissionObject.GetComponent<SmallMissionElement>();
        
        smallMissionObjects.Add(smallMissionObject);
        smallMissionElement.SetGague(gauge);
        multiLayoutGroup.AddOnHeight(SMALLMISSION_HEIGHT); //
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
        
        multiLayoutGroup.SwitchingScreen();
    }
}

using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

//미션 아이콘 스크립트
public class MissionElementUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int id;
    [SerializeField] EmployeeStatusWindow employeeStatusWindow;
    [SerializeField] GameObject addMissionMiniWindow; //미션 추가하는 윈도우
    [SerializeField] GameObject missionDescriptionPanel;

    [SerializeField] private UnityEngine.UI.Image icon;


    private MissionSO mission;

    RectTransform rf;

    bool isMissionON = false; //비어있는지 켜져있는지
    float dis; // 중심 아처와 descriptPanel의 거리
    
    void Start()
    {
        rf = GetComponent<RectTransform>();
        dis = (rf.rect.width + employeeStatusWindow.GetDescriptionPanelHeight()) / 2 + 30; //50은 오브젝트 크기
    }

    //UI 오브젝트 위에 마우스 커서를 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!isMissionON)
            return;

        missionDescriptionPanel.SetActive(true);

        TextMeshProUGUI description = missionDescriptionPanel.GetComponentInChildren<TextMeshProUGUI>();
        description.text = mission.GetName();
        missionDescriptionPanel.transform.position = gameObject.transform.position + new Vector3(dis, 0, 0);
    }

    //UI 오브젝트 위에 마우스 커서가 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        missionDescriptionPanel.SetActive(false);
    }


    //마우스 클릭했을때
    //ㄴ 1) 이미 미션이 있는 경우
    public void ClickMission()
    {
        if(isMissionON){
            //옮기는 함수
            employeeStatusWindow.RemoveMission(id);
        }
        else{
            //addMissionMiniWindow 위치조정 안해도 될거 같기도...?
            addMissionMiniWindow.SetActive(true);
        }
    }

    //ㄴ 2) 미션이 없는 경우 => 미션 생성해주는 미니창

    //아이콘 설정
    public void SetValue(MissionSO m)
    {
        mission = m;
        icon.sprite = m.GetIcon();
        SetIsMissionOn(true);
    }

    //아이콘 설정
    public void SetValue()
    {
        SetIsMissionOn(false);
    }

    private void SetIsMissionOn(bool isMissionOn)
    {
        this.isMissionON = isMissionOn;
        icon.gameObject.SetActive(isMissionON);
    }
    
}
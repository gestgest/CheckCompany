using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EmployeeStatusWindow : Window
{
    //Description Panel
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI careerText;
    [SerializeField] private TextMeshProUGUI timeText;

    //MissionPanel
    [SerializeField] private GameObject missionObjectParent; //5개
    private MissionElementUI[] missionUIs; //5개
    //나중에 MissionElementUI에서 클릭 하면 바로바로 여기서 미션을 가져와야 함
    // ㄴ 원래 이거였지만 어쩌다 보니 바뀜
    //션을 받을 5개의 미션
    
    [SerializeField] private GameObject descriptionPanel;
    private IEmployee employee;
    RectTransform rf_dPanel;

    //const IEmployee.MAX_MISSION_SIZE = 5;
    void Awake()
    {
        rf_dPanel = descriptionPanel.GetComponent<RectTransform>();
        missionUIs = new MissionElementUI[IEmployee.MAX_MISSION_SIZE];
    }
    
    protected override void Start()
    {
        base.Start();
        for(int i = 0; i < missionObjectParent.transform.childCount; i++)
        {
            Transform mObj = missionObjectParent.transform.GetChild(i);
            missionUIs[i] = mObj.GetComponent<MissionElementUI>();
        }
    }
    //EmployeeStatusWindow => 값 받기, panels 관련 애니메이션, panel이동 관련 함수
    //값 받기
    public void SetValue(IEmployee employee)
    {
        this.employee = employee;
        nameText.text = employee.Name + " 사원"; //일단 사원으로 함
        //이미지는 패스
        ageText.text = "나이 : " + employee.Age.ToString() + "살";
        costText.text = "연봉 : " + (employee.Cost * 12).ToString() + "원";
        careerText.text = "경력 기간 : " + employee.Career.ToString() + "개월";
        timeText.text = "근무시간 : " + employee._WorkTime.start.ToString() + " ~ " + employee._WorkTime.end.ToString();

        SetMission();
    }

    public float GetDescriptionPanelHeight()
    {
        return rf_dPanel.rect.width;
    }


    //////////////////////////////////////Mission////////////////////////////////////////
    public void RemoveMission(int index) 
    {
        employee.RemoveMission(index);
        SetMission();
    }

    private void SetMission()
    {
        for (int i = 0; i < employee.GetMissionSize(); i++)
        {
            MissionSO mission = employee.GetMission(i);
            
            if(mission.GetMissionType() != MissionType.NONE)
                missionUIs[i].SetValue(mission);
        }

        for (int i = employee.GetMissionSize(); i < IEmployee.MAX_MISSION_SIZE; i++)
        {
            missionUIs[i].SetValue();

        }
        
    }
}

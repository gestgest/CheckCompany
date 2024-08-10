using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    //나중에 MissionElementUI에서 클릭 하면 바로바로 여기서 미션을 가져와야 함
    [SerializeField] private Mission[] missions;

    [SerializeField] private GameObject descriptionPanel;
    RectTransform rf_dPanel;

    void Awake()
    {
        rf_dPanel = descriptionPanel.GetComponent<RectTransform>();
        missions = new Mission[5];
    }

    //EmployeeStatusWindow => 값 받기, panels 관련 애니메이션, panel이동 관련 함수
    //값 받기
    public void SetValue(IEmployee employee)
    {
        nameText.text = employee.Name + " 사원"; //일단 사원으로 함
        //이미지는 패스
        ageText.text = "나이 : " + employee.Age.ToString() + "살";
        costText.text = "연봉 : " + (employee.Cost * 12).ToString() + "원";
        careerText.text = "경력 기간 : " + employee.Career.ToString() + "개월";
        timeText.text = "근무시간 : " + employee._WorkTime.start.ToString() + " ~ " + employee._WorkTime.end.ToString();

        //미션도 해야함
        for (int i = 0; i < employee.GetMissionSize(); i++)
        {
            missions[i] = employee.GetMission(i);
        }
    }

    public float GetDescriptionPanelHeight()
    {
        //Debug.Log(rf_dPanel.rect.width);
        return rf_dPanel.rect.width;
    }

}

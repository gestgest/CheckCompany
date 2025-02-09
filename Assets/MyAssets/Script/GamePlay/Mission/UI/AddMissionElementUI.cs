using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//클릭하는 각각의 미션 아이콘이다
//누르면 미션 추가
public class AddMissionElementUI : MonoBehaviour
{
    private EmployeeStatusWindow employeeStatusWindow;

    [SerializeField] private MissionSO mission;
    [SerializeField] private Image icon;

    //OnEnable하면 필터링 함수

    //버튼 누르면 추가되는 방식
    public void AddMission()
    {
        employeeStatusWindow.AddMission(mission);
    }
    public void SetMission(MissionSO mission)
    {
        this.mission = mission;
        icon.sprite = mission.GetIcon();
    }
    public void SetEmployeeStatusWindow(EmployeeStatusWindow employeeStatusWindow)
    {
        this.employeeStatusWindow = employeeStatusWindow;
    }
}

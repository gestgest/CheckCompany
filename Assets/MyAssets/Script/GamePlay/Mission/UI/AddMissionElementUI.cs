using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//클릭하는 각각의 미션 아이콘이다
//누르면 미션 추가
public class AddMissionElementUI : MonoBehaviour
{
    private EmployeeStatusWindow employeeStatusWindow;

    [SerializeField] private Todo_Mission todo_mission;
    [SerializeField] private Image icon;

    //OnEnable하면 필터링 함수

    //버튼 누르면 추가되는 방식
    public void AddMission()
    {
        employeeStatusWindow.AddMission(todo_mission);
    }
    public void SetMission(Todo_Mission todo_mission)
    {
        this.todo_mission = todo_mission;
        icon.sprite = todo_mission.GetIcon();
    }
    public void SetEmployeeStatusWindow(EmployeeStatusWindow employeeStatusWindow)
    {
        this.employeeStatusWindow = employeeStatusWindow;
    }
}

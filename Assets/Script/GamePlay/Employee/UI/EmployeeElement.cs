using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//UI Employee를 보여주는 요소
public class EmployeeElement : MonoBehaviour
{
    //모델
    [SerializeField] private Image icon; //유형
    [SerializeField] private TextMeshProUGUI employee_name; //
    [SerializeField] private TextMeshProUGUI career; //
    [SerializeField] private TextMeshProUGUI weight_speed; //업무속도 => 가중치
    [SerializeField] private TextMeshProUGUI costUI;

    public int ID { get; set; } //직원 아이디

    public void SetEmployee(Sprite sprite, string name, int career, int weight_speed, int cost, int id)
    {
        SetIcon(sprite);
        SetEmployeeName(name);
        SetCareer(career);
        SetWeightSpeed(weight_speed);
        SetCost(cost);
        ID = id;
    }
    private void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
    private void SetEmployeeName(string name)
    {
        employee_name.text = name;
    }
    private void SetCareer(int career)
    {
        this.career.text = career.ToString();
        this.career.text += "개월";
    }
    private void SetWeightSpeed(int weight_speed)
    {
        this.weight_speed.text = weight_speed.ToString();
    }
    private void SetCost(int cost)
    {
        costUI.text = (cost / 10000).ToString() + "만원";
    }


    //싱글톤으로 Employee를 제거하는 함수
    public void RemoveEmployee()
    {
        EmployeeController.instance.RemoveEmployee(ID);
    }
}
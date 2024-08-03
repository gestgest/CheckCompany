using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EmployeeStatusWindow : Window
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI careerText;
    [SerializeField] private TextMeshProUGUI timeText;

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
    }
}

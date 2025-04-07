using UnityEngine;
using TMPro;
using System;
using System.Reflection;

public class CalendarPanel : Panel
{
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private GameObject[] weekObjects;  //6주

    Date currentDate;
    //내가 만든 자료형 Date로 굳이 변경할 이유가 있을까?
    protected override void Start()
    {
        base.Start();
        // 오늘 날짜 가져오기
        System.DateTime today = System.DateTime.Now;

        currentDate = new Date();
        currentDate.SetDate(today);

        UpdateCalendarStatus();
    }

    //상태 업데이트
    void UpdateCalendarStatus()
    {
        //date로 상태 업데이트

        //달력 상단바 -> 2025.04
        dateText.text = currentDate.Year + "." + currentDate.Month.ToString("00");

        //대충 4월의 1일의 요일을 기준으로 달력 세팅 => [일, 요일]
        SetFirstSunday();
    }

    void SetFirstSunday()
    {
        Date tmp = new Date();
        tmp.SetDate(System.DateTime.Now);

        int dis_day = tmp.Day - 1;

        tmp.Day -= dis_day; //1일
        tmp.Day -= ((int)tmp._Week + 1) % 7; //일요일로 맞추기 위해서 -1을 해줘야함
        //만약 6이면 끝, 0이면 -1


        Debug.Log(tmp.Day);
        SetCalendar(tmp);
    }

    /// <summary>
    /// 처음 일요일 스타팅 날짜 기준
    /// </summary>
    /// <param name="month"></param>
    /// <param name="day"></param>
    void SetCalendar(Date date)
    {
        int count = 0; //요
        //끝까지
        for (int i = date.Day; i <= Date.MONTH_DAY[date.Month - 1] + Date.addDay_LeapYear(date.Year, date.Month); i++)
        {
            if (count % 7 == 0)
            {
                weekObjects[count / 7].SetActive(true);
            }
            SetCalendarContent(count / 7, count % 7, i);

            //세팅
            count++;
        }
    }
    void SetCalendarContent(int y,int x, int day)
    {
        weekObjects[y].transform.GetChild(x).GetComponent<CalendarElement>().SetDay(day);
    }
}

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
    
    //UI
    #region Button

    public void SetDate(int year, int month)
    {
        currentDate.AddYear(year - currentDate.Year);
        currentDate.AddMonth(month - currentDate.Month);
        
        UpdateCalendarStatus();
    }
    
    /// <summary>달력 상단바의 왼쪽 오른쪽 버튼. </summary>
    /// <param name="month">더하는 월 값</param>
    public void AddMonth(int month)
    {
        currentDate.AddMonth(month);
        UpdateCalendarStatus();
    }

    //상단바 누르면
    public void SwitchingMiniPanel()
    {
        SwitchingPanel(0);
        panels[0].GetComponent<CalendarMiniPanel>()
            .SetValue(currentDate.Year, currentDate.Month);
    }
    
    #endregion
    

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
        tmp.SetDate(currentDate);

        int dis_day = tmp.Day - 1;

        tmp.Day -= dis_day; //1일
        //Debug.Log("기기긱" + tmp._Week + ", " + tmp.Day);

        //Debug.Log("기기긱" + tmp._Week + ", " + tmp.Day);
        tmp.Day -= ((int)tmp._Week + 1) % 7;
        //화요일 1 => 31일로 가야하니까 -2를 해야함
        //ㄴ 인줄 알았지만 1일에서 추가로 1을 더 빼야함
        //만약 토요일이라면 0인데? 만약 일요일이라면 -1? 인데?

        SetCalendar(tmp);
    }

    /// <summary>
    /// 처음 일요일 스타팅 날짜 기준
    /// </summary>
    /// <param name="month"></param>
    /// <param name="day"></param>
    void SetCalendar(Date date)
    {
        //초기화
        for (int i = 0; i < weekObjects.Length; i++)
        {
            weekObjects[i].SetActive(false);
        }
        
        int count = 0; //주

        
        //1이 아닌 경우 
        if (date.Day != 1)
        {
            count = ForCalendarElement(date, count);
        }

        count = ForCalendarElement(date, count);
        
        //이후 토요일이 아니라면 토요일까지 채우기
        for (; Week.SUN != date._Week ; date.Day++)
        {
            if (count % 7 == 0)
            {
                weekObjects[count / 7].SetActive(true);
            }
            SetCalendarContent(count / 7, date);

            //세팅
            count++;
        }
    }
    
    int ForCalendarElement(Date date, int count)
    {
        int max = Date.MONTH_DAY[date.Month - 1] + Date.addDay_LeapYear(date.Year, date.Month);
        //중심 월
        for (int i = date.Day; i <= max; i++)
        {
            //Debug.Log(date._Week);

            if (count % 7 == 0)
            {
                weekObjects[count / 7].SetActive(true);
            }
            SetCalendarContent(count / 7, date);

            //세팅
            count++;
            date.Day++;
        }

        return count;
    }
    void SetCalendarContent(int y, Date date)
    {
        int x = ((int)date._Week + 1) % 7;
        //Debug.Log("y : " + y + ", x : "+  date.Day);
        weekObjects[y].transform.GetChild(x).GetComponent<CalendarElement>().SetDay(date.Year, date.Month, date.Day);
        
        System.DateTime today = System.DateTime.Now;
        
        //Debug.Log(date.Day);
        //오늘이라면
        if (
            date.Year == today.Year 
            &&date.Month == today.Month
            && date.Day == today.Day
        )
        {
            weekObjects[y].transform.GetChild(x).GetComponent<CalendarElement>().SetToday();
        }
        
        
    }
}

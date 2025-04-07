using UnityEngine;

public class CalendarPanel : Panel
{
    Date date;
    //내가 만든 자료형 Date로 굳이 변경할 이유가 있을까?
    protected override void Start()
    {
        base.Start();
        // 오늘 날짜 가져오기
        System.DateTime today = System.DateTime.Now;

        date = new Date();
        date.SetDate(today);

        UpdateCalendarStatus();
    }

    //상태 업데이트
    void UpdateCalendarStatus()
    {
        //date로 상태 업데이트

        //달력 상단바 -> 2025.04
        //대충 4월의 1일의 요일을 기준으로 달력 세팅
    }
}

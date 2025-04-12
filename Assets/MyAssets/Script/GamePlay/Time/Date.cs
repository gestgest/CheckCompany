using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

//그냥 자료형으로 하자 그냥
public class Date
{
    int year;
    int month;
    int day;
    Week week; //MON ~ 

    int hour;
    int minute;

    public Date()
    {
        year = 1;
        month = 1;
        day = 1;
        week = Week.MON;
        hour = 1;
        minute = 1;
    }

    #region PROPERTY
    //헷갈리지 않기 위해 set은 protected로 설정하자.
    //괜히 외부에서 Year++ 이런거 쓰면 골치아프다 
    public virtual int Year
    {
        protected set { year = value; }
        get { return year; }
    }

    public virtual int Month //1~12
    {
        protected set
        {
            month = value;
            if (month > 12)
            {
                Year++;
                month %= 12;
            }

            //0월 => 12월
            if (month < 1)
            {
                Year--;
                month = month + 12;
            }
        }
        get { return month; }
    }

    public virtual int Day
    {
        set
        {
            //day--
            int before = day;

            //day = 5일
            day = value; //0 이하는 음수
            
            int dis = day - before; //음수면 음수값이 나옴

            //Debug.Log("정 답을 알려줘 - day : " + day + ", before : " + before + ", dis : " + dis);
            if (dis < 0)
            {
                //sunday는 
                dis = 7 - (-dis % 7);
            }

            while (true)
            {
                //매우 큰 day가 들어오면
                int day_leapYear = addDay_LeapYear(Year, Month);
                if (day > MONTH_DAY[Month - 1] + day_leapYear)
                {
                    day -= MONTH_DAY[Month - 1] + day_leapYear;
                    Month++; //위치 바꾸지 마라
                }
                else if (day <= 0) //0이어도 음수값이라 
                {
                    Month--;
                    day_leapYear = addDay_LeapYear(Year, Month);
                    day += MONTH_DAY[Month - 1] + day_leapYear;
                }
                else
                {
                    break;
                }
            }

            AddWeek(dis);
        }
        get { return day; }
    }

    public virtual Week _Week
    {
        protected set { week = value; }
        get { return week; }
    }

    public void AddWeek(int day)
    {
        day = (day + (int)_Week) % 7;
        _Week = (Week)day;
    }

    public virtual int Hour
    {
        protected set
        {
            hour = value;
            if (hour >= 24)
            {
                Day += hour / 24;
                hour %= 24;
            }
        }
        get { return hour; }
    }

    public virtual int Minute
    {
        set
        {
            minute = value;
            if (minute >= 60)
            {
                Hour += minute / 60;
                minute %= 60;
            }
        }
        get { return minute; }
    }

    public void SetDate(System.DateTime dateTime)
    {
        //기존 값이랑 값 비교 해야함

        day = dateTime.Day;
        month = dateTime.Month;
        year = dateTime.Year;
        week = dateTime.DayOfWeek switch
        {
            DayOfWeek.Monday => Week.MON,
            DayOfWeek.Tuesday => Week.TUE,
            DayOfWeek.Wednesday => Week.WED,
            DayOfWeek.Thursday => Week.THU,
            DayOfWeek.Friday => Week.FRI,
            DayOfWeek.Saturday => Week.SAT,
            DayOfWeek.Sunday => Week.SUN,
            _ => week
        };
        hour = dateTime.Hour;
        minute = dateTime.Minute;
    }
    
    //얇은 복사
    public void SetDate(Date date)
    {
        day = date.Day;
        month = date.Month;
        year = date.Year;
        week = date._Week;
        hour = date.Hour;
        minute = date.Minute;
    }

    #endregion

    public static int[] MONTH_DAY =
    {
        31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
    };

    public static int addDay_LeapYear(int year, int month)
    {
        if (month != 2)
        {
            return 0;
        }

        if (year % 400 == 0)
        {
            return 1;
        }

        if (year % 100 == 0)
        {
            return 0;
        }

        if (year % 4 == 0)
        {
            return 1;
        }

        return 0;
    }

    public void AddYear(int year)
    {
        //양수
        for (int i = 0; i < year; i++)
        {
            AddMonth(12);
        }
        //음수
        for (int i = 0; i < - year; i++)
        {
            AddMonth(-12);
        }
    }
    
    //달을 더하거나 빼는 기능.
    //3 - 1월 31일 => 2월 31일 반례 해결 용
    public void AddMonth(int month)
    {
        //양수
        for (int i = 0; i < month; i++)
        {
            Day = Day + (MONTH_DAY[this.month - 1] + addDay_LeapYear(year, month));
        }
        //음수
        for (int i = 0; i < - month; i++)
        {
            Day = Day - (MONTH_DAY[this.month - 1] + addDay_LeapYear(year, month));
        }
    }

    public override string ToString()
    {
        return Year + "년 "
                    + Month + "월 "
                    + Day + "일 "
                    + week + "요일 "
                    + Hour + "시 "
                    + Minute + "분";
    }
}

public enum Week
{
    MON,
    TUE,
    WED,
    THU,
    FRI,
    SAT,
    SUN
}
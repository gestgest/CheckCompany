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
    public virtual int Year
    {
        set
        {
            year = value;
        }
        get { return year; }
    }

    public virtual int Month //1~12
    {
        set
        {
            month = value;
            if (month > 12)
            {
                Year++;
                Month %= 12;
            }
        }
        get
        {
            return month;
        }
    }

    public virtual int Day
    {
        set
        {
            //10일 => 5일, before = 10일
            int before = day;

            //day = 5일
            day = value;
            int dis = day - before; //음수면 음수값이 나옴

            //매우 큰 day가 들어오면
            while (true)
            {
                int day_leapYear = addDay_LeapYear(Year, Month);
                if (Day > MONTH_DAY[Month - 1] + day_leapYear)
                {
                    Day %= MONTH_DAY[Month - 1] + day_leapYear;

                    Month++;
                }
                else if(Day < 0) //
                {
                    Month--;
                    day_leapYear = addDay_LeapYear(Year, Month);
                    Day = MONTH_DAY[Month - 1] + day_leapYear + Day + 1;
                }
                else
                {
                    break;
                }
            }
            AddWeek(dis);
        }
        get
        {
            return day;
        }
    }

    public virtual Week _Week { set { week = value; } get { return week; } }

    public void AddWeek(int day)
    {
        day = (day + (int)_Week) % 7;
        _Week = (Week)day;
    }

    public virtual int Hour
    {
        set
        {
            hour = value;
            if (hour >= 24)
            {
                Day += hour / 24;
                hour %= 24;
            }
        }
        get
        {
            return hour;
        }
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
        get
        {
            return minute;
        }
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
    MON, TUE, WED, THU, FRI, SAT, SUN
}
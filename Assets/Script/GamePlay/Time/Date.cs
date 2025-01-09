using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public int Year 
    { 
        set
    { 
        year = value;
    } get { return year; } }

    public int Month
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

    public int Day
    {
        set
        {
            int before = day;
            
            day = value;
            before = day - before;
            
            
            int day_leapYear = addDay_LeapYear();
            if (day > MONTH_DAY[month - 1] + day_leapYear)
            {
                Day %= MONTH_DAY[month - 1] + day_leapYear;

                Month++;
            }
            
            AddWeek(before);
        }
        get
        {
            return day; 
        } 
        
    }
    public Week _Week { set { week = value; } get { return week; } }

    private void AddWeek(int day)
    {
        day = (day + (int)week) % 7;
        week = (Week)day;
    }

    public int Hour
    {
        set
        {
            hour = value;
            if (hour >= 24)
            {
                hour %= 24;
                Day++;
            }
        }
        get
        {
            return hour;
        }
    }

    public int Minute
    {
        set
        {
            minute = value;
            if (minute >= 60)
            {
                minute %= 60;
                Hour++;
            }

            SetDateToServer(DateToJSON());
        }
        get
        {
            return minute; 
        }
    }
    #endregion

    private static int[] MONTH_DAY =
    {
        31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
    };

    private int addDay_LeapYear()
    {
        if (Month != 2)
        {
            return 0;
        }
        if (Year % 400 == 0)
        {
            return 1;
        }
        if (Year % 100 == 0)
        {
            return 0;
        }
        if (Year % 4 == 0)
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
                    + _Week + "요일 " 
                    + Hour + "시 " 
                    + Minute + "분";
    }

    #region SERVER

    //너무 데이터 낭비 아닐까 => year가 바뀌면 year만 수정하는 느낌으로
    //근데 또 그러기엔 여러번 서버에 전송하는 느낌
    public Dictionary<string, object> DateToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "year", year },
            { "month", month },
            { "day", day },
            { "week", (int)week },
            { "hour", hour },
            { "minute", minute }
        };
        
        return result;
    }
    public Dictionary<string, object>  YearToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "year", year },
        };
        
        return result;
    }

    public Dictionary<string, object>  MonthToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "month", month },
        };
        
        return result;
    }

    public Dictionary<string, object>  DayToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "day", day },
            { "week", (int)week },
        };
        
        return result;
    }

    public Dictionary<string, object> HourToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "hour", hour },
        };
        
        return result;
    }

    public Dictionary<string, object>  MinuteToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "minute", minute }
        };
        
        return result;
    }

    public void SetDateToServer(Dictionary<string, object> data)
    {
        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname ,
            "date",
            data
        );
    }
    public void GetDateFromJSON(Dictionary<string, object> data)
    {
        year = Convert.ToInt32(data["year"]);
        month = Convert.ToInt32(data["month"]);
        day = Convert.ToInt32(data["day"]);
        week = (Week)Convert.ToInt32(data["week"]);
        hour = Convert.ToInt32(data["hour"]);
        minute = Convert.ToInt32(data["minute"]);
    }

    #endregion
}

public enum Week
{            
    MON, TUE, WED, THU, FRI, SAT, SUN
}
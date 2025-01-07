using UnityEngine;

class Date
{
    int year;
    int month;
    int day;
    int week; //MON ~ 

    int hour;
    int minute;

    #region PROPERTY
    public int Year { set { year = value; } get { return year; } }

    public int Month { set { month = value; } get { return month; } }
    public int Day { set { day = value; } get { return day; } }
    public int Week { set { week = value; } get { return week; } }

    public int Hour { set { hour = value; } get { return hour; } }

    public int Minute { set { minute = value; } get { return minute; } }
    #endregion
}

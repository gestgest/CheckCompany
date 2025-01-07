using UnityEngine;

public class Date
{
    int year;
    int month;
    int day;
    int week; //MON ~ 

    int hour;
    int minute;

    public Date()
    {
        year = 1;
        month = 1;
        day = 1;
        week = 1;
        hour = 1;
        minute = 1;
    }
    #region PROPERTY
    public int Year { set { year = value; } get { return year; } }

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
            day = value;
            int day_leapYear = addDay_LeapYear();
            if (day > MONTH_DAY[month - 1] + day_leapYear)
            {
                Day %= MONTH_DAY[month - 1] + day_leapYear;

                Month++;
            }
        }
        get
        {
            return day; 
        } 
        
    }
    public int Week { set { week = value; } get { return week; } }

    public int Hour { set { hour = value; } get { return hour; } }

    public int Minute { set { minute = value; } get { return minute; } }
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
                    + Week + "요일 " 
                    + Hour + "시 " 
                    + Minute + "분";
    }
}

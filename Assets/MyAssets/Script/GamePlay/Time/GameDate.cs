using System.Collections.Generic;
using System;
using UnityEngine;

public class GameDate : Date
{
    public override int Month
    {
        get
        {
            return base.Month;
        }
        set
        {
            base.Month = value;

            //월급 차감
            if (EmployeeController.instance.PayEmployees())
            {
                Debug.Log("월급 차감");
            }
            else
            {
                Debug.Log("월급이 없습니다");
            }
        }
    }

    public override int Day
    {
        get
        {
            return base.Day;
        }
        set
        {
            base.Day = value;

            //임시 디버깅용 회복 함수
            EmployeeController.instance.AddStamina(70);
            Debug.Log("체력 회복");
        }
    }

    public override int Minute
    {
        get
        {
            return base.Minute;
        }
        set
        {
            base.Minute = value;
            SetDateToServer(DateToJSON());
        }
    }


    #region SERVER

    //너무 데이터 낭비 아닐까 => year가 바뀌면 year만 수정하는 느낌으로
    //근데 또 그러기엔 여러번 서버에 전송하는 느낌
    public Dictionary<string, object> DateToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "year", Year },
            { "month", Month },
            { "day", Day },
            { "week", (int)_Week },
            { "hour", Hour },
            { "minute", Minute }
        };

        return result;
    }
    public Dictionary<string, object> YearToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "year", Year },
        };

        return result;
    }

    public Dictionary<string, object> MonthToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "month", Month },
        };

        return result;
    }

    public Dictionary<string, object> DayToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "day", Day },
            { "week", (int)_Week },
        };

        return result;
    }

    public Dictionary<string, object> HourToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "hour", Hour },
        };

        return result;
    }

    public Dictionary<string, object> MinuteToJSON()
    {
        Dictionary<string, object> result = new Dictionary<string, object>()
        {
            { "minute", Minute }
        };

        return result;
    }

    public void SetDateToServer(Dictionary<string, object> data)
    {
        FireStoreManager.instance.SetFirestoreData("GamePlayUser",
            GameManager.instance.Nickname,
            "date",
            data
        );
    }
    public void GetDateFromJSON(Dictionary<string, object> data)
    {
        Year = Convert.ToInt32(data["year"]);
        Month = Convert.ToInt32(data["month"]);
        Day = Convert.ToInt32(data["day"]);
        _Week = (Week)Convert.ToInt32(data["week"]);
        Hour = Convert.ToInt32(data["hour"]);
        Minute = Convert.ToInt32(data["minute"]);
    }

    #endregion

}

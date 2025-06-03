using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class GameDate : Date
{
    public UnityAction<int> _onStaminaChanged;
    public SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    /// <summary> 생성자 </summary>
    /// <param name="onStaminaChanged">EmployeeControllerSO의 함수</param>
    public GameDate(UnityAction<int> onStaminaChanged, SendFirebaseEventChannelSO sendFirebaseEventChannelSO)
    {
        this._onStaminaChanged += onStaminaChanged;
        this._sendFirebaseEventChannelSO = sendFirebaseEventChannelSO;
    }

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
            // if (EmployeeController.instance.PayEmployees())
            // {
            //     Debug.Log("월급 차감");
            // }
            // else
            // {
            //     Debug.Log("월급이 없습니다");
            // }
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
            _onStaminaChanged.Invoke(70);
            //EmployeeControllerSO.instance.AddStamina(70);
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

    public override void GetDateFromJSON(Dictionary<string, object> data)
    {
        if (data == null)
        {
            SetDateNow();
            SetDateToServer(DateToJSON());
            return;
        }
        base.GetDateFromJSON(data);
    }

    public void SetDateToServer(Dictionary<string, object> data)
    {
        _sendFirebaseEventChannelSO._onSendEventRaised(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "date",
            data
        );
    }

    #endregion

}

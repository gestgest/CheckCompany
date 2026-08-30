using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class GameDate : Date
{
    public SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;

    //서버/기본값 로딩 중에는 Month setter가 여러 번 지나가므로, 로딩이 끝나기 전까지는
    //월급을 차감하면 안 된다(로그인할 때마다 월급이 빠지는 걸 방지).
    private bool _isLoaded;

    //서버가 없다는 로그는 한 번만 남긴다. 시간이 자동으로 흐르면 저장 시도가 계속 반복된다.
    private bool _warnedNoServer;

    /// <summary>서버(또는 기본값) 로딩이 끝났는지. 끝나기 전에는 시간을 돌리면 안 된다.</summary>
    public bool IsLoaded => _isLoaded;

    /// <summary> 생성자 </summary>
    public GameDate(SendFirebaseEventChannelSO sendFirebaseEventChannelSO)
    {
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

            if (!_isLoaded)
            {
                return;
            }

            //월급 차감
            if (GameManager.instance.PayEmployees())
            {
                Debug.Log("월급 차감");
            }
            else
            {
                Debug.Log("월급이 부족합니다");
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
            //31 => 1일로 넘어갈 때 Month가 바뀌면서 위 Month setter가 실제 월급 정산을 처리한다.
            base.Day = value;
            //체력 회복은 이제 EmployeeWorkAI가 근무 상태에 따라 실시간으로 처리한다.
        }
    }

    public void SetMinute(int minute, bool isServer = true)
    {
        base.SetMinute(minute);
        if(isServer)
            SetDateToServer(DateToJSON());
    }

    #region SERVER

    public override void GetDateFromJSON(Dictionary<string, object> data)
    {
        if (data == null)
        {
            SetDateNow();
            SetDateToServer(DateToJSON());
            _isLoaded = true;
            return;
        }
        base.GetDateFromJSON(data);
        _isLoaded = true;
    }

    public void SetDateToServer(Dictionary<string, object> data)
    {
        //서버 매니저(PersistentManager)가 없는 로컬 테스트에서는 아무도 채널을 구독하지 않는다.
        //예전에는 델리게이트를 직접 부르고 있어서 그때마다 NullReferenceException이 났다.
        //(GameManager가 로그인 실패를 "로컬 테스트로 시작합니다"로 넘기는 것과 같은 취급을 해야 한다)
        if (_sendFirebaseEventChannelSO == null || _sendFirebaseEventChannelSO._onSendEventRaised == null)
        {
            if (!_warnedNoServer)
            {
                _warnedNoServer = true;
                Debug.Log("[GameDate] 서버 매니저가 없어 날짜 저장을 건너뜁니다. (로컬 테스트)");
            }

            return;
        }

        _sendFirebaseEventChannelSO.RaiseEvent(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "date.gameDate",
            data
        );
    }

    #endregion

}

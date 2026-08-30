using UnityEngine;

/// <summary>
/// 게임 시간을 알아서 흐르게 한다.
///
/// 지금까지는 TimeButton을 꾹 누르고 있는 동안에만 시간이 갔다(1초에 1시간).
/// 그러면 플레이어가 손을 떼는 순간 회사가 멈추기 때문에 월급도, 체력도, 미션 마감도 아무것도 진행되지 않는다.
/// 여기서 그 "꾹 누르기"를 대신 해준다 - TimeButton은 그대로 두면 빨리감기 버튼이 된다.
///
/// GameManager와 같은 오브젝트에 붙는다. 통째로 지우면 예전(버튼으로만 조절)으로 돌아간다.
/// </summary>
public class GameClock : MonoBehaviour
{
    [Header("속도")]
    //한 틱에 흐르는 게임 시간(분). 60이면 실제 1틱 = 게임 1시간으로, 버튼을 꾹 눌렀을 때와 같다.
    [SerializeField] private int _gameMinutesPerTick = 60;

    //몇 초에 한 틱인지. 1이면 실제 1초에 게임 1시간.
    [SerializeField] private float _realSecondsPerTick = 1f;

    //배속. 나중에 x2/x4 버튼을 붙이면 여기를 바꾸면 된다.
    [SerializeField] private float _speed = 1f;

    [SerializeField] private bool _runOnStart = true;

    [Header("서버 저장")]
    //매 틱마다 서버에 쓰면 초당 1회 쓰기가 된다. Firestore 무료 할당량이 하루도 못 간다.
    //그래서 틱은 로컬에만 반영하고, 이만큼 모이면 한 번 저장한다. (24 = 게임 하루에 한 번)
    [SerializeField] private int _saveEveryTicks = 24;

    private float _elapsed;
    private int _ticksSinceSave;
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    /// <summary>배속. 0 이하는 멈춤이 아니라 무시된다 - 멈추려면 Pause()를 쓴다.</summary>
    public float Speed
    {
        get => _speed;
        set
        {
            if (value > 0f)
            {
                _speed = value;
            }
        }
    }

    private void Start()
    {
        _isRunning = _runOnStart;
    }

    public void Pause()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;

        //멈춘 시점의 시간은 남겨둔다. 안 그러면 마지막 저장 이후 흐른 시간이 통째로 날아간다.
        Save();
    }

    public void Resume()
    {
        _isRunning = true;
        _elapsed = 0f;
    }

    public void Toggle()
    {
        if (_isRunning)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Update()
    {
        if (!_isRunning || GameManager.instance == null)
        {
            return;
        }

        //서버에서 날짜를 받아오기 전에 시간을 돌리면, 곧이어 도착한 서버 값이 그걸 덮어쓴다.
        //로딩이 끝날 때까지는 가만히 기다린다.
        if (!GameManager.instance.IsDateReady)
        {
            return;
        }

        _elapsed += Time.deltaTime * _speed;

        //프레임이 크게 밀렸으면 밀린 만큼 여러 틱을 돌린다
        while (_elapsed >= _realSecondsPerTick)
        {
            _elapsed -= _realSecondsPerTick;
            Tick();
        }
    }

    private void Tick()
    {
        //서버 저장은 여기서 하지 않는다 (_saveEveryTicks마다 몰아서 한 번)
        GameManager.instance.AddDateMinute(_gameMinutesPerTick, false);

        _ticksSinceSave++;

        if (_ticksSinceSave >= _saveEveryTicks)
        {
            Save();
        }
    }

    private void Save()
    {
        _ticksSinceSave = 0;

        if (GameManager.instance != null)
        {
            GameManager.instance.SaveDate();
        }
    }

    //홈 버튼으로 앱을 내리거나 껐을 때, 마지막 저장 이후 흐른 시간을 잃지 않도록 한 번 쓴다
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && _ticksSinceSave > 0)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        if (_ticksSinceSave > 0)
        {
            Save();
        }
    }
}

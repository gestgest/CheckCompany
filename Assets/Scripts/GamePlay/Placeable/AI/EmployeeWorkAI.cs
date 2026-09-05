using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 직원이 근무시간(Employee._WorkTime)에 맞춰 출근하고, 자리에서 일하고, 퇴근한다.
/// 체력이 바닥나면 근무시간이라도 자리에서 일어나 쉰다.
///
///   OffDuty ──출근시간──> GoingToDesk ──도착──> SittingDown ──> Working
///      ↑                                                          │
///      └─ GoingHome(출입구로 이동) ←── StandingUp ←── 퇴근시간 ─────┘
///                 체력 0 ↓        ↑ 회복
///                        Resting ─┘
///
/// GoingHome은 놓인 문(ObjectType.Door)이 하나라도 있을 때만 거친다. 없으면 예전처럼 그 자리에서 바로 OffDuty.
/// 걸어가는 도중에 문 앞을 지나면 그 문이 열린다 - TickDoor 참고. 출근길과 퇴근길 양쪽에 똑같이 적용된다.
///
/// 예전에는 스폰되자마자 빈 책상으로 직진해서 도착하면 영원히 굳어 있었다(Working이 종착역).
/// 그래서 새벽 3시에도 출근해 있고, 체력이 0이 돼도 계속 일했다.
/// 이제 Working은 종착역이 아니고, 이 직원이 움직이는 이유는 전부 시간이나 체력에서 나온다.
///
/// SittingDown / StandingUp은 앉고 일어서는 모션이 재생되는 동안만 머무는 상태다.
/// 이 두 상태가 없으면 도착하자마자 앉은 자세로 굳고, 퇴근할 때는 앉은 채로 미끄러져 나간다.
///
/// EmployeeObjectSystem이 생성 직후 Init(employeeId)를 호출해줘야 한다.
/// </summary>
public class EmployeeWorkAI : MonoBehaviour
{
    private enum State
    {
        OffDuty,     //퇴근 상태. 배정받을 자리가 없어 대기하는 동안도 여기에 머문다
        GoingToDesk, //자리로 이동중
        SittingDown, //자리에 도착해 앉는 중
        Working,     //자리에서 근무중
        StandingUp,  //자리를 뜨려고 일어서는 중
        Resting,     //체력이 바닥나 쉬는 중 (근무시간이어도)
        GoingHome    //퇴근길, 출입구(문)로 이동중
    }

    [SerializeField] private WorkstationManagerSO _workstationManagerSO;
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    //자리 도착 판정 반경. NavMeshAgent.stoppingDistance에도 그대로 넣어서(Awake) 이 반경 안에서부터
    //agent 스스로 감속하게 만든다. remainingDistance만으로 판정하면 agent는 끝까지 전속력으로 오다가
    //문턱을 넘는 순간 급정지 + 주변 회피(obstacle avoidance) 보정이 겹쳐 도착 직전에 떨린다.
    [SerializeField] private float _arriveDistance = 0.3f;
    [SerializeField] private float _seatNavMeshSampleRadius = 2.0f; //자리(의자) 주변에서 NavMesh를 찾을 반경

    [Header("Decision")]
    //근무시간/체력을 다시 판단하는 간격. 매 프레임 볼 필요가 없고(게임 1시간이 실제 1초다),
    //직원마다 값이 달라지므로 여럿이 같은 순간에 같은 판단을 해서 떼로 움직이는 것도 자연히 줄어든다.
    [SerializeField] private float _decisionIntervalMin = 0.3f;
    [SerializeField] private float _decisionIntervalMax = 0.9f;

    //배정된 자리가 없을 때 빈 자리를 스스로 집어갈지. 끄면 플레이어가 UI로 꽂아준 자리에만 앉는다.
    [SerializeField] private bool _autoClaimSeat = true;

    //책상을 이 거리 이상 옮기면 앉은 채로 끌려가지 않고 다시 걸어간다
    [SerializeField] private float _seatMovedThreshold = 0.3f;

    [Header("Idle Wander")]
    [SerializeField] private float _wanderRadius = 3.0f;        //현재 자리에서 돌아다닐 반경
    [SerializeField] private float _wanderIntervalMin = 1.0f;   //다음 목적지를 고르기까지 최소 대기
    [SerializeField] private float _wanderIntervalMax = 5.0f;   //최대 대기

    [Header("Stamina")]
    [SerializeField] private float _staminaDrainPerMinute = 6f;    //근무 중 분당 체력 소모량
    [SerializeField] private float _staminaRecoverPerMinute = 12f; //비근무(이동/대기/휴식) 중 분당 체력 회복량

    //휴식에서 복귀하는 기준. 최대 체력 대비 비율이라 max_stamina가 다른 직원에게도 같이 통한다.
    [SerializeField, Range(0f, 1f)] private float _restExitStaminaRatio = 0.5f;

    [Header("Door")]
    //문에서 이 거리 안에 들어오면 문을 열기 시작한다. 문에 닿아서야 열면
    //여는 동안(_openDuration) 몸이 이미 문짝을 뚫고 지나가 있다.
    [SerializeField] private float _doorOpenDistance = 2.0f;

    [Header("Sit")]
    //앉기/일어서기 모션이 끝날 때까지 기다리는 시간. Assets/Animation의 StandToSit(2.23초),
    //SitToStand(2.27초) 길이에 맞춰둔 값이다. 클립을 갈아끼우면 여기도 같이 맞춰야
    //다 앉기도 전에 근무로 넘어가거나, 다 일어서기도 전에 걸어나간다.
    [SerializeField] private float _sitDownDuration = 2.2f;
    [SerializeField] private float _standUpDuration = 2.2f;

    //Animator의 앉기 3단(SitDown -> Seated -> StandUp)을 켜고 끄는 bool.
    //true인 동안 앉는 모션 -> 앉은 자세 유지, false로 내리면 일어서는 모션이 재생된다.
    private static readonly int IsSeatedHash = Animator.StringToHash("IsSeated");

    //출퇴근(자리로 걸어가는 GoingToDesk, 출입구로 걸어나가는 GoingHome)도 마찬가지로 전용 Walk 클립이
    //없어서 RunForward를 느리게(m_Speed 0.6) 재생해 걷는 것처럼 대체한다. 실제로 agent가 움직이는
    //상태는 지금은 이 둘뿐이라 상태만 보고 켜고 꺼도 충분하다 (WanderRoutine이 나중에 다시 켜지면 같이 고려).
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private NavMeshAgent _agent;
    private Animator _animator;

    //퇴근해서 회사에 없는 상태. OffDuty와 구분해야 한다 -
    //근무시간인데 빈 자리가 없어 대기하는 경우도 OffDuty지만, 그때는 회사에 있어야 한다.
    private bool _isHome;

    //퇴근했을 때 끌 것들. 매번 찾지 않도록 Awake에서 한 번 모은다.
    private Renderer[] _renderers;

    private State _state = State.OffDuty;
    private Transform _seat;

    //자리로 출발할 때의 의자 위치. 의자가 옮겨진 걸 알아채는 기준점이다.
    private Vector3 _seatAnchor;

    //지금 걸어가고 있는 목적지(NavMesh 위로 스냅된 좌표). 도착 판정에 쓴다.
    private Vector3 _destination;

    private int _employeeId;
    private Employee _employee;
    private float _staminaAccumulator;

    //지금 열어둔 채로 잡고 있는 문. 배치물과 컴포넌트를 같이 들고 있는 이유는 TickDoor 참고.
    private PlaceableObject _heldDoorObject;
    private Door _heldDoor;

    /// <summary>이 오브젝트가 대표하는 Employee의 ID. EmployeeObjectSystem에서 생성 직후 호출.</summary>
    public void Init(int employeeId)
    {
        _employeeId = employeeId;
        _employee = _employeeManagerSO.GetEmployeeById(employeeId);
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        //SkinnedMeshRenderer까지 같이 잡힌다. 꺼져있는 것도 포함해서 모아야
        //나중에 켤 때 원래 꺼져 있던 것만 골라낼 필요가 없다.
        _renderers = GetComponentsInChildren<Renderer>(true);

        //agent 자신의 감속 반경을 도착 판정 반경과 맞춘다. autoBraking이 이 값부터 미리 속도를 줄이므로
        //remainingDistance가 문턱을 넘을 때는 이미 거의 멈춰 있는 상태다.
        _agent.stoppingDistance = _arriveDistance;
    }

    private void Start()
    {
        StartCoroutine(DecisionRoutine());
        //StartCoroutine(WanderRoutine());
        ////일단 돌아다니는게 근본없어서 뺌. 나중에 키보드 딸깍거리고 토크하고 탕비실가고 이런거 원함
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        //잡은 채로 사라지면 그 문은 영영 열린 채로 남는다
        ReleaseDoor();

        //오브젝트만 사라지고 Employee 데이터는 남는 경우(씬 전환 등)에 근무중으로 굳어 있으면
        //아무도 앉아 있지 않은데 계속 돈이 들어온다
        if (_employee != null)
        {
            _employee.IsWorking = false;
        }

        //퇴사는 진짜로 자리를 비우는 것이다 (퇴근과 달리 배정까지 지운다)
        if (_seat != null)
        {
            _workstationManagerSO.ReleaseSeat(_employeeId);
        }
    }

    #region DECISION

    /// <summary>근무시간과 체력을 주기적으로 보고 상태를 옮긴다. 이 직원이 움직이는 이유는 전부 여기서 나온다.</summary>
    private IEnumerator DecisionRoutine()
    {
        //스폰 직후 전원이 같은 프레임에 판단하지 않도록 시작점도 흩뜨린다
        yield return new WaitForSeconds(Random.Range(0f, _decisionIntervalMax));

        while (true)
        {
            Decide();
            yield return new WaitForSeconds(Random.Range(_decisionIntervalMin, _decisionIntervalMax));
        }
    }

    private void Decide()
    {
        //직원 데이터가 아직 없으면 근무시간을 알 수 없다
        if (_employee == null)
        {
            return;
        }

        //날짜 로딩이 끝나기 전의 시각은 서버 값이 아니다.
        //그걸 보고 출근시키면 접속할 때마다 엉뚱한 시각에 움직이기 시작한다.
        if (GameManager.instance == null || !GameManager.instance.IsDateReady)
        {
            return;
        }

        //앉거나 일어서는 도중에는 판단을 미룬다. 모션이 끝나면 다음 판단 때 이어서 처리하면 된다.
        //(여기서 상태를 갈아버리면 앉다 만 자세로 걸어가거나 서 있는 채로 책상에 붙는다)
        if (_state == State.SittingDown || _state == State.StandingUp)
        {
            return;
        }

        bool onDuty = IsWorkTime();

        switch (_state)
        {
            case State.OffDuty:
                //자리가 없어 대기중인 경우도 여기로 오므로, 근무시간이면 매번 다시 자리를 노려본다
                if (onDuty && !IsExhausted())
                {
                    GoToDesk();
                }
                break;

            case State.GoingToDesk:
                if (!onDuty)
                {
                    LeaveWork();
                }
                else if (IsExhausted())
                {
                    EnterRest();
                }
                else if (IsSeatStale())
                {
                    GoToDesk();
                }
                break;

            case State.Working:
                //앉아 있으므로 무엇을 하든 먼저 일어서야 한다
                if (!onDuty)
                {
                    StandUpThen(LeaveWork);
                }
                else if (IsExhausted())
                {
                    StandUpThen(EnterRest);
                }
                else if (IsSeatStale())
                {
                    StandUpThen(GoToDesk);
                }
                break;

            case State.Resting:
                if (!onDuty)
                {
                    LeaveWork();
                }
                else if (IsRecovered())
                {
                    GoToDesk();
                }
                break;

            case State.GoingHome:
                //출입구로 걸어나가는 도중에 다시 근무시간이 된 경우(근무시간이 짧게 붙어있는 등) 발길을 돌린다
                if (onDuty && !IsExhausted())
                {
                    GoToDesk();
                }
                break;
        }
    }

    /// <summary>지금이 이 직원의 근무시간인지. WorkTime은 시(hour) 단위 실수다 (9.5 = 9시 30분).</summary>
    private bool IsWorkTime()
    {
        GameDate date = GameManager.instance._Date;

        if (date == null)
        {
            return false;
        }

        WorkTime workTime = _employee._WorkTime;

        //start == end는 근무시간 0시간으로 본다. 24시간 근무로 해석하면 영원히 퇴근을 안 한다.
        if (Mathf.Approximately(workTime.start, workTime.end))
        {
            return false;
        }

        float now = date.Hour + date.Minute / 60f;

        if (workTime.start < workTime.end)
        {
            return now >= workTime.start && now < workTime.end;
        }

        //22 ~ 6처럼 자정을 넘기는 근무
        return now >= workTime.start || now < workTime.end;
    }

    private bool IsExhausted()
    {
        return _employee.Stamina <= 0;
    }

    private bool IsRecovered()
    {
        return _employee.Stamina >= _employee.Max_Stamina * _restExitStaminaRatio;
    }

    #endregion

    #region STATE

    /// <summary>배정된 자리로 출발한다. 자리가 없으면 대기(OffDuty)로 돌아가 다음 판단 때 다시 시도한다.</summary>
    private void GoToDesk()
    {
        //출근. 퇴근해서 안 보이던 상태였으면 여기서 다시 나타난다.
        //자리를 못 찾아도(아래에서 EnterOffDuty로 빠져도) 출근은 한 것이므로 먼저 켠다 -
        //그래야 "자리가 없어서 서성이는 직원"이 플레이어 눈에 보인다.
        //ResolveSeat/MoveToSeat이 NavMeshAgent를 쓰므로 그 전에 켜야 한다.
        SetHome(false);

        Transform seat = ResolveSeat();

        if (seat == null)
        {
            //빈 자리가 없다. 근무시간이어도 할 수 있는 게 없으니 어슬렁거리며 기다린다.
            EnterOffDuty();
            return;
        }

        _seat = seat;
        _seatAnchor = seat.position;

        if (!MoveToSeat())
        {
            EnterOffDuty();
            return;
        }

        SetState(State.GoingToDesk);
    }

    /// <summary>
    /// 이 직원이 앉을 자리. 플레이어가 UI로 꽂아둔 배정이 있으면 그것이 우선이고,
    /// 없을 때만 빈 자리를 스스로 집어간다.
    /// </summary>
    private Transform ResolveSeat()
    {
        PlaceableObject assigned = _workstationManagerSO.GetAssignedWorkstation(_employeeId);

        if (assigned != null)
        {
            //자리는 컴퓨터에 배정되지만 실제로 걸어가는 곳은 그 책상에 붙어있는 의자다.
            //의자를 치우면 여기서 null이 나오고, 아래 IsSeatStale이 자리를 놓친 것으로 본다.
            return _workstationManagerSO.GetSeatPoint(assigned);
        }

        return _autoClaimSeat ? _workstationManagerSO.RequestSeat(_employeeId) : null;
    }

    /// <summary>
    /// 지금 향하고 있는(또는 앉아 있는) 자리가 더 이상 유효하지 않은지.
    /// 자리가 삭제됐거나, 플레이어가 다른 직원을 꽂아 배정이 바뀌었거나, 책상 자체가 옮겨진 경우다.
    /// </summary>
    private bool IsSeatStale()
    {
        PlaceableObject assigned = _workstationManagerSO.GetAssignedWorkstation(_employeeId);

        if (assigned == null || _seat == null)
        {
            return true;
        }

        Transform seat = _workstationManagerSO.GetSeatPoint(assigned);

        if (seat != _seat)
        {
            return true;
        }

        //의자를 옮기면 자리도 같이 움직인다. 앉은 자세 그대로 끌려가면 안 되므로 다시 걸어간다.
        return (seat.position - _seatAnchor).sqrMagnitude > _seatMovedThreshold * _seatMovedThreshold;
    }

    private bool MoveToSeat()
    {
        return MoveTo(_seat.position, $"'{_seat.name}'", "의자(또는 의자의 SeatPoint)를 NavMesh가 베이크된 바닥 위로 옮겨주세요.");
    }

    /// <summary>
    /// destination 주변에서 실제로 걸어갈 수 있는 가장 가까운 지점을 찾아 그리로 출발시킨다.
    /// 의자 자리/출입구 자체가 모델 안쪽 등 NavMesh가 없는 지점일 수 있어서 스냅이 필요하다.
    /// GoToDesk(자리)와 LeaveWork(출입구) 둘 다 이 함수를 쓴다.
    /// </summary>
    private bool MoveTo(Vector3 destination, string label, string hint = "")
    {
        if (!ResumeAgent())
        {
            return false;
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, _seatNavMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning(
                $"[EmployeeWorkAI] employee {_employeeId} : {label} 주변 {_seatNavMeshSampleRadius}m 안에 NavMesh가 없습니다. " +
                hint
            );
            return false;
        }

        if (!_agent.SetDestination(hit.position))
        {
            Debug.LogWarning($"[EmployeeWorkAI] employee {_employeeId} : {label}까지 가는 경로를 계산하지 못했습니다.");
            return false;
        }

        //도착 판정에 쓸 실제 목적지. agent의 remainingDistance를 믿으면 안 되는 이유는 HasArrived() 참고.
        _destination = hit.position;

        return true;
    }

    /// <summary>
    /// 지금 향하는 목적지에 닿았는지.
    ///
    /// 예전에는 pathPending / remainingDistance로 판정했는데, SetDestination() 직후 몇 프레임은
    /// 경로 계산이 끝나지 않아 remainingDistance가 0으로 나온다. 그 값을 그대로 믿으면
    /// 출발하자마자 "도착"으로 보고 그 자리에서 멈춰서, 책상으로도 문으로도 한 발짝을 안 뗐다.
    ///
    /// 그래서 agent 내부 상태 대신 목적지까지의 실제 거리로 본다.
    /// y는 빼고 잰다 - 바닥 높이 차이나 캐릭터 피벗 때문에 영원히 도착 못 하는 걸 막는다.
    /// </summary>
    private bool HasArrived()
    {
        Vector3 offset = _destination - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= _arriveDistance * _arriveDistance;
    }

    /// <summary>자리에 닿았다. 의자 쪽으로 돌아앉으면서 앉는 모션을 시작한다.</summary>
    private void ArriveAtDesk()
    {
        SetState(State.SittingDown);

        //도착 후에도 NavMeshAgent를 멈추지 않으면 계속 같은 목적지로 미세 보정을 시도해서
        //(특히 근처에 다른 직원이 있으면 서로 밀어내는 obstacle avoidance 때문에) 제자리에서 왔다갔다 떨리게 된다.
        StopAgent();

        transform.rotation = _seat.rotation;

        //근무/휴식이 바뀌는 순간의 체력만 서버에 남긴다 (TickStamina 주석 참고)
        SaveStamina();

        StartCoroutine(SitDownRoutine());

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}'에 도착해서 앉는 중 (SittingDown)");
    }

    /// <summary>앉는 모션이 끝나면 근무 상태로 넘어간다.</summary>
    private IEnumerator SitDownRoutine()
    {
        yield return new WaitForSeconds(_sitDownDuration);

        //기다리는 사이에 오브젝트가 지워졌거나 다른 데서 상태를 바꿨으면 손을 뗀다
        if (_state != State.SittingDown)
        {
            yield break;
        }

        SetState(State.Working);

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 근무 시작 (Working)");
    }

    /// <summary>
    /// 자리에서 일어난 다음 next를 실행한다. 앉아있지 않으면 곧바로 실행한다.
    ///
    /// 퇴근/휴식/자리 이동은 전부 "앉아있던 자리를 뜨는" 행동이라서, 일어서는 모션이 끝날 때까지
    /// NavMeshAgent를 세워둬야 한다. 안 그러면 앉은 자세 그대로 미끄러져 나간다.
    /// </summary>
    private void StandUpThen(System.Action next)
    {
        if (_state != State.Working)
        {
            next();
            return;
        }

        SetState(State.StandingUp);
        StartCoroutine(StandUpRoutine(next));

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 자리에서 일어나는 중 (StandingUp)");
    }

    private IEnumerator StandUpRoutine(System.Action next)
    {
        yield return new WaitForSeconds(_standUpDuration);

        if (_state != State.StandingUp)
        {
            yield break;
        }

        next();
    }

    /// <summary>
    /// 퇴근한다. 놓인 문이 있으면 가장 가까운 문까지 걸어나간 뒤(GoingHome) 도착해서야
    /// 진짜로 OffDuty가 되고, 없거나 경로를 못 찾으면 예전처럼 그 자리에서 바로 OffDuty로 처리한다.
    /// </summary>
    private void LeaveWork()
    {
        PlaceableObject exit = _workstationManagerSO.GetNearestDoor(transform.position);

        if (exit != null && MoveTo(exit.transform.position, "출입구", "문 위치를 NavMesh가 베이크된 곳으로 옮겨주세요."))
        {
            SetState(State.GoingHome);
            Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 퇴근길, 출입구로 이동중 (GoingHome)");
            return;
        }

        //문이 없거나 경로를 못 찾은 경우. 걸어나갈 곳이 없을 뿐 퇴근은 퇴근이라 그 자리에서 사라진다.
        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 퇴근 (OffDuty)");
        GoHome();
    }

    /// <summary>출입구에 도착해서 진짜로 퇴근 완료.</summary>
    private void ArriveHome()
    {
        GoHome();

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 출입구 도착, 퇴근 완료 (OffDuty)");
    }

    /// <summary>퇴근 처리. 회사에서 사라지고, 다음 출근시간에 GoToDesk()가 다시 켠다.</summary>
    private void GoHome()
    {
        //ArriveAtDesk와 같은 이유 - 도착 후에도 계속 목적지로 미세 보정하면 제자리에서 떨린다.
        //SetHome이 agent를 끄기 전에 불러야 한다(꺼진 agent는 건드릴 수 없다).
        StopAgent();
        EnterOffDuty();

        SetHome(true);
    }

    /// <summary>
    /// 퇴근/출근에 따라 회사에서 보이게 하거나 감춘다.
    ///
    /// GameObject를 통째로 SetActive(false)하면 안 된다 - Update와 DecisionRoutine이 같이 멈춰서
    /// 다음 날 출근시간이 와도 스스로 깨어나지 못하고, 집에서 체력이 회복되지도 않는다.
    /// 그래서 보이는 것(Renderer)과 길찾기(NavMeshAgent)만 끄고 판단 로직은 계속 돌게 둔다.
    ///
    /// agent까지 끄는 이유는, 켜둔 채로 두면 안 보이는 직원이 출입구에 서서 다른 직원의
    /// 회피 대상(obstacle avoidance)으로 남아 길을 막기 때문이다.
    /// </summary>
    private void SetHome(bool isHome)
    {
        if (_isHome == isHome)
        {
            return;
        }

        _isHome = isHome;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].enabled = !isHome;
            }
        }

        if (_agent != null)
        {
            _agent.enabled = !isHome;
        }
    }

    /// <summary>체력이 바닥나 자리에서 일어난다. 근무시간이어도 일을 멈춘다.</summary>
    private void EnterRest()
    {
        if (_state == State.Resting)
        {
            return;
        }

        SetState(State.Resting);
        ResumeAgent();
        SaveStamina();

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 체력이 바닥나 휴식 (Resting)");
    }

    private void EnterOffDuty()
    {
        if (_state == State.OffDuty)
        {
            return;
        }

        SetState(State.OffDuty);
        ResumeAgent();
        SaveStamina();

        //자리 배정은 일부러 남겨둔다.
        //플레이어가 UI로 "이 자리에 이 직원"을 꽂아두는데, 퇴근할 때마다 ReleaseSeat을 부르면
        //그 배정이 매일 밤 지워지고 다음 날 아무나 앉게 된다. 퇴근은 자리를 비우는 것이지 자리를 잃는 게 아니다.
        //진짜로 배정을 지우는 건 퇴사(OnDestroy)와 UI의 빼기 버튼(ReleaseSeatOf)뿐이다.
    }

    /// <summary>
    /// 상태를 옮기고, 수입 정산이 보는 IsWorking과 Animator 파라미터를 같이 맞춘다.
    /// _state를 직접 대입하면 둘이 어긋나서 퇴근한 직원이 계속 돈을 벌거나 그 반대가 된다.
    /// </summary>
    private void SetState(State next)
    {
        _state = next;

        if (_employee != null)
        {
            _employee.IsWorking = IsAtDesk(next);
        }

        if (_animator != null)
        {
            //앉는 중에도 true를 유지해야 SitDown -> Seated로 이어진다.
            //일어서는 중(StandingUp)에 false로 내리는 것이 곧 일어서라는 신호다.
            _animator.SetBool(IsSeatedHash, next == State.SittingDown || next == State.Working);
            _animator.SetBool(IsMovingHash, next == State.GoingToDesk || next == State.GoingHome);
        }
    }

    /// <summary>
    /// 자리에 붙어 있는 상태인지. 수입 정산과 체력 소모가 같이 보는 기준이다.
    ///
    /// 앉는 동작(SittingDown)까지 근무로 친다. 이 게임은 실제 1초가 게임 1시간이라
    /// 2초짜리 앉기 모션을 근무에서 빼면 하루 9시간 중 2시간이 통째로 날아간다.
    /// 자리에 도착한 순간부터 일어서기 시작할 때까지를 근무로 보는 편이 정산에도 맞다.
    /// </summary>
    private static bool IsAtDesk(State state)
    {
        return state == State.SittingDown || state == State.Working;
    }

    /// <summary>Agent를 세운다. NavMesh 밖이면 아무것도 하지 않는다(에러만 뱉는다).</summary>
    private void StopAgent()
    {
        //퇴근해서 꺼둔 agent는 isOnNavMesh를 읽는 것만으로도 에러가 난다
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return;
        }

        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
    }

    /// <summary>세워둔 Agent를 다시 움직일 수 있게 한다. NavMesh 위에 없으면 false.</summary>
    private bool ResumeAgent()
    {
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return false;
        }

        _agent.isStopped = false;
        return true;
    }

    #endregion

    #region WANDER

    /// <summary>
    /// 근무하지 않는 동안(퇴근/휴식/자리 대기) 제자리에 굳어 있지 않도록 주변을 어슬렁거린다.
    /// 씬에 따로 붙여두던 DebugNav가 하던 일을 상태 행동으로 가져온 것이다.
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_wanderIntervalMin, _wanderIntervalMax));

            //자리로 가는 중이거나 근무중이면 목적지를 덮어쓰면 안 된다
            if (_state != State.OffDuty && _state != State.Resting)
            {
                continue;
            }

            Wander();
        }
    }

    /// <summary>반경 안의 아무 데나 한 곳을 골라 걸어간다.</summary>
    private void Wander()
    {
        //NavMesh 위에 없으면 SetDestination이 에러만 뱉는다 (스폰 직후/베이크 안 된 바닥)
        if (!_agent.isOnNavMesh)
        {
            return;
        }

        Vector2 offset = Random.insideUnitCircle * _wanderRadius;
        Vector3 target = transform.position + new Vector3(offset.x, 0f, offset.y);

        //벽 너머처럼 갈 수 없는 지점을 찍으면 근처의 걸어갈 수 있는 곳으로 스냅한다
        if (!NavMesh.SamplePosition(target, out NavMeshHit hit, _wanderRadius, NavMesh.AllAreas))
        {
            return;
        }

        _agent.SetDestination(hit.position);
    }

    #endregion

    #region DOOR

    /// <summary>
    /// 출퇴근길에 문 앞을 지나면 그 문을 열어둔다.
    ///
    /// 문에 트리거를 달아두는 편이 자연스러워 보이지만 그러면 안 된다. ArriveHome() 뒤에 직원은
    /// 문 자리에 그대로 서서 OffDuty로 대기하기 때문에, 트리거 안에 사람이 계속 있는 셈이 되어
    /// 문이 밤새 열린 채로 있는다. 그래서 "걸어가는 중일 때만 연다"로 본다 -
    /// 이렇게 하면 출근길(GoingToDesk)과 퇴근길(GoingHome)이 규칙 하나로 같이 처리되고,
    /// 문 앞에 서 있기만 한 직원은 문을 열지 않는다.
    ///
    /// 여닫는 동작 자체와 여러 명이 같이 드나들 때의 처리는 Door가 맡는다. 여기서는 잡고 놓기만 한다.
    /// </summary>
    private void TickDoor()
    {
        PlaceableObject near = FindDoorWithinReach();

        //잡고 있던 문 그대로면 아무것도 하지 않는다. Door 컴포넌트를 다시 찾지 않으려고 배치물로 비교한다.
        if (near == _heldDoorObject)
        {
            return;
        }

        ReleaseDoor();

        if (near == null)
        {
            return;
        }

        _heldDoorObject = near;
        _heldDoor = Door.Of(near);

        if (_heldDoor != null)
        {
            _heldDoor.Hold();
        }
    }

    /// <summary>지금 열어야 할 문. 걸어가는 중이 아니거나 가장 가까운 문이 멀면 null.</summary>
    private PlaceableObject FindDoorWithinReach()
    {
        if (_state != State.GoingToDesk && _state != State.GoingHome)
        {
            return null;
        }

        PlaceableObject nearest = _workstationManagerSO.GetNearestDoor(transform.position);

        if (nearest == null)
        {
            return null;
        }

        //HasArrived와 같은 이유로 y는 뺀다 - 문틀 피벗 높이 때문에 거리가 부풀지 않도록
        Vector3 offset = nearest.transform.position - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= _doorOpenDistance * _doorOpenDistance ? nearest : null;
    }

    /// <summary>잡고 있던 문을 놓는다. 실제로 닫히기까지는 Door가 조금 더 기다려준다.</summary>
    private void ReleaseDoor()
    {
        if (_heldDoor != null)
        {
            _heldDoor.Release();
        }

        _heldDoor = null;
        _heldDoorObject = null;
    }

    #endregion

    private void Update()
    {
        TickStamina();
        TickDoor();

        if (_state != State.GoingToDesk && _state != State.GoingHome)
        {
            return;
        }

        if (!HasArrived())
        {
            return;
        }

        if (_state == State.GoingToDesk)
        {
            ArriveAtDesk();
        }
        else
        {
            ArriveHome();
        }
    }

    /// <summary>근무 중이면 체력을 소모하고, 그 외(이동/대기/휴식)에는 회복시킨다.</summary>
    private void TickStamina()
    {
        if (_employee == null)
        {
            return;
        }

        bool isDraining = IsAtDesk(_state);

        //이미 한계치(0 또는 최대)라면 계산할 필요 없음 - 불필요한 서버 쓰기 방지
        if (isDraining && _employee.Stamina <= 0)
        {
            _staminaAccumulator = 0f;
            return;
        }

        if (!isDraining && _employee.Stamina >= _employee.Max_Stamina)
        {
            _staminaAccumulator = 0f;
            return;
        }

        float ratePerMinute = isDraining ? -_staminaDrainPerMinute : _staminaRecoverPerMinute;
        _staminaAccumulator += ratePerMinute / 60f * Time.deltaTime;

        int wholePoints = (int)_staminaAccumulator;
        if (wholePoints == 0)
        {
            return;
        }

        _staminaAccumulator -= wholePoints;

        //toServer:false - 여기는 몇 초에 한 번씩 도는 자리다. 1점 바뀔 때마다 쓰면 직원 수만큼 초당 쓰기가 된다.
        //날짜를 틱마다 쓰지 않고 SaveDate()로 몰아 쓰는 것과 같은 이유로,
        //상태가 바뀌는 순간에만 SaveStamina()로 남긴다.
        _employee.SetStamina(_employee.Stamina + wholePoints, false);
    }

    /// <summary>지금 체력을 서버에 남긴다. 상태가 바뀌는 순간(출근/퇴근/휴식)에만 부른다.</summary>
    private void SaveStamina()
    {
        if (_employee == null)
        {
            return;
        }

        _employee.SetStamina(_employee.Stamina);
    }
}

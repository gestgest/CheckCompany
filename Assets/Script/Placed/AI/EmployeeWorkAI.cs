using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 직원이 근무시간(Employee._WorkTime)에 맞춰 출근하고, 자리에서 일하고, 퇴근한다.
/// 체력이 바닥나면 근무시간이라도 자리에서 일어나 쉰다.
///
///   OffDuty ──출근시간──> GoingToDesk ──도착──> Working
///      ↑                                          │
///      └──── 퇴근시간 ────────────────────────────┘
///                 체력 0 ↓        ↑ 회복
///                        Resting ─┘
///
/// 예전에는 스폰되자마자 빈 책상으로 직진해서 도착하면 영원히 굳어 있었다(Working이 종착역).
/// 그래서 새벽 3시에도 출근해 있고, 체력이 0이 돼도 계속 일했다.
/// 이제 Working은 종착역이 아니고, 이 직원이 움직이는 이유는 전부 시간이나 체력에서 나온다.
///
/// EmployeeObjectSystem이 생성 직후 Init(employeeId)를 호출해줘야 한다.
/// </summary>
public class EmployeeWorkAI : MonoBehaviour
{
    private enum State
    {
        OffDuty,     //퇴근 상태. 배정받을 자리가 없어 대기하는 동안도 여기에 머문다
        GoingToDesk, //자리로 이동중
        Working,     //자리에서 근무중
        Resting      //체력이 바닥나 쉬는 중 (근무시간이어도)
    }

    [SerializeField] private WorkstationManagerSO _workstationManagerSO;
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;
    [SerializeField] private float _arriveDistance = 0.15f;
    [SerializeField] private float _seatNavMeshSampleRadius = 2.0f; //SeatPoint 주변에서 NavMesh를 찾을 반경

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

    private NavMeshAgent _agent;

    private State _state = State.OffDuty;
    private Transform _seat;

    //자리로 출발할 때의 SeatPoint 위치. 책상이 옮겨진 걸 알아채는 기준점이다.
    private Vector3 _seatAnchor;

    private int _employeeId;
    private Employee _employee;
    private float _staminaAccumulator;

    /// <summary>이 오브젝트가 대표하는 Employee의 ID. EmployeeObjectSystem에서 생성 직후 호출.</summary>
    public void Init(int employeeId)
    {
        _employeeId = employeeId;
        _employee = _employeeManagerSO.GetEmployeeById(employeeId);
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(DecisionRoutine());
        StartCoroutine(WanderRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

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

        _state = State.GoingToDesk;
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
            return assigned.GetSeatPoint();
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

        Transform seat = assigned.GetSeatPoint();

        if (seat != _seat)
        {
            return true;
        }

        //책상을 옮기면 SeatPoint도 같이 움직인다. 앉은 자세 그대로 끌려가면 안 되므로 다시 걸어간다.
        return (seat.position - _seatAnchor).sqrMagnitude > _seatMovedThreshold * _seatMovedThreshold;
    }

    private bool MoveToSeat()
    {
        if (!ResumeAgent())
        {
            return false;
        }

        //SeatPoint(또는 그 대체값인 오브젝트 자신의 위치)가 책상 모델 안쪽 등 NavMesh가 없는 지점일 수 있으므로,
        //주변 반경 안에서 실제로 걸어갈 수 있는 가장 가까운 지점으로 스냅한다.
        if (!NavMesh.SamplePosition(_seat.position, out NavMeshHit hit, _seatNavMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning(
                $"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}' 주변 {_seatNavMeshSampleRadius}m 안에 NavMesh가 없습니다. " +
                "SeatPoint 위치를 책상 옆 바닥(NavMesh가 베이크된 곳)으로 옮겨주세요."
            );
            return false;
        }

        if (!_agent.SetDestination(hit.position))
        {
            Debug.LogWarning($"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}'까지 가는 경로를 계산하지 못했습니다.");
            return false;
        }

        return true;
    }

    private void ArriveAtDesk()
    {
        _state = State.Working;

        //도착 후에도 NavMeshAgent를 멈추지 않으면 계속 같은 목적지로 미세 보정을 시도해서
        //(특히 근처에 다른 직원이 있으면 서로 밀어내는 obstacle avoidance 때문에) 제자리에서 왔다갔다 떨리게 된다.
        StopAgent();

        transform.rotation = _seat.rotation;

        //근무/휴식이 바뀌는 순간의 체력만 서버에 남긴다 (TickStamina 주석 참고)
        SaveStamina();

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}'에 도착해서 근무 시작 (Working)");

        //TODO: Animator Controller에 근무 애니메이션 상태/파라미터가 추가되면 여기서 재생
    }

    /// <summary>퇴근한다.</summary>
    private void LeaveWork()
    {
        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 퇴근 (OffDuty)");
        EnterOffDuty();
    }

    /// <summary>체력이 바닥나 자리에서 일어난다. 근무시간이어도 일을 멈춘다.</summary>
    private void EnterRest()
    {
        if (_state == State.Resting)
        {
            return;
        }

        _state = State.Resting;
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

        _state = State.OffDuty;
        ResumeAgent();
        SaveStamina();

        //자리 배정은 일부러 남겨둔다.
        //플레이어가 UI로 "이 자리에 이 직원"을 꽂아두는데, 퇴근할 때마다 ReleaseSeat을 부르면
        //그 배정이 매일 밤 지워지고 다음 날 아무나 앉게 된다. 퇴근은 자리를 비우는 것이지 자리를 잃는 게 아니다.
        //진짜로 배정을 지우는 건 퇴사(OnDestroy)와 UI의 빼기 버튼(ReleaseSeatOf)뿐이다.
    }

    /// <summary>Agent를 세운다. NavMesh 밖이면 아무것도 하지 않는다(에러만 뱉는다).</summary>
    private void StopAgent()
    {
        if (_agent == null || !_agent.isOnNavMesh)
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
        if (_agent == null || !_agent.isOnNavMesh)
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

    private void Update()
    {
        TickStamina();

        if (_state != State.GoingToDesk)
        {
            return;
        }

        if (_agent.pathPending || _agent.remainingDistance > _arriveDistance)
        {
            return;
        }

        ArriveAtDesk();
    }

    /// <summary>근무 중이면 체력을 소모하고, 그 외(이동/대기/휴식)에는 회복시킨다.</summary>
    private void TickStamina()
    {
        if (_employee == null)
        {
            return;
        }

        bool isDraining = _state == State.Working;

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

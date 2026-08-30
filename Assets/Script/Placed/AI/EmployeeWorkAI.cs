using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 직원이 빈 책상(워크스테이션)을 배정받아 이동하고, 도착하면 근무 상태로 전환한다.
/// 책상을 못 받은 동안에는 주변을 어슬렁거린다(기존 DebugNav가 하던 일).
/// EmployeeObjectSystem이 생성 직후 Init(employeeId)를 호출해줘야 한다.
/// </summary>
public class EmployeeWorkAI : MonoBehaviour
{
    private enum State
    {
        Idle,         //배정받을 책상을 기다리는 중
        MovingToDesk, //책상으로 이동중
        Working       //책상에서 근무중
    }

    [SerializeField] private WorkstationManagerSO _workstationManagerSO;
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;
    [SerializeField] private float _arriveDistance = 0.15f;
    [SerializeField] private float _retryInterval = 3.0f; //빈 책상이 없을 때 재시도 간격
    [SerializeField] private float _seatNavMeshSampleRadius = 2.0f; //SeatPoint 주변에서 NavMesh를 찾을 반경

    [Header("Idle Wander")]
    [SerializeField] private float _wanderRadius = 3.0f;        //현재 자리에서 돌아다닐 반경
    [SerializeField] private float _wanderIntervalMin = 1.0f;   //다음 목적지를 고르기까지 최소 대기
    [SerializeField] private float _wanderIntervalMax = 5.0f;   //최대 대기

    [Header("Stamina")]
    [SerializeField] private float _staminaDrainPerMinute = 6f;    //근무 중 분당 체력 소모량
    [SerializeField] private float _staminaRecoverPerMinute = 12f; //비근무(이동/대기) 중 분당 체력 회복량

    private NavMeshAgent _agent;

    private State _state = State.Idle;
    private Transform _seat;
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
        //todo 일단 막아놓음
        //StartCoroutine(ClaimDeskRoutine());
        //StartCoroutine(WanderRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (_seat != null)
        {
            _workstationManagerSO.ReleaseSeat(_employeeId);
        }
    }

    /// <summary>
    /// 책상을 기다리는 동안 제자리에 굳어 있지 않도록 주변을 어슬렁거린다.
    /// 씬에 따로 붙여두던 DebugNav가 하던 일을 Idle 상태의 행동으로 가져온 것이다.
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_wanderIntervalMin, _wanderIntervalMax));

            //책상으로 가는 중이거나 근무중이면 목적지를 덮어쓰면 안 된다
            if (_state != State.Idle)
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

    /// <summary>빈 책상이 생길 때까지 주기적으로 재시도한다.</summary>
    private IEnumerator ClaimDeskRoutine()
    {
        while (_seat == null)
        {
            _seat = _workstationManagerSO.RequestSeat(_employeeId);

            if (_seat != null)
            {
                MoveToSeat();
                yield break;
            }

            Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : 빈 책상이 없어 대기중 ({_retryInterval}초 후 재시도)");
            yield return new WaitForSeconds(_retryInterval);
        }
    }

    private void MoveToSeat()
    {
        _state = State.MovingToDesk;

        //SeatPoint(또는 그 대체값인 오브젝트 자신의 위치)가 책상 모델 안쪽 등 NavMesh가 없는 지점일 수 있으므로,
        //주변 반경 안에서 실제로 걸어갈 수 있는 가장 가까운 지점으로 스냅한다.
        if (!NavMesh.SamplePosition(_seat.position, out NavMeshHit hit, _seatNavMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning(
                $"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}' 주변 {_seatNavMeshSampleRadius}m 안에 NavMesh가 없습니다. " +
                "SeatPoint 위치를 책상 옆 바닥(NavMesh가 베이크된 곳)으로 옮겨주세요."
            );
            return;
        }

        if (!_agent.SetDestination(hit.position))
        {
            Debug.LogWarning($"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}'까지 가는 경로를 계산하지 못했습니다.");
        }
    }

    private void Update()
    {
        // todo : 일단 막아놓음
        // TickStamina();
        //
        // if (_state != State.MovingToDesk)
        // {
        //     return;
        // }
        //
        // if (_agent.pathPending || _agent.remainingDistance > _arriveDistance)
        // {
        //     return;
        // }
        //
        // ArriveAtDesk();
    }

    /// <summary>근무 중이면 체력을 소모하고, 그 외(이동/대기)에는 회복시킨다.</summary>
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
        _employee.SetStamina(_employee.Stamina + wholePoints);
    }

    private void ArriveAtDesk()
    {
        _state = State.Working;

        //도착 후에도 NavMeshAgent를 멈추지 않으면 계속 같은 목적지로 미세 보정을 시도해서
        //(특히 근처에 다른 직원이 있으면 서로 밀어내는 obstacle avoidance 때문에) 제자리에서 왔다갔다 떨리게 된다.
        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        transform.rotation = _seat.rotation;

        Debug.Log($"[EmployeeWorkAI] employee {_employeeId} : '{_seat.name}'에 도착해서 근무 시작 (Working)");

        //TODO: Animator Controller에 근무 애니메이션 상태/파라미터가 추가되면 여기서 재생
    }
}

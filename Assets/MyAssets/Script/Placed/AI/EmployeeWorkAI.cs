using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 직원이 빈 책상(워크스테이션)을 배정받아 이동하고, 도착하면 근무 상태로 전환한다.
/// 기존 DebugNav(랜덤 배회 테스트 스크립트)를 대체한다.
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
        StartCoroutine(ClaimDeskRoutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (_seat != null)
        {
            _workstationManagerSO.ReleaseSeat(_employeeId);
        }
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
        TickStamina();

        if (_state != State.MovingToDesk)
        {
            return;
        }

        if (_agent.pathPending || _agent.remainingDistance > _arriveDistance)
        {
            return;
        }

        ArriveAtDesk();
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
        transform.rotation = _seat.rotation;

        //TODO: Animator Controller에 근무 애니메이션 상태/파라미터가 추가되면 여기서 재생
    }
}

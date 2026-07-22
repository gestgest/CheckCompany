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
    [SerializeField] private float _arriveDistance = 0.15f;
    [SerializeField] private float _retryInterval = 3.0f; //빈 책상이 없을 때 재시도 간격

    private NavMeshAgent _agent;

    private State _state = State.Idle;
    private Transform _seat;
    private int _employeeId;

    /// <summary>이 오브젝트가 대표하는 Employee의 ID. EmployeeObjectSystem에서 생성 직후 호출.</summary>
    public void Init(int employeeId)
    {
        _employeeId = employeeId;
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
                _agent.SetDestination(_seat.position);
                _state = State.MovingToDesk;
                yield break;
            }

            yield return new WaitForSeconds(_retryInterval);
        }
    }

    private void Update()
    {
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

    private void ArriveAtDesk()
    {
        _state = State.Working;
        transform.rotation = _seat.rotation;

        //TODO: Animator Controller에 근무 애니메이션 상태/파라미터가 추가되면 여기서 재생
    }
}

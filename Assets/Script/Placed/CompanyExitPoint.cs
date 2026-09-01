using UnityEngine;

/// <summary>
/// 퇴근할 때 직원이 걸어나갈 지점(회사 출입구). 씬에서 문 앞 등 원하는 위치에 빈 오브젝트로
/// 하나 배치해두면 된다. 직원 프리팹은 씬 오브젝트를 직접 참조할 수 없어서, RegisterWorkstation과
/// 같은 방식으로 이 오브젝트가 스스로 WorkstationManagerSO에 등록한다.
///
/// 씬에 이 컴포넌트가 하나도 없으면 EmployeeWorkAI는 예전처럼 그 자리에서 바로 퇴근 처리한다.
/// </summary>
public class CompanyExitPoint : MonoBehaviour
{
    [SerializeField] private WorkstationManagerSO _workstationManagerSO;

    private void OnEnable()
    {
        if (_workstationManagerSO != null)
        {
            _workstationManagerSO.RegisterExitPoint(transform);
        }
    }

    private void OnDisable()
    {
        if (_workstationManagerSO != null)
        {
            _workstationManagerSO.UnregisterExitPoint(transform);
        }
    }
}

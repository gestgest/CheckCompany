using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI DateText;

    [Header("직원 수 (n/m) - n: 고용된 직원, m: 놓인 책상 수")]
    [SerializeField] private TextMeshProUGUI employeeCountText;
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;
    [SerializeField] private WorkstationManagerSO _workstationManagerSO;

    //직원이 뽑히거나 나갈 때(EmployeeManagerSO.CreateEmployee/RemoveEmployee) 울린다.
    //책상 수가 바뀌는 건 알려주는 채널이 따로 없어서, 그쪽은 SetDateUI()가 도는 주기(게임 시계 틱)에 묻어간다.
    [SerializeField] private BoolEventChannelSO _isChangedEmployeePanelEventChannelSO;

    private void OnEnable()
    {
        if (_isChangedEmployeePanelEventChannelSO != null)
        {
            _isChangedEmployeePanelEventChannelSO._onEventRaised += OnEmployeeListChanged;
        }
    }

    private void OnDisable()
    {
        if (_isChangedEmployeePanelEventChannelSO != null)
        {
            _isChangedEmployeePanelEventChannelSO._onEventRaised -= OnEmployeeListChanged;
        }
    }

    private void OnEmployeeListChanged(bool _)
    {
        RefreshEmployeeCount();
    }

    public void SetMoneyText(long value)
    {
        moneyText.text = value.ToString();
    }

    public void SetDateText(Date value)
    {
        DateText.text = value.ToString();

        //책상 수(m)는 이벤트가 없어 여기(날짜가 갱신될 때마다)에 묻어서 같이 새로고침한다.
        //게임 시계가 게임 1시간마다 이걸 부르므로 몇 초 안에는 항상 맞는 값으로 돌아온다.
        RefreshEmployeeCount();
    }

    /// <summary>고용된 직원 수 / 놓인 책상 수를 "n/m"으로 표시한다.</summary>
    public void RefreshEmployeeCount()
    {
        if (employeeCountText == null || _employeeManagerSO == null || _workstationManagerSO == null)
        {
            return;
        }

        employeeCountText.text = $"{_employeeManagerSO.GetEmployees().Count}/{_workstationManagerSO.WorkstationCount}";
    }
}

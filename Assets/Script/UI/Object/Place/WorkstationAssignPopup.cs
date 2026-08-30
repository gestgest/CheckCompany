using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 배치된 자리(워크스테이션)를 짧게 누르면 뜨는 창. 그 자리에 앉힐 직원을 고른다.
///
/// 흐름 : PlacedObjectInput 탭 -> _tapEvent -> Open(workstation)
///                                          -> 직원 칸 클릭 -> WorkstationManagerSO.AssignEmployee()
///
/// DeleteConfirmPopup과 같은 방식이다 - 이 스크립트가 붙은 오브젝트는 항상 켜져 있고,
/// 실제로 켜고 끄는 것은 _root(딤 배경)다. PanelManager의 인덱스 트리에는 끼지 않는다.
/// </summary>
public class WorkstationAssignPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _root; //딤 배경 + 창 전체
    [SerializeField] private TextMeshProUGUI _titleText;

    //직원 칸이 쌓일 자리(ScrollView의 Content)
    [SerializeField] private Transform _elementParent;
    [SerializeField] private GameObject _elementPrefab;

    //지금 앉아있는 직원을 빼는 버튼. 비어 있는 자리에서는 꺼둔다.
    [SerializeField] private GameObject _releaseButton;

    [Header("Manager")]
    [SerializeField] private WorkstationManagerSO _workstationManagerSO;
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [Header("Listening to Event")]
    [SerializeField] private PlaceableObjectEventChannelSO _tapEvent;

    //배치/이동이 시작되면(오브젝트를 손에 들면) 이 창이 떠 있을 이유가 없다
    [SerializeField] private BoolEventChannelSO _isHandlingEvent;

    private readonly List<WorkstationEmployeeElement> _elements = new List<WorkstationEmployeeElement>();

    private PlaceableObject _workstation;

    private void Awake()
    {
        Close();
    }

    private void OnEnable()
    {
        if (_tapEvent != null)
        {
            _tapEvent._onEventRaised += Open;
        }

        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised += OnHandlingChanged;
        }
    }

    private void OnDisable()
    {
        if (_tapEvent != null)
        {
            _tapEvent._onEventRaised -= Open;
        }

        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised -= OnHandlingChanged;
        }
    }

    /// <summary>탭한 오브젝트가 근무 자리면 창을 연다. 그냥 장식(화분 등)이면 무시한다.</summary>
    public void Open(PlaceableObject workstation)
    {
        if (workstation == null || !workstation.IsWorkstation)
        {
            return;
        }

        _workstation = workstation;

        SetRootActive(true);
        Rebuild();
    }

    //닫기 버튼의 OnClick
    public void Close()
    {
        _workstation = null;
        SetRootActive(false);
    }

    /// <summary>지금 앉아있는 직원을 자리에서 뺀다. 빼기 버튼의 OnClick.</summary>
    public void Release()
    {
        if (_workstation == null)
        {
            return;
        }

        _workstationManagerSO.ReleaseSeatOf(_workstation);
        Rebuild();
    }

    /// <summary>직원 칸이 눌렸을 때. 칸이 직접 부른다.</summary>
    public void Assign(int employeeId)
    {
        if (_workstation == null)
        {
            return;
        }

        _workstationManagerSO.AssignEmployee(employeeId, _workstation);

        //배정하면 다른 자리에 앉아있던 상태가 바뀔 수 있으니 목록을 다시 그린다
        Rebuild();
    }

    /// <summary>직원 목록을 지우고 지금 상태로 다시 만든다.</summary>
    private void Rebuild()
    {
        if (_elementPrefab == null || _elementParent == null
            || _workstationManagerSO == null || _employeeManagerSO == null)
        {
            Debug.LogError(
                "[WorkstationAssignPopup] _elementPrefab / _elementParent / 매니저 두 개를 인스펙터에서 넣어주세요.",
                this);
            return;
        }

        ClearElements();

        int seatedId = _workstationManagerSO.GetAssignedEmployeeId(_workstation);

        if (_titleText != null)
        {
            //직원이 퇴사했는데 배정이 남아있는 경우가 있어 이름이 null일 수 있다.
            //TMP에 null을 넣으면 터지므로 반드시 빈 문자열로 막는다.
            Employee seated = seatedId == WorkstationManagerSO.NoEmployee
                ? null
                : _employeeManagerSO.GetEmployeeById(seatedId);

            _titleText.text = seated == null ? "비어 있는 자리" : seated.Name;
        }

        if (_releaseButton != null)
        {
            _releaseButton.SetActive(seatedId != WorkstationManagerSO.NoEmployee);
        }

        List<Employee> employees = _employeeManagerSO.GetEmployees();

        //직원 데이터가 아직 안 왔을 수 있다
        if (employees == null || employees.Count == 0)
        {
            //목록이 비면 창만 덩그러니 뜨는데, 배선이 틀린 건지 진짜 직원이 없는 건지 구분이 안 된다
            Debug.LogWarning(
                "[WorkstationAssignPopup] 배정할 직원이 없습니다. " +
                "고용한 직원이 0명이거나, 서버에서 직원 목록을 아직 못 받았습니다.",
                this);
            return;
        }

        for (int i = 0; i < employees.Count; i++)
        {
            CreateElement(employees[i], seatedId);
        }
    }

    private void CreateElement(Employee employee, int seatedId)
    {
        GameObject obj = Instantiate(_elementPrefab, _elementParent);
        WorkstationEmployeeElement element = obj.GetComponent<WorkstationEmployeeElement>();

        if (element == null)
        {
            Debug.LogError(
                "[WorkstationAssignPopup] _elementPrefab에 WorkstationEmployeeElement가 없습니다.",
                _elementPrefab);
            Destroy(obj);
            return;
        }

        //이 직원이 지금 어디에 앉아 있는지. 다른 자리에 앉아있는 직원을 데려오면 그쪽이 비므로 미리 알려준다.
        PlaceableObject current = _workstationManagerSO.GetAssignedWorkstation(employee.ID);

        element.Init(
            employee,
            _employeeManagerSO.GetIcon(employee.AssetID),
            employee.ID == seatedId,
            current != null,
            this);

        _elements.Add(element);
    }

    private void ClearElements()
    {
        for (int i = 0; i < _elements.Count; i++)
        {
            if (_elements[i] != null)
            {
                Destroy(_elements[i].gameObject);
            }
        }

        _elements.Clear();
    }

    /// <summary>오브젝트를 손에 들면(배치/이동 시작) 창을 닫는다.</summary>
    private void OnHandlingChanged(bool isHandling)
    {
        if (isHandling)
        {
            Close();
        }
    }

    private void SetRootActive(bool isActive)
    {
        if (_root == null)
        {
            Debug.LogError("[WorkstationAssignPopup] _root가 연결되지 않았습니다.", this);
            return;
        }

        _root.SetActive(isActive);
    }
}

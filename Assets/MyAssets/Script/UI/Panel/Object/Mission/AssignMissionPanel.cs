using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssignMissionPanel : MiniPanel
{
    [SerializeField] private GameObject _assignableEmployeeElementPrefab; //employeess
    [SerializeField] private Transform _defaultEmployeeParent;

    [Header("Manager")]
    [SerializeField] private EmployeeManagerSO _employeeManager;
    [SerializeField] private CreateMissionManagerSO _createMissionManager;

    [Space]
    [Header("Listening to Events")]
    [SerializeField] private BoolEventChannelSO _isChangedEmployeeEventChannelSO; 
    [SerializeField] private VoidEventChannelSO _changedAssignedEmployeeEventChannel;
    //isChanged employee

    //pooling employee max count = 4
    [SerializeField] private List<AssignEmployeeElement> _assignedEmployeeElements;
    private List<AssignEmployeeElement> _assignableEmployeeElements = new List<AssignEmployeeElement>();


    //private Mission mission;
    private bool _isChanged = true;


    protected override void Start()
    {
        base.Start();
        _isChangedEmployeeEventChannelSO._onEventRaised += SetIsChanged;
        _changedAssignedEmployeeEventChannel._onEventRaised += SetUI;
    }

    private void OnDestroy()
    {
        _isChangedEmployeeEventChannelSO._onEventRaised -= SetIsChanged;
        _changedAssignedEmployeeEventChannel._onEventRaised -= SetUI;
    }


    public void Init()
    {
        //this.mission = mission;
        //assignedEmployeeSize = mission.assignedEmployeeSize 할당해야할 미션
        //default => 1
        _createMissionManager.SetAssignableEmployeeSize(1);

        //이후 editMissionPanel때문에 미션에 할당된 직원을 가져와야함

        SetUI();
    }


    void OnEnable()
    {
        //employees => if employees changed
        if (_isChanged)
        {
            DeleteAllAssignableEmployeeElements();
            CreateAssignableEmployeeElements();
        }
    }

    private void SetUI()
    {
        DeleteAllAssignedEmployeeElements();
        CreateAssignedEmployeeElements();
        SelectedEmployees();
    }



    void DeleteAllAssignedEmployeeElements()
    {
        //assignedEmployees => debug one ~ two employee, but later 1 instead of mission
        for (int i = 0; i < _assignedEmployeeElements.Count; i++)
        {
            _assignedEmployeeElements[i].IsSelected = false;
            _assignedEmployeeElements[i].gameObject.SetActive(false);
        }
    }


    //직원 정보를 가져와야하는데
    void CreateAssignedEmployeeElements()
    {
        //assignedEmployees => debug one ~ two employee, but later 1 instead of mission
        for (int i = 0; i < _createMissionManager.GetAssignableEmployeeSize(); i++)
        {
            _assignedEmployeeElements[i].gameObject.SetActive(true);
            //_assignedEmployeeElements[i].IsSelected = false;
        }

        List<int> ids = _createMissionManager.GetRefEmployeesID();
        
        //assignedEmployee
        for (int i = 0; i < ids.Count; i++)
        {
            int index = _employeeManager.Search_Employee_Index(ids[i]);
            Employee employee = _employeeManager.GetEmployee(index);

            //icon setting
            _assignedEmployeeElements[i].SetEmployee(employee, _employeeManager.GetIcon(employee.AssetID));
            _assignedEmployeeElements[i].IsSelected = true;
        }
    }


    void DeleteAllAssignableEmployeeElements()
    {
        foreach (AssignEmployeeElement element in _assignableEmployeeElements)
        {
            Destroy(element.gameObject);
        }
        
        _assignableEmployeeElements.Clear();
    }

    
    void CreateAssignableEmployeeElements()
    {
        //default : not select employee
        foreach (Employee e in _employeeManager.GetEmployees())
        {
            GameObject obj = Instantiate(_assignableEmployeeElementPrefab, _defaultEmployeeParent);
            AssignEmployeeElement aee = obj.GetComponent<AssignEmployeeElement>();
            aee.SetEmployee(e);

            _assignableEmployeeElements.Add(aee);
        }
    }
    void SelectedEmployees()
    {
        foreach(AssignEmployeeElement element in _assignableEmployeeElements)
        {
            element.IsSelected = false;
        }

        foreach(int id in _createMissionManager.GetRefEmployeesID())
        {
            int index = _employeeManager.Search_Employee_Index(id); //log2
            _assignableEmployeeElements[index].IsSelected = true; 
        }
    }


    private void SetIsChanged(bool isChanged)
    {
        _isChanged = isChanged;
    }


    //EmployeeElement? 또 다른 버전

    //start => employees 가져오기 + 

}

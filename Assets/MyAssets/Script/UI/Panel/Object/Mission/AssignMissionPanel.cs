using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssignMissionPanel : MiniPanel
{
    [SerializeField] private GameObject _assignableEmployeeElementPrefab; //employeess
    [SerializeField] private Transform _defaultEmployeeParent;
    
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [Header("Listening to Events")]
    [SerializeField] private BoolEventChannelSO _isChangedAssignEmployeePanelEventChannelSO;

    //pooling employee max count = 4
    [SerializeField] private List<AssignEmployeeElement> _assignedEmployeeElements;
    [SerializeField] private List<Employee> _assignedEmployees = new List<Employee>();
    private List<AssignEmployeeElement> _assignableEmployeeElements = new List<AssignEmployeeElement>();

    private int assignedEmployeeSize;

    //private Mission mission;
    private bool _isChanged = true;


    protected override void Start()
    {
        base.Start();
        _isChangedAssignEmployeePanelEventChannelSO._onEventRaised += SetIsChanged;
    }

    private void OnDestroy()
    {
        _isChangedAssignEmployeePanelEventChannelSO._onEventRaised -= SetIsChanged;
    }

    
    public void Init()
    {
        //this.mission = mission;

        _assignedEmployees.Clear();
        //assignedEmployeeSize = mission.assignedEmployeeSize
        //default => 1
        assignedEmployeeSize = 1;
        

        DeleteAllAssignedEmployeeElements();
        CreateAssignedEmployeeElements();
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

    void DeleteAllAssignedEmployeeElements()
    {
        //assignedEmployees => debug one ~ two employee, but later 1 instead of mission
        for (int i = 0; i < _assignedEmployeeElements.Count; i++)
        {
            _assignedEmployeeElements[i].gameObject.SetActive(false);
        }
    }

    void CreateAssignedEmployeeElements()
    {
        //assignedEmployees => debug one ~ two employee, but later 1 instead of mission
        for (int i = 0; i < assignedEmployeeSize; i++)
        {
            _assignedEmployeeElements[i].gameObject.SetActive(true);
        }

        for(int i = 0; i < _assignedEmployees.Count; i++)
        {
            _assignedEmployeeElements[i].SetEmployee(_assignedEmployees[i]);
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
        foreach (Employee e in _employeeManagerSO.GetEmployees())
        {
            GameObject obj = Instantiate(_assignableEmployeeElementPrefab, _defaultEmployeeParent);
            
            _assignableEmployeeElements.Add(obj.GetComponent<AssignEmployeeElement>());
        }
    }

    private void SetIsChanged(bool isChanged)
    {
        _isChanged = isChanged;
    }
    
    //EmployeeElement? 또 다른 버전

    //start => employees 가져오기 + 
    
    


}

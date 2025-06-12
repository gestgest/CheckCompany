using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssignMissionPanel : MiniPanel
{
    private TextMeshProUGUI title;
    [SerializeField] private GameObject _assignEmployeeElementPrefab;
    [SerializeField] private Transform _selectedEmployeeParent;
    [SerializeField] private Transform _defaultEmployeeParent;
    
    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [Header("Listening to Events")]
    [SerializeField] private BoolEventChannelSO _isChangedAssignEmployeePanelEventChannelSO;

    private List<AssignEmployeeElement> _assignEmployeeElements = new List<AssignEmployeeElement>();
    private Mission mission;
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
    
    void OnEnable()
    {
        if (_isChanged)
        {
            DeleteAllAssignEmployeeElements();
            CreateAssignEmployeeElements();
        }
    }
    //자작 직원 icons

    void DeleteAllAssignEmployeeElements()
    {
        //default : not select employee
        foreach (AssignEmployeeElement element in _assignEmployeeElements)
        {
            Destroy(element.gameObject);
        }
        
        _assignEmployeeElements.Clear();
    }
    void CreateAssignEmployeeElements()
    {
        //default : not select employee
        foreach (Employee e in _employeeManagerSO.GetEmployees())
        {
            GameObject obj = Instantiate(_assignEmployeeElementPrefab, _defaultEmployeeParent);
            
            _assignEmployeeElements.Add(obj.GetComponent<AssignEmployeeElement>());
        }
    }

    private void SetIsChanged(bool isChanged)
    {
        _isChanged = isChanged;
    }
    
    //EmployeeElement? 또 다른 버전

    //start => employees 가져오기 + 
    
    


}

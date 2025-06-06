using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//sub
public class EmployeePanel : Panel
{
    [SerializeField] private GameObject employeeElementPrefab;

    //넣으면 안됨
    //[SerializeField] private GameObject employeeObjectPrefab;
    //List<GameObject> employeeObjects;

    [SerializeField] private GameObject element_parent;
    List<GameObject> employeeElementObjects;

    [Header("Manager")]
    [SerializeField] private EmployeeManagerSO _employeeManager;

    [Space]
    [Header("Listening to channels")]
    [SerializeField] private BoolEventChannelSO _isChangedEmployeePanelEventChannelSO;

    bool isChanged = false;

    protected override void Start()
    {
        base.Start();
        _isChangedEmployeePanelEventChannelSO._onEventRaised += SetIsChanged;

    }

    private void OnEnable()
    {
        SetUI();
    }

    private void OnDestroy()
    {
        _isChangedEmployeePanelEventChannelSO._onEventRaised -= SetIsChanged;
    }

    private void SetUI()
    {
        if (isChanged)
        {
            RemoveAllemployees();
            CreateEmployees();
        }
    }

    private void SetIsChanged(bool isChanged)
    {
        this.isChanged = isChanged;
        SetUI();
    }

    private void RemoveAllemployees()
    {
        if (employeeElementObjects == null)
        {
            employeeElementObjects = new List<GameObject>();
            return;
        }

        for (int i = 0; i < employeeElementObjects.Count; i++)
        {
            Destroy(employeeElementObjects[i]);
        }
        employeeElementObjects.Clear();
    }

    private void CreateEmployees()
    {
        foreach (Employee e in _employeeManager.GetEmployees())
        {
            CreateEmployeeElementUI(e);
        }
    }

    //show함수, index를 employees기준으로 하면 안된다. => 나중에 전체 ID로 바꿀 예정
    private void CreateEmployeeElementUI(Employee e)
    {
        GameObject employeeObject = Instantiate(employeeElementPrefab, Vector3.zero, Quaternion.identity);
        EmployeeElement employeeContent = employeeObject.GetComponent<EmployeeElement>();
        Button button = employeeObject.GetComponent<Button>();

        employeeContent.SetEmployee(e._EmployeeSO.GetIcon(), e.Name, e.CareerPeriod, 1, e.Salary, e.ID);
        employeeElementObjects.Add(employeeObject);
        employeeObject.transform.SetParent(element_parent.transform);

        //버튼 추가
        button.onClick.AddListener(() => { _employeeManager.ShowEmployeeStatusWindow(e.ID); });
    }

}

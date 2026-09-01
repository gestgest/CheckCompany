using System.Collections.Generic;
using UnityEngine;

public class EmployeeObjectSystem : MonoBehaviour
{
    private List<GameObject> _employeeObjects = new List<GameObject>();

    [SerializeField] private EmployeeManagerSO _employeeManagerSO;

    [SerializeField] private GameObject _employeePrefab;
    [SerializeField] private Transform _employeeParent;

    //EmployeeManagerSO
    [Header("Listening to Events")]
    [SerializeField] private IntEventChannelSO _onChangedCreateEvent;
    [SerializeField] private IntEventChannelSO _onChangedRemoveEvent;
    [SerializeField] private Int2EventChannelSO _onChangedUpdateEvent;

    private void OnEnable()
    {
        _onChangedCreateEvent._onEventRaised += CreateEmployeeObject;
        _onChangedRemoveEvent._onEventRaised += RemoveEmployeeObject;
        _onChangedUpdateEvent._onEventRaised += ChangeEmployeeObject;
    }

    private void OnDisable()
    {
        _onChangedCreateEvent._onEventRaised -= CreateEmployeeObject;
        _onChangedRemoveEvent._onEventRaised -= RemoveEmployeeObject;
        _onChangedUpdateEvent._onEventRaised -= ChangeEmployeeObject;
    }

    private void Start()
    {
        int n = _employeeManagerSO.GetEmployees().Count;
        //Debug.Log("수는 : "+  n);
        for (int i = 0; i < _employeeManagerSO.GetEmployees().Count; i++)
        {
            CreateEmployeeObject(i);
        }
    }


    //오래된 직원이 오면 index가 꼬인다.
    //create
    private void CreateEmployeeObject(int index)
    {
        Employee e = _employeeManagerSO.GetEmployee(index);

        GameObject obj = Instantiate(_employeePrefab, _employeeParent);
        obj.GetComponent<EmployeeWorkAI>().Init(e.ID);
        _employeeObjects.Add(obj);
    }

    private void RemoveEmployeeObject(int index)
    {
        //기존 코드는 Destroy 없이 리스트에서만 제거해서 오브젝트가 파괴되지 않고 남아있었고,
        //그 결과 EmployeeWorkAI.OnDestroy도 호출되지 않아 배정된 책상이 영원히 반납되지 않는 문제가 있었다.
        Destroy(_employeeObjects[index]);
        _employeeObjects.RemoveAt(index);
    }

    private void ChangeEmployeeObject(int a, int b)
    {
        GameObject tmp = _employeeObjects[a];
        _employeeObjects[a] = _employeeObjects[b];
        _employeeObjects[b] = tmp;
    }


}

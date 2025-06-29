using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ManagerMissionPanel : Panel
{
    [Header("Manager")]
    [SerializeField] protected MissionManagerSO _missionManager;
    [SerializeField] protected CreateMissionManagerSO _createMissionManager;
    
    [Space]
    [SerializeField] protected TMP_InputField title;
    [SerializeField] protected CustomRadioButtonGroup employeeTypeGroup;
    [SerializeField] protected CustomRadioButtonGroup levelGroup;
    
    [SerializeField] protected MultiLayoutGroup _multiLayoutGroup; //제일 아래 Layout => inputList
    [SerializeField] protected GameObject [] _todoMissionObjects; //todo
    
    protected static int TODO_MISSION_HEIGHT = 30;
    protected int todo_mission_size;

    protected virtual void Init()
    {
        _multiLayoutGroup.Init();

        todo_mission_size = 7;
        //you should make 210 size
        //Debug.Log(SMALL_MISSION_HEIGHT * smallMission_size - layoutGroup.GetHeight());
        _multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT * todo_mission_size - _multiLayoutGroup.GetHeight());
        for (int i = 1; i < _todoMissionObjects.Length; i++)
        {
            DeleteTodoMission();
        }
    }
    
    protected List<Todo_Mission> GetTodoMissions()
    {
        List<Todo_Mission> _todoMissions = new List<Todo_Mission>();
        for (int i = 0; i < todo_mission_size; i++)
        {
            bool isDone = AddIsDone(i);
            
            //여기 isDone 매개변수 자체가 false임
            _todoMissions.Add(
                new Todo_Mission(
                    _todoMissionObjects[i].transform.GetChild(0).GetComponent<TMP_InputField>().text
                    , isDone
                )
            );
        }

        return _todoMissions;
    }
    

    protected virtual bool AddIsDone(int i)
    {
        return false;
    }
    
    //max_count 7
    public void AddTodoMission()
    {
        if (todo_mission_size == Employee.MAX_TODO_MISSION_SIZE)
            return;
        _todoMissionObjects[todo_mission_size].SetActive(true);
        todo_mission_size++;

        _multiLayoutGroup.AddHeight(TODO_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }
    
    public void DeleteTodoMission()
    {
        if (todo_mission_size <= 1)
            return;
        todo_mission_size--;
        _todoMissionObjects[todo_mission_size].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        _todoMissionObjects[todo_mission_size].SetActive(false);

        _multiLayoutGroup.AddHeight(-TODO_MISSION_HEIGHT);
        //multiLayoutGroup.RerollScreen();
    }
    
    
    //AssignEmployeeButton function
    public void SwitchingAssignEmployeePanel()
    {
        AssignMissionPanel p = panels[0].GetComponent<AssignMissionPanel>();
        
        GamePanelManager.instance.PushIndexList(0);
        p.Init();
        //panels[0].OnPanel();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    public static MissionController instance;

    private int mission_count;

    //이미지 리스트?
    [SerializeField] private Sprite [] icons;
    private List<Todo_Mission> todo_missions;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        Destroy(this);
    }

    //서버 가져오는 함수 => 나중에 매개변수에 Dictionary<string, object> todo_missions와 id를 넣을 예정
    public void Init(Dictionary<string, object> data, int mission_count)
    {
        todo_missions = new List<Todo_Mission>();
        this.mission_count = mission_count;
        GetTodoMissionsFromJSON(data);

        //List<string> small_missions = new List<string>();

        //제거할 기능
        //small_missions.Add("버그 해결");
        //small_missions.Add("새로운 기술");
        //small_missions.Add("기능 추가");
        //예시 미션도 넣을까?
        //Add_TodoMission(new Todo_Mission(0, 2, "유니티", 0, 0, small_missions));

    }
    

    public int GetMissionCount()
    {
        return mission_count;
    }
    public Todo_Mission GetMission(int id)
    {
        return todo_missions[id];
    }

    public Sprite GetIcon(int index)
    {
        return icons[index];
    }

    public List<Todo_Mission> GetMissions()
    {
        return todo_missions;
    }

    public int GetAndIncrementCount()
    {
        return mission_count++;
    }

    public int GetMissionSize()
    {
        return todo_missions.Count;
    }
       
    public void Add_TodoMission(Todo_Mission todo_mission)
    {
        todo_missions.Add(todo_mission);
    }


    private void GetTodoMissionsFromJSON(Dictionary<string, object> data)
    {

        //id와 id 리스트들
        foreach (KeyValuePair<string, object> todo_mission in data)
        {
            Dictionary<string, object> tmp = (Dictionary<string, object>)(todo_mission.Value);

            Todo_Mission todo_Mission = new Todo_Mission();

            todo_Mission.ID = Convert.ToInt32(todo_mission.Key);
            todo_Mission.SetMissionFromJSON(tmp);
            Add_TodoMission(todo_Mission);

            //여기서 Todo_Mission 생성하고 SetFromJSON 각각
            //EmployeeSO employeeSO = RecruitmentController.instance.GetEmployeeSO(Convert.ToInt32(tmp["employeeType"]));
            //Employee employee = new EmployeeBuilder().BuildEmployee(employeeSO);
        }

    }
    
    
    #region BINARY_SEARCH
    //binary_search
    public int Search_Employee_Index(int id)
    {
        int index = Binary_Search_Employee_Index(0, todo_missions.Count - 1, id);
        if (todo_missions[index].ID == id)
        {
            return index;
        }
        return -1;
    }

    private int Binary_Search_Employee_Index(int start, int end, int id)
    {
        if (start > end)
        {
            return start;
        }
        int mid = (start + end) / 2;
        if (id > todo_missions[mid].ID)
        {
            return Binary_Search_Employee_Index(mid + 1, end, id);
        }
        else
        {
            return Binary_Search_Employee_Index(start, mid - 1, id);
        }
    }

    #endregion
}

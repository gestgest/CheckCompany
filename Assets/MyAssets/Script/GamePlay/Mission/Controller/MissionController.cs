using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    public static MissionController instance;

    private int mission_id;

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
    public void Init()
    {
        todo_missions = new List<Todo_Mission>();

        List<string> small_missions = new List<string>();

        //제거할 기능
        small_missions.Add("버그 해결");
        small_missions.Add("새로운 기술");
        small_missions.Add("기능 추가");
        //예시 미션도 넣을까?
        Add_TodoMission(new Todo_Mission(0, 2, "유니티", 0, 0, small_missions));


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

    public int GetAndIncrementID()
    {
        return mission_id++;
    }

    public int GetMissionSize()
    {
        return todo_missions.Count;
    }
       
    public void Add_TodoMission(Todo_Mission todo_mission)
    {
        todo_missions.Add(todo_mission);
    }
}

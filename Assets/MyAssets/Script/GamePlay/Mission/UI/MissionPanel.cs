using UnityEngine;

public class MissionPanel : Panel
{
    [SerializeField] private GameObject missionPrefab;
    [SerializeField] private Transform missionParent;
    protected override void Start()
    {
        base.Start();
        foreach (Todo_Mission mission in MissionController.instance.GetMissions())
        {
            CreateTodoMissionObject(mission);
        }
    }

    private void CreateTodoMissionObject(Todo_Mission todoMission)
    {
        //Todo_Mission 만들기
        GameObject missionObject = Instantiate(missionPrefab, missionParent);

        TodoMissionElementUI mission = missionObject.GetComponent<TodoMissionElementUI>();
        
        
    }
}

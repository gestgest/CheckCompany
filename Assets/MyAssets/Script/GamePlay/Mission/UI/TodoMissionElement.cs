using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TodoMissionElement : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Toggle my_toggle;

    private int mission_id;
    private int todo_mission_index;

    private bool isInit = true;

    private Gauge gauge;

    public void SetGague(Gauge gauge)
    {
        this.gauge = gauge;
    }

    public void SetTodoMission(Todo_Mission todoMission, int mission_id, int todo_mission_index)
    {
        this.title.text = todoMission.Title;
        this.my_toggle.isOn = todoMission.IsDone;
        this.mission_id = mission_id;
        this.todo_mission_index = todo_mission_index;
        isInit = false;
    }

    ///<summary>대충 토글 상태에 따라 폰트 바꾸는 코드 </summary>
    public void UpdateTodoMissionStatus()
    {
        if(isInit)
        {
            return;
        }
        if (my_toggle.isOn)
        {
            title.fontStyle = FontStyles.Italic;
            title.color = Color.gray;
            //gage 값 전달
            gauge.AddValue(1);
            
            if (gauge.GetValue() >= gauge.GetMaxValue())
            {
                //미션 다 했다는 이야기
            }
        }
        else
        {
            title.fontStyle = FontStyles.Normal;
            title.color = Color.black;
            //gage 값 전달
            gauge.AddValue(-1);
        }
        //취소선은 <s> 이걸로 하라는데

        Mission mission = MissionController.instance.GetMission(MissionController.instance.Search_Employee_Index(mission_id));
        
        mission.Set_TodoMission_IsDone(todo_mission_index, my_toggle.isOn);
        Debug.Log(my_toggle.isOn);

        //json이 아니라 배열 수정이다.
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "missions." + mission_id + ".todo_missions",
            mission.GetTodoMissions()
        );
    }
}
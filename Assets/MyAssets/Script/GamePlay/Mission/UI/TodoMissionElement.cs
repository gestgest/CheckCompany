using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TodoMissionElement : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Toggle my_toggle;

    private MissionController mc;
    private int mission_id;
    private int todo_mission_index;

    private bool isInit = true;

    private Gauge gauge;

    public void SetGague(Gauge gauge)
    {
        this.gauge = gauge;
    }

    private void Start()
    {
        mc = MissionController.instance;
        ChangeGaugeStatus(false);
    }

    ///<summary>MissionPanel이나 CreateMissionPanel의 start에서 호출, 설정하는 함수</summary>
    public void SetTodoMission(Todo_Mission todoMission, int mission_id, int todo_mission_index)
    {
        this.title.text = todoMission.Title;
        this.my_toggle.isOn = todoMission.IsDone;
        this.mission_id = mission_id;
        this.todo_mission_index = todo_mission_index;
        isInit = false;
    }

    ///<summary>대충 토글 상태에 따라 폰트 바꾸는 코드 => 토글 </summary>
    public void UpdateTodoMissionStatus()
    {
        if (isInit)
        {
            return;
        }
        ChangeGaugeStatus();
        MissionToServer();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isGage">게이지 값을 변경 가능한지</param>
    private void ChangeGaugeStatus(bool isGage = true)
    {
        if (my_toggle.isOn)
        {
            title.fontStyle = FontStyles.Italic;
            title.color = Color.gray;
            //gage 값 전달
            gauge.AddValue(1);

            //게이지가 다 채워졌으면 => Complete
            if (gauge.GetValue() >= gauge.GetMaxValue() && isGage)
            {
                int mission_index = mc.Search_Mission_Index(mission_id);
                Mission mission = mc.GetMission(mission_index);
                
                mission.Set_TodoMission_IsDone(todo_mission_index, my_toggle.isOn);

                //현재 미션은 제거, 완료된 미션에 온
                mc.GetMissionPanel().RemoveMissionObject(mission_index);
                mc.GetCompleteMissionPanel().AddMissionElementObject(mc.GetMission(mission_index));
            }
        }
        else if(isGage)
        {
            title.fontStyle = FontStyles.Normal;
            title.color = Color.black;

            //완료된 것을 미 완성으로 수정
            if (gauge.GetValue() >= gauge.GetMaxValue())
            {
                int mission_index = mc.Search_Mission_Index(mission_id);
                Mission mission = mc.GetMission(mission_index);
                
                mission.Set_TodoMission_IsDone(todo_mission_index, my_toggle.isOn);

                //현재 미션은 제거, 완료된 미션에 온
                mc.GetCompleteMissionPanel().RemoveMissionObject(mission_index);
                mc.GetMissionPanel().AddMissionElementObject(mc.GetMission(mission_index));
            }
            
            //gage 값 전달
            gauge.AddValue(-1);
        }
        //취소선은 <s> 이걸로 하라는데
    }

    private void MissionToServer()
    {
        Mission mission = mc.GetMission(MissionController.instance.Search_Mission_Index(mission_id));
        //mission.Set_TodoMission_IsDone(todo_mission_index, my_toggle.isOn);

        //json이 아니라 배열 수정이다.
        FireStoreManager.instance.SetFirestoreData(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "missions." + mission_id + ".todo_missions",
            mission.GetTodoMissions()
        );
    }
}
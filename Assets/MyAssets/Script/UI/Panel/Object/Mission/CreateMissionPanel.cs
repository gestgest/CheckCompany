using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CreateMissionPanel : ManagerMissionPanel
{
    [Space]
    [SerializeField] private MissionPanel missionPanel;

    [SerializeField] private Toggle[] toggles;

    private bool isFirst = true;




    protected void OnEnable()
    {
        if (!isFirst)
            Init();
        _createMissionManager.Init();
    }

    protected override void Start()
    {
        base.Start();
        Init();
        isFirst = false;
    }

    protected override void Init()
    {
        base.Init();

        title.text = "";

        employeeTypeGroup.SetIndex(0);
        levelGroup.SetIndex(0);
        
        _todoMissionObjects[0].transform.GetChild(0)
            .GetComponent<TMP_InputField>().text = "";
        
        //layoutGroup.RerollScreen();
        //layoutGroup.SwitchingScreen(true);
    }

    //버튼 - 미션 만드는 함수
    public void CreateMission()
    {
        
        Mission mission = _createMissionManager.CreateMission(
            _missionManager.GetAndIncrementCount(),
            title.text,
            GetTodoMissions()
        );

        _missionManager.AddMission(mission);
        _missionManager.SetServerMissionCount();

        missionPanel.CreateMissionElementObject(mission); //
        //missionPanel.OnPanel(); => 이거 자체가 서브 미션 Panel이라 전환이 안됨

        //초기화
        Init();
        GamePanelManager.instance.SwitchingPanelFromInt(1); //missionPanel로 전환
    }


}

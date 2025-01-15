using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;


//MonoBehaviour 대신 Panel해야하나?
public class EmployeeStatusWindow : MonoBehaviour
{
    //const Employee.MAX_MISSION_SIZE = 5;

    //Description Panel UI 부분
    [SerializeField] private TextMeshProUGUI [] nameTexts;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI salaryText;
    [SerializeField] private TextMeshProUGUI careerPeriodText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [SerializeField] private Bar staminaBar;
    [SerializeField] private Bar mentalBar;
    
    
    //MissionPanel
    [SerializeField] private GameObject missionObjectParent; //5개
    private MissionElementUI[] missionUIs; //5개 
    //나중에 MissionElementUI에서 클릭 하면 바로바로 여기서 미션을 가져와야 함
    // ㄴ 원래 이거였지만 어쩌다 보니 바뀜

    //Mission UI 부분
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private GameObject addMissionMiniWindow;
    [SerializeField] private GameObject processBar;
    [SerializeField] private TextMeshProUGUI processBar_text;
    
    //ㄴ MissionPanel의 AddMissionMiniWindow
    //private Mission[] missions;  //추가할 미션  => 아직  안쓴다. 대신 미션 필터링할때 여기에 담을 수 있다.
    [SerializeField] private Transform addMissionElement_parent;
    [SerializeField] private GameObject addMissionElement_prefab;

    //MissionPanel의 Mission
    [SerializeField] private GameObject[] smallMission_PoolObjects; //풀링용 오브젝트 (7개)
    [SerializeField] private GameObject smallMission_prefab; //토글
    
    private Employee employee;
    // ㄴ Mission : 미션 목록들은 여기에 있다 ****************
    //    ㄴ 이미 그 전
    
    RectTransform rf_dPanel;
    //아마 이거 달성률 임
    private int small_mission_size; // 이거 제거 예정이었으나 그냥 호출 한번하고 계속 보관하는 용도
    bool clearSmallMissionLock = false;


    void Awake()
    {
        rf_dPanel = descriptionPanel.GetComponent<RectTransform>();
        missionUIs = new MissionElementUI[Employee.MAX_MISSION_SIZE];

        for (int i = 0; i < missionObjectParent.transform.childCount; i++)
        {
            Transform mObj = missionObjectParent.transform.GetChild(i);
            missionUIs[i] = mObj.GetComponent<MissionElementUI>();
        }
    }

    void Start()
    {
    }



    //EmployeeStatusWindow => 값 받기, panels 관련 애니메이션, panel이동 관련 함수
    //값 받고, UI에게 전달 하기
    public void SetValue(Employee employee)
    {
        this.employee = employee;

        for(int i = 0; i < nameTexts.Length; i++)
        {
            nameTexts[i].text = employee.Name + " 사원"; //일단 사원으로 함
        }
        //이미지는 패스
        ageText.text = "나이 : " + employee.Age.ToString() + "살";
        salaryText.text = "연봉 : " + (employee.Salary * 12).ToString() + "원";
        careerPeriodText.text = "경력 기간 : " + employee.CareerPeriod.ToString() + "개월";
        timeText.text = "근무시간 : " + employee._WorkTime.start.ToString() + " ~ " + employee._WorkTime.end.ToString();
        
        staminaBar.Init(employee.Stamina, employee.Max_Stamina);
        mentalBar.Init(employee.Mental, employee.Max_Mental);
        
        SetMissionUI(); //
        AddMissionToMiniWindow();
    }

    public float GetDescriptionPanelHeight()
    {
        return rf_dPanel.rect.width;
    }



    ///////////////////////AddMissionMiniWindow
    /// AddMissionMiniWindow 에게 값 전달
    public void AddMissionToMiniWindow()
    {
        //나중에 개발자에 따라 기술에 따라 필터링기능 넣을 예정 ★★★★
        //missions 개수만큼 addMissionElement_prefab 생성
        //어,,, 이 미션은 controller로 가져와야 한다
        for (int i = 0; i < MissionController.instance.GetMissionSize(); i++)
        {
            GameObject addMissionElement = Instantiate(addMissionElement_prefab);

            //addMissionElement_parent에 넣고
            addMissionElement.transform.SetParent(addMissionElement_parent);

            //SetMission 함수 실행
            AddMissionElementUI element = addMissionElement.GetComponent<AddMissionElementUI>();
            element.SetMission(MissionController.instance.GetMission(i));
            element.SetEmployeeStatusWindow(this.GetComponent<EmployeeStatusWindow>());
        }
    }

    //////////////////////////////////////Mission////////////////////////////////////////
    public void RemoveMission(int index)
    {
        employee.RemoveMission(index);
        SetMissionUI();

        // if(index == 0)
        //     SetSmallMission();
    }

    public void AddMission(MissionSO m)
    {
        Mission mission = new Mission(m);
        employee.AddMission(mission);
        employee.AddMissionToServer(mission, GameManager.instance.Nickname, employee.ID);
        addMissionMiniWindow.SetActive(false);
        SetMissionUI();

        // if(employee.GetMissionSize() == 1)
        //     SetSmallMission();
        
    }

    //미션을 UI에 적용
    private void SetMissionUI()
    {
        int missionSize = employee.GetMissionSize();
        //Debug.Log("EmployeeStatusWindow 의 SetMission : " + missionSize);
        
        
        for (int i = 0; i < missionSize; i++)
        {
            Mission mission = employee.GetMission(i);

            //?
            if (mission.GetMissionSO().GetMissionType() != MissionType.NONE)
                missionUIs[i].SetValue(mission.GetMissionSO());
        }

        for (int i = missionSize; i < Employee.MAX_MISSION_SIZE; i++)
        {
            missionUIs[i].SetValue();
        }
        
        SetSmallMission();
    }

    /////////////////////////////////////소미션
    //소 미션을 클리어 한 경우
    public void ClearSmallMission(int index)
    {
        if(clearSmallMissionLock)
            return;
        //켰다면
        if (smallMission_PoolObjects[index].GetComponent<Toggle>().isOn)
        {
            Check_smallmission_achievement(index, true);
        }
        else
        {
            Check_smallmission_achievement(index, false);
        }

        //미션 클리어
        if (employee.GetMission(0).GetAchievementClearCount() == small_mission_size)
        {
            //디버깅용 돈 주는 이벤트
            GameManager.instance.SetMoney(GameManager.instance.Money + 10000);
            RemoveMission(0);
        }
    }

    //소 미션 셋팅 => 이 친구가 계속 안됨
    private void SetSmallMission()
    {
        //Debug.Log("EmployeeStatusWindow 의 GetMissionSize : " + employee.GetMissionSize());

        if(employee.GetMissionSize() == 0)
        {
            small_mission_size = 0;
            Set_smallMission_achievement_UI();

            //모두 소미션 오브젝트 비활성화
            for (int i = 0; i < Employee.MAX_SMALL_MISSION_SIZE; i++)
                smallMission_PoolObjects[i].SetActive(false);
            return;
        }

        Mission mission = employee.GetMission(0);
        string[] missions_text = mission.GetMissionSO().GetSmallMissions();
        int j;
        
        small_mission_size = missions_text.Length;
        Set_smallMission_achievement_UI();
        clearSmallMissionLock = true;

        //오브젝트 풀링? => 7
        for (j = 0; j < small_mission_size && j < Employee.MAX_SMALL_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(true);

            smallMission_PoolObjects[j].GetComponent<Toggle>().isOn = employee.Get_SmallMission_Achievement(j);
            Text _text = smallMission_PoolObjects[j].GetComponentInChildren<Text>();
            _text.text = missions_text[j];
        }

        clearSmallMissionLock = false;

        for (; j < Employee.MAX_SMALL_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(false);
        }
    }

    /// //////////////////프로퍼티 - small_mission_current_size
    public void Check_smallmission_achievement(int index, bool check)
    {
        Mission mission = employee.GetMission(0);

        //10은 그냥 적은거
        if(employee.Stamina >= 10)
        {
            mission.SetAchievement(index, check);
            employee.SetStamina(employee.Stamina - 10);
        }
        else
        {
            Debug.Log("체력이 부족합니다.");
            return;
        }
        if (mission.GetAchievementClearCount() != small_mission_size)
            employee.SetAllMissionToServer(GameManager.instance.Nickname, employee.ID);

        Set_smallMission_achievement_UI();
    }

    
    //UI 설정
    public void Set_smallMission_achievement_UI()
    {
        if (employee.GetMissionSize() == 0)
        {
            processBar_text.text = "0%";
            return;
        }
        int small_mission_achievement = employee.GetMission(0).GetAchievementClearCount();
        //Debug.Log("미션 완료 횟수 : " + small_mission_achievement);
        processBar_text.text = ((float)small_mission_achievement * 100 / small_mission_size).ToString() + "%";
    }
    
    public void SetStaminaBarUI(int employee_id, int value)
    {
        if (employee == null)
            return;
        if(employee.ID == employee_id)
            staminaBar.SetValue(value);
    }
    public void SetMentalBarUI(int employee_id, int value)
    {
        if (employee == null)
            return;
        
        if(employee.ID == employee_id)
            staminaBar.SetValue(value);
    }
}

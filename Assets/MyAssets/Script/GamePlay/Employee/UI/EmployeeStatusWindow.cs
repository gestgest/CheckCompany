using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Serialization;


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
    
    [FormerlySerializedAs("staminaBar")] [SerializeField] private Gauge staminaGauge;
    [FormerlySerializedAs("mentalBar")] [SerializeField] private Gauge mentalGauge;
    
    
    //MissionPanel
    [SerializeField] private GameObject missionObjectParent; //5개
    private MissionIconElement[] missionUIs; //5개 
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

    [Header("Model")]
    [SerializeField] private MissionsSO missionsSO;

    
    private Employee employee;
    // ㄴ Mission : 미션 목록들은 여기에 있다 ****************
    //    ㄴ 이미 그 전
    
    RectTransform rf_dPanel;
    //아마 이거 달성률 임
    private int small_mission_size; // 이거 제거 예정이었으나 그냥 호출 한번하고 계속 보관하는 용도
    bool clearSmallMissionLock = false;


    private static int WIDTH = 250;

    void Awake()
    {
        rf_dPanel = descriptionPanel.GetComponent<RectTransform>();
        missionUIs = new MissionIconElement[Employee.MAX_MISSION_SIZE];

        for (int i = 0; i < missionObjectParent.transform.childCount; i++)
        {
            Transform mObj = missionObjectParent.transform.GetChild(i);
            missionUIs[i] = mObj.GetComponent<MissionIconElement>();
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
        salaryText.text = "연봉 : " + (employee.Salary * 12 / 10000).ToString() + "만원";
        careerPeriodText.text = "경력 기간 : " + employee.CareerPeriod.ToString() + "개월";
        timeText.text = "근무시간 : " + employee._WorkTime.start.ToString() + " ~ " + employee._WorkTime.end.ToString();
        
        staminaGauge.Init(employee.Stamina, employee.Max_Stamina, WIDTH);
        mentalGauge.Init(employee.Mental, employee.Max_Mental, WIDTH);
        
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

        //기존의 gameObject는 초기화 예정  => pool링 해야하나?
        for (int i = 0; i < addMissionElement_parent.transform.childCount; i++)
        {
            //자식제거함수
            Destroy(addMissionElement_parent.GetChild(i).gameObject);
        }
        
        //missions 개수만큼 addMissionElement_prefab 생성
        for (int i = 0; i < missionsSO.GetMissionSize(); i++)
        {
            GameObject addMissionElement = Instantiate(addMissionElement_prefab);

            //addMissionElement_parent에 넣고
            addMissionElement.transform.SetParent(addMissionElement_parent);

            //SetMission 함수 실행
            AddMissionElementUI element = addMissionElement.GetComponent<AddMissionElementUI>();
            element.SetMission(missionsSO.GetMission(i));
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

    public void AddMission(Mission m)
    {
        UMUMUM mission = new UMUMUM(m);
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
            UMUMUM mission = employee.GetMission(i);
            missionUIs[i].SetValue(mission.GetTodo_Mission());

            //?  null 값이라면
            //if (mission.GetTodo_Mission().GetMissionType() != EmployeeType.NONE)
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
            GameManager.instance.SetMoney(GameManager.instance.Money + 30000 
                * employee.GetMission(0).GetTodo_Mission().GetTodoMissions().Count);
            RemoveMission(0);
        }
    }

    //소 미션 셋팅(init)
    private void SetSmallMission()
    {
        //Debug.Log("EmployeeStatusWindow 의 GetMissionSize : " + employee.GetMissionSize());

        if(employee.GetMissionSize() == 0)
        {
            small_mission_size = 0;
            Set_smallMission_achievement_UI();

            //모두 소미션 오브젝트 비활성화
            for (int i = 0; i < Employee.MAX_TODO_MISSION_SIZE; i++)
                smallMission_PoolObjects[i].SetActive(false);
            return;
        }

        UMUMUM mission = employee.GetMission(0);
        List<Todo_Mission> missions = mission.GetTodo_Mission().GetTodoMissions();
        int j;
        
        small_mission_size = missions.Count;
        Set_smallMission_achievement_UI();
        clearSmallMissionLock = true;

        //소 미션 오브젝트 풀링 => 7
        for (j = 0; j < small_mission_size && j < Employee.MAX_TODO_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(true);

            smallMission_PoolObjects[j].GetComponent<Toggle>().isOn = employee.Get_SmallMission_Achievement(j);
            Text _text = smallMission_PoolObjects[j].GetComponentInChildren<Text>();
            _text.text = missions[j].Title;
        }

        clearSmallMissionLock = false;

        for (; j < Employee.MAX_TODO_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(false);
        }
        BanSmallCheckMission();
    }

    /// //////////////////프로퍼티 - small_mission_current_size
    public void Check_smallmission_achievement(int index, bool check)
    {
        UMUMUM mission = employee.GetMission(0);
        if (check)
        {
            if(employee.Stamina >= 10)
            {
                employee.SetStamina(employee.Stamina - 10);
            }
            else
            {
                Debug.Log("체력이 부족합니다.");
                return;
            }
        }
        else //체크 해제
        {
            employee.SetStamina(employee.Stamina + 10);
        }
        mission.SetAchievement(index, check);

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

    //
    public void BanSmallCheckMission()
    {
        bool isBan = true;
        if (employee == null)
        {
            return;
        }
        if (employee.GetMissionSize() == 0)
        {
            return;
        }

        if (employee.Stamina >= 10)
        {
            isBan = false;
        }
        for (int j = 0; j < small_mission_size && j < Employee.MAX_TODO_MISSION_SIZE; j++)
        {
            Toggle t = smallMission_PoolObjects[j].GetComponent<Toggle>();
            if (isBan)
            {
                //토글이 체크 안되어있다면 비활성화
                if(!t.isOn)
                    t.interactable = false;
            }
            else
            {
                t.interactable = true;
            }
        }
        
    }
    
    public void SetStaminaBarUI(int employee_id, int value)
    {
        if (employee == null)
            return;
        
        if(employee.ID == employee_id)
            staminaGauge.SetValue(value);
    }
    
    public void SetMentalBarUI(int employee_id, int value)
    {
        if (employee == null)
            return;
        
        if(employee.ID == employee_id)
            staminaGauge.SetValue(value);
    }
}

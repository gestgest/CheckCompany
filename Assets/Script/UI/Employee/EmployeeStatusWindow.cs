using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EmployeeStatusWindow : MonoBehaviour
{
    //const IEmployee.MAX_MISSION_SIZE = 5;
    [SerializeField] private GameManager gameManager;



    //Description Panel
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI careerText;
    [SerializeField] private TextMeshProUGUI timeText;

    //MissionPanel
    [SerializeField] private GameObject missionObjectParent; //5개
    private MissionElementUI[] missionUIs; //5개
    //나중에 MissionElementUI에서 클릭 하면 바로바로 여기서 미션을 가져와야 함
    // ㄴ 원래 이거였지만 어쩌다 보니 바뀜

    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private GameObject addMissionMiniWindow;
    [SerializeField] private GameObject processBar;
    [SerializeField] private TextMeshProUGUI processBar_text;

    private IEmployee employee;
    RectTransform rf_dPanel;

    //ㄴ MissionPanel의 AddMissionMiniWindow
    [SerializeField] private MissionSO[] missions;  //추가할 미션 갯수
    [SerializeField] private Transform addMissionElement_parent;
    [SerializeField] private GameObject addMissionElement_prefab;

    //MissionPanel의 Mission
    [SerializeField] private GameObject[] smallMission_PoolObjects; //풀링용 오브젝트 (7개)
    [SerializeField] private GameObject smallMission_prefab; //토글
    private int small_mission_size;
    private int small_mission_current_size;
    bool clearSmallMissionLock = false;


    void Awake()
    {
        rf_dPanel = descriptionPanel.GetComponent<RectTransform>();
        missionUIs = new MissionElementUI[IEmployee.MAX_MISSION_SIZE];
    }

    void Start()
    {
        for (int i = 0; i < missionObjectParent.transform.childCount; i++)
        {
            Transform mObj = missionObjectParent.transform.GetChild(i);
            missionUIs[i] = mObj.GetComponent<MissionElementUI>();
        }
    }



    //EmployeeStatusWindow => 값 받기, panels 관련 애니메이션, panel이동 관련 함수
    //값 받고, UI에게 전달 하기
    public void SetValue(IEmployee employee)
    {
        this.employee = employee;
        nameText.text = employee.Name + " 사원"; //일단 사원으로 함
        //이미지는 패스
        ageText.text = "나이 : " + employee.Age.ToString() + "살";
        costText.text = "연봉 : " + (employee.Cost * 12).ToString() + "원";
        careerText.text = "경력 기간 : " + employee.Career.ToString() + "개월";
        timeText.text = "근무시간 : " + employee._WorkTime.start.ToString() + " ~ " + employee._WorkTime.end.ToString();

        SetMission();
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
        for (int i = 0; i < missions.Length; i++)
        {
            GameObject addMissionElement = Instantiate(addMissionElement_prefab);

            //addMissionElement_parent에 넣고
            addMissionElement.transform.SetParent(addMissionElement_parent);

            //SetMission 함수 실행
            AddMissionElementUI element = addMissionElement.GetComponent<AddMissionElementUI>();
            element.SetMission(missions[i]);
            element.SetEmployeeStatusWindow(this.GetComponent<EmployeeStatusWindow>());
        }
    }

    //////////////////////////////////////Mission////////////////////////////////////////
    public void RemoveMission(int index)
    {
        employee.RemoveMission(index);
        SetMission();
        if(index == 0)
            SetSmallMission();
    }

    public void AddMission(MissionSO m)
    {
        employee.AddMission(m);
        addMissionMiniWindow.SetActive(false);
        SetMission();
        if(employee.GetMissionSize() == 1)
            SetSmallMission();
        
    }

    private void SetMission()
    {
        int missionSize = employee.GetMissionSize();
        //Debug.Log("EmployeeStatusWindow 의 SetMission : " + missionSize);
        for (int i = 0; i < missionSize; i++)
        {
            MissionSO mission = employee.GetMission(i);

            if (mission.GetMissionType() != MissionType.NONE)
                missionUIs[i].SetValue(mission);
        }

        for (int i = missionSize; i < IEmployee.MAX_MISSION_SIZE; i++)
        {
            missionUIs[i].SetValue();
        }
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
            AddSmall_mission_current_size(+1);
        }
        else
        {
            AddSmall_mission_current_size(-1);
        }
        employee.SetIsClearSmallMission(index);

        //미션 클리어
        if (small_mission_current_size == small_mission_size)
        {
            //디버깅용 돈 주는 이벤트
            gameManager.Money += 10000;
            RemoveMission(0);
            
        }
    }

    //소 미션 셋팅
    private void SetSmallMission()
    {
        if(employee.GetMissionSize() == 0)
        {
            small_mission_size = 0;
            SetSmall_mission_current_size(0);

            //모두 소미션 오브젝트 비활성화
            for (int i = 0; i < IEmployee.MAX_SMALL_MISSION_SIZE; i++)
                smallMission_PoolObjects[i].SetActive(false);
            return;
        }

        MissionSO mission = employee.GetMission(0);
        string[] missions_text = mission.GetSmallMissions();
        int j;
        
        small_mission_size = missions_text.Length;
        SetSmall_mission_current_size(employee.GetIsClearSmallMissionSize());
        clearSmallMissionLock = true;

        //오브젝트 풀링? => 7
        for (j = 0; j < small_mission_size && j < IEmployee.MAX_SMALL_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(true);

            smallMission_PoolObjects[j].GetComponent<Toggle>().isOn = employee.GetIsClearSmallMission(j);
            Text _text = smallMission_PoolObjects[j].GetComponentInChildren<Text>();
            _text.text = missions_text[j];
        }

        clearSmallMissionLock = false;

        for (; j < IEmployee.MAX_SMALL_MISSION_SIZE; j++)
        {
            smallMission_PoolObjects[j].SetActive(false);
        }
    }

    /// //////////////////프로퍼티 - small_mission_current_size
    public void AddSmall_mission_current_size(int value)
    {
        SetSmall_mission_current_size(small_mission_current_size + value);
    }

    public void SetSmall_mission_current_size(int value)
    {
        small_mission_current_size = value;
        if (small_mission_size == 0)
            processBar_text.text = "0%";
        else
            processBar_text.text = ((float)small_mission_current_size * 100 / small_mission_size).ToString() + "%";
    }
}

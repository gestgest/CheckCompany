using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] UIManager ui_manager;
    //[SerializeField] FireStoreManager fireStoreManager;
    private FirebaseAuth auth;
    private FirebaseUser user;


    //managers
    [Header("Manager")]
    [SerializeField] RecruitmentManagerSO recruitmentControllerSO;
    [SerializeField] MissionManagerSO missionControllerSO;
    [SerializeField] EmployeeManagerSO employeeControllerSO;
    [SerializeField] PlacedObjectManager _placeManager;
    
    [Header("ServerEvent")]
    [SerializeField] private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;
    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    [SerializeField] private GetJSONFirebaseEventChannelSO _getJSONEventChannelSO;


    private string nickname;

    int employee_count = 0;
    long money;
    [SerializeField] private GameDate _gameDate;
    [SerializeField] private Date _currentDate;
    

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("RecruitmentElement : T 버튼 누름");
        }
        */
    }
    void Start()
    {
        recruitmentControllerSO.Init();
        missionControllerSO.Init();
        employeeControllerSO.Init();
        
        _placeManager.Init();
        
        _gameDate = new GameDate(employeeControllerSO.AddStamina, _sendFirebaseEventChannelSO);

        GameServerStart();
        //Don't put the LoginScene => main thread error
        //fireStoreManager.Init();
        //SetDateUI();
    }
    
    public async void GameServerStart()
    {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        //if you are not logged in, you must enter the default account
        if (user == null)
        {
            //default account
        }
        Debug.Log(user.Email);

        //서버에게 number 받는 거는 무조건 long 으로 해야한다, ToInt32
        //타입이 64비트가 나온다. => 8바이트 => long
        //int는 4바이트
        //convert로 하면 null이 0으로 바뀌어진다
        
        nickname = (string)await _getJSONEventChannelSO.RaiseEvent("User", user.Email, "nickname");

        long money;
        money = (long)(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "money") ?? (long)0);
        SetMoney(money, false);

        int employee_count =
            Convert.ToInt32(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "employee_count"));

        
        //MissionController.instance.Init();
        missionControllerSO.SetMissionData(
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "missions"),
            Convert.ToInt32(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "mission_count"))
        );

        Dictionary<string,object> recruitments = (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "recruitments");
        recruitmentControllerSO.JSONToRecruitments(recruitments);

        Dictionary<string, object> employees = (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "employees");
        employeeControllerSO.JSONToEmployees(employees);

        Dictionary<string, object> dateData =
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "date");

        if (dateData == null)
        {
            dateData = new Dictionary<string, object>();
        }

        _currentDate = new Date(true);
        // _currentDate.GetDateFromJSON(
        //     ConvertJSON.SafeGet<Dictionary<string, object>>(dateData,"currentDate", new Date().DateToJSON())
        // );  
        _gameDate.GetDateFromJSON(
            ConvertJSON.SafeGet<Dictionary<string, object>>(dateData,"gameDate", new Dictionary<string, object>())
        );
        
        _sendFirebaseEventChannelSO._onSendEventRaised(
            "GamePlayUser",
            GameManager.instance.Nickname,
            "date.currentDate",
            _currentDate.DateToJSON()
        );
        
        SetDateUI();
        
        //object_count 가져오고
        int object_count =
            Convert.ToInt32(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "placeableObject_id"));
        
        //각각 object 정보 가져오고
        _placeManager.SetPlacedObjects(
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "placeableObjects"),
            object_count
        );
    }

    #region property

    public string Nickname => nickname;

    public long Money => money;

    public void SetMoney(long value, bool toServer = true)
    {
        if (value == 0)
        {
            toServer = true;
        }
        money = value;

        if(toServer)
            _sendFirebaseEventChannelSO.RaiseEvent("GamePlayUser", nickname, "money", money);

        //서버 로딩
        ui_manager.SetMoneyText(money);
    }


    public int Employee_count
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return employee_count; }
        set
        {
            employee_count = value;
            _sendFirebaseEventChannelSO.RaiseEvent(
                "GamePlayUser",
                nickname ,
                "employee_count", 
                employee_count
            );
            //서버 로딩
            //ui_manager.SetMoneyText(value);
        }
    }

    public GameDate _Date
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return _gameDate; }
        set
        {
            _gameDate = value;
            //서버 로딩
            SetDateUI();
        }
    }

    public void AddDateMinute(int value)
    {
        _gameDate.Minute += value;
        recruitmentControllerSO.AddRandomApplicants(60 / value);
        SetDateUI();
    }

    public void SetDateUI()
    {
        ui_manager.SetDateText(_gameDate);
    }

    #endregion


    enum Reputation
    {
        single = 0, //혼자하는 느낌
        teamProject = 1, //조별수준
        club = 2, //동아리
        startup = 3, //스타트업 [동]


        //지역 대표
        //도 대표
        //국가 대표
        //대륙 대표
        //글로벌
    }

}

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
    [Header("Manager")] [SerializeField] RecruitmentManagerSO recruitmentControllerSO;
    [SerializeField] MissionManagerSO missionControllerSO;
    [SerializeField] EmployeeManagerSO employeeControllerSO;
    [SerializeField] PlacedObjectManager _placeManager;
    [SerializeField] WorkstationManagerSO _workstationManagerSO;

    [Header("ServerEvent")] [SerializeField]
    private DeleteFirebaseEventChannelSO _deleteFirebaseEventChannelSO;

    [SerializeField] private SendFirebaseEventChannelSO _sendFirebaseEventChannelSO;
    [SerializeField] private GetJSONFirebaseEventChannelSO _getJSONEventChannelSO;

    /// <summary>
    /// 테스트모드에서 시작하자마자 깔아둘 오브젝트 하나. 책상만이 아니라 의자·컴퓨터·문까지
    /// 한 번에 놓을 수 있어야 해서(컴퓨터는 책상 위 + 의자가 있어야 근무 자리로 인정되고,
    /// 퇴근은 문이 있어야 한다) 프리팹과 위치를 묶어 배열로 받는다.
    /// </summary>
    [Serializable]
    public struct TestPlacement
    {
        public GameObject prefab;

        //PlaceableObject가 이 값을 '시작 모서리'로 보고 피벗을 역산한다 (SetPlacedObjectData 참고)
        public Vector3 position;

        //y축 회전(도). 타일 격자에 맞춰야 하므로 0/90/180/270만 의미가 있다.
        public int rotation;
    }

    [Header("Test (서버/로그인 없이 테스트할 때 자동 배치)")]
    [SerializeField] private bool _testMode = false;
    [SerializeField] private TestPlacement[] _testPlacements;
    [SerializeField] private bool _testSpawnEmployee = false;

    //K를 누르면 이만큼(분) 시간을 한 번에 건너뛴다. 기본 3시간 - 출퇴근(문) 같은 시간 경계
    //이벤트를 실제로 몇 초씩 기다리지 않고 바로 확인하기 위한 디버그용.
    [SerializeField] private int _testTimeJumpMinutes = 180;


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

        //테스트모드에서 J를 누르면 채용 UI를 거치지 않고 직원을 바로 하나 추가한다.
        if (_testMode && Input.GetKeyDown(KeyCode.J))
        {
            SpawnTestEmployee();
        }

        //테스트모드에서 K를 누르면 시간을 한 번에 _testTimeJumpMinutes만큼 건너뛴다.
        //TimeButton과 같은 경로(AddDateMinute)라 수입 정산·서버 저장이 그대로 같이 따라온다.
        //IsDateReady 전에 누르면 _gameDate가 비어 있어 GameClock처럼 무시한다.
        if (_testMode && IsDateReady && Input.GetKeyDown(KeyCode.K))
        {
            AddDateMinute(_testTimeJumpMinutes);
        }
    }

    void Start()
    {
        recruitmentControllerSO.Init();
        missionControllerSO.Init();
        employeeControllerSO.Init();

        _placeManager.Init();
        _workstationManagerSO.Init();

        _gameDate = new GameDate(_sendFirebaseEventChannelSO);

        GameServerStart();
    }

    public async void GameServerStart()
    {
        //PersistentManager(AuthManager/FirestoreManager)가 없는 테스트 실행이면 Firebase를 아예 건들지 않는다.
        //FirebaseApp.CheckAndFixDependenciesAsync()는 AuthManager가 호출하므로, PersistentManager 없이
        //FirebaseAuth.DefaultInstance에 접근하면 초기화 예외가 나고, GameServerStart가 async void라
        //그 예외가 그대로 터져서 아래 SetDefaultProperty()까지 도달하지 못한다.
        if (_getJSONEventChannelSO._onEventRaised == null)
        {
            Debug.Log("[GameManager] 서버 매니저(PersistentManager)가 없어 로컬 테스트 데이터로 시작합니다.");
            SetDefaultProperty();
            return;
        }

        try
        {
            auth = FirebaseAuth.DefaultInstance;
            user = auth.CurrentUser;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[GameManager] Firebase 초기화 전이라 로컬 테스트 데이터로 시작합니다. ({e.Message})");
            SetDefaultProperty();
            return;
        }

        //로그인되어 있지 않으면 서버 호출 없이 로컬 기본값으로 테스트 플레이한다.
        if (user == null)
        {
            Debug.Log("[GameManager] 로그인되어 있지 않아 로컬 테스트 데이터로 시작합니다.");
            SetDefaultProperty();
            return;
        }

        Debug.Log(user.Email);

        //서버에게 number 받는 거는 무조건 long 으로 해야한다, ToInt32
        //타입이 64비트가 나온다. => 8바이트 => long
        //int는 4바이트
        //convert로 하면 null이 0으로 바뀌어진다

        nickname = (string)await _getJSONEventChannelSO.RaiseEvent("User", user.Email, "nickname");

        money = (long)(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "money") ?? (long)0);
        SetMoney(money, false);

        employee_count =
            Convert.ToInt32(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "employee_count"));


        //MissionController.instance.Init();
        missionControllerSO.SetMissionData(
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "missions"),
            Convert.ToInt32(await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "mission_count"))
        );

        Dictionary<string, object> recruitments =
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname,
                "recruitments");
        recruitmentControllerSO.JSONToRecruitments(recruitments);

        Dictionary<string, object> employees =
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname, "employees");
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
            ConvertJSON.SafeGet<Dictionary<string, object>>(dateData, "gameDate", new Dictionary<string, object>())
        );

        //RaiseEvent는 구독자가 없으면 조용히 넘어간다. 델리게이트를 직접 부르면 로컬 테스트에서 터진다.
        _sendFirebaseEventChannelSO.RaiseEvent(
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
            (Dictionary<string, object>)await _getJSONEventChannelSO.RaiseEvent("GamePlayUser", nickname,
                "placeableObjects"),
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

        if (toServer)
            _sendFirebaseEventChannelSO.RaiseEvent("GamePlayUser", nickname, "money", money);

        //서버 로딩
        ui_manager.SetMoneyText(money);
    }

    /// <summary>매달 직원 월급을 일괄 지급(차감)한다. 돈이 부족하면 false.</summary>
    public bool PayEmployees()
    {
        return employeeControllerSO.PayEmployees();
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
                nickname,
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

    /// <summary>날짜 로딩이 끝나 시간을 흘려도 되는 상태인지. GameClock이 이걸 보고 기다린다.</summary>
    public bool IsDateReady => _gameDate != null && _gameDate.IsLoaded;

    /// <param name="toServer">
    /// 서버에 바로 쓸지. GameClock처럼 자주 부르는 쪽은 false로 넘기고 나중에 SaveDate()로 몰아서 쓴다.
    /// 매번 쓰면 초당 1회 쓰기가 되어 Firestore 할당량이 남아나지 않는다.
    /// </param>
    public void AddDateMinute(int value, bool toServer = true)
    {
        //AddRandomApplicants의 60/value가 0으로 나누기가 된다
        if (value <= 0)
        {
            return;
        }

        _gameDate.SetMinute(_gameDate.Minute + value, toServer);
        recruitmentControllerSO.AddRandomApplicants(60 / value);

        //근무중인 직원이 번 돈. 시간이 흐르는 곳에 붙여야 빨리감기(TimeButton)와 자동 시계가 같은 양을 번다.
        //체력을 실제 시간(Time.deltaTime)으로 깎는 EmployeeWorkAI와 달리 여기는 게임 시간 기준이다.
        long earned = employeeControllerSO.CollectIncome(value);

        if (earned > 0)
        {
            //toServer를 그대로 넘긴다 - GameClock은 false로 불러 로컬에만 쌓고 SaveMoney()로 몰아서 쓴다
            SetMoney(money + earned, toServer);
        }

        SetDateUI();
    }

    /// <summary>지금 재화를 서버에 쓴다. 수입이 틱마다 들어오므로 날짜와 같이 몰아서 쓴다.</summary>
    public void SaveMoney()
    {
        //로그인 전(nickname이 없는 로컬 테스트)에는 쓸 곳이 없다
        if (string.IsNullOrEmpty(nickname))
        {
            return;
        }

        _sendFirebaseEventChannelSO.RaiseEvent("GamePlayUser", nickname, "money", money);
    }

    /// <summary>지금 날짜를 서버에 쓴다. 틱마다 쓰지 않고 모았다가 부르는 용도.</summary>
    public void SaveDate()
    {
        if (!IsDateReady)
        {
            return;
        }

        _gameDate.SetDateToServer(_gameDate.DateToJSON());
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

    private void SetDefaultProperty()
    {
        //default information
        nickname = "gest";
        SetMoney(0, false);
        employee_count = 0;
        missionControllerSO.SetMissionData(new Dictionary<string, object>(), 0);
        recruitmentControllerSO.JSONToRecruitments(new Dictionary<string, object>());
        employeeControllerSO.JSONToEmployees(new Dictionary<string, object>());
        _currentDate = new Date(true);

        _gameDate.GetDateFromJSON(
            new Dictionary<string, object>()
            {
                { "year", 2020 },
                { "month", 1 },
                { "day", 1 },
                { "week", Week.WED },
                { "hour", 0 },
                { "minute", 0 },
            }
        );
        SetDateUI();
        // object information

        if (_testMode)
        {
            SpawnTestSetup();
        }
    }

    /// <summary>
    /// 서버/로그인 없이 테스트할 때 배치 UI를 거치지 않고 _testPlacements의 오브젝트들과 직원을 바로 만들어준다.
    /// </summary>
    private void SpawnTestSetup()
    {
        if (_testPlacements != null)
        {
            for (int i = 0; i < _testPlacements.Length; i++)
            {
                PlaceTestObject(_testPlacements[i], i);
            }
        }

        if (_testSpawnEmployee)
        {
            SpawnTestEmployee();
        }
    }

    /// <summary>
    /// 테스트 배치 하나를 실제로 놓는다.
    ///
    /// object id로 배열 인덱스를 그대로 쓴다. 예전에는 0으로 고정돼 있었는데, 그러면 두 개 이상 놓는 순간
    /// WorkstationManagerSO가 id로 관리하는 자리 표(_seatOwners)에서 전부 같은 자리로 취급돼
    /// 직원 한 명만 앉을 수 있게 된다. 테스트모드에는 서버 데이터가 없으니 인덱스로 충분하다.
    /// </summary>
    private void PlaceTestObject(TestPlacement placement, int index)
    {
        if (placement.prefab == null)
        {
            return;
        }

        GameObject obj = Instantiate(placement.prefab, placement.position, Quaternion.identity);
        PlaceableObject placeableObject = obj.GetComponent<PlaceableObject>();

        if (placeableObject == null)
        {
            Debug.LogWarning(
                $"[GameManager] _testPlacements[{index}]의 '{placement.prefab.name}'에 PlaceableObject 컴포넌트가 없습니다.",
                placement.prefab);
            return;
        }

        //property_id는 프리팹이 들고 있는 ObjectSO에서 그대로 읽는다 (예전엔 0으로 박혀 있었다)
        placeableObject.SetPlacedObjectData(
            new PlacedObjectData(index, placeableObject.GetPropertyID(), placement.position, placement.rotation));

        placeableObject.Place();

        //책상이면 자리 풀에, 문이면 출입구 목록에 들어간다 (RegisterWorkstation이 타입 보고 나눈다)
        _workstationManagerSO.RegisterWorkstation(placeableObject);
    }

    /// <summary>
    /// 테스트모드에서 채용 UI(지원자 뽑기)를 거치지 않고 직원을 바로 하나 만들어 고용시킨다.
    /// SpawnTestSetup()의 시작 시 자동 생성과, Update()의 J 키 단축키가 이 함수를 함께 쓴다.
    /// </summary>
    private void SpawnTestEmployee()
    {
        Employee employee = new Employee(employeeControllerSO, true);
        employee.ID = Employee_count;
        Employee_count = employee.ID + 1;
        employee.Name = "테스트 직원 " + employee.ID;
        employee.Age = 20;
        employee.Max_Stamina = 100;
        employee.SetStamina(100, false);
        employee.Max_Mental = 100;
        employee.Mental = 100;
        employee.CareerPeriod = 0;
        employee.Salary = 1000000;

        //_WorkTime을 안 채우면 구조체 기본값(0~0)이 되고, EmployeeWorkAI.IsWorkTime()이
        //start==end를 "근무시간 0시간"으로 봐서 몇 시로 점프해도 영원히 OffDuty로 멈춘다.
        //서버 데이터 로딩 시 쓰는 기본값(9~18시)과 맞춘다.
        employee._WorkTime = new WorkTime(9.0f, 18.0f);

        employee._EmployeeSO = recruitmentControllerSO.GetEmployeeSO(0);

        employeeControllerSO.CreateEmployee(employee);
    }
}
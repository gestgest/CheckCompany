using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] UIManager ui_manager;
    [SerializeField] FireStoreManager fireStoreManager;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private string nickname;

    //int executive = 1; //임원, 임원 생성할때 이거 참조해야한다
    int employee_count = 0;
    long money;
    private Date date;
    
    //Reputation reputation = Reputation.single; //레벨 [명예]
    //int exp = 0;

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
        //절대로 LoginScene에 넣지마 => 메인 스레드 충돌 오류
        fireStoreManager.Init();
        date = new Date();
        //SetDateUI();
    }
    
    public async void GameStart()
    {
        //Auth로 가져오고
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        //만약 로그인 안했을 경우 무조건 디폴트 계정을 넣어야 한다.★★★
        if (user == null)
        {
            //디폴트 계정
        }
        Debug.Log(user.Email);


        //서버에게 number 받는 거는 무조건 long 으로 해야한다, ToInt32
        //타입이 64비트가 나온다. => 8바이트 => long
        //int는 4바이트

        //user.Email으로 쿼리 만들고
        nickname = (string)await fireStoreManager.GetFirestoreData("User", user.Email, "nickname");
        SetMoney((long)await fireStoreManager.GetFirestoreData("GamePlayUser", nickname, "money"), false);
        employee_count = Convert.ToInt32(await fireStoreManager.GetFirestoreData("GamePlayUser", nickname, "employee_count"));
        date.GetDateFromJSON((Dictionary<string, object>)await fireStoreManager.GetFirestoreData("GamePlayUser", nickname, "date"));
        SetDateUI();

        MissionController.instance.Init();

        Dictionary<string,object> recruitments = (Dictionary<string, object>)await fireStoreManager.GetFirestoreData("GamePlayUser", nickname, "recruitments");
        RecruitmentController.instance.GetRecruitmentsFromServer(recruitments);

        Dictionary<string, object> employees = (Dictionary<string, object>)await fireStoreManager.GetFirestoreData("GamePlayUser", nickname, "employees");
        EmployeeController.instance.GetEmployeesFromJSON(employees);
    }

    #region property

    public string Nickname => nickname;

    public long Money => money;

    public void SetMoney(long value, bool toServer = true)
    {
        //Debug.Log("돈 서버에게 입력 받음 : " + value);
        money = value;

        if(toServer)
            fireStoreManager.SetFirestoreData("GamePlayUser", nickname, "money", money);

        //서버 로딩
        ui_manager.SetMoneyText(value);
    }


    public int Employee_count
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return employee_count; }
        set
        {
            //Debug.Log("돈 서버에게 입력 받음 : " + value);
            employee_count = value;
            fireStoreManager.SetFirestoreData(
                "GamePlayUser",
                nickname ,
                "employee_count", 
                employee_count
            );
            //서버 로딩
            //ui_manager.SetMoneyText(value);
        }
    }

    public Date _Date
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return date; }
        set
        {
            date = value;
            //서버 로딩
            SetDateUI();
        }
    }

    public void AddDateMinute(int value)
    {
        date.Minute += value;
        RecruitmentController.instance.AddApplicants(60 / value);
        SetDateUI();
    }

    public void SetDateUI()
    {
        ui_manager.SetDateText(date);
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

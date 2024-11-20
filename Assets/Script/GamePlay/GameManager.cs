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

    //int executive = 1; //임원수
    int executive_count = 0;
    long money;
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

    void Start()
    {
        //절대로 LoginScene에 넣지마 => 메인 스레드 충돌 오류
        fireStoreManager.Init();
    }

    public async void Init()
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

        //user.Email으로 쿼리 만들고
        nickname = (string)await fireStoreManager.GetFirestore("User", user.Email, "nickname");
        Money = (long)await fireStoreManager.GetFirestore("GamePlayUser", nickname, "money");
        //서버에게 number 받는 거는 무조건 long 으로 해야한다
        //타입이 64비트가 나온다. => 8바이트 => long
        //int는 4바이트
    }

    public long Money
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return money; }
        set
        {
            //Debug.Log("돈 서버에게 입력 받음 : " + value);
            money = value;
            //fireStoreManager.SetFirestore("GamePlayUser", "milkan660" ,"money", money);
            //서버 로딩
            ui_manager.SetMoneyText(value);
        }
    }

    public int Executive_count
    {
        //애초에 서버에 데이터를 넣는 게 낫지 않나
        get { return executive_count; }
        set
        {
            //Debug.Log("돈 서버에게 입력 받음 : " + value);
            executive_count = value;
            //fireStoreManager.SetFirestore("GamePlayUser", "milkan660" ,"money", money);
            //서버 로딩
            //ui_manager.SetMoneyText(value);
        }
    }

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

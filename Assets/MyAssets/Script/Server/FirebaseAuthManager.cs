using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;


public class FirebaseAuthManager : MonoBehaviour
{
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser user;
    [SerializeField] private Image _debugIcon;

    [Header("Listening to eventChannels")]
    [SerializeField] private String2EventChannelSO _loginEvent;//UILoginMenu
    [SerializeField] private String4EventChannelSO _registerEvent;
    [SerializeField] private String2EventChannelSO _debugLoginEvent;
    
    [Space]
    //서버 send 함수
    [Header("Broadcasting on firebaseChannels")]
    [SerializeField] private VoidEventChannelSO _initFirebaseChannelEvent;
    [SerializeField] private SendFirebaseEventChannelSO _setNewFireStoreEvent;
    [SerializeField] private SendFirebaseEventChannelSO _setFireStoreEvent;

    [SerializeField] private BoolEventChannelSO _isLoginEvent; 
    [SerializeField] private LoadEventChannelSO _loadLocation; //sceneLoader?
    [SerializeField] private AssetReference _myCompanyScene;

    private bool isInit = true;
    void Awake()
    {
        //파이어베이스 서버 체크 => 전역 무언가를 생성
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            //이용가능하다면
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitFirebase();
                _initFirebaseChannelEvent.RaiseEvent();
            }
            else
            {
                Debug.LogError("연결 오류" + dependencyStatus);
            }
        });
        
        //이게 CheckAndFixDependenciesAsync함수를 동시에 실행되면 맛탱이가 가나.
        //정보. 파이어베이스를 두개가 동시에 실행한다면 그냥 유니티가 맛이 감
        //예를 들어 Auth와 FireStore가 동시에 실행되는 Start()면 맛이 감
    }

    private void OnEnable()
    {
        _debugIcon.color = Color.blue;
        _loginEvent._onEventRaised += Login;
        _registerEvent._onEventRaised += Register;
    }

    private void OnDisable()
    {
        _debugIcon.color = Color.red;
        _loginEvent._onEventRaised -= Login;
        _registerEvent._onEventRaised -= Register;
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance; //싱글톤으로 디폴트 FirebaseAuth 생성

        auth.StateChanged += AuthStatusChanged;
        AuthStatusChanged(this, null);
    }


    void AuthStatusChanged(object sender, System.EventArgs eventArgs)
    {
        try
        {
            bool userSame = auth.CurrentUser == user;
            if (!userSame)
            {
                bool signedIn = auth.CurrentUser != null;

                if (!signedIn && user != null)
                {
                    // 로그아웃 처리
                }

                user = auth.CurrentUser;
                if (signedIn)
                {
                    LoginEvent(true);
                }
                
            }
            LoginEvent(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthStatusChanged Error] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void LoginEvent(bool isLogin)
    {
        if (isInit)
        {
            isInit = false;
            _isLoginEvent.RaiseEvent(isLogin);
        }

    }
    
    public void Login(string email, string password)
    {
        _debugLoginEvent.RaiseEvent("어떻게든 나의 노력이 주변 사람들의 눈에 띄도록", "");

        StartCoroutine(
            LoginAynsc(
            email,
            password
        ));
    }

    private IEnumerator LoginAynsc(string email, string password)
    {
        _debugLoginEvent.RaiseEvent("안녕 난 엄준식이라고 해", "");

        //이메일 로그인 비동기 현황 변수
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        _debugLoginEvent.RaiseEvent("준식준식", "");

        yield return new WaitUntil(() => loginTask.IsCompleted);

        //만약 로그인 테스크가 계속 실행중이라면
        if (loginTask.Exception != null)
        {
            _debugLoginEvent.RaiseEvent("바이 : "+ loginTask.Exception, "");

            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because ";


            //보안을 위해 case에 상관없이 Login Failed?
            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missiong";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missiong";
                    break;
                default:
                    failedMessage += "Login Failed";
                    break;
            }
            _debugLoginEvent.RaiseEvent(failedMessage, "");
            Debug.LogError(authError);
        }
        else //로그인 성공
        {
            user = loginTask.Result.User;
            _loadLocation.RaiseEvent(_myCompanyScene);
        }
    }

    public void Register(string name, string email, string password, string confirmPassword)
    {
        StartCoroutine(RegisterAynsc(name,email,password,confirmPassword));
    }

    //비동기
    private IEnumerator RegisterAynsc(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            Debug.LogError("이름 넣어라");
        }
        else if (email == "")
        {
            Debug.LogError("이메일 넣어라");
        }
        else if (password == "")
        {
            Debug.LogError("비밀번호 넣어라");
        }
        else if (confirmPassword != password)
        {
            Debug.LogError("비밀번호 매치 안됨");
        }
        else
        {
            //이메일과 비밀번호로 비동기 생성
            Task<AuthResult> registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            //완료할때까지 대기
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Register Failed! Because ";

                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid";
                        break;
                    //일부로 보안때문에 case 더 안넣음
                    default:
                        failedMessage = "Registration Failed";
                        break;
                }

                Debug.LogError(failedMessage);
            }
            else //등록
            {
                user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile { DisplayName = name };

                Task updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    //유저 내용 제거
                    user.DeleteAsync();

                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    Debug.LogError("오류" + authError);
                }
                else
                {
                    //회원가입 성공
                    Debug.Log("회원가입 성공");

                    //user 닉네임 Document
                    _setNewFireStoreEvent.RaiseEvent("User", user.Email, "nickname", name);
                    GamePlayerInit(name);
                    PanelManager.instance.SwitchingPanelFromInt(1); //로그인 화면으로
                }

            }

        }
    }

    //초기값 설정
    private void GamePlayerInit(string name)
    {
        _setNewFireStoreEvent.RaiseEvent("GamePlayUser", name, "money", 0);
        _setFireStoreEvent.RaiseEvent("GamePlayUser", name, "employee_count", 0);
        Date date = new Date();
        _setFireStoreEvent.RaiseEvent("GamePlayUser", name, "date", date.DateToJSON());
        _setFireStoreEvent.RaiseEvent("GamePlayUser", name, "recruitments", new Dictionary<string, object>());
        _setFireStoreEvent.RaiseEvent("GamePlayUser", name, "employees",  new Dictionary<string, object>());
    }
}
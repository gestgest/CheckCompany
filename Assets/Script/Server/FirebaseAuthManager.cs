using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Extensions;
using TMPro;
using UnityEngine.AddressableAssets;


public class FirebaseAuthManager : MonoBehaviour
{
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser user;

    [Header("Listening to eventChannels")]
    [SerializeField] private String2EventChannelSO _loginEvent;//UILoginMenu
    [SerializeField] private String4EventChannelSO _registerEvent;
    
    [Space]
    //서버 send 함수
    [Header("Broadcasting on firebaseChannels")]
    [SerializeField] private VoidEventChannelSO _initFirebaseChannelEvent;
    [SerializeField] private SendFirebaseEventChannelSO _setNewFireStoreEvent;
    [SerializeField] private SendFirebaseEventChannelSO _setFireStoreEvent;

    [SerializeField] private BoolEventChannelSO _isLoginEvent;
    [SerializeField] private LoadEventChannelSO _loadLocation; //sceneLoader?
    [SerializeField] private AssetReference _myCompanyScene;

    [SerializeField] private VoidEventChannelSO _autoLoginRequestEvent; //UILoginMenu가 씬 진입 시 호출

    private bool _initialLoginBroadcastSent = false;
    [SerializeField] private float _initialLoginGraceSeconds = 2f; //세션 복원을 기다려줄 최대 시간

    //Firebase가 로그인 여부를 판단하기 전에 UILoginMenu가 자동로그인을 요청할 수도 있으므로 상태를 별도로 저장
    private bool _hasCheckedInitialLogin = false;
    private bool _isSignedIn = false;
    private bool _autoLoginRequested = false;
    private bool _autoLoginHandled = false;
    void Awake()
    {
        //FirebaseApp.DefaultInstance

        try
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
        }
        catch (Exception e)
        {
            Debug.LogError($"[AuthStatusChanged Error] {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
        }
        
        //이게 CheckAndFixDependenciesAsync함수를 동시에 실행되면 맛탱이가 가나.
        //정보. 파이어베이스를 두개가 동시에 실행한다면 그냥 유니티가 맛이 감
        //예를 들어 Auth와 FireStore가 동시에 실행되는 Start()면 맛이 감
    }

    private void OnEnable()
    {
        _loginEvent._onEventRaised += Login;
        _registerEvent._onEventRaised += Register;
        _autoLoginRequestEvent._onEventRaised += TryAutoLogin;
    }

    private void OnDisable()
    {
        _loginEvent._onEventRaised -= Login;
        _registerEvent._onEventRaised -= Register;
        _autoLoginRequestEvent._onEventRaised -= TryAutoLogin;
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance; //싱글톤으로 디폴트 FirebaseAuth 생성

        auth.StateChanged += AuthStatusChanged;
        AuthStatusChanged(this, null);
        StartCoroutine(FallbackToNotLoggedInAfterGrace());
    }

    //StateChanged가 맨 처음엔(파이어베이스가 저장된 세션을 아직 복원하기 전) false로 잘못 알려주고
    //복원이 끝난 뒤 다시 true로 알려주는 경우가 있다.
    //로그인 확정(true)은 확인되는 즉시 반영하고, 로그인 안 됨(false)은 일정 시간 기다려도
    //true가 안 오면 그때 확정짓는다. (그래야 진짜 로그인 상태를 false로 잘못 확정짓지 않는다)
    private IEnumerator FallbackToNotLoggedInAfterGrace()
    {
        yield return new WaitForSecondsRealtime(_initialLoginGraceSeconds);
        BroadcastInitialLoginStateOnce(auth.CurrentUser != null);
    }

    private void BroadcastInitialLoginStateOnce(bool isLogin)
    {
        if (_initialLoginBroadcastSent) return;
        _initialLoginBroadcastSent = true;
        _isLoginEvent.RaiseEvent(isLogin);
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
                    BroadcastInitialLoginStateOnce(true);
                }

            }

            //isInit 가드와 별개로 매 호출마다 실제 로그인 상태를 다시 확인.
            UpdateAutoLoginState();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AuthStatusChanged Error] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }

    //LoginMenu씬에 진입했을 때 UILoginMenu가 호출. 기존에 로그인된 정보(파이어베이스가 기억하는 세션)가 있다면 바로 게임씬으로 진입시킨다.
    private void TryAutoLogin()
    {
        _autoLoginRequested = true;
        if (_hasCheckedInitialLogin)
            ProceedAutoLogin();
        //아직 판단 전이면 UpdateAutoLoginState()가 결과 나올 때 알아서 처리
    }

    private void UpdateAutoLoginState()
    {
        _hasCheckedInitialLogin = true;
        _isSignedIn = auth.CurrentUser != null;

        if (_autoLoginRequested)
            ProceedAutoLogin();
    }

    private void ProceedAutoLogin()
    {
        if (_autoLoginHandled) return;
        if (!_isSignedIn) return;

        _autoLoginHandled = true;
        _loadLocation.RaiseEvent(_myCompanyScene);
    }

    public void Login(string email, string password)
    {
        StartCoroutine(
            LoginAynsc(
            email,
            password
        ));
    }

    private IEnumerator LoginAynsc(string email, string password)
    {

        //이메일 로그인 비동기 현황 변수
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        //만약 로그인 테스크가 계속 실행중이라면
        if (loginTask.Exception != null)
        {

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
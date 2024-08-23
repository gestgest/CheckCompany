using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.UI;


public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    private DependencyStatus dependencyStatus;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private FirebaseUser user;

    [Space]
    [Header("Login")]
    [SerializeField] private InputField emailLoginTextField;
    [SerializeField] private InputField passwordLoginTextField;

    [SerializeField] private InputField nameRegisterTextField;
    [SerializeField] private InputField emailRegisterTextField;
    [SerializeField] private InputField passwordRegisterTextField;
    [SerializeField] private InputField confirmPasswordRegisterTextField;


    // Start is called before the first frame update
    void Awake()
    {
        //전역 무언가를 생성
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitFirebase();
            }
            else
            {
                Debug.LogError("연결 오류" + dependencyStatus);
            }
        });
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStatusChanged;
        AuthStatusChanged(this, null);
    }

    void AuthStatusChanged(object sender, System.EventArgs eventArgs)
    {
        bool userSame = auth.CurrentUser == user;
        if (!userSame)
        {
            //로그인했는지
            bool signedIn = auth.CurrentUser != null;

            //로그인 안했는데 user값이 있다면
            if (!signedIn && user != null)
            {
                //로그아웃
            }
            user = auth.CurrentUser;

            if (signedIn)
            {
                //로그인
            }
        }
    }

    void Login()
    {
        StartCoroutine(LoginAynsc(emailLoginTextField.text, passwordLoginTextField.text));
    }

    private IEnumerator LoginAynsc(string email, string password)
    {
        //이메일 로그인 비동기 현황 변수
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        //만약 로그인 테스크가 계속 실행중이라면
        if(loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because ";


            //보안을 위해 case에 상관없이 Login Failed?
            switch(authError)
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

        }
    }

    private void Register()
    {
        //startCoroutine
    }

    private IEnumerator RegisterAynsc(string name, string email, string password, string confirmPassword)
    {
        if(name == "")
        {
            Debug.LogError("이름 넣어라");
        }
        yield return null;
    }


}

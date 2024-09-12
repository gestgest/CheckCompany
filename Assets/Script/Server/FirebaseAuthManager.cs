using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.ComponentModel;
using TMPro;


public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    private DependencyStatus dependencyStatus;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private FirebaseUser user;

    [Space]
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginTextField;
    [SerializeField] private TMP_InputField passwordLoginTextField;

    [SerializeField] private TMP_InputField nameRegisterTextField;
    [SerializeField] private TMP_InputField emailRegisterTextField;
    [SerializeField] private TMP_InputField passwordRegisterTextField;
    [SerializeField] private TMP_InputField confirmPasswordRegisterTextField;
    [SerializeField] private SceneLoader sceneLoader;

    void Awake()
    {
        //파이어베이스 서버 체크 => 전역 무언가를 생성
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            //이용가능하다면
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
        auth = FirebaseAuth.DefaultInstance; //싱글톤으로 디폴트 FirebaseAuth 생성

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

    public void Login()
    {
        StartCoroutine(LoginAynsc(emailLoginTextField.text, passwordLoginTextField.text));
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
            Debug.Log("성공" + user);
            SceneLoad();
        }
    }

    public void Register()
    {
        StartCoroutine(
            RegisterAynsc(
                nameRegisterTextField.text,
                emailRegisterTextField.text,
                passwordRegisterTextField.text,
                confirmPasswordRegisterTextField.text
        ));
    }

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
                    //일부로 case 더 안넣음
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
                }

            }

        }
    }

    //씬 로드
    public void SceneLoad()
    {
        sceneLoader.SceneLoad();
    }
}
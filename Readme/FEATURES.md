# 주요 화면
CheckCompany의 주요 화면

## 로그인 화면
<img width="404" height="878" alt="image" src="https://github.com/user-attachments/assets/9c52cf2e-ee90-47f0-a87d-408fa8430da1" /> 라이트모드<br>
<img width="404" height="878" alt="image" src="https://github.com/user-attachments/assets/9c52cf2e-ee90-47f0-a87d-408fa8430da1" /> 다크모드


메뉴<br><br>
<img width="407" height="883" alt="image" src="https://github.com/user-attachments/assets/8fb99001-636a-4307-9060-bb44d4f2dc18" /><br>
로그인 화면<br><br>

<img width="407" height="883" alt="image" src="https://github.com/user-attachments/assets/4e9f9aa2-bf62-4064-86dc-ec6b4bda2eb0" /><br>
회원가입 화면<br><br>

### 서버 통신 코드
```
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
```

---
## 게임 화면
  <img width="404" height="884" alt="image" src="https://github.com/user-attachments/assets/f8f9a9ed-139c-4f37-8358-46025570797e" /> <br>
  직원<br><br>

  <img width="406" height="881" alt="image" src="https://github.com/user-attachments/assets/7e7663e6-ea94-4ca0-9568-aada9a06c516" /><br>
  모집 생성 창<br><br>

  <img width="405" height="883" alt="image" src="https://github.com/user-attachments/assets/d9ddc794-0db3-4de4-bc65-96bd159cc1e5" /><br>
  모집 공고 창<br><br>

### 미션
<img width="411" height="879" alt="image" src="https://github.com/user-attachments/assets/8ae13b57-d02f-40bd-bfad-e8749b83e393" />
<img width="403" height="876" alt="image" src="https://github.com/user-attachments/assets/c4c41566-794f-4534-89ee-865fbe489f1c" />
<img width="408" height="877" alt="image" src="https://github.com/user-attachments/assets/476930e1-868d-46c4-acc5-1c69212640b0" />


<img width="404" height="878" alt="image" src="https://github.com/user-attachments/assets/ed4e74e3-7eb2-4e99-a55e-be538c3834ad" />
<img width="405" height="882" alt="image" src="https://github.com/user-attachments/assets/d25ec010-1ad1-4506-af40-c3bb2ad16d33" />
<img width="407" height="884" alt="image" src="https://github.com/user-attachments/assets/d3394b26-7f6a-4b50-bfb4-16cf87022d04" />
  
### 배치
<img width="410" height="885" alt="image" src="https://github.com/user-attachments/assets/6425d4b5-921e-4024-8ea4-916f221d32e9" />

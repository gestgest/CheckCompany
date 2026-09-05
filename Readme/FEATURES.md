# 게임플레이

CheckCompany의 주요 화면과 기능을 정리합니다.

---

## 1. 시작 & 로그인

라이트/다크 모드는 기기의 시스템 설정을 따라가며, `ThemeApplier`가 UI 색상과 3D 로비(빌딩) 조명을 함께 전환합니다.

### 시작 메뉴

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/9c52cf2e-ee90-47f0-a87d-408fa8430da1" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/ec387e0f-5c2b-4e7e-9c23-c95c0fc3deb0" /> |

### 로그인

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/326143b3-e5aa-4d89-b423-85aaf92c74ea" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/7a9e9005-8b8d-48cf-b030-6757e9cf7911" /> |

### 회원가입

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/a5b11a29-4d4f-4ce2-9a90-c47c67aea42c" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/466eb7b3-b033-4b98-a417-300058457a1a" /> |

### 서버 통신 (Firebase Auth)

로그인 요청은 코루틴으로 비동기 처리하고, `AuthError`를 코드별로 분기해 실패 사유를 안내합니다.

```csharp
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

## 2. 직원 & 채용

<img src="https://github.com/user-attachments/assets/f8f9a9ed-139c-4f37-8358-46025570797e" width="240"/>

재직 중인 직원 목록. 이름·경력·월급을 확인하고 해고할 수 있습니다.

| 모집 생성 | 모집 공고(지원자 목록) |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/7e7663e6-ea94-4ca0-9568-aada9a06c516" width="240"/> | <img src="https://github.com/user-attachments/assets/d9ddc794-0db3-4de4-bc65-96bd159cc1e5" width="240"/> |

직원 유형·레벨·모집 기간을 정하면 비용이 계산되고, 기간이 지나면 지원자가 모집 공고 목록에 등록됩니다.

---

## 3. 미션

목표 달성을 게임의 성장 요소로 삼는 핵심 시스템입니다. 미션 하나는 여러 개의 todo 항목으로 이루어지고, 전부 체크되면 완료로 처리됩니다.

| 미션 목록 (진행 중) | 미션 생성 | 미션 수정 |
|:---:|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/8ae13b57-d02f-40bd-bfad-e8749b83e393" width="220"/> | <img src="https://github.com/user-attachments/assets/d3394b26-7f6a-4b50-bfb4-16cf87022d04" width="220"/> | <img src="https://github.com/user-attachments/assets/c4c41566-794f-4534-89ee-865fbe489f1c" width="220"/> |
| 진행 중인 미션을 펼치면 todo 체크리스트가 나온다 | 유형(Dev/Designer/QA/Product)·난이도·todo 항목을 정해 새 미션을 만든다 | 기존 미션의 유형·난이도·todo를 다시 편집한다 |

| 완료된 미션 목록 | 완료된 미션 상세 | 미션 달력 |
|:---:|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/476930e1-868d-46c4-acc5-1c69212640b0" width="220"/> | <img src="https://github.com/user-attachments/assets/ed4e74e3-7eb2-4e99-a55e-be538c3834ad" width="220"/> | <img src="https://github.com/user-attachments/assets/d25ec010-1ad1-4506-af40-c3bb2ad16d33" width="220"/> |
| 완료 처리된 미션만 모아본다 | todo가 전부 체크된 상세 내역을 다시 확인한다 | 날짜별로 어떤 미션을 완료했는지 달력에서 돌아본다 |

---

## 4. 오브젝트 배치

<img src="https://github.com/user-attachments/assets/85ede414-f83d-449c-95d8-fe1df6734ace" width="280"/>

사무실 바닥을 타일 격자로 관리하며, 회전 · 제거 · 설치 · 취소가 가능합니다. 놓을 수 있는 칸은 초록, 이미 다른 오브젝트와 겹치는 칸은 빨간 타일로 미리 보여줍니다.

배치 가능한 오브젝트: **의자 · 책상 · 컴퓨터 · 문**
(컴퓨터는 바닥이 아니라 반드시 책상 위에만 놓을 수 있고, 책상+의자+컴퓨터가 갖춰져야 실제 근무 자리로 인정됩니다.)

---

## 5. 낮과 밤 (Day-Night Cycle)

게임 내 시간(`GameDate`)에 맞춰 조명을 동적으로 조절합니다. 단순히 Directional Light(직사광) 밝기만 낮추면 스카이박스와 환경광(Ambient)이 그대로 남아 실제로는 어두워 보이지 않는 문제가 있어서, **직사광 Intensity · Ambient Intensity · 스카이박스 Exposure** 세 가지를 함께 게임 시각에 맞춰 보간하도록 구현했습니다. 일출·일몰 구간은 SmoothStep으로 급격한 전환을 완화합니다.

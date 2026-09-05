# 🎮 게임플레이

CheckCompany의 주요 화면과 기능을 정리합니다.

## 목차
1. [시작 & 로그인](#1-시작--로그인)
2. [직원 & 채용](#2-직원--채용)
3. [상점 & 오브젝트 배치](#3-상점--오브젝트-배치)
4. [자리 배정](#4-자리-배정)
5. [직원 근무 AI](#5-직원-근무-ai)
6. [미션](#6-미션)
7. [게임 시간 시스템](#7-게임-시간-시스템)
8. [낮과 밤 (Day-Night Cycle)](#8-낮과-밤-day-night-cycle)

---

## 1. 시작 & 로그인

라이트/다크 모드는 기기의 시스템 설정을 따라가며, `ThemeApplier`가 UI 색상과 3D 로비(빌딩) 조명을 함께 전환합니다.

`ThemeSO` 하나가 낮/밤 프리셋 전체(스카이박스·조명·UI 색상)를 담고, 화면의 각 UI 요소는 `ThemedGraphic` 컴포넌트로 자신의 역할(`UIRole`: Backdrop/Surface/Button/PrimaryText/PlaceholderText)만 표시해둡니다. 테마가 바뀌면 `ThemeApplier`가 이 역할표를 보고 색을 일괄로 다시 칠합니다 — 화면마다 색을 따로 관리하지 않아도 되는 구조입니다.

### 시작 메뉴

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/9c52cf2e-ee90-47f0-a87d-408fa8430da1" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/ec387e0f-5c2b-4e7e-9c23-c95c0fc3deb0" /> |

### 로그인

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/326143b3-e5aa-4d89-b423-85aaf92c74ea" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/3641198a-8dca-4e5c-ba76-5660aa5d2775" /> |

### 회원가입

| 라이트 모드 | 다크 모드 |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/a5b11a29-4d4f-4ce2-9a90-c47c67aea42c" width="240"/> | <img width="240" alt="image" src="https://github.com/user-attachments/assets/a7cb1d64-b069-44eb-99c9-4675570a5365" /> |

### 서버 통신 (Firebase Auth)

로그인 요청은 코루틴으로 비동기 처리하고, `AuthError`를 코드별로 분기해 실패 사유를 안내합니다.

<details>
<summary>코드 보기</summary>

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

        //보안을 위해 case에 상관없이 Login Failed?이지만 일단 AuthError 타입 확인을 위해 이렇게 만들었다.
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

</details>

---

## 2. 직원 & 채용

<img src="https://github.com/user-attachments/assets/f8f9a9ed-139c-4f37-8358-46025570797e" width="240"/>

재직 중인 직원 목록. 이름·경력·월급을 확인하고 해고할 수 있습니다.

| 모집 생성 | 채용현황(모집 중인 공고) |
|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/7e7663e6-ea94-4ca0-9568-aada9a06c516" width="240"/> | <img src="https://github.com/user-attachments/assets/d9ddc794-0db3-4de4-bc65-96bd159cc1e5" width="240"/> |

직원 유형·레벨·모집 기간을 정하면 비용이 계산되고, 기간이 지나면 지원자가 등록됩니다. 매달 정산 때 월급을 지급하지 못하면 전 직원의 체력·멘탈이 함께 깎입니다.

---

## 3. 상점 & 오브젝트 배치

오브젝트는 상점에서 카테고리(의자·책상·컴퓨터·문)별로 골라 구매한 뒤 바로 배치 모드로 들어갑니다.

[상점에서 카테고리를 골라 오브젝트를 구매하는 이미지]

<img src="https://github.com/user-attachments/assets/85ede414-f83d-449c-95d8-fe1df6734ace" width="280"/>

사무실 바닥을 타일 격자로 관리하며, 회전 · 제거 · 설치 · 취소가 가능합니다. 놓을 수 있는 칸은 초록, 이미 다른 오브젝트와 겹치는 칸은 빨간 타일로 미리 보여줍니다.

**배치 가능한 오브젝트**: 의자 · 책상 · 컴퓨터 · 문

> ⚠️ 컴퓨터는 바닥이 아니라 반드시 책상 위에만 놓을 수 있고, 책상+의자+컴퓨터가 갖춰져야 실제 근무 자리로 인정됩니다.

### 문 여닫기

문은 Animator 없이 코드로 직접 돌립니다. 경첩(Hinge) 위치를 축으로 회전시키고, 여러 직원이 동시에 드나들 때는 마지막 사람이 지나갈 때까지 열린 상태를 유지합니다 — 닫히는 도중에 다음 직원이 오면 그 각도에서 그대로 이어서 다시 열립니다.

[직원이 다가오면 문이 자동으로 열리는 이미지]

---

## 4. 자리 배정

컴퓨터 한 대가 자리 하나의 단위입니다. 책상 위에 컴퓨터가 있고 그 옆에 의자가 붙어 있어야 실제로 앉을 수 있는 근무 자리로 인정됩니다.

- 플레이어가 특정 자리를 눌러 직원을 직접 배정하거나 배정을 해제할 수 있습니다.
- 배정하지 않아도 근무시간이 된 직원은 빈 자리를 스스로 찾아 앉습니다.
- 배정된 책상을 다른 곳으로 옮기면, 이미 그 자리로 향하던(또는 앉아있던) 직원이 새 위치로 다시 걸어갑니다.

[자리를 눌러 직원을 배정하는 팝업 이미지]

---

## 5. 직원 근무 AI

직원은 근무시간(기본 9시~18시)에 맞춰 스스로 출근하고, 자리에 앉아 일하고, 퇴근합니다.

```
OffDuty ──출근시간──▶ GoingToDesk ──도착──▶ SittingDown ──▶ Working
   ▲                                                            │
   └── GoingHome(출입구로 이동) ◀── StandingUp ◀── 퇴근시간 ──────┘
              체력 0 ↓        ▲ 회복
                     Resting ─┘
```

- **길찾기**: NavMesh로 자리·출입구까지 이동하고, 걸어가는 도중 문 앞을 지나면 그 문을 자동으로 엽니다.
- **체력 관리**: 근무 중에는 체력이 서서히 줄고, 근무가 아닐 때(이동·대기·휴식)는 회복됩니다. 체력이 바닥나면 근무시간이라도 자리에서 일어나 쉬러 갑니다.
- **출퇴근 연출**: 퇴근하면 문으로 걸어나가 화면에서 사라지고, 다음 출근시간에 같은 문 앞에서 다시 나타나 자리로 걸어갑니다. (판단 로직 자체는 멈추지 않도록 렌더러와 길찾기만 꺼서, 자는 동안에도 체력 회복이 정상적으로 진행됩니다.)

[직원이 출근해서 자리에 앉는 과정을 담은 이미지]

---

## 6. 미션

목표 달성을 게임의 성장 요소로 삼는 핵심 시스템입니다. 미션 하나는 여러 개의 To-Do 항목으로 이루어지고, 전부 체크되면 완료로 처리됩니다.

미션에 직원을 배정해두면, 완료 시 그 직원의 **업무속도가 난이도에 비례해 영구히 상승**합니다. 수입이 업무속도에 비례하므로, 배정해둔 미션을 완료할수록 그 직원이 앞으로 버는 돈이 늘어납니다.

### 진행 중인 미션

| 미션 목록 (진행 중) | 미션 생성 | 미션 수정 | 직원 미션 할당 |
|:---:|:---:|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/8ae13b57-d02f-40bd-bfad-e8749b83e393" width="220" alt="미션 목록"/> | <img src="https://github.com/user-attachments/assets/d3394b26-7f6a-4b50-bfb4-16cf87022d04" width="220" alt="미션 생성"/> | <img src="https://github.com/user-attachments/assets/c4c41566-794f-4534-89ee-865fbe489f1c" width="220" alt="미션 수정"/> | <img src="https://github.com/user-attachments/assets/e1802dcd-2fac-442e-8960-475d806cafb4" width="220" alt="직원 미션 할당"/> |
| 진행 중인 미션을 펼쳐 To-Do 체크리스트를 확인합니다. | 직군 유형, 난이도, To-Do 항목을 지정하여 새 미션을 생성합니다. | 기존 미션의 유형, 난이도, To-Do 항목을 수정합니다. | 특정 미션에 담당 직원을 지정 및 할당합니다. |

### 완료된 미션

| 완료된 미션 목록 | 완료된 미션 상세 | 미션 달력 |
|:---:|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/476930e1-868d-46c4-acc5-1c69212640b0" width="220"/> | <img src="https://github.com/user-attachments/assets/ed4e74e3-7eb2-4e99-a55e-be538c3834ad" width="220"/> | <img src="https://github.com/user-attachments/assets/d25ec010-1ad1-4506-af40-c3bb2ad16d33" width="220"/> |
| 완료 처리된 미션만 모아본다 | To-Do가 전부 체크된 상세 내역을 다시 확인한다 | 날짜별로 어떤 미션을 완료했는지 달력에서 돌아본다 |

---

## 7. 게임 시간 시스템

실제 1초가 게임 10분으로 흘러갑니다(하루가 약 2분 24초). 직원의 출퇴근·근무 판단이 전부 이 게임 시간을 기준으로 이뤄집니다.
서버 쓰기 비용을 아끼기 위해 시간이 흐를 때마다 매번 저장하지 않고, 일정 틱마다(기본 하루에 한 번) 모아서 저장합니다.


---

## 8. 낮과 밤 (Day-Night Cycle)

게임 내 시간(`GameDate`)에 맞춰 조명을 동적으로 조절합니다.

단순히 Directional Light(직사광) 밝기만 낮추면 스카이박스와 환경광(Ambient)이 그대로 남아 실제로는 어두워 보이지 않는 문제가 있었습니다. 이를 해결하기 위해 아래 세 가지 값을 게임 시각에 맞춰 함께 보간하도록 구현했습니다.

- **직사광 Intensity**
- **Ambient Intensity**
- **스카이박스 Exposure**

일출·일몰 구간은 SmoothStep으로 급격한 전환을 완화합니다.

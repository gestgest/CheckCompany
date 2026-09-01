# CheckCompany 개선 TODO

분석 근거는 [README.md](README.md) 참고. 우선순위: 🔴 시급 → 🟠 중요 → 🟡 개선 → 🟢 정리

배치 시스템(가구 · 회전 · 이동 · 삭제 · 사무실 범위), 자동 시계, 직원 출퇴근, 근무 수입까지 돌아간다.
**게임 루프는 닫혔다** — 뽑고, 앉히고, 일하고, 번다. 지금 비어 있는 건 **번 돈을 쓸 곳**과
**화면에 보이는 것**이다.

---

## 🟣 진행 중 — 직원 AI 다듬기

상태 기계는 시간 축을 얻었다. `EmployeeWorkAI`가 `Employee._WorkTime`을 보고 출근하고,
퇴근하고, 체력이 바닥나면 쉰다. 남은 건 **연출**이다.

```
OffDuty ──출근시간──> GoingToDesk ──도착──> Working
   ↑                                          │
   └─ GoingHome(출입구로 이동) ←── 퇴근시간 ──────┘
              체력 0 ↓         ↑ 회복(최대의 50%)
                     Resting ────┘
```

- [x] **(1) `workTime` 기반 출퇴근** (2026-09-01)
  - `DecisionRoutine()`이 주기적으로 근무시간·체력을 보고 상태를 옮긴다.
    시각은 `GameManager.instance._Date`, 판단은 `IsWorkTime()`(자정을 넘기는 근무도 처리)
  - **퇴근할 때 `ReleaseSeat`을 부르지 않기로 했다.** 원래 계획과 다른 선택이다 —
    자리는 이제 플레이어가 `WorkstationAssignPopup`으로 꽂는 것이라, 매일 밤 반납하면
    지정해둔 배정이 그때마다 지워진다. 퇴근은 **자리를 비우는 것이지 자리를 잃는 게 아니다**.
    배정이 진짜로 지워지는 곳은 퇴사(`OnDestroy`)와 UI 빼기 버튼(`ReleaseSeatOf`)뿐
  - `IsSeatStale()`이 매 판단마다 배정을 다시 조회한다. 그래서 팝업에서 자리를 바꾸면
    이벤트 배선 없이 다음 판단(≤0.9초)에 반영된다
- [x] **(4) 우르르 몰림** (2026-09-01)
  - `_decisionIntervalMin/Max` 사이의 랜덤 간격으로 판단한다. 시작 시점도 한 번 흩뜨린다
- [x] **(2) 도착 직전 떨림** (2026-09-01)
  - `_arriveDistance`를 0.15f → 0.3f로 완화하고, `Awake()`에서 `_agent.stoppingDistance`에
    같은 값을 넣었다. `remainingDistance`만 보고 판정하면 agent는 전속력으로 오다가 문턱을
    넘는 순간 급정지 + 주변 회피(obstacle avoidance) 보정이 겹쳐 떨렸는데, `stoppingDistance`를
    맞춰주면 그 반경부터 agent 스스로(`autoBraking`) 미리 감속해서 문턱을 넘을 땐 이미 거의 멈춰 있다
  - 프리팹(`HumanMale_Character_FREE.prefab`)에 `_arriveDistance: 0.15`로 박혀 있던 값도
    0.3으로 같이 맞췄다 — 스크립트 기본값만 바꾸면 이미 저장된 오버라이드가 이긴다
- [ ] **(3) 회전 스냅** ← 여기부터
  - `transform.rotation = _seat.rotation`이 한 프레임에 홱 돈다. Lerp 필요
- [ ] **(5) 먼 자리 배정**
  - `WorkstationManagerSO.RequestSeat`이 리스트 순서대로 첫 빈 자리를 준다.
    바로 옆 책상을 두고 반대편까지 걸어갈 수 있다. 가까운 자리 우선으로
- [x] **(6) 근무/이동 대체 애니메이션** (2026-09-01)
  - **진짜 앉기 모션은 여전히 없다** — 에셋팩(`Animations_Starter_Pack`)에 Movement/Combat/Gathering뿐이라
    Sit 클립 자체가 없음. 대신 `IsWorking`/`IsMoving` bool 두 개를 `LowPolyHumanAnimator.controller`에
    추가하고, `Working` 상태는 채굴 반복 동작(`MiningLoop`), `Moving` 상태는 `RunForward`를 0.6배속으로
    돌려서 걷는 것처럼 대체했다. `Idle ↔ Working`, `Idle ↔ Moving` 전환뿐이라 두 상태가 겹칠 일은 없다
    (agent가 멈춘 뒤에야 Working으로 들어간다)
  - `EmployeeWorkAI.SetState()` 한 곳에서 두 bool을 같이 세팅한다 — `IsWorking`/이동 상태 판정과
    같은 이유로, 여기 말고 다른 데서 애니메이터를 건드리면 조용히 어긋난다
  - **다음:** 정식 Walk 클립으로 교체, 진짜 앉기(의자별 포즈) 붙이기, 방향 전환을 Lerp로((3)과 같이)
- [x] **퇴근 이동(GoingHome)** (2026-09-01)
  - `LeaveWork()`가 무조건 그 자리에서 `OffDuty`로 바뀌던 것을, `WorkstationManagerSO.GetExitPoint()`가
    있으면 거기까지 걸어나간 뒤(`GoingHome`) 도착해서야 `OffDuty`로 바뀌게 확장. 출입구가 없거나
    경로를 못 찾으면 예전처럼 그 자리에서 바로 처리(안전한 폴백)
  - `CompanyExitPoint`(신규, `Assets/Script/Placed/CompanyExitPoint.cs`)를 씬에 놓고 `OnEnable`에서
    `WorkstationManagerSO.RegisterExitPoint()`로 스스로 등록하는 방식 — 프리팹은 씬 오브젝트를
    직접 참조 못 해서 `RegisterWorkstation`과 같은 패턴을 그대로 씀
  - `WorkstationManagerSO.Init()`에서 일부러 `_exitPoint`를 안 지운다 — `CompanyExitPoint.OnEnable()`이
    `GameManager.Start()`(Init을 부르는 곳)보다 먼저 도는 경우 Init이 등록을 바로 지워버리기 때문
  - **다음(액션 필요): 씬에 `CompanyExitPoint`를 실제로 배치 안 했다.** 문 앞 등 NavMesh가 베이크된
    위치에 빈 오브젝트로 하나 놓고 `_workstationManagerSO` 필드를 연결해야 실제로 동작한다.
    안 놓으면 이전처럼 그 자리에서 바로 퇴근 처리되니 깨지진 않는다

---

## 🔵 기획 결정 필요

구현 TODO가 아니라 **먼저 정해야 할 것들**. 코드를 훑다 보니 저자 스스로도 확정 못 지은
채 남겨둔 지점들이 있어서 따로 뺐다.

- [ ] **직원 이탈(퇴사) 조건**
  - `PayEmployees()`는 월급을 못 주면 스태미나/멘탈만 깎을 뿐(`UNPAID_SALARY_PENALTY`),
    그게 계속되거나 멘탈이 바닥나도 실제로 그만두는 로직이 없다.
    지금 유일한 퇴사 경로는 플레이어가 `EmployeeElement`에서 수동으로 해고하는 것뿐
  - 이게 없으면 "월급 안 줘도 그만"이라 P1의 수입/체력 밸런스가 압박으로 이어지지 않는다.
    **P1 밸런스 항목들과 묶어서 볼 것**
- [ ] **미션 시스템 설계**
  - `Mission.cs`에 저자가 직접 남긴 주석: `//회복, 지능, 기술, 명상 이런식으로 해야하나`.
    미션이 수입/직원 성장/스탯에 실제로 어떻게 연결되는지 아직 안 정해졌다
- [ ] **근무시간(WorkTime) 플레이어 편집**
  - 채용 시점에 고정되는 값으로 보이고(`Employee.cs:355`), 바꾸는 UI가 안 보인다.
    직원별 출퇴근 시간 조정을 플레이 요소로 쓸지 결정 필요
- [ ] **성장/승진 시스템**
  - `Employee.CareerPeriod` 필드는 있는데, 시간이 지나면 실제로 `WorkSpeed`나 `Salary`에
    영향을 주는 로직이 코드에 없다. 그냥 표시용 숫자인지 성장 시스템으로 키울지 결정 필요
- [ ] **엔드게임 / 목표**
  - `GameManager.Reputation`(개인 → 팀 프로젝트 → 동아리 → 스타트업 → …) enum이 정의만
    돼 있고 아무 데서도 참조되지 않는다. "승리 조건"이 뭔지 아직 정해지지 않은 상태
- [ ] **퇴근 후 행동**
  - `GoingHome`(위)은 출입구까지 걸어나가 멈추는 이동만 구현한 것. 사용자가 원한다고 밝힌
    "키보드 딸깍/토크/탕비실" 같은 목적 있는 유휴 행동은 기획 자체가 아직 없다.
    `WanderRoutine`을 되살릴지, 새 상태(휴게실 등)로 대체할지부터 정할 것

---

## 🔴 P0 — 보안 (지금 바로)

- [ ] **Firestore 보안 규칙 작성 & 배포**
  - `firestore.rules` 파일을 저장소에 추가
  - `User/{email}`, `GamePlayUser/{nickname}` 문서는 **소유자만 읽기/쓰기** 가능하도록 제한
  - `request.auth != null` 및 소유권 검증 추가 (재화 치트 차단)
- [ ] **추적 중인 민감정보 점검**
  - `google-services.json.meta`, `NotionAPIKeySO.asset` 등이 커밋 히스토리에 노출됐는지 확인
  - 노출 시 키 재발급 + `.gitignore` 보완
- [ ] (선택) 재화·핵심 수치는 Cloud Functions 등 **서버 검증**으로 이전 검토

---

## 🟠 P1 — 게임 루프

"직원을 뽑아 → 책상에 앉히고 → 일을 시켜 → 돈을 번다"가 전부 이어졌다. 남은 건 **밸런스**다.

- [x] **근무 → 수입** (2026-09-01) — 루프가 닫혔다
  - `EmployeeManagerSO.CollectIncome(gameMinutes)`가 `IsWorking`인 직원만 골라 합산한다.
    **시간당 수입 = `_incomePerWorkHour`(5,000) × (업무속도 / 100) × (체력 / 최대체력)**
  - 정산 지점을 `GameManager.AddDateMinute()`으로 잡았다. **실제 시간이 아니라 게임 시간 기준**이라
    빨리감기(`TimeButton`)와 자동 시계(`GameClock`)가 같은 시간에 같은 돈을 번다.
    월급(`GameDate.Month` setter)·지원자(`AddRandomApplicants`)도 이미 여기 걸려 있어서 시간 관련 로직이 한자리에 모인다
  - `Employee.WorkSpeed`를 새로 만들었다(100이 표준, 서버 왕복 포함).
    `weight_speed`는 UI 라벨 이름만 있고 데이터가 없어서 `EmployeePanel`이 `1`을 하드코딩해 넘기고 있었다.
    지원자는 `RMSO`의 80~120 범위에서 뽑히므로, **월급이 100만원 고정인 지금은 이 값이 지원자를 고르는 유일한 기준**이다
  - `Employee.IsWorking`은 **서버에 저장하지 않는다.** 접속할 때마다 근무시간과 체력을 보고 다시 결정되는 값이다.
    `EmployeeWorkAI`가 `SetState()` 한 곳에서만 상태와 함께 갱신한다 — `_state`를 직접 대입하면
    둘이 어긋나서 퇴근한 직원이 계속 돈을 번다
  - 손익분기점은 시간당 약 3,700원(월급 100만 ÷ 9시간 × 30일). 5,000원이면 체력이 넉넉할 때만 흑자다
- [x] **체력 0 처리** (2026-09-01)
  - 체력이 0이면 근무시간이어도 자리에서 일어나 쉰다(`Resting`). 최대의 50%까지 회복하면 복귀
  - 수입이 붙어서 이제 **쉬는 동안은 진짜로 못 번다.** `Resting`이면 `IsWorking`이 꺼진다
  - 다만 아래 "체력이 실질적으로 안 깎인다" 참고 — 지금 수치로는 이 손해가 발생하지 않는다
- [ ] **체력이 실질적으로 안 깎인다** ← 수입 공식이 체력을 보는데 체력이 안 움직인다
  - `EmployeeWorkAI.TickStamina()`만 **실제 시간**(`Time.deltaTime`) 기준이다.
    소모 6/실제 1분 = 0.1/초인데 1초가 게임 1시간이므로 **게임 1시간에 0.1**밖에 안 깎인다
  - 100 체력이 게임 41일을 간다. 하루(9시간 근무)에 0.9 깎이고 나머지 15시간에 3.0 회복되니
    **체력은 항상 최대**고, `Resting`도 사실상 발생하지 않는다
  - 그래서 수입 공식의 체력 계수가 늘 1.0으로 고정이다. "체력을 관리할 이유"가 아직 종이 위에만 있다
  - 고치려면 `TickStamina()`도 수입처럼 게임 시간 기준으로 옮기고(`AddDateMinute` 경로),
    근무 하루에 체력이 눈에 띄게 줄도록 수치를 다시 잡아야 한다. **밸런스 결정이라 값은 따로 정할 것**
- [ ] **지원자 UI에 업무속도가 안 보인다**
  - 고용된 직원 목록(`EmployeeElement`)에는 슬롯이 이미 있어서 실제 값이 나온다
  - 지원자 쪽(`ApplicantElement` / `ApplicantPanel`)에는 표시할 텍스트 자체가 없다. **프리팹 수정 필요**
  - 이게 없으면 뽑기 전에는 좋은 지원자인지 알 수가 없어서 랜덤 뽑기와 다를 게 없다
- [ ] **지원자가 너무 빨리 쌓인다**
  - `AddDateMinute`이 `AddRandomApplicants(60 / value)`를 부르는데, 시계가 `value = 60`으로
    부르므로 `Random.Range(0, 1)` = 항상 0 → **공고당 게임 1시간에 1명씩 무조건 추가**된다
  - 손으로 누를 때는 티가 안 났는데 시간이 자동으로 흐르니 금방 눈에 띈다. 가중치 재조정 필요
- [ ] **`WED요일`**
  - `Date.ToString()`(`:249`)이 `Week` enum을 그대로 이어붙인다. 한글 매핑 테이블 필요
- [ ] **월급 밀려도 직원이 안 나간다** ← 기획 결정 먼저(위 🔵 참고)
  - `PayEmployees()`가 스태미나/멘탈만 깎고 끝난다. 압박이 되려면 결국 퇴사로 이어져야 한다

---

## 🟠 P2 — 화면에 보이는 것


- [ ] **상점 아이콘이 12칸 전부 같다**
  - `PO_Element`의 Icon 스프라이트가 공유라 버튼만 봐선 뭐가 뭔지 모른다.
    가구별 스프라이트를 넣거나 이름 라벨을 붙여야 한다
- [ ] **상점 슬롯 3칸이 남는다**
  - `GamePlay` 씬 `PO_Element` 12칸 중 (9)(10)(11)은 아직 회의 책상을 가리킨다
- [ ] **직원 머리 위 표시**
  - 이름 / 체력 바가 없어서 화면상 저 사람이 누구인지, 뭘 하는지 알 방법이 없다
- [ ] **사운드가 아예 없다**
  - 발소리·UI SFX·알림음 등 오디오 관련 코드가 프로젝트 전체에 안 보인다. 필요 시점에 처음부터 설계
- [x] **상단 HUD** (2026-09-01)
  - 돈 텍스트 왼쪽에 아이콘(`112-01.png`, 지폐 모양)을 붙이고, 그 아래 줄에 직원 아이콘(`88-01.png`,
    서류가방)과 `n/m` 텍스트를 새로 놓았다. 둘 다 프로젝트에 이미 들어 있던 범용 아이콘 팩
    (`Assets/Resources/Img/ICON/`)에서 골랐다 — 새 이미지 반입 없음
  - `n` = 고용된 직원 수(`EmployeeManagerSO.GetEmployees().Count`), `m` = 놓인 책상 수
    (`WorkstationManagerSO.WorkstationCount`, 신규). 최대 인원 개념이 없어서 "직원 수 / 자리 수"로
    정의했다 — 자리보다 사람이 많아지면 자연히 티가 난다
  - `n`은 `_isChangedEmployeePanelEventChannelSO`(고용/해고 시 이미 울리던 채널)를 `UIManager`가
    직접 구독해 즉시 갱신한다. `m`은 배치/삭제를 알려주는 채널이 없어서 `SetDateUI()`가 도는
    주기(게임 시계 틱, ≤1초)에 묻어간다 — `IsSeatStale()`처럼 즉시 반영 대신 짧은 지연을 택함
  - 씬 편집은 Unity 배치 모드(`-executeMethod`)로 로드해 실제로 검증했다: 필드 4개 전부 연결,
    두 아이콘 스프라이트 로드, `TimeButton`(y 870~970)과 겹치지 않는 좌표(새 줄 y 810~860) 확인

---

## 🟠 P3 — 사무실 확장

- [ ] **확장 버튼 / 서버 저장**
  - `OfficeArea.Expand(Vector2Int)`는 있다. 부르는 UI와, 늘린 크기를 서버에 저장하는 경로가 없다
  - 저장할 때 Office의 origin/size를 같이 넣어야 한다. **안 넣으면 재접속 시 사무실이
    초기 크기로 돌아가고, 벽 밖에 있던 가구가 배치 불가 영역에 갇힌다**
- [ ] **확장 비용** ← 수입이 붙어서 이제 막힌 게 없다
  - 번 돈을 쓸 곳. 지금은 돈이 들어오기만 하고 월급 말고는 나갈 데가 없다

---

## 🟠 P4 — 배치 시스템 남은 것

- [x] **책상을 옮길 때 앉아있는 직원 처리** (2026-09-01)
  - 진입점(`OnSeatMoved()`)을 따로 만드는 대신 `IsSeatStale()`이 판단마다 확인한다.
    출발할 때의 SeatPoint 위치(`_seatAnchor`)와 지금 위치를 비교해서, 책상이 움직였으면 다시 걸어간다
  - 배선이 0개다. `PlaceSystem`이 이미 부르고 있는 `Unregister/RegisterWorkstation` 외에
    새로 부를 곳이 없다 (같은 오브젝트끼리 이벤트 채널을 안 쓰는 것과 같은 이유)
  - 남은 것 : 들고 있는 동안 직원이 커서를 따라 걷는다. 보기 싫으면 배치 중(`_isHandlingEvent`)에는
    판단을 멈추게 하면 된다
- [ ] **판매 / 환불**
  - 삭제는 되는데 환불 금액 지급이 없다

---

## 🟠 P5 — 성능 / 비용

- [ ] **`GameServerStart` 문서 단일 조회로 리팩터링**
  - 현재: `GamePlayUser/{nickname}`을 필드마다 통째로 재조회(약 9회)
  - 개선: 문서를 **한 번만** 읽어 `Dictionary`로 받은 뒤 각 매니저에 필드 분배
  - `GetFirestoreData(collection, id, key)` → `GetDocument(collection, id)` 형태의 API 추가
- [ ] **정렬/이진탐색 공용화**
  - Mission/Employee/Recruitment의 복붙된 O(n²) 정렬·이진탐색 제거
  - `id` 기준 공용 유틸(또는 `List.Sort` + `List.BinarySearch`)로 통합

---

## 🟡 P6 — 안정성 / 버그

- [ ] **`GetFirestoreData` 방어 코드**
  - `db == null` 시 조기 반환(로그만 남기고 진행 금지)
  - 스냅샷 미존재/`result == null` 시 `ContainsKey` 전에 null 체크 → NRE 방지
- [ ] **이진탐색 경계 처리**
  - 빈 리스트 / id 미존재 시 인덱싱 전에 범위 검사 (IndexOutOfRange 방지)
- [ ] **`AuthStatusChanged` 로직 정리**
  - `LoginEvent(true)` 직후 무조건 `LoginEvent(false)` 호출되는 흐름 재검토
  - `isInit` 가드에 의존하지 않도록 명확화

---

## 🟢 P7 — 정리

- [ ] **로그아웃 기능 구현** (README 명시 vs 코드 미구현)
  - `auth.SignOut()` + 상태 초기화 + 메뉴 씬 복귀
- [ ] **Notion 연동 정리**
  - `NotionAsync`의 `apiUrl = "api키"` 플레이스홀더 처리, `GetNexonData` 명칭 정리
  - 사용 안 하면 제거, 쓸 거면 실제 엔드포인트·용도 확정
- [ ] **매직 스트링 상수화**
  - `"GamePlayUser"`, `"missions."`, `"employees."` 등 경로를 상수/헬퍼로 추출
- [ ] **EditMode 테스트 추가**
  - `Date` 계산, `Mission`/`Employee` JSON 직렬화 왕복, 이진탐색부터 커버.
    직원 stamina/mental/WorkTime 등이 저장→로드 왕복에서 그대로 돌아오는지도 여기서 같이 검증할 것
- [ ] **`Mission.cs`의 안 쓰는 `using NUnit.Framework;`**
  - `Assert` 등 실제로 안 쓰는데 테스트 프레임워크를 프로덕션 코드에 import해뒀다. 제거 대상

---

## 지켜야 할 제약

완료된 작업에서 살려둔 규칙들. 어기면 조용히 깨지는 것들이라 지우지 말 것.

- **`property_id`는 `PlaceSystem._shopPlaceableObjects`의 인덱스와 같아야 한다.**
  `CreateHandlingObject()`가 프리팹의 `property_id`를 저장하고 `PlaceObject()`가 그 값으로
  배열을 인덱싱한다. 가구를 추가할 때는 **배열 끝에만** 붙인다.
  중간에 끼우면 이미 서버에 저장된 오브젝트가 전부 다른 가구로 바뀐다
- **잠긴 타일(`Tilemap_Locked`)은 배치용 타일맵과 분리한다.**
  `SetArea()`가 매 프레임 `mainTilemap`을 지웠다 다시 칠하므로 같이 쓰면 잠긴 타일이 같이 지워진다
- **날짜를 틱마다 서버에 쓰지 않는다.** 시간이 자동으로 흐르므로 매번 쓰면 초당 1회 쓰기가 된다.
  `AddDateMinute(value, toServer: false)`로 로컬에만 반영하고 `SaveDate()`로 몰아서 쓴다
- **퇴근할 때 `ReleaseSeat`을 부르지 않는다.** 자리 배정은 플레이어가 UI로 꽂아두는 값이다.
  퇴근마다 반납하면 그 지정이 매일 밤 지워진다. 배정을 지우는 건 퇴사와 UI 빼기 버튼뿐
- **체력을 매 틱 서버에 쓰지 않는다.** `TickStamina()`는 `SetStamina(..., toServer: false)`로
  로컬에만 반영하고, 상태가 바뀌는 순간에만 `SaveStamina()`로 남긴다.
  날짜를 `SaveDate()`로 몰아 쓰는 것과 같은 이유다 (여기는 직원 수만큼 곱해진다)
- **재화도 매 틱 서버에 쓰지 않는다.** 수입이 게임 1시간마다 들어오므로 `SetMoney`에
  `AddDateMinute`의 `toServer`를 그대로 넘기고, `GameClock`이 `SaveDate()` 옆에서 `SaveMoney()`로 몰아 쓴다.
  날짜·체력과 같은 이유이자 같은 주기다
- **수입은 게임 시간으로 정산한다.** `CollectIncome`은 `Time.deltaTime`이 아니라 `AddDateMinute`이 넘겨준
  게임 분(minute)을 받는다. 실제 시간으로 계산하면 빨리감기로 시간만 돌려 돈을 무한히 벌 수 있다
- **`IsWorking`은 `SetState()`로만 바꾼다.** `_state`를 직접 대입하면 수입 정산이 보는 값과 어긋난다.
  퇴근한 직원이 계속 돈을 벌거나, 일하는 직원이 한 푼도 못 버는 식으로 조용히 틀어진다
- **서버 채널은 `RaiseEvent()`로 부른다.** `_onSendEventRaised(...)`처럼 델리게이트를 직접 부르면
  구독자가 없는 로컬 테스트에서 NullReferenceException이 난다 (`RaiseEvent`는 `?.Invoke`)
- **같은 오브젝트에 붙은 것끼리는 SO 이벤트 채널을 쓰지 않는다.**
  `LongPressSelector`, `OfficeArea` 모두 `GetComponent`로 직접 참조한다. 씬 배선이 0개가 된다
- **칸 수는 콜라이더 월드 길이 ÷ `Grid.cellSize`로 올림하고, 거기에 여유 한 칸(`TilePadding`)을 더한다.**
  `CellSwizzle`이 XZY라 `cellSize.y`가 월드 z축 길이다.
  여유 한 칸은 의도된 것 — 딱 맞게 잡으면 가구끼리 붙어버린다
- **시작 모서리(`GetStartPosition`)는 네 꼭짓점 중 x·z 최솟값이다.**
  타일을 +x, +z 방향으로만 칠하기 때문. 회전하면 `vertices[0]`은 더 이상 최소 모서리가 아니다
- **배치된 오브젝트를 "전부 지우고 다시 만들기"는 쓸 수 없다.**
  이번 세션에 새로 놓은 것은 서버에만 있고 로컬 목록에는 없어서 같이 사라진다.
  `_createdObjectIds`(HashSet)로 이미 만든 id를 건너뛰는 방식을 유지할 것

---

## 진행 기록

- (2026-07-22) 코드베이스 분석 및 TODO 작성
- (2026-08-29) 롱프레스 감지(`LongPressSelector`) 구현
- (2026-08-30) 오브젝트 이동 모드 구현 — 이동 시작 시 목록에서 빼기, 취소 시 원위치, id 유지.
  드래그 중 자기 콜라이더에 레이가 맞던 문제와 바닥을 못 찾으면 원점으로 순간이동하던 문제 같이 수정
- (2026-08-30) 롱프레스–PlaceSystem 사이 SO 이벤트 채널 제거, 직접 참조로 변경
- (2026-08-30) 삭제 경로 + 확인 팝업 구현
- (2026-08-30) 90도 회전 구현. 회전을 서버에 저장하고, 비대칭이던 칸 수 계산과
  회전하면 어긋나던 시작 모서리 계산을 바로잡음
- (2026-08-30) 칸 수 계산에 여유 한 칸(`TilePadding`)을 되살림.
  회전 작업에서 실측값으로 바꾸며 예전 `+1` 여백이 사라져 배치 범위가 줄어 보였다 (3x5 → 2x5 → 3x6)
- (2026-08-30) `DebugNav`(랜덤 배회 테스트 스크립트)를 지우고 그 동작을
  `EmployeeWorkAI`의 Idle 상태 행동(`WanderRoutine`)으로 옮김
- (2026-08-30) 배치 가능 가구 8종 추가 (1인용 책상 2, 의자 3, 캐비닛 2, 화분 1) → 총 9종
- (2026-08-31) `OfficeArea` 도입. 바닥/벽/배치 범위를 소유 범위 값 하나에서 계산하도록 바꾸고,
  `CheckTile()`에 경계 검사를, 미개방 구역에 어두운 타일을 추가
- (2026-08-31) `GameClock` 추가. 시간이 스스로 흐른다.
  날짜 서버 쓰기를 틱마다에서 게임 하루에 한 번(+ 일시정지/종료 시점)으로 묶음
- (2026-08-31) `GameDate.SetDateToServer`가 델리게이트를 직접 불러 로컬 테스트에서 NRE가 나던 것 수정
- (2026-09-01) `EmployeeWorkAI`에 `workTime` 기반 출퇴근 상태 기계.
  `Idle/MovingToDesk/Working`(종착역) → `OffDuty/GoingToDesk/Working/Resting`.
  막아뒀던 `TickStamina()`와 이동 루틴을 다시 켜고, 체력 서버 쓰기는 상태 전환 시점으로 몰았다
  (1점 바뀔 때마다 쓰면 직원 수만큼 초당 쓰기가 된다)
- (2026-09-01) **근무 → 수입.** 게임 루프가 닫혔다.
  `EmployeeManagerSO.CollectIncome()`을 `GameManager.AddDateMinute()`에 걸어 게임 시간 기준으로 정산하고,
  재화 서버 쓰기는 날짜와 같은 주기(`GameClock.Save()`)로 묶었다.
  데이터가 없던 업무속도(`Employee.WorkSpeed`)를 실제 스탯으로 만들고
  `EmployeePanel`이 하드코딩해 넘기던 `1`을 실제 값으로 교체
- (2026-09-01) 상단 HUD에 돈 아이콘 + 직원 수(`n/m`) 추가.
  기존 아이콘 팩에서 골라 새 이미지 반입 없이 처리. 씬 변경은 Unity 배치 모드로 직접 로드해 검증
- (2026-09-01) 도착 직전 떨림 수정. `_arriveDistance`를 0.3으로 완화하고
  `_agent.stoppingDistance`에 같이 넣어 agent가 스스로 미리 감속하게 함.
  프리팹에 박혀 있던 구버전 값(0.15)도 같이 갱신. (이번엔 Unity 에디터가 없어 코드 리뷰로만 검증 —
  이전 HUD 작업과 달리 배치 모드 실행 확인은 못 했다)
- (2026-09-01) `WorkstationAssignPopup`이 열려 있는 동안 직원 목록이 갱신돼도(서버 응답 지연,
  고용/해고) 창이 그대로 "직원 없음"에 머물던 버그 수정. `EmployeePanel`/`UIManager`와 같은 패턴으로
  `_isChangedEmployeePanelEventChannelSO`를 구독해 열려 있을 때만 다시 그리도록 함
- (2026-09-01) 테스트모드(`_testMode`)에서 `J` 키로 채용 UI 없이 직원을 즉시 하나 추가하는 단축키 추가.
  `SpawnTestSetup()`에 있던 직원 생성 코드를 `SpawnTestEmployee()`로 분리해 재사용.
  씬의 `_testSpawnEmployee`도 꺼져 있던 걸 켜서 로컬/오프라인 테스트에서 시작부터 직원이 있게 함
- (2026-09-01) `Working`/`Moving` 애니메이션 상태 추가(위 "🟣 진행 중" 참고) 및
  퇴근 이동(`GoingHome`) + `CompanyExitPoint` 구현

---

## 발견한 버그 (아직 안 고침)

- **저장한 위치가 중심이 아니라 모서리다**
  - `PlaceSystem.PlaceHandlingObject()`가 `SetPosition(GetStartPosition())`로 **모서리** 좌표를
    저장하는데, 불러올 때 `PlaceableObject.SetPlacedObjectData()`는 그 값을
    `transform.position`(**중심**)에 그대로 넣는다
  - 그래서 껐다 켤 때마다 모든 오브젝트가 모서리 오프셋만큼(회의 책상 기준 x -1.5, z -4.1) 밀린다.
    회전이 들어가면 밀리는 방향까지 각도에 따라 달라진다
  - 고치려면 저장 값을 `transform.position`으로 바꾸면 되는데, 이미 저장된 데이터의 해석이
    바뀌므로(기존 오브젝트가 한 번 더 움직인다) 마이그레이션을 별도로 판단할 것

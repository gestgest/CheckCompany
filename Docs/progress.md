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
   └──── 퇴근시간 ────────────────────────────┘
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
- [ ] **(2) 도착 직전 떨림** ← 여기부터
  - `_arriveDistance = 0.15f`가 너무 빡빡하다. `_agent.stoppingDistance`를 안 쓰고
    `remainingDistance`로 직접 재고 있어서 도착 직전에 미세 보정이 계속 걸린다
  - `ArriveAtDesk()` 주석에도 떨림 얘기가 적혀 있는데, 멈추는 것으로 덮었을 뿐 원인은 그대로다
- [ ] **(3) 회전 스냅**
  - `transform.rotation = _seat.rotation`이 한 프레임에 홱 돈다. Lerp 필요
- [ ] **(5) 먼 자리 배정**
  - `WorkstationManagerSO.RequestSeat`이 리스트 순서대로 첫 빈 자리를 준다.
    바로 옆 책상을 두고 반대편까지 걸어갈 수 있다. 가까운 자리 우선으로
- [ ] **(6) 앉는 애니메이션**
  - `EmployeeWorkAI`의 `//TODO: Animator Controller...` 그대로
  - **(2)(3)(5) 다음에.** 모양이 틀린 상태에서 애니메이션을 넣으면 동상에 옷을 입히는 꼴이다

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

---

## 🟠 P2 — 화면에 보이는 것

스크린샷이 허전한 이유는 오브젝트 수가 아니라 **바닥과 벽이 아무것도 아닌 흰 면**이라서다.

- [ ] **벽 / 바닥 텍스처 가져오기** ← 체감 효과가 제일 큼
  - `Floor`와 `Wall`이 둘 다 빌트인 **Default-Material**(`fileID: 10303`)을 쓴다. 텍스처가 아예 없다
  - 프로젝트에 있는 텍스처는 `LowPolyOfficeProps_LITE/Textures/`의 소품용 아틀라스 3장뿐이라
    바닥/벽용은 새로 가져와야 한다 (카펫·장판·페인트 벽)
  - 타일링을 쓸 거면 바닥 큐브의 UV가 스케일을 안 따라간다. 머티리얼 `Tiling`을
    `OfficeArea`가 Land 크기에 맞춰 같이 갱신해주는 게 깔끔하다
- [ ] **바닥과 타일이 겹쳐 가끔 안 보인다 (z-fighting)**
  - `Floor`는 두께 1짜리 큐브를 y `-0.5`에 둬서 윗면이 정확히 y `0`, `Tilemap`은 y `0.01`.
    0.01은 너무 작아서 깊이 정밀도 싸움이 그대로 난다
  - 빌트인 RP라 URP Decal Projector는 못 쓴다. **Sprites-Default를 복사해 `Offset -1, -1`
    (깊이 바이어스)을 넣은 전용 머티리얼**을 `TilemapRenderer`에 물리는 게 정석
  - 더 확실히 하려면 배치 중일 때만 `ZTest Always`로 가구 위에도 그리기
    (심즈 · Two Point Hospital 방식). 다만 배치 중이 아닐 때 켜두면 지저분하다
- [ ] **상점 아이콘이 12칸 전부 같다**
  - `PO_Element`의 Icon 스프라이트가 공유라 버튼만 봐선 뭐가 뭔지 모른다.
    가구별 스프라이트를 넣거나 이름 라벨을 붙여야 한다
- [ ] **상점 슬롯 3칸이 남는다**
  - `GamePlay` 씬 `PO_Element` 12칸 중 (9)(10)(11)은 아직 회의 책상을 가리킨다
- [ ] **직원 머리 위 표시**
  - 이름 / 체력 바가 없어서 화면상 저 사람이 누구인지, 뭘 하는지 알 방법이 없다
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
  - `Date` 계산, `Mission`/`Employee` JSON 직렬화 왕복, 이진탐색부터 커버

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

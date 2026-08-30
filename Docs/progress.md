# CheckCompany 개선 TODO

분석 근거는 [README.md](README.md) 참고. 우선순위: 🔴 시급 → 🟠 중요 → 🟡 개선 → 🟢 정리

배치 시스템(가구 · 회전 · 이동 · 삭제 · 사무실 범위)과 자동 시계까지는 돌아간다.
지금 비어 있는 건 **그것들이 게임 상태에 영향을 주는 고리**와 **화면에 보이는 것**이다.

---

## 🟣 진행 중 — 직원 AI가 부자연스럽다

`EmployeeWorkAI`의 `ClaimDeskRoutine()`(`:52`)과 `Update()`의 `TickStamina()`(`:107`)를
주석 처리해 둔 상태다. **켜보니 부자연스러워서 껐다.** 원인은 연출이 아니라 상태 기계 모양이다.

```
Idle → MovingToDesk → Working (끝)
```

`Working`이 종착역이다. `ArriveAtDesk()`가 `isStopped = true`로 세우고 나면 다시는 안 움직인다.
그래서 실제로는 이렇게 보인다.

- 스폰되자마자 책상으로 직진한다 — **시간 개념이 없어서 새벽 3시에도 출근한다**
- 도착하면 영원히 그 자리에 굳는다
- 서 있기만 한다 (앉는 애니메이션 없음)
- 체력이 0이 돼도 계속 근무한다
- 퇴근도 휴식도 없다

사람이 아니라 가구가 하나 더 놓인 걸로 보인다.

### 재료는 이미 있다

`Employee.workTime`(기본 9~18시)이 **데이터에 이미 있고** `EmployeeStatusPanel.cs:117`에서
"근무시간 : 9 ~ 18"로 표시까지 한다. 그런데 **행동에는 아무 데서도 안 쓴다.**
이제 `GameClock`으로 시간이 실제로 흐르니 이 값이 상태 기계를 돌리는 축이 될 수 있다.

```
OffDuty ──출근시간──> GoingToDesk ──도착──> Working
   ↑                                          │
   └──── 퇴근시간 ────────────────────────────┘
              체력 낮음 ↓        ↑ 회복
                     Resting ────┘
```

핵심은 **직원이 이유가 있어서 움직인다**는 것. 출퇴근만 붙여도 절반은 해결된다.

- [ ] **(1) `workTime` 기반 출퇴근** ← 여기부터
  - `OffDuty` / `GoingToDesk` / `Working` / `Resting`로 상태를 늘리고 `Working`을 종착역에서 뺀다
  - 퇴근하면 자리를 반납(`ReleaseSeat`)해야 다음 날 다시 배정이 돈다
- [ ] **(2) 도착 직전 떨림**
  - `_arriveDistance = 0.15f`가 너무 빡빡하다. `_agent.stoppingDistance`를 안 쓰고
    `remainingDistance`로 직접 재고 있어서 도착 직전에 미세 보정이 계속 걸린다
  - `ArriveAtDesk()` 주석에도 떨림 얘기가 적혀 있는데, 멈추는 것으로 덮었을 뿐 원인은 그대로다
- [ ] **(3) 회전 스냅**
  - `transform.rotation = _seat.rotation`이 한 프레임에 홱 돈다. Lerp 필요
- [ ] **(4) 우르르 몰림**
  - 직원이 여럿이면 전부 같은 순간에 같은 판단을 해서 떼로 움직인다. 개체별 랜덤 지연 필요
- [ ] **(5) 먼 자리 배정**
  - `WorkstationManagerSO.RequestSeat`이 리스트 순서대로 첫 빈 자리를 준다.
    바로 옆 책상을 두고 반대편까지 걸어갈 수 있다. 가까운 자리 우선으로
- [ ] **(6) 앉는 애니메이션**
  - `EmployeeWorkAI`의 `//TODO: Animator Controller...` 그대로
  - **(1)~(5) 다음에.** 모양이 틀린 상태에서 애니메이션을 넣으면 동상에 옷을 입히는 꼴이다

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

"직원을 뽑아 → 책상에 앉히고 → 일을 시켜 → 돈을 번다"에서 아직 뒤 두 마디가 비어 있다.

- [ ] **근무 → 수입** ← 루프를 닫는 마지막 조각
  - `SetMoney`로 돈이 **느는** 곳은 두 군데뿐인데 둘 다 주석에 "디버깅"이라 적힌 수동 체크박스다
    (`TodoMissionElement.cs:69`, `EmployeeStatusPanel.cs:228`). **수입 시뮬레이션이 없다**
  - 돈이 **나가는** 건 월 1회 월급뿐(`GameDate.Month` setter). 지금은 나가기만 한다
  - `Working`인 직원이 시간당 벌게 하고 업무속도(`weight_speed`)와 체력을 곱하면
    "좋은 직원을 뽑을 이유"와 "체력을 관리할 이유"가 같이 생긴다
- [ ] **체력 0 처리**
  - 지금은 0이 돼도 계속 일한다. 강제 퇴근/휴식이 있어야 방치에 손해가 생긴다
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
- [ ] **상단 HUD**
  - 재화가 아이콘 없는 숫자 하나뿐이다. 돈 아이콘 + 직원 수(`n/m`) 정도는 있어야 HUD로 읽힌다

---

## 🟠 P3 — 사무실 확장

- [ ] **확장 버튼 / 서버 저장**
  - `OfficeArea.Expand(Vector2Int)`는 있다. 부르는 UI와, 늘린 크기를 서버에 저장하는 경로가 없다
  - 저장할 때 Office의 origin/size를 같이 넣어야 한다. **안 넣으면 재접속 시 사무실이
    초기 크기로 돌아가고, 벽 밖에 있던 가구가 배치 불가 영역에 갇힌다**
- [ ] **확장 비용**
  - 번 돈을 쓸 곳. 수입(P1)이 붙은 다음에 의미가 생긴다

---

## 🟠 P4 — 배치 시스템 남은 것

- [ ] **책상을 옮길 때 앉아있는 직원 처리**
  - `EmployeeWorkAI`는 `ArriveAtDesk()` 이후 경로를 재계산하지 않는다. `Working`이면 다시 걷지 않음
  - 책상 프리팹에 `NavMeshObstacle`이 있어서 옮기면 NavMesh가 다시 파이고 직원이 허공에 서 있게 된다
  - 이동 시작: `UnregisterWorkstation` + 직원을 `Idle`로 / 확정: `RegisterWorkstation` + 배정 재시작
  - `EmployeeWorkAI`에 `OnSeatMoved()` 진입점을 하나 만드는 게 깔끔.
    진행 중 항목 (1)에서 상태 기계를 손볼 때 같이 하는 게 낫다
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

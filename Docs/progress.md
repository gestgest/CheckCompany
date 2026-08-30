# CheckCompany 개선 TODO

분석 근거는 [README.md](README.md) 참고. 우선순위: 🔴 시급 → 🟠 중요 → 🟡 개선 → 🟢 정리

---

## 🟣 진행 중 — 오브젝트 꾹 눌러 이동

목표: 배치된 오브젝트를 롱프레스로 다시 잡아 옮긴다.
현재 이동은 **생성할 때만** 가능하다. `PlaceSystem.CreateHandlingObject()`에서 `HandlingObject`를
붙이고, `PlaceableObject.Place()`가 그것을 `Destroy`하기 때문에 (`PlaceableObject.cs:46`)
이미 놓인 오브젝트는 다시 잡을 경로가 없다.

- [x] **(a) 롱프레스 감지** — `LongPressSelector.cs`
  - 배치된 `PlaceableObject`를 `_longPressSeconds` 이상 누르면 `GameObjectEventChannelSO`로 방송
  - `CameraMoveManager`가 같은 입력으로 화면을 끌기 때문에, 화면 이동 거리가 `_cancelMoveDistance`를
    넘으면 카메라 드래그로 보고 롱프레스를 취소한다
  - 멀티터치(핀치 줌)와 UI 위 터치는 무시. `PlaceSystem.IsHandling`이면(이미 무언가 들고 있으면) 동작 안 함
  - **SO 이벤트 채널을 쓰지 않는다.** PlaceSystem과 같은 오브젝트에 붙으므로 `GetComponent`로 직접 호출.
    처음엔 `GameObjectEventChannelSO`를 끼웠다가 제거 - 서로 아는 사이에는 채널이 낭비다
- [x] **(b) `PlaceSystem.StartMoveMode(PlaceableObject)`**
  - `HandlingObject`를 다시 붙여 `Init(...)`, `selectedObject`에 대입, `_isHandlingEvent.RaiseEvent(true)`
  - `StartPlaceMode`와 대부분 공유 가능
- [x] **(c) 이동 시작 시 `_placedObjects`에서 제거**
  - 타일 칠하기/지우기 로직 자체는 이미 맞다. `SetArea()`가 매 프레임 전체를 지우고 다시 그리고,
    handling 중인 오브젝트는 `_placedObjects`에 없어서 자기 타일과 충돌하지 않는다
  - **다만 이동은 다르다.** 이미 놓인 오브젝트는 `_placedObjects`에 들어있는데,
    이 리스트에는 `Add`만 있고 `Remove`가 한 번도 없다 (`PlaceSystem.cs:134,223`)
  - 빼주지 않으면 `SetAllArea(true)`가 자기 옛 발자국을 칠하고,
    `CheckTile()`이 `_redTile`을 보고 **제자리에 다시 놓는 것조차 거부한다** (`PlaceSystem.cs:283`)
- [x] **(d) 취소(deny)는 Destroy가 아니라 원위치**
  - 지금 `TakeOffObject()`는 `Destroy(selectedObject.gameObject)` (`PlaceSystem.cs:216`)
  - 이동 모드는 시작 위치를 기억해뒀다가 되돌려야 하므로 분기 필요
  - 덤: 이 함수는 `selectedObject`가 null이면 그냥 터진다. `_denyEvent` 경로에 가드가 없음
- [x] **(e) 이동 시 object_id를 올리지 않기**
  - `PlaceHandlingObject()`의 `SetObjectID(GetObjectID() + 1)` (`PlaceSystem.cs:199`)는 신규 배치 전용
  - `SendPlaceableObject`가 `placeableObjects.<id>`로 쓰므로 같은 id면 서버는 알아서 덮어써진다.
    카운터만 안 올리면 됨
- [ ] **(f) 앉아있는 직원 처리** ← 가장 큼
  - `EmployeeWorkAI`는 `ArriveAtDesk()` 이후 경로를 재계산하지 않는다. `Working`이 되면 다시 걷지 않음
  - 책상 프리팹에 `NavMeshObstacle`이 있어(`Table_Conference.prefab:132`) 옮기면 NavMesh가 다시 파이고
    직원이 허공에 서 있게 된다
  - 이동 시작: `UnregisterWorkstation` + 직원을 `Idle`로 / 확정: `RegisterWorkstation` + `ClaimDeskRoutine` 재시작
  - `EmployeeWorkAI`에 `OnSeatMoved()` 진입점을 하나 만드는 게 깔끔
  - (f)는 마지막에 — 그 전까지는 "빈 책상만 옮긴다"로 두고 테스트 가능

**권장 순서: (a) → (b)(c)(d)(e) → 삭제 → (f) → 애니메이션**

## 🟠 P1.5 — 배치 시스템 미완성 기능

- [x] **삭제 경로** (판매/환불은 아직 없음)
  - 이동 모드(롱프레스)에서 ok/deny 옆에 삭제 버튼이 뜬다. `DeleteConfirmPopup`으로 한 번 확인받고
    `PlacedObjectManager.RemovePlaceableObject()`가 `DeleteFirebaseEventChannelSO`로
    `placeableObjects.<id>`를 지운다
  - 판매(환불 금액 지급)는 미구현
- [x] **회전**
  - 배치/이동 중에 ok/deny 옆에 회전 버튼(파란색)이 뜬다. 누를 때마다 90도씩 돈다.
    `RotateEventChannelSO` -> `PlaceSystem.RotateHandlingObject()` -> `PlaceableObject.Rotate()`
  - 회전은 `PlacedObjectData.rotation`(0/90/180/270)으로 서버에 같이 저장한다.
    예전에 저장한 데이터에는 이 필드가 없으므로 없으면 0도로 읽는다
  - **`CalculateTileSize()`를 고쳤다.** 기존 `(int)길이 * 2 + 1` / `(int)길이 + 1`은 x축만 두 배로
    세는 비대칭이라, 돌리면 같은 책상이 3x5 <-> 9x2가 돼서 회전이 성립하지 않았다.
    이제 콜라이더 길이에 스케일을 곱해 월드 길이를 구하고 `Grid.cellSize`로 나눠 올림한다
    (`CellSwizzle`이 XZY라 `cellSize.y`가 월드 z축). 회의 책상은 3x5칸 -> 2x5칸이 된다
  - **`GetStartPosition()`도 고쳤다.** 타일은 시작 모서리에서 +x, +z로만 칠하는데
    돌리고 나면 `vertices[0]`이 더 이상 최소 모서리가 아니다. 네 꼭짓점 중 x, z 최소값을 쓴다
    (0도면 예전과 같은 값이 나온다)
  - 이동을 취소하면 위치뿐 아니라 각도도 되돌린다 (`_moveOriginRotation`)
  - `EmployeeWorkAI.ArriveAtDesk()`의 `transform.rotation = _seat.rotation`은 그대로 둔다.
    SeatPoint가 책상 자식이라 책상을 돌리면 앉는 방향도 같이 돌아간다
- [x] **배치 가능 가구가 1종뿐**
  - `LowPolyOfficeProps_LITE`의 메시로 8종을 추가해 `Assets/Prefab/Object/Placed/`가 9종이 됐다.
    스케일은 `Table_Conference`와 같은 2로 맞췄다
  - `property_id`는 `PlaceSystem._shopPlaceableObjects`의 **인덱스와 같아야 한다.**
    `CreateHandlingObject()`가 프리팹의 `property_id`를 그대로 저장하고
    `PlaceObject()`가 그 값으로 배열을 인덱싱하기 때문에, 어긋나면 불러올 때 다른 가구가 나온다.
    가구를 추가할 때는 **배열 끝에만 붙인다** (중간에 끼우면 이미 저장된 데이터가 전부 밀린다)
  - 워크스테이션은 1인용 책상 2종(`Table_OfficeDesk`, `Table_OfficeDesk2`)뿐이다.
    의자/캐비닛/화분은 `_isWorkstation: 0`인 장식
  - 상점 버튼은 `GamePlay` 씬에 이미 있던 `PO_Element` 12칸 중 8칸의 OnClick 인자를 새 프리팹으로 덮어썼다.
    남은 3칸((9)(10)(11))은 아직 회의 책상을 가리킨다
  - **아이콘은 12칸이 전부 같다.** `PO_Element`의 Icon 스프라이트를 인스턴스별로 바꿔야 구분된다
- [ ] **앉는 애니메이션 미구현**
  - `EmployeeWorkAI.cs`의 `//TODO: Animator Controller...` 그대로.
    직원이 책상 옆에 서 있기만 해서 "앉는다"는 느낌이 안 난다
- [x] **`AllCreatePlacedObjects` 중복 방지 가드**
  - `Start()`에서도 부르고 `_onChangedEvent`로도 불린다. 지금은 호출 순서상 한 번만 돌지만,
    리로드/재접속을 넣으면 오브젝트가 두 배로 쌓인다
  - `PlaceSystem._createdObjectIds`(HashSet)로 이미 만든 id는 건너뛴다.
    데이터를 다시 받아와도 같은 오브젝트를 두 번 만들지 않는다
  - **전부 지우고 다시 만드는 방식은 쓸 수 없다.** 이번 세션에 새로 놓은 오브젝트는
    `SendPlaceableObject()`가 서버에만 쓰고 로컬 목록에는 없어서 같이 사라진다.
    그래서 배치 확정 시 `RegisterPlacedObjectData()`로 목록에도 넣어준다

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

## 🟠 P1 — 성능 / 비용

- [ ] **`GameServerStart` 문서 단일 조회로 리팩터링**
  - 현재: `GamePlayUser/{nickname}`을 필드마다 통째로 재조회(약 9회)
  - 개선: 문서를 **한 번만** 읽어 `Dictionary`로 받은 뒤 각 매니저에 필드 분배
  - `GetFirestoreData(collection, id, key)` → `GetDocument(collection, id)` 형태의 API 추가
- [ ] **정렬/이진탐색 공용화**
  - Mission/Employee/Recruitment의 복붙된 O(n²) 정렬·이진탐색 제거
  - `id` 기준 공용 유틸(또는 `List.Sort` + `List.BinarySearch`)로 통합

## 🟡 P2 — 안정성 / 버그

- [ ] **`GetFirestoreData` 방어 코드**
  - `db == null` 시 조기 반환(로그만 남기고 진행 금지)
  - 스냅샷 미존재/`result == null` 시 `ContainsKey` 전에 null 체크 → NRE 방지
- [ ] **이진탐색 경계 처리**
  - 빈 리스트 / id 미존재 시 인덱싱 전에 범위 검사 (IndexOutOfRange 방지)
- [ ] **`AuthStatusChanged` 로직 정리**
  - `LoginEvent(true)` 직후 무조건 `LoginEvent(false)` 호출되는 흐름 재검토
  - `isInit` 가드에 의존하지 않도록 명확화

## 🟢 P3 — 미완성 / 정리

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

## 진행 기록

<!-- 작업 완료 시 날짜와 함께 여기에 기록 -->
- (2026-07-22) 코드베이스 분석 및 TODO 작성
- (2026-08-29) 오브젝트 이동 로드맵 정리, (a) 롱프레스 감지 구현
- (2026-08-30) (b)~(e) 이동 모드 구현. 드래그 중 자기 콜라이더에 레이가 맞던 문제 + 바닥 못 찾으면 원점으로 순간이동하던 문제 같이 수정
- (2026-08-30) 롱프레스-PlaceSystem 사이 SO 이벤트 채널 제거, 직접 참조로 변경 (씬 배선 0개)
- (2026-08-30) 오브젝트 90도 회전 구현. 회전을 서버에 저장하고, 그 김에 비대칭이던 칸 수 계산과
  회전하면 어긋나던 시작 모서리 계산을 바로잡음
- (2026-08-30) 칸 수 계산에 여유 한 칸(`TilePadding`)을 되살림. 회전 작업에서 실측값으로 바꾸면서
  예전 `+1` 여백이 사라져 배치 범위가 줄어 보였다 (회의 책상 3x5 -> 2x5 -> 3x6)
- (2026-08-30) 씬에 남아있던 `DebugNav`(랜덤 배회 테스트 스크립트)를 지우고 그 동작을
  `EmployeeWorkAI`의 Idle 상태 행동(`WanderRoutine`)으로 옮김. 목적지를 NavMesh에 스냅하고,
  책상으로 이동중/근무중일 때는 목적지를 덮어쓰지 않는다
- (2026-08-30) 배치 가능 가구 8종 추가 (1인용 책상 2, 의자 3, 캐비닛 2, 화분 1)

---

## 발견한 버그 (아직 안 고침)

- **저장한 위치가 중심이 아니라 모서리다**
  - `PlaceSystem.PlaceHandlingObject()`가 `SetPosition(GetStartPosition())`로 **모서리** 좌표를 저장하는데
    (`PlaceSystem.cs:314`), 불러올 때 `PlaceableObject.SetPlacedObjectData()`는 그 값을
    `transform.position`(**중심**)에 그대로 넣는다
  - 그래서 껐다 켤 때마다 모든 오브젝트가 모서리 오프셋만큼(회의 책상 기준 x -1.5, z -4.1) 밀린다.
    회전을 넣으면 밀리는 방향까지 각도에 따라 달라진다
  - 고치려면 저장 값을 `transform.position`으로 바꾸면 되는데, 이미 저장돼 있는 데이터의
    해석이 바뀌므로(기존 오브젝트가 한 번 더 움직인다) 별도로 판단할 것

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
  - 멀티터치(핀치 줌)와 UI 위 터치는 무시. `_isHandlingEvent`가 true면(이미 무언가 들고 있으면) 동작 안 함
- [ ] **(b) `PlaceSystem.StartMoveMode(PlaceableObject)`**
  - `HandlingObject`를 다시 붙여 `Init(...)`, `selectedObject`에 대입, `_isHandlingEvent.RaiseEvent(true)`
  - `StartPlaceMode`와 대부분 공유 가능
- [ ] **(c) 이동 시작 시 `_placedObjects`에서 제거**
  - 타일 칠하기/지우기 로직 자체는 이미 맞다. `SetArea()`가 매 프레임 전체를 지우고 다시 그리고,
    handling 중인 오브젝트는 `_placedObjects`에 없어서 자기 타일과 충돌하지 않는다
  - **다만 이동은 다르다.** 이미 놓인 오브젝트는 `_placedObjects`에 들어있는데,
    이 리스트에는 `Add`만 있고 `Remove`가 한 번도 없다 (`PlaceSystem.cs:134,223`)
  - 빼주지 않으면 `SetAllArea(true)`가 자기 옛 발자국을 칠하고,
    `CheckTile()`이 `_redTile`을 보고 **제자리에 다시 놓는 것조차 거부한다** (`PlaceSystem.cs:283`)
- [ ] **(d) 취소(deny)는 Destroy가 아니라 원위치**
  - 지금 `TakeOffObject()`는 `Destroy(selectedObject.gameObject)` (`PlaceSystem.cs:216`)
  - 이동 모드는 시작 위치를 기억해뒀다가 되돌려야 하므로 분기 필요
  - 덤: 이 함수는 `selectedObject`가 null이면 그냥 터진다. `_denyEvent` 경로에 가드가 없음
- [ ] **(e) 이동 시 object_id를 올리지 않기**
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

- [ ] **삭제 / 판매 경로가 아예 없음**
  - 오브젝트를 놓기만 하고 치울 수 없다. `DeleteFirebaseEventChannelSO`는 만들어져 있는데
    `PlacedObjectManager`에서 쓰지 않는다
  - 롱프레스 UI에 "이동 / 삭제"를 같이 붙이면 한 번에 끝난다
- [ ] **회전 없음**
  - `EmployeeWorkAI.ArriveAtDesk()`가 `transform.rotation = _seat.rotation`으로 앉는 방향을 정하는데,
    회전이 없으면 모든 책상이 같은 방향만 본다 (벽에 붙인 책상 방향이 안 맞음)
  - `PlaceableObject.CalculateTileSize()`의 Size 계산(x축만 `*2 + 1`인 비대칭)까지 건드려야 해서
    이동보다 무겁다. 이동 다음에 할 것
- [ ] **배치 가능 가구가 1종뿐**
  - `Assets/Prefab/Object/Placed/`에 `Table_Conference` 하나. `_isWorkstation: 1`인 것도 그것뿐
- [ ] **앉는 애니메이션 미구현**
  - `EmployeeWorkAI.cs`의 `//TODO: Animator Controller...` 그대로.
    직원이 책상 옆에 서 있기만 해서 "앉는다"는 느낌이 안 난다
- [ ] **`AllCreatePlacedObjects` 중복 방지 가드**
  - `Start()`에서도 부르고 `_onChangedEvent`로도 불린다. 지금은 호출 순서상 한 번만 돌지만,
    리로드/재접속을 넣으면 오브젝트가 두 배로 쌓인다

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

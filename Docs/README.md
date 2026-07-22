# CheckCompany 프로젝트 분석

> 현실 목표 달성을 게임의 성장 요소로 삼는 자기개발 회사 경영 시뮬레이션 (Unity + Firebase, 1인 개발)

이 문서는 코드베이스를 분석한 결과입니다. 개선 작업 목록은 [progress.md](progress.md)를 참고하세요.

---

## 1. 기술 스택 & 규모

| 항목 | 내용 |
|------|------|
| 엔진 | Unity (C#) |
| 서버 | Firebase (Auth + **Firestore**) |
| 외부 연동 | Notion API (미완성) |
| 씬 로딩 | Addressables 기반 (`SceneLoader`) |
| 프로젝트 코드 | 자체 스크립트 약 96개 (`Assets/MyAssets/Script`) |
| 테스트 | 없음 (0개) |

---

## 2. 폴더 구조

```
Assets/MyAssets/Script/
├── Event/            # ScriptableObject 이벤트 채널 (Int, Bool, String, Void, Get 등)
├── Server/           # Firebase 연동
│   ├── FirebaseAuthManager.cs    # 로그인 / 회원가입
│   ├── FireStoreManager.cs       # Firestore CRUD (이벤트로 수신)
│   └── NotionAsync.cs            # Notion API (미완성)
├── GamePlay/
│   ├── GameManager.cs            # 싱글톤, 서버 로드 오케스트레이션
│   ├── Manager/                  # Mission/Employee/Recruitment ManagerSO
│   └── Object/                   # Mission, Employee, Recruitment, Date 등 도메인 모델
├── Placed/           # 오브젝트 배치 시스템 (사무실 꾸미기)
├── UI/               # 패널/엘리먼트 UI (Panel, Element 계층)
└── SceneLoader/      # Addressable 씬 전환
```

---

## 3. 아키텍처

### 3.1 이벤트 채널(ScriptableObject) 패턴
로직 간 결합을 낮추기 위해 `...EventChannelSO` 형태의 SO를 매개로 통신합니다. 예를 들어 Firestore 저장은 매니저가 직접 DB를 호출하지 않고 `SendFirebaseEventChannelSO.RaiseEvent(...)`를 발행하면 `FireStoreManager`가 이를 구독해 처리합니다. 서버 계층과 게임 로직이 분리되어 있어 확장에 유리한 구조입니다.

### 3.2 매니저 = ScriptableObject
`MissionManagerSO`, `EmployeeManagerSO`, `RecruitmentManagerSO`가 SO로 구현되어 상태(리스트)와 서버 동기화 로직을 담당합니다. `GameManager`(MonoBehaviour 싱글톤)가 시작 시 각 매니저의 `Init()`을 호출하고 서버 데이터를 로드합니다.

### 3.3 서버 로드 흐름 (`GameManager.GameServerStart`)
```
Auth(CurrentUser.Email)
  → User/{email}/nickname          (닉네임 조회)
  → GamePlayUser/{nickname}/money, employee_count, missions,
    recruitments, employees, date, placeableObjects ...
```
로그인한 이메일로 닉네임을 조회하고, 그 닉네임을 문서 ID로 삼아 게임 데이터를 순차적으로 불러옵니다.

### 3.4 Firestore 데이터 모델
```
User/{email}            → { nickname }
GamePlayUser/{nickname} → {
    money, employee_count, mission_count, date,
    missions:      { id: { type, name, icon, level, todo_missions[], refEmployees[], doneDate } },
    recruitments:  { id: { ..., applicants: { id: {...} } } },
    employees:     { id: { employeeType, stamina, ... } },
    placeableObjects, placeableObject_id
}
```
Firestore의 중첩 맵(dot-path: `missions.{id}.todo_missions`)을 이용해 부분 업데이트합니다.

---

## 4. 강점

- **관심사 분리**: 이벤트 채널 SO로 서버/UI/로직 계층이 느슨하게 결합됨.
- **부분 업데이트 활용**: `UpdateAsync` + dot-path로 필드 단위 동기화 → 불필요한 전체 덮어쓰기 회피.
- **Addressable 씬 관리**: 메뉴/게임 씬을 additive 로드/언로드로 안정적으로 전환.
- **트러블슈팅 문서화**: 중첩 레이아웃 갱신 문제 등 해결 과정을 기록(`Readme/TROUBLESHOOTING.md`).

---

## 5. 주요 이슈 & 개선 포인트

### 🔴 보안 (가장 시급)
- **Firestore 보안 규칙이 저장소에 없음**. 클라이언트가 `money`, `employee_count` 등을 직접 쓰기 때문에 규칙이 없으면 **인증된 아무나 다른 사용자 문서를 조작 가능**(재화 치트). 규칙 파일을 만들어 `GamePlayUser/{nickname}` 쓰기를 소유자로 제한해야 함.
- **재화가 완전 클라이언트 권한**: `SetMoney()`가 검증 없이 서버에 반영됨.
- `apikey.txt`, `google-services.json`은 `.gitignore` 처리됐으나 `google-services.json.meta`, `NotionAPIKeySO.asset`은 추적 중 → 민감정보 노출 여부 점검 필요.

### 🟠 성능 / 비용
- **동일 문서 중복 조회**: `GetFirestoreData`는 문서 전체를 받아 키 하나만 반환. `GameServerStart`에서 `GamePlayUser/{nickname}` 문서를 필드 개수만큼(약 9회) **반복해서 통째로 읽음** → Firestore 읽기 비용·로딩 시간 낭비. 문서를 **한 번만** 읽고 로컬에서 필드를 분배하도록 리팩터링 필요.
- **O(n²) 정렬 중복**: Mission/Employee/Recruitment 3개 매니저가 각각 selection sort + 이진탐색을 복붙. 공용 유틸로 통합하고 `List.Sort`/`BinarySearch` 사용 권장.

### 🟡 안정성 / 버그
- `GetFirestoreData`: `db == null`이면 `"why"`만 로그하고 진행 → NRE. 문서 없거나 result가 null이면 `result.ContainsKey`에서 **NullReferenceException**.
- 이진탐색 헬퍼가 삽입 위치(`start`)를 반환한 뒤 그대로 인덱싱 → **리스트가 비었거나 id 미존재 시 IndexOutOfRange** 위험.
- `FirebaseAuthManager.AuthStatusChanged`: `LoginEvent(true)` 직후 무조건 `LoginEvent(false)` 호출 + `isInit` 가드로 동작이 헷갈림. 로직 정리 필요.

### 🟢 미완성 / 정리
- **로그아웃 미구현**: README엔 있으나 코드는 `//로그아웃 처리` 주석만 존재.
- **Notion 연동 미완성**: `NotionAsync`의 `apiUrl = "api키"` 플레이스홀더, 메서드명 `GetNexonData`(넥슨) 불일치 — 죽은 코드.
- **매직 스트링 남발**: `"GamePlayUser"`, `"missions." + id` 등 컬렉션/필드 경로가 하드코딩 → 상수/헬퍼로 추출.
- **테스트 부재**: 도메인 로직(Date 계산, JSON 직렬화, 이진탐색)에 대한 EditMode 테스트가 전무.

---

## 6. 요약

이벤트 채널 SO 기반의 분리된 구조를 잘 잡아둔 1인 개발 프로젝트입니다. 다만 **① Firestore 보안 규칙 부재(치트 가능)**, **② 동일 문서 반복 조회로 인한 로딩/비용 낭비**, **③ 3중 복붙된 정렬·탐색 코드**가 우선 해결 대상입니다. 구체적 작업은 [progress.md](progress.md)에 우선순위별로 정리했습니다.

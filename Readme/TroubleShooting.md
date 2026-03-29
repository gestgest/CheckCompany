# 트러블슈팅
이 게시물은 CheckCompany 프로젝트를 하면서 생긴 해결 과정을 적은 게시물입니다.

---
### 문제
UI를 자동으로 배치해주는 관리자 컴포턴트인 vertical layout group, horizontal layout group을 계층구조로 사용 시, 동적으로 생성되거나 활성화된 자식 UI의 크기 변화가 부모 레이아웃에 즉시 반영되지 않은 버그가 발생했습니다.
유니티의 기본 Layout 시스템이 하위 요소의 height 변화를 상위 레이아웃 컴포넌트에 자동으로 전파하지 못해 발생하는 레이아웃 갱신 지연 현상이 원인이었습니다.
<img width="636" height="238" alt="image" src="https://github.com/user-attachments/assets/0c61d4d4-c1f9-440e-95de-a8e12573572e" />



### 해결 과정
레이아웃 갱신을 수동으로 제어하는 MultiLayoutGroup 클래스를 설계했습니다.
UI 요소가 생성되는 경우 AddHeight 함수를 호출하여 최상위 부모 오브젝트까지 변경된 높이 값을 역추적하여 갱신했습니다.

<img width="784" height="602" alt="image" src="https://github.com/user-attachments/assets/ec3f12b6-fabb-4bc2-a85f-2a6fb724e633" />


### 결과
메인 미션의 화살표 클릭 시 소미션들이 레이아웃 규칙에 맞춰 펼치고 정렬되는 UI
미션이라는 UI에 화살표를 누르면 숨겨져있던 소미션이 Layout 배치를 받고 정렬되는 것을 볼 수 있습니다.

<img width="410" height="875" alt="image" src="https://github.com/user-attachments/assets/ffed1e97-c529-47f0-9835-4518a42736a3" />

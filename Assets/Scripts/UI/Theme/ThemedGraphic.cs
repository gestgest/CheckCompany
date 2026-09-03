using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로그인 UI에서 실제로 쓰이는 역할 종류. 색은 안 들고 있고 이름표 역할만 한다 - 실제 값은 ThemeSO.GetUIColor().
/// </summary>
public enum UIRole
{
    Backdrop,        //화면 전체를 덮는 배경. 완전 투명(알파 0)으로 둬서 뒤 3D가 비치게 한다
    Surface,         //입력창처럼 반투명한 판
    Button,          //버튼 배경
    PrimaryText,     //제목/버튼 글자처럼 진하게 보여야 하는 텍스트
    PlaceholderText, //입력창 안내문구처럼 옅게 보여야 하는 텍스트
}

/// <summary>
/// UI 그래픽(Image, TextMeshProUGUI 등 Graphic을 상속하는 아무거나) 하나에 붙여서
/// "이 요소는 이런 역할이다"만 표시한다. 색 자체는 안 들고 있다가, UIThemeApplier가
/// 테마를 바꿀 때마다 ThemeSO.GetUIColor(Role)로 물어본 색을 Apply()로 꽂아준다.
///
/// 로그인 씬의 모든 Image/TMP_Text가 지금 유니티/TMP 기본색 그대로라(흰 배경, 회색 텍스트),
/// 다크모드를 실제로 타려면 대상 오브젝트마다 이 컴포넌트를 붙이고 역할만 골라줘야 한다.
/// 하나하나 붙이기 번거로우면 Tools > CheckCompany > UI 역할 자동 태깅 참고 -
/// 흔한 패턴(플레이스홀더는 알파 0.5다 등)을 보고 역할을 미리 찍어준다. 다만 추측이라
/// 항상 결과를 눈으로 확인하고 필요하면 인스펙터에서 역할을 고쳐야 한다.
/// </summary>
[RequireComponent(typeof(Graphic))]
public class ThemedGraphic : MonoBehaviour
{
    [SerializeField] private UIRole _role;

    private Graphic _graphic;

    public UIRole Role => _role;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
    }

    /// <summary>이 요소에 색을 입힌다. 알파도 같이 바뀌므로 반투명 카드/배경도 이걸로 표현한다.</summary>
    public void Apply(Color color)
    {
        if (_graphic == null)
        {
            _graphic = GetComponent<Graphic>();
        }

        if (_graphic != null)
        {
            _graphic.color = color;
        }
    }
}

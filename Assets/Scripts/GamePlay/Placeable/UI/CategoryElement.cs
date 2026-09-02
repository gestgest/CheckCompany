using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 왼쪽의 카테고리 버튼 하나. 누르면 ShopPanel이 넘겨준 전환 함수를
/// 자기 인덱스로 부른다.
///
/// 선택 표시(_selectedBackground)는 여기서 토글하지 않는다 - 패널이 실제로 바뀔 때
/// CategoryPanel.OnPanel/OffPanel이 IsSelected를 맞춰준다. 여기서 직접 토글하면
/// 이전에 눌린 칸이 안 꺼져서 여러 개가 동시에 켜진 것처럼 보인다.
/// </summary>
public class CategoryElement : MonoBehaviour
{
    [SerializeField] private GameObject _selectedBackground;
    [SerializeField] private Image _icon;

    //보통 이 칸 자신에 붙어있다. 비어있으면 직접 찾는다.
    [SerializeField] private Button _button;

    private bool _isSelected;

    //몇 번째 카테고리인지. ShopPanel의 panels 인덱스와 같다.
    private int _index;

    //누르면 부를 전환 함수. ShopPanel이 넘겨준다.
    private Action<int> _onClick;

    /// <summary>ShopPanel이 버튼을 만들면서 부른다.</summary>
    public void Init(Sprite icon, int index, Action<int> onClick, bool isSelected = false)
    {
        _icon.sprite = icon;
        _index = index;
        _onClick = onClick;

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        IsSelected = isSelected;

        if (_button == null)
        {
            Debug.LogError($"[CategoryElement] '{name}' : Button이 없어 누를 수 없습니다.", this);
            return;
        }

        //같은 칸을 다시 Init하면 리스너가 쌓여서 한 번 눌러도 여러 번 전환된다
        _button.onClick.RemoveListener(OnClick);
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        //전환 함수를 안 넘겨주면 눌러도 아무 일이 없어서 원인을 찾기 어렵다
        if (_onClick == null)
        {
            Debug.LogWarning($"[CategoryElement] '{name}' : 누를 때 부를 전환 함수가 없습니다.", this);
            return;
        }

        _onClick(_index);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (_isSelected)
            {
                _selectedBackground.SetActive(true);
            }
            else //클릭 안한 경우
            {
                _selectedBackground.SetActive(false);
            }
        }
    }
}

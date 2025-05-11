using UnityEngine;
using UnityEngine.UI;

public class CategoryElement : MonoBehaviour
{
    [SerializeField] private GameObject _selectedBackground;
    [SerializeField] private Image _icon;

    private bool _isSelected;
    //대충 Panel Switching 하는 함수 변수

    //클릭하면 element가 이미지 변환

    public void Init(Sprite icon, bool isSelected = false)
    {
        _icon.sprite = icon;
        IsSelected = isSelected;
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

    public void SwitchingOnOff()
    {
        IsSelected = !IsSelected;
    }

}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 목록의 칸 하나 (의자, 책상 ...). 누르면 그 오브젝트를 손에 들고 배치 모드로 들어간다.
///
/// 배치 자체는 여기서 하지 않는다 - PlacedObjectManager가 이벤트로 PlaceSystem에 넘기고,
/// 실제로 놓을지는 ok/deny 버튼이 정한다. 이 칸은 "어떤 프리팹인지"만 골라서 넘긴다.
/// </summary>
public class ShopObjectElement : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;

    //아직 오브젝트를 놓을 때 돈이 빠지지는 않는다. 안 넣으면 가격은 그냥 안 보인다.
    [SerializeField] private TextMeshProUGUI _moneyText;

    //보통 이 칸 자신에 붙어있다. 비어있으면 직접 찾는다.
    [SerializeField] private Button _button;

    private ObjectSO _objectSO;
    private PlacedObjectManager _placedObjectManager;

    /// <summary>ShopObjectPanel이 칸을 만들면서 부른다.</summary>
    public void Init(ObjectSO objectSO, PlacedObjectManager placedObjectManager)
    {
        _objectSO = objectSO;
        _placedObjectManager = placedObjectManager;

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        SetUI();

        if (_button == null)
        {
            Debug.LogError($"[ShopObjectElement] '{name}' : Button이 없어 누를 수 없습니다.", this);
            return;
        }

        //같은 칸을 다시 Init하면 리스너가 쌓여서 한 번 눌러도 여러 번 배치된다
        _button.onClick.RemoveListener(Place);
        _button.onClick.AddListener(Place);
    }

    private void SetUI()
    {
        if (_icon != null)
        {
            _icon.sprite = _objectSO.GetIcon();
        }

        if (_nameText != null)
        {
            _nameText.text = _objectSO.GetName();
        }

        if (_moneyText != null)
        {
            _moneyText.text = _objectSO.GetMoney().ToString();
        }
    }

    /// <summary>이 칸의 오브젝트를 손에 들려준다. 버튼 OnClick으로도 직접 연결할 수 있다.</summary>
    public void Place()
    {
        GameObject prefab = _objectSO.GetPrefab();

        //프리팹을 안 넣은 SO는 눌러도 아무 일이 없어서 원인을 찾기 어렵다
        if (prefab == null)
        {
            Debug.LogError(
                $"[ShopObjectElement] '{_objectSO.name}' : prefab이 비어 있어 배치할 수 없습니다.",
                _objectSO);
            return;
        }

        _placedObjectManager.CreatePlaceableObject(prefab);
    }
}

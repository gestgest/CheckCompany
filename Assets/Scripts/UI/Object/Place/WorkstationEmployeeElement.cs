using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직원 배정 창의 직원 한 줄. 누르면 그 직원을 지금 열려 있는 자리에 앉힌다.
/// </summary>
public class WorkstationEmployeeElement : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;

    //"이 자리에 앉아있음" / "다른 자리에 앉아있음" 표시. 오브젝트를 따로 두 개 두지 않는다 -
    //두 조건(isSeatedHere, isSeatedElsewhere && !isSeatedHere)이 겹칠 일이 없어서
    //하나 켜고 텍스트만 바꾸는 걸로 충분하다.
    [SerializeField] private GameObject _statusMark;
    [SerializeField] private TextMeshProUGUI _statusMarkText;

    //보통 이 줄 자신에 붙어있다. 비어있으면 직접 찾는다.
    [SerializeField] private Button _button;

    private Employee _employee;
    private WorkstationAssignPopup _popup;

    public void Init(Employee employee, Sprite icon, bool isSeatedHere, bool isSeatedElsewhere, WorkstationAssignPopup popup)
    {
        _employee = employee;
        _popup = popup;

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_icon != null)
        {
            _icon.sprite = icon;
        }

        if (_nameText != null)
        {
            _nameText.text = employee.Name;
        }

        if (_statusMark != null)
        {
            //이 자리에 앉아있는 직원은 "다른 자리"가 아니다
            bool isSeatedOnlyElsewhere = isSeatedElsewhere && !isSeatedHere;
            bool show = isSeatedHere || isSeatedOnlyElsewhere;

            _statusMark.SetActive(show);

            if (show && _statusMarkText != null)
            {
                _statusMarkText.text = isSeatedHere ? "근무중" : "다른 자리";
            }
        }

        if (_button == null)
        {
            Debug.LogError($"[WorkstationEmployeeElement] '{name}' : Button이 없어 누를 수 없습니다.", this);
            return;
        }

        //이미 이 자리에 앉아있으면 다시 누를 이유가 없다
        _button.interactable = !isSeatedHere;

        //같은 줄을 다시 Init하면 리스너가 쌓인다
        _button.onClick.RemoveListener(Assign);
        _button.onClick.AddListener(Assign);
    }

    /// <summary>이 직원을 지금 열려 있는 자리에 앉힌다.</summary>
    public void Assign()
    {
        _popup.Assign(_employee.ID);
    }
}

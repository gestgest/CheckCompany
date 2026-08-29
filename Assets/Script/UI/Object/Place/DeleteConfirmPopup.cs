using TMPro;
using UnityEngine;

/// <summary>
/// 배치된 오브젝트를 삭제하기 전에 한 번 더 묻는 팝업.
/// 삭제는 되돌릴 수 없고 서버 데이터까지 지우기 때문에 바로 지우지 않는다.
///
/// 흐름 : DeleteObjectButton -> Open() -> (삭제) Confirm() -> PlacedObjectManager.DeleteEvent()
///                                     -> (취소) Close()
///
/// 이 스크립트가 붙은 오브젝트는 항상 켜져 있어야 하고, 실제로 켜고 끄는 것은 _root(딤 배경)다.
/// </summary>
public class DeleteConfirmPopup : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _root; //딤 배경 + 창 전체
    [SerializeField] private TextMeshProUGUI _messageText;

    [TextArea]
    [SerializeField] private string _message = "이 오브젝트를 삭제할까요?\n되돌릴 수 없습니다.";

    [Header("Manager")]
    [SerializeField] private PlacedObjectManager _placedObjectManager;

    [Header("Listening to Event")]
    //배치/이동이 끝나면(오브젝트를 놓거나 취소하면) 팝업이 떠 있을 이유가 없다
    [SerializeField] private BoolEventChannelSO _isHandlingEvent;

    private void Awake()
    {
        if (_messageText != null)
        {
            _messageText.text = _message;
        }

        Close();
    }

    private void OnEnable()
    {
        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised += OnHandlingChanged;
        }
    }

    private void OnDisable()
    {
        if (_isHandlingEvent != null)
        {
            _isHandlingEvent._onEventRaised -= OnHandlingChanged;
        }
    }

    //DeleteObjectButton의 OnClick
    public void Open()
    {
        SetRootActive(true);
    }

    //취소 버튼의 OnClick
    public void Close()
    {
        SetRootActive(false);
    }

    //삭제 버튼의 OnClick
    public void Confirm()
    {
        //PlaceSystem이 오브젝트를 지우면서 버튼들을 정리한다
        Close();

        if (_placedObjectManager == null)
        {
            Debug.LogError("[DeleteConfirmPopup] PlacedObjectManager가 연결되지 않았습니다.", this);
            return;
        }

        _placedObjectManager.DeleteEvent();
    }

    /// <summary>손에 든 오브젝트가 없어지면(놓기/취소/삭제 완료) 팝업도 같이 닫는다.</summary>
    private void OnHandlingChanged(bool isHandling)
    {
        if (!isHandling)
        {
            Close();
        }
    }

    private void SetRootActive(bool isActive)
    {
        if (_root == null)
        {
            Debug.LogError("[DeleteConfirmPopup] _root가 연결되지 않았습니다.", this);
            return;
        }

        _root.SetActive(isActive);
    }
}

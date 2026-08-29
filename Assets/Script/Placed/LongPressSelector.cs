using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 이미 배치된 오브젝트를 일정 시간 이상 꾹 누르면 이동 대상으로 방송한다.
/// CameraMoveManager가 같은 입력(터치/좌클릭)으로 화면을 끌기 때문에,
/// 누른 채로 화면이 _cancelMoveDistance 이상 움직이면 "카메라 드래그"로 보고 롱프레스를 취소한다.
/// PlaceSystem이 있는 오브젝트(Grid_PlaceSystem)에 같이 붙이면 된다.
/// </summary>
public class LongPressSelector : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    [Header("Settings")]
    [SerializeField] private float _longPressSeconds = 0.5f;
    [SerializeField] private float _cancelMoveDistance = 20f; //화면 픽셀 기준
    [SerializeField] private float _rayMaxDistance = 200f;

    [Header("Listening to Event")]
    [SerializeField] private BoolEventChannelSO _isHandlingEvent;

    [Header("Broadcasting on Event")]
    [SerializeField] private GameObjectEventChannelSO _longPressObjectEvent;

    //이미 무언가를 손에 들고 있는 동안(배치/이동 중)에는 새로 잡지 않는다
    private bool _isHandling;

    private bool _isTracking;
    private bool _isFired;
    private float _pressStartTime;
    private Vector2 _pressStartPosition;
    private PlaceableObject _pressTarget;

    private void OnEnable()
    {
        _isHandlingEvent._onEventRaised += SetIsHandling;
    }

    private void OnDisable()
    {
        _isHandlingEvent._onEventRaised -= SetIsHandling;
    }

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void Update()
    {
        //핀치 줌 중에는 롱프레스로 보지 않는다
        if (Input.touchCount > 1)
        {
            CancelTracking();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginTracking();
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            CancelTracking();
            return;
        }

        if (Input.GetMouseButton(0))
        {
            TickTracking();
        }
    }

    private void BeginTracking()
    {
        CancelTracking();

        if (_isHandling || IsPointerOverUI())
        {
            return;
        }

        PlaceableObject target = RaycastPlacedObject(Input.mousePosition);
        if (target == null)
        {
            return;
        }

        _isTracking = true;
        _isFired = false;
        _pressStartTime = Time.unscaledTime;
        _pressStartPosition = Input.mousePosition;
        _pressTarget = target;
    }

    private void TickTracking()
    {
        if (!_isTracking || _isFired)
        {
            return;
        }

        //누르고 있는 사이에 오브젝트가 사라졌을 수 있다
        if (_pressTarget == null)
        {
            CancelTracking();
            return;
        }

        //임계값을 넘게 끌었으면 카메라를 움직이려는 것이므로 롱프레스 취소
        if (Vector2.Distance(Input.mousePosition, _pressStartPosition) > _cancelMoveDistance)
        {
            CancelTracking();
            return;
        }

        if (Time.unscaledTime - _pressStartTime < _longPressSeconds)
        {
            return;
        }

        _isFired = true;
        Fire(_pressTarget);
    }

    private void Fire(PlaceableObject target)
    {
        Debug.Log($"[LongPressSelector] '{target.name}' (id {target.GetObjectID()}) 롱프레스 - 이동 모드 요청", target);

        //아직 (b) StartMoveMode를 붙이지 않았다면 구독자가 없다
        if (_longPressObjectEvent == null || _longPressObjectEvent._onEventRaised == null)
        {
            Debug.LogWarning("[LongPressSelector] _longPressObjectEvent를 받는 쪽이 없습니다. (PlaceSystem.StartMoveMode 연결 필요)");
            return;
        }

        _longPressObjectEvent.RaiseEvent(target.gameObject);
    }

    private void CancelTracking()
    {
        _isTracking = false;
        _isFired = false;
        _pressTarget = null;
    }

    /// <summary>화면 좌표 아래에 있는, 이미 배치가 끝난 PlaceableObject를 찾는다.</summary>
    private PlaceableObject RaycastPlacedObject(Vector3 screenPosition)
    {
        Ray ray = _camera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, _rayMaxDistance))
        {
            return null;
        }

        //콜라이더가 자식에 있을 수 있으므로 부모까지 훑는다
        PlaceableObject placeableObject = hit.collider.GetComponentInParent<PlaceableObject>();

        //Placed가 아니면 지금 손에 들고 있는 오브젝트이므로 잡지 않는다
        if (placeableObject == null || !placeableObject.Placed)
        {
            return null;
        }

        return placeableObject;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void SetIsHandling(bool isHandling)
    {
        _isHandling = isHandling;

        if (isHandling)
        {
            CancelTracking();
        }
    }
}

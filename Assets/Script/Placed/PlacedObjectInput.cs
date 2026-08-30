using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 이미 배치된 오브젝트를 누른 것을 두 가지로 나눠서 처리한다.
/// - 꾹 누르기(_longPressSeconds 이상) : PlaceSystem에 이동 모드를 요청한다
/// - 짧게 누르기                        : _tapEvent로 그 오브젝트를 UI 쪽에 넘긴다 (직원 배정 창 등)
///
/// CameraMoveManager가 같은 입력(터치/좌클릭)으로 화면을 끌기 때문에,
/// 누른 채로 화면이 _cancelMoveDistance 이상 움직이면 "카메라 드래그"로 보고 둘 다 취소한다.
/// PlaceSystem과 같은 오브젝트(Grid_PlaceSystem)에 붙는다 - PlaceSystem이 RequireComponent로 끌고 온다.
/// 둘이 같은 오브젝트에 있으므로 이동 요청은 SO 이벤트 채널을 거치지 않고 직접 호출한다.
/// (탭은 다른 씬의 UI가 받아야 해서 채널을 거친다)
/// </summary>
public class PlacedObjectInput : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private PlaceSystem _placeSystem;

    [Header("Settings")]
    [SerializeField] private float _longPressSeconds = 0.5f;
    [SerializeField] private float _cancelMoveDistance = 20f; //화면 픽셀 기준
    [SerializeField] private float _rayMaxDistance = 200f;

    [Header("Broadcasting on Events")]
    //짧게 누른 오브젝트. 안 넣으면 탭은 그냥 무시되고 롱프레스만 동작한다.
    [SerializeField] private PlaceableObjectEventChannelSO _tapEvent;

    private bool _isTracking;
    private bool _isFired;
    private float _pressStartTime;
    private Vector2 _pressStartPosition;
    private PlaceableObject _pressTarget;

#if UNITY_EDITOR
    //컴포넌트를 붙였을 때 / 인스펙터에서 값이 바뀔 때 자동으로 채워준다
    private void Reset()
    {
        AutoAssignReferences();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }
#endif

    private void AutoAssignReferences()
    {
        if (_placeSystem == null)
        {
            _placeSystem = GetComponent<PlaceSystem>();
        }
    }

    private void Awake()
    {
        AutoAssignReferences();
    }

    /// <summary>
    /// Main Camera는 GamePlay.unity에 있고 PlaceSystem은 MyCompany.unity에 있어서,
    /// Awake 시점에는 아직 Camera.main이 null일 수 있다(씬 로드 순서 미보장).
    /// 그래서 캐싱하지 않고 필요할 때마다 확인한다.
    /// </summary>
    private Camera TargetCamera
    {
        get
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            return _camera;
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
            //손을 뗀 시점에 "짧게 눌렀다 뗀 것"인지 판정한다.
            //CancelTracking()이 상태를 지워버리므로 반드시 그 전에 봐야 한다.
            TryFireTap();
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

        //이미 무언가를 손에 들고 있는 동안(배치/이동 중)에는 새로 잡지 않는다
        if (_placeSystem.IsHandling || IsPointerOverUI())
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

    /// <summary>
    /// 손을 뗀 것이 "탭"이면 이벤트를 쏜다.
    /// 롱프레스가 이미 터졌으면(_isFired) 이동 모드로 들어간 것이므로 탭이 아니다.
    /// 끌어서 취소된 경우는 _isTracking이 이미 false라 여기까지 오지 않는다.
    /// </summary>
    private void TryFireTap()
    {
        if (!_isTracking || _isFired || _pressTarget == null || _tapEvent == null)
        {
            return;
        }

        //누르고 있는 사이에 손가락이 많이 움직였으면 카메라를 끈 것이다
        if (Vector2.Distance(Input.mousePosition, _pressStartPosition) > _cancelMoveDistance)
        {
            return;
        }

        _tapEvent.RaiseEvent(_pressTarget);
    }

    private void Fire(PlaceableObject target)
    {
        Debug.Log($"[PlacedObjectInput] '{target.name}' (id {target.GetObjectID()}) 롱프레스 - 이동 시작", target);

        _placeSystem.StartMoveMode(target);
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
        Camera camera = TargetCamera;

        if (camera == null)
        {
            Debug.LogError("[PlacedObjectInput] Camera.main을 찾지 못했습니다. 게임 카메라에 MainCamera 태그가 있는지 확인하세요.");
            return null;
        }

        Ray ray = camera.ScreenPointToRay(screenPosition);

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
}

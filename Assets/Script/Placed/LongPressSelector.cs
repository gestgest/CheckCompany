using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 이미 배치된 오브젝트를 일정 시간 이상 꾹 누르면 PlaceSystem에 이동 모드를 요청한다.
/// CameraMoveManager가 같은 입력(터치/좌클릭)으로 화면을 끌기 때문에,
/// 누른 채로 화면이 _cancelMoveDistance 이상 움직이면 "카메라 드래그"로 보고 롱프레스를 취소한다.
/// PlaceSystem과 같은 오브젝트(Grid_PlaceSystem)에 붙는다.
/// 둘이 같은 오브젝트에 있으므로 SO 이벤트 채널을 거치지 않고 직접 호출한다.
/// </summary>
[RequireComponent(typeof(PlaceSystem))]
public class LongPressSelector : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private PlaceSystem _placeSystem;

    [Header("Settings")]
    [SerializeField] private float _longPressSeconds = 0.5f;
    [SerializeField] private float _cancelMoveDistance = 20f; //화면 픽셀 기준
    [SerializeField] private float _rayMaxDistance = 200f;

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

    private void Fire(PlaceableObject target)
    {
        Debug.Log($"[LongPressSelector] '{target.name}' (id {target.GetObjectID()}) 롱프레스 - 이동 시작", target);

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
            Debug.LogError("[LongPressSelector] Camera.main을 찾지 못했습니다. 게임 카메라에 MainCamera 태그가 있는지 확인하세요.");
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

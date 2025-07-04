using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMoveManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    
    [SerializeField] private BoolEventChannelSO _isHandlingEventChannel;
    
    private Plane _plane;

    private Vector3 _lastMousePosition;

    private bool _isUI = false;
    private bool _isHandling = false;

    // + HandlingObject 카메라 무빙 기능도 그냥 여기에 넣자.
    private void OnEnable()
    {
        _isHandlingEventChannel._onEventRaised += SetIsHandling;
    }
    private void OnDisable()
    {
        _isHandlingEventChannel._onEventRaised -= SetIsHandling;
    }

    private void Update()
    {
        Vector3 delta1 = Vector3.zero;
        Vector3 delta2 = Vector3.zero;


        //나중에 핸들링오브젝트에 있는 카메라 이동 함수 넣을듯
        if (!_isHandling && Input.touchCount >= 1)
        {
            //ui touch
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                _isUI = true;
                return;
            }

            //정규화
            _plane = new Plane(transform.up, transform.position);
            //// UI 위를 클릭했다면 아무 일도 하지 않는다


            delta1 = PlanePositionDelta(Input.GetTouch(0));
            //움직였다면
            if (Input.GetTouch(0).phase == TouchPhase.Moved && !_isUI)
            {
                _camera.transform.Translate(delta1, Space.World);
            }
        }
        //
        else if (Input.touchCount >= 2)
        {
            Vector3 pos1 = PlanePosition(Input.GetTouch(0).position);
            Vector3 pos2 = PlanePosition(Input.GetTouch(1).position);

            Vector3 before_pos1 = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
            Vector3 before_pos2 = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

            //calc
            float zoom = Vector3.Distance(pos1, pos2) / Vector3.Distance(before_pos1, before_pos2);

            //edge case
            if (zoom == 0f || zoom > 10)
            {
                return;
            }

            _camera.transform.position = Vector3.LerpUnclamped(pos1, _camera.transform.position, 1 / zoom);

            //rotate?
        }
#if UNITY_ANDROID_
        //손에서 떈 경우
        else
        {
            isUI = false;
        }
#endif

#if UNITY_EDITOR  || UNITY_STANDALONE_WIN
        if (EventSystem.current.IsPointerOverGameObject())
        {
            _isUI = true;
        }

        if (!_isHandling && Input.GetMouseButton(0) && !_isUI)
        {
            _plane = new Plane(transform.up, transform.position);

            delta1 = PlanePositionDeltaMouse();
            _camera.transform.Translate(delta1, Space.World);
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isUI = false;
        }
        
        //외부 에디터, UI 스크롤 방지
        if (!IsMouseOverGameView() || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Vector2 mouseScroll = Input.mouseScrollDelta;
        if (mouseScroll.y != 0)
        {
            _camera.transform.Translate(0, 0, mouseScroll.y * 2);
        }

#endif
    }
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

    private Vector3 PlanePositionDeltaMouse()
    {
        Vector3 currentMousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePosition = currentMousePos;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                _isUI = true;
            }

            return Vector3.zero;
        }

        //if (Input.GetMouseButton(0))
        Ray rayBefore = _camera.ScreenPointToRay(_lastMousePosition);
        Ray rayNow = _camera.ScreenPointToRay(currentMousePos);

        if (_plane.Raycast(rayBefore, out float enterBefore) &&
            _plane.Raycast(rayNow, out float enterNow))
        {
            _lastMousePosition = currentMousePos;
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
        }

        return Vector3.zero;
    }
#endif


    //터치한 포지션 카메라 기준으로 delta
    private Vector3 PlanePositionDelta(Touch touch)
    {
        //터치 움직이지 않았냐 => 안 움직였으니 delta값은 0
        if (touch.phase != TouchPhase.Moved)
        {
            return Vector3.zero;
        }

        //전 좌표
        Ray rayBefore = _camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        Ray rayNow = _camera.ScreenPointToRay(touch.position);

        //계산
        if (_plane.Raycast(rayBefore, out float enterBefore)
            && _plane.Raycast(rayNow, out float enterNow))
        {
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
        }

        return Vector3.zero;
    }

    //터치한 좌표 
    private Vector3 PlanePosition(Vector2 screenPos)
    {
        //카메라 좌표 => 스크린 좌표
        Ray rayNow = _camera.ScreenPointToRay(screenPos);

        //3d 뭐시기 계산
        if (_plane.Raycast(rayNow, out float enterNow))
        {
            return rayNow.GetPoint(enterNow);
        }

        return Vector3.zero;
    }

    void SetIsHandling(bool isHandling)
    {
        this._isHandling = isHandling;
    }
    
    
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    
    public bool IsMouseOverGameView()
    {
        //Vector2 mousePosition = GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Input.mousePosition);
        Vector2 mousePosition = Input.mousePosition;

        if (mousePosition.x < 0 || mousePosition.y < 0)
        {
            return false;
        }
        else if (mousePosition.x > Screen.width || mousePosition.y > Screen.height)
        {
            return false;
        }
        return true;
        
    }
    
#endif
}
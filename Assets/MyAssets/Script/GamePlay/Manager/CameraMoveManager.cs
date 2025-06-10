using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMoveManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private Plane _plane;

    private Vector3 _lastMousePosition;

    private bool isUI = false;
    // + HandlingObject 카메라 무빙 기능도 그냥 여기에 넣자.
//#if UNITY_IOS || UNITY_ANDROID

    private void Update()
    {

        Vector3 delta1 = Vector3.zero;
        Vector3 delta2 = Vector3.zero;

        
        //나중에 핸들링오브젝트에 있는 카메라 이동 함수 넣을듯
        if (Input.touchCount >= 1)
        {
            //ui touch
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                isUI = true;
                return;
            }
            //정규화
            _plane = new Plane(transform.up, transform.position);
            //// UI 위를 클릭했다면 아무 일도 하지 않는다


            delta1 = PlanePositionDelta(Input.GetTouch(0));
            //움직였다면
            if (Input.GetTouch(0).phase == TouchPhase.Moved && !isUI)
            {
                _camera.transform.Translate(delta1, Space.World);
            }

        }
        //손에서 떈 경우
        else
        {
            isUI = false;

        }
        
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0) && !isUI)
        {
            _plane = new Plane(transform.up, transform.position);
            
            delta1 = PlanePositionDeltaMouse();
            Debug.Log(delta1);
            _camera.transform.Translate(delta1, Space.World);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isUI = false;
        }
#endif

    }
#if UNITY_EDITOR || UNITY_STANDALONE

    private Vector3 PlanePositionDeltaMouse()
    {
        Vector3 currentMousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePosition = currentMousePos;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                isUI = true;
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

    private Vector3 PlanePositionDelta(Touch touch)
    {
        //터치 움직이지 않았냐 => 안 움직였으니 delta값은 0
        if(touch.phase != TouchPhase.Moved)
        {
            return Vector3.zero;
        }

        //전 좌표
        Ray rayBefore = _camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        Ray rayNow = _camera.ScreenPointToRay(touch.position);

        //계산
        if(_plane.Raycast(rayBefore, out float enterBefore)
            && _plane.Raycast(rayNow, out float enterNow))
        {
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
        }
        return Vector3.zero;
    }

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

}

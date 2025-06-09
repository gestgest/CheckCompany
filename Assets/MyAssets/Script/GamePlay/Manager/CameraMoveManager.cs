using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMoveManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Plane _plane;

    // + HandlingObject 카메라 무빙 기능도 그냥 여기에 넣자.
//#if UNITY_IOS || UNITY_ANDROID

    private void Update()
    {

        Vector3 delta1 = Vector3.zero;
        Vector3 delta2 = Vector3.zero;

        //나중에 핸들링오브젝트에 있는 카메라 이동 함수 넣을듯
        if (Input.touchCount >= 1)
        {
            Debug.Log("엄엄");
            //정규화
            _plane = new Plane(transform.up, transform.position);
            //// UI 위를 클릭했다면 아무 일도 하지 않는다
            //if (EventSystem.current.IsPointerOverGameObject())
            //    return;


            delta1 = PlanePositionDelta(Input.GetTouch(0));
            //움직였다면
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                _camera.transform.Translate(delta1, Space.World);
            }

        }

#if UNITY_DESKTOP
        //디버깅
        if (Input.GetMouseButton(0))
        {
            //정규화
            _plane = new Plane(transform.up, transform.position);
            //// UI 위를 클릭했다면 아무 일도 하지 않는다
            //if (EventSystem.current.IsPointerOverGameObject())
            //    return;

            
            delta1 = PlanePositionDelta(Input.mousePosition);
            //움직였다면
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                _camera.transform.Translate(delta1, Space.World);
            }

        }
#endif

    }

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

        //3d 뭐시기 계산
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

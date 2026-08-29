using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandlingObject : MonoBehaviour
{
    private GameObject _okButton;
    private GameObject _denyButton;
    private Transform _cameraTransform;

    
    private VoidEventChannelSO _takenAreaEvent;
    private Vector3TransformChannelSO _snapCoordinateToGrid;
    
    
    [SerializeField] private Vector2 _screenEdge;
    
    
    //down
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // UI 위를 클릭했다면 아무 일도 하지 않는다
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            Vector3 mousePos = Input.mousePosition;
            MoveObject(mousePos);
            OffButton();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnButton();
        }
    }

    //OnMouseDrag()
    private void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        MoveObject(mousePos);
        MoveCamera(mousePos); //화면 움직이는 함수
    }
    
    public void Init(
        GameObject okButton,
        GameObject denyButton,
        GameObject camera,
        VoidEventChannelSO takenAreaEvent,
        Vector3TransformChannelSO snapCoordinateToGrid)
    {
        this._okButton = okButton;
        this._denyButton = denyButton;

        //camera null
        this._cameraTransform = camera.GetComponent<Transform>();
        
        _takenAreaEvent = takenAreaEvent;
        _snapCoordinateToGrid = snapCoordinateToGrid;
    }
    
    
    private void MoveObject(Vector3 mousePos)
    {
        //화면 포지션 값을 타일맵 좌표로 변환 => 기다려라 
        transform.position = _snapCoordinateToGrid.RaiseEvent(GetObjectPos(mousePos));

        //x축 => 왼쪽 아래, z축 => 오른쪽 아래
        _takenAreaEvent.RaiseEvent();   //색칠
    }

    private void OnButton()
    {
        _okButton.SetActive(true);
        _denyButton.SetActive(true);
        
        //UI도 그거에 따라 옮기는 함수
        _okButton.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-1.0f, 0.0f, -3.0f));
        _denyButton.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-3.0f, 0.0f, -1.0f));
    }
    private void OffButton()
    {
        _okButton.SetActive(false);
        _denyButton.SetActive(false);
    }
    
    ///<summary>
    /// 커서 아래 바닥 좌표를 구한다.
    /// 드래그 중인 오브젝트는 항상 커서 바로 아래(=카메라에 제일 가까움)에 있어서
    /// 단순 Raycast를 쓰면 자기 자신에게 맞아버린다. 그래서 자기 자신은 건너뛴다.
    ///</summary>
    private Vector3 GetObjectPos(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        float nearest = float.MaxValue;
        Vector3 result = Vector3.zero;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            //자기 자신(과 자식)에 맞은 것은 무시
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (!(hit.collider is BoxCollider) || hit.distance >= nearest)
            {
                continue;
            }

            nearest = hit.distance;
            result = hit.point;
            found = true;
        }

        //바닥을 못 찾았으면 제자리를 유지한다.
        //(기존에는 Vector3.zero를 돌려줘서 오브젝트가 월드 원점으로 순간이동했다)
        return found ? result : transform.position;
    }

    private void MoveCamera(Vector3 mousePos)
    {
        Vector3 mouseDir = Vector3.zero;
        
        // <- ^
        //왼쪽
        if (mousePos.x >= Screen.width - _screenEdge.x)
        {
            mouseDir += Vector3.left;
            mouseDir += Vector3.forward;
        }
        else if (mousePos.x <= _screenEdge.x)
        {
            mouseDir += Vector3.right;
            mouseDir += Vector3.back;
        }
        if (mousePos.y >= Screen.height - _screenEdge.y)
        {
            mouseDir += Vector3.left;
            mouseDir += Vector3.back;
        }
        else if (mousePos.y <= _screenEdge.y)
        {
            mouseDir += Vector3.right;
            mouseDir += Vector3.forward;
        }

        Vector3 newpos = _cameraTransform.position + mouseDir.normalized * Time.deltaTime;

        _cameraTransform.position = newpos;
    }
    
}

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
    
    ///<summary>선택된 오브젝트의 포지션 값 출력</summary>
    private Vector3 GetObjectPos(Vector3 mousePos)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out hit))
        {
            BoxCollider boxCollider = hit.collider as BoxCollider;
            if (boxCollider != null)
            {
                return hit.point;
            }
        }
        return Vector3.zero;
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

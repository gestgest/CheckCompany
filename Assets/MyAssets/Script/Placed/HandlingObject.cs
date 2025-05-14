using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingObject : MonoBehaviour
{
    private Vector3 offset;
    Transform _okButtonTransform;
    Transform _denyButtonTransform;

    private VoidEventChannelSO _takenAreaEvent;
    private Vector3EventChannelSO _snapCoordinateToGrid;


    private void OnMouseDown()
    {
        offset = transform.position - MoveObject();
    }

    //OnMouseDrag()
    private void FixedUpdate()
    {
        //마우스에 따라서 포지션값[화면] 수정
        Vector3 pos = MoveObject() + offset;
        
        //화면 포지션 값을 타일맵 좌표로 변환 => 기다려라 
        transform.position = _snapCoordinateToGrid.RaiseEvent(pos);

        //UI도 그거에 따라 옮기는 함수
        _okButtonTransform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-1.0f, 0.0f, -3.0f));
        _denyButtonTransform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-3.0f, 0.0f, -1.0f));
        //x축 => 왼쪽 아래, z축 => 오른쪽 아래

        _takenAreaEvent.RaiseEvent();   //BuildingSystem.instance.TakenArea();
    }

    ///<summary>선택된 오브젝트의 포지션 값 출력</summary>
    private Vector3 MoveObject()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

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

    public void Init(Transform okButton, Transform denyButton, VoidEventChannelSO takenAreaEvent)
    {
        _okButtonTransform = okButton;
        _denyButtonTransform = denyButton;

        _takenAreaEvent = takenAreaEvent;
    }
}

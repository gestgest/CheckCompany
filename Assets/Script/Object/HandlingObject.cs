using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingObject : MonoBehaviour
{
    private Vector3 offset;

    private void OnMouseDown()
    {
        offset = transform.position - ClickObject();
    }

    //OnMouseDrag()
    private void FixedUpdate()
    {
        //마우스에 따라서 포지션값[화면] 수정
        Vector3 pos = ClickObject() + offset;
        //화면 포지션 값을 타일맵 좌표로 변환
        transform.position = BuildingSystem.instance.SnapCoordinateToGrid(pos);
    }

    //클릭한 오브젝트의 포지션 값 출력
    private Vector3 ClickObject()
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
}

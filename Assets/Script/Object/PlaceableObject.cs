using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//건물의 타일맵 정보를 담는 클래스
public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    private Vector3[] vertices;

    private void Start()
    {
        VertexLocalPosition();
        CalculateTileSize();
    }

    public virtual void Place()
    {
        HandlingObject drag = gameObject.GetComponent<HandlingObject>();
        Destroy(drag);

        Placed = true;
    }

    //위치를 Vertex에 넣는 함수  
    public void VertexLocalPosition()
    {
        BoxCollider box = gameObject.GetComponent<BoxCollider>();
        vertices = new Vector3[4];
        //정육면체 아래 면의 사각형의 포지션  
        vertices[0] = box.center + new Vector3(-box.size.x, -box.size.y, -box.size.z) * 0.5f;
        vertices[1] = box.center + new Vector3(box.size.x, -box.size.y, -box.size.z) * 0.5f;
        vertices[2] = box.center + new Vector3(box.size.x, -box.size.y, box.size.z) * 0.5f;
        vertices[3] = box.center + new Vector3(-box.size.x, -box.size.y, box.size.z) * 0.5f;

    }

    //Vertex를 계산해서 타일 사이즈 측정  
    private void CalculateTileSize()
    {
        Vector3Int[] verticesInt = new Vector3Int[vertices.Length];

        for (int i = 0; i < verticesInt.Length; i++)
        {
            Vector3 worldpos = transform.TransformPoint(verticesInt[i]);
            //타일맵 기준  
            verticesInt[i] = BuildingSystem.instance.gridLayout.WorldToCell(worldpos);
        }

        int x = (int)Mathf.Abs((verticesInt[0] - verticesInt[1]).x);
        int y = (int)Mathf.Abs((verticesInt[0] - verticesInt[3]).y); //?  
        Size = new Vector3Int(x, y, 1);
    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(vertices[0]);
    }
}

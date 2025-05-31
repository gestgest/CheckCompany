using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//건물의 타일맵 정보를 담는 클래스
public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    
    private Vector3[] vertices;

    private int object_id;
    [SerializeField] private int property_id;

    public void Init()
    {
        VertexLocalPosition();
        CalculateTileSize();
    }
    public void SetObjectID(int object_id)
    {
        this.object_id = object_id;
    }

    /// <summary> 손에 있는 selectedObject 제거 </summary>
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

        //정육면체 아래 면의 사각형의 포지션  0.74, 2
        vertices[0] = new Vector3(-box.size.x, -box.size.y, -box.size.z) * 0.5f + box.center;
        vertices[1] = new Vector3(box.size.x, -box.size.y, -box.size.z) * 0.5f + box.center;
        vertices[2] = new Vector3(box.size.x, -box.size.y, box.size.z) * 0.5f + box.center;
        vertices[3] = new Vector3(-box.size.x, -box.size.y, box.size.z) * 0.5f + box.center;

    }

    //Vertex를 계산해서 타일 사이즈 측정  
    private void CalculateTileSize()
    {

        int x = (int)Mathf.Abs(vertices[0].x - vertices[1].x) * 2 + 1; //셀 사이즈
        int y = (int)Mathf.Abs(vertices[0].z - vertices[3].z) + 1;

        Size = new Vector3Int(x, 0, y);
        /*
        Vector3Int[] verticesInt = new Vector3Int[vertices.Length];

        for (int i = 0; i < verticesInt.Length; i++)
        {
            
            Vector3 worldpos = transform.TransformPoint(vertices[i]);
            //Debug.Log(worldpos);
            //타일맵 기준  
            verticesInt[i] = BuildingSystem.instance.gridLayout.WorldToCell(worldpos);
        }

        int x = (int)Mathf.Abs(verticesInt[0].x - verticesInt[1].x);

        //Debug.Log("엄x : " + verticesInt[0].x + " "+ verticesInt[3].x);
        //Debug.Log("엄z : " + verticesInt[0].z + " "+ verticesInt[3].z);
        int y = (int)Mathf.Abs(verticesInt[0].z - verticesInt[3].z);  
        */

    }

    public Vector3 GetStartPosition()
    {
        // if (vertices == null)
        //     Init();
        return transform.TransformPoint(vertices[0]); //왜 이게 문제일까
    }

    public Dictionary<string, object> ObjectToJSON()
    {
        Dictionary<string, object> pos = new Dictionary<string, object>()
        {
            { "x", GetStartPosition().x },
            { "y", GetStartPosition().y },
            { "z", GetStartPosition().z },
            
        };

        Dictionary<string, object> result = new Dictionary<string, object>
        {
            {"startPosition", pos},
            {"property_id", property_id},
        };
        return result;
    }
    public int GetObjectID()
    {
        return object_id;
    }
}

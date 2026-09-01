using System;
using UnityEngine;

//놓여진 데이터
[Serializable]
public class PlacedObjectData
{
    [SerializeField] private int id;
    [SerializeField] private int property_id;
    [SerializeField] private Vector3 position;

    //y축 회전(도). 타일 격자에 맞춰야 해서 0/90/180/270만 들어간다.
    [SerializeField] private int rotation;

    public PlacedObjectData(int id, int property_id, Vector3 position, int rotation = 0)
    {
        this.id = id;
        this.property_id = property_id;
        this.position = position;
        this.rotation = rotation;
    }

    public Vector3 GetPosition()
    {
        return position;
    }
    public int GetID()
    {
        return id;
    }
    public int GetPropertyID()
    {
        return property_id;
    }

    public int GetRotation()
    {
        return rotation;
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }

    public void SetRotation(int rotation)
    {
        this.rotation = rotation;
    }
    
}

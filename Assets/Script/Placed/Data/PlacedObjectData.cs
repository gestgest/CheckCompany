using System;
using UnityEngine;

[Serializable]
public class PlacedObjectData
{
    [SerializeField] private int id;
    [SerializeField] private int property_id;
    [SerializeField] private Vector3 position;

    public PlacedObjectData(int id, int property_id, Vector3 position)
    {
        this.id = id;
        this.property_id = property_id;
        this.position = position;
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

    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }
    
}

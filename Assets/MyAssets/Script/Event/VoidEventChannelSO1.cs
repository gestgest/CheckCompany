using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Vector3TransformChannelSO", menuName = "ScriptableObject/Event/Vector3TransformChannelSO")]
public class Vector3TransformChannelSO : ScriptableObject
{
    public Func<Vector3, Vector3> _onEventRaised;

    public Vector3 RaiseEvent(Vector3 pos)
    {
        return _onEventRaised.Invoke(pos);
    }
}

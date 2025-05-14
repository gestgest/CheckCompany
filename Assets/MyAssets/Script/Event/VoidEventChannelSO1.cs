using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Vector3EventChannelSO", menuName = "ScriptableObject/Event/Vector3EventChannelSO")]
public class Vector3EventChannelSO : ScriptableObject
{
    public UnityAction<Vector3> _onEventRaised;

    public void RaiseEvent(Vector3 pos)
    {
        _onEventRaised.Invoke(pos);
    }
}

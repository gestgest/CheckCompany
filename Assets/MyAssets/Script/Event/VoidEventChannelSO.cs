using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEventChannelSO", menuName = "ScriptableObject/Event/VoidEventChannelSO")]
public class VoidEventChannelSO : ScriptableObject
{
    public UnityAction _onEventRaised;

    public void RaiseEvent()
    {
        _onEventRaised.Invoke();
    }
}

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BoolEventChannelSO", menuName = "ScriptableObject/Event/BoolEventChannelSO")]

public class BoolEventChannelSO : ScriptableObject
{
    public UnityAction<bool> _onEventRaised;

    public void RaiseEvent(bool tmp)
    {
        _onEventRaised.Invoke(tmp);
    }
    
}

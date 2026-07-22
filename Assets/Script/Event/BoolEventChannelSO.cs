using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "BoolEventChannelSO", menuName = "ScriptableObject/Event/BoolEventChannelSO")]

public class BoolEventChannelSO : ScriptableObject
{
    public UnityAction<bool> _onEventRaised;

    public void RaiseEvent(bool tmp)
    {
        if (_onEventRaised != null)
            _onEventRaised.Invoke(tmp);
    }
    
}

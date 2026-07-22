using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "String2EventChannelSO", menuName = "ScriptableObject/Event/String2EventChannelSO")]

public class String2EventChannelSO : ScriptableObject
{
    
    public UnityAction<string, string> _onEventRaised;

    public void RaiseEvent(string email, string password)
    {
        _onEventRaised.Invoke(email, password);
    }

}

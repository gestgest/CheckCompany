using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "String4EventChannelSO", menuName = "ScriptableObject/Event/String4EventChannelSO")]

public class String4EventChannelSO : ScriptableObject
{
    public UnityAction<string, string, string, string> _onEventRaised;

    public void RaiseEvent(string name, string email, string password, string confirmPassword)
    {
        _onEventRaised.Invoke(name, email, password, confirmPassword);
    }
    
}

using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SendFirebaseEventChannelSO", menuName = "ScriptableObject/Event/SendFirebaseEventChannelSO")]
public class SendFirebaseEventChannelSO : ScriptableObject
{
    public UnityAction<string, string, string, object> _onSendEventRaised;
    
    public void RaiseEvent(string collection_name, string document_name, string key, object value)
    {
        _onSendEventRaised?.Invoke(collection_name, document_name, key, value);
    }
}

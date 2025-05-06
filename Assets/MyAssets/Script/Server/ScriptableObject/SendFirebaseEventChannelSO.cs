using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "SendFirebaseEventChannelSO", menuName = "ScriptableObject/Event/SendFirebaseEventChannelSO")]
public class SendFirebaseEventChannelSO : ScriptableObject
{
    public UnityEvent<string, string, string, object> OnSendEvent;
    
    public void RaiseEvent(string collection_name, string document_name, string key, object value)
    {
        OnSendEvent?.Invoke(collection_name, document_name, key, value);
    }
}

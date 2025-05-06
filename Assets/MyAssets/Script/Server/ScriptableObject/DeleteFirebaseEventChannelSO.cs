using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DeleteFirebaseEventChannelSO", menuName = "ScriptableObject/Event/DeleteFirebaseEventChannelSO")]
public class DeleteFirebaseEventChannelSO : ScriptableObject
{
    public UnityEvent<string, string, string> OnDeleteEvent;
    
    public void RaiseEvent(string collection_name, string document_name, string key)
    {
        OnDeleteEvent?.Invoke(collection_name, document_name, key);
    }

}

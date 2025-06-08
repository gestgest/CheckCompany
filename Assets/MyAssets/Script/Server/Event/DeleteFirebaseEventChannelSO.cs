using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DeleteFirebaseEventChannelSO", menuName = "ScriptableObject/Event/DeleteFirebaseEventChannelSO")]
public class DeleteFirebaseEventChannelSO : ScriptableObject
{
    public UnityAction<string, string, string> _onDeleteEventRaised;
    
    public void RaiseEvent(string collection_name, string document_name, string key)
    {
        _onDeleteEventRaised?.Invoke(collection_name, document_name, key);
    }

}

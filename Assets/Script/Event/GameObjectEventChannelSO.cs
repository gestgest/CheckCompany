using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameObjectEventChannelSO", menuName = "ScriptableObject/Event/GameObjectEventChannelSO")]
public class GameObjectEventChannelSO : ScriptableObject
{
    public UnityAction<GameObject> _onEventRaised;

    public void RaiseEvent(GameObject gameObject)
    {
        _onEventRaised.Invoke(gameObject);
    }
}

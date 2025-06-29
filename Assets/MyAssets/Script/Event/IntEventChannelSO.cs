using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "IntEventChannelSO", menuName = "ScriptableObject/Event/IntEventChannelSO")]
public class IntEventChannelSO : ScriptableObject
{
    public UnityAction<int> _onEventRaised;

    public void RaiseEvent(int value)
    {
        if (_onEventRaised != null)
        {
            _onEventRaised.Invoke(value);
        }
        else
        {
            Debug.Log("null이어도 노린 거");
        }
    }

}

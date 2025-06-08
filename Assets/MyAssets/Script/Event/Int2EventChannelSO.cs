using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Int2EventChannelSO", menuName = "ScriptableObject/Event/Int2EventChannelSO")]
public class Int2EventChannelSO : ScriptableObject
{
    public UnityAction<int, int> _onEventRaised;

    public void RaiseEvent(int a, int b)
    {
        if (_onEventRaised != null)
        {
            _onEventRaised.Invoke(a, b);
        }
        else
        {
            Debug.LogError("null");
        }
    }
}
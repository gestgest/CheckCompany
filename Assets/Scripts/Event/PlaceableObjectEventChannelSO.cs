using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PlaceableObjectEventChannelSO", menuName = "ScriptableObject/Event/PlaceableObjectEventChannelSO")]

//배치된 오브젝트를 짧게 눌렀을 때 그 오브젝트를 UI 쪽으로 넘긴다.
//누르는 쪽(MyCompany 씬)과 받는 쪽(GamePlay 씬)이 다른 씬이라 직접 참조가 안 되므로 채널을 거친다.
public class PlaceableObjectEventChannelSO : ScriptableObject
{
    public UnityAction<PlaceableObject> _onEventRaised;

    public void RaiseEvent(PlaceableObject placeableObject)
    {
        if (_onEventRaised != null)
            _onEventRaised.Invoke(placeableObject);
    }
}

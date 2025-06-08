using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "LoadEventChannelSO", menuName = "ScriptableObject/Event/LoadEventChannelSO")]

public class LoadEventChannelSO : ScriptableObject
{
    public UnityAction<AssetReference> OnLoadingRequested;

    //로딩 함수
    public void RaiseEvent(AssetReference loadScene)
    {
        if (OnLoadingRequested != null)
        {
            OnLoadingRequested.Invoke(loadScene);
            //Debug.Log(locationToLoad);
        }
        else
        {
            Debug.LogWarning("A Scene loading was requested, but nobody picked it up. " +
                "Check why there is no SceneLoader already present, " +
                "and make sure it's listening on this Load Event channel.");
        }
    }

}

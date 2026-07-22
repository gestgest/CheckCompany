using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class InitializationLoader : MonoBehaviour
{
    //PersistentManager
    //LoginMenu => 로그인 되어있다면 그냥 패스하고 싶은데

    [SerializeField] private AssetReference _managersScene = default;

    //맨 처음 게임 메뉴 Scene
    [SerializeField] private AssetReference  _loginMenuScene = default;
    [SerializeField] private AssetReference  _myCompanyGameScene = default;

    [Header("Listening on Event")] 
    [SerializeField] private BoolEventChannelSO _isLoginEvent;
    
    [Header("Broadcasting on")]
    [SerializeField] private AssetReference _menuToLoadEvent = default; //SceneLoader의 LoadMenu 함수
    [SerializeField] private AssetReference _myCompanyToLoadEvent = default; //SceneLoader의 _loadLocation 함수
    
    private void Start()
    {
        _isLoginEvent._onEventRaised = IsLoginEvent;
        _managersScene.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
    }

    // obj는 _managerScene.LoadSceneAsync() 결과 값
    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {
        _menuToLoadEvent.LoadAssetAsync<LoadEventChannelSO>().Completed
            += LoadMenuScene;
    }
    private void IsLoginEvent(bool isLogin)
    {
        // if (isLogin)
        // {
        //     _myCompanyToLoadEvent.LoadAssetAsync<LoadEventChannelSO>().Completed
        //         += LoadGameScene;
        // }
        // else
        // {
        //     _menuToLoadEvent.LoadAssetAsync<LoadEventChannelSO>().Completed
        //         += LoadMenuScene;
        // }
    }

    //이후 init씬 제거
    private void LoadGameScene(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        obj.Result.RaiseEvent(_myCompanyGameScene);
        SceneManager.UnloadSceneAsync(0); //init 씬 제거
    }
    //이후 init씬 제거
    private void LoadMenuScene(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        obj.Result.RaiseEvent(_loginMenuScene);
        SceneManager.UnloadSceneAsync(0); //init 씬 제거
    }

}

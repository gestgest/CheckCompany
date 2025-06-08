using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class InitializationLoader : MonoBehaviour
{
    //PersistentManager
    //LoginMenu => 로그인 되어있다면 그냥 패스하고 싶은데

    [SerializeField] private AssetReference _managersScene = default;

    //맨 처음 게임 메뉴 Scene
    [SerializeField] private AssetReference  _loginMenuScene = default;

    [Header("Broadcasting on")]
    [SerializeField] private AssetReference _menuToLoadEvent = default; //SceneLoader의 LoadMenu 함수



    private void Start()
    {
        _managersScene.LoadSceneAsync(LoadSceneMode.Additive, true).Completed += LoadEventChannel;
    }

    // obj는 _managerScene.LoadSceneAsync() 결과 값
    private void LoadEventChannel(AsyncOperationHandle<SceneInstance> obj)
    {

        _menuToLoadEvent.LoadAssetAsync<LoadEventChannelSO>().Completed += LoadMainMenu;
    }

    //이후 init씬 제거
    private void LoadMainMenu(AsyncOperationHandle<LoadEventChannelSO> obj)
    {
        //SceneLoader의 LoadMenu 함수 실행
        obj.Result.RaiseEvent(_loginMenuScene);

        SceneManager.UnloadSceneAsync(0); //init 씬 제거
    }
}

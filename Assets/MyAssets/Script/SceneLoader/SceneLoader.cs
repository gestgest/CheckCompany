using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private AssetReference _gameplayScene = default;
    //[SerializeField] private InputReader _inputReader = default;

    [Header("Listening to")]
    [SerializeField] private LoadEventChannelSO _loadLocation = default;
    [SerializeField] private LoadEventChannelSO _loadMenu = default;

    private AsyncOperationHandle<SceneInstance> _loadingOperationHandle;
    private AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle;

    //Parameters coming from scene loading requests
    private AssetReference _sceneToLoad;
    private AssetReference _beforeLoadedScene;
    //private bool _showLoadingScreen;

    private SceneInstance _gameplayManagerSceneInstance = new SceneInstance();
    //private float _fadeDuration = .5f;
    private bool _isLoading = false; //To prevent a new loading request while already loading a new scene

    private void OnEnable()
    {
        _loadLocation.OnLoadingRequested += LoadLocation;
        _loadMenu.OnLoadingRequested += LoadMenu;
    }

    private void OnDisable()
    {
        _loadLocation.OnLoadingRequested -= LoadLocation;
        _loadMenu.OnLoadingRequested -= LoadMenu;
    }

    
    /// <summary>
    /// This function loads the location scenes passed as array parameter
    /// </summary>
    private void LoadLocation(AssetReference locationToLoad)
    {
        Debug.Log("예아" + locationToLoad.ToString());
        if (_isLoading)
            return;

        _sceneToLoad = locationToLoad;
        //_showLoadingScreen = showLoadingScreen;
        _isLoading = true;
        
        

        //처음 시작하면 실행
        if (_gameplayManagerSceneInstance.Scene == null || !_gameplayManagerSceneInstance.Scene.isLoaded)
        {
            //게임매니저 씬 따로 저장
            //AsyncOperationHandle<SceneInstance> _gameplayManagerLoadingOpHandle  
            
            _gameplayManagerLoadingOpHandle = _gameplayScene.LoadSceneAsync(LoadSceneMode.Additive, true);
            _gameplayManagerLoadingOpHandle.Completed += OnGameplayManagersLoaded;
        }
        else
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }

    private void LoadMenu(AssetReference menuToLoad)
    {
        ////Prevent a double-loading, for situations where the player falls in two Exit colliders in one frame
        if (_isLoading)
            return;

        _sceneToLoad = menuToLoad;
        //_showLoadingScreen = showLoadingScreen;
        _isLoading = true; //로딩중

        //만약 메뉴로 가는데 GameManager가 있는 경우 제거하는 코드
        if (_gameplayManagerSceneInstance.Scene != null
            && _gameplayManagerSceneInstance.Scene.isLoaded)
            Addressables.UnloadSceneAsync(_gameplayManagerLoadingOpHandle, true);

        StartCoroutine(UnloadPreviousScene());
    }

    private void OnGameplayManagersLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        _gameplayManagerSceneInstance = _gameplayManagerLoadingOpHandle.Result;

        StartCoroutine(UnloadPreviousScene());
    }


    //씬 제거
    private IEnumerator UnloadPreviousScene()
    {
        //한마디로 게임플레이, 메뉴, 다이얼로그에 관한 입력 함수 비활성화  
        //_inputReader.DisableAllInput();
        //_fadeRequestChannel.FadeOut(_fadeDuration);

        //_fadeDuration초 함수 정지  
        yield return new WaitForSeconds(0.5f);

        //이전 씬 제거 함수라 맨 처음에는 무조건 null이다.  
        if (_beforeLoadedScene != null)
        {
            // 처음 제외하고 씬 전환을 할때에는 무조건 이 라인을 거친다  
            // //유니티 피셜, 만약 이 씬을 참조하는 애들이 없다면 isValid가 true가 된다고 한다.  
            // 즉 안전한 해제법 방식이다.  
            if (_beforeLoadedScene.OperationHandle.IsValid())
            {
                //Unload the scene through its AssetReference, i.e. through the Addressable system  
                _beforeLoadedScene.UnLoadScene();
            }
        }

        LoadNewScene();   //새로운 씬 생성
    }



    //씬 생성
    private void LoadNewScene()
    {

        _loadingOperationHandle = _sceneToLoad.LoadSceneAsync(LoadSceneMode.Additive, true, 0);
        _loadingOperationHandle.Completed += OnNewSceneLoaded;
    }


    private void OnNewSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        //씬 기록
        _beforeLoadedScene = _sceneToLoad;

        Scene s = obj.Result.Scene;
        SceneManager.SetActiveScene(s);
        LightProbes.TetrahedralizeAsync();

        _isLoading = false; //로딩 다했다

        //if (_showLoadingScreen)
        //    _toggleLoadingScreen.RaiseEvent(false);


        //StartGameplay(); //플레이어 스폰
    }


}

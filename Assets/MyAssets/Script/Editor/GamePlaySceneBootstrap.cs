#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Play 버튼을 누르면 GameManager가 있는 GamePlay.unity를 자동으로 additive 로드해준다.
/// MyCompany.unity/DebugCompany.unity 등 테스트하려는 씬 하나만 열어놓고 Play해도
/// GameManager가 항상 존재하게 하기 위함. Play를 끄면 자동으로 다시 닫아 씬 구성을 원래대로 되돌린다.
/// (PersistentManager.unity는 일부러 건드리지 않는다 - 안 열려있어야 로그인 없이 로컬 기본값으로 테스트 가능)
/// </summary>
[InitializeOnLoad]
public static class GamePlaySceneBootstrap
{
    private const string BootScenePath = "Assets/Scenes/Manager/GamePlay.unity";
    private const string InitializationScenePath = "Assets/Scenes/Initialization.unity";
    private const string MenuPath = "Tools/Scene Bootstrap/Auto-Load GamePlay Scene on Play";
    private const string EnabledPrefsKey = "CheckCompany.GamePlaySceneBootstrap.Enabled";
    private const string AddedSessionKey = "CheckCompany.GamePlaySceneBootstrap.Added";

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(EnabledPrefsKey, true);
        set => EditorPrefs.SetBool(EnabledPrefsKey, value);
    }

    static GamePlaySceneBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        Enabled = !Enabled;
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!Enabled)
        {
            return;
        }

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            //Initialization 씬은 자체적으로 매니저/게임 씬을 로드하는 흐름을 갖고 있으므로 건드리지 않는다.
            //(안 그러면 로그인 후 Addressables가 GamePlay.unity를 또 로드해서 중복 로드된다)
            if (IsSceneOpen(InitializationScenePath) || IsSceneOpen(BootScenePath))
            {
                return;
            }

            EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Additive);
            SessionState.SetBool(AddedSessionKey, true);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (!SessionState.GetBool(AddedSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(AddedSessionKey, false);

            Scene scene = SceneManager.GetSceneByPath(BootScenePath);
            if (scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    private static bool IsSceneOpen(string path)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).path == path)
            {
                return true;
            }
        }

        return false;
    }
}
#endif

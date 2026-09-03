using System;
using UnityEngine;

/// <summary>
/// 기기(OS)가 다크모드인지 알려준다.
///
/// 유니티에는 이걸 읽는 API가 없어서 안드로이드 Configuration.uiMode를 JNI로 직접 읽는다.
/// 플러그인 파일(.aar/.java)은 필요 없고 런타임 호출만으로 된다.
///
/// 주의 : 시스템 전역 다크모드는 안드로이드 10(API 29)부터다. 이 프로젝트의 minSdk는 23이라
/// 그보다 낮은 기기에서는 항상 '밝음'으로 나온다 (UI_MODE_NIGHT_UNDEFINED). 그래서
/// 다크모드를 못 읽는 경우는 전부 밝은 테마로 떨어뜨린다 - 로그인 화면이 안 보이는 것보다 낫다.
/// </summary>
public static class DeviceTheme
{
    //android.content.res.Configuration의 상수들. JNI로 필드를 또 읽어올 필요가 없어 그대로 박아둔다.
    private const int UiModeNightMask = 0x30;
    private const int UiModeNightYes = 0x20;

    /// <summary>
    /// 지금 기기가 다크모드인지. 읽지 못하면(에디터, 구버전 안드로이드, 다른 플랫폼) false.
    ///
    /// JNI 호출이라 공짜가 아니다. 매 프레임 부르지 말고 화면에 들어올 때나
    /// 앱으로 돌아왔을 때처럼 바뀔 만한 시점에만 부른다.
    /// </summary>
    public static bool IsDarkMode()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return ReadAndroidDarkMode();
#else
        //에디터에서는 JNI를 쓸 수 없다. ThemeApplier의 강제 모드로 낮/밤을 미리보기한다.
        return false;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool ReadAndroidDarkMode()
    {
        try
        {
            using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject resources = activity.Call<AndroidJavaObject>("getResources"))
            using (AndroidJavaObject configuration = resources.Call<AndroidJavaObject>("getConfiguration"))
            {
                //uiMode는 메서드가 아니라 public 필드라 Get으로 읽는다
                int uiMode = configuration.Get<int>("uiMode");

                return (uiMode & UiModeNightMask) == UiModeNightYes;
            }
        }
        catch (Exception e)
        {
            //기기/런처에 따라 currentActivity를 못 잡는 경우가 있다. 테마 하나 때문에 죽으면 안 된다.
            Debug.LogWarning($"[DeviceTheme] 다크모드를 읽지 못해 밝은 테마로 시작합니다. ({e.Message})");
            return false;
        }
    }
#endif
}

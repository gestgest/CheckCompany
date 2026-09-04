using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 기기의 다크모드에 맞춰 씬의 톤(조명/스카이박스/안개/빌딩 머티리얼)을 낮 또는 밤으로 맞춘다.
/// 로그인 씬의 빈 오브젝트 하나에 붙여두고 인스펙터에서 두 테마를 꽂아주면 된다.
///
/// 씬을 낮/밤 두 개로 만들지 않는 이유는 ThemeSO 주석 참고.
/// </summary>
public class ThemeApplier : MonoBehaviour
{
    /// <summary>어떤 기준으로 테마를 고를지. 에디터에서 낮/밤을 미리보기할 때 강제 모드를 쓴다.</summary>
    private enum Mode
    {
        FollowSystem, //기기(OS) 다크모드를 따라간다
        ForceLight,   //항상 낮
        ForceDark     //항상 밤
    }

    [Header("테마")]
    [SerializeField] private ThemeSO _lightTheme;
    [SerializeField] private ThemeSO _darkTheme;

    //에디터에서는 JNI를 못 써서 시스템 다크모드를 읽을 수 없다(항상 낮으로 나온다).
    //밤 테마를 확인하려면 여기를 ForceDark로 바꾼다.
    [SerializeField] private Mode _mode = Mode.FollowSystem;

    [Header("씬 참조")]
    //테마에 따라 색과 밝기가 바뀔 직사광
    [SerializeField] private Light _sunLight;

    // 라이트/다크모드로 전환시 빌딩이 바뀌는 연출
    [SerializeField] private Renderer[] _buildingRenderers;

    //이 밑에서 ThemedGraphic을 전부 찾아 색을 입힌다 (보통 Canvas 루트).
    //하나하나 배열에 끌어다 놓지 않아도 되도록 뿌리 하나만 받는다 - 로그인 UI에 그래픽이
    //수십 개라 늘어날 때마다 배열을 다시 채우는 건 못 할 짓이다.
    [SerializeField] private Transform _uiRoot;

    //매 프레임 새로 찾지 않도록 한 번만 모아둔다. 씬 로드 후에는 UI 구조가 안 바뀌는 전제.
    private ThemedGraphic[] _themedGraphics;

    //지금 적용되어 있는 테마가 밤인지. 앱으로 돌아왔을 때 실제로 바뀐 경우에만 다시 적용하려고 들고 있는다.
    private bool _isDarkApplied;
    private bool _hasApplied;

    private void Start()
    {
        Apply();
    }

    /// <summary>
    /// 앱을 내렸다가 돌아왔을 때 다시 확인한다.
    /// 유저가 알림창에서 다크모드를 켜고 돌아오는 경우가 흔한데, Start()에서 한 번만 보면 그대로 남는다.
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Apply();
        }
    }

    /// <summary>지금 기준에 맞는 테마를 씬에 입힌다. 바뀐 게 없으면 아무것도 하지 않는다.</summary>
    private void Apply()
    {
        bool isDark = ResolveIsDark();

        //같은 테마를 다시 칠할 이유가 없다 (OnApplicationFocus가 자주 불린다)
        if (_hasApplied && _isDarkApplied == isDark)
        {
            return;
        }

        ThemeSO theme = isDark ? _darkTheme : _lightTheme;

        if (theme == null)
        {
            Debug.LogWarning(
                $"[ThemeApplier] '{name}' : {(isDark ? "밤" : "낮")} 테마가 비어 있습니다. 인스펙터에서 넣어주세요.",
                this);
            return;
        }

        ApplyEnvironment(theme);
        ApplyLight(theme);
        ApplyBuildingMaterial(theme);
        ApplyUIColors(theme);

        _isDarkApplied = isDark;
        _hasApplied = true;
    }

    private bool ResolveIsDark()
    {
        switch (_mode)
        {
            case Mode.ForceLight:
                return false;

            case Mode.ForceDark:
                return true;

            default:
                return DeviceTheme.IsDarkMode();
        }
    }

    private void ApplyEnvironment(ThemeSO theme)
    {
        if (theme.Skybox != null)
        {
            RenderSettings.skybox = theme.Skybox;
        }

        //ambientMode가 Skybox면 ambientLight 값은 무시된다. 테마가 정한 색이 실제로 먹히도록 Flat으로 둔다.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = theme.AmbientColor;

        RenderSettings.fog = theme.FogEnabled;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = theme.FogColor;
        RenderSettings.fogStartDistance = theme.FogStartDistance;
        RenderSettings.fogEndDistance = theme.FogEndDistance;

        //스카이박스를 바꿔도 환경광은 바로 안 따라온다. 갱신해줘야 반사/앰비언트가 새 하늘을 반영한다.
        DynamicGI.UpdateEnvironment();
    }

    private void ApplyLight(ThemeSO theme)
    {
        if (_sunLight == null)
        {
            return;
        }

        _sunLight.color = theme.LightColor;
        _sunLight.intensity = theme.LightIntensity;
    }

    private void ApplyBuildingMaterial(ThemeSO theme)
    {
        Material material = theme.BuildingMaterial;

        //머티리얼을 아직 안 만들었으면 조명/스카이박스만 바뀐다
        if (material == null || _buildingRenderers == null)
        {
            return;
        }

        for (int i = 0; i < _buildingRenderers.Length; i++)
        {
            if (_buildingRenderers[i] == null)
            {
                continue;
            }

            //.material은 렌더러마다 머티리얼 복사본을 만들어서 배칭이 깨진다.
            //여기서는 '어떤 머티리얼을 쓸지'만 바꾸고 머티리얼 속성 자체는 건드리지 않으므로
            //sharedMaterial로 넣어도 에셋이 더러워지지 않는다.
            _buildingRenderers[i].sharedMaterial = material;
        }
    }

    private void ApplyUIColors(ThemeSO theme)
    {
        if (_uiRoot == null)
        {
            return;
        }

        //비활성 오브젝트(지금 안 보이는 Register 패널 등)도 미리 칠해둬야, 나중에 그 패널이
        //열리는 순간 예전 테마 색 그대로 한 프레임 보이는 일이 없다.
        if (_themedGraphics == null)
        {
            _themedGraphics = _uiRoot.GetComponentsInChildren<ThemedGraphic>(true);
        }

        for (int i = 0; i < _themedGraphics.Length; i++)
        {
            if (_themedGraphics[i] == null)
            {
                continue;
            }

            _themedGraphics[i].Apply(theme.GetUIColor(_themedGraphics[i].Role));
        }
    }
}

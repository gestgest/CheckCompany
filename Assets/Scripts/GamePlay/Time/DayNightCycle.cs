using UnityEngine;

/// <summary>
/// 게임 시간(GameDate)에 맞춰 씬 밝기를 조절한다. 낮에 가까울수록 밝고, 밤에 가까울수록 어둡다.
///
/// 기기 다크모드(ThemeApplier)와는 아무 상관이 없다 - 그건 UI를 읽기 편하게 하는 사용자 설정이고,
/// 이건 게임 안에서 지금 몇 시인지의 문제다. 라이트모드로 켜놨다고 밤 9시 사무실이 밝아지면 안 된다.
///
/// 씬에 저장된 밝기를 "한낮 기준"으로 삼고 밤이 될수록 그 값을 깎기만 한다.
/// 그래서 낮 톤을 바꾸고 싶으면 이 컴포넌트가 아니라 평소처럼 씬의 라이트/조명 설정을 만지면 된다.
///
/// 넣을 곳: MyCompany 씬. SceneLoader가 그 씬을 활성 씬으로 지정하는데(SetActiveScene),
/// RenderSettings(앰비언트/스카이박스)는 활성 씬 것만 적용되기 때문이다.
/// GamePlay 씬에 두면 라이트를 인스펙터에 꽂을 수도 없다 - 유니티는 씬을 넘나드는 참조를 막는다.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("시간대 (시)")]
    //이 시각을 지나면 완전한 낮
    [SerializeField] private float _sunriseHour = 7f;

    //이 시각을 지나면 완전한 밤. 퇴근 시간(EmployeeWorkAI 기본 18시)에 맞춰뒀다.
    [SerializeField] private float _sunsetHour = 18f;

    //해가 뜨고 지는 데 걸리는 시간. 2면 퇴근 1시간 전부터 어둑해지기 시작해 19시에 완전한 밤이 된다.
    [SerializeField] private float _transitionHours = 2f;

    [Header("밝기 (한낮을 1로 봤을 때 밤의 배율)")]
    //비워두면 로드된 모든 씬에서 직사광을 찾아 전부 같이 어둡게 한다.
    //지금은 GamePlay와 MyCompany에 하나씩 있어서, 하나만 낮추면 나머지가 계속 비춘다.
    [SerializeField] private Light[] _sunLights;

    //직사광. 0으로 두면 밤에 그림자가 아예 사라져서 오히려 밋밋해진다.
    [Range(0f, 1f)] [SerializeField] private float _nightLightScale = 0.08f;

    //하늘, 그리고 하늘에서 나오는 환경광(Ambient Mode가 Skybox라 둘이 같이 간다).
    //씬을 비추는 빛의 대부분이 이쪽이라, 어둡게 하고 싶으면 여기부터 내린다.
    [Range(0f, 1f)] [SerializeField] private float _nightSkyScale = 0.12f;

    /// <summary>지금 낮인 정도(0 = 완전한 밤, 1 = 한낮). 밤에만 켤 창문 조명 같은 걸 붙일 때 쓴다.</summary>
    public float Daylight => _daylight;

    private float _daylight = 1f;

    //씬에 저장된 "한낮" 기준값. 초기화 때 한 번 읽어두고 여기에 배율만 곱한다.
    private float[] _dayLightIntensities;
    private float _dayAmbientIntensity;
    private float _daySkyExposure;

    //스카이박스 에셋을 직접 고치면 플레이 중 낮춘 밝기가 .mat 파일에 그대로 남는다. 복사본만 건드린다.
    private Material _runtimeSkybox;
    private bool _hasSkyExposure;

    //GameClock은 실제 1초에 게임 1시간을 건너뛴다. 계산한 값을 그대로 쓰면 밝기가 1초마다 뚝뚝 끊긴다.
    //눈에 보이는 밝기는 목표치를 향해 부드럽게 따라가게 한다. (배속을 크게 올릴 일이 없어 상수로 둔다)
    private const float FollowSpeed = 3f;

    //환경광 재계산은 공짜가 아니라 매 프레임 부르면 안 된다. 밝기가 이만큼 변했을 때만 다시 굽는다.
    //해 지는 데 걸리는 12초 동안 50번 정도라 눈에는 이어져 보인다.
    private const float EnvUpdateStep = 0.02f;

    private float _lastBakedDaylight = -1f;

    private bool _initialized;

    private static readonly int ExposureID = Shader.PropertyToID("_Exposure");

    private void OnDestroy()
    {
        //new Material()로 만든 것은 씬을 떠나도 자동으로 지워지지 않는다.
        if (_runtimeSkybox != null)
        {
            Destroy(_runtimeSkybox);
        }
    }

    private void Update()
    {
        //날짜 로딩 전에는 시각이 0시(= 한밤중)로 잡혀 있다. 그대로 쓰면 로딩되는 순간 밝기가 튄다.
        if (GameManager.instance == null || !GameManager.instance.IsDateReady)
        {
            return;
        }

        float target = CalculateDaylight(GetHourOfDay());

        if (!_initialized)
        {
            Initialize();

            //들어오자마자 한낮에서 밤으로 밀려가는 게 보이지 않도록 첫 프레임은 목표치로 바로 맞춘다.
            _daylight = target;
        }
        else
        {
            //Lerp(a, b, speed * dt)는 프레임레이트에 따라 속도가 달라진다. 지수 감쇠로 맞춘다.
            _daylight = Mathf.Lerp(_daylight, target, 1f - Mathf.Exp(-FollowSpeed * Time.deltaTime));
        }

        Apply(_daylight);
    }

    /// <summary>
    /// 기준값 수집을 Start가 아니라 첫 Update에서 하는 이유:
    /// SceneLoader는 씬을 다 띄운 다음에야 SetActiveScene을 부르는데, RenderSettings는 활성 씬 것을 가리킨다.
    /// Start에서 읽으면 아직 GamePlay 씬 설정을 읽고 거기에 스카이박스를 꽂아버릴 수 있고,
    /// 그러면 MyCompany가 활성이 되는 순간 그 설정이 통째로 무시된다.
    /// 날짜가 준비된 시점이면 씬 전환은 이미 끝나 있다.
    /// </summary>
    private void Initialize()
    {
        _initialized = true;

        if (_sunLights == null || _sunLights.Length == 0)
        {
            _sunLights = FindDirectionalLights();
        }

        _dayLightIntensities = new float[_sunLights.Length];

        for (int i = 0; i < _sunLights.Length; i++)
        {
            if (_sunLights[i] != null)
            {
                _dayLightIntensities[i] = _sunLights[i].intensity;
            }
        }

        if (_sunLights.Length == 0)
        {
            Debug.LogWarning($"[DayNightCycle] '{name}' : 직사광을 못 찾았습니다. 환경광과 하늘만 어두워집니다.", this);
        }

        //ambientMode가 Skybox일 때 환경광 세기를 조절하는 값이 이것이다.
        //(Flat 모드로 바꾸면 이 값이 무시되므로, 그때는 ambientLight 색을 직접 깎아야 한다)
        _dayAmbientIntensity = RenderSettings.ambientIntensity;

        SetupSkybox();
    }

    /// <summary>로드된 모든 씬에서 켜져 있는 직사광을 모은다.</summary>
    private Light[] FindDirectionalLights()
    {
        Light[] all = FindObjectsByType<Light>(FindObjectsSortMode.None);
        int count = 0;

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].type == LightType.Directional)
            {
                all[count++] = all[i];
            }
        }

        Light[] result = new Light[count];
        System.Array.Copy(all, result, count);

        return result;
    }

    /// <summary>지금 시각을 0~24 사이 실수로. 12시 30분이면 12.5.</summary>
    private float GetHourOfDay()
    {
        Date date = GameManager.instance._Date;
        return date.Hour + date.Minute / 60f;
    }

    /// <summary>그 시각이 얼마나 낮인지(0 = 완전한 밤, 1 = 한낮).</summary>
    private float CalculateDaylight(float hour)
    {
        float half = Mathf.Max(_transitionHours, 0.01f) * 0.5f;

        //해가 뜨면서 0 → 1로 올라가고, 해가 지면서 다시 1 → 0으로 내려온다.
        //SmoothStep이라 경계에서 밝기가 각지지 않는다.
        float rising = Mathf.SmoothStep(0f, 1f,
            Mathf.InverseLerp(_sunriseHour - half, _sunriseHour + half, hour));
        float setting = Mathf.SmoothStep(0f, 1f,
            Mathf.InverseLerp(_sunsetHour - half, _sunsetHour + half, hour));

        return Mathf.Clamp01(rising - setting);
    }

    private void Apply(float daylight)
    {
        float lightScale = Mathf.Lerp(_nightLightScale, 1f, daylight);

        for (int i = 0; i < _sunLights.Length; i++)
        {
            if (_sunLights[i] != null)
            {
                _sunLights[i].intensity = _dayLightIntensities[i] * lightScale;
            }
        }

        float skyScale = Mathf.Lerp(_nightSkyScale, 1f, daylight);

        if (!_hasSkyExposure)
        {
            //_Exposure가 없는 스카이박스면 하늘은 못 건드리니 환경광 세기만 직접 깎는다.
            RenderSettings.ambientIntensity = _dayAmbientIntensity * skyScale;
            return;
        }

        _runtimeSkybox.SetFloat(ExposureID, _daySkyExposure * skyScale);

        //여기가 핵심이다. 이 씬은 Ambient Mode가 Skybox라 오브젝트를 비추는 빛의 대부분이
        //하늘에서 나오는데, 머티리얼의 _Exposure를 낮춘다고 이미 계산된 환경광이 따라오지는 않는다.
        //다시 구워주지 않으면 하늘만 어두워지고 오브젝트는 계속 대낮 밝기로 남는다.
        if (Mathf.Abs(daylight - _lastBakedDaylight) > EnvUpdateStep)
        {
            _lastBakedDaylight = daylight;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void SetupSkybox()
    {
        Material source = RenderSettings.skybox;

        //_Exposure가 없는 스카이박스(직접 만든 셰이더 등)면 하늘은 그대로 두고 조명만 어두워진다.
        if (source == null || !source.HasProperty(ExposureID))
        {
            return;
        }

        _runtimeSkybox = new Material(source);
        _daySkyExposure = _runtimeSkybox.GetFloat(ExposureID);
        _hasSkyExposure = true;

        RenderSettings.skybox = _runtimeSkybox;
    }
}

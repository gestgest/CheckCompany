using UnityEngine;

/// <summary>
/// 화면 톤 하나(낮/밤)를 통째로 담는 프리셋. 에셋 하나가 테마 하나를 맡는다.
///
/// 빌딩 모델은 낮/밤 공용으로 하나만 쓰고 조명과 머티리얼만 갈아끼운다 -
/// 낮용/밤용 모델을 따로 두면 모델을 고칠 때마다 양쪽을 다 고쳐야 한다.
/// 같은 이유로 씬도 하나만 쓴다 (로그인 UI를 고칠 때 양쪽을 다 고치는 일이 없도록).
/// </summary>
[CreateAssetMenu(fileName = "ThemeSO", menuName = "ScriptableObject/ThemeSO")]
public class ThemeSO : ScriptableObject
{
    [Header("빌딩")]
    //빌딩에 입힐 머티리얼. 밤 테마에는 창문 emissive를 켜둔 것을 넣는다.
    //비워두면 머티리얼은 건드리지 않고 조명만 바꾼다 - FBX 머티리얼을 아직 안 만들었어도
    //조명/스카이박스만으로 낮밤 차이를 먼저 확인할 수 있다.
    [SerializeField] private Material _buildingMaterial;

    [Header("환경")]
    [SerializeField] private Material _skybox;

    //그림자 안쪽의 밝기. 밤에는 어둡고 푸른 쪽으로 내려야 조명 대비가 산다.
    [SerializeField] private Color _ambientColor = new Color(0.5f, 0.5f, 0.5f);

    [Header("직사광 (Directional Light)")]
    [SerializeField] private Color _lightColor = Color.white;
    [SerializeField] private float _lightIntensity = 1f;

    [Header("안개 - 뒤쪽 빌딩을 흐리게 만들어 공간감을 낸다")]
    //URP/포스트프로세싱 없이 깊이감을 내는 제일 싼 방법이라 기본으로 켜둔다.
    [SerializeField] private bool _fogEnabled = true;
    [SerializeField] private Color _fogColor = new Color(0.5f, 0.5f, 0.55f);
    [SerializeField] private float _fogStartDistance = 30f;
    [SerializeField] private float _fogEndDistance = 120f;

    [Header("UI - 로그인 씬 Canvas의 색. 역할은 ThemedGraphic 참고")]
    //화면 전체를 덮는 배경. 알파를 낮춰야(보통 0) 뒤에 3D 빌딩이 비친다.
    [SerializeField] private Color _backdropColor = new Color(1f, 1f, 1f, 0f);

    //입력창처럼 반투명한 판. 배경이 투명해진 자리를 대신해 글자를 읽을 수 있게 잡아준다.
    [SerializeField] private Color _surfaceColor = new Color(1f, 1f, 1f, 0.85f);

    [SerializeField] private Color _buttonColor = new Color(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color _primaryTextColor = new Color(0.12f, 0.14f, 0.19f, 1f);
    [SerializeField] private Color _placeholderTextColor = new Color(0.12f, 0.14f, 0.19f, 0.5f);

    /// <summary>빌딩에 입힐 머티리얼. null이면 머티리얼을 바꾸지 않는다.</summary>
    public Material BuildingMaterial => _buildingMaterial;

    public Material Skybox => _skybox;
    public Color AmbientColor => _ambientColor;
    public Color LightColor => _lightColor;
    public float LightIntensity => _lightIntensity;

    public bool FogEnabled => _fogEnabled;
    public Color FogColor => _fogColor;
    public float FogStartDistance => _fogStartDistance;
    public float FogEndDistance => _fogEndDistance;

    /// <summary>역할 하나에 대응하는 UI 색. ThemedGraphic이 여기로 물어봐서 자기 Graphic에 입힌다.</summary>
    public Color GetUIColor(UIRole role)
    {
        switch (role)
        {
            case UIRole.Backdrop: return _backdropColor;
            case UIRole.Surface: return _surfaceColor;
            case UIRole.Button: return _buttonColor;
            case UIRole.PrimaryText: return _primaryTextColor;
            case UIRole.PlaceholderText: return _placeholderTextColor;
            default: return Color.magenta; //새 역할을 추가하고 여기 안 채운 경우 눈에 띄게
        }
    }
}

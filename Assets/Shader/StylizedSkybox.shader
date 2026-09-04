// 로우폴리 톤에 맞춘 그라데이션 스카이박스 (Built-in 파이프라인 전용).
//
// Skybox/Procedural은 대기 산란을 흉내내서 사실적인 대신 색을 직접 정할 수 없다.
// 여기서는 꼭대기/지평선/지면 세 색을 그대로 받아 섞기만 한다 - 낮/밤 테마가
// 색만 바꿔서 톤을 완전히 갈아끼울 수 있어야 하기 때문.
//
// 텍스처를 안 쓰므로 큐브맵/HDRI를 들고 다닐 필요가 없다.
Shader "Skybox/StylizedGradient"
{
    Properties
    {
        [Header(Sky)]
        _ZenithColor ("하늘 꼭대기", Color) = (0.24, 0.48, 0.82, 1)
        _HorizonColor ("지평선", Color) = (0.80, 0.90, 0.97, 1)
        _GroundColor ("지평선 아래", Color) = (0.70, 0.72, 0.75, 1)

        //지평선을 칼같이 자르면 종이 오린 것처럼 보인다. 살짝 번지게 두는 편이 자연스럽다.
        _HorizonSoftness ("지평선 번짐", Range(0.001, 0.5)) = 0.05

        //1보다 크면 지평선 색이 넓게 깔리고, 작으면 꼭대기 색이 아래까지 내려온다.
        _GradientPower ("그라데이션 곡률", Range(0.2, 4)) = 1.4

        [Header(Sun)]
        //해 위치는 RenderSettings.sun으로 지정된 directional light를 따라간다.
        //밤 테마처럼 해가 필요 없으면 _SunIntensity를 0으로 둔다.
        _SunColor ("해 색", Color) = (1, 0.97, 0.88, 1)
        _SunSize ("해 크기", Range(0, 0.3)) = 0.03
        _SunSoftness ("해 가장자리", Range(0.001, 0.3)) = 0.02
        _SunIntensity ("해 세기", Range(0, 5)) = 1

        _Exposure ("노출", Range(0, 3)) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            half4 _ZenithColor;
            half4 _HorizonColor;
            half4 _GroundColor;
            half _HorizonSoftness;
            half _GradientPower;

            half4 _SunColor;
            half _SunSize;
            half _SunSoftness;
            half _SunIntensity;

            half _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                //dir.y가 1이면 정수리, 0이면 지평선, -1이면 발밑
                float height = dir.y;

                //하늘: 지평선 색에서 꼭대기 색으로
                float t = pow(saturate(height), _GradientPower);
                half3 sky = lerp(_HorizonColor.rgb, _ZenithColor.rgb, t);

                //지평선을 경계로 지면 색과 섞는다
                float aboveHorizon = smoothstep(-_HorizonSoftness, _HorizonSoftness, height);
                half3 color = lerp(_GroundColor.rgb, sky, aboveHorizon);

                //해. 지평선 아래로 내려가면 지면에 가려야 하므로 aboveHorizon을 곱한다.
                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);
                float toSun = dot(dir, sunDir);
                float sun = smoothstep(1.0 - _SunSize - _SunSoftness, 1.0 - _SunSize, toSun);
                color += _SunColor.rgb * sun * _SunIntensity * aboveHorizon;

                color *= _Exposure;

                return half4(color, 1);
            }
            ENDCG
        }
    }

    Fallback Off
}

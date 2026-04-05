Shader "Custom/StandardWithOverlay"
{
    Properties
    {
        _Color          ("Color",                   Color)          = (1,1,1,1)
        _MainTex        ("Albedo (RGB)",             2D)             = "white" {}
        _UseMainTex     ("Use Main Texture",        Range(0,1))     = 1

        [Header(Overlay)]
        _OverlayTex     ("Overlay Mask (Grayscale)", 2D)            = "black" {}
        _OverlayColor   ("Overlay Color",           Color)          = (1,1,1,1)
        _OverlayOpacity ("Overlay Opacity",         Range(0.0,1.0)) = 0.0
        _OverlayThreshold ("Overlay Threshold (UV.x)", Range(0.0,1.0)) = 0.5
        _OverlaySoftness ("Overlay Softness",        Range(0.0,1.0)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _OverlayTex;
        half4     _Color;
        half      _UseMainTex;
        half4     _OverlayColor;
        half      _OverlayOpacity;
        half      _OverlayThreshold;
        half      _OverlaySoftness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_OverlayTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half4 mainCol = tex2D(_MainTex, IN.uv_MainTex);
            half4 albedo = lerp(_Color, mainCol * _Color, _UseMainTex);
            half  mask   = (1-tex2D(_OverlayTex, IN.uv_OverlayTex).r) * _OverlayOpacity;
            half  coord = (1.0 - IN.uv_OverlayTex.x);
            half  halfSoft = saturate(_OverlaySoftness * 0.5);
            half  lower = _OverlayThreshold - halfSoft;
            half  upper = _OverlayThreshold + halfSoft;
            half  thresholdMask = smoothstep(lower, upper, coord);
            mask *= thresholdMask;
            albedo.rgb   = lerp(albedo.rgb, _OverlayColor.rgb, mask);

            o.Albedo = albedo.rgb;
            o.Alpha  = albedo.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}

Shader "Custom/StandardWithOverlay"
{
    Properties
    {
        _Color          ("Color",                   Color)          = (1,1,1,1)
        _MainTex        ("Albedo (RGB)",             2D)             = "white" {}

        [Header(Overlay)]
        _OverlayTex     ("Overlay Mask (Grayscale)", 2D)            = "black" {}
        _OverlayColor   ("Overlay Color",           Color)          = (1,1,1,1)
        _OverlayOpacity ("Overlay Opacity",         Range(0.0,1.0)) = 0.0
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
        half4     _OverlayColor;
        half      _OverlayOpacity;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_OverlayTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            half4 albedo = tex2D(_MainTex,    IN.uv_MainTex) * _Color;
            half  mask   = tex2D(_OverlayTex, IN.uv_OverlayTex).r * _OverlayOpacity;
            albedo.rgb   = lerp(albedo.rgb, _OverlayColor.rgb, mask);

            o.Albedo = albedo.rgb;
            o.Alpha  = albedo.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}

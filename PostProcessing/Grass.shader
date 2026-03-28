Shader "Custom/TextureAlphaColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _NearColor ("Near Color", Color) = (1,1,1,1)
        _FarColor ("Far Color", Color) = (1,0,0,1)
        _NearDistance ("Near Distance", Float) = 0.0
        _FarDistance ("Far Distance", Float) = 50.0
        _BottomColor ("Bottom Color", Color) = (0,0,0,1)
        _HeightBlend ("Height Blend", Float) = 1.0
        _NoiseScale ("Noise Scale", Float) = 1.0
        _NoiseStrength ("Noise Strength", Float) = 0.5
        _MovingColor ("Moving Color", Color) = (1,1,0,1)
        _MovingBlend ("Moving Blend", Range(0,1)) = 1.0
        _WindSpeed ("Wind Speed", Float) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:clip vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;
        half _Metallic;
        half _Smoothness;
        fixed4 _NearColor;
        fixed4 _FarColor;
        float _NearDistance;
        float _FarDistance;
        fixed4 _BottomColor;
        float _HeightBlend;
        float _NoiseScale;
        float _NoiseStrength;
        fixed4 _MovingColor;
        float _MovingBlend;
        float _WindSpeed;
        float _Cutoff;

        // Simple smooth 3D value-noise. Not true Perlin but good for wind/grass animation.
        float hash(float3 p)
        {
            return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453123);
        }

        float noise3d(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            float3 u = f * f * (3.0 - 2.0 * f);

            float n000 = hash(i + float3(0.0, 0.0, 0.0));
            float n100 = hash(i + float3(1.0, 0.0, 0.0));
            float n010 = hash(i + float3(0.0, 1.0, 0.0));
            float n110 = hash(i + float3(1.0, 1.0, 0.0));
            float n001 = hash(i + float3(0.0, 0.0, 1.0));
            float n101 = hash(i + float3(1.0, 0.0, 1.0));
            float n011 = hash(i + float3(0.0, 1.0, 1.0));
            float n111 = hash(i + float3(1.0, 1.0, 1.0));

            float nx00 = lerp(n000, n100, u.x);
            float nx10 = lerp(n010, n110, u.x);
            float nx01 = lerp(n001, n101, u.x);
            float nx11 = lerp(n011, n111, u.x);

            float nxy0 = lerp(nx00, nx10, u.y);
            float nxy1 = lerp(nx01, nx11, u.y);

            float nxyz = lerp(nxy0, nxy1, u.z);

            // remap from [0,1] to [-1,1]
            return nxyz * 2.0 - 1.0;
        }

        // Vertex modifier: displace along normal using smooth noise (animated by time)
        void vert(inout appdata_full v)
        {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float3 p = worldPos.xyz * _NoiseScale;
            // animate noise by advancing z with time*speed
            p += float3(0.0, 0.0, _Time.y * _WindSpeed);
            float n = noise3d(p);
            float3 norm = normalize(v.normal);
            v.vertex.xyz += norm * (n * _NoiseStrength);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            float3 toCam = _WorldSpaceCameraPos - IN.worldPos;
            float dist = length(toCam);
            float range = max(0.0001, _FarDistance - _NearDistance);
            float t = saturate((dist - _NearDistance) / range);

            fixed3 blendCol = lerp(_NearColor.rgb, _FarColor.rgb, t);

            o.Albedo = c.rgb * blendCol;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = c.a;

            // Alpha cutout: discard fragments below cutoff so they write depth
            clip(o.Alpha - _Cutoff);

            // Vertical blend from _BottomColor up to the calculated albedo using single height parameter
            float hb = max(0.0001, _HeightBlend);
            float v = saturate(IN.worldPos.y / hb);
            o.Albedo = lerp(_BottomColor.rgb, o.Albedo, v);

            // Blend a color where the noise/wind is strongest so moving parts can tint differently
            float3 p = IN.worldPos * _NoiseScale;
            p += float3(0.0, 0.0, _Time.y * _WindSpeed);
            float n = noise3d(p);
            float m = saturate(abs(n) * _NoiseStrength * _MovingBlend);
            o.Albedo = lerp(o.Albedo, _MovingColor.rgb, m);
        }
        ENDCG
    }

    FallBack "Standard"
}

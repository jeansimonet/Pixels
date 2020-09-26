Shader "Custom/TriPlanarClearDiceFace"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "white" {}

        _Glossiness("", Range(0, 1)) = 0.5
        [Gamma] _Metallic("", Range(0, 1)) = 0

        _BumpScale("", Float) = 1
        _BumpMap("", 2D) = "bump" {}

        _MapScale("", Float) = 1

        _NumberColor ("Number Color", Color) = (1,1,1,1)

        _GlowTex ("Glow Tex", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows nolightmap alpha:blend

        #pragma target 3.0

        sampler2D _MainTex;

        half _BumpScale;
        sampler2D _BumpMap;

        half _MapScale;

        fixed4 _NumberColor;

        sampler2D _GlowTex;
        fixed4 _GlowColor;

        struct Input
        {
            float2 uv_MainTex;
            float3 localCoord;
            float3 localNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.localCoord = v.vertex.xyz;
            data.localNormal = v.normal.xyz;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Blending factor of triplanar mapping
            float3 bf = normalize(abs(IN.localNormal));
            bf /= dot(bf, (float3)1);

            // Triplanar mapping
            float2 tx = IN.localCoord.yz * _MapScale;
            float2 ty = IN.localCoord.zx * _MapScale;
            float2 tz = IN.localCoord.xy * _MapScale;

            // Base color
            half4 cx = tex2D(_MainTex, tx) * bf.x;
            half4 cy = tex2D(_MainTex, ty) * bf.y;
            half4 cz = tex2D(_MainTex, tz) * bf.z;
            half4 color = (cx + cy + cz) * _Color;

            // Normal map
            half4 nx = tex2D(_BumpMap, tx) * bf.x;
            half4 ny = tex2D(_BumpMap, ty) * bf.y;
            half4 nz = tex2D(_BumpMap, tz) * bf.z;
            o.Normal = UnpackScaleNormal(nx + ny + nz, _BumpScale);

            // Misc parameters
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            float4 glowMask = tex2D(_GlowTex, IN.uv_MainTex);
            float glowStrength = dot(_GlowColor.rgb, float3(0.299f, 0.587f, 0.114f));
            //float numberGlow = glowStrength * glow

            float numberStrength = glowMask.r * (1.0f - glowStrength) * _NumberColor.a;

            o.Emission = _GlowColor * glowMask.a;
            o.Albedo = _NumberColor.rgb * numberStrength + color.rgb * (1.0f - numberStrength);
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

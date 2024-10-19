Shader "Custom/UnderwaterCaustics"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CausticsTex ("Caustics Texture", 2D) = "white" {}
        _CausticsSpeed ("Caustics Speed", Float) = 1.0
        _CausticsIntensity ("Caustics Intensity", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        sampler2D _CausticsTex;
        float _CausticsSpeed;
        float _CausticsIntensity;
        float3 _PlayerPosition;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
            float2 causticsUV = IN.worldPos.xz * 0.1 + _Time.y * _CausticsSpeed;
            float3 caustics = tex2D(_CausticsTex, causticsUV).rgb;
            
            float distanceToPlayer = distance(IN.worldPos, _PlayerPosition);
            float causticsStrength = saturate(1 - distanceToPlayer * 0.1) * _CausticsIntensity;
            
            o.Emission = caustics * causticsStrength;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
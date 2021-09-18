// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/OKLab"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HueTiling("Hue Tiling", float) = .01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "OKLab.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HueTiling;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                // fixed4 col = tex2D(_MainTex, i.uv);
                float h = (i.worldPos.x) * _HueTiling;
                float3 normalizedWorldPos = i.worldPos % float3(1,1,1);
                fixed4 col = fixed4(okhsl_to_srgb(float3(h % 1, 1, .75)).rgb, 1.0);
                // apply fog

                // return fixed4(okhsl_to_srgb(float3(_PerceivedLightness, _A, _B).rgb), 1);

                // col = fixed4(okhsl_to_srgb(float3(h % 1, 1, .75)).rgb, 1.0);
                return col;
            }
            ENDCG
        }
    }
}

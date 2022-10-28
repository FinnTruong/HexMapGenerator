Shader "Unlit/ESB_simple_color"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Specular ("Specular", Range(0.0, 1.1)) = 0.3
        _Factor ("Color Factor", Float) = 0.3
        _Cid ("Color id", Int) = 2
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _VPos ("Vertex Position", vector) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Reflection ("Reflection", Cube) = "black" {}
        _3DTexture ("3D Texture", 3D) = "white" {}
        [Toggle] _Enable ("Enable ?", Float) = 0
        [KeywordEnum(Off, Red, Blue)]
        _Options ("Color Options", Float) = 0
        [Header(Specular Properties)]
        [Space(20)]
         [PowerSlider(3.0)]
        _Brightness ("Brightness", Range (0.01, 1)) = 0.08
        [IntRange]
        _Samples ("Samples", Range (0, 255)) = 100

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
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature _ENABLE_ON
            #pragma multi_compile _OPTIONS_OFF _OPTIONS_RED _OPTIONS_BLUE
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Brightness;
            int _Samples;
            float4 testFunc()
            {
                return float4(1,0,0,0);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 f = testFunc();
                #if _OPTIONS_OFF
                    return float4(1,1,1,1);
                #elif _OPTIONS_BLUE
                    return float4(0,0,1,1);
                #elif _OPTIONS_RED
                    return float4(1,0,0,1);
                #endif
                //fixed4 col = tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
            }
            ENDCG
        }
    }
}

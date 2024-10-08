Shader "UI/StencilMaskTargetInverted"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {} // 마스크 적용할 이미지
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Stencil
            {
                Ref 9987 // 스텐실 참조 값
                Comp notequal // 스텐실 값이 참조 값과 다를 때만 렌더링
            }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = v.texcoord;
                    return o;
                }

                sampler2D _MainTex;
                fixed4 _Color;

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                    return col;
                }
                ENDCG
            }
        }
            Fallback "Transparent/VertexLit"
}

Shader "UI/StencilMaskInverted"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {} // 마스크를 적용할 이미지
        _Color("Color", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Stencil
            {
                Ref 9987 // 스텐실 참조 값
                Comp always // 항상 참조 값을 스텐실 버퍼에 기록
                Pass replace // 패스 시 스텐실 버퍼 값을 참조 값으로 교체
            }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask 0 // 색상은 출력하지 않음

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

                fixed4 frag(v2f i) : SV_Target
                {
                    return fixed4(0, 0, 0, 0); // 투명 색상 (마스크만 적용하므로 출력 없음)
                }
                ENDCG
            }
        }
            Fallback "Transparent/VertexLit"
}

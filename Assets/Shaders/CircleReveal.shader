Shader "UI/CircleReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 1)) = 1
        _AspectRatio ("Aspect Ratio", Float) = 1.777
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Radius;
            float _AspectRatio;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * i.color;

                // 宽高比修正，保证正圆
                float2 uv = i.uv - 0.5;
                uv.x *= _AspectRatio;
                float dist = length(uv);
                float maxDist = length(float2(0.5 * _AspectRatio, 0.5));
                float nd = dist / maxDist;

                // 圆形内部透明，外部黑色
                tex.a *= step(_Radius, nd);
                return fixed4(0, 0, 0, tex.a);
            }
            ENDCG
        }
    }
}
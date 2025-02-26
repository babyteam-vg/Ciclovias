Shader "Custom/RoundedQuadShader"
{
    Properties
    {
        _Radius ("Radius", Range(0, 0.5)) = 0.1 // Radio de los bordes redondeados
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Radius;
            fixed4 _Color; // Color que se pasará desde el script

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5); // Centro del cuadrado
                float distance = length(i.uv - center); // Distancia desde el centro
                float alpha = smoothstep(_Radius, _Radius - 0.01, distance); // Suavizado de bordes
                return fixed4(_Color.rgb, _Color.a * alpha); // Aplica el color y la transparencia
            }
            ENDCG
        }
    }
}
Shader "Custom/SquareRadialBorderShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) // Base color of the border
        _Center ("Center", Vector) = (0,0,0,0) // Center point for the radial effect
        _FadeRadius ("Fade Radius", Float) = 5.0 // Radius at which opacity fades to 0
        _Softness ("Softness", Float) = 0.5 // Controls the smoothness of the opacity transition
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency
            ZWrite Off // Disable depth writing for transparency
            Cull Off // Render both sides of the mesh

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Input structure for the vertex shader
            struct appdata
            {
                float4 vertex : POSITION; // Vertex position
            };

            // Output structure for the vertex shader
            struct v2f
            {
                float4 vertex : SV_POSITION; // Clip space position
                float3 worldPos : TEXCOORD0; // World position of the vertex
            };

            // Shader properties
            float4 _Color;
            float3 _Center;
            float _FadeRadius;
            float _Softness;

            // Vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Transform vertex to clip space
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // Transform vertex to world space
                return o;
            }

            // Fragment shader
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the square distance from the current pixel to the center
                float2 offset = abs(i.worldPos.xz - _Center.xz); // Use X and Z coordinates for 2D plane
                float squareDistance = max(offset.x, offset.y); // Square radial effect

                // Calculate opacity using smoothstep for a softer transition
                float opacity = 1.0 - smoothstep(_FadeRadius - _Softness, _FadeRadius, squareDistance);

                // Return the color with adjusted opacity
                return fixed4(_Color.rgb, _Color.a * opacity);
            }
            ENDCG
        }
    }
}
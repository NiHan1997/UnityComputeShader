Shader "Custom/Grass"
{
    Properties
    {
        _BaseColor("Base color", Color) = (0, 0.5, 0, 1)
        _TipColor("Tip color", Color) = (0, 1, 0, 1)
    }

    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" }

            Cull Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Lighting.cginc"
            #include "UnityCG.cginc"

            #pragma target 5.0

            float4 _BaseColor;
            float4 _TipColor;

            struct DrawVertex 
            {
                float3 positionWS;
                float height;
            };            
            
            struct DrawTriangle 
            {
                float3 lightingNormalWS;
                DrawVertex vertices[3];
            };

            StructuredBuffer<DrawTriangle> DrawTriangles;

            struct v2f
            {
                float uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 positionCS : SV_POSITION;
            };

            v2f vert(uint vertexID : SV_VERTEXID)
            {
                v2f o;

                DrawTriangle tri = DrawTriangles[vertexID / 3];
                DrawVertex input = tri.vertices[vertexID % 3];

                o.positionWS = input.positionWS;
                o.normalWS = tri.lightingNormalWS;
                o.uv = input.height;
                o.positionCS = UnityWorldToClipPos(input.positionWS);

                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float3 worldPos = i.positionWS;
                fixed3 worldNormal = normalize(i.normalWS);
                fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                fixed3 diffuse = lerp(_BaseColor.rgb, _TipColor.rgb, i.uv) * max(0, dot(worldNormal, worldLightDir));

                return fixed4(diffuse, 1.0);
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}

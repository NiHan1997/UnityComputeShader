Shader "Custom/Grass"
{
    Properties
    {
        _BaseColor("Base color", Color) = (0, 0.5, 0, 1)
        _TipColor("Tip color", Color) = (0, 1, 0, 1)
    }

    SubShader
    {
        Cull Off

        Pass
        {
            Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Lighting.cginc"
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            #pragma multi_compile_fwdbase

            #pragma target 5.0

            float4 _BaseColor;
            float4 _TipColor;

            struct DrawVertex 
            {
                float3 vertex;
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
                float4 pos : SV_POSITION;
                float uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            v2f vert(uint vertexID : SV_VERTEXID)
            {
                v2f o;

                DrawTriangle tri = DrawTriangles[vertexID / 3];
                DrawVertex v = tri.vertices[vertexID % 3];

                o.positionWS = v.vertex;
                o.normalWS = tri.lightingNormalWS;
                o.uv = v.height;
                o.pos = UnityWorldToClipPos(v.vertex);

                TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                float3 worldPos = i.positionWS;
                fixed3 worldNormal = normalize(i.normalWS);
                fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 diffuse = _LightColor0.rgb * lerp(_BaseColor.rgb, _TipColor.rgb, i.uv) * saturate(dot(worldNormal, worldLightDir));

                return fixed4(ambient + diffuse * saturate(shadow + 0.4), 1.0);
            }

            ENDCG
        }

        Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

            #include "UnityCG.cginc"

			#pragma target 5.0
			#pragma multi_compile_shadowcaster

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
                float4 positionCS : SV_POSITION;
            };

            v2f vert(uint vertexID : SV_VERTEXID)
            {
                v2f o;

                DrawTriangle tri = DrawTriangles[vertexID / 3];
                DrawVertex input = tri.vertices[vertexID % 3];

                o.positionCS = UnityWorldToClipPos(input.positionWS);
                o.positionCS = UnityApplyLinearShadowBias(o.positionCS);

                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                SHADOW_CASTER_FRAGMENT(i);
            }

			ENDCG
		}
    }

    FallBack "Diffuse"
}

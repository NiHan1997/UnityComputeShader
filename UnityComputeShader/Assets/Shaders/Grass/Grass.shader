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

        CGINCLUDE

        #include "Lighting.cginc"
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"

        float4 _BaseColor;                  // 草地顶端颜色.
        float4 _TipColor;                   // 草地底部颜色.

        struct DrawVertex 
        {
            float3 vertex;
            float height;
        };            
        
        struct DrawTriangle 
        {
            float3 worldNormal;
            DrawVertex vertices[3];
        };

        StructuredBuffer<DrawTriangle> _DrawTriangles;

        ENDCG

        Pass
        {
            Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 5.0
            #pragma multi_compile_fwdbase
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            v2f vert(uint vertexID : SV_VERTEXID)
            {
                v2f o;

                DrawTriangle tri = _DrawTriangles[vertexID / 3];
                DrawVertex v = tri.vertices[vertexID % 3];

                o.worldPos = v.vertex;
                o.worldNormal = tri.worldNormal;
                o.uv = v.height;
                o.pos = UnityWorldToClipPos(v.vertex);

                TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                float3 worldPos = i.worldPos;
                fixed3 worldNormal = normalize(i.worldNormal);
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

			#pragma target 5.0
			#pragma multi_compile_shadowcaster

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(uint vertexID : SV_VERTEXID)
            {
                v2f o;

                DrawTriangle tri = _DrawTriangles[vertexID / 3];
                DrawVertex v = tri.vertices[vertexID % 3];

                o.pos = UnityWorldToClipPos(v.vertex);
                o.pos = UnityApplyLinearShadowBias(o.pos);

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

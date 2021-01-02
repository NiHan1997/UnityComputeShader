Shader "Custom/SimpleColor"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
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

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            StructuredBuffer<float3> _PositionBuffer;

            struct a2v
            {
                float3 vertex : POSITION;
                uint vid : SV_VERTEXID;
                uint instanceID : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            v2f vert(a2v v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float x = v.instanceID % 10;
                float y = v.instanceID / 10;

                v.vertex = _PositionBuffer[v.vid] + float4(x, 0, y, 0);

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
            }
            
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}

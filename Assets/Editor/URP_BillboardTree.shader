Shader "Custom/URP_Billboard_MultiTree"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { 
            "RenderType" = "TransparentCutout" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "AlphaTest" 
        }

        Pass
        {
            Cull Off 
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Permet de gérer plusieurs arbres différents efficacement
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Requis pour l'instancing
            };

            struct Varyings {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Requis pour l'instancing
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // On met les propriétés dans un buffer pour l'instancing
            CBUFFER_START(UnityPerMaterial)
                float _Cutoff;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                // Récupération de la matrice du Terrain
                float4x4 worldMat = GetObjectToWorldMatrix();
                
                // Position (Translation)
                float3 worldPos = worldMat._m03_m13_m23;
                
                // Scale (Taille individuelle de chaque arbre posé)
                float scaleX = length(float3(worldMat._m00_m10_m20));
                float scaleY = length(float3(worldMat._m01_m11_m21));

                // Direction vers la caméra
                float3 vDir = _WorldSpaceCameraPos - worldPos;
                vDir.y = 0;
                vDir = normalize(vDir);

                float3 up = float3(0, 1, 0);
                float3 right = normalize(cross(up, vDir));

                // Construction du Quad face caméra avec scale
                float3 worldVPos = worldPos + (right * input.positionOS.x * scaleX) + (up * input.positionOS.y * scaleY);

                output.positionCS = TransformWorldToHClip(worldVPos);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Découpe de la transparence
                clip(texColor.a - _Cutoff);
                
                return texColor;
            }
            ENDHLSL
        }
    }
}
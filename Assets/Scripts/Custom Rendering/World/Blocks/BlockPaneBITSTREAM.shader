Shader "Unlit/UnlitShaderExample"
{
    
    Properties //input
    {
//        _MainTex ("Texture", 2D) = "white" {}
        _BlockTexs("Block Textures", 2DArray) = "" {}
        [PerRendererData] _Width("Width", Int) = 1
        [PerRendererData] _Height("Height", Int) = 1
        
    }
    SubShader 
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
        #pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray

            #include "UnityCG.cginc"

            //auto filled out by unity
            struct MeshData //per-vertex mex data
            {
                float4 vertex : POSITION; //vertex position
                // float3 normals : NORMAL;
                float4 color : COLOR;
                // float4 tangent : TANGENT;
                float2 uv : TEXCOORD0; //uv0 coordinants
                // float2 uv1 : TEXCOORD1; //uv1 coordinants
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION; // clip space position
                float2 uv : TEXCOORD0; //can be wahtever you want
                float4 color  : COLOR;
                // float2 uv : TEXCOORD1; //can be wahtever you want
                // float2 uv : TEXCOORD2; //can be wahtever you want
                // float2 uv : TEXCOORD3; //can be wahtever you want
                // float2 uv : TEXCOORD4; //can be wahtever you want
            };

            struct BlockStateRenderData
            {
                int texIndex;
                int animated;
                int numFrames;
                float fps;
            };

            struct BlockRenderData
            {
                int stateI;
                int rotated;
            };

            struct SmartBitStreamStruct
            {
                int currentReadBitIndex;
                int currentReadLengthIndex;
                int segmentLength;
                int numLengths;
            };

            int BitStream_Read(StructuredBuffer<int> stream, int bitIndex, int numBits)
            {
                //& 31 does the same as % 32
                //>>5 does the division by 32
                
                const int arrayIndex = bitIndex >> 5;
                const int shiftAmount = bitIndex & 31;
                const uint mem = stream[arrayIndex];
                const uint mask = numBits == 32 ? 0xFFFFFFFFu : (1u << numBits) - 1;
                const uint shiftedMask = mask << shiftAmount;
                const uint shiftedVal = mem & shiftedMask;
                const uint val = shiftedVal >> shiftAmount;
                
                //Overflows into 2 bits (used to be an if but that is slower then just calculating it and adjusting it afterwards)
                const int actuallyOverflows = (((bitIndex + numBits) >> 5) - arrayIndex); // 1 if true, otherwise 0
                const int numBits2 = (bitIndex + numBits) & 31;
                // the -1 from the mask is automatically correct when numBits2 = 0
                const int overflowArrayIndex = arrayIndex + 1 * actuallyOverflows; // gets the same if its not actually extended
                const uint overflowMem = stream[overflowArrayIndex];
                const uint overflowMask = (1u << numBits2) - 1;
                const uint trueMask = (uint) actuallyOverflows * overflowMask;
                const uint maskedOverflow = overflowMem & trueMask;
                const uint shiftedOverflow = maskedOverflow << (numBits - numBits2);
                const int finalVal = val | shiftedOverflow;
                
                return finalVal;
            }

            int SmartBitStream_ReadNext(StructuredBuffer<int> stream, StructuredBuffer<int> lengths, inout SmartBitStreamStruct sbs)
            {
                const int numBits = lengths[sbs.currentReadLengthIndex];
                const int val = BitStream_Read(stream, sbs.currentReadBitIndex, numBits);
                sbs.currentReadBitIndex += numBits;
                sbs.currentReadLengthIndex = (sbs.currentReadLengthIndex + 1) % sbs.numLengths;
                return val;
            }

            void SmartBitStream_GoToSegment(inout SmartBitStreamStruct sbs, int index)
            {
                sbs.currentReadLengthIndex = 0;
                sbs.currentReadBitIndex = index * sbs.segmentLength;
            }
            
            // sampler2D _MainTex;
            float4 _Color;
            float _Width;
            float _Height;
            StructuredBuffer<int> _BlockDataSBS;
            StructuredBuffer<int> _BlockDataSBSLengths;
            int _BlockDataSBSSegmentLength;
            int _BlockDataSBSNumLength;
            StructuredBuffer<BlockStateRenderData> _BlockStateDataBuffer;
            

            v2f vert (MeshData v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // local space to clip space
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            //bool 0 1
            //int
            // float (32 bit float)
            //half (16 bit float)
            //fixed (lower precision [12?])
            //float4 -> half4 -> fix4
            //float4x4 -> half4x4
            
            UNITY_DECLARE_TEX2DARRAY(_BlockTexs);
            
             fixed4 frag (v2f i) : SV_Target
             {
                 const float time = _Time.y;
                 const float2 uv = i.uv;
                 
                 const float2 block_uv = float2(uv.x * _Width, uv.y * _Height);

                 const float2 tex_uv = fmod(block_uv, 1);

                 const int2 array_uv = int2((int) block_uv.x, (int) block_uv.y);

                 const int block_index = array_uv.y * _Width + array_uv.x;

                 //unpacking block data:

                 SmartBitStreamStruct blockDataSBSStruct = {0, 0, _BlockDataSBSSegmentLength, _BlockDataSBSNumLength};
                 SmartBitStream_GoToSegment(blockDataSBSStruct, block_index);
                 const int stateI = SmartBitStream_ReadNext(_BlockDataSBS, _BlockDataSBSLengths, blockDataSBSStruct);
                 const int rotated = SmartBitStream_ReadNext(_BlockDataSBS, _BlockDataSBSLengths, blockDataSBSStruct);

                 const BlockStateRenderData state = _BlockStateDataBuffer[stateI];

                 // type.animated is 0 if there is no animation so it will destroy any value generated by the rest of it
                 const float texIndex = state.texIndex + state.animated * fmod(floor(time * state.fps), (float) state.numFrames);

                 const float2 adjusted_tex_uv = (rotated ^ 1) * tex_uv + rotated * float2(tex_uv.y, tex_uv.x);

                 // float r = 0;
                 // if (state.texIndex == 1)
                 // {
                 //     r = 1;
                 // }
                 // return float4(r, 0, 0, 1);
                 
                 return UNITY_SAMPLE_TEX2DARRAY(_BlockTexs, float3(adjusted_tex_uv.x, adjusted_tex_uv.y, texIndex)) * i.color;

             }

            ENDCG
        }
    }
}

[Begin_ResourceLayout]
[directives:Profile LOW_PROFILE_OFF LOW_PROFILE]
[directives:Normals NORMAL_OFF NORMAL]
[directives:Tangents TANGENT_OFF TANGENT]

cbuffer Params : register(b0)
{
	int PositionOffset : packoffset(c0.x);
	int NormalOffset : packoffset(c0.y);
	int TangentOffset : packoffset(c0.z);
	int BlendIndicesOffset : packoffset(c0.w);
	int BlendWeightsOffset : packoffset(c1.x);
	uint NumVertices : packoffset(c1.y);
};

StructuredBuffer<float4x4> 	BoneBuffer 			: register(t0);
StructuredBuffer<float> 	PositionBuffer  	: register(t1);
StructuredBuffer<float> 	NormalBuffer  		: register(t2);
StructuredBuffer<float4> 	TangentBuffer		: register(t3);
StructuredBuffer<uint4> 	BoneIndicesBuffer	: register(t4);
StructuredBuffer<float4> 	BoneWeightsBuffer	: register(t5);

RWStructuredBuffer<float> OutputPositionBuffer	: register(u0);
RWStructuredBuffer<float> OutputNormalBuffer	: register(u1);
RWStructuredBuffer<float4> OutputTangentBuffer	: register(u2);

[End_ResourceLayout]

[Begin_Pass:Skinning]

[profile 11_0]
[entrypoints CS = CS]

#if LOW_PROFILE
static const uint numWeights = 2;
#else
static const uint numWeights = 4;
#endif

inline void ApplySkinning(
	inout float3 position,
#if NORMAL
	inout float3 normal,
#endif
#if TANGENT
	inout float4 tangent,
#endif
	uint4 blendIndices,
	float4 blendWeights)
{
	float4x4 m =
		(blendWeights[0] * BoneBuffer[blendIndices[0]]) +
		(blendWeights[1] * BoneBuffer[blendIndices[1]]) +
		(blendWeights[2] * BoneBuffer[blendIndices[2]]) +
		(blendWeights[3] * BoneBuffer[blendIndices[3]]);

#if NORMAL || TANGENT
	float3x3 m3 = (float3x3)m;
#endif

	position = mul(m, float4(position, 1)).xyz;

#if NORMAL
	normal = mul(m3, normal);
#endif

#if TANGENT
	tangent.xyz = mul(m3, tangent.xyz);
#endif

}

[numthreads(256, 1, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	const uint vIndex = threadID.x;

	if (vIndex < NumVertices)
	{
		int pIndex = (vIndex + PositionOffset) * 3;

		// Loads mesh data.
		float x = PositionBuffer[pIndex];
		float y = PositionBuffer[pIndex + 1];
		float z = PositionBuffer[pIndex + 2];
		float3 position = float3(x, y, z);
		uint4 blendIndices = BoneIndicesBuffer[vIndex + BlendIndicesOffset];
		float4 blendWeights = BoneWeightsBuffer[vIndex + BlendWeightsOffset];

#if NORMAL
		int nIndex = (vIndex + NormalOffset) * 3;
		float3 normal = float3(
			NormalBuffer[nIndex],
			NormalBuffer[nIndex + 1],
			NormalBuffer[nIndex + 2]);
#endif
#if TANGENT
		int tIndex = vIndex + TangentOffset;
		float4 tangent = TangentBuffer[tIndex];
#endif	
		ApplySkinning(
			position,
#if NORMAL
			normal,
#endif
#if TANGENT
			tangent,
#endif
			blendIndices,
			blendWeights);

		int oIndex = vIndex * 3;
		// Saves skinned info.
		OutputPositionBuffer[oIndex] = position.x;
		OutputPositionBuffer[oIndex + 1] = position.y;
		OutputPositionBuffer[oIndex + 2] = position.z;
#if NORMAL
		OutputNormalBuffer[oIndex] = normal.x;
		OutputNormalBuffer[oIndex + 1] = normal.y;
		OutputNormalBuffer[oIndex + 2] = normal.z;
#endif
#if TANGENT
		OutputTangentBuffer[vIndex] = tangent;
#endif
	}
}

[End_Pass]
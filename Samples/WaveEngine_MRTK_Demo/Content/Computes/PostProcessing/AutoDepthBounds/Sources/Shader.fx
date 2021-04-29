[Begin_ResourceLayout]

cbuffer Params : register(b0)
{
	float2 DepthRemap : packoffset(c0.x);	
}

Texture2D<float> Depth : register(t0);
RWStructuredBuffer<uint2> Output : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

groupshared uint minDepthUInt;
groupshared uint maxDepthUInt;

[numthreads(16, 16, 1)]
void CS(uint groupIndex : SV_GroupIndex, uint3 threadID : SV_DispatchThreadID)
{
	if (groupIndex == 0)
	{
		minDepthUInt = 0xffffffff;
		maxDepthUInt = 0;
	}

	GroupMemoryBarrierWithGroupSync();
		
	float z = Depth[threadID.xy];
	
	float linealDepth = 1.0 / (DepthRemap.x * z + DepthRemap.y);
	
	if (linealDepth < 1.0)
	{	
		uint depthUInt = asuint(linealDepth);
	
		InterlockedMin(minDepthUInt, depthUInt);
		InterlockedMax(maxDepthUInt, depthUInt);
	}
	
	GroupMemoryBarrierWithGroupSync();
	
	if(groupIndex == 0)
	{
		InterlockedMin(Output[0].x, minDepthUInt);
		InterlockedMax(Output[0].y, maxDepthUInt);
	}
}

[End_Pass]
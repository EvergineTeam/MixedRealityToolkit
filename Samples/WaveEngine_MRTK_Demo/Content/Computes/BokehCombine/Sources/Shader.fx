[Begin_ResourceLayout]
	[directives:Debug Debug_OFF Debug_ON]

cbuffer ParamsBuffer : register(b0)
{
	float FocalRegion : packoffset(c0.x); [Default(1.0)]
	float FadePower : packoffset(c0.y); [Default(1.0)]	
}

cbuffer PerCameraBuffer: register(b1)
{
	float FocalDistance : packoffset(c0.x); [CameraFocalDistance]
	float FarPlane : packoffset(c0.y); [CameraFarPlane]
}

Texture2D<float4> Focus : register(t0);
Texture2D<float4> Near : register(t1);
Texture2D<float4> Far : register(t2);
Texture2D<float> Depth : register(t3);
RWTexture2D<float4> Output : register(u0);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]


[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;

	// Original CoC to guarantee crisp edge of in-focus over far-field
	float z = Depth.SampleLevel(Sampler, uv, 0);
	
	float linearFocalDistance = FocalDistance / FarPlane;
	float linearFocalRegion = FocalRegion / FarPlane;
	float linearFocalRegionOverTwo = linearFocalRegion / 2;
	
	float3 FocusColor = Focus.SampleLevel(Sampler, uv, 0).rgb;

#if Debug_ON

	if (z < linearFocalDistance - linearFocalRegionOverTwo)
	{
		Output[threadID.xy] = float4(FocusColor.x,0,0, 1.0);
	}
	else if (z > linearFocalDistance + linearFocalRegionOverTwo)
	{
		Output[threadID.xy] = float4(0,0,FocusColor.z, 1.0);
	}
	else	
	{
		Output[threadID.xy] = float4(0,FocusColor.y,0, 1.0);
	}	
	
#else	
	float4 FarColor = Far.SampleLevel(Sampler, uv, 0);
	float4 NearColor = Near.SampleLevel(Sampler, uv, 0);	
	
	bool isInFocus = (z >= linearFocalDistance - linearFocalRegionOverTwo) &&
					 (z <= linearFocalDistance + linearFocalRegionOverTwo);
	if (isInFocus) FarColor.w = 0;
	
	// Alpha composite far field on the top of the original scene.
	float3 Result = FarColor.w * FarColor.rgb + (1.0 - FarColor.w) * FocusColor;
	
	// Alpha composite on the near field
	if (NearColor.w > 0) {
		float blendFactor = saturate(NearColor.w * FadePower);
		Result =  blendFactor * (NearColor.rgb) + (1.0 - blendFactor) * Result;
	}

	Output[threadID.xy] = float4(Result, 1.0);
#endif
}

[End_Pass]
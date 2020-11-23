[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float Near : packoffset(c0.x); [CameraNearPlane]
	float Far : packoffset(c0.y); [CameraFarPlane]
	float FocalDistance : packoffset(c0.z); [CameraFocalDistance]
	float FocalRegion : packoffset(c0.w); [Default(1)]
	float FocalLength : packoffset(c1.x); [CameraFocalDistance]
	float Aperture : packoffset(c1.y); [CameraAperture]
	float2 Jitter : packoffset(c1.z); [CameraJitter]
}

Texture2D<float4> Input : register(t0);
Texture2D<float> Depth : register(t1);
RWTexture2D<float4> NearOutput : register(u0); [Output(Input, 1.0)]
RWTexture2D<float4> FarOutput : register(u1); [Output(Input, 1.0)]
RWTexture2D<float> NearCoC : register(u2); [Output(Input, 1.0)]

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

float ComputeCircleOfConfusion(float depth, float linearFocalDistance, float linearFocalRegion)
{
	// artificial area where all content is in focus (starting at FocalLength, ending at FocalLength+FocalRegion)
	if(depth > linearFocalDistance - linearFocalRegion / 2)
	{
		depth = linearFocalDistance + max(0, depth - linearFocalDistance - linearFocalRegion);
	}
	
	// depth of the pixel
	float D = depth;
	// Focal length in mm (Camera propertty e.g 75mm)
	float F = FocalLength;
	// Plane in Focus
	float P = linearFocalDistance;
	// Camera property e.g. 0.5f, like aperture
	float A = Aperture;
	
	// convert units (100=1m) to mm
	P *= 0.001 / 100.0;
	D *= 0.001 / 100.0;
	
	float CoCRadius = Aperture * F * (P - D) / (D * (P - F));
	return saturate(abs(CoCRadius));
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	NearOutput.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	
	float ax = uv.x * 2.0 - 1.0;
	float ay = (1.0 - uv.y) * 2.0 - 1.0;
	float2 ndc = float2(ax,ay) - Jitter;
	float2 unjittered_uv = ndc.xy * float2(0.5,-0.5) + 0.5;

	float3 color = Input.SampleLevel(Sampler, uv, 0).rgb;
	float z = Depth.SampleLevel(Sampler, unjittered_uv,0);	
	
	float linearFocalDistance = FocalDistance / Far;
	float linearFocalRegion = FocalRegion / Far;
	float CircleOfConfusion = ComputeCircleOfConfusion(z, linearFocalDistance, linearFocalRegion);
	
	if(z < linearFocalDistance - linearFocalRegion / 2)
	{
		// Near
		NearOutput[threadID.xy] = float4(color, CircleOfConfusion);
		FarOutput[threadID.xy] = float4(0,0,0,0);	
		NearCoC[threadID.xy] = CircleOfConfusion; // Separate CoC to build Max TileMap
	}
	else if(z > linearFocalDistance + linearFocalRegion / 2)
	{
		// Far
		NearOutput[threadID.xy] = float4(0,0,0,0);
		FarOutput[threadID.xy] = float4(color, CircleOfConfusion);		
		NearCoC[threadID.xy] = 0;
	}
	else
	{
		// In-focus (inside focal region)
		NearOutput[threadID.xy] = float4(0,0,0,0);
		FarOutput[threadID.xy] = float4(0,0,0,0);
		NearCoC[threadID.xy] = 0;
	}
}

[End_Pass]
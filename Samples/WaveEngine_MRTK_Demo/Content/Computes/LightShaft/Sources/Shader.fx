[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float stepAmount : packoffset(c0.x); [Default(0.1)]	
	int numSteps : packoffset(c0.y); [Default(10)]	
}

cbuffer PerFrameBuffer : register(b1)
{	
	float4x4 View: packoffset(c0); [View];
	float4x4 Projection : packoffset(c4); [Projection]
	float3 lightDirection : packoffset(c8.x); [SunDirection]
}

Texture2D input : register(t0);
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
	
	// Remove rays in the evening
	if (lightDirection.y < 0)
	{
		Output[threadID.xy] = float4(0,0,0, 1.0);
		return;
	}
		
	float4x4 unjitteredProjection = Projection;
	unjitteredProjection[2][0] = 0;
	unjitteredProjection[2][1] = 0;
	float2 light = mul(mul(lightDirection, (float3x3)View), (float3x3)unjitteredProjection).xy * float2(0.5,-0.5) + 0.5;
	
	float2 dir = light- uv;
	float2 maxDist = length(dir);	
	dir = normalize(dir);
	
	float3 color = 0;
	
	[loop]
	for (int i=0; i < numSteps; i++)
	{
		float stepLength = min(maxDist, stepAmount * float(i));		
		float2 samplePos = clamp(uv + (stepLength * dir), 0, 1);		

		float3 textureVal = input.SampleLevel(Sampler, samplePos, 0).rgb;
		color += textureVal;		
	}
	color /= float(numSteps);

	Output[threadID.xy] = float4(color, 1.0);
}

[End_Pass]
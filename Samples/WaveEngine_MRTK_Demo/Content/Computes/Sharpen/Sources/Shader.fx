[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float amount : packoffset(c0.x); [Default(0.2)]
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
	float2 texSize = 1.0 / outputSize;
	
	float4 center = input.SampleLevel(Sampler, uv, 0);
	float4 top =	input.SampleLevel(Sampler, uv + int2(0, -1) * texSize, 0);
	float4 left =	input.SampleLevel(Sampler, uv + int2(-1, 0) * texSize, 0);
	float4 right =	input.SampleLevel(Sampler, uv + int2(1, 0) * texSize, 0);
	float4 bottom = input.SampleLevel(Sampler, uv + int2(0, 1) * texSize, 0);

	Output[threadID.xy] = saturate(center + (4 * center - top - bottom - left - right) * amount);
}

[End_Pass]
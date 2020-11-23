[Begin_ResourceLayout]

[directives:Blend Add Min Multiply Pow]

cbuffer ParamsBuffer : register(b0)
{
	float Intensity0 : packoffset(c0.x); [Default(1)]
	float Intensity1 : packoffset(c0.y); [Default(0.5)]
}

Texture2D input0 : register(t0);
Texture2D input1 : register(t1);
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
	
	float4 color0 = input0.SampleLevel(Sampler, uv, 0);
	float4 color1 = input1.SampleLevel(Sampler, uv, 0);

#if Add
	float3 blend = color0.rgb * Intensity0 + color1.rgb * Intensity1;
#elif Min
	float3 blend = color0.rgb * Intensity0 - color1.rgb * Intensity1;
#elif Multiply
	float3 blend = color0.rgb * Intensity0 * color1.rgb * Intensity1;
#elif Pow
	float3 blend = pow(color0.rgb * Intensity0, color1.rgb * Intensity1);
#endif

	Output[threadID.xy] = float4(blend, 1.0);
}

[End_Pass]
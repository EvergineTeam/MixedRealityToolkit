[Begin_ResourceLayout]

[directives:Offsets Square2x2 Cross3x3 Square3x3]

Texture2D input : register(t0);
RWTexture2D<float4> Output : register(u0); [Output(input, 0.5)]

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
	float2 texelSize = 1.0 / outputSize;
	float2 uv = threadID.xy + 0.5;

	float3 color = 0;

#if Square2x2
	color += input.SampleLevel(Sampler, uv * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(1, 0)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(0, 1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(1, 1)) * texelSize, 0).rgb;
	color *= 0.25;
#elif Cross3x3
	color += input.SampleLevel(Sampler, (uv + float2(-1, -1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(1, -1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(-1, 1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(1, 1)) * texelSize, 0).rgb;
	color *= 0.25;
#elif Square3x3
	color += input.SampleLevel(Sampler, (uv + float2(-1, +1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(-1, 0)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(-1, -1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(0, +1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, uv * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(0, -1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(+1, +1)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(+1, 0)) * texelSize, 0).rgb;
	color += input.SampleLevel(Sampler, (uv + float2(+1, -1)) * texelSize, 0).rgb;
	color *= 0.111;
#endif
	
	Output[threadID.xy] = float4(color, 1.0);
}

[End_Pass]
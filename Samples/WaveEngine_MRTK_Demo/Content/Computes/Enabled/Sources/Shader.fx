[Begin_ResourceLayout]
	[directives:Enabled OFF ON]

Texture2D Input0 : register(t0);
Texture2D Input1 : register(t1);
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
	
#if	ON
	Output[threadID.xy] = Input1.SampleLevel(Sampler, uv, 0);
#else
	Output[threadID.xy] = Input0.SampleLevel(Sampler, uv, 0);
#endif
}

[End_Pass]
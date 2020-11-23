[Begin_ResourceLayout]
[directives:DirtLens Dirt_Off Dirt_On]

cbuffer ParamsBuffer : register(b0)
{
	float ColorIntensity : packoffset(c0.x); [Default(1.0)]
	float BlurIntensity : packoffset(c0.y); [Default(0.8)]
	float DirtIntensity : packoffset(c0.z); [Default(0.5)]
}

Texture2D Input : register(t0);
Texture2D Blurred : register(t1);
Texture2D DirtTexture : register(t2);
RWTexture2D<float4> Output : register(u0); [Output(Input)]

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
	
	float3 blur = Blurred.SampleLevel(Sampler, uv, 0).rgb;
	float3 color = Input.SampleLevel(Sampler, uv, 0).rgb;
	
	color = blur * BlurIntensity + color  * ColorIntensity;
	
#if Dirt_On
	float3 dirt = DirtTexture.SampleLevel(Sampler, uv, 0).rgb;
	color += blur * dirt * DirtIntensity;
#endif
	
	Output[threadID.xy] = float4(color, 1.0);
}

[End_Pass]
[Begin_ResourceLayout]
	[directives:Output None_OFF Reflection]

	cbuffer Parameters : register(b0)
	{
		float Intensity : packoffset(c0.x); [Default(0.5)]
	}

	Texture2D Color : register(t0);
	Texture2D ReflectionColor : register(t1);
	Texture2D<float4> ZPrePass : register(t2); [ZPrePass]
	RWTexture2D<float4> Output : register(u0); [Output(Color)]

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

		float4 color = Color.SampleLevel(Sampler, uv,0);
		float4 reflectionColor = ReflectionColor.SampleLevel(Sampler, uv,0);		
		float2 roghnessAndMetallic = ZPrePass.SampleLevel(Sampler, uv,0).zw;		
		
#if Reflection
		Output[threadID.xy] = reflectionColor;
#else
		Output[threadID.xy] = lerp(color, reflectionColor, clamp(roghnessAndMetallic.y, 0.1, 0.9) * Intensity);
#endif

	}

[End_Pass]
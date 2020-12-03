[Begin_ResourceLayout]

cbuffer Parameters : register(b0)
{
	float DecayFactor : packoffset(c0.x); [Default(0.945)]
	float NumSamples : packoffset(c0.y); [Default(9)]
}

Texture2D<float4> input : register(t0);
Texture2D<float2> velocityMap : register(t1);
RWTexture2D<float4> output : register(u0); [Output(input)]

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;

	float2 velocity = velocityMap.SampleLevel(Sampler, uv, 0) / (2.0 * NumSamples);
	velocity.x = -velocity.x;

	float weight = 1.0 / NumSamples;
	float decay = 1.0 * DecayFactor;
	float4 color = input.SampleLevel(Sampler, uv, 0);
	float4 colorext = float4(0, 0, 0, 0);
	float t = 0.0;

	uv += velocity;

	// Accumulate in the color.	
	for (int i = 1; i < NumSamples; ++i)
	{
		float4 vcolor = input.SampleLevel(Sampler, uv, 0);
		float decrement = weight * decay;
		t += decrement;
		colorext += vcolor * decrement;
		uv += velocity;
		decay *= DecayFactor;
	}

	output[threadID.xy] = color * (1.0 - t) + colorext;

}

[End_Pass]
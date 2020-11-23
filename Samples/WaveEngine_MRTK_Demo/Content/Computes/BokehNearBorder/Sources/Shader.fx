[Begin_ResourceLayout]

Texture2D<float4> Input : register(t0);
RWTexture2D<float4> Output : register(u0);
SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

// Makes near color bleed into transparent texel 
// (poor-man way since pre-multiplied alpha doesn't play nice with alpha boost in the main blur pass)
[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	float2 texelSize = 1.0 / outputSize;
	
	float4 PixelColor = Input.SampleLevel(Sampler, uv,  0); 
	
	if (PixelColor.w == 0) // Only fill the empty areas around near field
	{
		PixelColor = 0;
		float Weight = 0;
		int RADIUS_TAPS = 4; // 8x8 taps, but shouldn't be heavy at such low resolution
		for (int u = -RADIUS_TAPS; u <= RADIUS_TAPS; ++u)
		{
			for (int v = -RADIUS_TAPS; v <= RADIUS_TAPS; ++v)
			{
				float4 tapValue = Input.SampleLevel(Sampler, uv + float2(u,v) * texelSize, 0); 
				float tapWeight = tapValue.w == 0.0? 0.0 : 1.0;
				PixelColor += tapWeight * tapValue;
				Weight += tapWeight;
			}
		}
		
		PixelColor /= (Weight + 0.000001);
		PixelColor.w = 0;
	}

	Output[threadID.xy] = PixelColor;
}

[End_Pass]
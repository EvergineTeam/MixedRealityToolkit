[Begin_ResourceLayout]

Texture2D<float> Input : register(t0);
RWTexture2D<float> Output : register(u0); [Output(Input,0.5)]
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
	float2 tapUV = (threadID.xy + 0.5) / outputSize;
	float2 texelSize = 1.0 / outputSize;
	
	float4 CoCs[4];
		
	CoCs[0] = Input.SampleLevel(Sampler, tapUV, 0);
	CoCs[1] = Input.SampleLevel(Sampler, tapUV + float2( 0.5,  0.0) * texelSize, 0);
	CoCs[2] = Input.SampleLevel(Sampler, tapUV + float2( 0.0,  0.5) * texelSize, 0);
	CoCs[3] = Input.SampleLevel(Sampler, tapUV + float2( 0.5,  0.5) * texelSize, 0);

	Output[threadID.xy] = max( CoCs[0], max( CoCs[1], max( CoCs[2], CoCs[3] ) ) );
}

[End_Pass]
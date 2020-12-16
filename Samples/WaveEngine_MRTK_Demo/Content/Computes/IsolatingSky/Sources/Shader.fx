[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float MinThreshold : packoffset(c0.x); [Default(0.1)]
	float MaxThreshold : packoffset(c0.y); [Default(0.4)]
	float Scale : packoffset(c0.z); [Default(0.05)]
	float Intensity : packoffset(c1.x); [Default(20)]		
}

Texture2D<float4> input : register(t0);
Texture2D<float> depth : register(t1);
SamplerState Sampler : register(s0);
RWTexture2D<float4> Output : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

static float3 luma = float3(0.2126, 0.7152, 0.0722); 

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 texelSize = 1.0 / outputSize;
	float2 uv = threadID.xy / outputSize + 0.5 / outputSize;

	float3 color = input.SampleLevel(Sampler, uv, 0).rgb;
	float z = depth.SampleLevel(Sampler, uv, 0);
	
	bool isSky = (z > 0.999);
	color *= isSky;
	
	float brightness = dot( luma, color );  // Luminance	
	brightness = clamp(brightness, MinThreshold, MaxThreshold);
 	float contribution = max( brightness - MinThreshold, 0.0);
 	//contribution *= step(contribution, MaxThreshold - MinThreshold);
 	
 	// Calculate amount of bloom.   
	// f(x) = ( b*(x-a)^2 ) / x  
	// where:  
	//  
	// x - input brightness  
	// f(x) - amount of bloom for given brightness  
	// b = bloom scale  
	// a = bloom threshold
	contribution *= saturate( contribution * Scale );  	
   
   // avoid too small denominator
 	const float bloomAmount = contribution / max(brightness, 0.0001);
 	
	// Apply the bloom amount to color  
 	color *= bloomAmount;  
   
 	// Perform final boost (usually multiplying by 100)  
 	color *= Intensity;
	
	Output[threadID.xy] = float4(color, 1.0);
}

[End_Pass]
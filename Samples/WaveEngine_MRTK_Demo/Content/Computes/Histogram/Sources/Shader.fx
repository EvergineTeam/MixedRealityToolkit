[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float minLogLuminance : packoffset(c0.x); [Default(-10)]
	float oneOverLogLuminanceRange : packoffset(c0.y); [Default(0.0833)]
}

Texture2D Input : register(t0);
RWStructuredBuffer<uint> LuminanceHistogram : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

#define EPSILON 0.005
groupshared uint HistogramShared[256];

inline float GetLuminance(float3 color)
{
    return dot(color, float3(0.2127, 0.7152, 0.0722));
}

inline uint HDRToHistogramBin(float3 hdrColor)
{
    float luminance = GetLuminance(hdrColor);
    
    if(luminance < EPSILON)
    {
        return 0;
    }
    
    float logLuminance = saturate((log2(luminance) - minLogLuminance) * oneOverLogLuminanceRange);
    return (uint)(logLuminance * 254.0 + 1.0);
}

[numthreads(16, 16, 1)]
void CS(uint groupIndex : SV_GroupIndex, uint3 threadID : SV_DispatchThreadID)
{
	float2 inputSize;
	Input.GetDimensions(inputSize.x, inputSize.y);
	
	HistogramShared[groupIndex] = 0;
	
	GroupMemoryBarrierWithGroupSync();
	
	if (threadID.x < inputSize.x && threadID.y < inputSize.y)
	{
		float3 hdrColor = Input[threadID.xy].rgb;
		uint binIndex = HDRToHistogramBin(hdrColor);
		InterlockedAdd(HistogramShared[binIndex], 1);
	}
	
	GroupMemoryBarrierWithGroupSync();
	
	InterlockedAdd(LuminanceHistogram[groupIndex], HistogramShared[groupIndex]);
}

[End_Pass]
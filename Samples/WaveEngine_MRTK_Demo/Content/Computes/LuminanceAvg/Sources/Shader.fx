[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	uint pixelCount : packoffset(c0.x);
	float minLogLuminance : packoffset(c0.y); [Default(-10)]
	float logLuminanceRange : packoffset(c0.z); [Default(12)]
	float timeDelta : packoffset(c0.w); [Default(0.016)]
	float tau : packoffset(c1.x); [Default(1.1)]
}

RWStructuredBuffer<uint> LuminanceHistogram : register(u0);
RWStructuredBuffer<float> LuminanceAvgResult : register(u1);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

groupshared float HistogramShared[256];

[numthreads(256, 1, 1)]
void CS(uint groupIndex : SV_GroupIndex)
{
	// Get the count from the histogram buffer
	float countForThisBin = (float)LuminanceHistogram[groupIndex];
	HistogramShared[groupIndex] = countForThisBin * (float)groupIndex;
	
	GroupMemoryBarrierWithGroupSync();
	
	// histogramSampleIndex>>=1 = histogramSampleIndex /= 2
	// Aggregate all bins in O(log2(N)) instead of O(N)
	// Sum result is stored in HistogramShared[0]
	[unroll]
	for (uint histogramSampleIndex = 128; histogramSampleIndex > 0; histogramSampleIndex>>=1)
	{
		if (groupIndex < histogramSampleIndex)
		{
			HistogramShared[groupIndex] += HistogramShared[groupIndex + histogramSampleIndex];
		}
		
		GroupMemoryBarrierWithGroupSync();
	}
	
	if(groupIndex == 0)
    {
	    // Here we take our weighted sum and divide it by the number of pixels
	    // that had luminance greater than zero (since the index == 0, we can
	    // use countForThisBin to find the number of black pixels)
        float weightedLogAverage = (HistogramShared[0].x / max((float)pixelCount - countForThisBin, 1.0)) - 1.0;
        
        // Map from our histogram space to actual luminance
        float weightedAverageLuminance = exp2(((weightedLogAverage / 254.0) * logLuminanceRange) + minLogLuminance);
        
        // The new stored value will be interpolated using the last frames value
    	// to prevent sudden shifts in the exposure.
        float luminanceLastFrame = LuminanceAvgResult[0];
        float adaptedLuminance = luminanceLastFrame + (weightedAverageLuminance - luminanceLastFrame) * (1 - exp(-timeDelta * tau));
		LuminanceAvgResult[0] = adaptedLuminance;
    }
}

[End_Pass]
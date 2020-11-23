[Begin_ResourceLayout]

[directives:Quality Low Medium High]

cbuffer ParamsBuffer : register(b0)
{
	float2 direction: packoffset(c0); [Default(1.0, 0.0)]
}

Texture2D input : register(t0);
RWTexture2D<float4> Output : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

// http://dev.theomader.com/gaussian-kernel-calculator/
#if Low
// Sigma: 1.34
static const int GAUSS_KERNEL = 9;
static const float gaussianWeightsNormalized[GAUSS_KERNEL] = {
	0.004112,
	0.026563,
	0.100519,
	0.223215,
	0.29118,
	0.223215,
	0.100519,
	0.026563,
	0.004112
};

static const int gaussianOffsets[GAUSS_KERNEL] = {
	-4,
	-3,
	-2,
	-1,
	0,
	1,
	2,
	3,
	4
};

#endif

#if Medium
// Sigma: 3.02
static const int GAUSS_KERNEL = 17;
static const float gaussianWeightsNormalized[GAUSS_KERNEL] = 
{
	0.004084,
	0.009225,
	0.018694,
	0.033981,
	0.055407,
	0.081043,
	0.106336,
	0.125157,
	0.132144,
	0.125157,
	0.106336,
	0.081043,
	0.055407,
	0.033981,
	0.018694,
	0.009225,
	0.004084,
};

static const int gaussianOffsets[GAUSS_KERNEL] = {
	-8,
	-7,
	-6,
	-5,
	-4,
	-3,
	-2,
	-1,
	0,
	1,
	2,
	3,
	4,
	5,
	6,
	7,
	8
};

#endif

#if High
// Sigma 6.9
static const int GAUSS_KERNEL = 33;
static const float gaussianWeightsNormalized[GAUSS_KERNEL] = {
	0.004013,
	0.005554,
	0.007527,
	0.00999,
	0.012984,
	0.016524,
	0.020594,
	0.025133,
	0.030036,
	0.035151,
	0.040283,
	0.045207,
	0.049681,
	0.053463,
	0.056341,
	0.058141,
	0.058754,
	0.058141,
	0.056341,
	0.053463,
	0.049681,
	0.045207,
	0.040283,
	0.035151,
	0.030036,
	0.025133,
	0.020594,
	0.016524,
	0.012984,
	0.00999,
	0.007527,
	0.005554,
	0.004013
};
static const int gaussianOffsets[GAUSS_KERNEL] = {
	-16,
	-15,
	-14,
	-13,
	-12,
	-11,
	-10,
	-9,
	-8,
	-7,
	-6,
	-5,
	-4,
	-3,
	-2,
	-1,
	0,
	1,
	2,
	3,
	4,
	5,
	6,
	7,
	8,
	9,
	10,
	11,
	12,
	13,
	14,
	15,
	16,
};

#endif

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float3 color = 0;
	for(int i = 0; i < GAUSS_KERNEL; i++)
	{
		float2 offset = direction * gaussianOffsets[i];
		uint2 uv = threadID.xy + offset;
		float3 pixel = input[uv];
		color += pixel * gaussianWeightsNormalized[i];
	}

	Output[threadID.xy] = float4(color,1);
}

[End_Pass]
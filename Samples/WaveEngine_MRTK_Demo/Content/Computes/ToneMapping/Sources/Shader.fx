[Begin_ResourceLayout]

[directives:HDR HDR_OFF HDR]
[directives:HDROperator REINHARD REINHARDSQ LUMAREINHARD FILMIC ACES ROMBINDAHOUSE]
[directives:ColorGradient LUT_OFF LUT]
[directives:Dither DITHER_OFF DITHER]
[directives:Chromatic CHRO_OFF CHRO]
[directives:Grain GRAIN_OFF GRAIN]
[directives:Vignette VIG_OFF VIG]
[directives:Distortion DIST_OFF DIST]

cbuffer ParamsBuffer : register(b0)
{
	float AberrationStrength : packoffset(c0.x); [Default(5.0)]
	float2 TexcoordOffset : packoffset(c0.y); [Default(0.005,0.005)]
	float GrainIntensity : packoffset(c0.w); [Default(0.5)]
	float VignettePower : packoffset(c1.x); [Default(1.0)]
	float VignetteRadio : packoffset(c1.y); [Default(1.25)]
}

cbuffer PerFrameBuffer : register(b1)
{
	float Time : packoffset(c0.x); [Time]
}

Texture2D input : register(t0);
Texture2D lookuptable	: register(t1);
Texture2D distortion : register(t2); [DistortionPass]
RWTexture2D<float4> Output : register(u0);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

float3 PDnrand3( float2 n ) {
	return frac( sin(dot(n.xy, float2(12.9898, 78.233)))* float3(43758.5453, 28001.8384, 50849.4141 ) );
}

float3 PDsrand3( float2 n ) {
	return PDnrand3( n ) * 2 - 1;
}

static const float BayerMatrix8[8][8] =
{
	{ 1.0 / 65.0, 49.0 / 65.0, 13.0 / 65.0, 61.0 / 65.0, 4.0 / 65.0, 52.0 / 65.0, 16.0 / 65.0, 64.0 / 65.0 },
	{ 33.0 / 65.0, 17.0 / 65.0, 45.0 / 65.0, 29.0 / 65.0, 36.0 / 65.0, 20.0 / 65.0, 48.0 / 65.0, 32.0 / 65.0 },
	{ 9.0 / 65.0, 57.0 / 65.0, 5.0 / 65.0, 53.0 / 65.0, 12.0 / 65.0, 60.0 / 65.0, 8.0 / 65.0, 56.0 / 65.0 },
	{ 41.0 / 65.0, 25.0 / 65.0, 37.0 / 65.0, 21.0 / 65.0, 44.0 / 65.0, 28.0 / 65.0, 40.0 / 65.0, 24.0 / 65.0 },
	{ 3.0 / 65.0, 51.0 / 65.0, 15.0 / 65.0, 63.0 / 65.0, 2.0 / 65.0, 50.0 / 65.0, 14.0 / 65.0, 62.0 / 65.0 },
	{ 35.0 / 65.0, 19.0 / 65.0, 47.0 / 65.0, 31.0 / 65.0, 34.0 / 65.0, 18.0 / 65.0, 46.0 / 65.0, 30.0 / 65.0 },
	{ 11.0 / 65.0, 59.0 / 65.0, 7.0 / 65.0, 55.0 / 65.0, 10.0 / 65.0, 58.0 / 65.0, 6.0 / 65.0, 54.0 / 65.0 },
	{ 43.0 / 65.0, 27.0 / 65.0, 39.0 / 65.0, 23.0 / 65.0, 42.0 / 65.0, 26.0 / 65.0, 38.0 / 65.0, 22.0 / 65.0 }
};

const static float LUTSize = 16;
const static float3 Luma = float3(0.2126, 0.7152, 0.0722);

inline float DitherMask(in float2 pixel)
{
	return BayerMatrix8[pixel.x % 8][pixel.y % 8];
}

inline float3 Filmic(float3 x)
{
	// Hable 2010, "Filmic Tonemapping Operators"
    // Based on Duiker's curve, optimized by Hejl and Burgess-Dawson
    // Gamma 2.2 correction is baked in, don't use with sRGB conversion!
    float3 c = max(0.0, x - 0.004);
    return (c * (c * 6.2 + 0.5)) / (c * (c * 6.2 + 1.7) + 0.06);
}

inline float3 Reinhard(float3 x, float k = 1.0)
{
	// Reinhard et al. 2002, "Photographic Tone Reproduction for Digital Images", Eq. 3
	return x / (x + k);
}

inline float3 ReinhardSq(float3 x, float k = 0.25)
{
	// Reinhard et al. 2002, "Photographic Tone Reproduction for Digital Images", Eq. 3
	float3 value = x / (x + k);
	return value * value;
}

inline float3 LumaReinhard(float3 x)
{
	// Reinhard et al. 2002, "Photographic Tone Reproduction for Digital Images", Eq. 3
	return x / (dot(x, Luma) + 1.0);
}

inline float3 RomBinDaHouse(float3 x)
{
	return exp(-1.0 / (2.75 * x + 0.15));
}

// sRGB => XYZ => D65_2_D60 => AP1 => RRT_SAT
static const float3x3 ACESInputMat =
{
    {0.59719, 0.35458, 0.04823},
    {0.07600, 0.90834, 0.01566},
    {0.02840, 0.13383, 0.83777}
};

// ODT_SAT => XYZ => D60_2_D65 => sRGB
static const float3x3 ACESOutputMat =
{
    { 1.60475, -0.53108, -0.07367},
    {-0.10208,  1.10813, -0.00605},
    {-0.00327, -0.07276,  1.07602}
};

float3 RRTAndODTFit(float3 v)
{
    float3 a = v * (v + 0.0245786f) - 0.000090537f;
    float3 b = v * (0.983729f * v + 0.4329510f) + 0.238081f;
    return a / b;
}

// From: https://github.com/TheRealMJP/BakingLab/blob/master/BakingLab/ACES.hlsl
float3 ACESFitted(float3 color)
{
    color = mul(ACESInputMat, color);

    // Apply RRT and ODT
    color = RRTAndODTFit(color);

    color = mul(ACESOutputMat, color);

    // Clamp to [0, 1]
    color = saturate(color);

    return color;
}

inline float3 Tonemap(float3 x)
{
#if REINHARD
	return Reinhard(x);
#elif REINHARDSQ
	return ReinhardSq(x);
#elif LUMAREINHARD
	return LumaReinhard(x);
#elif FILMIC
	return Filmic(x);
#elif ACES
	return ACESFitted(x);
#elif ROMBINDAHOUSE
	return RomBinDaHouse(x);
#endif
}

half3 UnwrappedTexture3DSample(Texture2D Texture, SamplerState Sampler, float3 UVW, float Size)
{
	float IntW = floor( UVW.z * Size - 0.5 );
	half FracW = UVW.z * Size - 0.5 - IntW;

	float U = ( UVW.x + IntW ) / Size;
	float V = UVW.y;

	half3 RG0 = Texture.SampleLevel(Sampler, float2(U, V), 0).rgb;
	half3 RG1 = Texture.SampleLevel(Sampler, float2(U + 1.0f / Size, V), 0).rgb;

	return lerp(RG0, RG1, FracW);
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	
	float2 coords = uv;
#if DIST
	float2 distortionOffset = distortion.SampleLevel(Sampler, uv, 0) .rg;
	coords += distortionOffset;
#endif

#if CHRO
	float2 offset = (coords - 0.5) * 2.0;
	float coordDot = dot(offset, offset); 
	float2 compute = TexcoordOffset.xy * AberrationStrength * coordDot * offset;
	float2 uvR = coords - compute;
	float2 uvB = coords + compute;

	float r = input.SampleLevel(Sampler, uvR, 0).r;
	float g = input.SampleLevel(Sampler, coords, 0).g;
	float b = input.SampleLevel(Sampler, uvB, 0).b;	
	float3 chromatic = float3(r, g, b);
#endif

#if HDR
	#if CHRO
		float3 hdr = chromatic;
	#else
		float3 hdr = input.SampleLevel(Sampler, coords, 0).rgb;
	#endif
	
	float3 ldr = saturate(Tonemap(hdr.rgb));
#else
	#if CHRO
		float3 ldr = chromatic;
	#else
		float3 ldr = input.SampleLevel(Sampler, coords, 0).rgb;
	#endif
#endif

#if LUT
	float3 UVW = ldr * ((LUTSize - 1) / LUTSize) + (0.5 / LUTSize);
	ldr = UnwrappedTexture3DSample(lookuptable, Sampler, UVW, LUTSize);
#endif

#if DITHER
	ldr += (DitherMask((float2)threadID.xy) - 0.5f) / 64.0f;
#endif

#if GRAIN
	float3 noise3 = PDsrand3(uv + sin(Time) + 0.6959174) * GrainIntensity * 0.1;
	ldr += noise3;
#endif

#if VIG
	float2 dist = (uv - 0.5) * VignetteRadio;
	dist.x = 1 - dot(dist, dist) * VignettePower;
	ldr *= dist.x;
#endif

	Output[threadID.xy] = float4(ldr, 1.0);
}

[End_Pass]
[Begin_ResourceLayout]

[directives:Multiview MULTIVIEW_OFF MULTIVIEW]
[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

cbuffer PerObject : register(b0)
{
	float4x4 World			: packoffset(c0); [World]
};

cbuffer PerCamera : register(b1)
{
	float4	  CameraPosition			: packoffset(c0.x); [CameraPosition]
	float4x4  ViewProj					: packoffset(c1.x); [ViewProjection]
	float     Exposure					: packoffset(c5.x); [CameraExposure]
	int       EyeCount					: packoffset(c5.y); [MultiviewCount]
	float     IblLuminance				: packoffset(c5.z); [IBLLuminance]
	float4x4  MultiviewViewProj[6]		: packoffset(c6.x); [MultiviewViewProjection]	
	float4    MultiviewPosition[6]		: packoffset(c30.x); [MultiviewPosition]	
};

cbuffer Parameters : register(b2)
{
	float Intensity : packoffset(c0.x); [Default(1)]
};


Texture2D Texture				: register(t0);
SamplerState TextureSampler		: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

float4 LinearToGamma(const float4 color)
{
	return float4(pow(color.rgb, 1/2.2), color.a);
}

struct VSInputPbr
{
	float3      Position            : POSITION;
	float2      TexCoord0           : TEXCOORD0;
	uint        InstId              : SV_InstanceID;
};

struct VSOutputPbr
{
	float4 Position		: SV_POSITION;
	float2 TexCoord0    : TEXCOORD0;
#if MULTIVIEW	
	uint ViewId         : SV_RenderTargetArrayIndex;
#endif	
};

VSOutputPbr VertexFunction(VSInputPbr input)
{
	VSOutputPbr output = (VSOutputPbr)0;
	
#if MULTIVIEW
	const int vid = input.InstId % EyeCount;
	const float4x4 viewProj = MultiviewViewProj[vid];	
	const float4 cameraPosition = MultiviewPosition[vid];

	// Note which view this vertex has been sent to. Used for matrix lookup.
	// Taking the modulo of the instance ID allows geometry instancing to be used
	// along with stereo instanced drawing; in that case, two copies of each 
	// instance would be drawn, one for left and one for right.
	
	output.ViewId = vid;
#else
	float4x4 viewProj = ViewProj;	
	float4 cameraPosition = CameraPosition;
#endif

	float4x4 world = World;
	world._m30_m31_m32 = cameraPosition;
	float4x4 worldViewProj = mul(world, viewProj);

	output.Position = mul(float4(input.Position, 1), worldViewProj);
	output.TexCoord0 = input.TexCoord0;

	return output;
}

float4 PixelFunction(VSOutputPbr input) : SV_Target
{
	float4 color = Texture.Sample(TextureSampler, input.TexCoord0);
	
	color.rgb *= Exposure * Intensity * IblLuminance;

#if GAMMA_COLORSPACE
	color = LinearToGamma(color);
#endif

	return color;
}
[End_Pass]
[Begin_ResourceLayout]

[directives:Diffuse DIFF_OFF DIFF]
[directives:Align ALIGN_OFF ALIGN]
[directives:Multiview MULTIVIEW_OFF MULTIVIEW]
[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

cbuffer PerDrawCall : register(b0)
{
	float4x4 	World					: packoffset(c0.x); [World]
	float4x4	WorldViewProjection		: packoffset(c4.x); [WorldViewProjection]
	float4x4 	WorldInverse			: packoffset(c8.x); [WorldInverse]
};

cbuffer PerCamera : register(b1)
{
	float3		CameraPosition			: packoffset(c0.x); [CameraPosition]
	int			EyeCount				: packoffset(c0.w); [MultiviewCount]
	float4x4	ViewProj[6]				: packoffset(c1.x); [MultiviewViewProjection]
	float4		StereoCameraPosition[6]	: packoffset(c25.x); [MultiviewPosition]
};

cbuffer Parameters : register(b2)
{
	float2		TextureOffset			: packoffset(c0.x); [Default(0.0, 0.0)]
	float2		TextureTiling			: packoffset(c0.z); [Default(1.0, 1.0)]
};

Texture2D DiffuseTexture	: register(t0);
SamplerState DiffuseSampler	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

float4 GammaToLinear(const float4 color)
{
	return float4(pow(color.rgb, 2.2), color.a);
}

struct VS_IN
{
	float4 Position 	: POSITION;
	uint   InstanceID	: SV_InstanceID;
	float4 Color		: COLOR;
	float2 TexCoord		: TEXCOORD0;
	float4 AxisSize		: TEXCOORD1;
};

struct PS_IN
{
	float4 Position	: SV_POSITION;
	float4 Color	: COLOR;
	float2 TexCoord	: TEXCOORD0;

#if MULTIVIEW
	uint ViewId		: SV_RenderTargetArrayIndex;
#endif
};
PS_IN VertexFunction(VS_IN input)
{
	PS_IN output = (PS_IN)0;
	
	float3 position = input.Position.xyz;

#if MULTIVIEW
	int vid = input.InstanceID % EyeCount;

	float3 cameraPositionWS = StereoCameraPosition[vid].xyz;
	float4x4 worldViewProj = mul(World, ViewProj[vid]);
	output.ViewId = vid;
#else
	float3 cameraPositionWS = CameraPosition;
	float4x4 worldViewProj = WorldViewProjection;
#endif

#if ALIGN
	
	float3 cameraPositionOS = mul(float4(cameraPositionWS, 1), WorldInverse).xyz;

	float4 axisSize = input.AxisSize;
	float3 forwardVector = cameraPositionOS - position;
	float3 directionVector = normalize(axisSize.xyz);
	float3 rightVector = normalize(cross(directionVector, forwardVector));

	float size = axisSize.w;
	position += (rightVector * size);
#endif
	
	output.Position = mul(float4(position, 1.0), worldViewProj);
	output.Color = GammaToLinear(input.Color);

#if DIFF
	output.TexCoord = (input.TexCoord * TextureTiling) + TextureOffset;
#endif
	return output;
}

float4 PixelFunction(PS_IN input) : SV_Target
{
	float4 baseColor = input.Color;

#if DIFF
	float4 diffuseColor = DiffuseTexture.Sample(DiffuseSampler, input.TexCoord);
	
#if !GAMMA_COLORSPACE
	diffuseColor = GammaToLinear(diffuseColor);
#endif

	baseColor *= diffuseColor;
#endif


	return baseColor;
}
[End_Pass]

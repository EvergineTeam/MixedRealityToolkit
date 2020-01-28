[Begin_ResourceLayout]

[directives:Diffuse DIFF DIFF_OFF]
[directives:Align ALIGN ALIGN_OFF]

cbuffer PerDrawCall : register(b0)
{
	float4x4 	World					: packoffset(c0.x); [World]
	float4x4 	WorldInverse			: packoffset(c4.x); [WorldInverse]
};

cbuffer PerCamera : register(b1)
{
	float4x4	ViewProj[2]			: packoffset(c0.x); [StereoCameraViewProjection]
	float4		CameraPosition[2]	: packoffset(c8.x); [StereoCameraPosition]
	int			EyeCount			: packoffset(c10.x); [StereoEyeCount]
};

cbuffer Parameters : register(b2)
{
	float2 TextureOffset : packoffset(c0.x); [Default(0.0, 0.0)]
	float2 TextureTiling : packoffset(c0.z); [Default(1.0, 1.0)]
};

Texture2D DiffuseTexture	: register(t0);
SamplerState DiffuseSampler	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

struct VS_IN
{
	float4 Position 	: POSITION;
	uint   InstanceID	: SV_InstanceID;		
	float4 Color		: COLOR;
	float2 TexCoord : TEXCOORD0;
	float4 AxisSize : TEXCOORD1;
};

struct PS_IN
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR;
	float2 TexCoord : TEXCOORD0;
	uint ArrayIndex : SV_RenderTargetArrayIndex;
};


PS_IN VertexFunction(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	int iid = input.InstanceID / EyeCount;
	int vid = input.InstanceID % EyeCount;	

	float4x4 worldViewProj = mul(World, ViewProj[vid]);
	
	float3 position = input.Position.xyz;
	
#if ALIGN
	float3 cameraPositionWS = CameraPosition[vid].xyz;
	float3 cameraPositionOS = mul(float4(cameraPositionWS, 1), WorldInverse).xyz;

	float4 axisSize = input.AxisSize;
	float3 forwardVector = cameraPositionOS - position;
	float3 directionVector = normalize(axisSize.xyz);
	float3 rightVector = normalize(cross(directionVector, forwardVector));

	float size = axisSize.w;
	position += (rightVector * size);
#endif
	
	output.Position = mul(float4(position, 1.0), worldViewProj);
	output.Color = input.Color;
	output.ArrayIndex = vid;	
#if DIFF
	output.TexCoord = (input.TexCoord * TextureTiling) + TextureOffset;
#endif
	return output;
}

float4 PixelFunction(PS_IN input) : SV_Target
{
	float4 baseColor = input.Color;	
#if DIFF
	baseColor *= DiffuseTexture.Sample(DiffuseSampler, input.TexCoord);
#endif
	return baseColor;
}
[End_Pass]
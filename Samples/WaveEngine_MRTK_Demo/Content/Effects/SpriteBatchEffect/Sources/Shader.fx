[Begin_ResourceLayout]

[directives TEXTURE2D TEXTURE2DARRAY TEXTURE3D]
[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

cbuffer CameraData : register(b0)
{
	float4x4 ViewProj	: packoffset(c0); [ViewProjection]
};

Texture2D Texture				: register(t0);
SamplerState Sampler			: register(s0);
Texture2DArray Texture2Array	: register(t1);
Texture3D Texture3				: register(t2);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VS PS = PS]

float4 GammaToLinear(const float4 color)
{
	return float4(pow(color.rgb, 2.2), color.a);
}

struct VS_IN_COLOR
{
	float2 Corners			: POSITION;

	// Instanced Sprite info
	float4 Color					: COLOR;
	float4 SourceRectangle			: TEXCOORD0;
	float4 OriginAndSpriteEffect	: TEXCOORD1;
	float4 SpriteMatrix0			: TEXCOORD2;
	float4 SpriteMatrix1			: TEXCOORD3;
	float4 SpriteMatrix2			: TEXCOORD4;
	float4 SpriteMatrix3			: TEXCOORD5;
	float3 TextureSizeAndSliceIndex	: TEXCOORD6;
};

struct VS_OUT_COLOR
{
	float4 Position			: SV_POSITION;
	float2 TexCoord			: TEXCOORD0;
	float4 Color 			: COLOR;
#if TEXTURE2DARRAY || TEXTURE3D
	float SliceIndex : TEXCOORD1;
#endif
};

VS_OUT_COLOR VS(VS_IN_COLOR input)
{
	VS_OUT_COLOR output = (VS_OUT_COLOR)0;

	float4 spriteMatrix0 = input.SpriteMatrix0;
	float4 spriteMatrix1 = input.SpriteMatrix1;
	float4 spriteMatrix2 = input.SpriteMatrix2;
	float4 spriteMatrix3 = input.SpriteMatrix3;

	float2 textureSize = input.TextureSizeAndSliceIndex.xy;
	float2 origin = input.OriginAndSpriteEffect.xy;
	float2 spriteEffect = input.OriginAndSpriteEffect.zw;

	float4x4 world = float4x4(spriteMatrix0,
		spriteMatrix1,
		spriteMatrix2,
		spriteMatrix3);

	float4x4 worldViewProj = mul(world, ViewProj);

	float2 spriteSize = input.SourceRectangle.zw * textureSize;
	float4 position = float4(spriteSize * (input.Corners-origin), 0, 1);
	float2 texCoord = spriteEffect * input.Corners + 0.5-spriteEffect * 0.5;
	texCoord = input.SourceRectangle.xy + (input.SourceRectangle.zw * texCoord);

	output.Position = mul(position, worldViewProj);
	output.TexCoord = texCoord;
	output.Color = input.Color;
#if TEXTURE2DARRAY || TEXTURE3D	    
	float sliceIndex = input.TextureSizeAndSliceIndex.z;
	output.SliceIndex = sliceIndex;
#endif

	return output;
}

float4 PS(VS_OUT_COLOR input) : SV_Target0
{
#if TEXTURE3D
	float4 color = Texture3.Sample(Sampler, float3(input.TexCoord, input.SliceIndex)) * input.Color;
#elif TEXTURE2DARRAY
	float4 color = Texture2Array.Sample(Sampler, float3(input.TexCoord, input.SliceIndex)) * input.Color;
#else
	float4 color = Texture.Sample(Sampler, input.TexCoord) * input.Color;
#endif
	
#if !GAMMA_COLORSPACE
	color = GammaToLinear(color);
#endif

	return color;

}

[End_Pass]

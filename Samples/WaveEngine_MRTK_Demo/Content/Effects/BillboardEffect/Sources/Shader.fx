[Begin_ResourceLayout]

[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

	cbuffer CameraData : register(b0)
	{
		float4x4 ViewProjection	 : packoffset(c0); [UnjitteredViewProjection]
	};

	Texture2D Texture		: register(t0);
	SamplerState Sampler	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 10_0]
	[entrypoints VS=VS PS=PS]

	float4 GammaToLinear(const float4 color)
	{
		return float4(pow(color.rgb, 2.2), color.a);
	}

	struct VS_IN_COLOR
	{
	    float4 Corners				: POSITION;
		
		// Instanced Sprite info
		float4 Color				: COLOR;
		float2 Origin				: TEXCOORD0;
		float2 FlipMode				: TEXCOORD1;

		float3 Position				: TEXCOORD2;
		float2 Scale				: TEXCOORD3;

		float3 UpVector				: TEXCOORD4;
		float3 RightVector			: TEXCOORD5;
	};

	struct VS_OUT_COLOR
	{
	    float4 Position			: SV_POSITION;
		float2 TexCoord			: TEXCOORD0;
	    float4 Color 			: COLOR;
	};

	VS_OUT_COLOR VS( VS_IN_COLOR input )
	{
	    VS_OUT_COLOR output = (VS_OUT_COLOR)0;

		float2 position = float2(input.Scale * (input.Corners.xy - input.Origin));
		float2 texCoord = input.FlipMode * input.Corners.zw + 0.5 - input.FlipMode * 0.5;
		
		float4 finalPosition = float4(input.Position + (input.RightVector * position.x) + (input.UpVector * position.y), 1);
		output.Position = mul(finalPosition, ViewProjection);
		output.TexCoord = texCoord;
	    output.Color = input.Color;	

	    return output;
	}

	float4 PS( VS_OUT_COLOR input ) : SV_Target0
	{
		float4 color = Texture.Sample(Sampler, input.TexCoord) * input.Color;

#if !GAMMA_COLORSPACE
		color = GammaToLinear(color);
#endif

		return color;
	}

[End_Pass]

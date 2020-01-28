[Begin_ResourceLayout]

	cbuffer Matrices : register(b0)
	{
	    float4x4	World : packoffset(c0); [World]
	};

	cbuffer PerCamera : register(b1)
	{
		float4x4	ViewProj[2]	: packoffset(c0); [StereoCameraViewProjection]
		int			EyeCount	: packoffset(c8); [StereoEyeCount]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 10_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN_COLOR
	{
	    float4 Position : POSITION;
	    float4 Color	: COLOR;
		uint   InstanceID	: SV_InstanceID;
	};

	struct VS_OUT_COLOR
	{
	    float4 Position : SV_POSITION;
	    float4 Color 	: COLOR;
		uint ArrayIndex : SV_RenderTargetArrayIndex;
	};

	VS_OUT_COLOR VS( VS_IN_COLOR input )
	{
	    VS_OUT_COLOR output = (VS_OUT_COLOR)0;

		int vid = input.InstanceID % EyeCount;

		float4x4 WorldViewProj = mul(World, ViewProj[vid]);

	    output.Position = mul(input.Position, WorldViewProj);
	    output.Color = input.Color;
		output.ArrayIndex = vid;

	    return output;
	}

	float4 PS( VS_OUT_COLOR input ) : SV_Target0
	{
	    return input.Color;
	}

[End_Pass]

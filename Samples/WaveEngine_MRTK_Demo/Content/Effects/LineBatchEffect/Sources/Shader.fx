[Begin_ResourceLayout]

[directives:Multiview MULTIVIEW_OFF MULTIVIEW]

	cbuffer Matrices : register(b0)
	{
	    float4x4	World					: packoffset(c0); [World]
		float4x4    WorldViewProjection		: packoffset(c4.x); [UnjitteredWorldViewProjection]
	};

	cbuffer PerCamera : register(b1)
	{
		float4x4	ViewProj[6]	: packoffset(c0); [MultiviewViewProjection]
		int			EyeCount	: packoffset(c24); [MultiviewCount]
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

#if MULTIVIEW
		uint ViewId     : SV_RenderTargetArrayIndex;
#endif
	};

	VS_OUT_COLOR VS( VS_IN_COLOR input )
	{
	    VS_OUT_COLOR output = (VS_OUT_COLOR)0;

#if MULTIVIEW
		int vid = input.InstanceID % EyeCount;
		float4x4 viewProjecton = ViewProj[vid];
		float4x4 worldViewProjection = mul(World, viewProjecton);
		output.ViewId = vid;
#else
		float4x4 worldViewProjection = WorldViewProjection;
#endif

	    output.Position = mul(input.Position, worldViewProjection);
	    output.Color = input.Color;

	    return output;
	}

	float4 PS( VS_OUT_COLOR input ) : SV_Target0
	{
	    return input.Color;
	}

[End_Pass]

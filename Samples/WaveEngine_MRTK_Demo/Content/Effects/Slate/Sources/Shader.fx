[Begin_ResourceLayout]

[directives:Multiview MULTIVIEW_OFF MULTIVIEW]
	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Parameters : register(b1)
	{
		float2 Tiling           : packoffset(c0.x);   [Default(1.0, 1.0)]
		float2 Offset           : packoffset(c0.z);   [Default(0.0, 0.0)]
	};
	
	cbuffer PerCamera : register(b2)
	{
		float4x4  MultiviewViewProj[2]		: packoffset(c0.x);  [StereoCameraViewProjection]
		int       EyeCount                  : packoffset(c10.x); [StereoEyeCount]
	};
	
	Texture2D Texture		: register(t0);
	SamplerState Sampler	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[profile 10_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float3 Normal	: NORMAL;
		float2 TexCoord : TEXCOORD;
		
#if MULTIVIEW
		uint InstId : SV_InstanceID;
#endif
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float2 Tex : TEXCOORD;
		
#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;


#if MULTIVIEW
		const int vid = input.InstId % EyeCount;
		const float4x4 viewProj = MultiviewViewProj[vid];
	
		float4x4 worldViewProj = mul(World, viewProj);
		
		// Note which view this vertex has been sent to. Used for matrix lookup.
		// Taking the modulo of the instance ID allows geometry instancing to be used
		// along with stereo instanced drawing; in that case, two copies of each 
		// instance would be drawn, one for left and one for right.
	
		output.ViewId = vid;
#else
		float4x4 worldViewProj = WorldViewProj;
#endif

		output.pos = mul(input.Position, worldViewProj);
		output.Tex = input.TexCoord;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		return Texture.Sample(Sampler, (input.Tex * Tiling) + Offset);
	}

[End_Pass]
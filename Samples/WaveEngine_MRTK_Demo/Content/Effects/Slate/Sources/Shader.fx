[Begin_ResourceLayout]

	cbuffer Base : register(b0)
	{
		float4x4 WorldViewProj	: packoffset(c0);	[WorldViewProjection]
	};

	cbuffer Matrices : register(b1)
	{
		float2 Tiling           : packoffset(c0.x);   [Default(1.0, 1.0)]
		float2 Offset           : packoffset(c0.z);   [Default(0.0, 0.0)]
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
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float3 Nor	: NORMAL;
		float2 Tex : TEXCOORD;
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		output.pos = mul(input.Position, WorldViewProj);
		output.Nor = input.Normal;
		output.Tex = input.TexCoord;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		return Texture.Sample(Sampler, (input.Tex * Tiling) + Offset);
		//return float4(Color,1);
	}

[End_Pass]
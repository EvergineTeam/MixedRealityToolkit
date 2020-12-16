[Begin_ResourceLayout]

	cbuffer PerFrame : register(b0)
	{		
		float4x4 WorldViewProj : packoffset(c0); [UnjitteredWorldViewProjection]		
	};
	
	cbuffer Params : register(b1)
	{		
		float Intensity : packoffset(c0.x); [Default(1.0)]		
	};
	
	Texture2D DistortionTexture : register(t0);
	SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Distortion]
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
		return (float4(DistortionTexture.Sample(Sampler, input.Tex).rg,0,1) * 2 -1 ) * Intensity;		
	}

[End_Pass]


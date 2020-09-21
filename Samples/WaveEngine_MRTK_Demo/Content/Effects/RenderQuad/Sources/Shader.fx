[Begin_ResourceLayout]

Texture2D DiffuseTexture 			: register(t0);
SamplerState Sampler			 	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[profile 10_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		uint id : SV_VertexID;
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float2 tex : TEXCOORD;
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN vertex[3] =
		{
			{ -1.0f, -1.0f, 0.0f, 1.0f }, { 0.0f,  1.0f },
			{ -1.0f,  3.0f, 0.0f, 1.0f }, { 0.0f, -1.0f },
			{  3.0f, -1.0f, 0.0f, 1.0f }, { 2.0f,  1.0f }
		};

		return vertex[input.id % 3];
	}

	float4 PS(PS_IN input) : SV_Target
	{
		float4 color = DiffuseTexture.Sample(Sampler, input.tex);
		return color;
	}

[End_Pass]
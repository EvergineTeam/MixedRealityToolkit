[Begin_ResourceLayout]

[directives:Multiview MULTIVIEW_OFF MULTIVIEW]

Texture2D DiffuseTexture 			: register(t0);
Texture2DArray DiffuseTextureArray	: register(t1);
SamplerState Sampler			 	: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]
	[profile 10_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		uint id			: SV_VertexID;
#if MULTIVIEW
		uint InstId		: SV_InstanceID;
#endif
	};
	
	struct Vertex{
		float4 pos;
		float2 tex;
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float2 tex : TEXCOORD;
		
#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif
	};

	PS_IN VS(VS_IN input)
	{
		Vertex vertices[3] =
		{
			{ -1.0f, -1.0f, 0.0f, 1.0f }, { 0.0f,  1.0f },
			{ -1.0f,  3.0f, 0.0f, 1.0f }, { 0.0f, -1.0f },
			{  3.0f, -1.0f, 0.0f, 1.0f }, { 2.0f,  1.0f }
		};


		PS_IN output = (PS_IN)0;

		Vertex vertex = vertices[input.id % 3];
		
		output.pos = vertex.pos;
		output.tex = vertex.tex;
		
#if MULTIVIEW
		output.ViewId = input.InstId;
#endif

		return output;
	}

	inline float4 LinearToGamma(const float4 color)
	{
		return float4(pow(abs(color.rgb), 1.0 / 2.2), color.a);
	}

	float4 PS(PS_IN input) : SV_Target
	{
	#if MULTIVIEW
		float4 color = DiffuseTextureArray.Sample(Sampler, float3(input.tex, input.ViewId));
	#else
		float4 color = DiffuseTexture.Sample(Sampler, input.tex);
	#endif
		
		return LinearToGamma(color);
	}

[End_Pass]
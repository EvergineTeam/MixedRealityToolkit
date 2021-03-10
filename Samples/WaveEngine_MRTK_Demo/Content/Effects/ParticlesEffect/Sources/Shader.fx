[Begin_ResourceLayout]

	[Directives:SBuffer SBUFFER_OFF SBUFFER]
	[Directives:Diffuse DIFF_OFF DIFF]
	[Directives:Angle ANGLE_OFF ANGLE]
	[Directives:Space LOCAL_OFF LOCAL]
	[Directives:PreserveHighlights HIGHLIGHTS_OFF HIGHLIGHTS]
	[Directives:Multiview MULTIVIEW_OFF MULTIVIEW]

	[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

	struct ParticleState
	{
		float3 Position; 	// 0		
		float Angle;		// 12		
		float4 Tint;		// 16
		float3 Velocity;	// 32	
		float Size;			// 44
	};

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 	WorldViewProj			: packoffset(c0.x); [WorldViewProjection]
		float4x4 	WorldInverse			: packoffset(c4.x); [WorldInverse]
		float4x4 	World					: packoffset(c8.x); [World]
	};

	cbuffer PerCamera : register(b1)
	{
		float3 	 	CameraUp				: packoffset(c0.x); [CameraUp]
		float 		Padding1 				: packoffset(c0.w);
		float3 		CameraRight				: packoffset(c1.x); [CameraRight]
		int 		EyeCount 				: packoffset(c1.w); [MultiviewCount]
		float4    	MultiviewEyePosition[6]	: packoffset(c2.x); [MultiviewPosition]
		float4x4  	MultiviewViewProj[6]	: packoffset(c8.x); [MultiviewViewProjection]	
	}

	Texture2D ParticleTexture 			: register(t0);
	SamplerState Sampler			 	: register(s0);

	StructuredBuffer<ParticleState> particleBuffer : register(t1);

[End_ResourceLayout]

[Begin_Pass:Default]

	[Profile 10_0]
	[Entrypoints VS = VS PS = PS]


	#if SBUFFER
	static float2 texCoords[4] =
	{
		float2(0,0),
		float2(1,0),
		float2(1,1),
		float2(0,1)
	};

	static float2 quadDisp[4] =
	{
		float2(-0.5, -0.5),
		float2(0.5, -0.5),
		float2(0.5,  0.5),
		float2(-0.5,  0.5)
	};
	#endif

	float4 GammaToLinear(const float4 color)
	{
		return float4(pow(color.rgb, 2.2), color.a);
	}

	struct VS_IN
	{
	#if SBUFFER
		uint   InstanceID    	: SV_VertexID;
	#else
		float3 Position			: POSITION;
		float4 Tint     		: COLOR;
		float  Size : TEXCOORD0;
		float  Angle : TEXCOORD1;
		float2 TexCoord			: TEXCOORD2;
	#endif
	#if MULTIVIEW
		uint   InstId           : SV_InstanceID;
	#endif
	};

	struct PS_IN
	{
		float4 pos : SV_POSITION;
		float4 tint: COLOR0;
	#if DIFF	
		float2 tex : TEXCOORD;
	#endif

	#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
	#endif
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

	#if SBUFFER

		uint particleIndex = input.InstanceID / 4;
		uint vertexInQuad = input.InstanceID % 4;

		ParticleState particle = particleBuffer[particleIndex];
		float size = particle.Size;
		float sinRotation = sin(particle.Angle);
		float cosRotation = cos(particle.Angle);
		float4 tint = particle.Tint;
		float3 center = particle.Position.xyz;

		float2 quad = quadDisp[vertexInQuad];

	#if DIFF
		float2 uv = texCoords[vertexInQuad];
	#endif
	#else
		float angle = input.Angle;
		float size = input.Size;
		float sinRotation = sin(angle);
		float cosRotation = cos(angle);
		float4 tint = input.Tint;
		float3 center = input.Position.xyz;

		float2 uv = input.TexCoord;
		float2 quad = uv - float2(0.5, 0.5);

	#endif

		float3 halfRight = size * CameraRight;
		float3 halfUp = size * CameraUp;
		
	#if LOCAL
		halfRight = mul(halfRight, (float3x3)WorldInverse);
		halfUp = mul(halfUp, (float3x3)WorldInverse);
	#endif

		float3 transformHalfRight = (halfRight * cosRotation) + (halfUp * sinRotation);
		float3 transformHalfUp = (halfRight * sinRotation) - (halfUp * cosRotation);

		float3 position = center + (transformHalfUp * quad.y) - (transformHalfRight * quad.x);

	#if DIFF
		output.tex = uv;
	#endif
		output.tint = tint;
		
	#if MULTIVIEW
		const int vid = input.InstId % EyeCount;
		const float4x4 viewProj = MultiviewViewProj[vid];

		// Note which view this vertex has been sent to. Used for matrix lookup.
		// Taking the modulo of the instance ID allows geometry instancing to be used
		// along with stereo instanced drawing; in that case, two copies of each 
		// instance would be drawn, one for left and one for right.

		output.ViewId = vid;

		float4x4 worldViewProj = mul(World, viewProj);
	#else
		float4x4 worldViewProj = WorldViewProj;
	#endif	

		output.pos = mul(float4(position, 1), worldViewProj);

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		float4 diffuse;
		#if DIFF
			diffuse = ParticleTexture.Sample(Sampler, input.tex);
			
		#if !GAMMA_COLORSPACE
			diffuse = GammaToLinear(diffuse);	
		#endif
		
		#if HIGHLIGHTS
			diffuse.x  *= lerp(input.tint.x, 1, diffuse.x * diffuse.x);
			diffuse.y  *= lerp(input.tint.y, 1, diffuse.y * diffuse.y);
			diffuse.z  *= lerp(input.tint.z, 1, diffuse.z * diffuse.z);
		#else
			diffuse *= input.tint;
		#endif
		
		#else
			return input.tint;
		#endif
		
		return diffuse;
	}

[End_Pass]
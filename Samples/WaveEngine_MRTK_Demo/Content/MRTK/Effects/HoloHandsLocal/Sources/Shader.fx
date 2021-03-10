[Begin_ResourceLayout]

	[Directives:Kind BASE PULSE]
	[Directives:Multiview MULTIVIEW_OFF MULTIVIEW]
	[Directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

	cbuffer Base : register(b0)
	{
		float4x4 WorldViewProj      : packoffset(c0);	[WorldViewProjection]
		float4x4 World              : packoffset(c4);	[World]
		float Time					: packoffset(c8.x); [Time]
	};

	cbuffer Matrices : register(b1)
	{
		float3 EdgeColor		: packoffset(c0.x); [Default(1,1,1)]
		float EdgeWidth			: packoffset(c0.w); [Default(0.01)]
		float3 FillColor0		: packoffset(c1.x); [Default(0.340732095, 0.22439724, 0.899506656)]
		float EdgeSmooth		: packoffset(c1.w); [Default(0.08)]
		float3 FillColor1		: packoffset(c2.x); [Default(0.040951978, 0.24433369, 0.973789928)]
		float Displacement		: packoffset(c2.w); [Default(0.2)]
		float t                 : packoffset(c3.x); [Default(0)]
		float distorsionH       : packoffset(c3.y); [Default(2)]
		
		float t0              : packoffset(c3.z); [Default(0.15)]
		float t1              : packoffset(c3.w); [Default(-0.3)]
		
		float Alpha           : packoffset(c4.x); [Default(0.8)]
	};

	cbuffer PerCamera : register(b2)
	{
		float4x4  MultiviewViewProj[6]		: packoffset(c0.x);  [MultiviewViewProjection]
		int       EyeCount                  : packoffset(c10.x); [MultiviewCount]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[Profile 11_0]
	[Entrypoints VS=VS GS=GS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		
#if MULTIVIEW
		uint InstId : SV_InstanceID;
#endif
	};

	struct GS_IN
	{
		float4 pos 		: SV_POSITION;
		
#if MULTIVIEW
		uint InstId         : SV_RenderTargetArrayIndex;
#endif
	};
	
	struct PS_IN
	{
		float4 pos 		: SV_POSITION;
		float4 info 	: TEXCOORD0;
		float4 extra	: TEXCOORD1;
		
#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif		
	};

	//Helpers Functions

	uint XorShift(inout uint state)
	{
		state ^= state << 13;
		state ^= state >> 17;
		state ^= state << 15;
		return state;
	}

	float RandomFloat(inout uint state)
	{
		return XorShift(state) * (1.f / 4294967296.f);
	}
	
	float3 GammaToLinear(const float3 color)
	{
		return pow(color.rgb, 2.2);
	}
	
#if PULSE
	float InCirc(float x)
	{
		return 1 - sqrt(1 - pow(x, 2));
	}
	
	float OutCirc(float x)
	{
		return sqrt(1 - pow(x - 1, 2));
	}
	
	inline float3 GetNormal(GS_IN input[3])
	{
		float3 a = input[0].pos.xyz - input[1].pos.xyz;
		float3 b = input[2].pos.xyz - input[1].pos.xyz;
		return normalize(cross(a, b));
	}
#endif

	GS_IN VS(VS_IN input)
	{
		GS_IN output = (GS_IN)0;

		output.pos = input.Position;
#if MULTIVIEW
		output.InstId = input.InstId;
#endif

		return output;
	}
	
[maxvertexcount(3)]
	void GS(triangle GS_IN input[3], inout TriangleStream<PS_IN> outStream)
    {   
        PS_IN output;
        
        uint seed0 = input[0].pos.x * 8731 - input[0].pos.z * 457 + input[0].pos.y * 599;
        uint seed1 = input[0].pos.x * -8969 + input[0].pos.z * 311 - input[0].pos.y * 523;
        float seed = seed0 + seed1;
        float rnd = RandomFloat(seed);

		float4 dir = 0;
		float distorsion = 0;
		
#if PULSE
        float4 center = (input[0].pos + input[1].pos + input[2].pos) / 3.0;

		//float pulse = input[0].pos.y + 0.25 + cos(Time) * 0.7;
		//pulse = smoothstep(0.1,0.5, pulse);
		
		//float tTest = cos(Time) * 0.5 + 0.5;
		float yStart = lerp(t0, t1, t);
		float pulse = saturate((center.z - yStart) * distorsionH);
        
        distorsion = lerp(InCirc(pulse), OutCirc(pulse), rnd);
		
		float4 normal = float4(GetNormal(input), 0);
#endif

#if MULTIVIEW
		const int vid = input[0].InstId % EyeCount;
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

		float3 vertexColor[3] = {
			{1, 0, 0},
			{0, 1, 0},
			{0, 0, 1}
		};
		
		for (int i = 0; i < 3; i++)
		{
		
			float4 newPos = input[i].pos;
#if PULSE
			dir = center - input[i].pos;
			newPos = input[i].pos + normal * distorsion * Displacement + dir * distorsion;
#endif
			
	        output.pos = mul(newPos, worldViewProj);
	        output.extra = distorsion.xxxx;
			output.info = float4(vertexColor[i], rnd);
	        outStream.Append(output);
		}
	}

	float4 PS(PS_IN input) : SV_Target
	{	
		float minBary = min(input.info.x, min(input.info.y, input.info.z));
		minBary = smoothstep(EdgeWidth, EdgeWidth + EdgeSmooth, minBary);
		
		// Transition Color
		float3 fillcolor = lerp(FillColor0, FillColor1, input.info.w);
		float3 color = lerp(EdgeColor, fillcolor, minBary);
		
		float alpha = Alpha;
#if PULSE
		alpha = saturate(alpha - input.extra);
#endif
		color *= alpha;
		
#if !GAMMA_COLORSPACE
		color = GammaToLinear(color);
#endif
		
		return float4(color, alpha);
	}

[End_Pass]
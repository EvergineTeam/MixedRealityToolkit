[Begin_ResourceLayout]
	[Directives:Range RANGE_OFF RANGE]
	[Directives:Decay DECAY_OFF DECAY]
	[Directives:SimulationSpace LOCAL_OFF LOCAL]

	struct AttractedParticle
	{
		float3 Position; 
		float Angle;
		
		float4 Tint;
		
		float3 Velocity;
		float Size;
		
	};

	cbuffer Matrices : register(b0)
	{
		float3		Position 				: packoffset(c0.x);
		float		EllapsedTime			: packoffset(c0.w);
		float4x4	ParticlesWorldInverse	: packoffset(c1.x);
	}
	
	cbuffer ParamsBuffer : register(b1)
	{
		float	Strength 		: packoffset(c0.x);
		float 	Range			: packoffset(c0.y);		
		uint 	MaxParticles	: packoffset(c0.z);
		float	TimeFactor		: packoffset(c0.w);
	}

	RWStructuredBuffer<AttractedParticle> ParticleBuffer : register(u0);

[End_ResourceLayout]

[Begin_Pass:Force]

	[Profile 11_0]
	[Entrypoints CS=CS]
	
	[numthreads(256, 1, 1)]
	void CS(uint3 id : SV_DispatchThreadID)
	{
		if (id.x < MaxParticles)
		{
			float time = EllapsedTime * TimeFactor;
	
			AttractedParticle p = ParticleBuffer[id.x];
			
			//p.Velocity += attract(p.Position, Position) * Strength * time;

			float3 distanceVector = Position - p.Position;

			float dist = length(distanceVector);
			
#if RANGE

			if(dist > Range)
			{
				return;
			}
#endif
			normalize(distanceVector);
			
			float3 f = Strength * time;			
				
#if DECAY
			f *= 1 / dist;
#endif

#if RANGE
			f *= (Range - dist) / Range;
#endif
				
			p.Velocity = lerp(p.Velocity, 0, 1 - exp(-f));
			
			ParticleBuffer[id.x] = p;
		}
	}

[End_Pass]
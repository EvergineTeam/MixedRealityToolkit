[Begin_ResourceLayout]
	[Directives:Random RANDOM_LOW RANDOM_MEDIUM RANDOM_HIGH]
	[Directives:Entity ENTITY_OFF ENTITY_SK ENTITY_STATIC_SK ENTITY_STATIC]	
	[Directives:EmitMeshType FROM_VERTEX FROM_EDGE FROM_TRIANGLE]	
	[Directives:Range RANGE_OFF RANGE]
	[Directives:Decay DECAY_OFF DECAY]
	[Directives:Cutout CUTOUT_OFF CUTOUT]
	[Directives:SimulationSpace LOCAL_OFF LOCAL]

	struct AttractedParticle
	{
		float3 Position; 
		float Angle;
		
		float4 Tint;
		
		float3 Velocity;
		float Size;
	};
	
	
	struct AttractedParticleB
	{
		float3 InitVelocity;
		uint Seed;
		
		float4 InitColor;
		
		float InitSize;
		float AngularVelocity;
		float DeadTime;
		float RemainingLerp;
	};


	struct EntityMeshVertexData
	{
		float3 	Position;
		uint 	MeshIndex;	
		
		float3 	Normal;
		float 	Padding;
	};

	cbuffer Matrices : register(b0)
	{
		float3		Position	 			: packoffset(c0.x);
		float		EllapsedTime			: packoffset(c0.w);
		
		float4x4	ParticlesWorldInverse	: packoffset(c1);
		
		float4x4 	SKRootJoint				: packoffset(c5);
	}
	
	cbuffer ParamsBuffer : register(b1)
	{
		float	Strength 			: packoffset(c0.x);
		float 	Range				: packoffset(c0.y);		
		uint 	MaxParticles		: packoffset(c0.z);
		float	TimeFactor			: packoffset(c0.w);
		
		float	CutoutRange			: packoffset(c1.x);
		float 	CutoutStrength		: packoffset(c1.y);
		uint	NumEntityVertices	: packoffset(c1.z);		
		uint	NumEntityIndices	: packoffset(c1.w);		
		
		uint	NumSKPositions		: packoffset(c2.x);
	}
	
	// Entity emitter data.
	StructuredBuffer<EntityMeshVertexData> 	EntityVertices		:register(t0);
	StructuredBuffer<uint> 					EntityIndices		:register(t1);
	StructuredBuffer<float4x4> 				EntityTransforms	:register(t2);
	
	StructuredBuffer<float> 				EntitySKPositions	:register(t3);

	RWStructuredBuffer<AttractedParticle> 	ParticleBuffer 		: register(u0);
	RWStructuredBuffer<AttractedParticleB> 	ParticleBufferB 	:register(u1);

[End_ResourceLayout]

[Begin_Pass:Force]

	[Profile 11_0]
	[Entrypoints CS=CS]
	
#if ENTITY_SK || ENTITY_STATIC_SK || ENTITY_STATIC		
	uint rand_lcg(inout uint rng_state)
	{
		// LCG values from Numerical Recipes
		rng_state = 1664525 * rng_state + 1013904223;
		return rng_state;
	}
	
	uint rand_xorshift(inout uint rng_state)
	{
		// Xorshift algorithm from George Marsaglia's paper
		rng_state ^= (rng_state << 13);
		rng_state ^= (rng_state >> 17);
		rng_state ^= (rng_state << 5);
		return rng_state;
	}
	
	// Helper function
	uint wang_hash(inout uint seed)
	{
		seed = (seed ^ 61) ^ (seed >> 16);
		seed *= 9;
		seed = seed ^ (seed >> 4);
		seed *= 0x27d4eb2d;
		seed = seed ^ (seed >> 15);
		return seed;
	}
	
	uint RandomUInt(inout uint state)
	{
#if RANDOM_HIGH
		return wang_hash(state);
#elif RANDOM_MEDIUM
		return rand_xorshift(state);
#else
		return rand_lcg(state);
#endif
	}
	
	float RandomFloat(inout uint state)
	{
#if RANDOM_HIGH
		float r = wang_hash(state);
#elif RANDOM_MEDIUM
		float r = rand_xorshift(state);
#else
		float r = rand_lcg(state);
#endif

		return r * (1.f / 4294967296.f);
	}
	
	float3 GetEntityPosition( inout float seed)
	{	
		float3 position;
		float4x4 transform;
		
#if ENTITY_STATIC_SK
		if( (uint)(RandomFloat(seed) * (NumEntityVertices + NumSKPositions)) > NumEntityVertices)
		{
#endif
#if ENTITY_STATIC_SK || ENTITY_STATIC
#if FROM_TRIANGLE
			int index = ((int)(RandomFloat(seed) * (NumEntityIndices / 3))) * 3;
			
			uint vId1 = EntityIndices[index];
			uint vId2 = EntityIndices[index + 1];
			uint vId3 = EntityIndices[index + 2];
			
			EntityMeshVertexData v1 = EntityVertices[vId1];
			EntityMeshVertexData v2 = EntityVertices[vId2];
			EntityMeshVertexData v3 = EntityVertices[vId3];
			
			float r1 = RandomFloat(seed);
			float r2 = RandomFloat(seed);
			
			float3 p1 = v1.Position;
			float3 p2 = v2.Position - p1;
			float3 p3 = v3.Position - p1;
			
			if ((r1 + r2) > 1)
	        {
	            r1 = 1 - r1;
	            r2 = 1 - r2;
	        }
	        
	        position = p1 + (r1 * p2) + (r2 * p3);
#else
			int vId = (int)( RandomFloat(seed) * NumEntityVertices);		
			EntityMeshVertexData v1 = EntityVertices[vId];
			position = v1.Position;
#endif
			transform = EntityTransforms[v1.MeshIndex];
#endif
#if ENTITY_STATIC_SK
		}
		else
		{	
#endif
#if ENTITY_SK || ENTITY_STATIC_SK
			uint vId = (uint)(RandomFloat(seed) * NumSKPositions);
			uint sIndex = vId * 3;
			
			transform = SKRootJoint;
			position.x = EntitySKPositions[sIndex];
			position.y = EntitySKPositions[sIndex + 1];
			position.z = EntitySKPositions[sIndex + 2];
#endif
#if ENTITY_STATIC_SK
		}
#endif

        position = mul(float4(position, 1), transform).xyz;
        
		return position;
	}
#endif
	
	[numthreads(256, 1, 1)]
	void CS(uint3 id : SV_DispatchThreadID)
	{
		if (id.x < MaxParticles)
		{
			float time = EllapsedTime * TimeFactor;
	
			AttractedParticle p = ParticleBuffer[id.x];
			
#if ENTITY_STATIC_SK || ENTITY_STATIC || ENTITY_SK
			uint seed = ParticleBufferB[id.x].Seed;				
			float3 targetPosition = GetEntityPosition(seed);
#else 
			float3 targetPosition = Position;
#endif

			float3 distanceVector = targetPosition - p.Position;

			float dist = length(distanceVector);
			
#if RANGE

			if(dist > Range)
			{
				return;
			}
#endif

#if CUTOUT
			if(dist < CutoutRange)
			{
				float strength = (dist / CutoutRange);
				strength *= strength;
				strength = 1 - strength;
				
				float cutoutLerp = 1 - exp(-CutoutStrength * strength * time);
				
				p.Velocity = lerp(p.Velocity, 0, 2 *  cutoutLerp);
				p.Position = lerp(p.Position, targetPosition, cutoutLerp);
			}
			else
			{
#endif
			normalize(distanceVector);
			
			float3 f = Strength * time * distanceVector;				
				
#if DECAY
			f *= 1 / dist;
#endif

#if RANGE
			f *= (Range - dist) / Range;
#endif
				
			p.Velocity += f;
			
#if CUTOUT
			}
#endif
			
			ParticleBuffer[id.x] = p;
		}
	}

[End_Pass]
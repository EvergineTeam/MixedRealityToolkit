[Begin_ResourceLayout]
	[Directives:Random RANDOM_LOW RANDOM_MEDIUM RANDOM_HIGH]
	[Directives:EmitShape POINT SPHERE BOX ENTITY CIRCLE EDGE]
	[Directives:EmitSurface FROM_SURFACE_OFF FROM_SURFACE]
	[Directives:EmitFromNormal FROM_NORMAL_OFF FROM_NORMAL]
	[Directives:EmitRandomized EMIT_RANDOMIZED_OFF EMIT_RANDOMIZED EMIT_FULL_RANDOMIZED]
	[Directives:EmitMeshType FROM_OFF FROM_VERTEX FROM_EDGE FROM_TRIANGLE]	
	[Directives:EmitEntity ENTITY_OFF ENTITY_SK ENTITY_STATIC_SK ENTITY_STATIC]	
	[Directives:Space LOCAL_OFF LOCAL]
	
	[Directives:RandomLife RANDOM_LIFE_OFF RANDOM_LIFE]
	[Directives:RandomColor RANDOM_COLOR_OFF RANDOM_COLOR]
	[Directives:RandomSize RANDOM_SIZE_OFF RANDOM_SIZE]
	[Directives:RandomAngle RANDOM_ANGLE_OFF RANDOM_ANGLE]
	[Directives:RandomAngularVelocity RANDOM_ANGULAR_VELOCITY_OFF RANDOM_ANGULAR_VELOCITY]
	[Directives:RandomVelocity RANDOM_VELOCITY_OFF RANDOM_VELOCITY]
	
	[Directives:Noise NOISE_OFF NOISE]
	[Directives:ColorAnimated COLOR_ANIMATED_OFF COLOR_ANIMATED]
	[Directives:SizeAnimated SIZE_ANIMATED_OFF SIZE_ANIMATED]
	[Directives:Drag DRAG_OFF DRAG]
	
	struct ParticleStateA
	{
		float3 Position; 
		float Angle;
		
		float4 Tint;
		
		float3 Velocity;
		float Size;
		
	};
	
	struct ParticleStateB
	{
		float3 InitVelocity;
		uint Seed;
		
		float4 InitColor;
		
		float InitSize;
		float AngularVelocity;
		float DeadTime;
		float RemainingLerp;
		
	};
	
	struct EmitMeshVertexData
	{
		float3 	Position;
		uint 	MeshIndex;	
		float3 	Normal;
		float 	Padding;
	};
	
	struct EmitMeshInfo
	{
		uint PositionOffset;
		uint PositionStride;
		uint NormalOffset;
		uint NormalStride;
		
		uint IndexOffset;
		uint IndexStride;
		uint NumOfVertices;
		uint NumOfTriangles;
	};
	
	struct ForceInfo
	{
		float3 	Position;
		uint 	ForceType;
		
		float3 	Direction;
		float 	RangeSqr;
		
		uint	Category;		
		float 	Strength;		
		int		RangeEnabled;
		int 	DecayEnabled;		
	};
	
	struct EmitCounters
	{
		uint deadParticles;
	};
	
	cbuffer Base : register(b0)
	{
		float4x4 World			: packoffset(c0);
		float4x4 WorldInverse	: packoffset(c4);		
		float EllapsedTime 		: packoffset(c8.x);
		uint FrameCount			: packoffset(c8.y);
		uint NewParticles		: packoffset(c8.z);
		float TotalTime			: packoffset(c8.w);
		float4x4 SKRootJoint	: packoffset(c9);
	};
	
	cbuffer ParticleSystemInfo : register(b1)
	{
		float3 EmitSize				: packoffset(c0);
		float  EmitRandomize		: packoffset(c0.w);
		
		bool   EmitFromSurface		: packoffset(c1.x);
		bool   EmitFromNormal		: packoffset(c1.y);
		float  TimeFactor			: packoffset(c1.z); [Default(1)]
		float  LifeFactor			: packoffset(c1.w);	[Default(1)]
		
		float3 EmitVelocityOffset 	: packoffset(c2);
		uint MaxParticles			: packoffset(c2.w);
		
		float4 InitColor 			: packoffset(c3);
		
		float4 InitColor2 			: packoffset(c4);
		
		float InitVelocity 			: packoffset(c5.x);
		float InitVelocity2			: packoffset(c5.y);
		float InitAngle 			: packoffset(c5.z);
		float InitAngle2 			: packoffset(c5.w);
		
		float AngularVelocity 		: packoffset(c6.x);
		float AngularVelocity2 		: packoffset(c6.y);
		float InitLife 				: packoffset(c6.z);
		float InitLife2				: packoffset(c6.w);
		
		float InitSize 				: packoffset(c7.x);
		float InitSize2				: packoffset(c7.y);
		float Gravity				: packoffset(c7.z);
		float FloorBounciness		: packoffset(c7.w);
		
		uint	NumVertices			: packoffset(c8.x);		
		uint	NumIndices			: packoffset(c8.y);
		float 	Drag				: packoffset(c8.z);
		
		float	InvNoiseSize		: packoffset(c8.w);
		float	NoiseFrequency		: packoffset(c9.x);
		float	NoiseStrength		: packoffset(c9.y);
		uint	NumSKPositions		: packoffset(c9.z);

		float3 	NoiseSpeed   		: packoffset(c10.x);
	}
	
	Texture1D  ColorOverLife 					:register(t0);	
	Texture1D  SizeOverLife						:register(t1);
	Texture3D  NoiseTexture						:register(t2);
	
	SamplerState AnimationSampler				:register(s0);
	SamplerState NoiseSampler					:register(s1);
	
	// Entity emitter data.
	StructuredBuffer<EmitMeshVertexData> 	EntityVertices		:register(t3);
	StructuredBuffer<uint> 					EntityIndices		:register(t4);
	StructuredBuffer<float4x4> 				EntityTransforms	:register(t5);
	
	StructuredBuffer<float> 				EntitySKPositions	:register(t6);
	StructuredBuffer<float> 				EntitySKNormals		:register(t7);
	
	// Particle updatable buffers.
	RWStructuredBuffer<ParticleStateA> 	particleBufferA		: register(u0);
	RWStructuredBuffer<ParticleStateB> 	particleBufferB 	: register(u1);
	RWStructuredBuffer<uint> 			deadList			: register(u2);
	RWStructuredBuffer<EmitCounters> 	counters			: register(u3);

[End_ResourceLayout]

[Begin_Pass:Reset]

	[profile 11_0]
	[entrypoints CS=CS]
	[numthreads(256, 1, 1)]
	void CS(uint3 id : SV_DispatchThreadID)
	{
		if (id.x < MaxParticles)
		{
			deadList[id.x] = id.x;			
			particleBufferA[id.x] = (ParticleStateA)0;
			particleBufferB[id.x] = (ParticleStateB)0;
			counters[0].deadParticles = MaxParticles;
		}
	}

[End_Pass]

[Begin_Pass:Emit]

	[Profile 11_0]
	[Entrypoints CS=CS]
	
	
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
	
	float3 RandomInUnitSphere(inout uint seed)
	{
		float3 hash3 = float3(RandomFloat(seed), RandomFloat(seed), RandomFloat(seed));
		float3 h = hash3 * float3(2., 6.28318530718, 1.) - float3(1, 0, 0);
		float phi = h.y;
		float r = pow(h.z, 1. / 3.0);
		return r * float3(sqrt(1.0 - h.x * h.x) * float2(sin(phi), cos(phi)), h.x);
	}
	
	float3 RandomOnUnitSphere(inout uint seed)
	{
		float PI2 = 6.28318530718;
    	float z = 1 - 2 * RandomFloat(seed);
    	float xy = sqrt(1.0 - z * z);
    	float sn, cs;
    	sincos(PI2 * RandomFloat(seed), sn, cs);
    	return float3(sn * xy, cs * xy, z);
	}
	
	float3 RandomInUnitCircle(inout uint seed)
	{
    	float r = sqrt(RandomFloat(seed));
		float PI2 = 6.28318530718;
    	float sn, cs;
    	sincos(PI2 * RandomFloat(seed), sn, cs);
    	return float3(sn * r, 0, cs * r);
	}
	
	float3 RandomOnUnitCircle(inout uint seed)
	{
		float PI2 = 6.28318530718;
    	float sn, cs;
    	sincos(PI2 * RandomFloat(seed), sn, cs);
    	return float3(sn, 0, cs);
	}
	
#if SPHERE	
	void EmitSphere (inout float seed, inout float3 position, inout float3 direction)
	{
		// Sphere emit
#if FROM_SURFACE
				position =  RandomOnUnitSphere(seed);
#else
				position = RandomInUnitSphere(seed);
#endif
				
#if EMIT_FULL_RANDOMIZED
		direction = RandomOnUnitSphere(seed);		
#elif FROM_NORMAL && FROM_SURFACE			
				direction = position;
#elif FROM_NORMAL
				direction = normalize(position);
#else
				direction = float3(0,1,0);
#endif		
		
				position *= EmitSize.x;
	}
#endif

#if CIRCLE	
	void EmitCircle(inout float seed, inout float3 position, inout float3 direction)
	{
		// Sphere emit
#if FROM_SURFACE
				position =  RandomInUnitCircle(seed);
#else
				position = RandomOnUnitCircle(seed);
#endif
				
#if EMIT_FULL_RANDOMIZED
		direction = RandomOnUnitSphere(seed);		
#elif FROM_NORMAL && FROM_SURFACE			
				direction = position;
#elif FROM_NORMAL
				direction = normalize(position);
#else
				direction = float3(0,1,0);
#endif		
		
				position *= EmitSize.x;
	}
#endif

#if EDGE
	void EmitEdge (inout float seed, inout float3 position, inout float3 direction)
	{
		position = float3((RandomFloat(seed) - 0.5) * EmitSize.x, 0, 0);		

#if EMIT_FULL_RANDOMIZED
		direction = RandomOnUnitSphere(seed);
#elif FROM_NORMAL
		float angle = RandomFloat(seed) * 6.28318530718;
		direction = float3(0, cos(angle), sin(angle));
#else
		direction = float3(0, 1, 0);
#endif
	}
#endif

#if BOX
	void EmitBox (inout float seed, inout float3 position, inout float3 direction)
	{
		position = float3(RandomFloat(seed), RandomFloat(seed), RandomFloat(seed));		
		position = (position - 0.5) * EmitSize;
		
#if EMIT_FULL_RANDOMIZED
		direction = RandomOnUnitSphere(seed);
#elif FROM_NORMAL
		direction = normalize(position);
#else
		direction = float3(0, 1, 0);
#endif
	}
#endif

#if ENTITY	
	void EmitMesh( inout float seed, inout float3 position, inout float3 direction)
	{
		
		float3 normal;
		float4x4 transform;
		
#if ENTITY_STATIC_SK
		if( (uint)(RandomFloat(seed) * (NumVertices + NumSKPositions)) > NumVertices)
		{
#endif
#if ENTITY_STATIC_SK || ENTITY_STATIC
#if FROM_TRIANGLE
			int index = ((int)(RandomFloat(seed) * (NumIndices / 3))) * 3;
			
			uint vId1 = EntityIndices[index];
			uint vId2 = EntityIndices[index + 1];
			uint vId3 = EntityIndices[index + 2];
			
			EmitMeshVertexData v1 = EntityVertices[vId1];
			EmitMeshVertexData v2 = EntityVertices[vId2];
			EmitMeshVertexData v3 = EntityVertices[vId3];
			
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
	        
	        transform = EntityTransforms[v1.MeshIndex];
	        position = mul(float4(position, 1), transform).xyz;
	        normal = v1.Normal;
#else
			uint vId = (uint)(RandomFloat(seed) * NumVertices);
			
			EmitMeshVertexData vertex = EntityVertices[vId];
			transform = EntityTransforms[vertex.MeshIndex];
			position = mul(float4(vertex.Position, 1), transform).xyz;
			normal = vertex.Normal;
#endif
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
			
			position = mul(float4(position, 1), transform).xyz;
			normal.x = EntitySKNormals[sIndex];
			normal.y = EntitySKNormals[sIndex + 1];
			normal.z = EntitySKNormals[sIndex + 2];
#endif
#if ENTITY_STATIC_SK
		}
#endif

#if EMIT_FULL_RANDOMIZED
		direction = RandomOnUnitSphere(seed);
#elif FROM_NORMAL
		direction = normal;
#else
		direction = float3(0, 1, 0);
#endif
		
#if ENTITY_SK ||ENTITY_STATIC_SK || ENTITY_STATIC

		direction = mul(direction, (float3x3)transform);
		normalize(direction);
#else
		direction = float3(0,1,0);
#endif

	}
#endif

	[numthreads(32, 1, 1)]
	void CS(uint3 id : SV_DispatchThreadID)
	{
		// Check to make sure we don't emit more particles than we specified
		if (id.x < NewParticles)
		{
			//GroupMemoryBarrierWithGroupSync();
	
			int nDead;
			InterlockedAdd(counters[0].deadParticles, -1, nDead);
	
			uint seed = (id.x * 1973 + 9277 + FrameCount * 26699) | 1;
	
			// Initialize the particle data to zero to avoid any unexpected results
			ParticleStateA pA = (ParticleStateA)0;
			ParticleStateB pB = (ParticleStateB)0;

#if RANDOM_ANGULAR_VELOCITY
			float initAngularVelocity = lerp(AngularVelocity, AngularVelocity2, RandomFloat(seed));
#else
			float initAngularVelocity = AngularVelocity;
#endif

#if RANDOM_VELOCITY
			float initVelocity = lerp(InitVelocity, InitVelocity2, RandomFloat(seed));
#else
			float initVelocity = InitVelocity;
#endif
			float3 initDirection;

#if RANDOM_SIZE
			float initSize = lerp(InitSize, InitSize2, RandomFloat(seed));
#else
			float initSize = InitSize;
#endif
			
#if RANDOM_ANGLE
			float initAngle = lerp(InitAngle, InitAngle2, RandomFloat(seed));
#else
			float initAngle = InitAngle;
#endif

#if RANDOM_LIFE
			float initLife = lerp(InitLife, InitLife2, RandomFloat(seed));
#else
			float initLife = InitLife;
#endif

#if RANDOM_COLOR
			float4 initColor = lerp(InitColor, InitColor2, RandomFloat(seed));
#else
			float4 initColor = InitColor;
#endif
			pB.Seed = RandomUInt(seed);

#if SPHERE
			EmitSphere(seed, pA.Position, initDirection);
#elif EDGE
			EmitEdge(seed, pA.Position, initDirection);
#elif BOX
			EmitBox(seed, pA.Position, initDirection);
#elif CIRCLE
			EmitCircle(seed, pA.Position, initDirection);
#elif ENTITY
			uint emitSeed = pB.Seed;
			EmitMesh(emitSeed, pA.Position, initDirection);
#else
			pA.Position = float3(0,0,0);	
			initDirection = float3(0,1,0);
#endif

#if EMIT_RANDOMIZED
			float3 randomVelocity = RandomOnUnitSphere(seed);
			initDirection = lerp(initDirection, randomVelocity, EmitRandomize);
#endif

			normalize(initDirection);
				
			pA.Angle = initAngle;
			pA.Tint = initColor;
			pA.Size = initSize;
			pA.Velocity = (initVelocity * initDirection) + EmitVelocityOffset;
	
#if ENTITY
	#if LOCAL
			pA.Position = mul(float4(pA.Position, 1), WorldInverse).xyz;
			pA.Velocity = mul(pA.Velocity, (float3x3)WorldInverse);
	#endif
#else
	#if !LOCAL
			pA.Position = mul(float4(pA.Position, 1), World).xyz;
			pA.Velocity = mul(pA.Velocity, (float3x3)World);
	#endif
#endif
			
			pB.InitVelocity = initVelocity;			
			pB.InitColor = initColor;
			pB.InitSize = initSize;
			pB.AngularVelocity = initAngularVelocity;
			pB.DeadTime = initLife;
			pB.RemainingLerp = max(0.001, (initLife - (RandomFloat(seed) * EllapsedTime))/initLife); 
	
	
			uint pIndex = deadList[nDead - 1];
	
			particleBufferA[pIndex] = pA;
			particleBufferB[pIndex] = pB;
		}
	}

[End_Pass]

[Begin_Pass:Simulate]

	[Profile 11_0]
	[Entrypoints CS=CS]

	
	// noise functions
	// returns random value in [-1, 1]
	float3 noise3f(float3 p) 
	{
		return NoiseTexture.SampleLevel(NoiseSampler, (p  + (NoiseSpeed * TotalTime))* InvNoiseSize, 0).xyz;
	}
	
	// fractal sum
	float3 fBm3f(float3 p, int octaves, float lacunarity, float gain)
	{
		float freq = 1.0, amp = 0.5;
		float3 sum = float3(0, 0, 0);
		for (int i = 0; i < octaves; i++) {
			sum += noise3f(p * freq) * amp;
			freq *= lacunarity;
			amp *= gain;
		}
		return sum;
	}

	[numthreads(256, 1, 1)]
	void CS(uint3 id : SV_DispatchThreadID)
	{
		if (id.x < MaxParticles)
		{
			// Wait after draw args are written so no other threads can write to them before they are initialized
			//GroupMemoryBarrierWithGroupSync();
			
			float ellapsed = EllapsedTime * TimeFactor;
	
			ParticleStateA pA = particleBufferA[id.x];
			ParticleStateB pB = particleBufferB[id.x];
	
			if (pB.RemainingLerp > 0)
			{
				pB.RemainingLerp -= (ellapsed * LifeFactor) / pB.DeadTime;
	
				if (pB.RemainingLerp <= 0)
				{
					pB.RemainingLerp = 0;
					pA.Size = 0;
	
					int dId;					
					InterlockedAdd(counters[0].deadParticles, 1, dId);
	
					deadList[dId] = id.x;
				}
				else
				{	
					pA.Angle += pB.AngularVelocity * ellapsed;
					pA.Position += pA.Velocity * ellapsed;				
					pA.Velocity -= float3(0, (Gravity * ellapsed), 0);
					
#if NOISE
					pA.Velocity += fBm3f((pA.Position) * NoiseFrequency, 4, 2.0, 0.5) * NoiseStrength;// * length(pA.Velocity);
#endif

#if COLOR_ANIMATED || SIZE_ANIMATED
					float ageLerp = 1 - pB.RemainingLerp;
#endif
					
#if COLOR_ANIMATED
					
					pA.Tint = pB.InitColor * ColorOverLife.SampleLevel(AnimationSampler, ageLerp, 0);
#endif

#if SIZE_ANIMATED
					float sizeOverLife = SizeOverLife.SampleLevel(AnimationSampler, ageLerp, 0).r;
					pA.Size = pB.InitSize * sizeOverLife;
#endif

#if DRAG
					pA.Velocity = lerp(pA.Velocity, 0, 1 - exp(-Drag * ellapsed));
#endif
				}
			}
	
			particleBufferA[id.x] = pA;
			particleBufferB[id.x] = pB;
		}
	}

[End_Pass]
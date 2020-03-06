[Begin_ResourceLayout]

	cbuffer Base : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Matrices : register(b1)
	{
		float3 Color			: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float EdgeWidth			: packoffset(c0.w); [Default(0.01)]
		float3 EdgeColor		: packoffset(c1.x); [Default(1,1,1)]
		float EdgeSmooth		: packoffset(c1.w); [Default(0.08)]
		float3 FillColor0		: packoffset(c2.x); [Default(0.613, 0.507, 0.953)]
		float Time				: packoffset(c2.w); [Time]// [Default(4.7)]//
		float3 FillColor1		: packoffset(c3.x); [Default(0.234, 0.527, 0.988)]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 11_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
	};

	struct GS_IN
	{
		float4 pos 		: SV_POSITION;
		float4 ipos		: TEXCOORD0;
	};
	
	struct PS_IN
	{
		float4 pos 		: SV_POSITION;
		float3 barycentricCoordinates : TEXCOORD0;
		
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
	
	float InCirc(float x)
	{
		return 1 - sqrt(1 - pow(x, 2));
	}
	
	float OutCirc(float x)
	{
		return sqrt(1 - pow(x - 1, 2));
	}
	
	GS_IN VS(VS_IN input)
	{
		GS_IN output = (GS_IN)0;

		output.pos = mul(input.Position, WorldViewProj);
		output.ipos = input.Position;

		return output;
	}

	[maxvertexcount(3)]
	void GS(triangle GS_IN input[3], inout TriangleStream<PS_IN> outStream)
    {   
        /*PS_IN output;
        
        uint seed0 = input[0].ipos.x * 8731 - input[0].ipos.z * 457 + input[0].ipos.y * 599;
        uint seed1 = input[0].ipos.x * -8969 + input[0].ipos.z * 311 - input[0].ipos.y * 523;
        float seed = seed0 + seed1;
        float rnd = RandomFloat(seed);
        
        float4 center = (input[0].pos + input[1].pos + input[2].pos) / 3.0;
        float pulse = input[0].ipos.y + 0.25 + cos(Time * 5) * 0.7;
        pulse = smoothstep(0.1,0.5, pulse);

		float distorsion = lerp(InCirc(pulse), OutCirc(pulse), rnd);

        // vertex0
        float4 dir = center - input[0].pos;
        output.pos = input[0].pos + dir * distorsion;
        output.extra = distorsion.xxxx;
		output.info = float4(1, 0, 0, rnd);
        outStream.Append(output);
        
        // vertex1
        dir = center - input[1].pos;
        output.pos = input[1].pos + dir * distorsion;
        output.extra = distorsion.xxxx;
        output.info = float4(0, 1, 0, rnd);
		outStream.Append(output);
        
        // vertex2
        dir = center - input[2].pos;
		output.pos = input[2].pos + dir * distorsion;
		output.extra = distorsion.xxxx;
        output.info = float4(0, 0, 1, rnd);
        outStream.Append(output);*/
        
        
        PS_IN output;
        output.pos = input[0].pos;
        output.barycentricCoordinates = float3(1, 0, 0);
        outStream.Append(output);
        
        output.pos = input[1].pos;
        output.barycentricCoordinates = float3(0, 1, 0);
        outStream.Append(output);
        
        output.pos = input[2].pos;
        output.barycentricCoordinates = float3(0, 0, 1);
        outStream.Append(output);
	}

	float4 PS(PS_IN input) : SV_Target
	{	
		return float4(input.barycentricCoordinates, 1);
	
		/*float4 albedo = float4(1, 1, 1, 1);
		
		float3 barys;
		barys.xy = input.barycentricCoordinates;
		barys.z = 1 - barys.x - barys.y;
		float minBary = min(barys.x, min(barys.y, barys.z));
		
		return float4(barys, 1);*/
	
		/*float minBary = min(input.info.x, min(input.info.y, input.info.z));
		minBary = smoothstep(EdgeWidth, EdgeWidth + EdgeSmooth, minBary);
		
		// Transition Color
		float3 fillcolor = lerp(FillColor0, FillColor1, input.info.w);
		float3 color = lerp(EdgeColor, fillcolor, minBary);
		
		float alpha = 1 - input.extra;
		color *= alpha;
		return float4(color, alpha);*/
		
		//float value = input.ipos.y + 0.2 + sin(Time) * 0.6;
		//value = smoothstep(0.1,0.3, value);
		//return float4(value.xxx, 1);
		//float value = input.info.w;
		//return float4(value.xxx ,1);
	}

[End_Pass]
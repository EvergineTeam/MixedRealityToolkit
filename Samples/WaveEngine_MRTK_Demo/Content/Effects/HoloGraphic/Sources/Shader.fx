[Begin_ResourceLayout]

	cbuffer Base : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Matrices : register(b1)
	{
		float3 Color			: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float3 EdgeColor		: packoffset(c1.x); [Default(1,1,1)]
		float EdgeWidth			: packoffset(c0.w); [Default(0.01)]
		float EdgeOffset		: packoffset(c1.w); [Default(0.08)]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 11_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	struct PS_IN
	{
		float4 pos 		: SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

		output.pos = mul(input.Position, WorldViewProj);
		output.uv = input.uv;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{	
		float2 distanceToEdge;
        distanceToEdge.x = abs(input.uv.x - 0.5) * 2.0;
        distanceToEdge.y = abs(input.uv.y - 0.5) * 2.0;
        float minDist = 1 - saturate((1 - max(distanceToEdge.x, distanceToEdge.y)) * EdgeWidth + EdgeOffset);
	
		return float4(Color + EdgeColor * minDist, 1);
	}

[End_Pass]
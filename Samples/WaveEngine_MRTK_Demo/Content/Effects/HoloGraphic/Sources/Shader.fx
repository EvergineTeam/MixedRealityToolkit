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
	[entrypoints VS=VS GS=GS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct GS_IN
	{
		float4 pos 		: SV_POSITION;
		float4 ipos		: TEXCOORD0;
		float2 uv		: TEXCOORD1;
	};
	
	struct PS_IN
	{
		float4 pos 		: SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 barycentricCoordinates : TEXCOORD1;
	};
	
	GS_IN VS(VS_IN input)
	{
		GS_IN output = (GS_IN)0;

		output.pos = mul(input.Position, WorldViewProj);
		output.ipos = input.Position;
		output.uv = input.uv;

		return output;
	}

	[maxvertexcount(3)]
	void GS(triangle GS_IN input[3], inout TriangleStream<PS_IN> outStream)
    {   
        PS_IN output;
        output.pos = input[0].pos;
        output.barycentricCoordinates = float3(1, 0, 0);
        output.uv = input[0].uv;
        outStream.Append(output);
        
        output.pos = input[1].pos;
        output.barycentricCoordinates = float3(0, 1, 0);
        output.uv = input[1].uv;
        outStream.Append(output);
        
        output.pos = input[2].pos;
        output.barycentricCoordinates = float3(0, 0, 1);
        output.uv = input[2].uv;
        outStream.Append(output);
	}

	float4 PS(PS_IN input) : SV_Target
	{	
		float2 distanceToEdge;
        distanceToEdge.x = abs(input.uv.x - 0.5) * 2.0;
        distanceToEdge.y = abs(input.uv.y - 0.5) * 2.0;
        float minDist = 1 - saturate((1 - max(distanceToEdge.x, distanceToEdge.y)) * EdgeWidth + EdgeOffset);
	
		/*if(minDist < 0.95)
			discard;
		return float4(1, 1, 1, 1);*/
	
		return float4(Color + EdgeColor * minDist, 1);
		
		/*float3 barys = input.barycentricCoordinates;
		float3 deltas = fwidth(barys);
		
		float3 smoothing = deltas * EdgeSmooth;
		float3 thickness = deltas * EdgeWidth;
		barys = smoothstep(thickness, thickness + smoothing, barys);
		
		float minBary = min(barys.x, min(barys.y, barys.z));
		
		return float4(lerp(EdgeColor, Color, minBary), 1);*/
	}

[End_Pass]
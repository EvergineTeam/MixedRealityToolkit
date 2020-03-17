[Begin_ResourceLayout]

[directives:Multiview MULTIVIEW_OFF MULTIVIEW]

	cbuffer Base : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Matrices : register(b1)
	{
		float3 Color			: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float3 EdgeColor		: packoffset(c1.x); [Default(1,1,1)]
		float EdgeWidth			: packoffset(c0.w); [Default(1)]
		float EdgeSmooth		: packoffset(c1.w); [Default(1)]
	};

	cbuffer PerCamera : register(b2)
	{
		float4x4  MultiviewViewProj[2]		: packoffset(c0.x);  [StereoCameraViewProjection]
		int       EyeCount                  : packoffset(c10.x); [StereoEyeCount]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 11_0]
	[entrypoints VS=VS GS=GS PS=PS]

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
		float4 ipos		: TEXCOORD0;
		
#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif
	};
	
	struct PS_IN
	{
		float4 pos 		: SV_POSITION;
		float3 barycentricCoordinates : TEXCOORD1;

#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif
	};
	
	GS_IN VS(VS_IN input)
	{
		GS_IN output = (GS_IN)0;

#if MULTIVIEW
		const int vid = input.InstId % EyeCount;
		const float4x4 viewProj = MultiviewViewProj[vid];
	
		float4x4 worldViewProj = mul(World, viewProj);
		
		// Note which view this vertex has been sent to. Used for matrix lookup.
		// Taking the modulo of the instance ID allows geometry instancing to be used
		// along with stereo instanced drawing; in that case, two copies of each 
		// instance would be drawn, one for left and one for right.
	
		output.ViewId = vid;
#else
		float4x4 worldViewProj = WorldViewProj;
#endif

		output.pos = mul(input.Position, worldViewProj);
		output.ipos = input.Position;

		return output;
	}

	[maxvertexcount(3)]
	void GS(triangle GS_IN input[3], inout TriangleStream<PS_IN> outStream)
    {   
        PS_IN output;

#if MULTIVIEW
        output.ViewId = input[0].ViewId;
#endif
        
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
		float3 barys = input.barycentricCoordinates;
		float3 deltas = fwidth(barys);
		
		float3 smoothing = deltas * EdgeSmooth;
		float3 thickness = deltas * EdgeWidth;
		barys = smoothstep(thickness, thickness + smoothing, barys);
		
		float minBary = min(barys.x, min(barys.y, barys.z));
		
		return float4(lerp(EdgeColor, Color, minBary), 1);
	}

[End_Pass]
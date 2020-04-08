[Begin_ResourceLayout]

[directives:FINGERS_DIST FINGERS_DIST NO_FINGERS_DIST]
[directives:BORDER_LIGHT BORDER_LIGHT_OFF BORDER_LIGHT]
[directives:INNER_GLOW INNER_GLOW_OFF INNER_GLOW]
[directives:HoverLight HOVER_LIGHT_OFF HOVER_LIGHT_ON]
[directives:Multiview MULTIVIEW_OFF MULTIVIEW]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Parameters : register(b1)
	{
		float3 Color			: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float Alpha             : packoffset(c0.w); [Default(1.0)]

		float3 InnerGlowColor   : packoffset(c1);   [Default(1.0, 1.0, 1.0)]
		float InnerGlowAlpha    : packoffset(c1.w); [Default(1.0)]
		float InnerGlowPower    : packoffset(c2.w); [Default(10.0)]
		
		float MaxFingerDist     : packoffset(c3.w); [Default(1.0)]
		float3 FingerPosLeft    : packoffset(c2);
		float3 FingerPosRight   : packoffset(c3);	
		
		float3 BorderLightColor : packoffset(c4);   [Default(1.0, 1.0, 1.0)]
		float BorderLightWidth   : packoffset(c4.w); [Default(0.1)]
	};
	
	cbuffer PerCamera : register(b2)
	{
		float4x4  MultiviewViewProj[2]		: packoffset(c0.x);  [StereoCameraViewProjection]
		int       EyeCount                  : packoffset(c10.x); [StereoEyeCount]
	};

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 11_0]
	[entrypoints VS=VS PS=PS]

	struct VS_IN
	{
		float4 Position : POSITION;
		float2 uv : TEXCOORD0;
		
#if MULTIVIEW
		uint InstId : SV_InstanceID;
#endif
	};
	
	struct PS_IN
	{
		float4 pos      : SV_POSITION;
		float4 worldPos : TEXCOORD0;
		float4 uv       : TEXCOORD1;
		
#if MULTIVIEW
		uint ViewId         : SV_RenderTargetArrayIndex;
#endif
	};
	
	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

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
		output.worldPos = mul(input.Position, World);
		output.uv.xy = input.uv.xy;
		
		output.uv.z = length(mul(float4(1, 0, 0, 0), World)); //Scale in X
		output.uv.w = length(mul(float4(0, 0, 1, 0), World)); //Scale in Y
		
		float minV = min(output.uv.z, output.uv.w);
		output.uv.z = output.uv.z / minV;
		output.uv.w = output.uv.w / minV;

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{	
		float2 distanceToEdge;
        distanceToEdge.x = (1 - abs(input.uv.x - 0.5) * 2.0) * input.uv.z;
        distanceToEdge.y = (1 - abs(input.uv.y - 0.5) * 2.0) * input.uv.w;
        
        float2 distanceToCenter;
        distanceToCenter.x = 1 - saturate(distanceToEdge.x);
        distanceToCenter.y = 1 - saturate(distanceToEdge.y);
        
        float4 output = float4(Color, Alpha);

#if BORDER_LIGHT
        //Border light
        float border = (1 - saturate((min(distanceToEdge.x, distanceToEdge.y)) / BorderLightWidth));
        border = smoothstep(0.0, 0.2, border);
        output.rgb = lerp(output.rgb, BorderLightColor * border, border);
        output.a += 1.0f * border;
#endif

#if INNER_GLOW
        //Inner Glow
        float2 uvGlow = pow(distanceToCenter * InnerGlowAlpha, InnerGlowPower);
        output.rgb += lerp(float3(0.0, 0.0, 0.0), InnerGlowColor, uvGlow.x + uvGlow.y);
        output.a   += lerp(0.0, InnerGlowAlpha, uvGlow.x + uvGlow.y);
#endif
        
#if FINGERS_DIST || HOVER_LIGHT_ON
        float minDist = min(length(input.worldPos - FingerPosLeft), length(input.worldPos - FingerPosRight));
#if FINGERS_DIST 
		output.a *= lerp(1, 0, saturate(minDist / MaxFingerDist));
#endif
		
#if HOVER_LIGHT_ON
		output += lerp(float4(1.0, 1.0, 1.0, 1.0) * 1.0, float4(0.0, 0.0, 0.0, 0.0), saturate(minDist / 0.03));
#endif
#endif
		output.rgb *= output.a;
	
		return output;
	}

[End_Pass]
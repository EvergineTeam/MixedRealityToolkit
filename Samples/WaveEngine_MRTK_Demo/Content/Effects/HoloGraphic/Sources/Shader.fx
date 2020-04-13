[Begin_ResourceLayout]

[directives:BORDER_LIGHT BORDER_LIGHT_OFF BORDER_LIGHT]
[directives:BORDER_LIGHT_REPLACES_ALBEDO BORDER_LIGHT_REPLACES_ALBEDO_OFF BORDER_LIGHT_REPLACES_ALBEDO]
[directives:BORDER_LIGHT_OPAQUE BORDER_LIGHT_OPAQUE_OFF BORDER_LIGHT_OPAQUE]
[directives:INNER_GLOW INNER_GLOW_OFF INNER_GLOW]
[directives:ROUND_CORNERS ROUND_CORNERS_OFF ROUND_CORNERS]
[directives:IGNORE_Z_SCALE IGNORE_Z_SCALE_OFF IGNORE_Z_SCALE]
[directives:NEAR_LIGHT_FADE NEAR_LIGHT_FADE_OFF NEAR_LIGHT_FADE]
[directives:HOVER_LIGHT HOVER_LIGHT_OFF HOVER_LIGHT]
[directives:PROXIMITY_LIGHT PROXIMITY_LIGHT_OFF PROXIMITY_LIGHT_ON]
[directives:Multiview MULTIVIEW_OFF MULTIVIEW]

	cbuffer PerDrawCall : register(b0)
	{
		float4x4 WorldViewProj		: packoffset(c0);	[WorldViewProjection]
		float4x4 World				: packoffset(c4);	[World]
	};

	cbuffer Parameters : register(b1)
	{
		float3 Color				: packoffset(c0);   [Default(0.3, 0.3, 1.0)]
		float Alpha             	: packoffset(c0.w); [Default(1.0)]

		float3 InnerGlowColor   	: packoffset(c1);   [Default(1.0, 1.0, 1.0)]
		float InnerGlowAlpha   		: packoffset(c1.w); [Default(0.75)]
		float InnerGlowPower    	: packoffset(c2.x); [Default(4.0)] //Range(2.0, 32.0)
		
		float BorderWidth  			: packoffset(c2.y); [Default(0.1)] //Range(0.0, 1.0) 
		float BorderMinValue 		: packoffset(c2.z); [Default(0.1)] //Range(0.0, 1.0)
		float FluentLightIntensity  : packoffset(c2.w); [Default(1.0)] //Range(0.0, 1.0)
		
		float RoundCornerRadious	: packoffset(c3.x); [Default(0.25)]  //Range(0.0, 0.5)
		float RoundCornerMargin 	: packoffset(c3.y); [Default(0.01)]  //Range(0.0, 0.5)
		float Cutoff				: packoffset(c3.z); [Default(0.5)]	 //Range(0.0, 0.5)

		// BORDER_LIGHT OR ROUND_CORNERS
		float EdgeSmoothingValue	: packoffset(c3.w); [Default(0.002)] //Range(0.0, 0.2)
		
		float FadeBeginDistance     : packoffset(c4.x); [Default(0.01)] //Range(0.0, 10.0)
        float FadeCompleteDistance  : packoffset(c5.y); [Default(0.1)]  //Range(0.0, 10.0)
        float FadeMinValue          : packoffset(c5.z); [Default(0.0)]  //Range(0.0, 1.0)
        
        float4 HoverLightData[6]     : packoffset(c20);
        float4 ProximityLightData[12] : packoffset(c26);
	};
	
	cbuffer PerCamera : register(b2)
	{
		float4x4  MultiviewViewProj[2]		: packoffset(c0.x);  [StereoCameraViewProjection]
		int       EyeCount                  : packoffset(c10.x); [StereoEyeCount]
	};

[End_ResourceLayout]

[Begin_Pass:Default]
	
#define IF(a, b, c) lerp(b, c, step((float) (a), 0.0));
	
#if ROUND_CORNERS
    inline float PointVsRoundedBox(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
    {
        return length(max(abs(position) - cornerCircleDistance, 0.0)) - cornerCircleRadius;
    }

    inline float RoundCornersSmooth(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
    {
        return smoothstep(1.0, 0.0, PointVsRoundedBox(position, cornerCircleDistance, cornerCircleRadius) / EdgeSmoothingValue);
    }

    inline float RoundCornersF(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
    {
    	//return RoundCornersSmooth(position, cornerCircleDistance, cornerCircleRadius);
        return PointVsRoundedBox(position, cornerCircleDistance, cornerCircleRadius) < 0.0;
    }
#endif

	[profile 11_0]
	[entrypoints VS=VS PS=PS]

#if HOVER_LIGHT || NEAR_LIGHT_FADE
	//#if MULTI_HOVER_LIGHT
		#define HOVER_LIGHT_COUNT 3
	//#else
	//	#define HOVER_LIGHT_COUNT 1
	//#endif
	#define HOVER_LIGHT_DATA_SIZE 2
		//float4 _HoverLightData[HOVER_LIGHT_COUNT * HOVER_LIGHT_DATA_SIZE];
	#if HOVER_COLOR_OVERRIDE
		fixed3 _HoverColorOverride;
	#endif
#endif
	
#if PROXIMITY_LIGHT || NEAR_LIGHT_FADE
	#define PROXIMITY_LIGHT_COUNT 2
	#define PROXIMITY_LIGHT_DATA_SIZE 6
		//float4 _ProximityLightData[PROXIMITY_LIGHT_COUNT * PROXIMITY_LIGHT_DATA_SIZE];
	#if PROXIMITY_LIGHT_COLOR_OVERRIDE
		float4 _ProximityLightCenterColorOverride;
		float4 _ProximityLightMiddleColorOverride;
		float4 _ProximityLightOuterColorOverride;
	#endif
#endif

#if HOVER_LIGHT || PROXIMITY_LIGHT || BORDER_LIGHT
	float _FluentLightIntensity;
#endif

#if NEAR_LIGHT_FADE
    static const float _MaxNearLightDistance = 10.0;

    inline float NearLightDistance(float4 light, float3 worldPosition)
    {
        return distance(worldPosition, light.xyz) + ((1.0 - light.w) * _MaxNearLightDistance);
    }
#endif

#if HOVER_LIGHT
    inline float HoverLight(float4 hoverLight, float inverseRadius, float3 worldPosition)
    {
        return (1.0 - saturate(length(hoverLight.xyz - worldPosition) * inverseRadius)) * hoverLight.w;
    }
#endif

	struct VS_IN
	{
		float4 Position : POSITION;
		float2 uv : TEXCOORD0;
		float3 normal : NORMAL;
		
#if MULTIVIEW
		uint InstId : SV_InstanceID;
#endif
	};
	
	struct PS_IN
	{
		float4 pos      : SV_POSITION;
		float4 worldPos : TEXCOORD0;
#if BORDER_LIGHT
        float4 uv 		: TEXCOORD1;
#elif INNER_GLOW || ROUND_CORNERS
        float2 uv 		: TEXCOORD1;
#endif

#if BORDER_LIGHT || ROUND_CORNERS
		float3 scale 	: TEXCOORD2;
#endif
		
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

		float3 localNormal = input.normal;


		output.pos = mul(input.Position, worldViewProj);
		output.worldPos = mul(input.Position, World);

#if NEAR_LIGHT_FADE
		float rangeInverse = 1.0 / (FadeBeginDistance - FadeCompleteDistance);
	    float fadeDistance = _MaxNearLightDistance;
	
	    [unroll]
	    for (int hoverLightIndex = 0; hoverLightIndex < HOVER_LIGHT_COUNT; ++hoverLightIndex)
	    {
	        int dataIndex = hoverLightIndex * HOVER_LIGHT_DATA_SIZE;
	        fadeDistance = min(fadeDistance, NearLightDistance(HoverLightData[dataIndex], output.worldPos.xyz));
	    }
	
	    /*[unroll]
	    for (int proximityLightIndex = 0; proximityLightIndex < PROXIMITY_LIGHT_COUNT; ++proximityLightIndex)
	    {
	        int dataIndex = proximityLightIndex * PROXIMITY_LIGHT_DATA_SIZE;
	        fadeDistance = min(fadeDistance, NearLightDistance(ProximityLightData[dataIndex], output.worldPos.xyz));
	    }*/

		output.worldPos.w = max(saturate(mad(fadeDistance, rangeInverse, - FadeCompleteDistance * rangeInverse)), FadeMinValue);
#endif

#if BORDER_LIGHT || ROUND_CORNERS
        output.scale.x = length(mul(float4(1.0, 0.0, 0.0, 0.0), World));
        output.scale.y = length(mul(float4(0.0, 1.0, 0.0, 0.0), World));
#if IGNORE_Z_SCALE
        output.scale.z = output.scale.x;
#else        
        output.scale.z = length(mul(float4(0.0, 0.0, 1.0, 0.0), World));
#endif
        
        output.uv.xy = input.uv;

        float minScale = min(min(output.scale.x, output.scale.y), output.scale.z);

#if BORDER_LIGHT
        float maxScale = max(max(output.scale.x, output.scale.y), output.scale.z);
        float minOverMiddleScale = minScale / (output.scale.x + output.scale.y + output.scale.z - minScale - maxScale);

        float areaYZ = output.scale.y * output.scale.z;
        float areaXZ = output.scale.z * output.scale.x;
        float areaXY = output.scale.x * output.scale.y;

        float borderWidth = BorderWidth;
#endif

        if (abs(localNormal.x) == 1.0) // Y,Z plane.
        {
            output.scale.x = output.scale.z;
            output.scale.y = output.scale.y;

#if BORDER_LIGHT
            if (areaYZ > areaXZ && areaYZ > areaXY)
            {
                borderWidth *= minOverMiddleScale;
            }
#endif
        }
        else if (abs(localNormal.y) == 1.0) // X,Z plane.
        {
            output.scale.x = output.scale.x;
            output.scale.y = output.scale.z;

#if BORDER_LIGHT
            if (areaXZ > areaXY && areaXZ > areaYZ)
            {
                borderWidth *= minOverMiddleScale;
            }
#endif
        }
        else  // X,Y plane.
        {
            output.scale.x = output.scale.x;
            output.scale.y = output.scale.y;

#if BORDER_LIGHT
            if (areaXY > areaYZ && areaXY > areaXZ)
            {
                borderWidth *= minOverMiddleScale;
            }
#endif
        }

        output.scale.z = minScale;

#if BORDER_LIGHT
        float scaleRatio = min(output.scale.x, output.scale.y) / max(output.scale.x, output.scale.y);
        output.uv.z = IF(output.scale.x > output.scale.y, 1.0 - (borderWidth * scaleRatio), 1.0 - borderWidth);
        output.uv.w = IF(output.scale.x > output.scale.y, 1.0 - borderWidth, 1.0 - (borderWidth * scaleRatio));
#endif
#elif INNER_GLOW
        output.uv = input.uv;
#endif

		return output;
	}

	float4 PS(PS_IN input) : SV_Target
	{
		float4 albedo = float4(Color, Alpha);
	
#if BORDER_LIGHT || INNER_GLOW || ROUND_CORNERS
		float2 distanceToEdge;
        distanceToEdge.x = abs(input.uv.x - 0.5) * 2.0;
        distanceToEdge.y = abs(input.uv.y - 0.5) * 2.0;
#endif
        
#if ROUND_CORNERS
        float2 halfScale = input.scale.xy * 0.5;
        float2 roundCornerPosition = distanceToEdge * halfScale;
        float cornerCircleRadius = saturate(max(RoundCornerRadious - RoundCornerMargin, 0.01)) * input.scale.z;
        float2 cornerCircleDistance = halfScale - (RoundCornerMargin * input.scale.z) - cornerCircleRadius;
        float roundCornerClip = RoundCornersF(roundCornerPosition, cornerCircleDistance, cornerCircleRadius);
#endif

#if BORDER_LIGHT
		float borderValue;
#if ROUND_CORNERS
		float borderMargin = RoundCornerMargin + BorderWidth * 0.5;
		cornerCircleRadius = saturate(max(RoundCornerRadious - borderMargin, 0.01)) * input.scale.z;
        cornerCircleDistance = halfScale - (borderMargin * input.scale.z) - cornerCircleRadius;
        borderValue =  1.0 - RoundCornersSmooth(roundCornerPosition, cornerCircleDistance, cornerCircleRadius);
#else
		borderValue = max(smoothstep(input.uv.z - EdgeSmoothingValue, input.uv.z + EdgeSmoothingValue, distanceToEdge.x),
                          smoothstep(input.uv.w - EdgeSmoothingValue, input.uv.w + EdgeSmoothingValue, distanceToEdge.y));
#endif
		float3 borderColor = float3(1.0, 1.0, 1.0);
		float3 borderContribution = borderColor * borderValue * BorderMinValue * FluentLightIntensity;

#if BORDER_LIGHT_REPLACES_ALBEDO
		albedo.rgb = lerp(albedo.rgb, borderContribution, borderValue);
#else
		albedo.rgb += borderContribution;
#endif

#if BORDER_LIGHT_OPAQUE
		float BorderLightOpaqueAlpha = 1.0f;
		albedo.a = max(albedo.a, borderValue * BorderLightOpaqueAlpha);
#endif

#endif

#if ROUND_CORNERS
		albedo *= roundCornerClip;
        clip(albedo.a - Cutoff);
        albedo.a = Alpha;
#endif
		float4 output = albedo;

#if INNER_GLOW
        float2 uvGlow = pow(distanceToEdge * InnerGlowAlpha, InnerGlowPower);
        output.rgb += lerp(float3(0.0, 0.0, 0.0), InnerGlowColor, uvGlow.x + uvGlow.y);
        output.a += lerp(0.0, InnerGlowAlpha, uvGlow.x + uvGlow.y);
#endif

#if (NEAR_LIGHT_FADE)
        output.a *= input.worldPos.w;
#endif
	
		output.rgb *= output.a;
	
		return output;
	}

[End_Pass]
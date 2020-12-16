[Begin_ResourceLayout]
	
	[directives:SunDisk SUNDISK_OFF SUNDISK]
	[directives:Multiview MULTIVIEW_OFF MULTIVIEW]
	[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

	cbuffer PerCamera : register(b0)
	{
		float3	  CameraPosition			: packoffset(c0.x); [CameraPosition]
		float4x4  ViewProj					: packoffset(c1.x); [ViewProjection]

		float     Exposure        			: packoffset(c5.x); [CameraExposure]
		float     IblLuminance				: packoffset(c5.y); [IBLLuminance]
		int       EyeCount 					: packoffset(c5.z); [MultiviewCount]
		float4    MultiviewPosition[6]		: packoffset(c6.x); [MultiviewPosition]
		float4x4  MultiviewViewProj[6]		: packoffset(c12.x); [MultiviewViewProjection]
		
	};
	
	cbuffer PerScene : register(b1)
	{
		float3 SunDirection  	: packoffset(c0); [SunDirection]
		float3 SunColor     	: packoffset(c1); [SunColor]
		float SunIntensity  	: packoffset(c1.w); [SunIntensity]
	};

	cbuffer Parameters : register(b2)
	{
		float SunSize				: packoffset(c0.x); [Default(0.02)]
    	float SunSizeConvergence	: packoffset(c0.y); [Default(500.000)]
	};
	
	Texture2D Texture				: register(t0);
	SamplerState TextureSampler		: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

	[profile 10_0]
	[entrypoints VS=VS PS=PS]

    #define MIE_G (-0.990)
    #define MIE_G2 0.9801

    #define SKY_GROUND_THRESHOLD -0.0075

	float3 LinearToGamma(const float3 color)
	{
		return pow(color.rgb, 1 / 2.2);
	}

	struct VS_IN
	{
		float4 vertex 	: POSITION;
		uint InstId		: SV_InstanceID;
	};

	struct PS_IN
	{
		float4  pos             : SV_POSITION;
		float3  rayDir          : TEXCOORD0;

	#if MULTIVIEW
		uint ViewId         	: SV_RenderTargetArrayIndex;
	#endif
	};

	PS_IN VS(VS_IN input)
	{
		PS_IN output = (PS_IN)0;

	#if MULTIVIEW
		const int iid = input.InstId / EyeCount;
		const int vid = input.InstId % EyeCount;		
		const float4x4 viewProj = MultiviewViewProj[vid];
		const float3 cameraPosition = MultiviewPosition[vid].xyz;

		// Note which view this vertex has been sent to. Used for matrix lookup.
		// Taking the modulo of the instance ID allows geometry instancing to be used
		// along with stereo instanced drawing; in that case, two copies of each 
		// instance would be drawn, one for left and one for right.
	
		output.ViewId = vid;		
	#else
		const float3 cameraPosition = CameraPosition;		
		const float4x4 viewProj = ViewProj;
	#endif
		
		float4 vertexPosition = input.vertex;		
		vertexPosition.xyz += cameraPosition.xyz;
		output.pos = mul(vertexPosition, viewProj);

        // Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
		output.rayDir  = -normalize(input.vertex.xyz);

		return output;
	}
	
    // Calculates the Mie phase function
    half getMiePhase(half eyeCos, half eyeCos2)
    {
        half temp = 1.0 + MIE_G2 - 2.0 * MIE_G * eyeCos;
        temp = pow(temp, pow(SunSize,0.65) * 10);
        temp = max(temp,1.0e-4); // prevent division by zero, esp. in half precision
        temp = 1.5 * ((1.0 - MIE_G2) / (2.0 + MIE_G2)) * (1.0 + eyeCos2) / temp;
        return temp;
    }
    
    static const float2 invAtan = float2(0.1591, 0.3183);
	float2 SampleSphericalMap(float3 v)
	{
	    float2 uv = float2(atan2(v.z, v.x), asin(v.y));
	    uv *= invAtan;
	    uv += 0.5;
	    uv.x = 1 - uv.x;
	    return uv;
	}

	float AngleBetween(in float3 dir0, in float3 dir1)
	{
		return acos(dot(dir0, dir1));
	}
	
    #if SUNDISK
    // Calculates the sun shape
    half calcSunAttenuation(half3 lightPos, half3 ray)
    {

        half3 delta = lightPos - ray;
        half dist = length(delta);
        half spot = 1.0 - smoothstep(0.0, SunSize, dist);
        return spot * spot;

    }
    #endif

	float4 PS(PS_IN input) : SV_Target
	{
        half3 ray = normalize(input.rayDir.xyz);        
        half y = ray.y / SKY_GROUND_THRESHOLD;

		float3 col = Texture.Sample(TextureSampler, SampleSphericalMap(ray)).rgb;
		
		#if SUNDISK
        if(y > 0)
        {
        	float3 sunColor = SunColor * SunIntensity;
        	float3 sunAttenuation = saturate(y)* calcSunAttenuation(normalize(SunDirection), -ray);
           	col += sunColor * sunAttenuation;
        }
        #endif

		col *= IblLuminance * Exposure;

#if GAMMA_COLORSPACE
		col = LinearToGamma(col);
#endif

        return float4(col ,1.0);
	}

[End_Pass]
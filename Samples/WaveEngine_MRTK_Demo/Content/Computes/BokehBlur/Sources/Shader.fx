[Begin_ResourceLayout]
	[directives:Field Near Far]
	[directives:Bokeh Circle Pentagon Hexagon Heptagon]	

cbuffer ParamsBuffer : register(b0)
{
	float ShapeSize : packoffset(c0.x); [Default(10)] // size of the bokeh blur radius in texel space
	float ShapeRotation : packoffset(c0.y); [Default(0)] // rotation in radius to apply to the bokeh shape	
}

Texture2D<float4> Input : register(t0);
Texture2D<float> CoC : register(t1); // NearCoC
RWTexture2D<float4> Output : register(u0);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

const static float BLUR_TAP_COUNT = 8;
static const float PI = 3.14159265358979323846;

#if Pentagon
static const float BladesNumber = 5;
#elif Hexagon
static const float BladesNumber = 6;
#elif Heptagon
static const float BladesNumber = 7;
#endif

// Shirley 97 "A Low Distortion Map Between Disk and Square"
float2 UnitSquareToUnitDiskPolar(float2 uv)
{
	float radius;
	float angle;

	const float PI_BY_2 = 1.5707963; // PI / 2
	const float PI_BY_4 = 0.785398;  // PI / 4
	const float EPSILON = 0.000001;
	
	// Remap [0, 1] to [-1, 1] centered
	float a = (2.0 * uv.x) - 1.0;
	float b = (2.0 * uv.y) - 1.0;
	
	// Morph to unit disk
	if (abs(a) > abs(b)) 
	{
		// First region (left and right quadrants of the disk)
		radius = a;
		angle = b / (a + EPSILON) * PI_BY_4;
	} 
	else 
	{
		// Second region (top and bottom quadrants of the disk)
		radius = b;
		angle = PI_BY_2 - (a / (b + EPSILON) * PI_BY_4);
	}

	if (radius < 0)
	{
		radius *= -1.0;
		angle += PI;
	}

	return float2(radius, angle);
}

// Remap a unit square in [0, 1] to a unit polygon in [-1, 1]
// Returns new cartesian coordinates (u,v) 
float2 SquareToPolygonMapping(float2 uv) {	
	float2 PolarCoord = UnitSquareToUnitDiskPolar(uv); // (radius, angle)
	
#if !Circle	
	// Re-scale radius to match a polygon shape
	PolarCoord.x *= ( cos(PI / BladesNumber) / ( cos(PolarCoord.y - (2.0 * PI / BladesNumber) * floor((BladesNumber * PolarCoord.y + PI) / 2.0 / PI ) )));
	
	// Apply a rotation to the polygon shape. 
	PolarCoord.y += ShapeRotation; 
#endif
	
	return float2(PolarCoord.x * cos(PolarCoord.y), PolarCoord.x * sin(PolarCoord.y));
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 UVAndScreenPos = (threadID.xy + 0.5) / outputSize;
	float2 texelSize = 1.0 / outputSize;

#if Far
	float PixelCoC = Input.SampleLevel(Sampler, UVAndScreenPos, 0).w;
#else
	float PixelCoC = CoC.SampleLevel(Sampler, UVAndScreenPos, 0);
#endif
	
	float3 ResultColor = 0;
	float Weight = 0;

	int TAP_COUNT = BLUR_TAP_COUNT; // Higher means less noise and make floodfilling easier after
	
	// Multiplying by PixelCoC guarantees a smooth evolution of the blur radius
	// especially visible on plane (like the floor) where CoC slowly grows with the distance.
	// This makes all the difference between a natural bokeh and some noticeable
	// in-focus and out-of-focus layer blending
#if Far
	float radius = ShapeSize * PixelCoC;
#else
	float radius = ShapeSize.x * 0.7 * PixelCoC; 
#endif
	
	if (PixelCoC > 0) // Ignore any pixel not belonging to far field
	{ 	
		// Weighted average of the texture samples inside the bokeh pattern
		// High radius and low sample count can create "gaps" which are fixed later (floodfill).
		for (int u = 0; u < TAP_COUNT; ++u)
		{
			for (int v = 0; v < TAP_COUNT; ++v)
			{
				// map to [0, 1]
				float2 uv = float2(u, v) / (TAP_COUNT - 1);
				
				// map to bokeh shape, then to texel size
				uv = SquareToPolygonMapping( uv ) * texelSize;
				uv = UVAndScreenPos.xy + radius * uv; 
				
				float4 tapColor = Input.SampleLevel(Sampler, uv, 0);
				
				#if Far
					// Weighted by CoC. Gives more influence to taps with a CoC higher than us.
					float TapWeight = tapColor.w * saturate(1.0 - (PixelCoC - tapColor.w)); 
				#else
					float TapWeight = saturate(tapColor.w * 10.0);
				#endif
				
				ResultColor +=  tapColor.xyz * TapWeight; 
				Weight += TapWeight;
			}
		}
				
		ResultColor /= (Weight + 0.0000001);
		Weight /= (TAP_COUNT * TAP_COUNT);		
	}
	
#if Far
	// From CoC 0.1, completely rely on the far field layer and stop lerping with in-focus layer
	Weight = saturate(Weight * 10);
#endif
	
	Output[threadID.xy] = float4(ResultColor, Weight);
}

[End_Pass]
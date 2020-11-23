[Begin_ResourceLayout]

cbuffer ParamsBuffer : register(b0)
{
	float SampleCount : packoffset(c0.x); [Default(16)]
	float Range : packoffset(c0.y); [Default(0.8)]
	float Power : packoffset(c0.z); [Default(1.0)]
	float ScaleBias : packoffset(c0.w); [Default(500.0)]	
	float Time : packoffset(c1.x); [Time]
}

cbuffer PerCameraBuffer : register(b1)
{
	float4x4 ViewProjection : packoffset(c0); [ViewProjection]
}

Texture2D<float> Depth : register(t0);
Texture2D<float4> Position : register(t1);
Texture2D<float4> Normal : register(t2); [ZPrePass]
RWTexture2D<float4> Output : register(u0); [Output(Position, 1.0, R8G8B8A8_UNorm)]

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

static const float PI = 3.14159265358979323846;

// returns a random float in range (0, 1). seed must be >0!
inline float rand(inout float seed, in float2 uv)
{
	float result = frac(sin(seed * dot(uv, float2(12.9898, 78.233))) * 43758.5453);
	seed += 1.0;
	return result;
}

// A uniform 2D random generator for hemisphere sampling
// http://holger.dammertz.org/stuff/notes_HammersleyOnHemisphere.html
//	idx	: iteration index
//	num	: number of iterations in total
inline float2 hammersley2d(uint idx, uint num) {
	uint bits = idx;
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555) << 1u) | ((bits & 0xAAAAAAAA) >> 1);
	bits = ((bits & 0x33333333) << 2u) | ((bits & 0xCCCCCCCC) >> 2);
	bits = ((bits & 0x0F0F0F0F) << 4u) | ((bits & 0xF0F0F0F0) >> 4);
	bits = ((bits & 0x00FF00FF) << 8u) | ((bits & 0xFF00FF00) >> 8);
	const float radicalInverse_VdC = float(bits) * 2.3283064365386963e-10; // / 0x100000000

	return float2(float(idx) / float(num), radicalInverse_VdC);
}

// Point on hemisphere with uniform distribution
//	u, v : in range [0, 1]
float3 hemispherepoint_uniform(float u, float v) {
	float phi = v * 2.0 * PI;
	float cosTheta = 1.0 - u;
	float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
	return float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
}

inline bool is_saturated(float a) { return a == saturate(a); }

inline float3 Decode( float2 f )
{
    f = f * 2.0 - 1.0;
 
    // https://twitter.com/Stubbesaurus/status/937994790553227264
    float3 n = float3( f.x, f.y, 1.0 - abs( f.x ) - abs( f.y ) );
    float t = saturate( -n.z );
    n.xy += n.xy >= 0.0 ? -t : t;
    return normalize( n );
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	float z = Depth.SampleLevel(Sampler, uv, 0);

	if (z == 1.0)
	{
		Output[threadID.xy] = float4(1,1,1,1);
		return;
	}
	
	float3 position = Position.SampleLevel(Sampler, uv, 0).xyz;
	float3 normal = Decode(Normal.SampleLevel(Sampler,uv,0).xy);

	float seed = Time;
	const float3 noiseRand = float3(rand(seed, uv), rand(seed, uv), rand(seed, uv)) * 2 - 1;
	
	const float3 tangent = normalize(noiseRand - normal * dot(noiseRand, normal));
	const float3 bitangent = cross(normal, tangent);
	const float3x3 tangentSpace = float3x3(tangent, bitangent, normal);
	
	float ao = 0;
	
	for (uint i = 0; i < SampleCount; ++i)
	{
		const float2 hamm = hammersley2d(i, SampleCount);
		const float3 hemisphere = hemispherepoint_uniform(hamm.x, hamm.y);
		const float3 cone = mul(hemisphere, tangentSpace);
		
		// modulate ray-length a bit to avoid uniform look
		const float ray_range = Range * lerp(0.2, 1.0, rand(seed, uv));
		const float3 sam = position + cone * ray_range;

		float4 vProjectedCoord = mul(float4(sam, 1.0), ViewProjection);
		vProjectedCoord.xyz /= vProjectedCoord.w;
		vProjectedCoord.xy = vProjectedCoord.xy * float2(0.5, -0.5) + float2(0.5, 0.5);
		
		if (is_saturated(vProjectedCoord.xy))
		{
			const float ray_depth_real = vProjectedCoord.z;
			float ray_depth_sample = Depth.SampleLevel(Sampler, vProjectedCoord.xy, 0);
			float depth_fix = 1.0 - saturate(abs(z - ray_depth_sample) * ScaleBias);
			ao += (ray_depth_sample < ray_depth_real  ? 1.0 : 0.0) * depth_fix;
		}
	}
	
	ao /= (float)SampleCount;
	
	Output[threadID.xy] = pow(saturate(1 - ao), Power);
}

[End_Pass]
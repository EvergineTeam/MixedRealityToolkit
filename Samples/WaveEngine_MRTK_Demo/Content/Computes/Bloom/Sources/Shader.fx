[Begin_ResourceLayout]
	[directives:DirtLens Dirt_Off Dirt_On]
	[directives:Rays Rays_Off Rays_On]
	[directives:LensFlare Flare_Off Flare_On]
	[directives:DebugMode None_Off Bloom Rays LensFlare]

cbuffer ParamsBuffer : register(b0)
{
	float ColorIntensity : packoffset(c0.x); [Default(1.0)]
	float BloomIntensity : packoffset(c0.y); [Default(0.8)]
	float DirtIntensity : packoffset(c0.z); [Default(0.5)]
	float RaysIntensity : packoffset(c0.w); [Default(1.0)]
	float LensFlareIntensity : packoffset(c1.x); [Default(2.0)]
}

cbuffer PerFrameBuffer : register(b1)
{	
	float4x4 View: packoffset(c0); [View];
	float4x4 Projection : packoffset(c4); [Projection]
	float3 lightDirection : packoffset(c8.x); [SunDirection]
}

Texture2D Input : register(t0);
Texture2D BloomTexture : register(t1);
Texture2D DirtTexture : register(t2);
Texture2D RaysTexture : register(t3);
Texture2D LensFlareTexture : register(t4);
Texture2D Starburst : register(t5);
RWTexture2D<float4> Output : register(u0); [Output(Input)]

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	
	float3 blur = BloomTexture.SampleLevel(Sampler, uv, 0).rgb;
	float3 color = Input.SampleLevel(Sampler, uv, 0).rgb;
	float3 rays = 0;
	float3 features = 0;
	
	color = blur * BloomIntensity + color  * ColorIntensity;
	
#if Dirt_On

	float3 dirt = DirtTexture.SampleLevel(Sampler, uv, 0).rgb;	
	color += blur * dirt * DirtIntensity;
	
#endif

#if Rays_On
	rays = RaysTexture.SampleLevel(Sampler, uv, 0).rgb;
	color += rays * RaysIntensity; 
#endif

#if Flare_On

	float4x4 unjitteredProjection = Projection;
	unjitteredProjection[2][0] = 0;
	unjitteredProjection[2][1] = 0;
	float2 sun = mul(mul(lightDirection, (float3x3)View), (float3x3)unjitteredProjection).xy * float2(0.5,-0.5) + 0.5;
	
	// starburst
	float2 centerVec = uv - sun;
	float d = length(centerVec);
	float radial = acos(centerVec.x / d);
	float mask = 
		  Starburst.SampleLevel(Sampler, float2(radial + 1 * 1.0, 0.0),0).r
		* Starburst.SampleLevel(Sampler, float2(radial - 1 * 0.5, 0.0),0).r
		;
	mask = saturate(mask + (1.0 - smoothstep(0.0, 0.3, d)));
	
	#if Dirt_On
		mask *= dirt.r * DirtIntensity;
	#endif
	
	features = LensFlareTexture.SampleLevel(Sampler, uv,0).rgb;
	color += features * mask * LensFlareIntensity;
	
#endif
	
#if Bloom
	Output[threadID.xy] = float4(blur, 1.0);
#elif Rays
	Output[threadID.xy] = float4(rays, 1.0);
#elif LensFlare
	Output[threadID.xy] = float4(features, 1.0);
#else
	Output[threadID.xy] = float4(color, 1.0);
#endif
}

[End_Pass]
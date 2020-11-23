[Begin_ResourceLayout]
[directives:Technique Linear Exponential ExponentialSquared]
[directives:Distance Distance_Off Distance_On]
[directives:Height Height_Off Height_On]

cbuffer ParamsBuffer : register(b0)
{
	float3 FogColor : packoffset(c0.x); [Default(0.5, 0.5, 0.5)]
	float Density : packoffset(c0.w); [Default(0.115)]
	float HeightDensity : packoffset(c1.x); [Default(2.0)]
	float Height : packoffset(c1.y); [Default(0.0)]
	float StartDistance : packoffset(c1.z); [Default(1.0)]
	float EndDistance : packoffset(c1.w); [Default(50.0)]
	float3 CameraWS : packoffset(c2.x); [CameraPosition]
}

Texture2D<float4> Input : register(t0);
Texture2D<float4> Position : register(t1);
RWTexture2D<float4> Output : register(u0);
SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

half ComputeFogFactor (float d)
{
	float fogFac = 0.0;
	
#if Linear
	fogFac = (EndDistance - d) / (EndDistance - StartDistance);
#elif Exponential
	fogFac = (Density / 2.0) * d;
	fogFac = exp2(-fogFac);
#elif ExponentialSquared
	fogFac = (Density / 2.0) * d;
	fogFac = exp2(-fogFac*fogFac);
#endif

	return saturate(fogFac);
}
	
// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
float ComputeHalfSpace (float3 wsDir)
{
	float3 wpos = CameraWS + wsDir;
	float FH = Height;
	float3 C = CameraWS;
	float3 V = wsDir;
	float3 P = wpos;
	float3 aV = (HeightDensity / 2.0) * V;
	float FdotC = CameraWS.y - Height;
	float k = FdotC <= 0.0 ? 1.0 : 0.0;
	float FdotP = P.y - FH;
	float FdotV = wsDir.y;
	float c1 = k * (FdotP + FdotC);
	float c2 = (1-(2 * k)) * FdotP;
	float g = min(c2, 0.0);
	g = -length(aV) * (c1 - g * g / abs(FdotV+1.0e-5f));
	return g;
}


[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;

	float3 color = Input.SampleLevel(Sampler, uv, 0).rgb;
	float3 positionWS = Position.SampleLevel(Sampler, uv, 0).rgb;
	
	float3 rayDir = positionWS - CameraWS;
	float d = 0.0;
	
#if Distance_On
	d += length(rayDir);
#endif

#if Height_On
	d += ComputeHalfSpace(rayDir);
#endif

	float fogAmount = ComputeFogFactor(max(0.0,d));

	float3 final = 0.0;

	final = lerp(FogColor, color, fogAmount);

	Output[threadID.xy] = float4(final, 1.0);
}

[End_Pass]
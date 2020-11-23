[Begin_ResourceLayout]

cbuffer Parmams : register(b0)
{
	uint cubemapSize 				: packoffset(c0.x);	// Size of the cubemap face in pixels at the current mipmap level.
	uint firstMip					: packoffset(c0.y);	// The first mip level to generate.
	uint numMipLevelsToGenerate		: packoffset(c0.z); // The number of mips to generate.
	uint totalNumMipLevels			: packoffset(c0.w);
};

TextureCube<float4> SourceTexture : register(t0);

RWTexture2DArray<float4> OutputMip1 : register(u0);
RWTexture2DArray<float4> OutputMip2 : register(u1);
RWTexture2DArray<float4> OutputMip3 : register(u2);
RWTexture2DArray<float4> OutputMip4 : register(u3);
RWTexture2DArray<float4> OutputMip5 : register(u4);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

static const float pi = 3.141592653589793238462643383279f;

// Transform from dispatch ID to cubemap face direction
static const float3x3 rotateUV[6] =
{
	// +X
	float3x3(0,  0,  1,
			 0, -1,  0,
			 -1,  0,  0),
			 // -X
			 float3x3(0,  0, -1,
					  0, -1,  0,
					  1,  0,  0),
					  // +Y
					  float3x3(1,  0,  0,
							   0,  0,  1,
							   0,  1,  0),
							   // -Y
							   float3x3(1,  0,  0,
										0,  0, -1,
										0, -1,  0),
										// +Z
										float3x3(1,  0,  0,
												 0, -1,  0,
												 0,  0,  1),
												 // -Z
												 float3x3(-1,  0,  0,
														  0,  -1,  0,
														  0,   0, -1)
};

// Normal Distribution function
float D_GGX(float dotNH, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float denom = dotNH * dotNH * (a2 - 1.0) + 1.0;
	return a2 / (pi * denom * denom);
}

static float distributionGGX(float3 N, float3 H, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.f);
	float NdotH2 = NdotH * NdotH;

	float nom = a2;
	float denom = (NdotH2 * (a2 - 1.f) + 1.f);
	denom = pi * denom * denom;

	return nom / max(denom, 0.001f);
}

static float radicalInverse_VdC(uint bits)
{
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

static float2 hammersley(uint i, uint N)
{
	return float2(float(i) / float(N), radicalInverse_VdC(i));
}

static float3 importanceSampleGGX(float2 Xi, float3 N, float roughness)
{
	float a = roughness * roughness;

	float phi = 2.f * pi * Xi.x;
	float cosTheta = sqrt((1.f - Xi.y) / (1.f + (a * a - 1.f) * Xi.y));
	float sinTheta = sqrt(1.f - cosTheta * cosTheta);

	// from spherical coordinates to cartesian coordinates
	float3 H;
	H.x = cos(phi) * sinTheta;
	H.y = sin(phi) * sinTheta;
	H.z = cosTheta;

	// from tangent-space vector to world-space sample vector
	float3 up = abs(N.z) < 0.999 ? float3(0.f, 0.f, 1.f) : float3(1.f, 0.f, 0.f);
	float3 tangent = normalize(cross(up, N));
	float3 bitangent = cross(N, tangent);

	float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
	return normalize(sampleVec);
}

static float4 preFilterEnvMap(uint mip, float3 R)
{
	float3 N = R;
	float3 V = R;

	float roughness = float(mip) / (totalNumMipLevels - 1);

	const uint SAMPLE_COUNT = 1024u;
	float totalWeight = 0.f;
	float4 prefilteredColor = float4(0, 0, 0, 0);


	uint width, height, numMipLevels;
	SourceTexture.GetDimensions(0, width, height, numMipLevels);

	float mapSize = width;
	float omegaP = 4.0 * pi / (6.0 * mapSize * mapSize);
	float mipBias = 1.0f; // Original paper suggest biasing the mip to improve the results


	for (uint i = 0u; i < SAMPLE_COUNT; ++i)
	{
		float2 Xi = hammersley(i, SAMPLE_COUNT);
		float3 H = importanceSampleGGX(Xi, N, roughness);
		float3 L = normalize(2.f * dot(V, H) * H - V);

		float NoL = max(dot(N, L), 0.f);
		if (NoL > 0.f)
		{
			if (roughness == 0.0)
			{
				prefilteredColor = float4(SourceTexture.SampleLevel(Sampler, L, 0).xyz * NoL, 0);
				break;
			}

			// optmize: https://placeholderart.wordpress.com/2015/07/28/implementation-notes-runtime-environment-map-filtering-for-image-based-lighting/
			float NoH = max(dot(N, H), 0.f);
			float VoH = max(dot(V, H), 0.f);
			float NoV = max(dot(N, V), 0.f);

			// Probability Distribution Function
			float pdf = D_GGX(NoH, roughness) * NoH / ((4.0f * VoH) + 0.0001) /*avoid division by 0*/;

			// Solid angle represented by this sample
			float omegaS = 1.0 / (float(SAMPLE_COUNT) * pdf);
			// Solid angle covered by 1 pixel with 6 faces that are EnvMapSize X EnvMapSize

			float mipLevel = max(0.5 * log2(omegaS / omegaP) + mipBias, 0.0f);

			prefilteredColor += float4(SourceTexture.SampleLevel(Sampler, L, mipLevel).rgb * NoL, NoL);
		}
	}

	if (prefilteredColor.w != 0.0)
	{
		prefilteredColor.rgb = prefilteredColor.rgb / prefilteredColor.w; // divide by the weight
	}

	return float4(prefilteredColor.rgb, 1);
}

[numthreads(16, 16, 1)]
void CS(uint3 dispatchThreadID : SV_DispatchThreadID, uint  groupIndex : SV_GroupIndex)
{
	// Cubemap texture coords.
	uint3 texCoord = dispatchThreadID;

	// First check if the thread is in the cubemap dimensions.
	if (texCoord.x >= cubemapSize || texCoord.y >= cubemapSize)
	{
		return;
	}

	// Map the UV coords of the cubemap face to a direction
	// [(0, 0), (1, 1)] => [(-0.5, -0.5), (0.5, 0.5)]

	float3 N = float3(texCoord.xy / float(cubemapSize) - 0.5f, 0.5f);
	N = normalize(mul(rotateUV[texCoord.z], N));
	N.x = -N.x;

	OutputMip1[texCoord] = preFilterEnvMap(firstMip, N);

	if (numMipLevelsToGenerate > 1 && (groupIndex & 0x11) == 0)
	{
		OutputMip2[uint3(texCoord.xy / 2, texCoord.z)] = preFilterEnvMap(firstMip + 1, N);
	}

	if (numMipLevelsToGenerate > 2 && (groupIndex & 0x33) == 0)
	{
		OutputMip3[uint3(texCoord.xy / 4, texCoord.z)] = preFilterEnvMap(firstMip + 2, N);
	}

	if (numMipLevelsToGenerate > 3 && (groupIndex & 0x77) == 0)
	{
		OutputMip4[uint3(texCoord.xy / 8, texCoord.z)] = preFilterEnvMap(firstMip + 3, N);
	}

	if (numMipLevelsToGenerate > 4 && (groupIndex & 0xFF) == 0)
	{
		OutputMip5[uint3(texCoord.xy / 16, texCoord.z)] = preFilterEnvMap(firstMip + 4, N);
	}
}

[End_Pass]
[Begin_ResourceLayout]

cbuffer Parameters : register(b0)
{
	uint IrradianceMapSize : packoffset(c0.x); // Size of the cubemap face in pixels.
};

TextureCube<float4> SourceTexture : register(t0);

RWTexture2DArray<float4> OutputIrradiance : register(u0);

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
	float3x3(0,  0,  -1,
			 0, -1,  0,
			 -1,  0,  0),
			 // -X
			 float3x3(0,  0, 1,
					  0, -1,  0,
					  1,  0,  0),
					  // +Y
					  float3x3(-1,  0,  0,
							   0,  0,  1,
							   0,  1,  0),
							   // -Y
							   float3x3(-1,  0,  0,
										0,  0, -1,
										0, -1,  0),
										// +Z
										float3x3(-1,  0,  0,
												 0, -1,  0,
												 0,  0,  1),
												 // -Z
												 float3x3(1,  0,  0,
														  0,  -1,  0,
														  0,   0, -1)
};


[numthreads(16, 16, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	// Cubemap texture coords.
	uint3 texCoord = threadID;

	// First check if the thread is in the cubemap dimensions.
	if (texCoord.x >= IrradianceMapSize || texCoord.y >= IrradianceMapSize)
	{
		return;
	}

	float3x3 rotateMatrix = rotateUV[texCoord.z];

	// Map the UV coords of the cubemap face to a direction.
	// [(0, 0), (1, 1)] => [(-0.5, -0.5), (0.5, 0.5)]
	float3 normal = normalize(float3(texCoord.xy / float(IrradianceMapSize) - 0.5f, 0.5f));

	float3 up = float3(0.f, 1.f, 0.f);
	float3 right = normalize(cross(up, normal));
	up = cross(normal, right);

	uint srcWidth, srcHeight, numMipLevels;
	SourceTexture.GetDimensions(0, srcWidth, srcHeight, numMipLevels);

	float sampleMipLevel = log2((float)srcWidth / (float)IrradianceMapSize);

	const float sampleDelta = 0.025f;
	float nrSamples = 0.f;

	float3 irradiance = float3(0.f, 0.f, 0.f);
	for (float phi = 0.f; phi < 2.f * pi; phi += sampleDelta)
	{
		float sinPhi = sin(phi);
		float cosPhi = cos(phi);

		for (float theta = 0.f; theta < 0.5f * pi; theta += sampleDelta)
		{
			float sinTheta = sin(theta);
			float cosTheta = cos(theta);

			// Spherical to cartesian (in tangent space).
			float3 sphereCoord = float3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta);

			// Tangent space to world.
			float3 sampleVec = sphereCoord.x * right + sphereCoord.y * up + sphereCoord.z * normal;
			sampleVec = mul(rotateMatrix, sampleVec);

			float4 color = SourceTexture.SampleLevel(Sampler, sampleVec, sampleMipLevel);
			irradiance += color.xyz * cosTheta * sinTheta;
			nrSamples++;
		}
	}

	irradiance = pi * irradiance * (1.f / float(nrSamples));

	OutputIrradiance[texCoord] = float4(irradiance, 1);
}

[End_Pass]
[Begin_ResourceLayout]

[directives:IBL IBL_OFF IBL]
[directives:ShadingModel MT_RG_OFF MT_RG_TEXTURED]
[directives:Albedo DIFF_OFF DIFF]
[directives:Emissive EMIS_OFF EMIS]
[directives:Normal NORMAL_OFF NORMAL]
[directives:AlphaTest ATEST_OFF ATEST]
[directives:Lighting LIT_OFF LIT]
[directives:DualTexture DUAL_OFF DUAL]
[directives:DualTextureType DUAL_LMAP_OFF DUAL_MUL DUAL_ADD DUAL_MSK]
[directives:ClearCoat CLEAR_OFF CLEAR CLEAR_NORMAL]
[directives:VertexColor VCOLOR_OFF VCOLOR]
[directives:AmbientOcclusion AO_OFF AO]
[directives:Multiview MULTIVIEW_OFF MULTIVIEW]
[directives:LowProfile LOW_PROFILE_OFF LOW_PROFILE]
[directives:ColorSpace GAMMA_COLORSPACE_OFF GAMMA_COLORSPACE]

struct LightProperties
{
	float3	Position;
	float	Falloff;
	float3	Color;
	float	Intensity;
	float3	Direction;
	uint	IESindex;
	float2	Scale;
	uint	LightType;
	float	Radius;
	float3	Left;
	uint	padding0;
};

cbuffer PerDrawCall : register(b0)
{
	float4x4 World                : packoffset(c0.x); [World]
	uint2    ForwardLightMask     : packoffset(c4.x); [ForwardLightMask]
};

cbuffer PerCamera : register(b1)
{
	float4x4  ViewProj					: packoffset(c0.x); [ViewProjection]
	float3    EyePosition				: packoffset(c4.x); [CameraPosition]
	int       EyeCount					: packoffset(c4.w); [MultiviewCount]
	float     EV100						: packoffset(c5.x); [EV100]
	float     Exposure					: packoffset(c5.y); [CameraExposure]
	uint      IblMaxMipLevel			: packoffset(c5.z); [IBLMipMapLevel]
	float     IblLuminance				: packoffset(c5.w); [IBLLuminance]
	float4x4  MultiviewViewProj[6]		: packoffset(c6.x); [MultiviewViewProjection]
	float4    MultiviewEyePosition[6]	: packoffset(c30.x); [MultiviewPosition]
};

cbuffer Parameters : register(b2)
{
	float3	BaseColor			: packoffset(c0.x); [Default(1, 1, 1)]
	float	Alpha				: packoffset(c0.w); [Default(1)]

	float	Metallic			: packoffset(c1.x);
	float	Roughness			: packoffset(c1.y);
	float	Reflectance			: packoffset(c1.z); [Default(0.5)]
	float	ReferenceAlpha		: packoffset(c1.w);

	float	ClearCoat			: packoffset(c2.x);
	float	ClearCoatRoughness	: packoffset(c2.y);

	float2	TextureOffset0		: packoffset(c2.z);
	float2	TextureOffset1		: packoffset(c3.x);

	float3	Emissive			: packoffset(c4.x); [Default(1, 1, 1)]
	float	EmissiveIntensity	: packoffset(c4.w); [Default(3)]
};

cbuffer LightBuffer : register(b3)
{
	uint LightBufferCount		: packoffset(c0.x); [LightCount]
	LightProperties Lights[64]	: packoffset(c1.x); [LightBuffer]
};

cbuffer IrradianceSHBuffer : register(b4)
{
	float4 IrradianceSH[9]		: packoffset(c0.x); [IrradianceSH]
};


Texture2D BaseColorTexture				: register(t0);
SamplerState BaseColorSampler			: register(s0);

Texture2D NormalTexture					: register(t1);
SamplerState NormalSampler				: register(s1);

Texture2D MetallicRoughnessTexture      : register(t2); // Green(y)=Roughness, Blue(z)=Metallic
SamplerState MetallicRoughnessSampler   : register(s2);

Texture2D BaseColorTexture2				: register(t3);
SamplerState BaseColorSampler2			: register(s3);

Texture2D OcclusionTexture				: register(t4); // Red channel 
SamplerState OcclusionSampler			: register(s4);

Texture2D EmissiveTexture				: register(t5);
SamplerState EmissiveSampler			: register(s5);

Texture2D ClearCoatNormalTexture		: register(t6);
SamplerState ClearCoatNormalSampler		: register(s6);

Texture2D IblDFGTexture					: register(t7); [DFGLut]
SamplerState IblDFGSampler				: register(s7);

TextureCube IBLRadianceTexture			: register(t8); [IBLRadiance]
SamplerState IBLRadianceSampler			: register(s8);

TextureCube IBLIrradianceTexture			: register(t9); [IBLIrradiance]
SamplerState IBLIrradianceSampler			: register(s9);

[End_ResourceLayout]

[Begin_Pass:ZPrePass]
[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

struct VSInputPbr
{
	float3      Position            : POSITION;
#if NORMAL || MT_RG_TEXTURED
	float2      TexCoord0           : TEXCOORD0;
#endif
	float3      Normal              : NORMAL;
#if NORMAL
	float4      Tangent             : TANGENT;
#endif
	uint        InstId              : SV_InstanceID;
};

struct VSOutputPbr
{
	float4 PositionProj : SV_POSITION;
	float3 NormalWS		: NORMAL;
#if NORMAL
	float3 TangentWS	: TANGENT;
	float3 BitangentWS	: BINORMAL;
#endif
#if NORMAL || MT_RG_TEXTURED
	float2 TexCoord0    : TEXCOORD0;
#endif
	
	#if MULTIVIEW
	uint ViewId         : SV_RenderTargetArrayIndex;
	#endif
};


VSOutputPbr VertexFunction(VSInputPbr input)
{
	VSOutputPbr output = (VSOutputPbr)0;

#if MULTIVIEW
	const int vid = input.InstId % EyeCount;
	const float4x4 viewProj = MultiviewViewProj[vid];

	// Note which view this vertex has been sent to. Used for matrix lookup.
	// Taking the modulo of the instance ID allows geometry instancing to be used
	// along with stereo instanced drawing; in that case, two copies of each 
	// instance would be drawn, one for left and one for right.

	output.ViewId = vid;
#else
	float4x4 viewProj = ViewProj;
#endif

	const float4 transformedPosWorld = mul(float4(input.Position, 1), World);

	output.PositionProj = mul(transformedPosWorld, viewProj);	
	
	output.NormalWS = mul(float4(input.Normal, 0), World).xyz;
	#if NORMAL
	output.TangentWS = normalize(mul(float4(input.Tangent.xyz, 0), World).xyz);
	output.BitangentWS = normalize(cross(output.NormalWS, output.TangentWS) * input.Tangent.w);
	#endif

#if NORMAL || MT_RG_TEXTURED
	output.TexCoord0 = input.TexCoord0 + TextureOffset0;
#endif

	return output;
}

float2 OctWrap(float2 v)
{
	return (1.0 - abs(v.yx)) * (v.xy >= 0.0 ? 1.0 : -1.0);
}

float2 Encode(float3 n)
{
	n /= (abs(n.x) + abs(n.y) + abs(n.z));
	n.xy = n.z >= 0.0 ? n.xy : OctWrap(n.xy);
	n.xy = n.xy * 0.5 + 0.5;
	return n.xy;
}

float4 PixelFunction(VSOutputPbr input) : SV_Target
{
	float3 normal = input.NormalWS;
#if NORMAL
	float3 normalTex = NormalTexture.Sample(NormalSampler, input.TexCoord0).rgb * 2 - 1;
	float3x3 tangentToWorld = float3x3(normalize(input.TangentWS), normalize(input.BitangentWS), normalize(input.NormalWS));
	normal = normalize(mul(normalTex, tangentToWorld));
#endif

	float roughness = Roughness;
	float metallic = Metallic;
#if MT_RG_TEXTURED
	float2 metallicAndRoughness = MetallicRoughnessTexture.Sample(MetallicRoughnessSampler, input.TexCoord0).yz;
	roughness = metallicAndRoughness.x;
	metallic = metallicAndRoughness.y;
#endif

	return float4(Encode(normal),roughness, metallic);
}

[End_Pass]


[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2
#define TUBE_LIGHT 3
#define RECTANGLE_LIGHT 4
#define DISK_LIGHT 5
#define SPHERE_LIGHT 6

static const float PI = 3.14159265;
static const float MinNoV = 1e-4;

#if LOW_PROFILE
static const float MinPerceptualRoughness = 0.089f;
static const float MinRoughness = 0.007921;
#else
static const float MinPerceptualRoughness = 0.045f;
static const float MinRoughness = 0.002025f;
#endif

struct VSInputPbr
{
	float3      Position            : POSITION;
#if DIFF || NORMAL || EMIS || MT_RG_TEXTURED || CLEAR_NORMAL || ANIS || AO
	float2      TexCoord0           : TEXCOORD0;
#endif

#if DUAL
	float2      TexCoord1           : TEXCOORD1;
#endif

#if LIT || IBL
	float3      Normal              : NORMAL;

#if NORMAL || CLEAR_NORMAL || ANIS
	float4      Tangent             : TANGENT;
#endif
#endif
#if VCOLOR
	float4 		Color 				: COLOR;
#endif

	uint        InstId              : SV_InstanceID;
};

struct VSOutputPbr
{
	float4 PositionProj : SV_POSITION;
	float3 PositionWorld : POSITION1;

#if LIT || IBL
	float3 NormalWS	: NORMAL;
#if NORMAL || CLEAR_NORMAL || ANIS
	float3 TangentWS : TANGENT;
	float3 BitangentWS: BINORMAL;
#endif    
#endif


#if DIFF || NORMAL || EMIS || MT_RG_TEXTURED || CLEAR_NORMAL || ANIS || AO
	float2 TexCoord0    : TEXCOORD0;
#endif

#if DUAL
	float2 TexCoord1    : TEXCOORD1;
#endif

#if VCOLOR
	float4 		Color 				: COLOR;
#endif

#if MULTIVIEW
	uint ViewId         : SV_RenderTargetArrayIndex;
#endif
};

float4 LinearToGamma(const float4 color)
{
	return float4(pow(abs(color.rgb), 1.0 / 2.2), color.a);
}

float4 GammaToLinear(const float4 color)
{
	return float4(pow(color.rgb, 2.2), color.a);
}

float3 LinearToGamma(const float3 color)
{
	return pow(color, 1 / 2.2);
}

float3 GammaToLinear(const float3 color)
{
	return pow(color, 2.2);
}

VSOutputPbr VertexFunction(VSInputPbr input)
{
	VSOutputPbr output = (VSOutputPbr)0;

#if MULTIVIEW
	const int vid = input.InstId % EyeCount;
	const float4x4 viewProj = MultiviewViewProj[vid];

	// Note which view this vertex has been sent to. Used for matrix lookup.
	// Taking the modulo of the instance ID allows geometry instancing to be used
	// along with stereo instanced drawing; in that case, two copies of each 
	// instance would be drawn, one for left and one for right.

	output.ViewId = vid;
#else
	float4x4 viewProj = ViewProj;
#endif

	const float4 transformedPosWorld = mul(float4(input.Position, 1), World);
	output.PositionProj = mul(transformedPosWorld, viewProj);
#if LIT || IBL
	output.PositionWorld = transformedPosWorld.xyz;
	output.NormalWS = normalize(mul(float4(input.Normal, 0), World).xyz);

#if NORMAL || CLEAR_NORMAL || ANIS
	output.TangentWS = normalize(mul(float4(input.Tangent.xyz, 0), World).xyz);
	output.BitangentWS = normalize(cross(output.NormalWS, output.TangentWS) * input.Tangent.w);
#endif
#endif

#if DIFF || NORMAL || EMIS || MT_RG_TEXTURED || CLEAR_NORMAL || ANIS || AO
	output.TexCoord0 = input.TexCoord0 + TextureOffset0;
#endif

#if DUAL
	output.TexCoord1 = input.TexCoord1 + TextureOffset1;
#endif

#if VCOLOR
	output.Color = GammaToLinear(input.Color);
#endif

	return output;
}

struct PixelParams
{
	float3  diffuseColor;
#if LIT || IBL    
	float perceptualRoughness;
	float3  f0;
	float roughness;
	float3  dfg;
	float3  energyCompensation;

	#if CLEAR || CLEAR_NORMAL
	float clearCoat;
	float clearCoatPerceptualRoughness;
	float clearCoatRoughness;
	#endif
#endif
};

struct SurfaceToLight
{
	float3 L;	// surface to light vector (normalized)
	float3 H;	// half-vector between view vector and light vector
	float NoL;	// cos angle between normal and light direction
	float NoH;	// cos angle between normal and half vector
	float LoH;	// cos angle between light direction and half vector
	float VoH;	// cos angle between view direction and half vector
};

struct MaterialInputs
{
	float4  baseColor;
#if LIT || IBL
	float3  normal;
	float roughness;
	float metallic;
	float reflectance;
	float ambientOcclusion;

	#if CLEAR || CLEAR_NORMAL
	float clearCoat;
	float clearCoatRoughness;
	float3 clearCoatNormal;
	#endif
#endif

#if EMIS	
	float4  emissive;
#endif
};

struct ShadingParams
{
	float3x3  tangentToWorld;	// TBN matrix
	float3  position;           // position of the fragment in world space
	float3  view;               // normalized vector from the fragment to the eye
	float3  normal;             // normalized normal, in world space
	float3  reflected;          // reflection of view about normal
	float NoV;                  // dot(normal, view), always strictly >= MIN_N_DOT_V

#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)
	float3	clearCoatNormal;	// normalized clear coat layer normal, in world space
#endif
};

float3 ComputeDiffuseColor(const float4 baseColor, float metallic)
{
	return baseColor.rgb * (1.0 - metallic);
}

float ComputeDielectricF0(float reflectance)
{
	return 0.16 * reflectance * reflectance;
}

float3 ComputeF0(const float4 baseColor, float metallic, float reflectance)
{
	return baseColor.rgb * metallic + (reflectance * (1.0 - metallic));
}

float PerceptualRoughnessToRoughness(float perceptualRoughness)
{
	return perceptualRoughness * perceptualRoughness;
}

float ClampNoV(float NoV)
{
	// Neubelt and Pettineo 2013, "Crafting a Next-gen Material Pipeline for The Order: 1886"
	return max(NoV, MinNoV);
}

float3 PrefilteredDFG_LUT(float lod, float NoV) {
	// coord = sqrt(linear_roughness), which is the mapping used by cmgen.   
	return IblDFGTexture.SampleLevel(IblDFGSampler, float2(NoV, lod), 0.0).rgb;
}

float3 PrefilteredDFG(float perceptualRoughness, float NoV) {
	// PrefilteredDFG_LUT() takes a LOD, which is sqrt(roughness) = perceptualRoughness
	return PrefilteredDFG_LUT(perceptualRoughness, NoV);
}

float ComputePreExposedIntensity(const float intensity, const float exposure) {
	return intensity * exposure;
}

void GetCommonPixelParams(const MaterialInputs material, inout PixelParams pixel)
{
#if LIT || IBL
	pixel.diffuseColor = ComputeDiffuseColor(material.baseColor, material.metallic);
	float reflectance = ComputeDielectricF0(material.reflectance);

	pixel.f0 = ComputeF0(material.baseColor, material.metallic, reflectance);
#else
	pixel.diffuseColor = material.baseColor.rgb;
#endif    
}

#if LIT || IBL

void GetClearCoatPixelParams(const MaterialInputs material, inout PixelParams pixel) {

#if CLEAR || CLEAR_NORMAL
	pixel.clearCoat = material.clearCoat;

	// Clamp the clear coat roughness to avoid divisions by 0
	float clearCoatPerceptualRoughness = material.clearCoatRoughness;
	clearCoatPerceptualRoughness = clamp(clearCoatPerceptualRoughness, MinPerceptualRoughness, 1.0);

	pixel.clearCoatPerceptualRoughness = clearCoatPerceptualRoughness;
	pixel.clearCoatRoughness = PerceptualRoughnessToRoughness(clearCoatPerceptualRoughness);
#endif
}

void GetRoughnessPixelParams(const MaterialInputs material, inout PixelParams pixel)
{
	float perceptualRoughness = material.roughness;

	// Clamp the roughness to a minimum value to avoid divisions by 0 during lighting
	perceptualRoughness = clamp(perceptualRoughness, MinPerceptualRoughness, 1.0);

#if (CLEAR || CLEAR_NORMAL)
// This is a hack but it will do: the base layer must be at least as rough
// as the clear coat layer to take into account possible diffusion by the
// top layer
	float basePerceptualRoughness = max(perceptualRoughness, pixel.clearCoatPerceptualRoughness);
	perceptualRoughness = lerp(perceptualRoughness, basePerceptualRoughness, pixel.clearCoat);
#endif

	// Remaps the roughness to a perceptually linear roughness (roughness^2)
	pixel.perceptualRoughness = perceptualRoughness;
	pixel.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
}
#endif

#if LIT || IBL
void GetEnergyCompensationPixelParams(const ShadingParams shading, inout PixelParams pixel)
{
	// Pre-filtered DFG term used for image-based lighting
	pixel.dfg = PrefilteredDFG(pixel.perceptualRoughness, shading.NoV);
	pixel.energyCompensation = 1.0 + pixel.f0 * (1.0 / pixel.dfg.y - 1.0);
}
#endif

void GetPixelParams(const ShadingParams shading, const MaterialInputs material, out PixelParams pixel)
{
	pixel = (PixelParams)0;
	GetCommonPixelParams(material, pixel);
#if LIT || IBL
	GetClearCoatPixelParams(material, pixel);
	GetRoughnessPixelParams(material, pixel);
	GetEnergyCompensationPixelParams(shading, pixel);
#endif
}

void InitMaterial(const VSOutputPbr input, inout MaterialInputs material)
{
	// Base Color
	material.baseColor = float4(BaseColor, Alpha);
#if DIFF
	float4 baseColorTexture = BaseColorTexture.Sample(BaseColorSampler, input.TexCoord0);
	baseColorTexture.rgb = GammaToLinear(baseColorTexture.rgb);
	material.baseColor *= baseColorTexture;
#endif

#if VCOLOR
	material.baseColor *= input.Color;
#endif

#if ATEST
	if (material.baseColor.a <= ReferenceAlpha)
	{
		discard;
	}
#endif    

#if LIT || IBL
#if NORMAL
	material.normal = NormalTexture.Sample(NormalSampler, input.TexCoord0).rgb * 2 - 1;
#else
	material.normal = float3(0, 0, 1);
#endif

	// Metallic & Roughness values
	material.metallic = Metallic;
	material.roughness = Roughness;
	material.reflectance = Reflectance;

#if MT_RG_TEXTURED
	float4 metalRoughness = MetallicRoughnessTexture.Sample(MetallicRoughnessSampler, input.TexCoord0);
	float  metallicFromTexture = metalRoughness.b;
	float  roughnessFromTexture = metalRoughness.g;
	material.metallic = metallicFromTexture;
	material.roughness = roughnessFromTexture;
#endif    

	// Ambient occlusion
	material.ambientOcclusion = 1.0f;
#if AO
	material.ambientOcclusion = OcclusionTexture.Sample(OcclusionSampler, input.TexCoord0).r;
#endif

#if CLEAR || CLEAR_NORMAL
	material.clearCoat = ClearCoat;
	material.clearCoatRoughness = ClearCoatRoughness;
#if CLEAR_NORMAL
	material.clearCoatNormal = ClearCoatNormalTexture.Sample(ClearCoatNormalSampler, input.TexCoord0).rgb * 2 - 1;
#else
	material.clearCoatNormal = float3(0, 0, 1);
#endif
#endif
#endif

#if EMIS
	material.emissive.rgb = Emissive;
	material.emissive.rgb *= EmissiveTexture.Sample(EmissiveSampler, input.TexCoord0).rgb;
	material.emissive.w = EmissiveIntensity;
#endif
}

void ComputeShadingParams(const VSOutputPbr input, inout ShadingParams shading)
{
#if MULTIVIEW
	int iid = input.ViewId / EyeCount;
	int vid = input.ViewId % EyeCount;
	float3 cameraPosition = MultiviewEyePosition[vid].xyz;
#else
	float3 cameraPosition = EyePosition;
#endif

#if LIT || IBL
	// Normal
#if NORMAL || CLEAR_NORMAL || ANIS
	shading.tangentToWorld = float3x3(
		normalize(input.TangentWS),
		normalize(input.BitangentWS),
		normalize(input.NormalWS));
#else
	shading.tangentToWorld[2] = normalize(input.NormalWS);
#endif    
#endif

	shading.position = input.PositionWorld;
	shading.view = normalize(cameraPosition - input.PositionWorld);
}

void PrepareMaterial(const MaterialInputs material, inout ShadingParams shading)
{
#if LIT || IBL
#if NORMAL || CLEAR_NORMAL || ANIS
	shading.normal = normalize(mul(material.normal, shading.tangentToWorld));

#if CLEAR || CLEAR_NORMAL
	shading.clearCoatNormal = normalize(mul(material.clearCoatNormal, shading.tangentToWorld));
#endif
#else
	shading.normal = shading.tangentToWorld[2];
#if CLEAR || CLEAR_NORMAL
	shading.clearCoatNormal = shading.normal;
#endif
#endif

	shading.NoV = ClampNoV(dot(shading.normal, shading.view));
	shading.reflected = reflect(-shading.view, shading.normal);
#endif
}

// Lighting section
#if LIT || IBL

float ComputeSpecularAO(float NoV, float visibility, float roughness) {
#if AO && MT_RG_TEXTURED
	return saturate(pow(NoV + visibility, exp2(-16.0 * roughness - 1.0)) - 1.0 + visibility);
#else
	return 1.0;
#endif
}

float3 SpecularDFG(const PixelParams pixel)
{
	return lerp(pixel.dfg.xxx, pixel.dfg.yyy, pixel.f0);
}

float3 GetSpecularDominantDirection(const float3 n, const float3 r, float roughness)
{
	return lerp(r, n, roughness * roughness);
}

float3 GetReflectedVector(const ShadingParams shading, const PixelParams pixel, const float3 n)
{
	float3 r = shading.reflected;
	return GetSpecularDominantDirection(n, r, pixel.roughness);
}

float3 decodeDataForIBL(const float4 data)
{
	return data.rgb;
}

float3 PrefilteredRadiance(const float3 r, float perceptualRoughness)
{
	// lod = lod_count * sqrt(roughness), which is the mapping used by cmgen
	// where roughness = perceptualRoughness^2
	// using all the mip levels requires seamless cubemap sampling
	float lod = IblMaxMipLevel * perceptualRoughness;
	return IBLRadianceTexture.SampleLevel(IBLRadianceSampler, r, lod).rgb;
}

float3 GtaoMultiBounce(float visibility, const float3 albedo)
{
	// Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion"
	float3 a = 2.0404 * albedo - 0.3324;
	float3 b = -4.7951 * albedo + 0.6417;
	float3 c = 2.7552 * albedo + 0.6903;

	return max(float3(visibility, visibility, visibility), ((visibility * a + b) * visibility + c) * visibility);
}

float SingleBounceAO(float visibility)
{
#if AO
	return 1.0;
#else
	return visibility;
#endif
}

void MultiBounceAO(float visibility, const float3 albedo, inout float3 color)
{
#if AO
	color *= GtaoMultiBounce(visibility, albedo);
#endif
}

void MultiBounceSpecularAO(float visibility, const float3 albedo, inout float3 color)
{
#if AO && MT_RG_TEXTURED
	color *= GtaoMultiBounce(visibility, albedo);
#endif
}

float3 Irradiance_SphericalHarmonics(const float3 n)
{
	return max(
		IrradianceSH[0].rgb
		+ IrradianceSH[1].rgb * (n.y)
		+ IrradianceSH[2].rgb * (n.z)
		+ IrradianceSH[3].rgb * (n.x)
		+ IrradianceSH[4].rgb * (n.y * n.x)
		+ IrradianceSH[5].rgb * (n.y * n.z)
		+ IrradianceSH[6].rgb * (3.0 * n.z * n.z - 1.0)
		+ IrradianceSH[7].rgb * (n.z * n.x)
		+ IrradianceSH[8].rgb * (n.x * n.x - n.y * n.y)
		, 0.0);
}

float3 Irradiance_Cubemap(const float3 n)
{
	return IBLIrradianceTexture.Sample(IBLIrradianceSampler, n).rgb;
}

float3 DiffuseIrradiance(const float3 n)
{
	#if IBL
	return Irradiance_Cubemap(n);
	#endif
}

float Pow5(float x)
{
	float x2 = x * x;
	return x2 * x2 * x;
}

float F_Schlick(const float f0, float f90, float VoH)
{
	// Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"
	return f0 + (f90 - f0) * Pow5(1.0 - VoH);
}

void EvaluateClearCoatIBL(const ShadingParams shading, const PixelParams pixel, float specularAO, inout float3 Fd, inout float3 Fr)
{
#if CLEAR || CLEAR_NORMAL  
	// We want to use the geometric normal for the clear coat layer
	float clearCoatNoV = ClampNoV(dot(shading.clearCoatNormal, shading.view));
	float3 clearCoatR = reflect(-shading.view, shading.clearCoatNormal);

	// The clear coat layer assumes an IOR of 1.5 (4% reflectance)
	float Fc = F_Schlick(0.04, 1.0, clearCoatNoV) * pixel.clearCoat;
	float attenuation = 1.0 - Fc;
	Fd *= attenuation;
	Fr *= attenuation;
	Fr += PrefilteredRadiance(clearCoatR, pixel.clearCoatPerceptualRoughness) * (specularAO * Fc);
#endif
}

#if IBL
void EvaluateIBL(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, inout float3 color)
{
	float3 n = shading.normal;
	float diffuseAO = material.ambientOcclusion;
	float specularAO = ComputeSpecularAO(shading.NoV, diffuseAO, pixel.roughness);

	// specular layer
	float3 Fr;

	float3 E = SpecularDFG(pixel);
	float3 r = GetReflectedVector(shading, pixel, n);
	Fr = E * PrefilteredRadiance(r, pixel.perceptualRoughness);

	Fr *= SingleBounceAO(specularAO) * pixel.energyCompensation;

	// diffuse layer
	float diffuseBRDF = SingleBounceAO(diffuseAO); // Fd_Lambert() is baked in the SH below

	float3 diffuseIrradiance = DiffuseIrradiance(n);
	float3 Fd = pixel.diffuseColor * diffuseIrradiance * (1.0 - E) * diffuseBRDF;

	// clear coat layer
	EvaluateClearCoatIBL(shading, pixel, specularAO, Fd, Fr);

	// extra ambient occlusion term
	MultiBounceAO(diffuseAO, pixel.diffuseColor, Fd);
	MultiBounceSpecularAO(specularAO, pixel.f0, Fr);

	// Note: iblLuminance is already premultiplied by the exposure
	color.rgb += (Fd + Fr) * IblLuminance;
}
#endif  

float D_GGX(float3 normal, float roughness, float NoH, const float3 h)
{
	// Walter et al. 2007, "Microfacet Models for Refraction through Rough Surfaces"

	// In mediump, there are two problems computing 1.0 - NoH^2
	// 1) 1.0 - NoH^2 suffers floating point cancellation when NoH^2 is close to 1 (highlights)
	// 2) NoH doesn't have enough precision around 1.0
	// Both problem can be fixed by computing 1-NoH^2 in highp and providing NoH in highp as well

	// However, we can do better using Lagrange's identity:
	//      ||a x b||^2 = ||a||^2 ||b||^2 - (a . b)^2
	// since N and H are unit vectors: ||N x H||^2 = 1.0 - NoH^2
	// This computes 1.0 - NoH^2 directly (which is close to zero in the highlights and has
	// enough precision).
	// Overall this yields better performance, keeping all computations in mediump
#if LOW_PROFILE
	float3 NxH = cross(normal, h);
	float oneMinusNoHSquared = dot(NxH, NxH);
#else
	float oneMinusNoHSquared = 1.0 - NoH * NoH;
#endif

	float a = NoH * roughness;
	float k = roughness / (oneMinusNoHSquared + a * a);
	float d = k * k * (1.0 / PI);
	return d;
}

float V_SmithGGXCorrelated(float roughness, float NoV, float NoL)
{
	// Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"
	float a2 = roughness * roughness;

	// TODO: lambdaV can be pre-computed for all the lights, it should be moved out of this function
	float lambdaV = NoL * sqrt((NoV - a2 * NoV) * NoV + a2);
	float lambdaL = NoV * sqrt((NoL - a2 * NoL) * NoL + a2);
	float v = 0.5 / (lambdaV + lambdaL);
	// a2=0 => v = 1 / 4*NoL*NoV   => min=1/4, max=+inf
	// a2=1 => v = 1 / 2*(NoL+NoV) => min=1/4, max=+inf
	// clamp to the maximum value representable in mediump
	return v;
}

float V_SmithGGXCorrelated_Fast(float roughness, float NoV, float NoL)
{
	// Hammon 2017, "PBR Diffuse Lighting for GGX+Smith Microsurfaces"
	float v = 0.5 / lerp(2.0 * NoL * NoV, NoL + NoV, roughness);
	return v;
}

float3 F_Schlick(const float3 f0, float VoH)
{
	float f = pow(1.0 - VoH, 5.0);
	return f + f0 * (1.0 - f);
}

float3 F_Schlick(const float3 f0, float f90, float VoH)
{
	// Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"
	return f0 + (f90 - f0) * Pow5(1.0 - VoH);
}

float Distribution(float3 normal, float roughness, float NoH, const float3 h)
{
	return D_GGX(normal, roughness, NoH, h);
}

float Visibility(float roughness, float NoV, float NoL)
{
#if LOW_PROFILE
	return V_SmithGGXCorrelated_Fast(roughness, NoV, NoL);
#else
	return V_SmithGGXCorrelated(roughness, NoV, NoL);
#endif
}

float3 Fresnel(const float3 f0, float LoH)
{
#if LOW_PROFILE
	return F_Schlick(f0, LoH); // f90 = 1.0
#else
	const float v = 50.0 * 0.33;
	float f90 = saturate(dot(f0, v.xxx));
	return F_Schlick(f0, f90, LoH);
#endif
}

inline SurfaceToLight CreateSurfaceToLight(in PixelParams pixel, in ShadingParams shading, in float3 L)
{
	SurfaceToLight surfaceToLight;

	float3 H = normalize(shading.view + L);
	surfaceToLight.L = L;
	surfaceToLight.H = H;

	surfaceToLight.NoL = saturate(dot(shading.normal, L));
	surfaceToLight.NoH = saturate(dot(shading.normal, H));
	surfaceToLight.LoH = saturate(dot(L, H));
	surfaceToLight.VoH = saturate(dot(shading.view, H));

	return surfaceToLight;
}

inline SurfaceToLight CreateSurfaceToLight(in PixelParams pixel, in ShadingParams shading, in float3 L, in float NoL)
{
	SurfaceToLight surfaceToLight;

	float3 H = normalize(shading.view + L);
	surfaceToLight.L = L;
	surfaceToLight.H = H;

	surfaceToLight.NoL = NoL;
	surfaceToLight.NoH = saturate(dot(shading.normal, H));
	surfaceToLight.LoH = saturate(dot(L, H));
	surfaceToLight.VoH = saturate(dot(shading.view, H));

	return surfaceToLight;
}

float3 IsotropicLobe(const ShadingParams shading, const PixelParams pixel, const float3 h, float NoV, float NoL, float NoH, float LoH)
{
	float D = Distribution(shading.normal, pixel.roughness, NoH, h);
	float V = Visibility(pixel.roughness, NoV, NoL);
	float3  F = Fresnel(pixel.f0, LoH);

	return (D * V) * F;
}

float3 SpecularLobe(const ShadingParams shading, const PixelParams pixel, const SurfaceToLight surfaceToLight)
{
	return IsotropicLobe(shading, pixel, surfaceToLight.H, shading.NoV, surfaceToLight.NoL, surfaceToLight.NoH, surfaceToLight.LoH);
}

float Fd_Lambert()
{
	return 1.0 / PI;
}

float Fd_Burley(float roughness, float NoV, float NoL, float LoH)
{
	// Burley 2012, "Physically-Based Shading at Disney"
	float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
	float lightScatter = F_Schlick(1.0, f90, NoL);
	float viewScatter = F_Schlick(1.0, f90, NoV);
	return lightScatter * viewScatter * (1.0 / PI);
}

float3 DiffuseLobe(const PixelParams pixel)
{
	return pixel.diffuseColor * Fd_Lambert();
}

// Clear coat section
#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)

float DistributionClearCoat(float3 normal, float roughness, float NoH, const float3 h)
{
	return D_GGX(normal, roughness, NoH, h);
}

float V_Kelemen(float LoH)
{
	// Kelemen 2001, "A Microfacet Based Coupled Specular-Matte BRDF Model with Importance Sampling"
	return 0.25 / (LoH * LoH);
}

float VisibilityClearCoat(float LoH)
{
	return V_Kelemen(LoH);
}

float ClearCoatLobe(const ShadingParams shading, const PixelParams pixel, const SurfaceToLight light, out float Fcc)
{
#if CLEAR_NORMAL
	// If the material has a normal map, we want to use the geometric normal
	// instead to avoid applying the normal map details to the clear coat layer
	float clearCoatNoH = saturate(dot(shading.clearCoatNormal, light.H));
#else
	float clearCoatNoH = light.NoH;
#endif

	// clear coat specular lobe
	float D = DistributionClearCoat(shading.normal, pixel.clearCoatRoughness, clearCoatNoH, light.H);
	float V = VisibilityClearCoat(light.LoH);
	float F = F_Schlick(0.04, 1.0, light.LoH) * pixel.clearCoat; // fix IOR to 1.5

	Fcc = F;
	return D * V * F;
}

// End clear coat section
#endif 

#if LIT
float3 SurfaceShading(const ShadingParams shading, const PixelParams pixel, const SurfaceToLight surfaceToLight, float3 lightColor)
{
	float3 Fd = DiffuseLobe(pixel);
	float3 Fr = SpecularLobe(shading, pixel, surfaceToLight);

	// TODO: attenuate the diffuse lobe to avoid energy gain

#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)
	float Fcc;
	float clearCoat = ClearCoatLobe(shading, pixel, surfaceToLight, Fcc);
	// Energy compensation and absorption; the clear coat Fresnel term is
	// squared to take into account both entering through and exiting through
	// the clear coat layer
	float attenuation = 1.0 - Fcc;

#if CLEAR_NORMAL
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation * surfaceToLight.NoL;

	// If the material has a normal map, we want to use the geometric normal
	// instead to avoid applying the normal map details to the clear coat layer
	float clearCoatNoL = saturate(dot(shading.clearCoatNormal, surfaceToLight.L));
	color += clearCoat * clearCoatNoL;

	// Early exit to avoid the extra multiplication by NoL
	return color * lightColor;
#else
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation + clearCoat;
#endif
#else
	// The energy compensation term is used to counteract the darkening effect
	// at high roughness
	float3 color = Fd + Fr * pixel.energyCompensation;
#endif

	return color * surfaceToLight.NoL * lightColor;
}

float3 SurfaceShadingAreaLight(const ShadingParams shading, const PixelParams pixel, const SurfaceToLight surfaceToLight, float3 lightColor, float specularAttenuation)
{
	float3 Fd = DiffuseLobe(pixel);
	float3 Fr = SpecularLobe(shading, pixel, surfaceToLight) * specularAttenuation * surfaceToLight.NoL;

#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)
	float Fcc;
	float clearCoat = ClearCoatLobe(shading, pixel, surfaceToLight, Fcc);
	// Energy compensation and absorption; the clear coat Fresnel term is
	// squared to take into account both entering through and exiting through
	// the clear coat layer
	float attenuation = 1.0 - Fcc;

#if CLEAR_NORMAL
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation;

	// If the material has a normal map, we want to use the geometric normal
	// instead to avoid applying the normal map details to the clear coat layer
	float clearCoatNoL = saturate(dot(shading.clearCoatNormal, surfaceToLight.L));
	color += clearCoat * clearCoatNoL;

	// Early exit to avoid the extra multiplication by NoL
	return color * lightColor;
#else
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation + clearCoat;
#endif
#else
	// The energy compensation term is used to counteract the darkening effect
	// at high roughness
	float3 color = Fd + Fr * pixel.energyCompensation;
#endif

	return color * lightColor;
}


float GetSquareFalloffAttenuation(float distanceSquare, float falloff)
{
	float factor = distanceSquare * falloff;
	float smoothFactor = saturate(1.0 - factor * factor);
	// We would normally divide by the square distance here
	// but we do it at the call site
	return smoothFactor * smoothFactor;
}

float GetDistanceAttenuation(const float3 posToLight, float falloff)
{
	float distanceSquare = dot(posToLight, posToLight);
	float attenuation = GetSquareFalloffAttenuation(distanceSquare, falloff);
	// Assume a punctual light occupies a volume of 1cm to avoid a division by 0
	return attenuation * 1.0 / max(distanceSquare, 1e-4);
}

float GetDistanceRadiusAttenuation(const float3 posToLight, float falloff, float sqrRadius)
{
	float distanceSquare = dot(posToLight, posToLight) - sqrRadius;
	float attenuation = GetSquareFalloffAttenuation(distanceSquare, falloff);
	// Assume a punctual light occupies a volume of 1cm to avoid a division by 0
	return attenuation * 1.0 / max(distanceSquare, 1e-4);
}

float3 ClosestPointOnLine(float3 a, float3 b, float3 c)
{
	float3 ab = b - a;
	float t = dot(c - a, ab) / dot(ab, ab);
	return a + t * ab;
}

float3 ClosestPointOnSegment(float3 a, float3 b, float3 c)
{
	float3 ab = b - a;
	float t = dot(c - a, ab) / dot(ab, ab);
	return a + saturate(t) * ab;
}

float RectangleSolidAngle(float3 worldPos,
	float3 p0, float3 p1,
	float3 p2, float3 p3)
{
	float3 v0 = p0 - worldPos;
	float3 v1 = p1 - worldPos;
	float3 v2 = p2 - worldPos;
	float3 v3 = p3 - worldPos;

	float3 n0 = normalize(cross(v0, v1));
	float3 n1 = normalize(cross(v1, v2));
	float3 n2 = normalize(cross(v2, v3));
	float3 n3 = normalize(cross(v3, v0));


	float g0 = acos(dot(-n0, n1));
	float g1 = acos(dot(-n1, n2));
	float g2 = acos(dot(-n2, n3));
	float g3 = acos(dot(-n3, n0));

	return g0 + g1 + g2 + g3 - 2 * PI;
}

float illuminanceSphereOrDisk(float cosTheta, float sinSigmaSqr)
{
	float sinTheta = sqrt(1.0f - cosTheta * cosTheta);

	float illuminance = 0.0f;
	// Note: Following test is equivalent to the original formula. 
	// There is 3 phase in the curve: cosTheta > sqrt(sinSigmaSqr), 
	// cosTheta > -sqrt(sinSigmaSqr) and else it is 0 
	// The two outer case can be merge into a cosTheta * cosTheta > sinSigmaSqr 
	// and using saturate(cosTheta) instead. 
	if (cosTheta * cosTheta > sinSigmaSqr)
	{
		illuminance = PI * sinSigmaSqr * saturate(cosTheta);
	}
	else
	{
		float x = sqrt(1.0f / sinSigmaSqr - 1.0f); // For a disk this simplify to x = d / r 
		float y = -x * (cosTheta / sinTheta);
		float sinThetaSqrtY = sinTheta * sqrt(1.0f - y * y);
		illuminance = (cosTheta * acos(y) - x * sinThetaSqrtY) * sinSigmaSqr + atan(sinThetaSqrtY / x);
	}

	return max(illuminance, 0.0f);
}

float TracePlane(float3 o, float3 d, float3 planeOrigin, float3 planeNormal)
{
	return dot(planeNormal, (planeOrigin - o) / dot(planeNormal, d));
}

float TraceTriangle(float3 o, float3 d, float3 A, float3 B, float3 C)
{
	float3 planeNormal = normalize(cross(B - A, C - B));
	float t = TracePlane(o, d, A, planeNormal);
	float3 p = o + d * t;

	float3 N1 = normalize(cross(B - A, p - B));
	float3 N2 = normalize(cross(C - B, p - C));
	float3 N3 = normalize(cross(A - C, p - A));

	float d0 = dot(N1, N2);
	float d1 = dot(N2, N3);

	float threshold = 1.0f - 0.001f;
	return (d0 > threshold && d1 > threshold) ? 1.0f : 0.0f;
}

float TraceRectangle(float3 o, float3 d, float3 A, float3 B, float3 C, float3 D)
{
	return max(TraceTriangle(o, d, A, B, C), TraceTriangle(o, d, C, D, A));
}

inline float3 GetSpecularDominantDirArea(float3 N, float3 R, float roughness)
{
	// Simple linear approximation 
	float lerpFactor = (1 - roughness);

	return normalize(lerp(N, R, lerpFactor));
}

void PointLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 worldPosition = shading.position;
	float3 posToLight = lightProperties.Position - worldPosition;
	float3 L = normalize(posToLight);
	float attenuation = GetDistanceAttenuation(posToLight, lightProperties.Falloff);
	float NoL = saturate(dot(shading.normal, L));

	[branch]
	if (NoL * attenuation > 0)
	{
		float3 lightColor = lightProperties.Color * ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * attenuation;
		SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L, NoL);
		color += SurfaceShading(shading, pixel, surfaceToLight, lightColor);
	}
}

float GetAngleAttenuation(const float3 lightDir, const float3 l, const float2 scaleOffset)
{
	float cd = dot(lightDir, l);
	float attenuation = saturate(cd * scaleOffset.x + scaleOffset.y);
	return attenuation * attenuation;
}

void SpotLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 worldPosition = shading.position;
	float3 posToLight = lightProperties.Position - worldPosition;
	float3 L = normalize(posToLight);
	float attenuation = GetDistanceAttenuation(posToLight, lightProperties.Falloff);
	attenuation *= GetAngleAttenuation(-lightProperties.Direction, L, lightProperties.Scale);
	float NoL = saturate(dot(shading.normal, L));

	[branch]
	if (NoL * attenuation > 0)
	{
		float3 lightColor = lightProperties.Color * ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * attenuation;
		SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L, NoL);
		color += SurfaceShading(shading, pixel, surfaceToLight, lightColor);
	}
}

void DirectionalLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 L = lightProperties.Direction;
	float NoL = saturate(dot(shading.normal, L));

	[branch]
	if (NoL > 0)
	{
		float3 lightColor = lightProperties.Color * ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion;
		SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L, NoL);
		color += SurfaceShading(shading, pixel, surfaceToLight, lightColor);
	}
}

void TubeLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 lightLeft = lightProperties.Left;
	float lightWidth = lightProperties.Scale.x * 0.5;
	float lightRadius = lightProperties.Radius;
	float3 lightPosition = lightProperties.Position;

	float3 P0 = lightPosition - lightLeft * lightWidth;
	float3 P1 = lightPosition + lightLeft * lightWidth;

	float3 forward = normalize(ClosestPointOnLine(P0, P1, shading.position) - shading.position);
	float3 lightUp = cross(lightLeft, forward);

	float3 p0 = lightPosition - lightLeft * lightWidth + lightRadius * lightUp;
	float3 p1 = lightPosition - lightLeft * lightWidth - lightRadius * lightUp;
	float3 p2 = lightPosition + lightLeft * lightWidth - lightRadius * lightUp;
	float3 p3 = lightPosition + lightLeft * lightWidth + lightRadius * lightUp;

	float solidAngle = RectangleSolidAngle(shading.position, p0, p1, p2, p3);

	float fLight = solidAngle * 0.2 * (
		saturate(dot(normalize(p0 - shading.position), shading.normal)) +
		saturate(dot(normalize(p1 - shading.position), shading.normal)) +
		saturate(dot(normalize(p2 - shading.position), shading.normal)) +
		saturate(dot(normalize(p3 - shading.position), shading.normal)) +
		saturate(dot(normalize(lightPosition - shading.position), shading.normal)));

	float3 spherePosition = ClosestPointOnSegment(P0, P1, shading.position);
	float3 sphereUnormL = spherePosition - shading.position;
	float3 sphereL = normalize(sphereUnormL);
	float sqrSphereDistance = dot(sphereUnormL, sphereUnormL);

	float fLightSphere = PI * saturate(dot(sphereL, shading.normal)) * ((lightRadius * lightRadius) / sqrSphereDistance);
	fLight += fLightSphere;
	fLight *= GetSquareFalloffAttenuation(sqrSphereDistance, lightProperties.Falloff);

	[branch]
	if (fLight > 0)
	{
		float3 r = shading.reflected;
		r = GetSpecularDominantDirArea(shading.normal, r, material.roughness);

		// First, the closest point to the ray on the segment
		float3 L0 = P0 - shading.position;
		float3 L1 = P1 - shading.position;
		float3 Ld = L1 - L0;
		float rdotLdSqr = dot(r, Ld);
		float t = dot(r, L0) * rdotLdSqr - dot(L0, Ld);
		t /= dot(Ld, Ld) - rdotLdSqr * rdotLdSqr;

		float3 L = (L0 + saturate(t) * Ld);

		// Then I place a sphere on that point and calculate the lisght vector like for sphere light.
		float3 centerToRay = dot(L, r) * r - L;
		float3 closestPoint = L + centerToRay * saturate(lightRadius / length(centerToRay));
		L = normalize(closestPoint);

		SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L);

		float3 lightColor = lightProperties.Color;
		lightColor *= ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * fLight;

		color += SurfaceShadingAreaLight(shading, pixel, surfaceToLight, lightColor, 1);
	}
}

void RectangleLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 lunormalized = lightProperties.Position - shading.position;

	float halfwidth = lightProperties.Scale.x * 0.5;
	float halfheight = lightProperties.Scale.y * 0.5;
	float3 lightPlaneNormal = lightProperties.Direction;
	float3 lightLeft = lightProperties.Left;
	float3 lightUp = cross(lightLeft, lightPlaneNormal);
	float3 lightPosition = lightProperties.Position;

	float3 p0 = lightPosition + lightLeft * -halfwidth + lightUp * halfheight;
	float3 p1 = lightPosition + lightLeft * -halfwidth + lightUp * -halfheight;
	float3 p2 = lightPosition + lightLeft * halfwidth + lightUp * -halfheight;
	float3 p3 = lightPosition + lightLeft * halfwidth + lightUp * halfheight;

	float solidAngle = RectangleSolidAngle(shading.position, p0, p1, p2, p3);

	if (dot(lightPlaneNormal, shading.position - lightPosition) < 0)
	{
		float fLight = solidAngle * 0.2 * (
			saturate(dot(normalize(p0 - shading.position), shading.normal)) +
			saturate(dot(normalize(p1 - shading.position), shading.normal)) +
			saturate(dot(normalize(p2 - shading.position), shading.normal)) +
			saturate(dot(normalize(p3 - shading.position), shading.normal)) +
			saturate(dot(normalize(lightPosition - shading.position), shading.normal)));

		float sqrDist = dot(lunormalized, lunormalized);
		fLight *= GetSquareFalloffAttenuation(sqrDist, lightProperties.Falloff);

		float3 r = shading.reflected;
		r = GetSpecularDominantDirArea(shading.normal, r, material.roughness);
		float specularAttenuation = saturate(abs(dot(lightPlaneNormal, r))); // if ray is perpendicular to light plane, it would break specular, so fade in that case

		float3 L;

		// We approximate L by the closest point on the reflection ray to the light source (representative point technique) to achieve a nice looking specular reflection
		[branch]
		if ((specularAttenuation * fLight) > 0)
		{
			float traced = TraceRectangle(shading.position, r, p0, p1, p2, p3);
			[branch]
			if (traced > 0)
			{
				// Trace succeeded so the light vector L is the reflection vector itself
				L = r;
			}
			else
			{
				// The trace didn't succeed, so we need to find the closest point to the ray on the rectangle

				// We find the intersection point on the plane of the rectangle
				float3 tracedPlane = shading.position + r * TracePlane(shading.position, r, lightProperties.Position, lightPlaneNormal);

				// Then find the closest point along the edges of the rectangle (edge = segment)
				float3 PC[4] = {
					ClosestPointOnSegment(p0, p1, tracedPlane),
					ClosestPointOnSegment(p1, p2, tracedPlane),
					ClosestPointOnSegment(p2, p3, tracedPlane),
					ClosestPointOnSegment(p3, p0, tracedPlane),
				};
				float dist[4] = {
					distance(PC[0], tracedPlane),
					distance(PC[1], tracedPlane),
					distance(PC[2], tracedPlane),
					distance(PC[3], tracedPlane),
				};

				float3 min = PC[0];
				float minDist = dist[0];
				[unroll]
				for (uint iLoop = 1; iLoop < 4; iLoop++)
				{
					if (dist[iLoop] < minDist)
					{
						minDist = dist[iLoop];
						min = PC[iLoop];
					}
				}

				L = min - shading.position;
				L = normalize(L); // TODO: Is it necessary?
			}

			SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L);

			float3 lightColor = lightProperties.Color;
			lightColor *= ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * fLight;

			color += SurfaceShadingAreaLight(shading, pixel, surfaceToLight, lightColor, specularAttenuation);
		}
	}
}

void DiskLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 Lunnormalized = lightProperties.Position - shading.position;
	float sqrDist = dot(Lunnormalized, Lunnormalized);
	float3 L = normalize(Lunnormalized);
	float radius = lightProperties.Radius;

	float cosTheta = clamp(dot(shading.normal, L), -0.999, 0.999);
	float sqrLightRadius = radius * radius;
	float sinSigmaSqr = min(sqrLightRadius / sqrDist, 0.9999);
	float fLight = illuminanceSphereOrDisk(cosTheta, sinSigmaSqr) * saturate(dot(lightProperties.Direction, L));
	fLight *= GetSquareFalloffAttenuation(sqrDist, lightProperties.Falloff);

	[branch]
	if (fLight > 0)
	{
		float3 r = shading.reflected;
		r = GetSpecularDominantDirArea(shading.normal, r, material.roughness);

		float specularAttenuation = saturate(abs(dot(lightProperties.Direction, r)));

		[branch]
		if (specularAttenuation > 0)
		{
			float t = TracePlane(shading.position, r, lightProperties.Position, lightProperties.Direction);
			float3 p = shading.position + r * t;
			float3 centerToRay = p - lightProperties.Position;
			float3 closestPoint = Lunnormalized + centerToRay * saturate(radius / length(centerToRay));
			L = normalize(closestPoint);
			SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L);

			float3 lightColor = lightProperties.Color;
			lightColor *= ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * fLight;

			color += SurfaceShadingAreaLight(shading, pixel, surfaceToLight, lightColor, specularAttenuation);
		}
	}
}

void SphereLight(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, const LightProperties lightProperties, inout float3 color)
{
	float3 Lunnormalized = lightProperties.Position - shading.position;
	float sqrDist = dot(Lunnormalized, Lunnormalized);
	float3 L = normalize(Lunnormalized);
	float radius = lightProperties.Radius;

	float cosTheta = clamp(dot(shading.normal, L), -0.999, 0.999);
	float sqrLightRadius = radius * radius;
	float sinSigmaSqr = min(sqrLightRadius / sqrDist, 0.9999);
	float fLight = illuminanceSphereOrDisk(cosTheta, sinSigmaSqr);
	fLight *= GetSquareFalloffAttenuation(sqrDist, lightProperties.Falloff);

	if (fLight > 0)
	{
		float3 r = shading.reflected;
		r = GetSpecularDominantDirArea(shading.normal, r, material.roughness);

		float3 centerToRay = dot(Lunnormalized, r) * r - Lunnormalized;
		float3 closestPoint = Lunnormalized + centerToRay * saturate(radius / length(centerToRay));
		L = normalize(closestPoint);


		SurfaceToLight surfaceToLight = CreateSurfaceToLight(pixel, shading, L);

		float3 lightColor = lightProperties.Color;
		lightColor *= ComputePreExposedIntensity(lightProperties.Intensity, Exposure) * material.ambientOcclusion * fLight;

		color += SurfaceShadingAreaLight(shading, pixel, surfaceToLight, lightColor, 1);
	}
}

uint findIndexLSB(uint value)
{
	// http://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightMultLookup
	const uint MultiplyDeBruijnBitPosition[32] =
	{
	  0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
	  31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
	};
	return MultiplyDeBruijnBitPosition[((uint)((value & -value) * 0x077CB531U)) >> 27];
}

void EvaluateDirectLights(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, inout float3 color)
{
	[branch]
	if (any(ForwardLightMask))
	{
		// Loop through light buckets for the draw call:
		const uint first_item = 0;
		const uint last_item = first_item + LightBufferCount - 1;
		const uint first_bucket = first_item / 32;
		const uint last_bucket = min(last_item / 32, 1); // only 2 buckets max (uint2) for forward pass!
		[loop]
		for (uint bucket = first_bucket; bucket <= last_bucket; ++bucket)
		{
			uint bucket_bits = ForwardLightMask[bucket];
			[loop]
			while (bucket_bits != 0)
			{
#if LOW_PROFILE
				const uint bucket_bit_index = findIndexLSB(bucket_bits);
#else
				const uint bucket_bit_index = firstbitlow(bucket_bits);
#endif
				const uint light_index = bucket * 32 + bucket_bit_index;
				bucket_bits ^= 1 << bucket_bit_index;

				LightProperties lightProperties = Lights[light_index];

				[branch]
				switch (lightProperties.LightType)
				{
				case DIRECTIONAL_LIGHT:
				{
					DirectionalLight(shading, material, pixel, lightProperties, color);
					break;
				}
				case POINT_LIGHT:
				{
					PointLight(shading, material, pixel, lightProperties, color);
					break;
				}

				case SPOT_LIGHT:
				{
					SpotLight(shading, material, pixel, lightProperties, color);
					break;
				}

				case TUBE_LIGHT:
				{
					TubeLight(shading, material, pixel, lightProperties, color);
					break;
				}

				case RECTANGLE_LIGHT:
				{
					RectangleLight(shading, material, pixel, lightProperties, color);
					break;
				}

				case DISK_LIGHT:
				{
					DiskLight(shading, material, pixel, lightProperties, color);
					break;
				}

				case SPHERE_LIGHT:
				{
					SphereLight(shading, material, pixel, lightProperties, color);
					break;
				}
				}
			}
		}
	}
}
#endif

float4 EvaluateLights(const ShadingParams shading, const MaterialInputs material)
{
	PixelParams pixel;
	GetPixelParams(shading, material, pixel);

	// Ideally we would keep the diffuse and specular components separate
	// until the very end but it costs more ALUs on mobile. The gains are
	// currently not worth the extra operations
	float3 color = float3(0, 0, 0);

	// We always evaluate the IBL as not having one is going to be uncommon,
	// it also saves 1 shader variant
#if IBL    
	EvaluateIBL(shading, material, pixel, color);
#endif

#if LIT
	EvaluateDirectLights(shading, material, pixel, color);
#endif

	return float4(color, material.baseColor.a);
}

// End Lighting section
#endif

#if EMIS
void AddEmissive(const MaterialInputs material, inout float4 color)
{
	// The Emissive property applies independently of the shading model
	// It is defined as a color + exposure compensation
	float4 Emissive = material.emissive;
	float attenuation = Exposure * ComputePreExposedIntensity(pow(2.0, EV100 + Emissive.w - 3.0), Exposure);
	color.rgb += Emissive.rgb * attenuation;
}
#endif

float4 EvaluateMaterial(const ShadingParams shading, const MaterialInputs material)
{
	float4 color;
#if LIT || IBL
	color = EvaluateLights(shading, material);
#if EMIS
	AddEmissive(material, color);
#endif
#else
	color = material.baseColor;
#if EMIS
	color.rgb += material.emissive.rgb;
#endif
#endif    
	return color;
}

float4 PixelFunction(VSOutputPbr input) : SV_Target
{
	ShadingParams shading = (ShadingParams)0;	
	ComputeShadingParams(input, shading);	

	MaterialInputs material = (MaterialInputs)0;
	InitMaterial(input, material);

	PrepareMaterial(material, shading);

	float4 color = EvaluateMaterial(shading, material);
	color.rgb *= Alpha;

#if GAMMA_COLORSPACE
	color = LinearToGamma(color);
#endif

	return color;
}
[End_Pass]
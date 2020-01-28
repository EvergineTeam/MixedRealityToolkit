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
[directives:Stereo STEREO_OFF STEREO]

struct LightProperties 
{
	float3 	Position;
	float	Falloff;
	float3 	Color;
	float	Intensity;
	float3 	Direction;
	uint	IESindex;
	float2 	ScaleOffset;
	uint 	LightType;
	uint 	Padding;
};

cbuffer PerDrawCall : register(b0)
{
	float4x4     World                : packoffset(c0.x); [World]
	uint2        ForwardLightMask     : packoffset(c4.x); [ForwardLightMask]
};

cbuffer PerCamera : register(b1)
{
	float4x4  ViewProj[2]		: packoffset(c0.x); [StereoCameraViewProjection]
	float3    EyePosition[2]    : packoffset(c8.x); [StereoCameraPosition]
	int       EyeCount        	: packoffset(c10.x); [StereoEyeCount]
	float     EV100            	: packoffset(c10.y); [EV100]
	float     Exposure        	: packoffset(c10.z); [Exposure]
	uint      IblMaxMipLevel   	: packoffset(c10.w); [IBLMipMapLevel]    
	float     IblLuminance		: packoffset(c11.x); [IBLLuminance]    
};

cbuffer Parameters : register(b2)
{
	float3   BaseColor				: packoffset(c0.x); [Default(1, 1, 1)]
	float    Alpha					: packoffset(c0.w); [Default(1)]
	
	float    Metallic				: packoffset(c1.x);
	float    Roughness				: packoffset(c1.y);
	float    Reflectance			: packoffset(c1.z); [Default(0.5)]
	float    ReferenceAlpha			: packoffset(c1.w);
	
	float     ClearCoat				: packoffset(c2.x);
	float     ClearCoatRoughness	: packoffset(c2.y);   

	float2    TextureOffset0		: packoffset(c2.z);
	float2    TextureOffset1		: packoffset(c3.x);

	float3   Emissive				: packoffset(c4.x);	[Default(1, 1, 1)]
	float    EmissiveIntensity		: packoffset(c4.w);	[Default(3)]
};

cbuffer LightBuffer : register(b3)
{
	uint LightBufferCount			: packoffset(c0.x); [LightCount]
	LightProperties Lights[64]		: packoffset(c1.x); [LightBuffer]
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

TextureCube IBLTexture					: register(t8); [IBLRadiance]
SamplerState IBLSampler					: register(s8);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VertexFunction PS = PixelFunction]

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

static const float PI = 3.14159265f;
static const float MinPerceptualRoughness = 0.089f;
static const float MinRoughness = 0.007921;
static const float MinNoV = 1e-4;

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

#if STEREO
	uint ViewId         : SV_RenderTargetArrayIndex;
#endif
};

VSOutputPbr VertexFunction(VSInputPbr input)
{
	VSOutputPbr output = (VSOutputPbr)0;    
	const int iid = input.InstId / EyeCount;
	const int vid = input.InstId % EyeCount;
	
	const float4x4 viewProj = ViewProj[vid];

	float4x4 worldViewProj = mul(World, viewProj);

	// Note which view this vertex has been sent to. Used for matrix lookup.
	// Taking the modulo of the instance ID allows geometry instancing to be used
	// along with stereo instanced drawing; in that case, two copies of each 
	// instance would be drawn, one for left and one for right.
#if STEREO
	output.ViewId = vid;
#else
	
#endif

	const float4 transformedPosWorld = mul(float4(input.Position, 1), World);
	output.PositionProj = mul(transformedPosWorld, viewProj);
#if LIT || IBL
	output.PositionWorld = transformedPosWorld.xyz;
	output.NormalWS = normalize(mul(float4(input.Normal, 0), World).xyz);
	
	#if NORMAL || CLEAR_NORMAL || ANIS
	output.TangentWS = normalize(mul(float4(input.Tangent.xyz, 0), World).xyz);
	output.BitangentWS = normalize(mul(float4(cross(input.Normal, input.Tangent.xyz) * input.Tangent.w, 0), World).xyz);
	#endif
#endif

#if DIFF || NORMAL || EMIS || MT_RG_TEXTURED || CLEAR_NORMAL || ANIS || AO
	output.TexCoord0 = input.TexCoord0 + TextureOffset0;
#endif

#if DUAL
	output.TexCoord1 = input.TexCoord1 + TextureOffset1;
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

//#ifdef ANISOTROPY_ENABLED
//    float3  anisotropicT;
//    float3  anisotropicB;
//    float anisotropy;
//#endif
#endif
};

struct Light 
{
	float4 colorIntensity;  // rgb, pre-exposed intensity
	float3 l;
	float attenuation;
	float NoL;
};

struct MaterialInputs 
{
	float4  baseColor;
#if LIT || IBL
	float3  normal;
	float roughness;
	float metallic;
	float reflectance;
	//float3 specularColor;
	//float glossiness;
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

//#ifdef ANISOTROPY_ENABLED
//    float anisotropy;
//    #ifdef HAS_ANISOTROPY_TEXTURE
//    float3  anisotropyDirection;
//    #endif    
//#endif
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

float4 LinearToSrgb(const float4 color)
{
	return float4(pow(color.rgb, 1 / 2.2), color.a);
}

float4 SrgbToLinear(const float4 color)
{
	return float4(pow(color.rgb, 2.2), color.a);
}

float3 LinearToSrgb(const float3 color)
{
	return pow(color, 1 / 2.2);
}

float3 SrgbToLinear(const float3 color)
{
	return pow(color, 2.2);
}

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
	pixel.diffuseColor  = material.baseColor.rgb;
#endif    
}

#if LIT || IBL

void GetClearCoatPixelParams(const MaterialInputs material, inout PixelParams pixel) {

#if CLEAR || CLEAR_NORMAL
	pixel.clearCoat = material.clearCoat;

	// Clamp the clear coat roughness to avoid divisions by 0
	float clearCoatPerceptualRoughness = material.clearCoatRoughness;
	clearCoatPerceptualRoughness = clamp(clearCoatPerceptualRoughness, MinPerceptualRoughness, 1.0);

//#if defined(GEOMETRIC_SPECULAR_AA)
//    clearCoatPerceptualRoughness =
//            normalFiltering(clearCoatPerceptualRoughness, getWorldGeometricNormalVector());
//#endif

	pixel.clearCoatPerceptualRoughness = clearCoatPerceptualRoughness;
	pixel.clearCoatRoughness = PerceptualRoughnessToRoughness(clearCoatPerceptualRoughness);

////#if defined(CLEAR_COAT_IOR_CHANGE)
////    // The base layer's f0 is computed assuming an interface from air to an IOR
////    // of 1.5, but the clear coat layer forms an interface from IOR 1.5 to IOR
////    // 1.5. We recompute f0 by first computing its IOR, then reconverting to f0
////    // by using the correct interface
////    pixel.f0 = mix(pixel.f0, f0ClearCoatToSurface(pixel.f0), pixel.clearCoat);
////#endif
#endif
}

void GetRoughnessPixelParams(const MaterialInputs material, inout PixelParams pixel) 
{
	float perceptualRoughness = material.roughness;

	// Clamp the roughness to a minimum value to avoid divisions by 0 during lighting
	perceptualRoughness = clamp(perceptualRoughness, MinPerceptualRoughness, 1.0);

//#if defined(GEOMETRIC_SPECULAR_AA)
//    perceptualRoughness = normalFiltering(perceptualRoughness, getWorldGeometricNormalVector());
//#endif

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

//void getAnisotropyPixelParams(const VSOutputPbr input, const MaterialInputs material, inout PixelParams pixel) 
//{
//#ifdef ANISOTROPY_ENABLED
//    float3 direction = material.anisotropyDirection;
//    pixel.anisotropy = material.anisotropy;
//    pixel.anisotropicT = normalize((input.TBN * direction);
//    pixel.anisotropicB = normalize(cross(getWorldGeometricNormalVector(), pixel.anisotropicT));
//#endif
//}

#if LIT || IBL
void GetEnergyCompensationPixelParams(const ShadingParams shading, inout PixelParams pixel) 
{	
	// Pre-filtered DFG term used for image-based lighting
	pixel.dfg = PrefilteredDFG(pixel.perceptualRoughness, shading.NoV);

//#if !defined(SHADING_MODEL_CLOTH)
	// Energy compensation for multiple scattering in a microfacet model
	// See "Multiple-Scattering Microfacet BSDFs with the Smith Model"
	pixel.energyCompensation = 1.0 + pixel.f0 * (1.0 / pixel.dfg.y - 1.0);
//#else
	//pixel.energyCompensation = float3(1.0, 1.0, 1.0);
//#endif
}
#endif

void GetPixelParams(const ShadingParams shading, const MaterialInputs material, out PixelParams pixel)
{
	pixel = (PixelParams)0;
	GetCommonPixelParams(material, pixel);
	#if LIT || IBL
	GetClearCoatPixelParams(material, pixel);    	
	GetRoughnessPixelParams(material, pixel);	
	//GetSubsurfacePixelParams(material, pixel);
	//GetAnisotropyPixelParams(material, pixel);
	GetEnergyCompensationPixelParams(shading, pixel);    
	#endif
}

void InitMaterial(const VSOutputPbr input, inout MaterialInputs material)
{
	// Base Color
	material.baseColor = float4(BaseColor, Alpha);
#if DIFF
	float4 baseColorTexture = BaseColorTexture.Sample(BaseColorSampler, input.TexCoord0);
	baseColorTexture.rgb = SrgbToLinear(baseColorTexture.rgb);
	material.baseColor *= baseColorTexture;
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
	material.normal = float3(0,0,1);
	#endif

	// Metallic & Roughness values
	material.metallic = Metallic;
	material.roughness = Roughness;    
	material.reflectance = Reflectance;
	
	#if MT_RG_TEXTURED
	float4 metalRoughness = MetallicRoughnessTexture.Sample(MetallicRoughnessSampler, input.TexCoord0);
	float  metallicFromTexture = metalRoughness.b;
	float  roughnessFromTexture  = metalRoughness.g;
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
		material.clearCoatNormal = float3(0,0,1);
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
#if STEREO
	int iid = input.ViewId / EyeCount;
	int vid = input.ViewId % EyeCount;
	float3 cameraPosition = EyePosition[vid];
#else
	float3 cameraPosition = EyePosition[0];
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

//#if defined(SHADING_MODEL_CLOTH)
//    return pixel.f0 * pixel.dfg.z;
//#else
	  return lerp(pixel.dfg.xxx, pixel.dfg.yyy, pixel.f0);
//#endif
}

float3 GetSpecularDominantDirection(const float3 n, const float3 r, float roughness) 
{
	return lerp(r, n, roughness * roughness);
}

float3 GetReflectedVector(const ShadingParams shading, const PixelParams pixel, const float3 n) 
{
//#if defined(MATERIAL_HAS_ANISOTROPY)
//    vec3 r = getReflectedVector(pixel, shading_view, n);
//#else
	float3 r = shading.reflected;
//#endif
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
	return IBLTexture.SampleLevel(IBLSampler, r, lod).rgb;
}

float3 GtaoMultiBounce(float visibility, const float3 albedo) 
{
	// Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion"
	float3 a =  2.0404 * albedo - 0.3324;
	float3 b = -4.7951 * albedo + 0.6417;
	float3 c =  2.7552 * albedo + 0.6903;

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

float3 DiffuseIrradiance(const float3 n) 
{
	return Irradiance_SphericalHarmonics(n);
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
//#if IBL_INTEGRATION == IBL_INTEGRATION_IMPORTANCE_SAMPLING
//    isEvaluateClearCoatIBL(pixel, specularAO, Fd, Fr);
//    return;
//#endif

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
	//float ssao = float(1);
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
	//evaluateClothIndirectDiffuseBRDF(pixel, diffuseBRDF);
	
	float3 diffuseIrradiance = DiffuseIrradiance(n);
	float3 Fd = pixel.diffuseColor * diffuseIrradiance * (1.0 - E) * diffuseBRDF;
	
	// clear coat layer
	EvaluateClearCoatIBL(shading, pixel, specularAO, Fd, Fr);

	// subsurface layer
	//evaluateSubsurfaceIBL(pixel, diffuseIrradiance, Fd, Fr);
	
	// extra ambient occlusion term
	MultiBounceAO(diffuseAO, pixel.diffuseColor, Fd);
	MultiBounceSpecularAO(specularAO, pixel.f0, Fr);

	// Note: iblLuminance is already premultiplied by the exposure
	color.rgb += (Fd + Fr) * IblLuminance;	  
}
#endif  

float D_GGX(float roughness, float NoH, const float3 h) 
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
//#if defined(TARGET_MOBILE)
//    vec3 NxH = cross(shading_normal, h);
//    float oneMinusNoHSquared = dot(NxH, NxH);
//#else
	float oneMinusNoHSquared = 1.0 - NoH * NoH;
//#endif

	float a = NoH * roughness;
	float k = roughness / (oneMinusNoHSquared + a * a);
	float d = k * k * (1.0 / PI);
	return d;
}

float V_SmithGGXCorrelated_Fast(float roughness, float NoV, float NoL) 
{
	// Hammon 2017, "PBR Diffuse Lighting for GGX+Smith Microsurfaces"
	float v = 0.5 / lerp(2.0 * NoL * NoV, NoL + NoV, roughness);
	return v;
}

float3 F_Schlick(const float3 f0, float f90, float VoH) 
{
	// Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"
	return f0 + (f90 - f0) * Pow5(1.0 - VoH);
}

float Distribution(float roughness, float NoH, const float3 h) 
{
	return D_GGX(roughness, NoH, h);
}

float Visibility(float roughness, float NoV, float NoL) 
{
//#if BRDF_SPECULAR_V == SPECULAR_V_SMITH_GGX
//    return V_SmithGGXCorrelated(roughness, NoV, NoL);
//#elif BRDF_SPECULAR_V == SPECULAR_V_SMITH_GGX_FAST
	return V_SmithGGXCorrelated_Fast(roughness, NoV, NoL);
//#endif
}

float3 Fresnel(const float3 f0, float LoH) 
{
//#if BRDF_SPECULAR_F == SPECULAR_F_SCHLICK
//#if defined(TARGET_MOBILE)
//    return F_Schlick(f0, LoH); // f90 = 1.0
//#else
	const float v = 50.0 * 0.33;
	float f90 = saturate(dot(f0, v.xxx));
	return F_Schlick(f0, f90, LoH);
//#endif
//#endif
}

float3 IsotropicLobe(const PixelParams pixel, const Light light, const float3 h, float NoV, float NoL, float NoH, float LoH) 
{
	float D = Distribution(pixel.roughness, NoH, h);
	float V = Visibility(pixel.roughness, NoV, NoL);
	float3  F = Fresnel(pixel.f0, LoH);

	return (D * V) * F;
}

float3 SpecularLobe(const PixelParams pixel, const Light light, const float3 h, float NoV, float NoL, float NoH, float LoH) 
{
//#if defined(MATERIAL_HAS_ANISOTROPY)
//    return anisotropicLobe(pixel, light, h, NoV, NoL, NoH, LoH);
//#else
	return IsotropicLobe(pixel, light, h, NoV, NoL, NoH, LoH);
//#endif
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
	float viewScatter  = F_Schlick(1.0, f90, NoV);
	return lightScatter * viewScatter * (1.0 / PI);
}

float diffuse(float roughness, float NoV, float NoL, float LoH) 
{
//#if BRDF_ALBEDO == ALBEDO_LAMBERT
//    return Fd_Lambert();
//#elif BRDF_ALBEDO == ALBEDO_BURLEY
	return Fd_Burley(roughness, NoV, NoL, LoH);
//#endif
}

float3 DiffuseLobe(const PixelParams pixel, float NoV, float NoL, float LoH) 
{
	return pixel.diffuseColor * diffuse(pixel.roughness, NoV, NoL, LoH);
}

// Clear coat section
#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)

float DistributionClearCoat(float roughness, float NoH, const float3 h) 
{
//#if BRDF_CLEAR_COAT_D == SPECULAR_D_GGX
	return D_GGX(roughness, NoH, h);
//#endif
}

float V_Kelemen(float LoH) 
{
	// Kelemen 2001, "A Microfacet Based Coupled Specular-Matte BRDF Model with Importance Sampling"
	return 0.25 / (LoH * LoH);
}

float VisibilityClearCoat(float LoH) 
{
//#if BRDF_CLEAR_COAT_V == SPECULAR_V_KELEMEN
	return V_Kelemen(LoH);
//#endif
}

float ClearCoatLobe(const ShadingParams shading, const PixelParams pixel, const float3 h, float NoH, float LoH, out float Fcc) 
{
#if CLEAR_NORMAL
	// If the material has a normal map, we want to use the geometric normal
	// instead to avoid applying the normal map details to the clear coat layer
	float clearCoatNoH = saturate(dot(shading.clearCoatNormal, h));
#else
	float clearCoatNoH = NoH;
#endif

	// clear coat specular lobe
	float D = DistributionClearCoat(pixel.clearCoatRoughness, clearCoatNoH, h);
	float V = VisibilityClearCoat(LoH);
	float F = F_Schlick(0.04, 1.0, LoH) * pixel.clearCoat; // fix IOR to 1.5

	Fcc = F;
	return D * V * F;
}

// End clear coat section
#endif 

#if LIT
float3 SurfaceShading(const ShadingParams shading, const PixelParams pixel, const Light light, float occlusion)
{
	float3 h = normalize(shading.view + light.l);

	float NoV = shading.NoV;
	float NoL = saturate(light.NoL);
	float NoH = saturate(dot(shading.normal, h));
	float LoH = saturate(dot(light.l, h));

	float3 Fr = SpecularLobe(pixel, light, h, NoV, NoL, NoH, LoH);
	float3 Fd = DiffuseLobe(pixel, NoV, NoL, LoH);
	
	// TODO: attenuate the diffuse lobe to avoid energy gain

#if (CLEAR || CLEAR_NORMAL) && (LIT || IBL)
	float Fcc;
	float clearCoat = ClearCoatLobe(shading, pixel, h, NoH, LoH, Fcc);
	// Energy compensation and absorption; the clear coat Fresnel term is
	// squared to take into account both entering through and exiting through
	// the clear coat layer
	float attenuation = 1.0 - Fcc;

#if CLEAR_NORMAL
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation * NoL;

	// If the material has a normal map, we want to use the geometric normal
	// instead to avoid applying the normal map details to the clear coat layer
	float clearCoatNoL = saturate(dot(shading.clearCoatNormal, light.l));
	color += clearCoat * clearCoatNoL;

	// Early exit to avoid the extra multiplication by NoL
	return (color * light.colorIntensity.rgb) *
			(light.colorIntensity.w * light.attenuation * occlusion);
#else
	float3 color = (Fd + Fr * pixel.energyCompensation) * attenuation + clearCoat;
#endif
#else
	// The energy compensation term is used to counteract the darkening effect
	// at high roughness
	float3 color = Fd + Fr * pixel.energyCompensation;
#endif

	return (color * light.colorIntensity.rgb) *
			(light.colorIntensity.w * light.attenuation * NoL * occlusion);

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

void SetupPunctualLight(const ShadingParams shading, inout Light light, const float3 position, const float falloff) 
{
	float3 worldPosition = shading.position;
	float3 posToLight = position - worldPosition;
	light.l = normalize(posToLight);
	light.attenuation = GetDistanceAttenuation(posToLight, falloff);
	light.NoL = saturate(dot(shading.normal, light.l));
}

Light GetPointLight(const ShadingParams shading, const LightProperties lightProperties)
{
	Light light;
	
	light.colorIntensity.rgb = lightProperties.Color;
	light.colorIntensity.w = ComputePreExposedIntensity(lightProperties.Intensity, Exposure);
	SetupPunctualLight(shading, light, lightProperties.Position, lightProperties.Falloff);
	
	return light;
}

float GetAngleAttenuation(const float3 lightDir, const float3 l, const float2 scaleOffset) 
{
    float cd = dot(lightDir, l);
    float attenuation  = saturate(cd * scaleOffset.x + scaleOffset.y);
    return attenuation * attenuation;
}

Light GetSpotLight(const ShadingParams shading, const LightProperties lightProperties)
{
	Light light;
	
	light.colorIntensity.rgb = lightProperties.Color;
	light.colorIntensity.w = ComputePreExposedIntensity(lightProperties.Intensity, Exposure);
	
	SetupPunctualLight(shading, light, lightProperties.Position, lightProperties.Falloff);
	
	light.attenuation *= GetAngleAttenuation(-lightProperties.Direction, light.l, lightProperties.ScaleOffset);
	
	return light;
}

float3 SampleSunAreaLight(const float3 lightDirection) 
{
//#if defined(SUN_AS_AREA_LIGHT)
//    if (frameUniforms.sun.w >= 0.0) {
//        // simulate sun as disc area light
//        float LoR = dot(lightDirection, shading_reflected);
//        float d = frameUniforms.sun.x;
//        highp vec3 s = shading_reflected - LoR * lightDirection;
//        return LoR < d ?
//                normalize(lightDirection * d + normalize(s) * frameUniforms.sun.y) : shading_reflected;
//    }
//#endif
	return lightDirection;
}

Light GetDirectionalLight(const ShadingParams shading, const LightProperties lightProperties)
{
	Light light;
	
	light.colorIntensity = float4(lightProperties.Color, lightProperties.Intensity);
	light.l = SampleSunAreaLight(lightProperties.Direction);
	light.attenuation = 1.0;
	light.NoL = saturate(dot(shading.normal, light.l));
	
	return light;
}

void EvaluatePunctualLights(const ShadingParams shading, const MaterialInputs material, const PixelParams pixel, inout float3 color) 
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
				// Retrieve global entity index from local bucket, then remove bit from local bucket:
				const uint bucket_bit_index = firstbitlow(bucket_bits);
				const uint light_index = bucket * 32 + bucket_bit_index;
				bucket_bits ^= 1 << bucket_bit_index;

				LightProperties lightProperties = Lights[light_index];
			
				[branch]
				switch (lightProperties.LightType)
				{
				case DIRECTIONAL_LIGHT:
					{
						Light light = GetDirectionalLight(shading, lightProperties);					
						if (light.NoL > 0)
						{						

							color.rgb += SurfaceShading(shading, pixel, light, material.ambientOcclusion);
						}
						
						break;
					}
				case POINT_LIGHT:
					{
						Light light = GetPointLight(shading, lightProperties);
						if (light.NoL > 0)
						{
							color.rgb += SurfaceShading(shading, pixel, light,  material.ambientOcclusion);
						}
						break;
					}
					
				case SPOT_LIGHT:
					{
						Light light = GetSpotLight(shading, lightProperties);
						if (light.NoL > 0)
						{
							color.rgb += SurfaceShading(shading, pixel, light,  material.ambientOcclusion);
						}
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
	float3 color = float3(0,0,0);
	
	// We always evaluate the IBL as not having one is going to be uncommon,
	// it also saves 1 shader variant
#if IBL    
	EvaluateIBL(shading, material, pixel, color);
#endif

#if LIT
	EvaluatePunctualLights(shading, material, pixel, color);
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
	color = LinearToSrgb(color);

	return color;
}
[End_Pass]
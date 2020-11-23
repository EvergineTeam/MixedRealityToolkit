[Begin_ResourceLayout]

[directives:FlipProjection FLIPPROJECTION_OFF FLIPPROJECTION]

cbuffer SunProperties : register(b0)
{
	float3 SunDirection						: packoffset(c0.x); [SunDirection]	
};

cbuffer AtmosphereProperties : register(b1)
{
	float3 RayleighScatteringCoefficient    : packoffset(c0.x); [Default(5.5, 13.0, 22.4)] 	// Rayleigh scattering coefficient
	float RayleighScaleHeight               : packoffset(c0.w); [Default(8000)]             // Rayleigh scale height
	float SunIntensity                      : packoffset(c1.x); [Default(22)]               // intensity of the sun
	float PlanetRadiusInKm                  : packoffset(c1.y); [Default(6371)]             // radius of the planet in meters
	float AtmosphereRadiusInKm              : packoffset(c1.z); [Default(6471)]             // radius of the atmosphere in meters
	float MieScatteringCoefficient          : packoffset(c1.w); [Default(21.0)]             // Mie scattering coefficient	
	float MieScaleHeight                    : packoffset(c2.x); [Default(1200)]             // Mie scale height
	float MiePreferredScatteringDirection   : packoffset(c2.y); [Default(0.758)]            // Mie preferred scattering direction
};

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 10_0]
[entrypoints VS = VS PS = PS]

#define Pi 3.14159265359f
#define TwoPi 6.28318530718f
#define HalfPi 1.57079632679f

#define iSteps 16
#define jSteps 8

float2 rsi(float3 r0, float3 rd, float sr) {
    // ray-sphere intersection that assumes
    // the sphere is centered at the origin.
    // No intersection when result.x > result.y
    float a = dot(rd, rd);
    float b = 2.0 * dot(rd, r0);
    float c = dot(r0, r0) - (sr * sr);
    float d = (b*b) - 4.0*a*c;
    if (d < 0.0) return float2(1e5,-1e5);
    return float2(
        (-b - sqrt(d))/(2.0*a),
        (-b + sqrt(d))/(2.0*a)
    );
}

float3 atmosphere(float3 r, float3 r0, float3 pSun, float iSun, float rPlanet, float rAtmos, float3 kRlh, float kMie, float shRlh, float shMie, float g) {
    // Normalize the sun and view directions.
    pSun = normalize(pSun);
    r = normalize(r);

    // Calculate the step size of the primary ray.
    float2 p = rsi(r0, r, rAtmos);
    if (p.x > p.y) return float3(0,0,0);
    p.y = min(p.y, rsi(r0, r, rPlanet).x);
    float iStepSize = (p.y - p.x) / float(iSteps);

    // Initialize the primary ray time.
    float iTime = 0.0;

    // Initialize accumulators for Rayleigh and Mie scattering.
    float3 totalRlh = float3(0,0,0);
    float3 totalMie = float3(0,0,0);

    // Initialize optical depth accumulators for the primary ray.
    float iOdRlh = 0.0;
    float iOdMie = 0.0;

    // Calculate the Rayleigh and Mie phases.
    float mu = dot(r, pSun);
    float mumu = mu * mu;
    float gg = g * g;
    float pRlh = 3.0 / (16.0 * Pi) * (1.0 + mumu);
    float pMie = 3.0 / (8.0 * Pi) * ((1.0 - gg) * (mumu + 1.0)) / (pow(abs(1.0 + gg - 2.0 * mu * g), 1.5) * (2.0 + gg));

    // Sample the primary ray.
    for (int i = 0; i < iSteps; i++) {

        // Calculate the primary ray sample position.
        float3 iPos = r0 + r * (iTime + iStepSize * 0.5);

        // Calculate the height of the sample.
        float iHeight = length(iPos) - rPlanet;

        // Calculate the optical depth of the Rayleigh and Mie scattering for this step.
        float odStepRlh = exp(-iHeight / shRlh) * iStepSize;
        float odStepMie = exp(-iHeight / shMie) * iStepSize;

        // Accumulate optical depth.
        iOdRlh += odStepRlh;
        iOdMie += odStepMie;

        // Calculate the step size of the secondary ray.
        float jStepSize = rsi(iPos, pSun, rAtmos).y / float(jSteps);

        // Initialize the secondary ray time.
        float jTime = 0.0;

        // Initialize optical depth accumulators for the secondary ray.
        float jOdRlh = 0.0;
        float jOdMie = 0.0;

        // Sample the secondary ray.
        for (int j = 0; j < jSteps; j++) {

            // Calculate the secondary ray sample position.
            float3 jPos = iPos + pSun * (jTime + jStepSize * 0.5);

            // Calculate the height of the sample.
            float jHeight = length(jPos) - rPlanet;

            // Accumulate the optical depth.
            jOdRlh += exp(-jHeight / shRlh) * jStepSize;
            jOdMie += exp(-jHeight / shMie) * jStepSize;

            // Increment the secondary ray time.
            jTime += jStepSize;
        }

        // Calculate attenuation.
        float3 attn = exp(-(kMie * (iOdMie + jOdMie) + kRlh * (iOdRlh + jOdRlh)));

        // Accumulate scattering.
        totalRlh += odStepRlh * attn;
        totalMie += odStepMie * attn;

        // Increment the primary ray time.
        iTime += iStepSize;

    }

    // Calculate and return the final color.
    return iSun * (pRlh * kRlh * totalRlh + pMie * kMie * totalMie);
}

struct VS_IN
{
	uint id : SV_VertexID;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD;
};

#if FLIPPROJECTION
static const PS_IN vertices[3] =
{
	{ -1.0f, -1.0f, 0.0f, 1.0f }, { 0.0f,  0.0f },
	{  3.0f, -1.0f, 0.0f, 1.0f }, { 2.0f,  0.0f },
	{ -1.0f,  3.0f, 0.0f, 1.0f }, { 0.0f, 2.0f }	
};
#else
static const PS_IN vertices[3] =
{
	{ -1.0f, -1.0f, 0.0f, 1.0f }, { 0.0f,  1.0f },
	{ -1.0f,  3.0f, 0.0f, 1.0f }, { 0.0f, -1.0f },
	{  3.0f, -1.0f, 0.0f, 1.0f }, { 2.0f,  1.0f }
};
#endif

PS_IN VS(VS_IN input)
{
	return vertices[input.id % 3];
}

float4 PS(PS_IN input) : SV_Target
{
	float2 uv = input.tex;
	uv.x = 1 - uv.x;
	
	float a = uv.x * TwoPi;
	float b = HalfPi - (uv.y * Pi);
	
	float cosa = cos(a);
	float sina = sin(a);
	float cosb = cos(b);
	float sinb = sin(b);
	
	
	float3 ray = float3(cosb * cosa, sinb, cosb * sina);

	float PlanetRadiusInM = PlanetRadiusInKm * 1000;
	float AtmosphereRadiusInM = AtmosphereRadiusInKm * 1000;


	float3 color = atmosphere(
        ray,
        float3(0,PlanetRadiusInM + 1000,0),               // ray origin
        SunDirection,                        // position of the sun
        SunIntensity,                           // intensity of the sun
        PlanetRadiusInM,                         // radius of the planet in meters
        AtmosphereRadiusInM,                         // radius of the atmosphere in meters
        RayleighScatteringCoefficient / 1000000.0, // Rayleigh scattering coefficient
        MieScatteringCoefficient / 1000000.0,                          // Mie scattering coefficient
        RayleighScaleHeight,                            // Rayleigh scale height
        MieScaleHeight,                          // Mie scale height
        MiePreferredScatteringDirection                           // Mie preferred scattering direction
    );

	color = 1.0 - exp(-1 * color);



	return float4(color, 1);
}
[End_Pass]
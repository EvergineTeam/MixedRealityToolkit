[Begin_ResourceLayout]

	[directives:Quality Performance Default Extreme]

Texture2D<float4> Color : register(t0);
RWTexture2D<float4> Output : register(u0);

SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

#if Performance
	const static float qualitySubPix = 0.25;
	const static float qualityEdgeThreshold	= 0.25;
	const static float qualityEdgeThresholdMin = 0.0833;
#elif Default
	const static float qualitySubPix = 0.75;
	const static float qualityEdgeThreshold	= 0.166;
	const static float qualityEdgeThresholdMin = 0.0833;
#elif Extreme
	const static float qualitySubPix = 1.0;
	const static float qualityEdgeThreshold	= 0.125;
	const static float qualityEdgeThresholdMin = 0.05;
#endif

#define FXAA_QUALITY__PS 12
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.0
#define FXAA_QUALITY__P2 1.0
#define FXAA_QUALITY__P3 1.0
#define FXAA_QUALITY__P4 1.0
#define FXAA_QUALITY__P5 1.5
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 2.0
#define FXAA_QUALITY__P9 2.0
#define FXAA_QUALITY__P10 4.0
#define FXAA_QUALITY__P11 8.0

float luminance(float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);
	float2 texC = (threadID.xy + 0.5) / outputSize;
	float2 rcpTexDim = 1.0 / outputSize;

    float2 posM = texC;
    float4 color = Color.SampleLevel(Sampler, texC, 0);
    float lumaM = luminance(color.rgb);
    float lumaS = luminance(Color.SampleLevel(Sampler, posM, 0, int2(0, 1)).rgb);
    float lumaE = luminance(Color.SampleLevel(Sampler, posM, 0, int2(1, 0)).rgb);
    float lumaN = luminance(Color.SampleLevel(Sampler, posM, 0, int2(0, -1)).rgb);
    float lumaW = luminance(Color.SampleLevel(Sampler, posM, 0, int2(-1, 0)).rgb);

    float maxSM = max(lumaS, lumaM);
    float minSM = min(lumaS, lumaM);
    float maxESM = max(lumaE, maxSM);
    float minESM = min(lumaE, minSM);
    float maxWN = max(lumaN, lumaW);
    float minWN = min(lumaN, lumaW);
    float rangeMax = max(maxWN, maxESM);
    float rangeMin = min(minWN, minESM);
    float rangeMaxScaled = rangeMax * qualityEdgeThreshold;
    float range = rangeMax - rangeMin;
    float rangeMaxClamped = max(qualityEdgeThresholdMin, rangeMaxScaled);
    bool earlyExit = range < rangeMaxClamped;

    if (earlyExit)
        Output[threadID.xy] = color;

    float lumaNW = luminance(Color.SampleLevel(Sampler, posM, 0, int2(-1, -1)).rgb);
    float lumaSE = luminance(Color.SampleLevel(Sampler, posM, 0, int2(1, 1)).rgb);
    float lumaNE = luminance(Color.SampleLevel(Sampler, posM, 0, int2(1, -1)).rgb);
    float lumaSW = luminance(Color.SampleLevel(Sampler, posM, 0, int2(-1, 1)).rgb);

    float lumaNS = lumaN + lumaS;
    float lumaWE = lumaW + lumaE;
    float subpixRcpRange = 1.0 / range;
    float subpixNSWE = lumaNS + lumaWE;
    float edgeHorz1 = (-2.0 * lumaM) + lumaNS;
    float edgeVert1 = (-2.0 * lumaM) + lumaWE;

    float lumaNESE = lumaNE + lumaSE;
    float lumaNWNE = lumaNW + lumaNE;
    float edgeHorz2 = (-2.0 * lumaE) + lumaNESE;
    float edgeVert2 = (-2.0 * lumaN) + lumaNWNE;

    float lumaNWSW = lumaNW + lumaSW;
    float lumaSWSE = lumaSW + lumaSE;
    float edgeHorz4 = (abs(edgeHorz1) * 2.0) + abs(edgeHorz2);
    float edgeVert4 = (abs(edgeVert1) * 2.0) + abs(edgeVert2);
    float edgeHorz3 = (-2.0 * lumaW) + lumaNWSW;
    float edgeVert3 = (-2.0 * lumaS) + lumaSWSE;
    float edgeHorz = abs(edgeHorz3) + edgeHorz4;
    float edgeVert = abs(edgeVert3) + edgeVert4;

    float subpixNWSWNESE = lumaNWSW + lumaNESE;
    float lengthSign = rcpTexDim.x;
    bool horzSpan = edgeHorz >= edgeVert;
    float subpixA = subpixNSWE * 2.0 + subpixNWSWNESE;

    if (!horzSpan)
        lumaN = lumaW;
    if (!horzSpan)
        lumaS = lumaE;
    if (horzSpan)
        lengthSign = rcpTexDim.y;
    float subpixB = (subpixA * (1.0 / 12.0)) - lumaM;

    float gradientN = lumaN - lumaM;
    float gradientS = lumaS - lumaM;
    float lumaNN = lumaN + lumaM;
    float lumaSS = lumaS + lumaM;
    bool pairN = abs(gradientN) >= abs(gradientS);
    float gradient = max(abs(gradientN), abs(gradientS));
    if (pairN)
        lengthSign = -lengthSign;
    float subpixC = saturate(abs(subpixB) * subpixRcpRange);
    
    float2 posB;
    posB.x = posM.x;
    posB.y = posM.y;
    float2 offNP;
    offNP.x = (!horzSpan) ? 0.0 : rcpTexDim.x;
    offNP.y = (horzSpan) ? 0.0 : rcpTexDim.y;
    if (!horzSpan)
        posB.x += lengthSign * 0.5;
    if (horzSpan)
        posB.y += lengthSign * 0.5;

    float2 posN;
    posN.x = posB.x - offNP.x * FXAA_QUALITY__P0;
    posN.y = posB.y - offNP.y * FXAA_QUALITY__P0;
    float2 posP;
    posP.x = posB.x + offNP.x * FXAA_QUALITY__P0;
    posP.y = posB.y + offNP.y * FXAA_QUALITY__P0;
    float subpixD = ((-2.0) * subpixC) + 3.0;
    float lumaEndN = luminance(Color.SampleLevel(Sampler, posN, 0).rgb);
    float subpixE = subpixC * subpixC;
    float lumaEndP = luminance(Color.SampleLevel(Sampler, posP, 0).rgb);

    if (!pairN)
        lumaNN = lumaSS;
    float gradientScaled = gradient * 1.0 / 4.0;
    float lumaMM = lumaM - lumaNN * 0.5;
    float subpixF = subpixD * subpixE;
    bool lumaMLTZero = lumaMM < 0.0;

    lumaEndN -= lumaNN * 0.5;
    lumaEndP -= lumaNN * 0.5;
    bool doneN = abs(lumaEndN) >= gradientScaled;
    bool doneP = abs(lumaEndP) >= gradientScaled;
    if (!doneN)
        posN.x -= offNP.x * FXAA_QUALITY__P1;
    if (!doneN)
        posN.y -= offNP.y * FXAA_QUALITY__P1;
    bool doneNP = (!doneN) || (!doneP);
    if (!doneP)
        posP.x += offNP.x * FXAA_QUALITY__P1;
    if (!doneP)
        posP.y += offNP.y * FXAA_QUALITY__P1;

    if (doneNP)
    {
        if (!doneN)
            lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
        if (!doneP)
            lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
        if (!doneN)
            lumaEndN = lumaEndN - lumaNN * 0.5;
        if (!doneP)
            lumaEndP = lumaEndP - lumaNN * 0.5;
        doneN = abs(lumaEndN) >= gradientScaled;
        doneP = abs(lumaEndP) >= gradientScaled;
        if (!doneN)
            posN.x -= offNP.x * FXAA_QUALITY__P2;
        if (!doneN)
            posN.y -= offNP.y * FXAA_QUALITY__P2;
        doneNP = (!doneN) || (!doneP);
        if (!doneP)
            posP.x += offNP.x * FXAA_QUALITY__P2;
        if (!doneP)
            posP.y += offNP.y * FXAA_QUALITY__P2;
/*--------------------------------------------------------------------------*/
        if (doneNP)
        {
            if (!doneN)
                lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
            if (!doneP)
                lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
            if (!doneN)
                lumaEndN = lumaEndN - lumaNN * 0.5;
            if (!doneP)
                lumaEndP = lumaEndP - lumaNN * 0.5;
            doneN = abs(lumaEndN) >= gradientScaled;
            doneP = abs(lumaEndP) >= gradientScaled;
            if (!doneN)
                posN.x -= offNP.x * FXAA_QUALITY__P3;
            if (!doneN)
                posN.y -= offNP.y * FXAA_QUALITY__P3;
            doneNP = (!doneN) || (!doneP);
            if (!doneP)
                posP.x += offNP.x * FXAA_QUALITY__P3;
            if (!doneP)
                posP.y += offNP.y * FXAA_QUALITY__P3;
/*--------------------------------------------------------------------------*/
            if (doneNP)
            {
                if (!doneN)
                    lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                if (!doneP)
                    lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                if (!doneN)
                    lumaEndN = lumaEndN - lumaNN * 0.5;
                if (!doneP)
                    lumaEndP = lumaEndP - lumaNN * 0.5;
                doneN = abs(lumaEndN) >= gradientScaled;
                doneP = abs(lumaEndP) >= gradientScaled;
                if (!doneN)
                    posN.x -= offNP.x * FXAA_QUALITY__P4;
                if (!doneN)
                    posN.y -= offNP.y * FXAA_QUALITY__P4;
                doneNP = (!doneN) || (!doneP);
                if (!doneP)
                    posP.x += offNP.x * FXAA_QUALITY__P4;
                if (!doneP)
                    posP.y += offNP.y * FXAA_QUALITY__P4;
/*--------------------------------------------------------------------------*/

                if (doneNP)
                {
                    if (!doneN)
                        lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                    if (!doneP)
                        lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                    if (!doneN)
                        lumaEndN = lumaEndN - lumaNN * 0.5;
                    if (!doneP)
                        lumaEndP = lumaEndP - lumaNN * 0.5;
                    doneN = abs(lumaEndN) >= gradientScaled;
                    doneP = abs(lumaEndP) >= gradientScaled;
                    if (!doneN)
                        posN.x -= offNP.x * FXAA_QUALITY__P5;
                    if (!doneN)
                        posN.y -= offNP.y * FXAA_QUALITY__P5;
                    doneNP = (!doneN) || (!doneP);
                    if (!doneP)
                        posP.x += offNP.x * FXAA_QUALITY__P5;
                    if (!doneP)
                        posP.y += offNP.y * FXAA_QUALITY__P5;
/*--------------------------------------------------------------------------*/
                    if (doneNP)
                    {
                        if (!doneN)
                            lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                        if (!doneP)
                            lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                        if (!doneN)
                            lumaEndN = lumaEndN - lumaNN * 0.5;
                        if (!doneP)
                            lumaEndP = lumaEndP - lumaNN * 0.5;
                        doneN = abs(lumaEndN) >= gradientScaled;
                        doneP = abs(lumaEndP) >= gradientScaled;
                        if (!doneN)
                            posN.x -= offNP.x * FXAA_QUALITY__P6;
                        if (!doneN)
                            posN.y -= offNP.y * FXAA_QUALITY__P6;
                        doneNP = (!doneN) || (!doneP);
                        if (!doneP)
                            posP.x += offNP.x * FXAA_QUALITY__P6;
                        if (!doneP)
                            posP.y += offNP.y * FXAA_QUALITY__P6;
/*--------------------------------------------------------------------------*/
                        if (doneNP)
                        {
                            if (!doneN)
                                lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                            if (!doneP)
                                lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                            if (!doneN)
                                lumaEndN = lumaEndN - lumaNN * 0.5;
                            if (!doneP)
                                lumaEndP = lumaEndP - lumaNN * 0.5;
                            doneN = abs(lumaEndN) >= gradientScaled;
                            doneP = abs(lumaEndP) >= gradientScaled;
                            if (!doneN)
                                posN.x -= offNP.x * FXAA_QUALITY__P7;
                            if (!doneN)
                                posN.y -= offNP.y * FXAA_QUALITY__P7;
                            doneNP = (!doneN) || (!doneP);
                            if (!doneP)
                                posP.x += offNP.x * FXAA_QUALITY__P7;
                            if (!doneP)
                                posP.y += offNP.y * FXAA_QUALITY__P7;
/*--------------------------------------------------------------------------*/

                            if (doneNP)
                            {
                                if (!doneN)
                                    lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                                if (!doneP)
                                    lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                                if (!doneN)
                                    lumaEndN = lumaEndN - lumaNN * 0.5;
                                if (!doneP)
                                    lumaEndP = lumaEndP - lumaNN * 0.5;
                                doneN = abs(lumaEndN) >= gradientScaled;
                                doneP = abs(lumaEndP) >= gradientScaled;
                                if (!doneN)
                                    posN.x -= offNP.x * FXAA_QUALITY__P8;
                                if (!doneN)
                                    posN.y -= offNP.y * FXAA_QUALITY__P8;
                                doneNP = (!doneN) || (!doneP);
                                if (!doneP)
                                    posP.x += offNP.x * FXAA_QUALITY__P8;
                                if (!doneP)
                                    posP.y += offNP.y * FXAA_QUALITY__P8;
/*--------------------------------------------------------------------------*/
                                if (doneNP)
                                {
                                    if (!doneN)
                                        lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                                    if (!doneP)
                                        lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                                    if (!doneN)
                                        lumaEndN = lumaEndN - lumaNN * 0.5;
                                    if (!doneP)
                                        lumaEndP = lumaEndP - lumaNN * 0.5;
                                    doneN = abs(lumaEndN) >= gradientScaled;
                                    doneP = abs(lumaEndP) >= gradientScaled;
                                    if (!doneN)
                                        posN.x -= offNP.x * FXAA_QUALITY__P9;
                                    if (!doneN)
                                        posN.y -= offNP.y * FXAA_QUALITY__P9;
                                    doneNP = (!doneN) || (!doneP);
                                    if (!doneP)
                                        posP.x += offNP.x * FXAA_QUALITY__P9;
                                    if (!doneP)
                                        posP.y += offNP.y * FXAA_QUALITY__P9;
/*--------------------------------------------------------------------------*/
                                    if (doneNP)
                                    {
                                        if (!doneN)
                                            lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                                        if (!doneP)
                                            lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                                        if (!doneN)
                                            lumaEndN = lumaEndN - lumaNN * 0.5;
                                        if (!doneP)
                                            lumaEndP = lumaEndP - lumaNN * 0.5;
                                        doneN = abs(lumaEndN) >= gradientScaled;
                                        doneP = abs(lumaEndP) >= gradientScaled;
                                        if (!doneN)
                                            posN.x -= offNP.x * FXAA_QUALITY__P10;
                                        if (!doneN)
                                            posN.y -= offNP.y * FXAA_QUALITY__P10;
                                        doneNP = (!doneN) || (!doneP);
                                        if (!doneP)
                                            posP.x += offNP.x * FXAA_QUALITY__P10;
                                        if (!doneP)
                                            posP.y += offNP.y * FXAA_QUALITY__P10;
/*--------------------------------------------------------------------------*/
                                        if (doneNP)
                                        {
                                            if (!doneN)
                                                lumaEndN = luminance(Color.SampleLevel(Sampler, posN.xy, 0).rgb);
                                            if (!doneP)
                                                lumaEndP = luminance(Color.SampleLevel(Sampler, posP.xy, 0).rgb);
                                            if (!doneN)
                                                lumaEndN = lumaEndN - lumaNN * 0.5;
                                            if (!doneP)
                                                lumaEndP = lumaEndP - lumaNN * 0.5;
                                            doneN = abs(lumaEndN) >= gradientScaled;
                                            doneP = abs(lumaEndP) >= gradientScaled;
                                            if (!doneN)
                                                posN.x -= offNP.x * FXAA_QUALITY__P11;
                                            if (!doneN)
                                                posN.y -= offNP.y * FXAA_QUALITY__P11;
                                            doneNP = (!doneN) || (!doneP);
                                            if (!doneP)
                                                posP.x += offNP.x * FXAA_QUALITY__P11;
                                            if (!doneP)
                                                posP.y += offNP.y * FXAA_QUALITY__P11;

/*--------------------------------------------------------------------------*/
                                        }
/*--------------------------------------------------------------------------*/
                                    }
/*--------------------------------------------------------------------------*/
                                }
/*--------------------------------------------------------------------------*/
                            }                        	
/*--------------------------------------------------------------------------*/
                        }
/*--------------------------------------------------------------------------*/
                    }
/*--------------------------------------------------------------------------*/
                }
/*--------------------------------------------------------------------------*/
            }
/*--------------------------------------------------------------------------*/
        }
/*--------------------------------------------------------------------------*/
    }
/*--------------------------------------------------------------------------*/
    float dstN = posM.x - posN.x;
    float dstP = posP.x - posM.x;
    if (!horzSpan)
        dstN = posM.y - posN.y;
    if (!horzSpan)
        dstP = posP.y - posM.y;
/*--------------------------------------------------------------------------*/
    bool goodSpanN = (lumaEndN < 0.0) != lumaMLTZero;
    float spanLength = (dstP + dstN);
    bool goodSpanP = (lumaEndP < 0.0) != lumaMLTZero;
    float spanLengthRcp = 1.0 / spanLength;
/*--------------------------------------------------------------------------*/
    bool directionN = dstN < dstP;
    float dst = min(dstN, dstP);
    bool goodSpan = directionN ? goodSpanN : goodSpanP;
    float subpixG = subpixF * subpixF;
    float pixelOffset = (dst * (-spanLengthRcp)) + 0.5;
    float subpixH = subpixG * qualitySubPix;
/*--------------------------------------------------------------------------*/
    float pixelOffsetGood = goodSpan ? pixelOffset : 0.0;
    float pixelOffsetSubpix = max(pixelOffsetGood, subpixH);
    if (!horzSpan)
        posM.x += pixelOffsetSubpix * lengthSign;
    if (horzSpan)
        posM.y += pixelOffsetSubpix * lengthSign;

    Output[threadID.xy] = float4(Color.SampleLevel(Sampler, posM, 0).xyz, color.w);
}

[End_Pass]
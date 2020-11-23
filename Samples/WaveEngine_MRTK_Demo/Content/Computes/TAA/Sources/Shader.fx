[Begin_ResourceLayout]
	
	Texture2D Input : register(t0);	
	Texture2D<float> LinearDepth : register(t1);
	Texture2D VelocityMap : register(t2);
	Texture2D History : register(t3); [TemporalHistory]
	
	RWTexture2D<float4> Output : register(u0);
	
	SamplerState Sampler: register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

float4 clip_aabb( float3 aabb_min, // cn_min
	float3 aabb_max, // cn_max
	float4 p, // c_in’
	float4 q) // c_hist
{
	float3 p_clip = 0.5 * (aabb_max + aabb_min);
	float3 e_clip = 0.5 * (aabb_max - aabb_min);
	float4 v_clip = q - float4(p_clip, p.w);
	float3 v_unit = v_clip.xyz / e_clip;
	float3 a_unit = abs(v_unit);
	float ma_unit = max(a_unit.x, max(a_unit.y, a_unit.z));
	if (ma_unit > 1.0)
		return float4(p_clip, p.w) + v_clip / ma_unit;
	else
		return q;// point inside aabb
}

float luminance(float3 color)
{
    return dot(color.xyz, float3(0.299, 0.587, 0.114));
}

float luminance(Texture2D texture_in, float2 uv, float bias) {
	float4 sample_color = texture_in.SampleLevel(Sampler, uv, bias);
	return luminance(sample_color.rgb);
}

float luminance(Texture2D texture_in, float2 uv) {
	return luminance(texture_in, uv, 0.0);
}

[numthreads(8, 8, 1)]
void CS(uint3 DTid : SV_DispatchThreadID, uint3 GTid : SV_GroupThreadID, uint3 Gid : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	float2 outputSize;
	Output.GetDimensions(outputSize.x, outputSize.y);	
	const float2 uv = (DTid.xy + 0.5f) / outputSize; // Jittered
		
	float3 neighborhoodMin = 100000;
	float3 neighborhoodMax = -100000;
	float3 neighborhoodAvg = 0;
	float3 current;
	float bestDepth = 1;

	// Search for best velocity and compute color clamping range in 3x3 neighborhood:
	int2 bestPixel = int2(0, 0);
	for (int x = -1; x <= 1; ++x)
	{
		for (int y = -1; y <= 1; ++y)
		{
			const int2 curPixel = DTid.xy + int2(x, y);

			const float3 neighbor = Input[curPixel].rgb;
			neighborhoodMin = min(neighborhoodMin, neighbor);
			neighborhoodMax = max(neighborhoodMax, neighbor);
			neighborhoodAvg += neighbor;
			if (x == 0 && y == 0)
			{
				current = neighbor;
			}

			const float depth = LinearDepth[curPixel];
			if (depth < bestDepth)
			{
				bestDepth = depth;
				bestPixel = curPixel;
			}
		}
	}
	neighborhoodAvg /= 9;
	
	const float2 velocity = VelocityMap[bestPixel].xy;
	
	const float2 prevUV = uv + velocity;

	// we cannot avoid the linear filter here because point sampling could sample irrelevant pixels but we try to correct it later:
	float3 history = History.SampleLevel(Sampler, prevUV,0).rgb;
	
	// Color Constraint Cross
	float4 cn_cross_max = float4(0.0, 0.0, 0.0, 0.0);
	float4 cn_cross_min = float4(1.0, 1.0, 1.0, 0.0);
	float4 cn_cross_avg = float4(0,0,0,0);

	float2 cross_temp;

	float2 inv_res = 1 / outputSize;
	cross_temp = uv.xy + float2((-1.0) * inv_res.x, 0.0);
	float4 cn_temp = Input.SampleLevel(Sampler, cross_temp , 0);
	cn_cross_min = min(cn_cross_min, cn_temp);
	cn_cross_max = max(cn_cross_max, cn_temp);
	cn_cross_avg += cn_temp;

	cross_temp = uv.xy + float2(0.0, (1.0) * inv_res.y);
	cn_temp = Input.SampleLevel(Sampler, cross_temp , 0);
	cn_cross_min = min(cn_cross_min, cn_temp);
	cn_cross_max = max(cn_cross_max, cn_temp);
	cn_cross_avg += cn_temp;

	cn_temp = Input.SampleLevel(Sampler, uv, 0);
	cn_cross_min = min(cn_cross_min, cn_temp);
	cn_cross_max = max(cn_cross_max, cn_temp);
	cn_cross_avg += cn_temp;

	cross_temp = uv.xy + float2(0.0, (-1.0) * inv_res.y);
	cn_temp = Input.SampleLevel(Sampler, cross_temp , 0);
	cn_cross_min = min(cn_cross_min, cn_temp);
	cn_cross_max = max(cn_cross_max, cn_temp);
	cn_cross_avg += cn_temp;

	cross_temp = uv.xy + float2((1.0) * inv_res.x, 0.0);
	cn_temp = Input.SampleLevel(Sampler, cross_temp , 0);
	cn_cross_min = min(cn_cross_min, cn_temp);
	cn_cross_max = max(cn_cross_max, cn_temp);
	cn_cross_avg += cn_temp;

	// Mix min-max averaging
	neighborhoodMin = lerp(neighborhoodMin, cn_cross_min.xyz, 0.5);
	neighborhoodMax = lerp(neighborhoodMax, cn_cross_max.xyz, 0.5);
	neighborhoodAvg = lerp(neighborhoodAvg, cn_cross_avg.xyz, 0.5);

	// simple correction of image signal incoherency (eg. moving shadows or lighting changes):
	//history.rgb = clamp(history.rgb, neighborhoodMin, neighborhoodMax);
	history.rgb = clip_aabb(neighborhoodMin.xyz, neighborhoodMax.xyz, float4(clamp(neighborhoodAvg, neighborhoodMin, neighborhoodMax),1), float4(history.rgb,1)).rgb;

	// Feedback Luminance Weighting
	float lum_current = luminance(current);
	float lum_history = luminance(history);

	float unbiased_diff = abs(lum_current - lum_history) / max(lum_current, max(lum_history, 0.2));
	float unbiased_weight = 1.0 - unbiased_diff;
	float unbiased_weight_sqr = unbiased_weight * unbiased_weight;
	float k_feedback = lerp(0.979, 0.925, unbiased_weight_sqr);

	// do the temporal super sampling by linearly accumulating previous samples with the current one:
	float4 resolved = float4(lerp(current.rgb,history.rgb, k_feedback),1);

	Output[DTid.xy] = resolved;
}

[End_Pass]
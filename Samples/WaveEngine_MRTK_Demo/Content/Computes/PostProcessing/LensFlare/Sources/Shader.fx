[Begin_ResourceLayout]
	[Directives:GhostTint Tint_Off Tint_On]
	[Directives:Chromatic Chro_Off Chro_On]

	cbuffer Parameters : register(b0)
	{
		int uGhostCount : packoffset(c0.x); [Default(8)]
		float uGhostSpacing : packoffset(c0.y); [Default(0.3)]
		float uGhostThreshold : packoffset(c0.z); [Default(12)]
		float uGhostChromaticAberration : packoffset(c0.w); [Default(0.002)]
		float uHaloRadius : packoffset(c1.x); [Default(0.5)]
		float uHaloThickness : packoffset(c1.y); [Default(0.05)]
		float uHaloThreshold : packoffset(c1.z); [Default(20)]
		float uHaloChromaticAberration : packoffset(c1.w); [Default(0.002)]
	}

	Texture2D Input: register(t0);
	Texture2D ColorGradient : register(t1);
	RWTexture2D<float4> Output : register(u0);

	SamplerState Sampler : register(s0);

[End_ResourceLayout]

[Begin_Pass:Default]

	[Profile 11_0]
	[Entrypoints CS = CS]


	float3 ApplyThreshold(in float3 rgb, in float threshold)
	{
		float3 thresholdVec = threshold;
		return max(rgb - thresholdVec, 0.0);
	}

	float3 SampleSceneColor(in float2 _uv, float chromaticAberration)
	{
	#if Chro_On	
		float2 offset = normalize(0.5 - _uv) * chromaticAberration;
		return float3(
			Input.SampleLevel(Sampler, _uv + offset, 0).r,
			Input.SampleLevel(Sampler, _uv, 0).g,
			Input.SampleLevel(Sampler, _uv - offset, 0).b
			);
	#else
		return Input.SampleLevel(Sampler, _uv,0).rgb;
	#endif
	}

	float3 SampleGhosts(in float2 _uv, in float _threshold)
	{
		float3 ret = 0.0;
		float2 ghostVec = (0.5 - _uv) * uGhostSpacing;
		for (int i = 0; i < uGhostCount; ++i) {
		 // sample scene color
			float2 suv = frac(_uv + ghostVec * float2(i,i));
			float3 s = SampleSceneColor(suv, uGhostChromaticAberration);
			s = ApplyThreshold(s, _threshold);
			
		 // tint/weight
			float distanceToCenter = distance(suv, float2(0.5,0.5));
			#if Tint_On
				s *= ColorGradient.SampleLevel(Sampler,float2(distanceToCenter, 0.5), 0).rgb; // incorporate weight into tint gradient
			#else
				float weight = smoothstep(0.0, 0.75, distanceToCenter); // analytical weight
				s *= weight;
			#endif

			ret += s;
		}
		#if !Tint_On
			ret *= ColorGradient.SampleLevel(Sampler, float2(distance(_uv, float2(0.5,0.5)), 0.5), 0.0).rgb;
		#endif

		return ret;
	}

	// Cubic window; map [0, _radius] in [1, 0] as a cubic falloff from _center.
	float Window_Cubic(float _x, float _center, float _radius)
	{
		_x = min(abs(_x - _center) / _radius, 1.0);
		return 1.0 - _x * _x * (3.0 - 2.0 * _x);
	}

	float3 SampleHalo(in float2 _uv, in float _radius, in float _aspectRatio, in float _threshold)
	{
		float2 haloVec = 0.5 - _uv;	
		haloVec.x /= _aspectRatio;
		haloVec = normalize(haloVec);
		haloVec.x *= _aspectRatio;
		float2 wuv = (_uv - float2(0.5, 0.0)) / float2(_aspectRatio, 1.0) + float2(0.5, 0.0);
		float haloWeight = distance(wuv, float2(0.5,0.5));	
		haloVec *= _radius;
		haloWeight = Window_Cubic(haloWeight, _radius, uHaloThickness);
		return ApplyThreshold(SampleSceneColor(_uv + haloVec,uHaloChromaticAberration), _threshold) * haloWeight;
	}

	[numthreads(8, 8, 1)]
	void CS(uint3 threadID : SV_DispatchThreadID)
	{
		float2 outputSize;
		Output.GetDimensions(outputSize.x, outputSize.y);
		float2 uv = (threadID.xy + 0.5) / outputSize;
		
		float aspectRatio = outputSize.y / outputSize.x;	
		float3 ret = 0.0;

		ret += SampleGhosts(uv, uGhostThreshold);
		ret += SampleHalo(uv, uHaloRadius, aspectRatio, uHaloThreshold);

		Output[threadID.xy] = float4(ret,1);
	}

[End_Pass]
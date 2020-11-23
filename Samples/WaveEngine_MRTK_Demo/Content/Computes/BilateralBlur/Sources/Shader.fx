[Begin_ResourceLayout]

cbuffer consts : register(b0)
{
	float sigmaD : packoffset(c0.x); [Default(4.0)]
	float sigmaR : packoffset(c0.y); [Default(4.0)]
};

Texture2D Input : register(t0);
RWTexture2D<float4> Result : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

#define MINF asfloat(0xff800000)

float  gaussD(float sigma, int x, int y)
{
	return exp(-((x*x+y*y)/(2.0f*sigma*sigma)));
}

float gaussR(float sigma, float dist)
{
	return exp(-(dist*dist)/(2.0*sigma*sigma));
}

[numthreads(8, 8, 1)]
void CS(int3 threadID : SV_DispatchThreadID)
{
	float2 BufferSize;
	Input.GetDimensions(BufferSize.x, BufferSize.y);
		
	int kernelRadius = (int)ceil(2.0 * sigmaD);
	int kernelSize = 2 * kernelRadius + 1;
	
	float3 sum = float3(0.0, 0.0, 0.0);
	float sumWeight = 0.0;
	
	Result[threadID.xy] = float4(MINF, MINF, MINF, MINF);

	float4 intCenter = Input[threadID.xy];
	if(intCenter.x != MINF)
	{
		for(int m = threadID.x-kernelRadius; m <= threadID.x+kernelRadius; m++)
		{
			for(int n = threadID.y-kernelRadius; n <= threadID.y+kernelRadius; n++)
			{
				if(m >= 0 && n >= 0 && m < BufferSize.x && n < BufferSize.y)
				{
					uint2 pos = uint2(m, n);
					float4 intKerPos = Input[pos];

					if(intKerPos.x != MINF)
					{
						float d = distance(intKerPos.xyz, intCenter.xyz);
						float weight = gaussD(sigmaD, m-threadID.x, n-threadID.y)*gaussR(sigmaR, d);
								
						sumWeight += weight;
						sum += weight*intKerPos.xyz;
					}
				}
			}
		}

		if(sumWeight > 0.0)
		{
			Result[threadID.xy] = float4(sum / sumWeight, 1.0);
		}
	}
}

[End_Pass]
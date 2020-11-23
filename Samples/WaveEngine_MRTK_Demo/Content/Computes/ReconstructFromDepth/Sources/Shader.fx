[Begin_ResourceLayout]

// Source: http://www.humus.name/temp/Linearize%20depth.txt
// n = near
// f = far
// z = depth buffer Z-value
// EZ  = eye Z value
// LZ  = depth buffer Z-value remapped to a linear [0..1] range (near plane to far plane)
// LZ2 = depth buffer Z-value remapped to a linear [0..1] range (eye to far plane)
// 
// 
// DX:
// EZ  = (n * f) / (f - z * (f - n))
// LZ  = (eyeZ - n) / (f - n) = z / (f - z * (f - n))
// LZ2 = eyeZ / f = n / (f - z * (f - n))
// 
// 
// GL:
// EZ  = (2 * n * f) / (f + n - z * (f - n))
// LZ  = (eyeZ - n) / (f - n) = n * (z + 1.0) / (f + n - z * (f - n))
// LZ2 = eyeZ / f = (2 * n) / (f + n - z * (f - n))
// 
// 
// 
// LZ2 in two instructions:
// LZ2 = 1.0 / (c0 * z + c1)

// DX:
//   c1 = f / n
//   c0 = 1.0 - c1
// 
// GL:
//   c0 = (1 - f / n) / 2
//   c1 = (1 + f / n) / 2

cbuffer ParamsBuffer : register(b0)
{
	float4x4 ViewProjectionInverse : packoffset(c0); [ViewProjectionInverse]
	float4x4 ViewProjection : packoffset(c4); [ViewProjection]
	float4x4 PreviousViewProjection : packoffset(c8); [PreviousViewProjection]
	float Near : packoffset(c12.x); [CameraNearPlane]
	float Far : packoffset(c12.y); [CameraFarPlane]
	float2 Jitter : packoffset(c12.z); [CameraJitter]
	float2 PreviousJitter : packoffset(c13.x); [CameraPreviousJitter]
}

Texture2D<float> Depth : register(t0);
RWTexture2D<float4> PositionOutput : register(u0); [Output(Depth,1,R16G16B16A16_Float)]
RWTexture2D<float2> VelocityOutput : register(u1); [Output(Depth, 1, R16G16_Float)]
RWTexture2D<float> LinealDepthOutput : register(u2); [Output(Depth, 1, R32_Float)]

[End_ResourceLayout]

[Begin_Pass:Default]

[profile 11_0]
[entrypoints CS = CS]

inline float4 GetClipSpacePosition(in float2 uv, in float z)
{
	float x = uv.x * 2.0 - 1.0;
  	float y = (1.0 - uv.y) * 2.0 - 1.0;
 	return float4(x, y, z, 1.0);
}

inline float4 ReconstructPosition(in float2 uv, in float z, in float4x4 InvVP)
{
  float4 position_c = GetClipSpacePosition(uv, z);
  float4 position_v = mul(position_c, InvVP);
  return position_v.xyzw / position_v.w;
}

[numthreads(8, 8, 1)]
void CS(uint3 threadID : SV_DispatchThreadID)
{
	float2 outputSize;
	PositionOutput.GetDimensions(outputSize.x, outputSize.y);
	float2 uv = (threadID.xy + 0.5) / outputSize;
	
	float z = Depth[threadID.xy];

	// Position
	float4 position_c = GetClipSpacePosition(uv, z);
	float4 position_v = mul(position_c, ViewProjectionInverse);
  	float4 P0 = position_v.xyzw / position_v.w;	
	PositionOutput[threadID.xy] = P0;

	// Velocity	
	float4 currentPosition = mul(P0, ViewProjection);	
	float2 positionNDC = currentPosition.xy / currentPosition.w + Jitter;	
		
	float4 prePosition = mul(P0, PreviousViewProjection);	
	float2 prePositionNDC = prePosition.xy / prePosition.w + PreviousJitter;		
	
	VelocityOutput[threadID.xy] = (prePositionNDC - positionNDC) * float2(0.5, -0.5);	

	// Lineal Depth
	float c1 = (Far / Near);
	float c0 = 1.0 - c1;	
	LinealDepthOutput[threadID.xy] = 1.0 / (c0 * z + c1);
}

[End_Pass]
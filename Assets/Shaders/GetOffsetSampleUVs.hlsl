void GetOffsetSampleUVs_float(float4 UV, float2 TexelSize,
	out float2 UVOriginal, out float2 UVTop, out float2 UVBottom, out float2 UVLeft, out float2 UVRight){
	UVOriginal = UV;
	UVTop = float2(UV.x + TexelSize.x, UV.y);
	UVBottom = float2(UV.x - TexelSize.x, UV.y);
	UVLeft = float2(UV.x, UV.y - TexelSize.y);
	UVRight = float2(UV.x, UV.y + TexelSize.y);
}
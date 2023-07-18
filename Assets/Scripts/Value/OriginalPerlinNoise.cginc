#ifndef ORIGINAL_PERLIN_NOISE
#define ORIGINAL_PERLIN_NOISE

// 補間関数（3次エルミート曲線）= smoothstep
float2 interpolate(float2 t)
{
	return t*t*(3.0 - 2.0*t);
}
float3 interpolate(float3 t)
{
	return t*t*(3.0 - 2.0*t);
}
float4 interpolate(float4 t)
{
	return t*t*(3.0 - 2.0*t);
}

// 疑似乱数生成
float2 pseudoRandom(float2 v)
{
	v = float2(dot(v, float2(127.1, 311.7)), dot(v, float2(269.5, 183.3)));
	return -1.0 + 2.0 * frac(sin(v) * 43758.5453123);
}
float3 pseudoRandom(float3 v)
{
	v = float3(dot(v, float3(127.1, 311.7, 542.3)), dot(v, float3(269.5, 183.3, 461.7)), dot(v, float3(732.1, 845.3, 231.7)));
	return -1.0 + 2.0 * frac(sin(v) * 43758.5453123);
}
float4 pseudoRandom(float4 v)
{
	v = float4(
		dot(v, float4(127.1, 311.7, 542.3, 215.1)),
		dot(v, float4(269.5, 183.3, 461.7, 523.3)),
		dot(v, float4(732.1, 845.3, 231.7, 641.1)),
		dot(v, float4(321.3, 195.7, 591.5, 104.3)));
	return -1.0 + 2.0 * frac(sin(v) * 43758.5453123);
}

// Original Perlin Noise 2D
float _originalPerlinNoise(float2 v)
{
	// 格子の整数部の座標
	float2 i = floor(v);
	// 格子の小数部の座標
	float2 f = frac(v);

	// 格子の4つの角の座標値
	float2 i00 = i;
	float2 i10 = i + float2(1.0, 0.0);
	float2 i01 = i + float2(0.0, 1.0);
	float2 i11 = i + float2(1.0, 1.0);

	// それぞれの格子点から点Pへのベクトル
	float2 p00 = f;
	float2 p10 = f - float2(1.0, 0.0);
	float2 p01 = f - float2(0.0, 1.0);
	float2 p11 = f - float2(1.0, 1.0);

	// 格子点それぞれの勾配
	float2 g00 = pseudoRandom(i00);
	float2 g10 = pseudoRandom(i10);
	float2 g01 = pseudoRandom(i01);
	float2 g11 = pseudoRandom(i11);

	// 正規化
	g00 = normalize(g00);
	g10 = normalize(g10);
	g01 = normalize(g01);
	g11 = normalize(g11);

	// 各格子点のノイズの値を計算
	float n00 = dot(g00, p00);
	float n10 = dot(g10, p10);
	float n01 = dot(g01, p01);
	float n11 = dot(g11, p11);

	// 補間
	float2 u_xy = interpolate(f.xy);
	float2 n_x  = lerp(float2(n00, n01), float2(n10, n11), u_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, u_xy.y);
	return n_xy;
}
float _originalPerlinNoise(float3 v)
{
	// 格子の整数部の座標
	float3 i = floor(v);
	// 格子の小数部の座標
	float3 f = frac(v);

	// 格子の8つの角の座標値
	float3 i000 = i;
	float3 i100 = i + float3(1.0, 0.0, 0.0);
	float3 i010 = i + float3(0.0, 1.0, 0.0);
	float3 i110 = i + float3(1.0, 1.0, 0.0);
	float3 i001 = i + float3(0.0, 0.0, 1.0);
	float3 i101 = i + float3(1.0, 0.0, 1.0);
	float3 i011 = i + float3(0.0, 1.0, 1.0);
	float3 i111 = i + float3(1.0, 1.0, 1.0);

	// それぞれの格子点から点Pへのベクトル
	float3 p000 = f;
	float3 p100 = f - float3(1.0, 0.0, 0.0);
	float3 p010 = f - float3(0.0, 1.0, 0.0);
	float3 p110 = f - float3(1.0, 1.0, 0.0);
	float3 p001 = f - float3(0.0, 0.0, 1.0);
	float3 p101 = f - float3(1.0, 0.0, 1.0);
	float3 p011 = f - float3(0.0, 1.0, 1.0);
	float3 p111 = f - float3(1.0, 1.0, 1.0);

	// 格子点それぞれの勾配
	float3 g000 = pseudoRandom(i000);
	float3 g100 = pseudoRandom(i100);
	float3 g010 = pseudoRandom(i010);
	float3 g110 = pseudoRandom(i110);
	float3 g001 = pseudoRandom(i001);
	float3 g101 = pseudoRandom(i101);
	float3 g011 = pseudoRandom(i011);
	float3 g111 = pseudoRandom(i111);

	// 正規化
	g000 = normalize(g000);
	g100 = normalize(g100);
	g010 = normalize(g010);
	g110 = normalize(g110);
	g001 = normalize(g001);
	g101 = normalize(g101);
	g011 = normalize(g011);
	g111 = normalize(g111);

	// 各格子点のノイズの値を計算
	float n000 = dot(g000, p000);
	float n100 = dot(g100, p100);
	float n010 = dot(g010, p010);
	float n110 = dot(g110, p110);
	float n001 = dot(g001, p001);
	float n101 = dot(g101, p101);
	float n011 = dot(g011, p011);
	float n111 = dot(g111, p111);

	// 補間
	float3 u_xyz = interpolate(f);
	float4 n_z   = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), u_xyz.z);
	float2 n_yz  = lerp(n_z.xy, n_z.zw, u_xyz.y);
	float  n_xyz = lerp(n_yz.x, n_yz.y, u_xyz.x);
	return n_xyz;
}

float _originalPerlinNoise(float4 v)
{
	// 格子の整数部の座標
	float4 i = floor(v);
	// 格子の小数部の座標
	float4 f = frac(v);

	// 格子の16つの角の座標値
	float4 i0000 = i;
	float4 i1000 = i + float4(1.0, 0.0, 0.0, 0.0);
	float4 i0100 = i + float4(0.0, 1.0, 0.0, 0.0);
	float4 i1100 = i + float4(1.0, 1.0, 0.0, 0.0);
	float4 i0010 = i + float4(0.0, 0.0, 1.0, 0.0);
	float4 i1010 = i + float4(1.0, 0.0, 1.0, 0.0);
	float4 i0110 = i + float4(0.0, 1.0, 1.0, 0.0);
	float4 i1110 = i + float4(1.0, 1.0, 1.0, 0.0);
	float4 i0001 = i + float4(0.0, 0.0, 0.0, 1.0);
	float4 i1001 = i + float4(1.0, 0.0, 0.0, 1.0);
	float4 i0101 = i + float4(0.0, 1.0, 0.0, 1.0);
	float4 i1101 = i + float4(1.0, 1.0, 0.0, 1.0);
	float4 i0011 = i + float4(0.0, 0.0, 1.0, 1.0);
	float4 i1011 = i + float4(1.0, 0.0, 1.0, 1.0);
	float4 i0111 = i + float4(0.0, 1.0, 1.0, 1.0);
	float4 i1111 = i + float4(1.0, 1.0, 1.0, 1.0);

	// それぞれの格子点から点Pへのベクトル
	float4 p0000 = f;
	float4 p1000 = f - float4(1.0, 0.0, 0.0, 0.0);
	float4 p0100 = f - float4(0.0, 1.0, 0.0, 0.0);
	float4 p1100 = f - float4(1.0, 1.0, 0.0, 0.0);
	float4 p0010 = f - float4(0.0, 0.0, 1.0, 0.0);
	float4 p1010 = f - float4(1.0, 0.0, 1.0, 0.0);
	float4 p0110 = f - float4(0.0, 1.0, 1.0, 0.0);
	float4 p1110 = f - float4(1.0, 1.0, 1.0, 0.0);
	float4 p0001 = f - float4(0.0, 0.0, 0.0, 1.0);
	float4 p1001 = f - float4(1.0, 0.0, 0.0, 1.0);
	float4 p0101 = f - float4(0.0, 1.0, 0.0, 1.0);
	float4 p1101 = f - float4(1.0, 1.0, 0.0, 1.0);
	float4 p0011 = f - float4(0.0, 0.0, 1.0, 1.0);
	float4 p1011 = f - float4(1.0, 0.0, 1.0, 1.0);
	float4 p0111 = f - float4(0.0, 1.0, 1.0, 1.0);
	float4 p1111 = f - float4(1.0, 1.0, 1.0, 1.0);

	// 格子点それぞれの勾配
	float4 g0000 = pseudoRandom(i0000);
	float4 g1000 = pseudoRandom(i1000);
	float4 g0100 = pseudoRandom(i0100);
	float4 g1100 = pseudoRandom(i1100);
	float4 g0010 = pseudoRandom(i0010);
	float4 g1010 = pseudoRandom(i1010);
	float4 g0110 = pseudoRandom(i0110);
	float4 g1110 = pseudoRandom(i1110);
	float4 g0001 = pseudoRandom(i0001);
	float4 g1001 = pseudoRandom(i1001);
	float4 g0101 = pseudoRandom(i0101);
	float4 g1101 = pseudoRandom(i1101);
	float4 g0011 = pseudoRandom(i0011);
	float4 g1011 = pseudoRandom(i1011);
	float4 g0111 = pseudoRandom(i0111);
	float4 g1111 = pseudoRandom(i1111);

	// 正規化
	g0000 = normalize(g0000);
	g1000 = normalize(g1000);
	g0100 = normalize(g0100);
	g1100 = normalize(g1100);
	g0010 = normalize(g0010);
	g1010 = normalize(g1010);
	g0110 = normalize(g0110);
	g1110 = normalize(g1110);
	g0001 = normalize(g0001);
	g1001 = normalize(g1001);
	g0101 = normalize(g0101);
	g1101 = normalize(g1101);
	g0011 = normalize(g0011);
	g1011 = normalize(g1011);
	g0111 = normalize(g0111);
	g1111 = normalize(g1111);

	// 各格子点のノイズの値を計算
	float n0000 = dot(g0000, p0000);
	float n1000 = dot(g1000, p1000);
	float n0100 = dot(g0100, p0100);
	float n1100 = dot(g1100, p1100);
	float n0010 = dot(g0010, p0010);
	float n1010 = dot(g1010, p1010);
	float n0110 = dot(g0110, p0110);
	float n1110 = dot(g1110, p1110);
	float n0001 = dot(g0001, p0001);
	float n1001 = dot(g1001, p1001);
	float n0101 = dot(g0101, p0101);
	float n1101 = dot(g1101, p1101);
	float n0011 = dot(g0011, p0011);
	float n1011 = dot(g1011, p1011);
	float n0111 = dot(g0111, p0111);
	float n1111 = dot(g1111, p1111);

	// 補間係数を求める
	float4 u_xyzw = interpolate(f);
	// 補間
	float4 n_0w = lerp(float4(n0000, n1000, n0100, n1100), float4(n0001, n1001, n0101, n1101), u_xyzw.w);
	float4 n_1w = lerp(float4(n0010, n1010, n0110, n1110), float4(n0011, n1011, n0111, n1111), u_xyzw.w);
	float4 n_zw = lerp(n_0w, n_1w, u_xyzw.z);
	float2 n_yzw = lerp(n_zw.xy, n_zw.zw, u_xyzw.y);
	float  n_xyzw = lerp(n_yzw.x, n_yzw.y, u_xyzw.x);

	return n_xyzw;
}

float originalPerlinNoise(float2 P)
{
	int octaves = 5;
	float value = 0.0;
	float amp = 0.5;
	for (int i = 0; i < octaves; i++)
	{
		value += _simplexNoise(P)*amp;
		P *= 2.0;
		amp *= 0.5;
	}
	return value;
}
float originalPerlinNoise(float3 P)
{
	int octaves = 5;
	float value = 0.0;
	float amp = 0.5;
	for (int i = 0; i < octaves; i++)
	{
		value += _originalPerlinNoise(P)*amp;
		P *= 2.0;
		amp *= 0.5;
	}
	return value;
}
float originalPerlinNoise(float4 P)
{
	int octaves = 5;
	float value = 0.0;
	float amp = 0.5;
	for (int i = 0; i < octaves; i++)
	{
		value += _originalPerlinNoise(P)*amp;
		P *= 2.0;
		amp *= 0.5;
	}
	return value;
}
#endif


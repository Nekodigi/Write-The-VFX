#ifndef SIMPLEX_NOISE
#define SIMPLEX_NOISE

// https://github.com/stegu/webgl-noise/blob/master/src/noise2D.glsl
//
// Description : Array and textureless GLSL 2D simplex noise function.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise
// 

// 289の剰余を求める
float4 mod289(float4 x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float3 mod289(float3 x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float2 mod289(float2 x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float mod289(float x)
{
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}

// 0～288の値を重複なく並べ換える
float4 permute(float4 x)
{
	return mod289(((x * 34.0) + 1.0) * x);
}
float3 permute(float3 x)
{
	return mod289(((x * 34.0) + 1.0) * x);
}
float permute(float x)
{
	return mod289(((x * 34.0) + 1.0) * x);
}

// rは0.7近辺の値として 1.0/sqrt(r) のテイラー展開による近似
float4 taylorInvSqrt(float4 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}
float3 taylorInvSqrt(float3 r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}
float taylorInvSqrt(float r)
{
	return 1.79284291400159 - 0.85373472095314 * r;
}

// 4次元の勾配ベクトル
float4 grad4(float j, float4 ip)
{
	const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
	float4 p, s;
	p.xyz = floor(frac((float3)j * ip.xyz) * 7.0) * ip.z - 1.0;
	p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
	// lessthan GLSL -> HLSL
	// https://gist.github.com/fadookie/25adf86ae7e2753d717c
	s = float4(1.0 - step((float4)0.0, p));
	p.xyz = p.xyz + (s.xyz * 2.0 - 1.0) * s.www;
	return p;
}

// (sqrt(5.0) - 1.0) / 4.0 = F4
#define F4 0.309016994374947451

// Simplex Noise 2D
float _simplexNoise(float2 v)
{
	// 定数
	const float4 C = float4
	(
		0.211324865405187,   // (3.0-sqrt(3.0))/6.0
		0.366025403784439,   // 0.5*(sqrt(3.0)-1.0)
		-0.577350269189626,  // -1.0 + 2.0 * C.x
		0.024390243902439    // 1.0 / 41.0
	);

	float2 i  = floor(v + dot(v, C.yy)); // 変形した座標の整数部
	float2 x0 = v - i + dot(i, C.xx);    // 単体1つめの頂点 
	float2 x1 = x0.xy + C.xx;			 // 単体2つめの頂点
	float2 x2 = x0.xy + C.zz;			 // 単体3つめの頂点

	// 単体のユニットの原点（x0）からの相対的なx, y成分を比較し、
	// 2つめの頂点の座標がどちらであるか判定
	float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
	x1 -= i1;

	// 勾配ベクトル計算時のインデックスを並べ替え
	i = mod289(i); // 並べ換え時、オーバーフローが起きないように値を0～288に制限
	float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0))
		+ i.x + float3(0.0, i1.x, 1.0));

	// 放射状円ブレンドカーネル（放射円状に減衰）
	float3 m = max(0.5 - float3(dot(x0, x0), dot(x1.xy, x1.xy), dot(x2.xy, x2.xy)), 0.0);
	m = m * m;
	m = m * m;

	// 勾配を計算
	// 2次元正軸体（45°回転した四角形）の境界に均一に分散した41個の点
	// 41という数字は、ほどよく分散しかつ、41×7=287と289 に近い数値であるから
	float3 x  = 2.0 * frac(p * C.www) - 1.0; // -1.0～1.0の範囲で41個に分布したx軸の値
	float3 h  = abs(x) - 0.5;				 // 勾配のy成分
	float3 ox = floor(x + 0.5);				 // 四捨五入(=round())
	float3 a0 = x - ox;						 // 勾配のx成分

	// mをスケーリングすることで、間接的に勾配ベクトルを正規化
	m *= taylorInvSqrt(a0*a0 + h*h);

	// 点Pにおけるノイズの値を計算
	float3 g;
	g.x  = a0.x  * x0.x               + h.x  * x0.y;
	g.yz = a0.yz * float2(x1.x, x2.x) + h.yz * float2(x1.y, x2.y);

	// 値の範囲が[-1, 1]となるように、任意の因数でスケーリング
	return 130.0 * dot(m, g);
}
// Simplex Noise 3D
float _simplexNoise(float3 v)
{
	// 定数
	const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
	const float4 D = float4(0.0, 0.5, 1.0, 2.0);

	float3 i  = floor(v + dot(v, C.yyy)); // 変形した座標の整数部
	float3 x0 = v   - i + dot(i, C.xxx);  // 単体1つめの頂点 
	
	float3 g = step(x0.yzx, x0.xyz);	  // 成分比較
	float3 l = 1.0 - g;
	float3 i1 = min(g.xyz, l.zxy);
	float3 i2 = max(g.xyz, l.zxy);

	//     x0 = x0 - 0. + 0.0 * C       // 単体1つめの頂点 
	float3 x1 = x0 - i1 + 1.0 * C.xxx;	// 単体2つめの頂点 
	float3 x2 = x0 - i2 + 2.0 * C.xxx;	// 単体3つめの頂点 
	float3 x3 = x0 - 1. + 3.0 * C.xxx;	// 単体4つめの頂点 

	// 勾配ベクトル計算時のインデックスを並べ替え
	i = mod289(i);
	float4 p = permute(permute(permute(
		  i.z + float4(0.0, i1.z, i2.z, 1.0))
		+ i.y + float4(0.0, i1.y, i2.y, 1.0))
		+ i.x + float4(0.0, i1.x, i2.x, 1.0));

	// 勾配ベクトルを計算
	float  n_ = 0.142857142857; // 1.0 / 7.0
	float3 ns = n_ * D.wyz - D.xzx;

	float4 j = p - 49.0 * floor(p * ns.z * ns.z);	// fmod(p, 7*7)

	float4 x_ = floor(j * ns.z);
	float4 y_ = floor(j - 7.0 * x_); // fmod(j, N)

	float4 x = x_ * ns.x + ns.yyyy;
	float4 y = y_ * ns.x + ns.yyyy;
	float4 h = 1.0 - abs(x) - abs(y);

	float4 b0 = float4(x.xy, y.xy);
	float4 b1 = float4(x.zw, y.zw);

	float4 s0 = floor(b0) * 2.0 + 1.0;
	float4 s1 = floor(b1) * 2.0 + 1.0;
	float4 sh = -step(h, float4(0.0, 0.0, 0.0, 0.0));

	float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

	float3 p0 = float3(a0.xy, h.x);
	float3 p1 = float3(a0.zw, h.y);
	float3 p2 = float3(a1.xy, h.z);
	float3 p3 = float3(a1.zw, h.w);

	// 勾配を正規化
	float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;

	// 放射円状ブレンドカーネル（放射円状に減衰）
	float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
	m = m * m;
	// 最終的なノイズの値を算出
	return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1),
		dot(p2, x2), dot(p3, x3)));
}

#define F4 0.309016994374947451

// Simplex Noise 4D
float _simplexNoise(float4 v)
{
	// 定数
	const float4 C = float4(
		0.138196601125011,   // (5 - sqrt(5))/20  G4
		0.276393202250021,   // 2 * G4
		0.414589803375032,   // 3 * G4
		-0.447213595499958); // -1 + 4 * G4

	float4 i = floor(v + dot(v, (float4)F4)); // 変形した座標の整数部
	float4 x0 = v - i + dot(i, C.xxxx);       // 単体1つめの頂点
	
	// 点Pが属する単体の判定のための順序付け
	// by Bill Licea-Kane, AMD (formerly ATI)
	float4 i0;
	float3 isX = step(x0.yzw, x0.xxx);
	float3 isYZ = step(x0.zww, x0.yyz);
	// i0.x  = dot(isX, float3(1.0, 1.0, 1.0));
	i0.x = isX.x + isX.y + isX.z;
	i0.yzw = 1.0 - isX;
	// i0.y += dot(isYZ.xy, float2(1.0, 1.0));
	i0.y += isYZ.x + isYZ.y;
	i0.zw += 1.0 - isYZ.xy;
	i0.z += isYZ.z;
	i0.w += 1.0 - isYZ.z;

	// i0のそれぞれの成分に0,1,2,3のどれかの値を含む
	float4 i3 = clamp(i0, 0.0, 1.0);
	float4 i2 = clamp(i0 - 1.0, 0.0, 1.0);
	float4 i1 = clamp(i0 - 2.0, 0.0, 1.0);

	//     x0 = x0 - 0.0 + 0.0 * C.xxxx  // 単体1つめの頂点
	float4 x1 = x0 - i1 + 1.0 * C.xxxx;  // 単体2つめの頂点
	float4 x2 = x0 - i2 + 2.0 * C.xxxx;	 // 単体3つめの頂点
	float4 x3 = x0 - i3 + 3.0 * C.xxxx;	 // 単体4つめの頂点
	float4 x4 = x0 - 1. + 4.0 * C.xxxx;  // 単体5つめの頂点


	// 勾配ベクトル計算時のインデックスを並べ替え
	i = mod289(i);
	float  j0 = permute(permute(permute(permute(i.w) + i.z) + i.y) + i.x);
	float4 j1 = permute(permute(permute(permute(
		i.w + float4(i1.w, i2.w, i3.w, 1.0))
		+ i.z + float4(i1.z, i2.z, i3.z, 1.0))
		+ i.y + float4(i1.y, i2.y, i3.y, 1.0))
		+ i.x + float4(i1.x, i2.x, i3.x, 1.0));

	// 勾配ベクトルを計算
	float4 ip = float4(1.0 / 294.0, 1.0 / 49.0, 1.0 / 7.0, 0.0);

	float4 p0 = grad4(j0, ip);
	float4 p1 = grad4(j1.x, ip);
	float4 p2 = grad4(j1.y, ip);
	float4 p3 = grad4(j1.z, ip);
	float4 p4 = grad4(j1.w, ip);

	// 勾配ベクトルを正規化
	float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	p4 *= taylorInvSqrt(dot(p4, p4));

	// 放射円状ブレンドカーネル（放射円状に減衰）
	float3 m0 = max(0.6 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
	float2 m1 = max(0.6 - float2(dot(x3, x3), dot(x4, x4)), 0.0);
	m0 = m0 * m0;
	m1 = m1 * m1;
	// 最終的なノイズの値を算出（5つの角からの影響を計算）
	return 49.0 * (
		  dot(m0 * m0, float3(dot(p0, x0), dot(p1, x1), dot(p2, x2)))
		+ dot(m1 * m1, float2(dot(p3, x3), dot(p4, x4)))
		);
}

float simplexNoise(float2 P)
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
float simplexNoise(float3 P)
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
float simplexNoise(float4 P)
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
#endif


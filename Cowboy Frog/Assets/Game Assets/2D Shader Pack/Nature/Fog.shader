Shader "2D Shader Pack/Nature/Fog" {
	Properties {
		[HideInInspector]_MainTex ("Main", 2D) = "white" {}
		_Light1Color ("Light 1 Color", Color) = (1.0, 0.3, 0.3, 1)
		_Light1Pos   ("Light 1 Position", Vector) = (0, 0, 0, 0)
		_Light2Color ("Light 2 Color", Color) = (0.3, 1.0, 0.3, 1)
		_Light2Pos   ("Light 2 Position", Vector) = (0, 0, 0, 0)
		_Light3Color ("Light 3 Color", Color) = (0.3, 0.3, 1.0, 1)
		_Light3Pos   ("Light 3 Position", Vector) = (0, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
			//////////////////////////////////////////////////////////////////////////////////////////////
			float3 mod289 (float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
			float4 mod289 (float4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
			float4 permute (float4 x) { return mod289(((x * 34.0) + 1.0) * x); }
			float4 taylorInvSqrt (float4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

			float snoise (float3 v)
			{
				const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
				const float4 D = float4(0.0, 0.5, 1.0, 2.0);

				float3 i = floor(v + dot(v, C.yyy));
				float3 x0 = v - i + dot(i, C.xxx);

				float3 g = step(x0.yzx, x0.xyz);
				float3 l = 1.0 - g;
				float3 i1 = min(g.xyz, l.zxy);
				float3 i2 = max(g.xyz, l.zxy);

				float3 x1 = x0 - i1 + C.xxx;
				float3 x2 = x0 - i2 + C.yyy;
				float3 x3 = x0 - D.yyy;

				i = mod289(i);
				float4 p = permute(permute(permute(
							i.z + float4(0.0, i1.z, i2.z, 1.0 ))
							+ i.y + float4(0.0, i1.y, i2.y, 1.0))
							+ i.x + float4(0.0, i1.x, i2.x, 1.0));

				float n_ = 0.142857142857; // 1.0/7.0
				float3 ns = n_ * D.wyz - D.xzx;

				float4 j = p - 49.0 * floor(p * ns.z * ns.z);

				float4 x_ = floor(j * ns.z);
				float4 y_ = floor(j - 7.0 * x_);

				float4 x = x_ *ns.x + ns.yyyy;
				float4 y = y_ *ns.x + ns.yyyy;
				float4 h = 1.0 - abs(x) - abs(y);

				float4 b0 = float4(x.xy, y.xy);
				float4 b1 = float4(x.zw, y.zw);

				float4 s0 = floor(b0) * 2.0 + 1.0;
				float4 s1 = floor(b1) * 2.0 + 1.0;
				float4 sh = -step(h, float4(0, 0, 0, 0));

				float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
				float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

				float3 p0 = float3(a0.xy, h.x);
				float3 p1 = float3(a0.zw, h.y);
				float3 p2 = float3(a1.xy, h.z);
				float3 p3 = float3(a1.zw, h.w);

				float4 norm = taylorInvSqrt(float4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
				p0 *= norm.x;
				p1 *= norm.y;
				p2 *= norm.z;
				p3 *= norm.w;

				float4 m = max(0.6 - float4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
				m = m * m;
				return 42.0 * dot(m * m, float4(dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
			}
			float normnoise (float f) { return 0.5 * (f + 1.0); }

			float clouds (float2 uv)
			{
				float t = _Time.y;
				uv += float2(t * 0.05, t * 0.01);

				float2 off1 = float2(50.0,33.0);
				float2 off2 = float2(0.0, 0.0);
				float2 off3 = float2(-300.0, 50.0);
				float2 off4 = float2(-100.0, 200.0);
				float2 off5 = float2(400.0, -200.0);
				float2 off6 = float2(100.0, -1000.0);
				float scale1 = 3.0;
				float scale2 = 6.0;
				float scale3 = 12.0;
				float scale4 = 24.0;
				float scale5 = 48.0;
				float scale6 = 96.0;
				return normnoise(snoise(float3((uv+off1)*scale1, t*0.5)) * 0.8 +
								snoise(float3((uv+off2)*scale2, t*0.4)) * 0.4 +
								snoise(float3((uv+off3)*scale3, t*0.1)) * 0.2 +
								snoise(float3((uv+off4)*scale4, t*0.7)) * 0.1 +
								snoise(float3((uv+off5)*scale5, t*0.2)) * 0.05 +
								snoise(float3((uv+off6)*scale6, t*0.3)) * 0.025);
			}
			//////////////////////////////////////////////////////////////////////////////////////////////
			float4 _Light1Pos, _Light1Color, _Light2Pos, _Light2Color, _Light3Pos, _Light3Color;

			half4 frag (v2f_img input) : SV_Target
			{
				float2 uv = input.uv;
				float3 c1 = _Light1Color.rgb;
				float3 c2 = _Light2Color.rgb;
				float3 c3 = _Light3Color.rgb;

				float cloud1 = 0.7 * (1.0 - (2.5 * distance(uv, _Light1Pos.xy)));
				float light1 = 1.0 / (100.0 * distance(uv, _Light1Pos.xy));

				float cloud2 = 0.7 * (1.0 - (2.5 * distance(uv, _Light2Pos.xy)));
				float light2 = 1.0 / (100.0 * distance(uv, _Light2Pos.xy));

				float cloud3 = 0.7 * (1.0 - (2.5 * distance(uv, _Light3Pos.xy)));
				float light3 = 1.0 / (100.0 * distance(uv, _Light3Pos.xy));

				float3 f = clouds(uv).xxx;
				return float4(cloud1 * f * c1 + light1 * c1 + cloud2 * f * c2 + light2 * c2 + cloud3 * f * c3 + light3 * c3, 1.0);
			}
			ENDCG
		}
	}
	Fallback Off
}
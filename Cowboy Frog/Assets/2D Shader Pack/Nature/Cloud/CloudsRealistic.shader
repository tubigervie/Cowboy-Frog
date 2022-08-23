Shader "2D Shader Pack/Nature/CloudRealistic" {
	Properties {
		[HideInInspector]_MainTex  ("Main", 2D) = "black" {}
		_CloudScale     ("Cloud Scale", Float) = 1.1
		_Speed          ("Speed", Float) = 0.03
		_CloudDark      ("Cloud Dark", Range(0, 1)) = 0.5
		_CloudCover     ("Cloud Cover", Range(-1, 4)) = 0.2
		_CloudAlpha     ("Cloud Alpha", Float) = 8.0
		_SkyTint        ("Sky Tint", Range(-0.5, 1)) = 0.5
		_SkyColorTop    ("Sky Color Top", Color) = (0.2, 0.4, 0.6, 1.0)
		_SkyColorBottom ("Sky Color Bottom", Color) = (0.4, 0.7, 1.0, 1.0)
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off Lighting Off ZWrite Off
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_ST;
			half _CloudScale, _Speed, _CloudDark, _CloudCover, _CloudAlpha, _SkyTint;
			half4 _SkyColorTop, _SkyColorBottom;

			struct v2f
			{
				float4 pos    : SV_POSITION;
				float2 uv     : TEXCOORD0;
				float4 scrpos : TEXCOORD1;
			};
			v2f vert (appdata_base input)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(input.vertex);
				o.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
				o.scrpos = ComputeScreenPos(o.pos);
				return o;
			}
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			float2 hash (float2 p) {
				p = float2(dot(p, float2(127.1 , 311.7)), dot(p, float2(269.5, 183.3)));
				return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
			}
			float noise (in float2 p) {
				const float K1 = 0.366025404; // ( sqrt ( 3 ) - 1 ) / 2 ; 
				const float K2 = 0.211324865; // ( 3 - sqrt ( 3 ) ) / 6 ; 
				float2 i = floor(p + (p.x + p.y) * K1);
				float2 a = p - i + (i.x + i.y) * K2;
				float2 o = (a.x > a.y) ? float2(1.0 , 0.0) : float2(0.0 , 1.0);
				float2 b = a - o + K2;
				float2 c = a - 1.0 + 2.0 * K2;
				float3 h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
				float3 n = h * h * h * h * float3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
				return dot(n, float3(70.0, 70.0, 70.0));
			}
			float fbm (float2 n) {
				float total = 0.0, amplitude = 0.1;
				for (int i = 0; i < 7; i++) {
					total += noise(n) * amplitude;
					n = mul(float2x2(1.6, 1.2, -1.2, 1.6), n);
					amplitude *= 0.4;
				}
				return total;
			}
			///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			#define FLT_MIN 1.175494351e-38
			half4 frag (v2f input) : SV_Target
			{
				float2 fragCoord = (input.scrpos.xy / (input.scrpos.w + FLT_MIN)) * _ScreenParams.xy;
				float2 p = fragCoord.xy / _ScreenParams.xy;
				half2 ratio = half2(_ScreenParams.x / _ScreenParams.y, 1.0);
				float2 uv = p * ratio;
				float time = _Time.y * _Speed;
				float q = fbm(uv * _CloudScale * 0.5);
				int i = 0;

				// ridged noise shape
				float r = 0.0;
				uv *= _CloudScale;
				uv -= q - time;
				float weight = 0.8;
				for (i = 0; i < 8; i++) {
					r += abs(weight * noise(uv));
					uv = mul(float2x2(1.6, 1.2, -1.2, 1.6), uv) + time;
					weight *= 0.7;
				}

				// noise shape
				float f = 0.0;
				uv = p * ratio;
				uv *= _CloudScale;
				uv -= q - time;
				weight = 0.7;
				for (i = 0; i < 8; i++) {
					f += weight * noise(uv);
					uv = mul(float2x2(1.6, 1.2, -1.2, 1.6), uv) + time;
					weight *= 0.6;
				}
				f *= r + f;

				// noise colour
				float c = 0.0;
				time = _Time.y * _Speed * 2.0;
				uv = p * ratio;
				uv *= _CloudScale * 2.0;
				uv -= q - time;
				weight = 0.4;
				for (i = 0; i < 7; i++) {
					c += weight * noise(uv);
					uv = mul(float2x2(1.6, 1.2, -1.2, 1.6), uv) + time;
					weight *= 0.6;
				}

				// noise ridge colour
				float c1 = 0.0;
				time = _Time.y * _Speed * 3.0;
				uv = p * ratio;
				uv *= _CloudScale * 3.0;
				uv -= q - time;
				weight = 0.4;
				for (i = 0; i < 7; i++) {
					c1 += abs(weight * noise(uv));
					uv = mul(float2x2(1.6, 1.2, -1.2, 1.6), uv) + time;
					weight *= 0.6;
				}
				c += c1;

				half3 skycolour = lerp(_SkyColorBottom, _SkyColorTop, p.y);
				half3 cloudcolour = half3(1.1, 1.1, 0.9) * saturate(_CloudDark + 0.3 * c);

				f = _CloudCover + _CloudAlpha * f * r;

				half3 result = lerp(skycolour, saturate(_SkyTint * skycolour + cloudcolour), saturate(f + c));
				return half4(result , 1.0) - 0.1;
			}
			ENDCG
		}
	}
	Fallback Off
}
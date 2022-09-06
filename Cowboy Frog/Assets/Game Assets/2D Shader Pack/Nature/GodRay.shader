Shader "2D Shader Pack/GodRay" {
	Properties {
		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color    ("Color", Color) = (1, 1, 1, 1)
		_Angle    ("Angle", Range(-1, 1)) = -0.3
		_Spread   ("Spread", Range(0, 1)) = 0.5
		_Position ("Position", Range(-1, 1)) = -0.2
		_Cutoff   ("Cutoff", Range(-1, 1)) = 0.1
		_Falloff  ("Falloff", Range(0, 1)) = 0.2
		_EdgeFade ("EdgeFade", Range(0, 1)) = 0.15
		_Speed    ("Speed", Range(0, 8)) = 1
		_Seed     ("Seed", Float) = 5
		_Ray1Density   ("Ray1 Density", Float) = 8
		_Ray2Density   ("Ray2 Density", Float) = 30
		_Ray2Intensity ("Ray2 Density", Range(0, 1)) = 0.3
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
			float random (float2 v) { return frac(sin(dot(v, float2(12.9898, 78.233))) * 43758.5453123); }
			float noise (in float2 uv)
			{
				float2 i = floor(uv);
				float a = random(i);
				float b = random(i + float2(1.0, 0.0));
				float c = random(i + float2(0.0, 1.0));
				float d = random(i + float2(1.0, 1.0));

				float2 f = frac(uv);
				float2 u = f * f * (3.0 - 2.0 * f);   // cubic hermine curve
				return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
			}
			float2x2 rotate (float angle)
			{
				float s = sin(angle);
				float c = cos(angle);
				return float2x2(float2(c, -s), float2(s, c));
			}
			float3 screen (float3 base, float3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }
			//////////////////////////////////////////////////////////////////////////////////////////////
			sampler2D _MainTex;
			float _Angle, _Position, _Spread, _Cutoff, _Falloff, _EdgeFade, _Speed, _Ray1Density, _Ray2Density, _Ray2Intensity, _Seed;
			float4 _Color;

			half4 frag (v2f_img input) : SV_Target
			{
				float2 uv = float2(input.uv.x, 1.0 - input.uv.y);

				float2 newUv = mul(rotate(_Angle), (uv - _Position))  / ((uv.y + _Spread) - (uv.y * _Spread));  // rotate + skew + move the UVs

				float2 ray1 = float2(newUv.x * _Ray1Density + sin(_Time.x * 1 * _Speed) * (_Ray1Density * 0.2) + _Seed, 1.0);
				float2 ray2 = float2(newUv.x * _Ray2Density + sin(_Time.x * 2 * _Speed) * (_Ray2Density * 0.2) + _Seed, 1.0);

				float cut = step(_Cutoff, newUv.x) * step(_Cutoff, 1.0 - newUv.x);
				ray1 *= cut;
				ray2 *= cut;

				float rays = saturate(noise(ray1) + (noise(ray2) * _Ray2Intensity));
				rays *= smoothstep(0.0, _Falloff, (1.0 - uv.y));
				rays *= smoothstep(0.0 + _Cutoff, _EdgeFade + _Cutoff, newUv.x);
				rays *= smoothstep(0.0 + _Cutoff, _EdgeFade + _Cutoff, 1.0 - newUv.x);

				half3 c = rays * _Color.rgb;
				c = screen(tex2D(_MainTex, input.uv).rgb, c);
				return half4(c, 1.0);
			}
			ENDCG
		}
	}
	Fallback Off
}
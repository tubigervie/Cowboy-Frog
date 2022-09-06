Shader "2D Shader Pack/Nature/Electricity" {
	Properties {
		_Color       ("Color", Color) = (1, 1, 1, 1)
		_Strength    ("Strength", Range(0, 200)) = 144
		_Height      ("Height", Range(0, 2)) = 0.44
		_GlowFallOff ("Glow FallOff", Range(0, 0.1)) = 0.01
		_Speed       ("Speed", Range(0, 3)) = 1.86
		_SampleDist  ("Sample Dist", Range(0, 0.04)) = 0.0076
		_Glow        ("Glow", Range(0, 1)) = 0.5
		_GlowHeight  ("Glow Height", Range(0, 8)) = 1.68
		_VertNoise   ("Vert Noise", Range(0, 1)) = 0.78
		[Header(RenderState)]
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend Src", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend Dst", Int) = 0
	}
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass {
			Blend [_BlendSrc] [_BlendDst]

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"
			float4 _Color;
			float _Strength, _GlowFallOff, _Glow, _GlowHeight;
			float _Height, _Speed, _SampleDist, _VertNoise;
			sampler3D _Global_NoiseTex;
			float4 frag (v2f_img input) : SV_Target
			{
				float2 uv = (input.uv - 0.5) * 2;
				float2 t = float2(_Speed * _Time.y * 0.6 - _VertNoise * abs(uv.y), _Speed * _Time.y);

				float xs0 = uv.x - _SampleDist;
				float xs1 = uv.x;
				float xs2 = uv.x + _SampleDist;
				float noise0 = tex3D(_Global_NoiseTex, float3(xs0, t)).r;
				float noise1 = tex3D(_Global_NoiseTex, float3(xs1, t)).r;
				float noise2 = tex3D(_Global_NoiseTex, float3(xs2, t)).r;

				float mid0 = _Height * (noise0 * 2 - 1) * (1 - xs0 * xs0);
				float mid1 = _Height * (noise1 * 2 - 1) * (1 - xs1 * xs1);
				float mid2 = _Height * (noise2 * 2 - 1) * (1 - xs2 * xs2);

				float dist0 = abs(uv.y - mid0);
				float dist1 = abs(uv.y - mid1);
				float dist2 = abs(uv.y - mid2);

				float glow = 1.0 - pow(0.25 * (dist0 + 2 * dist1 + dist2), _GlowFallOff);
				float ambGlow = _Glow * (1.0 - xs1 * xs1) * (1.0 - abs(_GlowHeight * uv.y));
				return (_Strength * glow * glow + ambGlow) * _Color;
			}
			ENDCG
		}
	}
	FallBack Off
}
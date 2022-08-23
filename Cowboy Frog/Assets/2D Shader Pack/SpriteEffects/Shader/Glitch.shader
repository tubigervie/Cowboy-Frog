Shader "2D Shader Pack/Sprite/Glitch" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Glitch)]
		_GlitchInterval   ("Interval Seconds", Float) = 0.16
		[Header(Displacement Glitch)]
		_DispProbability  ("Probability", Float) = 0.022
		_DispIntensity    ("Intensity", Float) = 0.09
		[Header(Color Glitch)]
		_ColorProbability ("Probability", Float) = 0.02
		_ColorIntensity   ("Intensity", Float) = 0.07
	}
	SubShader {
		Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		Cull Off Lighting Off ZWrite Off Fog { Mode Off } Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Common.cginc"

			float _GlitchInterval;
			float _DispIntensity, _DispProbability;
			float _ColorIntensity, _ColorProbability;

			float rand (float x, float y) { return frac(sin(x * 12.9898 + y * 78.233) * 43758.5453); }  // 0~1
			half4 frag (v2f input) : SV_Target
			{
				float it1 = floor(_Time.y / _GlitchInterval) * _GlitchInterval;  // interval time
				float it2 = it1 + 2.793;   // make value more random

				// position based random
				float t = it1 + UNITY_MATRIX_MV[0][3] + UNITY_MATRIX_MV[1][3];
				float t2 = it2 + UNITY_MATRIX_MV[0][3] + UNITY_MATRIX_MV[1][3];

				float rnd1 = rand(t, -t);
				float rnd2 = rand(t, t);

				float offset = float((rand(t2, t2) - 0.5) / 50.0);
				if (rnd1 < _DispProbability)
				{
					input.uv.x += (rand(floor(input.uv.y / (0.2 + offset)) - t, floor(input.uv.y / (0.2 + offset)) + t) - 0.5) * _DispIntensity;
					input.uv.x = clamp(input.uv.x, 0, 1);
				}

				// color shift
				float rShift = (rand(-t, t) - 0.5) * _ColorIntensity;
				float gShift = (rand(-t, -t) - 0.5) * _ColorIntensity;
				float bShift = (rand(-t2, -t2) - 0.5) * _ColorIntensity;

				half4 cc = tex2D(_MainTex, input.uv);
				half4 r = tex2D(_MainTex, float2(input.uv.x + rShift, input.uv.y + rShift));
				half4 g = tex2D(_MainTex, float2(input.uv.x + gShift, input.uv.y + gShift));
				half4 b = tex2D(_MainTex, float2(input.uv.x + bShift, input.uv.y + bShift));
				half4 c = 0.0;
				if (rnd2 < _ColorProbability)
				{
					c.r = r.r;
					c.g = g.g;
					c.b = b.b;
					c.a = (r.a + g.a + b.a) / 3.0;
				}
				else
				{
					c = cc;
				}
				c *= input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
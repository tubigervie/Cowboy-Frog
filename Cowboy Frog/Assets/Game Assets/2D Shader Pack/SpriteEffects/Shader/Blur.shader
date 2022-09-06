Shader "2D Shader Pack/Sprite/Blur" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Blur)]
		_Intensity ("Intensity", Range(1, 10)) = 3
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Common.cginc"

			half _Intensity;

			float BlurFactor (float s, float x) { return exp(-(x * x) / (2.0 * s * s)); }

			#define N         16
			#define HalfN     8
			#define PixelSize 0.00390625
			half4 DoBlur (float2 uv, sampler2D tex, float inten)
			{
				float sigma = 0.1 + inten * 0.5;
				float sum = 0.0;
				half4 ret = 0.0;
				for (int y = 0; y < N; ++y)
				{
					float fy = BlurFactor(sigma, float(y) - HalfN);
					float offsety = float(y - HalfN) * PixelSize;
					for (int x = 0; x < N; ++x)
					{
						float fx = BlurFactor(sigma, float(x) - HalfN);
						float offsetx = float(x - HalfN) * PixelSize;
						sum += fx * fy;
						ret += tex2D(tex, uv + float2(offsetx, offsety)) * fx * fy;
					}
				}
				return ret / sum;
			}
			half4 frag (v2f input) : SV_Target
			{
				half4 c = DoBlur(input.uv, _MainTex, _Intensity) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
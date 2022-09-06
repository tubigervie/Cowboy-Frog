Shader "2D Shader Pack/Sprite/Glow" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		[Header(Glow)]
		_Size      ("Size", Range(0, 0.1)) = 0.025
		_Color1    ("Color", COLOR) = (0, 0.7, 1, 1)
		_Intensity ("Intensity", Range(0, 1)) = 1
		_PureColor ("Pure Color", Range(0, 1)) = 1
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Common.cginc"

			half _Size, _Intensity, _PureColor;
			half4 _Color1;

			half4 Glow (sampler2D src, float2 uv, float size, float4 c, float intensity, float fade)
			{
				int samples = 32;
				int samples2 = samples * 0.5;
				float4 r = 0;

				for (int iy = -samples2; iy < samples2; iy++)
				{
					for (int ix = -samples2; ix < samples2; ix++)
					{
						float2 uv2 = float2(ix, iy);
						uv2 /= samples;
						uv2 *= size;
						uv2 = saturate(uv + uv2);
						r += tex2D(src, uv2);
					}
				}
				r = lerp(0, r / (samples * samples), intensity * 4);
				r.rgb = c.rgb;
				float4 m = r;
				float4 o = tex2D(src, uv);
				r = lerp(r, o, o.a);
				r = lerp(m, r, fade);
				return r;
			}
			half4 frag (v2f input) : SV_Target
			{
				return Glow(_MainTex, input.uv, _Size, _Color1, _Intensity, 1.0 - _PureColor) * input.col;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}

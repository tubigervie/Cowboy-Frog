Shader "2D Shader Pack/Sprite/Thermal" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Thermal)]
		_CoolColor ("Cool", Color) = (0, 0, 1, 1)
		_MidColor  ("Mid", Color) = (1, 1, 0, 1)
		_WarmColor ("Warm", Color) = (1, 0, 0, 1)
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

			half3 _CoolColor, _MidColor, _WarmColor;

			half4 frag (v2f input) : SV_Target
			{
				half4 c = tex2D(_MainTex, input.uv) * input.col;
				float lum = Luminance(c.rgb);

				float ix = step(0.5, lum);
				half3 range1 = lerp(_CoolColor, _MidColor, (lum - ix * 0.5) * 2.0);
				half3 range2 = lerp(_MidColor, _WarmColor, (lum - ix * 0.5) * 2.0);
				half3 res = lerp(range1, range2, ix);

				res *= c.a;
				return half4(res, c.a);
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
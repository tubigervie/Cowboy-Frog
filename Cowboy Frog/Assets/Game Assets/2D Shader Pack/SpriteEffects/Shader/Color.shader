Shader "2D Shader Pack/Sprite/Color" {
	Properties {
		[PerRendererData] _MainTex ("Main", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Color)]
		_ColorOnly         ("Color Only", Color) = (0, 0, 0, 0)
		[Toggle] _AddColor ("Add", Range(0, 1)) = 0
		_GrayScale         ("Gray Scale", Range(0, 1)) = 0
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

			half4 _ColorOnly;
			half _AddColor, _GrayScale;

			half4 frag (v2f i) : SV_Target
			{
				half4 c = tex2D(_MainTex, i.uv) * i.col;
				half3 c2 = lerp(_ColorOnly.rgb, c.rgb + _ColorOnly.rgb, _AddColor);
				c.rgb = lerp(c.rgb, c2, _ColorOnly.a);
				c.rgb *= c.a;

				half3 gs = dot(c.rgb, half3(0.3, 0.59, 0.11));
				c.rgb = lerp(c.rgb, gs, _GrayScale);
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
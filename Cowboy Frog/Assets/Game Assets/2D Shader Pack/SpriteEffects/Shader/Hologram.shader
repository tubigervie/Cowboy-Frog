Shader "2D Shader Pack/Sprite/Hologram" {
	Properties {
		[PerRendererData] _MainTex ("Main", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Hologram)]
		_ScanlineColor ("Scaneline", Color) = (1, 1, 1, 1)
		_ScanlineSpeed ("Scaneline Speed", Float) = 1
		_ScanlineScale ("Scaneline Scale", Float) = 200
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

			half3 _ScanlineColor;
			half _ScanlineSpeed, _ScanlineScale;

			half4 frag (v2f input) : SV_Target
			{
				float l = frac((input.uv.y * _ScanlineScale) + _Time.y * _ScanlineSpeed);

				half4 c = tex2D(_MainTex, input.uv) * input.col;
				c.rgb = c.rgb + l * _ScanlineColor;
				c.rgb *= (c.a * l);
				c.a *= l;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
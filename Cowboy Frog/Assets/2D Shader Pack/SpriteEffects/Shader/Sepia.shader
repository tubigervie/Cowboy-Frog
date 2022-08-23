Shader "2D Shader Pack/Sprite/Sepia" {
	Properties {
		[PerRendererData] _MainTex ("Main", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Sepia)]
		[KeywordEnum(Mode1, Mode2)] _Sepiamode ("Sepia Mode", Float) = 0
		_Sepia ("Sepia", Range(0, 1)) = 0
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
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _SEPIAMODE_MODE1 _SEPIAMODE_MODE2
			#include "UnityCG.cginc"
			#include "Common.cginc"

			half _Sepia;

			half4 frag (v2f i) : SV_Target
			{
				half4 c = tex2D(_MainTex, i.uv) * i.col;
#if _SEPIAMODE_MODE1
				half lumn = Luminance(c.rgb);
				half3 c2 = lumn.xxx * half3(1.2, 1.0, 0.8);
#elif _SEPIAMODE_MODE2
				half3 c2;
				c2.r = dot(c.rgb, half3(0.393, 0.769, 0.189));
				c2.g = dot(c.rgb, half3(0.349, 0.686, 0.168));
				c2.b = dot(c.rgb, half3(0.272, 0.534, 0.131));
#endif
				c.rgb = lerp(c.rgb, c2, _Sepia);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
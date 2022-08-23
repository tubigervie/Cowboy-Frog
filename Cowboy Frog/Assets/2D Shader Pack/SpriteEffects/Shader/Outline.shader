Shader "2D Shader Pack/Sprite/Outline" {
	Properties {
		[PerRendererData] _MainTex ("Main", 2D) = "white" {}
		_Color ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Outline)]
		_OtlColor         ("Color", Color) = (1, 1, 1, 1)
		_OtlThickness     ("Thickness", Range(0, 2)) = 1
		[Toggle] _OtlOnly ("Outline Only", Range(0, 1)) = 0
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
			#include "UnityCG.cginc"
			#include "Common.cginc"

			half4 _OtlColor;
			half _OtlThickness, _OtlOnly;

			half4 frag (v2f input) : SV_Target
			{
				half4 c = tex2D(_MainTex, input.uv) * input.col;
				c.rgb *= c.a;

				half4 oc = _OtlColor;
				oc.a *= ceil(c.a);
				oc.rgb *= oc.a;

				half u = tex2D(_MainTex, input.uv + half2(0, _MainTex_TexelSize.y * _OtlThickness)).a;
				half d = tex2D(_MainTex, input.uv - half2(0, _MainTex_TexelSize.y * _OtlThickness)).a;
				half r = tex2D(_MainTex, input.uv + half2(_MainTex_TexelSize.x * _OtlThickness, 0)).a;
				half l = tex2D(_MainTex, input.uv - half2(_MainTex_TexelSize.x * _OtlThickness, 0)).a;

				c = lerp(c, 0, _OtlOnly);
				return lerp(oc, c, ceil(u * d * r * l));
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
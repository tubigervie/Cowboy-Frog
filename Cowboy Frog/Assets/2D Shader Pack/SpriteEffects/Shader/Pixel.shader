Shader "2D Shader Pack/Sprite/Pixel" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Pixel)]
		_Pixels ("Pixels", Range(0.001, 0.06)) = 0.01
		[Toggle] _Gameboy ("Gameboy", Range(0, 1)) = 0
		[HideInInspector] _Darkest ("Darkest", Color) = (0.0588235, 0.21961, 0.0588235)
		[HideInInspector] _Dark    ("Dark", Color) = (0.188235, 0.38431, 0.188235)
		[HideInInspector] _Ligt    ("Light", Color) = (0.545098, 0.6745098, 0.0588235)
		[HideInInspector] _Ligtest ("Lightest", Color) = (0.607843, 0.7372549, 0.0588235)
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

			float _Pixels, _Gameboy;
			float4 _Darkest, _Dark, _Ligt, _Ligtest;

			float4 frag (v2f input) : SV_Target
			{
				float2 sz = _Pixels.xx;

				float2 uv = input.uv;
				uv /= sz;
				uv = round(uv);
				uv *= sz;

				float4 c1 = tex2D(_MainTex, uv) * input.col;

				float luma = dot(c1.rgb, float3(0.2126, 0.7152, 0.0722));
				float posterized = floor(luma * 4) / (4 - 1);
				float luma3 = posterized * 3.0;

				float darkest = saturate(luma3);
				float4 c2 = lerp(_Darkest, _Dark, darkest);

				float light = saturate(luma3 - 1.0);
				c2 = lerp(c2, _Ligt, light);

				float lightest = saturate(luma3 - 2.0);
				c2 = lerp(c2, _Ligtest, lightest);

				return lerp(c1, c2, _Gameboy) * c1.a;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
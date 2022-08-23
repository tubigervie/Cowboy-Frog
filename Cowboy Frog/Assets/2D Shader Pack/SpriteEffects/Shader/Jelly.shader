Shader "2D Shader Pack/Sprite/Jelly" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Jelly)]
		_Distortion ("Distortion", Range(0, 2)) = 0.1
		_Speed      ("Speed", Range(1, 16)) = 1
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

			float _Distortion, _Speed;

			float4 frag (v2f input) : SV_Target
			{
				float2 uv = input.uv;
				float t = _Time.y * _Speed;
				uv.x += (sin(uv.y + t) * 0.019 * _Distortion);
				uv.y += (cos(uv.x + t) * 0.009 * _Distortion);
				uv.x = lerp(uv.x, input.uv.x, 1.0 - input.uv.y);
				uv.y = lerp(uv.y, input.uv.y, 1.0 - input.uv.y);

				float4 c = tex2D(_MainTex, uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
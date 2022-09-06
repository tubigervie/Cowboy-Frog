Shader "2D Shader Pack/Sprite/Shake" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Shake)]
		RangeX ("Range X", Range(0, 0.1)) = 0
		RangeY ("Range Y", Range(0, 0.1)) = 0.05
		Speed  ("Speed", Range(0, 1)) = 0.02
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

			half RangeX, RangeY, Speed;

			float2 ShakeUV (float2 uv, float rngx, float rngy, float speed)
			{
				float t1 = sin(_Time.x * speed * 5000.0);
				float t2 = sin(_Time.x * speed * 5000.0);
				uv += float2(rngx * t1, rngy * t2);
				return uv;
			}
			half4 frag (v2f input) : SV_Target
			{
				float2 uv = ShakeUV(input.uv, RangeX, RangeY, Speed);
				half4 c = tex2D(_MainTex, uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
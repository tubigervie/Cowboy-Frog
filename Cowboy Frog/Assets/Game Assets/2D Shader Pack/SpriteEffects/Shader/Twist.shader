Shader "2D Shader Pack/Sprite/Twist" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Twist)]
		_Amount ("Amount", Range(0, 1)) = 1
		_Speed  ("Speed", Range(0, 4)) = 1
		_PosX   ("PosX", Range(-1, 2)) = 0.5
		_PosY   ("PosY", Range(-1, 2)) = 0.3
		_Radius ("Radius", Range(0, 1)) = 0.3
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

			float _Amount, _PosX, _PosY, _Radius, _Speed;

			float2 TwistUV (float2 uv, float f, float x, float y, float r)
			{
				float2 tc = uv - float2(x, y);
				float d = length(tc);
				if (d < r)
				{
					float percent = (r - d) / r;
					float theta = percent * percent * 16.0 * sin(f);
					float s = sin(theta);
					float c = cos(theta);
					tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
				}
				tc += float2(x, y);
				return tc;
			}
			float4 frag (v2f input) : SV_Target
			{
				float2 uv = TwistUV(input.uv, _Amount * sin(_Time.y * _Speed), _PosX, _PosY, _Radius);
				float4 c = tex2D(_MainTex, uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
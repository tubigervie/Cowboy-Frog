Shader "2D Shader Pack/Sprite/LiquidWave" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Liquid Wave)]
		_WaveX     ("Wave X", Range(0, 8)) = 1
		_WaveY     ("Wave Y", Range(0, 8)) = 1
		_DistanceX ("Distance X", Range(0, 1)) = 0.1
		_DistanceY ("Distance Y", Range(0, 1)) = 0.1
		_Speed     ("Speed", Range(-6, 6)) = 1
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

			float _WaveX, _WaveY, _DistanceX, _DistanceY, _Speed;

			float2 WaveUV (float2 p, float wx, float wy, float dx, float dy, float speed)
			{
				speed *= _Time.y;
				float x = sin(p.y * 4 * wx + speed);
				float y = cos(p.x * 4 * wy + speed);
				x += sin(p.x) * 0.1;
				y += cos(p.y) * 0.1;
				x *= y;
				y *= x;
				x *= y + wy * 8;
				y *= x + wx * 8;
				p.x = p.x + x * dx * 0.015;
				p.y = p.y + y * dy * 0.015;
				return p;
			}
			float4 frag (v2f input) : SV_Target
			{
				float2 uv = WaveUV(input.uv, _WaveX, _WaveY, _DistanceX, _DistanceY, _Speed);
				float4 c = tex2D(_MainTex, uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
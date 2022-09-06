Shader "2D Shader Pack/Sprite/Rainbow" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Rainbow)]
		_Strength ("Strength", Range(0, 1)) = 0.5
		_Speed    ("Speed", Range(0, 10)) = 0.5
		_Angle    ("Size", Range(0, 360)) = 0
		_Scale    ("Scale", Vector) = (1, 1, 0, 0)
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

			half _Strength, _Speed, _Angle;
			half2 _Scale;

			float mod (float x, float y) { return x - y * floor(x / y); }
			half4 frag (v2f input) : SV_Target
			{
				float2 uv = input.uv * _Scale;
				float hue = uv.x * cos(radians(_Angle)) - uv.y * sin(radians(_Angle));
				hue = frac(hue + frac(_Time.y  * _Speed));
				float x = 1.0 - abs(mod(hue / (1.0 / 6.0), 2.0) - 1.0);
				float3 rainbow = 0.0;
				if (hue < 1.0 / 6.0)
					rainbow = float3(1, x, 0);
				else if (hue < 1.0 / 3.0)
					rainbow = float3(x, 1, 0);
				else if (hue < 0.5)
					rainbow = float3(0, 1, x);
				else if (hue < 2.0 / 3.0)
					rainbow = float3(0, x, 1);
				else if (hue < 5.0 / 6.0)
					rainbow = float3(x, 0, 1);
				else
					rainbow = float3(1, 0, x);

				half4 c = tex2D(_MainTex, input.uv) * input.col;
				c = lerp(c, half4(rainbow, c.a), _Strength);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
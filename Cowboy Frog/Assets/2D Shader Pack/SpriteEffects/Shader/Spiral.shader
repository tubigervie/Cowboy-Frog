Shader "2D Shader Pack/Sprite/Spiral" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Spiral)]
		_PosX      ("PosX", Range(0, 1)) = 0.5
		_PosY      ("PosY", Range(0, 1)) = 0.5
		_Size      ("Size", Range(0, 2)) = 1
		_LineSize  ("LineSize", Range(-16, 16)) = 1
		_Speed     ("Speed", Range(-2, 2)) = 0
		_Intensity ("Intensity", Range(-0.3, 0.3)) = 0.022
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

			half _PosX, _PosY, _Size, _LineSize, _Speed, _Intensity;

			half GenerateSpiral (half2 uv, half x, half y, half sz, half lineSz, half speed)
			{
				half t = _Time.x * speed * 8.0;
				half2 m = half2(x, y) - uv;
				half r = length(m) * sz;
				half v = sin(100.0 * (sqrt(r) - (0.02 * lineSz) * atan2(m.y, m.x) - 0.3 * t));
				return saturate(v);
			}
			half4 frag (v2f input) : SV_Target
			{
				half spiral = GenerateSpiral(input.uv, _PosX, _PosY, _Size, _LineSize, _Speed);
				half2 uv = DisplacementUV(input.uv, spiral * spiral, 0.0, _Intensity);
				half4 c = tex2D(_MainTex, uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
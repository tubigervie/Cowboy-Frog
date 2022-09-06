Shader "2D Shader Pack/Sprite/Shiny" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Shiny)][Space(5)]
		_ShinyColor ("Shiny Color", Color) = (1, 1, 1, 1)
		_Offset     ("Offset", Range(-1, 1)) = 0
		_Smooth     ("Smooth", Range(0, 3)) = 1
		_Intensity  ("Intensity", Range(0, 2)) = 1
		_Degree     ("Rotate", Range(-180, 180)) = 0
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

			half4 _ShinyColor;
			float _Offset, _Smooth, _Intensity, _Degree;

			float2 rotate2DPoint (float2 v, float2 center, half radian)
			{
				half radius = distance(v, center);
				half angle = atan((v.y - center.y) / (v.x - center.x)) - radian;
				v.x = cos(angle) * radius + center.x;
				v.y = sin(angle) * radius + center.y;
				return v;
			}
			half4 frag (v2f input) : SV_Target
			{
				float2 uv = input.uv;
				half4 c = tex2D(_MainTex, input.uv) * input.col;

				float2 normUv = input.uv;
				normUv.x += _Offset;
				normUv = CalcNormalizedUv(normUv);
				//return half4(normUv, 0, 1);

				normUv = rotate2DPoint(normUv, 0.5, radians(_Degree));

				half f = normUv.x * 2.0 - 1.0;
				f = abs(f);
				f = 1.0 - f;
				f = smoothstep(0, _Smooth, f);
				f *= _Intensity;
				//return half4(f, f, f, 1);

				c.rgb += (f * _ShinyColor.rgb);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
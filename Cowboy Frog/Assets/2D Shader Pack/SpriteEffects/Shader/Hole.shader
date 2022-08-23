Shader "2D Shader Pack/Sprite/Hole" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Hole)]
		_Center ("Center", Vector) = (0.5, 0.5, 0, 0)
		_Range  ("Range", Range(0, 0.5)) = 0.15
		[MaterialToggle] _Inside ("Inside", Float) = 0
		_Alpha  ("Alpha", Range(0, 1)) = 1.0
		_Smooth ("Smooth", Range(0, 0.3)) = 0.15
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
//		Blend One OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Common.cginc"

			half2 _Center;
			half _Range, _Inside, _Alpha, _Smooth;

			half4 frag (v2f input) : SV_Target
			{
				float2 uv = input.uv;
				half4 tc = tex2D(_MainTex, uv) * input.col;

				float dist = 1.0 - smoothstep(_Range, _Range + _Smooth, length(_Center - uv));
				float c = 0.0;
				if (_Inside == 0.0) { c = dist; } else { c = 1.0 - dist; }
				tc.a *= (c - (1.0 - _Alpha));
				return tc;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
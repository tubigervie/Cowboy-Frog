Shader "2D Shader Pack/Sprite/BlackWhite" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(BlackWhite)]
		_Threshold ("Threshold", Range(0, 1)) = 0
		_ThresholdSmooth ("Smooth", Range(0, 0.2)) = 0.0001
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

			float _Threshold, _ThresholdSmooth;

			float4 frag (v2f input) : SV_Target
			{
				float4 c = tex2D(_MainTex, input.uv);
				float l = (c.r + c.g + c.b) * 0.333;
				c.rgb = smoothstep(_Threshold, _Threshold + _ThresholdSmooth, l);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
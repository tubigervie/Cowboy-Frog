Shader "2D Shader Pack/Sprite/Dissolve" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Dissolve)]
		_DissolveTex          ("Dissolve", 2D) = "white" {}
		_DissolveSpeed        ("Speed", Range(0, 3)) = 1
		_DissolveBorderWidth  ("Border width", Float) = 10
		_DissolveGlowColor    ("Glow color", Color) = (1,1,1,1)
		_DissolveGlowStrength ("Glow strength", Range(0, 5)) = 1
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Common.cginc"

			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			half _DissolveSpeed, _DissolveBorderWidth, _DissolveGlowStrength;
			half4 _DissolveGlowColor;

			half3 DissolveBlending (half d, half width, half blendAmount, half3 c, half strength)
			{
				half remap = (blendAmount * -1) + 1;
				half extended = pow(((d * remap) * 5.0), 35.0);
				clip(clamp(extended, 0, 1) - 0.5);
				half a = step(width, extended);
				half b = step(extended, width);
				return c * (lerp(b, 0, a * b) * strength);
			}
			half4 frag (v2f i) : SV_Target
			{
				half d = tex2D(_DissolveTex, TRANSFORM_TEX(i.uv, _DissolveTex)).r;
				half db = sin(_Time.y * _DissolveSpeed) * 0.5 + 0.5;
				half3 emissive = DissolveBlending(d, _DissolveBorderWidth, db, _DissolveGlowColor.rgb, _DissolveGlowStrength);
			
				half4 c = tex2D(_MainTex, i.uv) * i.col;
				c.rgb *= c.a;
				c.rgb += emissive;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
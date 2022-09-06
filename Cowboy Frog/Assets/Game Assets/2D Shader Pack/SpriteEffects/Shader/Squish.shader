Shader "2D Shader Pack/Sprite/Squish" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Squish)]
		_Factor ("Factor", Float) = 2
		_Bulge  ("Bulge", Range(-1, 1)) = 0.5
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

			half4 _Color;
			sampler2D _MainTex;
			half _Factor, _Bulge;

			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 col : COLOR;
				float2 uv  : TEXCOORD0;
			};
			v2f vert (appdata_full v)
			{
				float4 p = v.vertex;
				p.x *= _Factor;

				v2f o;
				o.pos = UnityObjectToClipPos(p);
				o.uv = v.texcoord;
				o.col = v.color * _Color;
#ifdef PIXELSNAP_ON
				o.pos = UnityPixelSnap(o.pos);
#endif
				return o;
			}
			float bulge_function (float y) { return sqrt(1.0 - y * y); }
			half4 frag (v2f input) : SV_Target
			{
				float2 uv = input.uv * 2.0 - 1.0;
				uv.x *= _Factor;
				float disp = 1.0 + _Bulge * bulge_function(uv.y);
				uv.x /= disp;
				uv = (uv + 1.0) / 2.0;

				//return (uv.x >= 0.0 && uv.x <= 1.0) ? half4(uv, 0, 1) : half4(0, 0, 0, 0.5);
				if (uv.x >= 0.0 && uv.x <= 1.0)
				{
					half4 c = tex2D(_MainTex, uv) * input.col;
					c.rgb *= c.a;
					return c;
				}
				else
				{
					return 0;
				}
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
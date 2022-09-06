Shader "2D Shader Pack/Sprite/Reflection" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Reflection)]
		_Color1      ("Reflection Tint", COLOR) = (0.2, 0.2, 0.2, 1)
		_Offset      ("Offset", Vector) = (0.1, 0.1, 0, 0)
		_Height      ("Height", Range(0, 1)) = 0.2
		_HeightFade  ("Height Fade", Range(0, 1)) = 0.2
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

			sampler2D _MainTex;
			half4 _Color, _Color1, _Offset;
			float _Height, _HeightFade;

			struct v2f
			{
				float4 pos : SV_POSITION;
				half4 col  : COLOR;
				float2 uv  : TEXCOORD0;
			};
			v2f vert (appdata_full v)
			{
				float3 p = v.vertex;
				p.xy += _Offset.xy;
				p.y *= -1;

				v2f o;
				o.pos = UnityObjectToClipPos(p);
				o.uv = v.texcoord;
				o.col = v.color * _Color;
#ifdef PIXELSNAP_ON
				o.pos = UnityPixelSnap(o.pos);
#endif
				return o;
			}
			half4 frag (v2f input) : SV_Target
			{
				float h = input.uv.y;
				h = 1 - smoothstep(_Height, _Height + _HeightFade, h);
			
				half4 c = tex2D(_MainTex, input.uv) * input.col;
				c.rgb *= c.a;
				c.a *= h;
				c *= _Color1;
				return c;
			}
			ENDCG
		}
	}
	FallBack Off
}

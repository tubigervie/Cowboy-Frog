Shader "2D Shader Pack/Sprite/Default" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
			half4 _Color;
			struct v2f
			{
				float4 pos : SV_POSITION;
				half4 col  : COLOR;
				float2 uv  : TEXCOORD0;
			};
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.col = v.color * _Color;
#ifdef PIXELSNAP_ON
				o.pos = UnityPixelSnap(o.pos);
#endif
				return o;
			}
			half4 frag (v2f input) : SV_Target
			{
				return tex2D(_MainTex, input.uv) * input.col;
			}
			ENDCG
		}
	}
	Fallback Off
}

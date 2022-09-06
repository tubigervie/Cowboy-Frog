Shader "2D Shader Pack/ColorShift" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Hue)]
		_HRangeMin ("Hue Range Min", Range(0, 1)) = 0
		_HRangeMax ("Hue Range Max", Range(0, 1)) = 1
		_HOffset ("Hue Offset", Range(0, 1)) = 0
		_SOffset ("Saturation Offset", Range(-1, 0)) = 0
		_VOffset ("Brightness Offset", Range(0, 1)) = 0
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
			//////////////////////////////////////////////////////////////////////////////////////////////
			half4 _Color;
			sampler2D _MainTex;

			struct v2f
			{
				float4 pos : SV_POSITION;
				half4  col : COLOR;
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
			//////////////////////////////////////////////////////////////////////////////////////////////
			float3 rgb2hsv (float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			float3 hsv2rgb (float3 c)
			{
				c = float3(c.x, clamp(c.yz, 0.0, 1.0));
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}
			float _HRangeMin, _HRangeMax, _HOffset, _SOffset, _VOffset;
			float3 modify (float3 c)
			{
				float3 hsv = rgb2hsv(c.rgb);
				float f = step(_HRangeMin, hsv.r) * step(hsv.r, _HRangeMax);
				return hsv2rgb(hsv + float3(_HOffset, _SOffset, _VOffset) * f);
			}
			//////////////////////////////////////////////////////////////////////////////////////////////
			half4 frag (v2f i) : SV_Target
			{
				half4 c = tex2D(_MainTex, i.uv) * i.col;
				c.rgb *= c.a;
				c.rgb = modify(c.rgb);
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
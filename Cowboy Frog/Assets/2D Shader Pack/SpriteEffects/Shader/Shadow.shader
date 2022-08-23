Shader "2D Shader Pack/Sprite/Shadow" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color                     ("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Header(Shadow)]
		_Color1             ("Shadow Color", COLOR) = (0.2, 0.2, 0.2, 1)
		_Offset             ("Offset", Vector) = (0.1, 0.1, 0, 0)
		_Squish             ("Squish", Range(0, 1)) = 1
		_UvFadeLeft         ("Uv Fade Left Side", Float) = 0
		_UvFadeRight        ("Uv Fade Right Side", Float) = 1
		[Toggle(BLUR)] _1   ("Blur Enable", Float) = 0
		_Blur               ("Blur", Range(0, 0.04)) = 0.025
		[Toggle(FLIP_Y)] _2 ("Flip Y", Float) = 0
		_Slant              ("Slant", Range(-2, 2)) = 0
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
			#pragma multi_compile _ BLUR
			#pragma multi_compile _ FLIP_Y
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float _Blur, _Slant, _Squish, _UvFadeLeft, _UvFadeRight;
			half4 _Color, _Color1, _Offset;
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
				p.y *= _Squish;
#ifdef FLIP_Y
				p.x += (_Slant * smoothstep(0, 1, v.texcoord.y));
				p.y *= -1;
#endif
				v2f o;
				o.pos = UnityObjectToClipPos(p);
				o.uv = v.texcoord;
				o.col = v.color * _Color;
#ifdef PIXELSNAP_ON
				o.pos = UnityPixelSnap(o.pos);
#endif
				return o;
			}
			float4 Blur (sampler2D src, float2 uv, float size, float4 c)
			{
				int samples = 32;
				int samples2 = samples * 0.5;
				float4 r = 0;

				for (int iy = -samples2; iy < samples2; iy++)
				{
					for (int ix = -samples2; ix < samples2; ix++)
					{
						float2 uv2 = float2(ix, iy);
						uv2 /= samples;
						uv2 *= size;
						uv2 = saturate(uv + uv2);
						r += tex2D(src, uv2);
					}
				}
				r /= (samples * samples);
				return c * r.a;
			}
			half4 frag (v2f input) : SV_Target
			{
				float fade = smoothstep(_UvFadeLeft, _UvFadeRight, input.uv.y);
#ifdef BLUR
				half4 c = Blur(_MainTex, input.uv, _Blur, _Color1) * input.col;
				return half4(c.rgb, c.a * fade);
#else
				half4 c = tex2D(_MainTex, input.uv) * input.col;
				return half4(_Color1.rgb, c.a * fade);
#endif
			}
			ENDCG
		}
	}
	Fallback Off
}

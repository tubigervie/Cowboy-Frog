Shader "2D Shader Pack/Sprite/Scrawl" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1, 1, 1, 1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[Header(Scrawl)]
		_DoodleScale ("Doodle Scale", Range(0, 1)) = 0.2
		_SnappedTime ("Snapped Time", Range(0, 1)) = 0.2
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
			//////////////////////////////////////////////////////////////////////////////////////////////
			float _DoodleScale, _SnappedTime;
			float3 random3 (float3 c)   // discontinuous pseudorandom uniformly distributed in [-0.5, +0.5]^3
			{
				float f = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
				float3 r;
				r.z = frac(512.0 * f);
				f *= 0.125;
				r.x = frac(512.0 * f);
				f *= 0.125;
				r.y = frac(512.0 * f);
				return r - 0.5;
			}
			float3 snappedTime () { return _SnappedTime * round(_Time.y / _SnappedTime); }
			//////////////////////////////////////////////////////////////////////////////////////////////
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 col : COLOR;
				float2 uv  : TEXCOORD0;
			};
			fixed4 _Color;
			sampler2D _MainTex;

			v2f vert (appdata_full v)
			{
				float2 t = random3(v.vertex.xyz + snappedTime()) * _DoodleScale;
				v.vertex.xy += t;

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
				half4 c = tex2D(_MainTex, input.uv) * input.col;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
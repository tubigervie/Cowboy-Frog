Shader "2D Shader Pack/Nature/Sea" {
	Properties {
		[HideInInspector] _MainTex ("Main", 2D) = "black" {}
		[NoScaleOffset]_ColorTex   ("Color", 2D) = "black" {}
		[NoScaleOffset]_BumpTex    ("Bump", 2D) = "black" {}
		[NoScaleOffset]_SpecTex    ("Specular", 2D) = "black" {}
		_WaveOffset     ("Wave Offset", Vector) = (0.1, 0, 0.2, 0)
		_WaveTiling     ("Wave Tiling", Vector) = (0.5, 0.8, 0, 0)
		_LitDir         ("Light Direction", Vector) = (0, 0, 1, 0)
		_WaveSpeed      ("Wave Speed", Float) = 2
		_SpecIntensity  ("Spec Intensity", Float) = 1
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex, _ColorTex, _BumpTex, _SpecTex;
	float4 _WaveOffset, _WaveTiling, _LitDir;
	float _WaveSpeed, _SpecIntensity;
	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 uv0 : TEXCOORD0;
		float4 uv1 : TEXCOORD1;
		float3 dir : TEXCOORD2;
	};
	v2f vert (appdata_base v)
	{
		float4 uvs = v.vertex.xyxy * _WaveTiling + _WaveOffset * _Time.y * _WaveSpeed;

		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);

		float4 tmp;
		tmp.x = (o.pos.x + o.pos.w) * 0.5;
		tmp.y = (o.pos.y + o.pos.w) * 0.5;
		tmp.zw = o.pos.zw;

		o.uv0 = tmp;
		o.uv1 = uvs;
		o.dir = normalize(_LitDir) + float3(0, 0, 1);
		return o;
	}
	float4 frag (v2f i) : SV_Target
	{
		float4 bump = tex2D(_BumpTex, i.uv1.xy) + tex2D(_BumpTex, i.uv1.zw) - 1.0;
		float4 uv;
		uv.xy = i.uv0.xy - bump.xy;
		uv.zw = i.uv0.zw;
		float d = dot(bump.xyz, i.dir);
		return tex2Dproj(_ColorTex, uv) + _SpecIntensity * tex2D(_SpecTex, float2(0, d)).w;
	}
	ENDCG
	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	Fallback Off
}
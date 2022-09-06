Shader "2D Shader Pack/Nature/Fire" {
	Properties {
		[Header(RenderState)]
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend Src", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend Dst", Int) = 0
		[Header(Parameters)]
		[NoScaleOffset]_NoiseTex   ("Noise", 2D) = "white" {}
		[NoScaleOffset]_BaseTex    ("Base", 2D) = "white" {}
		[NoScaleOffset]_OpacityTex ("Opacity", 2D) = "white" {}
		_LayerSpeed         ("Layer Speed", Vector) = (0.68, 0.52, 0.75, 1)
		_DistortionStrength ("Distortion Strength", Vector) = (0.373, 0.162, 0.108, 1)
		_HeightAttenuation  ("Height Attenuation", Vector) = (0.62, 0, 0, 1)
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _BaseTex, _NoiseTex, _OpacityTex;
	float4 _LayerSpeed, _DistortionStrength, _HeightAttenuation;
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
		float2 uv2 : TEXCOORD2;
		float2 uv3 : TEXCOORD3;
	};
	v2f vert (appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv0 = v.texcoord;
		o.uv1 = float2(v.texcoord.x, v.texcoord.y - _LayerSpeed.x * _Time.y);
		o.uv2 = float2(v.texcoord.x, v.texcoord.y - _LayerSpeed.y * _Time.y);
		o.uv3 = float2(v.texcoord.x, v.texcoord.y - _LayerSpeed.z * _Time.y);
		return o;
	}
	float4 FConv (float4 f)
	{
		return f * 2.0 - 1.0;  // -1.0~1.0
	}
	float4 frag (v2f i) : SV_Target
	{
		float4 n0 = tex2D(_NoiseTex, i.uv1);
		float4 n1 = tex2D(_NoiseTex, i.uv2);
		float4 n2 = tex2D(_NoiseTex, i.uv3);

		float4 sum = FConv(n0) * _DistortionStrength.x + FConv(n1) * _DistortionStrength.y + FConv(n2) * _DistortionStrength.z;
		float2 uv = i.uv0 + sum * (i.uv0.y * _HeightAttenuation.x + _HeightAttenuation.y);
		float4 base = tex2D(_BaseTex, uv);
		float4 opacity = tex2D(_OpacityTex, uv);
		return float4(base.rgb * opacity.rgb, opacity.r);
	}
	ENDCG
	Subshader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			Blend [_BlendSrc] [_BlendDst]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	Fallback Off
//	CustomEditor "NatureFireShaderGUI"
}

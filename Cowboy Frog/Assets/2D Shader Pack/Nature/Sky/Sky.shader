Shader "2D Shader Pack/Nature/Sky" {
	Properties {
		[HideInInspector] _MainTex ("Main", 2D) = "black" {}
		[Header(Cloud)]
		[NoScaleOffset]_Noise0Tex ("Noise 0", 2D) = "black" {}
		[NoScaleOffset]_Noise1Tex ("Noise 1", 2D) = "black" {}
		[NoScaleOffset]_Noise2Tex ("Noise 2", 2D) = "black" {}
		[NoScaleOffset]_Noise3Tex ("Noise 3", 2D) = "black" {}
		_CloudSpeed     ("Cloud Speed", Float) = 0.1
		_CloudDensity   ("Cloud Density", Float) = 0.2
		_CloudSharpness ("Cloud Sharpness", Float) = 1.0
		_CloudColor     ("Cloud Color", Color) = (1, 1, 1, 1)
		[Header(ThickCloud)]
		[NoScaleOffset]_ThickCloudTex  ("Thick Cloud", 2D) = "white" {}
		[Header(Sky)]
		_SkyTop         ("Sky Top", Color) = (0.3984, 0.5117, 0.7305, 1)
		_SkyBottom      ("Sky Bottom", Color) = (0.7031, 0.4687, 0.1055, 1)   // sun color equal sky bottom color
		_Sun            ("Sun", Vector) = (0, 0, 0, 0)
		_SunSize        ("Sun Size", Float) = 10
		_BottomSunLevel ("Bottom Sun Level", Float) = 0.3
		_TopDarkLevel   ("Top Dark Level", Float) = 1.5
		[Header(Star)]
		_StarStrength   ("Star Strength", Float) = 1
		_StarDensity    ("Star Density", Float) = 0.996
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex, _Noise0Tex, _Noise1Tex, _Noise2Tex, _Noise3Tex, _ThickCloudTex;
	float4 _Sun, _CloudColor, _SkyTop, _SkyBottom;
	float _SunSize, _BottomSunLevel, _TopDarkLevel, _CloudSpeed, _CloudDensity, _CloudSharpness, _StarDensity, _StarStrength;
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
		float4 uv0 : TEXCOORD1;
		float4 uv1 : TEXCOORD2;
	};
	v2f vert (appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		o.uv0.xy = v.texcoord + _Time.x * 1.0 * _CloudSpeed * float2( 1,  0);
		o.uv0.zw = v.texcoord + _Time.x * 1.5 * _CloudSpeed * float2( 0,  1);
		o.uv1.xy = v.texcoord + _Time.x * 2.0 * _CloudSpeed * float2( 0, -1);
		o.uv1.zw = v.texcoord + _Time.x * 2.5 * _CloudSpeed * float2(-1,  0);
		o.uv0.y = 1.0 - o.uv0.y;
		o.uv0.w = 1.0 - o.uv0.w;
		o.uv1.y = 1.0 - o.uv1.y;
		o.uv1.w = 1.0 - o.uv1.w;
		return o;
	}
	float rand (float2 v)  { return frac(sin(dot(v, float2(12.9898, 78.233))) * 43758.5453); }
	float3 blinkStar (float2 uv)
	{
		float c = 0;
		if (rand(uv) > _StarDensity)
		{
			float r = rand(uv * _ScreenParams.xy);
			c = r * (0.85 * sin(_Time.y * (r * 5.0) + 720.0 * r) + 0.95);
		}
		return c.xxx * _StarStrength;
	}
	float4 frag (v2f i) : SV_Target
	{
		float2 uv = i.uv;
		float2 sunVec = _Sun.xy;

		// sky
		float sun = max(1.0 - (1.0 + _SunSize * sunVec.y + uv.y) * length(uv - sunVec), 0.0)
			+ _BottomSunLevel * pow(1.0 - uv.y, 12.0) * (1.6 - sunVec.y);
		float3 mix = lerp(_SkyTop.rgb, _SkyBottom.rgb, sun);
		float3 c = mix * ((0.5 + pow(sunVec.y, 0.4)) * (_TopDarkLevel - uv.y) + pow(sun, 5.2)
				  * sunVec.y * (5.0 + 15.0 * sunVec.y));

		// cloud
#if USE_ThickCloud
		float4 n0 = tex2D(_ThickCloudTex, float2(uv.x + (_Time.y * 0.025), uv.y * (sin(_Time.y * 0.1) * 0.25 + 0.75)));
		float4 n1 = tex2D(_ThickCloudTex, float2(uv.x - (n0.r * 0.2), uv.y - (n0.r * 0.3)));
		float3 f = n0.r * sin(n1.r) * 1.375;
		
		float r = abs(tan(f + 0.025));
		float g = abs(tan(f - 0.05));
		float b = abs(tan(f - 0.1));
		float3 cloud = float3(r, g, b);
		c += blinkStar(uv);
		c = lerp(c, cloud, uv.y);
#else
		float4 n0 = tex2D(_Noise0Tex, i.uv0.xy);
		float4 n1 = tex2D(_Noise1Tex, i.uv0.zw);
		float4 n2 = tex2D(_Noise2Tex, i.uv1.xy);
		float4 n3 = tex2D(_Noise3Tex, i.uv1.zw);
		float4 fbm = 0.5 * n0 + 0.25 * n1 + 0.125 * n2 + 0.0625 * n3;
		fbm = (clamp(fbm, _CloudDensity, _CloudSharpness) -  _CloudDensity)/(_CloudSharpness - _CloudDensity);

		float4 gradient = float4(0.0, 0.2, 0.4, 0.6);
		float amount = dot(max(fbm - gradient, 0), (0.25).xxxx);
		float3 cloud = amount * _CloudColor.rgb + 2.0 * (1.0 - amount) * 0.4;

		// blend sky and cloud, accumulate blink star
		c = lerp(c, cloud, amount) + blinkStar(uv);
#endif
		return float4(c, 1.0);
	}
	ENDCG
	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ USE_ThickCloud
			ENDCG
		}
	}
	Fallback Off
}

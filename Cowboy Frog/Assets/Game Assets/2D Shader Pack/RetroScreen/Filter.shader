Shader "2D Shader Pack/Filter" {
	Properties {
		[HideInInspector]_MainTex ("Main", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma multi_compile _ USE_Quantize
			#pragma multi_compile _ USE_TvCurvature
			#pragma multi_compile _ USE_PixelMask
			#pragma multi_compile _ USE_RollingFlicker
			#pragma multi_compile _ USE_Gameboy
			#pragma multi_compile _ USE_Pixelate
			#include "UnityCG.cginc"
			//////////////////////////////////////////////////////////////////////////////////////////////
			sampler2D _MainTex, _PixelMaskTex;
			float _PixelateSize, _TvCurvature;
			float4 _QuantizeRGB, _PixelMaskParams, _RollingFlickerParams;
			float _VignetteRadius, _VignetteSoftness, _VignetteIntensity;
			float2 tvcurv (float2 uv)
			{
				float2 orig = uv;
				uv = (uv - 0.5) * 2.0;
				float2 offs;
				offs.x = (1.0 - uv.y * uv.y) * _TvCurvature * (uv.x); 
				offs.y = (1.0 - uv.x * uv.x) * _TvCurvature * (uv.y);
				return orig - offs;
			}
			//////////////////////////////////////////////////////////////////////////////////////////////
			half4 frag (v2f_img input) : SV_Target
			{
				float2 uv = input.uv;
#if USE_TvCurvature
				uv = tvcurv(input.uv);
#endif
#if USE_Pixelate
				float ratioX = (int)(uv.x / _PixelateSize) * _PixelateSize;
				float ratioY = (int)(uv.y / _PixelateSize) * _PixelateSize;
				uv = float2(ratioX, ratioY);
#endif
				half4 c = tex2D(_MainTex, uv);
#if USE_Quantize
				float3 reciprocal = 1.0 / _QuantizeRGB.xyz;
				c.rgb = floor(c.rgb * _QuantizeRGB.xyz) * reciprocal;
#endif
#if USE_PixelMask
				c.rgb *= tex2D(_PixelMaskTex, uv * _PixelMaskParams.xy).rgb * _PixelMaskParams.z;
#endif
#if USE_RollingFlicker
				float3 params = _RollingFlickerParams.xyz;
				float3 c1 = c.rgb * ((1.0 - fmod(uv.y + params.x, 1.0)) * params.z + (1.0 - params.z));
				float3 c2 = c.rgb * ((1.0 - fmod(uv.y + params.y, 1.0)) * params.z + (1.0 - params.z));
				c.rgb = (c1 + c2) * 0.5;
#endif
#if USE_Gameboy
				float lumn = dot(c.rgb, float3(0.3, 0.59, 0.11));
				if (lumn <= 0.25)
					c.rgb = fixed3(0.06, 0.22, 0.06);
				else if (lumn > 0.75)
					c.rgb = fixed3(0.6, 0.74, 0.06);
				else if (lumn > 0.25 && lumn <= 0.5)
					c.rgb = fixed3(0.19, 0.38, 0.19);
				else
					c.rgb = fixed3(0.54, 0.67, 0.06);
#endif

				// vignette
				float2 p = input.uv - 0.5;
				float l = length(p);
				float vgt = smoothstep(_VignetteRadius, _VignetteRadius - _VignetteSoftness, l);
				c.rgb = lerp(c.rgb, c.rgb * vgt, _VignetteIntensity);
				return c;
			}
			ENDCG
		}
	}
	Fallback Off
}
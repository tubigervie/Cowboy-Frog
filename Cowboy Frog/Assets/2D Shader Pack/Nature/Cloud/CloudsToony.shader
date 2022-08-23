Shader"2D Shader Pack/Nature/CloudsToony"{
	Properties {
		[HideInInspector]_MainTex ("Main", 2D) = "black" {}
//		_BackColor  ("BackColor", Color) = (0.0, 0.4, 0.58, 0)
//		_CloudColor ("CloudColor", Color) = (0.18, 0.7, 0.87, 0)
		_Blur ("Bottom Blur", Range(0, 16)) = 4
		_LayerSpeed ("Layer Speed", Range(-2, 2)) = 0.1
		_LayerUp    ("Layer Up", Range(-0.8, 0.8)) = 0.3
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			#define TAU 6.28318530718
			const half3 _BackColor  = half3(0.0,  0.4,  0.58);
			const half3 _CloudColor = half3(0.18, 0.70, 0.87);
//			half3 _BackColor, _CloudColor;
			half _Blur, _LayerSpeed, _LayerUp;

			half Func  (half pX) { return 0.6 * (0.5 * sin(0.1 * pX) + 0.5 * sin(0.553 * pX) + 0.7 * sin(1.2 * pX)); }
			half FuncR (half pX) { return 0.5 + 0.25 * (1.0 + sin(fmod(40.0 * pX, TAU))); }
			half Layer (half2 pQ, half pT)
			{
				half2 Qt = 3.5 * pQ;
				pT *= 0.5;
				Qt.x += pT;

				half Xi = floor(Qt.x);
				half Xf = Qt.x - Xi - 0.5;

				half D = 1.0 - step(Qt.y, Func(Qt.x));
				half E = pT / 80.0;

				// disk
				half Yi = Func(Xi + 0.5);
				half2 C = half2(Xf, Qt.y - Yi);
				D = min(D, length(C) - FuncR(Xi + E));

				// previous disk
				Yi = Func(Xi + 1.0 + 0.5);
				C = half2(Xf - 1.0, Qt.y - Yi);
				D = min(D, length(C) - FuncR(Xi + 1.0 + E));

				// next disk
				Yi = Func(Xi - 1.0 + 0.5);
				C = half2(Xf + 1.0, Qt.y - Yi);
				D = min(D, length(C) - FuncR(Xi - 1.0 + E));
				return min(1.0, D);
			}
			half4 frag (v2f_img input) : SV_Target
			{
				half2 uv = 2.0 * (input.uv - 0.5);
				half3 c = _BackColor;
				[unroll(100)]
				for (half i = 0.0; i <= 1.0; i += 0.2)
				{
					// cloud layer
					half Lt = _Time.y * (0.5 + i) * (1.0 + _LayerSpeed * sin(226.0 * i)) + 17.0 * i;
					half2 Lp = half2(0.0, _LayerUp + 1.5 * (i - 0.5));
					half L = Layer(uv + Lp, Lt);

					// blur and color
					half blur = _Blur * (0.5 * abs(2.0 - 5.0 * i)) / (11.0 - 5.0 * i);

					half v = lerp(0.0, 1.0, 1.0 - smoothstep(0.0, 0.01 + 0.2 * blur, L));
					half3 lc = lerp(_CloudColor, 1.0, i);
					c = lerp(c, lc, v);
				}
				return half4(c, 1.0) + half4(0, 0.5, 0.5, 0);
			}
			ENDCG
		}
	}
	FallBack Off
}


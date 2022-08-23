using UnityEngine;

namespace ShaderPack2D
{
	public class Noise3D
	{
		Texture3D m_3DTex;
		int m_Size;

		public void Create(int size, int smoothness)
		{
			m_Size = size;
			int XSIZE = size, YSIZE = size, ZSIZE = size;
			float XSCALE = 0.08f, YSCALE = 0.16f, ZSCALE = 0.16f;

			// generate 3d noise
			Color[] rgb = new Color[m_Size * m_Size * m_Size];
			for (int z = 0; z < m_Size; z++)
			{
				for (int y = 0; y < m_Size; y++)
				{
					for (int x = 0; x < m_Size; x++)
					{
						float f = Perlin.TileableTurbulence3(XSCALE * x, YSCALE * y, ZSCALE * z, XSIZE * XSCALE, YSIZE * YSCALE, ZSIZE * ZSCALE, smoothness);
						f = f * 0.5f + 0.5f;
						Color c = new Color(f, f, f);
						rgb[z * m_Size * m_Size + y * m_Size + x] = c;
					}
				}
			}

			// generate 3d texture
			m_3DTex = new Texture3D(m_Size, m_Size, m_Size, TextureFormat.RGB24, true);
			m_3DTex.name = "3DNoiseTexture";
			m_3DTex.wrapMode = TextureWrapMode.Repeat;
			m_3DTex.filterMode = FilterMode.Bilinear;
			m_3DTex.SetPixels(rgb);
			m_3DTex.Apply();
		}
		public Texture3D Get()  { return m_3DTex; }
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public static class Perlin
	{
		public static float TileableTurbulence3(float x, float y, float z, float w, float h, float d, float freq)
		{
			float t = 0f;
			do {
				t += TileableNoise3(freq * x, freq * y, freq * z, w * freq, h * freq, d * freq) / freq;
				freq *= 0.5f;
			} while (freq >= 1f);
			return t;
		}
		static float TileableNoise3(float x, float y, float z, float w, float h, float d)
		{
			return (Noise(x,     y,     z)     * (w - x) * (h - y) * (d - z) +
					Noise(x - w, y,     z)     *      x  * (h - y) * (d - z) +
					Noise(x,     y - h, z)     * (w - x) *      y  * (d - z) +
					Noise(x - w, y - h, z)     *      x  *      y  * (d - z) + 
					Noise(x,     y,     z - d) * (w - x) * (h - y) *      z  +
					Noise(x - w, y,     z - d) *      x  * (h - y) *      z  +
					Noise(x,     y - h, z - d) * (w - x) *      y  *      z  +
					Noise(x - w, y - h, z - d) *      x  *      y  *      z) / (w * h * d);
		}
		static float Noise(float x, float y, float z)
		{
			var X = Mathf.FloorToInt(x) & 0xff;
			var Y = Mathf.FloorToInt(y) & 0xff;
			var Z = Mathf.FloorToInt(z) & 0xff;
			x -= Mathf.Floor(x);
			y -= Mathf.Floor(y);
			z -= Mathf.Floor(z);
			var u = Fade(x);
			var v = Fade(y);
			var w = Fade(z);
			var A  = (perm[X    ] + Y) & 0xff;
			var B  = (perm[X + 1] + Y) & 0xff;
			var AA = (perm[A    ] + Z) & 0xff;
			var BA = (perm[B    ] + Z) & 0xff;
			var AB = (perm[A + 1] + Z) & 0xff;
			var BB = (perm[B + 1] + Z) & 0xff;
			return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA  ], x, y  , z  ), Grad(perm[BA  ], x-1, y  , z  )),
								Lerp(u, Grad(perm[AB  ], x, y-1, z  ), Grad(perm[BB  ], x-1, y-1, z  ))),
						Lerp(v, Lerp(u, Grad(perm[AA+1], x, y  , z-1), Grad(perm[BA+1], x-1, y  , z-1)),
								Lerp(u, Grad(perm[AB+1], x, y-1, z-1), Grad(perm[BB+1], x-1, y-1, z-1))));
		}
		static float Fade(float t)  { return t * t * t * (t * (t * 6 - 15) + 10); }
		static float Lerp(float t, float a, float b)  { return a + t * (b - a); }
		static float Grad(int hash, float x, float y, float z)
		{
			var h = hash & 15;
			var u = h < 8 ? x : y;
			var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
		}
		static int[] perm = {
			151,160,137,91,90,15,
			131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
			190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
			88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
			77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
			102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
			135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
			5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
			223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
			129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
			251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
			49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
			138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,151
		};
	}
}
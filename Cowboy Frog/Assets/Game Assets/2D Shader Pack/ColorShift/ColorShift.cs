using UnityEngine;

namespace ShaderPack2D
{
	public class ColorShift : MonoBehaviour
	{
		[Range(0f, 1f)] public float m_HueModifyMin = 0f;
		[Range(0f, 1f)] public float m_HueModifyMax = 1f;
		[Range(0f, 1f)] public float m_HueOffset = 0f;
		[Range(0f, 1f)] public float m_SaturationOffset = 0f;
		[Range(0f, 1f)] public float m_ValueOffset = 0f;
		Renderer[] m_Rdrs;

		void Start()
		{
			m_Rdrs = GetComponentsInChildren<Renderer>();
		}
		void Update()
		{
			if (m_HueModifyMin >= m_HueModifyMax)
				m_HueModifyMin = m_HueModifyMax - 0.01f;

			for (int i = 0; i < m_Rdrs.Length; i++)
			{
				Renderer rd = m_Rdrs[i];
				rd.material.SetFloat("_HRangeMin", m_HueModifyMin);
				rd.material.SetFloat("_HRangeMax", m_HueModifyMax);
				rd.material.SetFloat("_HOffset", m_HueOffset);
				rd.material.SetFloat("_SOffset", m_SaturationOffset);
				rd.material.SetFloat("_VOffset", m_ValueOffset);
			}
		}
	}
}

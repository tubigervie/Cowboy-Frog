using UnityEngine;

namespace ShaderPack2D
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class Sky : MonoBehaviour
	{
		[Header("Time")]
		[Range(-1.45f, 1.45f)] public float m_TimeOfDay = 0f;
		[Header("Sun")]
		[Range(0, 0.8f)] public float m_BottomSunLevel = 0.3f;
		[Range(1f, 64f)] public float m_SunSize = 10f;
		[Header("Cloud")]
		[Range(0.01f, 1f)] public float m_CloudSpeed = 0.1f;
		[Range(0.01f, 0.3f)] public float m_CloudDensity = 0.2f;
		[Range(0.01f, 1f)] public float m_CloudSharpness = 1f;
		public Color m_CloudColor = Color.white;
		public bool m_UseThickCloud = false;
		[Header("Star")]
		[Range(0.99f, 1f)] public float m_StarDensity = 0.996f;
		SpriteRenderer m_Spr;

		void Start()
		{
			m_Spr = GetComponent<SpriteRenderer>();
		}
		void Update()
		{
			float dt = m_TimeOfDay;   // Time.time
			float sunX = Mathf.Lerp(0.1f, 0.9f, Mathf.Sin(dt) * 0.5f + 0.5f);
			float sunY = Mathf.Lerp(-0.01f, 0.9f, Mathf.Cos(2f * dt) * 0.5f + 0.5f);

			m_Spr.material.SetVector("_Sun", new Vector4(sunX, sunY, 0f, 1f));
			m_Spr.material.SetFloat("_BottomSunLevel", m_BottomSunLevel);
			m_Spr.material.SetFloat("_SunSize", m_SunSize);
			m_Spr.material.SetFloat("_TopDarkLevel", 1.5f * sunY + 0.3f);
			m_Spr.material.SetFloat("_CloudSpeed", m_CloudSpeed);
			m_Spr.material.SetFloat("_CloudDensity", m_CloudDensity);
			m_Spr.material.SetFloat("_CloudSharpness", m_CloudSharpness);
			m_Spr.material.SetColor("_CloudColor", m_CloudColor);
			m_Spr.material.SetFloat("_StarStrength", 1f - sunY);
			m_Spr.material.SetFloat("_StarDensity", m_StarDensity);
			if (m_UseThickCloud)
				m_Spr.material.EnableKeyword("USE_ThickCloud");
			else
				m_Spr.material.DisableKeyword("USE_ThickCloud");
		}
	}
}

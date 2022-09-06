using UnityEngine;

namespace ShaderPack2D
{
	public class NatureDemo : MonoBehaviour
	{
//		public enum EDemo { Sky = 0, Sea, CloudsToony, CloudsRealistic, GodRay, Fog, FireAndElectricity };
//		public EDemo m_Demo = EDemo.Sky;
//		EDemo m_PrevDemo = EDemo.Sky;
		Noise3D m_Noise = new Noise3D();
//		public GameObject m_Sky;
//		public GameObject m_Sea;
//		public GameObject m_CloudsToony;
//		public GameObject m_CloudsRealistic;
//		public GameObject m_FireAndElectricity;
//		public GameObject m_GodRay;
//		public GameObject m_Fog;

		void Start()
		{
			m_Noise.Create(64, 2);
//			ChangeDemo();
		}
		void Update()
		{
//			if (m_PrevDemo != m_Demo)
//			{
//				ChangeDemo();
//				m_PrevDemo = m_Demo;
//			}
			Shader.SetGlobalTexture("_Global_NoiseTex", m_Noise.Get());
		}
//		void ChangeDemo()
//		{
//			m_Sky.SetActive(false);
//			m_Sea.SetActive(false);
//			m_CloudsToony.SetActive(false);
//			m_CloudsRealistic.SetActive(false);
//			m_FireAndElectricity.SetActive(false);
//			m_GodRay.SetActive(false);
//			m_Fog.SetActive(false);
//
//			if (m_Demo == EDemo.Sky)
//				m_Sky.SetActive(true);
//			if (m_Demo == EDemo.Sea)
//				m_Sea.SetActive(true);
//			if (m_Demo == EDemo.CloudsToony)
//				m_CloudsToony.SetActive(true);
//			if (m_Demo == EDemo.CloudsRealistic)
//				m_CloudsRealistic.SetActive(true);
//			if (m_Demo == EDemo.FireAndElectricity)
//				m_FireAndElectricity.SetActive(true);
//			if (m_Demo == EDemo.GodRay)
//				m_GodRay.SetActive(true);
//			if (m_Demo == EDemo.Fog)
//				m_Fog.SetActive(true);
//		}
	}
}
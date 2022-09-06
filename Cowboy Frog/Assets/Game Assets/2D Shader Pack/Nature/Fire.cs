using UnityEngine;

namespace ShaderPack2D
{
	public class Fire : MonoBehaviour
	{
		public Vector4 m_LayerSpeed = new Vector4(0.68f, 0.52f, 0.75f, 1f);
		public Vector4 m_DistortionStrength = new Vector4(0.373f, 0.162f, 0.108f, 1f);

		void Update()
		{
			Renderer rd = GetComponent<Renderer>();
			rd.material.SetVector("_LayerSpeed", m_LayerSpeed);
			rd.material.SetVector("_DistortionStrength", m_DistortionStrength);
		}
	}
}

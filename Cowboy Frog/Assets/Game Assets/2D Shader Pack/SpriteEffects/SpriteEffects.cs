using UnityEngine;

namespace ShaderPack2D
{
	public class SpriteEffects : MonoBehaviour
	{
		SpriteRenderer m_SprRdr;

		void Start()
		{
			m_SprRdr = GetComponent<SpriteRenderer>();
		}
		void Update()
		{
			Vector4 rc = UnityEngine.Sprites.DataUtility.GetOuterUV(m_SprRdr.sprite);
			m_SprRdr.material.SetVector("_UvRect", rc);
		}
	}
}

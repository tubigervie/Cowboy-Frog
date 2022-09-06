using UnityEngine;

namespace ShaderPack2D
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SetupMaterials : MonoBehaviour
	{
		public Material[] m_Materials;

		void Start()
		{
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			sr.materials = m_Materials;
		}
	}
}
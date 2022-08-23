using UnityEngine;

namespace ShaderPack2D
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class BackgroundSprite : MonoBehaviour
	{
		void Start()
		{
			transform.localPosition = Vector3.zero;
		}
		void Update()
		{
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			float w = sr.sprite.bounds.size.x;
			float h = sr.sprite.bounds.size.y;
			float scrHeight = Camera.main.orthographicSize * 2f;
			float scrWidth = scrHeight / Screen.height * Screen.width;
			transform.localScale = new Vector3(scrWidth / w, scrHeight / h, 1f);
		}
	}
}
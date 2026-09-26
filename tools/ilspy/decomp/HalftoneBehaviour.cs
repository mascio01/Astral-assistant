using UnityEngine;
using UnityEngine.UI;

public class HalftoneBehaviour : RawImage
{
	public float Scale = 0.01f;

	public Vector2 Offset = Vector2.zero;

	protected override void Awake()
	{
		Update();
	}

	public void Update()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		Vector2 vector = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		base.uvRect = new Rect(Offset, vector * Scale);
	}
}

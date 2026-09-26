using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarBehaviour : MonoBehaviour
{
	private float Amount;

	private bool Flashing;

	private Color Col;

	private Color DotCol;

	public RawImage UnityFill;

	public Outline UnityFillOutline;

	public List<GameObject> UnityNotches = new List<GameObject>();

	public Material Mat;

	private static float FlashingFrequency = 2f;

	private static Color FlashingCol = Color.white;

	public void Awake()
	{
		UnityFill = base.transform.Find("Fill").gameObject.GetComponent<RawImage>();
		UnityFillOutline = UnityFill.GetComponent<Outline>();
		Col = UnityFill.color;
		if (UnityFill.material != null && UnityFill.material.shader.name == "Custom/UI/HalftoneGradient")
		{
			Mat = new Material(UnityFill.material);
			UnityFill.material = Mat;
			Col = Mat.color;
			DotCol = Mat.GetColor(ShaderHash._DotColor);
		}
		Update();
	}

	public void OnDestroy()
	{
		if (Mat != null)
		{
			UnityEngine.Object.Destroy(Mat);
		}
	}

	public void SetNotches(float notchDist)
	{
		if (InfoScreen.Notch == null || InfoScreen.Notch.GetAsset() == null)
		{
			return;
		}
		RectTransform rectTransform = (RectTransform)base.transform;
		int num = 0;
		for (float num2 = 0f; num2 <= 1.01f; num2 += notchDist)
		{
			GameObject gameObject = null;
			if (num < UnityNotches.Count)
			{
				gameObject = UnityNotches[num];
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(InfoScreen.Notch.GetAsset(), base.transform, worldPositionStays: false);
				UnityNotches.Add(gameObject);
			}
			((RectTransform)gameObject.transform).anchoredPosition = new Vector2(num2 * rectTransform.rect.width, 0f);
			num++;
		}
		while (num < UnityNotches.Count)
		{
			UnityEngine.Object.Destroy(UnityNotches[num]);
			UnityNotches.RemoveAt(num);
		}
	}

	public void Update()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		Vector2 sizeDelta = new Vector2(rectTransform.rect.width * Amount, 0f);
		UnityFill.rectTransform.sizeDelta = sizeDelta;
		float t = (Flashing ? (Mathf.Sin(Time.unscaledTime * (MathF.PI * 2f) * FlashingFrequency) * 0.5f + 0.5f) : 0f);
		UnityFill.color = Color.Lerp(Col, FlashingCol, t);
		if (Mat != null)
		{
			Mat.color = Color.Lerp(Col, FlashingCol, t);
			Mat.SetColor(ShaderHash._DotColor, Color.Lerp(DotCol, FlashingCol, t));
		}
	}

	public void SetValue(float v)
	{
		Amount = Mathf.Clamp01(v);
	}

	public float GetValue()
	{
		return Amount;
	}

	public void SetFlashing(bool on)
	{
		Flashing = on;
	}

	public void SetCol(Color col)
	{
		UnityFill.color = col;
		Col = (DotCol = col);
	}

	public void SetCol(Color col, Color dotCol)
	{
		UnityFill.color = col;
		Col = col;
		DotCol = dotCol;
	}

	public void SetWidth(float w)
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		rectTransform.sizeDelta = new Vector2(w, rectTransform.sizeDelta.y);
	}

	public void SetTicks(int enabledCount)
	{
		for (int i = 1; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(i <= enabledCount);
		}
	}

	public void SetFillOutlineEnabled(bool enabled)
	{
		if (UnityFillOutline != null)
		{
			UnityFillOutline.enabled = enabled;
		}
	}
}

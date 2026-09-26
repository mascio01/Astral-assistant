using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterSwitchBarBehaviour : MonoBehaviour
{
	private GameObject UnityContent;

	private GameObject UnityTooltip;

	private TextMeshProUGUI UnityTooltipText;

	public float MaxWidth = 956f;

	public float Border = 4f;

	public int SelectedCharacterIndex;

	private int LastSelectedCharacterIndex;

	public List<CharacterIconBehaviour> CharacterIcons = new List<CharacterIconBehaviour>();

	private static StringBuilder sb = new StringBuilder();

	private void Awake()
	{
		UnityContent = base.transform.Find("Scroll View/Viewport/Content").gameObject;
		UnityTooltip = base.transform.Find("Tooltip").gameObject;
		UnityTooltipText = base.transform.Find("Tooltip/Text").GetComponent<TextMeshProUGUI>();
	}

	public void Update()
	{
		float num = Border;
		float num2 = 0f;
		RectTransform rectTransform = (RectTransform)base.transform;
		RectTransform rectTransform2 = (RectTransform)UnityContent.transform;
		RectTransform rectTransform3 = (RectTransform)UnityTooltip.transform;
		for (int i = 0; i < UnityContent.transform.childCount; i++)
		{
			RectTransform rectTransform4 = (RectTransform)UnityContent.transform.GetChild(i).gameObject.transform;
			if (i == SelectedCharacterIndex)
			{
				num2 = num + rectTransform4.rect.width * 0.5f;
			}
			if (i == LastSelectedCharacterIndex)
			{
				rectTransform3.anchoredPosition = new Vector2(RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform, rectTransform4).center.x, rectTransform3.anchoredPosition.y);
			}
			num += rectTransform4.rect.width + Border;
		}
		LastSelectedCharacterIndex = SelectedCharacterIndex;
		rectTransform.sizeDelta = new Vector2(Mathf.Min(MaxWidth, num), rectTransform.sizeDelta.y);
		rectTransform2.localPosition = new Vector3(Mathf.Clamp(0f - num2 + MaxWidth * 0.5f, Mathf.Min(0f, MaxWidth - num), 0f), 0f, 0f);
		int num3 = SelectedCharacterIndex - 3;
		int num4 = SelectedCharacterIndex + 3;
		if (num3 < 0)
		{
			num3 = 0;
			num4 = Math.Min(num3 + 7, CharacterIcons.Count - 1);
		}
		else if (num4 >= CharacterIcons.Count)
		{
			num4 = CharacterIcons.Count - 1;
			num3 = Math.Max(num4 - 7, 0);
		}
		for (int j = 0; j < CharacterIcons.Count; j++)
		{
			CharacterIcons[j].SetOnScreen(j >= num3 && j <= num4);
		}
		if (Application.isPlaying && Hud.Instance != null && Hud.Instance.LocalControlledCharacter != null)
		{
			sb.Length = 0;
			Hud.Instance.LocalControlledCharacter.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
			UnityTooltipText.SetUnityTextIfDifferent(sb);
		}
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TranslateBehaviour : MonoBehaviour
{
	private TextMeshProUGUI TextMesh;

	private Text Text;

	public bool Translated;

	public bool WantApplyForumla;

	private int LastAppliedFormula;

	private int LastTranslated;

	private string OriginalText;

	public void DontTranslate()
	{
		Translated = true;
		LastTranslated = int.MaxValue;
	}

	public void SetTranslationTag(string tag)
	{
		Translated = false;
		OriginalText = tag;
	}

	public void Translate()
	{
		if (GameImpl.Instance == null || (Translated && LastTranslated >= GameImpl.Instance.LastChangedLanguageFrame && (!WantApplyForumla || LastAppliedFormula >= InputFunctionManager.Instance.CurrentInputTypeFrame)))
		{
			return;
		}
		if (OriginalText == null)
		{
			string originalText = null;
			if (TextMesh != null)
			{
				originalText = TextMesh.text;
			}
			if (Text != null)
			{
				originalText = Text.text;
			}
			OriginalText = originalText;
		}
		if (OriginalText != null)
		{
			string str = GameImpl.Translate(OriginalText);
			if (WantApplyForumla)
			{
				str = StringUtil.ApplyFormulae(str);
				LastAppliedFormula = Time.frameCount;
			}
			if (TextMesh != null)
			{
				TextMesh.SetUnityText(str);
			}
			if (Text != null)
			{
				Text.SetUnityText(str);
			}
		}
		Translated = true;
		LastTranslated = Math.Max(LastTranslated, Time.frameCount);
	}

	private void Start()
	{
		TextMesh = base.gameObject.GetComponent<TextMeshProUGUI>();
		Text = base.gameObject.GetComponent<Text>();
		Translate();
	}

	private void Update()
	{
		Translate();
	}
}

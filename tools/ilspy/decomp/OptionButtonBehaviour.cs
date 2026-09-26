using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionButtonBehaviour : MonoBehaviour
{
	private string Text;

	private bool Ticked;

	private bool WantRepopulate;

	private RawImage UnityTick;

	private TextMeshProUGUI UnityText;

	private void Awake()
	{
		UnityTick = base.gameObject.FindChild("Tick").GetComponent<RawImage>();
		UnityText = base.gameObject.FindChild("Text").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (WantRepopulate)
		{
			UnityTick.gameObject.SetActive(Ticked);
			UnityText.SetUnityText(Text);
			WantRepopulate = false;
		}
	}

	public void SetTicked(bool ticked)
	{
		Ticked = ticked;
		if (UnityTick != null)
		{
			UnityTick.gameObject.SetActive(Ticked);
		}
		else
		{
			WantRepopulate = true;
		}
	}

	public void SetCaption(string text)
	{
		Text = text;
		if (UnityText != null)
		{
			UnityText.SetUnityText(Text);
		}
		else
		{
			WantRepopulate = true;
		}
	}
}

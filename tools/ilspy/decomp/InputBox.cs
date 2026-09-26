using TMPro;
using UnityEngine;

public class InputBox : BaseDialog
{
	public delegate void AcceptFunction(InputFrame inputFrame, string resultText);

	private TMP_InputField UnityInputField;

	public AcceptFunction OnAccept;

	public bool ReadOnly;

	public string Title;

	public string InitialText;

	public bool NeedSession;

	public override void OnActivate()
	{
		base.OnActivate();
		GameObject gameObject = base.gameObject.transform.Find("InputField").gameObject;
		UnityInputField = gameObject.GetComponent<TMP_InputField>();
		UnityInputField.readOnly = ReadOnly;
		base.gameObject.FindChild("TitleText").GetComponent<TextMeshProUGUI>().SetUnityText(Title);
		base.gameObject.FindChild("InputField").GetComponent<TMP_InputField>().SetUnityText(InitialText);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (OKSelected)
		{
			OnAccept(inputFrame, UnityInputField.text);
			Finished = true;
			OKSelected = false;
		}
	}

	public override bool NeedsSession()
	{
		return NeedSession;
	}
}

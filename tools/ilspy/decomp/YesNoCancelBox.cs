using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YesNoCancelBox : BaseDialog
{
	public delegate void ConfirmFunction(InputFrame inputFrame);

	public string Message;

	public bool NeedSession;

	public ConfirmFunction OnYes;

	public ConfirmFunction OnNo;

	protected Button UnityYesButton;

	protected Button UnityNoButton;

	protected TextMeshProUGUI UnityYesText;

	protected TextMeshProUGUI UnityNoText;

	private static int MENU_Yes = StringUtil.JenkinsHash("MENU_Yes");

	private static int MENU_No = StringUtil.JenkinsHash("MENU_No");

	protected bool YesSelected;

	protected bool NoSelected;

	public override void OnActivate()
	{
		base.OnActivate();
		base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>().SetUnityText(Message);
		GameObject gameObject = base.gameObject.FindChild("MenuLayout/YesButton");
		GameObject gameObject2 = base.gameObject.FindChild("MenuLayout/NoButton");
		GameObject gameObject3 = base.gameObject.FindChild("MenuLayout/YesButton/Text");
		GameObject gameObject4 = base.gameObject.FindChild("MenuLayout/NoButton/Text");
		UnityYesButton = ((gameObject != null) ? gameObject.GetComponent<Button>() : null);
		UnityNoButton = ((gameObject2 != null) ? gameObject2.GetComponent<Button>() : null);
		UnityYesText = ((gameObject3 != null) ? gameObject3.GetComponent<TextMeshProUGUI>() : null);
		UnityNoText = ((gameObject4 != null) ? gameObject4.GetComponent<TextMeshProUGUI>() : null);
	}

	public void OnYesClicked()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		YesSelected = true;
	}

	public void OnNoClicked()
	{
		SoundManager.PlayMenuSound(SoundManager.CancelSound);
		NoSelected = true;
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (UnityYesButton != null)
		{
			BaseDialog.sb.Length = 0;
			if (WantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				BaseDialog.sb.AppendButtonPromptString(InputFunction.MenuSelect);
			}
			BaseDialog.sb.Append(GameImpl.Translate(MENU_Yes));
			UnityYesText.SetUnityText(BaseDialog.sb);
			UnityYesButton.interactable = !WantOkCancelButtonPromptsForController || SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		}
		if (UnityNoButton != null)
		{
			BaseDialog.sb.Length = 0;
			if (WantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				BaseDialog.sb.AppendButtonPromptString(InputFunction.AltAction);
			}
			BaseDialog.sb.Append(GameImpl.Translate(MENU_No));
			UnityNoText.SetUnityText(BaseDialog.sb);
			UnityNoButton.interactable = !WantOkCancelButtonPromptsForController || SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		}
		if (UnityYesButton != null && WantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons && instance.IsJustPressed(InputFunction.MenuSelect))
		{
			OnYesClicked();
		}
		if (UnityNoButton != null && WantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons && instance.IsJustPressed(InputFunction.AltAction))
		{
			OnNoClicked();
		}
		if (YesSelected)
		{
			Finished = true;
			OnYes(inputFrame);
			YesSelected = false;
		}
		if (NoSelected)
		{
			Finished = true;
			OnNo(inputFrame);
			NoSelected = false;
		}
	}

	public override bool NeedsSession()
	{
		return NeedSession;
	}
}

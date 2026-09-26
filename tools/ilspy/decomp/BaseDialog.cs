using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseDialog : BaseSelectionOwner
{
	protected Button UnityOKButton;

	protected Button UnityCancelButton;

	protected TextMeshProUGUI UnityOKText;

	protected TextMeshProUGUI UnityCancelText;

	protected bool OKSelected;

	protected bool Finished;

	protected static StringBuilder sb = new StringBuilder();

	public static int MENU_OK = StringUtil.JenkinsHash("MENU_OK");

	public static int MENU_Cancel = StringUtil.JenkinsHash("MENU_Cancel");

	public int OkButtonHash = MENU_OK;

	public bool WantOkCancelButtonPromptsForController = true;

	public override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}

	public virtual void OnActivate()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		base.gameObject.SetActive(value: true);
		GameObject gameObject = base.gameObject.FindChild("MenuLayout/OKButton");
		GameObject gameObject2 = base.gameObject.FindChild("MenuLayout/CancelButton");
		GameObject gameObject3 = base.gameObject.FindChild("MenuLayout/OKButton/Text");
		GameObject gameObject4 = base.gameObject.FindChild("MenuLayout/CancelButton/Text");
		UnityOKButton = ((gameObject != null) ? gameObject.GetComponent<Button>() : null);
		UnityCancelButton = ((gameObject2 != null) ? gameObject2.GetComponent<Button>() : null);
		UnityOKText = ((gameObject3 != null) ? gameObject3.GetComponent<TextMeshProUGUI>() : null);
		UnityCancelText = ((gameObject4 != null) ? gameObject4.GetComponent<TextMeshProUGUI>() : null);
	}

	public virtual void OnDeactivate()
	{
		DeactivateEventSystem(popped: true);
		Finished = false;
	}

	public virtual void OnHidden()
	{
		if ((bool)UnityEventSystem)
		{
			UnityEventSystem.gameObject.SetActive(value: false);
		}
		UnityCanvasGroup.interactable = false;
	}

	public virtual void OnShown()
	{
	}

	public virtual void PreHandleInput(InputFrame inputFrame)
	{
	}

	public virtual void HandleInput(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (UnityOKButton != null && WantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons && instance.IsJustPressed(InputFunction.MenuSelect))
		{
			OnOK();
		}
		if (instance.IsJustPressed(InputFunction.Back))
		{
			OnCancel();
		}
	}

	public virtual void OnOK()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		OKSelected = true;
		InputFunctionManager.Instance.Capture(InputFunction.MenuSelect, untilReleased: true);
	}

	public virtual void OnCancel()
	{
		SoundManager.PlayMenuSound(SoundManager.CancelSound);
		Finished = true;
	}

	public virtual void DialogUpdate()
	{
		bool wantOkCancelButtonPromptsForController = WantOkCancelButtonPromptsForController;
		if (UnityOKButton != null)
		{
			sb.Length = 0;
			if (wantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				sb.AppendButtonPromptString(InputFunction.MenuSelect);
			}
			sb.Append(GameImpl.Translate(OkButtonHash));
			UnityOKText.SetUnityText(sb);
			UnityOKButton.interactable = !wantOkCancelButtonPromptsForController || SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		}
		if (UnityCancelButton != null)
		{
			sb.Length = 0;
			if (wantOkCancelButtonPromptsForController && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				sb.AppendButtonPromptString(InputFunction.Back);
			}
			sb.Append(GameImpl.Translate(MENU_Cancel));
			UnityCancelText.SetUnityText(sb);
			UnityCancelButton.interactable = !wantOkCancelButtonPromptsForController || SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		}
		if (UnityEventSystem != null && EventSystem.current == null)
		{
			UnityEventSystem.gameObject.SetActive(value: true);
		}
		UnityCanvasGroup.interactable = true;
	}

	public bool IsFinished()
	{
		return Finished;
	}

	public virtual bool NeedsSession()
	{
		return false;
	}

	public virtual ActionMenu GetActionMenu()
	{
		return null;
	}
}

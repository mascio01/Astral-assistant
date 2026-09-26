using TMPro;

public class ConfirmationBox : BaseDialog
{
	public delegate void ConfirmFunction(InputFrame inputFrame);

	public string Message;

	public ConfirmFunction OnConfirm;

	public bool NeedSession;

	public override void OnActivate()
	{
		base.OnActivate();
		base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>().SetUnityText(Message);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (OKSelected)
		{
			Finished = true;
			OnConfirm(inputFrame);
			OKSelected = false;
		}
	}

	public override bool NeedsSession()
	{
		return NeedSession;
	}
}

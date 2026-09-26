using TMPro;

public class MessageBox : BaseDialog
{
	public string Message;

	public override void OnActivate()
	{
		base.OnActivate();
		base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>().SetUnityText(Message);
	}

	public override void OnOK()
	{
		base.OnOK();
		Finished = true;
	}
}

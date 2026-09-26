public class AllowJoinDialog : BaseDialog
{
	public delegate void AcceptFunction();

	public AcceptFunction OnAccept;

	public void OnAnyone()
	{
		Accept(AllowJoinMode.AllowAnyoneToJoin);
	}

	public void OnFriendsOnly()
	{
		Accept(AllowJoinMode.AllowFriendsToJoin);
	}

	public void OnInviteOnly()
	{
		Accept(AllowJoinMode.DontAllowAnyoneToJoin);
	}

	private void Accept(AllowJoinMode allowJoinMode)
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.Settings.AllowJoinMode = allowJoinMode;
		Finished = true;
		OnAccept();
	}
}

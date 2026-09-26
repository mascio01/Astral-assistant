public class QuestFailedBox : BaseDialog
{
	public void OnContinue()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Finished = true;
		NotificationManager.Instance.CloseCurrentNotification();
	}

	public void OnReload()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Finished = true;
		NotificationManager.Instance.CloseCurrentNotification();
		Session.Instance.WantFinish = WantFinishState.LoadLatestSave;
	}
}

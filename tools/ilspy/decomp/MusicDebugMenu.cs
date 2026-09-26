public class MusicDebugMenu : DebugMenu
{
	public MusicDebugMenu()
		: base(GameImpl.Translate("DEBUG_MusicDebug"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		MusicManager musicManager = GameImpl.Instance.MusicManager;
		Items.Clear();
		foreach (MusicPlayer item in musicManager.AllPlayingMusic)
		{
			Items.Add(new DebugMenuItemText(item.MusicSituation.ToString() + ((item == musicManager.CurrentMusic) ? "  (active)" : ""), item.SongName + ", transition: " + item.Transition + ((item.UnityAudioSource != null && item.UnityAudioSource.isPlaying) ? (", time: " + item.UnityAudioSource.time + "s") : " (paused)")));
		}
	}
}

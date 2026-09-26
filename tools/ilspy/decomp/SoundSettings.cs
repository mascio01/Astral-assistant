public class SoundSettings : BaseMenu
{
	private bool WantRepopulate;

	private bool WantSaveSettings;

	public override void OnActivate()
	{
		base.OnActivate();
		Populate();
	}

	public override void OnDeactivate(bool popped)
	{
		if (WantSaveSettings)
		{
			GameImpl.Instance.AutoSaveSettings();
			WantSaveSettings = false;
		}
		base.OnDeactivate(popped);
	}

	public void Populate()
	{
		WantRepopulate = false;
		SetSliderValue("MenuLayout/GameplayVolumeField/Slider", SoundManager.WorldSoundVolume, 0f, 1f);
		SetSliderValue("MenuLayout/BackgroundVolumeField/Slider", SoundManager.BackgroundSoundVolume, 0f, 1f);
		SetSliderValue("MenuLayout/MusicVolumeField/Slider", SoundManager.MusicSoundVolume, 0f, 1f);
		SetSliderValue("MenuLayout/UIVolumeField/Slider", SoundManager.MenuSoundVolume, 0f, 1f);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			Populate();
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}

	public void OnSetGameplayVolume(float v)
	{
		GameImpl.Instance.Settings.SoundFXVolume = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetBackgroundVolume(float v)
	{
		GameImpl.Instance.Settings.BackgroundSoundVolume = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnMusicVolume(float v)
	{
		GameImpl.Instance.Settings.MusicSoundVolume = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetMenuVolume(float v)
	{
		GameImpl.Instance.Settings.MenuSoundVolume = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}
}

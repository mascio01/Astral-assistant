using UnityEngine;

public class SettingsMenu : BaseMenu
{
	public override void OnActivate()
	{
		base.OnActivate();
		GameObject gameObject = base.transform.Find("MenuLayout/OnlineSettingsButton").gameObject;
		GameObject obj = base.transform.Find("MenuLayout/TOSButton").gameObject;
		gameObject.SetActive(GameImpl.Instance.IsMultiplayerEnabled());
		obj.SetActive(value: false);
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public void OnOnlineSettings(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("NetworkSessionMenuPanel"));
	}

	public void OnControlsSettings(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("ControlsSettingsPanel"));
	}

	public void OnSoundSettings(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("SoundSettingsPanel"));
	}

	public void OnGraphicsSettings(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("GraphicsSettingsPanel"));
	}

	public void OnLanguageSettings(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("LanguagePanel"));
	}

	public void OnCredits(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("CreditsPanel"));
	}

	public void OnPatchNotes(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("PatchNotesPanel"));
	}

	public void OnTOS(string folder)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		Application.OpenURL("https://www.xbox.com/legal/community-standards");
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}
}

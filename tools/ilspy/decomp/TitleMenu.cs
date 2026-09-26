using System;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : BaseMenu
{
	private bool WantLatestSave;

	private TextMeshProUGUI UnityTickerText;

	private Button UnityJoinGameBtn;

	public override bool GetWantLatestSave()
	{
		return WantLatestSave;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GameObject gameObject = base.transform.Find("MenuLayout/ResumeLastSaveButton").gameObject;
		GameObject gameObject2 = base.transform.Find("MenuLayout/NewGameButton").gameObject;
		GameObject gameObject3 = base.transform.Find("MenuLayout/JoinGameButton").gameObject;
		GameObject gameObject4 = base.transform.Find("MenuLayout/TextDumpButton").gameObject;
		GameObject obj = base.transform.Find("MenuLayout/EditorButton").gameObject;
		GameObject obj2 = base.transform.Find("DemoTitle").gameObject;
		GameObject gameObject5 = base.transform.Find("DiscordLink").gameObject;
		gameObject.SetActive(SaveGameManager.Instance.HasAnySaveGames);
		obj2.SetActive(GameImpl.Instance.IsDemo());
		gameObject3.SetActive(GameImpl.Instance.IsMultiplayerEnabled());
		UnityJoinGameBtn = gameObject3.GetComponent<Button>();
		gameObject4.SetActive(value: false);
		obj.SetActive(!GameImpl.Instance.IsSteamDeck());
		UnityEventSystem.firstSelectedGameObject = (gameObject.activeSelf ? gameObject : gameObject2);
		bool flag = GameImpl.Instance.Settings.Language == Language.Russian && (!SaveGameManager.Instance.HasAnySaveGames || MathUtil.NonDeterministicRand.RandomChoice(0.05f));
		UnityTickerText = base.transform.Find("Ticker/Text").GetComponent<TextMeshProUGUI>();
		UnityTickerText.transform.parent.gameObject.SetActive(flag);
		((RectTransform)gameObject5.transform).anchoredPosition = new Vector2(-32f, flag ? 112f : 64f);
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantLatestSave = false;
	}

	public void OnNewGame()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SelectStoryMenu selectStoryMenu = (SelectStoryMenu)GameImpl.Instance.GetMenuBehaviourByPanelName("SelectStoryPanel");
		selectStoryMenu.ActiveMods.Clear();
		selectStoryMenu.FinishedAction = SelectStoryMenu.Action.StartNewGame;
		OpenChildMenu(selectStoryMenu);
	}

	public void OnResumeLastSave()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		if (GameImpl.Instance.WantShowAllowJoinDialog())
		{
			GameImpl.Instance.ShowAllowJoinDialogBox(ResumeLastSave);
		}
		else
		{
			ResumeLastSave();
		}
	}

	private void ResumeLastSave()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantLatestSave = true;
		WantFadeOut = true;
	}

	public void OnLoadGame()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SaveGameMenu saveGameMenu = GameImpl.Instance.GetMenuBehaviourByPanelName("SaveGameMenuPanel") as SaveGameMenu;
		saveGameMenu.Loading = true;
		OpenChildMenu(saveGameMenu);
	}

	public void OnJoinGame()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("JoinGameMenuPanel"));
	}

	public void OnSettings()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("SettingsMenuPanel"));
	}

	public void OnRunEditor()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SelectStoryMenu selectStoryMenu = (SelectStoryMenu)GameImpl.Instance.GetMenuBehaviourByPanelName("SelectStoryPanel");
		selectStoryMenu.ActiveMods.Clear();
		selectStoryMenu.FinishedAction = SelectStoryMenu.Action.Editor;
		OpenChildMenu(selectStoryMenu);
	}

	public void OnQuit()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Application.Quit();
	}

	public void OnDiscordLink()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Application.OpenURL("https://discord.gg/5mKaRg6NWs");
	}

	public void OnTextDump()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		OpenFileName openFileName = new OpenFileName();
		openFileName.structSize = Marshal.SizeOf(openFileName);
		openFileName.filter = "sav files (*.sav)\0*.sav\0All files (*.*)\0*.*\0\0";
		openFileName.file = new string(new char[256]);
		openFileName.maxFile = openFileName.file.Length;
		openFileName.fileTitle = new string(new char[64]);
		openFileName.maxFileTitle = openFileName.fileTitle.Length;
		openFileName.initialDir = GameImpl.Instance.SaveGamePath + "/OutOfSync";
		openFileName.title = "Open Savegame";
		openFileName.defExt = "SAV";
		openFileName.flags = 530440;
		if (DllTest.GetOpenFileName(openFileName))
		{
			WantFadeOut = GameImpl.Instance.LoadGameFromPathForTextDump(openFileName.file);
		}
	}

	public void OnPlayRecording()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		OpenFileName openFileName = new OpenFileName();
		openFileName.structSize = Marshal.SizeOf(openFileName);
		openFileName.filter = "rec files (*.rec)\0*.rec\0All files (*.*)\0*.*\0\0";
		openFileName.file = new string(new char[256]);
		openFileName.maxFile = openFileName.file.Length;
		openFileName.fileTitle = new string(new char[64]);
		openFileName.maxFileTitle = openFileName.fileTitle.Length;
		openFileName.initialDir = GameImpl.Instance.SaveGamePath + "/OutOfSync";
		openFileName.title = "Open Savegame";
		openFileName.defExt = "SAV";
		openFileName.flags = 530440;
		if (!DllTest.GetOpenFileName(openFileName))
		{
			return;
		}
		using Stream stream = File.OpenRead(openFileName.file);
		using CustomBinaryReader reflector = new CustomBinaryReader(stream);
		try
		{
			InputsRecord inputsRecord = new InputsRecord();
			inputsRecord.Reflect(reflector);
			Session.WantPlayingInputs = inputsRecord;
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			Debug.Log("Error opening " + openFileName.file + ": " + ex.ToString());
		}
	}

	public override void Update()
	{
		base.Update();
		UnityJoinGameBtn.interactable = GameImpl.Instance.IsOnline();
	}
}

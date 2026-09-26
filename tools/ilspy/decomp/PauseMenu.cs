using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : BaseMenu
{
	public bool WantQuit;

	public bool WantRespawn;

	private bool ShowingQuicksavePrompt;

	private GameObject UnityQuickSaveObj;

	private RawImage UnityQuickSaveIcon;

	private GameObject UnitySpacerObj;

	private TMP_Text UnityQuickSaveCount;

	private TMP_Text UnityQuickSavePrompt;

	public override void OnActivate()
	{
		base.OnActivate();
		Session instance = Session.Instance;
		OnlineParty instance2 = OnlineParty.Instance;
		bool flag = !instance2.IsInMultiplayerGame() || instance2.IsInMultiplayerGameAsLeader();
		GameObject obj = base.transform.Find("MenuLayout/RespawnButton").gameObject;
		GameObject obj2 = base.transform.Find("MenuLayout/SaveGameButton").gameObject;
		GameObject gameObject = base.transform.Find("MenuLayout/LoadGameButton").gameObject;
		GameObject gameObject2 = base.transform.Find("MenuLayout/SelectModsButton").gameObject;
		GameObject gameObject3 = base.transform.Find("MenuLayout/EditorButton").gameObject;
		obj2.SetActive(flag && instance != null && !instance.Editor && !instance.DifficultySettings.SaveTokensRequired);
		gameObject.SetActive(flag);
		gameObject2.SetActive(!instance2.IsInMultiplayerGame());
		gameObject3.SetActive(!GameImpl.Instance.IsSteamDeck());
		bool active = false;
		if (instance != null && instance.State == SessionState.Started && instance.IsInMultiplayerGameAsCommunityFollower())
		{
			PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
			if (localPlayerRecord != null && localPlayerRecord.HasCreatedCharacter && instance.IncomingCharacter == null && instance.InputFrame > instance.InputFrameWhenWeLastSentCreateCharacter && instance.CommunityManager.PlayerCommunity.HasAnyActiveMembers())
			{
				Character avatarForPlayer = instance.CharacterManager.GetAvatarForPlayer(localPlayerRecord.PlayerID);
				if ((avatarForPlayer == null || !avatarForPlayer.AliveAndNotZombie || avatarForPlayer.Disappeared) && (avatarForPlayer == null || avatarForPlayer.Rank != Rank.Leader))
				{
					active = true;
				}
			}
		}
		obj.SetActive(active);
		UnityQuickSaveObj = base.transform.Find("Quicksave").gameObject;
		UnityQuickSaveIcon = base.transform.Find("Quicksave/Icon").GetComponent<RawImage>();
		UnitySpacerObj = base.transform.Find("Quicksave/Spacer").gameObject;
		UnityQuickSaveCount = base.transform.Find("Quicksave/Count").GetComponent<TMP_Text>();
		UnityQuickSavePrompt = base.transform.Find("Quicksave/Name").GetComponent<TMP_Text>();
		UpdateQuickSavePrompt();
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantQuit = false;
		WantRespawn = false;
	}

	private void UpdateQuickSavePrompt()
	{
		Session instance = Session.Instance;
		OnlineParty instance2 = OnlineParty.Instance;
		bool flag = !instance2.IsInMultiplayerGame() || instance2.IsInMultiplayerGameAsLeader();
		ShowingQuicksavePrompt = flag && instance != null && !instance.Editor && !InputFunctionManager.Instance.IsMapped(InputFunction.QuickSave);
		if (ShowingQuicksavePrompt)
		{
			UnityQuickSaveIcon.gameObject.SetActive(instance.DifficultySettings.SaveTokensRequired);
			UnityQuickSaveCount.gameObject.SetActive(instance.DifficultySettings.SaveTokensRequired);
			UnitySpacerObj.SetActive(instance.DifficultySettings.SaveTokensRequired);
			if (instance.DifficultySettings.SaveTokensRequired)
			{
				int num = instance.CommunityManager.PlayerCommunity.CountInventoryItemsOfClass(typeof(SavegameToken), includeBuildings: true);
				ShowingQuicksavePrompt = num > 0;
				if (num > 0)
				{
					Equipment equipment = instance.CommunityManager.PlayerCommunity.FindInventoryItemOfClass(typeof(SavegameToken));
					if (equipment != null)
					{
						UnityQuickSaveIcon.texture = equipment.GetIcon(out var mat, out var col, highlighted: false);
						UnityQuickSaveIcon.material = mat;
						UnityQuickSaveIcon.color = col;
					}
					UnityQuickSaveCount.SetUnityTextIfDifferent(num.ToString());
				}
			}
			UnityQuickSavePrompt.SetUnityTextIfDifferent(StringUtil.GetButtonPromptString(InputFunction.AltAction) + " " + GameImpl.Translate("INPUT_QuickSave"));
		}
		UnityQuickSaveObj.SetActive(ShowingQuicksavePrompt);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		UpdateQuickSavePrompt();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantFadeOut = true;
		}
		if (!ShowingQuicksavePrompt || !instance.IsJustPressed(InputFunction.AltAction))
		{
			return;
		}
		Session instance2 = Session.Instance;
		if (instance2.Editor || !instance2.IsPartyLeader())
		{
			return;
		}
		if (instance2.DifficultySettings.SaveTokensRequired)
		{
			if (inputFrame != null)
			{
				inputFrame.AddAction(new InputAction(InputActionType.QuickSaveUsingToken));
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				WantFadeOut = true;
			}
		}
		else
		{
			GameImpl.Instance.RequestedAutoSaveFromThread = SaveGameType.QuickSave;
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
			WantFadeOut = true;
		}
	}

	public void OnResumeGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantFadeOut = true;
	}

	public void OnRespawn()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantFadeOut = true;
		WantRespawn = true;
	}

	public void OnLoadGame()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SaveGameMenu saveGameMenu = GameImpl.Instance.GetMenuBehaviourByPanelName("SaveGameMenuPanel") as SaveGameMenu;
		saveGameMenu.Loading = true;
		OpenChildMenu(saveGameMenu);
	}

	public void OnSaveGame()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SaveGameMenu saveGameMenu = GameImpl.Instance.GetMenuBehaviourByPanelName("SaveGameMenuPanel") as SaveGameMenu;
		saveGameMenu.Loading = false;
		OpenChildMenu(saveGameMenu);
	}

	public void OnSelectMods()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		SelectStoryMenu selectStoryMenu = (SelectStoryMenu)GameImpl.Instance.GetMenuBehaviourByPanelName("SelectStoryPanel");
		GameImpl.Instance.GetCurrentActiveMods(selectStoryMenu.ActiveMods);
		selectStoryMenu.FinishedAction = SelectStoryMenu.Action.SelectMods;
		OpenChildMenu(selectStoryMenu);
	}

	public void OnSettings()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("SettingsMenuPanel"));
	}

	public void OnEditEquipment()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditorMenuPanel"));
	}

	public void OnQuitGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.OnlineParty.LeaveLobby();
		WantQuit = true;
		WantFadeOut = true;
	}
}

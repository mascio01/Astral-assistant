using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkSessionMenu : BaseMenu
{
	public static NetworkSessionMenu Instance;

	public bool WantRepopulate;

	private bool WantSaveSettings;

	public ActionMenu ActionMenu = new ActionMenu();

	private PlayerID PlayerIDToKick;

	private GameObject UnityNetworkProtocolDropdown;

	private List<Selectable> Selectables = new List<Selectable>();

	private List<Button> NameButtons = new List<Button>();

	private List<Button> KickButtons = new List<Button>();

	private static float TooltipMenuXOffset = 170f;

	private static int MENU_NetworkProtocolHelp = StringUtil.JenkinsHash("MENU_NetworkProtocolHelp");

	public override ActionMenu GetActionMenu()
	{
		if (!(ChildMenu != null))
		{
			return ActionMenu;
		}
		return ChildMenu.GetActionMenu();
	}

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		Instance = this;
	}

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
		Selectables.Clear();
		NameButtons.Clear();
		KickButtons.Clear();
		OnlineParty onlineParty = GameImpl.Instance.OnlineParty;
		GameObject gameObject = base.transform.Find("PlayerList").gameObject;
		gameObject.SetActive(onlineParty.IsInMultiplayerGame());
		if (gameObject.activeSelf)
		{
			int num = 0;
			foreach (PartyMember partyMember in onlineParty.PartyMembers)
			{
				if (partyMember.IsBanned)
				{
					continue;
				}
				PlayerID playerID = partyMember.PlayerID;
				GameObject gameObject2 = ((num >= gameObject.transform.childCount) ? Object.Instantiate((GameObject)Hud.PlayerDisplayPrefab, gameObject.transform) : gameObject.transform.GetChild(num).gameObject);
				gameObject2.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()
					.SetUnityText(partyMember.GetPlayerName());
				Button component = gameObject2.transform.GetChild(0).GetComponent<Button>();
				component.onClick.RemoveAllListeners();
				component.onClick.AddListener(delegate
				{
					SteamFriends.ActivateGameOverlayToUser("steamid", playerID.SteamID);
				});
				Button component2 = gameObject2.transform.GetChild(1).GetComponent<Button>();
				component2.gameObject.SetActive(onlineParty.IsInMultiplayerGameAsLeader() && !partyMember.IsLocal);
				if (component2.gameObject.activeSelf)
				{
					component2.onClick.RemoveAllListeners();
					component2.onClick.AddListener(delegate
					{
						PartyMember partyMemberByID = OnlineParty.Instance.GetPartyMemberByID(playerID);
						if (partyMemberByID != null && !partyMemberByID.IsBanned)
						{
							PlayerIDToKick = partyMemberByID.PlayerID;
							GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_AreYouSureKick").Replace("%1", partyMember.GetPlayerName()), OnConfirmKick);
						}
					});
				}
				Selectables.Add(component);
				NameButtons.Add(component);
				KickButtons.Add(component2);
				num++;
			}
			for (; num < gameObject.transform.childCount; num++)
			{
				Object.Destroy(gameObject.transform.GetChild(num).gameObject);
			}
		}
		GameObject gameObject3 = base.transform.Find("MenuLayout/ResetBannedButton").gameObject;
		gameObject3.SetActive(GameImpl.Instance.Settings.BannedPlayers != null && GameImpl.Instance.Settings.BannedPlayers.Count > 0);
		if (gameObject3.activeSelf)
		{
			Selectables.Add(gameObject3.GetComponent<Button>());
		}
		Selectables.Add(SetDropdownValue("MenuLayout/AllowJoinModeField/Dropdown", (int)GameImpl.Instance.Settings.AllowJoinMode, new string[3]
		{
			GameImpl.Translate("MENU_AllowNoone"),
			GameImpl.Translate("MENU_AllowFriendsOnly"),
			GameImpl.Translate("MENU_AllowAnyone")
		}));
		Selectables.Add(SetCheckboxValue("MenuLayout/AskMeEachTimeField/Toggle", GameImpl.Instance.Settings.ShowAllowJoinDialog));
		int partySizeLimit = OnlineParty.GetPartySizeLimit();
		string[] array = new string[partySizeLimit - 1];
		for (int num2 = 2; num2 <= partySizeLimit; num2++)
		{
			array[num2 - 2] = num2.ToString();
		}
		Selectables.Add(SetDropdownValue("MenuLayout/MaxPartySizeField/Dropdown", MathUtil.Clamp(GameImpl.Instance.Settings.MaxPartySize, 2, partySizeLimit) - 2, array));
		Selectables.Add(SetCheckboxValue("MenuLayout/VoiceChatEnabledField/Toggle", GameImpl.Instance.Settings.VoiceChatEnabled));
		Selectables.Add(SetCheckboxValue("MenuLayout/PushButtonToTalkField/Toggle", GameImpl.Instance.Settings.PushBtnToTalkEnabled));
		UnityNetworkProtocolDropdown = base.transform.Find("MenuLayout/NetworkProtocolField/Dropdown").gameObject;
		bool flag = GameImpl.Instance.GetState() == GameState.TitleMenu;
		base.transform.Find("MenuLayout/NetworkProtocolField").gameObject.SetActive(flag);
		if (flag)
		{
			Selectables.Add(SetDropdownValue("MenuLayout/NetworkProtocolField/Dropdown", (int)GameImpl.Instance.Settings.NetworkProtocol, new string[2]
			{
				GameImpl.Translate("MENU_SteamNetworking"),
				GameImpl.Translate("MENU_SteamNetworkingMessages")
			}));
		}
		GameObject gameObject4 = base.transform.Find("MenuLayout/InviteButton").gameObject;
		gameObject4.SetActive(OnlineParty.Instance.CurrentLobby != null);
		Selectables.Add(gameObject4.GetComponent<Button>());
		Selectables.Add(base.gameObject.FindChild("MenuLayout/BackButton").GetComponent<Button>());
		BaseMenu.SetupNavigation(Selectables, topAndBottomAreAutomatic: false);
		for (int num3 = 0; num3 < NameButtons.Count; num3++)
		{
			Navigation navigation = NameButtons[num3].navigation;
			Navigation navigation2 = KickButtons[num3].navigation;
			navigation.selectOnRight = KickButtons[num3];
			navigation2.mode = Navigation.Mode.Explicit;
			navigation2.selectOnLeft = NameButtons[num3];
			navigation2.selectOnUp = ((num3 > 0) ? KickButtons[num3 - 1] : null);
			navigation2.selectOnDown = ((num3 < KickButtons.Count - 1) ? KickButtons[num3 + 1] : ((NameButtons.Count < Selectables.Count) ? Selectables[NameButtons.Count] : null));
			NameButtons[num3].navigation = navigation;
			KickButtons[num3].navigation = navigation2;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && current.currentSelectedGameObject == UnityNetworkProtocolDropdown)
		{
			ActionMenu.FocusUnityObj = UnityNetworkProtocolDropdown;
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_NetworkProtocolHelp), TooltipMenuXOffset, leftAligned: true));
		}
		ActionMenu.OnFinishAddingActions();
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

	public void OnResetBannedPlayers()
	{
		GameImpl.Instance.ResetBannedPlayers();
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetAllowJoinMode(int i)
	{
		if (GameImpl.Instance.Settings.AllowJoinMode != (AllowJoinMode)i)
		{
			GameImpl.Instance.SetAllowJoinMode((AllowJoinMode)i);
			WantSaveSettings = true;
		}
	}

	public void OnSetNetworkProtocol(int i)
	{
		if (GameImpl.Instance.Settings.NetworkProtocol != (NetworkProtocol)i)
		{
			OnlineParty.Instance.SetNetworkProtocol((NetworkProtocol)i);
		}
	}

	public void OnToggleAskMeEachTime(bool on)
	{
		if (GameImpl.Instance.Settings.ShowAllowJoinDialog != on)
		{
			GameImpl.Instance.Settings.ShowAllowJoinDialog = on;
			WantSaveSettings = true;
		}
	}

	public void OnSetMaxPartySize(int i)
	{
		int num = i + 2;
		if (GameImpl.Instance.Settings.MaxPartySize != num)
		{
			GameImpl.Instance.SetMaxPartySize(num);
			WantSaveSettings = true;
		}
	}

	public void OnToggleVoiceChatEnabled(bool on)
	{
		if (GameImpl.Instance.Settings.VoiceChatEnabled != on)
		{
			GameImpl.Instance.Settings.VoiceChatEnabled = on;
			WantSaveSettings = true;
		}
	}

	public void OnTogglePushButtonToTalk(bool on)
	{
		if (GameImpl.Instance.Settings.PushBtnToTalkEnabled != on)
		{
			GameImpl.Instance.Settings.PushBtnToTalkEnabled = on;
			WantSaveSettings = true;
		}
	}

	public void OnInvite()
	{
		Lobby currentLobby = OnlineParty.Instance.CurrentLobby;
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		if (currentLobby != null)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(currentLobby.LobbyID.SteamID);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}

	public void OnConfirmKick(InputFrame inputFrame)
	{
		OnlineParty.Instance.KickFromGame(PlayerIDToKick, kickedOnJoin: false, banned: true);
		GameImpl.Instance.BanPlayer(PlayerIDToKick);
		WantSaveSettings = true;
		WantRepopulate = true;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoinGameMenu : BaseMenu
{
	public ActionMenu ActionMenu = new ActionMenu();

	private Dictionary<GameObject, Lobby> UnityLobbyButtons = new Dictionary<GameObject, Lobby>();

	public static float JoinGameMenuXOffset = 180f;

	private static int MENU_ActiveMods = StringUtil.JenkinsHash("MENU_ActiveMods");

	private static Dictionary<StorySource, int> StoryHashCache = new Dictionary<StorySource, int>();

	public override ActionMenu GetActionMenu()
	{
		if (!(ChildMenu != null))
		{
			return ActionMenu;
		}
		return ChildMenu.GetActionMenu();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		StoryHashCache.Clear();
		GameImpl.Instance.OnlineParty.SearchForLobbies();
		UpdateLobbyList();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			if (GameImpl.Instance.OnlineParty.CurrentState == OnlineParty.State.SearchingForLobbies || GameImpl.Instance.OnlineParty.CurrentState == OnlineParty.State.JoiningLobby)
			{
				return;
			}
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
		ActionMenu.ClearActions();
		Lobby value = null;
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && UnityLobbyButtons.TryGetValue(current.currentSelectedGameObject, out value))
		{
			ActionMenu.FocusUnityObj = current.currentSelectedGameObject;
			foreach (StorySource storySource in value.StorySources)
			{
				if (storySource.WorkshopId != 0L)
				{
					string translatedName = storySource.GetTranslatedName();
					if (!string.IsNullOrEmpty(translatedName))
					{
						bool leftAligned = WorkshopManager.Instance.IsDownloaded(storySource.WorkshopId);
						ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.ModName, translatedName, JoinGameMenuXOffset, leftAligned));
					}
				}
			}
			if (ActionMenu.HeaderActions.Count > 0)
			{
				string text = GameImpl.Translate(MENU_ActiveMods);
				ActionMenu.HeaderActions.Insert(0, new AvailableAction(CursorAction.Tooltip, text, JoinGameMenuXOffset, leftAligned: true));
				ActionMenu.HeaderActions.Insert(1, new AvailableAction(CursorAction.Separator, null));
			}
		}
		ActionMenu.OnFinishAddingActions();
		ActionMenu.HandleInputForMenuActions(inputFrame);
	}

	public void OnSelectLobbyToJoin(Lobby lobby)
	{
		if (OnlineParty.Instance.CanJoinOrCreateLobby() && GameImpl.Instance.OnlineParty.CurrentState != OnlineParty.State.SearchingForLobbies && GameImpl.Instance.OnlineParty.CurrentState != OnlineParty.State.JoiningLobby)
		{
			GameImpl.Instance.OnlineParty.JoinLobby(lobby);
			UpdateLobbyList();
		}
	}

	public void OnRefresh()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		if (GameImpl.Instance.OnlineParty.CurrentState == OnlineParty.State.Idle)
		{
			GameImpl.Instance.OnlineParty.SearchForLobbies();
			UpdateLobbyList();
		}
	}

	public void OnBack()
	{
		if (GameImpl.Instance.OnlineParty.CurrentState != OnlineParty.State.SearchingForLobbies && GameImpl.Instance.OnlineParty.CurrentState != OnlineParty.State.JoiningLobby)
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public override void OnLobbyListSearchFinished()
	{
		UpdateLobbyList();
	}

	public override void OnJoinLobbyFailed(string errorMsg)
	{
		UpdateLobbyList();
	}

	public override void OnWorkshopItemQueryFinished()
	{
		UpdateLobbyList();
	}

	public static bool HasStoryHashMismatch(Lobby lobby, ref StorySource mismatchedStorySource)
	{
		for (int i = 0; i < lobby.StorySources.Count; i++)
		{
			int value = 0;
			if (!string.IsNullOrEmpty(lobby.StorySources[i].AbsolutePath) && !StoryHashCache.TryGetValue(lobby.StorySources[i], out value))
			{
				value = (StoryHashCache[lobby.StorySources[i]] = Story.CalcStoryHash(lobby.StorySources[i].AbsolutePath));
			}
			if (value != 0 && value != lobby.StoryHashes[i])
			{
				mismatchedStorySource = lobby.StorySources[i];
				return true;
			}
		}
		return false;
	}

	private void UpdateLobbyList()
	{
		try
		{
			OnlineParty onlineParty = GameImpl.Instance.OnlineParty;
			GameObject gameObject = base.gameObject.transform.Find("JoinGameList/Viewport/Content").gameObject;
			for (int num = gameObject.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
			}
			if (onlineParty.CurrentState == OnlineParty.State.SearchingForLobbies)
			{
				AddMenuText(gameObject, 0, GameImpl.Translate("MENU_Searching"));
				return;
			}
			if (onlineParty.CurrentState == OnlineParty.State.JoiningLobby)
			{
				AddMenuText(gameObject, 0, GameImpl.Translate("MENU_Joining"));
				return;
			}
			if (onlineParty.Lobbies.Count == 0)
			{
				AddMenuText(gameObject, 0, GameImpl.Translate("MENU_NoGamesFound"));
				return;
			}
			for (int i = 0; i < onlineParty.Lobbies.Count; i++)
			{
				Lobby lobby = onlineParty.Lobbies[i];
				int maxLobbyMembers = lobby.GetMaxLobbyMembers();
				bool enabled = lobby.NumLobbyMembers < maxLobbyMembers;
				StorySource mismatchedStorySource = null;
				if (HasStoryHashMismatch(lobby, ref mismatchedStorySource))
				{
					enabled = false;
				}
				string description = ((lobby.Day > 0) ? LoadingMenu.BuildSessionDescriptionString(lobby.Day, lobby.DayOfYear) : string.Empty);
				string text = string.Empty;
				if (lobby.StoryIndex >= 0 && lobby.StoryIndex < lobby.StorySources.Count)
				{
					text = lobby.StorySources[lobby.StoryIndex].GetTranslatedName();
					int num2 = lobby.StorySources.Count - (lobby.StoryIndex + 1);
					if (num2 > 0)
					{
						text = text + " (" + GameImpl.Translate(SelectStoryMenu.MENU_Mods) + ": " + num2 + ")";
					}
				}
				GameObject key = AddJoinGameButton(gameObject, i, enabled, text, lobby.LeaderName, lobby.NumLobbyMembers, maxLobbyMembers, lobby.CommunitySize, description, delegate
				{
					if (enabled)
					{
						OnSelectLobbyToJoin(lobby);
					}
					else
					{
						for (int j = 0; j < lobby.StorySources.Count; j++)
						{
							int value = 0;
							if (!string.IsNullOrEmpty(lobby.StorySources[j].AbsolutePath))
							{
								StoryHashCache.TryGetValue(lobby.StorySources[j], out value);
							}
							if (value != 0 && value != lobby.StoryHashes[j])
							{
								GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_StoryHashMismatch").Replace("%1", lobby.StorySources[j].GetTranslatedName()));
								break;
							}
						}
					}
				});
				UnityLobbyButtons[key] = lobby;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}
}

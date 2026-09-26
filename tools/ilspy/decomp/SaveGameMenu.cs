using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class SaveGameMenu : BaseMenu
{
	public enum State
	{
		None,
		Enumerating,
		Selecting,
		Saving,
		Deleting
	}

	private struct ButtonCreated
	{
		public GameObject UnityObj;

		public SaveGame SaveGame;
	}

	public bool Loading;

	public bool Fail;

	public State CurrentState;

	private SaveRequest CurrentRequest;

	private Thread MyThread;

	private List<SaveGame> SaveGames = new List<SaveGame>();

	private List<ButtonCreated> ButtonsCreated = new List<ButtonCreated>();

	public SaveGame WantLoadSaveGame;

	private byte[] ThumbnailBytes;

	private bool First;

	private bool StillRequesting;

	private int AddedSaveGameIndex;

	private static string MainButtonPanelText = "MainButton/Panel/Text";

	private GameObject UnityContents;

	private GameObject UnityQuota;

	private static GameProfiler UpdateEnumeratingTimer = new GameProfiler("UpdateEnumerating");

	private static GameProfiler UpdateRequestingModsTimer = new GameProfiler("UpdateRequestingMods");

	private long RemainingQuota;

	private static GameProfiler EnumerateSaveGameTimer = new GameProfiler("EnumerateSaveGame");

	public override SaveGame GetWantLoadSaveGame()
	{
		return WantLoadSaveGame;
	}

	public override void OnActivate()
	{
		Shutdown();
		base.OnActivate();
		First = true;
		if (!Loading)
		{
			ThumbnailBytes = GameImpl.Instance.CreateScreenshot();
		}
		RemainingQuota = 0L;
		UnityQuota.SetActive(value: false);
		UpdateRemaingQuota();
		StartEnumerating();
	}

	private void Shutdown()
	{
		if (CurrentState == State.Enumerating && MyThread != null)
		{
			MyThread.Join();
			MyThread = null;
		}
		foreach (ButtonCreated item in ButtonsCreated)
		{
			UnityEngine.Object.Destroy(item.UnityObj);
		}
		ButtonsCreated.Clear();
		foreach (SaveGame saveGame in SaveGames)
		{
			if (saveGame.Thumbnail != null)
			{
				if (saveGame.Thumbnail.IsFinishedLoading())
				{
					saveGame.Thumbnail.UnloadResource();
				}
				else
				{
					BaseResource.WantUnloadResources.Add(saveGame.Thumbnail);
				}
			}
		}
		SaveGames.Clear();
		AddedSaveGameIndex = 0;
		WantLoadSaveGame = null;
	}

	public override void OnDeactivate(bool popped)
	{
		Shutdown();
		base.OnDeactivate(popped);
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		UnityContents = base.gameObject.FindChild("SaveGameList/Viewport/Content");
		UnityQuota = base.gameObject.FindChild("Quota");
		UnityQuota.SetActive(value: false);
	}

	private void UpdateUnityQuota()
	{
		UnityQuota.SetActive(RemainingQuota != 0);
		if (RemainingQuota != 0L)
		{
			UnityQuota.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate("MENU_QuotaRemaining").Replace("%1", RemainingQuota.ToBytesString()));
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (First)
		{
			SetMenuCaption(Loading ? "MENU_LoadGameCaption" : "MENU_SaveGameCaption");
			First = false;
		}
		switch (CurrentState)
		{
		case State.Enumerating:
			if (MyThread == null || !MyThread.Join(0))
			{
				break;
			}
			using (new ProfileMarker(UpdateEnumeratingTimer))
			{
				MyThread = null;
				CurrentState = State.Selecting;
				if (!Loading)
				{
					ButtonCreated item = new ButtonCreated
					{
						UnityObj = AddNewSaveGameButton(UnityContents, ButtonsCreated.Count, OnSelectNewSaveGame)
					};
					ButtonsCreated.Add(item);
				}
				StillRequesting = true;
				AddedSaveGameIndex = 0;
				if (ButtonsCreated.Count > 0)
				{
					SelectFirstButton();
				}
				else
				{
					UnityEventSystem.firstSelectedGameObject = base.gameObject.FindChild("MenuLayout/BackButton");
				}
				break;
			}
		case State.Selecting:
		{
			if (AddedSaveGameIndex < SaveGames.Count)
			{
				RectTransform rectTransform = (RectTransform)UnityContents.transform;
				if (rectTransform.anchoredPosition.y + HudBehaviour.Instance.HudPanelRectTransform.rect.height * 2f >= rectTransform.rect.height)
				{
					SaveGame saveGame = SaveGames[AddedSaveGameIndex];
					if (saveGame.Thumbnail == null && File.Exists(saveGame.JPGFilePath))
					{
						saveGame.Thumbnail = new Resource<Texture2D>(saveGame.JPGFilePath);
					}
					else if (saveGame.Thumbnail == null || saveGame.Thumbnail.IsFinishedLoading())
					{
						string text = saveGame.BuildNameString(ref StillRequesting);
						string dayText = LoadingMenu.BuildSessionDescriptionString(saveGame.Data.Day, saveGame.Data.DayOfYear);
						string communitySizeText = GameImpl.Translate("MENU_CommunitySizeSave").Replace("%1", saveGame.Data.CommunitySize.ToString());
						Texture2D tex = ((saveGame.Thumbnail != null) ? saveGame.Thumbnail.GetAsset() : null);
						bool isTokenSave = saveGame.IsTokenSave();
						ButtonCreated item2 = new ButtonCreated
						{
							SaveGame = saveGame,
							UnityObj = AddSaveGameButton(UnityContents, ButtonsCreated.Count, text, dayText, communitySizeText, saveGame.Data.CommunityName, saveGame.Data.CharacterName, isTokenSave, tex, delegate
							{
								OnSelectSaveGame(saveGame);
							}, delegate
							{
								OnOpenSaveGameFolder(saveGame);
							}, delegate
							{
								OnDeleteSaveGame(saveGame);
							})
						};
						ButtonsCreated.Add(item2);
						AddedSaveGameIndex++;
						StillRequesting = true;
						if (ButtonsCreated.Count == 1)
						{
							SelectFirstButton();
						}
					}
				}
			}
			for (int num = 0; num < ButtonsCreated.Count; num++)
			{
				Button[] componentsInChildren = ButtonsCreated[num].UnityObj.GetComponentsInChildren<Button>();
				for (int num2 = 0; num2 < componentsInChildren.Length; num2++)
				{
					componentsInChildren[num2].interactable = Loading || GameImpl.Instance.GetState() == GameState.RunningSession;
				}
			}
			if (!StillRequesting || !GameImpl.Instance.IsOnline())
			{
				break;
			}
			using (new ProfileMarker(UpdateRequestingModsTimer))
			{
				bool requesting = false;
				for (int num3 = 0; num3 < ButtonsCreated.Count; num3++)
				{
					SaveGame saveGame2 = ButtonsCreated[num3].SaveGame;
					if (saveGame2 != null)
					{
						string str = saveGame2.BuildNameString(ref requesting);
						ButtonsCreated[num3].UnityObj.transform.Find(MainButtonPanelText).GetComponent<TextMeshProUGUI>().SetUnityText(str);
					}
				}
				StillRequesting = requesting;
				break;
			}
		}
		case State.Saving:
			if (CurrentRequest.CurrentState != SaveRequest.State.Running)
			{
				if (CurrentRequest.CurrentState == SaveRequest.State.Failed)
				{
					GameImpl.Instance.ShowMessageBox(CurrentRequest.GetErrorMessage());
					CurrentRequest = null;
					CurrentState = State.Selecting;
				}
				else
				{
					WantPop = true;
				}
			}
			break;
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}

	private void StartEnumerating()
	{
		CurrentState = State.Enumerating;
		MyThread = new Thread(EnumerateThreadFunc);
		MyThread.Start();
	}

	private void UpdateRemaingQuota()
	{
		if (Loading)
		{
			return;
		}
		GameImpl instance = GameImpl.Instance;
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			if (driveInfo.IsReady && instance.SaveGamePath.StartsWith(driveInfo.Name))
			{
				RemainingQuota = driveInfo.TotalFreeSpace;
				break;
			}
		}
		UpdateUnityQuota();
	}

	private void EnumerateThreadFunc()
	{
		try
		{
			while (SaveGameManager.Instance.IsSaving())
			{
				Thread.Sleep(10);
			}
			_ = GameImpl.Instance;
			string[] saveGameFolders = SaveGameManager.GetSaveGameFolders();
			for (int i = 0; i < saveGameFolders.Length; i++)
			{
				EnumerateSaveGame(saveGameFolders[i]);
			}
			SaveGames.Sort(SortSaveGamesByTimeStampDescending.Instance);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.ToString());
		}
		Profiler.EndThreadProfiling();
	}

	private void EnumerateSaveGame(string path)
	{
		using (new ProfileMarker(EnumerateSaveGameTimer))
		{
			string fileName = Path.GetFileName(path);
			string path2 = path + "/info.xml";
			string jPGFilePath = path + "/thumbnail.jpg";
			SaveGameManager.ParseSaveGameDirName(fileName, out var saveGameType, out var _, out var gameUniqueId);
			if ((!Loading && saveGameType != SaveGameType.Normal) || saveGameType == SaveGameType.Invalid)
			{
				return;
			}
			try
			{
				SaveGame saveGame = new SaveGame();
				using Stream stream = SaveGameManager.OpenSaveGameFile(path2);
				if (stream != null)
				{
					SaveGameData data = (SaveGameData)new XmlSerializer(typeof(SaveGameData)).Deserialize(stream);
					saveGame.Type = saveGameType;
					saveGame.DirName = fileName;
					saveGame.GameUniqueId = gameUniqueId;
					saveGame.Data = data;
					saveGame.JPGFilePath = jPGFilePath;
					SaveGames.Add(saveGame);
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("Error reading " + fileName + ": " + ex.ToString());
			}
		}
	}

	private bool SaveGameExistsWithDirName(string dirName)
	{
		foreach (SaveGame saveGame in SaveGames)
		{
			if (saveGame.DirName == dirName)
			{
				return true;
			}
		}
		return false;
	}

	private string PickNewSaveGameDirName()
	{
		int num = -1;
		string text;
		do
		{
			num++;
			text = "savegame" + num;
		}
		while (SaveGameExistsWithDirName(text));
		return text;
	}

	public void OnSelectNewSaveGame()
	{
		OnSelectSaveGame(null);
	}

	public void OnSelectSaveGame(SaveGame saveGame)
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		if (Loading)
		{
			if (!OnlineParty.Instance.IsInMultiplayerGame() && instance.WantShowAllowJoinDialog())
			{
				instance.ShowAllowJoinDialogBox(delegate
				{
					OnLaunchSaveGame(saveGame);
				});
			}
			else
			{
				OnLaunchSaveGame(saveGame);
			}
			return;
		}
		if (instance.GetState() != GameState.RunningSession)
		{
			instance.ShowMessageBox(GameImpl.Translate("MENU_SessionFinishedCantSave"));
			return;
		}
		CurrentRequest = new SaveRequest();
		CurrentRequest.SaveGame = new SaveGame();
		CurrentRequest.SaveGame.Type = SaveGameType.Normal;
		CurrentRequest.SaveGame.DirName = ((saveGame != null) ? saveGame.DirName : PickNewSaveGameDirName());
		CurrentRequest.Writer = instance2.SerialiseGame(out CurrentRequest.SaveGame.Data);
		CurrentRequest.ThumbnailBytes = ThumbnailBytes;
		CurrentRequest.TerrainHash = instance2.TerrainHash;
		SaveGameManager.Instance.PushSaveRequest(CurrentRequest);
		instance2.LastSavedTime = instance2.PlayTime;
		CurrentState = State.Saving;
	}

	public void OnLaunchSaveGame(SaveGame saveGame)
	{
		WantLoadSaveGame = saveGame;
		WantFadeOut = true;
	}

	public void OnOpenSaveGameFolder(SaveGame saveGame)
	{
		string arguments = (GameImpl.Instance.SaveGamePath + "/" + saveGame.DirName).Replace('/', '\\');
		Process.Start("explorer.exe", arguments);
	}

	private int FindButtonForSaveGame(SaveGame saveGame)
	{
		for (int i = 0; i < ButtonsCreated.Count; i++)
		{
			if (ButtonsCreated[i].SaveGame == saveGame)
			{
				return i;
			}
		}
		return -1;
	}

	public void OnDeleteSaveGame(SaveGame saveGame)
	{
		bool requesting = false;
		string message = GameImpl.Translate("MENU_AreYouSureDeleteSave").Replace("%1", saveGame.BuildNameString(ref requesting));
		GameImpl.Instance.ShowConfirmationBox(message, delegate
		{
			SaveGameManager.DeleteSaveGame(saveGame.DirName, delegate
			{
				OnSaveGameDeleted(saveGame);
			});
		});
	}

	private void OnSaveGameDeleted(SaveGame saveGame)
	{
		int num = SaveGames.IndexOf(saveGame);
		if (num >= 0)
		{
			SaveGames.RemoveAt(num);
			if (SaveGames.Count == 0 && Loading)
			{
				SaveGameManager.Instance.HasAnySaveGames = false;
			}
		}
		int num2 = FindButtonForSaveGame(saveGame);
		if (num2 >= 0)
		{
			UnityEngine.Object.Destroy(ButtonsCreated[num2].UnityObj);
			ButtonsCreated.RemoveAt(num2);
		}
		UpdateRemaingQuota();
	}

	private void SelectFirstButton()
	{
		Button componentInChildren = ButtonsCreated[0].UnityObj.GetComponentInChildren<Button>();
		UnityEventSystem.firstSelectedGameObject = componentInChildren.gameObject;
		UnityEventSystem.SetSelectedGameObject(componentInChildren.gameObject);
		SelectableBehaviour.StaticOnSelect(componentInChildren, wantMoveCursorOnSelect: true);
	}
}

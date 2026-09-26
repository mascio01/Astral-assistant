using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class WorkshopManager
{
	public enum State
	{
		Idle,
		CreatingItem,
		UpdatingItem,
		QueryingItem,
		SearchingStories,
		SearchingMods,
		DownloadingImage,
		DownloadingItem,
		GettingUserVote,
		SettingUserVote,
		SubscribingItem,
		UnsubscribingItem
	}

	public State CurrentState;

	public Story ProcessingStory;

	public PublishedFileId_t CurrentStoryId;

	public ulong ProcessedBytes;

	public ulong TotalBytes;

	public bool WantGetSubscribedItems = true;

	public bool WantDownloadItems = true;

	public List<StorySource> DownloadedItems = new List<StorySource>();

	public Dictionary<PublishedFileId_t, SteamUGCDetails_t> QueriedItems = new Dictionary<PublishedFileId_t, SteamUGCDetails_t>();

	public Dictionary<PublishedFileId_t, Texture2D> DownloadedImages = new Dictionary<PublishedFileId_t, Texture2D>();

	public Dictionary<PublishedFileId_t, VoteStatus> QueriedVoteStatus = new Dictionary<PublishedFileId_t, VoteStatus>();

	public Dictionary<PublishedFileId_t, SubscribeStatus> SubscribedItems = new Dictionary<PublishedFileId_t, SubscribeStatus>();

	public List<PublishedFileId_t> QueuedItemsToSubscribe = new List<PublishedFileId_t>();

	public List<PublishedFileId_t> QueuedItemsToUnsubscribe = new List<PublishedFileId_t>();

	private CallResult<CreateItemResult_t> CreateItemCallResult;

	private CallResult<SubmitItemUpdateResult_t> UpdateItemCallResult;

	private CallResult<SteamUGCQueryCompleted_t> QueryItemCallResult;

	private CallResult<SteamUGCQueryCompleted_t> QueryAllItemsCallResult;

	private CallResult<RemoteStorageSubscribePublishedFileResult_t> SubscribeItemCallResult;

	private CallResult<RemoteStorageUnsubscribePublishedFileResult_t> UnsubscribeItemCallResult;

	private CallResult<RemoteStorageDownloadUGCResult_t> DownloadImageCallResult;

	private CallResult<GetUserItemVoteResult_t> GetUserItemVoteCallResult;

	private CallResult<SetUserItemVoteResult_t> SetUserItemVoteCallResult;

	private Callback<DownloadItemResult_t> DownloadItemCallback;

	private UGCUpdateHandle_t UGCUpdateHandle;

	public static WorkshopManager Instance;

	public static string IsMod = "IsMod";

	public void Init()
	{
		Instance = this;
		if (GameImpl.Instance.SteamInitialized)
		{
			CreateItemCallResult = CallResult<CreateItemResult_t>.Create(OnCreatedItem);
			UpdateItemCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnUpdatedItem);
			QueryItemCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
			QueryAllItemsCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryAllItemsCompleted);
			SubscribeItemCallResult = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create(OnSubscribedItem);
			UnsubscribeItemCallResult = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create(OnUnsubscribedItem);
			DownloadImageCallResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(OnDownloadedImage);
			GetUserItemVoteCallResult = CallResult<GetUserItemVoteResult_t>.Create(OnGotUserItemVote);
			SetUserItemVoteCallResult = CallResult<SetUserItemVoteResult_t>.Create(OnSetUserItemVote);
			DownloadItemCallback = Callback<DownloadItemResult_t>.Create(OnDownloadedItem);
		}
	}

	public void Unload()
	{
		Instance = null;
	}

	public void CreateWorkshopItem(Story story)
	{
		CurrentState = State.CreatingItem;
		ProcessingStory = story;
		SteamAPICall_t hAPICall = SteamUGC.CreateItem((AppId_t)GameImpl.AppId, EWorkshopFileType.k_EWorkshopFileTypeFirst);
		CreateItemCallResult.Set(hAPICall);
	}

	public void OnCreatedItem(CreateItemResult_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			ProcessingStory.Settings.SteamWorkshopId = result.m_nPublishedFileId.m_PublishedFileId;
			for (int i = 0; i < ProcessingStory.Settings.Dependencies.Count; i++)
			{
				if (ProcessingStory.Settings.Dependencies[i].WorkshopId == 0L)
				{
					StorySettings storySettings = StorySettings.LoadFromFile(GameImpl.Instance.StreamingAssetsPath + "/" + ProcessingStory.Settings.Dependencies[i].Folder + "/Settings.xml");
					if (storySettings.SteamWorkshopId != 0L)
					{
						StoryId value = new StoryId
						{
							Folder = string.Empty,
							WorkshopId = storySettings.SteamWorkshopId
						};
						ProcessingStory.Settings.Dependencies[i] = value;
					}
				}
			}
			if (!ProcessingStory.Settings.SaveToFile(ProcessingStory.Path + "/Settings.xml"))
			{
				ClearState();
			}
			else if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_WorkshopLegalAgreement"), delegate
				{
					SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
				});
				ClearState();
			}
			else
			{
				UpdateWorkshopItem(ProcessingStory);
			}
		}
		else
		{
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_UploadToWorkshopFail").Replace("%1", ProcessingStory.StorySource.GetTranslatedName()).Replace("%2", result.m_eResult.ToString()));
			ClearState();
		}
	}

	public void UpdateWorkshopItem(Story story)
	{
		CurrentState = State.UpdatingItem;
		ProcessingStory = story;
		UGCUpdateHandle = SteamUGC.StartItemUpdate((AppId_t)GameImpl.AppId, (PublishedFileId_t)ProcessingStory.Settings.SteamWorkshopId);
		List<string> list = new List<string>();
		if (ProcessingStory.Settings.IsMod)
		{
			list.Add(IsMod);
		}
		SteamUGC.SetItemTitle(UGCUpdateHandle, (ProcessingStory.Settings.NativeName != null) ? ProcessingStory.Settings.NativeName : "");
		SteamUGC.SetItemDescription(UGCUpdateHandle, (ProcessingStory.Settings.NativeDescription != null) ? ProcessingStory.Settings.NativeDescription : "");
		SteamUGC.SetItemTags(UGCUpdateHandle, list);
		SteamUGC.SetItemContent(UGCUpdateHandle, ProcessingStory.Path);
		SteamUGC.SetItemPreview(UGCUpdateHandle, ProcessingStory.Path + "/CoverIcon.jpg");
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(UGCUpdateHandle, "");
		UpdateItemCallResult.Set(hAPICall);
	}

	public void OnUpdatedItem(SubmitItemUpdateResult_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_WorkshopLegalAgreement"), delegate
				{
					SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
				});
				ClearState();
				return;
			}
			ulong workshopId = ProcessingStory.Settings.SteamWorkshopId;
			for (int num = 0; num < ProcessingStory.Settings.Dependencies.Count; num++)
			{
				StoryId storyId = ProcessingStory.Settings.Dependencies[num];
				if (storyId.WorkshopId != 0L)
				{
					SteamUGC.AddDependency(new PublishedFileId_t(workshopId), new PublishedFileId_t(storyId.WorkshopId));
				}
			}
			GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_UploadToWorkshopSuccess").Replace("%1", ProcessingStory.StorySource.GetTranslatedName()), delegate
			{
				SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + workshopId);
			});
			ClearState();
		}
		else
		{
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_UploadToWorkshopFail").Replace("%1", ProcessingStory.StorySource.GetTranslatedName()).Replace("%2", result.m_eResult.ToString()));
			ClearState();
		}
	}

	public void StartDownloadingItems(bool online)
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		SubscribedItems.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			SubscribedItems[array[i]] = SubscribeStatus.Subscribed;
			uint itemState = SteamUGC.GetItemState(array[i]);
			string[] obj = new string[5] { "Subscribed Item: ", null, null, null, null };
			PublishedFileId_t publishedFileId_t = array[i];
			obj[1] = publishedFileId_t.ToString();
			obj[2] = " (state: ";
			obj[3] = itemState.ToString();
			obj[4] = ")";
			Debug.Log(string.Concat(obj));
			if ((itemState & 4) != 0)
			{
				AddDownloadedItem(array[i]);
			}
			if (online && (itemState & 8) != 0)
			{
				publishedFileId_t = array[i];
				Debug.Log("Downloading item " + publishedFileId_t.ToString());
				SteamUGC.DownloadItem(array[i], bHighPriority: true);
			}
		}
	}

	public void StartDownloadingItem(ulong rawId)
	{
		if (CurrentState == State.Idle && GameImpl.Instance.IsOnline())
		{
			PublishedFileId_t publishedFileId_t = new PublishedFileId_t(rawId);
			uint itemState = SteamUGC.GetItemState(publishedFileId_t);
			if ((itemState & 4) != 0)
			{
				AddDownloadedItem(publishedFileId_t);
			}
			if ((itemState & 8) != 0)
			{
				SteamUGC.DownloadItem(publishedFileId_t, bHighPriority: true);
				CurrentState = State.DownloadingItem;
				CurrentStoryId = publishedFileId_t;
			}
		}
	}

	private void OnDownloadedItem(DownloadItemResult_t callback)
	{
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			AddDownloadedItem(callback.m_nPublishedFileId);
		}
		if (CurrentState == State.DownloadingItem && CurrentStoryId == callback.m_nPublishedFileId)
		{
			ClearState();
		}
	}

	private void AddDownloadedItem(PublishedFileId_t id)
	{
		if (SteamUGC.GetItemInstallInfo(id, out var punSizeOnDisk, out var pchFolder, 256u, out var punTimeStamp))
		{
			string[] obj = new string[9] { "Item is installed ", null, null, null, null, null, null, null, null };
			PublishedFileId_t publishedFileId_t = id;
			obj[1] = publishedFileId_t.ToString();
			obj[2] = " (path: ";
			obj[3] = pchFolder;
			obj[4] = ", size: ";
			obj[5] = punSizeOnDisk.ToString();
			obj[6] = " bytes, timestamp: ";
			obj[7] = punTimeStamp.ToString();
			obj[8] = ")";
			Debug.Log(string.Concat(obj));
			RemoveDownloadedItem(id.m_PublishedFileId);
			StorySource item = StorySource.FromDownloadedWorkshopItem(id.m_PublishedFileId, pchFolder);
			DownloadedItems.Add(item);
		}
	}

	private void RemoveDownloadedItem(ulong workshopId)
	{
		foreach (StorySource downloadedItem in DownloadedItems)
		{
			if (downloadedItem.WorkshopId == workshopId)
			{
				DownloadedItems.Remove(downloadedItem);
				break;
			}
		}
	}

	public bool StartSearchStore(bool mods, string searchText, int page)
	{
		GameImpl instance = GameImpl.Instance;
		if (CurrentState != State.Idle || !instance.IsWorkshopEnabled())
		{
			return false;
		}
		WorkshopSortOrder workshopSortOrder = instance.Settings.WorkshopSortOrder;
		if (workshopSortOrder == WorkshopSortOrder.ActiveMods && !mods)
		{
			workshopSortOrder = WorkshopSortOrder.DownloadedMods;
		}
		if (!instance.IsOnline())
		{
			workshopSortOrder = WorkshopSortOrder.DownloadedMods;
		}
		UGCQueryHandle_t handle;
		switch (workshopSortOrder)
		{
		case WorkshopSortOrder.HighestRated:
			handle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByVote, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, (AppId_t)GameImpl.AppId, (AppId_t)GameImpl.AppId, (uint)page);
			break;
		case WorkshopSortOrder.Trending:
			handle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByTrend, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, (AppId_t)GameImpl.AppId, (AppId_t)GameImpl.AppId, (uint)page);
			SteamUGC.SetRankedByTrendDays(handle, 14u);
			break;
		case WorkshopSortOrder.Newest:
			handle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByPublicationDate, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, (AppId_t)GameImpl.AppId, (AppId_t)GameImpl.AppId, (uint)page);
			break;
		case WorkshopSortOrder.DownloadedMods:
		{
			List<StorySource> list2 = new List<StorySource>();
			SelectStoryMenu.GetListOfDownloadedStoryFolders(list2, mods, !mods, searchText);
			instance.OnSearchStoreFinished(mods, list2, list2.Count);
			return true;
		}
		case WorkshopSortOrder.ActiveMods:
		{
			List<StorySource> list = new List<StorySource>();
			foreach (StorySource activeMod in SelectStoryMenu.Instance.ActiveMods)
			{
				if (activeMod.ContainsSearchText(searchText))
				{
					list.Add(activeMod);
				}
			}
			instance.OnSearchStoreFinished(mods, list, list.Count);
			return true;
		}
		default:
			return false;
		}
		if (mods)
		{
			SteamUGC.AddRequiredTag(handle, IsMod);
		}
		else
		{
			SteamUGC.AddExcludedTag(handle, IsMod);
		}
		if (!string.IsNullOrEmpty(searchText))
		{
			SteamUGC.SetSearchText(handle, searchText);
		}
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
		QueryAllItemsCallResult.Set(hAPICall);
		CurrentState = (mods ? State.SearchingMods : State.SearchingStories);
		return true;
	}

	public void OnQueryAllItemsCompleted(SteamUGCQueryCompleted_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			List<StorySource> list = new List<StorySource>();
			for (int i = 0; i < (int)result.m_unNumResultsReturned; i++)
			{
				if (SteamUGC.GetQueryUGCResult(result.m_handle, (uint)i, out var pDetails))
				{
					QueriedItems[pDetails.m_nPublishedFileId] = pDetails;
					StorySource workshopItem = GetWorkshopItem(pDetails.m_nPublishedFileId.m_PublishedFileId);
					workshopItem.CachedIsMod = ((CurrentState == State.SearchingMods) ? StorySource.CachedBool.True : StorySource.CachedBool.False);
					list.Add(workshopItem);
				}
			}
			GameImpl.Instance.OnSearchStoreFinished(CurrentState == State.SearchingMods, list, (int)result.m_unTotalMatchingResults);
		}
		ClearState();
	}

	public void ClearState()
	{
		if (CurrentState != State.Idle)
		{
			if (CurrentState == State.CreatingItem || CurrentState == State.UpdatingItem)
			{
				GameImpl.Instance.GetMenuBehaviourByPanelName("LoadingPanel").WantPop = true;
			}
			CurrentState = State.Idle;
			ProcessingStory = null;
			UGCUpdateHandle = default(UGCUpdateHandle_t);
		}
	}

	public void Update()
	{
		if (CurrentState == State.UpdatingItem)
		{
			SteamUGC.GetItemUpdateProgress(UGCUpdateHandle, out ProcessedBytes, out TotalBytes);
		}
		if (CurrentState == State.Idle && GameImpl.Instance.SteamInitialized)
		{
			bool flag = GameImpl.Instance.IsOnline();
			if (WantGetSubscribedItems || (WantDownloadItems && flag))
			{
				WantGetSubscribedItems = false;
				if (flag)
				{
					WantDownloadItems = false;
				}
				StartDownloadingItems(flag);
			}
		}
		if (CurrentState == State.Idle && QueuedItemsToSubscribe.Count > 0 && SubscribeItem(QueuedItemsToSubscribe[0].m_PublishedFileId, canQueue: false))
		{
			QueuedItemsToSubscribe.RemoveAt(0);
		}
		if (CurrentState == State.Idle && QueuedItemsToUnsubscribe.Count > 0 && UnsubscribeItem(QueuedItemsToUnsubscribe[0].m_PublishedFileId, canQueue: false))
		{
			QueuedItemsToUnsubscribe.RemoveAt(0);
		}
	}

	public bool IsSubscribed(ulong id)
	{
		if (SubscribedItems.TryGetValue(new PublishedFileId_t(id), out var value))
		{
			return value == SubscribeStatus.Subscribed;
		}
		return false;
	}

	public bool HadSubscribeError(ulong id)
	{
		if (SubscribedItems.TryGetValue(new PublishedFileId_t(id), out var value))
		{
			return value == SubscribeStatus.Error;
		}
		return false;
	}

	public void ClearSubscribeErrors()
	{
		List<PublishedFileId_t> list = new List<PublishedFileId_t>();
		foreach (KeyValuePair<PublishedFileId_t, SubscribeStatus> subscribedItem in SubscribedItems)
		{
			if (subscribedItem.Value == SubscribeStatus.Error)
			{
				list.Add(subscribedItem.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			SubscribedItems[list[i]] = SubscribeStatus.None;
		}
	}

	public bool IsDownloaded(ulong id)
	{
		foreach (StorySource downloadedItem in DownloadedItems)
		{
			if (downloadedItem.WorkshopId == id)
			{
				return true;
			}
		}
		return false;
	}

	public StorySource GetDownloadedItem(ulong id)
	{
		foreach (StorySource downloadedItem in DownloadedItems)
		{
			if (downloadedItem.WorkshopId == id)
			{
				return downloadedItem;
			}
		}
		return null;
	}

	public StorySource GetWorkshopItem(ulong id)
	{
		foreach (StorySource downloadedItem in DownloadedItems)
		{
			if (downloadedItem.WorkshopId == id)
			{
				return downloadedItem;
			}
		}
		return StorySource.FromQueriedWorkshopItem(id);
	}

	public void ApplyAbsolutePathToDownloadedItem(StorySource storySource)
	{
		foreach (StorySource downloadedItem in DownloadedItems)
		{
			if (downloadedItem.WorkshopId == storySource.WorkshopId)
			{
				storySource.AbsolutePath = downloadedItem.AbsolutePath;
				storySource.Folder = downloadedItem.Folder;
				break;
			}
		}
	}

	public bool GetWorkshopItemDetails(PublishedFileId_t id, out SteamUGCDetails_t details)
	{
		if (QueriedItems.TryGetValue(id, out details))
		{
			return true;
		}
		if (CurrentState != State.Idle || !GameImpl.Instance.IsOnline())
		{
			return false;
		}
		CurrentState = State.QueryingItem;
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { id }, 1u));
		QueryItemCallResult.Set(hAPICall);
		return false;
	}

	public void OnQueryCompleted(SteamUGCQueryCompleted_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK && SteamUGC.GetQueryUGCResult(result.m_handle, 0u, out var pDetails))
		{
			QueriedItems[pDetails.m_nPublishedFileId] = pDetails;
			GameImpl.Instance.OnWorkshopItemQueryFinished();
		}
		ClearState();
	}

	public bool SubscribeItem(ulong id, bool canQueue)
	{
		if (!GameImpl.Instance.IsOnline())
		{
			return false;
		}
		if (CurrentState != State.Idle)
		{
			if (canQueue)
			{
				QueuedItemsToUnsubscribe.Remove(new PublishedFileId_t(id));
				QueuedItemsToSubscribe.Add(new PublishedFileId_t(id));
				return true;
			}
			return false;
		}
		SteamAPICall_t hAPICall = SteamUGC.SubscribeItem(new PublishedFileId_t(id));
		SubscribeItemCallResult.Set(hAPICall);
		CurrentState = State.SubscribingItem;
		return true;
	}

	public bool UnsubscribeItem(ulong id, bool canQueue)
	{
		if (!GameImpl.Instance.IsOnline())
		{
			return false;
		}
		if (CurrentState != State.Idle)
		{
			if (canQueue)
			{
				RemoveDownloadedItem(id);
				QueuedItemsToSubscribe.Remove(new PublishedFileId_t(id));
				QueuedItemsToUnsubscribe.Add(new PublishedFileId_t(id));
				return true;
			}
			return false;
		}
		RemoveDownloadedItem(id);
		SteamAPICall_t hAPICall = SteamUGC.UnsubscribeItem(new PublishedFileId_t(id));
		UnsubscribeItemCallResult.Set(hAPICall);
		CurrentState = State.UnsubscribingItem;
		return true;
	}

	public void OnSubscribedItem(RemoteStorageSubscribePublishedFileResult_t result, bool bIOFailure)
	{
		ClearState();
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			SubscribedItems[result.m_nPublishedFileId] = SubscribeStatus.Subscribed;
			StartDownloadingItem(result.m_nPublishedFileId.m_PublishedFileId);
		}
		else
		{
			SubscribedItems[result.m_nPublishedFileId] = SubscribeStatus.Error;
		}
	}

	public void OnUnsubscribedItem(RemoteStorageUnsubscribePublishedFileResult_t result, bool bIOFailure)
	{
		ClearState();
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			SubscribedItems[result.m_nPublishedFileId] = SubscribeStatus.None;
			RemoveDownloadedItem(result.m_nPublishedFileId.m_PublishedFileId);
		}
		else
		{
			SubscribedItems[result.m_nPublishedFileId] = SubscribeStatus.Error;
		}
	}

	public Texture2D GetUGCImage(ulong id)
	{
		Texture2D value = null;
		if (!DownloadedImages.TryGetValue(new PublishedFileId_t(id), out value))
		{
			if (CurrentState != State.Idle)
			{
				return null;
			}
			if (!GameImpl.Instance.IsOnline())
			{
				return null;
			}
			if (!GetWorkshopItemDetails(new PublishedFileId_t(id), out var details))
			{
				return null;
			}
			SteamAPICall_t hAPICall = SteamRemoteStorage.UGCDownload(details.m_hPreviewFile, 0u);
			DownloadImageCallResult.Set(hAPICall);
			CurrentState = State.DownloadingImage;
			CurrentStoryId = new PublishedFileId_t(id);
		}
		return value;
	}

	private void OnDownloadedImage(RemoteStorageDownloadUGCResult_t result, bool bIOFailure)
	{
		Texture2D texture2D = new Texture2D(2, 2);
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			byte[] array = new byte[result.m_nSizeInBytes];
			SteamRemoteStorage.UGCRead(result.m_hFile, array, result.m_nSizeInBytes, 0u, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
			texture2D.LoadImage(array);
		}
		DownloadedImages[CurrentStoryId] = texture2D;
		ClearState();
	}

	public VoteStatus GetUserVote(ulong id)
	{
		if (QueriedVoteStatus.TryGetValue(new PublishedFileId_t(id), out var value))
		{
			return value;
		}
		if (CurrentState != State.Idle || !GameImpl.Instance.IsOnline())
		{
			return VoteStatus.Unknown;
		}
		SteamAPICall_t userItemVote = SteamUGC.GetUserItemVote(new PublishedFileId_t(id));
		GetUserItemVoteCallResult.Set(userItemVote);
		CurrentState = State.GettingUserVote;
		return VoteStatus.Unknown;
	}

	private void OnGotUserItemVote(GetUserItemVoteResult_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			QueriedVoteStatus[result.m_nPublishedFileId] = (result.m_bVotedUp ? VoteStatus.Like : ((!result.m_bVotedDown) ? VoteStatus.Neutral : VoteStatus.Dislike));
		}
		else
		{
			QueriedVoteStatus[result.m_nPublishedFileId] = VoteStatus.Error;
		}
		ClearState();
	}

	public bool SetUserVote(ulong id, bool up)
	{
		if (CurrentState != State.Idle || !GameImpl.Instance.IsOnline())
		{
			return false;
		}
		SteamAPICall_t hAPICall = SteamUGC.SetUserItemVote(new PublishedFileId_t(id), up);
		SetUserItemVoteCallResult.Set(hAPICall);
		CurrentState = State.SettingUserVote;
		return true;
	}

	private void OnSetUserItemVote(SetUserItemVoteResult_t result, bool bIOFailure)
	{
		if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
		{
			QueriedVoteStatus[result.m_nPublishedFileId] = (result.m_bVoteUp ? VoteStatus.Like : VoteStatus.Dislike);
		}
		ClearState();
	}

	public void SetSortOrder(WorkshopSortOrder sortOrder)
	{
		GameImpl.Instance.Settings.WorkshopSortOrder = sortOrder;
		GameImpl.Instance.AutoSaveSettings();
	}

	public bool IsSearching()
	{
		if (CurrentState != State.SearchingMods)
		{
			return CurrentState == State.SearchingStories;
		}
		return true;
	}
}

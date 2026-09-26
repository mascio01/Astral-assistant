using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingMenu : BaseMenu
{
	private ProgressBarBehaviour UnityProgressBar;

	private GameObject UnityCancelButton;

	private GameObject UnityLoadingText;

	private GameObject UnityGeneratingText;

	private GameObject UnityReceivingNetworkSessionText;

	private GameObject UnityUploadingText;

	private TextMeshProUGUI UnityDateText;

	private TextMeshProUGUI UnityStatusText;

	public bool Cancelled;

	public static int[] MonthNameHash = new int[12]
	{
		StringUtil.JenkinsHash("MONTH_January"),
		StringUtil.JenkinsHash("MONTH_February"),
		StringUtil.JenkinsHash("MONTH_March"),
		StringUtil.JenkinsHash("MONTH_April"),
		StringUtil.JenkinsHash("MONTH_May"),
		StringUtil.JenkinsHash("MONTH_June"),
		StringUtil.JenkinsHash("MONTH_July"),
		StringUtil.JenkinsHash("MONTH_August"),
		StringUtil.JenkinsHash("MONTH_September"),
		StringUtil.JenkinsHash("MONTH_October"),
		StringUtil.JenkinsHash("MONTH_November"),
		StringUtil.JenkinsHash("MONTH_December")
	};

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		GameObject gameObject = base.transform.Find("Progress").gameObject;
		UnityProgressBar = gameObject.GetComponent<ProgressBarBehaviour>();
		UnityCancelButton = base.transform.Find("MenuLayout/CancelButton").gameObject;
		UnityLoadingText = base.transform.Find("LoadingText").gameObject;
		UnityGeneratingText = base.transform.Find("GeneratingText").gameObject;
		UnityReceivingNetworkSessionText = base.transform.Find("ReceivingNetworkSessionText").gameObject;
		UnityUploadingText = base.transform.Find("UploadingText").gameObject;
		UnityDateText = base.transform.Find("DateText").GetComponent<TextMeshProUGUI>();
		UnityStatusText = base.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
		PaperTextureAmount = 0f;
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		Cancelled = false;
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		GameImpl instance = GameImpl.Instance;
		if (instance.GetState() == GameState.Loading)
		{
			BaseResource.GetResourcesLoadedCount(out var loaded, out var total);
			UnityProgressBar.SetValue((float)loaded / (float)total);
			UnityLoadingText.SetActive(instance.UIEnglishTranslation != null);
			UnityGeneratingText.SetActive(value: false);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: false);
			UnityCancelButton.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: false);
		}
		else if (instance.GetState() == GameState.LoadingSession)
		{
			instance.GetReaderProgress(out var progress, out var total2, out var data, out var generating);
			UnityDateText.SetUnityText(data.HasValue ? BuildSessionDescriptionString(data.Value.Day, data.Value.DayOfYear) : null);
			UnityProgressBar.SetValue((float)progress / (float)total2);
			UnityLoadingText.SetActive(!generating);
			UnityGeneratingText.SetActive(generating);
			UnityDateText.gameObject.SetActive(value: true);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: false);
			UnityCancelButton.SetActive(value: false);
		}
		else if (instance.GetState() == GameState.UnityInitSession)
		{
			Session.Instance.GetUnityInitProgress(out var loaded2, out var total3);
			UnityDateText.SetUnityText(BuildSessionDescriptionString(Session.Instance.Day, (int)Session.Instance.DayOfYear));
			UnityProgressBar.SetValue((float)loaded2 / (float)total3);
			UnityLoadingText.SetActive(value: true);
			UnityGeneratingText.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: true);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: false);
			UnityCancelButton.SetActive(value: false);
		}
		else if (instance.GetState() == GameState.ReceivingNetworkSession)
		{
			OnlineParty instance2 = OnlineParty.Instance;
			int num = instance2.ReceivingChunkIndex + instance2.ReceivingTerrainChunkIndex;
			int num2 = instance2.ReceivingChunkCount + instance2.ReceivingTerrainChunkCount;
			UnityProgressBar.SetValue((num2 > 0) ? ((float)num / (float)num2) : 0f);
			UnityLoadingText.SetActive(value: false);
			UnityGeneratingText.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: false);
			UnityReceivingNetworkSessionText.SetActive(value: true);
			UnityUploadingText.SetActive(value: false);
			UnityCancelButton.SetActive(value: true);
		}
		else if (instance.GetState() == GameState.SendingNetworkSession)
		{
			OnlineParty instance3 = OnlineParty.Instance;
			int num3 = 0;
			int num4 = 0;
			foreach (PartyMember partyMember in instance3.PartyMembers)
			{
				if (!partyMember.IsLocal && !partyMember.IsBannedOrIgnored())
				{
					num3 += partyMember.AcknowledgedChunkIndex + 1;
					num3 += partyMember.AcknowledgedTerrainChunkIndex + 1;
					num4 += instance3.ChunkMessagesToSend.Count;
					num4 += partyMember.TerrainChunkMessagesToSend.Count;
				}
			}
			UnityProgressBar.SetValue((num4 > 0) ? ((float)num3 / (float)num4) : 0f);
			UnityLoadingText.SetActive(value: false);
			UnityGeneratingText.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: false);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: true);
			UnityCancelButton.SetActive(value: true);
		}
		else if (WorkshopManager.Instance.CurrentState != WorkshopManager.State.Idle)
		{
			WorkshopManager instance4 = WorkshopManager.Instance;
			UnityProgressBar.SetValue((instance4.TotalBytes != 0) ? ((float)instance4.ProcessedBytes / (float)instance4.TotalBytes) : 0f);
			UnityLoadingText.SetActive(value: false);
			UnityGeneratingText.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: false);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: true);
			UnityCancelButton.SetActive(value: false);
		}
		else if (SelectStoryMenu.Instance != null && SelectStoryMenu.Instance.LoadStoryThread != null)
		{
			int loadingProgress = Story.LoadingProgress;
			int num5 = Story.TotalFilesToLoad;
			if (num5 == 0)
			{
				num5 = 1;
			}
			UnityProgressBar.SetValue(Mathf.Clamp01((float)loadingProgress / (float)num5));
			UnityLoadingText.SetActive(value: true);
			UnityGeneratingText.SetActive(value: false);
			UnityDateText.gameObject.SetActive(value: false);
			UnityReceivingNetworkSessionText.SetActive(value: false);
			UnityUploadingText.SetActive(value: false);
			UnityCancelButton.SetActive(value: false);
		}
		string status = OnlineParty.Instance.Status;
		if (status != null)
		{
			UnityStatusText.SetUnityText(status);
		}
		UnityStatusText.gameObject.SetActive(status != null);
	}

	public static string GetSeasonString(int dayOfYear)
	{
		return GameImpl.TranslateUIOnly(MonthNameHash[dayOfYear / 7 % 12]);
	}

	public static string BuildSessionDescriptionString(int day, int dayOfYear)
	{
		return GameImpl.TranslateUIOnly("MENU_Day").Replace("%1", day.ToString()).Replace("%2", GetSeasonString(dayOfYear));
	}

	public void OnCancel()
	{
		OnlineParty instance = OnlineParty.Instance;
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		instance.LeaveLobby();
		if (GameImpl.Instance.GetState() == GameState.SendingNetworkSession)
		{
			List<PlayerID> playerIDs = new List<PlayerID>();
			List<string> playerNames = new List<string>();
			GameImpl.Instance.LoadNetworkGame(instance.SendingChunksSessionId, instance.SessionToLoadStorySources, instance.SessionToLoadBytes, instance.SessionToLoadBytesLength, instance.SessionToLoadTerrainHash, playerIDs, playerNames);
			instance.SendingChunksSessionId = 0;
			instance.ChunkMessagesToSend.Clear();
			instance.CleanUpSessionToLoad();
		}
		else
		{
			SetFinished();
			Cancelled = true;
		}
	}
}

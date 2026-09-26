using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModButtonBehaviour : MonoBehaviour
{
	public StorySource StorySource;

	public Button UnityButton;

	private Button UnityDownloadButton;

	private Button UnityDeleteButton;

	private Toggle UnityCheckbox;

	private RawImage UnityIcon;

	private RawImage UnityDownloadProgress;

	private RawImage UnityDownloadProgressFill;

	private RawImage UnityLike;

	private RawImage UnityDislike;

	private TextMeshProUGUI UnityNameText;

	private TextMeshProUGUI UnityDescriptionText;

	private TextMeshProUGUI UnityAuthorText;

	private TextMeshProUGUI UnityDownloadText;

	private TextMeshProUGUI UnityDeleteText;

	private TextMeshProUGUI UnityDownloadingText;

	private GameObject UnityDetailsPanel;

	private GameObject UnityStarsPanel;

	private GameObject UnityLikePanel;

	private Resource<Texture2D> Icon;

	private bool WantLaunch;

	private static int MENU_By = StringUtil.JenkinsHash("MENU_By");

	private static StringBuilder sb = new StringBuilder();

	public bool Checked => UnityCheckbox.isOn;

	public void Awake()
	{
		UnityButton = base.gameObject.FindChild("MainButton").GetComponent<Button>();
		UnityDownloadButton = base.gameObject.FindChild("Options/DownloadButton").GetComponent<Button>();
		UnityDeleteButton = base.gameObject.FindChild("Options/DeleteButton").GetComponent<Button>();
		UnityCheckbox = base.gameObject.FindChild("Options/Toggle").GetComponent<Toggle>();
		UnityIcon = base.gameObject.FindChild("MainButton/Image").GetComponent<RawImage>();
		UnityDownloadProgress = base.gameObject.FindChild("Options/DownloadProgress").GetComponent<RawImage>();
		UnityDownloadProgressFill = base.gameObject.FindChild("Options/DownloadProgress/Fill").GetComponent<RawImage>();
		UnityLike = base.gameObject.FindChild("Options/LikePanel/LikeButton").GetComponent<RawImage>();
		UnityDislike = base.gameObject.FindChild("Options/LikePanel/DislikeButton").GetComponent<RawImage>();
		UnityNameText = base.gameObject.FindChild("MainButton/Panel/NameText").GetComponent<TextMeshProUGUI>();
		UnityDescriptionText = base.gameObject.FindChild("MainButton/Panel/DescriptionText").GetComponent<TextMeshProUGUI>();
		UnityAuthorText = base.gameObject.FindChild("MainButton/Panel/DetailsPanel/AuthorText").GetComponent<TextMeshProUGUI>();
		UnityDownloadText = base.gameObject.FindChild("Options/DownloadText").GetComponent<TextMeshProUGUI>();
		UnityDeleteText = base.gameObject.FindChild("Options/DeleteText").GetComponent<TextMeshProUGUI>();
		UnityDownloadingText = base.gameObject.FindChild("Options/DownloadingText").GetComponent<TextMeshProUGUI>();
		UnityDetailsPanel = base.gameObject.FindChild("MainButton/Panel/DetailsPanel");
		UnityStarsPanel = base.gameObject.FindChild("MainButton/Panel/DetailsPanel/StarsPanel");
		UnityLikePanel = base.gameObject.FindChild("Options/LikePanel");
	}

	public void OnDestroy()
	{
		if (Icon != null)
		{
			Icon.UnloadResource();
			Icon = null;
		}
	}

	public void Populate(StorySource storySource)
	{
		if (!storySource.IsValid())
		{
			WorkshopManager.Instance.ApplyAbsolutePathToDownloadedItem(storySource);
		}
		if (StorySource == null || !StorySource.Equals(storySource))
		{
			if (Icon != null)
			{
				Icon.UnloadResource();
				Icon = null;
			}
			UnityIcon.texture = null;
			StorySource = storySource;
			UnityCheckbox.isOn = SelectStoryMenu.Instance.IsActiveMod(StorySource);
			if (!string.IsNullOrEmpty(StorySource.AbsolutePath))
			{
				string path = StorySource.AbsolutePath + "/CoverIcon.jpg";
				if (File.Exists(path))
				{
					Icon = new Resource<Texture2D>(path);
				}
			}
		}
		string translatedName = storySource.GetTranslatedName();
		string translatedDescription = storySource.GetTranslatedDescription();
		UnityNameText.SetUnityTextIfDifferent(translatedName);
		UnityDescriptionText.SetUnityTextIfDifferent(translatedDescription);
		UnityDetailsPanel.SetActive(storySource.WorkshopId != 0);
		if (UnityDetailsPanel.activeSelf)
		{
			string author = storySource.GetAuthor();
			string dateString = storySource.GetDateString();
			int numVotes;
			int num = Mathf.CeilToInt(storySource.GetScore(out numVotes) * 5f);
			sb.Length = 0;
			sb.Append(GameImpl.TranslateUIOnly(MENU_By));
			sb.Append(' ');
			sb.Append(author);
			sb.Append(' ');
			sb.Append('-');
			sb.Append(' ');
			sb.Append(dateString);
			UnityAuthorText.SetUnityTextIfDifferent(sb);
			UnityStarsPanel.gameObject.SetActive(numVotes >= 30);
			if (UnityStarsPanel.gameObject.activeSelf)
			{
				for (int i = 0; i < UnityStarsPanel.transform.childCount; i++)
				{
					UnityStarsPanel.transform.GetChild(i).GetComponent<RawImage>().color = ((i < num) ? Color.yellow : Color.white);
				}
			}
		}
		Vector2 uIObjectCentreOnScreen = HudBehaviour.Instance.GetUIObjectCentreOnScreen(UnityIcon.gameObject);
		bool flag = uIObjectCentreOnScreen.y >= -128f && uIObjectCentreOnScreen.y <= 1208f;
		if (UnityIcon.texture == null)
		{
			if (!string.IsNullOrEmpty(StorySource.AbsolutePath))
			{
				if (Icon != null && Icon.IsFinishedLoading() && Icon.GetAsset() != null)
				{
					UnityIcon.texture = Icon.GetAsset();
				}
			}
			else if (flag)
			{
				UnityIcon.texture = WorkshopManager.Instance.GetUGCImage(StorySource.WorkshopId);
			}
		}
		UnityDownloadButton.gameObject.SetActive(storySource.WorkshopId != 0L && !WorkshopManager.Instance.IsSubscribed(storySource.WorkshopId) && GameImpl.Instance.IsOnline());
		UnityDeleteButton.gameObject.SetActive(storySource.WorkshopId != 0L && WorkshopManager.Instance.IsSubscribed(storySource.WorkshopId) && GameImpl.Instance.IsOnline());
		UnityDownloadText.gameObject.SetActive(UnityDownloadButton.gameObject.activeSelf);
		UnityDeleteText.gameObject.SetActive(UnityDeleteButton.gameObject.activeSelf);
		bool active = false;
		if (StorySource.WorkshopId != 0L && (SteamUGC.GetItemState(new PublishedFileId_t(StorySource.WorkshopId)) & 8) != 0 && SteamUGC.GetItemDownloadInfo(new PublishedFileId_t(StorySource.WorkshopId), out var punBytesDownloaded, out var punBytesTotal))
		{
			UnityDownloadProgressFill.rectTransform.sizeDelta = new Vector2(UnityDownloadProgress.rectTransform.rect.width * (float)punBytesDownloaded / (float)punBytesTotal, UnityDownloadProgress.rectTransform.rect.height);
			active = true;
		}
		UnityDownloadProgress.gameObject.SetActive(active);
		UnityDownloadingText.gameObject.SetActive(active);
		UnityCheckbox.gameObject.SetActive(StorySource.GetCachedIsMod() && SelectStoryMenu.Instance.FinishedAction != SelectStoryMenu.Action.Editor);
		UnityCheckbox.interactable = SelectStoryMenu.Instance.FinishedAction != SelectStoryMenu.Action.SelectMods || !GameImpl.Instance.IsInCurrentStorySources(StorySource);
		UnityDeleteButton.interactable = UnityCheckbox.interactable;
		UnityLikePanel.gameObject.SetActive(StorySource.WorkshopId != 0L && GameImpl.Instance.IsOnline());
		if (flag)
		{
			VoteStatus userVote = WorkshopManager.Instance.GetUserVote(StorySource.WorkshopId);
			UnityLike.color = ((userVote == VoteStatus.Like) ? Color.yellow : Color.white);
			UnityDislike.color = ((userVote == VoteStatus.Dislike) ? Color.yellow : Color.white);
			UnityLike.transform.localScale = Vector3.one * ((userVote == VoteStatus.Like) ? 1.2f : 1f);
			UnityDislike.transform.localScale = Vector3.one * ((userVote == VoteStatus.Dislike) ? 1.2f : 1f);
		}
		if (WantLaunch)
		{
			if (WorkshopManager.Instance.IsDownloaded(StorySource.WorkshopId))
			{
				OnClicked();
				WantLaunch = false;
			}
			else if (WorkshopManager.Instance.IsSubscribed(StorySource.WorkshopId))
			{
				WorkshopManager.Instance.StartDownloadingItem(StorySource.WorkshopId);
			}
			else if (WorkshopManager.Instance.HadSubscribeError(StorySource.WorkshopId))
			{
				GameImpl.Instance.ShowMessageBox(GameImpl.TranslateUIOnly("MENU_FailedToLoadStoryFromWorkshop").Replace("%1", StorySource.GetTranslatedName()));
				WantLaunch = false;
				UnityCheckbox.isOn = false;
			}
		}
	}

	private Button GetUnitySubscribeButton()
	{
		if (!UnityDownloadButton.gameObject.activeSelf)
		{
			if (!UnityDeleteButton.gameObject.activeSelf)
			{
				return null;
			}
			return UnityDeleteButton;
		}
		return UnityDownloadButton;
	}

	public void SetupNavigation(List<KeyValuePair<Selectable, ModButtonBehaviour>> selectables, int i)
	{
		Button unitySubscribeButton = GetUnitySubscribeButton();
		Navigation navigation = UnityButton.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnRight = (UnityCheckbox.gameObject.activeSelf ? ((Selectable)UnityCheckbox) : ((Selectable)unitySubscribeButton));
		navigation.selectOnUp = ((i > 0) ? selectables[i - 1].Key : null);
		navigation.selectOnDown = ((i < selectables.Count - 1) ? selectables[i + 1].Key : null);
		UnityButton.navigation = navigation;
		if (UnityCheckbox.gameObject.activeSelf)
		{
			Navigation navigation2 = UnityCheckbox.navigation;
			navigation2.mode = Navigation.Mode.Explicit;
			navigation2.selectOnLeft = UnityButton;
			navigation2.selectOnUp = ((i <= 0) ? null : ((!(selectables[i - 1].Value != null)) ? selectables[i - 1].Key : (selectables[i - 1].Value.UnityLikePanel.activeSelf ? ((Selectable)selectables[i - 1].Value.UnityLike.GetComponent<Button>()) : ((Selectable)selectables[i - 1].Value.UnityCheckbox))));
			navigation2.selectOnDown = ((unitySubscribeButton != null) ? unitySubscribeButton : ((i >= selectables.Count - 1) ? null : ((selectables[i + 1].Value != null) ? selectables[i + 1].Value.UnityCheckbox : selectables[i + 1].Key)));
			UnityCheckbox.navigation = navigation2;
		}
		else if (unitySubscribeButton != null && unitySubscribeButton.gameObject.activeSelf)
		{
			Navigation navigation3 = UnityDownloadButton.navigation;
			navigation3.mode = Navigation.Mode.Explicit;
			navigation3.selectOnLeft = UnityButton;
			navigation3.selectOnUp = ((i <= 0) ? null : ((selectables[i - 1].Value != null && selectables[i - 1].Value.UnityLikePanel.activeSelf) ? selectables[i - 1].Value.UnityLike.GetComponent<Button>() : selectables[i - 1].Key));
			navigation3.selectOnDown = UnityLike.GetComponent<Button>();
			UnityDownloadButton.navigation = navigation3;
		}
	}

	public void OnClicked()
	{
		if (!StorySource.IsValid() && WorkshopManager.Instance.IsDownloaded(StorySource.WorkshopId))
		{
			WorkshopManager.Instance.ApplyAbsolutePathToDownloadedItem(StorySource);
		}
		if (StorySource.GetCachedIsMod() && SelectStoryMenu.Instance.FinishedAction != SelectStoryMenu.Action.Editor)
		{
			if (WantLaunch)
			{
				if (UnityCheckbox.isOn)
				{
					SelectStoryMenu.Instance.AddActiveMod(StorySource);
				}
			}
			else if (UnityCheckbox.interactable)
			{
				UnityCheckbox.isOn = !UnityCheckbox.isOn;
			}
			else
			{
				GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_CantDeselectMods"));
			}
		}
		else if (StorySource.IsValid())
		{
			SelectStoryMenu.Instance.OnSelectStory(StorySource, acceptedWarning: false);
		}
		else if (WorkshopManager.Instance.SubscribeItem(StorySource.WorkshopId, canQueue: true))
		{
			WantLaunch = true;
		}
	}

	public void OnDownload()
	{
		WorkshopManager.Instance.SubscribeItem(StorySource.WorkshopId, canQueue: true);
	}

	public void OnDelete()
	{
		if (StorySource.GetCachedIsMod() && SelectStoryMenu.Instance.FinishedAction != SelectStoryMenu.Action.Editor)
		{
			UnityCheckbox.isOn = false;
			SelectStoryMenu.Instance.RemoveActiveMod(StorySource);
		}
		if (WorkshopManager.Instance.UnsubscribeItem(StorySource.WorkshopId, canQueue: true))
		{
			StorySource.AbsolutePath = string.Empty;
			StorySource.Folder = string.Empty;
		}
	}

	public void OnToggle(bool on)
	{
		if (on)
		{
			if (!StorySource.IsValid() && WorkshopManager.Instance.IsDownloaded(StorySource.WorkshopId))
			{
				StorySource = WorkshopManager.Instance.GetWorkshopItem(StorySource.WorkshopId);
			}
			if (!StorySource.IsValid() || (StorySource.WorkshopId != 0L && !WorkshopManager.Instance.IsDownloaded(StorySource.WorkshopId)))
			{
				if (WorkshopManager.Instance.SubscribeItem(StorySource.WorkshopId, canQueue: true))
				{
					WantLaunch = true;
				}
				else
				{
					UnityCheckbox.isOn = false;
				}
			}
			else
			{
				SelectStoryMenu.Instance.AddActiveMod(StorySource);
			}
		}
		else
		{
			SelectStoryMenu.Instance.RemoveActiveMod(StorySource);
		}
	}

	public void OnLike()
	{
		WorkshopManager.Instance.SetUserVote(StorySource.WorkshopId, up: true);
	}

	public void OnDislike()
	{
		WorkshopManager.Instance.SetUserVote(StorySource.WorkshopId, up: false);
	}
}

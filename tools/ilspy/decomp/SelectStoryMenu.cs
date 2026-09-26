using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectStoryMenu : BaseMenu
{
	public enum Action
	{
		StartNewGame,
		Editor,
		SelectMods
	}

	public enum Tab
	{
		Stories,
		Mods,
		Count
	}

	public enum Source
	{
		Local,
		Workshop,
		Count
	}

	private struct FolderOrder : IComparable<FolderOrder>
	{
		public StorySource StorySource;

		public int Order;

		public int CompareTo(FolderOrder other)
		{
			if (Order < other.Order)
			{
				return -1;
			}
			if (Order > other.Order)
			{
				return 1;
			}
			return StorySource.GetTranslatedName().CompareTo(other.StorySource.GetTranslatedName());
		}
	}

	public static SelectStoryMenu Instance;

	public Action FinishedAction;

	public Tab CurrentTab;

	private TabBehaviour UnityStoriesTab;

	private TabBehaviour UnityModsTab;

	private TextMeshProUGUI UnityModsTabText;

	private TextMeshProUGUI UnityTabLeftPrompt;

	private TextMeshProUGUI UnityTabRightPrompt;

	private GameObject UnityCreateNewButton;

	private GameObject UnityStoryListFrame;

	private GameObject UnityStoryList;

	private GameObject UnityCommunityModsTitle;

	private GameObject UnityShowCommunityContentButton;

	private GameObject UnityMoreButton;

	private GameObject UnityNoMods;

	private GameObject UnityPagesPanel;

	private Button UnityBackButton;

	private List<ModButtonBehaviour> UnityButtonsCreated = new List<ModButtonBehaviour>();

	private bool WantKick;

	private List<StorySource>[] StorySources = new List<StorySource>[2];

	private List<StorySource>[] ModSources = new List<StorySource>[2];

	private bool WantHideCommunityStories;

	private bool HasClickedShowCommunityStories;

	private bool WantSearchStore;

	private GameObject WantScrollToButton;

	private int WantScrollToButtonCountdown;

	private string SearchText = string.Empty;

	private int Page;

	private int PageCountSoFar;

	private int ResultsSoFar;

	private int TotalResults;

	private StorySource WantSelectStorySource;

	private List<StorySource> StorySourcesToSelect = new List<StorySource>();

	public List<StorySource> ActiveMods = new List<StorySource>();

	public Thread LoadStoryThread;

	public ActionMenu ActionMenu = new ActionMenu();

	public static float StoryTooltipXOffset = -180f;

	private static string DropDownStr = "Dropdown";

	private static string SearchTextStr = "SearchText";

	private List<KeyValuePair<Selectable, ModButtonBehaviour>> Selectables = new List<KeyValuePair<Selectable, ModButtonBehaviour>>();

	public static int MENU_Mods = StringUtil.JenkinsHash("MENU_Mods");

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
		UnityStoriesTab = base.gameObject.FindChild("Panel/TabStories").GetComponent<TabBehaviour>();
		UnityModsTab = base.gameObject.FindChild("Panel/TabMods").GetComponent<TabBehaviour>();
		UnityModsTabText = base.gameObject.FindChild("Panel/TabMods/Background/Text").GetComponent<TextMeshProUGUI>();
		UnityTabLeftPrompt = base.transform.Find("Panel/TabLeftPrompt").GetComponent<TextMeshProUGUI>();
		UnityTabRightPrompt = base.transform.Find("Panel/TabRightPrompt").GetComponent<TextMeshProUGUI>();
		UnityCreateNewButton = base.gameObject.FindChild("MenuLayout/CreateNewButton");
		UnityBackButton = base.gameObject.FindChild("MenuLayout/BackButton").GetComponent<Button>();
		UnityStoryListFrame = base.gameObject.FindChild("StoryList");
		UnityStoryList = base.gameObject.FindChild("StoryList/Viewport/Content");
		UnityStoryList.DeleteAllChildren();
	}

	public static void GetListOfLocalStoryFolders(List<StorySource> storySources, bool skipBaseStory)
	{
		List<FolderOrder> list = new List<FolderOrder>();
		string streamingAssetsPath = GameImpl.Instance.StreamingAssetsPath;
		if (Directory.Exists(streamingAssetsPath))
		{
			bool flag = LanguageMenu.IsTranslationComplete(GameImpl.Instance.Settings.Language);
			string[] directories = Directory.GetDirectories(streamingAssetsPath);
			foreach (string path in directories)
			{
				FolderOrder item = new FolderOrder
				{
					StorySource = StorySource.FromFolder(Path.GetFileName(path))
				};
				switch (item.StorySource.Folder)
				{
				case "BaseStory":
					item.Order = -2;
					break;
				case "Common":
					item.Order = -1;
					break;
				case "BlankStory":
					item.Order = -1;
					break;
				case "MainStory":
					item.Order = ((!flag) ? 1 : 0);
					break;
				case "Sandbox":
					item.Order = (flag ? 1 : 0);
					break;
				case "ExampleStory":
					item.Order = 2;
					break;
				default:
					item.Order = 3;
					break;
				case "UI":
					continue;
				}
				if (!skipBaseStory || (!(item.StorySource.Folder == "BaseStory") && !(item.StorySource.Folder == "Common")))
				{
					list.Add(item);
				}
			}
		}
		list.Sort();
		for (int j = 0; j < list.Count; j++)
		{
			storySources.Add(list[j].StorySource);
		}
	}

	public static void GetListOfDownloadedStoryFolders(List<StorySource> storySources, bool mods, bool stories, string searchText)
	{
		List<FolderOrder> list = new List<FolderOrder>();
		foreach (StorySource downloadedItem in WorkshopManager.Instance.DownloadedItems)
		{
			if (!string.IsNullOrEmpty(downloadedItem.AbsolutePath) && (mods || !downloadedItem.GetCachedIsMod()) && (stories || downloadedItem.GetCachedIsMod()) && downloadedItem.ContainsSearchText(searchText))
			{
				list.Add(new FolderOrder
				{
					StorySource = downloadedItem,
					Order = 4
				});
			}
		}
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			storySources.Add(list[i].StorySource);
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		SetMenuCaption((FinishedAction == Action.StartNewGame) ? "MENU_NewGameCaption" : ((FinishedAction == Action.SelectMods) ? "MENU_SelectMods" : "MENU_EditorCaption"));
		SetTab((FinishedAction == Action.SelectMods) ? Tab.Mods : Tab.Stories);
		UnityStoriesTab.gameObject.SetActive(FinishedAction != Action.SelectMods);
		UnityCreateNewButton.SetActive(FinishedAction == Action.Editor);
		for (int i = 0; i < 2; i++)
		{
			StorySources[i] = new List<StorySource>();
			ModSources[i] = new List<StorySource>();
		}
		GetListOfLocalStoryFolders(StorySources[0], FinishedAction != Action.Editor);
		for (int j = 0; j < 2; j++)
		{
			for (int k = 0; k < StorySources[j].Count; k++)
			{
				if (StorySources[j][k].GetCachedIsMod())
				{
					ModSources[j].Add(StorySources[j][k]);
					StorySources[j].RemoveAt(k);
					k--;
				}
			}
		}
		WantHideCommunityStories = false;
		if (FinishedAction == Action.StartNewGame && !SaveGameManager.Instance.HasAnySaveGames && !HasClickedShowCommunityStories)
		{
			WantHideCommunityStories = true;
		}
		Populate();
		OnActiveModsChanged();
	}

	private void UnityDeleteContent()
	{
		Page = 1;
		PageCountSoFar = 1;
		ResultsSoFar = 0;
		TotalResults = 0;
		for (int i = 0; i < UnityButtonsCreated.Count; i++)
		{
			UnityEngine.Object.Destroy(UnityButtonsCreated[i].gameObject);
		}
		UnityButtonsCreated.Clear();
		if (UnityCommunityModsTitle != null)
		{
			UnityEngine.Object.Destroy(UnityCommunityModsTitle);
		}
		UnityCommunityModsTitle = null;
		UnityPagesPanel = null;
		if (UnityShowCommunityContentButton != null)
		{
			UnityEngine.Object.Destroy(UnityShowCommunityContentButton);
		}
		UnityShowCommunityContentButton = null;
		if (UnityMoreButton != null)
		{
			UnityEngine.Object.Destroy(UnityMoreButton);
		}
		UnityMoreButton = null;
		if (UnityNoMods != null)
		{
			UnityEngine.Object.Destroy(UnityNoMods);
		}
		UnityNoMods = null;
	}

	public override void OnDeactivate(bool popped)
	{
		if (popped || LoadStoryThread == null)
		{
			UnityDeleteContent();
			WantSearchStore = false;
			WantKick = false;
			WantSelectStorySource = null;
			StorySourcesToSelect.Clear();
		}
		base.OnDeactivate(popped);
	}

	public override void OnSearchStoreFinished(bool mods, List<StorySource> results, int total)
	{
		List<StorySource>[] array = (mods ? ModSources : StorySources);
		array[1].Clear();
		foreach (StorySource result in results)
		{
			array[1].Add(result);
		}
		if (Page == 1)
		{
			ResultsSoFar = results.Count;
		}
		else
		{
			ResultsSoFar += results.Count;
		}
		TotalResults = total;
		PageCountSoFar = Math.Max(Page, PageCountSoFar);
		if (UnityPagesPanel != null)
		{
			UnityPagesPanel.SetActive(PageCountSoFar > 1);
			for (int i = 1; i <= PageCountSoFar; i++)
			{
				if (i > UnityPagesPanel.transform.childCount)
				{
					int localPage = i;
					Button component = UnityEngine.Object.Instantiate((GameObject)BaseMenu.PageButton, UnityPagesPanel.transform).GetComponent<Button>();
					component.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetUnityText(i.ToString());
					component.onClick.AddListener(delegate
					{
						GoToPage(localPage);
					});
				}
				GameObject obj = UnityPagesPanel.transform.GetChild(i - 1).gameObject;
				obj.GetComponent<RawImage>().enabled = Page == i;
				obj.GetComponent<Outline>().enabled = Page == i;
			}
			for (int num = PageCountSoFar + 1; num < UnityPagesPanel.transform.childCount; num++)
			{
				UnityEngine.Object.Destroy(UnityPagesPanel.transform.GetChild(num).gameObject);
			}
		}
		int num2 = 0;
		for (int num3 = 0; num3 < UnityButtonsCreated.Count; num3++)
		{
			if (UnityButtonsCreated[num3].StorySource.WorkshopId != 0L)
			{
				num2++;
				if (num2 > 3)
				{
					UnityEngine.Object.Destroy(UnityButtonsCreated[num3].gameObject);
					UnityButtonsCreated.RemoveAt(num3);
					num3--;
				}
			}
		}
	}

	public static bool IsInStorySources(List<StorySource> existingSources, StorySource storySource)
	{
		foreach (StorySource existingSource in existingSources)
		{
			if (existingSource.Equals(storySource))
			{
				return true;
			}
		}
		return false;
	}

	public static StorySource FindStorySourceForFolder(List<StorySource> storySources, string folderName)
	{
		foreach (StorySource storySource in storySources)
		{
			if (storySource.Folder == folderName)
			{
				return storySource;
			}
		}
		return null;
	}

	private void Populate()
	{
		GameImpl instance = GameImpl.Instance;
		bool flag = instance.IsWorkshopEnabled();
		if (FinishedAction == Action.Editor)
		{
			UnityModsTab.gameObject.SetActive(value: true);
		}
		else
		{
			UnityModsTab.gameObject.SetActive(flag && !WantHideCommunityStories);
		}
		RectTransform rectTransform = (RectTransform)UnityStoryList.transform;
		List<StorySource>[] array = ((CurrentTab == Tab.Mods) ? ModSources : StorySources);
		bool flag2 = ResultsSoFar < TotalResults && rectTransform.anchoredPosition.y + HudBehaviour.Instance.HudPanelRectTransform.rect.height * 2f >= rectTransform.rect.height;
		bool flag3 = TotalResults == 0 && !WantSearchStore && !WorkshopManager.Instance.IsSearching() && instance.SteamInitialized && instance.IsOnline() && instance.IsWorkshopEnabled() && instance.Settings.WorkshopSortOrder != WorkshopSortOrder.ActiveMods && instance.Settings.WorkshopSortOrder != WorkshopSortOrder.DownloadedMods && string.IsNullOrEmpty(SearchText);
		Selectables.Clear();
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (num2 < array.Length)
			{
				if (num2 == 1 && flag)
				{
					if (WantHideCommunityStories)
					{
						if (UnityShowCommunityContentButton == null && CurrentTab == Tab.Stories)
						{
							UnityShowCommunityContentButton = UnityEngine.Object.Instantiate((GameObject)BaseMenu.ShowCommunityModsButton, UnityStoryList.transform);
							UnityShowCommunityContentButton.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
							{
								WantHideCommunityStories = false;
								HasClickedShowCommunityStories = true;
								if (UnityShowCommunityContentButton != null)
								{
									UnityEngine.Object.Destroy(UnityShowCommunityContentButton);
								}
								UnityShowCommunityContentButton = null;
								Page = 1;
								SearchText = string.Empty;
								WantSearchStore = true;
								WantKick = true;
							});
						}
						if (UnityShowCommunityContentButton != null && UnityShowCommunityContentButton.activeSelf)
						{
							Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(UnityShowCommunityContentButton.transform.GetChild(1).GetComponent<Button>(), null));
						}
					}
					else
					{
						if (UnityCommunityModsTitle == null)
						{
							UnityCommunityModsTitle = UnityEngine.Object.Instantiate((GameObject)BaseMenu.CommunityModsTitle, UnityStoryList.transform);
							UnityPagesPanel = UnityCommunityModsTitle.FindChild("PagesPanel");
							UnityPagesPanel.SetActive(value: false);
							UnityPagesPanel.DeleteAllChildrenImmediately();
							TMP_Dropdown component = UnityCommunityModsTitle.FindChild(DropDownStr).GetComponent<TMP_Dropdown>();
							TMP_InputField component2 = UnityCommunityModsTitle.FindChild(SearchTextStr).GetComponent<TMP_InputField>();
							List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
							for (int num3 = 0; num3 < 5; num3++)
							{
								if (num3 != 4 || CurrentTab == Tab.Mods)
								{
									WorkshopSortOrder workshopSortOrder = (WorkshopSortOrder)num3;
									list.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_" + workshopSortOrder)));
								}
							}
							component.interactable = instance.IsOnline();
							component.options = list;
							component.value = (instance.IsOnline() ? Math.Min((int)instance.Settings.WorkshopSortOrder, list.Count - 1) : 3);
							component.onValueChanged.AddListener(delegate(int v)
							{
								WorkshopManager.Instance.SetSortOrder((WorkshopSortOrder)v);
								Page = 1;
								PageCountSoFar = 1;
								WantSearchStore = true;
								WantKick = true;
							});
							component2.interactable = instance.IsOnline();
							component2.onValueChanged.AddListener(delegate(string v)
							{
								SearchText = v;
								Page = 1;
								PageCountSoFar = 1;
								WantSearchStore = true;
								WantKick = true;
							});
							WantKick = true;
						}
						if (UnityCommunityModsTitle != null)
						{
							TMP_Dropdown component3 = UnityCommunityModsTitle.FindChild(DropDownStr).GetComponent<TMP_Dropdown>();
							if (component3.interactable)
							{
								Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(component3, null));
							}
							TMP_InputField component4 = UnityCommunityModsTitle.FindChild(SearchTextStr).GetComponent<TMP_InputField>();
							if (component4.interactable)
							{
								Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(component4, null));
							}
							if (UnityPagesPanel.activeSelf && UnityPagesPanel.transform.childCount > 0)
							{
								Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(UnityPagesPanel.transform.GetChild(MathUtil.Clamp(Page - 1, 0, UnityPagesPanel.transform.childCount - 1)).GetComponent<Button>(), null));
							}
						}
					}
				}
				if (num2 != 1 || CurrentTab != Tab.Stories || !WantHideCommunityStories)
				{
					foreach (StorySource item in array[num2])
					{
						if (num < UnityButtonsCreated.Count)
						{
							ModButtonBehaviour component5 = UnityButtonsCreated[num].GetComponent<ModButtonBehaviour>();
							component5.Populate(item);
							Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(component5.UnityButton, component5));
							num++;
							continue;
						}
						if (rectTransform.anchoredPosition.y + HudBehaviour.Instance.HudPanelRectTransform.rect.height * 2f >= rectTransform.rect.height)
						{
							ModButtonBehaviour component5 = UnityEngine.Object.Instantiate((GameObject)BaseMenu.ModButton, UnityStoryList.transform).GetComponent<ModButtonBehaviour>();
							if (UnityButtonsCreated.Count == 0)
							{
								UnityEventSystem.firstSelectedGameObject = component5.gameObject;
							}
							UnityButtonsCreated.Add(component5);
							component5.Populate(item);
							Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(component5.UnityButton, component5));
							num++;
							WantKick = true;
						}
						goto end_IL_059c;
					}
				}
				num2++;
				continue;
			}
			if (flag2 && UnityMoreButton == null && !WorkshopManager.Instance.IsSearching())
			{
				UnityMoreButton = UnityEngine.Object.Instantiate((GameObject)BaseMenu.NextPageButton, UnityStoryList.transform);
				UnityMoreButton.transform.SetAsLastSibling();
				UnityMoreButton.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
				{
					if (!WantSearchStore && !WorkshopManager.Instance.IsSearching())
					{
						GoToPage(Page + 1);
						WantScrollToButton = UnityCommunityModsTitle;
						WantScrollToButtonCountdown = 2;
					}
				});
				WantKick = true;
			}
			if (UnityMoreButton != null && UnityMoreButton.gameObject.activeSelf)
			{
				Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(UnityMoreButton.transform.GetChild(0).GetComponent<Button>(), null));
			}
			if (flag3 && UnityNoMods == null)
			{
				UnityNoMods = UnityEngine.Object.Instantiate((GameObject)BaseMenu.NoMods, UnityStoryList.transform);
				UnityNoMods.transform.SetAsLastSibling();
				WantKick = true;
			}
			Selectables.Add(new KeyValuePair<Selectable, ModButtonBehaviour>(UnityBackButton, null));
			break;
			continue;
			end_IL_059c:
			break;
		}
		while (num < UnityButtonsCreated.Count)
		{
			UnityEngine.Object.Destroy(UnityButtonsCreated[num].gameObject);
			UnityButtonsCreated.RemoveAt(num);
		}
		if (UnityMoreButton != null && !flag2)
		{
			UnityEngine.Object.Destroy(UnityMoreButton);
			UnityMoreButton = null;
		}
		if (UnityMoreButton != null)
		{
			UnityMoreButton.transform.SetAsLastSibling();
		}
		if (UnityNoMods != null && !flag3)
		{
			UnityEngine.Object.Destroy(UnityNoMods);
			UnityNoMods = null;
		}
		if (UnityNoMods != null)
		{
			UnityNoMods.transform.SetAsLastSibling();
		}
		for (int num4 = 0; num4 < Selectables.Count; num4++)
		{
			if (Selectables[num4].Value != null)
			{
				Selectables[num4].Value.SetupNavigation(Selectables, num4);
			}
		}
		Selectables.Clear();
	}

	public void GoToPage(int page)
	{
		Page = page;
		WantSearchStore = true;
		WantKick = true;
	}

	public void ScrollToButton(GameObject unityButtonObj)
	{
		if (unityButtonObj != null)
		{
			UnityStoryList.transform.localPosition = new Vector3(0f, 0f - unityButtonObj.transform.localPosition.y - ((RectTransform)UnityStoryListFrame.transform).rect.height * 0.5f + ((RectTransform)unityButtonObj.transform).rect.height * 0.5f, 0f);
		}
	}

	public void OnActiveModsChanged()
	{
		UnityModsTabText.SetUnityText(GameImpl.Translate(MENU_Mods) + ((FinishedAction == Action.Editor) ? "" : (" (" + ActiveMods.Count + ")")));
	}

	public void AddActiveMod(StorySource storySource)
	{
		if (!IsActiveMod(storySource))
		{
			ActiveMods.Add(storySource);
			OnActiveModsChanged();
		}
	}

	public void RemoveActiveMod(StorySource storySource)
	{
		ActiveMods.Remove(storySource);
		OnActiveModsChanged();
	}

	public bool IsActiveMod(StorySource storySource)
	{
		foreach (StorySource activeMod in ActiveMods)
		{
			if (activeMod.Equals(storySource))
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (GameImpl.Instance.IsWorkshopEnabled() && WantSearchStore && WorkshopManager.Instance.StartSearchStore(CurrentTab == Tab.Mods, SearchText, Page))
		{
			WantSearchStore = false;
		}
		Populate();
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && UnityCreateNewButton == current.currentSelectedGameObject)
		{
			ActionMenu.FocusUnityObj = UnityCreateNewButton;
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate("MENU_CreateNewStoryDesc"), StoryTooltipXOffset, leftAligned: false));
		}
		ActionMenu.OnFinishAddingActions();
		if (WantScrollToButton != null && !WantSearchStore && !WantKick && !WorkshopManager.Instance.IsSearching())
		{
			WantScrollToButtonCountdown--;
			if (WantScrollToButtonCountdown <= 0)
			{
				ScrollToButton(WantScrollToButton);
				WantScrollToButton = null;
				WantScrollToButtonCountdown = 0;
			}
		}
		if (WantKick)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
			WantKick = false;
		}
		bool flag = UnityStoriesTab.gameObject.activeSelf && UnityModsTab.gameObject.activeSelf;
		UnityTabLeftPrompt.gameObject.SetActive(flag);
		UnityTabRightPrompt.gameObject.SetActive(flag);
		if (flag)
		{
			StringUtil.SetUnityTextButtonPrompt(UnityTabLeftPrompt, InputFunction.TabLeft);
			StringUtil.SetUnityTextButtonPrompt(UnityTabRightPrompt, InputFunction.TabRight);
		}
		UnityModsTab.Interactable = UnityCanvasGroup.interactable;
		UnityStoriesTab.Interactable = UnityCanvasGroup.interactable;
	}

	public override void MenuUpdate()
	{
		base.MenuUpdate();
		if (WantSelectStorySource == null)
		{
			return;
		}
		StorySourcesToSelect.Clear();
		if (FinishedAction == Action.SelectMods)
		{
			StorySourcesToSelect.AddRange(GameImpl.Instance.GetCurrentStorySources());
		}
		List<StorySource> list = new List<StorySource>();
		StorySourcesToSelect.CopyToList(list);
		string error = string.Empty;
		if (AddStoryDependenciesRecursive(WantSelectStorySource, null, StorySourcesToSelect, list, ref error, WantSelectStorySource))
		{
			bool flag = true;
			foreach (StorySource activeMod in ActiveMods)
			{
				if (!AddStoryDependenciesRecursive(activeMod, activeMod, StorySourcesToSelect, list, ref error, WantSelectStorySource))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (LoadStoryThread == null)
				{
					LoadStoryThread = new Thread((ThreadStart)delegate
					{
						GameImpl.Instance.SetCurrentStory(StorySourcesToSelect);
					});
					LoadStoryThread.Start();
					OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("LoadingPanel"));
					if (FinishedAction == Action.SelectMods)
					{
						BaseMenu.MenuTransition = ChildMenu.MenuLevel;
						ChildMenu.ActivateFrame();
					}
				}
				else if (LoadStoryThread.Join(0))
				{
					LoadStoryThread = null;
					WantSelectStorySource = null;
					StorySourcesToSelect.Clear();
					if (ChildMenu != null)
					{
						switch (FinishedAction)
						{
						case Action.StartNewGame:
						{
							ChildMenu.WantRemoveFromChain = true;
							CharacterCreationMenu characterCreationMenu = (CharacterCreationMenu)GameImpl.Instance.GetMenuBehaviourByPanelName("CharacterCreationMenuPanel");
							characterCreationMenu.JoiningNetworkGame = false;
							characterCreationMenu.Respawning = false;
							ChildMenu.OpenChildMenu(characterCreationMenu);
							break;
						}
						case Action.Editor:
							ChildMenu.WantRemoveFromChain = true;
							ChildMenu.OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditorMenuPanel"));
							break;
						case Action.SelectMods:
							Session.Instance.StorySources = GameImpl.Instance.GetCurrentStorySources();
							BaseMenu.MenuTransition = MenuLevel;
							WantPop = true;
							ChildMenu.WantPop = true;
							ChildMenu.OnDeactivate(popped: true);
							ActivateFrame();
							break;
						}
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(error))
		{
			GameImpl.Instance.ShowMessageBox(error);
			WantSelectStorySource = null;
		}
	}

	public static bool AddStoryDependenciesRecursive(StorySource storySource, StorySource addingMod, List<StorySource> results, List<StorySource> recursionCheck, ref string error, StorySource wantSelectStorySource)
	{
		foreach (StorySource item in recursionCheck)
		{
			if (item.Equals(storySource))
			{
				return true;
			}
		}
		if (addingMod != null && !storySource.GetCachedIsMod())
		{
			error = GameImpl.Translate("MENU_ModDependsOnStory").Replace("%1", addingMod.GetTranslatedName()).Replace("%2", wantSelectStorySource.GetTranslatedName())
				.Replace("%3", storySource.GetTranslatedName());
			return false;
		}
		if (storySource.WorkshopId != 0L && !WorkshopManager.Instance.IsSubscribed(storySource.WorkshopId))
		{
			if (WorkshopManager.Instance.HadSubscribeError(storySource.WorkshopId))
			{
				error = GameImpl.Translate("MENU_FailedToLoadStoryFromWorkshop").Replace("%1", storySource.GetTranslatedName());
				return false;
			}
			WorkshopManager.Instance.SubscribeItem(storySource.WorkshopId, canQueue: false);
			return false;
		}
		if (storySource.WorkshopId != 0L && !WorkshopManager.Instance.IsDownloaded(storySource.WorkshopId))
		{
			WorkshopManager.Instance.StartDownloadingItem(storySource.WorkshopId);
			return false;
		}
		if (string.IsNullOrEmpty(storySource.AbsolutePath))
		{
			WorkshopManager.Instance.ApplyAbsolutePathToDownloadedItem(storySource);
		}
		recursionCheck.Add(storySource);
		bool flag = true;
		foreach (StorySource dependency in storySource.GetDependencies())
		{
			flag &= AddStoryDependenciesRecursive(dependency, addingMod, results, recursionCheck, ref error, wantSelectStorySource);
		}
		results.Add(storySource);
		return flag;
	}

	public override void PreHandleInputImpl(InputFrame inputFrame)
	{
		if (CurrentTab != Tab.Stories && UnityStoriesTab.Selected)
		{
			SetTab(Tab.Stories);
		}
		if (CurrentTab != Tab.Mods && UnityModsTab.Selected)
		{
			SetTab(Tab.Mods);
		}
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (UnityModsTab.gameObject.activeSelf && UnityStoriesTab.gameObject.activeSelf)
		{
			if (instance.IsJustPressed(InputFunction.TabLeft) && CurrentTab > Tab.Stories)
			{
				SoundManager.PlayMenuSound(SoundManager.TabSound);
				SetTab(CurrentTab - 1);
			}
			if (instance.IsJustPressed(InputFunction.TabRight) && CurrentTab < Tab.Mods)
			{
				SoundManager.PlayMenuSound(SoundManager.TabSound);
				SetTab(CurrentTab + 1);
			}
		}
		if (instance.IsJustPressed(InputFunction.Back))
		{
			OnBack();
		}
	}

	public void OnSelectStory(StorySource storySource, bool acceptedWarning)
	{
		if (LoadStoryThread != null || ChildMenu != null)
		{
			return;
		}
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		if (!acceptedWarning && FinishedAction == Action.Editor && (EditorMenu.IsReservedStoryName(storySource.Folder) || storySource.WorkshopId != 0L))
		{
			string message = ((!(storySource.Folder == "BaseStory")) ? GameImpl.Translate("EDITOR_OnlinePlayWarning").Replace("%1", storySource.GetTranslatedName()) : GameImpl.Translate("EDITOR_OnlinePlayWarningBaseStory"));
			GameImpl.Instance.ShowConfirmationBox(message, delegate
			{
				OnSelectStory(storySource, acceptedWarning: true);
			});
		}
		else
		{
			WantSelectStorySource = storySource;
		}
	}

	public void OnCreateNewStory()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterStoryFolderName"), "", multiline: false, readOnly: false);
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string folder)
	{
		try
		{
			Directory.CreateDirectory(GameImpl.Instance.StreamingAssetsPath + "/" + folder);
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			return;
		}
		List<StorySource> list = new List<StorySource>();
		list.Add(StorySource.FromFolder("BaseStory"));
		list.Add(StorySource.FromFolder(folder));
		GameImpl.Instance.SetCurrentStory(list);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditorMenuPanel"));
	}

	public bool HasChangedActiveMods()
	{
		foreach (StorySource activeMod in ActiveMods)
		{
			if (!GameImpl.Instance.IsInCurrentStorySources(activeMod))
			{
				return true;
			}
		}
		return false;
	}

	public void OnBack()
	{
		if (LoadStoryThread == null)
		{
			if (FinishedAction == Action.SelectMods && HasChangedActiveMods())
			{
				WantSelectStorySource = GameImpl.Instance.CurrentStory.StorySource;
				return;
			}
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public void SetTab(Tab tab)
	{
		CurrentTab = tab;
		UnityStoriesTab.Selected = tab == Tab.Stories;
		UnityModsTab.Selected = tab == Tab.Mods;
		UnityDeleteContent();
		SearchText = string.Empty;
		WantSearchStore = true;
		WantKick = true;
	}
}

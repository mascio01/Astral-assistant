using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoScreen : BaseTabbableMenu
{
	public static InfoScreen Instance;

	public static Resource<GameObject> EquipmentOption;

	public static Resource<GameObject> EquipmentIcon;

	public static Resource<GameObject> EquipmentTotal;

	public static Resource<GameObject> CharacterIcon;

	public static Resource<GameObject> CharacterIconWide;

	public static Resource<GameObject> Tab;

	public static Resource<GameObject> SkillDisplay;

	public static Resource<GameObject> SurvivalFactorDisplay;

	public static Resource<GameObject> RoleDisplay;

	public static Resource<GameObject> CommunityStatDisplay;

	public static Resource<GameObject> TemperatureMarker;

	public static Resource<GameObject> Notch;

	public static Resource<GameObject> Quest;

	public static Resource<GameObject> QuestGroup;

	public static Resource<GameObject> QuestGroupSectionTitle;

	public static Resource<GameObject> InvaderIcon;

	public static Resource<GameObject> MineralButtonPrefab;

	public static Resource<GameObject> OpinionDisplay;

	public static Resource<GameObject> MemoryDisplay;

	public static Resource<GameObject> DiscoverMore;

	public static Resource<GameObject> ArrowSell;

	public static Resource<GameObject> ArrowBuy;

	public static Resource<GameObject> BasketItem;

	public static Resource<GameObject> BasketButtons;

	public static Resource<GameObject> LogEntryText;

	public static Resource<GameObject> LogEntryIcon;

	public static Resource<GameObject> LogEntrySpeechLeft;

	public static Resource<GameObject> LogEntrySpeechRight;

	public static Resource<GameObject> LogEntryQuest;

	public static Resource<GameObject> LogSeparator;

	public static Color QuestCol;

	public static Color QuestGroupCol;

	public static Resource<Material> HalftoneEquipped;

	public static Resource<Material> HalftoneSelected;

	public static Resource<Material> HalftoneDisabled;

	public static Resource<Material> HalftoneTab;

	public static Resource<Material> HalftoneDialog;

	public static Resource<Material> HalftoneNewRecipe;

	public bool Active;

	public float Transition;

	public bool WantClose;

	public bool HideActionMenu;

	public TileObject CurrentObject;

	public Type PageToOpen;

	public TileObject ObjToOpen;

	public bool WantSwitchCharacter;

	private static string InfoScreenUpdateStr = "InfoScreenUpdate";

	public static GameObject CurrentHoveredGameObject;

	public static bool AllowViewInfoOnAnyone = false;

	public bool SentScavengingFinished;

	public const float MaxDistForSwapSupplies = 3f;

	public Equipment Pourer;

	public Character PourerActor;

	public bool ActiveAndFullyTransitionedIn
	{
		get
		{
			if (Active)
			{
				return Transition == 1f;
			}
			return false;
		}
	}

	public static void LoadContent()
	{
		EquipmentOption = new Resource<GameObject>("Prefabs/UI/EquipmentOption");
		EquipmentIcon = new Resource<GameObject>("Prefabs/UI/EquipmentIcon");
		EquipmentTotal = new Resource<GameObject>("Prefabs/UI/EquipmentTotal");
		CharacterIcon = new Resource<GameObject>("Prefabs/UI/CharacterIcon");
		CharacterIconWide = new Resource<GameObject>("Prefabs/UI/CharacterIconWide");
		Tab = new Resource<GameObject>("Prefabs/UI/Tab");
		SkillDisplay = new Resource<GameObject>("Prefabs/UI/SkillDisplay");
		SurvivalFactorDisplay = new Resource<GameObject>("Prefabs/UI/SurvivalFactorDisplay");
		RoleDisplay = new Resource<GameObject>("Prefabs/UI/RoleDisplay");
		CommunityStatDisplay = new Resource<GameObject>("Prefabs/UI/CommunityStatDisplay");
		TemperatureMarker = new Resource<GameObject>("Prefabs/UI/TempMarker");
		Notch = new Resource<GameObject>("Prefabs/UI/Notch");
		Quest = new Resource<GameObject>("Prefabs/UI/Quest");
		QuestGroup = new Resource<GameObject>("Prefabs/UI/QuestGroup");
		QuestGroupSectionTitle = new Resource<GameObject>("Prefabs/UI/QuestGroupSectionTitle");
		InvaderIcon = new Resource<GameObject>("Prefabs/UI/InvaderIcon");
		MineralButtonPrefab = new Resource<GameObject>("Prefabs/UI/MineralButton");
		OpinionDisplay = new Resource<GameObject>("Prefabs/UI/OpinionDisplay");
		MemoryDisplay = new Resource<GameObject>("Prefabs/UI/MemoryDisplay");
		DiscoverMore = new Resource<GameObject>("Prefabs/UI/DiscoverMore");
		ArrowSell = new Resource<GameObject>("Prefabs/UI/ArrowSell");
		ArrowBuy = new Resource<GameObject>("Prefabs/UI/ArrowBuy");
		BasketItem = new Resource<GameObject>("Prefabs/UI/BasketItem");
		BasketButtons = new Resource<GameObject>("Prefabs/UI/BasketButtons");
		LogEntryText = new Resource<GameObject>("Prefabs/UI/LogEntryText");
		LogEntryIcon = new Resource<GameObject>("Prefabs/UI/LogEntryIcon");
		LogEntrySpeechLeft = new Resource<GameObject>("Prefabs/UI/LogEntrySpeechLeft");
		LogEntrySpeechRight = new Resource<GameObject>("Prefabs/UI/LogEntrySpeechRight");
		LogEntryQuest = new Resource<GameObject>("Prefabs/UI/LogEntryQuest");
		LogSeparator = new Resource<GameObject>("Prefabs/UI/LogSeparator");
		HalftoneEquipped = new Resource<Material>("Materials/UI/UI-HalftoneGradient-Equipped");
		HalftoneSelected = new Resource<Material>("Materials/UI/UI-HalftoneGradient-Selected");
		HalftoneDisabled = new Resource<Material>("Materials/UI/UI-HalftoneGradient-Pip");
		HalftoneTab = new Resource<Material>("Materials/UI/UI-HalftoneGradient-Tab");
		HalftoneDialog = new Resource<Material>("Materials/UI/UI-HalftoneGradient-Dialog");
		HalftoneNewRecipe = new Resource<Material>("Materials/UI/UI-HalftoneGradient-NewRecipe");
		for (int i = 0; i < SkillsPage.SkillTooltip.Length; i++)
		{
			int[] skillTooltip = SkillsPage.SkillTooltip;
			int num = i;
			SkillType skillType = (SkillType)i;
			skillTooltip[num] = StringUtil.JenkinsHash("HUD_" + skillType.ToString() + "_Tooltip");
		}
	}

	public static void OnAllContentLoaded()
	{
		QuestCol = Quest.GetAsset().GetComponent<RawImage>().color;
		QuestGroupCol = QuestGroup.GetAsset().GetComponent<RawImage>().color;
	}

	public override void OnAwake()
	{
		Instance = this;
	}

	public override void OnActivate(int page)
	{
		Active = true;
		HideActionMenu = false;
		SoundManager.PlayMenuSound(SoundManager.HudOnSound);
		if (Hud.Instance.Cursor.IsPlacingBuilding)
		{
			Hud.Instance.Cursor.StopPlacingBuilding();
		}
		if (Hud.Instance.Cursor.IsSettingCropPatch != null)
		{
			Hud.Instance.Cursor.IsSettingCropPatch = null;
		}
		Hud.Instance.Cursor.ResetShortcutTimer();
		base.OnActivate(page);
	}

	public override void OnDeactivate()
	{
		CancelPourInto();
		base.OnDeactivate();
		Active = false;
		WantClose = false;
		CurrentObject = null;
	}

	public void InfoScreenUpdate()
	{
		using (new UnityProfileMarker(InfoScreenUpdateStr))
		{
			if (PageToOpen != null)
			{
				if (Active)
				{
					int num = FindPageIndexByType(PageToOpen);
					if (num == -1)
					{
						OnDeactivate();
						Activate(ObjToOpen, PageToOpen);
					}
					else if (GetCurrentPage().GetType() == PageToOpen)
					{
						CloseInfoScreen();
					}
					else
					{
						SoundManager.PlayMenuSound(SoundManager.TabSound, 0.5f);
						SetPage(num);
					}
				}
				else
				{
					Activate(ObjToOpen, PageToOpen);
				}
				PageToOpen = null;
				ObjToOpen = null;
			}
			if (Active)
			{
				if (WantSwitchCharacter)
				{
					Hud instance = Hud.Instance;
					SwitchCurrentObject(instance.LocalControlledCharacter, instance.LocalControlledCharacterOrBuildingTheyAreIn);
				}
				Transition = Mathf.Clamp(Transition + (WantClose ? (-1f) : 1f) * Time.unscaledDeltaTime / 0.25f, 0f, 1f);
				if (Transition == 0f && WantClose)
				{
					OnDeactivate();
					return;
				}
				RectTransform obj = (RectTransform)base.gameObject.transform;
				obj.anchoredPosition = new Vector2(Mathf.Lerp(0f - obj.rect.width, 0f, Transition), 0f);
				CheckWantRepopulate();
			}
			else if (Session.Instance.WantTokenSave)
			{
				Session.Instance.AutoSave(SaveGameType.Token, Session.Instance.WantSurpriseSaveGameOverwrite, fromSuspend: false, Session.Instance.TokenSaveCameFromCarrierId, Session.Instance.TokenSaveProto);
				Session.Instance.WantTokenSave = false;
				Session.Instance.WantSurpriseSaveGameOverwrite = false;
				Session.Instance.TokenSaveCameFromCarrierId = 0;
				Session.Instance.TokenSaveProto = null;
				HintManager.Instance.Hints[11].MarkPerformed();
				if (Session.Instance.PlaySoundAfterTokenSave)
				{
					int num2 = Session.Instance.CommunityManager.PlayerCommunity.CountInventoryItemsOfClass(typeof(SavegameToken), includeBuildings: true);
					string str = GameImpl.Translate("HINT_SaveTokensRemaining").Replace("%1", num2.ToString());
					HudBehaviour.Instance.SetStatusBarMsg(StringUtil.ApplyFormulae(str, null, num2));
					SoundManager.PlayMenuSound(SoundManager.HallelujahSound, SoundManager.HallelujahVolume);
					Session.Instance.PlaySoundAfterTokenSave = false;
				}
			}
			WantSwitchCharacter = false;
		}
	}

	public void PreHandleInput(InputFrame inputFrame)
	{
		EquipmentBehaviour.CurrentHovered = null;
		CharacterIconBehaviour.CurrentHovered = null;
		EquipmentTotalBehaviour.CurrentHovered = null;
		CurrentHoveredGameObject = null;
		if (!Active || GameImpl.Instance.IsMenuOpen())
		{
			return;
		}
		InfoPage infoPage = GetCurrentPage() as InfoPage;
		if (infoPage != null)
		{
			infoPage.PreHandleInput(inputFrame);
		}
		if (Transition != 1f)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && (SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor || SelectableBehaviour.CurrentCursorHovered != null))
		{
			CurrentHoveredGameObject = current.currentSelectedGameObject;
			EquipmentBehaviour.CurrentHovered = CurrentHoveredGameObject.GetComponent<EquipmentBehaviour>();
			CharacterIconBehaviour.CurrentHovered = CurrentHoveredGameObject.GetComponent<CharacterIconBehaviour>();
			EquipmentTotalBehaviour.CurrentHovered = CurrentHoveredGameObject.GetComponent<EquipmentTotalBehaviour>();
			if (EquipmentBehaviour.CurrentHovered != null && EquipmentBehaviour.CurrentHovered.HoveredTransition == 0f)
			{
				EquipmentBehaviour.CurrentHovered = null;
			}
			if (CharacterIconBehaviour.CurrentHovered != null && CharacterIconBehaviour.CurrentHovered.HoveredTransition == 0f)
			{
				CharacterIconBehaviour.CurrentHovered = null;
			}
			if (EquipmentTotalBehaviour.CurrentHovered != null && EquipmentTotalBehaviour.CurrentHovered.HoveredTransition == 0f)
			{
				EquipmentTotalBehaviour.CurrentHovered = null;
			}
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
		if (!Active && localPlayerRecord != null && localPlayerRecord.SyncedSwappingSuppliesWith != null)
		{
			if (!SentScavengingFinished && inputFrame != null)
			{
				inputFrame.AddAction(new InputAction(InputActionType.ScavengingFinished));
				SentScavengingFinished = true;
			}
		}
		else
		{
			if (WantClose)
			{
				return;
			}
			if (Active)
			{
				if (instance2.IsJustPressed(InputFunction.Back))
				{
					MapPage mapPage = GetCurrentPage() as MapPage;
					if (Pourer != null)
					{
						CancelPourInto();
					}
					if (mapPage != null && mapPage.AltMenuOpened)
					{
						mapPage.AltMenuOpened = false;
					}
					else if (mapPage != null && Hud.Instance.DraggingZone)
					{
						Hud.Instance.DraggingZone = false;
					}
					else if (CheckATrade())
					{
						CloseInfoScreen();
						return;
					}
				}
				InfoPage infoPage = GetCurrentPage() as InfoPage;
				if (CanToggleActionMenuVisible(infoPage))
				{
					int buttonPromptHash = (HideActionMenu ? ButtonPromptBarBehaviour.PROMPT_ShowInventoryActionMenu : ButtonPromptBarBehaviour.PROMPT_HideInventoryActionMenu);
					if (instance2.IsJustPressed(InputFunction.InventoryHideActionMenu, capture: true, buttonPromptHash))
					{
						HideActionMenu = !HideActionMenu;
					}
				}
				if (infoPage != null)
				{
					infoPage.HandleInput(inputFrame);
				}
				if ((infoPage is TakePage || infoPage is TradePage) && ShouldStopSwappingSupplies(localPlayerRecord))
				{
					CloseInfoScreen();
					return;
				}
				base.HandleInput(inputFrame);
			}
			Type pageToOpen = null;
			TileObject tileObject = ((Active && CurrentObject != null && (CurrentObject.GetCommunity() == instance.CommunityManager.PlayerCommunity || AllowViewInfoOnAnyone || instance.Editor)) ? CurrentObject : GetObjectToShow());
			int buttonPromptHash2 = 0;
			if (!Active && tileObject != null && tileObject is Prop && tileObject.GetMaxInventoryWeight() > 0f && !(tileObject is EnterableVehicle { IsMoving: not false }))
			{
				buttonPromptHash2 = ButtonPromptBarBehaviour.PROMPT_Inventory;
			}
			if (instance2.IsJustPressed(InputFunction.Inventory, capture: true, buttonPromptHash2))
			{
				pageToOpen = ((tileObject is Character) ? typeof(CharacterPage) : typeof(BuildingPage));
			}
			if (instance2.IsJustPressed(InputFunction.Community))
			{
				pageToOpen = typeof(CommunityPage);
			}
			if (instance2.IsJustPressed(InputFunction.Quests))
			{
				pageToOpen = typeof(QuestPage);
			}
			if (instance2.IsJustPressed(InputFunction.Map))
			{
				pageToOpen = typeof(MapPage);
			}
			if (instance2.IsJustPressed(InputFunction.Log))
			{
				pageToOpen = typeof(LogPage);
			}
			PageToOpen = pageToOpen;
			ObjToOpen = tileObject;
		}
	}

	public void CloseInfoScreen()
	{
		CloseInfoScreen(playSound: true);
	}

	public void CloseInfoScreen(bool playSound)
	{
		if (Active)
		{
			if (playSound)
			{
				SoundManager.PlayMenuSound(SoundManager.HudOffSound);
			}
			WantClose = true;
		}
	}

	public void OnCloseButtonClicked()
	{
		if (CheckATrade())
		{
			CloseInfoScreen();
		}
	}

	public bool CanToggleActionMenuVisible(InfoPage page)
	{
		if (GameImpl.Instance.IsDialogOpen())
		{
			return false;
		}
		if (Pourer != null)
		{
			return false;
		}
		if (!(page == null))
		{
			return page.CanToggleActionMenuVisible();
		}
		return true;
	}

	public bool CheckATrade()
	{
		TradePage tradePage = GetCurrentPage() as TradePage;
		if (tradePage != null && tradePage.PendingTrades.Count > 0)
		{
			GameImpl.Instance.ShowYesNoCancelBox(GameImpl.Translate("HUD_ConfirmTrade"), delegate(InputFrame inputFrame2)
			{
				GameImpl.Instance.PopDialog();
				if (tradePage.ConfirmTrade(inputFrame2, closeOnTrade: true))
				{
					CloseInfoScreen();
				}
			}, delegate(InputFrame inputFrame2)
			{
				GameImpl.Instance.PopDialog();
				if (tradePage.CancelTrade(inputFrame2))
				{
					CloseInfoScreen();
				}
			}, needsSession: true);
			return false;
		}
		BrainScanPage brainScanPage = GetCurrentPage() as BrainScanPage;
		if (brainScanPage != null && brainScanPage.FromMapPageObj != null)
		{
			TileObject fromMapPageObj = brainScanPage.FromMapPageObj;
			Hud.Instance.OpenInfoScreenFor = fromMapPageObj;
			Hud.Instance.WantInfoScreenType = Hud.OpenInfoScreenType.ExitBrainScanToMap;
			return false;
		}
		return true;
	}

	public static bool GetAllowViewInfoOnAnyone()
	{
		return AllowViewInfoOnAnyone;
	}

	public static void SetAllowViewInfoOnAnyone(bool on)
	{
		AllowViewInfoOnAnyone = on;
	}

	public TileObject GetObjectToShow()
	{
		Session instance = Session.Instance;
		Hud instance2 = Hud.Instance;
		if (instance.Editor)
		{
			return instance2.EditorSelectedObject;
		}
		TileObject result = ((instance2.LocalControlledCharacter != null && instance2.LocalControlledCharacter.InsideBuilding != null) ? ((MultiTileObject)instance2.LocalControlledCharacter.InsideBuilding) : ((MultiTileObject)instance2.LocalControlledCharacter));
		if (instance.GameCamera.FlyCam)
		{
			TileObject cursorTargetObject = instance2.CursorTargetObject;
			if (cursorTargetObject != null && cursorTargetObject.GetUnderConstructionInfo() == null && (cursorTargetObject.GetCommunity() == instance.CommunityManager.PlayerCommunity || AllowViewInfoOnAnyone || instance.Editor) && cursorTargetObject.GetMaxInventoryWeight() > 0f)
			{
				result = cursorTargetObject;
			}
		}
		return result;
	}

	private bool ShouldStopSwappingSupplies(PlayerRecord localPlayerRecord)
	{
		if (localPlayerRecord.SyncedSwappingSuppliesWith != null)
		{
			if (localPlayerRecord.SyncedSwappingSuppliesWith.Deleted)
			{
				return true;
			}
			if (!localPlayerRecord.SyncedSwappingSupplies.IsControllableByPlayer() || localPlayerRecord.SyncedSwappingSupplies.InCombat)
			{
				return true;
			}
			if (!(localPlayerRecord.SyncedSwappingSuppliesWith is Character { IsAwake: not false } character))
			{
				if (localPlayerRecord.SyncedSwappingSupplies.CurrentActionAnim != ActionAnim.ScavengeLoop && localPlayerRecord.SyncedSwappingSupplies.CurrentActionAnim != ActionAnim.ScavengeCorpseLoop)
				{
					return true;
				}
			}
			else
			{
				if (character.InCombat)
				{
					return true;
				}
				if (localPlayerRecord.SyncedSwappingSuppliesMode == SwappingSuppliesMode.Stealing && character.IsAwake)
				{
					return true;
				}
			}
			TerrainCoord tile = localPlayerRecord.SyncedSwappingSupplies.Tile;
			TerrainCoord nearestTileTo = localPlayerRecord.SyncedSwappingSuppliesWith.GetNearestTileTo(tile);
			if (tile.GetDistSquared(nearestTileTo) > 9f)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void Activate(TileObject obj, Type selectedPageType)
	{
		Session instance = Session.Instance;
		ClearTabs();
		Character character = obj as Character;
		if (character != null)
		{
			AddTab(CharacterPage.Instance.Initialize(character));
			if (character.Community != null && (instance.FollowerCommandsEnabled || AllowViewInfoOnAnyone || instance.Editor || (character.Community.GetLivingNonZombieMemberCount() > 1 && character.Community.CommunityType == CommunityType.Player)))
			{
				AddTab(CommunityPage.Instance.Initialize(character.Community));
			}
		}
		if (obj is Prop prop && (prop.GetMaxInventoryWeight() > 0f || obj is Building))
		{
			AddTab(BuildingPage.Instance.Initialize(prop));
			if (instance.FollowerCommandsEnabled || (instance.CommunityManager.PlayerCommunity != null && instance.CommunityManager.PlayerCommunity.GetLivingNonZombieMemberCount() > 1))
			{
				AddTab(CommunityPage.Instance.Initialize(instance.CommunityManager.PlayerCommunity));
			}
		}
		if (instance.FollowerCommandsEnabled || StoryManager.Instance.QuestInstances.Count > 0)
		{
			AddTab(QuestPage.Instance.Initialize());
			AddTab(MapPage.Instance.Initialize());
		}
		if (instance.FollowerCommandsEnabled || instance.LogEvents.Count > 0)
		{
			AddTab(LogPage.Instance.Initialize((character != null) ? character : Hud.Instance.LocalControlledCharacter));
		}
		CurrentObject = obj;
		OnActivate(Math.Max(0, (selectedPageType != null) ? FindPageIndexByType(selectedPageType) : 0));
		if (selectedPageType != null)
		{
			if (obj is Building)
			{
				HintManager.Instance.Hints[34].MarkPerformed();
			}
			else if (character != null)
			{
				HintManager.Instance.Hints[12].MarkPerformed();
			}
		}
	}

	public void SwitchCurrentObject(TileObject obj, TileObject objOrBuildingTheyAreIn)
	{
		BaseTabPage currentPage = GetCurrentPage();
		bool flag = currentPage is MapPage;
		bool flag2 = currentPage is CommunityPage;
		if (CurrentObject != obj && CurrentObject != objOrBuildingTheyAreIn && !flag2)
		{
			Session.Instance.GameCamera.TeleportToObject(objOrBuildingTheyAreIn);
			OnDeactivate();
			Activate(obj, null);
		}
		else
		{
			if (flag2)
			{
				RemoveTab(0);
				Character character = obj as Character;
				if (character != null)
				{
					InsertTab(0, CharacterPage.Instance.Initialize(character));
				}
				if (obj is Prop prop && (prop.GetMaxInventoryWeight() > 0f || obj is Building))
				{
					InsertTab(0, BuildingPage.Instance.Initialize(prop));
				}
				ReInitTab(LogPage.Instance.Initialize((character != null) ? character : Hud.Instance.LocalControlledCharacter));
				CurrentObject = obj;
			}
			WantRepopulate = true;
		}
		if (flag)
		{
			SetPage(FindPageIndexByType(typeof(MapPage)));
		}
		ForceCanvasUpdate();
	}

	public void ForceCanvasUpdate()
	{
		Canvas.ForceUpdateCanvases();
		foreach (InfoPage page in Pages)
		{
			page.UnityTab.Update();
			page.UnityTab.UnityBackground.Update();
		}
		Update();
	}

	public void ActivateTake(TileObject takeFrom, Character taker, SwappingSuppliesMode mode)
	{
		ClearTabs();
		AddTab(TakePage.Instance.Initialize(takeFrom, taker, mode));
		CurrentObject = takeFrom;
		OnActivate(0);
	}

	public void ActivateTrade(Character takeFrom, Character taker, SwappingSuppliesMode mode)
	{
		ClearTabs();
		AddTab(TradePage.Instance.Initialize(takeFrom, taker, mode));
		CurrentObject = takeFrom;
		OnActivate(0);
	}

	public void ActivateGather(Prop gatherFrom, Character gatherer, SwappingSuppliesMode mode)
	{
		ClearTabs();
		AddTab(GatherPage.Instance.Initialize(gatherFrom, gatherer, mode));
		CurrentObject = gatherFrom;
		OnActivate(0);
	}

	public void ActivateBrainScan(Character character, bool fullBrainScan, TileObject fromMapPageObj)
	{
		HintManager.Instance.Hints[18].MarkPerformed();
		ClearTabs();
		AddTab(BrainScanPage.Instance.Initialize(character, fullBrainScan || AllowViewInfoOnAnyone || Session.Instance.Editor, fromMapPageObj));
		CurrentObject = character;
		OnActivate(0);
	}

	public bool IsShowingInventoryFor(TileObject obj)
	{
		if (!Active || CurrentPage == -1)
		{
			return false;
		}
		return ((InfoPage)Pages[CurrentPage]).IsShowingInventoryFor(obj);
	}

	public bool IsShowingBrainScanFor(Character character)
	{
		if (!Active || CurrentPage == -1)
		{
			return false;
		}
		return ((InfoPage)Pages[CurrentPage]).IsShowingBrainScanFor(character);
	}

	public bool IsShowingCommunity()
	{
		return GetCurrentPage() is CommunityPage;
	}

	public bool IsShowingQuests()
	{
		return GetCurrentPage() is QuestPage;
	}

	public bool IsShowingLog()
	{
		return GetCurrentPage() is LogPage;
	}

	public bool IsShowingMap()
	{
		return GetCurrentPage() is MapPage;
	}

	public void StartPourInto(TileObject carrier, Equipment pourer, Character actor)
	{
		Pourer = pourer;
		PourerActor = actor;
		HideActionMenu = false;
	}

	public void CancelPourInto()
	{
		Pourer = null;
		PourerActor = null;
	}
}

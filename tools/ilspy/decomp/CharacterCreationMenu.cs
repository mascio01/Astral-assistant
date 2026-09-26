using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterCreationMenu : BaseMenu
{
	public static CharacterCreationMenu Instance;

	public bool JoiningNetworkGame;

	public bool Respawning;

	public bool WantNewGame;

	public bool WantReturnToTitleMenu;

	public CharacterCreationSettings Settings = new CharacterCreationSettings();

	public HairType GenderSwappedHairType;

	public FaceType GenderSwappedFaceType;

	public FacialHairType GenderSwappedFacialHairType;

	public Color32 GenderSwappedUnderwearColor;

	public string GenderSwappedFirstName = "";

	public bool CommunityUsingLeadersName;

	public CharacterCreationTabsPanel TabsPanel;

	public Button UnityStartButton;

	public Human PreviewGimp = new Human();

	public List<Equipment> PreviewEquipment = new List<Equipment>();

	public RenderTexture PreviewRenderTexture;

	public float CameraAngle;

	public float ZoomAmount = 1f;

	public bool Dragging;

	public Vector2 LastDragPosition;

	public bool ChangedAppearance;

	public bool ChangedBones;

	public string LoadedFavouriteCharacterName = "";

	public static bool IsRepopulating = false;

	public ActionMenu ActionMenu = new ActionMenu();

	private static float DragRotSpeed = 10f;

	private static float CamRotSpeed = 180f;

	private static float ZoomSpeed = 2f;

	public int DisableInput;

	private bool IsMouseOverCharacterPreviewPanel;

	private static float PreviewZoomedInCamDist = 0.5f;

	private static float PreviewZoomedInCamOffsetY = -0.1f;

	private static float PreviewZoomedOutCamDist = 1.8f;

	private static float PreviewZoomedOutCamHeight = HumanAppearance.MaleDefaultHeight * 0.5f;

	public static float EquipmentPageLoadoutSelectMenuXOffset = 150f;

	public static float SkillsPageLoadoutSelectMenuXOffset = -320f;

	public static float SkillsPageMinusButtonTooltipXOffset = 0f;

	public static float SkillsPagePlusButtonTooltipExtraXOffset = -250f;

	public static float DifficultySelectMenuXOffset = -700f;

	public override bool GetWantNewGame()
	{
		return WantNewGame;
	}

	public override CharacterCreationSettings GetCharacterCreationSettings()
	{
		return Settings;
	}

	public void SetChangedClothes()
	{
		ChangedAppearance = true;
	}

	public void SetChangedAppearance()
	{
		ChangedAppearance = true;
		if (!IsRepopulating)
		{
			LoadedFavouriteCharacterName = "";
		}
	}

	public void SetChangedBonesFromSkill()
	{
		ChangedBones = true;
	}

	public void SetChangedBones()
	{
		ChangedBones = true;
		if (!IsRepopulating)
		{
			LoadedFavouriteCharacterName = "";
		}
	}

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
		UnityStartButton = base.gameObject.FindChild("StartButton").GetComponent<Button>();
		TabsPanel = base.gameObject.FindChild("CharacterCreationTabsPanel").GetComponent<CharacterCreationTabsPanel>();
		TabsPanel.Owner = this;
		ClothesPage.Instance = TabsPanel.gameObject.FindChild("Panel/ClothesPage").GetComponent<ClothesPage>();
		EquipmentPage.Instance = TabsPanel.gameObject.FindChild("Panel/EquipmentPage").GetComponent<EquipmentPage>();
		FacePage.Instance = TabsPanel.gameObject.FindChild("Panel/FacePage").GetComponent<FacePage>();
		VitalStatsPage.Instance = TabsPanel.gameObject.FindChild("Panel/VitalStatsPage").GetComponent<VitalStatsPage>();
		SkillsPage.Instance = TabsPanel.gameObject.FindChild("Panel/SkillsPage").GetComponent<SkillsPage>();
		PersonalityPage.Instance = TabsPanel.gameObject.FindChild("Panel/PersonalityPage").GetComponent<PersonalityPage>();
		WorldPage.Instance = TabsPanel.gameObject.FindChild("Panel/WorldPage").GetComponent<WorldPage>();
		PaperTextureAmount = 0.25f;
	}

	public void RandomiseAll()
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		ClothesPage.Instance.ResetClothes(this);
		EquipmentPage.Instance.ResetEquipment(this);
		SkillsPage.Instance.ResetSkills(this);
		GenderType genderType = (GenderType)nonDeterministicRand.Next(2);
		DifficultySettings difficultySettings = Settings.DifficultySettings;
		Settings = new CharacterCreationSettings();
		Settings.RandomSeed = nonDeterministicRand.Next();
		Settings.DifficultySettings = difficultySettings;
		Settings.StartDayOfYear = (settings.ProcedurallyGenerated ? nonDeterministicRand.Next(Weather.DaysInAYear) : settings.StartDayOfYear);
		Settings.StartHourOfDay = (settings.ProcedurallyGenerated ? (nonDeterministicRand.RandomFloat() * 24f) : ((float)settings.StartHourOfDay));
		Settings.Appearance.Gender = genderType;
		Settings.Appearance.Age = (int)HumanAppearance.PickRandomAge(nonDeterministicRand);
		Settings.GetHumanAppearance().Randomize(InfectionType.None, nonDeterministicRand);
		Settings.FirstName = GameImpl.Instance.PickRandomName((Settings.Appearance.Gender != GenderType.Male) ? NameType.FemaleFirstName : NameType.MaleFirstName, nonDeterministicRand);
		Settings.Surname = GameImpl.Instance.PickRandomName(NameType.Surname, nonDeterministicRand);
		Settings.GangName.Randomise(nonDeterministicRand, unique: false, null);
		GenderType genderType2 = ((genderType == GenderType.Male) ? GenderType.Female : GenderType.Male);
		GenderSwappedHairType = HumanAppearance.PickRandomHairType(genderType2, Settings.Appearance.Age, nonDeterministicRand);
		GenderSwappedFaceType = HumanAppearance.PickRandomFaceType(InfectionType.None, genderType2, Settings.Appearance.Age, nonDeterministicRand);
		GenderSwappedFacialHairType = HumanAppearance.PickRandomFacialHairType(genderType2, Settings.Appearance.Age, nonDeterministicRand);
		GenderSwappedUnderwearColor = HumanAppearance.PickRandomUnderwearColor(genderType2, nonDeterministicRand);
		GenderSwappedFirstName = GameImpl.Instance.PickRandomName((genderType2 != GenderType.Male) ? NameType.FemaleFirstName : NameType.MaleFirstName, nonDeterministicRand);
		ClothesPage.Instance.ClothesPointsRemaining = settings.CalcClothesPoints(Settings.StartDayOfYear);
		EquipmentPage.Instance.EquipmentPointsRemaining = settings.EquipmentPoints;
		ClothesPage.Instance.RandomiseClothes(this, nonDeterministicRand);
		if (settings.Loadouts.Count > 0)
		{
			SetLoadout(settings.Loadouts[nonDeterministicRand.Next(settings.Loadouts.Count)]);
		}
		else
		{
			EquipmentPage.Instance.RandomiseEquipment(this, nonDeterministicRand);
			SkillsPage.Instance.RandomiseSkillPoints(this, nonDeterministicRand);
		}
		PreviewGimp.RandomizePersonality(MathUtil.NonDeterministicRand, PersonalityGroup.NormalFaction);
		PreviewGimp.Personality.CopyToList(Settings.Personality);
		TabsPanel.WantRepopulate = true;
	}

	public void SetLoadout(Loadout loadout)
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		SkillsPage.Instance.ResetSkills(this);
		EquipmentPage.Instance.ResetEquipment(this);
		foreach (LoadoutEquipment item in loadout.Equipment)
		{
			int equipmentIndex = settings.GetEquipmentIndex(item.Name);
			if (equipmentIndex != -1)
			{
				for (int i = 0; i < item.Amount; i++)
				{
					AddStartingEquipment(equipmentIndex, toggle: false);
				}
			}
		}
		for (int j = 0; j < loadout.SkillPoints.Length; j++)
		{
			for (int k = 0; k < loadout.SkillPoints[j]; k++)
			{
				SkillsPage.Instance.IncrementSkill(j);
			}
		}
		TabsPanel.WantRepopulate = true;
	}

	public Equipment AddStartingEquipment(int index, bool toggle)
	{
		StartingEquipment startingEquipment = GameImpl.Instance.CurrentStory.Settings.StartingEquipmentOptions[index];
		Equipment equipment = PreviewEquipment[index];
		if (equipment == null)
		{
			return null;
		}
		if (equipment.GetClothingType() != ClothingType.Invalid)
		{
			RemoveStartingClothes(equipment.GetClothingType());
		}
		if (Settings.Inventory.Contains(equipment))
		{
			if (toggle || !equipment.CanBeCombined())
			{
				return null;
			}
			Equipment equipment2 = Equipment.Create(GameImpl.Instance.FindEquipmentPrototypeByName(startingEquipment.Name));
			equipment2.SetNewAmount(startingEquipment.GetAmount());
			PreviewGimp.Inventory.Add(PreviewGimp, equipment2);
		}
		else
		{
			PreviewGimp.Inventory.Add(PreviewGimp, equipment);
			Settings.Inventory.Add(equipment);
			if (equipment.GetClothingType() != ClothingType.Invalid)
			{
				equipment.Wear(PreviewGimp);
				SetChangedClothes();
			}
		}
		if (equipment.GetClothingType() != ClothingType.Invalid && equipment.GetClothingType() != ClothingType.Backpack && equipment.GetClothingType() != ClothingType.BodyArmor && equipment.GetClothingType() != ClothingType.LegArmor)
		{
			ClothesPage.Instance.ClothesPointsRemaining -= startingEquipment.Points;
		}
		else
		{
			EquipmentPage.Instance.EquipmentPointsRemaining -= startingEquipment.Points;
		}
		TabsPanel.WantRepopulate = true;
		return equipment;
	}

	public void RemoveStartingClothes(ClothingType clothingType)
	{
		if (PreviewGimp.Clothes[(int)clothingType] != null)
		{
			int num = PreviewEquipment.IndexOf(PreviewGimp.Clothes[(int)clothingType]);
			if (num != -1)
			{
				RemoveStartingEquipment(num, toggle: true);
			}
		}
	}

	public void RemoveStartingEquipment(int index, bool toggle)
	{
		StartingEquipment startingEquipment = GameImpl.Instance.CurrentStory.Settings.StartingEquipmentOptions[index];
		Equipment equipment = PreviewEquipment[index];
		if (equipment != null && Settings.Inventory.Contains(equipment))
		{
			int num = (toggle ? (equipment.GetAmount() / startingEquipment.GetAmount() * startingEquipment.Points) : startingEquipment.Points);
			if (equipment.GetClothingType() != ClothingType.Invalid && equipment.GetClothingType() != ClothingType.Backpack && equipment.GetClothingType() != ClothingType.BodyArmor && equipment.GetClothingType() != ClothingType.LegArmor)
			{
				ClothesPage.Instance.ClothesPointsRemaining += num;
			}
			else
			{
				EquipmentPage.Instance.EquipmentPointsRemaining += num;
			}
			TabsPanel.WantRepopulate = true;
			if (toggle || !equipment.CanBeCombined() || equipment.GetAmount() <= startingEquipment.GetAmount())
			{
				Settings.Inventory.Remove(equipment);
				PreviewGimp.Inventory.Remove(PreviewGimp, equipment);
				equipment.SetNewAmount(startingEquipment.GetAmount());
			}
			else
			{
				equipment.IncrementAmount(-startingEquipment.GetAmount());
			}
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Instance = this;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		if (settings.DifficultySettings.Count > 1 && settings.GetDifficultySettings(settings.DefaultDifficulty) != null)
		{
			Settings.DifficultySettings = settings.GetDifficultySettings(settings.DefaultDifficulty).MakeCopy();
		}
		else if (settings.DifficultySettings.Count > 0)
		{
			Settings.DifficultySettings = settings.DifficultySettings[0].MakeCopy();
		}
		PreviewGimp.Usage = CharacterUsage.PreviewGimp;
		foreach (StartingEquipment startingEquipmentOption in settings.StartingEquipmentOptions)
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(startingEquipmentOption.Name);
			if (equipmentPrototype != null)
			{
				CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
				Equipment equipment = Equipment.Create(equipmentPrototype);
				if (!equipmentPrototype.CanBeCombined)
				{
					startingEquipmentOption.Amount = 1;
				}
				equipment.SetNewAmount(startingEquipmentOption.GetAmount());
				equipment.RandomiseVariations(nonDeterministicRand);
				PreviewEquipment.Add(equipment);
			}
			else
			{
				PreviewEquipment.Add(null);
			}
		}
		LoadedFavouriteCharacterName = "";
		IsRepopulating = true;
		TabsPanel.Activate();
		IsRepopulating = false;
		RandomiseAll();
		PreviewGimp.Appearance = Settings.Appearance;
		PreviewGimp.UnityInit();
		PreviewGimp.UnityActivate();
		PreviewGimp.Unity.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		PreviewGimp.Unity.Animator.Play(AnimHash.Idle);
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		GameImpl instance = GameImpl.Instance;
		instance.UnityCharacterCreationCameraObj.SetActive(value: false);
		instance.UnityCharacterCreationCameraObj.GetComponent<Camera>().targetTexture = null;
		TabsPanel.OnDeactivate();
		PreviewGimp.UnityDeactivate();
		PreviewGimp.UnityDelete();
		if (PreviewRenderTexture != null)
		{
			PreviewRenderTexture.Release();
			PreviewRenderTexture = null;
		}
		PreviewGimp.Inventory.RemoveAll(PreviewGimp);
		PreviewEquipment.Clear();
		WantReturnToTitleMenu = false;
		WantNewGame = false;
		Instance = null;
	}

	public override void PreHandleInputImpl(InputFrame inputFrame)
	{
		BaseCharacterCreationPage baseCharacterCreationPage = TabsPanel.GetCurrentPage() as BaseCharacterCreationPage;
		IsMouseOverCharacterPreviewPanel = baseCharacterCreationPage != null && baseCharacterCreationPage.PreviewImage != null && InputFunctionManager.Instance.IsMouseOverPanel(baseCharacterCreationPage.PreviewImage.rectTransform);
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (DisableInput > 0)
		{
			DisableInput--;
			return;
		}
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (instance.IsJustPressed(InputFunction.Back))
		{
			OnBack();
			return;
		}
		float num = instance.GetAxis(InputFunction.MapZoom) * GameImpl.UnscaledDeltaTime * ZoomSpeed;
		float num2 = instance.GetAxis(InputFunction.CharacterPreviewRotate) * GameImpl.UnscaledDeltaTime * CamRotSpeed * (MathF.PI / 180f);
		if (Dragging)
		{
			if (instance.IsKeyPressed(KeyCode.Mouse0))
			{
				Vector3 vector = instance.GetMousePosition();
				num2 += DragRotSpeed * (vector.x - LastDragPosition.x) / (float)Screen.width;
				LastDragPosition = vector;
			}
			else
			{
				Dragging = false;
			}
		}
		if (IsMouseOverCharacterPreviewPanel)
		{
			num += instance.GetMouseAxis(MouseAxis.ScrollWheel) * GameImpl.UnscaledDeltaTime * ZoomSpeed;
			if (instance.IsKeyJustPressed(KeyCode.Mouse0, capture: false))
			{
				instance.CaptureKey(KeyCode.Mouse0, Captured.ThisFrame);
				Dragging = true;
				LastDragPosition = instance.GetMousePosition();
			}
		}
		CameraAngle += num2;
		ZoomAmount = Mathf.Clamp01(ZoomAmount + num);
		TabsPanel.HandleInput(inputFrame);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		OnlineParty instance = OnlineParty.Instance;
		Session instance2 = Session.Instance;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		BaseTabPage currentPage = TabsPanel.GetCurrentPage();
		if (currentPage != null && !currentPage.gameObject.activeSelf)
		{
			currentPage.gameObject.SetActive(value: true);
		}
		IsRepopulating = true;
		TabsPanel.CheckWantRepopulate();
		IsRepopulating = false;
		if (ChangedAppearance)
		{
			PreviewGimp.UnityOnChangedAppearance();
			PreviewGimp.UnityUpdateAppearance();
			ChangedAppearance = false;
		}
		if (ChangedBones)
		{
			PreviewGimp.Appearance = Settings.Appearance;
			((HumanAppearance)PreviewGimp.Appearance).SetupDNA();
			PreviewGimp.UnityOnChangedBones();
			ChangedBones = false;
		}
		PreviewGimp.UnityUpdate();
		Vector3 vector = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(CameraAngle));
		Vector3 a = PreviewGimp.Position + new Vector3(0f, PreviewZoomedOutCamHeight, 0f) + vector * PreviewZoomedOutCamDist;
		Vector3 b = PreviewGimp.EyePosition + new Vector3(0f, PreviewZoomedInCamOffsetY, 0f) + vector * PreviewZoomedInCamDist;
		GameImpl instance3 = GameImpl.Instance;
		instance3.UnityCharacterCreationCameraObj.SetActive(value: true);
		instance3.UnityCharacterCreationCameraObj.transform.position = Vector3.Lerp(a, b, ZoomAmount);
		instance3.UnityCharacterCreationCameraObj.transform.rotation = Quaternion.LookRotation(-vector, Vector3.up);
		instance3.Sun.SetupLightForTime(0.5f, useMiddayReflectionTex: true);
		ActionMenu.ClearActions();
		EquipmentBehaviour equipmentBehaviour = null;
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && (SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor || SelectableBehaviour.CurrentCursorHovered != null))
		{
			equipmentBehaviour = current.currentSelectedGameObject.GetComponent<EquipmentBehaviour>();
		}
		if (equipmentBehaviour != null)
		{
			int index = PreviewEquipment.IndexOf(equipmentBehaviour.Item);
			int points = GameImpl.Instance.CurrentStory.Settings.StartingEquipmentOptions[index].Points;
			EquipmentOptionBehaviour component = equipmentBehaviour.transform.parent.GetComponent<EquipmentOptionBehaviour>();
			ActionMenu.FocusUnityObj = equipmentBehaviour.gameObject;
			ActionMenu.AddHeaderActionsForEquipment(PreviewGimp, PreviewGimp, equipmentBehaviour.Item, null, SwappingSuppliesMode.None);
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.StartingPoints, null, equipmentBehaviour.Item, points, (!component.HasEnoughPoints()) ? CursorActionDisabledReason.Disabled : CursorActionDisabledReason.Enabled));
		}
		if (current != null && SkillsPage.Instance.UnityLoadoutDropdown != null && SkillsPage.Instance.UnityLoadoutDropdown.gameObject == current.currentSelectedGameObject && SkillsPage.Instance.UnityLoadoutDropdown.value < settings.Loadouts.Count)
		{
			ActionMenu.FocusUnityObj = SkillsPage.Instance.UnityLoadoutDropdown.gameObject;
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(settings.Loadouts[SkillsPage.Instance.UnityLoadoutDropdown.value].GetDescriptionKey()), SkillsPageLoadoutSelectMenuXOffset, leftAligned: false));
		}
		if (current != null)
		{
			int buttonIdx = -1;
			SkillType skillTypeFromUnityButton = SkillsPage.Instance.GetSkillTypeFromUnityButton(current.currentSelectedGameObject, out buttonIdx);
			if (skillTypeFromUnityButton != SkillType.Count)
			{
				ActionMenu.FocusUnityObj = current.currentSelectedGameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(SkillsPage.SkillTooltip[(int)skillTypeFromUnityButton]), SkillsPageMinusButtonTooltipXOffset + (float)buttonIdx * SkillsPagePlusButtonTooltipExtraXOffset, leftAligned: false));
			}
		}
		if (current != null && EquipmentPage.Instance.UnityLoadoutDropdown != null && EquipmentPage.Instance.UnityLoadoutDropdown.gameObject == current.currentSelectedGameObject && EquipmentPage.Instance.UnityLoadoutDropdown.value < settings.Loadouts.Count)
		{
			ActionMenu.FocusUnityObj = EquipmentPage.Instance.UnityLoadoutDropdown.gameObject;
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(settings.Loadouts[EquipmentPage.Instance.UnityLoadoutDropdown.value].GetDescriptionKey()), EquipmentPageLoadoutSelectMenuXOffset, leftAligned: true));
		}
		if (current != null && WorldPage.Instance.UnityDifficultyDropdown != null && WorldPage.Instance.UnityDifficultyDropdown.gameObject == current.currentSelectedGameObject && WorldPage.Instance.UnityDifficultyDropdown.value < settings.DifficultySettings.Count)
		{
			ActionMenu.FocusUnityObj = WorldPage.Instance.UnityDifficultyDropdown.gameObject;
			ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(settings.DifficultySettings[WorldPage.Instance.UnityDifficultyDropdown.value].GetDescriptionKey()), DifficultySelectMenuXOffset, leftAligned: false));
		}
		ActionMenu.OnFinishAddingActions();
		bool flag = !string.IsNullOrEmpty(Settings.FirstName) && !string.IsNullOrEmpty(Settings.Surname);
		if (!JoiningNetworkGame)
		{
			flag &= !Settings.GangName.IsNull();
		}
		UnityStartButton.interactable = flag;
		if (!JoiningNetworkGame)
		{
			return;
		}
		if (Respawning)
		{
			if (instance2 == null || instance2.State != SessionState.Started || !instance2.IsInMultiplayerGameAsCommunityFollower())
			{
				WantFadeOut = true;
			}
		}
		else if (!instance.IsInMultiplayerGameAsFollower())
		{
			instance.LeaveLobby();
			WantReturnToTitleMenu = true;
			WantFadeOut = true;
		}
	}

	public void OnNewGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		int num = Character.CalcRequiredInsulationForTemperature(Weather.CalcAverageTemperatureInCelsiusFromDayOfYear(GetCharacterCreationSettings().StartDayOfYear));
		if (EquipmentPage.Instance.EquipmentPointsRemaining > 0 || (ClothesPage.Instance.ClothesPointsRemaining > 0 && PreviewGimp.GetTotalClothingInsulation() < num) || SkillsPage.Instance.SkillPointsRemaining > 0)
		{
			string text = GameImpl.Translate("MENU_StillHavePointsRemaining");
			text += " (";
			bool flag = false;
			if (EquipmentPage.Instance.EquipmentPointsRemaining > 0)
			{
				if (flag)
				{
					text += ", ";
					flag = false;
				}
				text += GameImpl.Translate("MENU_EquipmentPoints").Replace("%1", EquipmentPage.Instance.EquipmentPointsRemaining.ToString());
				flag = true;
			}
			if (ClothesPage.Instance.ClothesPointsRemaining > 0)
			{
				if (flag)
				{
					text += ", ";
					flag = false;
				}
				text += GameImpl.Translate("MENU_ClothesPoints2").Replace("%1", ClothesPage.Instance.ClothesPointsRemaining.ToString());
				flag = true;
			}
			if (SkillsPage.Instance.SkillPointsRemaining > 0)
			{
				if (flag)
				{
					text += ", ";
					flag = false;
				}
				text += GameImpl.Translate("MENU_SkillPoints").Replace("%1", SkillsPage.Instance.SkillPointsRemaining.ToString());
				flag = true;
			}
			text += ")";
			GameImpl.Instance.ShowConfirmationBox(text, delegate
			{
				GameImpl.Instance.PopDialog();
				NewGameCheckDifficulty();
			}, JoiningNetworkGame);
		}
		else
		{
			NewGameCheckDifficulty();
		}
	}

	public void NewGameCheckDifficulty()
	{
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		if (JoiningNetworkGame && Session.Instance != null)
		{
			Session.Instance.IncomingCharacter = characterCreationSettings;
			WantFadeOut = true;
		}
		else if (GameImpl.Instance.CurrentStory.Settings.DifficultySettings.Count > 1 && GameImpl.Instance.CurrentStory.Settings.FindMatchingDifficultySettings(characterCreationSettings.DifficultySettings) != null)
		{
			GameImpl.Instance.ShowDifficultyDialogBox(OnAcceptDifficulty, characterCreationSettings);
		}
		else
		{
			NewGameCheckAllowJoin();
		}
	}

	public void OnAcceptDifficulty()
	{
		GameImpl.Instance.PopDialog();
		NewGameCheckAllowJoin();
	}

	public void NewGameCheckAllowJoin()
	{
		if (GameImpl.Instance.WantShowAllowJoinDialog())
		{
			GameImpl.Instance.ShowAllowJoinDialogBox(NewGame);
		}
		else
		{
			NewGame();
		}
	}

	private void NewGame()
	{
		WantNewGame = true;
		WantFadeOut = true;
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		if (JoiningNetworkGame)
		{
			if (!Respawning)
			{
				OnlineParty.Instance.LeaveLobby();
				WantReturnToTitleMenu = true;
			}
			WantFadeOut = true;
		}
		else
		{
			WantPop = true;
		}
	}

	public void UpdatePreviewGimpSkills()
	{
		PreviewGimp.Appearance = Settings.Appearance;
		for (int i = 0; i < Settings.Skills.Length; i++)
		{
			PreviewGimp.Skillset.SetCap(PreviewGimp, (SkillType)i, 5);
			PreviewGimp.Skillset.SetLevel(PreviewGimp, (SkillType)i, Settings.Skills[i]);
		}
		SetChangedBonesFromSkill();
		TabsPanel.WantRepopulate = true;
	}

	public void SetupPreviewImage(RawImage previewImage)
	{
		GameImpl instance = GameImpl.Instance;
		int num = (int)(instance.ScreenScale * previewImage.rectTransform.rect.width);
		int num2 = (int)(instance.ScreenScale * previewImage.rectTransform.rect.height);
		if (PreviewRenderTexture != null && (num != PreviewRenderTexture.width || num2 != PreviewRenderTexture.height))
		{
			PreviewRenderTexture.Release();
			PreviewRenderTexture = null;
			instance.UnityCharacterCreationCameraObj.GetComponent<Camera>().targetTexture = null;
		}
		if (PreviewRenderTexture == null)
		{
			PreviewRenderTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32);
			instance.UnityCharacterCreationCameraObj.GetComponent<Camera>().targetTexture = PreviewRenderTexture;
		}
		previewImage.texture = PreviewRenderTexture;
	}
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseMenu : BaseSelectionOwner
{
	public static float FadeTime = 0.25f;

	protected bool WantFadeOut;

	public bool WantPop;

	public bool WantRemoveFromChain;

	protected bool Ready;

	private static float FadeTransition = 0f;

	public static float MenuTransition = 0f;

	public int MenuLevel;

	public BaseMenu ChildMenu;

	public GameObject UnityBackgroundObj;

	public RawImage UnityPortrait;

	public RawImage UnityBackground;

	public RawImage UnityOverlay;

	public TextMeshProUGUI UnityWIPText;

	public TileObject PortraitSubject;

	public PortraitPose PortraitPose;

	public PortraitPose TreelinePose = PortraitPose.Treeline;

	public Vector2 PortraitOffset;

	protected float PaperTextureAmount = 0.75f;

	private string MenuCaption;

	public static Resource<GameObject> MenuBackground;

	public static Resource<GameObject> MenuOverlay;

	public static Resource<GameObject> MenuText;

	public static Resource<GameObject> MenuButton;

	public static Resource<GameObject> NewSaveGameButton;

	public static Resource<GameObject> SaveGameButton;

	public static Resource<GameObject> JoinGameButton;

	public static Resource<GameObject> ScriptObjectPrefab;

	public static Resource<GameObject> CreditsHeading;

	public static Resource<GameObject> CreditsLineDouble;

	public static Resource<GameObject> CreditsLineSingle;

	public static Resource<GameObject> CreditsLineIcon;

	public static Resource<GameObject> CreditsSpacer;

	public static Resource<GameObject> PatchNotesHeading;

	public static Resource<GameObject> PatchNotesLine;

	public static Resource<GameObject> OptionsRow;

	public static Resource<GameObject> OptionButton;

	public static Resource<GameObject> PersonalityTag;

	public static Resource<GameObject> ControlsRow;

	public static Resource<GameObject> ControlsRowXBox;

	public static Resource<GameObject> DifficultyToggle;

	public static Resource<GameObject> ModButton;

	public static Resource<GameObject> DownloadedModsTitle;

	public static Resource<GameObject> CommunityModsTitle;

	public static Resource<GameObject> ShowCommunityModsButton;

	public static Resource<GameObject> NextPageButton;

	public static Resource<GameObject> PageButton;

	public static Resource<GameObject> NoMods;

	public const string UnityButtonTextObjName = "Text";

	public const string UnityButtonImageObjName = "Image";

	public const string UnityStoryFolderTextObjName = "StoryFolderText";

	public const string UnityLeaderNameTextObjName = "LeaderNameText";

	public const string UnityNumMembersTextObjName = "NumMembersText";

	public const string UnityCommunitySizeTextObjName = "CommunitySizeText";

	public const string UnityDescriptionTextObjName = "DescriptionText";

	public virtual bool GetWantLatestSave()
	{
		if (!(ChildMenu != null))
		{
			return false;
		}
		return ChildMenu.GetWantLatestSave();
	}

	public virtual bool GetWantNewGame()
	{
		if (!(ChildMenu != null))
		{
			return false;
		}
		return ChildMenu.GetWantNewGame();
	}

	public virtual CharacterCreationSettings GetCharacterCreationSettings()
	{
		if (!(ChildMenu != null))
		{
			return null;
		}
		return ChildMenu.GetCharacterCreationSettings();
	}

	public virtual bool GetWantRunEditor()
	{
		if (!(ChildMenu != null))
		{
			return false;
		}
		return ChildMenu.GetWantRunEditor();
	}

	public virtual SaveGame GetWantLoadSaveGame()
	{
		if (!(ChildMenu != null))
		{
			return null;
		}
		return ChildMenu.GetWantLoadSaveGame();
	}

	public virtual ActionMenu GetActionMenu()
	{
		if (!(ChildMenu != null))
		{
			return null;
		}
		return ChildMenu.GetActionMenu();
	}

	public override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
		AwakeImpl();
	}

	public override void Update()
	{
		base.Update();
		RectTransform rectTransform = (RectTransform)base.gameObject.transform;
		base.gameObject.transform.localPosition = new Vector3(((float)MenuLevel - MenuTransition) * rectTransform.rect.width, 0f, 0f);
		UpdateImpl();
		if (PortraitSubject != null || PortraitPose == PortraitPose.Helicopter || PortraitPose == PortraitPose.BigHelicopter)
		{
			if (UnityBackgroundObj == null)
			{
				UnityBackgroundObj = UnityEngine.Object.Instantiate((GameObject)MenuBackground, base.transform, worldPositionStays: false);
				UnityBackgroundObj.transform.localPosition = Vector3.zero;
				UnityBackgroundObj.transform.SetAsFirstSibling();
				UnityPortrait = UnityBackgroundObj.transform.Find("FrameOutline/Frame/Portrait").GetComponent<RawImage>();
				UnityBackground = UnityBackgroundObj.transform.Find("FrameOutline/Frame/Background").GetComponent<RawImage>();
				UnityWIPText = UnityBackgroundObj.transform.Find("WIP").gameObject.GetComponent<TextMeshProUGUI>();
				UpdateWIPText();
			}
			if (UnityPortrait != null && UnityBackground != null)
			{
				PortraitPose pose = PortraitPose;
				if (PortraitSubject is Character character && character.IsNaked())
				{
					pose = PortraitPose.Treeline;
				}
				Texture2D portrait = PortraitGallery.Instance.GetPortrait(PortraitSubject, pose);
				UnityPortrait.texture = portrait;
				UnityPortrait.rectTransform.anchoredPosition = PortraitOffset;
				Texture2D portrait2 = PortraitGallery.Instance.GetPortrait(PortraitSubject, TreelinePose);
				UnityBackground.texture = portrait2;
				Ready |= portrait != null && portrait2 != null;
			}
		}
		if (PaperTextureAmount > 0f)
		{
			if (UnityOverlay == null)
			{
				UnityOverlay = UnityEngine.Object.Instantiate((GameObject)MenuOverlay, base.transform, worldPositionStays: false).GetComponent<RawImage>();
				UnityOverlay.color = new Color(1f, 1f, 1f, PaperTextureAmount);
			}
			UnityOverlay.transform.SetAsLastSibling();
		}
	}

	public void UpdateWIPText()
	{
		if (!(UnityWIPText == null))
		{
			UnityWIPText.SetUnityText(string.Empty);
		}
	}

	public virtual void AwakeImpl()
	{
	}

	public virtual void PreHandleInputImpl(InputFrame inputFrame)
	{
	}

	public virtual void HandleInputImpl(InputFrame inputFrame)
	{
	}

	public virtual void UpdateImpl()
	{
	}

	public void SetFinished()
	{
		WantFadeOut = true;
	}

	public bool IsFadeFinished()
	{
		if (FadeTransition == 0f)
		{
			if (!WantFadeOut)
			{
				if (ChildMenu != null)
				{
					return ChildMenu.IsFadeFinished();
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected void SetMenuCaption(string key)
	{
		MenuCaption = GameImpl.Translate(key);
		if (string.IsNullOrEmpty(MenuCaption))
		{
			return;
		}
		GameObject gameObject = base.gameObject.FindChild("Caption/Text");
		if (gameObject != null)
		{
			TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				component.SetUnityText(GameImpl.Translate(key));
			}
		}
		MenuCaption = null;
	}

	public void ActivateFrame()
	{
		RectTransform rectTransform = (RectTransform)base.gameObject.transform;
		base.gameObject.transform.localPosition = new Vector3(((float)MenuLevel - MenuTransition) * rectTransform.rect.width, 0f, 0f);
		base.gameObject.SetActive(value: true);
	}

	public virtual void OnActivate()
	{
		ActivateFrame();
		StartGeneratingPortrait();
		UpdateWIPText();
	}

	public void StartGeneratingPortrait(SavedCharacter savedCharacter = null, GenderType genderType = GenderType.Count)
	{
		Ready = true;
		if (PortraitPose != PortraitPose.None)
		{
			CustomRandom customRandom = (PortraitGeneratorMenu.Instance.IsGeneratingAchievementIcons() ? PortraitGeneratorMenu.Instance.Rand : MathUtil.NonDeterministicRand);
			Human human = new Human();
			PortraitPose portraitPose = PortraitPose;
			if (portraitPose == PortraitPose.ZombieCharging || (uint)(portraitPose - 9) <= 2u)
			{
				human.Infection = (InfectionType)customRandom.Next(1, 5);
			}
			human.SetInfectionProgression((human.Infection != InfectionType.None) ? 1f : 0f);
			if (savedCharacter != null)
			{
				savedCharacter.ApplyToCharacter(human, includeClothes: true);
			}
			else
			{
				HumanAppearance humanAppearance = new HumanAppearance((genderType < GenderType.Count) ? genderType : ((!customRandom.RandomChoice(0.5f)) ? GenderType.Female : GenderType.Male), HumanAppearance.PickRandomAge(customRandom));
				humanAppearance.Randomize(human.Infection, customRandom);
				human.Appearance = humanAppearance;
				human.RandomizeClothing(customRandom, seasonallyAppropriate: false, isPortrait: true, mustHaveBackpack: false, mustHaveBodyArmor: false, mustHaveHelmet: false, mustHaveLegArmor: false);
			}
			EquipmentPrototype equipmentPrototype = null;
			switch (PortraitPose)
			{
			case PortraitPose.SpikedBatOverShoulder:
				equipmentPrototype = EquipmentPrototype.SpikedBaseballBat;
				break;
			case PortraitPose.SpikedBatOverShoulderCloseUp:
				equipmentPrototype = EquipmentPrototype.SpikedBaseballBat;
				break;
			case PortraitPose.RifleIdle:
				equipmentPrototype = EquipmentPrototype.AssaultRifle;
				break;
			case PortraitPose.RifleIdleCloseUp:
				equipmentPrototype = EquipmentPrototype.AssaultRifle;
				break;
			case PortraitPose.InWatchtower:
				equipmentPrototype = EquipmentPrototype.AssaultRifle;
				break;
			case PortraitPose.ShotgunAiming:
				equipmentPrototype = EquipmentPrototype.Shotgun;
				break;
			case PortraitPose.PistolAiming:
				equipmentPrototype = EquipmentPrototype.Pistol;
				break;
			case PortraitPose.HandsUp:
				equipmentPrototype = EquipmentPrototype.Pistol;
				break;
			case PortraitPose.BowAiming:
				equipmentPrototype = EquipmentPrototype.Bow;
				break;
			case PortraitPose.CrouchingWithKnife:
				equipmentPrototype = ((EquipmentPrototype.HuntingKnives.Count > 0) ? EquipmentPrototype.HuntingKnives[0] : null);
				break;
			case PortraitPose.ChoppingWithAxe:
				equipmentPrototype = EquipmentPrototype.Axe;
				break;
			case PortraitPose.PickaxeOverShoulder:
				equipmentPrototype = EquipmentPrototype.Pickaxe;
				break;
			case PortraitPose.PickaxeOverShoulderCloseUp:
				equipmentPrototype = EquipmentPrototype.Pickaxe;
				break;
			case PortraitPose.OneHandedAttack1:
				equipmentPrototype = EquipmentPrototype.Axe;
				break;
			case PortraitPose.OneHandedAttack1CloseUp:
				equipmentPrototype = EquipmentPrototype.Axe;
				break;
			case PortraitPose.OneHandedAttack2:
				equipmentPrototype = EquipmentPrototype.Axe;
				break;
			case PortraitPose.Drinking:
				equipmentPrototype = EquipmentPrototype.VodkaBottle;
				break;
			case PortraitPose.Reading:
				equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName("Calypso1");
				break;
			}
			if (equipmentPrototype != null)
			{
				human.EquippedItem = Equipment.Create(equipmentPrototype);
			}
			PortraitSubject = human;
			Ready = false;
		}
	}

	public virtual void OnDeactivate(bool popped)
	{
		if (ChildMenu != null && popped)
		{
			ChildMenu.OnDeactivate(popped: true);
			ChildMenu = null;
		}
		DeactivateEventSystem(popped);
		WantPop = false;
		WantFadeOut = false;
		Ready = false;
	}

	public virtual void PreHandleInput(InputFrame inputFrame)
	{
		if (ChildMenu != null)
		{
			ChildMenu.PreHandleInput(inputFrame);
			return;
		}
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (instance.IsJustPressed(InputFunction.Back, capture: false))
		{
			GameObject gameObject = UnityEventSystem.currentSelectedGameObject;
			while (gameObject != null)
			{
				gameObject = ((gameObject.transform.parent != null) ? gameObject.transform.parent.gameObject : null);
				if (gameObject != null && gameObject.GetComponent<TMP_Dropdown>() != null)
				{
					instance.Capture(InputFunction.Back, untilReleased: true);
					gameObject.GetComponent<TMP_Dropdown>().Hide();
					break;
				}
			}
		}
		PreHandleInputImpl(inputFrame);
	}

	public void HandleInput(InputFrame inputFrame)
	{
		if (ChildMenu != null)
		{
			ChildMenu.HandleInput(inputFrame);
		}
		else
		{
			HandleInputImpl(inputFrame);
		}
	}

	public virtual void MenuUpdate()
	{
		if (!WantPop && Ready && (ChildMenu == null || !ChildMenu.Ready || ChildMenu.WantPop))
		{
			if (FadeTransition > 0f)
			{
				MenuTransition = MathUtil.Delt(MenuTransition, MenuLevel, Time.unscaledDeltaTime / FadeTime);
			}
			else
			{
				MenuTransition = MenuLevel;
			}
			FadeTransition = Mathf.Clamp(FadeTransition + (WantFadeOut ? (-1f) : 1f) * Time.unscaledDeltaTime / FadeTime, 0f, 1f);
		}
		if (UnityEventSystem != null && MenuTransition == (float)MenuLevel)
		{
			UnityCanvasGroup.interactable = !GameImpl.Instance.IsDialogOpen() && !WantPop && !WantFadeOut && ChildMenu == null;
			UnityEventSystem.gameObject.SetActive(UnityCanvasGroup.interactable);
		}
		if (!(ChildMenu != null))
		{
			return;
		}
		if (ChildMenu.WantRemoveFromChain && !ChildMenu.gameObject.activeSelf && MenuTransition >= (float)(MenuLevel + 2))
		{
			BaseMenu childMenu = ChildMenu;
			ChildMenu = childMenu.ChildMenu;
			childMenu.ChildMenu = null;
			childMenu.OnDeactivate(popped: true);
			BaseMenu childMenu2 = ChildMenu;
			while (childMenu2 != null)
			{
				childMenu2.MenuLevel--;
				childMenu2 = childMenu2.ChildMenu;
			}
			MenuTransition -= 1f;
		}
		if (ChildMenu.WantPop && !base.gameObject.activeSelf)
		{
			OnActivate();
		}
		if (!ChildMenu.WantPop && MenuTransition >= (float)MenuLevel + 1f && base.gameObject.activeSelf)
		{
			OnDeactivate(popped: false);
		}
		ChildMenu.MenuUpdate();
		if (ChildMenu.WantPop && MenuTransition <= (float)MenuLevel)
		{
			OnChildMenuFinished();
		}
	}

	public void OpenChildMenu(BaseMenu menu)
	{
		ChildMenu = menu;
		ChildMenu.MenuLevel = MenuLevel + 1;
		ChildMenu.OnActivate();
	}

	protected virtual void OnChildMenuFinished()
	{
		ChildMenu.OnDeactivate(popped: true);
		ChildMenu = null;
		if (WantRemoveFromChain)
		{
			WantPop = true;
		}
	}

	public static float GetFadeBlackness()
	{
		return 1f - FadeTransition;
	}

	public virtual void OnPartyChanged()
	{
		if (ChildMenu != null)
		{
			ChildMenu.OnPartyChanged();
		}
	}

	public virtual void OnLobbyListSearchFinished()
	{
		if (ChildMenu != null)
		{
			ChildMenu.OnLobbyListSearchFinished();
		}
	}

	public virtual void OnJoinLobbyFailed(string errorMsg)
	{
		if (ChildMenu != null)
		{
			ChildMenu.OnJoinLobbyFailed(errorMsg);
		}
	}

	public virtual void OnWorkshopItemQueryFinished()
	{
		if (ChildMenu != null)
		{
			ChildMenu.OnWorkshopItemQueryFinished();
		}
	}

	public virtual void OnSearchStoreFinished(bool mods, List<StorySource> results, int total)
	{
		if (ChildMenu != null)
		{
			ChildMenu.OnSearchStoreFinished(mods, results, total);
		}
	}

	public static void LoadContent()
	{
		MenuBackground = new Resource<GameObject>("Prefabs/UI/MenuBackground");
		MenuOverlay = new Resource<GameObject>("Prefabs/UI/MenuOverlay");
		MenuText = new Resource<GameObject>("Prefabs/UI/MenuText");
		MenuButton = new Resource<GameObject>("Prefabs/UI/MenuButton");
		NewSaveGameButton = new Resource<GameObject>("Prefabs/UI/NewSaveGameButton");
		SaveGameButton = new Resource<GameObject>("Prefabs/UI/SaveGameButton");
		JoinGameButton = new Resource<GameObject>("Prefabs/UI/JoinGameButton");
		ScriptObjectPrefab = new Resource<GameObject>("Prefabs/UI/ScriptObject");
		CreditsHeading = new Resource<GameObject>("Prefabs/UI/CreditsHeading");
		CreditsLineDouble = new Resource<GameObject>("Prefabs/UI/CreditsLine");
		CreditsLineSingle = new Resource<GameObject>("Prefabs/UI/CreditsLineSingle");
		CreditsLineIcon = new Resource<GameObject>("Prefabs/UI/CreditsLineIcon");
		CreditsSpacer = new Resource<GameObject>("Prefabs/UI/CreditsSpacer");
		PatchNotesHeading = new Resource<GameObject>("Prefabs/UI/PatchNotesHeading");
		PatchNotesLine = new Resource<GameObject>("Prefabs/UI/PatchNotesLine");
		OptionsRow = new Resource<GameObject>("Prefabs/UI/OptionsRow");
		OptionButton = new Resource<GameObject>("Prefabs/UI/OptionButton");
		PersonalityTag = new Resource<GameObject>("Prefabs/UI/Personality");
		ControlsRow = new Resource<GameObject>("Prefabs/UI/ControlsRow");
		ControlsRowXBox = new Resource<GameObject>("Prefabs/UI/ControlsRowXBox");
		DifficultyToggle = new Resource<GameObject>("Prefabs/UI/DifficultyToggle");
		ModButton = new Resource<GameObject>("Prefabs/UI/ModButton");
		DownloadedModsTitle = new Resource<GameObject>("Prefabs/UI/DownloadedModsTitle");
		CommunityModsTitle = new Resource<GameObject>("Prefabs/UI/CommunityModsTitle");
		ShowCommunityModsButton = new Resource<GameObject>("Prefabs/UI/ShowCommunityModsButton");
		NextPageButton = new Resource<GameObject>("Prefabs/UI/NextPage");
		PageButton = new Resource<GameObject>("Prefabs/UI/PageButton");
		NoMods = new Resource<GameObject>("Prefabs/UI/NoMods");
	}

	public GameObject AddMenuText(GameObject parent, int i, string text)
	{
		GameObject obj = UnityEngine.Object.Instantiate(MenuText.GetAsset());
		obj.name = "";
		obj.transform.SetParent(parent.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		obj.GetComponent<TextMeshProUGUI>().SetUnityText(text);
		obj.GetComponent<TranslateBehaviour>().DontTranslate();
		return obj;
	}

	public GameObject AddMenuButton(GameObject parent, int i, string text, UnityAction call)
	{
		GameObject obj = UnityEngine.Object.Instantiate(MenuButton.GetAsset());
		obj.name = "";
		obj.transform.SetParent(parent.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		obj.GetComponent<Button>().onClick.RemoveAllListeners();
		obj.GetComponent<Button>().onClick.AddListener(call);
		GameObject obj2 = obj.transform.Find("Text").gameObject;
		obj2.GetComponent<TextMeshProUGUI>().SetUnityText(text);
		obj2.GetComponent<TranslateBehaviour>().DontTranslate();
		return obj;
	}

	public GameObject AddNewSaveGameButton(GameObject parent, int i, UnityAction call)
	{
		GameObject obj = UnityEngine.Object.Instantiate(NewSaveGameButton.GetAsset());
		obj.name = "";
		obj.transform.SetParent(parent.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		obj.GetComponent<Button>().onClick.RemoveAllListeners();
		obj.GetComponent<Button>().onClick.AddListener(call);
		return obj;
	}

	public GameObject AddSaveGameButton(GameObject parent, int i, string text, string dayText, string communitySizeText, string communityName, string characterName, bool isTokenSave, Texture2D tex, UnityAction call, UnityAction openFolderCall, UnityAction deleteCall)
	{
		GameObject obj = UnityEngine.Object.Instantiate(SaveGameButton.GetAsset());
		obj.name = "";
		obj.transform.SetParent(parent.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		GameObject obj2 = obj.transform.Find("MainButton").gameObject;
		obj2.GetComponent<Button>().onClick.RemoveAllListeners();
		obj2.GetComponent<Button>().onClick.AddListener(call);
		GameObject obj3 = obj.transform.Find("Options/OpenFolderButton").gameObject;
		obj3.GetComponent<Button>().onClick.RemoveAllListeners();
		obj3.GetComponent<Button>().onClick.AddListener(openFolderCall);
		GameObject obj4 = obj.transform.Find("Options/DeleteButton").gameObject;
		obj4.GetComponent<Button>().onClick.RemoveAllListeners();
		obj4.GetComponent<Button>().onClick.AddListener(deleteCall);
		obj.transform.Find("MainButton/Panel/Text").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(text);
		obj.transform.Find("MainButton/Panel/Day").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(dayText);
		obj.transform.Find("MainButton/Panel/CommunitySize").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(communitySizeText);
		obj.transform.Find("MainButton/Panel/Name").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText((string.IsNullOrEmpty(communityName) || string.IsNullOrEmpty(characterName)) ? "" : (characterName + " (" + communityName + ")"));
		obj.transform.Find("MainButton/Panel/TokenIcon").gameObject.SetActive(isTokenSave);
		obj.transform.Find("MainButton/Image").gameObject.GetComponent<RawImage>().texture = tex;
		return obj;
	}

	public GameObject AddJoinGameButton(GameObject parent, int i, bool enabled, string storyName, string leaderName, int numMembers, int maxMembers, int communitySize, string description, UnityAction call)
	{
		GameObject obj = UnityEngine.Object.Instantiate(JoinGameButton.GetAsset());
		obj.name = "";
		obj.transform.SetParent(parent.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		obj.GetComponent<Button>().onClick.RemoveAllListeners();
		obj.GetComponent<Button>().onClick.AddListener(call);
		GameObject gameObject = obj.transform.Find("StoryFolderText").gameObject;
		GameObject gameObject2 = obj.transform.Find("LeaderNameText").gameObject;
		GameObject gameObject3 = obj.transform.Find("NumMembersText").gameObject;
		GameObject gameObject4 = obj.transform.Find("CommunitySizeText").gameObject;
		GameObject obj2 = obj.transform.Find("DescriptionText").gameObject;
		gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(storyName);
		gameObject2.GetComponent<TextMeshProUGUI>().SetUnityText(leaderName);
		gameObject3.GetComponent<TextMeshProUGUI>().SetUnityText(numMembers + "/" + maxMembers);
		gameObject4.GetComponent<TextMeshProUGUI>().SetUnityText((communitySize > 0) ? communitySize.ToString() : string.Empty);
		obj2.GetComponent<TextMeshProUGUI>().SetUnityText(description);
		gameObject.GetComponent<TextMeshProUGUI>().color = (enabled ? Color.white : Color.gray);
		gameObject2.GetComponent<TextMeshProUGUI>().color = (enabled ? Color.white : Color.gray);
		gameObject3.GetComponent<TextMeshProUGUI>().color = (enabled ? Color.white : Color.gray);
		gameObject4.GetComponent<TextMeshProUGUI>().color = (enabled ? Color.white : Color.gray);
		obj2.GetComponent<TextMeshProUGUI>().color = (enabled ? Color.white : Color.gray);
		return obj;
	}

	public void SetMenuButtonText(string buttonName, string text)
	{
		base.gameObject.transform.Find(buttonName).gameObject.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(text);
	}

	public void SetMenuButtonActive(string buttonName, bool active)
	{
		base.gameObject.transform.Find(buttonName).gameObject.SetActive(active);
	}

	public static GameObject AddInputField(GameObject panel, ref int index, string name, int i, string v, UnityAction<string> call)
	{
		return AddInputField(panel, ref index, name, i, showArrayIndex: true, v, call);
	}

	public static GameObject AddInputField(GameObject panel, ref int index, string name, int i, bool showArrayIndex, string v, UnityAction<string> call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/TextField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.FindChild("Text");
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(name) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : "") + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject inputFieldObj = gameObject.FindChild("InputField");
		inputFieldObj.GetComponent<TMP_InputField>().onValueChanged.RemoveAllListeners();
		inputFieldObj.GetComponent<TMP_InputField>().onEndEdit.RemoveAllListeners();
		inputFieldObj.GetComponent<TMP_InputField>().SetUnityText(v);
		inputFieldObj.GetComponent<TMP_InputField>().onEndEdit.AddListener(call);
		inputFieldObj.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate
		{
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)inputFieldObj.transform);
		});
		return gameObject;
	}

	public static GameObject AddColorField(GameObject panel, ref int index, string name, int i, Color32 v, UnityAction<string> call)
	{
		return AddColorField(panel, ref index, name, i, showArrayIndex: true, v, call);
	}

	public static GameObject AddColorField(GameObject panel, ref int index, string name, int i, bool showArrayIndex, Color32 v, UnityAction<string> call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/ColorField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.FindChild("Text");
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(name) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : "") + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject obj2 = gameObject.FindChild("InputField");
		obj2.GetComponent<TMP_InputField>().onEndEdit.RemoveAllListeners();
		obj2.GetComponent<TMP_InputField>().SetUnityText(StringUtil.ColorToHex(v));
		obj2.GetComponent<TMP_InputField>().onEndEdit.AddListener(call);
		gameObject.FindChild("Color").GetComponent<RawImage>().color = v;
		return gameObject;
	}

	public static GameObject AddCheckbox(GameObject panel, ref int index, string name, int i, bool v, UnityAction<bool> call)
	{
		return AddCheckbox(panel, ref index, name, i, showArrayIndex: true, v, call);
	}

	public static GameObject AddCheckbox(GameObject panel, ref int index, string name, int i, bool showArrayIndex, bool v, UnityAction<bool> call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/CheckboxField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.transform.Find("Text").gameObject;
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(name) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : "") + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject obj2 = gameObject.transform.Find("Toggle").gameObject;
		obj2.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
		obj2.GetComponent<Toggle>().isOn = v;
		obj2.GetComponent<Toggle>().onValueChanged.AddListener(call);
		return gameObject;
	}

	public static GameObject AddDropDown(GameObject panel, ref int index, string name, int i, string displayName, int v, List<string> options, UnityAction<int> call)
	{
		return AddDropDown(panel, ref index, name, i, showArrayIndex: true, displayName, v, options, call);
	}

	public static GameObject AddDropDown(GameObject panel, ref int index, string name, int i, bool showArrayIndex, string displayName, int v, List<string> options, UnityAction<int> call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/DropDownField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.FindChild("Text");
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(string.IsNullOrEmpty(displayName) ? name : displayName) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : "") + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		TMP_Dropdown component = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
		component.ClearOptions();
		component.AddOptions(options);
		component.RefreshShownValue();
		component.onValueChanged.RemoveAllListeners();
		component.value = v;
		component.onValueChanged.AddListener(call);
		return gameObject;
	}

	public static GameObject AddDoubleDropDown(GameObject panel, ref int index, string name, int i, string displayName, int v, List<string> options, UnityAction<int> call, int v2, List<string> options2, UnityAction<int> call2)
	{
		return AddDoubleDropDown(panel, ref index, name, i, showArrayIndex: true, displayName, v, options, call, v2, options2, call2);
	}

	public static GameObject AddDoubleDropDown(GameObject panel, ref int index, string name, int i, bool showArrayIndex, string displayName, int v, List<string> options, UnityAction<int> call, int v2, List<string> options2, UnityAction<int> call2)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/DoubleDropDownField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.FindChild("Text");
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(string.IsNullOrEmpty(displayName) ? name : displayName) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : "") + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		TMP_Dropdown component = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
		component.ClearOptions();
		component.AddOptions(options);
		component.RefreshShownValue();
		component.onValueChanged.RemoveAllListeners();
		component.value = v;
		component.onValueChanged.AddListener(call);
		TMP_Dropdown component2 = gameObject.FindChild("Dropdown2").GetComponent<TMP_Dropdown>();
		component2.ClearOptions();
		component2.AddOptions(options2);
		component2.RefreshShownValue();
		component2.onValueChanged.RemoveAllListeners();
		component2.value = v2;
		component2.onValueChanged.AddListener(call2);
		return gameObject;
	}

	public static GameObject AddButtonField(GameObject panel, ref int index, string name, int i, UnityAction call)
	{
		return AddButtonField(panel, ref index, name, i, showArrayIndex: true, call);
	}

	public static GameObject AddButtonField(GameObject panel, ref int index, string name, int i, bool showArrayIndex, UnityAction call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/ButtonField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			GameObject obj = gameObject.FindChild("Button/Text");
			obj.GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(name) + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : ""));
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject obj2 = gameObject.FindChild("Button");
		obj2.GetComponent<Button>().onClick.RemoveAllListeners();
		obj2.GetComponent<Button>().onClick.AddListener(call);
		return gameObject;
	}

	public static GameObject AddTitleField(GameObject panel, ref int index, string name, int i, UnityAction call)
	{
		return AddTitleField(panel, ref index, name, i, showArrayIndex: true, call);
	}

	public static GameObject AddTitleField(GameObject panel, ref int index, string name, int i, bool showArrayIndex, UnityAction call)
	{
		GameObject gameObject = panel.FindChild(name + ((i >= 0) ? i.ToString() : ""));
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/TitleField"), panel.transform, worldPositionStays: false);
			gameObject.name = name + ((i >= 0) ? i.ToString() : "");
			gameObject.FindChild("Text").GetComponent<TMP_Text>().SetUnityText(name + ((i >= 0 && showArrayIndex) ? (" [" + i + "]") : ""));
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject gameObject2 = gameObject.FindChild("Button");
		if (call != null)
		{
			gameObject2.SetActive(value: true);
			gameObject2.GetComponent<Button>().onClick.RemoveAllListeners();
			gameObject2.GetComponent<Button>().onClick.AddListener(call);
		}
		else
		{
			gameObject2.SetActive(value: false);
		}
		return gameObject;
	}

	public Toggle SetCheckboxValue(string name, bool on)
	{
		Toggle component = base.gameObject.FindChild(name).GetComponent<Toggle>();
		component.isOn = on;
		return component;
	}

	public Slider SetSliderValue(string name, float val, float min, float max)
	{
		Slider component = base.gameObject.FindChild(name).GetComponent<Slider>();
		component.minValue = min;
		component.maxValue = max;
		component.value = val;
		return component;
	}

	public TMP_Dropdown SetDropdownValue(string name, int val, string[] optionStrings)
	{
		TMP_Dropdown component = base.gameObject.FindChild(name).GetComponent<TMP_Dropdown>();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		list.Clear();
		for (int i = 0; i < optionStrings.Length; i++)
		{
			list.Add(new TMP_Dropdown.OptionData(optionStrings[i]));
		}
		component.options = list;
		component.value = val;
		return component;
	}

	public static void SetupNavigation(List<Selectable> selectables, bool topAndBottomAreAutomatic)
	{
		for (int i = 0; i < selectables.Count; i++)
		{
			Selectable selectable = null;
			Selectable selectable2 = null;
			int num = i - 1;
			int j = i + 1;
			while (num >= 0)
			{
				if (selectables[num].IsActive())
				{
					selectable = selectables[num];
					break;
				}
				num--;
			}
			for (; j < selectables.Count; j++)
			{
				if (selectables[j].IsActive())
				{
					selectable2 = selectables[j];
					break;
				}
			}
			Navigation navigation = selectables[i].navigation;
			navigation.mode = (((selectable != null && selectable2 != null) || !topAndBottomAreAutomatic) ? Navigation.Mode.Explicit : Navigation.Mode.Automatic);
			navigation.selectOnUp = selectable;
			navigation.selectOnDown = selectable2;
			selectables[i].navigation = navigation;
		}
		selectables.Clear();
	}

	public static List<string> GetEquipmentOptions()
	{
		List<string> list = new List<string>();
		list.Add("");
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			list.Add(item.Key);
		}
		return list;
	}

	public static List<string> GetLiquidOptions()
	{
		List<string> list = new List<string>();
		list.Add("");
		foreach (KeyValuePair<string, LiquidPrototype> item in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			list.Add(item.Key);
		}
		return list;
	}

	public static List<string> GetPropOptions()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			list.Add(item.Value.Name);
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetPropOptions(BaseObjectType baseObjectType)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.TypeName == baseObjectType)
			{
				list.Add(item.Value.Name);
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetPropOptions(Type type)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			BaseObject baseObject = BaseObjectManager.PrototypeGameObjects[(int)item.Value.TypeName];
			if (baseObject != null && baseObject.GetType().IsA(type))
			{
				list.Add(item.Value.Name);
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetLootLocationOptions()
	{
		GameImpl instance = GameImpl.Instance;
		List<string> list = new List<string>();
		foreach (Story currentStory in instance.CurrentStories)
		{
			if (currentStory.LootLocationsList == null)
			{
				continue;
			}
			foreach (LootLocationDef lootLocation in currentStory.LootLocationsList.LootLocations)
			{
				list.Add(lootLocation.Name);
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetPersonalityFactionOptions()
	{
		GameImpl instance = GameImpl.Instance;
		List<string> list = new List<string>();
		foreach (Story currentStory in instance.CurrentStories)
		{
			if (currentStory.PersonalitiesList == null)
			{
				continue;
			}
			foreach (PersonalityGroup personalityGroup in currentStory.PersonalitiesList.PersonalityGroups)
			{
				if (!list.Contains(personalityGroup.Faction))
				{
					list.Add(personalityGroup.Faction);
				}
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetPrefabOptions()
	{
		List<string> list = new List<string>();
		foreach (Resource<GameObject> item in Resource<GameObject>.AllResourcesOfType)
		{
			if ((!(item is PrefabResource prefabResource) || !prefabResource.IsSkinned()) && !list.Contains(item.GetPath()))
			{
				list.Add(item.GetPath());
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetMaterialOptions()
	{
		List<string> list = new List<string>();
		foreach (Resource<Material> item in Resource<Material>.AllResourcesOfType)
		{
			if (!list.Contains(item.GetPath()))
			{
				list.Add(item.GetPath());
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetSoundOptions()
	{
		List<string> list = new List<string>();
		foreach (Resource<AudioClip> item in Resource<AudioClip>.AllResourcesOfType)
		{
			if (!list.Contains(item.GetPath()))
			{
				list.Add(item.GetPath());
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetAnimalOptions(bool includeHuman)
	{
		List<string> list = new List<string>();
		list.Add("");
		list.Add("Chicken");
		list.Add("Deer");
		if (includeHuman)
		{
			list.Add("Human");
		}
		list.Add("Rabbit");
		return list;
	}

	public static List<string> GetTemplateOptions()
	{
		List<string> list = new List<string>();
		foreach (Story currentStory in GameImpl.Instance.CurrentStories)
		{
			foreach (KeyValuePair<string, BaseScriptObject> item in currentStory.ScriptObjectsByUniqueID)
			{
				if (item.Value is Template && !list.Contains(item.Key))
				{
					list.Add(item.Key);
				}
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}

	public static List<string> GetInvaderOptions()
	{
		List<string> list = new List<string>();
		foreach (Story currentStory in GameImpl.Instance.CurrentStories)
		{
			foreach (KeyValuePair<string, BaseScriptObject> item in currentStory.ScriptObjectsByUniqueID)
			{
				if (item.Value is Invader && !list.Contains(item.Key))
				{
					list.Add(item.Key);
				}
			}
		}
		list.Sort();
		list.Insert(0, "");
		return list;
	}
}

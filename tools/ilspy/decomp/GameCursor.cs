using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameCursor : ActionMenu
{
	public struct EquipmentAmount : IComparable<EquipmentAmount>
	{
		public TileObject Owner;

		public EquipmentPrototype Proto;

		public LiquidPrototype Liquid;

		public float Amount;

		public EquipmentAmount(TileObject owner, EquipmentPrototype proto, LiquidPrototype liquid)
		{
			Owner = owner;
			Proto = proto;
			Liquid = liquid;
			if (Proto != null)
			{
				Amount = owner.GetInventory().CountItemsOfType(Proto);
			}
			else if (Liquid != null)
			{
				Amount = owner.GetInventory().GetTotalLiquid(Liquid);
			}
			else
			{
				Amount = 0f;
			}
		}

		public int CompareTo(EquipmentAmount other)
		{
			if (Amount < other.Amount)
			{
				return -1;
			}
			if (Amount > other.Amount)
			{
				return 1;
			}
			return 0;
		}
	}

	private CursorState State = CursorState.Select;

	private bool Forbidden;

	public BaseObject LastTarget;

	public Building LastInsideBuilding;

	private List<SpeechWithCachedPriority> SpeechOptions = new List<SpeechWithCachedPriority>();

	private List<string> SpeechText = new List<string>();

	public Speech LastSpeechToReplyTo;

	public BaseObject LastSpeechObject;

	public MemoryParam LastSpeechParam;

	public string LastSpeechToReplyToText;

	public Character Speaker;

	public Character Listener;

	public Character CanSkipConversationWith;

	public bool ShowGiftMenu;

	public CraftingProp ShowCraftMenuFor;

	public CraftingProp ShowDisassembleMenuFor;

	public bool WantRefreshSpeechOptions;

	public float ShowEquipmentSelectTimer;

	public float EquipmentSelectWasRecentlyHiddenTimer;

	public bool IsPlacingBuilding;

	public bool IsPlacingBuildingThisFrame;

	public bool WasInFlyModeBeforePlacingBuilding;

	public Recipe IsPlacingBuildingRecipe;

	public Equipment IsPlacingBuildingUsingItem;

	public TileObject GhostBuilding;

	public Prop.OrientationType GhostBuildingOrientation;

	public bool PlacingBuildingFailed;

	public PropPrototype IsSettingCropPatch;

	public bool SetingCropPatchFailed;

	public bool WasInFlyModeBeforeSettingCropPatch;

	public static Texture2D CursorPointer;

	public static Resource<Texture2D> CursorLink;

	public static Resource<Texture2D> CursorDrag;

	public static Resource<Texture2D> CursorSelect;

	public static Resource<Texture2D> CursorGoTo;

	public static Resource<Texture2D> CursorMove;

	public static Resource<Texture2D> CursorEnterBuilding;

	public static Resource<Texture2D> CursorUseWateringCan;

	public static Resource<Texture2D> CursorPlant;

	public static Resource<Texture2D> CursorGather;

	public static Resource<Texture2D> CursorEat;

	public static Resource<Texture2D> CursorToilet;

	public static Resource<Texture2D> CursorWaterBottle;

	public static Resource<Texture2D> CursorSleep;

	public static Resource<Texture2D> CursorGuard;

	public static Resource<Texture2D> CursorAttack;

	public static Resource<Texture2D> CursorBandages;

	public static Resource<Texture2D> CursorClothing;

	public static Resource<Texture2D> CursorMedication;

	public static Resource<Texture2D> CursorAmmo;

	public static Resource<Texture2D> CursorTake;

	public static Resource<Texture2D> CursorBuild;

	public static Resource<Texture2D> CursorFlag;

	public static Resource<Texture2D> CursorRepair;

	public static Resource<Texture2D> CursorDig;

	public static Resource<Texture2D> CursorCraft;

	public static Resource<Texture2D> CursorCook;

	public static Resource<Texture2D> CursorAxe;

	public static Resource<Texture2D> CursorPickaxe;

	public static Resource<Texture2D> CursorCampfire;

	public static Resource<Texture2D> CursorShelter;

	public static Resource<Texture2D> CursorSpeak;

	public static Resource<Texture2D> CursorKnife;

	public static Resource<Texture2D> CursorRestrain;

	public static Resource<Texture2D> CursorDoor;

	public static Resource<Texture2D> CursorKnockOut;

	public static Resource<Texture2D> CursorZone;

	public static Resource<Texture2D> CursorOrganize;

	public static Resource<Texture2D> DepressedIcon;

	public static Resource<Texture2D> AlertIcon;

	public static Resource<Texture2D> SkillIcon;

	public static Resource<Texture2D> SkillIconSmall;

	public static Resource<Texture2D> SkillUnknownIcon;

	public static Resource<Texture2D> CamouflageIcon;

	public static Resource<Texture2D> NoseIcon;

	public static Resource<Texture2D> EarIcon;

	public static Resource<Texture2D> EyeIcon;

	public static Resource<Texture2D> CrossIcon;

	public static Resource<Texture2D> CurrentQuestIcon;

	public static Resource<Texture2D> CurrentQuestIconCharacter;

	public static Resource<Texture2D> CompletedQuestIcon;

	public static Resource<Texture2D> FailedQuestIcon;

	public static Resource<Texture2D> ExitArrow;

	public static Resource<Texture2D> BloodLossIcon;

	public static Resource<Texture2D> BiohazardIcon;

	public static Resource<Texture2D> TownIcon;

	public static Resource<Texture2D> PopulatedIcon;

	public static Resource<Texture2D> InvisibleStrainIcon;

	public static Resource<Texture2D> FrostIcon;

	public static Resource<Texture2D> HelmetIcon;

	public static Resource<Texture2D> KevlarIcon;

	public static Resource<Texture2D> LegArmorIcon;

	public static Resource<Texture2D> TrapperIcon;

	public static Resource<Texture2D> EggIcon;

	public static Resource<Texture2D> ChickenIcon;

	public static Resource<Texture2D> DownTimeIcon;

	public static Resource<Texture2D> SmallDialogStudio;

	public static Resource<Texture2D> WhiteTex;

	private static List<SpeechParamResult> _paramResults = new List<SpeechParamResult>();

	private static float EquipmentSelectTimeout = 2f;

	public float CooldownSinceEnteredBuilding;

	private static int HUD_SelectionGroupCreatedMultiple = StringUtil.JenkinsHash("HUD_SelectionGroupCreatedMultiple");

	private static int HUD_SelectionGroupCreatedOneOther = StringUtil.JenkinsHash("HUD_SelectionGroupCreatedOneOther");

	private static int HUD_SelectionGroupCreatedSingle = StringUtil.JenkinsHash("HUD_SelectionGroupCreatedSingle");

	private int MeleeWeaponShortcutCount;

	private int RangedWeaponShortcutCount;

	private int ThrowableWeaponShortcutCount;

	private float LastWeaponShortcutTime;

	private InputFunction PressedGroupSelect = InputFunction.Invalid;

	private float PressedGroupSelectTime;

	private bool SentIsNavigatingMenus;

	private int SentMarkInvestigated;

	private static StringBuilder sb = new StringBuilder(50);

	public static int HUD_Lie = StringUtil.JenkinsHash("HUD_Lie");

	public static int HUD_Reset = StringUtil.JenkinsHash("HUD_Reset");

	public static List<EquipmentPrototype> GiftProtos = new List<EquipmentPrototype>();

	public static List<LiquidPrototype> GiftLiquids = new List<LiquidPrototype>();

	private static List<PropPrototype> AvailableCropTypes = new List<PropPrototype>();

	private static List<EquipmentPrototype> AvailableAmmoTypes = new List<EquipmentPrototype>();

	public static List<EquipmentAmount> EquipmentAmounts = new List<EquipmentAmount>();

	private static List<AvailableAction> VisibleButUnavailableActions = new List<AvailableAction>();

	private static float SkillTooltipOffset = -140f;

	private static float SkillTooltipWidth = 600f;

	public static float HitTheRoadRange = 16f;

	private static float CropPatchAlpha = 0.5f;

	public static bool DrawAllCropPatches = false;

	private static float ExitArrowScale = 1.5f;

	private static float ExitArrowYOffset = 0.25f;

	private static float ExitArrowHorizOffset = 1f;

	private static float GateInsideTextOffset = -1.5f;

	private static float GateOutsideTextOffset = 3.5f;

	public static float MaxTerrainHeightVariationForBuilding = 2f;

	private static List<TileObject> ObjectsInRect = new List<TileObject>();

	private static List<TileObject> ObjectsInRectThread = new List<TileObject>();

	private static float FuelRequiredToStartFireInFlOz = 1f;

	private static int[] TempSkillLevel = new int[10];

	private static float CanSeeTakeAllInFlyModeDist = 2f;

	private static List<Character> _nearbyCharacters = new List<Character>();

	private static float OnScreenDist = 16f;

	public static bool CachedIsAnyoneOnScreenInCombat;

	public static void LoadContent()
	{
		CursorLink = new Resource<Texture2D>("Textures/HUD/CursorLink");
		CursorDrag = new Resource<Texture2D>("Textures/HUD/CursorDrag");
		CursorSelect = new Resource<Texture2D>("Textures/HUD/CursorSelect");
		CursorGoTo = new Resource<Texture2D>("Textures/HUD/CursorGoTo");
		CursorMove = new Resource<Texture2D>("Textures/HUD/CursorMove");
		CursorEnterBuilding = new Resource<Texture2D>("Textures/HUD/CursorEnterBuilding");
		CursorUseWateringCan = new Resource<Texture2D>("Textures\\HUD\\CursorUseWateringCan");
		CursorPlant = new Resource<Texture2D>("Textures\\HUD\\CursorPlant");
		CursorGather = new Resource<Texture2D>("Textures\\HUD\\CursorGather");
		CursorEat = new Resource<Texture2D>("Textures\\HUD\\CursorEat");
		CursorToilet = new Resource<Texture2D>("Textures\\HUD\\CursorToilet");
		CursorWaterBottle = new Resource<Texture2D>("Textures\\HUD\\CursorWaterBottle");
		CursorSleep = new Resource<Texture2D>("Textures\\HUD\\CursorSleep");
		CursorGuard = new Resource<Texture2D>("Textures\\HUD\\CursorGuard");
		CursorAttack = new Resource<Texture2D>("Textures\\HUD\\CursorAttack");
		CursorBandages = new Resource<Texture2D>("Textures\\HUD\\CursorBandages");
		CursorClothing = new Resource<Texture2D>("Textures\\HUD\\CursorClothing");
		CursorMedication = new Resource<Texture2D>("Textures\\HUD\\CursorMedication");
		CursorAmmo = new Resource<Texture2D>("Textures\\HUD\\CursorAmmo");
		CursorTake = new Resource<Texture2D>("Textures\\HUD\\CursorTake");
		CursorBuild = new Resource<Texture2D>("Textures\\HUD\\CursorBuild");
		CursorRepair = new Resource<Texture2D>("Textures\\HUD\\CursorRepair");
		CursorFlag = new Resource<Texture2D>("Textures\\HUD\\CursorFlag");
		CursorDig = new Resource<Texture2D>("Textures\\HUD\\CursorShovel");
		CursorCraft = new Resource<Texture2D>("Textures\\HUD\\CursorCraft");
		CursorCook = new Resource<Texture2D>("Textures\\HUD\\CursorCook");
		CursorAxe = new Resource<Texture2D>("Textures\\HUD\\CursorAxe");
		CursorPickaxe = new Resource<Texture2D>("Textures\\HUD\\CursorPickaxe");
		CursorCampfire = new Resource<Texture2D>("Textures\\HUD\\CursorCampfire");
		CursorShelter = new Resource<Texture2D>("Textures\\HUD\\CursorShelter");
		CursorSpeak = new Resource<Texture2D>("Textures\\HUD\\CursorSpeak");
		CursorKnife = new Resource<Texture2D>("Textures\\HUD\\CursorKnife");
		CursorRestrain = new Resource<Texture2D>("Textures\\HUD\\Restrain");
		CursorDoor = new Resource<Texture2D>("Textures\\HUD\\CursorDoor");
		CursorKnockOut = new Resource<Texture2D>("Textures\\HUD\\KnockedOut");
		CursorZone = new Resource<Texture2D>("Textures\\HUD\\CursorZone");
		CursorOrganize = new Resource<Texture2D>("Textures\\HUD\\CursorOrganize");
		DepressedIcon = new Resource<Texture2D>("Textures\\HUD\\DepressedIcon");
		AlertIcon = new Resource<Texture2D>("Textures\\HUD\\Alert");
		SkillIcon = new Resource<Texture2D>("Textures\\HUD\\SkillIcon");
		SkillIconSmall = new Resource<Texture2D>("Textures\\HUD\\SkillIconSmall");
		SkillUnknownIcon = new Resource<Texture2D>("Textures\\HUD\\SkillUnknownIcon");
		CamouflageIcon = new Resource<Texture2D>("Textures\\HUD\\Camouflage");
		NoseIcon = new Resource<Texture2D>("Textures\\HUD\\Nose");
		EarIcon = new Resource<Texture2D>("Textures\\HUD\\Ear");
		EyeIcon = new Resource<Texture2D>("Textures\\HUD\\Eye");
		CrossIcon = new Resource<Texture2D>("Textures\\HUD\\CrossNoOutline");
		CurrentQuestIcon = new Resource<Texture2D>("Textures\\HUD\\QuestIcon");
		CurrentQuestIconCharacter = new Resource<Texture2D>("Textures\\HUD\\QuestIconCharacter");
		CompletedQuestIcon = new Resource<Texture2D>("Textures\\HUD\\Tick");
		FailedQuestIcon = new Resource<Texture2D>("Textures\\HUD\\Cross");
		ExitArrow = new Resource<Texture2D>("Textures\\HUD\\ExitArrow");
		BloodLossIcon = new Resource<Texture2D>("Textures\\HUD\\BloodLoss");
		BiohazardIcon = new Resource<Texture2D>("Textures\\HUD\\Biohazard");
		TownIcon = new Resource<Texture2D>("Textures\\HUD\\Town");
		PopulatedIcon = new Resource<Texture2D>("Textures\\HUD\\Populated");
		InvisibleStrainIcon = new Resource<Texture2D>("Textures\\HUD\\InvisibleStrain");
		FrostIcon = new Resource<Texture2D>("Textures\\HUD\\Frost");
		HelmetIcon = new Resource<Texture2D>("Textures\\HUD\\helmet");
		KevlarIcon = new Resource<Texture2D>("Textures\\HUD\\kevlar");
		LegArmorIcon = new Resource<Texture2D>("Textures\\HUD\\LegArmor");
		TrapperIcon = new Resource<Texture2D>("Textures\\HUD\\Trap");
		EggIcon = new Resource<Texture2D>("Textures\\HUD\\EggIcon");
		ChickenIcon = new Resource<Texture2D>("Textures\\HUD\\Chicken");
		DownTimeIcon = new Resource<Texture2D>("Textures\\HUD\\DownTime");
		SmallDialogStudio = new Resource<Texture2D>("Textures\\HUD\\SmallDialogStudio");
		WhiteTex = new Resource<Texture2D>("Textures\\HUD\\White");
	}

	public CursorState GetCursorState()
	{
		return State;
	}

	public Texture2D GetCursorTexture(out float angle, out Vector2 offset, out CursorLockMode wantCursorLockMode, out bool hovering, out bool forbidden, out Color cursorCol)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		InfoScreen instance3 = InfoScreen.Instance;
		if (instance2.IsDebugMenuOpen() || instance.IsMenuOpen() || instance.IsDialogOpen() || instance3.Active)
		{
			wantCursorLockMode = CursorLockMode.None;
		}
		else if (InputFunctionManager.Instance.CurrentInputType != InputType.MouseAndKeyboard && !Hud.Instance.Dragging)
		{
			wantCursorLockMode = CursorLockMode.Locked;
		}
		else
		{
			wantCursorLockMode = CursorLockMode.Confined;
		}
		angle = 0f;
		offset = Vector3.zero;
		hovering = false;
		forbidden = Forbidden;
		cursorCol = Color.white;
		switch (State)
		{
		case CursorState.Pointer:
			return CursorPointer;
		case CursorState.Link:
			return CursorLink;
		case CursorState.Drag:
			return CursorDrag;
		case CursorState.DirectControl:
			wantCursorLockMode = CursorLockMode.Locked;
			return null;
		case CursorState.Select:
			return CursorSelect;
		case CursorState.SelectHover:
			hovering = true;
			return CursorSelect;
		case CursorState.GoTo:
			return CursorGoTo;
		case CursorState.SetZone:
			return CursorZone;
		case CursorState.EnterBuilding:
			return CursorEnterBuilding;
		case CursorState.Build:
			return CursorBuild;
		case CursorState.Repair:
			return CursorRepair;
		case CursorState.Capture:
			return CursorFlag;
		case CursorState.SetCropsPatch:
			if (IsSettingCropPatch != null)
			{
				cursorCol = IsSettingCropPatch.Color;
			}
			return CursorPlant;
		case CursorState.Take:
			return CursorTake;
		case CursorState.Craft:
			return CursorCraft;
		case CursorState.Cook:
			return CursorCook;
		case CursorState.Farm:
			return CursorPlant;
		case CursorState.Lumberjack:
			return CursorAxe;
		case CursorState.Mine:
			return CursorPickaxe;
		case CursorState.Trapper:
			return TrapperIcon;
		case CursorState.AnimalFeeder:
			return ChickenIcon;
		case CursorState.Gather:
			return CursorGather;
		case CursorState.Guard:
			return CursorGuard;
		case CursorState.Attack:
			return CursorAttack;
		case CursorState.WateringCan:
			return CursorUseWateringCan;
		case CursorState.WaterBottle:
			return CursorWaterBottle;
		case CursorState.Bandage:
			return CursorBandages;
		case CursorState.Inject:
			return CursorMedication;
		case CursorState.Eat:
			return CursorEat;
		case CursorState.Dig:
			return CursorDig;
		case CursorState.Campfire:
			return CursorCampfire;
		case CursorState.Speak:
			return CursorSpeak;
		case CursorState.Knife:
			return CursorKnife;
		case CursorState.Restrain:
			return CursorRestrain;
		case CursorState.Door:
			return CursorDoor;
		case CursorState.KnockOut:
			return CursorKnockOut;
		case CursorState.MoveLeft:
			angle = -MathF.PI / 2f;
			offset = new Vector2(0.5f, 0f);
			return CursorMove;
		case CursorState.MoveRight:
			angle = MathF.PI / 2f;
			offset = new Vector2(-0.5f, 0f);
			return CursorMove;
		case CursorState.MoveUp:
			angle = MathF.PI;
			offset = new Vector2(0f, -0.5f);
			return CursorMove;
		case CursorState.MoveDown:
			angle = 0f;
			offset = new Vector2(0f, 0.5f);
			return CursorMove;
		case CursorState.MoveUpLeft:
			angle = MathF.PI * -3f / 4f;
			offset = new Vector2(0.353f, -0.353f);
			return CursorMove;
		case CursorState.MoveUpRight:
			angle = MathF.PI * 3f / 4f;
			offset = new Vector2(-0.353f, -0.353f);
			return CursorMove;
		case CursorState.MoveDownLeft:
			angle = -MathF.PI / 4f;
			offset = new Vector2(0.353f, 0.353f);
			return CursorMove;
		case CursorState.MoveDownRight:
			angle = MathF.PI / 4f;
			offset = new Vector2(-0.353f, 0.353f);
			return CursorMove;
		default:
			return CursorPointer;
		}
	}

	public void ShowEquipmentSelect()
	{
		ShowEquipmentSelectTimer = EquipmentSelectTimeout;
	}

	public void HideEquipmentSelect()
	{
		ShowEquipmentSelectTimer = 0f;
		SelectedAction = 0;
	}

	public void HandleInput(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		InfoScreen instance2 = InfoScreen.Instance;
		Session instance3 = Session.Instance;
		Hud instance4 = Hud.Instance;
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		CooldownSinceEnteredBuilding = Math.Max(0f, CooldownSinceEnteredBuilding - GameImpl.UnscaledDeltaTime);
		if (IsPlacingBuilding)
		{
			float justPressedAxis = instance.GetJustPressedAxis(InputFunction.RotateBuilding, ButtonPromptBarBehaviour.PROMPT_RotateBuilding);
			if (justPressedAxis != 0f)
			{
				if (GhostBuilding is Prop)
				{
					if (justPressedAxis < 0.5f)
					{
						GhostBuildingOrientation = (Prop.OrientationType)((int)(GhostBuildingOrientation + 4 - 1) % 4);
					}
					if (justPressedAxis > 0.5f)
					{
						GhostBuildingOrientation = (Prop.OrientationType)((int)(GhostBuildingOrientation + 1) % 4);
					}
				}
				SoundManager.PlayMenuSound(SoundManager.MoveSelectSound);
			}
			if (inputFrame != null && instance.IsPressed(InputFunction.MainAction))
			{
				if (IsCursorActionEnabled())
				{
					PlaceBuilding(localControlledCharacter, inputFrame);
				}
				else if (!PlacingBuildingFailed)
				{
					SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
					HudBehaviour.Instance.SetStatusBarMsg(GetAvailableAction());
				}
				PlacingBuildingFailed = true;
			}
			else
			{
				PlacingBuildingFailed = false;
			}
			TileObject underConstructionObjectOnTile = GameTerrain.Instance.GetUnderConstructionObjectOnTile(instance4.CursorRayCastResult.Tile.x, instance4.CursorRayCastResult.Tile.y);
			if (instance4.LocalControlledCharacter != null && underConstructionObjectOnTile != null && underConstructionObjectOnTile.GetUnderConstructionInfo() != null && underConstructionObjectOnTile.GetCommunity() == instance4.LocalControlledCharacter.Community && inputFrame != null && instance.IsPressed(InputFunction.Clear, capture: true, ButtonPromptBarBehaviour.PROMPT_Clear))
			{
				inputFrame.AddAction(InputAction.Demolish(underConstructionObjectOnTile));
			}
			if (instance.IsJustPressed(InputFunction.Back, capture: true, (instance4.LocalControlledCharacter.Community.UnderConstructionBuildings.Count > 0) ? ButtonPromptBarBehaviour.PROMPT_Finished : ButtonPromptBarBehaviour.PROMPT_Cancel))
			{
				StopPlacingBuilding();
				if (!WasInFlyModeBeforePlacingBuilding)
				{
					instance3.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
				}
			}
		}
		if (IsSettingCropPatch == null && !IsPlacingBuilding && CanSelectEquipment(localControlledCharacter))
		{
			int buttonPromptHash = ((FindAvailableActionByType(CursorAction.Unequip) != -1 && InputFunctionManager.Instance.IsMapped(InputFunction.ShowEquipmentSelector)) ? ButtonPromptBarBehaviour.PROMPT_ShowEquipmentSelector : 0);
			if (instance.IsJustPressed(InputFunction.ShowEquipmentSelector, capture: true, buttonPromptHash))
			{
				if (ShowEquipmentSelectTimer > 0f)
				{
					HideEquipmentSelect();
					ResetShortcutTimer();
				}
				else
				{
					ShowEquipmentSelect();
				}
			}
		}
		if ((ShowGiftMenu || ShowCraftMenuFor != null || ShowDisassembleMenuFor != null) && instance.IsJustPressed(InputFunction.Back, capture: true, ButtonPromptBarBehaviour.PROMPT_Cancel))
		{
			BackOutOfSubMenu();
		}
		AvailableAction availableAction = GetAvailableAction();
		GetAvailableActions(out var outTarget, out var wantTargetHeader);
		if (wantTargetHeader && HeaderActions.Count == 0)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.DisplayNameAndIcon, null, outTarget, CursorActionDisabledReason.Enabled));
			if (outTarget is PlantableCrop)
			{
				HeaderActions.Add(new AvailableAction(CursorAction.CropRating, null, outTarget, CursorActionDisabledReason.Enabled));
			}
			if (AvailableActions.Count > 0)
			{
				HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
			}
		}
		OnFinishAddingActions();
		if (IsSettingCropPatch != null && instance3.GameCamera.FlyCamTransition >= 1f)
		{
			EquipmentPrototype proto = GetAvailableAction().Proto;
			if (proto != null && IsSettingCropPatch != proto.GetSeedForPlantType())
			{
				IsSettingCropPatch = proto.GetSeedForPlantType();
				SetingCropPatchFailed = false;
			}
			if (inputFrame != null && instance.IsPressed(InputFunction.MainAction, capture: true, ButtonPromptBarBehaviour.PROMPT_SetCropsPatchHere))
			{
				if (IsCursorActionEnabled())
				{
					SetingCropPatchFailed = false;
					inputFrame.AddAction(InputAction.SetCropsPatch(instance4.CursorRayCastResult.Tile, IsSettingCropPatch));
				}
				else if (!SetingCropPatchFailed)
				{
					SetingCropPatchFailed = true;
					SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
					HudBehaviour.Instance.SetStatusBarMsg(GetAvailableAction());
				}
			}
			if (instance3.CropsManager.GetPatch(instance4.CursorRayCastResult.Tile, instance4.LocalControlledCharacter.GetCommunityId()) != null && inputFrame != null && instance.IsPressed(InputFunction.Clear, capture: true, ButtonPromptBarBehaviour.PROMPT_Clear))
			{
				inputFrame.AddAction(InputAction.SetCropsPatch(instance4.CursorRayCastResult.Tile, null));
			}
			if (instance.IsJustPressed(InputFunction.Back, capture: true, ButtonPromptBarBehaviour.PROMPT_Finished))
			{
				IsSettingCropPatch = null;
				if (!WasInFlyModeBeforeSettingCropPatch)
				{
					instance3.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
				}
			}
		}
		Building building = localControlledCharacter?.InsideBuilding;
		if ((outTarget != LastTarget || building != LastInsideBuilding) && !IsPlacingBuilding && IsSettingCropPatch == null && !availableAction.IsEqual(GetAvailableAction()))
		{
			SelectedAction = GetFirstSelectableActionIndex();
			LastTarget = outTarget;
			LastInsideBuilding = building;
		}
		else if (AvailableActions.Count > 0)
		{
			SelectedAction = Math.Min(SelectedAction, AvailableActions.Count - 1);
			while (!AvailableActions[SelectedAction].IsSelectable() && SelectedAction < AvailableActions.Count - 1)
			{
				SelectedAction++;
			}
			if (AvailableActions[SelectedAction].ActionType != availableAction.ActionType)
			{
				if (availableAction.ActionType == CursorAction.SetCropsPatch && AvailableActions[SelectedAction].ActionType == CursorAction.SetCropsPatchHere)
				{
					for (int i = 0; i < AvailableActions.Count; i++)
					{
						if (AvailableActions[i].ActionType == CursorAction.SetCropsPatchHere && AvailableActions[i].Proto == availableAction.Proto && AvailableActions[i].IsSelectable())
						{
							SelectedAction = i;
							break;
						}
					}
				}
				else
				{
					for (int j = 0; j < AvailableActions.Count; j++)
					{
						if (AvailableActions[j].ActionType == availableAction.ActionType && AvailableActions[j].IsSelectable())
						{
							SelectedAction = j;
							break;
						}
					}
				}
			}
		}
		else
		{
			SelectedAction = 0;
		}
		if (AvailableActions.Count > 1)
		{
			float num = 0f;
			float axis = instance.GetAxis(InputFunction.SelectAction);
			if (axis != 0f)
			{
				float num2 = (instance.IsMappedToMouseWheel(InputFunction.SelectAction) ? 0f : (((DPadHeldDown == 0) ? 0f : ((DPadHeldDown == 1) ? 10f : 4f)) / 60f));
				if (GameImpl.UnscaledTime - LastDPadTime >= num2)
				{
					num += axis;
					LastDPadTime = GameImpl.UnscaledTime;
					DPadHeldDown++;
				}
			}
			else
			{
				DPadHeldDown = 0;
			}
			if (num < 0f)
			{
				while (SelectedAction < AvailableActions.Count - 1)
				{
					SelectedAction++;
					if (GetAvailableAction().IsSelectable())
					{
						break;
					}
				}
			}
			else if (num > 0f)
			{
				while (SelectedAction > GetFirstSelectableActionIndex())
				{
					SelectedAction--;
					if (GetAvailableAction().IsSelectable())
					{
						break;
					}
				}
			}
			if (!instance3.GameCamera.FlyCam)
			{
				switch (GetCursorAction())
				{
				case CursorAction.FollowMe:
				case CursorAction.StopFollowingMe:
				case CursorAction.TalkTo:
				case CursorAction.SpeechToReplyTo:
				case CursorAction.OpenGiftMenu:
				case CursorAction.Gift:
				case CursorAction.Back:
					if (inputFrame != null && Speaker != null && Listener != null && (!Speaker.IsCrouching() || Listener.IsFullyTracked(Speaker)) && LastSpeechToReplyTo == null)
					{
						Squad squad = Listener.GetSquad();
						if (squad == null || !squad.MovedAlong)
						{
							inputFrame.AddAction(InputAction.SetRecentActivityTalkToMe(Listener));
						}
					}
					break;
				}
			}
		}
		EquipmentSelectWasRecentlyHiddenTimer = Math.Max(EquipmentSelectWasRecentlyHiddenTimer - GameImpl.DeltaTime, 0f);
		if (ShowEquipmentSelectTimer > 0f)
		{
			if (!CanSelectEquipment(localControlledCharacter))
			{
				HideEquipmentSelect();
			}
			else if (AvailableActions.Count > 0)
			{
				HideEquipmentSelect();
			}
			else
			{
				ShowEquipmentSelectTimer -= GameImpl.DeltaTime;
				if (ShowEquipmentSelectTimer <= 0f)
				{
					ShowEquipmentSelectTimer = 0f;
					EquipmentSelectWasRecentlyHiddenTimer = 1f;
				}
				if (localControlledCharacter != null)
				{
					List<EquipmentContainer.Equippable> list = localControlledCharacter.Inventory.BuildEquippableList(localControlledCharacter, localControlledCharacter.DesiredEquippedItem, Hud.Instance.LocalPressingAim, takeAll: false, out SelectedAction);
					Equipment bestMeleeWeaponForShortcut = localControlledCharacter.GetBestMeleeWeaponForShortcut();
					EquipmentPrototype ammoType;
					InfectionType infectedWith;
					Equipment bestAmmoWeaponForShortcut = localControlledCharacter.GetBestAmmoWeaponForShortcut(out ammoType, out infectedWith);
					Equipment bestThrowableForShortcut = localControlledCharacter.GetBestThrowableForShortcut();
					for (int k = 0; k < list.Count; k++)
					{
						InputFunction shortcut = InputFunction.Invalid;
						if (list[k].Item != null)
						{
							if (list[k].Item == bestMeleeWeaponForShortcut)
							{
								shortcut = InputFunction.SelectMeleeWeapon;
							}
							if (list[k].Item == bestAmmoWeaponForShortcut && list[k].AmmoType == ammoType && list[k].InfectedWith == infectedWith)
							{
								shortcut = InputFunction.SelectRangedWeapon;
							}
							if (list[k].Item == bestThrowableForShortcut)
							{
								shortcut = InputFunction.SelectThrowingWeapon;
							}
						}
						AvailableActions.Add(new AvailableAction(CursorAction.EquipmentSelect, localControlledCharacter, list[k].Item, list[k].AmmoType, list[k].InfectedWith, list[k].Amount, CursorActionDisabledReason.Enabled, shortcut));
					}
				}
			}
		}
		if (inputFrame != null && !GameImpl.Instance.IsDialogOpen())
		{
			if (instance.IsJustPressed(InputFunction.SelectMeleeWeapon))
			{
				EquipmentPrototype resultAmmoType;
				InfectionType resultInfectedWith;
				Equipment bestWeaponForShortcut = localControlledCharacter.Inventory.GetBestWeaponForShortcut(localControlledCharacter, typeof(MeleeWeapon), MeleeWeaponShortcutCount, out resultAmmoType, out resultInfectedWith);
				if (bestWeaponForShortcut != null)
				{
					inputFrame.AddAction(InputAction.SetDesiredWeapon(bestWeaponForShortcut, resultAmmoType, resultInfectedWith));
					MeleeWeaponShortcutCount++;
					RangedWeaponShortcutCount = 0;
					ThrowableWeaponShortcutCount = 0;
					LastWeaponShortcutTime = GameImpl.UnscaledTime;
					if (ShowEquipmentSelectTimer > 0f)
					{
						ShowEquipmentSelect();
					}
				}
			}
			if (instance.IsJustPressed(InputFunction.SelectRangedWeapon))
			{
				EquipmentPrototype resultAmmoType2;
				InfectionType resultInfectedWith2;
				Equipment bestWeaponForShortcut2 = localControlledCharacter.Inventory.GetBestWeaponForShortcut(localControlledCharacter, typeof(AmmoWeapon), RangedWeaponShortcutCount, out resultAmmoType2, out resultInfectedWith2);
				if (bestWeaponForShortcut2 != null)
				{
					inputFrame.AddAction(InputAction.SetDesiredWeapon(bestWeaponForShortcut2, resultAmmoType2, resultInfectedWith2));
					MeleeWeaponShortcutCount = 0;
					RangedWeaponShortcutCount++;
					ThrowableWeaponShortcutCount = 0;
					LastWeaponShortcutTime = GameImpl.UnscaledTime;
					if (ShowEquipmentSelectTimer > 0f)
					{
						ShowEquipmentSelect();
					}
				}
			}
			if (instance.IsJustPressed(InputFunction.SelectThrowingWeapon))
			{
				EquipmentPrototype resultAmmoType3;
				InfectionType resultInfectedWith3;
				Equipment bestWeaponForShortcut3 = localControlledCharacter.Inventory.GetBestWeaponForShortcut(localControlledCharacter, typeof(Throwable), ThrowableWeaponShortcutCount, out resultAmmoType3, out resultInfectedWith3);
				if (bestWeaponForShortcut3 != null)
				{
					inputFrame.AddAction(InputAction.SetDesiredWeapon(bestWeaponForShortcut3, resultAmmoType3, resultInfectedWith3));
					MeleeWeaponShortcutCount = 0;
					RangedWeaponShortcutCount = 0;
					ThrowableWeaponShortcutCount++;
					LastWeaponShortcutTime = GameImpl.UnscaledTime;
					if (ShowEquipmentSelectTimer > 0f)
					{
						ShowEquipmentSelect();
					}
				}
			}
		}
		if (LastWeaponShortcutTime > 0f && GameImpl.UnscaledTime - LastWeaponShortcutTime > 4f)
		{
			ResetShortcutTimer();
		}
		if (inputFrame != null && !GameImpl.Instance.IsDialogOpen())
		{
			if (PressedGroupSelect == InputFunction.Invalid)
			{
				for (int l = 67; l <= 76; l++)
				{
					InputFunction inputFunction = (InputFunction)l;
					if (instance.IsJustPressed(inputFunction, capture: false))
					{
						PressedGroupSelect = inputFunction;
						PressedGroupSelectTime = GameImpl.UnscaledTime;
					}
					instance.Capture(inputFunction, untilReleased: false);
				}
			}
			else if (GameImpl.UnscaledTime - PressedGroupSelectTime >= 1f)
			{
				inputFrame.AddAction(InputAction.SetShortcutGroup((int)(PressedGroupSelect - 67), instance4.LocalControlledCharacter, instance4.SelectedCharacters));
				string empty = string.Empty;
				int num3 = instance4.SelectedCharacters.Count - (instance4.SelectedCharacters.Contains(instance4.LocalControlledCharacter) ? 1 : 0);
				if (num3 == 1 && GameImpl.HasTranslationForHash(HUD_SelectionGroupCreatedOneOther, englishOnly: false))
				{
					empty = GameImpl.Translate(HUD_SelectionGroupCreatedOneOther);
				}
				else if (num3 > 0)
				{
					empty = GameImpl.Translate(HUD_SelectionGroupCreatedMultiple);
					empty = empty.Replace("%3", num3.ToString());
				}
				else
				{
					empty = GameImpl.Translate(HUD_SelectionGroupCreatedSingle);
				}
				empty = empty.Replace("%1", StringUtil.GetButtonPromptString(PressedGroupSelect));
				empty = empty.Replace("%2", instance4.LocalControlledCharacter.GetDisplayNameString());
				HudBehaviour.Instance.SetStatusBarMsg(empty);
				PressedGroupSelect = InputFunction.Invalid;
				HintManager.Instance.Hints[30].MarkPerformed();
			}
			else if (!instance.IsPressed(PressedGroupSelect))
			{
				ShortcutGroup shortcutGroup = instance3.GetLocalPlayerRecord().GetShortcutGroup((int)(PressedGroupSelect - 67));
				if (shortcutGroup != null)
				{
					if (shortcutGroup.ControlledCharacter != null && shortcutGroup.ControlledCharacter.IsControllableByPlayer())
					{
						instance4.SetLocalControlledCharacter(shortcutGroup.ControlledCharacter, inputFrame);
					}
					if (!instance.IsPressed(InputFunction.AddToSelection, capture: false))
					{
						for (int num4 = instance4.SelectedCharacters.Count - 1; num4 >= 0; num4--)
						{
							Character character = instance4.SelectedCharacters[num4];
							if (character != null && character.IsSelectable() && !shortcutGroup.SelectedCharacters.Contains(character))
							{
								instance4.DeselectCharacter(character, inputFrame);
							}
						}
					}
					foreach (Character selectedCharacter in shortcutGroup.SelectedCharacters)
					{
						if (selectedCharacter != null && selectedCharacter.IsSelectable() && !selectedCharacter.NonDeterministicSelected)
						{
							instance4.SelectCharacter(selectedCharacter, inputFrame);
						}
					}
					HudBehaviour.Instance.ShowCharacterSwitchBar();
					SoundManager.PlayMenuSound(SoundManager.TabSound);
				}
				PressedGroupSelect = InputFunction.Invalid;
			}
		}
		UpdateCursorState();
		if (instance2.Active && instance2.HideActionMenu && SelectedAction >= 0 && SelectedAction < AvailableActions.Count)
		{
			InfoPage infoPage = instance2.GetCurrentPage() as InfoPage;
			if (infoPage != null && infoPage.WantShowActionMenuOptionOnButtonPromptBar())
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.MainAction, AvailableActions[SelectedAction].GetCaption());
			}
		}
	}

	public void ResetShortcutTimer()
	{
		LastWeaponShortcutTime = 0f;
		MeleeWeaponShortcutCount = 0;
		RangedWeaponShortcutCount = 0;
		ThrowableWeaponShortcutCount = 0;
	}

	public void UpdateCursorState()
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		InfoScreen instance3 = InfoScreen.Instance;
		Hud instance4 = Hud.Instance;
		CursorState newState = CursorState.Pointer;
		bool forbidden = false;
		if (!instance2.IsDebugMenuOpen() && !instance.IsMenuOpen() && !instance.IsDialogOpen())
		{
			if (instance3.Active)
			{
				if (MapPage.Instance.Dragging)
				{
					newState = CursorState.Drag;
				}
				else if (AvailableActions.Count > 0 && SelectedAction < AvailableActions.Count)
				{
					switch (AvailableActions[SelectedAction].ActionType)
					{
					case CursorAction.SetMarker:
						newState = CursorState.Pointer;
						break;
					case CursorAction.ClearMarker:
						newState = CursorState.Link;
						break;
					case CursorAction.SetZone:
					case CursorAction.PauseZone:
					case CursorAction.ResumeZone:
					case CursorAction.ClearZone:
					case CursorAction.CopyZone:
						newState = CursorState.SetZone;
						break;
					case CursorAction.StopFollowingMe:
					case CursorAction.GroupFollowMe:
					case CursorAction.GroupStopFollowingMe:
					case CursorAction.SelectFollowers:
					case CursorAction.SelectEveryone:
					case CursorAction.SelectNoone:
					case CursorAction.SelectCharacter:
					case CursorAction.DeselectCharacter:
						newState = CursorState.SelectHover;
						break;
					default:
						newState = CursorState.Link;
						break;
					}
				}
				else if (MapPage.Instance.IsInRangeOfMarker() != -1)
				{
					newState = CursorState.Link;
				}
			}
			else if (instance2.GameCamera.FlyCam)
			{
				if (instance4.Dragging && instance4.ReallyDragging)
				{
					newState = CursorState.Drag;
				}
				else if (!GetScreenEdgeCursorState(ref newState) && AvailableActions.Count > 0)
				{
					AvailableAction availableAction = AvailableActions[SelectedAction];
					switch (availableAction.ActionType)
					{
					case CursorAction.GoTo:
						newState = CursorState.GoTo;
						break;
					case CursorAction.SetZone:
					case CursorAction.PauseZone:
					case CursorAction.ResumeZone:
					case CursorAction.ClearZone:
					case CursorAction.CopyZone:
						newState = CursorState.SetZone;
						break;
					case CursorAction.EnterBuilding:
					case CursorAction.EnterBuildingNotInvestigated:
						newState = CursorState.EnterBuilding;
						break;
					case CursorAction.BuildHere:
					case CursorAction.ResumeBuilding:
					case CursorAction.SetBuilderRole:
					case CursorAction.StopBuilding:
					case CursorAction.Demolish:
					case CursorAction.Abandon:
					case CursorAction.ResumeBuildingRole:
						newState = CursorState.Build;
						break;
					case CursorAction.Repair:
					case CursorAction.ResumeRepairing:
						newState = CursorState.Repair;
						break;
					case CursorAction.TakeOver:
					case CursorAction.ResumeCapturing:
						newState = CursorState.Capture;
						break;
					case CursorAction.SetCropsPatch:
					case CursorAction.SetCropsPatchHere:
						newState = CursorState.SetCropsPatch;
						break;
					case CursorAction.Control:
					case CursorAction.FollowMe:
					case CursorAction.StopFollowingMe:
					case CursorAction.GroupFollowMe:
					case CursorAction.GroupStopFollowingMe:
					case CursorAction.SelectFollowers:
					case CursorAction.SelectEveryone:
					case CursorAction.SelectNoone:
					case CursorAction.SelectCharacter:
					case CursorAction.DeselectCharacter:
					case CursorAction.ControlVehicle:
						newState = CursorState.SelectHover;
						break;
					case CursorAction.Scavenge:
					case CursorAction.Steal:
					case CursorAction.ScavengeEmpty:
					case CursorAction.StealEmpty:
					case CursorAction.ScavengeNotInvestigated:
					case CursorAction.StealNotInvestigated:
					case CursorAction.TakeAll:
					case CursorAction.StealAll:
					case CursorAction.PickUp:
					case CursorAction.PickUpSteal:
					case CursorAction.Drop:
					case CursorAction.PutInBuilding:
					case CursorAction.Grab:
					case CursorAction.GrabSteal:
					case CursorAction.Harvest:
					case CursorAction.StealCrops:
					case CursorAction.ClearCrops:
					case CursorAction.ScoopSnow:
					case CursorAction.Pickpocket:
						newState = CursorState.Take;
						break;
					case CursorAction.Cook:
					case CursorAction.CancelCook:
					case CursorAction.ResumeCook:
					case CursorAction.OpenCookMenu:
						newState = CursorState.Cook;
						break;
					case CursorAction.RepairArmor:
					case CursorAction.Craft:
					case CursorAction.ResumeCrafting:
						newState = ((availableAction.Recipe != null && availableAction.Recipe.IsProductDrinkableOrEdible()) ? CursorState.Cook : CursorState.Craft);
						break;
					case CursorAction.Skin:
					case CursorAction.ResumeCraftingRole:
					case CursorAction.OpenCraftMenu:
					case CursorAction.OpenDisassembleMenu:
						newState = CursorState.Craft;
						break;
					case CursorAction.ChopTree:
					case CursorAction.ChopLog:
					case CursorAction.ChopTreeStealing:
					case CursorAction.ChopLogStealing:
					case CursorAction.SetLumberjack:
					case CursorAction.ClearBush:
					case CursorAction.CancelLumberjack:
					case CursorAction.ResumeLumberjack:
					case CursorAction.PauseLumberjack:
						newState = CursorState.Lumberjack;
						break;
					case CursorAction.Mine:
					case CursorAction.MineStealing:
					case CursorAction.SetMiner:
					case CursorAction.CancelMining:
					case CursorAction.ResumeMining:
					case CursorAction.PauseMining:
						newState = CursorState.Mine;
						break;
					case CursorAction.Plant:
					case CursorAction.SetFarmer:
					case CursorAction.CancelFarming:
					case CursorAction.ResumeFarming:
					case CursorAction.PauseFarming:
						newState = CursorState.Farm;
						break;
					case CursorAction.Gather:
					case CursorAction.CancelGathering:
					case CursorAction.ResumeGathering:
					case CursorAction.PauseGathering:
						newState = CursorState.Gather;
						break;
					case CursorAction.Guard:
					case CursorAction.CancelGuarding:
					case CursorAction.ResumeGuarding:
					case CursorAction.PauseGuarding:
						newState = CursorState.Guard;
						break;
					case CursorAction.ResetTrap:
					case CursorAction.SetTrapper:
					case CursorAction.CancelTrapper:
					case CursorAction.ResumeTrapper:
					case CursorAction.PauseTrapper:
						newState = CursorState.Trapper;
						break;
					case CursorAction.SetAnimalFeeder:
					case CursorAction.CancelAnimalFeeding:
					case CursorAction.ResumeAnimalFeeding:
					case CursorAction.PauseAnimalFeeding:
						newState = CursorState.AnimalFeeder;
						break;
					case CursorAction.Attack:
						newState = CursorState.Attack;
						break;
					case CursorAction.WaterCrops:
						newState = CursorState.WateringCan;
						return;
					case CursorAction.GiveWater:
					case CursorAction.FillFromWell:
					case CursorAction.FillFromRiver:
					case CursorAction.FillFromSnow:
					case CursorAction.PourFuel:
						newState = CursorState.WaterBottle;
						break;
					case CursorAction.Feed:
						newState = CursorState.Eat;
						break;
					case CursorAction.Bandage:
					case CursorAction.SetMedic:
					case CursorAction.CancelMedic:
					case CursorAction.ResumeMedic:
					case CursorAction.PauseMedic:
						newState = CursorState.Bandage;
						break;
					case CursorAction.Inject:
						newState = CursorState.Inject;
						break;
					case CursorAction.Bury:
					case CursorAction.Dig:
						newState = CursorState.Dig;
						break;
					case CursorAction.LightFire:
					case CursorAction.LightFireWithMatch:
					case CursorAction.LightFireWithFlint:
					case CursorAction.AddMaterialToFire:
						newState = CursorState.Campfire;
						break;
					case CursorAction.TalkTo:
					case CursorAction.SpeechToReplyTo:
					case CursorAction.OpenGiftMenu:
					case CursorAction.Gift:
					case CursorAction.Eulogy:
					case CursorAction.VisitGrave:
						newState = CursorState.Speak;
						break;
					case CursorAction.ChokeHold:
					case CursorAction.BludgeonUnconscious:
						newState = CursorState.KnockOut;
						break;
					case CursorAction.SlitThroat:
					case CursorAction.KillUnconscious:
					case CursorAction.Slaughter:
						newState = CursorState.Knife;
						break;
					case CursorAction.Restrain:
						newState = CursorState.Restrain;
						break;
					case CursorAction.Open:
					case CursorAction.Close:
					case CursorAction.Lock:
					case CursorAction.Unlock:
					case CursorAction.Knock:
						newState = CursorState.Door;
						break;
					}
					forbidden = availableAction.Enabled != CursorActionDisabledReason.Enabled;
				}
			}
			else
			{
				newState = CursorState.DirectControl;
			}
		}
		if (newState != State)
		{
			WantRefreshSpeechOptions = true;
		}
		State = newState;
		Forbidden = forbidden;
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		Hud instance = Hud.Instance;
		bool flag = IsPlacingBuilding || IsSettingCropPatch != null || ShowGiftMenu || ShowCraftMenuFor != null || ShowDisassembleMenuFor != null;
		if (instance.LocalControlledCharacter != null && instance.LocalControlledCharacter.DirectControlled && instance.LocalControlledCharacter.IsInSurvivorGoalWithNoSubGoal())
		{
			flag |= AvailableActions.Count > 0 && instance.LocalTargetObject != null;
			flag |= GameImpl.Instance.IsDialogOpen();
			flag |= InfoScreen.Instance.Transition > 0f;
		}
		if (SentIsNavigatingMenus != flag)
		{
			inputFrame.AddAction(InputAction.SetSyncedIsNavigatingMenus(flag));
			SentIsNavigatingMenus = flag;
		}
		int num = FindAvailableActionByType(CursorAction.TakeAll);
		if (num == -1)
		{
			num = FindAvailableActionByType(CursorAction.StealAll);
		}
		if (num == -1)
		{
			num = FindAvailableActionByType(CursorAction.ScavengeEmpty);
		}
		if (num == -1)
		{
			num = FindAvailableActionByType(CursorAction.StealEmpty);
		}
		if (num == -1)
		{
			num = FindAvailableActionByType(CursorAction.OpenCraftMenu);
		}
		if (num == -1)
		{
			num = FindAvailableActionByType(CursorAction.OpenCookMenu);
		}
		if (num == -1)
		{
			return;
		}
		TileObject tileObject = AvailableActions[num].Target as TileObject;
		if (!tileObject.IsInvestigated() && SentMarkInvestigated != tileObject.Id)
		{
			PropPrototype propPrototype = tileObject.GetPropPrototype();
			if (propPrototype == null || propPrototype.CanAutoInvestigate)
			{
				inputFrame.AddAction(InputAction.MarkInvestigated(tileObject, instance.LocalControlledCharacter));
				SentMarkInvestigated = tileObject.Id;
			}
		}
	}

	public void AddMovementZoneActions(Character controlledCharacter, TerrainCoord hoveredTile)
	{
		Hud instance = Hud.Instance;
		if (controlledCharacter == null || !controlledCharacter.Community.CanUseRoleCommands())
		{
			return;
		}
		if (controlledCharacter.MovementZone != TerrainRect.Invalid)
		{
			if (controlledCharacter.MovementZonePaused)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.ResumeZone, controlledCharacter, hoveredTile, CursorActionDisabledReason.Enabled));
			}
			else
			{
				AvailableActions.Add(new AvailableAction(CursorAction.PauseZone, controlledCharacter, hoveredTile, CursorActionDisabledReason.Enabled));
			}
			AvailableActions.Add(new AvailableAction(CursorAction.ClearZone, controlledCharacter, hoveredTile, CursorActionDisabledReason.Enabled));
		}
		AvailableActions.Add(new AvailableAction(CursorAction.SetZone, controlledCharacter, hoveredTile, CursorActionDisabledReason.Enabled));
		int count = AvailableActions.Count;
		bool flag = false;
		foreach (Character selectedCharacter in instance.SelectedCharacters)
		{
			if (selectedCharacter != controlledCharacter && selectedCharacter.IsInPlayerCommunity())
			{
				flag |= controlledCharacter.MovementZone != selectedCharacter.MovementZone || controlledCharacter.MovementZonePaused != selectedCharacter.MovementZonePaused;
			}
		}
		if (controlledCharacter.MovementZone != TerrainRect.Invalid && flag)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.CopyZone, controlledCharacter, hoveredTile, CursorActionDisabledReason.Enabled));
		}
		AddFollowingActions(controlledCharacter);
		AddGlobalSelectActions(controlledCharacter);
		if (AvailableActions.Count > count)
		{
			AvailableActions.Insert(count, new AvailableAction(CursorAction.Separator, null));
		}
	}

	public void AddFollowingActions(Character controlledCharacter)
	{
		Hud instance = Hud.Instance;
		if (instance.SelectedCharacters.Count > 0)
		{
			bool flag = true;
			bool flag2 = false;
			foreach (Character selectedCharacter in instance.SelectedCharacters)
			{
				if (selectedCharacter != controlledCharacter)
				{
					bool flag3 = selectedCharacter.IsInSameSquad(controlledCharacter);
					flag = flag && flag3;
					flag2 = flag2 || flag3;
				}
			}
			if (!flag)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.GroupFollowMe, controlledCharacter, null, CursorActionDisabledReason.Enabled));
			}
			if (flag2)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.GroupStopFollowingMe, controlledCharacter, null, CursorActionDisabledReason.Enabled));
			}
		}
		if (controlledCharacter.Followers == null || controlledCharacter.Followers.Count <= 0)
		{
			return;
		}
		CursorActionDisabledReason cursorActionDisabledReason = CursorActionDisabledReason.Disabled;
		foreach (Character follower in controlledCharacter.Followers)
		{
			if (!follower.NonDeterministicSelected)
			{
				cursorActionDisabledReason = CursorActionDisabledReason.Enabled;
				break;
			}
		}
		if (cursorActionDisabledReason == CursorActionDisabledReason.Disabled && !InputFunctionManager.Instance.IsPressed(InputFunction.AddToSelection, capture: false))
		{
			foreach (Character member in controlledCharacter.Community.Members)
			{
				if (member.NonDeterministicSelected && !member.IsInSameSquad(controlledCharacter) && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
				{
					cursorActionDisabledReason = CursorActionDisabledReason.Enabled;
					break;
				}
			}
		}
		AvailableActions.Add(new AvailableAction(CursorAction.SelectFollowers, controlledCharacter, null, cursorActionDisabledReason));
	}

	public void AddGlobalSelectActions(Character controlledCharacter)
	{
		Hud instance = Hud.Instance;
		CursorActionDisabledReason enabled = CursorActionDisabledReason.Disabled;
		int num = 0;
		foreach (Character member in controlledCharacter.Community.Members)
		{
			if (!member.AliveAndNotZombie || member.GetBaseObjectType() != BaseObjectType.Human)
			{
				continue;
			}
			num++;
			if (!member.NonDeterministicSelected)
			{
				enabled = CursorActionDisabledReason.Enabled;
				if (num > 1)
				{
					break;
				}
			}
		}
		if (num > 1)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SelectEveryone, controlledCharacter, null, enabled));
		}
		if (instance.SelectedCharacters.Count > 0)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SelectNoone, controlledCharacter, null, CursorActionDisabledReason.Enabled));
		}
	}

	public void AddCharacterFollowActions(Character controlledCharacter, Character targetCharacter)
	{
		if (targetCharacter != null && targetCharacter.AliveAndNotZombie && targetCharacter is Human && targetCharacter != controlledCharacter && targetCharacter.SparringType != SparringType.Feuding && targetCharacter.SparringType != SparringType.FightToTheDeath && (Session.Instance.GameCamera.FlyCam || GetConversationWith(controlledCharacter, targetCharacter) == null) && targetCharacter.CanFollowPlayerIncludeAllies() && Session.Instance.FollowerCommandsEnabled)
		{
			if (controlledCharacter.IsInSameSquad(targetCharacter))
			{
				AvailableActions.Add(new AvailableAction(CursorAction.StopFollowingMe, controlledCharacter, targetCharacter, CursorActionDisabledReason.Enabled));
			}
			else if (targetCharacter.IsAwake)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.FollowMe, controlledCharacter, targetCharacter, targetCharacter.IsTooDepressedToFollowOrders() ? CursorActionDisabledReason.TooDepressed : CursorActionDisabledReason.Enabled));
			}
		}
	}

	public void AddBuildingFollowActions(Character controlledCharacter, Building targetBuilding)
	{
		if (targetBuilding == null)
		{
			return;
		}
		bool allAreFollowingMe = false;
		bool allAreDepressed = false;
		bool allAreBusy = false;
		if (targetBuilding.HasInhabitantsWhoCanFollowMe(controlledCharacter, out allAreFollowingMe, out allAreDepressed, out allAreBusy) && Session.Instance.FollowerCommandsEnabled)
		{
			if (allAreFollowingMe)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.StopFollowingMe, controlledCharacter, targetBuilding, CursorActionDisabledReason.Enabled));
			}
			else
			{
				AvailableActions.Add(new AvailableAction(CursorAction.FollowMe, controlledCharacter, targetBuilding, allAreDepressed ? CursorActionDisabledReason.TooDepressed : CursorActionDisabledReason.Enabled));
			}
		}
	}

	public void AddPourIntoAction(Character controlledCharacter, Prop targetProp, Equipment equippedItem, ref int equipmentActions)
	{
		if (targetProp == null || targetProp.GetLiquidType() == null)
		{
			return;
		}
		CursorAction actionType = ((targetProp is EnterableVehicle) ? CursorAction.Refuel : CursorAction.PourWater);
		CursorActionDisabledReason cursorActionDisabledReason = targetProp.CanBeFilled();
		switch (cursorActionDisabledReason)
		{
		case CursorActionDisabledReason.Enabled:
			if (controlledCharacter.Consciousness == Consciousness.Unconscious)
			{
				cursorActionDisabledReason = CursorActionDisabledReason.Unconscious;
			}
			break;
		case CursorActionDisabledReason.Disabled:
			return;
		}
		if (equippedItem != null && equippedItem.GetLiquidContentsType() == targetProp.GetLiquidType())
		{
			AvailableActions.Add(new AvailableAction(actionType, controlledCharacter, targetProp, equippedItem, cursorActionDisabledReason));
			equipmentActions++;
			return;
		}
		Equipment equipment = controlledCharacter.Inventory.FindBestItemWithLiquid(targetProp.GetLiquidType());
		if (equipment != null && equipment != equippedItem)
		{
			AvailableActions.Add(new AvailableAction(actionType, controlledCharacter, targetProp, equipment, cursorActionDisabledReason));
		}
		else
		{
			AvailableActions.Add(new AvailableAction(actionType, controlledCharacter, targetProp, null, (cursorActionDisabledReason == CursorActionDisabledReason.Enabled) ? CursorActionDisabledReason.NeedLiquid : cursorActionDisabledReason));
		}
	}

	public void AddCraftingActions(CraftingProp targetCraftingProp, Character controlledCharacter, bool inDisassembleMenu)
	{
		bool flag = false;
		foreach (Recipe item in GameImpl.Instance.CurrentRecipesSortedBySkill)
		{
			if (item.IsDisassembly != inDisassembleMenu || !item.IsCraftingPropForRecipe(targetCraftingProp))
			{
				continue;
			}
			CursorActionDisabledReason cursorActionDisabledReason = CanStartCraftingWithProp(controlledCharacter, targetCraftingProp, item, controlledCharacter.EquippedItem, ingredientsMustBeOnMe: false, wantCheckEquipmentPolicy: false, checkIfCrafting: false);
			if (cursorActionDisabledReason == CursorActionDisabledReason.Disabled)
			{
				continue;
			}
			if (!item.ShownDiscoveredNotification)
			{
				flag = true;
				continue;
			}
			if (controlledCharacter.GetSkillLevelWithEffects(item.SkillType) < item.SkillLevel)
			{
				cursorActionDisabledReason = GetSkillTooLowReasonForSkillType(item.SkillType);
			}
			Equipment equipment = ((controlledCharacter.EquippedItem != null && item.GetIngredient(controlledCharacter.EquippedItem) != null) ? controlledCharacter.EquippedItem : null);
			if (item.IsProductDrinkableOrEdible() && controlledCharacter.ShouldRefuseToEatHumanMeat())
			{
				if (equipment != null && equipment.GetPrototype().ContainsHumanMeat)
				{
					cursorActionDisabledReason = CursorActionDisabledReason.RefuseToEat;
				}
				if (item.ProductPrototype != null && item.ProductPrototype.ContainsHumanMeat)
				{
					cursorActionDisabledReason = CursorActionDisabledReason.RefuseToEat;
				}
				foreach (Ingredient ingredient in item.Ingredients)
				{
					if (ingredient.Prototypes == null || ingredient.Prototypes.Count <= 0)
					{
						continue;
					}
					bool flag2 = true;
					foreach (EquipmentPrototype prototype in ingredient.Prototypes)
					{
						if (!prototype.ContainsHumanMeat)
						{
							flag2 = false;
						}
					}
					if (flag2)
					{
						cursorActionDisabledReason = CursorActionDisabledReason.RefuseToEat;
						break;
					}
				}
			}
			(item.HasAllIngredients(controlledCharacter, controlledCharacter, equipment, null) ? AvailableActions : VisibleButUnavailableActions).Add(new AvailableAction(CursorAction.Craft, controlledCharacter, item, equipment, targetCraftingProp.GetCentreTile(), cursorActionDisabledReason));
		}
		if (flag)
		{
			VisibleButUnavailableActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate((targetCraftingProp is Campfire) ? AvailableAction.HINT_NoCookingRecipes : AvailableAction.HINT_NoCraftingRecipes)));
		}
		if (VisibleButUnavailableActions.Count > 0)
		{
			if (AvailableActions.Count > 0 && AvailableActions[AvailableActions.Count - 1].ActionType != CursorAction.Separator)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
			}
			for (int i = 0; i < VisibleButUnavailableActions.Count; i++)
			{
				AvailableActions.Add(VisibleButUnavailableActions[i]);
			}
		}
		if (VisibleButUnavailableActions.Count > 0)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
		}
		AvailableActions.Add(new AvailableAction(CursorAction.Back, controlledCharacter, targetCraftingProp, CursorActionDisabledReason.Enabled));
	}

	public void AddCraftingPropActions(CraftingProp targetCraftingProp, Character controlledCharacter, Equipment equippedItem, ref bool hasMaterialToLightFire, ref int equipmentActions)
	{
		Campfire campfire = targetCraftingProp as Campfire;
		WorkBench workBench = targetCraftingProp as WorkBench;
		CursorActionDisabledReason cursorActionDisabledReason = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		if (campfire != null)
		{
			if (campfire.State == CampfireState.Burning && !controlledCharacter.Sitting && controlledCharacter.FindActiveGoal(GoalType.SitAroundFireGoal) == null)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SitByFire, controlledCharacter, campfire, enabledIfConscious));
			}
			foreach (Equipment content in campfire.Inventory.Contents)
			{
				if (content.IsEdible() || content.IsDrinkable())
				{
					CursorAction actionType = (IsStealingToTakeFrom(controlledCharacter, targetCraftingProp) ? CursorAction.StealFromPot : CursorAction.EatFromPot);
					CursorActionDisabledReason enabled = cursorActionDisabledReason;
					if (content.GetPrototype().GetCaffeine() > controlledCharacter.GetSleepDeprivation())
					{
						enabled = CursorActionDisabledReason.NotSleepy;
					}
					if (content.GetPrototype().GetNutrition() > controlledCharacter.GetHunger())
					{
						enabled = CursorActionDisabledReason.NotHungry;
					}
					if (content.GetPrototype().ContainsHumanMeat && controlledCharacter.ShouldRefuseToEatHumanMeat())
					{
						enabled = CursorActionDisabledReason.RefuseToEat;
					}
					AvailableActions.Add(new AvailableAction(actionType, controlledCharacter, campfire, content, enabled));
				}
			}
		}
		if (workBench != null && workBench.CanRepairArmor() && controlledCharacter.Inventory.FindItemOfClass(typeof(Armor)) != null)
		{
			CursorActionDisabledReason enabled2 = (controlledCharacter.Inventory.HasAnyArmorThatNeedsRepairing() ? enabledIfConscious : CursorActionDisabledReason.ArmorIsAllRepaired);
			AvailableActions.Add(new AvailableAction(CursorAction.RepairArmor, controlledCharacter, workBench, enabled2));
		}
		if (targetCraftingProp.Inventory.Count > 0)
		{
			if (CanSeeTakeAllOption(controlledCharacter, targetCraftingProp))
			{
				CursorActionDisabledReason enabled3 = CanTakeAll(controlledCharacter, targetCraftingProp);
				AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetCraftingProp) ? CursorAction.StealAll : CursorAction.TakeAll, controlledCharacter, targetCraftingProp, enabled3));
			}
			AvailableActions.Add(new AvailableAction(GetScavengeAction(controlledCharacter, targetCraftingProp), controlledCharacter, targetCraftingProp, cursorActionDisabledReason));
		}
		if (campfire != null)
		{
			CursorActionDisabledReason enabled4 = ((campfire.WoodRemaining < 1f) ? enabledIfConscious : CursorActionDisabledReason.CampfireWoodFull);
			foreach (Equipment content2 in controlledCharacter.Inventory.Contents)
			{
				if (CanAddMaterialToFire(content2.GetPrototype()))
				{
					AvailableActions.Add(new AvailableAction(CursorAction.AddMaterialToFire, controlledCharacter, campfire, content2, enabled4));
					hasMaterialToLightFire = true;
				}
			}
		}
		if ((campfire == null || campfire.State == CampfireState.Burning) && (targetCraftingProp.Community == null || targetCraftingProp.Community == controlledCharacter.Community))
		{
			Character currentCrafter = targetCraftingProp.CurrentCrafter;
			if (targetCraftingProp.IsCrafting())
			{
				RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Cook);
				RoleInfo roleInfo2 = controlledCharacter.GetRoleInfo(new RoleInfo(Role.Crafter, targetCraftingProp.CraftingRecipe, targetCraftingProp.GetCentreTile()));
				if (currentCrafter == null || !currentCrafter.IsCraftingWithProp(targetCraftingProp) || (currentCrafter == controlledCharacter && roleInfo2.Valid && roleInfo2.Paused) || (currentCrafter == controlledCharacter && roleInfo.Valid && roleInfo.Paused))
				{
					CursorActionDisabledReason enabled5 = ((!controlledCharacter.CanUseRecipe(targetCraftingProp.CraftingRecipe)) ? GetSkillTooLowReasonForSkillType(targetCraftingProp.CraftingRecipe.SkillType) : CursorActionDisabledReason.Enabled);
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeCrafting, controlledCharacter, targetCraftingProp.CraftingRecipe, targetCraftingProp, enabled5));
				}
			}
			if (!targetCraftingProp.IsCrafting() || controlledCharacter != currentCrafter)
			{
				CursorActionDisabledReason enabled6 = CursorActionDisabledReason.Enabled;
				if (campfire != null)
				{
					if (campfire.Inventory.FindItemOfType(EquipmentPrototype.Pot) != null || campfire.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) != null)
					{
						if (campfire.Inventory.Count > 1)
						{
							enabled6 = CursorActionDisabledReason.CampfireHasItems;
						}
					}
					else if (campfire.Inventory.Count > 0)
					{
						enabled6 = CursorActionDisabledReason.CampfireHasItems;
					}
				}
				if (campfire != null)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.OpenCookMenu, controlledCharacter, targetCraftingProp, enabled6));
				}
				else
				{
					AvailableActions.Add(new AvailableAction(CursorAction.OpenCraftMenu, controlledCharacter, targetCraftingProp, enabled6));
					AvailableActions.Add(new AvailableAction(CursorAction.OpenDisassembleMenu, controlledCharacter, targetCraftingProp, enabled6));
				}
			}
			if (campfire != null)
			{
				RoleInfo roleInfo3 = controlledCharacter.GetRoleInfo(Role.Cook);
				if (roleInfo3.Valid)
				{
					CursorActionDisabledReason enabled7 = ((roleInfo3.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
					AvailableActions.Add(new AvailableAction(roleInfo3.Paused ? CursorAction.ResumeCook : CursorAction.CancelCook, controlledCharacter, campfire, enabled7));
				}
				else if (controlledCharacter.Community.CanUseRoleCommands())
				{
					CursorActionDisabledReason cursorActionDisabledReason2 = ((campfire.CurrentCrafter != null && campfire.CurrentCrafter != controlledCharacter && campfire.IsCrafting()) ? CursorActionDisabledReason.CampfireInUse : CursorActionDisabledReason.Enabled);
					if (cursorActionDisabledReason2 == CursorActionDisabledReason.Enabled && controlledCharacter.Roles.Count >= 10)
					{
						cursorActionDisabledReason2 = CursorActionDisabledReason.TooManyRoles;
					}
					AvailableActions.Add(new AvailableAction(CursorAction.Cook, controlledCharacter, campfire, cursorActionDisabledReason2));
				}
			}
		}
		if (campfire == null || !campfire.IsBurning())
		{
			return;
		}
		CursorActionDisabledReason cursorActionDisabledReason3 = CursorActionDisabledReason.Disabled;
		float waterNeeded = EquipmentPrototype.PlasticBottle.LiquidCapacity - 0.01f;
		if (equippedItem != null && equippedItem.GetLiquidContentsType() == LiquidPrototype.Water)
		{
			cursorActionDisabledReason3 = ((equippedItem.GetLiquidContentsAmount() >= EquipmentPrototype.PlasticBottle.LiquidCapacity - 0.01f) ? enabledIfConscious : CursorActionDisabledReason.NeedMoreWaterToPutOutFire);
			AvailableActions.Add(new AvailableAction(CursorAction.PutOutFire, controlledCharacter, campfire, equippedItem, cursorActionDisabledReason3));
			equipmentActions++;
		}
		if (cursorActionDisabledReason3 != enabledIfConscious)
		{
			Equipment bestWateringToPutOutFire = controlledCharacter.Inventory.GetBestWateringToPutOutFire(waterNeeded);
			if (bestWateringToPutOutFire != null && bestWateringToPutOutFire != equippedItem)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.PutOutFire, controlledCharacter, campfire, bestWateringToPutOutFire, enabledIfConscious));
			}
		}
	}

	public void AddPropActions(Prop targetProp, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		if (!(targetProp.GetMaxInventoryWeight() > 0f))
		{
			return;
		}
		Grave grave = targetProp as Grave;
		CursorActionDisabledReason cursorActionDisabledReason = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		if (grave != null && grave.GraveState != GraveState.Open && grave.Corpse != null && !grave.Corpse.Deleted)
		{
			if (!grave.Corpse.EulogyGiven)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Eulogy, controlledCharacter, grave, cursorActionDisabledReason));
			}
			else if (Session.Instance.PlayTime - grave.LastVisitedTime >= Grave.MinTimeBetweenVisits && grave.Corpse.NameKnown)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.VisitGrave, controlledCharacter, grave, cursorActionDisabledReason));
			}
		}
		bool flag = targetProp.IsLockable() && targetProp.Community != null && targetProp.Community != controlledCharacter.Community;
		if (!flag && CanSeeTakeAllOption(controlledCharacter, targetProp))
		{
			CursorActionDisabledReason enabled = (flag ? CursorActionDisabledReason.InventoryLocked : CanTakeAll(controlledCharacter, targetProp));
			AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetProp) ? CursorAction.StealAll : CursorAction.TakeAll, controlledCharacter, targetProp, enabled));
		}
		AvailableActions.Add(new AvailableAction(GetScavengeAction(controlledCharacter, targetProp), controlledCharacter, targetProp, flag ? CursorActionDisabledReason.InventoryLocked : cursorActionDisabledReason));
		if (targetProp.Community == controlledCharacter.Community && targetProp.StoragePolicies != null && targetProp.StoragePolicies.Count > 0)
		{
			RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Organizer);
			if (roleInfo.Valid)
			{
				CursorActionDisabledReason enabled2 = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeOrganizing : CursorAction.CancelOrganizing, controlledCharacter, targetProp, enabled2));
			}
			else if (controlledCharacter.Community.CanUseRoleCommands())
			{
				CursorActionDisabledReason enabled3 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.SetOrganizer, controlledCharacter, targetProp, enabled3));
			}
			return;
		}
		if (targetProp.Investigated && targetProp.Inventory.Count > 0 && controlledCharacter.Community.Buildings.Count > 0 && (targetProp.Community == null || !targetProp.Community.HasAnyActiveMembers()))
		{
			CursorActionDisabledReason enabled4 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(CursorAction.Gather, controlledCharacter, targetProp, enabled4));
		}
		for (int i = 0; i < controlledCharacter.Roles.Count; i++)
		{
			if (controlledCharacter.Roles[i].Role == Role.Gatherer && targetProp.Tile == controlledCharacter.Roles[i].TargetLocation)
			{
				CursorActionDisabledReason enabled5 = ((controlledCharacter.Roles[i].Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(controlledCharacter.Roles[i].Paused ? CursorAction.ResumeGathering : CursorAction.CancelGathering, controlledCharacter, controlledCharacter.Roles[i].ResourceType, targetProp.Tile, enabled5));
			}
		}
	}

	public void AddEquipmentUseAction(Equipment item, Character user, TileObject carrier, bool onlyIfEnabled, ref int equipmentActions)
	{
		if (onlyIfEnabled && (user.IsUsingEquippedItem() || user.IsCraftingAnim()))
		{
			return;
		}
		CursorActionDisabledReason cursorActionDisabledReason = ((user.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		if (user != carrier && carrier.WantToKeepGift(item))
		{
			cursorActionDisabledReason = CursorActionDisabledReason.WantToKeepGift;
		}
		LiquidPrototype liquidContentsType = item.GetLiquidContentsType();
		if (liquidContentsType != null)
		{
			if (liquidContentsType.DrinkableOrEdible)
			{
				AvailableActions.Add(new AvailableAction(liquidContentsType.Edible ? CursorAction.Eat : CursorAction.Drink, user, carrier, item, cursorActionDisabledReason));
				equipmentActions++;
			}
		}
		else if (item.IsEdible() || item.IsDrinkable())
		{
			CursorActionDisabledReason enabled = cursorActionDisabledReason;
			if (item.GetPrototype().GetCaffeine() > user.GetSleepDeprivation())
			{
				enabled = CursorActionDisabledReason.NotSleepy;
			}
			if (item.GetNutrition() > user.GetHunger())
			{
				enabled = CursorActionDisabledReason.NotHungry;
			}
			if (item.GetPrototype().ContainsHumanMeat && user.HasPersonality(CachedPersonalityType.Moral) && user.GetHunger() < Character.HungerExtraCriticalTime)
			{
				enabled = CursorActionDisabledReason.RefuseToEat;
			}
			AvailableActions.Add(new AvailableAction(item.IsEdible() ? CursorAction.Eat : CursorAction.Drink, user, carrier, item, enabled));
			equipmentActions++;
		}
		if (item.GetBandageLevel() > -1)
		{
			int num = Math.Min(user.GetSkillLevelWithEffects(SkillType.Medicine), item.GetBandageLevel());
			CursorActionDisabledReason cursorActionDisabledReason2 = ((user.Injuries.Count == 0) ? CursorActionDisabledReason.NoInjuries : (user.HasUnbandagedInjury(num) ? cursorActionDisabledReason : ((num == 0) ? CursorActionDisabledReason.NoUnbandagedInjuries : CursorActionDisabledReason.NoInjuriesWithLowerBandageLevel)));
			if (cursorActionDisabledReason2 == cursorActionDisabledReason || !onlyIfEnabled)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Use, user, carrier, item, num, cursorActionDisabledReason2));
				equipmentActions++;
			}
		}
		if (item.GetAntigenType() != InfectionType.None)
		{
			CursorActionDisabledReason cursorActionDisabledReason3 = (user.HasInjuryWithInfectionType(item.GetAntigenType()) ? cursorActionDisabledReason : CursorActionDisabledReason.NoInfectedInjuries);
			if (cursorActionDisabledReason3 == cursorActionDisabledReason || !onlyIfEnabled)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Use, user, carrier, item, cursorActionDisabledReason3));
				equipmentActions++;
			}
		}
		if (Session.Instance.DifficultySettings.SaveTokensRequired && item is SavegameToken)
		{
			CursorActionDisabledReason enabled2 = (Session.Instance.IsPartyLeader() ? cursorActionDisabledReason : CursorActionDisabledReason.OnlyPartyLeaderCanUseSaveTokens);
			AvailableActions.Add(new AvailableAction(CursorAction.Use, user, carrier, item, enabled2));
			equipmentActions++;
		}
		if (item.GetPrototype().HasCustomUseAction)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Use, user, carrier, item, cursorActionDisabledReason));
			equipmentActions++;
		}
		if (user == null || user != carrier)
		{
			return;
		}
		if (item.GetBandageLevel() > -1 && user.IsControllableByPlayer())
		{
			if (user.HasRole(Role.Medic))
			{
				AvailableActions.Add(new AvailableAction(CursorAction.CancelMedic, user, null, CursorActionDisabledReason.Enabled));
				equipmentActions++;
			}
			else if (user.Community.CanUseRoleCommands() && user.AIOverridesControl() == AIOverridesControlReason.None)
			{
				CursorActionDisabledReason enabled3 = ((user.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.SetMedic, user, null, enabled3));
				equipmentActions++;
			}
		}
		foreach (Recipe item2 in GameImpl.Instance.CurrentRecipesSortedBySkill)
		{
			RecipeType recipeType = item2.RecipeType;
			if (((uint)recipeType > 1u && ((uint)(recipeType - 2) > 1u || item2.ProductPropPrototype != null)) || !item2.ShownDiscoveredNotification)
			{
				continue;
			}
			Ingredient ingredient = item2.GetIngredient(item);
			if (ingredient == null)
			{
				continue;
			}
			int skillLevelWithEffects = user.GetSkillLevelWithEffects(item2.SkillType);
			CursorActionDisabledReason cursorActionDisabledReason4 = cursorActionDisabledReason;
			switch (item2.RecipeType)
			{
			case RecipeType.HuntingKnife:
				if (user.Inventory.GetHuntingKnife() == null)
				{
					cursorActionDisabledReason4 = CursorActionDisabledReason.NeedHuntingKnife;
				}
				break;
			case RecipeType.Toolbox:
				if (user.Inventory.GetToolbox() == null)
				{
					cursorActionDisabledReason4 = CursorActionDisabledReason.NeedToolbox;
				}
				break;
			case RecipeType.Shovel:
				if (user.Inventory.GetShovel() != null)
				{
					cursorActionDisabledReason4 = CursorActionDisabledReason.NeedShovel;
				}
				break;
			}
			if (cursorActionDisabledReason4 == CursorActionDisabledReason.Enabled || !onlyIfEnabled)
			{
				if (!item2.HasSuitableContainer(user, item, out var hasSomeButNotEnough, out var _, out var _))
				{
					cursorActionDisabledReason4 = ((item2.ProductLiquidPrototype == null || !item2.ProductLiquidPrototype.CanPourIntoBottles) ? (hasSomeButNotEnough ? CursorActionDisabledReason.DontEnoughSealedContainers : CursorActionDisabledReason.DontHaveSealedContainer) : (hasSomeButNotEnough ? CursorActionDisabledReason.DontEnoughSuitableContainers : CursorActionDisabledReason.DontHaveSuitableContainer));
				}
				if (item.GetPrototype().ContainsHumanMeat && item2.IsProductDrinkableOrEdible() && user.ShouldRefuseToEatHumanMeat())
				{
					cursorActionDisabledReason4 = CursorActionDisabledReason.RefuseToEat;
				}
				if (skillLevelWithEffects < item2.SkillLevel)
				{
					cursorActionDisabledReason4 = GetSkillTooLowReasonForSkillType(item2.SkillType);
				}
				if ((!item2.HasNonInterchangeableIngredients() || (!ingredient.Interchangeable && item2.HasAllIngredients(user, user, item, null))) && (Session.Instance.FollowerCommandsEnabled || item2.HasAllIngredients(user, user, item, null)))
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Craft, user, item2, item, cursorActionDisabledReason4));
					equipmentActions++;
				}
			}
		}
	}

	public static void AddEquipmentPolicyAction(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectedWith, Character controlledCharacter, List<AvailableAction> availableActions)
	{
		if ((proto != null && proto.GetNutrition() > 0f && (!proto.ContainsHumanMeat || infectedWith == InfectionType.None)) || (liquid != null && liquid.Edible))
		{
			availableActions.Add(new AvailableAction(CursorAction.SetFoodPolicy, controlledCharacter, proto, liquid, infectedWith, 0f, CursorActionDisabledReason.Enabled));
		}
		else if (liquid != null && liquid.Drinkable)
		{
			availableActions.Add(new AvailableAction(CursorAction.SetDrinkPolicy, controlledCharacter, null, liquid, infectedWith, 0f, CursorActionDisabledReason.Enabled));
		}
		else if (proto != null && proto.AmmoPrototypes != null)
		{
			foreach (EquipmentPrototype ammoPrototype in proto.AmmoPrototypes)
			{
				if (ammoPrototype.Discovered)
				{
					availableActions.Add(new AvailableAction(CursorAction.SetWeaponAmmoPolicy, controlledCharacter, ammoPrototype, null, infectedWith, 0f, CursorActionDisabledReason.Enabled));
				}
			}
			availableActions.Add(new AvailableAction(CursorAction.SetEquipmentPolicy, controlledCharacter, proto, null, InfectionType.None, 0f, CursorActionDisabledReason.Enabled));
		}
		else if (proto != null && (EquipmentPrototype.AllAmmoTypes.Contains(proto) || BaseObjectManager.PrototypeGameObjects[(int)proto.TypeName] is MolotovCocktail || BaseObjectManager.PrototypeGameObjects[(int)proto.TypeName] is PipeBomb))
		{
			availableActions.Add(new AvailableAction(CursorAction.SetAmmoPolicy, controlledCharacter, proto, null, infectedWith, 0f, CursorActionDisabledReason.Enabled));
		}
		else if (EquipmentPolicy.IsTargetAmountPossibleForItem(proto, liquid, int.MaxValue) || EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.AutoCollect, proto, liquid) || EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.AutoDeposit, proto, liquid) || EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanPlant, proto, liquid) || EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanCraftWith, proto, liquid))
		{
			availableActions.Add(new AvailableAction(CursorAction.SetEquipmentPolicy, controlledCharacter, proto, liquid, infectedWith, 0f, CursorActionDisabledReason.Enabled));
		}
	}

	public static void AddEquipmentTotalActions(EquipmentTotalBehaviour hoveredEquipmentTotal, Community community, Character controlledCharacter, Prop settingStoragePolicyForProp, Equipment designatingLiquidForItem, List<AvailableAction> headerActions, List<AvailableAction> availableActions)
	{
		headerActions.Add(new AvailableAction(CursorAction.EquipmentTotalName, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, 0, CursorActionDisabledReason.Enabled));
		if ((hoveredEquipmentTotal.Proto != null && hoveredEquipmentTotal.Proto.DescriptionHash != 0) || (hoveredEquipmentTotal.Liquid != null && hoveredEquipmentTotal.Liquid.DescriptionHash != 0))
		{
			headerActions.Add(new AvailableAction(CursorAction.EquipmentDescription, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, 0, CursorActionDisabledReason.Enabled));
		}
		if (designatingLiquidForItem != null)
		{
			headerActions.Add(new AvailableAction(CursorAction.Separator, null));
			availableActions.Add(new AvailableAction((designatingLiquidForItem.DesignatedLiquid == hoveredEquipmentTotal.Liquid) ? CursorAction.DeselectLiquid : CursorAction.SelectLiquid, null, hoveredEquipmentTotal.Liquid, 0, CursorActionDisabledReason.Enabled));
			return;
		}
		if (settingStoragePolicyForProp != null)
		{
			headerActions.Add(new AvailableAction(CursorAction.Separator, null));
			if (settingStoragePolicyForProp.WantStoreHere(hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid))
			{
				availableActions.Add(new AvailableAction(CursorAction.DontStoreHere, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, 0, CursorActionDisabledReason.Enabled));
			}
			else
			{
				availableActions.Add(new AvailableAction(CursorAction.StoreHere, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, 0, CursorActionDisabledReason.Enabled));
			}
			if (community == null)
			{
				return;
			}
			int num = 0;
			{
				foreach (Prop building in community.Buildings)
				{
					if (building.CanSetStoragePolicy() && building.WantStoreHere(hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid))
					{
						if (num == 0)
						{
							availableActions.Add(new AvailableAction(CursorAction.Separator, null));
						}
						CursorActionDisabledReason enabled = ((building == settingStoragePolicyForProp) ? CursorActionDisabledReason.Disabled : CursorActionDisabledReason.Enabled);
						availableActions.Add(new AvailableAction(CursorAction.LocateProp, building, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, 0f, enabled));
						num++;
					}
				}
				return;
			}
		}
		bool flag = community.IsActionAllowedForItemIgnoringInfection(hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, EquipmentPolicyAction.AutoCollect);
		int num2 = 0;
		foreach (Character member in community.Members)
		{
			if (!member.AliveAndNotZombie)
			{
				continue;
			}
			for (int i = 0; i < member.Roles.Count; i++)
			{
				switch (member.Roles[i].Role)
				{
				case Role.Crafter:
					if (member.Roles[i].Recipe != null && ((hoveredEquipmentTotal.Proto != null && member.Roles[i].Recipe.ProductPrototype == hoveredEquipmentTotal.Proto) || (hoveredEquipmentTotal.Liquid != null && member.Roles[i].Recipe.ProductLiquidPrototype == hoveredEquipmentTotal.Liquid)))
					{
						num2++;
					}
					break;
				case Role.Lumberjack:
					if (hoveredEquipmentTotal.Proto == EquipmentPrototype.Wood)
					{
						num2++;
					}
					break;
				case Role.Farmer:
					if (hoveredEquipmentTotal.Proto != null && Session.Instance.CropsManager.IsFarming(community.Id, hoveredEquipmentTotal.Proto))
					{
						num2++;
					}
					break;
				case Role.Gatherer:
					if (hoveredEquipmentTotal.Proto != null && member.Roles[i].ResourceType == hoveredEquipmentTotal.Proto)
					{
						num2++;
					}
					break;
				case Role.Miner:
					if (hoveredEquipmentTotal.Proto == member.Roles[i].ResourceType)
					{
						num2++;
					}
					break;
				}
				flag |= member.IsActionAllowedForItemIgnoringInfection(hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, EquipmentPolicyAction.AutoCollect, includeCommunityPolicy: false);
			}
		}
		if (num2 > 0)
		{
			headerActions.Add(new AvailableAction(CursorAction.NumberOfCrafters, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, num2, CursorActionDisabledReason.Enabled));
			headerActions.Add(new AvailableAction(CursorAction.Separator, null));
		}
		else
		{
			headerActions.Add(new AvailableAction(CursorAction.Separator, null));
		}
		if (flag || num2 > 0)
		{
			int amount = int.MaxValue;
			if (hoveredEquipmentTotal.Proto != null)
			{
				amount = community.GetCraftingLimit(hoveredEquipmentTotal.Proto);
			}
			else if (hoveredEquipmentTotal.Liquid != null)
			{
				amount = community.GetCraftingLimit(hoveredEquipmentTotal.Liquid);
			}
			availableActions.Add(new AvailableAction(CursorAction.CraftingLimit, community, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, amount, CursorActionDisabledReason.Enabled));
		}
		if (controlledCharacter != null)
		{
			AddEquipmentPolicyAction(hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid, InfectionType.None, controlledCharacter, availableActions);
		}
		EquipmentAmounts.Clear();
		foreach (Prop building2 in community.Buildings)
		{
			EquipmentAmount item = new EquipmentAmount(building2, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid);
			if (item.Amount > 0f)
			{
				EquipmentAmounts.Add(item);
			}
		}
		foreach (Character member2 in community.Members)
		{
			if (member2.AliveAndNotZombie)
			{
				EquipmentAmount item2 = new EquipmentAmount(member2, hoveredEquipmentTotal.Proto, hoveredEquipmentTotal.Liquid);
				if (item2.Amount > 0f)
				{
					EquipmentAmounts.Add(item2);
				}
			}
		}
		EquipmentAmounts.Sort();
		for (int j = 0; j < EquipmentAmounts.Count; j++)
		{
			availableActions.Add(new AvailableAction(CursorAction.LocateEquipment, EquipmentAmounts[j].Owner, EquipmentAmounts[j].Proto, EquipmentAmounts[j].Liquid, EquipmentAmounts[j].Amount, CursorActionDisabledReason.Enabled));
		}
		EquipmentAmounts.Clear();
	}

	public void AddEquipmentItemActions(Equipment item, InfoPage page, TileObject carrier, Character controlledCharacter)
	{
		Character character = carrier as Character;
		TakePage takePage = page as TakePage;
		TileObject other = page.GetOther(carrier);
		AddHeaderActionsForEquipment(controlledCharacter, carrier, item, other, (page is TradePage) ? SwappingSuppliesMode.Trading : ((takePage != null) ? takePage.Mode : SwappingSuppliesMode.None));
		HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
		GatherPage gatherPage = page as GatherPage;
		if (gatherPage != null)
		{
			AddGatherPageActions(gatherPage, controlledCharacter, carrier, item);
		}
		TileObject to;
		CursorAction cursorAction;
		CantTransferReason cantTransferReason = page.CanStartTransferEquipment(carrier, item, out to, out cursorAction);
		if (cantTransferReason != CantTransferReason.NotOnCorrectPage)
		{
			CursorActionDisabledReason enabled = ((cantTransferReason != CantTransferReason.CanTransfer) ? CursorActionDisabledReason.CantTransfer : CursorActionDisabledReason.Enabled);
			if (character != null && item.IsWorn(character) && character.IsOutdoors() && IsTargetCharacterInCombat(character) && !InfoScreen.AllowViewInfoOnAnyone)
			{
				enabled = CursorActionDisabledReason.CannotChangeClothesWhileInCombat;
			}
			switch (cursorAction)
			{
			case CursorAction.Give:
			case CursorAction.Store:
			case CursorAction.Sell:
				AvailableActions.Add(new AvailableAction(cursorAction, character, to, item, enabled, cantTransferReason));
				break;
			case CursorAction.Take:
			case CursorAction.Buy:
				AvailableActions.Add(new AvailableAction(cursorAction, to as Character, carrier, item, enabled, cantTransferReason));
				break;
			}
		}
		TradePage tradePage = page as TradePage;
		if (!(tradePage == null) && tradePage.PendingTrades.Count != 0)
		{
			return;
		}
		CursorActionDisabledReason cursorActionDisabledReason = ((controlledCharacter != null && controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		if (page.CanEquipOrWear(carrier, item))
		{
			if (character != null && item.CanBeEquipped())
			{
				if (character.EquippedItem == item)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Unequip, character, item, cursorActionDisabledReason));
				}
				else
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Equip, character, item, cursorActionDisabledReason));
				}
			}
			if (character != null && item.GetClothingType() != ClothingType.Invalid && item.GetClothingType() != ClothingType.Backpack)
			{
				if (character.Clothes[(int)item.GetClothingType()] == item)
				{
					CursorActionDisabledReason enabled2 = cursorActionDisabledReason;
					if (controlledCharacter != null && controlledCharacter.IsOutdoors() && IsTargetCharacterInCombat(controlledCharacter) && !InfoScreen.AllowViewInfoOnAnyone)
					{
						enabled2 = CursorActionDisabledReason.CannotChangeClothesWhileInCombat;
					}
					AvailableActions.Add(new AvailableAction(CursorAction.Strip, character, item, enabled2));
				}
				else
				{
					CursorActionDisabledReason enabled3 = (character.CanWear(item.GetPrototype()) ? cursorActionDisabledReason : CursorActionDisabledReason.ClothesDontFit);
					if (controlledCharacter != null && controlledCharacter.IsOutdoors() && IsTargetCharacterInCombat(controlledCharacter) && !InfoScreen.AllowViewInfoOnAnyone)
					{
						enabled3 = CursorActionDisabledReason.CannotChangeClothesWhileInCombat;
					}
					AvailableActions.Add(new AvailableAction(CursorAction.Wear, character, item, enabled3));
				}
			}
		}
		if (controlledCharacter != null && page.CanUse(carrier, item))
		{
			Character character2 = controlledCharacter;
			if (carrier != controlledCharacter && !(page is TakePage))
			{
				if (character != null && character.IsControllableByPlayer() && character.IsConscious)
				{
					character2 = character;
				}
				else if (carrier != controlledCharacter.InsideBuilding && (character == null || character.InsideBuilding == null || character.InsideBuilding != controlledCharacter.InsideBuilding))
				{
					character2 = null;
				}
			}
			if (character2 != null)
			{
				int equipmentActions = 0;
				AddEquipmentUseAction(item, character2, carrier, onlyIfEnabled: false, ref equipmentActions);
			}
		}
		if (page.CanUnloadGun(carrier, item))
		{
			AmmoWeapon ammoWeapon = (AmmoWeapon)item;
			AvailableAmmoTypes.Clear();
			List<EquipmentPrototype> ammoTypes = ammoWeapon.GetAmmoTypes();
			if (ammoTypes != null)
			{
				foreach (EquipmentPrototype item2 in ammoTypes)
				{
					if (item2.Discovered && carrier.GetInventory().FindItemOfType(item2) != null)
					{
						AvailableAmmoTypes.Add(item2);
					}
				}
			}
			if (ammoWeapon.CurrentAmmo < ammoWeapon.GetMaxAmmo() || AvailableAmmoTypes.Count > 1 || (AvailableAmmoTypes.Count == 1 && AvailableAmmoTypes[0] != ammoWeapon.CurrentAmmoType))
			{
				foreach (EquipmentPrototype availableAmmoType in AvailableAmmoTypes)
				{
					CursorActionDisabledReason enabled4 = ((availableAmmoType == ammoWeapon.CurrentAmmoType && ammoWeapon.CurrentAmmo >= ammoWeapon.GetMaxAmmo()) ? CursorActionDisabledReason.Disabled : cursorActionDisabledReason);
					AvailableActions.Add(new AvailableAction(CursorAction.LoadAmmo, controlledCharacter, carrier, ammoWeapon, availableAmmoType, enabled4));
				}
			}
			AvailableAmmoTypes.Clear();
			if (ammoWeapon.CurrentAmmo > 0)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Unload, controlledCharacter, carrier, ammoWeapon, cursorActionDisabledReason));
			}
		}
		if (page.CanLightFuse(controlledCharacter, carrier, item))
		{
			CursorActionDisabledReason enabled5 = cursorActionDisabledReason;
			if (controlledCharacter.GetSkillLevelWithEffects(SkillType.Stealth) < GetSkillLevelNeededToLightPipeBombFuse(carrier))
			{
				enabled5 = CursorActionDisabledReason.StealthSkillTooLow;
			}
			AvailableActions.Add(new AvailableAction(CursorAction.LightFuse, controlledCharacter, carrier, item, enabled5));
		}
		if (item.GetLiquidContentsAmount() > 0f && page.CanPourAwayLiquid(carrier, item))
		{
			AvailableActions.Add(new AvailableAction(CursorAction.PourAway, controlledCharacter, carrier, item, cursorActionDisabledReason));
			AvailableActions.Add(new AvailableAction(CursorAction.StartPourInto, controlledCharacter, carrier, item, cursorActionDisabledReason));
		}
		if (page.CanShowEquipmentPolicy(controlledCharacter, carrier, item))
		{
			AddEquipmentPolicyAction((item.GetLiquidContentsType() == null) ? item.GetPrototype() : null, item.GetLiquidContentsType(), item.InfectedWith, (InfoScreen.GetAllowViewInfoOnAnyone() && character != null) ? character : controlledCharacter, AvailableActions);
			if (item.GetLiquidCapacity() > 0f && carrier.GetCommunity() == controlledCharacter.Community)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SetDesignatedLiquid, controlledCharacter, carrier, item, CursorActionDisabledReason.Enabled));
			}
		}
		if (page.CanDeleteEquipment(carrier, item))
		{
			AvailableActions.Add(new AvailableAction(CursorAction.DestroyItem, controlledCharacter, carrier, item, cursorActionDisabledReason));
		}
	}

	public static int GetSkillLevelNeededToLightPipeBombFuse(TileObject carrier)
	{
		if (!(carrier is Character { IsAwake: not false }))
		{
			return 0;
		}
		return 5;
	}

	public void AddCharacterSelectActions(Character controlledCharacter)
	{
		if (controlledCharacter.NonDeterministicSelected)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.DeselectCharacter, controlledCharacter, null, CursorActionDisabledReason.Enabled));
		}
		else if (controlledCharacter.IsSelectable() && controlledCharacter.Community.CanUseRoleCommands())
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SelectCharacter, controlledCharacter, null, CursorActionDisabledReason.Enabled));
		}
	}

	public void AddCharacterIconActions(InfoPage page, Character controlledCharacter, CharacterIconBehaviour hoveredCharacterIcon, out BaseObject outTarget)
	{
		Session instance = Session.Instance;
		Hud instance2 = Hud.Instance;
		outTarget = hoveredCharacterIcon.Character;
		FocusUnityObj = InfoScreen.CurrentHoveredGameObject;
		HeaderActions.Add(new AvailableAction(CursorAction.CharacterName, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
		if (hoveredCharacterIcon.Character.HasUnbandagedInjury(0) || hoveredCharacterIcon.Character.GetInfectionProgression() > 0f || hoveredCharacterIcon.Character.GetThirst() >= Character.ThirstCriticalTime || hoveredCharacterIcon.Character.GetHunger() >= Character.HungerCriticalTime || hoveredCharacterIcon.Character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime || hoveredCharacterIcon.Character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia || hoveredCharacterIcon.Character.IsTooDepressedToFollowOrders())
		{
			HeaderActions.Add(new AvailableAction(CursorAction.CharacterStatus, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
		}
		HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
		if (hoveredCharacterIcon.Character != instance2.BuildingSelectedInhabitant && page is BuildingPage)
		{
			AvailableActions.Add(new AvailableAction(hoveredCharacterIcon.Character.IsControllableByPlayer() ? CursorAction.ControlCharacterInBuilding : CursorAction.ViewInventoryInBuilding, null, hoveredCharacterIcon.Character, CursorActionDisabledReason.Enabled));
		}
		if (hoveredCharacterIcon.Character.IsControllableByPlayer() || instance.Editor || InfoScreen.GetAllowViewInfoOnAnyone())
		{
			AvailableActions.Add(new AvailableAction(CursorAction.ViewCharacter, null, hoveredCharacterIcon.Character, CursorActionDisabledReason.Enabled));
		}
		if (instance.GameCamera.FlyCam && !instance.Editor)
		{
			AddCharacterSelectActions(hoveredCharacterIcon.Character);
		}
		if (!(page is CommunityPage))
		{
			return;
		}
		for (int i = 0; i < hoveredCharacterIcon.Character.Roles.Count; i++)
		{
			RoleInfo roleInfo = hoveredCharacterIcon.Character.Roles[i];
			if (roleInfo.Paused)
			{
				CursorActionDisabledReason enabled = ((hoveredCharacterIcon.Character.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				switch (roleInfo.Role)
				{
				case Role.Farmer:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeFarming, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Gatherer:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeGathering, hoveredCharacterIcon.Character, roleInfo.ResourceType, roleInfo.TargetLocation, enabled));
					break;
				case Role.Guard:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeGuarding, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Lumberjack:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeLumberjack, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Miner:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeMining, hoveredCharacterIcon.Character, null, roleInfo.ResourceType, enabled));
					break;
				case Role.Cook:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeCook, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Trapper:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeTrapper, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Builder:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeBuildingRole, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Capturing:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeCapturing, hoveredCharacterIcon.Character, roleInfo.TargetLocation, enabled));
					break;
				case Role.Repairing:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeRepairing, hoveredCharacterIcon.Character, roleInfo.TargetLocation, enabled));
					break;
				case Role.Crafter:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeCraftingRole, hoveredCharacterIcon.Character, roleInfo.Recipe, null, roleInfo.TargetLocation, enabled));
					break;
				case Role.AnimalFeeder:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeAnimalFeeding, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Organizer:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeOrganizing, hoveredCharacterIcon.Character, null, enabled));
					break;
				case Role.Medic:
					AvailableActions.Add(new AvailableAction(CursorAction.ResumeMedic, hoveredCharacterIcon.Character, null, enabled));
					break;
				}
			}
			else
			{
				switch (roleInfo.Role)
				{
				case Role.Farmer:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseFarming, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Gatherer:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseGathering, hoveredCharacterIcon.Character, roleInfo.ResourceType, roleInfo.TargetLocation, CursorActionDisabledReason.Enabled));
					break;
				case Role.Guard:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseGuarding, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Lumberjack:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseLumberjack, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Miner:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseMining, hoveredCharacterIcon.Character, null, roleInfo.ResourceType, CursorActionDisabledReason.Enabled));
					break;
				case Role.Cook:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseCook, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Trapper:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseTrapper, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Builder:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseBuildingRole, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Capturing:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseCapturing, hoveredCharacterIcon.Character, roleInfo.TargetLocation, CursorActionDisabledReason.Enabled));
					break;
				case Role.Repairing:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseRepairing, hoveredCharacterIcon.Character, roleInfo.TargetLocation, CursorActionDisabledReason.Enabled));
					break;
				case Role.Crafter:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseCraftingRole, hoveredCharacterIcon.Character, roleInfo.Recipe, null, roleInfo.TargetLocation, CursorActionDisabledReason.Enabled));
					break;
				case Role.AnimalFeeder:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseAnimalFeeding, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Organizer:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseOrganizing, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				case Role.Medic:
					AvailableActions.Add(new AvailableAction(CursorAction.PauseMedic, hoveredCharacterIcon.Character, null, CursorActionDisabledReason.Enabled));
					break;
				}
			}
		}
		int count = AvailableActions.Count;
		if (instance.GameCamera.FlyCam && !instance.Editor && controlledCharacter.Community.CanUseRoleCommands())
		{
			AddGlobalSelectActions(controlledCharacter);
		}
		if (instance.CommunityManager.PlayerCommunity.IsAnyMemberPaused())
		{
			if (hoveredCharacterIcon.Character.HasAnyPausedRoles())
			{
				CursorActionDisabledReason enabled2 = ((hoveredCharacterIcon.Character.DownTime != 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.ResumeAllMyRoles, hoveredCharacterIcon.Character, null, enabled2));
			}
			CursorActionDisabledReason enabled3 = ((!instance.CommunityManager.PlayerCommunity.IsAnyMemberPausedAndNotTakingABreak()) ? CursorActionDisabledReason.EveryoneTakingABreak : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(CursorAction.ResumeAllEveryonesRoles, null, null, enabled3));
		}
		if (AvailableActions.Count > count)
		{
			AvailableActions.Insert(count, new AvailableAction(CursorAction.Separator, null));
		}
	}

	public void AddGatherPageActions(GatherPage gatherPage, Character controlledCharacter, TileObject carrier, Equipment item)
	{
		AvailableActions.Add(new AvailableAction(CursorAction.SelectToGather, controlledCharacter, carrier, item, CursorActionDisabledReason.Enabled));
		if (gatherPage.UnityTakeFromInventory.UnityItems.Count > 1)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SelectAllToGather, controlledCharacter, carrier, null, CursorActionDisabledReason.Enabled));
		}
	}

	public void AddMapPageActions(MapPage mapPage, Character controlledCharacter)
	{
		GameTerrain instance = GameTerrain.Instance;
		Hud instance2 = Hud.Instance;
		if (mapPage.Dragging || mapPage.AltDragging)
		{
			return;
		}
		if (instance2.DraggingZone)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SetZone, controlledCharacter, instance2.CursorRayCastResult.Tile, CursorActionDisabledReason.Enabled));
			return;
		}
		InvaderIconBehaviour currentHovered = InvaderIconBehaviour.CurrentHovered;
		if (currentHovered != null)
		{
			FocusUnityObj = InvaderIconBehaviour.CurrentHoveredGameObject;
			InvaderInstance invaderInstance = currentHovered.InvaderInstance;
			if (invaderInstance != null)
			{
				string descriptionString = invaderInstance.Invader.GetDescriptionString(invaderInstance.SourceObject, invaderInstance);
				if (!string.IsNullOrEmpty(descriptionString))
				{
					HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, descriptionString, 0f, leftAligned: true));
				}
			}
			else if (currentHovered.Squad != null && currentHovered.Squad.SquadOwner != null)
			{
				sb.Length = 0;
				currentHovered.Squad.SquadOwner.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
				if (sb.Length > 0)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, sb.ToString(), 0f, leftAligned: true));
				}
			}
		}
		else if (mapPage.AltMenuOpened)
		{
			TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(mapPage.CursorPosWorldXZ);
			PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
			if (localPlayerRecord != null)
			{
				int num = mapPage.IsInRangeOfMarker();
				if (num != -1)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.ClearMarker, localPlayerRecord.MapMarkerLocations[num].Type, localPlayerRecord.MapMarkerLocations[num].Tile, CursorActionDisabledReason.Enabled));
				}
				if (num == -1 || localPlayerRecord.MapMarkerLocations[num].Type != mapPage.MarkerType)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.SetMarker, mapPage.MarkerType, tileCoordForPosXZ, CursorActionDisabledReason.Enabled));
				}
			}
			AddMovementZoneActions(controlledCharacter, tileCoordForPosXZ);
		}
		else if (mapPage.HoveredObj is Town town)
		{
			if (!town.TownName.IsEmpty())
			{
				HeaderActions.Add(new AvailableAction(CursorAction.TownName, null, mapPage.HoveredObj, CursorActionDisabledReason.Enabled));
				if (town.Infection != InfectionType.None)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.TownInfectionType, null, mapPage.HoveredObj, CursorActionDisabledReason.Enabled));
				}
			}
		}
		else
		{
			if (mapPage.HoveredObj == null)
			{
				return;
			}
			HeaderActions.Add(new AvailableAction(CursorAction.DisplayNameAndIcon, null, mapPage.HoveredObj, CursorActionDisabledReason.Enabled));
			bool flag = false;
			if (mapPage.HoveredObj is Building building && building.GetInhabitantCount() > 0)
			{
				for (int i = 0; i < building.Inhabitants.Length; i++)
				{
					Character character = building.Inhabitants[i];
					if (character != null)
					{
						if (!flag)
						{
							HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
							flag = true;
						}
						AvailableActions.Add(new AvailableAction(CursorAction.CharacterNameAndIcon, character, null, CursorActionDisabledReason.Enabled));
					}
				}
			}
			EquipmentContainer inventory = mapPage.HoveredObj.GetInventory();
			if (inventory == null || !(mapPage.HoveredObj.GetMaxInventoryWeight() > 0f))
			{
				return;
			}
			Community community = mapPage.HoveredObj.GetCommunity();
			Character character2 = mapPage.HoveredObj as Character;
			if (community != null && community.CommunityType != CommunityType.Player && (character2 == null || character2.IsConscious) && community.HasAnyActiveMembers() && !Session.Instance.Editor && !InfoScreen.AllowViewInfoOnAnyone)
			{
				return;
			}
			if (!mapPage.HoveredObj.IsInvestigated() && !InfoScreen.GetAllowViewInfoOnAnyone() && !Session.Instance.Editor)
			{
				if (!flag)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
					flag = true;
				}
				AvailableActions.Add(new AvailableAction(CursorAction.NotInvestigated, null, mapPage.HoveredObj, CursorActionDisabledReason.Enabled));
				return;
			}
			if (inventory.IsEmptyExceptForWornItems(mapPage.HoveredObj))
			{
				if (!flag)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
					flag = true;
				}
				AvailableActions.Add(new AvailableAction(CursorAction.Empty, null, mapPage.HoveredObj, CursorActionDisabledReason.Enabled));
				return;
			}
			for (int j = 0; j < inventory.Count; j++)
			{
				Equipment item = inventory.GetItem(j);
				if (item.IsWornOrRemovedForSparring(mapPage.HoveredObj) || item.WasGifted() || item.Concealed)
				{
					continue;
				}
				int amount = item.GetAmount();
				if (!item.CanBeCombined())
				{
					bool flag2 = false;
					int num2 = 0;
					for (int k = 0; k < inventory.Count; k++)
					{
						Equipment item2 = inventory.GetItem(k);
						if (item2.GetPrototype() == item.GetPrototype() && item2.GetLiquidContentsType() == item.GetLiquidContentsType())
						{
							if (k < j)
							{
								flag2 = true;
								break;
							}
							num2++;
						}
					}
					if (flag2)
					{
						continue;
					}
					amount = num2;
				}
				if (!flag)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
					flag = true;
				}
				AvailableActions.Add(new AvailableAction(CursorAction.EquipmentNameAndIcon, null, item, amount, CursorActionDisabledReason.Enabled));
			}
		}
	}

	public void AddInfoPageActions(Character controlledCharacter, out BaseObject outTarget)
	{
		outTarget = null;
		InfoPage infoPage = InfoScreen.Instance.GetCurrentPage() as InfoPage;
		MapPage mapPage = infoPage as MapPage;
		EquipmentBehaviour currentHovered = EquipmentBehaviour.CurrentHovered;
		EquipmentTotalBehaviour currentHovered2 = EquipmentTotalBehaviour.CurrentHovered;
		CharacterIconBehaviour currentHovered3 = CharacterIconBehaviour.CurrentHovered;
		if (CloseIconBehaviour.Hovered != null)
		{
			if (CloseIconBehaviour.Hovered.Tooltip != CursorAction.None)
			{
				FocusUnityObj = CloseIconBehaviour.HoveredGameObject;
				HeaderActions.Add(new AvailableAction(CloseIconBehaviour.Hovered.Tooltip, null, null, CursorActionDisabledReason.Enabled));
			}
		}
		else if (mapPage != null)
		{
			AddMapPageActions(mapPage, controlledCharacter);
		}
		else if (infoPage != null && currentHovered != null)
		{
			Equipment item = currentHovered.Item;
			TileObject carrier = currentHovered.Carrier;
			outTarget = item;
			FocusUnityObj = InfoScreen.CurrentHoveredGameObject;
			if (InfoScreen.Instance.Pourer != null)
			{
				LiquidPrototype liquidContentsType = InfoScreen.Instance.Pourer.GetLiquidContentsType();
				if (liquidContentsType != null)
				{
					CursorActionDisabledReason enabled = ((!(item.GetLiquidCapacity() > 0f)) ? CursorActionDisabledReason.Disabled : CursorActionDisabledReason.Enabled);
					if (item.GetLiquidContentsType() != null && item.GetLiquidContentsType() != liquidContentsType)
					{
						enabled = CursorActionDisabledReason.Disabled;
					}
					if (item.GetLiquidContentsAmount() >= item.GetLiquidCapacity())
					{
						enabled = CursorActionDisabledReason.Disabled;
					}
					if (item.IsBottle() && !liquidContentsType.CanPourIntoBottles)
					{
						enabled = CursorActionDisabledReason.CantPourIntoBottles2;
					}
					AvailableActions.Add(new AvailableAction(CursorAction.PourInto, InfoScreen.Instance.PourerActor, carrier, item, liquidContentsType, enabled));
					return;
				}
				InfoScreen.Instance.CancelPourInto();
			}
			AddEquipmentItemActions(item, infoPage, carrier, controlledCharacter);
		}
		else if (infoPage != null && currentHovered2 != null)
		{
			CommunityPage communityPage = infoPage as CommunityPage;
			if (communityPage != null)
			{
				AddEquipmentTotalActions(currentHovered2, communityPage.CurrentCommunity, controlledCharacter, null, null, HeaderActions, AvailableActions);
			}
		}
		else if (infoPage != null && currentHovered3 != null)
		{
			AddCharacterIconActions(infoPage, controlledCharacter, currentHovered3, out outTarget);
		}
		else if (SkillDisplayBehaviour.Hovered != null)
		{
			FocusUnityObj = SkillDisplayBehaviour.HoveredGameObject;
			DesiredWidth = SkillTooltipWidth;
			HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(SkillsPage.SkillTooltip[(int)SkillDisplayBehaviour.Hovered.SkillType]), SkillTooltipOffset, leftAligned: false));
		}
		else if (BasketItemBehaviour.Hovered != null)
		{
			FocusUnityObj = BasketItemBehaviour.HoveredGameObject;
			HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(HUD_Reset)));
		}
	}

	public void AddInsideBuildingActions(Character controlledCharacter)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		GameTerrain instance3 = GameTerrain.Instance;
		for (int i = 0; i < controlledCharacter.InsideBuilding.GetEntranceDefs().Length; i++)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(controlledCharacter.InsideBuilding.GetEntrancePos(i));
			CursorActionDisabledReason enabled = CursorActionDisabledReason.Enabled;
			Building building = instance3.GetBuilding(tileCoordForPos.x, tileCoordForPos.y);
			if (building != null && building != controlledCharacter.InsideBuilding)
			{
				enabled = building.CanEnterReason(controlledCharacter);
			}
			else
			{
				TileObject fixedObjectOnTile = instance3.GetFixedObjectOnTile(tileCoordForPos.x, tileCoordForPos.y);
				if (fixedObjectOnTile != null && fixedObjectOnTile != controlledCharacter.InsideBuilding && fixedObjectOnTile.IsImpassable(controlledCharacter, 17920, tileCoordForPos))
				{
					enabled = CursorActionDisabledReason.ExitBlocked;
				}
			}
			AvailableActions.Add(new AvailableAction(CursorAction.ExitBuilding, controlledCharacter, controlledCharacter.InsideBuilding, i, enabled));
		}
		if (controlledCharacter.InsideBuilding.HasMultipleExternalSlots())
		{
			InhabitantSlotDef[] inhabitantSlotDefs = controlledCharacter.InsideBuilding.GetInhabitantSlotDefs();
			for (int j = 0; j < inhabitantSlotDefs.Length; j++)
			{
				if (inhabitantSlotDefs[j].External && j < controlledCharacter.InsideBuilding.Inhabitants.Length)
				{
					CursorActionDisabledReason enabled2 = ((controlledCharacter.InsideBuilding.Inhabitants[j] == controlledCharacter) ? CursorActionDisabledReason.Disabled : CursorActionDisabledReason.Enabled);
					AvailableActions.Add(new AvailableAction(CursorAction.ChangeBuildingSlot, controlledCharacter, controlledCharacter.InsideBuilding, j, enabled2));
				}
			}
		}
		if (!(controlledCharacter.InsideBuilding is EnterableVehicle { IsDriveable: not false } enterableVehicle) || !enterableVehicle.GetPropPrototype().VehicleCanHitTheRoad || enterableVehicle.GetPredictedOrElseThisVehicle().IsDrivingTooFastToShowMenu() || !instance2.IsPartyLeader())
		{
			return;
		}
		Vector2 posXZ = enterableVehicle.GetPredictedOrElseThisVehicle().PosXZ;
		if (instance3.GetNearestHitTheRoadPoint(posXZ, HitTheRoadRange, out var _) == null)
		{
			return;
		}
		CursorActionDisabledReason enabled3 = CursorActionDisabledReason.Enabled;
		if (enterableVehicle.GetLiquidAmount() < enterableVehicle.GetLiquidCapacity() * (instance.CurrentStory.Settings.HitTheRoadMinFuelPercent / 100f))
		{
			enabled3 = CursorActionDisabledReason.NeedMoreFuel;
		}
		if (enterableVehicle.GetDamageFraction() > instance.CurrentStory.Settings.HitTheRoadMaxDamagePercent / 100f)
		{
			enabled3 = CursorActionDisabledReason.CantLeaveWhileVehicleDamaged;
		}
		foreach (Character member in Session.Instance.CommunityManager.PlayerCommunity.Members)
		{
			if (member.AliveAndNotZombie && member.IsPlayerAvatar() && member.AliveAndNotZombie && !member.Disappeared && member.InsideBuilding != enterableVehicle)
			{
				enabled3 = CursorActionDisabledReason.NeedPlayerInVehicle;
				break;
			}
		}
		Character[] inhabitants = enterableVehicle.Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && !character.AliveAndNotZombie)
			{
				enabled3 = CursorActionDisabledReason.CantLeaveWithCorpse;
				break;
			}
			if (character != null && character.HasUnbandagedInjury(0))
			{
				enabled3 = CursorActionDisabledReason.CantLeaveWhileBleeding;
				break;
			}
			if (character != null && character.HasAnyInfectedInjuries())
			{
				enabled3 = CursorActionDisabledReason.CantLeaveWhileInfected;
				break;
			}
			if (character != null && character.IsPregnant())
			{
				enabled3 = CursorActionDisabledReason.CantLeaveWhilePregnant;
				break;
			}
		}
		if (CheckForRelationsWeCantLeaveWithout(enterableVehicle, out var _) != null)
		{
			enabled3 = CursorActionDisabledReason.CantLeaveWithoutRelation;
		}
		AvailableActions.Add(new AvailableAction(CursorAction.LoadNewMap, controlledCharacter, controlledCharacter.InsideBuilding, enabled3));
	}

	public void AddCarryingCharacterActions(TileObject target, Character controlledCharacter)
	{
		Building building = target as Building;
		Grave grave = target as Grave;
		Character character = controlledCharacter.CarryingObject as Character;
		if (grave != null && grave.GetUnderConstructionInfo() == null && grave.GraveState == GraveState.Open && character != null && !character.Alive)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Bury, controlledCharacter, controlledCharacter.CarryingObject, grave, CursorActionDisabledReason.Enabled));
		}
		else if (building != null && building.GetUnderConstructionInfo() == null)
		{
			building.AddTalkToInhabitantActions(controlledCharacter, AvailableActions);
			if (!building.IsGuardPost() && character != null)
			{
				CursorActionDisabledReason enabled = building.CanEnterReason(character, controlledCharacter);
				AvailableActions.Add(new AvailableAction(CursorAction.PutInBuilding, controlledCharacter, controlledCharacter.CarryingObject, building, enabled));
			}
		}
		AvailableActions.Add(new AvailableAction(CursorAction.Drop, controlledCharacter, controlledCharacter.CarryingObject, CursorActionDisabledReason.Enabled));
	}

	public void AddPlacingBuildingActions(Character controlledCharacter, Equipment equippedItem)
	{
		RecipeType recipeType = RecipeType.Normal;
		if (equippedItem is Toolbox)
		{
			recipeType = RecipeType.Toolbox;
		}
		if (equippedItem is Shovel)
		{
			recipeType = RecipeType.Shovel;
		}
		bool flag = false;
		foreach (Recipe item in GameImpl.Instance.CurrentRecipesSortedBySkill)
		{
			if (IsPlacingBuildingRecipe != null)
			{
				if (item != IsPlacingBuildingRecipe)
				{
					continue;
				}
			}
			else if (item.RecipeType != recipeType)
			{
				continue;
			}
			if (item.ProductPropPrototype == null)
			{
				continue;
			}
			if (!item.ShownDiscoveredNotification)
			{
				flag = true;
				continue;
			}
			int skillLevelWithEffects = controlledCharacter.GetSkillLevelWithEffects(item.SkillType);
			CursorActionDisabledReason enabled = ((GhostBuilding != null) ? CanBuildHere(controlledCharacter, GhostBuilding, checkOtherCharacters: true, checkCropPatches: false, controlledCharacter.Community) : CursorActionDisabledReason.Enabled);
			if (controlledCharacter.Community.UnderConstructionBuildings.Count >= AvailableAction.MaxQueuedBuildings)
			{
				enabled = CursorActionDisabledReason.FenceBuildingLimitReached;
			}
			if (skillLevelWithEffects < item.SkillLevel)
			{
				enabled = GetSkillTooLowReasonForSkillType(item.SkillType);
			}
			Equipment target = ((IsPlacingBuildingUsingItem != null && !IsPlacingBuildingUsingItem.Deleted && controlledCharacter.InventoryContains(IsPlacingBuildingUsingItem)) ? IsPlacingBuildingUsingItem : null);
			AvailableActions.Add(new AvailableAction(CursorAction.BuildHere, controlledCharacter, item, target, Hud.Instance.CursorRayCastResult.Tile, enabled));
			IsPlacingBuilding = true;
			IsPlacingBuildingThisFrame = true;
		}
		if (flag)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(AvailableAction.HINT_NoBuildingRecipes)));
		}
	}

	public void AddPlacingCropZoneActions(Character controlledCharacter)
	{
		Session instance = Session.Instance;
		CursorActionDisabledReason enabled = CanPlantHere(controlledCharacter, Hud.Instance.CursorRayCastResult.Tile, checkCharacters: false, IsSettingCropPatch);
		if (instance.CropsManager.GetCommunityTilesCount(controlledCharacter.GetCommunityId()) >= controlledCharacter.Community.GetLivingNonZombieMemberCount() * 64)
		{
			enabled = CursorActionDisabledReason.MaxCropPatchTilesReached;
		}
		AvailableCropTypes.Clear();
		for (int i = 0; i < controlledCharacter.Inventory.Count; i++)
		{
			Equipment item = controlledCharacter.Inventory.GetItem(i);
			if (item.GetSeedForPlantType() != null && !AvailableCropTypes.Contains(item.GetSeedForPlantType()))
			{
				AvailableCropTypes.Add(item.GetSeedForPlantType());
			}
		}
		if (controlledCharacter.Community != null)
		{
			foreach (CropPatch patch in instance.CropsManager.Patches)
			{
				if (patch.CommunityId == controlledCharacter.Community.Id && !AvailableCropTypes.Contains(patch.CropType))
				{
					AvailableCropTypes.Add(patch.CropType);
				}
			}
		}
		for (int j = 0; j < AvailableCropTypes.Count; j++)
		{
			EquipmentPrototype equipmentPrototype = ((AvailableCropTypes[j].HarvestSeedsPrototype != null) ? AvailableCropTypes[j].HarvestSeedsPrototype : AvailableCropTypes[j].HarvestPrototype);
			if (equipmentPrototype != null)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SetCropsPatchHere, controlledCharacter, equipmentPrototype, Hud.Instance.CursorRayCastResult.Tile, enabled));
			}
		}
		AvailableCropTypes.Clear();
	}

	public void AddUnderConstructionActions(TileObject target, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		UnderConstructionInfo underConstructionInfo = target.GetUnderConstructionInfo();
		CursorActionDisabledReason enabled = CursorActionDisabledReason.Enabled;
		if (!controlledCharacter.CanUseRecipe(underConstructionInfo.Recipe))
		{
			enabled = GetSkillTooLowReasonForSkillType(underConstructionInfo.Recipe.SkillType);
		}
		else if (underConstructionInfo.Recipe.RecipeType == RecipeType.Toolbox && controlledCharacter.Inventory.GetToolbox() == null)
		{
			enabled = CursorActionDisabledReason.NeedToolbox;
		}
		else if (underConstructionInfo.Recipe.RecipeType == RecipeType.Shovel && controlledCharacter.Inventory.GetShovel() == null)
		{
			enabled = CursorActionDisabledReason.NeedShovel;
		}
		AvailableActions.Add(new AvailableAction(controlledCharacter.HasRole(Role.Builder) ? CursorAction.ResumeBuilding : CursorAction.SetBuilderRole, controlledCharacter, underConstructionInfo.Recipe, target, enabled));
	}

	public void AddBuildingActions(Building targetBuilding, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Hud instance = Hud.Instance;
		CursorActionDisabledReason enabled = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		CursorActionDisabledReason cursorActionDisabledReason = targetBuilding.CanEnterReason(controlledCharacter);
		bool flag = ShowNotInvestigatedOnScavengeAction(controlledCharacter, targetBuilding);
		if (cursorActionDisabledReason == CursorActionDisabledReason.Enabled)
		{
			bool flag2 = controlledCharacter.InsideBuilding == targetBuilding;
			foreach (Character selectedCharacter in instance.SelectedCharacters)
			{
				flag2 &= selectedCharacter.InsideBuilding == targetBuilding;
			}
			if (!flag2)
			{
				AvailableActions.Add(new AvailableAction(flag ? CursorAction.EnterBuildingNotInvestigated : CursorAction.EnterBuilding, controlledCharacter, targetBuilding, enabledIfConscious));
			}
			targetBuilding.AddTalkToInhabitantActions(controlledCharacter, AvailableActions);
		}
		else if (!targetBuilding.AddTalkToInhabitantActions(controlledCharacter, AvailableActions) && cursorActionDisabledReason != CursorActionDisabledReason.Disabled)
		{
			AvailableActions.Add(new AvailableAction(flag ? CursorAction.EnterBuildingNotInvestigated : CursorAction.EnterBuilding, controlledCharacter, targetBuilding, cursorActionDisabledReason));
		}
		if (cursorActionDisabledReason == CursorActionDisabledReason.Enabled || cursorActionDisabledReason == CursorActionDisabledReason.BuildingFull)
		{
			RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Guard);
			if (roleInfo.Valid && targetBuilding.Tile == roleInfo.TargetLocation)
			{
				cursorActionDisabledReason = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeGuarding : CursorAction.CancelGuarding, controlledCharacter, targetBuilding, cursorActionDisabledReason));
			}
			else if (CanGuard(controlledCharacter, targetBuilding) && controlledCharacter.Community.CanUseRoleCommands())
			{
				cursorActionDisabledReason = ((!roleInfo.Valid && controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.Guard, controlledCharacter, targetBuilding, cursorActionDisabledReason));
			}
			else if (targetBuilding.Investigated && targetBuilding.Inventory.Count > 0 && controlledCharacter.Community.Buildings.Count > 0 && (targetBuilding.Community == null || !targetBuilding.Community.HasAnyActiveMembers()) && controlledCharacter.Community.CanUseRoleCommands())
			{
				cursorActionDisabledReason = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.Gather, controlledCharacter, targetBuilding, cursorActionDisabledReason));
			}
		}
		for (int i = 0; i < controlledCharacter.Roles.Count; i++)
		{
			if (controlledCharacter.Roles[i].Role == Role.Gatherer && targetBuilding.Tile == controlledCharacter.Roles[i].TargetLocation)
			{
				cursorActionDisabledReason = ((controlledCharacter.Roles[i].Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(controlledCharacter.Roles[i].Paused ? CursorAction.ResumeGathering : CursorAction.CancelGathering, controlledCharacter, controlledCharacter.Roles[i].ResourceType, targetBuilding.Tile, cursorActionDisabledReason));
			}
		}
		AddBuildingFollowActions(controlledCharacter, targetBuilding);
		if (targetBuilding.CanBeEmptied() && targetBuilding.GetLiquidType() != null && targetBuilding.GetLiquidAmount() > 0f)
		{
			AddFillFromPropActions(targetBuilding, targetBuilding.GetLiquidType(), controlledCharacter, equippedItem, ref equipmentActions);
		}
		if (!IsStealingToTakeFrom(controlledCharacter, targetBuilding) && (!flag || Session.Instance.CommunityManager.GetRelationship(controlledCharacter.Community, targetBuilding.Community) == CommunityRelationshipType.Hostile) && !targetBuilding.HasInhabitantsWhoBlockMeFromTakingThings(controlledCharacter.Community))
		{
			if (CanSeeTakeAllOption(controlledCharacter, targetBuilding))
			{
				CursorActionDisabledReason enabled2 = CanTakeAll(controlledCharacter, targetBuilding);
				AvailableActions.Add(new AvailableAction(CursorAction.TakeAll, controlledCharacter, targetBuilding, enabled2));
			}
			AvailableActions.Add(new AvailableAction(GetScavengeAction(controlledCharacter, targetBuilding), controlledCharacter, targetBuilding, enabled));
		}
		AddPourIntoAction(controlledCharacter, targetBuilding, equippedItem, ref equipmentActions);
		if (targetBuilding is Mine mine && targetBuilding.CouldEnterIfNotFull(controlledCharacter))
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
			Equipment pickaxe = controlledCharacter.Inventory.GetPickaxe();
			if (pickaxe != null && controlledCharacter.FindActiveGoal(GoalType.MoveToAndMine) == null)
			{
				for (int j = 0; j < 4; j++)
				{
					MineralType mineralType = (MineralType)j;
					if (!mine.HasRichDeposits(mineralType))
					{
						continue;
					}
					EquipmentPrototype equipmentPrototype = EquipmentPrototype.MiningResources[(int)mineralType];
					if (equipmentPrototype != null)
					{
						CursorActionDisabledReason enabled3 = (controlledCharacter.HasInventorySpaceFor(equipmentPrototype.Weight) ? enabledIfConscious : CursorActionDisabledReason.InventoryFullSpaceNeeded);
						AvailableActions.Add(new AvailableAction(CursorAction.Mine, controlledCharacter, targetBuilding, pickaxe, equipmentPrototype, enabled3));
						if (pickaxe == equippedItem)
						{
							equipmentActions++;
						}
					}
				}
				AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
			}
			bool flag3 = false;
			for (int k = 0; k < 4; k++)
			{
				MineralType mineralType2 = (MineralType)k;
				if (!mine.HasRichDeposits(mineralType2))
				{
					continue;
				}
				EquipmentPrototype equipmentPrototype2 = EquipmentPrototype.MiningResources[(int)mineralType2];
				if (equipmentPrototype2 != null)
				{
					int roleIndex = controlledCharacter.GetRoleIndex(new RoleInfo(Role.Miner, equipmentPrototype2));
					if (roleIndex != -1)
					{
						cursorActionDisabledReason = ((controlledCharacter.Roles[roleIndex].Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(controlledCharacter.Roles[roleIndex].Paused ? CursorAction.ResumeMining : CursorAction.CancelMining, controlledCharacter, null, equipmentPrototype2, cursorActionDisabledReason));
						flag3 = true;
					}
					else if (controlledCharacter.Community.CanUseRoleCommands())
					{
						cursorActionDisabledReason = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(CursorAction.SetMiner, controlledCharacter, targetBuilding, equipmentPrototype2, cursorActionDisabledReason));
						flag3 = true;
					}
				}
			}
			if (flag3 && targetBuilding.Community == controlledCharacter.Community)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
			}
		}
		if (targetBuilding.Community == controlledCharacter.Community && targetBuilding.StoragePolicies != null && targetBuilding.StoragePolicies.Count > 0)
		{
			RoleInfo roleInfo2 = controlledCharacter.GetRoleInfo(Role.Organizer);
			if (roleInfo2.Valid)
			{
				cursorActionDisabledReason = ((roleInfo2.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(roleInfo2.Paused ? CursorAction.ResumeOrganizing : CursorAction.CancelOrganizing, controlledCharacter, targetBuilding, cursorActionDisabledReason));
			}
			else if (controlledCharacter.Community.CanUseRoleCommands())
			{
				cursorActionDisabledReason = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.SetOrganizer, controlledCharacter, targetBuilding, cursorActionDisabledReason));
			}
		}
	}

	public void AddCropActions(PlantableCrop targetCrop, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		if (targetCrop.CanBeHarvested() && !targetCrop.IsDead())
		{
			CursorAction actionType = (IsStealingToTakeFrom(controlledCharacter, targetCrop) ? CursorAction.StealCrops : CursorAction.Harvest);
			CursorActionDisabledReason enabled = (controlledCharacter.HasInventorySpaceFor(targetCrop.GetHarvestWeight()) ? enabledIfConscious : CursorActionDisabledReason.InventoryFullSpaceNeeded);
			AvailableActions.Add(new AvailableAction(actionType, controlledCharacter, targetCrop, targetCrop.GetHarvestPrototype(), enabled));
		}
		if (!targetCrop.IsDead())
		{
			CursorActionDisabledReason cursorActionDisabledReason = CursorActionDisabledReason.Disabled;
			if (equippedItem != null && equippedItem.GetLiquidCapacity() > 0f)
			{
				cursorActionDisabledReason = ((equippedItem.GetLiquidContentsType() == null) ? CursorActionDisabledReason.LiquidContainerIsEmpty : ((equippedItem.GetLiquidContentsType() == LiquidPrototype.Water) ? enabledIfConscious : CursorActionDisabledReason.NeedWaterForCrops));
				AvailableActions.Add(new AvailableAction(CursorAction.WaterCrops, controlledCharacter, targetCrop, equippedItem, cursorActionDisabledReason));
				equipmentActions++;
			}
			if (cursorActionDisabledReason != enabledIfConscious)
			{
				Equipment bestWateringCan = controlledCharacter.Inventory.GetBestWateringCan(mustHaveWater: true);
				if (bestWateringCan != null && bestWateringCan != equippedItem)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.WaterCrops, controlledCharacter, targetCrop, bestWateringCan, enabledIfConscious));
				}
			}
		}
		if ((!targetCrop.CanBeHarvested() || targetCrop.IsDead()) && targetCrop.CommunityId == controlledCharacter.GetCommunityId())
		{
			AvailableActions.Add(new AvailableAction(CursorAction.ClearCrops, controlledCharacter, targetCrop, enabledIfConscious));
		}
	}

	public void AddTreeActions(TreeProp targetTree, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Equipment axe = controlledCharacter.Inventory.GetAxe();
		if (axe == null)
		{
			return;
		}
		if (controlledCharacter.FindActiveGoal(GoalType.MoveToAndChop) == null && EquipmentPrototype.Wood != null)
		{
			CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
			AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetTree) ? CursorAction.ChopTreeStealing : CursorAction.ChopTree, controlledCharacter, targetTree, axe, EquipmentPrototype.Wood, enabledIfConscious));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
		RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Lumberjack);
		if (roleInfo.Valid)
		{
			CursorActionDisabledReason enabled = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeLumberjack : CursorAction.CancelLumberjack, controlledCharacter, targetTree, enabled));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
		else if (controlledCharacter.Community.CanUseRoleCommands())
		{
			CursorActionDisabledReason enabled2 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(CursorAction.SetLumberjack, controlledCharacter, targetTree, enabled2));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
	}

	public void AddFallenTreeActions(FallenTreeProp targetFallenTree, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Equipment axe = controlledCharacter.Inventory.GetAxe();
		if (axe == null)
		{
			return;
		}
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		if (targetFallenTree.IsStump())
		{
			if (controlledCharacter.FindActiveGoal(GoalType.MoveToAndChop) == null)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.ClearBush, controlledCharacter, targetFallenTree, axe, enabledIfConscious));
				if (axe == equippedItem)
				{
					equipmentActions++;
				}
			}
			return;
		}
		if (controlledCharacter.FindActiveGoal(GoalType.MoveToAndChop) == null && EquipmentPrototype.Wood != null)
		{
			CursorActionDisabledReason enabled = (controlledCharacter.HasInventorySpaceFor(EquipmentPrototype.Wood.Weight) ? enabledIfConscious : CursorActionDisabledReason.InventoryFullSpaceNeeded);
			AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetFallenTree) ? CursorAction.ChopLogStealing : CursorAction.ChopLog, controlledCharacter, targetFallenTree, axe, EquipmentPrototype.Wood, enabled));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
		RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Lumberjack);
		if (roleInfo.Valid)
		{
			CursorActionDisabledReason enabled2 = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeLumberjack : CursorAction.CancelLumberjack, controlledCharacter, targetFallenTree, enabled2));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
		else if (controlledCharacter.Community.CanUseRoleCommands())
		{
			CursorActionDisabledReason enabled3 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(CursorAction.SetLumberjack, controlledCharacter, targetFallenTree, enabled3));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
	}

	public void AddBushActions(Bush targetBush, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Equipment axe = controlledCharacter.Inventory.GetAxe();
		if (axe != null && controlledCharacter.FindActiveGoal(GoalType.MoveToAndChop) == null)
		{
			CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
			AvailableActions.Add(new AvailableAction(CursorAction.ClearBush, controlledCharacter, targetBush, axe, enabledIfConscious));
			if (axe == equippedItem)
			{
				equipmentActions++;
			}
		}
	}

	public void AddMiningActions(TileObject target, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Equipment pickaxe = controlledCharacter.Inventory.GetPickaxe();
		if (pickaxe == null)
		{
			return;
		}
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		if (controlledCharacter.FindActiveGoal(GoalType.MoveToAndMine) == null)
		{
			CursorActionDisabledReason enabled = (controlledCharacter.HasInventorySpaceFor(target.GetMiningResourceType().Weight) ? enabledIfConscious : CursorActionDisabledReason.InventoryFullSpaceNeeded);
			AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, target) ? CursorAction.MineStealing : CursorAction.Mine, controlledCharacter, target, pickaxe, target.GetMiningResourceType(), enabled));
			if (pickaxe == equippedItem)
			{
				equipmentActions++;
			}
		}
		RoleInfo roleInfo = controlledCharacter.GetRoleInfo(new RoleInfo(Role.Miner, target.GetMiningResourceType()));
		if (roleInfo.Valid)
		{
			CursorActionDisabledReason enabled2 = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeMining : CursorAction.CancelMining, controlledCharacter, null, target.GetMiningResourceType(), enabled2));
			if (pickaxe == equippedItem)
			{
				equipmentActions++;
			}
		}
		else if (controlledCharacter.Community.CanUseRoleCommands())
		{
			CursorActionDisabledReason enabled3 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
			AvailableActions.Add(new AvailableAction(CursorAction.SetMiner, controlledCharacter, target, target.GetMiningResourceType(), enabled3));
			if (pickaxe == equippedItem)
			{
				equipmentActions++;
			}
		}
	}

	public void AddFillFromPropActions(Prop targetProp, LiquidPrototype liquid, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(controlledCharacter);
		CursorActionDisabledReason cursorActionDisabledReason = CursorActionDisabledReason.Disabled;
		if (equippedItem != null && equippedItem.GetLiquidCapacity() > 0f)
		{
			cursorActionDisabledReason = enabledIfConscious;
			if (equippedItem.GetLiquidContentsType() != liquid && equippedItem.GetLiquidContentsType() != null)
			{
				cursorActionDisabledReason = CursorActionDisabledReason.AlreadyContainsOtherLiquid;
			}
			else if (equippedItem.GetLiquidContentsAmount() >= equippedItem.GetLiquidCapacity())
			{
				cursorActionDisabledReason = CursorActionDisabledReason.AlreadyFullOfWater;
			}
			AvailableActions.Add(new AvailableAction(CursorAction.FillFromWell, controlledCharacter, targetProp, equippedItem, cursorActionDisabledReason));
			equipmentActions++;
		}
		if (cursorActionDisabledReason != enabledIfConscious)
		{
			Equipment bestLiquidContainerToFill = controlledCharacter.Inventory.GetBestLiquidContainerToFill(liquid);
			if (bestLiquidContainerToFill != null && bestLiquidContainerToFill != equippedItem)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.FillFromWell, controlledCharacter, targetProp, bestLiquidContainerToFill, enabledIfConscious));
			}
		}
	}

	public void AddGateActions(Gate targetGate, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		Session instance = Session.Instance;
		if (targetGate.GateState == GateState.Locked && controlledCharacter.Inventory.FindItemOfType(targetGate.KeyProto) != null)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Unlock, controlledCharacter, targetGate, CursorActionDisabledReason.Enabled));
		}
		switch (targetGate.GateState)
		{
		case GateState.Open:
			if (!instance.GameCamera.FlyCam || targetGate.Community == controlledCharacter.Community)
			{
				CursorActionDisabledReason cursorActionDisabledReason2 = targetGate.CanOpenMeReason(controlledCharacter, fromAI: false, dontOpenOurGates: false);
				if (cursorActionDisabledReason2 == CursorActionDisabledReason.GatePolicyIsAlwaysClosed)
				{
					cursorActionDisabledReason2 = CursorActionDisabledReason.Enabled;
				}
				if (cursorActionDisabledReason2 == CursorActionDisabledReason.Enabled && targetGate.GatePolicy == GatePolicy.CompletelyOpen)
				{
					cursorActionDisabledReason2 = CursorActionDisabledReason.Disabled;
				}
				AvailableActions.Add(new AvailableAction(CursorAction.Close, controlledCharacter, targetGate, cursorActionDisabledReason2));
			}
			break;
		case GateState.Closed:
		case GateState.Locked:
		{
			CursorActionDisabledReason cursorActionDisabledReason = targetGate.CanOpenMeReason(controlledCharacter, fromAI: false, dontOpenOurGates: false);
			if (cursorActionDisabledReason == CursorActionDisabledReason.GatePolicyIsAlwaysClosed && targetGate.Community == controlledCharacter.Community)
			{
				cursorActionDisabledReason = CursorActionDisabledReason.Enabled;
			}
			if (cursorActionDisabledReason != CursorActionDisabledReason.Enabled && cursorActionDisabledReason != CursorActionDisabledReason.CantOpenEnemyGate && targetGate.Community != null && controlledCharacter.Community != null && targetGate.Community.HasAnyActiveMembers())
			{
				CommunityRelationshipType relationship = controlledCharacter.Community.GetRelationship(targetGate.Community);
				if (relationship == CommunityRelationshipType.Unknown || (relationship == CommunityRelationshipType.Introducing && !targetGate.Community.IsAnyoneIntroducing(controlledCharacter.Community)))
				{
					CursorActionDisabledReason enabled = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
					AvailableActions.Add(new AvailableAction(CursorAction.Knock, controlledCharacter, targetGate, enabled));
				}
			}
			if (!instance.GameCamera.FlyCam || targetGate.Community == controlledCharacter.Community)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.Open, controlledCharacter, targetGate, cursorActionDisabledReason));
			}
			break;
		}
		}
		if (targetGate.Community == controlledCharacter.Community)
		{
			AvailableActions.Add(new AvailableAction(CursorAction.SetGatePolicy, controlledCharacter, targetGate, CursorActionDisabledReason.Enabled));
			if (controlledCharacter.Community.HasAnyLivingNonZombieMembersOfSpecies(BaseObjectType.Chicken))
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SetBlockAnimals, controlledCharacter, targetGate, (!targetGate.BlockAnimals) ? 1 : 0, CursorActionDisabledReason.Enabled));
			}
		}
	}

	public void AddTrapActions(TileObject target, ITrap targetTrap, Prop targetProp, Character controlledCharacter, Equipment equippedItem, ref int equipmentActions)
	{
		CursorActionDisabledReason cursorActionDisabledReason = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		if (targetProp != null && targetProp.Inventory.Count > 0)
		{
			if (CanSeeTakeAllOption(controlledCharacter, targetProp))
			{
				CursorActionDisabledReason enabled = CanTakeAll(controlledCharacter, targetProp);
				AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, target) ? CursorAction.StealAll : CursorAction.TakeAll, controlledCharacter, targetProp, enabled));
			}
			AvailableActions.Add(new AvailableAction(GetScavengeAction(controlledCharacter, target), controlledCharacter, targetProp, cursorActionDisabledReason));
		}
		if (targetTrap.CanResetTrap())
		{
			EquipmentPrototype equipmentNeededForReset = targetTrap.GetEquipmentNeededForReset();
			CursorActionDisabledReason enabled2 = ((equipmentNeededForReset != null && controlledCharacter.Inventory.FindItemOfType(equipmentNeededForReset) == null) ? CursorActionDisabledReason.EquipmentNeeded : cursorActionDisabledReason);
			AvailableActions.Add(new AvailableAction(CursorAction.ResetTrap, controlledCharacter, target, equipmentNeededForReset, enabled2));
		}
		if (target.GetCommunity() == controlledCharacter.Community)
		{
			RoleInfo roleInfo = controlledCharacter.GetRoleInfo(Role.Trapper);
			if (roleInfo.Valid)
			{
				CursorActionDisabledReason enabled3 = ((roleInfo.Paused && controlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeTrapper : CursorAction.CancelTrapper, controlledCharacter, target, enabled3));
			}
			else if (controlledCharacter.Community.CanUseRoleCommands())
			{
				CursorActionDisabledReason enabled4 = ((controlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
				AvailableActions.Add(new AvailableAction(CursorAction.SetTrapper, controlledCharacter, target, enabled4));
			}
		}
	}

	public void AddCharacterActions(Character targetCharacter, Character controlledCharacter, Character interestingSpeaker, ref bool wantTargetHeader, ref bool canTalk)
	{
		Session instance = Session.Instance;
		Hud instance2 = Hud.Instance;
		CursorActionDisabledReason cursorActionDisabledReason = ((controlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		if (!targetCharacter.IsAwake)
		{
			if (instance.GameCamera.FlyCam)
			{
				if (targetCharacter.IsControllableByPlayer())
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Control, targetCharacter, null, CursorActionDisabledReason.Enabled));
				}
				AddCharacterSelectActions(targetCharacter);
			}
			if (targetCharacter == controlledCharacter)
			{
				return;
			}
			if (CanSeeTakeAllOption(controlledCharacter, targetCharacter))
			{
				CursorActionDisabledReason enabled = CanTakeAll(controlledCharacter, targetCharacter);
				AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetCharacter) ? CursorAction.StealAll : CursorAction.TakeAll, controlledCharacter, targetCharacter, enabled));
			}
			AvailableActions.Add(new AvailableAction(GetScavengeAction(controlledCharacter, targetCharacter), controlledCharacter, targetCharacter, cursorActionDisabledReason));
			if (targetCharacter.Consciousness == Consciousness.Unconscious)
			{
				if (targetCharacter.HasUnbandagedInjury(controlledCharacter.GetSkillLevelWithEffects(SkillType.Medicine)))
				{
					Equipment equipment = controlledCharacter.Inventory.FindItemWithHighestBandageLevel();
					CursorActionDisabledReason enabled2 = ((equipment != null && equipment.GetBandageLevel() > targetCharacter.GetInjuryBandageLevel()) ? cursorActionDisabledReason : CursorActionDisabledReason.NeedBandage);
					AvailableActions.Add(new AvailableAction(CursorAction.Bandage, controlledCharacter, targetCharacter, equipment, enabled2));
				}
				if (!targetCharacter.Zombie)
				{
					if (targetCharacter.HasAnyInfectedInjuries())
					{
						Equipment equipment2 = controlledCharacter.Inventory.FindAntigenForInfectionType(targetCharacter.GetWorstInfectionTypeInProgression());
						CursorActionDisabledReason enabled3 = ((equipment2 != null) ? cursorActionDisabledReason : CursorActionDisabledReason.NeedAntigen);
						AvailableActions.Add(new AvailableAction(CursorAction.Inject, controlledCharacter, targetCharacter, equipment2, enabled3));
					}
					if (targetCharacter.GetThirst() >= Character.ThirstyTime)
					{
						Equipment equipment3 = controlledCharacter.Inventory.FindBestItemWithLiquid(LiquidPrototype.Water);
						CursorActionDisabledReason enabled4 = ((equipment3 != null) ? cursorActionDisabledReason : CursorActionDisabledReason.NeedWater);
						AvailableActions.Add(new AvailableAction(CursorAction.GiveWater, controlledCharacter, targetCharacter, equipment3, enabled4));
					}
					bool flag = false;
					foreach (Equipment content in controlledCharacter.Inventory.Contents)
					{
						if (content.GetNutrition() > 0f)
						{
							if (targetCharacter.GetHunger() >= content.GetNutrition() || content.GetLiquidContentsType() != null)
							{
								AvailableActions.Add(new AvailableAction(CursorAction.Feed, controlledCharacter, targetCharacter, content, cursorActionDisabledReason));
							}
							flag = true;
						}
					}
					if (!flag && targetCharacter.GetHunger() >= Character.HungryTime)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.Feed, controlledCharacter, targetCharacter, null, CursorActionDisabledReason.NeedFood));
					}
				}
				if (targetCharacter.Community != controlledCharacter.Community || (controlledCharacter.SparringPartner == targetCharacter && controlledCharacter.SparringType == SparringType.FightToTheDeath))
				{
					CursorActionDisabledReason enabled5 = (controlledCharacter.RefuseToAttackTarget(targetCharacter) ? CursorActionDisabledReason.RefuseToAttack : cursorActionDisabledReason);
					Equipment huntingKnife = controlledCharacter.Inventory.GetHuntingKnife();
					if (huntingKnife != null && targetCharacter.GetBaseObjectType() == BaseObjectType.Human)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.KillUnconscious, controlledCharacter, targetCharacter, huntingKnife, enabled5));
					}
					AvailableActions.Add(new AvailableAction(CursorAction.BludgeonUnconscious, controlledCharacter, targetCharacter, null, enabled5));
				}
			}
			if (CanPickUp(controlledCharacter, targetCharacter))
			{
				AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(controlledCharacter, targetCharacter, pickUp: true) ? CursorAction.PickUpSteal : CursorAction.PickUp, controlledCharacter, targetCharacter, cursorActionDisabledReason));
			}
		}
		else if (CanChokeHold(controlledCharacter, targetCharacter))
		{
			CursorActionDisabledReason enabled6 = cursorActionDisabledReason;
			if (controlledCharacter.RefuseToAttackTarget(targetCharacter))
			{
				enabled6 = CursorActionDisabledReason.RefuseToAttack;
			}
			Equipment huntingKnife2 = controlledCharacter.Inventory.GetHuntingKnife();
			if (huntingKnife2 != null)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SlitThroat, controlledCharacter, targetCharacter, huntingKnife2, enabled6));
			}
			else
			{
				HintManager.Instance.ShowNeedKnifeToAssassinateHint();
			}
			AvailableActions.Add(new AvailableAction(CursorAction.ChokeHold, controlledCharacter, targetCharacter, null, enabled6));
			AvailableActions.Add(new AvailableAction(CursorAction.Pickpocket, controlledCharacter, targetCharacter, cursorActionDisabledReason));
		}
		else if (CanRestrain(controlledCharacter, targetCharacter))
		{
			AvailableActions.Add(new AvailableAction(CursorAction.Restrain, controlledCharacter, targetCharacter, cursorActionDisabledReason));
		}
		else
		{
			if (!CanSpeakToTarget(targetCharacter))
			{
				return;
			}
			if (instance.GameCamera.FlyCam)
			{
				if (targetCharacter.IsControllableByPlayer())
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Control, targetCharacter, null, CursorActionDisabledReason.Enabled));
				}
				AddCharacterSelectActions(targetCharacter);
				if (targetCharacter == controlledCharacter)
				{
					AddMovementZoneActions(controlledCharacter, instance2.CursorRayCastResult.Tile);
				}
			}
			if (!CanShowDialogOptions(controlledCharacter, targetCharacter))
			{
				return;
			}
			Conversation conversation = targetCharacter.FindActiveGoal(GoalType.Conversation) as Conversation;
			Conversation conversationWith = GetConversationWith(controlledCharacter, targetCharacter);
			if (conversationWith != null)
			{
				wantTargetHeader = false;
			}
			if (interestingSpeaker != null && interestingSpeaker != targetCharacter)
			{
				return;
			}
			if (conversation == null && !IsTargetCharacterInCombat(targetCharacter) && targetCharacter.FindActiveGoal(GoalType.MoveToAndInteractGoal) == null)
			{
				canTalk = true;
			}
			else if (conversationWith != null && conversationWith.IsWaitingForReply())
			{
				if (!(controlledCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation2) || conversation2.GetTargetCharacter() != targetCharacter || !conversation2.IsSpeaking())
				{
					canTalk = true;
				}
			}
			else if (conversationWith != null && conversationWith.IsSkippable(targetCharacter))
			{
				CanSkipConversationWith = targetCharacter;
			}
			else
			{
				if (conversationWith != null || conversation == null)
				{
					return;
				}
				if (conversation.IsWaitingForReply() || conversation.IsWaitingForPlayerToSelectAReply())
				{
					Character targetCharacter2 = conversation.GetTargetCharacter();
					if (targetCharacter2 != null && targetCharacter2.FindActiveGoal(GoalType.Conversation) is Conversation conversation3 && (conversation3.IsWaitingForReply() || conversation3.IsWaitingForPlayerToSelectAReply()))
					{
						canTalk = true;
					}
				}
				else if (conversation.IsMovingToTarget() && conversation.OpeningSpeech != null && conversation.OpeningSpeech.Importance < Importance.Normal)
				{
					canTalk = true;
				}
			}
		}
	}

	public void GetAvailableActions(out BaseObject outTarget, out bool wantTargetHeader)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		Hud instance3 = Hud.Instance;
		InfoScreen instance4 = InfoScreen.Instance;
		GameTerrain instance5 = GameTerrain.Instance;
		_ = Session.Instance.CommunityManager;
		Character localControlledCharacter = instance3.LocalControlledCharacter;
		bool flag = instance3.GuessIfIAmControllingLocalControlledCharacter();
		Character mostInterestingSpeaker = StoryManager.Instance.GetMostInterestingSpeaker(skipIfLowerPriorityThanLocalPlayerConversation: true, skipIfLowerPriorityThanAnyPlayerConversation: false);
		DesiredWidth = 0f;
		ClearActions();
		VisibleButUnavailableActions.Clear();
		CanSkipConversationWith = null;
		IsPlacingBuildingThisFrame = false;
		wantTargetHeader = false;
		if (instance2.IsDebugMenuOpen() || instance.IsMenuOpen() || instance.IsDialogOpen() || instance3.ReallyDragging || instance3.ReallyDragSelecting)
		{
			outTarget = null;
			return;
		}
		if (instance4.Active)
		{
			outTarget = null;
			if (instance4.ActiveAndFullyTransitionedIn)
			{
				AddInfoPageActions(localControlledCharacter, out outTarget);
			}
			if (AvailableActions.Count == 0 && HeaderActions.Count > 0 && HeaderActions[HeaderActions.Count - 1].ActionType == CursorAction.Separator)
			{
				HeaderActions.RemoveAt(HeaderActions.Count - 1);
			}
			return;
		}
		bool isPlacingBuilding = IsPlacingBuilding;
		IsPlacingBuilding = false;
		PropPrototype isSettingCropPatch = IsSettingCropPatch;
		IsSettingCropPatch = null;
		bool showGiftMenu = ShowGiftMenu;
		ShowGiftMenu = false;
		CraftingProp showCraftMenuFor = ShowCraftMenuFor;
		ShowCraftMenuFor = null;
		CraftingProp showDisassembleMenuFor = ShowDisassembleMenuFor;
		ShowDisassembleMenuFor = null;
		TileObject localTargetObject = instance3.GetLocalTargetObject();
		Character character = localTargetObject as Character;
		Animal animal = localTargetObject as Animal;
		Prop prop = localTargetObject as Prop;
		Building building = localTargetObject as Building;
		PlantableCrop plantableCrop = localTargetObject as PlantableCrop;
		TreeProp treeProp = localTargetObject as TreeProp;
		Flower flower = localTargetObject as Flower;
		FallenTreeProp fallenTreeProp = localTargetObject as FallenTreeProp;
		Bush bush = localTargetObject as Bush;
		Well well = localTargetObject as Well;
		Gate gate = localTargetObject as Gate;
		Campfire campfire = localTargetObject as Campfire;
		CraftingProp craftingProp = localTargetObject as CraftingProp;
		EnterableVehicle enterableVehicle = localTargetObject as EnterableVehicle;
		AnimalFeederProp animalFeederProp = localTargetObject as AnimalFeederProp;
		AnimalDrinkerProp animalDrinkerProp = localTargetObject as AnimalDrinkerProp;
		ITrap trap = localTargetObject as ITrap;
		outTarget = localTargetObject;
		if (instance2.Editor)
		{
			if (character != null)
			{
				wantTargetHeader = true;
				AvailableActions.Add(new AvailableAction(CursorAction.EditorSelect, character, null, CursorActionDisabledReason.Enabled));
			}
		}
		else
		{
			if (localControlledCharacter == null || (instance3.LocalWantLockOnTarget && (localControlledCharacter.CurrentActionAnim != ActionAnim.HandsUp || character == null || GetConversationWith(localControlledCharacter, localTargetObject) == null)) || ShowEquipmentSelectTimer > 0f)
			{
				return;
			}
			if (localTargetObject != null && flower == null)
			{
				wantTargetHeader = true;
			}
			bool canTalk = false;
			bool flag2 = flag && localControlledCharacter.AliveAndNotZombie && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.None;
			if (instance2.GameCamera.FlyCam)
			{
				foreach (Character selectedCharacter in instance3.SelectedCharacters)
				{
					flag2 |= selectedCharacter.AliveAndNotZombie && selectedCharacter.AIOverridesControl() == AIOverridesControlReason.None;
				}
			}
			if (!flag2)
			{
				if (instance2.GameCamera.FlyCam && character != null)
				{
					if (character.IsControllableByPlayer())
					{
						AvailableActions.Add(new AvailableAction(CursorAction.Control, character, null, CursorActionDisabledReason.Enabled));
					}
					AddCharacterSelectActions(character);
				}
				if (CanSpeakToTarget(character) && CanShowDialogOptions(localControlledCharacter, character) && localControlledCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation)
				{
					if (conversation.IsWaitingForPlayerToSelectAReply())
					{
						canTalk = true;
					}
					if (conversation.IsSkippable(localControlledCharacter))
					{
						CanSkipConversationWith = character;
					}
					wantTargetHeader = false;
				}
				if (mostInterestingSpeaker != null)
				{
					if (mostInterestingSpeaker.FindActiveGoal(GoalType.Conversation) is Conversation conversation2 && conversation2.IsSkippable(mostInterestingSpeaker))
					{
						CanSkipConversationWith = mostInterestingSpeaker;
					}
					else if (mostInterestingSpeaker.IsSpeechSkippable())
					{
						CanSkipConversationWith = mostInterestingSpeaker;
					}
				}
				else if (Hud.Instance.Pip.FocusObjectPredicted is Character character2 && character2.IsSpeechSkippable())
				{
					CanSkipConversationWith = character2;
				}
				if (!canTalk)
				{
					if (instance2.GameCamera.FlyCam && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.InLabor)
					{
						AddCharacterFollowActions(localControlledCharacter, character);
						AddBuildingFollowActions(localControlledCharacter, building);
					}
					return;
				}
			}
			if (!instance2.GameCamera.FlyCam)
			{
				if (!flag || localControlledCharacter.IsPushingAgainstWaistHighWall())
				{
					return;
				}
				if (localControlledCharacter.IsInsideBuildingUnderConstruction(out var building2))
				{
					AvailableActions.Add(new AvailableAction(CursorAction.StopBuilding, localControlledCharacter, building2, CursorActionDisabledReason.Enabled));
					return;
				}
				if (localControlledCharacter.IsBeingBittenOrChoked() || localControlledCharacter.IsChokingSomeone() || localControlledCharacter.IsGettingUp() || (!localControlledCharacter.CanReceiveUserInputs() && !canTalk))
				{
					return;
				}
				if (localControlledCharacter.InsideBuilding != null && (character == null || character.CurrentActionAnim != ActionAnim.HandsUp))
				{
					if (CooldownSinceEnteredBuilding <= 0f)
					{
						AddInsideBuildingActions(localControlledCharacter);
					}
					return;
				}
				if (localControlledCharacter.CarryingObject != null && (character == null || !character.AliveAndNotZombie || !character.IsAwake || !(character is Human)) && gate == null)
				{
					AddCarryingCharacterActions(localTargetObject, localControlledCharacter);
					return;
				}
				if (character != null && !character.Alive && localControlledCharacter.FindActiveGoal(GoalType.MoveToAndSkin) is MoveToAndSkin moveToAndSkin && moveToAndSkin.GetTargetObject() == character)
				{
					return;
				}
			}
			Equipment equipment = localControlledCharacter.EquippedItem;
			if (equipment != localControlledCharacter.DesiredEquippedItem)
			{
				equipment = null;
			}
			TerrainCoord terrainCoord = (instance2.GameCamera.FlyCam ? instance3.CursorRayCastResult.Tile : localControlledCharacter.Tile);
			CursorActionDisabledReason cursorActionDisabledReason = ((localControlledCharacter.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
			CursorActionDisabledReason enabledIfConscious = GetEnabledIfConscious(localControlledCharacter);
			if (instance2.GameCamera.FlyCam && isPlacingBuilding)
			{
				AddPlacingBuildingActions(localControlledCharacter, equipment);
			}
			else if (instance2.GameCamera.FlyCam && isSettingCropPatch != null)
			{
				IsSettingCropPatch = isSettingCropPatch;
				AddPlacingCropZoneActions(localControlledCharacter);
			}
			else if (instance3.DraggingZone)
			{
				AvailableActions.Add(new AvailableAction(CursorAction.SetZone, localControlledCharacter, instance3.CursorRayCastResult.Tile, CursorActionDisabledReason.Enabled));
			}
			else
			{
				if (!instance2.GameCamera.FlyCam && (localControlledCharacter.InteractionObject != null || localControlledCharacter.IsCraftingAnim() || localControlledCharacter.IsBeingBittenOrChoked() || localControlledCharacter.IsChokingSomeone() || localControlledCharacter.CurrentActionAnim == ActionAnim.Hugged))
				{
					return;
				}
				if (character is Animal && character.IsAwake)
				{
					foreach (Equipment content in localControlledCharacter.Inventory.Contents)
					{
						if (character.LikesFood(content.GetPrototype()))
						{
							AvailableActions.Add(new AvailableAction(CursorAction.Feed, localControlledCharacter, character, content, cursorActionDisabledReason));
						}
					}
					if (character is Chicken)
					{
						AvailableActions.Add(new AvailableAction(IsStealingToTakeFrom(localControlledCharacter, localTargetObject, pickUp: true) ? CursorAction.PickUpSteal : CursorAction.PickUp, localControlledCharacter, character, cursorActionDisabledReason));
					}
				}
				if (showGiftMenu && character == Listener && StoryManager.CanOfferGifts(localControlledCharacter, character, checkIfLeader: false))
				{
					for (int i = 0; i < localControlledCharacter.Inventory.Count; i++)
					{
						Equipment item = localControlledCharacter.Inventory.GetItem(i);
						if (item.WasGifted())
						{
							continue;
						}
						EquipmentPrototype prototype = item.GetPrototype();
						LiquidPrototype liquidContentsType = item.GetLiquidContentsType();
						if (prototype.CanBeGift() && !GiftProtos.Contains(prototype))
						{
							CursorActionDisabledReason enabled = CursorActionDisabledReason.Enabled;
							if (item is Book && (character.HasReadBook(prototype) || character.Inventory.FindItemOfType(prototype) != null))
							{
								enabled = CursorActionDisabledReason.AlreadyReadBook;
							}
							if (prototype.GiftLimit > 0 && character.Inventory.CountGiftedItemsOfType(prototype) >= prototype.GiftLimit)
							{
								enabled = CursorActionDisabledReason.AlreadyHasGift;
							}
							AvailableActions.Add(new AvailableAction(CursorAction.Gift, localControlledCharacter, character, item, enabled));
							GiftProtos.Add(prototype);
						}
						if (liquidContentsType != null && liquidContentsType.CanBeGift() && !GiftLiquids.Contains(liquidContentsType))
						{
							AvailableActions.Add(new AvailableAction(CursorAction.Gift, localControlledCharacter, character, item, CursorActionDisabledReason.Enabled));
							GiftLiquids.Add(liquidContentsType);
						}
					}
					AvailableActions.Add(new AvailableAction(CursorAction.Back, localControlledCharacter, character, CursorActionDisabledReason.Enabled));
					GiftProtos.Clear();
					GiftLiquids.Clear();
					ShowGiftMenu = true;
					return;
				}
				bool num = showCraftMenuFor != null && showCraftMenuFor == craftingProp;
				bool flag3 = showDisassembleMenuFor != null && showDisassembleMenuFor == craftingProp;
				if ((num || flag3) && (!craftingProp.IsCrafting() || craftingProp.CurrentCrafter != localControlledCharacter))
				{
					AddCraftingActions(craftingProp, localControlledCharacter, flag3);
					ShowCraftMenuFor = showCraftMenuFor;
					ShowDisassembleMenuFor = showDisassembleMenuFor;
					return;
				}
				int equipmentActions = 0;
				bool hasMaterialToLightFire = false;
				bool flag4 = false;
				if (building == null)
				{
					AddPourIntoAction(localControlledCharacter, prop, equipment, ref equipmentActions);
				}
				if ((animalFeederProp != null || animalDrinkerProp != null) && prop.GetCommunity() == localControlledCharacter.Community)
				{
					RoleInfo roleInfo = localControlledCharacter.GetRoleInfo(Role.AnimalFeeder);
					if (roleInfo.Valid)
					{
						CursorActionDisabledReason enabled2 = ((roleInfo.Paused && localControlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(roleInfo.Paused ? CursorAction.ResumeAnimalFeeding : CursorAction.CancelAnimalFeeding, localControlledCharacter, localTargetObject, enabled2));
					}
					else if (localControlledCharacter.Community.CanUseRoleCommands())
					{
						CursorActionDisabledReason enabled3 = ((localControlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(CursorAction.SetAnimalFeeder, localControlledCharacter, localTargetObject, enabled3));
					}
				}
				if (localTargetObject != null && localTargetObject.GetGrabbableEquipmentType() != null)
				{
					CursorActionDisabledReason enabled4 = ((!localControlledCharacter.HasInventorySpaceFor(localTargetObject.GetGrabbableEquipmentType().Weight)) ? CursorActionDisabledReason.InventoryFullSpaceNeeded : CursorActionDisabledReason.Enabled);
					bool flag5 = IsStealingToTakeFrom(localControlledCharacter, localTargetObject);
					AvailableActions.Add(new AvailableAction(flag5 ? CursorAction.GrabSteal : CursorAction.Grab, localControlledCharacter, localTargetObject, localTargetObject.GetGrabbableEquipmentType(), enabled4));
				}
				if (instance2.GameCamera.FlyCam && localTargetObject != null && localControlledCharacter.IsEnemy(localTargetObject))
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Attack, localControlledCharacter, localTargetObject, enabledIfConscious));
				}
				else if (localTargetObject != null && localTargetObject.GetUnderConstructionInfo() != null && localTargetObject.GetCommunity() == localControlledCharacter.Community)
				{
					AddUnderConstructionActions(localTargetObject, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (building != null)
				{
					AddBuildingActions(building, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (plantableCrop != null)
				{
					AddCropActions(plantableCrop, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (treeProp != null)
				{
					AddTreeActions(treeProp, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (fallenTreeProp != null)
				{
					AddFallenTreeActions(fallenTreeProp, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (bush != null)
				{
					AddBushActions(bush, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (localTargetObject != null && localTargetObject.GetMiningResourceType() != null)
				{
					AddMiningActions(localTargetObject, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (well != null)
				{
					AddFillFromPropActions(well, LiquidPrototype.Water, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (gate != null)
				{
					AddGateActions(gate, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (trap != null)
				{
					AddTrapActions(localTargetObject, trap, prop, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (craftingProp != null)
				{
					AddCraftingPropActions(craftingProp, localControlledCharacter, equipment, ref hasMaterialToLightFire, ref equipmentActions);
				}
				else if (prop != null)
				{
					AddPropActions(prop, localControlledCharacter, equipment, ref equipmentActions);
				}
				else if (character != null)
				{
					AddCharacterActions(character, localControlledCharacter, mostInterestingSpeaker, ref wantTargetHeader, ref canTalk);
				}
				else if (localTargetObject == null && instance2.GameCamera.FlyCam)
				{
					CursorActionDisabledReason enabled5 = (instance5.IsImpassable(instance3.CursorRayCastResult.Tile.x, instance3.CursorRayCastResult.Tile.y, 0, localControlledCharacter, null) ? CursorActionDisabledReason.Disabled : enabledIfConscious);
					AvailableActions.Add(new AvailableAction(CursorAction.GoTo, localControlledCharacter, instance3.CursorRayCastResult.Tile, enabled5));
					AddMovementZoneActions(localControlledCharacter, instance3.CursorRayCastResult.Tile);
				}
				Character character3 = localControlledCharacter;
				Character character4 = (canTalk ? character : null);
				Speech speech = null;
				Speech myLastSpecialBehaviourTriggeredFromSpeech = null;
				BaseObject myLastSpeechObject = null;
				MemoryParam myLastSpeechParam = default(MemoryParam);
				Conversation conversation3 = null;
				if (character4 != null)
				{
					Conversation conversationWith = GetConversationWith(character3, character4);
					if (conversationWith != null && conversationWith.IsWaitingForReply())
					{
						speech = conversationWith.GetLastSpeechToReplyTo(out myLastSpeechObject, out myLastSpeechParam, out myLastSpecialBehaviourTriggeredFromSpeech);
						conversation3 = conversationWith;
					}
				}
				if (character3 != Speaker || character4 != Listener || speech != LastSpeechToReplyTo)
				{
					SelectedAction = 0;
					WantRefreshSpeechOptions = true;
				}
				if (WantRefreshSpeechOptions)
				{
					WantRefreshSpeechOptions = false;
					Speaker = character3;
					Listener = character4;
					LastSpeechToReplyTo = speech;
					LastSpeechObject = myLastSpeechObject;
					LastSpeechParam = myLastSpeechParam;
					LastSpeechToReplyToText = conversation3?.GetLastSpeechToReplyToText(character4);
					SpeechOptions.Clear();
					SpeechText.Clear();
					if (Speaker != null && Listener != null)
					{
						Speech lastSpeechToReplyTo = ((myLastSpecialBehaviourTriggeredFromSpeech != null) ? myLastSpecialBehaviourTriggeredFromSpeech : LastSpeechToReplyTo);
						if (!StoryManager.Instance.GetSpeechOptions(Speaker, Listener, SpeechOptions, lastSpeechToReplyTo, LastSpeechObject, LastSpeechParam))
						{
							LastSpeechObject = null;
							LastSpeechParam = default(MemoryParam);
						}
					}
					for (int j = 0; j < SpeechOptions.Count; j++)
					{
						Speech speech2 = SpeechOptions[j].Speech;
						Speech.EvaluateSpeechParams(speech2.Params, Speaker, Listener, SpeechOptions[j].CachedObject, _paramResults, default(MemoryParam), null, null, null, null, speech2.UniqueID);
						Speech.BuildSpeechText(speech2.TextHash, speech2.Params, Speaker, Listener, SpeechOptions[j].CachedObject, _paramResults, out var speechText, null, englishOnly: false, isQuest: false);
						if (SpeechOptions[j].Speech.Lie)
						{
							string text = GameImpl.Translate(HUD_Lie);
							if (!string.IsNullOrEmpty(text))
							{
								if (text[text.Length - 1] != ' ')
								{
									text += " ";
								}
								speechText = text + speechText;
							}
						}
						SpeechText.Add(speechText);
					}
				}
				if (LastSpeechToReplyToText != null)
				{
					HeaderActions.Add(new AvailableAction(CursorAction.SpeechToReplyTo, LastSpeechToReplyToText));
					HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
					wantTargetHeader = false;
				}
				int count = AvailableActions.Count;
				AddCharacterFollowActions(localControlledCharacter, character);
				for (int k = 0; k < SpeechOptions.Count; k++)
				{
					if (SpeechOptions[k].Visible)
					{
						if (SpeechOptions[k].Speech.Situation == SpeechSituation.OfferGift || SpeechOptions[k].Speech.SpecialBehaviour == SpecialSpeechBehaviour.OfferGift)
						{
							AvailableActions.Add(new AvailableAction(CursorAction.OpenGiftMenu, Speaker, Listener, SpeechOptions[k].CachedObject, SpeechOptions[k].CachedMemoryParam, SpeechOptions[k].Enabled ? cursorActionDisabledReason : CursorActionDisabledReason.WrongPersonality, SpeechOptions[k].Speech, SpeechText[k]));
						}
						else
						{
							AvailableActions.Add(new AvailableAction(CursorAction.TalkTo, Speaker, Listener, SpeechOptions[k].CachedObject, SpeechOptions[k].CachedMemoryParam, SpeechOptions[k].Enabled ? cursorActionDisabledReason : CursorActionDisabledReason.WrongPersonality, SpeechOptions[k].Speech, SpeechText[k]));
						}
					}
				}
				int num2 = AvailableActions.Count;
				if (instance2.PlaySpeed != PlaySpeed.Paused && flag)
				{
					if (mostInterestingSpeaker != null)
					{
						if (mostInterestingSpeaker.FindActiveGoal(GoalType.Conversation) is Conversation conversation4 && conversation4.IsSkippable(mostInterestingSpeaker))
						{
							CanSkipConversationWith = mostInterestingSpeaker;
						}
						else if (mostInterestingSpeaker.IsSpeechSkippable())
						{
							CanSkipConversationWith = mostInterestingSpeaker;
						}
					}
					else if (Hud.Instance.Pip.FocusObjectPredicted is Character character5 && character5.IsSpeechSkippable())
					{
						CanSkipConversationWith = character5;
					}
				}
				if (equipment != null && equipment.GetSeedForPlantType() != null)
				{
					CursorActionDisabledReason cursorActionDisabledReason2 = cursorActionDisabledReason;
					if (Session.Instance.Weather.TemperatureInCelsius <= 0f)
					{
						cursorActionDisabledReason2 = CursorActionDisabledReason.TooColdForPlanting;
					}
					if (cursorActionDisabledReason2 == CursorActionDisabledReason.Enabled)
					{
						cursorActionDisabledReason2 = CanPlantHere(localControlledCharacter, terrainCoord, checkCharacters: false);
					}
					AvailableActions.Add(new AvailableAction(CursorAction.Plant, localControlledCharacter, terrainCoord, cursorActionDisabledReason2));
					equipmentActions++;
				}
				if (!CanSpeakToTarget(character))
				{
					Equipment huntingKnife = localControlledCharacter.Inventory.GetHuntingKnife();
					if (huntingKnife != null && character != null && !character.Alive)
					{
						CursorActionDisabledReason enabled6 = cursorActionDisabledReason;
						Recipe recipe = null;
						if (character.SkinnedAmount < 1f)
						{
							recipe = GameImpl.Instance.FindRecipeByProduct(null, character.GetMeatType());
							if (recipe != null && recipe.RecipeType == RecipeType.HuntingKnife && !localControlledCharacter.CanUseRecipe(recipe))
							{
								enabled6 = GetSkillTooLowReasonForSkillType(recipe.SkillType);
							}
						}
						else
						{
							enabled6 = CursorActionDisabledReason.AlreadySkinned;
						}
						AvailableActions.Add(new AvailableAction(CursorAction.Skin, localControlledCharacter, recipe, character, huntingKnife, enabled6));
						if (huntingKnife == equipment)
						{
							equipmentActions++;
						}
					}
					CursorActionDisabledReason cursorActionDisabledReason3 = CanLightFire(localControlledCharacter, localTargetObject);
					if (cursorActionDisabledReason3 != CursorActionDisabledReason.Disabled)
					{
						if (huntingKnife != null && localControlledCharacter.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null && (cursorActionDisabledReason3 == cursorActionDisabledReason || equipment is HuntingKnife))
						{
							AvailableActions.Add(new AvailableAction(CursorAction.LightFireWithFlint, localControlledCharacter, localTargetObject, huntingKnife, cursorActionDisabledReason3));
							if (equipment is HuntingKnife)
							{
								equipmentActions++;
							}
							flag4 = true;
						}
						Equipment equipment2 = localControlledCharacter.Inventory.FindItemOfType(EquipmentPrototype.Match);
						if (equipment2 != null && (cursorActionDisabledReason3 == cursorActionDisabledReason || equipment == equipment2))
						{
							AvailableActions.Add(new AvailableAction(CursorAction.LightFireWithMatch, localControlledCharacter, localTargetObject, equipment2, cursorActionDisabledReason3));
							if (equipment == equipment2)
							{
								equipmentActions++;
							}
							flag4 = true;
						}
					}
					if (campfire != null && !campfire.IsBurning() && (!hasMaterialToLightFire || campfire.WoodRemaining > 0f) && !flag4)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.LightFire, localControlledCharacter, localTargetObject, cursorActionDisabledReason));
					}
					if (enterableVehicle != null && enterableVehicle.HasEverMoved && !enterableVehicle.IsMoving && enterableVehicle.IsDriveable && !instance2.GameCamera.FlyCam)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.Park, localControlledCharacter, localTargetObject, cursorActionDisabledReason));
					}
					if (localTargetObject != null && character == null && localTargetObject.GetGrabbableEquipmentType() == null && !localTargetObject.IsBurning())
					{
						PropPrototype propPrototype = localTargetObject.GetPropPrototype();
						if (propPrototype != null && localControlledCharacter.Community != null)
						{
							Type type = (propPrototype.NeedToolboxToRepair() ? typeof(Toolbox) : null);
							if (type == null || localControlledCharacter.Inventory.FindItemOfClass(type) != null)
							{
								if (localTargetObject.GetCommunity() == localControlledCharacter.Community)
								{
									if (localTargetObject.GetDamageFraction() > 0f && localTargetObject.GetRepairResourceType() != null)
									{
										CursorActionDisabledReason enabled7 = ((localControlledCharacter.Roles.Count >= 10 && !localControlledCharacter.HasRole(new RoleInfo(Role.Repairing))) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
										if (localControlledCharacter.GetSkillLevelWithEffects(SkillType.Construction) < propPrototype.RepairSkillNeeded)
										{
											enabled7 = CursorActionDisabledReason.ConstructionSkillTooLow;
										}
										AvailableActions.Add(new AvailableAction(CursorAction.Repair, localControlledCharacter, localTargetObject, enabled7));
										if (type != null)
										{
											equipmentActions++;
										}
									}
								}
								else if (CanTakeOver(localControlledCharacter, localTargetObject))
								{
									bool flag6 = localControlledCharacter.HasRole(new RoleInfo(Role.Capturing, localTargetObject.GetTile()));
									CursorActionDisabledReason enabled8 = ((localControlledCharacter.Roles.Count >= 10 && !flag6) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
									if (flag6)
									{
										AvailableActions.Add(new AvailableAction(CursorAction.CancelCapturing, localControlledCharacter, localTargetObject.GetTile(), enabled8));
									}
									else
									{
										if (localControlledCharacter.GetSkillLevelWithEffects(SkillType.Construction) < propPrototype.CaptureSkillNeeded)
										{
											enabled8 = CursorActionDisabledReason.ConstructionSkillTooLow;
										}
										AvailableActions.Add(new AvailableAction(CursorAction.TakeOver, localControlledCharacter, localTargetObject, enabled8));
									}
									if (type != null)
									{
										equipmentActions++;
									}
								}
							}
						}
						if (!instance2.GameCamera.FlyCam)
						{
							PropPrototype propPrototype2 = localTargetObject.GetPropPrototype();
							if (propPrototype2 != null)
							{
								Recipe recipe2 = GameImpl.Instance.FindRecipeByProduct(propPrototype2, null);
								Type type2 = typeof(Toolbox);
								if (recipe2 != null)
								{
									type2 = recipe2.RecipeType switch
									{
										RecipeType.Toolbox => typeof(Toolbox), 
										RecipeType.Shovel => typeof(Shovel), 
										RecipeType.HuntingKnife => typeof(HuntingKnife), 
										_ => null, 
									};
								}
								if (type2 == null || (equipment != null && type2.IsInstanceOfType(equipment)))
								{
									Community community = localTargetObject.GetCommunity();
									if (community == localControlledCharacter.Community)
									{
										if (propPrototype2.CanBeDemolished)
										{
											CursorActionDisabledReason cursorActionDisabledReason4 = CanDemolish(localControlledCharacter, localTargetObject);
											if (cursorActionDisabledReason4 != CursorActionDisabledReason.Disabled)
											{
												AvailableActions.Add(new AvailableAction(CursorAction.Demolish, localControlledCharacter, localTargetObject, cursorActionDisabledReason4));
												if (type2 != null)
												{
													equipmentActions++;
												}
											}
										}
										if (prop != null && prop.GetUnderConstructionInfo() == null)
										{
											AvailableActions.Add(new AvailableAction(CursorAction.Abandon, localControlledCharacter, localTargetObject, CursorActionDisabledReason.Enabled));
											if (type2 != null)
											{
												equipmentActions++;
											}
										}
									}
									else if ((community == null || !community.HasAnyActiveMembers()) && propPrototype2.CanBeDemolished)
									{
										CursorActionDisabledReason cursorActionDisabledReason5 = CanDemolish(localControlledCharacter, localTargetObject);
										if (cursorActionDisabledReason5 == CursorActionDisabledReason.Enabled && localControlledCharacter.Encumbered)
										{
											cursorActionDisabledReason5 = CursorActionDisabledReason.CantDemolishEncumbered;
										}
										if (cursorActionDisabledReason5 != CursorActionDisabledReason.Disabled)
										{
											AvailableActions.Add(new AvailableAction(CursorAction.Demolish, localControlledCharacter, localTargetObject, cursorActionDisabledReason5));
											if (type2 != null)
											{
												equipmentActions++;
											}
										}
									}
								}
							}
						}
					}
				}
				bool flag7 = false;
				if (CanFarm(localControlledCharacter, localTargetObject))
				{
					RoleInfo roleInfo2 = localControlledCharacter.GetRoleInfo(Role.Farmer);
					if (roleInfo2.Valid)
					{
						CursorActionDisabledReason enabled9 = ((roleInfo2.Paused && localControlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(roleInfo2.Paused ? CursorAction.ResumeFarming : CursorAction.CancelFarming, localControlledCharacter, localTargetObject, enabled9));
					}
					else if (localControlledCharacter.Community.CanUseRoleCommands())
					{
						CursorActionDisabledReason enabled10 = ((localControlledCharacter.Roles.Count >= 10) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
						if (localControlledCharacter.Inventory.GetBestWateringCan() == null)
						{
							enabled10 = CursorActionDisabledReason.NeedWateringCan;
						}
						AvailableActions.Add(new AvailableAction(CursorAction.SetFarmer, localControlledCharacter, localTargetObject, enabled10));
					}
					if (plantableCrop != null && localControlledCharacter.Community.CanUseRoleCommands())
					{
						EquipmentPrototype proto = ((plantableCrop.GetHarvestSeedsPrototype() != null) ? plantableCrop.GetHarvestSeedsPrototype() : plantableCrop.GetHarvestPrototype());
						CursorActionDisabledReason enabled11 = ((localControlledCharacter.Roles.Count >= 10 && !localControlledCharacter.HasRole(Role.Farmer)) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
						AvailableActions.Add(new AvailableAction(CursorAction.SetCropsPatch, localControlledCharacter, null, proto, enabled11));
						flag7 = true;
					}
				}
				if (!flag7 && equipment != null && equipment.GetSeedForPlantType() != null && localControlledCharacter.Community.CanUseRoleCommands() && (plantableCrop == null || plantableCrop.GetCommunity() == localControlledCharacter.GetCommunity()))
				{
					CursorActionDisabledReason enabled12 = ((localControlledCharacter.Roles.Count >= 10 && !localControlledCharacter.HasRole(Role.Farmer)) ? CursorActionDisabledReason.TooManyRoles : CursorActionDisabledReason.Enabled);
					AvailableActions.Add(new AvailableAction(CursorAction.SetCropsPatch, localControlledCharacter, null, equipment.GetPrototype(), enabled12));
					equipmentActions++;
				}
				if (equipment != null && equipment.GetLiquidContentsType() != null && equipment.GetLiquidContentsType().Flammable && localTargetObject != null && localTargetObject.GetFlammability() == Flammability.Medium_RequiresFuel && (character == null || !character.IsAwake))
				{
					AvailableActions.Add(new AvailableAction(CursorAction.PourFuel, localControlledCharacter, localTargetObject, equipment, enabledIfConscious));
					equipmentActions++;
				}
				if (prop != null && prop.Community == localControlledCharacter.Community && prop.CanSetStoragePolicy() && localControlledCharacter.Community != null && localControlledCharacter.Community.CanUseRoleCommands())
				{
					AvailableActions.Add(new AvailableAction(CursorAction.SetStoragePolicy, localControlledCharacter, prop, CursorActionDisabledReason.Enabled));
				}
				if (localTargetObject != null && localTargetObject.CanSetPropName() && localTargetObject.GetCommunity() == localControlledCharacter.Community)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.SetPropName, localControlledCharacter, localTargetObject, CursorActionDisabledReason.Enabled));
				}
				if (animal != null && animal.Alive)
				{
					Equipment huntingKnife2 = localControlledCharacter.Inventory.GetHuntingKnife();
					if (huntingKnife2 != null)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.Slaughter, localControlledCharacter, character, huntingKnife2, cursorActionDisabledReason));
					}
				}
				if (prop != null)
				{
					bool flag8 = true;
					foreach (Character member in localControlledCharacter.Community.Members)
					{
						for (int l = 0; l < member.Roles.Count; l++)
						{
							if (member.Roles[l].TargetLocation == prop.Tile || member.Roles[l].TargetLocation == prop.GetCentreTile())
							{
								if (flag8 && AvailableActions.Count > 0)
								{
									AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
									flag8 = false;
								}
								AvailableActions.Add(new AvailableAction(CursorAction.SelectCharacterWithRoleOnProp, member, prop, l, (member == localControlledCharacter && !instance2.GameCamera.FlyCam) ? CursorActionDisabledReason.Disabled : CursorActionDisabledReason.Enabled));
							}
						}
					}
					if (!flag8)
					{
						AvailableActions.Add(new AvailableAction(CursorAction.Separator, null));
					}
				}
				if (AvailableActions.Count == 0 && localTargetObject == null)
				{
					if (equipment != null && equipment.GetPrototype() != EquipmentPrototype.Snowball)
					{
						AddEquipmentUseAction(equipment, localControlledCharacter, localControlledCharacter, onlyIfEnabled: true, ref equipmentActions);
					}
					if ((equipment is Toolbox || equipment is Shovel) && localControlledCharacter.FindActiveGoal(GoalType.BuildGoal) == null && (instance2.GameCamera.FlyCam || instance2.IsPaused() || !CachedIsAnyoneOnScreenInCombat))
					{
						AvailableActions.Add(new AvailableAction((equipment is Shovel) ? CursorAction.Dig : CursorAction.Build, localControlledCharacter, null, CursorActionDisabledReason.Enabled));
						equipmentActions++;
					}
					if (equipment != null && (equipment is WateringCan || equipment.GetSeedForPlantType() != null))
					{
						RoleInfo roleInfo3 = localControlledCharacter.GetRoleInfo(Role.Farmer);
						if (roleInfo3.Valid)
						{
							CursorActionDisabledReason enabled13 = ((roleInfo3.Paused && localControlledCharacter.DownTime > 0f) ? CursorActionDisabledReason.TakingABreak : CursorActionDisabledReason.Enabled);
							AvailableActions.Add(new AvailableAction(roleInfo3.Paused ? CursorAction.ResumeFarming : CursorAction.CancelFarming, localControlledCharacter, null, enabled13));
							equipmentActions++;
						}
					}
					if (instance5.IsTileRiver(terrainCoord.x, terrainCoord.y))
					{
						CursorActionDisabledReason cursorActionDisabledReason6 = CursorActionDisabledReason.Disabled;
						if (equipment != null && equipment.GetLiquidCapacity() > 0f)
						{
							cursorActionDisabledReason6 = enabledIfConscious;
							if (equipment.GetLiquidContentsType() != LiquidPrototype.Water && equipment.GetLiquidContentsType() != null)
							{
								cursorActionDisabledReason6 = CursorActionDisabledReason.AlreadyContainsOtherLiquid;
							}
							else if (equipment.GetLiquidContentsAmount() >= equipment.GetLiquidCapacity())
							{
								cursorActionDisabledReason6 = CursorActionDisabledReason.AlreadyFullOfWater;
							}
							if (instance2.Weather.TemperatureInCelsius < Weather.RiversDrinkableTemperatureInCelsius)
							{
								cursorActionDisabledReason6 = CursorActionDisabledReason.RiverIsFrozen;
							}
							AvailableActions.Add(new AvailableAction(CursorAction.FillFromRiver, localControlledCharacter, equipment, terrainCoord, cursorActionDisabledReason6));
							equipmentActions++;
						}
						if (cursorActionDisabledReason6 != enabledIfConscious && instance2.Weather.TemperatureInCelsius >= Weather.RiversDrinkableTemperatureInCelsius)
						{
							Equipment bestLiquidContainerToFill = localControlledCharacter.Inventory.GetBestLiquidContainerToFill(LiquidPrototype.Water);
							if (bestLiquidContainerToFill != null && bestLiquidContainerToFill != equipment)
							{
								AvailableActions.Add(new AvailableAction(CursorAction.FillFromRiver, localControlledCharacter, bestLiquidContainerToFill, terrainCoord, enabledIfConscious));
							}
						}
						AvailableActions.Add(new AvailableAction(CursorAction.DrinkFromRiver, localControlledCharacter, terrainCoord, (instance2.Weather.TemperatureInCelsius > Weather.RiversDrinkableTemperatureInCelsius) ? enabledIfConscious : CursorActionDisabledReason.RiverIsFrozen));
					}
					else if (instance2.Weather.SnowOnGroundAmount >= Weather.ScoopableSnowOnGroundAmount && (instance2.GameCamera.DirectControlPitch >= GameCamera.MaxPitch - 0.34906584f || (equipment != null && equipment.GetPrototype() == EquipmentPrototype.Snowball)))
					{
						if (EquipmentPrototype.Snowball != null)
						{
							CursorActionDisabledReason enabled14 = ((!localControlledCharacter.HasInventorySpaceFor(EquipmentPrototype.Snowball.Weight)) ? CursorActionDisabledReason.Disabled : enabledIfConscious);
							if (instance2.Weather.TemperatureInCelsius >= Equipment.SnowballMeltTemp)
							{
								enabled14 = CursorActionDisabledReason.TooWarmForSnowballs;
							}
							if (instance5.RecentSnowScoops.Contains(terrainCoord))
							{
								enabled14 = CursorActionDisabledReason.AlreadyScoopedSnowHere;
							}
							AvailableActions.Add(new AvailableAction(CursorAction.ScoopSnow, localControlledCharacter, terrainCoord, enabled14));
							if (equipment != null && equipment.GetPrototype() == EquipmentPrototype.Snowball)
							{
								equipmentActions++;
							}
						}
						if (LiquidPrototype.Snow != null)
						{
							CursorActionDisabledReason cursorActionDisabledReason7 = CursorActionDisabledReason.Disabled;
							if (equipment != null && equipment.GetLiquidCapacity() > 0f)
							{
								cursorActionDisabledReason7 = enabledIfConscious;
								if (!LiquidPrototype.Snow.CanPourIntoBottles && (equipment.IsBottle() || equipment is WateringCan))
								{
									cursorActionDisabledReason7 = CursorActionDisabledReason.CantPourIntoBottles;
								}
								else if (equipment.GetLiquidContentsType() != LiquidPrototype.Snow && equipment.GetLiquidContentsType() != null)
								{
									cursorActionDisabledReason7 = CursorActionDisabledReason.AlreadyContainsOtherLiquid;
								}
								else if (equipment.GetLiquidContentsAmount() >= equipment.GetLiquidCapacity())
								{
									cursorActionDisabledReason7 = CursorActionDisabledReason.AlreadyFullOfWater;
								}
								AvailableActions.Add(new AvailableAction(CursorAction.FillFromSnow, localControlledCharacter, equipment, terrainCoord, cursorActionDisabledReason7));
								equipmentActions++;
							}
							if (cursorActionDisabledReason7 != enabledIfConscious)
							{
								Equipment bestLiquidContainerToFill2 = localControlledCharacter.Inventory.GetBestLiquidContainerToFill(LiquidPrototype.Snow);
								if (bestLiquidContainerToFill2 != null && bestLiquidContainerToFill2 != equipment)
								{
									AvailableActions.Add(new AvailableAction(CursorAction.FillFromSnow, localControlledCharacter, bestLiquidContainerToFill2, terrainCoord, enabledIfConscious));
								}
							}
						}
					}
					if (equipment != null && equipment.GetPrototype() == EquipmentPrototype.Snowball)
					{
						AddEquipmentUseAction(equipment, localControlledCharacter, localControlledCharacter, onlyIfEnabled: true, ref equipmentActions);
					}
				}
				if (equipmentActions > 1)
				{
					AvailableActions.Add(new AvailableAction(CursorAction.Unequip, localControlledCharacter, null, cursorActionDisabledReason));
				}
				if (count != num2)
				{
					if (count > 0)
					{
						AvailableActions.Insert(count, new AvailableAction(CursorAction.Separator, null));
						num2++;
					}
					if (num2 < AvailableActions.Count)
					{
						AvailableActions.Insert(num2, new AvailableAction(CursorAction.Separator, null));
					}
				}
				if (AvailableActions.Count > 0 && AvailableActions[AvailableActions.Count - 1].ActionType == CursorAction.Separator)
				{
					AvailableActions.RemoveAt(AvailableActions.Count - 1);
				}
			}
		}
	}

	public static bool GetScreenEdgeCursorState(ref CursorState newState)
	{
		if (NotificationManager.Instance.IsDisplayingNotification())
		{
			return false;
		}
		float num = 1f;
		GameImpl instance = GameImpl.Instance;
		HudBehaviour instance2 = HudBehaviour.Instance;
		Vector2 mousePosition = InputFunctionManager.Instance.GetMousePosition();
		int num2 = 0;
		int num3 = 0;
		if (mousePosition.x <= (float)instance.LeftOnScreen + num)
		{
			num2 = -1;
		}
		else if (mousePosition.x >= (float)(instance.LeftOnScreen + instance2.MainPanelWidthOnScreen - 1) - num)
		{
			num2 = 1;
		}
		if (mousePosition.y <= (float)instance.TopOnScreen + num)
		{
			num3 = -1;
		}
		else if (mousePosition.y >= (float)(instance.TopOnScreen + instance.HeightOnScreen - 1) - num)
		{
			num3 = 1;
		}
		if (num2 == -1 && num3 == -1)
		{
			newState = CursorState.MoveDownLeft;
			return true;
		}
		if (num2 == -1 && num3 == 1)
		{
			newState = CursorState.MoveUpLeft;
			return true;
		}
		if (num2 == 1 && num3 == -1)
		{
			newState = CursorState.MoveDownRight;
			return true;
		}
		if (num2 == 1 && num3 == 1)
		{
			newState = CursorState.MoveUpRight;
			return true;
		}
		switch (num2)
		{
		case -1:
			newState = CursorState.MoveLeft;
			return true;
		case 1:
			newState = CursorState.MoveRight;
			return true;
		default:
			switch (num3)
			{
			case -1:
				newState = CursorState.MoveDown;
				return true;
			case 1:
				newState = CursorState.MoveUp;
				return true;
			default:
				return false;
			}
		}
	}

	public void StartSettingCropPatch(PropPrototype cropType, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		IsSettingCropPatch = cropType;
		SetingCropPatchFailed = false;
		WasInFlyModeBeforeSettingCropPatch = instance.GameCamera.FlyCam;
		instance.GameCamera.SetFlyCamMode(on: true, snap: false, inputFrame);
	}

	public void OnPostRender()
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		if (instance.GameCamera.FlyCam || DrawAllCropPatches)
		{
			int id = instance.CommunityManager.PlayerCommunity.Id;
			int num = 40;
			TerrainCoord tileCoordForPos = instance2.GetTileCoordForPos(instance.GameCamera.Focus);
			TerrainRect rect = new TerrainRect(tileCoordForPos - new TerrainCoord(num, num), tileCoordForPos + new TerrainCoord(num, num));
			Vector3 vector = new Vector3(0f, 0.1f, 0f);
			foreach (CropPatch patch in instance.CropsManager.Patches)
			{
				if ((patch.CommunityId == id || DrawAllCropPatches) && patch.Rect.Overlaps(rect))
				{
					Color col = patch.CropType.Color;
					col.a = CropPatchAlpha;
					DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: true);
					for (int i = 0; i < patch.Tiles.Count; i++)
					{
						int x = patch.Tiles[i].x;
						int y = patch.Tiles[i].y;
						Vector3 p = instance2.GetVertexPos(x, y) + vector;
						Vector3 p2 = instance2.GetVertexPos(x + 1, y) + vector;
						Vector3 p3 = instance2.GetVertexPos(x + 1, y + 1) + vector;
						Vector3 p4 = instance2.GetVertexPos(x, y + 1) + vector;
						DebugGraphics.DrawQuad(p, p2, p3, p4, col);
					}
					DebugGraphics.EndDrawQuads();
				}
			}
		}
		switch (GetCursorAction())
		{
		case CursorAction.ExitBuilding:
		{
			AvailableAction availableAction2 = GetAvailableAction();
			Building building2 = availableAction2.Target as Building;
			if (!(building2 is EnterableVehicle enterableVehicle2) || !enterableVehicle2.GetPredictedOrElseThisVehicle().IsDrivingTooFastToShowMenu())
			{
				TerrainCoord tileCoordForPos2 = GameTerrain.Instance.GetTileCoordForPos(building2.GetEntrancePos(availableAction2.Amount));
				if (instance2.GetBuilding(tileCoordForPos2.x, tileCoordForPos2.y) == null)
				{
					DrawExitArrow(building2, availableAction2.Amount, 1f);
				}
			}
			break;
		}
		case CursorAction.ChangeBuildingSlot:
		{
			AvailableAction availableAction = GetAvailableAction();
			Building building = availableAction.Target as Building;
			if (!(building is EnterableVehicle enterableVehicle) || !enterableVehicle.GetPredictedOrElseThisVehicle().IsDrivingTooFastToShowMenu())
			{
				Vector3 vector2 = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(building.GetSlotAngleRad(availableAction.Amount)));
				DrawArrow(building.GetSlotPos(availableAction.Amount) + vector2 + Vector3.up * 0.1f, vector2);
			}
			break;
		}
		}
		if (!IsPlacingBuilding || GhostBuilding == null)
		{
			return;
		}
		if (GhostBuilding is Building building3)
		{
			for (int j = 0; j < building3.GetEntranceDefs().Length; j++)
			{
				DrawExitArrow(building3, j, building3.HasAnyInternalSlots() ? 1f : (-1f));
			}
		}
		if (GhostBuilding is Gate { Pos: var pos } gate)
		{
			TerrainCoord terrainCoord = instance2.ClampTileWithinBounds(instance2.GetTileCoordForPos(pos));
			pos.y = Math.Max(pos.y, instance2.GetTileMaxHeight(terrainCoord.x, terrainCoord.y)) + ExitArrowYOffset;
			Vector3 right = gate.Right;
			Vector3 pos2 = pos + right * ExitArrowHorizOffset;
			DrawArrow(pos2, right);
		}
		if (GhostBuilding is CraftingProp craftingProp && !(craftingProp is Campfire))
		{
			Vector3 boundingBoxCentre = craftingProp.GetBoundingBoxCentre();
			TerrainCoord terrainCoord2 = instance2.ClampTileWithinBounds(instance2.GetTileCoordForPos(boundingBoxCentre));
			boundingBoxCentre.y = Math.Max(boundingBoxCentre.y, instance2.GetTileMaxHeight(terrainCoord2.x, terrainCoord2.y)) + ExitArrowYOffset;
			Vector3 vector3 = ((craftingProp is Nitrary) ? craftingProp.Forward : craftingProp.Right);
			boundingBoxCentre += vector3 * (ExitArrowHorizOffset + ((craftingProp is Nitrary) ? 2f : ((craftingProp is Kiln) ? 1.5f : 1.25f)));
			DrawArrow(boundingBoxCentre, -vector3);
		}
	}

	public void Update()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		UpdateGhostBuilding();
		if (IsPlacingBuilding && GhostBuilding is Gate)
		{
			instance.UnityGateInsideText.SetActive(value: true);
			instance.UnityGateOutsideText.SetActive(value: true);
			GameTerrain instance2 = GameTerrain.Instance;
			Gate gate = (Gate)GhostBuilding;
			Vector3 pos = gate.Pos;
			TerrainCoord terrainCoord = instance2.ClampTileWithinBounds(instance2.GetTileCoordForPos(pos));
			pos.y = Math.Max(pos.y, instance2.GetTileMaxHeight(terrainCoord.x, terrainCoord.y)) + ExitArrowYOffset;
			Vector3 right = gate.Right;
			Vector3 position = pos + right * GateInsideTextOffset;
			Vector3 position2 = pos + right * GateOutsideTextOffset;
			Quaternion rotation = Quaternion.Euler(90f, (float)gate.Orientation * 90f + 90f, 0f);
			instance.UnityGateInsideText.transform.position = position;
			instance.UnityGateInsideText.transform.rotation = rotation;
			instance.UnityGateOutsideText.transform.position = position2;
			instance.UnityGateOutsideText.transform.rotation = rotation;
		}
		else
		{
			instance.UnityGateInsideText.SetActive(value: false);
			instance.UnityGateOutsideText.SetActive(value: false);
		}
	}

	public static void DrawExitArrow(Building building, int exitIndex, float flipDir)
	{
		GameTerrain instance = GameTerrain.Instance;
		Vector3 entrancePos = building.GetEntrancePos(exitIndex);
		TerrainCoord terrainCoord = instance.ClampTileWithinBounds(instance.GetTileCoordForPos(entrancePos));
		entrancePos.y = Math.Max(entrancePos.y, instance.GetTileMaxHeight(terrainCoord.x, terrainCoord.y)) + ExitArrowYOffset;
		Vector3 vector = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(building.GetEntranceAngle(exitIndex)));
		entrancePos += vector * ExitArrowHorizOffset;
		DrawArrow(entrancePos, vector * flipDir);
	}

	public static void DrawArrow(Vector3 pos, Vector3 forward)
	{
		Vector3 vector = MathUtil.ToX0Y(MathUtil.RightNormal(MathUtil.ToXZ(forward)));
		Vector3 p = pos + (-forward - vector) * ExitArrowScale;
		Vector3 p2 = pos + (-forward + vector) * ExitArrowScale;
		Vector3 p3 = pos + (forward + vector) * ExitArrowScale;
		Vector3 p4 = pos + (forward - vector) * ExitArrowScale;
		DebugGraphics.StartDrawTexturedQuads(Matrix4x4.identity, ExitArrow);
		DebugGraphics.DrawQuad(p, p2, p3, p4);
		DebugGraphics.EndDrawQuads();
	}

	public void StartPlacingBuilding(Recipe recipe, Equipment usingItem, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		IsPlacingBuilding = true;
		IsPlacingBuildingRecipe = recipe;
		IsPlacingBuildingUsingItem = usingItem;
		PlacingBuildingFailed = false;
		WasInFlyModeBeforePlacingBuilding = instance.GameCamera.FlyCam;
		instance.GameCamera.SetFlyCamMode(on: true, snap: false, inputFrame);
	}

	public void StopPlacingBuilding()
	{
		IsPlacingBuilding = false;
		IsPlacingBuildingRecipe = null;
		IsPlacingBuildingUsingItem = null;
		PlacingBuildingFailed = false;
	}

	public void PlaceBuilding(Character controlledCharacter, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		if (GhostBuilding == null)
		{
			return;
		}
		Prop.OrientationType orientation = ((GhostBuilding is Prop) ? (GhostBuilding as Prop).Orientation : Prop.OrientationType.Deg0);
		AvailableAction availableAction = GetAvailableAction();
		if (availableAction.Recipe.RecipeType == RecipeType.Toolbox || availableAction.Recipe.RecipeType == RecipeType.Shovel)
		{
			if (controlledCharacter.GetGoal() is SurvivorGoal survivorGoal && survivorGoal.FindSubGoalByType(GoalType.BuildGoal) is BuildGoal)
			{
				Hud.Instance.CheckMovementZone(GhostBuilding);
				inputFrame.AddAction(InputAction.BuildHere(availableAction.Recipe, availableAction.Tile, orientation, isDoubleClick: false));
				if (controlledCharacter.Community.UnderConstructionBuildings.Count >= AvailableAction.MaxQueuedBuildings - 1)
				{
					instance2.Capture(InputFunction.MainAction, untilReleased: true);
					instance.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
				}
				OnPlacingBuildingSuccessful(controlledCharacter, availableAction.Recipe, availableAction.Tile, orientation, inputFrame);
			}
		}
		else
		{
			Hud.Instance.CheckMovementZone(availableAction.Tile);
			CraftAmountBox.ShowRestrictedItemsMessageIfNeeded(availableAction.Recipe, availableAction.Actor, recurring: false);
			inputFrame.AddAction(InputAction.Craft(availableAction.Recipe, availableAction.Actor, availableAction.Target as Equipment, recurring: false, 1, availableAction.Tile, orientation, isDoubleClick: false));
			instance2.Capture(InputFunction.MainAction, untilReleased: true);
			instance.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
			OnPlacingBuildingSuccessful(controlledCharacter, availableAction.Recipe, availableAction.Tile, orientation, inputFrame);
		}
	}

	public void OnPlacingBuildingSuccessful(Character controlledCharacter, Recipe recipe, TerrainCoord tile, Prop.OrientationType orientation, InputFrame inputFrame)
	{
	}

	public void Unload()
	{
		if (GhostBuilding != null)
		{
			GhostBuilding.UnityDeactivate();
			GhostBuilding.UnityDelete();
			GhostBuilding = null;
		}
	}

	private void UpdateGhostBuilding()
	{
		PropPrototype propPrototype = (IsPlacingBuildingThisFrame ? AvailableActions[SelectedAction].Recipe : null)?.ProductPropPrototype;
		PropPrototype propPrototype2 = ((GhostBuilding != null) ? GhostBuilding.GetPropPrototype() : null);
		if (propPrototype != propPrototype2)
		{
			if (propPrototype2 != null)
			{
				GhostBuilding.UnityDeactivate();
				GhostBuilding.UnityDelete();
				GhostBuilding = null;
			}
			if (propPrototype != null)
			{
				GhostBuilding = TileObject.CreateProp(propPrototype);
				if (GhostBuilding is Prop prop)
				{
					prop.SetOrientationType(GhostBuildingOrientation);
					if (propPrototype.PrefabIndexWhenBuiltByPlayer >= 0 && propPrototype.PrefabIndexWhenBuiltByPlayer < propPrototype.Prefabs.Count)
					{
						prop.SetVariation(propPrototype.PrefabIndexWhenBuiltByPlayer);
					}
					if (IsPlacingBuildingUsingItem != null && prop.Prototype != null)
					{
						if (prop.Prototype.GetNumMaterialVariations() > 0)
						{
							prop.MaterialVariation = IsPlacingBuildingUsingItem.MaterialVariation % prop.Prototype.GetNumMaterialVariations();
						}
						if (prop.Prototype.GetNumColorVariations() > 0)
						{
							prop.ColorVariation = IsPlacingBuildingUsingItem.ColorVariation % prop.Prototype.GetNumColorVariations();
						}
						if (prop.Prototype.GetNumColorVariations2() > 0)
						{
							prop.ColorVariation2 = IsPlacingBuildingUsingItem.ColorVariation2 % prop.Prototype.GetNumColorVariations2();
						}
						if (prop.Prototype.GetNumColorVariations3() > 0)
						{
							prop.ColorVariation3 = IsPlacingBuildingUsingItem.ColorVariation3 % prop.Prototype.GetNumColorVariations3();
						}
					}
				}
				if (GhostBuilding != null)
				{
					GhostBuilding.UnityInit();
					if (!GhostBuilding.IsUnityObjectActive())
					{
						GhostBuilding.UnityActivate();
					}
				}
			}
		}
		if (GhostBuilding != null)
		{
			GhostBuilding.SetTileGhost(Hud.Instance.CursorRayCastResult.Tile);
			if (GhostBuilding.GetOrientationType() != GhostBuildingOrientation)
			{
				GhostBuilding.SetOrientationType(GhostBuildingOrientation);
			}
		}
	}

	public TileObject GetGhostBuilding()
	{
		return GhostBuilding;
	}

	public void BackOutOfSubMenu()
	{
		ShowGiftMenu = false;
		ShowCraftMenuFor = null;
		ShowDisassembleMenuFor = null;
		SelectedAction = 0;
	}

	public static CursorActionDisabledReason CanBuildHere(Character character, TileObject ghostBuilding, bool checkOtherCharacters, Community community, bool forceGraveDigging = false)
	{
		return CanBuildHere(character, ghostBuilding, checkOtherCharacters, checkCropPatches: false, community, forceGraveDigging);
	}

	public static CursorActionDisabledReason CanBuildHere(Character character, TileObject ghostBuilding, bool checkOtherCharacters, bool checkCropPatches, Community community, bool forceGraveDigging = false)
	{
		TerrainCoord minTile = ghostBuilding.GetMinTile();
		TerrainCoord maxTile = ghostBuilding.GetMaxTile();
		if (ghostBuilding is Prop prop)
		{
			prop.CalcMinMaxTile(prop.Tile, prop.Orientation, out minTile, out maxTile);
		}
		return CanBuildHere(character, ghostBuilding, minTile, maxTile, checkOtherCharacters, checkCropPatches, community, forceGraveDigging);
	}

	public static CursorActionDisabledReason CanBuildHere(Character character, TileObject ghostBuilding, TerrainCoord minTile, TerrainCoord maxTile, bool checkOtherCharacters, Community community, bool forceGraveDigging = false)
	{
		return CanBuildHere(character, ghostBuilding, minTile, maxTile, checkOtherCharacters, checkCropPatches: false, community, forceGraveDigging);
	}

	public static CursorActionDisabledReason CanBuildHere(Character character, TileObject ghostBuilding, TerrainCoord minTile, TerrainCoord maxTile, bool checkOtherCharacters, bool checkCropPatches, Community community, bool forceGraveDigging = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		PitTrap pitTrap = ghostBuilding as PitTrap;
		Grave grave = ghostBuilding as Grave;
		BaseFence baseFence = ghostBuilding as BaseFence;
		Tripwire tripwire = ghostBuilding as Tripwire;
		if (ghostBuilding is Prop && !instance.IsTileRectWithinBounds(minTile - new TerrainCoord(1, 1), maxTile + new TerrainCoord(1, 1)))
		{
			return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
		}
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				if (instance.IsLookupSquareOutsideBounds(i, j))
				{
					return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
				}
				int ownerCommunityIdForLookupSquare = instance.GetOwnerCommunityIdForLookupSquare(i, j);
				if (ownerCommunityIdForLookupSquare != 0 && BaseObjectManager.Instance.FindBaseObjectByID(ownerCommunityIdForLookupSquare) is Community community2 && community2 != community && community2.HasAnyLivingNonZombieMembers() && community2.GetRelationship(community) != CommunityRelationshipType.Allied)
				{
					return CursorActionDisabledReason.CantBuildNearOtherCommunity;
				}
			}
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		for (int k = minTile.x; k <= maxTile.x; k++)
		{
			for (int l = minTile.y; l <= maxTile.y; l++)
			{
				if (instance.IsImpassable(k, l, (checkOtherCharacters ? 3 : 0) | 4 | 0x400 | 0x1000, character, ghostBuilding))
				{
					return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
				}
				if (instance.IsTileRiver(k, l) && baseFence == null)
				{
					return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
				}
				if (instance.GetTileTerrainType(new TerrainCoord(k, l)) == TerrainType.Road && grave != null && !forceGraveDigging)
				{
					return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
				}
				num = Math.Min(num, instance.GetTileMinHeight(k, l));
				num2 = Math.Max(num2, instance.GetTileMaxHeight(k, l));
			}
		}
		List<TileObject> list = (Util.AmIOnMainThread() ? ObjectsInRect : ObjectsInRectThread);
		instance.GetObjectsInRect(minTile, maxTile, list);
		foreach (TileObject item in list)
		{
			if (item != null && GameTerrain.IsFixedObject(item) && !item.CanBeClearedForBuilding(community, ghostBuilding))
			{
				return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
			}
		}
		list.Clear();
		for (int m = minTile.x - 1; m <= maxTile.x + 1; m++)
		{
			for (int n = minTile.y - 1; n <= maxTile.y + 1; n++)
			{
				if (pitTrap != null || grave != null)
				{
					if (instance.IsTileOutsideBounds(m, n))
					{
						return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
					}
					if (instance.IsTileRiver(m, n) || instance.IsSlopeOrImpassableRaw(m, n))
					{
						return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
					}
					if (!forceGraveDigging)
					{
						TerrainType tileTerrainType = instance.GetTileTerrainType(new TerrainCoord(m, n));
						if (tileTerrainType == TerrainType.Road || tileTerrainType == TerrainType.Rock)
						{
							return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
						}
					}
				}
				if (!ghostBuilding.WantFlattenTerrain())
				{
					continue;
				}
				TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(m, n);
				if (fixedObjectOnTile != null)
				{
					if (fixedObjectOnTile is Grave && ghostBuilding.WantFlattenTerrain())
					{
						return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
					}
					if (grave != null && !forceGraveDigging && fixedObjectOnTile.WantFlattenTerrain())
					{
						return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
					}
				}
			}
		}
		if (ghostBuilding is Mine mine)
		{
			bool flag = false;
			for (int num3 = 0; num3 < 4; num3++)
			{
				flag |= mine.HasRichDeposits((MineralType)num3);
			}
			if (!flag)
			{
				return CursorActionDisabledReason.NoRichDeposits;
			}
		}
		if (ghostBuilding is CraftingProp craftingProp)
		{
			TerrainRect standingArea = craftingProp.GetStandingArea();
			if (standingArea != TerrainRect.Invalid)
			{
				for (int num4 = standingArea.min.x; num4 <= standingArea.max.x; num4++)
				{
					for (int num5 = standingArea.min.y; num5 <= standingArea.max.y; num5++)
					{
						if (instance.IsImpassable(num4, num5, (checkOtherCharacters ? 3 : 0) | 4, character, ghostBuilding))
						{
							return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
						}
						if (instance.IsTileRiver(num4, num5))
						{
							return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
						}
					}
				}
			}
		}
		if (checkCropPatches && Session.Instance.CropsManager.AreAnyCropPatchesInRect(minTile, maxTile))
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (baseFence == null && tripwire == null && pitTrap == null && !(num2 - num <= MaxTerrainHeightVariationForBuilding))
		{
			return CursorActionDisabledReason.UnsuitableTerrainForBuilding;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public bool CanFarm(Character controlledCharacter, TileObject target)
	{
		if (target is PlantableCrop && controlledCharacter.GetCommunity() == target.GetCommunity() && Session.Instance.CropsManager.DoesCommunityHaveAnyCrops(controlledCharacter.GetCommunityId()))
		{
			return true;
		}
		return false;
	}

	public static CursorActionDisabledReason CanPlantHere(Character character, TerrainCoord tile, bool checkCharacters)
	{
		return CanPlantHere(character, tile, checkCharacters, null);
	}

	public static CursorActionDisabledReason CanPlantHere(Character character, TerrainCoord tile, bool checkCharacters, PropPrototype isSettingCropPatch)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (!GameTerrain.CanPlantCropsOnTerrainType(instance.GetTileTerrainType(tile)))
		{
			return CursorActionDisabledReason.UnsuitableTerrainForPlanting;
		}
		if (instance.IsTileRiver(tile.x, tile.y))
		{
			return CursorActionDisabledReason.UnsuitableTerrainForPlanting;
		}
		TileObject ignore = null;
		if (isSettingCropPatch != null)
		{
			PlantableCrop plant = instance.GetPlant(tile.x, tile.y);
			if (plant != null && plant.Prototype == isSettingCropPatch)
			{
				ignore = plant;
			}
		}
		int num = 196;
		if (checkCharacters)
		{
			num |= 3;
		}
		if (instance.IsImpassable(tile.x, tile.y, num, character, ignore))
		{
			return CursorActionDisabledReason.UnsuitableTerrainForPlanting;
		}
		int ownerCommunityIdForTile = instance.GetOwnerCommunityIdForTile(tile.x, tile.y);
		if (ownerCommunityIdForTile != 0 && BaseObjectManager.Instance.FindBaseObjectByID(ownerCommunityIdForTile) is Community community && community != character.Community && community.HasAnyLivingNonZombieMembers())
		{
			return CursorActionDisabledReason.CantPlantNearOtherCommunity;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public static CursorActionDisabledReason CanStartCraftingWithProp(Character controlledCharacter, CraftingProp prop, Recipe recipe, Equipment usingItem, bool ingredientsMustBeOnMe, bool wantCheckEquipmentPolicy, bool checkIfCrafting)
	{
		if (checkIfCrafting && prop.IsCrafting())
		{
			return CursorActionDisabledReason.Disabled;
		}
		switch (recipe.RecipeType)
		{
		case RecipeType.Campfire_Pot:
		{
			if (prop.Inventory.FindItemOfType(EquipmentPrototype.Pot) != null)
			{
				if (prop.Inventory.Count > 1)
				{
					return CursorActionDisabledReason.CampfireHasItems;
				}
				break;
			}
			if (prop.Inventory.Count > 0 && ingredientsMustBeOnMe)
			{
				return CursorActionDisabledReason.CampfireHasItems;
			}
			foreach (Equipment content in prop.Inventory.Contents)
			{
				if (content.GetLiquidContentsType() != null)
				{
					return CursorActionDisabledReason.CampfireHasItems;
				}
			}
			Equipment bestCookingPot = controlledCharacter.Inventory.GetBestCookingPot();
			if (bestCookingPot == null)
			{
				return CursorActionDisabledReason.NeedPot;
			}
			if (bestCookingPot.GetLiquidContentsAmount() > 0f && recipe.GetLiquidIngredient(bestCookingPot.GetLiquidContentsType()) == null)
			{
				return CursorActionDisabledReason.NeedEmptyPot;
			}
			break;
		}
		case RecipeType.Campfire_FryingPan:
			if (prop.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) != null)
			{
				if (prop.Inventory.Count > 1)
				{
					return CursorActionDisabledReason.CampfireHasItems;
				}
				break;
			}
			if (prop.Inventory.Count > 0 && ingredientsMustBeOnMe)
			{
				return CursorActionDisabledReason.CampfireHasItems;
			}
			foreach (Equipment content2 in prop.Inventory.Contents)
			{
				if (content2.GetLiquidContentsType() != null)
				{
					return CursorActionDisabledReason.CampfireHasItems;
				}
			}
			if (controlledCharacter.Inventory.GetFryingPan() == null)
			{
				return CursorActionDisabledReason.NeedFryingPan;
			}
			break;
		case RecipeType.Campfire_SpitRoast_Rabbit:
		case RecipeType.Campfire_SpitRoast_Chicken:
		case RecipeType.Campfire_SpitRoast_Venison:
		case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
			if (prop.Inventory.Count > 0 && ingredientsMustBeOnMe)
			{
				return CursorActionDisabledReason.CampfireHasItems;
			}
			foreach (Equipment content3 in prop.Inventory.Contents)
			{
				if (content3.GetLiquidContentsType() != null)
				{
					return CursorActionDisabledReason.CampfireHasItems;
				}
			}
			break;
		}
		if (ingredientsMustBeOnMe || controlledCharacter.Community == null)
		{
			if (!recipe.HasAllIngredients(controlledCharacter, controlledCharacter, usingItem, wantCheckEquipmentPolicy ? controlledCharacter : null))
			{
				return CursorActionDisabledReason.DontHaveAllIngredients;
			}
			if (recipe.RecipeType != RecipeType.Campfire_Pot && !recipe.HasSuitableContainer(controlledCharacter, usingItem, out var hasSomeButNotEnough, out var _, out var _))
			{
				if (recipe.ProductLiquidPrototype != null && recipe.ProductLiquidPrototype.CanPourIntoBottles)
				{
					if (!hasSomeButNotEnough)
					{
						return CursorActionDisabledReason.DontHaveSuitableContainer;
					}
					return CursorActionDisabledReason.DontEnoughSuitableContainers;
				}
				if (!hasSomeButNotEnough)
				{
					return CursorActionDisabledReason.DontHaveSealedContainer;
				}
				return CursorActionDisabledReason.DontEnoughSealedContainers;
			}
		}
		return CursorActionDisabledReason.Enabled;
	}

	public CursorActionDisabledReason CanLightFire(Character character, TileObject target)
	{
		if (target != null)
		{
			CursorActionDisabledReason result = ((character.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
			if (target is Character { IsAwake: not false })
			{
				return CursorActionDisabledReason.Disabled;
			}
			if (target is Campfire { WoodRemaining: <=0f })
			{
				return CursorActionDisabledReason.NeedWoodToLightFire;
			}
			if (target.GetFlammability() == Flammability.High && !target.IsBurning())
			{
				return result;
			}
			if (target.GetFlammability() == Flammability.Medium_RequiresFuel)
			{
				if (target is Prop prop)
				{
					float num = 0f;
					TerrainCoord terrainCoord = TerrainCoord.Invalid;
					for (int i = 0; i < prop.DamagePoints.Count; i++)
					{
						if (!prop.DamagePoints[i].Burning && prop.DamagePoints[i].Fuel > num)
						{
							num = prop.DamagePoints[i].Fuel;
							terrainCoord = GameTerrain.Instance.GetTileCoordForPos(prop.DamagePoints[i].Pos);
						}
					}
					if (terrainCoord != TerrainCoord.Invalid)
					{
						if (!(num >= FuelRequiredToStartFireInFlOz))
						{
							return CursorActionDisabledReason.NeedFuelToLightFire;
						}
						return result;
					}
				}
				else if (!target.IsBurning())
				{
					if (!(target.GetFuel() >= FuelRequiredToStartFireInFlOz))
					{
						return CursorActionDisabledReason.NeedFuelToLightFire;
					}
					return result;
				}
			}
		}
		return CursorActionDisabledReason.Disabled;
	}

	public static void BuildListOfEquipmentPrototypesThatCanBeAddedToFire()
	{
	}

	public static bool CanUseRecipesOfType(Character character, RecipeType recipeType)
	{
		for (int i = 0; i < TempSkillLevel.Length; i++)
		{
			TempSkillLevel[i] = -1;
		}
		foreach (KeyValuePair<string, Recipe> item in GameImpl.Instance.CurrentRecipesDeterministic)
		{
			Recipe value = item.Value;
			if (value.RecipeType != recipeType || value.Deprecated)
			{
				continue;
			}
			SkillType skillType = value.SkillType;
			if (skillType > SkillType.Invalid && skillType < SkillType.Count)
			{
				int num = TempSkillLevel[(int)skillType];
				if (num == -1)
				{
					num = character.GetSkillLevelWithEffects(skillType);
				}
				if (num >= value.SkillLevel)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasEnoughInventorySpaceForMaterialForFire(Character character)
	{
		foreach (EquipmentPrototype item in GameImpl.Instance.EquipmentPrototypesThatCanBeAddedToFire)
		{
			if (character.HasInventorySpaceFor(item.Weight))
			{
				return true;
			}
		}
		return false;
	}

	public static CursorActionDisabledReason GetSkillTooLowReasonForSkillType(SkillType skillType)
	{
		return skillType switch
		{
			SkillType.Construction => CursorActionDisabledReason.ConstructionSkillTooLow, 
			SkillType.Cooking => CursorActionDisabledReason.CookingSkillTooLow, 
			SkillType.Farming => CursorActionDisabledReason.FarmingSkillTooLow, 
			SkillType.HandToHand => CursorActionDisabledReason.HandToHandSkillTooLow, 
			SkillType.Medicine => CursorActionDisabledReason.MedicalSkillTooLow, 
			SkillType.Stealth => CursorActionDisabledReason.StealthSkillTooLow, 
			_ => CursorActionDisabledReason.Disabled, 
		};
	}

	public static bool CanAddMaterialToFire(EquipmentPrototype proto)
	{
		BuildListOfEquipmentPrototypesThatCanBeAddedToFire();
		return GameImpl.Instance.EquipmentPrototypesThatCanBeAddedToFire.Contains(proto);
	}

	public static CursorActionDisabledReason CanPowerNap(Character controlledCharacter)
	{
		Session instance = Session.Instance;
		if (!instance.IsInMultiplayerGame())
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (controlledCharacter.IsOutdoors())
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (controlledCharacter.HasUnbandagedInjury(0))
		{
			return CursorActionDisabledReason.Disabled;
		}
		PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
		if (instance.PlayTime - localPlayerRecord.LastPowerNapTime < Sun.DayLength)
		{
			return CursorActionDisabledReason.TimeBetweenPowerNaps;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public static Character CheckForRelationsWeCantLeaveWithout(EnterableVehicle vehicle, out Character relation)
	{
		Character[] inhabitants = vehicle.Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character == null)
			{
				continue;
			}
			for (int j = 0; j < character.Relationships.Count; j++)
			{
				if (character.Relationships[j].RelationshipTarget == null || character.Relationships[j].RelationshipTarget.InsideBuilding == vehicle || !character.Relationships[j].RelationshipTarget.AliveAndNotZombie || character.Relationships[j].RelationshipTarget.Disappeared || Session.Instance.CommunityManager.PlayerCommunity.GetRelationship(character.Relationships[j].RelationshipTarget.Community) == CommunityRelationshipType.Hostile)
				{
					continue;
				}
				RelationshipType relationshipType = character.Relationships[j].RelationshipType;
				if ((uint)(relationshipType - 1) <= 5u)
				{
					character.CalcAbsApprovalRating(character.Relationships[j].RelationshipTarget, out var approval, out var _);
					if (!(approval < -50f))
					{
						relation = character.Relationships[j].RelationshipTarget;
						return character;
					}
				}
			}
		}
		relation = null;
		return null;
	}

	public bool CanSelectEquipment(Character controlledCharacter)
	{
		if (controlledCharacter != null && (controlledCharacter.IsUsingEquippedItem() || controlledCharacter.IsCraftingAnim()))
		{
			return false;
		}
		if (InfoScreen.Instance.Active)
		{
			return false;
		}
		return true;
	}

	private CursorActionDisabledReason GetEnabledIfConscious(Character controlledCharacter)
	{
		if (controlledCharacter.Consciousness < Consciousness.Unconscious)
		{
			return CursorActionDisabledReason.Enabled;
		}
		foreach (Character selectedCharacter in Hud.Instance.SelectedCharacters)
		{
			if (selectedCharacter.Consciousness < Consciousness.Unconscious)
			{
				return CursorActionDisabledReason.Enabled;
			}
		}
		return CursorActionDisabledReason.Unconscious;
	}

	public bool CanSpeakToTarget(Character targetCharacter)
	{
		if (targetCharacter != null && targetCharacter.IsAwake && !targetCharacter.Zombie)
		{
			return targetCharacter is Human;
		}
		return false;
	}

	public bool CanShowDialogOptions(Character controlledCharacter, Character targetCharacter)
	{
		Session instance = Session.Instance;
		if (controlledCharacter != targetCharacter && controlledCharacter.SparringPartner == null && (targetCharacter.SparringPartner == null || targetCharacter.IsSurrendering()) && targetCharacter.CanShowDialogOptions() && instance.PlaySpeed == PlaySpeed.Normal && !instance.IsSwappingSupplies(targetCharacter))
		{
			if (controlledCharacter.IsEnemy(targetCharacter) && controlledCharacter.CurrentActionAnim != ActionAnim.HandsUp)
			{
				return targetCharacter.CurrentActionAnim == ActionAnim.HandsUp;
			}
			return true;
		}
		return false;
	}

	public bool CanGuard(Character controlledCharacter, TileObject target)
	{
		if (target.IsGuardPost())
		{
			if (target is Building building && building.GetUnderConstructionInfo() == null)
			{
				return building.Community == controlledCharacter.Community;
			}
			return false;
		}
		return false;
	}

	public static bool CanTakeOverCommunityBuildings(Character controlledCharacter, Community community)
	{
		if (community == null)
		{
			return true;
		}
		if (controlledCharacter != null && community == controlledCharacter.Community)
		{
			return false;
		}
		if (community.BuildingsCantBeCaptured)
		{
			return false;
		}
		if (!community.HasAnyLivingNonZombieMembers())
		{
			return true;
		}
		return false;
	}

	private static bool CanTakeOver(Character controlledCharacter, TileObject target)
	{
		if (target.GetCaptureResourceType() == null)
		{
			return false;
		}
		if (!Session.Instance.FollowerCommandsEnabled)
		{
			return false;
		}
		if (target.IsForcedInvulnerable())
		{
			return false;
		}
		Community community = target.GetCommunityThatOwnsThisArea();
		if (community == controlledCharacter.Community && target.GetCommunity() == null)
		{
			community = null;
		}
		if (community != null && target.GetCommunityId() == 0 && target is EnterableVehicle && !community.IsTileInsidePerimeter(target.GetCentreTile()))
		{
			community = null;
		}
		if (!CanTakeOverCommunityBuildings(controlledCharacter, community))
		{
			return false;
		}
		return true;
	}

	public static bool CanPickUp(Character controlledCharacter, TileObject target)
	{
		if (target is Character character && character.GetGrabbableEquipmentType() == null)
		{
			return true;
		}
		return false;
	}

	public bool CanSeeTakeAllOption(Character character, TileObject target)
	{
		EquipmentContainer inventory = target.GetInventory();
		if (inventory == null)
		{
			return false;
		}
		PropPrototype propPrototype = target.GetPropPrototype();
		if (propPrototype != null && !propPrototype.CanAutoInvestigate && !target.IsInvestigated())
		{
			return false;
		}
		if (inventory.IsEmptyExceptForWornItems(target))
		{
			return false;
		}
		if (Session.Instance.GameCamera.FlyCam)
		{
			if (!target.IsInvestigated())
			{
				return target.GetTileRect().GetClosestDistSqTo(character.Tile) <= CanSeeTakeAllInFlyModeDist * CanSeeTakeAllInFlyModeDist;
			}
			return true;
		}
		return true;
	}

	public CursorActionDisabledReason CanTakeAll(Character character, TileObject target)
	{
		EquipmentContainer inventory = target.GetInventory();
		if (inventory == null)
		{
			return CursorActionDisabledReason.Disabled;
		}
		int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Strength);
		CursorActionDisabledReason result = ((character.Consciousness >= Consciousness.Unconscious) ? CursorActionDisabledReason.Unconscious : CursorActionDisabledReason.Enabled);
		Equipment bestBackpack = inventory.GetBestBackpack(skillLevelWithEffects);
		Equipment bestBackpack2 = character.Inventory.GetBestBackpack(skillLevelWithEffects);
		if (bestBackpack != null && (bestBackpack2 == null || bestBackpack.GetCarryWeightEffect(skillLevelWithEffects) > bestBackpack2.GetCarryWeightEffect(skillLevelWithEffects)))
		{
			return result;
		}
		if (!inventory.IsEmptyExceptForWornItems(target))
		{
			Equipment equipment = inventory.FindLightestTakeableItem(target);
			if (equipment == null)
			{
				return CursorActionDisabledReason.Disabled;
			}
			if (character.GetAmountOfEquipmentThatCanBeStored(equipment) <= 0)
			{
				return CursorActionDisabledReason.InventoryFull;
			}
			return result;
		}
		return CursorActionDisabledReason.Disabled;
	}

	public static bool CanChokeHold(Character controlledCharacter, TileObject target)
	{
		if (target is Character { Consciousness: Consciousness.Conscious, InsideBuilding: null } character && character is Human && controlledCharacter.IsCrouching() && controlledCharacter.CarryingObject == null && !character.IsControllableByPlayer() && !character.IsFacing(controlledCharacter.PosXZ, MathF.PI / 2f))
		{
			return (controlledCharacter.PosXZ - character.PosXZ).sqrMagnitude <= MathUtil.Squared(Hud.DirectControlTargetableChokeHoldRadius);
		}
		return false;
	}

	public static bool CanRestrain(Character controlledCharacter, Character targetCharacter)
	{
		if (targetCharacter != null && targetCharacter.SparringPartner != null && targetCharacter.SparringType == SparringType.Feuding && targetCharacter.Consciousness == Consciousness.Conscious && targetCharacter.InsideBuilding == null && targetCharacter is Human && !targetCharacter.Zombie && targetCharacter != controlledCharacter && (!controlledCharacter.IsInMyCommunityOrAlly(targetCharacter.SparringPartner) || controlledCharacter.IsInMyCommunityOrAlly(targetCharacter)))
		{
			return controlledCharacter.CarryingObject == null;
		}
		return false;
	}

	public CursorActionDisabledReason CanDemolish(Character controlledCharacter, TileObject target)
	{
		if (target.IsForcedInvulnerable())
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (target.GetInventory() != null && target.GetInventory().Count > 0)
		{
			return CursorActionDisabledReason.CantDemolishInventoryNotEmpty;
		}
		if (target is Building building && building.GetInhabitantCount() > 0)
		{
			return CursorActionDisabledReason.CantDemolishInhabitants;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public static bool IsStealingToTakeFrom(Character controlledCharacter, TileObject target, bool pickUp = false)
	{
		Community community = target.GetCommunity();
		bool flag = target.GetMiningResourceType() != null || target.GetBaseObjectType() == BaseObjectType.TreeProp || target.GetBaseObjectType() == BaseObjectType.FallenTreeProp;
		if (flag)
		{
			community = target.GetCommunityThatOwnsThisArea();
		}
		if (community == null)
		{
			return false;
		}
		if (community == controlledCharacter.Community)
		{
			return false;
		}
		if (!community.HasAnyLivingNonZombieMembers() && !community.BuildingsCantBeCaptured)
		{
			return false;
		}
		if (community.IsAnimalCommunity())
		{
			return false;
		}
		CommunityRelationshipType relationship = Session.Instance.CommunityManager.GetRelationship(controlledCharacter.Community, community);
		if (relationship == CommunityRelationshipType.Hostile)
		{
			return false;
		}
		if (relationship == CommunityRelationshipType.Allied && flag)
		{
			return false;
		}
		if (pickUp && target.GetBaseObjectType() == BaseObjectType.Human)
		{
			Character character = target as Character;
			if (Session.Instance.PlayTime - character.LastRescuedByPlayer < RescueGoal.MinTimeBetweenAssumingPlayerIsRescuingSomeone)
			{
				return true;
			}
			if (Character.IsStealingToPickUp(target))
			{
				return true;
			}
			float num = MathUtil.Squared(Mathf.Sqrt(community.GetDistSqToNearestLivingNonZombieMember(controlledCharacter.Tile, character, ignoreThoseIndoors: false)) + 32f);
			int num2 = 0;
			foreach (Character member in community.Members)
			{
				if (member != character && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !(MathUtil.ToXZ(member.Pos - controlledCharacter.Pos).sqrMagnitude >= num) && num2 < 6)
				{
					if (RescueGoal.IsSuspiciousOfPlayerCarryingBody(member, controlledCharacter, character))
					{
						return true;
					}
					num2++;
				}
			}
			return false;
		}
		return true;
	}

	private static bool ShowNotInvestigatedOnScavengeAction(Character controlledCharacter, TileObject target)
	{
		PropPrototype propPrototype = target.GetPropPrototype();
		if (!target.IsInvestigated())
		{
			if (propPrototype != null && !propPrototype.CanAutoInvestigate)
			{
				return true;
			}
			if (Session.Instance.GameCamera.FlyCam && target.GetTileRect().GetClosestDistSqTo(controlledCharacter.Tile) > CanSeeTakeAllInFlyModeDist * CanSeeTakeAllInFlyModeDist)
			{
				return true;
			}
		}
		return false;
	}

	public static CursorAction GetScavengeAction(Character controlledCharacter, TileObject target)
	{
		PropPrototype propPrototype = target.GetPropPrototype();
		bool flag = propPrototype == null || propPrototype.CanAutoInvestigate || target.IsInvestigated();
		bool flag2 = ShowNotInvestigatedOnScavengeAction(controlledCharacter, target);
		EquipmentContainer inventory = target.GetInventory();
		if (IsStealingToTakeFrom(controlledCharacter, target))
		{
			if (target.IsLockable() && target.GetCommunityId() != 0 && target.GetCommunityId() != controlledCharacter.GetCommunityId())
			{
				return CursorAction.Steal;
			}
			if (!flag2)
			{
				if (!flag || !inventory.IsEmptyExceptForWornItems(target, showCampfirePot: true))
				{
					return CursorAction.Steal;
				}
				return CursorAction.StealEmpty;
			}
			return CursorAction.StealNotInvestigated;
		}
		if (!flag2)
		{
			if (!flag || !inventory.IsEmptyExceptForWornItems(target, showCampfirePot: true))
			{
				return CursorAction.Scavenge;
			}
			return CursorAction.ScavengeEmpty;
		}
		return CursorAction.ScavengeNotInvestigated;
	}

	public static Conversation GetConversationWith(Character controlledCharacter, TileObject target)
	{
		if (!(target is Character character))
		{
			return null;
		}
		if (!(character.FindActiveGoal(GoalType.Conversation) is Conversation conversation))
		{
			return null;
		}
		if (conversation.GetTargetCharacter() == controlledCharacter)
		{
			return conversation;
		}
		return null;
	}

	public static bool IsTargetCharacterInCombat(Character targetCharacter)
	{
		if (targetCharacter == null)
		{
			return false;
		}
		if (targetCharacter.CurrentActionAnim == ActionAnim.HandsUp)
		{
			return false;
		}
		if (targetCharacter.DirectControlled)
		{
			int num = Character.BaseSightRange * 2;
			GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(targetCharacter.Tile - new TerrainCoord(num, num), targetCharacter.Tile + new TerrainCoord(num, num), _nearbyCharacters);
			foreach (Character nearbyCharacter in _nearbyCharacters)
			{
				if (!nearbyCharacter.DirectControlled && nearbyCharacter.InCombat && nearbyCharacter.CurrentActionAnim != ActionAnim.HandsUp && !(nearbyCharacter.Tile.GetDistSquared(targetCharacter.Tile) > (float)(num * num)))
				{
					_nearbyCharacters.Clear();
					return true;
				}
			}
			_nearbyCharacters.Clear();
		}
		if (targetCharacter.SparringPartner != null)
		{
			return true;
		}
		if (targetCharacter.IsBeingBittenOrChoked())
		{
			return true;
		}
		if (targetCharacter.IsChokingSomeone())
		{
			return true;
		}
		return targetCharacter.InCombat;
	}

	public static bool IsAnyoneOnScreenInCombat()
	{
		if (!Util.AmIOnMainThread())
		{
			return CachedIsAnyoneOnScreenInCombat;
		}
		Session instance = Session.Instance;
		Vector2 vector = MathUtil.ToXZ(instance.GameCamera.Focus);
		foreach (TileObject item in instance.MovableUnityObjectsVisibleInMainView)
		{
			if (item is Character { InCombat: not false, CurrentActionAnim: not ActionAnim.HandsUp } character && (character.PosXZ - vector).sqrMagnitude <= OnScreenDist * OnScreenDist)
			{
				return true;
			}
		}
		return false;
	}

	public bool WantDisplayBrainScanInMinimap()
	{
		if (InfoScreen.Instance.Active)
		{
			return false;
		}
		if (Session.Instance.GameCamera.FlyCam)
		{
			return false;
		}
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		Conversation conversationWith = GetConversationWith(localControlledCharacter, Hud.Instance.LocalTargetObject);
		CursorAction cursorAction = GetCursorAction();
		if (conversationWith != null || cursorAction == CursorAction.TalkTo || cursorAction == CursorAction.FollowMe || cursorAction == CursorAction.StopFollowingMe || cursorAction == CursorAction.OpenGiftMenu || cursorAction == CursorAction.Gift || cursorAction == CursorAction.SpeechToReplyTo)
		{
			if (localControlledCharacter != null)
			{
				return localControlledCharacter.Inventory.FindItemOfClass(typeof(BrainScanner)) != null;
			}
			return false;
		}
		return false;
	}
}

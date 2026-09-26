using System;
using System.Text;

public struct AvailableAction
{
	public CursorAction ActionType;

	public CursorActionDisabledReason Enabled;

	public Character Actor;

	public TileObject Object;

	public BaseObject Target;

	public TerrainCoord Tile;

	public CantTransferReason Reason;

	public Speech Speech;

	public string SpeechText;

	public Recipe Recipe;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public InfectionType InfectedWith;

	public int Amount;

	public float FloatAmount;

	public InputFunction Shortcut;

	public MemoryParam SpeechParam;

	public MapMarkerType MarkerType;

	private static int HUD_Empty_Female = StringUtil.JenkinsHash("HUD_Empty_Female");

	private static int HUD_ScavengeEmpty_Female = StringUtil.JenkinsHash("HUD_ScavengeEmpty_Female");

	private static int HUD_StealEmpty_Female = StringUtil.JenkinsHash("HUD_StealEmpty_Female");

	private static int HUD_NotInvestigated_Female = StringUtil.JenkinsHash("HUD_NotInvestigated_Female");

	private static int HUD_ScavengeNotInvestigated_Female = StringUtil.JenkinsHash("HUD_ScavengeNotInvestigated_Female");

	private static int HUD_StealNotInvestigated_Female = StringUtil.JenkinsHash("HUD_StealNotInvestigated_Female");

	private static int HUD_EnterBuildingNotInvestigated_Female = StringUtil.JenkinsHash("HUD_EnterBuildingNotInvestigated_Female");

	private static int HUD_SetFarmer_Female = StringUtil.JenkinsHash("HUD_SetFarmer_Female");

	private static int HUD_SetGuard_Female = StringUtil.JenkinsHash("HUD_SetGuard_Female");

	private static int HUD_SetLumberjack_Female = StringUtil.JenkinsHash("HUD_SetLumberjack_Female");

	private static int HUD_SetCook_Female = StringUtil.JenkinsHash("HUD_SetCook_Female");

	private static int HUD_SetMiner_Female = StringUtil.JenkinsHash("HUD_SetMiner_Female");

	private static int HUD_SetTrapper_Female = StringUtil.JenkinsHash("HUD_SetTrapper_Female");

	private static int HUD_SetAnimalFeeder_Female = StringUtil.JenkinsHash("HUD_SetAnimalFeeder_Female");

	private static int HUD_SetOrganizer_Female = StringUtil.JenkinsHash("HUD_SetOrganizer_Female");

	private static int HUD_SetBuilderRole_Female = StringUtil.JenkinsHash("HUD_SetBuilderRole_Female");

	private static int HUD_SetMedic_Female = StringUtil.JenkinsHash("HUD_SetMedic_Female");

	private static int HUD_FollowMe_Female = StringUtil.JenkinsHash("HUD_FollowMe_Female");

	private static int HUD_StopFollowingMe_Female = StringUtil.JenkinsHash("HUD_StopFollowingMe_Female");

	public static int[] Caption = new int[266]
	{
		0,
		StringUtil.JenkinsHash("HUD_SortOrderAscending"),
		StringUtil.JenkinsHash("HUD_SortOrderDescending"),
		StringUtil.JenkinsHash("HUD_SortByTime"),
		StringUtil.JenkinsHash("HUD_SortByType"),
		StringUtil.JenkinsHash("HUD_SortByWeight"),
		StringUtil.JenkinsHash("HUD_SortByWeightTotal"),
		StringUtil.JenkinsHash("HUD_SortByValue"),
		StringUtil.JenkinsHash("HUD_SortByValueTotal"),
		StringUtil.JenkinsHash("HUD_SortCharactersByTimeJoined"),
		StringUtil.JenkinsHash("HUD_SortCharactersByName"),
		StringUtil.JenkinsHash("HUD_SortCharactersByFitness"),
		StringUtil.JenkinsHash("HUD_SortCharactersByHandToHand"),
		StringUtil.JenkinsHash("HUD_SortCharactersByArchery"),
		StringUtil.JenkinsHash("HUD_SortCharactersByFirearms"),
		StringUtil.JenkinsHash("HUD_SortCharactersByStealth"),
		StringUtil.JenkinsHash("HUD_SortCharactersByConstruction"),
		StringUtil.JenkinsHash("HUD_SortCharactersByFarming"),
		StringUtil.JenkinsHash("HUD_SortCharactersByMedicine"),
		StringUtil.JenkinsHash("HUD_SortCharactersByCooking"),
		StringUtil.JenkinsHash("HUD_SortCharactersByConstitution"),
		StringUtil.JenkinsHash("PROMPT_TradeReset"),
		StringUtil.JenkinsHash("PROMPT_TradeConfirm"),
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		0,
		StringUtil.JenkinsHash("HUD_Empty"),
		StringUtil.JenkinsHash("HUD_NotInvestigated"),
		0,
		0,
		0,
		0,
		0,
		0,
		StringUtil.JenkinsHash("HUD_EditorSelect"),
		StringUtil.JenkinsHash("HUD_Control"),
		StringUtil.JenkinsHash("HUD_GoTo"),
		StringUtil.JenkinsHash("HUD_SetZone"),
		StringUtil.JenkinsHash("HUD_PauseZone"),
		StringUtil.JenkinsHash("HUD_ResumeZone"),
		StringUtil.JenkinsHash("HUD_ClearZone"),
		StringUtil.JenkinsHash("HUD_CopyZone"),
		StringUtil.JenkinsHash("PROMPT_SetMapMarker"),
		StringUtil.JenkinsHash("PROMPT_ClearMapMarker"),
		StringUtil.JenkinsHash("HUD_EnterBuilding"),
		StringUtil.JenkinsHash("HUD_EnterBuildingNotInvestigated"),
		StringUtil.JenkinsHash("HUD_ExitBuilding"),
		StringUtil.JenkinsHash("HUD_ChangeBuildingSlot"),
		StringUtil.JenkinsHash("HUD_PowerNap"),
		StringUtil.JenkinsHash("HUD_LoadNewMap"),
		StringUtil.JenkinsHash("HUD_Attack"),
		StringUtil.JenkinsHash("HUD_ChokeHold"),
		StringUtil.JenkinsHash("HUD_Assassinate"),
		StringUtil.JenkinsHash("HUD_Restrain"),
		StringUtil.JenkinsHash("HUD_Bludgeon"),
		StringUtil.JenkinsHash("HUD_Assassinate"),
		StringUtil.JenkinsHash("HUD_Slaughter"),
		StringUtil.JenkinsHash("HUD_FollowMe"),
		StringUtil.JenkinsHash("HUD_StopFollowingMe"),
		StringUtil.JenkinsHash("HUD_GroupFollowMe"),
		StringUtil.JenkinsHash("HUD_GroupStopFollowingMe"),
		StringUtil.JenkinsHash("HUD_SelectFollowers"),
		StringUtil.JenkinsHash("HUD_SelectEveryone"),
		StringUtil.JenkinsHash("HUD_SelectNoone"),
		0,
		StringUtil.JenkinsHash("HUD_TalkToInhabitant"),
		0,
		0,
		0,
		StringUtil.JenkinsHash("HUD_Feed"),
		StringUtil.JenkinsHash("HUD_GiveWater"),
		StringUtil.JenkinsHash("HUD_Bandage"),
		StringUtil.JenkinsHash("HUD_Inject"),
		StringUtil.JenkinsHash("HUD_Back"),
		StringUtil.JenkinsHash("HUD_Scavenge"),
		StringUtil.JenkinsHash("HUD_Steal"),
		StringUtil.JenkinsHash("HUD_ScavengeEmpty"),
		StringUtil.JenkinsHash("HUD_StealEmpty"),
		StringUtil.JenkinsHash("HUD_ScavengeNotInvestigated"),
		StringUtil.JenkinsHash("HUD_StealNotInvestigated"),
		StringUtil.JenkinsHash("HUD_TakeAll"),
		StringUtil.JenkinsHash("HUD_StealAll"),
		StringUtil.JenkinsHash("HUD_PickUp"),
		StringUtil.JenkinsHash("HUD_PickUpStealing"),
		StringUtil.JenkinsHash("HUD_Drop"),
		StringUtil.JenkinsHash("HUD_PutInBuilding"),
		StringUtil.JenkinsHash("HUD_Grab"),
		StringUtil.JenkinsHash("HUD_Steal"),
		StringUtil.JenkinsHash("HUD_Bury"),
		StringUtil.JenkinsHash("HUD_Eulogy"),
		StringUtil.JenkinsHash("HUD_VisitGrave"),
		StringUtil.JenkinsHash("HUD_Open"),
		StringUtil.JenkinsHash("HUD_Close"),
		StringUtil.JenkinsHash("HUD_Lock"),
		StringUtil.JenkinsHash("HUD_Unlock"),
		StringUtil.JenkinsHash("HUD_Knock"),
		StringUtil.JenkinsHash("HUD_SetGatePolicy"),
		StringUtil.JenkinsHash("HUD_SetBlockAnimals"),
		StringUtil.JenkinsHash("HUD_SetStoragePolicy"),
		StringUtil.JenkinsHash("HUD_SetPropName"),
		0,
		StringUtil.JenkinsHash("HUD_Build"),
		StringUtil.JenkinsHash("HUD_Dig"),
		StringUtil.JenkinsHash("HUD_ResumeBuilding"),
		StringUtil.JenkinsHash("HUD_SetBuilderRole"),
		StringUtil.JenkinsHash("HUD_StopBuilding"),
		StringUtil.JenkinsHash("HUD_Demolish"),
		StringUtil.JenkinsHash("HUD_Repair"),
		StringUtil.JenkinsHash("HUD_TakeOver"),
		StringUtil.JenkinsHash("HUD_Park"),
		StringUtil.JenkinsHash("HUD_Abandon"),
		StringUtil.JenkinsHash("HUD_ResetTrap"),
		StringUtil.JenkinsHash("HUD_WaterCrops"),
		StringUtil.JenkinsHash("HUD_Harvest"),
		StringUtil.JenkinsHash("HUD_StealCrops"),
		StringUtil.JenkinsHash("HUD_ClearCrops"),
		StringUtil.JenkinsHash("HUD_FillLiquidContainer"),
		StringUtil.JenkinsHash("HUD_FillLiquidContainer"),
		StringUtil.JenkinsHash("HUD_FillLiquidContainer"),
		StringUtil.JenkinsHash("HUD_ScoopSnow"),
		StringUtil.JenkinsHash("HUD_Plant"),
		StringUtil.JenkinsHash("HUD_ChopTree"),
		StringUtil.JenkinsHash("HUD_ChopLog"),
		StringUtil.JenkinsHash("HUD_ChopTreeStealing"),
		StringUtil.JenkinsHash("HUD_ChopLogStealing"),
		StringUtil.JenkinsHash("HUD_SetLumberjack"),
		StringUtil.JenkinsHash("HUD_ClearBush"),
		StringUtil.JenkinsHash("HUD_Mine"),
		StringUtil.JenkinsHash("HUD_MineStealing"),
		StringUtil.JenkinsHash("HUD_SetMiner"),
		StringUtil.JenkinsHash("HUD_LightFire"),
		StringUtil.JenkinsHash("HUD_LightFireWithMatch"),
		StringUtil.JenkinsHash("HUD_LightFireWithFlint"),
		StringUtil.JenkinsHash("HUD_AddMaterialToFire"),
		StringUtil.JenkinsHash("HUD_PourFuel"),
		StringUtil.JenkinsHash("HUD_PutOutFire"),
		StringUtil.JenkinsHash("HUD_PourInto"),
		StringUtil.JenkinsHash("HUD_Refuel"),
		StringUtil.JenkinsHash("HUD_Skin"),
		StringUtil.JenkinsHash("HUD_SitByFire"),
		StringUtil.JenkinsHash("HUD_RepairArmor"),
		StringUtil.JenkinsHash("HUD_SetFarmer"),
		StringUtil.JenkinsHash("HUD_SetCropsPatch"),
		0,
		StringUtil.JenkinsHash("HUD_SetGatherer"),
		StringUtil.JenkinsHash("HUD_SetGuard"),
		StringUtil.JenkinsHash("HUD_SetCook"),
		StringUtil.JenkinsHash("HUD_SetTrapper"),
		StringUtil.JenkinsHash("HUD_SetAnimalFeeder"),
		StringUtil.JenkinsHash("HUD_SetOrganizer"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_ResumeAllMyRoles"),
		StringUtil.JenkinsHash("HUD_ResumeAll"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		StringUtil.JenkinsHash("HUD_Equip"),
		StringUtil.JenkinsHash("HUD_Unequip"),
		StringUtil.JenkinsHash("HUD_Wear"),
		StringUtil.JenkinsHash("HUD_Strip"),
		StringUtil.JenkinsHash("HUD_Use"),
		StringUtil.JenkinsHash("HUD_Eat"),
		StringUtil.JenkinsHash("HUD_Eat"),
		StringUtil.JenkinsHash("HUD_StealAndEat"),
		StringUtil.JenkinsHash("HUD_Drink"),
		StringUtil.JenkinsHash("HUD_DrinkFromRiver"),
		StringUtil.JenkinsHash("HUD_Unload"),
		StringUtil.JenkinsHash("HUD_LoadAmmo"),
		StringUtil.JenkinsHash("HUD_Take"),
		StringUtil.JenkinsHash("HUD_Give"),
		StringUtil.JenkinsHash("HUD_Store"),
		StringUtil.JenkinsHash("HUD_Sell"),
		StringUtil.JenkinsHash("HUD_Buy"),
		StringUtil.JenkinsHash("HUD_DestroyItem"),
		StringUtil.JenkinsHash("HUD_PourAway"),
		StringUtil.JenkinsHash("HUD_StartPourInto"),
		StringUtil.JenkinsHash("HUD_PourInto"),
		StringUtil.JenkinsHash("HUD_SetFoodPolicy"),
		StringUtil.JenkinsHash("HUD_SetDrinkPolicy"),
		StringUtil.JenkinsHash("HUD_SetAmmoPolicy"),
		StringUtil.JenkinsHash("HUD_SetEquipmentPolicy"),
		StringUtil.JenkinsHash("HUD_SetEquipmentPolicy"),
		StringUtil.JenkinsHash("HUD_DesignateContents"),
		StringUtil.JenkinsHash("HUD_SelectLiquid"),
		StringUtil.JenkinsHash("HUD_DeselectLiquid"),
		StringUtil.JenkinsHash("HUD_OpenCraftMenu"),
		StringUtil.JenkinsHash("HUD_OpenDisassembleMenu"),
		StringUtil.JenkinsHash("HUD_OpenCookMenu"),
		0,
		StringUtil.JenkinsHash("HUD_ResumeCrafting"),
		StringUtil.JenkinsHash("HUD_SelectToGather"),
		StringUtil.JenkinsHash("HUD_SelectAllToGather"),
		StringUtil.JenkinsHash("HUD_Control"),
		StringUtil.JenkinsHash("HUD_Inventory"),
		StringUtil.JenkinsHash("HUD_ViewCharacter"),
		StringUtil.JenkinsHash("HUD_SelectCharacter"),
		StringUtil.JenkinsHash("HUD_DeselectLiquid"),
		StringUtil.JenkinsHash("HUD_Control"),
		StringUtil.JenkinsHash("HUD_StoreHere"),
		StringUtil.JenkinsHash("HUD_DontStoreHere"),
		StringUtil.JenkinsHash("HUD_SetMedic"),
		StringUtil.JenkinsHash("HUD_CancelRole"),
		StringUtil.JenkinsHash("HUD_ResumeRole"),
		StringUtil.JenkinsHash("HUD_PauseRole"),
		0,
		0,
		StringUtil.JenkinsHash("HUD_Pickpocket"),
		StringUtil.JenkinsHash("HUD_LightFuse")
	};

	public static int[] DisabledReasons = new int[100]
	{
		0,
		0,
		StringUtil.JenkinsHash("HINT_NoInjuries"),
		StringUtil.JenkinsHash("HINT_NoUnbandagedInjuries"),
		StringUtil.JenkinsHash("HINT_NoInjuriesWithLowerBandageLevel"),
		StringUtil.JenkinsHash("HINT_NoInfectedInjuries"),
		StringUtil.JenkinsHash("HINT_DontHaveAllIngredients"),
		StringUtil.JenkinsHash("HINT_DontHaveSuitableContainer"),
		StringUtil.JenkinsHash("HINT_DontHaveEnoughSuitableContainers"),
		StringUtil.JenkinsHash("HINT_DontHaveSealedContainer"),
		StringUtil.JenkinsHash("HINT_DontHaveEnoughSealedContainers"),
		0,
		StringUtil.JenkinsHash("HINT_ClothesDontFit"),
		StringUtil.JenkinsHash("HINT_FenceBuildingLimitReached"),
		StringUtil.JenkinsHash("HINT_UnsuitableTerrainForBuilding"),
		StringUtil.JenkinsHash("HINT_UnsuitableTerrainForPlanting"),
		StringUtil.JenkinsHash("HINT_TooColdForPlanting"),
		StringUtil.JenkinsHash("HINT_CantBuildNearOtherCommunity"),
		StringUtil.JenkinsHash("HINT_CantPlantNearOtherCommunity"),
		StringUtil.JenkinsHash("HINT_InventoryFull"),
		StringUtil.JenkinsHash("HINT_InventoryFullSpaceNeeded"),
		StringUtil.JenkinsHash("HINT_ConstructionSkillTooLow"),
		StringUtil.JenkinsHash("HINT_CookingSkillTooLow"),
		StringUtil.JenkinsHash("HINT_FarmingSkillTooLow"),
		StringUtil.JenkinsHash("HINT_HandToHandSkillTooLow"),
		StringUtil.JenkinsHash("HINT_MedicalSkillTooLow"),
		StringUtil.JenkinsHash("HINT_NeedToolbox"),
		StringUtil.JenkinsHash("HINT_NeedShovel"),
		StringUtil.JenkinsHash("HINT_NeedHuntingKnife"),
		StringUtil.JenkinsHash("HINT_NeedPot"),
		StringUtil.JenkinsHash("HINT_NeedFryingPan"),
		StringUtil.JenkinsHash("HINT_NeedEmptyPot"),
		StringUtil.JenkinsHash("HINT_BuildingFull"),
		StringUtil.JenkinsHash("HINT_OccupiedByOtherCommunity"),
		StringUtil.JenkinsHash("HINT_OwnedByOtherCommunity"),
		StringUtil.JenkinsHash("HINT_LiquidContainerIsEmpty"),
		StringUtil.JenkinsHash("HINT_NeedWaterForCrops"),
		StringUtil.JenkinsHash("HINT_AlreadyContainsOtherLiquid"),
		StringUtil.JenkinsHash("HINT_AlreadyFullOfWater"),
		StringUtil.JenkinsHash("HINT_CantOpenEnemyGate"),
		StringUtil.JenkinsHash("HINT_CantOpenUnintroducedGate"),
		StringUtil.JenkinsHash("HINT_GateLocked"),
		StringUtil.JenkinsHash("HINT_InventoryLocked"),
		StringUtil.JenkinsHash("HINT_CampfireWoodFull"),
		StringUtil.JenkinsHash("HINT_CampfireInUse"),
		StringUtil.JenkinsHash("HINT_CampfireHasItems"),
		StringUtil.JenkinsHash("HINT_CantDemolishInventoryNotEmpty"),
		StringUtil.JenkinsHash("HINT_CantDemolishInhabitants"),
		StringUtil.JenkinsHash("HINT_CantDemolishEncumbered"),
		StringUtil.JenkinsHash("HINT_NeedFuelToLightFire"),
		StringUtil.JenkinsHash("HINT_NeedWoodToLightFire"),
		StringUtil.JenkinsHash("HINT_NeedMoreWaterToPutOutFire"),
		StringUtil.JenkinsHash("HINT_RiverIsFrozen"),
		StringUtil.JenkinsHash("HINT_WrongPersonality"),
		StringUtil.JenkinsHash("HINT_NeedWateringCan"),
		StringUtil.JenkinsHash("HINT_MaxCropPatchTilesReached"),
		StringUtil.JenkinsHash("HINT_AlreadyReadBook"),
		StringUtil.JenkinsHash("HINT_TooBusyToTalk"),
		StringUtil.JenkinsHash("HINT_Depressed"),
		StringUtil.JenkinsHash("HINT_RefuseToAttack"),
		StringUtil.JenkinsHash("HINT_OnlyPartyLeaderCanUseSaveTokens"),
		StringUtil.JenkinsHash("HINT_AlreadySkinned"),
		StringUtil.JenkinsHash("HINT_EquipmentNeeded"),
		StringUtil.JenkinsHash("HINT_GatePolicyIsAlwaysClosed"),
		StringUtil.JenkinsHash("HINT_AlreadyScoopedSnowHere"),
		StringUtil.JenkinsHash("HINT_AlreadyHasGift"),
		StringUtil.JenkinsHash("HINT_CantPourIntoBottles"),
		StringUtil.JenkinsHash("HINT_ArmorIsAllRepaired"),
		StringUtil.JenkinsHash("HINT_CannotChangeClothesWhileInCombat"),
		StringUtil.JenkinsHash("HINT_NoRichDeposits"),
		StringUtil.JenkinsHash("HINT_NotHungry"),
		StringUtil.JenkinsHash("HINT_NeedBandage"),
		StringUtil.JenkinsHash("HINT_NeedAntigen"),
		StringUtil.JenkinsHash("HINT_NeedFood"),
		StringUtil.JenkinsHash("HINT_NeedWater"),
		StringUtil.JenkinsHash("HINT_TooBusyToFollow"),
		StringUtil.JenkinsHash("HINT_TooManyRoles"),
		StringUtil.JenkinsHash("HINT_Unconscious"),
		StringUtil.JenkinsHash("HINT_TimeBetweenPowerNaps"),
		StringUtil.JenkinsHash("HINT_TooWarmForSnowballs"),
		StringUtil.JenkinsHash("HINT_AlreadyFullOfFuel"),
		StringUtil.JenkinsHash("HINT_NeedLiquid"),
		StringUtil.JenkinsHash("HINT_NeedPlayerInVehicle"),
		StringUtil.JenkinsHash("HINT_NeedMoreFuel"),
		StringUtil.JenkinsHash("HINT_CantLeaveWhileBleeding"),
		StringUtil.JenkinsHash("HINT_CantLeaveWhileInfected"),
		StringUtil.JenkinsHash("HINT_CantLeaveWhilePregnant"),
		StringUtil.JenkinsHash("HINT_CantLeaveWhileVehicleDamaged"),
		StringUtil.JenkinsHash("HINT_CantLeaveWithoutRelation"),
		StringUtil.JenkinsHash("HINT_CantLeaveWithCorpse"),
		StringUtil.JenkinsHash("HINT_InLabor"),
		StringUtil.JenkinsHash("HINT_NeedToRepair"),
		StringUtil.JenkinsHash("HINT_TakingABreak"),
		StringUtil.JenkinsHash("HINT_EveryoneTakingABreak"),
		StringUtil.JenkinsHash("HINT_NotSleepy"),
		StringUtil.JenkinsHash("HINT_ExitBlocked"),
		StringUtil.JenkinsHash("HINT_RefuseToEat"),
		StringUtil.JenkinsHash("HUD_WantToKeep"),
		StringUtil.JenkinsHash("HINT_CantPourIntoBottles2"),
		StringUtil.JenkinsHash("HINT_StealthSkillTooLow")
	};

	public static string SensibleFloatFormat = "#0.#";

	public static int HINT_InventoryFullSpaceNeededKg = StringUtil.JenkinsHash("HINT_InventoryFullSpaceNeededKg");

	public static int HINT_DontHaveEnoughSuitableContainersLiters = StringUtil.JenkinsHash("HINT_DontHaveEnoughSuitableContainersLiters");

	public static int HINT_DontHaveEnoughSealedContainersLiters = StringUtil.JenkinsHash("HINT_DontHaveEnoughSealedContainersLiters");

	public static int HINT_NoCookingRecipes = StringUtil.JenkinsHash("HINT_NoCookingRecipes");

	public static int HINT_NoCraftingRecipes = StringUtil.JenkinsHash("HINT_NoCraftingRecipes");

	public static int HINT_NoBuildingRecipes = StringUtil.JenkinsHash("HINT_NoBuildingRecipes");

	public static int HINT_InventoryLocked_Female = StringUtil.JenkinsHash("HINT_InventoryLocked_Female");

	public static int MaxQueuedBuildings = 100;

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, int amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, EquipmentPrototype proto, int amount, CursorActionDisabledReason enabled, InputFunction shortcut)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = shortcut;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, EquipmentPrototype proto, InfectionType infectionType, int amount, CursorActionDisabledReason enabled, InputFunction shortcut)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = null;
		InfectedWith = infectionType;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = shortcut;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TileObject obj, BaseObject target, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TileObject obj, BaseObject target, EquipmentPrototype proto, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TileObject obj, BaseObject target, LiquidPrototype liquid, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = liquid;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TileObject obj, BaseObject target, int amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TileObject obj, BaseObject target, CursorActionDisabledReason enabled, CantTransferReason reason)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = reason;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, TerrainCoord tile, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = null;
		Object = null;
		Tile = tile;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, TerrainCoord tile, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = tile;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, EquipmentPrototype proto, TerrainCoord tile, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = null;
		Object = null;
		Tile = tile;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, BaseObject target, EquipmentPrototype proto, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character speaker, Character listener, BaseObject speechObject, MemoryParam speechParam, CursorActionDisabledReason enabled, Speech speech, string speechText)
	{
		ActionType = actionType;
		Actor = speaker;
		Object = listener;
		Target = speechObject;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = speech;
		SpeechText = speechText;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = speechParam;
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, string text)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = CursorActionDisabledReason.Enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = text;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, string text, float offsetX, bool leftAligned)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = CursorActionDisabledReason.Enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = text;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = ((!leftAligned) ? 1 : (-1));
		FloatAmount = offsetX;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, Recipe recipe, BaseObject target, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = recipe;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, Recipe recipe, TileObject obj, BaseObject target, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = recipe;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Character actor, Recipe recipe, BaseObject target, TerrainCoord tile, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = actor;
		Target = target;
		Object = null;
		Tile = tile;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = recipe;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, EquipmentPrototype proto, LiquidPrototype liquid, int amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = liquid;
		InfectedWith = InfectionType.None;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, TileObject obj, EquipmentPrototype proto, LiquidPrototype liquid, float amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = liquid;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = amount;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, TileObject obj, EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, float amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = obj;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = liquid;
		InfectedWith = infectionType;
		Amount = 0;
		FloatAmount = amount;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, Community community, EquipmentPrototype proto, LiquidPrototype liquid, int amount, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = null;
		Target = community;
		Object = null;
		Tile = TerrainCoord.Invalid;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = proto;
		Liquid = liquid;
		InfectedWith = InfectionType.None;
		Amount = amount;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = MapMarkerType.White;
	}

	public AvailableAction(CursorAction actionType, MapMarkerType markerType, TerrainCoord tile, CursorActionDisabledReason enabled)
	{
		ActionType = actionType;
		Actor = null;
		Target = null;
		Object = null;
		Tile = tile;
		Enabled = enabled;
		Reason = CantTransferReason.CanTransfer;
		Speech = null;
		SpeechText = null;
		Recipe = null;
		Proto = null;
		Liquid = null;
		InfectedWith = InfectionType.None;
		Amount = 0;
		FloatAmount = 0f;
		Shortcut = InputFunction.Invalid;
		SpeechParam = default(MemoryParam);
		MarkerType = markerType;
	}

	public bool IsEqual(AvailableAction other)
	{
		if (ActionType == other.ActionType && Enabled == other.Enabled && Actor == other.Actor && Object == other.Object && Target == other.Target && Speech == other.Speech && Recipe == other.Recipe && Proto == other.Proto)
		{
			return Liquid == other.Liquid;
		}
		return false;
	}

	public bool IsSelectable()
	{
		switch (ActionType)
		{
		case CursorAction.None:
		case CursorAction.Separator:
		case CursorAction.DisplayNameAndIcon:
		case CursorAction.CharacterName:
		case CursorAction.EquipmentName:
		case CursorAction.EquipmentDescription:
		case CursorAction.BasePriceAndWeight:
		case CursorAction.TradePriceAndWeight:
		case CursorAction.SalePriceAndWeight:
		case CursorAction.MarkedUpSalePriceAndWeight:
		case CursorAction.Insulation:
		case CursorAction.LoadedAmmo:
		case CursorAction.SightBonus:
		case CursorAction.SkillBonus:
		case CursorAction.SkillOnConsumption:
		case CursorAction.StartingPoints:
		case CursorAction.EquipmentTotalName:
		case CursorAction.Tooltip:
		case CursorAction.WeaponStats:
		case CursorAction.TastinessStat:
		case CursorAction.SpeechToReplyTo:
			return false;
		default:
			return true;
		}
	}

	public string GetCaption()
	{
		switch (ActionType)
		{
		case CursorAction.None:
		case CursorAction.Tooltip:
		case CursorAction.TalkTo:
		case CursorAction.SpeechToReplyTo:
		case CursorAction.OpenGiftMenu:
			return SpeechText;
		case CursorAction.Craft:
			return GameImpl.Translate(Recipe.NameHash);
		case CursorAction.LocateEquipment:
			return HudBehaviour.Instance.BuildLocateEquipmentString(this).ToString();
		case CursorAction.CraftingLimit:
			return HudBehaviour.Instance.BuildCraftingLimitString(this).ToString();
		case CursorAction.LocateProp:
			return Object.GetDisplayNameString();
		case CursorAction.TakeOver:
			if (Target is EnterableVehicle)
			{
				return GameImpl.Translate(Caption[130]);
			}
			break;
		case CursorAction.FollowMe:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_FollowMe_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.StopFollowingMe:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_StopFollowingMe_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.Empty:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_Empty_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.ScavengeEmpty:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_ScavengeEmpty_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.StealEmpty:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_StealEmpty_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.NotInvestigated:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_NotInvestigated_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.ScavengeNotInvestigated:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_ScavengeNotInvestigated_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.StealNotInvestigated:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_StealNotInvestigated_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.EnterBuildingNotInvestigated:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_EnterBuildingNotInvestigated_Female, Target.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.Guard:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetGuard_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetFarmer:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetFarmer_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetLumberjack:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetLumberjack_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.Cook:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetCook_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetMiner:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetMiner_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetTrapper:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetTrapper_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetBuilderRole:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetBuilderRole_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetAnimalFeeder:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetAnimalFeeder_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetOrganizer:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetOrganizer_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		case CursorAction.SetMedic:
			return GameImpl.Translate(Caption[(int)ActionType], HUD_SetMedic_Female, Actor.GetGender(GameImpl.Instance.Settings.Language));
		}
		return GameImpl.Translate(Caption[(int)ActionType]);
	}

	public void BuildDisabledReasonString(StringBuilder sb)
	{
		sb.Length = 0;
		if (DisabledReasons[(int)Enabled] == 0)
		{
			return;
		}
		sb.Append(GameImpl.Translate(DisabledReasons[(int)Enabled]));
		switch (Enabled)
		{
		case CursorActionDisabledReason.FenceBuildingLimitReached:
			sb.Replace("%1", MaxQueuedBuildings.ToString());
			StringUtil.ApplyFormulae(sb, Actor, MaxQueuedBuildings);
			break;
		case CursorActionDisabledReason.NoInjuriesWithLowerBandageLevel:
			sb.Replace("%1", Amount.ToString());
			StringUtil.ApplyFormulae(sb, Actor, Amount);
			break;
		case CursorActionDisabledReason.LiquidContainerIsEmpty:
		case CursorActionDisabledReason.AlreadyContainsOtherLiquid:
		case CursorActionDisabledReason.AlreadyFullOfWater:
			sb.Replace("%1", Actor.EquippedItem.GetDisplayNameString());
			StringUtil.ApplyFormulae(sb, Actor, Actor.EquippedItem);
			break;
		case CursorActionDisabledReason.DontEnoughSuitableContainers:
		case CursorActionDisabledReason.DontEnoughSealedContainers:
		{
			bool useMetricWeights3 = GameImpl.Instance.Settings.UseMetricWeights;
			if (useMetricWeights3)
			{
				sb.Length = 0;
				sb.Append(GameImpl.Translate((Enabled == CursorActionDisabledReason.DontEnoughSealedContainers) ? HINT_DontHaveEnoughSealedContainersLiters : HINT_DontHaveEnoughSuitableContainersLiters));
			}
			Recipe.HasSuitableContainer(Actor, Actor.EquippedItem, out var _, out var productAmount, out var freeCapacity);
			productAmount *= (useMetricWeights3 ? 0.0295735f : 1f);
			freeCapacity *= (useMetricWeights3 ? 0.0295735f : 1f);
			sb.Replace("%1", productAmount.ToString(SensibleFloatFormat));
			sb.Replace("%2", freeCapacity.ToString(SensibleFloatFormat));
			StringUtil.ApplyFormulae(sb, Actor, productAmount, freeCapacity);
			break;
		}
		case CursorActionDisabledReason.InventoryFullSpaceNeeded:
		{
			bool useMetricWeights2 = GameImpl.Instance.Settings.UseMetricWeights;
			if (useMetricWeights2)
			{
				sb.Length = 0;
				sb.Append(GameImpl.Translate(HINT_InventoryFullSpaceNeededKg));
			}
			float num = Proto.Weight * (useMetricWeights2 ? 0.45359236f : 1f);
			float num2 = Actor.GetAvailableInventorySpace() * (useMetricWeights2 ? 0.45359236f : 1f);
			sb.Replace("%1", num.ToString(SensibleFloatFormat));
			sb.Replace("%2", num2.ToString(SensibleFloatFormat));
			StringUtil.ApplyFormulae(sb, Actor, num, num2);
			break;
		}
		case CursorActionDisabledReason.WrongPersonality:
		case CursorActionDisabledReason.NotHungry:
		case CursorActionDisabledReason.Unconscious:
		case CursorActionDisabledReason.TakingABreak:
		case CursorActionDisabledReason.NotSleepy:
		case CursorActionDisabledReason.RefuseToEat:
			sb.Replace("%1", Actor.GetDisplayNameString());
			StringUtil.ApplyFormulae(sb, Actor, Actor);
			break;
		case CursorActionDisabledReason.AlreadyReadBook:
		case CursorActionDisabledReason.AlreadyHasGift:
		case CursorActionDisabledReason.WantToKeepGift:
			sb.Replace("%1", Object.GetDisplayNameString(noStrangers: false, englishOnly: false));
			StringUtil.ApplyFormulae(sb, Actor, Object);
			break;
		case CursorActionDisabledReason.TooBusyToTalk:
		case CursorActionDisabledReason.InLabor:
			if (Object is Human human)
			{
				int num3 = human.CalcAccompaniedAmountInLabor();
				sb.Replace("%1", human.GetDisplayNameString(noStrangers: false, englishOnly: false));
				sb.Replace("%2", num3.ToString());
				StringUtil.ApplyFormulae(sb, Actor, human, num3);
			}
			break;
		case CursorActionDisabledReason.TooDepressed:
		case CursorActionDisabledReason.TooBusyToFollow:
			if (Target is Character character)
			{
				sb.Replace("%1", character.GetDisplayNameString(noStrangers: false, englishOnly: false));
				StringUtil.ApplyFormulae(sb, Actor, character);
			}
			else
			{
				if (!(Target is Building building))
				{
					break;
				}
				Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
				Character[] inhabitants = building.Inhabitants;
				foreach (Character character2 in inhabitants)
				{
					if (character2 != null && character2 != localControlledCharacter && !character2.IsInSameSquad(localControlledCharacter) && character2.CanFollowPlayerIncludeAllies())
					{
						sb.Replace("%1", character2.GetDisplayNameString(noStrangers: false, englishOnly: false));
						StringUtil.ApplyFormulae(sb, Actor, character2);
					}
				}
			}
			break;
		case CursorActionDisabledReason.RefuseToAttack:
			sb.Replace("%1", Actor.GetDisplayNameString(noStrangers: true, englishOnly: false));
			sb.Replace("%2", Object.GetDisplayNameString(noStrangers: false, englishOnly: false));
			StringUtil.ApplyFormulae(sb, Actor, Actor, Target);
			break;
		case CursorActionDisabledReason.CampfireInUse:
			if (Target is Campfire { CurrentCrafter: not null } campfire)
			{
				sb.Replace("%1", campfire.CurrentCrafter.GetDisplayNameString(noStrangers: true, englishOnly: false));
				StringUtil.ApplyFormulae(sb, Actor, campfire.CurrentCrafter);
			}
			break;
		case CursorActionDisabledReason.EquipmentNeeded:
			sb.Replace("%1", GameImpl.Translate(Proto.NameHash));
			StringUtil.ApplyFormulae(sb, Actor, Proto);
			break;
		case CursorActionDisabledReason.CantPourIntoBottles2:
			sb.Replace("%1", GameImpl.Translate(Liquid.NameHash));
			StringUtil.ApplyFormulae(sb, Actor, Liquid);
			break;
		case CursorActionDisabledReason.ConstructionSkillTooLow:
		case CursorActionDisabledReason.CookingSkillTooLow:
		case CursorActionDisabledReason.FarmingSkillTooLow:
		case CursorActionDisabledReason.HandToHandSkillTooLow:
		case CursorActionDisabledReason.MedicalSkillTooLow:
		case CursorActionDisabledReason.StealthSkillTooLow:
		{
			int num4 = ActionType switch
			{
				CursorAction.TakeOver => ((TileObject)Target).GetCaptureSkillNeeded(), 
				CursorAction.Repair => ((TileObject)Target).GetRepairSkillNeeded(), 
				CursorAction.LightFuse => GameCursor.GetSkillLevelNeededToLightPipeBombFuse(Object), 
				_ => Recipe.SkillLevel, 
			};
			sb.Replace("%1", num4.ToString());
			StringUtil.ApplyFormulae(sb, Actor, num4);
			break;
		}
		case CursorActionDisabledReason.TooManyRoles:
			sb.Replace("%1", 10.ToString());
			StringUtil.ApplyFormulae(sb, Actor, 10f);
			break;
		case CursorActionDisabledReason.NeedLiquid:
			if (Object is Prop prop && prop.GetLiquidType() != null)
			{
				sb.Replace("%1", GameImpl.Translate(prop.GetLiquidType().GetNameKey()));
				StringUtil.ApplyFormulae(sb, Actor, prop.GetLiquidType());
			}
			break;
		case CursorActionDisabledReason.NeedPlayerInVehicle:
		{
			foreach (Character member in Session.Instance.CommunityManager.PlayerCommunity.Members)
			{
				if (member.AliveAndNotZombie && member.IsPlayerAvatar() && member.InsideBuilding != Target)
				{
					sb.Replace("%1", member.GetDisplayNameString());
					StringUtil.ApplyFormulae(sb, Actor, member);
					break;
				}
			}
			break;
		}
		case CursorActionDisabledReason.CantLeaveWhileBleeding:
			if (!(Target is EnterableVehicle { Inhabitants: var inhabitants2 }))
			{
				break;
			}
			foreach (Character character3 in inhabitants2)
			{
				if (character3 != null && character3.HasUnbandagedInjury(0))
				{
					sb.Replace("%1", character3.GetDisplayNameString());
					StringUtil.ApplyFormulae(sb, Actor, character3);
					break;
				}
			}
			break;
		case CursorActionDisabledReason.CantLeaveWhileInfected:
			if (!(Target is EnterableVehicle { Inhabitants: var inhabitants4 }))
			{
				break;
			}
			foreach (Character character6 in inhabitants4)
			{
				if (character6 != null && character6.HasAnyInfectedInjuries())
				{
					sb.Replace("%1", character6.GetDisplayNameString());
					StringUtil.ApplyFormulae(sb, Actor, character6);
					break;
				}
			}
			break;
		case CursorActionDisabledReason.CantLeaveWhilePregnant:
			if (!(Target is EnterableVehicle { Inhabitants: var inhabitants3 }))
			{
				break;
			}
			foreach (Character character5 in inhabitants3)
			{
				if (character5 != null && character5.IsPregnant())
				{
					sb.Replace("%1", character5.GetDisplayNameString());
					StringUtil.ApplyFormulae(sb, Actor, character5);
					break;
				}
			}
			break;
		case CursorActionDisabledReason.CantLeaveWithoutRelation:
			if (Target is EnterableVehicle vehicle)
			{
				Character relation;
				Character character4 = GameCursor.CheckForRelationsWeCantLeaveWithout(vehicle, out relation);
				if (character4 != null && relation != null)
				{
					int relationshipNameHash = Relationship.GetRelationshipNameHash(Relationship.GetRelationship(character4, relation), relation.GetGender());
					StringUtil.ApplyFormulae(sb, Actor, character4, relation, relation);
					sb.Replace("%1", character4.GetDisplayNameString());
					sb.Replace("%2", GameImpl.Translate(relationshipNameHash));
					sb.Replace("%3", relation.GetDisplayNameString());
				}
			}
			break;
		case CursorActionDisabledReason.NeedMoreFuel:
		{
			GameImpl instance2 = GameImpl.Instance;
			if (Target is EnterableVehicle enterableVehicle)
			{
				bool useMetricWeights = instance2.Settings.UseMetricWeights;
				sb.Append(' ');
				sb.Append('(');
				sb.AppendWithoutGarbage(enterableVehicle.GetLiquidAmount() * (useMetricWeights ? 0.0295735f : 1f), 1);
				sb.Append('/');
				sb.AppendWithoutGarbage(enterableVehicle.GetLiquidCapacity() * (useMetricWeights ? 0.0295735f : 1f) * (instance2.CurrentStory.Settings.HitTheRoadMinFuelPercent / 100f), 1);
				sb.Append(' ');
				sb.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
				sb.Append(')');
			}
			StringUtil.ApplyFormulae(sb, Actor, Target);
			break;
		}
		case CursorActionDisabledReason.CantLeaveWhileVehicleDamaged:
		{
			GameImpl instance = GameImpl.Instance;
			if (Target is EnterableVehicle)
			{
				sb.Append(' ');
				sb.Append('(');
				sb.Append('<');
				sb.AppendWithoutGarbage(instance.CurrentStory.Settings.HitTheRoadMaxDamagePercent / 100f, 1);
				sb.Append('%');
				sb.Append(')');
			}
			StringUtil.ApplyFormulae(sb, Actor, Target);
			break;
		}
		case CursorActionDisabledReason.InventoryLocked:
			StringUtil.ApplyFormulae(sb, Actor, Target);
			break;
		default:
			StringUtil.ApplyFormulae(sb, Actor, Target);
			break;
		}
	}

	public void GetTransferVars(out TileObject carrier, out TileObject to, out Equipment item, out bool trading, out int minTransferrable, out int maxTransferrable)
	{
		trading = ActionType == CursorAction.Sell || ActionType == CursorAction.Buy;
		carrier = ((ActionType == CursorAction.Take || ActionType == CursorAction.Buy) ? Object : Actor);
		to = ((ActionType == CursorAction.Take || ActionType == CursorAction.Buy) ? Actor : Object);
		item = (Equipment)Target;
		minTransferrable = 1;
		maxTransferrable = item.GetAmount();
		if (trading)
		{
			TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
			if (tradePage != null)
			{
				maxTransferrable = tradePage.GetAmountAfterPendingTrade(item, carrier);
			}
		}
		else
		{
			TakePage takePage = InfoScreen.Instance.GetCurrentPage() as TakePage;
			if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
			{
				maxTransferrable = 1;
			}
			else
			{
				maxTransferrable = Math.Min(maxTransferrable, item.GetMaxTransferrableTo(to));
			}
		}
	}
}

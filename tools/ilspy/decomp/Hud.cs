using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Hud
{
	public enum OpenInfoScreenType
	{
		Inventory,
		Notes,
		BrainScan,
		Gather,
		ExitBrainScanToMap,
		QuestPage,
		CommunityPage
	}

	public static Resource<GameObject> FocusedArrow;

	public static Resource<GameObject> FocusedArrowLarge;

	public static Resource<GameObject> BloodLossBar;

	public static Resource<GameObject> OverheadActionIcon;

	public static Resource<GameObject> CraftingProgressIndicator;

	public static Resource<GameObject> CraftingProgressResource;

	public static Resource<GameObject> QuestDestinationArrow;

	public static Resource<GameObject> PlayerNamePrefab;

	public static Resource<GameObject> PlayerDisplayPrefab;

	public static Resource<GameObject> WorldSpeechBubble;

	public static Resource<GameObject> ActionWrapper;

	public static Resource<GameObject> AvailableActionPrefab;

	public static Resource<GameObject> PriceAndWeight;

	public static Resource<GameObject> SkillBonus;

	public static Resource<GameObject> CropRating;

	public static Resource<GameObject> ActionSeparator;

	public static Resource<GameObject> Insulation;

	public static Resource<GameObject> CraftAction;

	public static Resource<GameObject> CraftActionLine;

	public static Resource<GameObject> RecipeItem;

	public static Resource<GameObject> RecipeLiquidContainer;

	public static Resource<GameObject> RecipeInfectedLiquidContainer;

	public static Resource<GameObject> RecipeText;

	public static Resource<GameObject> RecipeSpacer;

	public static Resource<GameObject> RecipeTextIconPair;

	public static Resource<GameObject> IngredientIcon;

	public static Resource<GameObject> IngredientText;

	public static Resource<GameObject> BuildHere;

	public static Resource<GameObject> BuildingIngredient;

	public static Resource<GameObject> EquipmentSelect;

	public static Resource<GameObject> NameAndIcon;

	public static Resource<GameObject> WeaponStats;

	public static Resource<GameObject> TastinessStat;

	public static Resource<GameObject> ButtonPrompt;

	public static Resource<GameObject> Snore;

	public static Resource<GameObject> ShortcutPrefab;

	public static Resource<GameObject> StarsPanelPrefab;

	public static Resource<GameObject> SkillsChangePopupPrefab;

	public static Resource<Material> MapCursorMat;

	public static Resource<Material> MapIconMat;

	public static Resource<Material> OpinionGraphMat;

	public static Resource<Material> OutlineBlackMat;

	public static Resource<Material> OutlineRedMat;

	public static Resource<Material> Opaque;

	public static Resource<Material> HalftonePip;

	public static Resource<Texture2D> WeightIcon;

	public static Resource<Texture2D> GoldIcon;

	public static Resource<Texture2D> Dot;

	public static Resource<Texture2D> SmallDot;

	public static Resource<Texture2D> TargetBodyLocation;

	public static Resource<Texture2D> TargetBodyLocationCrippled;

	public static Resource<Texture2D> DeerIcon;

	public static Hud Instance;

	public GameCursor Cursor = new GameCursor();

	public Pip Pip = new Pip();

	public Minimap Minimap = new Minimap();

	public Character LocalControlledCharacter;

	public TileObject LocalTargetObject;

	public TileObject PotentialTargetObject;

	public TileObject[] PotentialTargetsOnEachSide = new TileObject[2];

	public bool LocalWantLockOnTarget;

	public bool LocalPressingAim;

	public bool LocalTargettingSky;

	public Vector3 LocalTargetPos;

	public float LocalThrowAngle;

	public float LocalThrowSpeed;

	public float LocalThrowTime;

	public Vector3 LocalThrowHitPos;

	public bool LocalThrowBlocked;

	public TargettableBodyLocation LocalTargetBodyLocationToAimFor;

	public TargettableBodyLocation PotentialTargetBodyLocationToAimFor;

	public float PotentialTargetSwitchAmount;

	public List<Character> SelectedCharacters = new List<Character>();

	public Character BuildingSelectedInhabitant;

	public bool Dragging;

	public bool ReallyDragging;

	public Vector3 DragStartWorldPos;

	public Vector2 DragStartCursorPos;

	public bool DragSelecting;

	public bool ReallyDragSelecting;

	public float DragSelectHoldTime;

	public Vector2 DragSelectStartXZ;

	public Vector2 DragSelectFinishXZ;

	public bool DraggingZone;

	public bool SentMovementZone;

	public TerrainCoord DragZoneStartTile;

	public TerrainCoord DragZoneFinishTile;

	private float BodyLocationCountdown;

	private float TargetSwitchCountdown;

	public static float Cos45 = (float)Math.Cos(0.7853981852531433);

	public static float DefaultMouseLookSensitivity = 0.5f;

	public static float DefaultMouseTargetThreshold = 50f;

	public static float DefaultMouseBodyLocationThreshold = 15f;

	public static float GamepadTargetSwitchThreshold = 0.9f;

	public static float BodyLocationCountdownTime = 1f / 6f;

	public static float TargetSwitchCountdownTime = 1f / 3f;

	public static float MinDragDist = 0.2f;

	private Vector2[] MouseTargetHistory = new Vector2[5];

	private int MouseTargetHistoryIndex;

	private TileObject SentTargetObject;

	private bool SentWantLockOnTarget;

	private Vector3 SendTargetPos;

	private float SendThrowAngle;

	private float SendThrowSpeed;

	private TargettableBodyLocation SentTargetBodyLocationToAimFor;

	private bool SentIsInInfoScreen;

	public TileObject CursorTargetObject;

	public RaycastResult CursorRayCastResult;

	public TileObject EditorSelectedObject;

	private TimeSpan LastClickTime;

	public float HoldTime;

	public float BrainScanHoldTime;

	public static float HoldTimeout = 1f;

	public static float ThrowAngleExtraPitch = 15f;

	public Character ParryPromptAttacker;

	public int ParryPromptHash;

	public bool ParryPrompt;

	public bool ParryPromptEnabled;

	public bool ParryPromptSuccess;

	public float ParryPromptCurrentSize = 1f;

	public float ParryPromptPrevSize = 1f;

	public float ParryPromptSuccessTransition;

	public float ParryPromptTargetEscapePower;

	public float ParryPromptCurrentEscapePower;

	public float ParryPromptTimeoutRadius;

	public int LastParryPromptFrame;

	public InputFunction BigHintPrompt = InputFunction.Invalid;

	public int BigHintPromptHash;

	public List<TileObject> PotentialTargets = new List<TileObject>();

	public List<TileObject> PotentialTargetsForStick = new List<TileObject>();

	public List<Character> CharactersInSelectionBox = new List<Character>();

	public List<TileObject> BuildingsInSelectionBox = new List<TileObject>();

	private static List<Character> TempCharacters = new List<Character>();

	private static string PickTargetInDirectControlModeStr = "PickTargetInDirectControlMode";

	private static int HUD_Cancel = StringUtil.JenkinsHash("HUD_Cancel");

	public static float ReticuleHeight = 0.25f;

	public static float PotentialTargetHeight = 0.05f;

	public static float ReticuleOffsetFromTargetPos = 1f;

	public static float TargetBodyLocationHeight = 0.5f;

	public static float CamouflageIconYOffset = 0.2f;

	public static float CamouflageIconScale = 1f;

	private int WantCursorAnimHash;

	public TileObject OpenInfoScreenFor;

	public OpenInfoScreenType WantInfoScreenType;

	private static string HudStr = "Hud";

	private static float FlyCamTargetableRadius = 1.5f;

	private static float ReticulePitch = 8f;

	private static float ReticulePitchInWatchTower = 4f;

	private static List<Character> NearbyCharacters = new List<Character>();

	public static float DirectControlTargetableRadius = 1.5f;

	public static float DirectControlTargetableSittingRadius = 12f;

	public static float DirectControlTargetableLockRadiusHysteresis = 10f;

	public static float DirectControlTargetableConversationRadius = 5f;

	public static float DirectControlTargetableAngleFrac = 1.5f;

	public static float DirectControlTargetableThrowableAngleDeg = 2f;

	public static float DirectControlTargetableChokeHoldRadius = 3f;

	public static float DirectControlTargetAngleBiasPer90Deg = 16f;

	public float FocusedCharacterArrowTimeout;

	public float PlayerNameTimeout = 8f;

	public static float DamageVignetteAmountWhenLosingBlood = 0.5f;

	public float VignetteAmount;

	public TimeSpan WantVignetteTillTime = TimeSpan.FromSeconds(-100000.0);

	private int PantingCharacterId;

	private static float DotSeparation = 0.5f;

	private static float DotRadius = 0.05f;

	private static float DotRadiusZoomedOut = 0.125f;

	private static List<TileObject> TempObjects = new List<TileObject>();

	private static string DrawThrowingArcStr = "DrawThrowingArc";

	private static float SelectionBoxYOffset = 0.1f;

	private static float SelectionLineThickness = 0.05f;

	private static float ZoneLineThickness = 0.1f;

	public TileObject LocalControlledCharacterOrBuildingTheyAreIn
	{
		get
		{
			if (LocalControlledCharacter != null && LocalControlledCharacter.InsideBuilding != null && !LocalControlledCharacter.InTerrain)
			{
				return LocalControlledCharacter.InsideBuilding;
			}
			return LocalControlledCharacter;
		}
	}

	public static void LoadContent()
	{
		FocusedArrow = new Resource<GameObject>("Prefabs/HUD/FocusedArrow");
		FocusedArrowLarge = new Resource<GameObject>("Prefabs/HUD/FocusedArrowLarge");
		BloodLossBar = new Resource<GameObject>("Prefabs/HUD/BloodLossBar");
		CraftingProgressIndicator = new Resource<GameObject>("Prefabs/HUD/CraftingProgressIndicator");
		CraftingProgressResource = new Resource<GameObject>("Prefabs/HUD/CraftingProgressResource");
		OverheadActionIcon = new Resource<GameObject>("Prefabs/HUD/OverheadActionIcon");
		QuestDestinationArrow = new Resource<GameObject>("Prefabs/HUD/QuestDestinationIcon");
		PlayerNamePrefab = new Resource<GameObject>("Prefabs/HUD/PlayerName");
		WorldSpeechBubble = new Resource<GameObject>("Prefabs/UI/WorldSpeechBubble");
		ActionWrapper = new Resource<GameObject>("Prefabs/UI/ActionWrapper");
		AvailableActionPrefab = new Resource<GameObject>("Prefabs/UI/AvailableAction");
		SkillBonus = new Resource<GameObject>("Prefabs/UI/SkillBonus");
		CropRating = new Resource<GameObject>("Prefabs/UI/CropRating");
		ActionSeparator = new Resource<GameObject>("Prefabs/UI/ActionSeparator");
		Insulation = new Resource<GameObject>("Prefabs/UI/Insulation");
		CraftAction = new Resource<GameObject>("Prefabs/UI/CraftAction");
		CraftActionLine = new Resource<GameObject>("Prefabs/UI/CraftActionLine");
		RecipeItem = new Resource<GameObject>("Prefabs/UI/RecipeItem");
		RecipeLiquidContainer = new Resource<GameObject>("Prefabs/UI/RecipeLiquidContainer");
		RecipeInfectedLiquidContainer = new Resource<GameObject>("Prefabs/UI/RecipeInfectedLiquidContainer");
		RecipeText = new Resource<GameObject>("Prefabs/UI/RecipeText");
		RecipeSpacer = new Resource<GameObject>("Prefabs/UI/RecipeSpacer");
		RecipeTextIconPair = new Resource<GameObject>("Prefabs/UI/RecipeTextIconPair");
		IngredientIcon = new Resource<GameObject>("Prefabs/UI/IngredientIcon");
		IngredientText = new Resource<GameObject>("Prefabs/UI/IngredientText");
		BuildHere = new Resource<GameObject>("Prefabs/UI/BuildHere");
		BuildingIngredient = new Resource<GameObject>("Prefabs/UI/BuildingIngredient");
		EquipmentSelect = new Resource<GameObject>("Prefabs/UI/EquipmentSelect");
		NameAndIcon = new Resource<GameObject>("Prefabs/UI/NameAndIcon");
		WeaponStats = new Resource<GameObject>("Prefabs/UI/WeaponStats");
		TastinessStat = new Resource<GameObject>("Prefabs/UI/TastinessStat");
		ButtonPrompt = new Resource<GameObject>("Prefabs/UI/ButtonPrompt");
		Snore = new Resource<GameObject>("Prefabs/UI/Snore");
		PlayerDisplayPrefab = new Resource<GameObject>("Prefabs/UI/PlayerDisplay");
		ShortcutPrefab = new Resource<GameObject>("Prefabs/UI/Shortcut");
		StarsPanelPrefab = new Resource<GameObject>("Prefabs/UI/StarsPanel");
		SkillsChangePopupPrefab = new Resource<GameObject>("Prefabs/UI/SkillsChangePopup");
		MapCursorMat = new Resource<Material>("Materials/UI/UI-MapCursor");
		MapIconMat = new Resource<Material>("Materials/UI/UI-MapIcon");
		OpinionGraphMat = new Resource<Material>("Materials/UI/UI-OpinionGraph");
		OutlineBlackMat = new Resource<Material>("Materials/UI/UI-OutlineBlack");
		OutlineRedMat = new Resource<Material>("Materials/UI/UI-OutlineRed");
		Opaque = new Resource<Material>("Materials/UI/UI-Opaque");
		HalftonePip = new Resource<Material>("Materials/UI/UI-HalftonePip");
		WeightIcon = new Resource<Texture2D>("Textures/HUD/Scales");
		GoldIcon = new Resource<Texture2D>("Textures/HUD/Gold");
		Dot = new Resource<Texture2D>("Textures/HUD/Dot");
		SmallDot = new Resource<Texture2D>("Textures/HUD/SmallDot");
		TargetBodyLocation = new Resource<Texture2D>("Textures/HUD/TargetBodyLocation");
		TargetBodyLocationCrippled = new Resource<Texture2D>("Textures/HUD/TargetBodyLocationCrippled");
		DeerIcon = new Resource<Texture2D>("Textures/HUD/Deer");
	}

	public static TileObject GetGhostBuilding()
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.Cursor.GhostBuilding;
	}

	public Hud()
	{
		Instance = this;
	}

	public void Unload()
	{
		Cursor.Unload();
		PotentialTargets.Clear();
		PotentialTargetsForStick.Clear();
		CharactersInSelectionBox.Clear();
		BuildingsInSelectionBox.Clear();
		Instance = null;
	}

	public void OnStart()
	{
		if (!Session.Instance.Editor)
		{
			PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
			if (localPlayerRecord != null)
			{
				BuildingSelectedInhabitant = (LocalControlledCharacter = localPlayerRecord.PlayerCharacter);
				localPlayerRecord.SelectedCharacters.CopyToList(SelectedCharacters);
			}
		}
		Pip.OnStart();
		Minimap.OnStart();
		MapPage.Instance.OnStart();
	}

	public void HandleInput(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		GameImpl instance2 = GameImpl.Instance;
		Session instance3 = Session.Instance;
		GameTerrain instance4 = GameTerrain.Instance;
		InfoScreen instance5 = InfoScreen.Instance;
		if (!instance5.Active)
		{
			NotificationManager.Instance.HandleInput(inputFrame);
		}
		if (LocalControlledCharacter != null && LocalControlledCharacter.Community != instance3.CommunityManager.PlayerCommunity && instance3.CommunityManager.PlayerCommunity != null && instance3.CommunityManager.PlayerCommunity.Leader != null && inputFrame != null)
		{
			SetLocalControlledCharacter(instance3.CommunityManager.PlayerCommunity.Leader, inputFrame);
		}
		Character localControlledCharacter = LocalControlledCharacter;
		bool flag = GuessIfIAmControllingLocalControlledCharacter();
		if (localControlledCharacter != null && !localControlledCharacter.ConsciousAndNotZombie)
		{
			flag = false;
		}
		bool parryPrompt = ParryPrompt;
		if (inputFrame != null)
		{
			ParryPrompt = false;
		}
		bool flag2 = false;
		bool flag3 = false;
		bool localTargettingSky = false;
		TileObject tileObject = null;
		TileObject potentialTargetObject = null;
		TileObject tileObject2 = null;
		TileObject tileObject3 = null;
		Vector3 localTargetPos = LocalTargetPos;
		Vector3 hitPos = LocalTargetPos;
		float throwAngle = 0f;
		float throwSpeed = 0f;
		float t = 0f;
		bool blocked = false;
		TargettableBodyLocation targettableBodyLocation = TargettableBodyLocation.Torso;
		TargettableBodyLocation potentialTargetBodyLocationToAimFor = TargettableBodyLocation.Torso;
		float potentialTargetSwitchAmount = 0f;
		PotentialTargets.Clear();
		PotentialTargetsForStick.Clear();
		CursorTargetObject = null;
		if (!instance5.Active && !instance2.IsDialogOpen())
		{
			if (instance3.GameCamera.FlyCam)
			{
				int num = 0;
				if (!Cursor.IsPlacingBuilding)
				{
					num |= 0x100255;
				}
				if (instance3.Editor)
				{
					num |= 0x10180;
				}
				if (InfoScreen.AllowViewInfoOnAnyone)
				{
					num |= 0x10000;
				}
				if (SpawnPoint.DebugShowSpawnPoints)
				{
					num |= 0x80;
				}
				CursorRayCastResult = instance3.GameCamera.RayCastFromPointOnScreen(instance.GetCursorPos(), num);
				CursorTargetObject = PickTargetInFlyCamMode();
				localTargetPos = CursorRayCastResult.GetHitPosition();
				if (CursorTargetObject != null && (instance3.Editor || instance3.IsDebugMenuOpen()) && instance.IsJustPressed(InputFunction.MainAction))
				{
					EditorSelectedObject = CursorTargetObject;
				}
			}
			else if (inputFrame != null)
			{
				if ((localControlledCharacter?.InTerrain ?? false) && flag)
				{
					flag3 = (flag2 = instance.IsPressed(InputFunction.Aim));
				}
				if (localControlledCharacter != null && localControlledCharacter.EquippedItem != null && !localControlledCharacter.EquippedItem.AllowLockOn(localControlledCharacter))
				{
					flag3 = false;
					Character predictedOrElseThisCharacter = localControlledCharacter.GetPredictedOrElseThisCharacter();
					if (flag2 && predictedOrElseThisCharacter.EquippedItem != null && !predictedOrElseThisCharacter.EquippedItem.AllowLockOn(predictedOrElseThisCharacter) && localControlledCharacter.DesiredEquippedItem == localControlledCharacter.EquippedItem && predictedOrElseThisCharacter.DesiredEquippedItem == predictedOrElseThisCharacter.EquippedItem && localControlledCharacter.CurrentActionAnim != ActionAnim.HandsUp && predictedOrElseThisCharacter.CurrentActionAnim != ActionAnim.HandsUp && Cursor.ShowEquipmentSelectTimer == 0f)
					{
						Character character = FindNearestEnemy(localControlledCharacter);
						bool flag4 = character != null && MathUtil.ToXZ(character.Pos - localControlledCharacter.Pos).sqrMagnitude >= MathUtil.Squared(8f);
						EquipmentPrototype bestAmmoType;
						InfectionType bestInfectedWith;
						Equipment bestWeapon = localControlledCharacter.Inventory.GetBestWeapon(localControlledCharacter, character, flag4, bluntOnly: false, out bestAmmoType, out bestInfectedWith);
						if (bestWeapon == null)
						{
							bestWeapon = localControlledCharacter.Inventory.GetBestWeapon(localControlledCharacter, character, !flag4, bluntOnly: false, out bestAmmoType, out bestInfectedWith);
						}
						inputFrame.AddAction(InputAction.SetDesiredWeapon(bestWeapon, bestAmmoType, bestInfectedWith));
					}
				}
				if (localControlledCharacter != null && localControlledCharacter.IsChokingSomeone())
				{
					flag3 = false;
				}
				using (new UnityProfileMarker(PickTargetInDirectControlModeStr))
				{
					tileObject = PickTargetInDirectControlMode(flag3);
				}
				targettableBodyLocation = LocalTargetBodyLocationToAimFor;
				if (flag3 && tileObject != LocalTargetObject && tileObject != null && localControlledCharacter != null)
				{
					targettableBodyLocation = TargettableBodyLocation.Torso;
				}
				if (flag3 && localControlledCharacter != null)
				{
					if (tileObject == null)
					{
						float num2 = (float)localControlledCharacter.GetSightRange() + 1f;
						if (localControlledCharacter.EquippedItem is RangedWeapon)
						{
							num2 = Math.Max(num2, localControlledCharacter.EquippedItem.GetRangeIncludingEffects(localControlledCharacter, tileObject) + 1f);
						}
						int num3 = 40;
						float num4 = 0f;
						float num5 = 0f;
						float t2 = 0f;
						Vector3 localTargetPos2 = LocalTargetPos;
						bool flag5 = false;
						RaycastResult raycastResult;
						if (localControlledCharacter.EquippedItem is Throwable)
						{
							num3 |= 0x1000;
							Vector2 horizDir = MathUtil.SafeNormalize(MathUtil.ToXZ(instance3.GameCamera.GetForward()), MathUtil.ToXZ(localControlledCharacter.Forward));
							num4 = Mathf.Min((0f - instance3.GameCamera.DirectControlPitch) * 57.29578f + ThrowAngleExtraPitch, 80f) * (MathF.PI / 180f);
							num5 = BaseThrownProjectile.CalcThrowSpeed(localControlledCharacter, null);
							raycastResult = BaseThrownProjectile.ParabolicRayCast(localControlledCharacter, null, localControlledCharacter.ThrowPosition, horizDir, num4, num5, num3, predicted: false, debugDraw: false, out t2);
							localTargetPos2 = raycastResult.GetHitPosition();
							flag5 = raycastResult.HitObject != null;
						}
						else
						{
							raycastResult = instance4.RayCast(new Ray(instance3.GameCamera.GetPos(), GetGameCameraForwardDirWithLockedOnExtraPitch()), num2, num3, localControlledCharacter.InsideBuilding, localControlledCharacter, null, predicted: false);
							localTargetPos2 = raycastResult.GetHitPosition();
						}
						localTargetPos = raycastResult.GetHitPosition();
						throwAngle = num4;
						throwSpeed = num5;
						t = t2;
						hitPos = localTargetPos2;
						blocked = flag5;
						localTargettingSky = raycastResult.HitObject == null;
					}
					else if (tileObject != null)
					{
						localTargetPos = tileObject.Pos;
					}
				}
			}
		}
		bool flag6 = false;
		if (!instance5.Active && !instance2.IsDialogOpen() && localControlledCharacter != null && (localControlledCharacter.DirectControlled || localControlledCharacter.CurrentActionAnim == ActionAnim.HandsUp) && flag && flag3 && tileObject != null)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			TargettableBodyLocation targettableBodyLocation2 = targettableBodyLocation;
			Vector2 vector = new Vector2(instance.GetMouseAxis(InputFunction.SwitchTarget), instance.GetMouseAxis(InputFunction.SwitchTargetBodyLocation));
			if (instance.IsMappedToMouseWheel(InputFunction.SwitchTarget))
			{
				zero.x += vector.x;
				zero2.x += vector.x;
				vector.x = 0f;
			}
			if (instance.IsMappedToMouseWheel(InputFunction.SwitchTargetBodyLocation))
			{
				zero.y += vector.y;
				zero2.y += vector.y;
				vector.y = 0f;
			}
			MouseTargetHistory[MouseTargetHistoryIndex % MouseTargetHistory.Length] = vector;
			MouseTargetHistoryIndex++;
			vector = Vector2.zero;
			for (int num6 = MouseTargetHistoryIndex - 1; num6 >= Math.Max(0, MouseTargetHistoryIndex - MouseTargetHistory.Length); num6--)
			{
				vector += MouseTargetHistory[num6 % MouseTargetHistory.Length];
				if (Mathf.Abs(vector.x) >= MathUtil.Squared(instance2.Settings.MouseTargetSensitivity) || Mathf.Abs(vector.y) >= MathUtil.Squared(instance2.Settings.MouseBodyLocationSensitivity))
				{
					break;
				}
			}
			zero += vector;
			zero2 += Vector2.Min(Vector2.one, vector / new Vector2(instance2.Settings.MouseTargetSensitivity, instance2.Settings.MouseBodyLocationSensitivity));
			Vector2 vector2 = new Vector2(instance.GetGamepadAxis(InputFunction.SwitchTarget), instance.GetGamepadAxis(InputFunction.SwitchTargetBodyLocation));
			zero += vector2;
			zero2 += Vector2.Min(Vector2.one, vector2 / GamepadTargetSwitchThreshold);
			Vector2 vector3 = new Vector2(instance.GetButtonAxis(InputFunction.SwitchTarget), instance.GetButtonAxis(InputFunction.SwitchTargetBodyLocation));
			zero += vector3;
			zero2 += vector3;
			zero = MathUtil.SafeNormalize(zero, Vector2.zero);
			if (tileObject is Character && (localControlledCharacter.CanTargetBodyLocation(TargettableBodyLocation.Legs, tileObject) || localControlledCharacter.CanTargetBodyLocation(TargettableBodyLocation.Head, tileObject)))
			{
				flag6 = true;
				TargettableBodyLocation targettableBodyLocation3 = targettableBodyLocation;
				if (BodyLocationCountdown <= 0f)
				{
					switch (targettableBodyLocation)
					{
					case TargettableBodyLocation.Head:
						if (zero.y <= 0f - Cos45)
						{
							targettableBodyLocation3 = TargettableBodyLocation.Torso;
						}
						break;
					case TargettableBodyLocation.Torso:
						if (zero.y >= Cos45)
						{
							targettableBodyLocation3 = TargettableBodyLocation.Head;
						}
						else if (zero.y <= 0f - Cos45)
						{
							targettableBodyLocation3 = TargettableBodyLocation.Legs;
						}
						break;
					case TargettableBodyLocation.Legs:
						if (zero.y >= Cos45)
						{
							targettableBodyLocation3 = TargettableBodyLocation.Torso;
						}
						break;
					}
				}
				if (localControlledCharacter.CanTargetBodyLocation(targettableBodyLocation3, tileObject))
				{
					if (Mathf.Abs(zero2.y) >= 1f)
					{
						targettableBodyLocation = targettableBodyLocation3;
					}
					else if (targettableBodyLocation != targettableBodyLocation3)
					{
						potentialTargetBodyLocationToAimFor = targettableBodyLocation3;
						potentialTargetSwitchAmount = Mathf.Abs(zero2.y);
					}
				}
				else if (!localControlledCharacter.CanTargetBodyLocation(targettableBodyLocation, tileObject))
				{
					targettableBodyLocation = TargettableBodyLocation.Torso;
				}
				if (targettableBodyLocation != targettableBodyLocation2)
				{
					SoundManager.PlayMenuSound(SoundManager.MoveSelectSound);
					BodyLocationCountdown = BodyLocationCountdownTime;
				}
			}
			int pixelWidth = (int)HudBehaviour.Instance.Size.x;
			int pixelHeight = (int)HudBehaviour.Instance.Size.y;
			Matrix4x4 viewProjMatrix = instance3.GameCamera.ProjectionMatrix * instance3.GameCamera.WorldToCameraMatrix;
			float num7 = float.MaxValue;
			float num8 = float.MaxValue;
			float num9 = float.MaxValue;
			TileObject tileObject4 = null;
			TileObject tileObject5 = null;
			TileObject tileObject6 = null;
			Vector3 vector4 = MathUtil.WorldToScreenPoint(tileObject.Pos, viewProjMatrix, pixelWidth, pixelHeight);
			foreach (TileObject item4 in PotentialTargetsForStick)
			{
				if (item4 != tileObject)
				{
					float num10 = 0f;
					if (!localControlledCharacter.IsEnemy(item4))
					{
						num10 += 1000f;
					}
					Vector3 vector5 = MathUtil.WorldToScreenPoint(item4.Pos, viewProjMatrix, pixelWidth, pixelHeight);
					float num11 = vector4.x - vector5.x;
					float num12 = vector5.x - vector4.x;
					if (num11 > 0f && num11 + num10 < num8)
					{
						tileObject5 = item4;
						num8 = num11 + num10;
					}
					if (num12 > 0f && num12 + num10 < num9)
					{
						tileObject6 = item4;
						num9 = num12 + num10;
					}
					float num13 = ((zero.x > Cos45) ? num12 : ((zero.x < 0f - Cos45) ? num11 : 0f));
					if (num13 > 0f && num13 + num10 < num7)
					{
						tileObject4 = item4;
						num7 = num13 + num10;
					}
				}
			}
			if (TargetSwitchCountdown <= 0f)
			{
				if (tileObject4 != null)
				{
					if (Mathf.Abs(zero2.x) >= 1f)
					{
						tileObject = tileObject4;
						targettableBodyLocation = (localControlledCharacter.CanTargetBodyLocation(targettableBodyLocation, tileObject) ? targettableBodyLocation : TargettableBodyLocation.Torso);
						TargetSwitchCountdown = TargetSwitchCountdownTime;
						HintManager.Instance.Hints[4].MarkPerformed();
					}
					else
					{
						potentialTargetObject = tileObject4;
						potentialTargetSwitchAmount = Mathf.Abs(zero2.x);
					}
				}
				else if (localControlledCharacter.EquippedItem is Throwable && Mathf.Abs(zero2.x) >= 1f)
				{
					tileObject = null;
					TargetSwitchCountdown = TargetSwitchCountdownTime;
				}
			}
			tileObject2 = tileObject5;
			tileObject3 = tileObject6;
		}
		else
		{
			MouseTargetHistoryIndex = 0;
		}
		if (!flag6)
		{
			targettableBodyLocation = TargettableBodyLocation.Torso;
		}
		BodyLocationCountdown = Math.Max(0f, BodyLocationCountdown - GameImpl.UnscaledDeltaTime);
		TargetSwitchCountdown = Math.Max(0f, TargetSwitchCountdown - GameImpl.UnscaledDeltaTime);
		if (flag3 && tileObject != LocalTargetObject)
		{
			instance3.GameCamera.OnLockedTargetChanged();
		}
		if (tileObject != null && flag3 && localControlledCharacter != null && localControlledCharacter.EquippedItem is Throwable throwable)
		{
			Vector3 throwPosition = localControlledCharacter.ThrowPosition;
			InjuryLocation injuryLocation;
			Vector3 targetAimPos = localControlledCharacter.GetTargetAimPos(tileObject, tileObject.GetBoundingBoxBottom(), visible: true, targettableBodyLocation, deterministic: true, out injuryLocation);
			BaseThrownProjectile.PickUnblockedAngleAndSpeed(localControlledCharacter, tileObject, throwPosition, targetAimPos, throwable.GetDamageRadius(), predicted: true, ai: false, out throwAngle, out throwSpeed, out t, out blocked, out hitPos);
		}
		LocalPressingAim = flag2;
		LocalWantLockOnTarget = flag3;
		LocalTargetObject = tileObject;
		LocalTargetBodyLocationToAimFor = targettableBodyLocation;
		LocalTargetPos = localTargetPos;
		LocalThrowAngle = throwAngle;
		LocalThrowSpeed = throwSpeed;
		LocalThrowTime = t;
		LocalThrowHitPos = hitPos;
		LocalThrowBlocked = blocked;
		LocalTargettingSky = localTargettingSky;
		PotentialTargetBodyLocationToAimFor = potentialTargetBodyLocationToAimFor;
		PotentialTargetObject = potentialTargetObject;
		PotentialTargetsOnEachSide[0] = tileObject2;
		PotentialTargetsOnEachSide[1] = tileObject3;
		PotentialTargetSwitchAmount = potentialTargetSwitchAmount;
		Cursor.HandleInput(inputFrame);
		if (!instance5.Active && !instance2.IsDialogOpen() && !NotificationManager.Instance.IsDisplayingNotification())
		{
			if (GetLocalTargetObject() is Character character2 && localControlledCharacter != null && (((!character2.InCombat || character2.SparringPartner != null) && character2.AliveAndNotZombie && character2.GetBaseObjectType() == BaseObjectType.Human && !LocalWantLockOnTarget) || InfoScreen.GetAllowViewInfoOnAnyone()))
			{
				HintManager.Instance.ShowUseBrainScanHint(character2);
				bool flag7 = character2.BrainScanned || (!instance3.GameCamera.FlyCam && localControlledCharacter.Inventory.FindItemOfClass(typeof(BrainScanner)) != null);
				if (BrainScanHoldTime == 0f)
				{
					bool flag8 = !instance.IsKeyPressed(InputFunction.BrainScan, capture: false);
					if (instance.IsJustPressed(InputFunction.BrainScan, !flag8))
					{
						if (flag8)
						{
							instance.Capture(InputFunction.BrainScan, untilReleased: false);
							BrainScanHoldTime += GameImpl.UnscaledDeltaTime;
						}
						else
						{
							BrainScanHoldTime = HoldTimeout;
						}
					}
				}
				else if (instance.IsPressed(InputFunction.BrainScan))
				{
					BrainScanHoldTime += GameImpl.UnscaledDeltaTime;
				}
				else
				{
					BrainScanHoldTime = 0f;
				}
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.BrainScan, flag7 ? BrainScanPage.INFOPAGE_BrainScan : BrainScanPage.INFOPAGE_CharacterNotes, BrainScanHoldTime);
				if (BrainScanHoldTime >= HoldTimeout)
				{
					BrainScanHoldTime = 0f;
					if (flag7 && !character2.BrainScanned)
					{
						inputFrame?.AddAction(InputAction.BrainScan(character2));
					}
					OpenInfoScreenFor = character2;
					WantInfoScreenType = ((!flag7) ? OpenInfoScreenType.Notes : OpenInfoScreenType.BrainScan);
				}
			}
			else
			{
				BrainScanHoldTime = 0f;
			}
		}
		else
		{
			BrainScanHoldTime = 0f;
		}
		instance5.HandleInput(inputFrame);
		if (!instance5.Active && NotificationManager.Instance.IsDisplayingNotification())
		{
			return;
		}
		if (instance3.Editor && Cursor.GetCursorAction() != CursorAction.None && (instance.IsJustPressed(InputFunction.MainAction) || (InfoScreen.Instance.ActiveAndFullyTransitionedIn && instance.IsJustPressed(InputFunction.MenuSelect))) && Cursor.IsCursorActionEnabled())
		{
			OnSelectedActionInEditor(Cursor.GetAvailableAction());
		}
		if (inputFrame == null)
		{
			return;
		}
		bool transferAll = false;
		CursorAction cursorAction = Cursor.GetCursorAction();
		if ((uint)(cursorAction - 226) <= 4u)
		{
			transferAll = instance.IsPressed(InputFunction.TransferAll, capture: true, ButtonPromptBarBehaviour.PROMPT_TransferAll);
		}
		bool captureAltAction = false;
		if (HoldTime != 0f && Cursor.ActionRequiresHold())
		{
			if (instance.IsPressed(InputFunction.MainAction, capture: false))
			{
				AvailableAction availableAction = Cursor.GetAvailableAction();
				HoldTime += GameImpl.UnscaledDeltaTime;
				inputFrame.AddAction(new InputAction(InputActionType.SetDirectControlled));
				if (HoldTime >= HoldTimeout)
				{
					instance.Capture(InputFunction.MainAction, untilReleased: true);
					HoldTime = 0f;
					Dragging = false;
					ReallyDragging = false;
					if (Cursor.IsCursorActionEnabled())
					{
						OnSelectedAction(inputFrame, availableAction, isDoubleClick: false, transferAll, ref captureAltAction);
					}
					else
					{
						SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
						HudBehaviour.Instance.SetStatusBarMsg(availableAction);
					}
				}
				else if (!Dragging)
				{
					instance.Capture(InputFunction.MainAction, untilReleased: false);
				}
			}
			else
			{
				HoldTime = 0f;
			}
		}
		else
		{
			HoldTime = 0f;
		}
		bool flag9 = false;
		if (instance3.GameCamera.FlyCam && !InfoScreen.Instance.Active && !instance2.IsDialogOpen() && !DraggingZone)
		{
			if (!Dragging)
			{
				CursorAction cursorAction2 = Cursor.GetCursorAction();
				if (cursorAction2 != CursorAction.SetZone && cursorAction2 != CursorAction.SetCropsPatchHere && cursorAction2 != CursorAction.BuildHere && instance.IsJustPressed(InputFunction.MainAction, capture: false))
				{
					instance.Capture(InputFunction.MainAction, untilReleased: false);
					Dragging = true;
					ReallyDragging = false;
					DragStartCursorPos = instance.GetCursorPos();
					DragStartWorldPos = CursorRayCastResult.GetHitPosition();
				}
			}
			else
			{
				if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor)
				{
					ReallyDragging |= (instance.GetCursorPos() - DragStartCursorPos).sqrMagnitude >= MathUtil.Squared(1f);
					Vector2 cursorPos = instance.GetCursorPos();
					Ray rayFromPointOnScreen = instance3.GameCamera.GetRayFromPointOnScreen(cursorPos);
					Plane plane = new Plane(Vector3.up, DragStartWorldPos);
					if (ReallyDragging && plane.Raycast(rayFromPointOnScreen, out var enter))
					{
						Vector3 point = rayFromPointOnScreen.GetPoint(enter);
						instance3.GameCamera.SetFocusInFlyMode(instance3.GameCamera.Focus + (DragStartWorldPos - point));
					}
				}
				if (instance.IsPressed(InputFunction.MainAction))
				{
					if (!ReallyDragging && Cursor.ActionRequiresHold())
					{
						instance.Capture(InputFunction.MainAction, untilReleased: false);
						HoldTime += GameImpl.UnscaledDeltaTime;
					}
					else
					{
						HoldTime = 0f;
					}
				}
				else
				{
					flag9 = !ReallyDragging;
					ReallyDragging = false;
					Dragging = false;
					HoldTime = 0f;
				}
			}
		}
		if (DraggingZone)
		{
			if (instance.IsJustPressed(InputFunction.Clear, capture: true, HUD_Cancel))
			{
				DraggingZone = false;
			}
			else if (instance5.Active)
			{
				MapPage mapPage = instance5.GetCurrentPage() as MapPage;
				if (mapPage != null)
				{
					DragZoneFinishTile = instance4.ClampTileWithinBounds(instance4.GetTileCoordForPosXZ(mapPage.CursorPosWorldXZ));
				}
			}
			else if (instance.IsJustPressed(InputFunction.Back))
			{
				DraggingZone = false;
			}
			else if (instance3.GameCamera.FlyCam)
			{
				DragZoneFinishTile = instance4.ClampTileWithinBounds(CursorRayCastResult.Tile);
			}
			else
			{
				DraggingZone = false;
			}
		}
		bool capture = !Cursor.ActionRequiresHold();
		if (parryPrompt && instance.IsMappedToSameInput(InputFunction.MainAction, InputFunction.Parry))
		{
			capture = false;
		}
		if (Cursor.EquipmentSelectWasRecentlyHiddenTimer > 0f && instance.IsJustPressed(InputFunction.MainAction))
		{
			Cursor.EquipmentSelectWasRecentlyHiddenTimer = 0f;
		}
		else if (Cursor.GetCursorAction() != CursorAction.None && (flag9 || instance.IsJustPressed(InputFunction.MainAction, capture) || (InfoScreen.Instance.ActiveAndFullyTransitionedIn && instance.IsJustPressed(InputFunction.MenuSelect))))
		{
			AvailableAction availableAction2 = Cursor.GetAvailableAction();
			if (Cursor.ActionRequiresHold())
			{
				instance.Capture(InputFunction.MainAction, untilReleased: false);
				HoldTime += GameImpl.UnscaledDeltaTime;
			}
			else if (Cursor.IsCursorActionEnabled())
			{
				TimeSpan time = GameImpl.Instance.GetTime();
				bool flag10 = time - LastClickTime < TimeSpan.FromSeconds(0.5);
				LastClickTime = time;
				SoundManager.PlayMenuSound(flag10 ? SoundManager.InGameDoubleClickSound : SoundManager.InGameClickSound);
				OnSelectedAction(inputFrame, availableAction2, flag10, transferAll, ref captureAltAction);
			}
			else
			{
				OnSelectedDisabledAction(availableAction2);
			}
		}
		if (!captureAltAction)
		{
			if (instance5.Active)
			{
				AvailableAction availableAction3 = Cursor.GetAvailableAction();
				switch (availableAction3.ActionType)
				{
				case CursorAction.Take:
				case CursorAction.Give:
				case CursorAction.Store:
					if (!instance.IsJustPressed(InputFunction.AltAction))
					{
						break;
					}
					if (availableAction3.Enabled == CursorActionDisabledReason.Enabled)
					{
						availableAction3.GetTransferVars(out var carrier2, out var to2, out var item2, out var trading2, out var minTransferrable2, out var _);
						if (minTransferrable2 <= 0)
						{
							break;
						}
						SoundManager.PlayMenuSound(SoundManager.SelectSound);
						if (trading2)
						{
							inputFrame.AddAction(InputAction.EquipmentTrade(item2, carrier2, to2, minTransferrable2));
							break;
						}
						TakePage takePage = InfoScreen.Instance.GetCurrentPage() as TakePage;
						if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
						{
							inputFrame.AddAction(InputAction.EquipmentPickpocket(item2, carrier2, to2, minTransferrable2, takePage.PickpocketItemDetection));
						}
						else
						{
							inputFrame.AddAction(InputAction.EquipmentTransfer(item2, carrier2, to2, minTransferrable2));
						}
					}
					else
					{
						OnSelectedDisabledAction(availableAction3);
					}
					break;
				case CursorAction.Sell:
					if (instance.IsJustPressed(InputFunction.AltAction))
					{
						TradePage tradePage2 = InfoScreen.Instance.GetCurrentPage() as TradePage;
						if (tradePage2 != null && availableAction3.Enabled == CursorActionDisabledReason.Enabled)
						{
							availableAction3.GetTransferVars(out var _, out var _, out var item3, out var _, out var minTransferrable3, out var _);
							SoundManager.PlayMenuSound(SoundManager.SelectSound);
							tradePage2.AddPendingTrade(item3, sell: true, minTransferrable3);
						}
					}
					break;
				case CursorAction.Buy:
					if (instance.IsJustPressed(InputFunction.AltAction))
					{
						TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
						if (tradePage != null && availableAction3.Enabled == CursorActionDisabledReason.Enabled)
						{
							availableAction3.GetTransferVars(out var _, out var _, out var item, out var _, out var minTransferrable, out var _);
							SoundManager.PlayMenuSound(SoundManager.SelectSound);
							tradePage.AddPendingTrade(item, sell: false, minTransferrable);
						}
					}
					break;
				}
			}
			else if (DragSelecting)
			{
				bool addToSelection = instance.IsPressed(InputFunction.AddToSelection, capture: false, MapPage.INPUT_AddToSelection);
				DragSelectFinishXZ = MathUtil.ToXZ(CursorRayCastResult.GetHitPosition());
				ReallyDragSelecting |= (DragSelectFinishXZ - DragSelectStartXZ).magnitude >= MinDragDist;
				if (ReallyDragSelecting)
				{
					DragSelectCharactersInBox(DragSelectStartXZ, DragSelectFinishXZ, addToSelection, inputFrame);
				}
				if (instance3.GameCamera.FlyCam && instance.IsPressed(InputFunction.AltAction))
				{
					DragSelectHoldTime += GameImpl.UnscaledDeltaTime;
				}
				else
				{
					if (DragSelectHoldTime < MapPage.QuickClickTime && !ReallyDragSelecting)
					{
						DeselectAllCharacters(inputFrame);
					}
					DragSelecting = false;
					ReallyDragSelecting = false;
					DragSelectHoldTime = 0f;
				}
			}
			else if (!DraggingZone && instance3.GameCamera.FlyCam && !Cursor.IsPlacingBuilding && Cursor.IsSettingCropPatch == null)
			{
				int buttonPromptHash = ((SelectedCharacters.Count > 0) ? ButtonPromptBarBehaviour.PROMPT_DeselectAll : 0);
				if (instance.IsJustPressed(InputFunction.AltAction, capture: false, buttonPromptHash))
				{
					instance.Capture(InputFunction.AltAction, untilReleased: false);
					DragSelecting = true;
					ReallyDragSelecting = false;
					DragSelectHoldTime = 0f;
					DragSelectFinishXZ = (DragSelectStartXZ = MathUtil.ToXZ(CursorRayCastResult.GetHitPosition()));
				}
			}
		}
		CharactersInSelectionBox.Clear();
		for (int num14 = SelectedCharacters.Count - 1; num14 >= 0; num14--)
		{
			Character character3 = SelectedCharacters[num14];
			if (!character3.IsSelectable())
			{
				DeselectCharacter(character3, inputFrame);
			}
		}
		if (SelectedCharacters.Count > 0 && !instance3.GameCamera.FlyCam && !instance5.Active)
		{
			Instance.DeselectAllCharacters(inputFrame);
		}
		Character character4 = LocalTargetObject as Character;
		if (Cursor.CanSkipConversationWith != null && !parryPrompt)
		{
			if (instance.IsJustPressed(InputFunction.SkipConversation, capture: true, ButtonPromptBarBehaviour.PROMPT_Skip))
			{
				inputFrame.AddAction(InputAction.SkipConversation(Cursor.CanSkipConversationWith));
			}
		}
		else if (!LocalWantLockOnTarget && localControlledCharacter != null && character4 != null && !character4.InCombat && (!localControlledCharacter.IsEnemy(character4) || localControlledCharacter.CurrentActionAnim == ActionAnim.HandsUp || character4.CurrentActionAnim == ActionAnim.HandsUp))
		{
			instance.Capture(InputFunction.SkipConversation, untilReleased: true);
		}
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		if (SentTargetObject != LocalTargetObject)
		{
			inputFrame.AddAction(InputAction.SetTargetObject(LocalTargetObject, LocalTargetBodyLocationToAimFor));
			SentTargetObject = LocalTargetObject;
		}
		if ((SendTargetPos - LocalTargetPos).sqrMagnitude >= MathUtil.Squared(0.1f))
		{
			inputFrame.AddAction(InputAction.SetTargetPos(LocalTargetPos));
			SendTargetPos = LocalTargetPos;
		}
		if (Mathf.Abs(SendThrowAngle - LocalThrowAngle) > 0.01f || Mathf.Abs(SendThrowSpeed - LocalThrowSpeed) > 0.01f)
		{
			inputFrame.AddAction(InputAction.SetThrowAngle(LocalThrowAngle, LocalThrowSpeed));
			SendThrowAngle = LocalThrowAngle;
			SendThrowSpeed = LocalThrowSpeed;
		}
		if (SentWantLockOnTarget != LocalWantLockOnTarget)
		{
			inputFrame.AddAction(new InputAction(LocalWantLockOnTarget ? InputActionType.SetWantLockOnTarget : InputActionType.ClearWantLockOnTarget));
			SentWantLockOnTarget = LocalWantLockOnTarget;
		}
		if (SentTargetBodyLocationToAimFor != LocalTargetBodyLocationToAimFor)
		{
			inputFrame.AddAction(InputAction.SetTargetBodyLocationToAimFor(LocalTargetBodyLocationToAimFor));
			SentTargetBodyLocationToAimFor = LocalTargetBodyLocationToAimFor;
		}
		bool active = InfoScreen.Instance.Active;
		if (SentIsInInfoScreen != active)
		{
			inputFrame.AddAction(InputAction.SetSyncedInInfoScreen(active));
			SentIsInInfoScreen = active;
		}
		Cursor.PostHandleInput(inputFrame);
	}

	public void Update()
	{
		using (new UnityProfileMarker(HudStr))
		{
			if (WantCursorAnimHash != 0)
			{
				GameImpl.Instance.UnityCursorAnimator.SetTrigger(WantCursorAnimHash);
				WantCursorAnimHash = 0;
			}
			if (OpenInfoScreenFor != null)
			{
				switch (WantInfoScreenType)
				{
				case OpenInfoScreenType.Notes:
				case OpenInfoScreenType.BrainScan:
					InfoScreen.Instance.ActivateBrainScan((Character)OpenInfoScreenFor, WantInfoScreenType == OpenInfoScreenType.BrainScan, null);
					break;
				case OpenInfoScreenType.Gather:
					InfoScreen.Instance.ActivateGather((Prop)OpenInfoScreenFor, LocalControlledCharacter, SwappingSuppliesMode.Gathering);
					break;
				case OpenInfoScreenType.ExitBrainScanToMap:
					InfoScreen.Instance.OnDeactivate();
					InfoScreen.Instance.Activate(OpenInfoScreenFor, typeof(MapPage));
					break;
				case OpenInfoScreenType.QuestPage:
					InfoScreen.Instance.Activate(OpenInfoScreenFor, typeof(QuestPage));
					break;
				case OpenInfoScreenType.CommunityPage:
					InfoScreen.Instance.Activate(OpenInfoScreenFor, typeof(CommunityPage));
					break;
				default:
					if (InfoScreen.Instance.CurrentObject != OpenInfoScreenFor || InfoScreen.Instance.GetCurrentPage() as CharacterPage == null)
					{
						if (InfoScreen.Instance.Active)
						{
							InfoScreen.Instance.OnDeactivate();
						}
						InfoScreen.Instance.Activate(OpenInfoScreenFor, null);
					}
					break;
				}
				OpenInfoScreenFor = null;
			}
			Cursor.Update();
			Pip.Update();
			Minimap.Update();
			InfoScreen.Instance.InfoScreenUpdate();
			UpdateDamageVignette();
			OutlineBehaviour.ActiveOutlineCameras.Clear();
			TileObject tileObject = GetLocalTargetObject();
			if (tileObject == null && Cursor.GetCursorAction() == CursorAction.ExitBuilding)
			{
				AvailableAction availableAction = Cursor.GetAvailableAction();
				Building building = availableAction.Target as Building;
				TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(building.GetEntrancePos(availableAction.Amount));
				Building building2 = GameTerrain.Instance.GetBuilding(tileCoordForPos.x, tileCoordForPos.y);
				if (building2 != null)
				{
					tileObject = building2;
				}
			}
			FocusedCharacterArrowTimeout = Math.Max(FocusedCharacterArrowTimeout - Time.unscaledDeltaTime, 0f);
			PlayerNameTimeout = Math.Max(PlayerNameTimeout - Time.unscaledDeltaTime, 0f);
			OutlineCameraBehaviour unityFocusedOutlineBehaviour = HudBehaviour.Instance.UnityFocusedOutlineBehaviour;
			unityFocusedOutlineBehaviour.ClearObjectToOutline();
			if ((FocusedCharacterArrowTimeout > 0f && LocalControlledCharacterOrBuildingTheyAreIn != null) || (Session.Instance.GameCamera.FlyCam && SelectedCharacters.Contains(LocalControlledCharacter)) || tileObject == LocalControlledCharacterOrBuildingTheyAreIn)
			{
				unityFocusedOutlineBehaviour.SetObjectToOutline(LocalControlledCharacterOrBuildingTheyAreIn, Color.white);
			}
			OutlineCameraBehaviour unityTargetOutlineBehaviour = HudBehaviour.Instance.UnityTargetOutlineBehaviour;
			unityTargetOutlineBehaviour.ClearObjectToOutline();
			if (tileObject != null && tileObject != LocalControlledCharacterOrBuildingTheyAreIn)
			{
				unityTargetOutlineBehaviour.SetObjectToOutline(tileObject, tileObject.MapColor);
			}
			OutlineCameraBehaviour unitySelectedOutlineBehaviour = HudBehaviour.Instance.UnitySelectedOutlineBehaviour;
			unitySelectedOutlineBehaviour.ClearObjectToOutline();
			if (Session.Instance.GameCamera.FlyCam)
			{
				foreach (Character selectedCharacter in SelectedCharacters)
				{
					if (selectedCharacter != LocalControlledCharacter && selectedCharacter != tileObject)
					{
						unitySelectedOutlineBehaviour.SetObjectToOutline(selectedCharacter.IsOutdoors() ? ((MultiTileObject)selectedCharacter) : ((MultiTileObject)selectedCharacter.InsideBuilding), selectedCharacter.MapColor);
					}
				}
			}
			Transform transform = HudBehaviour.Instance.UnityGameCamera.transform;
			GameObject unityParryPrompt = HudBehaviour.Instance.UnityParryPrompt;
			bool flag = (ParryPrompt || ParryPromptSuccess || BigHintPrompt != InputFunction.Invalid) && ParryPromptAttacker != null && !ParryPromptAttacker.Deleted && InfoScreen.Instance.Transition == 0f;
			if (flag)
			{
				LastParryPromptFrame = Time.frameCount;
				GameObject unityParryPromptTextDisplay = HudBehaviour.Instance.UnityParryPromptTextDisplay;
				GameObject unityParryPromptSuccessDisplay = HudBehaviour.Instance.UnityParryPromptSuccessDisplay;
				TextMeshProUGUI unityParryPromptText = HudBehaviour.Instance.UnityParryPromptText;
				TextMeshProUGUI unityParryPromptSuccessText = HudBehaviour.Instance.UnityParryPromptSuccessText;
				RawImage unityParryPromptImage = HudBehaviour.Instance.UnityParryPromptImage;
				RawImage unityParryPromptBang = HudBehaviour.Instance.UnityParryPromptBang;
				RawImage unityParryPromptForbidden = HudBehaviour.Instance.UnityParryPromptForbidden;
				unityParryPromptImage.gameObject.SetActive(ParryPrompt || (ParryPromptSuccess && ParryPromptTargetEscapePower >= 0f));
				unityParryPromptTextDisplay.SetActive(!ParryPromptSuccess);
				unityParryPromptSuccessDisplay.SetActive(!ParryPrompt);
				unityParryPromptForbidden.gameObject.SetActive(ParryPrompt && !ParryPromptEnabled);
				Character predictedOrElseThisCharacter = ParryPromptAttacker.GetPredictedOrElseThisCharacter();
				Vector2 vector = Vector2.zero;
				float y = predictedOrElseThisCharacter.Height + 0.75f;
				float a = 1f;
				if (ParryPromptSuccess)
				{
					float num = Mathf.Sqrt(ParryPromptSuccessTransition);
					a = Mathf.Lerp(0f, 1f, (1f - num) * 10f);
					string str = GameImpl.Translate(ParryPromptHash);
					unityParryPromptSuccessText.SetUnityTextIfDifferent(str);
					unityParryPromptSuccessDisplay.transform.localPosition = new Vector3(0f, -3.5f, 0f);
					unityParryPromptSuccessDisplay.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, num);
					unityParryPromptSuccessText.color = new Color(1f, 1f, 1f, a);
					unityParryPromptBang.color = new Color(unityParryPromptBang.color.r, unityParryPromptBang.color.g, unityParryPromptBang.color.b, a);
					ParryPromptSuccessTransition += Time.deltaTime / 0.25f;
					if (ParryPromptSuccessTransition > 1f)
					{
						ParryPromptSuccess = false;
					}
					ParryPromptCurrentSize = 1f;
				}
				else if (ParryPrompt)
				{
					float num2 = 1f;
					float num3 = ParryPromptCurrentSize - ParryPromptPrevSize;
					ParryPromptPrevSize = ParryPromptCurrentSize;
					ParryPromptCurrentSize += num3 * 0.85f;
					ParryPromptCurrentSize += (num2 - ParryPromptCurrentSize) * 0.15f;
					unityParryPromptTextDisplay.transform.localScale = Vector3.one * ParryPromptCurrentSize;
					StringUtil.SetUnityTextButtonPrompt(unityParryPromptText, InputFunction.Parry);
				}
				else if (BigHintPrompt != InputFunction.Invalid)
				{
					y = predictedOrElseThisCharacter.Height * 0.75f;
					vector = new Vector2(256f, 0f);
					unityParryPromptTextDisplay.transform.localScale = Vector3.one * (1f + Mathf.Abs(Mathf.Cos(GameImpl.UnscaledTime * 10f)) * 0.1f);
					StringUtil.SetUnityTextButtonPrompt(unityParryPromptText, BigHintPrompt);
					unityParryPromptSuccessDisplay.transform.localPosition = new Vector3(0f, -64f, 0f);
					unityParryPromptSuccessDisplay.transform.localScale = Vector3.one * 0.75f;
					unityParryPromptSuccessText.color = Color.white;
					unityParryPromptSuccessText.SetUnityTextIfDifferent(GameImpl.Translate(BigHintPromptHash));
					unityParryPromptBang.color = new Color(unityParryPromptBang.color.r, unityParryPromptBang.color.g, unityParryPromptBang.color.b, 0f);
					BigHintPrompt = InputFunction.Invalid;
				}
				if (ParryPromptTargetEscapePower >= 0f)
				{
					ParryPromptCurrentEscapePower = ParryPromptTargetEscapePower;
				}
				unityParryPromptImage.material.SetFloat(ShaderHash._Radius, ParryPromptTimeoutRadius);
				unityParryPromptImage.material.SetFloat(ShaderHash._FillAmount, (ParryPromptTargetEscapePower >= 0f) ? ParryPromptCurrentEscapePower : 0f);
				unityParryPromptImage.material.SetFloat(ShaderHash._FillAlpha, (ParryPromptTargetEscapePower >= 0f) ? 0.5f : 0f);
				Color color = (ParryPromptEnabled ? Color.red : Color.gray);
				color.a = a;
				unityParryPromptImage.color = color;
				Vector3 worldPos = predictedOrElseThisCharacter.GetUnityPos() + new Vector3(0f, y, 0f);
				Vector2 vector2 = MathUtil.ToXY(Session.Instance.GameCamera.GetScreenPosFromPointInWorld(worldPos)) + vector;
				Vector2 sizeDelta = HudBehaviour.Instance.MainPanelRectTransform.sizeDelta;
				float num4 = 256f;
				((RectTransform)unityParryPrompt.transform).anchoredPosition = new Vector2(Mathf.Clamp(vector2.x, num4, sizeDelta.x - num4), Mathf.Clamp(vector2.y, num4, sizeDelta.y - num4));
			}
			else
			{
				ParryPromptAttacker = null;
				ParryPromptSuccess = false;
				ParryPrompt = false;
				ParryPromptCurrentEscapePower = 0f;
				ParryPromptCurrentSize = 1f;
				ParryPromptPrevSize = 1f;
				BigHintPrompt = InputFunction.Invalid;
			}
			unityParryPrompt.SetActive(flag);
			SetHudSoundVolume(GameImpl.Instance.UnityParryPromptSound, ParryPrompt ? Mathf.Clamp01(1f - ParryPromptTimeoutRadius) : 0f);
			GameObject unityReticule = HudBehaviour.Instance.UnityReticule;
			Animator unityReticuleAnimator = HudBehaviour.Instance.UnityReticuleAnimator;
			TileObject tileObject2 = ((LocalTargetObject != null) ? LocalTargetObject.GetPredictedOrElseThis() : null);
			Character character = tileObject2 as Character;
			Character character2 = ((LocalControlledCharacter != null) ? LocalControlledCharacter.GetPredictedOrElseThisCharacter() : null);
			RangedWeapon rangedWeapon = ((character2 != null) ? (character2.EquippedItem as RangedWeapon) : null);
			bool flag2 = LocalWantLockOnTarget && tileObject2 != null && tileObject2.IsUnityObjectActive() && character2 != null && (character2.EquippedItem == null || character2.EquippedItem is AmmoWeapon || character2.EquippedItem is MeleeWeapon || character2.EquippedItem is Bow);
			bool flag3 = LocalWantLockOnTarget && character2 != null && character2.DirectControlled && rangedWeapon != null;
			bool num5 = character != null && character.CurrentActionAnim == ActionAnim.HandsUp;
			bool flag4 = character != null && !character.Alive;
			bool flag5 = num5 || flag4;
			unityReticule.SetActive(flag3);
			if (flag3)
			{
				float rangeIncludingEffects = rangedWeapon.GetRangeIncludingEffects(character2, tileObject2);
				bool flag6 = LocalTargettingSky || ((tileObject?.PosXZ ?? MathUtil.ToXZ(LocalTargetPos)) - character2.PosXZ).sqrMagnitude > rangeIncludingEffects * rangeIncludingEffects;
				flag5 |= flag6 || character2.IsTargetBlocked(character2.GetTarget(tileObject), LocalTargetPos, LocalThrowAngle, LocalThrowSpeed);
				if (character != null && character.IsUnityObjectActive())
				{
					Vector3 vector3 = LocalTargetBodyLocationToAimFor switch
					{
						TargettableBodyLocation.Head => character.GetUnityBoneTransform(Bone.Head).Translation(), 
						TargettableBodyLocation.Legs => Vector3.Lerp(character.GetUnityBoneTransform(Bone.LeftLeg).Translation(), character.GetUnityBoneTransform(Bone.RightLeg).Translation(), 0.5f), 
						_ => character.GetUnityBoneTransform(Bone.Spine).Translation(), 
					};
					unityReticule.transform.position = vector3 - transform.forward * character.Radius;
				}
				else if (tileObject2 != null)
				{
					unityReticule.transform.position = tileObject2.GetBoundingBoxCentre() - transform.forward * MathUtil.ToXZ(tileObject2.GetBoundingBox().extents).magnitude;
				}
				else
				{
					unityReticule.transform.position = LocalTargetPos - transform.forward * ReticuleOffsetFromTargetPos;
				}
				unityReticule.transform.rotation = transform.rotation;
				unityReticule.transform.localScale = Vector3.one * 0.5f;
				if (unityReticuleAnimator.isInitialized)
				{
					unityReticuleAnimator.SetFloat(AnimHash.Accuracy, character2.GetAccuracy());
					unityReticuleAnimator.SetBool(AnimHash.Blocked, flag5);
				}
			}
			GameObject unityTargetBodyLocation = HudBehaviour.Instance.UnityTargetBodyLocation;
			bool active = false;
			if (flag2 && character != null && (!flag || ParryPromptAttacker != LocalTargetObject) && (character2.CanTargetBodyLocation(TargettableBodyLocation.Legs, LocalTargetObject) || character2.CanTargetBodyLocation(TargettableBodyLocation.Head, LocalTargetObject)))
			{
				character.GetUnityHeadHeight();
				float num6 = character.Unity.OverheadIconsTopY + ReticuleHeight;
				active = true;
				unityTargetBodyLocation.transform.position = tileObject2.GetUnityPos() + new Vector3(0f, character.GetUnityHeadHeight(), 0f) + transform.up * num6;
				unityTargetBodyLocation.transform.rotation = transform.rotation;
				unityTargetBodyLocation.transform.localScale = Vector3.one * 0.5f;
				for (int i = 0; i < 6; i++)
				{
					TargettableBodyLocation targettableBodyLocation = TargettableBodyLocation.Torso;
					switch ((InjuryLocation)i)
					{
					case InjuryLocation.Head:
						targettableBodyLocation = TargettableBodyLocation.Head;
						break;
					case InjuryLocation.LeftLeg:
					case InjuryLocation.RightLeg:
						targettableBodyLocation = TargettableBodyLocation.Legs;
						break;
					}
					Color color2 = Color.white;
					if (LocalTargetBodyLocationToAimFor == targettableBodyLocation)
					{
						color2 = Color.red;
					}
					else if (PotentialTargetBodyLocationToAimFor == targettableBodyLocation)
					{
						color2 = Color.Lerp(color2, new Color(1f, 0.75f, 0.75f), PotentialTargetSwitchAmount);
					}
					if (flag5 || !character2.CanTargetBodyLocation(targettableBodyLocation, LocalTargetObject))
					{
						color2 *= new Color(0.25f, 0.25f, 0.25f, 1f);
					}
					RawImage component = unityTargetBodyLocation.transform.GetChild(i).gameObject.GetComponent<RawImage>();
					if (targettableBodyLocation == TargettableBodyLocation.Legs && character.GetLegDamage() >= character.GetMaxLimbDamage())
					{
						component.texture = (Texture2D)TargetBodyLocationCrippled;
					}
					else
					{
						component.texture = (Texture2D)TargetBodyLocation;
					}
					component.color = color2;
				}
				character.Unity.OverheadIconsTopY += TargetBodyLocationHeight;
			}
			unityTargetBodyLocation.SetActive(active);
			for (int j = 0; j < 2; j++)
			{
				GameObject gameObject = HudBehaviour.Instance.UnityTargetIndicator[j];
				if (PotentialTargetsOnEachSide[j] != null)
				{
					TileObject predictedOrElseThis = PotentialTargetsOnEachSide[j].GetPredictedOrElseThis();
					float num7 = 0f;
					float num8 = PotentialTargetHeight;
					if (predictedOrElseThis is Character character3)
					{
						num7 += character3.GetUnityHeadHeight();
						num8 += character3.Unity.OverheadIconsTopY;
					}
					gameObject.transform.position = predictedOrElseThis.GetUnityPos() + new Vector3(0f, num7, 0f) + transform.up * num8;
					gameObject.transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, (j == 0) ? (-90f) : 90f);
					gameObject.transform.localScale = Vector3.one * 0.5f;
					Color color3 = Color.white;
					if (PotentialTargetObject == PotentialTargetsOnEachSide[j])
					{
						color3 = Color.Lerp(color3, new Color(1f, 0.75f, 0.75f), PotentialTargetSwitchAmount);
					}
					gameObject.GetComponent<RawImage>().color = color3;
				}
				gameObject.SetActive(PotentialTargetsOnEachSide[j] != null);
			}
			GameCamera gameCamera = Session.Instance.GameCamera;
			float t = gameCamera.FlyCamTransition * (gameCamera.FlyModeZoomDist / GameCamera.ZoomedOutDistMax);
			float num9 = Mathf.Lerp(1f, Character.FocusArrowScaleFlyMode, t);
			float num10 = Character.OverheadIconsYOffset;
			float num11 = Character.OverheadIconsYOffset;
			PlantableCrop plantableCrop = tileObject as PlantableCrop;
			Campfire campfire = tileObject as Campfire;
			PitTrap pitTrap = tileObject as PitTrap;
			RabbitTrap rabbitTrap = tileObject as RabbitTrap;
			Prop prop = tileObject as Prop;
			SingleTileProp singleTileProp = tileObject as SingleTileProp;
			bool flag7 = false;
			bool flag8 = false;
			float value = 0f;
			float value2 = 0f;
			Color col = Color.white;
			Color col2 = Color.white;
			if (campfire != null)
			{
				flag7 = true;
				value = Mathf.Clamp01(campfire.WoodRemaining);
				col = new Color(0.5f, 0.25f, 0f);
			}
			else if (pitTrap != null)
			{
				flag7 = true;
				value = Mathf.Clamp01(1f - (float)pitTrap.FreeResets / (float)PitTrap.MaxFreeResets);
				col = new Color(0.5f, 0.25f, 0f);
			}
			else if (rabbitTrap != null)
			{
				flag7 = true;
				value = Mathf.Clamp01(1f - (float)rabbitTrap.FreeResets / (float)RabbitTrap.MaxFreeResets);
				col = new Color(1f, 0.5f, 0f);
			}
			else if (plantableCrop != null)
			{
				flag7 = true;
				value = ((plantableCrop.GetMoisture() >= 0f) ? plantableCrop.GetMoisture() : Mathf.Clamp01(plantableCrop.GetMoisture() / PlantableCrop.CriticalDehydrationLevel));
				col = ((!(plantableCrop.GetMoisture() >= 0f)) ? Color.red : ((LiquidPrototype.Water != null) ? ((Color)LiquidPrototype.Water.Col) : Color.blue));
				if (plantableCrop.Frost > 0f)
				{
					flag8 = true;
					value2 = plantableCrop.Frost;
					col2 = Color.blue;
				}
			}
			else if (prop != null && prop.GetDamageFraction() > 0f)
			{
				flag7 = true;
				value = prop.GetDamageFraction();
				col = Color.red;
			}
			else if (singleTileProp != null && singleTileProp.GetDamageFraction() > 0f)
			{
				flag7 = true;
				value = singleTileProp.GetDamageFraction();
				col = Color.red;
			}
			if (prop != null && prop.GetLiquidAmount() > 0f && prop.GetLiquidCapacity() > 0f && prop.GetLiquidType() != null && (prop.CanBeFilled() == CursorActionDisabledReason.Enabled || prop.CanBeEmptied()))
			{
				flag8 = true;
				value2 = prop.GetLiquidAmount() / prop.GetLiquidCapacity();
				col2 = prop.GetLiquidType().Col;
			}
			GameObject unityHealthBar = HudBehaviour.Instance.UnityHealthBar;
			unityHealthBar.SetActive(flag7 || flag8);
			if (flag7 || flag8)
			{
				num10 += Character.BloodLossBarYOffset;
				unityHealthBar.transform.position = tileObject.GetUnityPos() + new Vector3(0f, tileObject.GetUnityOverheadIconYOffset(), 0f) + transform.up * num9 * num10;
				unityHealthBar.transform.rotation = transform.rotation;
				unityHealthBar.transform.localScale = Vector3.one * Character.BloodLossBarScale * num9;
				ProgressBarBehaviour component2 = unityHealthBar.transform.GetChild(0).GetComponent<ProgressBarBehaviour>();
				ProgressBarBehaviour component3 = unityHealthBar.transform.GetChild(2).GetComponent<ProgressBarBehaviour>();
				component2.gameObject.SetActive(flag7);
				if (flag7)
				{
					component2.SetValue(value);
					component2.SetCol(col);
				}
				unityHealthBar.transform.GetChild(1).gameObject.SetActive(value: false);
				component3.gameObject.SetActive(flag8);
				if (flag8)
				{
					component3.SetTicks(0);
					component3.SetFillOutlineEnabled(enabled: true);
					component3.SetValue(value2);
					component3.SetCol(col2);
				}
				num10 += Character.BloodLossBarYOffset;
			}
			CraftingProp craftingProp = tileObject as CraftingProp;
			TileObject tileObject3 = null;
			float craftingProgress = 0f;
			Recipe recipe = null;
			Equipment usingItem = null;
			UnderConstructionInfo underConstructionInfo = null;
			Character crafter = null;
			bool flag9 = false;
			bool wantCheckEquipmentPolicy = false;
			if (tileObject != null && tileObject.GetUnderConstructionInfo() != null && tileObject.GetCommunity() == Session.Instance.CommunityManager.PlayerCommunity)
			{
				tileObject3 = tileObject;
				underConstructionInfo = tileObject.GetUnderConstructionInfo();
				craftingProgress = underConstructionInfo.GetProgress();
				recipe = underConstructionInfo.Recipe;
			}
			else if (craftingProp != null && craftingProp.IsCrafting() && craftingProp.CurrentCrafter != null && craftingProp.CurrentCrafter.GetCommunity() == Session.Instance.CommunityManager.PlayerCommunity)
			{
				tileObject3 = craftingProp;
				craftingProgress = craftingProp.GetCraftingProgress();
				recipe = craftingProp.CraftingRecipe;
				crafter = craftingProp.CurrentCrafter;
			}
			else if (character2 != null && !character2.IsOutdoors() && character2.GetCraftingProgressForDisplay(out craftingProgress, out recipe, out usingItem, out underConstructionInfo, out wantCheckEquipmentPolicy))
			{
				tileObject3 = LocalControlledCharacterOrBuildingTheyAreIn;
				crafter = LocalControlledCharacter;
				flag9 = true;
			}
			RawImage unityCraftingProgressIndicator = HudBehaviour.Instance.UnityCraftingProgressIndicator;
			unityCraftingProgressIndicator.gameObject.SetActive(tileObject3 != null);
			if (tileObject3 != null && recipe != null)
			{
				if (flag9)
				{
					num11 += Character.CraftingProgressIndicatorYOffset;
				}
				else
				{
					num10 += Character.CraftingProgressIndicatorYOffset;
				}
				TileObject predictedOrElseThis2 = tileObject3.GetPredictedOrElseThis();
				float y2 = ((predictedOrElseThis2.GetUnderConstructionInfo() != null && !predictedOrElseThis2.GetUnderConstructionInfo().ShowHalfBuiltModel()) ? HumanAppearance.MaleDefaultHeight : predictedOrElseThis2.GetUnityOverheadIconYOffset());
				if (!(predictedOrElseThis2 is Character))
				{
					predictedOrElseThis2.GetBoundingBoxBottom();
				}
				else
				{
					predictedOrElseThis2.GetUnityPos();
				}
				unityCraftingProgressIndicator.transform.position = predictedOrElseThis2.GetUnityPos() + new Vector3(0f, y2, 0f) + transform.up * num9 * (flag9 ? num11 : num10);
				unityCraftingProgressIndicator.transform.rotation = transform.rotation;
				unityCraftingProgressIndicator.transform.localScale = Vector3.one * Character.CraftingProgressIndicatorScale * num9;
				unityCraftingProgressIndicator.material.SetFloat(ShaderHash._FillAmount, craftingProgress);
				Character.PopulateCraftingProgressDisplay(unityCraftingProgressIndicator.gameObject, crafter, craftingProgress, recipe, usingItem, underConstructionInfo, craftingProp?.UsedIngredients, wantCheckEquipmentPolicy);
				if (flag9)
				{
					num11 += Character.CraftingProgressIndicatorYOffset;
				}
				else
				{
					num10 += Character.CraftingProgressIndicatorYOffset;
				}
			}
			bool flag10 = false;
			if (character2 != null && character2.InsideBuilding != null && !character2.InTerrain && character2.IsCrouching())
			{
				flag10 = true;
			}
			RawImage unityOverheadActionIcon = HudBehaviour.Instance.UnityOverheadActionIcon;
			unityOverheadActionIcon.gameObject.SetActive(flag10);
			if (flag10)
			{
				Building insideBuilding = character2.InsideBuilding;
				float value3 = LocalControlledCharacter.CalcCamouflageIconFillAmount();
				num11 += CamouflageIconYOffset;
				unityOverheadActionIcon.transform.position = insideBuilding.GetUnityPos() + new Vector3(0f, insideBuilding.GetUnityOverheadIconYOffset(), 0f) + transform.up * num9 * num11;
				unityOverheadActionIcon.transform.rotation = transform.rotation;
				unityOverheadActionIcon.transform.localScale = Vector3.one * Mathf.Lerp(CamouflageIconScale, Character.FocusArrowScale, gameCamera.FlyCamTransition) * num9;
				unityOverheadActionIcon.material.SetFloat(ShaderHash._FillAmount, value3);
				num11 += CamouflageIconYOffset;
			}
		}
	}

	public void OnSelectedActionInEditor(AvailableAction action)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		switch (action.ActionType)
		{
		case CursorAction.EditorSelect:
			SetLocalControlledCharacter(CursorTargetObject as Character, null);
			break;
		case CursorAction.Equip:
			((Equipment)action.Target).Equip(action.Actor);
			break;
		case CursorAction.Unequip:
			((Equipment)action.Target).Unequip(action.Actor);
			break;
		case CursorAction.Wear:
			((Equipment)action.Target).Wear(action.Actor);
			break;
		case CursorAction.Strip:
			((Equipment)action.Target).Strip(action.Actor);
			break;
		case CursorAction.LoadAmmo:
			instance2.EquipmentLoadAmmo((AmmoWeapon)action.Target, action.Object, action.Proto, triggeredByLocalPlayer: true);
			break;
		case CursorAction.Unload:
			instance2.EquipmentUnloadAmmo((AmmoWeapon)action.Target, action.Object, triggeredByLocalPlayer: true);
			break;
		case CursorAction.DestroyItem:
			instance2.EquipmentDestroy((Equipment)action.Target, action.Object, triggeredByLocalPlayer: true, int.MaxValue);
			break;
		case CursorAction.PourAway:
			instance2.EquipmentPourAway((Equipment)action.Target, action.Object);
			break;
		case CursorAction.StartPourInto:
			InfoScreen.Instance.StartPourInto(action.Object, (Equipment)action.Target, action.Actor);
			break;
		case CursorAction.ViewCharacter:
			EditorSelectedObject = action.Target as TileObject;
			OpenInfoScreenFor = action.Target as TileObject;
			WantInfoScreenType = OpenInfoScreenType.Inventory;
			break;
		case CursorAction.ControlCharacterInBuilding:
		case CursorAction.ViewInventoryInBuilding:
			EditorSelectedObject = action.Target as TileObject;
			BuildingSelectedInhabitant = action.Target as Character;
			break;
		case CursorAction.CraftingLimit:
			instance.ShowCraftLimitBox((Community)action.Target, action.Proto, action.Liquid, action.Amount);
			break;
		}
	}

	public void OnSelectedAction(InputFrame inputFrame, AvailableAction action, bool isDoubleClick, bool transferAll, ref bool captureAltAction)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		GameTerrain instance3 = GameTerrain.Instance;
		Character localControlledCharacter = LocalControlledCharacter;
		switch (action.ActionType)
		{
		case CursorAction.Demolish:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Demolish(action.Target as TileObject));
			break;
		case CursorAction.Abandon:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Abandon(action.Target as TileObject));
			break;
		case CursorAction.ClearCrops:
			if (action.Target is PlantableCrop plantableCrop2)
			{
				CheckMovementZone(plantableCrop2.Tile);
				inputFrame.AddAction(InputAction.Harvest(plantableCrop2.Tile, isDoubleClick: false));
			}
			break;
		case CursorAction.Steal:
		case CursorAction.StealEmpty:
		case CursorAction.StealNotInvestigated:
		case CursorAction.Pickpocket:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Take(action.Target as TileObject, isDoubleClick: false));
			break;
		case CursorAction.StealFromPot:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object, InteractionType.EatFromPot, action.Target as Equipment, isDoubleClick: false));
			break;
		case CursorAction.StealCrops:
			if (action.Target is PlantableCrop plantableCrop && localControlledCharacter.HasInventorySpaceFor(plantableCrop.GetHarvestWeight()))
			{
				CheckMovementZone(plantableCrop);
				inputFrame.AddAction(InputAction.Harvest(plantableCrop.Tile, isDoubleClick: false));
			}
			break;
		case CursorAction.PickUpSteal:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.PickUp(action.Target as TileObject, isDoubleClick: false));
			break;
		case CursorAction.GrabSteal:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Grab(action.Target as TileObject, isDoubleClick: false));
			break;
		case CursorAction.ChopTreeStealing:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChopTree(action.Object, isDoubleClick: false));
			break;
		case CursorAction.ChopLogStealing:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChopLog(action.Object, isDoubleClick: false));
			break;
		case CursorAction.MineStealing:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Mine(action.Object, action.Proto.GetMineralType(), isDoubleClick: false));
			break;
		case CursorAction.Slaughter:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.KillUnconscious(action.Object as Character, isDoubleClick: false));
			break;
		case CursorAction.LoadNewMap:
		{
			Vector2 posXZ = ((EnterableVehicle)action.Target).GetPredictedOrElseThisVehicle().PosXZ;
			GameTerrain.Instance.GetNearestHitTheRoadPoint(posXZ, GameCursor.HitTheRoadRange, out var traits);
			GameImpl.Instance.ShowHitTheRoadBox(traits, delegate(InputFrame inputFrame2)
			{
				inputFrame2?.AddAction(InputAction.LoadNewMap(action.Target));
			});
			break;
		}
		case CursorAction.EquipmentSelect:
			Cursor.HideEquipmentSelect();
			Cursor.ResetShortcutTimer();
			break;
		case CursorAction.Control:
			if (CursorTargetObject is Character)
			{
				SetLocalControlledCharacter(CursorTargetObject as Character, inputFrame);
				if (!instance2.Editor)
				{
					instance2.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
				}
			}
			break;
		case CursorAction.ControlVehicle:
			SetLocalControlledCharacter(action.Actor, inputFrame);
			instance2.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
			break;
		case CursorAction.SelectCharacterWithRoleOnProp:
			if (LocalControlledCharacter == Cursor.GetAvailableAction().Actor)
			{
				instance2.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
			}
			else
			{
				SetLocalControlledCharacter(Cursor.GetAvailableAction().Actor, inputFrame);
			}
			break;
		case CursorAction.GoTo:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.GoTo(instance3.ClampTileWithinBounds(action.Tile), isDoubleClick));
			WantCursorAnimHash = AnimHash.GoTo;
			break;
		case CursorAction.SetZone:
			if (DraggingZone)
			{
				DraggingZone = false;
				SentMovementZone = true;
				TerrainRect movementZone = new TerrainRect(DragZoneStartTile, DragZoneStartTile).Include(DragZoneFinishTile);
				inputFrame.AddAction(InputAction.SetMovementZone(movementZone));
			}
			else
			{
				DraggingZone = true;
				DragZoneStartTile = (DragZoneFinishTile = instance3.ClampTileWithinBounds(action.Tile));
			}
			break;
		case CursorAction.ClearZone:
			inputFrame.AddAction(InputAction.SetMovementZone(TerrainRect.Invalid));
			break;
		case CursorAction.ResumeZone:
			inputFrame.AddAction(new InputAction(InputActionType.ResumeMovementZone));
			break;
		case CursorAction.PauseZone:
			inputFrame.AddAction(new InputAction(InputActionType.PauseMovementZone));
			break;
		case CursorAction.CopyZone:
			inputFrame.AddAction(new InputAction(InputActionType.CopyMovementZone));
			break;
		case CursorAction.SetMarker:
			inputFrame.AddAction(InputAction.SetMapMarkerTile(action.MarkerType, action.Tile));
			break;
		case CursorAction.ClearMarker:
			inputFrame.AddAction(InputAction.ClearMapMarkerTile(action.MarkerType, action.Tile));
			break;
		case CursorAction.EnterBuilding:
		case CursorAction.EnterBuildingNotInvestigated:
			Cursor.CooldownSinceEnteredBuilding = 1f;
			if (instance2.GameCamera.FlyCam)
			{
				CheckMovementZone(CursorTargetObject);
				inputFrame.AddAction(InputAction.GoToAndEnterBuilding(CursorTargetObject as Building, isDoubleClick));
			}
			else
			{
				inputFrame.AddAction(InputAction.EnterBuilding(LocalTargetObject as Building, isDoubleClick));
			}
			break;
		case CursorAction.ExitBuilding:
			inputFrame.AddAction(InputAction.ExitBuilding(action.Amount, isDoubleClick));
			break;
		case CursorAction.ChangeBuildingSlot:
			inputFrame.AddAction(InputAction.ChangeBuildingSlot(action.Amount));
			break;
		case CursorAction.FollowMe:
			inputFrame.AddAction(InputAction.FollowMe(instance2.GameCamera.FlyCam ? CursorTargetObject : LocalTargetObject));
			break;
		case CursorAction.StopFollowingMe:
			inputFrame.AddAction(InputAction.StopFollowingMe(instance2.GameCamera.FlyCam ? CursorTargetObject : LocalTargetObject));
			break;
		case CursorAction.GroupFollowMe:
			inputFrame.AddAction(new InputAction(InputActionType.GroupFollowMe));
			break;
		case CursorAction.GroupStopFollowingMe:
			inputFrame.AddAction(new InputAction(InputActionType.GroupStopFollowingMe));
			break;
		case CursorAction.SelectFollowers:
			SelectFollowers(action.Actor, InputFunctionManager.Instance.IsPressed(InputFunction.AddToSelection, capture: false), inputFrame);
			break;
		case CursorAction.SelectEveryone:
			SelectAllCharacters(inputFrame);
			break;
		case CursorAction.SelectNoone:
			DeselectAllCharacters(inputFrame);
			break;
		case CursorAction.Attack:
			inputFrame.AddAction(InputAction.Attack(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.Equip:
		{
			Equipment equipment2 = (Equipment)action.Target;
			if (InfoScreen.Instance.Active)
			{
				inputFrame.AddAction(InputAction.EquipmentEquip(equipment2, action.Actor));
			}
			else
			{
				inputFrame.AddAction(InputAction.SetDesiredWeapon(equipment2, equipment2.GetCurrentAmmoType(), equipment2.InfectedWith));
			}
			break;
		}
		case CursorAction.Unequip:
		{
			Equipment equipment = (Equipment)action.Target;
			if (InfoScreen.Instance.Active)
			{
				inputFrame.AddAction(InputAction.EquipmentUnequip(equipment, action.Actor));
			}
			else
			{
				inputFrame.AddAction(InputAction.SetDesiredWeapon(null, null, InfectionType.None));
			}
			break;
		}
		case CursorAction.Wear:
			inputFrame.AddAction(InputAction.EquipmentWear((Equipment)action.Target, action.Actor));
			break;
		case CursorAction.Strip:
			inputFrame.AddAction(InputAction.EquipmentStrip((Equipment)action.Target, action.Actor));
			break;
		case CursorAction.Use:
			if (InfoScreen.Instance.Active)
			{
				inputFrame.AddAction(InputAction.EquipmentUse((Equipment)action.Target, action.Object, action.Actor));
			}
			else
			{
				inputFrame.AddAction(InputAction.Use((Equipment)action.Target));
			}
			break;
		case CursorAction.Eat:
			if (InfoScreen.Instance.Active)
			{
				inputFrame.AddAction(InputAction.EquipmentUse((Equipment)action.Target, action.Object, action.Actor));
			}
			else
			{
				inputFrame.AddAction(InputAction.Eat((Equipment)action.Target));
			}
			break;
		case CursorAction.Drink:
			if (InfoScreen.Instance.Active)
			{
				inputFrame.AddAction(InputAction.EquipmentUse((Equipment)action.Target, action.Object, action.Actor));
			}
			else
			{
				inputFrame.AddAction(InputAction.Drink((Equipment)action.Target));
			}
			break;
		case CursorAction.DrinkFromRiver:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.DrinkFromRiver(action.Tile, isDoubleClick));
			break;
		case CursorAction.LoadAmmo:
			inputFrame.AddAction(InputAction.EquipmentLoadAmmo((Equipment)action.Target, action.Object, action.Proto));
			break;
		case CursorAction.Unload:
			inputFrame.AddAction(InputAction.EquipmentUnloadAmmo((Equipment)action.Target, action.Object));
			break;
		case CursorAction.LightFuse:
			inputFrame.AddAction(InputAction.EquipmentLightFuse((Equipment)action.Target, action.Object));
			break;
		case CursorAction.DestroyItem:
		{
			Equipment item3 = (Equipment)action.Target;
			if (item3.GetAmount() > 1)
			{
				GameImpl.Instance.ShowEquipmentTransferAmountBox(action.Object, item3, action.Actor, 1, item3.GetAmount(), EquipmentTransferAmountBox.Mode.Destroying);
				break;
			}
			StringBuilder stringBuilder2 = new StringBuilder(100);
			stringBuilder2.Append(GameImpl.Translate("HUD_AreYouSureDestroy"));
			string text = item3.GetDisplayNameString();
			if (item3.GetAmount() > 1)
			{
				text = text + " (x" + item3.GetAmount() + ")";
			}
			stringBuilder2.Replace("%1", text);
			StringUtil.ApplyFormulae(stringBuilder2, null, item3.GetPrototype(), item3.GetAmount());
			GameImpl.Instance.ShowConfirmationBox(stringBuilder2.ToString(), delegate(InputFrame inputFrame2)
			{
				inputFrame2?.AddAction(InputAction.EquipmentDestroy(item3, action.Object, action.Actor, 1));
			}, needsSession: true);
			break;
		}
		case CursorAction.PourAway:
		{
			Equipment item2 = (Equipment)action.Target;
			LiquidPrototype liquidContentsType = item2.GetLiquidContentsType();
			if (liquidContentsType != null)
			{
				StringBuilder stringBuilder = new StringBuilder(100);
				stringBuilder.Append(GameImpl.Translate("HUD_AreYouSurePourAway"));
				stringBuilder.Replace("%1", GameImpl.Translate(liquidContentsType.NameHash));
				StringUtil.ApplyFormulae(stringBuilder, null, item2);
				GameImpl.Instance.ShowConfirmationBox(stringBuilder.ToString(), delegate(InputFrame inputFrame2)
				{
					inputFrame2?.AddAction(InputAction.EquipmentPourAway(item2, action.Object, action.Actor));
				}, needsSession: true);
			}
			break;
		}
		case CursorAction.StartPourInto:
			InfoScreen.Instance.StartPourInto(action.Object, (Equipment)action.Target, action.Actor);
			break;
		case CursorAction.PourInto:
			inputFrame.AddAction(InputAction.EquipmentPourInto(InfoScreen.Instance.Pourer, (Equipment)action.Target, InfoScreen.Instance.PourerActor));
			InfoScreen.Instance.CancelPourInto();
			break;
		case CursorAction.Give:
		case CursorAction.Store:
		{
			InfoPage infoPage2 = InfoScreen.Instance.GetCurrentPage() as InfoPage;
			if (infoPage2 != null)
			{
				infoPage2.OnTransferEquipment(action.Actor, action.Object, (Equipment)action.Target, action.ActionType == CursorAction.Sell, transferAll, inputFrame);
			}
			captureAltAction = true;
			break;
		}
		case CursorAction.Take:
		{
			InfoPage infoPage = InfoScreen.Instance.GetCurrentPage() as InfoPage;
			if (infoPage != null)
			{
				infoPage.OnTransferEquipment(action.Object, action.Actor, (Equipment)action.Target, action.ActionType == CursorAction.Buy, transferAll, inputFrame);
			}
			captureAltAction = true;
			break;
		}
		case CursorAction.Sell:
		{
			TradePage tradePage2 = InfoScreen.Instance.GetCurrentPage() as TradePage;
			if (tradePage2 != null)
			{
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				Equipment item = (Equipment)action.Target;
				int amountAfterPendingTrade2 = tradePage2.GetAmountAfterPendingTrade(item, action.Actor);
				if (amountAfterPendingTrade2 > 1 && !transferAll)
				{
					GameImpl.Instance.ShowEquipmentTransferAmountBox(action.Actor, item, action.Object, 1, amountAfterPendingTrade2, EquipmentTransferAmountBox.Mode.Trading);
				}
				else if (amountAfterPendingTrade2 > 0)
				{
					tradePage2.AddPendingTrade(item, sell: true, amountAfterPendingTrade2);
				}
				captureAltAction = true;
			}
			break;
		}
		case CursorAction.Buy:
		{
			TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
			if (tradePage != null)
			{
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				Equipment equipment3 = (Equipment)action.Target;
				int amountAfterPendingTrade = tradePage.GetAmountAfterPendingTrade(equipment3, action.Object);
				int num = tradePage.TakeFrom.CanSellItemToPlayerCommunity(equipment3);
				amountAfterPendingTrade -= equipment3.GetAmount() - num;
				if (amountAfterPendingTrade > 1 && !transferAll)
				{
					GameImpl.Instance.ShowEquipmentTransferAmountBox(action.Object, equipment3, action.Actor, 1, amountAfterPendingTrade, EquipmentTransferAmountBox.Mode.Trading);
				}
				else if (amountAfterPendingTrade > 0)
				{
					tradePage.AddPendingTrade(equipment3, sell: false, amountAfterPendingTrade);
				}
				captureAltAction = true;
			}
			break;
		}
		case CursorAction.Open:
			inputFrame.AddAction(InputAction.Open(action.Target as Gate));
			break;
		case CursorAction.Unlock:
			inputFrame.AddAction(InputAction.Unlock(action.Target as Gate, isDoubleClick));
			break;
		case CursorAction.Close:
			inputFrame.AddAction(InputAction.Close(action.Target as Gate));
			break;
		case CursorAction.Knock:
			inputFrame.AddAction(InputAction.Knock(action.Target as Gate, isDoubleClick));
			break;
		case CursorAction.SetGatePolicy:
			GameImpl.Instance.ShowGatePolicyDialog(action.Target as Gate);
			break;
		case CursorAction.SetBlockAnimals:
			inputFrame.AddAction(InputAction.SetBlockAnimals(action.Target as Gate, action.Amount != 0));
			break;
		case CursorAction.SetStoragePolicy:
			GameImpl.Instance.ShowStoragePolicyDialog(action.Target as Prop);
			break;
		case CursorAction.SetDesignatedLiquid:
			GameImpl.Instance.ShowDesignateLiquidDialog(action.Object, action.Target as Equipment);
			break;
		case CursorAction.ResetTrap:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.ResetTrap(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.FillFromWell:
			CheckMovementZone(action.Object as Prop);
			inputFrame.AddAction(InputAction.Fill(action.Target as Equipment, (action.Object as Prop).Tile, isDoubleClick));
			break;
		case CursorAction.FillFromRiver:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.Fill(action.Target as Equipment, action.Tile, isDoubleClick));
			break;
		case CursorAction.FillFromSnow:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.Fill(action.Target as Equipment, action.Tile, isDoubleClick));
			break;
		case CursorAction.ScoopSnow:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.ScoopSnow(action.Tile, isDoubleClick));
			break;
		case CursorAction.Plant:
			CheckMovementZone(action.Tile);
			inputFrame.AddAction(InputAction.Plant(localControlledCharacter.EquippedItem, action.Tile, isDoubleClick));
			break;
		case CursorAction.AddMaterialToFire:
			CheckMovementZone(action.Object.GetTile());
			inputFrame.AddAction(InputAction.AddMaterialToFire(action.Target as Equipment, action.Object.GetTile(), isDoubleClick));
			break;
		case CursorAction.WaterCrops:
		case CursorAction.PourFuel:
		case CursorAction.PutOutFire:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.PourOnto(action.Target as Equipment, action.Object, isDoubleClick));
			break;
		case CursorAction.PourWater:
		case CursorAction.Refuel:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.PourInto(action.Target as Equipment, action.Object, isDoubleClick));
			break;
		case CursorAction.EatFromPot:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object, InteractionType.EatFromPot, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.Harvest:
			if (action.Target is PlantableCrop plantableCrop3 && localControlledCharacter.HasInventorySpaceFor(plantableCrop3.GetHarvestWeight()))
			{
				CheckMovementZone(plantableCrop3.Tile);
				inputFrame.AddAction(InputAction.Harvest(plantableCrop3.Tile, isDoubleClick));
			}
			break;
		case CursorAction.ChopTree:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChopTree(action.Object, isDoubleClick));
			break;
		case CursorAction.ChopLog:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChopLog(action.Object, isDoubleClick));
			break;
		case CursorAction.ClearBush:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChopLog(action.Object, isDoubleClick));
			break;
		case CursorAction.SetLumberjack:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Lumberjack(action.Target as TileObject));
			break;
		case CursorAction.Mine:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Mine(action.Object, action.Proto.GetMineralType(), isDoubleClick));
			break;
		case CursorAction.SetMiner:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.SetMiner(action.Target as TileObject, action.Proto.GetMineralType()));
			break;
		case CursorAction.Cook:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Cook(action.Target as TileObject));
			break;
		case CursorAction.SetTrapper:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.SetTrapper(action.Target as TileObject));
			break;
		case CursorAction.SetAnimalFeeder:
			inputFrame.AddAction(InputAction.SetAnimalFeeder());
			break;
		case CursorAction.CancelGuarding:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Guard, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelGathering:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Gatherer, action.Proto, null, action.Tile));
			break;
		case CursorAction.CancelFarming:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Farmer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelLumberjack:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Lumberjack, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelCook:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Cook, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelMining:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Miner, action.Proto, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelTrapper:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Trapper, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelBuildingRole:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Builder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelCraftingRole:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Crafter, null, action.Recipe, action.Tile));
			break;
		case CursorAction.CancelCapturing:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Capturing, null, null, action.Tile));
			break;
		case CursorAction.CancelRepairing:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Repairing, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelAnimalFeeding:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.AnimalFeeder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelOrganizing:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Organizer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.CancelMedic:
			inputFrame.AddAction(InputAction.CancelRole(action.Actor, Role.Medic, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeGuarding:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Guard, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeGathering:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Gatherer, action.Proto, null, action.Tile));
			break;
		case CursorAction.ResumeFarming:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Farmer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeLumberjack:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Lumberjack, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeCook:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Cook, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeMining:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Miner, action.Proto, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeTrapper:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Trapper, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeBuildingRole:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Builder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeCraftingRole:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Crafter, null, action.Recipe, action.Tile));
			break;
		case CursorAction.ResumeCapturing:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Capturing, null, null, action.Tile));
			break;
		case CursorAction.ResumeRepairing:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Repairing, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeAnimalFeeding:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.AnimalFeeder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeOrganizing:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Organizer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeMedic:
			inputFrame.AddAction(InputAction.ResumeRole(action.Actor, Role.Medic, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseGuarding:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Guard, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseGathering:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Gatherer, action.Proto, null, action.Tile));
			break;
		case CursorAction.PauseFarming:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Farmer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseLumberjack:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Lumberjack, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseCook:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Cook, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseMining:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Miner, action.Proto, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseTrapper:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Trapper, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseBuildingRole:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Builder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseCraftingRole:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Crafter, null, action.Recipe, action.Tile));
			break;
		case CursorAction.PauseCapturing:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Capturing, null, null, action.Tile));
			break;
		case CursorAction.PauseRepairing:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Repairing, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseAnimalFeeding:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.AnimalFeeder, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseOrganizing:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Organizer, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.PauseMedic:
			inputFrame.AddAction(InputAction.PauseRole(action.Actor, Role.Medic, null, null, TerrainCoord.Zero));
			break;
		case CursorAction.ResumeAllMyRoles:
			inputFrame.AddAction(InputAction.ResumeAllRoles(action.Actor));
			break;
		case CursorAction.ResumeAllEveryonesRoles:
			inputFrame.AddAction(InputAction.ResumeAllRoles(null));
			break;
		case CursorAction.PickUp:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.PickUp(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.Drop:
			inputFrame.AddAction(InputAction.Drop());
			break;
		case CursorAction.Grab:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Grab(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.PutInBuilding:
		case CursorAction.Bury:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Bury(action.Object as Character, action.Target as Prop, isDoubleClick));
			break;
		case CursorAction.Eulogy:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Eulogy(action.Target as Grave, isDoubleClick));
			break;
		case CursorAction.VisitGrave:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.VisitGrave(action.Target as Grave, isDoubleClick));
			break;
		case CursorAction.Scavenge:
		case CursorAction.ScavengeEmpty:
		case CursorAction.ScavengeNotInvestigated:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Take(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.TakeAll:
		case CursorAction.StealAll:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.TakeAll(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.Build:
		case CursorAction.Dig:
			Cursor.StartPlacingBuilding(null, null, inputFrame);
			break;
		case CursorAction.ResumeBuilding:
		case CursorAction.SetBuilderRole:
			inputFrame.AddAction(InputAction.ResumeBuilding(action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.StopBuilding:
			inputFrame.AddAction(new InputAction(InputActionType.StopBuilding));
			break;
		case CursorAction.Repair:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Repair(action.Target as TileObject));
			break;
		case CursorAction.TakeOver:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.TakeOver(action.Target as TileObject));
			break;
		case CursorAction.Park:
			inputFrame.AddAction(InputAction.VehiclePark(action.Target as EnterableVehicle));
			break;
		case CursorAction.ControlCharacterInBuilding:
		case CursorAction.ViewInventoryInBuilding:
		{
			Character character2 = action.Target as Character;
			if (character2 != null && character2.IsControllableByPlayer())
			{
				Instance.SetLocalControlledCharacter(action.Target as Character, inputFrame);
			}
			BuildingSelectedInhabitant = character2;
			InfoScreen.Instance.WantRepopulate = true;
			break;
		}
		case CursorAction.ViewCharacter:
			if (action.Target is Character character && character.IsControllableByPlayer())
			{
				Instance.SetLocalControlledCharacter(character, inputFrame);
			}
			OpenInfoScreenFor = action.Target as TileObject;
			WantInfoScreenType = OpenInfoScreenType.Inventory;
			break;
		case CursorAction.SelectCharacter:
			SelectCharacter(action.Actor, inputFrame);
			break;
		case CursorAction.DeselectCharacter:
			DeselectCharacter(action.Actor, inputFrame);
			break;
		case CursorAction.TalkTo:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.SpeakTo(action.Object as Character, action.Target, action.SpeechParam, action.Speech, Cursor.LastSpeechToReplyTo, Cursor.LastSpeechObject, Cursor.LastSpeechParam, isDoubleClick));
			break;
		case CursorAction.Gift:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.GiveGift(action.Object as Character, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.OpenGiftMenu:
			Cursor.ShowGiftMenu = true;
			Cursor.SelectedAction = 0;
			break;
		case CursorAction.OpenCraftMenu:
		case CursorAction.OpenCookMenu:
			Cursor.ShowCraftMenuFor = action.Target as CraftingProp;
			Cursor.SelectedAction = 0;
			break;
		case CursorAction.OpenDisassembleMenu:
			Cursor.ShowDisassembleMenuFor = action.Target as CraftingProp;
			Cursor.SelectedAction = 0;
			break;
		case CursorAction.Back:
			Cursor.BackOutOfSubMenu();
			break;
		case CursorAction.TalkToInhabitant:
			if (action.Object == LocalControlledCharacter)
			{
				inputFrame.AddAction(InputAction.ExitBuilding(0, isDoubleClick));
				break;
			}
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.TalkToInhabitant(action.Target as Building, action.Object as Character));
			break;
		case CursorAction.ChokeHold:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.ChokeHold(action.Object as Character, isDoubleClick));
			break;
		case CursorAction.SlitThroat:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.SlitThroat(action.Object as Character, isDoubleClick));
			HintManager.Instance.Hints[16].MarkPerformed();
			break;
		case CursorAction.Restrain:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Restrain(action.Target as Character, isDoubleClick));
			break;
		case CursorAction.BludgeonUnconscious:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.BludgeonUnconscious(action.Object as Character, isDoubleClick));
			break;
		case CursorAction.KillUnconscious:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.KillUnconscious(action.Object as Character, isDoubleClick));
			break;
		case CursorAction.Bandage:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object as Character, InteractionType.ApplyBandage, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.Inject:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object as Character, InteractionType.GiveAntigen, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.Feed:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object as Character, InteractionType.Feed, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.GiveWater:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Interact(action.Object as Character, InteractionType.GiveWater, action.Target as Equipment, isDoubleClick));
			break;
		case CursorAction.Craft:
			if (action.Recipe.ProductPropPrototype != null)
			{
				InfoScreen.Instance.CloseInfoScreen();
				Cursor.StartPlacingBuilding(action.Recipe, action.Target as Equipment, inputFrame);
			}
			else
			{
				TerrainCoord tile = localControlledCharacter.Tile;
				if (action.Recipe.RequiredPropToWorkOn() != BaseObjectType.Invalid)
				{
					tile = action.Tile;
					CheckMovementZone(tile);
				}
				instance.ShowCraftAmountBox(action.Recipe, action.Actor, tile, action.Target as Equipment);
			}
			Cursor.BackOutOfSubMenu();
			break;
		case CursorAction.ResumeCrafting:
			inputFrame.AddAction(InputAction.ResumeCrafting(action.Target as CraftingProp, isDoubleClick));
			break;
		case CursorAction.CraftingLimit:
			instance.ShowCraftLimitBox((Community)action.Target, action.Proto, action.Liquid, action.Amount);
			break;
		case CursorAction.LightFire:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.LightFire(null, action.Target as TileObject, isDoubleClick));
			break;
		case CursorAction.LightFireWithMatch:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.LightFire(action.Target as Equipment, action.Object, isDoubleClick));
			break;
		case CursorAction.LightFireWithFlint:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.LightFire(action.Target as Equipment, action.Object, isDoubleClick));
			break;
		case CursorAction.SitByFire:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.SitByFire(action.Target as Campfire, isDoubleClick));
			break;
		case CursorAction.RepairArmor:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.RepairArmor(action.Target as WorkBench, isDoubleClick));
			break;
		case CursorAction.Skin:
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Skin(action.Object as Character, isDoubleClick));
			HintManager.Instance.Hints[14].MarkPerformed();
			break;
		case CursorAction.SetFarmer:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Farm(action.Target as TileObject));
			break;
		case CursorAction.SetCropsPatch:
			Cursor.StartSettingCropPatch(action.Proto.GetSeedForPlantType(), inputFrame);
			break;
		case CursorAction.Gather:
			WantInfoScreenType = OpenInfoScreenType.Gather;
			OpenInfoScreenFor = action.Target as Prop;
			if (!OpenInfoScreenFor.IsInvestigated())
			{
				inputFrame?.AddAction(InputAction.MarkInvestigated(OpenInfoScreenFor, localControlledCharacter));
			}
			break;
		case CursorAction.Guard:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.Guard(action.Target as TileObject));
			break;
		case CursorAction.SelectToGather:
			InfoScreen.Instance.CloseInfoScreen();
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Gather(action.Object as Prop, (action.Target as Equipment).GetPrototype()));
			break;
		case CursorAction.SelectAllToGather:
			InfoScreen.Instance.CloseInfoScreen();
			CheckMovementZone(action.Object);
			inputFrame.AddAction(InputAction.Gather(action.Object as Prop, null));
			break;
		case CursorAction.LocateEquipment:
			LocateObject(action.Object, inputFrame);
			break;
		case CursorAction.SetOrganizer:
			CheckMovementZone(action.Target as TileObject);
			inputFrame.AddAction(InputAction.SetOrganizer());
			break;
		case CursorAction.SetMedic:
			inputFrame.AddAction(new InputAction(InputActionType.SetMedic));
			break;
		case CursorAction.SetPropName:
		{
			TileObject target = (TileObject)action.Target;
			Prop prop = target as Prop;
			Animal animal = target as Animal;
			string initialText = string.Empty;
			if (prop != null && !GameImpl.WantCensoredString(englishOnly: false, prop.PropNameVerified))
			{
				initialText = prop.CustomName;
			}
			if (animal != null && !GameImpl.WantCensoredString(englishOnly: false, animal.FirstNameVerified))
			{
				initialText = animal.FirstName;
			}
			GameImpl.Instance.ShowInputBox(delegate(InputFrame inputFrame2, string result)
			{
				inputFrame2?.AddAction(InputAction.SetPropName(target, result));
			}, GameImpl.Translate("HUD_SetPropName"), initialText, multiline: false, readOnly: false, needsSession: true);
			break;
		}
		case CursorAction.SetFoodPolicy:
		case CursorAction.SetDrinkPolicy:
		case CursorAction.SetAmmoPolicy:
		case CursorAction.SetWeaponAmmoPolicy:
		case CursorAction.SetEquipmentPolicy:
			GameImpl.Instance.ShowEquipmentPolicyDialog(action.Object as Character, action.Proto, action.Liquid, action.InfectedWith);
			break;
		case CursorAction.PowerNap:
			inputFrame.AddAction(InputAction.PowerNap(action.Actor));
			break;
		case CursorAction.EquipmentName:
		case CursorAction.EquipmentNameAndIcon:
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
		case CursorAction.CropRating:
		case CursorAction.StartingPoints:
		case CursorAction.EquipmentTotalName:
		case CursorAction.Tooltip:
		case CursorAction.Empty:
		case CursorAction.NotInvestigated:
		case CursorAction.NumberOfCrafters:
		case CursorAction.WeaponStats:
		case CursorAction.TastinessStat:
		case CursorAction.MapQuadrants:
		case CursorAction.EditorSelect:
		case CursorAction.SpeechToReplyTo:
		case CursorAction.Lock:
		case CursorAction.BuildHere:
		case CursorAction.SetCropsPatchHere:
		case CursorAction.SelectLiquid:
		case CursorAction.DeselectLiquid:
		case CursorAction.StoreHere:
		case CursorAction.DontStoreHere:
		case CursorAction.LocateProp:
		case CursorAction.ModName:
			break;
		}
	}

	public void OnSelectedDisabledAction(AvailableAction action)
	{
		SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
		if (InfoScreen.Instance.Active)
		{
			switch (action.ActionType)
			{
			case CursorAction.Give:
			case CursorAction.Store:
			case CursorAction.Sell:
				InfoPage.ShowCantTransferReason(action.Reason, action.Actor, (Equipment)action.Target, action.Object);
				return;
			case CursorAction.Take:
			case CursorAction.Buy:
				InfoPage.ShowCantTransferReason(action.Reason, action.Object, (Equipment)action.Target, action.Actor);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder(100);
			action.BuildDisabledReasonString(stringBuilder);
			if (stringBuilder.Length > 0)
			{
				GameImpl.Instance.ShowMessageBox(stringBuilder.ToString());
			}
		}
		else
		{
			HudBehaviour.Instance.SetStatusBarMsg(action);
		}
	}

	public static void SetHudSoundVolume(AudioSource unitySound, float volume)
	{
		unitySound.volume = volume * SoundManager.MenuSoundVolume;
		if (unitySound.volume > 0f && !unitySound.isPlaying && !GameImpl.Instance.IsMenuOpen())
		{
			unitySound.Play();
			unitySound.time = MathUtil.NonDeterministicRand.RandomFloat() * unitySound.clip.length;
		}
		else if (unitySound.volume == 0f && unitySound.isPlaying)
		{
			unitySound.Stop();
		}
	}

	public void ShowBigHintPrompt(InputFunction inputFunction, Character character, int hash)
	{
		BigHintPrompt = inputFunction;
		BigHintPromptHash = hash;
		ParryPromptAttacker = character;
	}

	public void ShowParryPrompt(Character attacker, bool enabled, float power)
	{
		if (attacker.GetTimeTillAttack(LocalControlledCharacter, out var remainingTime, out var totalTime))
		{
			ParryPromptTimeoutRadius = Mathf.Lerp(0.2f, 1f, (float)remainingTime.TotalSeconds / (float)totalTime.TotalSeconds);
			ParryPromptAttacker = attacker;
			ParryPromptEnabled = enabled;
			ParryPrompt = true;
			ParryPromptSuccess = false;
			ParryPromptTargetEscapePower = power;
		}
	}

	public void OnParrySucceeded(Character attacker, int hash, float power)
	{
		ParryPromptHash = hash;
		ParryPrompt = false;
		ParryPromptSuccess = true;
		ParryPromptSuccessTransition = 0f;
		ParryPromptAttacker = attacker;
		ParryPromptTargetEscapePower = power;
		SoundManager.PlayMenuSoundFromList(SoundManager.NotificationSounds);
	}

	public void OnZombieEscapeButtonMashed(Character attacker)
	{
		ParryPromptCurrentSize = 1f;
		ParryPromptPrevSize = 0.5f;
	}

	public void ShowChokePrompt(Character victim, bool enabled, float power)
	{
		ParryPromptTimeoutRadius = 1f;
		ParryPromptAttacker = victim;
		ParryPromptEnabled = enabled;
		ParryPrompt = true;
		ParryPromptSuccess = false;
		ParryPromptTargetEscapePower = power;
		if (LastParryPromptFrame < GameImpl.FrameCount - 1)
		{
			ParryPromptCurrentEscapePower = power;
		}
	}

	public void SelectCharacter(Character character, InputFrame inputFrame)
	{
		inputFrame.AddAction(InputAction.AddSelectedCharacter(character));
		SelectedCharacters.Add(character);
		character.NonDeterministicSelected = true;
	}

	public void DeselectCharacter(Character character, InputFrame inputFrame)
	{
		inputFrame.AddAction(InputAction.RemoveSelectedCharacter(character));
		SelectedCharacters.Remove(character);
		character.NonDeterministicSelected = false;
	}

	public void SelectFollowers(Character actor, bool addToSelection, InputFrame inputFrame)
	{
		if (actor.Followers != null)
		{
			if (!addToSelection)
			{
				foreach (Character selectedCharacter in SelectedCharacters)
				{
					selectedCharacter.NonDeterministicSelected = false;
				}
				SelectedCharacters.Clear();
			}
			foreach (Character follower in actor.Followers)
			{
				if (!follower.NonDeterministicSelected)
				{
					follower.NonDeterministicSelected = true;
					SelectedCharacters.Add(follower);
				}
			}
		}
		if (addToSelection)
		{
			inputFrame.AddAction(new InputAction(InputActionType.AddFollowersToSelection));
		}
		else
		{
			inputFrame.AddAction(new InputAction(InputActionType.SelectFollowers));
		}
	}

	public void SelectAllCharacters(InputFrame inputFrame)
	{
		foreach (Character member in Session.Instance.CommunityManager.PlayerCommunity.Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.NonDeterministicSelected)
			{
				member.NonDeterministicSelected = true;
				SelectedCharacters.Add(member);
			}
		}
		inputFrame.AddAction(new InputAction(InputActionType.SelectEveryone));
	}

	public void DeselectAllCharacters(InputFrame inputFrame)
	{
		foreach (Character selectedCharacter in SelectedCharacters)
		{
			selectedCharacter.NonDeterministicSelected = false;
		}
		SelectedCharacters.Clear();
		inputFrame.AddAction(new InputAction(InputActionType.ClearSelectedCharacters));
	}

	public void DragSelectCharactersInBox(Vector2 dragStartXZ, Vector2 dragFinishXZ, bool addToSelection, InputFrame inputFrame)
	{
		GameTerrain instance = GameTerrain.Instance;
		Vector2 vector = new Vector2(Math.Min(dragStartXZ.x, dragFinishXZ.x), Math.Min(dragStartXZ.y, dragFinishXZ.y));
		Vector2 vector2 = new Vector2(Math.Max(dragStartXZ.x, dragFinishXZ.x), Math.Max(dragStartXZ.y, dragFinishXZ.y));
		TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(vector);
		TerrainCoord tileCoordForPosXZ2 = instance.GetTileCoordForPosXZ(vector2);
		instance.CharacterMapWho.GetObjectsInRect(vector, vector2, CharactersInSelectionBox);
		instance.GetObjectsOfTypeInRect(tileCoordForPosXZ, tileCoordForPosXZ2, BuildingsInSelectionBox, typeof(Building));
		foreach (Building item in BuildingsInSelectionBox)
		{
			for (int i = 0; i < item.Inhabitants.Length; i++)
			{
				if (item.Inhabitants[i] != null)
				{
					CharactersInSelectionBox.Add(item.Inhabitants[i]);
				}
			}
		}
		foreach (Character item2 in CharactersInSelectionBox)
		{
			if (item2.IsSelectable() && !item2.NonDeterministicSelected)
			{
				SelectCharacter(item2, inputFrame);
				HintManager.Instance.Hints[28].MarkPerformed();
				if (addToSelection)
				{
					HintManager.Instance.Hints[29].MarkPerformed();
				}
			}
		}
		if (addToSelection)
		{
			return;
		}
		for (int num = SelectedCharacters.Count - 1; num >= 0; num--)
		{
			Character character = SelectedCharacters[num];
			if (!CharactersInSelectionBox.Contains(character))
			{
				DeselectCharacter(character, inputFrame);
			}
		}
	}

	private TileObject PickTargetInFlyCamMode()
	{
		GameTerrain instance = GameTerrain.Instance;
		if (ReallyDragging)
		{
			return null;
		}
		if (Cursor.IsPlacingBuilding)
		{
			return null;
		}
		if (CursorRayCastResult.HitObject is TileObject tileObject && tileObject.IsTargetable())
		{
			return tileObject;
		}
		float flyCamTargetableRadius = FlyCamTargetableRadius;
		int num = (int)Math.Ceiling(flyCamTargetableRadius);
		instance.GetObjectsInRect(CursorRayCastResult.Tile - new TerrainCoord(num, num), CursorRayCastResult.Tile + new TerrainCoord(num, num), PotentialTargets);
		Vector3 hitPosition = CursorRayCastResult.GetHitPosition();
		float num2 = float.MaxValue;
		TileObject result = null;
		foreach (TileObject potentialTarget in PotentialTargets)
		{
			if (potentialTarget is Character && potentialTarget.IsTargetable())
			{
				float magnitude = Get2DVectorToTarget(MathUtil.ToXZ(hitPosition), potentialTarget).magnitude;
				if (!(magnitude > flyCamTargetableRadius) && magnitude < num2)
				{
					num2 = magnitude;
					result = potentialTarget;
				}
			}
		}
		return result;
	}

	public Vector2 Get2DVectorToTarget(Vector2 posXZ, TileObject target)
	{
		bool round;
		return Get2DVectorToTarget(posXZ, target, out round);
	}

	public Vector2 Get2DVectorToTarget(Vector2 posXZ, TileObject target, out bool round)
	{
		if (target is Character character)
		{
			round = true;
			return character.PosXZ - posXZ;
		}
		if (target.GetTileRect().TilesArea <= 1 && (target.IsRound() || !target.CanEncloseAnArea()))
		{
			round = true;
			return target.PosXZ - posXZ;
		}
		Bounds boundingBox = target.GetBoundingBox();
		Vector2 tl = MathUtil.ToXZ(boundingBox.min);
		Vector2 br = MathUtil.ToXZ(boundingBox.max);
		round = false;
		return MathUtil.GetShortestVectorFromPointToRect(posXZ, tl, br);
	}

	public Vector3 GetVectorToTarget(Vector3 pos, TileObject target, bool isUsingThrowable)
	{
		if (target is Character character)
		{
			return character.Pos + new Vector3(0f, isUsingThrowable ? 0f : (character.Height * 0.5f), 0f) - pos;
		}
		Bounds boundingBox = target.GetBoundingBox();
		Vector2 tl = MathUtil.ToXZ(boundingBox.min);
		Vector2 br = MathUtil.ToXZ(boundingBox.max);
		return MathUtil.ToXZY(MathUtil.GetShortestVectorFromPointToRect(pos, tl, br), isUsingThrowable ? boundingBox.min.y : boundingBox.center.y);
	}

	public Vector3 GetGameCameraForwardDirWithLockedOnExtraPitch()
	{
		Vector3 forward = Session.Instance.GameCamera.GetForward();
		Vector3 right = Session.Instance.GameCamera.GetRight();
		return Matrix4x4.Rotate(Quaternion.AngleAxis((LocalControlledCharacter != null && LocalControlledCharacter.IsGuarding()) ? ReticulePitchInWatchTower : ReticulePitch, -right)).MultiplyVector(forward);
	}

	public static Character FindNearestEnemy(Character controlledCharacter)
	{
		TerrainCoord tile = controlledCharacter.Tile;
		int num = 32;
		GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(tile - new TerrainCoord(num, num), tile + new TerrainCoord(num, num), NearbyCharacters);
		Character result = null;
		float num2 = float.MaxValue;
		foreach (Character nearbyCharacter in NearbyCharacters)
		{
			if (nearbyCharacter.IsAwake && controlledCharacter.IsEnemy(nearbyCharacter, includeJustActivatedInvisibleStrain: true))
			{
				float sqrMagnitude = MathUtil.ToXZ(nearbyCharacter.Pos - controlledCharacter.Pos).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					result = nearbyCharacter;
				}
			}
		}
		NearbyCharacters.Clear();
		return result;
	}

	private TileObject PickTargetInDirectControlMode(bool newWantLockOnTarget)
	{
		GameTerrain instance = GameTerrain.Instance;
		GameCamera gameCamera = Session.Instance.GameCamera;
		Character localControlledCharacter = LocalControlledCharacter;
		if (localControlledCharacter == null || !localControlledCharacter.IsAwake)
		{
			return null;
		}
		if (!localControlledCharacter.IsOutdoors())
		{
			return null;
		}
		if (localControlledCharacter.InteractionObject is Character || localControlledCharacter.IsBeingBittenOrChoked() || localControlledCharacter.IsChokingSomeone() || localControlledCharacter.CurrentActionAnim == ActionAnim.Hugged)
		{
			return null;
		}
		if (!newWantLockOnTarget && !localControlledCharacter.IsSitting() && localControlledCharacter.FindActiveGoal(GoalType.ObeyLeaderGoal) is ObeyLeaderGoal obeyLeaderGoal)
		{
			TileObject targetObject = obeyLeaderGoal.GetTargetObject();
			if (targetObject != null)
			{
				if (targetObject.IsTargetable() && targetObject.GetTileRect().GetClosestDistSqTo(localControlledCharacter.Tile) <= MathUtil.Squared(3f))
				{
					return targetObject;
				}
				if (obeyLeaderGoal.GetCommand() is Conversation && !(targetObject is Character { Disappeared: not false }))
				{
					return targetObject;
				}
			}
			return null;
		}
		bool flag = localControlledCharacter.EquippedItem is RangedWeapon || localControlledCharacter.DesiredEquippedItem is RangedWeapon;
		bool flag2 = localControlledCharacter.EquippedItem is Throwable || localControlledCharacter.DesiredEquippedItem is Throwable;
		if (flag2 && TargetSwitchCountdown > 0f && LocalTargetObject == null)
		{
			return null;
		}
		TerrainCoord tile = localControlledCharacter.Tile;
		float num = DirectControlTargetableRadius;
		if (newWantLockOnTarget)
		{
			localControlledCharacter.GetSightRange(out var _, out var fogEnd);
			num = fogEnd;
		}
		else if (localControlledCharacter.IsSitting())
		{
			num = DirectControlTargetableSittingRadius;
		}
		int num2 = (int)Math.Ceiling(Math.Max(num, Math.Max(DirectControlTargetableChokeHoldRadius, DirectControlTargetableConversationRadius)));
		instance.CharacterMapWho.GetObjectsInRect(tile - new TerrainCoord(num2, num2), tile + new TerrainCoord(num2, num2), PotentialTargets);
		if (!newWantLockOnTarget)
		{
			instance.GetObjectsInRect(tile - new TerrainCoord(1, 1), tile + new TerrainCoord(1, 1), PotentialTargets, clearList: false);
		}
		if (newWantLockOnTarget && LocalTargetObject != null && !PotentialTargets.Contains(LocalTargetObject))
		{
			PotentialTargets.Add(LocalTargetObject);
		}
		if (localControlledCharacter.CurrentActionAnim == ActionAnim.HandsUp)
		{
			instance.CharacterMapWho.GetObjectsInRect(tile - new TerrainCoord(128, 128), tile + new TerrainCoord(128, 128), TempCharacters);
			foreach (Character tempCharacter in TempCharacters)
			{
				Conversation conversationWith = GameCursor.GetConversationWith(localControlledCharacter, tempCharacter);
				if (conversationWith != null && !conversationWith.IsMovingToTarget() && !PotentialTargets.Contains(tempCharacter))
				{
					PotentialTargets.Add(tempCharacter);
				}
			}
			TempCharacters.Clear();
		}
		if (localControlledCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation)
		{
			Character targetCharacter = conversation.GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.CurrentActionAnim == ActionAnim.HandsUp && !PotentialTargets.Contains(targetCharacter))
			{
				PotentialTargets.Add(targetCharacter);
			}
		}
		Vector3 vector = LocalTargetPos;
		bool flag3 = false;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		TileObject result = null;
		foreach (TileObject potentialTarget in PotentialTargets)
		{
			if (potentialTarget == localControlledCharacter || potentialTarget == localControlledCharacter.InsideBuilding || potentialTarget == localControlledCharacter.CarryingObject || !potentialTarget.IsTargetable())
			{
				continue;
			}
			float num5 = num;
			if (newWantLockOnTarget)
			{
				num5 *= potentialTarget.GetTargetableRadiusFactor(flag);
				if (potentialTarget == LocalTargetObject)
				{
					num5 += DirectControlTargetableLockRadiusHysteresis;
				}
			}
			Character character2 = potentialTarget as Character;
			if (localControlledCharacter.IsSitting() && (character2 == null || !character2.IsSitting()))
			{
				num5 = DirectControlTargetableConversationRadius;
			}
			Conversation conversationWith2 = GameCursor.GetConversationWith(localControlledCharacter, potentialTarget);
			bool flag4 = conversationWith2 != null && !conversationWith2.IsMovingToTarget();
			if (flag4)
			{
				num5 = Math.Max(num5, DirectControlTargetableConversationRadius);
				if (character2 != null && character2.CurrentActionAnim == ActionAnim.HandsUp)
				{
					num5 = float.MaxValue;
				}
			}
			if (GameCursor.CanChokeHold(localControlledCharacter, potentialTarget))
			{
				num5 = Math.Max(num5, DirectControlTargetableChokeHoldRadius);
			}
			bool round;
			Vector2 v = Get2DVectorToTarget(MathUtil.ToXZ(localControlledCharacter.Position), potentialTarget, out round);
			float magnitude = v.magnitude;
			if (magnitude > num5)
			{
				continue;
			}
			bool flag5 = newWantLockOnTarget || localControlledCharacter.IsSitting();
			Vector2 rhs = (flag5 ? MathUtil.SafeNormalize(MathUtil.ToXZ(gameCamera.GetForward()), MathUtil.ToXZ(localControlledCharacter.Forward)) : MathUtil.ToXZ(localControlledCharacter.Forward));
			if (newWantLockOnTarget)
			{
				v = Get2DVectorToTarget(MathUtil.ToXZ(gameCamera.GetPos()), potentialTarget, out round);
			}
			float num6 = Mathf.Clamp(Vector2.Dot(MathUtil.SafeNormalize(v, Vector2.zero), rhs), -1f, 1f);
			float num7 = (float)Math.Acos(num6);
			if (round && !newWantLockOnTarget)
			{
				float num8 = magnitude * num6;
				float num9 = Mathf.Tan(num7) * num8;
				float humanRadius = Character.HumanRadius;
				num9 = Math.Max(0f, num9 - humanRadius);
				num7 = Mathf.Atan2(num9, num8);
			}
			float num10 = magnitude + num7 / (MathF.PI / 2f) * DirectControlTargetAngleBiasPer90Deg;
			if (newWantLockOnTarget)
			{
				bool flag6 = potentialTarget == LocalTargetObject && LocalWantLockOnTarget;
				if (!flag6)
				{
					if ((LocalTargetObject == null || !LocalWantLockOnTarget) && flag)
					{
						Vector3 vectorToTarget = GetVectorToTarget(gameCamera.GetPos(), potentialTarget, flag2);
						Vector3 rhs2;
						float num11;
						if (flag2)
						{
							if (!LocalWantLockOnTarget && !flag3)
							{
								float length = (float)localControlledCharacter.GetSightRange() + 1f;
								int flags = 40;
								vector = instance.RayCast(new Ray(gameCamera.GetPos(), GetGameCameraForwardDirWithLockedOnExtraPitch()), length, flags, localControlledCharacter.InsideBuilding, localControlledCharacter, null, predicted: false).GetHitPosition();
								flag3 = true;
							}
							rhs2 = MathUtil.SafeNormalize(vector - gameCamera.GetPos(), gameCamera.GetForward());
							num11 = DirectControlTargetableThrowableAngleDeg * (MathF.PI / 180f);
						}
						else
						{
							rhs2 = GetGameCameraForwardDirWithLockedOnExtraPitch();
							num11 = DirectControlTargetableAngleFrac * Mathf.Atan2(potentialTarget.Height * 0.5f, vectorToTarget.magnitude);
						}
						if ((float)Math.Acos(Mathf.Clamp(Vector3.Dot(MathUtil.SafeNormalize(vectorToTarget, Vector3.zero), rhs2), -1f, 1f)) > num11)
						{
							continue;
						}
					}
					else if (num7 > potentialTarget.GetTargetableAngle())
					{
						continue;
					}
				}
				if (character2 == null)
				{
					continue;
				}
				if (flag6)
				{
					num10 -= 10000f;
				}
				if (!character2.Alive)
				{
					if (character2 != LocalTargetObject || !LocalWantLockOnTarget)
					{
						continue;
					}
					num10 += 100f;
				}
				else
				{
					bool flag7 = localControlledCharacter.IsEnemy(potentialTarget);
					bool flag8 = flag7;
					flag8 |= character2.Community != null && character2.Community.IsAnimalCommunity();
					flag8 = flag8 || flag;
					if (localControlledCharacter.SparringPartner == potentialTarget)
					{
						if (localControlledCharacter.SparringType == SparringType.Boxing)
						{
							flag8 |= localControlledCharacter.EquippedItem == null;
						}
						else if (localControlledCharacter.SparringType == SparringType.Fencing)
						{
							flag8 |= localControlledCharacter.EquippedItem == null || localControlledCharacter.EquippedItem.GetInjuryType() == InjuryType.BluntObject;
						}
						else if (localControlledCharacter.SparringType == SparringType.SnowballFight)
						{
							flag8 |= localControlledCharacter.EquippedItem == null || localControlledCharacter.EquippedItem.GetPrototype() == EquipmentPrototype.Snowball;
						}
						else if (localControlledCharacter.SparringType == SparringType.Feuding || localControlledCharacter.SparringType == SparringType.FightToTheDeath)
						{
							flag8 = true;
						}
					}
					else
					{
						flag8 &= character2.Zombie || (character2.Community != localControlledCharacter.Community && !localControlledCharacter.Community.CachedAllies.Contains(character2.Community));
					}
					if (!flag8)
					{
						continue;
					}
					if (character2.Consciousness == Consciousness.Unconscious)
					{
						num10 += 10f;
					}
					if (character2.IsRagdollOrProneOrRecovering())
					{
						num10 += 2f;
					}
					if (character2.Zombie && character2.ShouldLimp())
					{
						num10 += 2f;
					}
					if (!flag7)
					{
						num10 += 1000f;
					}
					PotentialTargetsForStick.Add(character2);
				}
			}
			else if (flag5)
			{
				if (((character2 == null || !character2.AliveAndNotZombie) && (!(potentialTarget.GetCentreTile() == localControlledCharacter.SittingAround) || !(magnitude <= TileObject.MaxFireHeatRange))) || num7 > MathF.PI / 4f)
				{
					continue;
				}
			}
			else
			{
				if ((num6 < 0f && !flag4 && (character2 == null || character2.IsAwake)) || (localControlledCharacter.InsideBuilding != null && (character2 == null || character2.CurrentActionAnim != ActionAnim.HandsUp)))
				{
					continue;
				}
				if (flag4)
				{
					num10 *= 0.5f;
					if (num6 >= 0f)
					{
						num10 -= DirectControlTargetableConversationRadius;
					}
				}
				if (character2 == null)
				{
					num10 += 2f;
					switch (potentialTarget.GetBaseObjectType())
					{
					case BaseObjectType.Bush:
						num10 += 8f;
						break;
					case BaseObjectType.TreeProp:
					case BaseObjectType.Rock:
					case BaseObjectType.FallenTreeProp:
					case BaseObjectType.Boulder:
					case BaseObjectType.PlantableCrop:
						num10 += 4f;
						break;
					}
					if (potentialTarget.GetUnderConstructionInfo() != null)
					{
						num10 *= 0.99f;
					}
				}
			}
			float num12 = Vector2.Dot(MathUtil.SafeNormalize(MathUtil.ToXZ(potentialTarget.Pos - (newWantLockOnTarget ? gameCamera.GetPos() : localControlledCharacter.Pos)), Vector2.zero), MathUtil.ToXZ(localControlledCharacter.Forward));
			if (num10 < num3 || (num10 == num3 && num12 > num4))
			{
				num3 = num10;
				num4 = num12;
				result = potentialTarget;
			}
		}
		return result;
	}

	public bool GuessIfIAmControllingLocalControlledCharacter()
	{
		return Session.Instance.GetPlayerControllingCharacter(LocalControlledCharacter)?.IsLocal ?? true;
	}

	public void SetLocalControlledCharacter(Character character, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InfoScreen instance2 = InfoScreen.Instance;
		if (character == null)
		{
			return;
		}
		if (LocalControlledCharacter != character)
		{
			BuildingSelectedInhabitant = (LocalControlledCharacter = character);
			inputFrame?.AddAction(InputAction.SetControlledCharacter(character));
			if (!instance.GameCamera.FlyCam)
			{
				instance.GameCamera.WantSnapZoom = true;
			}
			ShowFocusedCharacterHud();
			ShowPlayerName();
			VignetteAmount = (LocalControlledCharacter.HasUnbandagedInjury(0) ? DamageVignetteAmountWhenLosingBlood : 0f);
			WantVignetteTillTime = Target.Never;
			if (Cursor.IsPlacingBuilding)
			{
				Cursor.StopPlacingBuilding();
			}
			if (Cursor.IsSettingCropPatch != null)
			{
				Cursor.IsSettingCropPatch = null;
			}
			Cursor.ResetShortcutTimer();
		}
		if (LocalControlledCharacter != null && instance2.Active)
		{
			instance2.WantSwitchCharacter = true;
		}
	}

	public void LocateObject(TileObject obj, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		if (obj is Character)
		{
			SetLocalControlledCharacter(obj as Character, inputFrame);
			instance.GameCamera.SetFlyCamMode(on: false, snap: false, inputFrame);
		}
		else
		{
			instance.GameCamera.SetFlyCamMode(on: true, snap: true, inputFrame);
			instance.GameCamera.Teleport(GameTerrain.Instance.ClampPosToSurface(obj.Pos));
		}
		InfoScreen.Instance.CloseInfoScreen();
	}

	public void ClearCursorTarget()
	{
		CursorTargetObject = null;
		DraggingZone = false;
	}

	public TileObject GetLocalTargetObject()
	{
		if (!Session.Instance.GameCamera.FlyCam)
		{
			return LocalTargetObject;
		}
		return CursorTargetObject;
	}

	public bool HasAnyPotentialEnemyTargetsToSwitchTo()
	{
		foreach (TileObject item in PotentialTargetsForStick)
		{
			if (item != LocalTargetObject && item is Character { Alive: not false } character && LocalControlledCharacter.IsEnemy(character))
			{
				return true;
			}
		}
		return false;
	}

	public void ShowFocusedCharacterHud()
	{
		FocusedCharacterArrowTimeout = 8f;
	}

	public void ShowPlayerName()
	{
		PlayerNameTimeout = 8f;
	}

	public void TriggerDamageVignette()
	{
		WantVignetteTillTime = Session.Instance.PlayTime + TimeSpan.FromSeconds(0.25);
	}

	private void UpdateDamageVignette()
	{
		if (Session.Instance.PlayTime <= WantVignetteTillTime)
		{
			VignetteAmount = MathUtil.Delt(VignetteAmount, 1f, Time.deltaTime / 0.125f);
		}
		else if (VignetteAmount > DamageVignetteAmountWhenLosingBlood)
		{
			VignetteAmount *= 1f - 0.3f * Time.deltaTime;
		}
		Character localControlledCharacter = LocalControlledCharacter;
		if (localControlledCharacter == null)
		{
			return;
		}
		SoundManager instance = SoundManager.Instance;
		bool flag = Session.Instance.IsPaused();
		if (instance.UnityHeartBeatAudioSource != null)
		{
			float adrenaline = localControlledCharacter.GetAdrenaline();
			if (adrenaline > 0f && !instance.UnityHeartBeatAudioSource.isPlaying && !flag && !GameImpl.Instance.IsMenuOpen())
			{
				instance.UnityHeartBeatAudioSource.Play();
			}
			else if (instance.UnityHeartBeatAudioSource.isPlaying && adrenaline == 0f)
			{
				instance.UnityHeartBeatAudioSource.Stop();
			}
			if (instance.UnityHeartBeatAudioSource.isPlaying)
			{
				instance.UnityHeartBeatAudioSource.volume = SoundManager.WorldSoundVolume;
			}
		}
		if (instance.UnityMalePantingAudioSource != null)
		{
			float fatigueMinusAdrenaline = localControlledCharacter.GetFatigueMinusAdrenaline();
			if (fatigueMinusAdrenaline >= 1f && !instance.UnityMalePantingAudioSource.isPlaying && localControlledCharacter.Appearance.Gender == GenderType.Male && !flag && !GameImpl.Instance.IsMenuOpen())
			{
				instance.UnityMalePantingAudioSource.Play();
			}
			else if (instance.UnityMalePantingAudioSource.isPlaying && (fatigueMinusAdrenaline < Character.ExhaustedFatigueLevel || localControlledCharacter.Appearance.Gender != GenderType.Male || flag))
			{
				instance.UnityMalePantingAudioSource.Stop();
				if (PantingCharacterId == localControlledCharacter.Id && !flag)
				{
					localControlledCharacter.PlayVoiceSound(SoundManager.MaleCatchingBreathSound, looping: false, VoiceSoundType.Exhausted);
				}
				PantingCharacterId = 0;
			}
			if (instance.UnityMalePantingAudioSource.isPlaying)
			{
				instance.UnityMalePantingAudioSource.volume = SoundManager.WorldSoundVolume;
				PantingCharacterId = localControlledCharacter?.Id ?? 0;
			}
		}
		if (!(instance.UnityFemalePantingAudioSource != null))
		{
			return;
		}
		float fatigueMinusAdrenaline2 = localControlledCharacter.GetFatigueMinusAdrenaline();
		if (fatigueMinusAdrenaline2 >= 1f && !instance.UnityFemalePantingAudioSource.isPlaying && localControlledCharacter.Appearance.Gender == GenderType.Female && !flag && !GameImpl.Instance.IsMenuOpen())
		{
			instance.UnityFemalePantingAudioSource.Play();
		}
		else if (instance.UnityFemalePantingAudioSource.isPlaying && (fatigueMinusAdrenaline2 < Character.ExhaustedFatigueLevel || localControlledCharacter.Appearance.Gender != GenderType.Female || flag))
		{
			instance.UnityFemalePantingAudioSource.Stop();
			if (PantingCharacterId == localControlledCharacter.Id && !flag)
			{
				localControlledCharacter.PlayVoiceSound(SoundManager.FemaleCatchingBreathSound, looping: false, VoiceSoundType.Exhausted);
			}
			PantingCharacterId = 0;
		}
		if (instance.UnityFemalePantingAudioSource.isPlaying)
		{
			instance.UnityFemalePantingAudioSource.volume = SoundManager.WorldSoundVolume;
			PantingCharacterId = localControlledCharacter?.Id ?? 0;
		}
	}

	public void OnPostRender()
	{
		Cursor.OnPostRender();
		Session instance = Session.Instance;
		if (instance.PlaySpeed == PlaySpeed.Paused)
		{
			Character character = LocalControlledCharacter;
			if (InfoScreen.GetAllowViewInfoOnAnyone() && Pip.FocusObject is Character)
			{
				character = Pip.FocusObject as Character;
			}
			if (character != null)
			{
				GameTerrain instance2 = GameTerrain.Instance;
				GameCamera gameCamera = instance.GameCamera;
				GameCameraBehaviour unityGameCameraBehaviour = HudBehaviour.Instance.UnityGameCameraBehaviour;
				float num = Mathf.Lerp(DotRadius, DotRadiusZoomedOut, gameCamera.FlyCamTransition);
				Vector3 vector = gameCamera.GetUp() * num;
				Vector3 vector2 = gameCamera.GetRight() * num;
				Vector3 vector3 = character.Position;
				DebugGraphics.StartDrawTexturedQuads(Matrix4x4.identity, Dot);
				float num2 = 0f;
				for (int i = 0; i < character.GetRouteCount(); i++)
				{
					TerrainCoord routeNode = character.GetRouteNode(i);
					Vector3 tileCentrePos = instance2.GetTileCentrePos(routeNode);
					Bounds aabb = MathUtil.CreateBoundsMinMax(Vector3.Min(vector3, tileCentrePos), Vector3.Max(vector3, tileCentrePos));
					if (unityGameCameraBehaviour.IsInFrustum(aabb))
					{
						Vector3 vector4 = tileCentrePos - vector3;
						float magnitude = vector4.magnitude;
						if (!(magnitude < 0.0001f))
						{
							vector4 /= magnitude;
							num2 += magnitude;
							while (num2 > DotSeparation)
							{
								vector3 += vector4 * DotSeparation;
								num2 -= DotSeparation;
								Vector3 vector5 = vector3;
								vector5.y = instance2.GetTileHeightAtPos(vector3.x, vector3.z) + num;
								DebugGraphics.DrawQuad(vector5 + vector - vector2, vector5 + vector + vector2, vector5 - vector + vector2, vector5 - vector - vector2);
							}
						}
					}
					else
					{
						num2 = 0f;
						vector3 = tileCentrePos;
					}
				}
				DebugGraphics.EndDrawQuads();
			}
		}
		if (DragSelecting && (DragSelectFinishXZ - DragSelectStartXZ).magnitude >= MinDragDist)
		{
			GameTerrain instance3 = GameTerrain.Instance;
			Vector2 vector6 = Vector2.Min(DragSelectStartXZ, DragSelectFinishXZ);
			Vector2 vector7 = Vector2.Max(DragSelectStartXZ, DragSelectFinishXZ);
			DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: true);
			float num3 = vector6.x;
			float x = num3;
			while (true)
			{
				num3 += Mathf.Min(vector7.x - num3, 1f - (num3 - Mathf.Floor(num3)));
				DebugGraphics.DrawThickLine(instance3.GetSurfaceOffset(x, SelectionBoxYOffset, vector6.y), instance3.GetSurfaceOffset(num3, SelectionBoxYOffset, vector6.y), Color.white, SelectionLineThickness);
				DebugGraphics.DrawThickLine(instance3.GetSurfaceOffset(x, SelectionBoxYOffset, vector7.y), instance3.GetSurfaceOffset(num3, SelectionBoxYOffset, vector7.y), Color.white, SelectionLineThickness);
				if (num3 >= vector7.x - 0.001f)
				{
					break;
				}
				x = num3;
			}
			float num4 = vector6.y;
			float z = num4;
			while (true)
			{
				num4 += Mathf.Min(vector7.y - num4, 1f - (num4 - Mathf.Floor(num4)));
				DebugGraphics.DrawThickLine(instance3.GetSurfaceOffset(vector6.x, SelectionBoxYOffset, z), instance3.GetSurfaceOffset(vector6.x, SelectionBoxYOffset, num4), Color.white, SelectionLineThickness);
				DebugGraphics.DrawThickLine(instance3.GetSurfaceOffset(vector7.x, SelectionBoxYOffset, z), instance3.GetSurfaceOffset(vector7.x, SelectionBoxYOffset, num4), Color.white, SelectionLineThickness);
				if (num4 >= vector7.y - 0.001f)
				{
					break;
				}
				z = num4;
			}
			DebugGraphics.EndDrawQuads();
		}
		if (DraggingZone)
		{
			TerrainRect terrainRect = new TerrainRect(DragZoneStartTile, DragZoneStartTile).Include(DragZoneFinishTile);
			DrawDottedZone(terrainRect.min, terrainRect.max, paused: false);
		}
		else if (instance.GameCamera.FlyCam && LocalControlledCharacter != null && LocalControlledCharacter.MovementZone != TerrainRect.Invalid && !SentMovementZone)
		{
			DrawDottedZone(LocalControlledCharacter.MovementZone.min, LocalControlledCharacter.MovementZone.max, LocalControlledCharacter.MovementZonePaused);
		}
		if (!LocalWantLockOnTarget || LocalControlledCharacter == null || !(LocalControlledCharacter.EquippedItem is Throwable))
		{
			return;
		}
		using (new UnityProfileMarker(DrawThrowingArcStr))
		{
			GameTerrain instance4 = GameTerrain.Instance;
			Vector3 throwPosition = LocalControlledCharacter.ThrowPosition;
			InjuryLocation injuryLocation;
			Vector3 vector8 = ((LocalTargetObject == null) ? LocalTargetPos : LocalControlledCharacter.GetTargetAimPos(LocalTargetObject, LocalTargetObject.GetBoundingBoxBottom(), visible: true, LocalTargetBodyLocationToAimFor, deterministic: true, out injuryLocation));
			Vector3 vector9 = (LocalThrowBlocked ? LocalThrowHitPos : vector8);
			MathUtil.SafeNormalize(vector9 - throwPosition, LocalControlledCharacter.Forward);
			Vector2 vector10 = MathUtil.SafeNormalize(MathUtil.ToXZ(vector9 - throwPosition), MathUtil.ToXZ(LocalControlledCharacter.Forward));
			float sqrMagnitude = MathUtil.ToXZ(vector9 - throwPosition).sqrMagnitude;
			Vector3 vector11 = MathUtil.ToX0Y(MathUtil.RightNormal(vector10)) * 0.1f;
			float damageRadius = LocalControlledCharacter.EquippedItem.GetPrototype().DamageRadius;
			bool flag = false;
			bool flag2 = false;
			if (damageRadius > 0f && Session.Instance.DifficultySettings.FriendlyFireSplashDamage > 0f)
			{
				bool flag3 = LocalControlledCharacter.EquippedItem is MolotovCocktail;
				bool flag4 = LocalControlledCharacter.EquippedItem is PipeBomb;
				TerrainCoord tileCoordForPos = instance4.GetTileCoordForPos(vector9);
				int num5 = Mathf.CeilToInt(damageRadius);
				instance4.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num5, num5), tileCoordForPos + new TerrainCoord(num5, num5), TempObjects);
				foreach (TileObject tempObject in TempObjects)
				{
					if (tempObject is Character character2)
					{
						if (character2.AliveAndNotZombie && (character2.Pos - vector9).magnitude < damageRadius && !LocalControlledCharacter.IsEnemy(character2))
						{
							flag = true;
							break;
						}
					}
					else if (!flag2 && tempObject.GetCommunityId() != 0 && (!flag3 || tempObject.IsFlammable()) && (!flag4 || !tempObject.IsExplosionProof()) && !LocalControlledCharacter.IsEnemy(tempObject) && tempObject.GetBoundingBox().Intersects(new BoundingSphere(vector9, damageRadius)))
					{
						flag2 = true;
					}
				}
				TempObjects.Clear();
			}
			Color color = (flag ? Color.red : (flag2 ? Color.yellow : Color.white));
			if (LocalThrowBlocked && LocalTargetObject != null)
			{
				color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, color.a);
			}
			DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: true);
			Vector3 vector12 = throwPosition;
			float num6 = 0f;
			while (vector12.y > 0f && num6 < LocalThrowTime - 0.001f && MathUtil.ToXZ(vector12 - throwPosition).sqrMagnitude < sqrMagnitude)
			{
				num6 += 1f / 60f;
				num6 = Math.Min(num6, LocalThrowTime);
				Vector3 posAtTime = BaseThrownProjectile.GetPosAtTime(num6, LocalThrowSpeed, LocalThrowAngle, throwPosition, vector10);
				DebugGraphics.DrawQuad(vector12 - vector11, vector12 + vector11, posAtTime + vector11, posAtTime - vector11, color * new Color(1f, 1f, 1f, Mathf.Clamp01(num6 * 4f)), color * new Color(1f, 1f, 1f, Mathf.Clamp01((num6 + 1f / 60f) * 4f)));
				vector12 = posAtTime;
			}
			if (damageRadius > 0f)
			{
				color *= new Color(1f, 1f, 1f, 0.75f);
				for (int j = 0; j < 32; j++)
				{
					float num7 = (float)j / 32f;
					float num8 = (float)(j + 1) / 32f;
					Vector3 vector13 = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(num7 * (MathF.PI * 2f)));
					Vector3 vector14 = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(num8 * (MathF.PI * 2f)));
					Vector3 p = instance4.ClampPosAboveSurface(vector9 + vector13 * damageRadius, 0.25f, ignoreIce: true, ignoreRoadCamber: true);
					Vector3 p2 = instance4.ClampPosAboveSurface(vector9 + vector14 * damageRadius, 0.25f, ignoreIce: true, ignoreRoadCamber: true);
					Vector3 p3 = instance4.ClampPosAboveSurface(vector9 + vector14 * (damageRadius - 0.1f), 0.25f, ignoreIce: true, ignoreRoadCamber: true);
					Vector3 p4 = instance4.ClampPosAboveSurface(vector9 + vector13 * (damageRadius - 0.1f), 0.25f, ignoreIce: true, ignoreRoadCamber: true);
					DebugGraphics.DrawQuad(p, p2, p3, p4, color);
				}
			}
			DebugGraphics.EndDrawQuads();
		}
	}

	public void DrawDottedZone(TerrainCoord dragZoneStartTile, TerrainCoord dragZoneFinishTile, bool paused)
	{
		GameTerrain instance = GameTerrain.Instance;
		Color col = (paused ? Color.gray : Color.white);
		DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: true);
		Vector2 vertexPosXZ = instance.GetVertexPosXZ(dragZoneStartTile.x, dragZoneStartTile.y);
		Vector2 vertexPosXZ2 = instance.GetVertexPosXZ(dragZoneFinishTile.x + 1, dragZoneFinishTile.y + 1);
		bool flag = true;
		float num = vertexPosXZ.x;
		float x = num;
		while (true)
		{
			num += Mathf.Min(vertexPosXZ2.x - num, 1f - (num - Mathf.Floor(num)));
			if (flag)
			{
				DebugGraphics.DrawThickLine(instance.GetSurfaceOffset(x, SelectionBoxYOffset, vertexPosXZ.y), instance.GetSurfaceOffset(num, SelectionBoxYOffset, vertexPosXZ.y), col, ZoneLineThickness);
				DebugGraphics.DrawThickLine(instance.GetSurfaceOffset(x, SelectionBoxYOffset, vertexPosXZ2.y), instance.GetSurfaceOffset(num, SelectionBoxYOffset, vertexPosXZ2.y), col, ZoneLineThickness);
			}
			if (num >= vertexPosXZ2.x - 0.001f)
			{
				break;
			}
			x = num;
			flag = !flag;
		}
		float num2 = vertexPosXZ.y;
		float z = num2;
		while (true)
		{
			num2 += Mathf.Min(vertexPosXZ2.y - num2, 1f - (num2 - Mathf.Floor(num2)));
			if (flag)
			{
				DebugGraphics.DrawThickLine(instance.GetSurfaceOffset(vertexPosXZ.x, SelectionBoxYOffset, z), instance.GetSurfaceOffset(vertexPosXZ.x, SelectionBoxYOffset, num2), col, ZoneLineThickness);
				DebugGraphics.DrawThickLine(instance.GetSurfaceOffset(vertexPosXZ2.x, SelectionBoxYOffset, z), instance.GetSurfaceOffset(vertexPosXZ2.x, SelectionBoxYOffset, num2), col, ZoneLineThickness);
			}
			if (num2 >= vertexPosXZ2.y - 0.001f)
			{
				break;
			}
			z = num2;
			flag = !flag;
		}
		DebugGraphics.EndDrawQuads();
	}

	public void CheckMovementZone(TileObject obj)
	{
		if (LocalControlledCharacter != null && LocalControlledCharacter.HasMovementZone() && !LocalControlledCharacter.MovementZone.Overlaps(obj.GetTileRect()))
		{
			HudBehaviour.Instance.ShowOutsideZoneMsg(LocalControlledCharacter);
		}
		else
		{
			if (LocalControlledCharacter.Followers == null)
			{
				return;
			}
			foreach (Character follower in LocalControlledCharacter.Followers)
			{
				if (follower != null && follower.HasMovementZone() && !follower.MovementZone.Expand(FollowGoal.OutsideZoneWarningDist).Overlaps(obj.GetTileRect()))
				{
					HudBehaviour.Instance.ShowOutsideZoneMsg(follower);
					break;
				}
			}
		}
	}

	public void CheckMovementZone(TerrainCoord tile)
	{
		if (LocalControlledCharacter != null && LocalControlledCharacter.HasMovementZone() && !LocalControlledCharacter.MovementZone.Contains(tile))
		{
			HudBehaviour.Instance.ShowOutsideZoneMsg(LocalControlledCharacter);
		}
		else
		{
			if (LocalControlledCharacter.Followers == null)
			{
				return;
			}
			foreach (Character follower in LocalControlledCharacter.Followers)
			{
				if (follower != null && follower.HasMovementZone() && !follower.MovementZone.Expand(FollowGoal.OutsideZoneWarningDist).Contains(tile))
				{
					HudBehaviour.Instance.ShowOutsideZoneMsg(follower);
					break;
				}
			}
		}
	}
}

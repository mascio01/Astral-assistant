using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Prop : MultiTileObject
{
	public enum OrientationType
	{
		Deg0,
		Deg90,
		Deg180,
		Deg270,
		Count
	}

	public static Resource<Texture2D> PitTrapIcon;

	public static Resource<Texture2D> GraveIcon;

	public static Resource<Texture2D> BushIcon;

	public static Resource<Texture2D> TreeIcon;

	public static Resource<Texture2D> StumpIcon;

	public static Resource<Material> MudMaterial;

	private static PrefabResource[] Models = new PrefabResource[30]
	{
		new PrefabResource("Prefabs/Props/Gibbet"),
		new PrefabResource("Prefabs/Props/PileOfSkulls"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_barricade_01"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_barricade_02"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_spike_barricade_01"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_spike_barricade_02"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_spike_wall_01"),
		new PrefabResource("Prefabs/Props/Wooden Barricades Pack/Wooden_spike_wall_02"),
		new PrefabResource("Prefabs/Props/Mailboxes/Mailbox1"),
		new PrefabResource("Prefabs/Props/Mailboxes/Mailbox1_Closed"),
		new PrefabResource("Prefabs/Props/Mailboxes/Mailbox2"),
		new PrefabResource("Prefabs/Props/Mailboxes/Mailbox2_Closed"),
		new PrefabResource("Prefabs/Props/Icemachine"),
		new PrefabResource("Prefabs/Props/TelegraphPole/TelegraphPole1"),
		new PrefabResource("Prefabs/Props/TelegraphPole/TelegraphPole2"),
		new PrefabResource("Prefabs/Props/TelegraphPole/TelegraphPole3"),
		new PrefabResource("Prefabs/Props/StreetLamps/StreetLamp2_Tall (Black)"),
		new PrefabResource("Prefabs/Props/StreetLamps/StreetLamp2_Tall (Concrete)"),
		new PrefabResource("Prefabs/Props/StreetLamps/StreetLamp2_TallDouble (Black)"),
		new PrefabResource("Prefabs/Props/StreetLamps/StreetLamp2_TallDouble (Concrete)"),
		new PrefabResource("Prefabs/Props/RoadSigns/StopSign"),
		new PrefabResource("Prefabs/Props/RoadSigns/DeerSign"),
		new PrefabResource("Prefabs/Props/RoadSigns/SeatbeltSign"),
		new PrefabResource("Prefabs/Props/RoadSigns/RestAreaSign"),
		new PrefabResource("Prefabs/Props/RoadSigns/SpeedLimit50Sign"),
		new PrefabResource("Prefabs/Props/Barricades/kw_barrier_police"),
		new PrefabResource("Prefabs/Props/Barricades/kw_barrier_striped"),
		new PrefabResource("Prefabs/Props/Barricades/kw_barriers_cone"),
		new PrefabResource("Prefabs/Props/Junction_Box"),
		new PrefabResource("Prefabs/Props/Gate_P1")
	};

	private static PrefabResource OverheadWirePrefab = new PrefabResource("Prefabs/Props/OverheadWire");

	private static PrefabResource[] BoardedWindowPrefabs = new PrefabResource[4]
	{
		new PrefabResource("Prefabs/BoardedWindows/BoardedWindow1"),
		new PrefabResource("Prefabs/BoardedWindows/BoardedWindow2"),
		new PrefabResource("Prefabs/BoardedWindows/BoardedWindow3"),
		new PrefabResource("Prefabs/BoardedWindows/BoardedWindow4")
	};

	public static VehicleSettingsBehaviour VehicleSettings;

	public static string[] OrientationTypeNames = StringUtil.GetEnumNames<OrientationType>();

	public GameObject UnityObj;

	public GameObject UnityConstructionCordon;

	public GameObject UnityBurningEffect;

	public GameObject UnityDemolitionSmokeEffect;

	public GameObject UnityWires;

	public List<GameObject> UnityWindows;

	public PropPrototype Prototype;

	public TerrainCoord Tile;

	public OrientationType Orientation;

	public Community Community;

	public Town Town;

	public float CaptureAmount;

	public bool Investigated;

	public bool Destroyed;

	protected float DemolitionSpeed;

	protected float DemolitionTransition;

	public Vector3 DemolitionShakeOffset;

	public List<StructureDamagePoint> DamagePoints = new List<StructureDamagePoint>();

	protected float Damage;

	protected float LiquidAmount;

	public string CustomName;

	public StringStatus PropNameVerified;

	public int Variation = -1;

	public int MaterialVariation = -1;

	public int ColorVariation = -1;

	public int ColorVariation2 = -1;

	public int ColorVariation3 = -1;

	public int ColorVariation4 = -1;

	public int TerrainModifiedPatchId;

	public bool MossCleanedAway;

	public bool ForceInvulnerable;

	public List<Prop> ConnectedWires;

	public UnderConstructionInfo UnderConstructionInfo;

	public EquipmentContainer Inventory = new EquipmentContainer();

	public List<StoragePolicy> StoragePolicies;

	public static int FlattenTerrainBorder = 2;

	private const int SoftMaxDamagePoints = 16;

	private static string TransparentStr = "Transparent";

	private static string RaycastAgainstModelStr = "RaycastAgainstModel";

	private static string IntersectsOBBStr = "IntersectsOBB";

	private static string IntersectsBoxStr = "IntersectsBoxStr";

	private static string IntersectsSphereStr = "IntersectsSphere";

	private static string IntersectsMeshStr = "IntersectsMesh";

	protected int IconIndex;

	private static float ExplosionImpactDamageMaxSize = 1f;

	private static float BurnDamageMaxSize = 0.25f;

	public static float ProjectileImpactFactor = 1000f;

	public static float VehicleImpactFactor = 0.1f;

	public static float VehicleCollisionDamageFactor = 0.0005f;

	public static float MinSpeedMphForDamagingFriendly = 20f;

	public static float MaxMassForDamageToVehicleCalc = 200f;

	protected DeletionState _deleted;

	private static float DemolitionAcceleration = 0.5f;

	private static float DemolitionShakeAmount = 0.01f;

	public static PrefabResource ConstructionSiteCordonModel = new PrefabResource("Prefabs/Buildings/Cordon");

	private const string SignStr = "Sign";

	private const string PosterStr = "Poster";

	private static string CONSTRUCTION_PROGRESS_ENABLED = "CONSTRUCTION_PROGRESS_ENABLED";

	public override int NameHash
	{
		get
		{
			if (Prototype == null)
			{
				return base.NameHash;
			}
			return Prototype.NameHash;
		}
	}

	public override int UnityModelIndexFromPrototype
	{
		get
		{
			if (Variation < 0 || Variation >= Prototype.Prefabs.Count)
			{
				return MathUtil.RandomInt(Id, Prototype.Prefabs.Count);
			}
			return Variation;
		}
	}

	public override CoverType CoverType
	{
		get
		{
			if (Prototype == null)
			{
				return base.CoverType;
			}
			return Prototype.CoverType;
		}
	}

	public override string Category
	{
		get
		{
			if (Prototype == null)
			{
				return string.Empty;
			}
			return Prototype.Category;
		}
	}

	public virtual TerrainCoord ExtentsMin
	{
		get
		{
			if (Prototype == null)
			{
				return new TerrainCoord(0, 0);
			}
			return Prototype.ExtentsMin;
		}
	}

	public virtual TerrainCoord ExtentsMax
	{
		get
		{
			if (Prototype == null)
			{
				return new TerrainCoord(0, 0);
			}
			return Prototype.ExtentsMax;
		}
	}

	public override Vector3 Pos => World.Translation();

	public override Vector2 PosXZ => MathUtil.ToXZ(World.Translation());

	public override Color32 MapColor
	{
		get
		{
			if (!(GetMaxInventoryWeight() > 0f))
			{
				return GameTerrain.MinimapSettings.BoulderCol;
			}
			return GameTerrain.MinimapSettings.PropCol;
		}
	}

	public override bool Deleted => _deleted == DeletionState.Deleted;

	public virtual Vector3 ModelOffset
	{
		get
		{
			if (Prototype == null || Prototype.ModelOffset.Count <= 0)
			{
				return Vector3.zero;
			}
			return Prototype.ModelOffset[Math.Min(UnityModelIndexFromPrototype, Prototype.ModelOffset.Count - 1)];
		}
	}

	public static void LoadContent()
	{
		PitTrapIcon = new Resource<Texture2D>("Textures\\BuildingIcons\\PitTrapIcon");
		GraveIcon = new Resource<Texture2D>("Textures\\BuildingIcons\\Grave");
		BushIcon = new Resource<Texture2D>("Textures\\BuildingIcons\\Bush");
		TreeIcon = new Resource<Texture2D>("Textures\\BuildingIcons\\Deciduous");
		StumpIcon = new Resource<Texture2D>("Textures\\BuildingIcons\\Stump");
		MudMaterial = new Resource<Material>("Materials\\Mud");
		VehicleSettings = GameObject.Find("Vehicle Settings").GetComponent<VehicleSettingsBehaviour>();
	}

	public static OrientationType GetClosestOrientationTypeToAngle(float angle)
	{
		return (OrientationType)((int)((angle + MathF.PI * 2f + MathF.PI / 4f) / (MathF.PI / 2f)) % 4);
	}

	public static bool IsDiagonalOrientation(float angle)
	{
		return MathUtil.AngleDiff(angle, GetAngleFromOrientationType(GetClosestOrientationTypeToAngle(angle))) >= MathF.PI / 8f;
	}

	public static float GetAngleFromOrientationType(OrientationType orientation)
	{
		return (float)orientation * 90f * (MathF.PI / 180f);
	}

	public static TerrainCoord GetDirFromOrientationType(OrientationType orientation)
	{
		return orientation switch
		{
			OrientationType.Deg0 => new TerrainCoord(0, 1), 
			OrientationType.Deg90 => new TerrainCoord(1, 0), 
			OrientationType.Deg180 => new TerrainCoord(0, -1), 
			OrientationType.Deg270 => new TerrainCoord(-1, 0), 
			_ => TerrainCoord.Zero, 
		};
	}

	public static Prop Spawn(BaseObjectType objectType, TerrainCoord tile, OrientationType orientation)
	{
		Prop obj = (Prop)BaseObjectManager.Create(objectType);
		obj.Tile = tile;
		obj.Orientation = orientation;
		obj.OnSpawn();
		return obj;
	}

	public static Prop Spawn(PropPrototype proto, TerrainCoord tile, OrientationType orientation)
	{
		Prop obj = (Prop)BaseObjectManager.Create(proto.TypeName);
		obj.Prototype = proto;
		obj.Tile = tile;
		obj.Orientation = orientation;
		obj.OnSpawn();
		return obj;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Prop;
	}

	public override void SetPropPrototype(PropPrototype propPrototype)
	{
		Prototype = propPrototype;
	}

	public override PropPrototype GetPropPrototype()
	{
		return Prototype;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		if (Prototype == null)
		{
			return base.GetGender(language);
		}
		return Prototype.GetGenderInLanguage(language);
	}

	public virtual bool SupportsVariation()
	{
		return true;
	}

	public override PrefabResource GetUnityModel()
	{
		if (Investigated && Prototype != null && Prototype.OpenedPrefabs != null && Prototype.OpenedPrefabs.Count > 0)
		{
			return Prototype.OpenedPrefabs[UnityModelIndexFromPrototype % Prototype.OpenedPrefabs.Count];
		}
		if (Prototype == null || Prototype.Prefabs.Count <= 0)
		{
			return base.GetUnityModel();
		}
		return Prototype.Prefabs[UnityModelIndexFromPrototype];
	}

	public override bool IsUnityObjectAlwaysActive()
	{
		if (Prototype == null)
		{
			return base.IsUnityObjectAlwaysActive();
		}
		return Prototype.VisibleFromFarAway;
	}

	public override float GetMaxInventoryWeight()
	{
		if (Prototype == null)
		{
			return base.GetMaxInventoryWeight();
		}
		return Prototype.MaxInventoryWeight;
	}

	public override bool CanSetPropName()
	{
		return GetMaxInventoryWeight() > 0f;
	}

	public override bool IsLockable()
	{
		if (Prototype == null)
		{
			return base.IsLockable();
		}
		return Prototype.Lockable;
	}

	public int GetMaterialVariation()
	{
		if (Prototype.GetNumMaterialVariations() <= 0)
		{
			return -1;
		}
		if (MaterialVariation != -1)
		{
			return MaterialVariation % Prototype.MaterialVariationNames.Count;
		}
		return MathUtil.RandomInt(Id + 46920, Prototype.MaterialVariationNames.Count);
	}

	public int GetColorVariation()
	{
		if (Prototype.GetNumColorVariations() <= 0)
		{
			return -1;
		}
		if (ColorVariation != -1)
		{
			return ColorVariation % Prototype.ColorVariations.Length;
		}
		return MathUtil.RandomInt(Id + 54562, Prototype.ColorVariations.Length);
	}

	public int GetColorVariation2()
	{
		if (Prototype.GetNumColorVariations2() <= 0)
		{
			return -1;
		}
		if (ColorVariation2 != -1)
		{
			return ColorVariation2 % Prototype.ColorVariations2.Length;
		}
		return MathUtil.RandomInt(Id + 14897, Prototype.ColorVariations2.Length);
	}

	public int GetColorVariation3()
	{
		if (Prototype.GetNumColorVariations3() <= 0)
		{
			return -1;
		}
		if (ColorVariation3 != -1)
		{
			return ColorVariation3 % Prototype.ColorVariations3.Length;
		}
		return MathUtil.RandomInt(Id + 94892, Prototype.ColorVariations3.Length);
	}

	public int GetColorVariation4()
	{
		if (Prototype.GetNumColorVariations4() <= 0)
		{
			return -1;
		}
		if (ColorVariation4 != -1)
		{
			return ColorVariation4 % Prototype.ColorVariations4.Length;
		}
		return MathUtil.RandomInt(Id + 36987, Prototype.ColorVariations4.Length);
	}

	public override string GetLootLocation()
	{
		if (Prototype != null && Prototype.LootLocation != null)
		{
			return Prototype.LootLocation;
		}
		return base.GetLootLocation();
	}

	public override ActionAnim GetTakeAnim()
	{
		if (Prototype == null)
		{
			return base.GetTakeAnim();
		}
		if (!Prototype.CrouchToLoot)
		{
			return ActionAnim.Scavenge;
		}
		return ActionAnim.ScavengeCorpse;
	}

	public override EquipmentPrototype GetRepairResourceType()
	{
		if (Prototype == null)
		{
			return base.GetRepairResourceType();
		}
		return Prototype.RepairResourceProto;
	}

	public override float GetRepairResourceNeeded()
	{
		if (Prototype == null)
		{
			return base.GetRepairResourceNeeded();
		}
		return Prototype.RepairResourceNeeded;
	}

	public override int GetRepairSkillNeeded()
	{
		if (Prototype == null)
		{
			return base.GetRepairSkillNeeded();
		}
		return Prototype.RepairSkillNeeded;
	}

	public override EquipmentPrototype GetCaptureResourceType()
	{
		if (Prototype == null)
		{
			return base.GetCaptureResourceType();
		}
		return Prototype.CaptureResourceProto;
	}

	public override float GetCaptureResourceNeeded()
	{
		if (Prototype == null)
		{
			return base.GetCaptureResourceNeeded();
		}
		return Prototype.CaptureResourceNeeded;
	}

	public override int GetCaptureSkillNeeded()
	{
		if (Prototype == null)
		{
			return base.GetCaptureSkillNeeded();
		}
		return Prototype.CaptureSkillNeeded;
	}

	public override bool IsForcedInvulnerable()
	{
		return ForceInvulnerable;
	}

	public override void SetForceInvulnerable(bool v)
	{
		ForceInvulnerable = v;
	}

	public override Flammability GetFlammability()
	{
		if (!ForceInvulnerable)
		{
			if (Prototype == null)
			{
				return base.GetFlammability();
			}
			return Prototype.Flammability;
		}
		return Flammability.Invulnerable;
	}

	public override bool IsLooterProp()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.IsLooterProp;
	}

	public override ImpactSusceptibility GetImpactSusceptibility()
	{
		if (!ForceInvulnerable)
		{
			if (Prototype == null)
			{
				return base.GetImpactSusceptibility();
			}
			return Prototype.ImpactSusceptibility;
		}
		return ImpactSusceptibility.CompletelyInvulnerable;
	}

	public override BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		if (Prototype == null)
		{
			return BulletHitEffect.Ricochet;
		}
		return Prototype.BulletHitEffect;
	}

	public override bool WantDebrisOnDemolition()
	{
		if (Prototype == null)
		{
			return base.WantDebrisOnDemolition();
		}
		return Prototype.WantDebrisOnDemolition;
	}

	public override bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.CanBeClearedForBuilding;
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.GrabbableEquipmentPrototype;
	}

	public virtual bool CanSetStoragePolicy()
	{
		return GetMaxInventoryWeight() > 0f;
	}

	public virtual LiquidPrototype GetLiquidType()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.LiquidPrototype;
	}

	public virtual CursorActionDisabledReason CanBeFilled()
	{
		return CursorActionDisabledReason.Disabled;
	}

	public virtual bool CanBeEmptied()
	{
		return false;
	}

	public virtual float GetLiquidCapacity()
	{
		if (Prototype == null)
		{
			return 500f;
		}
		return Prototype.LiquidCapacity;
	}

	public float GetLiquidAmount()
	{
		return LiquidAmount;
	}

	public void SetLiquidAmount(float liquidAmount)
	{
		LiquidAmount = liquidAmount;
	}

	public void FillWithLiquid(float liquidAmount)
	{
		LiquidAmount = Mathf.Min(LiquidAmount + liquidAmount, GetLiquidCapacity());
	}

	public float ExtractLiquid(float desiredAmount)
	{
		float num = Math.Min(desiredAmount, LiquidAmount);
		LiquidAmount = Mathf.Max(0f, LiquidAmount - num);
		return num;
	}

	public override Vector2[] GetConnectors()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.Connectors;
	}

	public virtual bool CanRepairArmor()
	{
		return false;
	}

	public override void SetupPropIconCam(Camera cam)
	{
		base.SetupPropIconCam(cam);
		if (Prototype != null && Prototype.IconScale > 0f)
		{
			cam.orthographicSize /= Prototype.IconScale;
		}
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.PropManager.Add(this);
		UpdateWorldTransformAndBounds();
		AddToTerrain();
		if (IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		if (IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
		}
		if (_deleted == DeletionState.WantDelete || Destroyed || IsBurning())
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		if (UnderConstructionInfo != null && Community != null && !Community.UnderConstructionBuildings.Contains(this))
		{
			Community.UnderConstructionBuildings.Add(this);
		}
		if (CanBeRepaired() && Community != null && !Community.NeedsRepair.Contains(this))
		{
			Community.NeedsRepair.Add(this);
		}
		Session.Instance.ClearGrassForBuilding(this);
	}

	public override void Delete()
	{
		ClearAllConnectedWires();
		if (CanBeRepaired() && Community != null)
		{
			Community.NeedsRepair.Remove(this);
		}
		if (UnderConstructionInfo != null && Community != null)
		{
			Community.UnderConstructionBuildings.Remove(this);
		}
		if (Community != null)
		{
			foreach (Character member in Community.Members)
			{
				if (member.IsBuildingSomething() == this)
				{
					member.OnThingIWasBuildingGotDeleted(this);
				}
			}
			Community.RemoveBuilding(this);
		}
		if (IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, Tile);
		}
		if (IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
		RemoveFromTerrain();
		Inventory.DeleteAll(this, carrierBeingDeleted: true);
		Session.Instance.PropManager.Remove(this);
		_deleted = DeletionState.Deleted;
		base.Delete();
		if (TerrainModifiedPatchId != 0 && !Destroyed)
		{
			GameTerrain.Instance.RemoveModifiedPatch(TerrainModifiedPatchId);
			TerrainModifiedPatchId = 0;
		}
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
		UpdateNearbyPitTraps();
	}

	public void SetVariation(int v)
	{
		Variation = v;
		OnUnityModelChanged();
	}

	public void SetMaterialVariation(int v)
	{
		MaterialVariation = v;
		OnUnityModelChanged();
	}

	public override void OnUnityModelChanged()
	{
		bool num = HasUnityObject();
		bool flag = IsUnityObjectActive();
		if (flag)
		{
			UnityDeactivate();
		}
		if (num)
		{
			UnityDelete();
		}
		if (num)
		{
			UnityInit();
		}
		if (flag)
		{
			UnityActivate();
		}
		UpdateWorldTransformAndBounds();
		IconIndex = -1;
	}

	public int SetTerrainModifiedPatch(TerrainModificationType type)
	{
		GameTerrain instance = GameTerrain.Instance;
		CalcMinMaxTile(Tile, Orientation, out var minTile, out var maxTile);
		int flattenTerrainBorder = FlattenTerrainBorder;
		ModifiedPatch patch = new ModifiedPatch
		{
			Type = type
		};
		if (Id == 0)
		{
			AssignId(BaseObjectManager.Instance.Assign(this));
		}
		patch.BuildingId = Id;
		patch.MinTile = minTile - new TerrainCoord(flattenTerrainBorder, flattenTerrainBorder);
		patch.MaxTile = maxTile + new TerrainCoord(flattenTerrainBorder, flattenTerrainBorder);
		patch.FlattenBorder = flattenTerrainBorder;
		if (patch.MaxTile.x > patch.MinTile.x && patch.MaxTile.y > patch.MinTile.y)
		{
			float num = 0f;
			float num2 = 0f;
			float val = float.MinValue;
			float num3 = float.MinValue;
			for (int i = Math.Max(patch.MinTile.x, minTile.x); i <= Math.Min(patch.MaxTile.x, maxTile.x) + 1; i++)
			{
				for (int j = Math.Max(patch.MinTile.y, minTile.y); j <= Math.Min(patch.MaxTile.y, maxTile.y) + 1; j++)
				{
					float num4 = GameTerrain.Vertices[MathUtil.Clamp(i, 0, instance.Size), MathUtil.Clamp(j, 0, instance.Size)];
					num += num4;
					num2 += 1f;
					if (!instance.Generating && !Session.Instance.Editor && instance.IsVertexRoadOrRiver(i, j))
					{
						val = ((!(instance.GetAmountOnPath(instance.GetVertexPosXZ(i, j), out var H, wantRiver: false) < 1f)) ? Math.Max(val, GameTerrain.OriginalVertices[MathUtil.Clamp(i, 0, instance.Size), MathUtil.Clamp(j, 0, instance.Size)]) : Math.Max(val, H));
					}
					if (instance.IsBuildingThatFlattensTerrainOnVertex(new TerrainCoord(i, j), Id, onThread: false))
					{
						num3 = Math.Max(num3, num4);
					}
				}
			}
			if (num2 > 0f)
			{
				patch.FlattenHeight = num / num2;
				if (num3 > float.MinValue)
				{
					patch.FlattenHeight = num3;
				}
				patch.FlattenHeight = Math.Max(val, patch.FlattenHeight);
				for (int k = minTile.x; k <= maxTile.x; k++)
				{
					for (int l = minTile.y; l <= maxTile.y; l++)
					{
						if (!instance.IsTileOutsideBounds(k, l) && !new TerrainCoord(k, l).IsWithinBounds(patch.MinTile, patch.MaxTile))
						{
							if (instance.IsTilePit(k, l))
							{
								patch.FlattenHeight = Math.Min(patch.FlattenHeight, instance.GetOriginalTileMinHeight(k, l));
							}
							else
							{
								patch.FlattenHeight = Math.Min(patch.FlattenHeight, instance.GetTileMinHeight(k, l));
							}
						}
					}
				}
				return instance.AddModifiedPatch(patch);
			}
		}
		return 0;
	}

	public override void OnSpawn()
	{
		if (WantFlattenTerrain())
		{
			TerrainModifiedPatchId = SetTerrainModifiedPatch(TerrainModificationType.Flatten);
		}
		base.OnSpawn();
		if (!Session.Instance.Editor && GetCommunityId() != 0)
		{
			GameTerrain.Instance.BuildCommunityAreaOwner(GetTileRect());
		}
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			GameTerrain.Instance.BuildMinimap(GetMinTile(), GetMaxTile());
		}
		if (Prototype != null && Prototype.ChildProps != null)
		{
			for (int i = 0; i < Prototype.ChildProps.Length; i++)
			{
				ChildPropDef childPropDef = Prototype.ChildProps[i];
				InvisibleWall.Spawn(Tile + RotateByOrientation(childPropDef.Offset, Orientation), Orientation, childPropDef.ExtentsMin, childPropDef.ExtentsMax);
			}
		}
	}

	public override void ReflectEarly(Reflector reflector)
	{
		base.ReflectEarly(reflector);
		if (reflector.Version < 216)
		{
			Prototype = GameImpl.Instance.FindPropPrototypeByName(GetBaseObjectType().ToString());
		}
		else if (reflector.Version >= 221)
		{
			reflector.Add(ref Prototype);
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version >= 216 && reflector.Version < 221)
		{
			reflector.Add(ref Prototype);
		}
		reflector.AddAfter(ref Variation, 217);
		reflector.AddAfter(ref MaterialVariation, 445);
		reflector.AddAfter(ref ColorVariation, 445);
		reflector.AddAfter(ref ColorVariation2, 445);
		reflector.AddAfter(ref ColorVariation3, 445);
		reflector.AddAfter(ref ColorVariation4, 446);
		reflector.AddAfter(ref TerrainModifiedPatchId, 426);
		reflector.AddAfter(ref MossCleanedAway, 465);
		reflector.AddAfter(ref ForceInvulnerable, 519);
		if (reflector.Version >= 447)
		{
			reflector.AddGameObjectRefList(ref ConnectedWires);
		}
		reflector.Add(ref Tile);
		reflector.Add(ref Orientation);
		reflector.Add(Inventory);
		if (reflector.Version >= 353)
		{
			reflector.AddListThatCanBeNull(ref StoragePolicies);
		}
		if (reflector.IsDeserialising && reflector.Version < 147)
		{
			foreach (Equipment content in Inventory.Contents)
			{
				content.InventoryOwner = this;
			}
		}
		reflector.Add(ref Investigated);
		bool value = UnderConstructionInfo != null;
		reflector.Add(ref value);
		if (value)
		{
			if (reflector.IsDeserialising)
			{
				UnderConstructionInfo = new UnderConstructionInfo();
			}
			UnderConstructionInfo.Reflect(reflector, this);
		}
		reflector.Add(ref DamagePoints);
		if (reflector.IsDeserialising && DamagePoints.Count > 16)
		{
			DamagePoints.RemoveRange(16, DamagePoints.Count - 16);
		}
		reflector.Add(ref Damage);
		reflector.AddAfter(ref LiquidAmount, 468);
		reflector.Add(ref CaptureAmount);
		reflector.Add(ref Destroyed);
		reflector.Add(ref DemolitionSpeed);
		reflector.Add(ref DemolitionTransition);
		reflector.Add(ref DemolitionShakeOffset);
		reflector.Add(ref _deleted);
		reflector.Add(ref Community);
		reflector.Add(ref Town);
		reflector.AddAfter(ref CustomName, 129);
		if (!reflector.IsDoingNetworkChecksum)
		{
			reflector.AddAfter(ref PropNameVerified, 595);
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (!string.IsNullOrEmpty(CustomName))
		{
			sb.Append(CustomName);
		}
		else
		{
			base.BuildDisplayName(sb, noStrangers, englishOnly);
		}
	}

	public override void BuildSubDisplayName(StringBuilder sb)
	{
		base.BuildSubDisplayName(sb);
		if (Town != null && sb.Length == 0 && (Town.Visited || InfoScreen.AllowViewInfoOnAnyone || Session.Instance.Editor))
		{
			Town.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
		}
	}

	public void CalcMinMaxTile(TerrainCoord tile, OrientationType orientation, out TerrainCoord minTile, out TerrainCoord maxTile)
	{
		switch (orientation)
		{
		default:
			minTile = ExtentsMin;
			maxTile = ExtentsMax;
			break;
		case OrientationType.Deg90:
			minTile = new TerrainCoord(ExtentsMin.y, -ExtentsMax.x);
			maxTile = new TerrainCoord(ExtentsMax.y, -ExtentsMin.x);
			break;
		case OrientationType.Deg180:
			minTile = -ExtentsMax;
			maxTile = -ExtentsMin;
			break;
		case OrientationType.Deg270:
			minTile = new TerrainCoord(-ExtentsMax.y, ExtentsMin.x);
			maxTile = new TerrainCoord(-ExtentsMin.y, ExtentsMax.x);
			break;
		}
		minTile += tile;
		maxTile += tile;
	}

	public static TerrainCoord RotateByOrientation(TerrainCoord tile, OrientationType orientation)
	{
		return orientation switch
		{
			OrientationType.Deg90 => new TerrainCoord(tile.y, -tile.x), 
			OrientationType.Deg180 => -tile, 
			OrientationType.Deg270 => new TerrainCoord(-tile.y, tile.x), 
			_ => tile, 
		};
	}

	protected override void OnMinMaxTilesChanged(TerrainCoord oldMinTile, TerrainCoord oldMaxTile, TerrainCoord newMinTile, TerrainCoord newMaxTile)
	{
		GameTerrain.Instance.BuildMinimap(oldMinTile, oldMaxTile);
		GameTerrain.Instance.BuildMinimap(newMinTile, newMaxTile);
	}

	protected virtual Vector3 CalcWorldPos()
	{
		GameTerrain instance = GameTerrain.Instance;
		float num = float.MaxValue;
		CalcMinMaxTile(Tile, Orientation, out var minTile, out var maxTile);
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				num = ((instance.IsTileOutsideBounds(i, j) || !instance.IsTilePit(i, j)) ? Math.Min(num, instance.IsTileOutsideBounds(i, j) ? 0f : instance.GetTileMinHeight(i, j)) : Math.Min(num, instance.GetOriginalTileMinHeight(i, j)));
			}
		}
		Vector3 result = MathUtil.ToX0Y(instance.GetTileCentreXZ(Tile));
		result.y = num;
		if (Destroyed && !(this is Gate))
		{
			result += new Vector3(DemolitionShakeOffset.x, DemolitionShakeOffset.y - DemolitionTransition, DemolitionShakeOffset.z);
		}
		return result;
	}

	public virtual float CalcBoundingBoxHeight()
	{
		float result = 0f;
		PrefabResource unityModel = GetUnityModel();
		if (unityModel != null && unityModel.GetAsset() != null)
		{
			result = CalcTransformedBounds(unityModel.IdentityBounds, GetCustomModelTransform()).max.y - World.Translation().y;
		}
		else if (unityModel != null)
		{
			Debug.LogWarning(GetDisplayNameString() + " prefab not loaded: " + unityModel.GetPath());
		}
		return result;
	}

	public virtual void UpdateWorldTransformAndBounds()
	{
		World = MathUtil.CreateTranslation(CalcWorldPos()) * MathUtil.CreateRotationY(GetFacingAngleRad());
		float y = ((UnderConstructionInfo == null || UnderConstructionInfo.ShowHalfBuiltModel()) ? CalcBoundingBoxHeight() : UnderConstructionInfo.UnderConstructionHeight);
		Vector3 lhs = World.MultiplyPoint(new Vector3((float)ExtentsMin.x - 0.499f, 0f, (float)ExtentsMin.y - 0.499f));
		Vector3 rhs = World.MultiplyPoint(new Vector3((float)ExtentsMax.x + 0.499f, y, (float)ExtentsMax.y + 0.499f));
		SetBoundingBox(MathUtil.CreateBoundsMinMax(Vector3.Min(lhs, rhs), Vector3.Max(lhs, rhs)));
		UpdateUnityTransform();
	}

	public override void OnTerrainHeightChanged()
	{
		UpdateWorldTransformAndBounds();
	}

	public override int GetCommunityId()
	{
		if (Community == null)
		{
			return 0;
		}
		return Community.Id;
	}

	public override Community GetCommunity()
	{
		return Community;
	}

	public override void SetCommunity(Community community)
	{
		if (!Session.Instance.Editor && InTerrain && Community != null)
		{
			Community.AddConstructionRecord(Prototype, Tile, Orientation);
		}
		if (community != null)
		{
			community.AddBuilding(this);
		}
		else if (Community != null)
		{
			Community.RemoveBuilding(this);
		}
		if (!Session.Instance.Editor && InTerrain && GetBaseObjectType() != BaseObjectType.EnterableVehicle)
		{
			GameTerrain.Instance.BuildCommunityAreaOwner(GetTileRect());
		}
		if (InTerrain && IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
	}

	public void SetTown(Town town)
	{
		if (town != null)
		{
			town.AddBuilding(this);
		}
		else if (Town != null)
		{
			Town.RemoveBuilding(this);
		}
	}

	public float GetFacingAngleRad()
	{
		return MathF.PI / 180f * (float)Orientation * 90f;
	}

	public void RotateClockwise()
	{
		SetOrientationType((OrientationType)((int)(Orientation + 1) % 4));
	}

	public void RotateAnticlockwise()
	{
		SetOrientationType((OrientationType)((int)(Orientation + 4 - 1) % 4));
	}

	public override void SetOrientationType(OrientationType orientation)
	{
		Orientation = orientation;
		UpdateWorldTransformAndBounds();
		UnityReInitConnectedWires();
	}

	public override OrientationType GetOrientationType()
	{
		return Orientation;
	}

	public void BuildAngleString(ref StringBuilder value)
	{
		value.Append(OrientationTypeNames[(int)Orientation]);
	}

	public override int GetTileX()
	{
		return Tile.x;
	}

	public override int GetTileY()
	{
		return Tile.y;
	}

	public override TerrainCoord GetTile()
	{
		return Tile;
	}

	public override void SetTileX(int x)
	{
		SetTile(new TerrainCoord(x, Tile.y));
	}

	public override void SetTileY(int y)
	{
		SetTile(new TerrainCoord(Tile.x, y));
	}

	public override void SetTile(TerrainCoord tile)
	{
		if (IsBurning() && IsAuthoritative())
		{
			GameTerrain.Instance.BurningMapWho.OnMoved(this, Tile, tile);
		}
		if (InTerrain && IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
		Tile = tile;
		UpdateWorldTransformAndBounds();
		UnityReInitConnectedWires();
		if (InTerrain && IsImpassableProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
	}

	public override bool IsTargetable()
	{
		Session instance = Session.Instance;
		if (Destroyed)
		{
			return false;
		}
		if (instance.Editor || PropEditor.AllowTargetingAllProps)
		{
			return true;
		}
		Character localControlledCharacter = instance.Hud.LocalControlledCharacter;
		if (localControlledCharacter != null && localControlledCharacter.EquippedItem is Toolbox)
		{
			if (localControlledCharacter.Community != null && localControlledCharacter.Community == Community)
			{
				return true;
			}
			if (Prototype != null && Prototype.CanBeDemolished)
			{
				return true;
			}
		}
		if (GetMaxInventoryWeight() != 0f || UnderConstructionInfo != null || this is Building)
		{
			if (!FogOfWar.DebugFogOfWarEnabled)
			{
				return true;
			}
			return GameTerrain.Instance.FogOfWar.IsAnyTileInRectExplored(MinTile, MaxTile);
		}
		return false;
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if (IsImpassableProp())
		{
			if ((options & 0x200) != 0 && CoverType == CoverType.WaistHigh)
			{
				return false;
			}
			if ((options & 0x10) != 0 && IsFlammable() && !requester.IsInMyCommunityOrAlly(this))
			{
				return false;
			}
			if ((options & 0x20) != 0 && !IsExplosionProof() && !requester.IsInMyCommunityOrAlly(this))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if (Destroyed)
		{
			return null;
		}
		if ((flags & 4) != 0 && GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
		{
			return distance;
		}
		if ((flags & 8) != 0)
		{
			Bounds boundingBox = GetBoundingBox();
			if (GetOpenTransition() > 0f)
			{
				boundingBox.Expand(2f);
			}
			else
			{
				boundingBox.Expand(1f);
			}
			if (UnderConstructionInfo != null)
			{
				return null;
			}
			if (boundingBox.IntersectRay(ray, out var distance2) && distance2 < length)
			{
				return RaycastAgainstModel(GetUnityModel(), GetCustomModelTransform(), ray, length, 0, out normal, GetOpenTransition(), (flags & 0x1000) != 0);
			}
		}
		return null;
	}

	public override float? RaycastThreadSafe(Ray ray, float length, AStarMoveableObstacle obstacle, TerrainCoord tile, byte modelIndex, bool includeWireFences)
	{
		if (obstacle.PropModel != null && obstacle.PropBoundingBox.IntersectRay(ray, out var _))
		{
			Vector3 hitNormal = Vector3.zero;
			return RaycastAgainstModel(obstacle.PropModel, obstacle.PropWorldMatrix, ray, length, 0, out hitNormal, (obstacle.GateState == GateState.Open) ? 1f : 0f, includeWireFences);
		}
		return null;
	}

	public static float? RaycastAgainstModel(PrefabResource res, Matrix4x4 rootTransform, Ray ray, float length, int recursionDepth, out Vector3 hitNormal, float gateOpenTransition, bool includeWireFences)
	{
		using (new UnityProfileMarker(RaycastAgainstModelStr))
		{
			float? result = null;
			hitNormal = Vector3.zero;
			if (res != null)
			{
				using (new UnityProfileMarker(IntersectsOBBStr))
				{
					if (!ray.IntersectsOBB(ref rootTransform, res.IdentityBounds, out var dist) || dist >= length)
					{
						return result;
					}
				}
				foreach (Collidable collidable in res.Collidables)
				{
					if (!includeWireFences && collidable.Name.Contains(TransparentStr))
					{
						continue;
					}
					Matrix4x4 matrix4x = rootTransform * collidable.WorldFromLocalTrans;
					float childAngle = Gate.GetChildAngle(collidable.Name, gateOpenTransition);
					matrix4x = MathUtil.CreateTranslation(matrix4x.Translation()) * MathUtil.CreateRotationY(childAngle) * MathUtil.CreateTranslation(-matrix4x.Translation()) * matrix4x;
					Matrix4x4 inverse = matrix4x.inverse;
					Ray ray2 = ray;
					ray2.origin = inverse.MultiplyPoint(ray2.origin);
					Vector3 vector = inverse.MultiplyVector(ray2.direction * length);
					length = vector.magnitude;
					if (length < 1E-06f)
					{
						return null;
					}
					ray2.direction = vector / length;
					float distance = float.MaxValue;
					Vector3 vector2 = Vector3.zero;
					switch (collidable.CollidableType)
					{
					case CollidableType.Box:
						using (new UnityProfileMarker(IntersectsBoxStr))
						{
							if (collidable.Box.IntersectRay(ray2, out distance))
							{
								vector2 = -ray2.direction;
							}
						}
						break;
					case CollidableType.Sphere:
						using (new UnityProfileMarker(IntersectsSphereStr))
						{
							float? num2 = ray2.IntersectsSphere(collidable.Sphere);
							if (num2.HasValue)
							{
								Vector3 vector6 = ray2.origin + ray2.direction * num2.Value;
								distance = num2.Value;
								vector2 = MathUtil.SafeNormalize(vector6 - collidable.Sphere.position, -ray2.direction);
							}
						}
						break;
					case CollidableType.Mesh:
						using (new UnityProfileMarker(IntersectsMeshStr))
						{
							for (int i = 0; i < collidable.Triangles.Length; i += 3)
							{
								Vector3 vector3 = collidable.Vertices[collidable.Triangles[i]];
								Vector3 vector4 = collidable.Vertices[collidable.Triangles[i + 1]];
								Vector3 vector5 = collidable.Vertices[collidable.Triangles[i + 2]];
								float barycentricU = 0f;
								float barycentricV = 0f;
								float? num = ray2.IntersectsTriangle(vector3, vector4, vector5, ref barycentricU, ref barycentricV);
								if (num.HasValue && num.Value >= 0f && num.Value < distance)
								{
									distance = num.Value;
									vector2 = MathUtil.SafeNormalize(Vector3.Cross(vector4 - vector3, vector5 - vector3), -ray2.direction);
								}
							}
						}
						break;
					}
					if (distance < float.MaxValue)
					{
						float magnitude = matrix4x.MultiplyVector(ray2.direction * distance).magnitude;
						if (!result.HasValue || magnitude < result.Value)
						{
							result = magnitude;
							hitNormal = MathUtil.SafeNormalize(matrix4x.MultiplyVector(vector2), vector2);
						}
					}
				}
			}
			return result;
		}
	}

	public static Bounds CalcObjectBounds(GameObject obj, Matrix4x4 trans, bool physics, bool render)
	{
		Bounds result = new Bounds(trans.Translation(), Vector3.zero);
		if (physics)
		{
			MeshCollider component = obj.GetComponent<MeshCollider>();
			if (component != null)
			{
				Mesh sharedMesh = component.sharedMesh;
				if (sharedMesh == null)
				{
					Debug.LogError("missing physics mesh " + obj.name);
				}
				result.Encapsulate(CalcTransformedBounds(sharedMesh.bounds, trans));
			}
			BoxCollider component2 = obj.GetComponent<BoxCollider>();
			if (component2 != null)
			{
				result.Encapsulate(CalcTransformedBounds(new Bounds(component2.center, component2.size), trans));
			}
		}
		if (render)
		{
			MeshFilter component3 = obj.GetComponent<MeshFilter>();
			if (component3 != null)
			{
				Mesh sharedMesh2 = component3.sharedMesh;
				if (sharedMesh2 == null)
				{
					Debug.LogError("missing render mesh " + obj.name);
				}
				result.Encapsulate(CalcTransformedBounds(sharedMesh2.bounds, trans));
			}
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			Bounds bounds = CalcObjectBounds(gameObject, trans * Matrix4x4.TRS(gameObject.transform.localPosition, gameObject.transform.localRotation, gameObject.transform.localScale), physics, render);
			result.Encapsulate(bounds);
		}
		return result;
	}

	public static Bounds CalcTransformedBounds(Bounds obb, Matrix4x4 trans)
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3 extents = obb.extents;
		Vector3[] array = new Vector3[8]
		{
			trans.MultiplyPoint(obb.center + new Vector3(extents.x, extents.y, extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(extents.x, extents.y, 0f - extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(extents.x, 0f - extents.y, extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(extents.x, 0f - extents.y, 0f - extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(0f - extents.x, extents.y, extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(0f - extents.x, extents.y, 0f - extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(0f - extents.x, 0f - extents.y, extents.z)),
			trans.MultiplyPoint(obb.center + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z))
		};
		for (int i = 0; i < array.Length; i++)
		{
			vector = Vector3.Min(vector, array[i]);
			vector2 = Vector3.Max(vector2, array[i]);
		}
		return MathUtil.CreateBoundsMinMax(vector, vector2);
	}

	public bool IsTall()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.Tall;
	}

	public override bool IsRound()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.Round;
	}

	public virtual bool IsImpassableProp()
	{
		if (Prototype == null || (UnderConstructionInfo != null && !UnderConstructionInfo.IsBuiltEnoughToBeAnObstacle()))
		{
			return false;
		}
		return Prototype.CoverType != CoverType.None;
	}

	public override bool CanEncloseAnArea()
	{
		return IsImpassableProp();
	}

	public virtual bool IsMovingVehicle()
	{
		return false;
	}

	public virtual bool IsDriveableVehicle()
	{
		return false;
	}

	public override bool IsAccommodation()
	{
		return base.IsAccommodation();
	}

	public override bool IsDestroyed()
	{
		return Destroyed;
	}

	public override Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		Texture2D iconResource = GetIconResource();
		if (iconResource != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			col = ((Prototype != null && Prototype.GetNumColorVariations() > 0) ? ((Color)Prototype.ColorVariations[ColorVariation]) : Color.white);
			return iconResource;
		}
		if (UnderConstructionInfo != null && (UnityObj == null || UnityObj.GetComponent<MeshPostProcessSettings>() == null))
		{
			col = Color.white;
			return IconGenerator.Instance.GetIconForObjectType(Prototype, out mat, highlighted);
		}
		iconResource = IconGenerator.Instance.PropIconPool.GetTexture(IconIndex, Id);
		if (iconResource == null)
		{
			IconGenerator.Instance.Queue.Push(IconGenerationRequest.Prop(this));
		}
		mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		col = ((iconResource != null) ? Color.white : MathUtil.TransparentBlackCol);
		return iconResource;
	}

	public static void ReplaceLayerRecursively(GameObject obj, int oldLayer, int newLayer, bool skipParticleEffects)
	{
		if (obj.layer == oldLayer && (!skipParticleEffects || !(obj.GetComponent<ParticleSystem>() != null)))
		{
			obj.layer = newLayer;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			ReplaceLayerRecursively(obj.transform.GetChild(i).gameObject, oldLayer, newLayer, skipParticleEffects);
		}
	}

	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			SetLayerRecursively(obj.transform.GetChild(i).gameObject, newLayer);
		}
	}

	public void DrawForPropIcon()
	{
		if (UnityObj != null)
		{
			GameImpl instance = GameImpl.Instance;
			Session instance2 = Session.Instance;
			IconGenerator instance3 = IconGenerator.Instance;
			bool activeSelf = UnityObj.activeSelf;
			if (UnderConstructionInfo != null)
			{
				MeshPostProcessSettings component = UnityObj.GetComponent<MeshPostProcessSettings>();
				if (component != null && component.ExtraInfo == MeshExtraInfo.ConstructionProgressInfo)
				{
					UnitySetUnderConstruction(UnityObj, 1f);
				}
			}
			ReplaceLayerRecursively(UnityObj, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
			UnityObj.SetActive(value: true);
			UnityObj.transform.position = Sun.IconGenerationPos;
			PrefabResource unityModel = GetUnityModel();
			UnityObj.transform.rotation = unityModel?.LocalToWorldMatrix.rotation ?? Quaternion.identity;
			instance.Sun.SetupLightForTime(0.5f, useMiddayReflectionTex: true);
			Camera component2 = instance.UnityIconCameraObj.GetComponent<Camera>();
			RenderTexture renderTexture = (component2.targetTexture = instance3.PropIconPool.RenderTex);
			component2.gameObject.SetActive(value: true);
			RenderTexture.active = renderTexture;
			SetupPropIconCam(component2);
			component2.Render();
			IconIndex = instance3.PropIconPool.AddTexture(Id);
			Texture2D texture = instance3.PropIconPool.GetTexture(IconIndex, Id);
			if (texture != null)
			{
				texture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
				texture.Apply();
			}
			RenderTexture.active = null;
			component2.gameObject.SetActive(value: false);
			component2.targetTexture = null;
			if (instance2 != null)
			{
				instance.Sun.SetupLightForTime(instance2.DaysSinceStart, useMiddayReflectionTex: false);
			}
			UnityObj.SetActive(activeSelf);
			ReplaceLayerRecursively(UnityObj, Character.IconGimpLayer, Character.DefaultLayer, skipParticleEffects: false);
			UpdateUnityTransform();
			if (UnderConstructionInfo != null)
			{
				MeshPostProcessSettings component3 = UnityObj.GetComponent<MeshPostProcessSettings>();
				if (component3 != null && component3.ExtraInfo == MeshExtraInfo.ConstructionProgressInfo)
				{
					float progress = UnderConstructionInfo.GetProgress();
					UnitySetUnderConstruction(UnityObj, progress);
				}
			}
		}
		IconGenerator.Instance.Queue.FinishedProcessing();
	}

	private void AddDamagePoint(StructureDamagePoint damagePoint)
	{
		if (damagePoint.Burning || damagePoint.Fuel > 0f)
		{
			for (int num = DamagePoints.Count - 1; num >= 0; num--)
			{
				if (DamagePoints[num].Fuel > 0f)
				{
					damagePoint.Fuel += DamagePoints[num].Fuel;
					DamagePoints.RemoveAt(num);
				}
			}
		}
		if (DamagePoints.Count >= 16)
		{
			int num2 = -1;
			float num3 = float.MaxValue;
			for (int i = 0; i < DamagePoints.Count; i++)
			{
				if (damagePoint.Type == DamagePoints[i].Type && damagePoint.Source == DamagePoints[i].Source && damagePoint.Burning == DamagePoints[i].Burning)
				{
					float sqrMagnitude = (damagePoint.Pos - DamagePoints[i].Pos).sqrMagnitude;
					if (sqrMagnitude < num3)
					{
						num2 = i;
						num3 = sqrMagnitude;
					}
				}
			}
			if (num2 != -1)
			{
				damagePoint.Fuel += DamagePoints[num2].Fuel;
				damagePoint.Size = Math.Min(damagePoint.Size + DamagePoints[num2].Size, ExplosionImpactDamageMaxSize);
				DamagePoints[num2] = damagePoint;
			}
		}
		DamagePoints.Add(damagePoint);
	}

	public override float OnExplosionImpact(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, bool fromFoundations, SkillType skillType, InfectionType infectionType, bool itsATrap, TileObject bomb)
	{
		if (IsExplosionProof() && !fromFoundations)
		{
			return 0f;
		}
		if (!GetExplosionHitInfo(centre, damageRadius, out var damageFraction, out var hitPos, out var hitNormal, out var _))
		{
			return 0f;
		}
		damage *= damageFraction * damageFraction * damageFraction * damageFraction;
		AddDamagePoint(StructureDamagePoint.Create(StructureDamageType.Impact, hitPos, hitNormal, damageFraction * ExplosionImpactDamageMaxSize, source, 0f, burning: false));
		if (IsAuthoritative())
		{
			ApplyDamage(source, damage, from_impact: false, hitPos, burning: false, damageRadius);
			if (bomb == this && this is Building)
			{
				Character[] inhabitants = ((Building)this).Inhabitants;
				foreach (Character character in inhabitants)
				{
					if (character == null)
					{
						continue;
					}
					float damage2 = damage * 0.1f;
					if (character.IsFriendlyFire(source, character, itsATrap: false))
					{
						damage2 *= Session.Instance.DifficultySettings.FriendlyFireSplashDamage / 100f;
						if (damage2 == 0f)
						{
							continue;
						}
					}
					TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
					int num = character.Id * 1000 + (int)currentTime.TotalSeconds;
					InjuryLocation injuryLocation = (InjuryLocation)MathUtil.RandomInt(num, 6);
					character.PickRandomHitPos(injuryLocation, num + 1, GetBoundingBoxCentre(), out var bone, out var hitPosInBoneSpace);
					character.OnDamaged(source, character, InjuryType.Explosion, InjuryLocation.Torso, InfectionType.None, null, SkillType.Construction, ref damage2, 0f, Vector3.zero, Vector3.zero, bone, hitPosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
				}
			}
			source?.Skillset.AddProgress(source, skillType, damage * 0.01f);
		}
		return damage;
	}

	public override float OnBurned(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, float fuel, SkillType skillType, InfectionType infectionType, bool assassinate, SecrecyMode secret)
	{
		if (!IsFlammable())
		{
			return 0f;
		}
		if (!GetExplosionHitInfo(centre, damageRadius, out var damageFraction, out var hitPos, out var hitNormal, out var _))
		{
			return 0f;
		}
		bool flag = IsBurning();
		damage *= damageFraction;
		AddDamagePoint(StructureDamagePoint.Create(StructureDamageType.Fire, hitPos, hitNormal, damageFraction * BurnDamageMaxSize, source, fuel, burning: true));
		if (IsAuthoritative())
		{
			ApplyDamage(source, damage, from_impact: false, hitPos, burning: false, damageRadius);
			source?.Skillset.AddProgress(source, skillType, damage * 0.1f);
			if (!flag && IsBurning())
			{
				GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
			}
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		return damage;
	}

	public override float GetFireEffectOnCharacter(Character character)
	{
		float num = 0f;
		if (DamagePoints.Count > 100)
		{
			Debug.Log(GetDisplayNameString() + ": " + Id + ": " + ((Community != null) ? Community.GetDisplayNameString() : "") + " has DamagePoints: " + DamagePoints.Count);
		}
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			if (DamagePoints[i].Burning)
			{
				float val = 1f - Mathf.Clamp01((character.PosXZ - MathUtil.ToXZ(DamagePoints[i].Pos)).magnitude / TileObject.MaxFireHeatRange);
				num = Math.Max(num, val);
			}
		}
		return num;
	}

	public override void OnProjectileHit(Character shooter, Vector3 deterministicDir, Vector3 deterministicHitPos, float damage, bool predicted)
	{
		Vector3 impactForce = deterministicDir * damage * ProjectileImpactFactor;
		if (predicted)
		{
			if (Damage + damage >= GetMaxDamage() && !(this is EnterableVehicle))
			{
				UnityImpactDamageDestruction(impactForce, deterministicHitPos);
			}
			return;
		}
		ApplyDamage(shooter, damage, from_impact: true, deterministicHitPos, burning: false, 0f);
		if (Destroyed)
		{
			OnDemolitionFinished();
			UnityImpactDamageDestruction(impactForce, deterministicHitPos);
		}
	}

	public override float GetMassEstimate()
	{
		if (Prototype != null)
		{
			return Prototype.Mass;
		}
		return base.GetMassEstimate();
	}

	public static float CalcVehicleCollisionDamageToProp(TileObject prop, BaseObject other, Vector3 relativeVelocity, out Character otherDriver)
	{
		float num = other.GetMassEstimate();
		EnterableVehicle enterableVehicle = other as EnterableVehicle;
		if (prop is EnterableVehicle && (enterableVehicle == null || !enterableVehicle.IsJuggernaut() || !enterableVehicle.IsMoving))
		{
			num = Math.Min(num, MaxMassForDamageToVehicleCalc);
		}
		float num2 = relativeVelocity.magnitude * num * VehicleCollisionDamageFactor;
		otherDriver = enterableVehicle?.GetDriver();
		if (enterableVehicle != null && enterableVehicle.IsMoving)
		{
			Community community = ((otherDriver != null) ? otherDriver.GetCommunity() : enterableVehicle.GetCommunity());
			Community community2 = prop.GetCommunity();
			if (community != null && community2 != null)
			{
				if (community == community2 || community2.CachedAllies.Contains(community))
				{
					if (enterableVehicle.GetSpeedInMph() <= MinSpeedMphForDamagingFriendly && !enterableVehicle.IsJuggernaut())
					{
						return 0f;
					}
					num2 *= Session.Instance.DifficultySettings.FriendlyFireSplashDamage / 100f;
				}
				else if (community2.HasAnyActiveMembers() && !community2.IsZombieCommunity() && Session.Instance.CommunityManager.GetRelationship(community, community2) != CommunityRelationshipType.Hostile && enterableVehicle.GetSpeedInMph() <= MinSpeedMphForDamagingFriendly && !enterableVehicle.IsJuggernaut())
				{
					return 0f;
				}
			}
		}
		return num2;
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		if (other is Character character && character.IsRagdollOrProneOrRecovering())
		{
			return 0f;
		}
		if (other == GameTerrain.Instance && GameTerrain.Instance.GetTreeFromCollisionPoint(contactPoint) == null)
		{
			return 0f;
		}
		Character otherDriver;
		float num = CalcVehicleCollisionDamageToProp(this, other, relativeVelocity, out otherDriver);
		if (num > 0f)
		{
			bool flag = IsSusceptibleToVehicleCollisions(juggernaut: false);
			if (predicted)
			{
				if (Damage + num >= GetMaxDamage() && !(this is EnterableVehicle) && flag)
				{
					UnityImpactDamageDestruction(relativeVelocity * other.GetMassEstimate() * VehicleImpactFactor, contactPoint);
				}
			}
			else
			{
				ApplyDamage(otherDriver, num, flag, contactPoint, burning: false, 0f);
				if (Destroyed && flag)
				{
					OnDemolitionFinished();
					UnityImpactDamageDestruction(relativeVelocity * other.GetMassEstimate() * VehicleImpactFactor, contactPoint);
					if (other is EnterableVehicle enterableVehicle && enterableVehicle.IsJuggernaut() && otherDriver != null && otherDriver.IsControllableByPlayer())
					{
						AchievementsManager.Instance.IncrementAchievementStat(Achievement.Story_DumpTruckKills, 1);
					}
				}
			}
		}
		return num;
	}

	public void UnityImpactDamageDestruction(Vector3 impactForce, Vector3 contactPoint)
	{
		if (UnityObj != null && UnderConstructionInfo == null)
		{
			UnityObj.AddComponent<ImpactedBehaviour>().Init(GetAuthoritativeOrElseThis(), GetUnityModel(), impactForce, contactPoint);
			UnityObj = null;
		}
	}

	public override bool PropWantDelete()
	{
		return _deleted == DeletionState.WantDelete;
	}

	public void MarkForDeletion()
	{
		if (_deleted == DeletionState.None)
		{
			_deleted = DeletionState.WantDelete;
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		bool flag = false;
		bool flag2 = false;
		if (!Destroyed)
		{
			float fireDamage = 0f;
			Vector3 zero = Vector3.zero;
			int num = 0;
			Character source = null;
			for (int i = 0; i < DamagePoints.Count; i++)
			{
				if (DamagePoints[i].Burning)
				{
					flag = true;
					DamagePoints[i] = DamagePoints[i].Burn(this, dt, ref fireDamage);
					if (DamagePoints[i].Source != null)
					{
						source = DamagePoints[i].Source;
					}
					zero += DamagePoints[i].Pos;
					num++;
					flag2 |= DamagePoints[i].Burning;
					Prop prop = GetPredictedOrElseThis() as Prop;
					if (prop.UnityBurningEffect == null && prop.IsUnityObjectActive())
					{
						prop.UnityBurningEffect = SpecialEffectManager.Instance.SpawnBurningEffect(prop.GetUnityObject());
					}
				}
			}
			if (fireDamage > 0f)
			{
				zero /= (float)num;
				ApplyDamage(source, fireDamage, from_impact: false, zero, burning: true, 0f);
			}
			stillNeedUpdating |= flag2;
		}
		if (flag && !IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, Tile);
		}
		if (!flag2)
		{
			Prop prop2 = GetPredictedOrElseThis() as Prop;
			if (prop2.UnityBurningEffect != null)
			{
				SpecialEffectManager.StopParticleEffect(prop2.UnityBurningEffect);
				prop2.UnityBurningEffect = null;
			}
		}
		if (!Destroyed || _deleted != DeletionState.None)
		{
			return;
		}
		stillNeedUpdating = true;
		if (this is EnterableVehicle { IsMoving: not false })
		{
			return;
		}
		DemolitionShakeOffset = Session.Instance.DeterministicRand.RandomVec3() * DemolitionShakeAmount;
		DemolitionTransition += DemolitionSpeed * (float)dt.TotalSeconds;
		DemolitionSpeed += DemolitionAcceleration * (float)dt.TotalSeconds;
		Bounds boundingBox = GetBoundingBox();
		if (DemolitionTransition < boundingBox.max.y - boundingBox.min.y)
		{
			UpdateWorldTransformAndBounds();
			if (UnityDemolitionSmokeEffect == null && IsUnityObjectActive())
			{
				bool madeOfSnow = Prototype != null && Prototype.BulletHitEffect == BulletHitEffect.SnowPuff;
				UnityDemolitionSmokeEffect = SpecialEffectManager.Instance.SpawnDemolitionSmokeEffect(UnityObj, madeOfSnow);
			}
		}
		else
		{
			if (UnityDemolitionSmokeEffect != null)
			{
				UnityEngine.Object.Destroy(UnityDemolitionSmokeEffect);
				UnityDemolitionSmokeEffect = null;
			}
			OnDemolitionFinished();
		}
	}

	protected virtual void OnDemolitionFinished()
	{
		if (WantDebrisOnDemolition())
		{
			GameTerrain.Instance.SetDebris(MinTile, MaxTile);
		}
		MarkForDeletion();
		if (Community != null)
		{
			Community.AddConstructionRecord(Prototype, Tile, Orientation);
		}
		InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(this);
		if (invaderInstanceThatCreatedHunter != null)
		{
			StoryManager.QueueEvents(invaderInstanceThatCreatedHunter.Invader.OnKilledEvents, null, null, this, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
		}
	}

	public virtual void ApplyDamage(Character source, float damage, bool from_impact, Vector3 hitPos, bool burning, float damageRadius)
	{
		if (Destroyed)
		{
			return;
		}
		float maxDamage = GetMaxDamage();
		if (maxDamage >= float.MaxValue)
		{
			return;
		}
		if (Community != null)
		{
			Community.OnPropertyDamaged(GameTerrain.Instance.GetTileCoordForPos(hitPos), damageRadius, this, source);
		}
		if (UnderConstructionInfo != null)
		{
			if (!UnderConstructionInfo.IsBuiltEnoughToBeAnObstacle())
			{
				GetCommunity()?.AddConstructionRecord(Prototype, Tile, Orientation);
				Damage = maxDamage;
				MarkForDeletion();
				return;
			}
			damage *= 2f - UnderConstructionInfo.GetProgress();
		}
		Damage += damage;
		if (Damage >= maxDamage)
		{
			Damage = maxDamage;
			OnDestroyed(source, from_impact, hitPos);
		}
	}

	public void SetDamageFraction(float damageFrac)
	{
		float maxDamage = GetMaxDamage();
		if (!(maxDamage >= float.MaxValue))
		{
			Damage = damageFrac * maxDamage;
		}
	}

	public override Resource<AudioClip> GetDemolitionSound()
	{
		if (Prototype == null || !Prototype.WantSoundOnDemolition)
		{
			return null;
		}
		return SoundManager.DemolitionSound;
	}

	public virtual void OnDestroyed(Character source, bool from_impact, Vector3 hitPos)
	{
		Destroyed = true;
		DemolitionSpeed = 0f;
		DemolitionTransition = 0f;
		ClearAllConnectedWires();
		if ((IsUnityObjectActive() || IsBeingPredicted()) && GetDemolitionSound() != null && !from_impact)
		{
			SoundManager.PlaySound3D(GetDemolitionSound(), Pos);
		}
		if (UnityObj != null)
		{
			SetLayerRecursively(UnityObj, Character.GibLayer);
		}
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		if (source != null)
		{
			Community community = GetCommunity();
			if (community != null && source.Community != community && community.HasAnyActiveMembers())
			{
				Memory.OnMemorableEvent(MemoryPrototype.DestroyedProperty, source, community, null, 1f, SecrecyMode.Public, null, fakeNews: false, this);
			}
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.BuildingDestroyed, source, this);
	}

	private bool GetExplosionHitInfo(Vector3 centre, float damageRadius, out float damageFraction, out Vector3 hitPos, out Vector3 hitNormal, out Vector3 dir)
	{
		hitPos = centre;
		damageFraction = 1f;
		Bounds boundingBox = GetBoundingBox();
		dir = MathUtil.SafeNormalize(GetBoundingBoxCentre() - centre, Vector3.up);
		hitNormal = -dir;
		if (!boundingBox.Contains(centre))
		{
			Ray ray = new Ray(centre, dir);
			if (!boundingBox.IntersectRay(ray, out var distance))
			{
				return false;
			}
			damageFraction = ((damageRadius > 0f) ? Math.Max(0f, 1f - distance / damageRadius) : 0f);
			if (damageFraction == 0f)
			{
				return false;
			}
			hitPos = centre + dir * distance;
		}
		return true;
	}

	public override bool IsBurningEnoughToDestroy()
	{
		float maxDamage = GetMaxDamage();
		if (maxDamage >= float.MaxValue)
		{
			return false;
		}
		float num = 0f;
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			if (DamagePoints[i].Burning)
			{
				num += DamagePoints[i].Fuel / TileObject.FuelBurningRateFlOzPerSecond * StructureDamagePoint.FireDamageRate;
			}
		}
		return num >= (maxDamage - Damage) * 1.1f;
	}

	public override bool IsBurning()
	{
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			if (DamagePoints[i].Burning)
			{
				return true;
			}
		}
		return false;
	}

	public override void LightFire(Character character)
	{
		bool flag = IsBurning();
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			if (DamagePoints[i].Type == StructureDamageType.Fire && !(DamagePoints[i].Fuel <= 0f) && !DamagePoints[i].Burning)
			{
				float sqrMagnitude = (character.PosXZ - MathUtil.ToXZ(DamagePoints[i].Pos)).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num = i;
					num2 = sqrMagnitude;
				}
			}
		}
		if (num == -1)
		{
			if (GetExplosionHitInfo(character.Pos, 1000f, out var _, out var hitPos, out var hitNormal, out var _))
			{
				AddDamagePoint(StructureDamagePoint.Create(StructureDamageType.Fire, hitPos, hitNormal, 0.5f * BurnDamageMaxSize, character, 0f, burning: true));
				Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
			}
		}
		else
		{
			StructureDamagePoint value = DamagePoints[num];
			value.Burning = true;
			DamagePoints[num] = value;
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		if (!flag && IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
		}
	}

	public override float GetFuel()
	{
		float num = 0f;
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			num += DamagePoints[i].Fuel;
		}
		return num;
	}

	public override void AddFuel(Character character, float amount)
	{
		int num = -1;
		float num2 = 16f;
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			if (DamagePoints[i].Type == StructureDamageType.Fire)
			{
				float sqrMagnitude = (character.PosXZ - MathUtil.ToXZ(DamagePoints[i].Pos)).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num = i;
					num2 = sqrMagnitude;
				}
			}
		}
		if (num == -1)
		{
			if (GetExplosionHitInfo(character.Pos, 1000f, out var _, out var hitPos, out var hitNormal, out var _))
			{
				AddDamagePoint(StructureDamagePoint.Create(StructureDamageType.Fire, hitPos, hitNormal, 0.5f * BurnDamageMaxSize, character, amount, burning: false));
			}
		}
		else
		{
			StructureDamagePoint value = DamagePoints[num];
			value.Fuel += amount;
			DamagePoints[num] = value;
		}
	}

	public virtual float GetMaxDamage()
	{
		if (Prototype == null || !(Prototype.MaxDamage > 0f))
		{
			return float.MaxValue;
		}
		return Prototype.MaxDamage;
	}

	public override float GetDamageFraction()
	{
		return Damage / GetMaxDamage();
	}

	public override float GetCaptureFraction()
	{
		return CaptureAmount;
	}

	public override void RepairDamage(float amount, Vector3 fromPos)
	{
		float damage = Damage;
		Damage = Math.Max(0f, Damage - amount * GetMaxDamage());
		if (Damage < 0.001f)
		{
			Damage = 0f;
		}
		int num = (int)((float)DamagePoints.Count * Damage / damage);
		while (DamagePoints.Count > num)
		{
			int num2 = FindDamagePointNearestPos(fromPos);
			if (num2 < 0 || num2 >= DamagePoints.Count)
			{
				break;
			}
			DamagePoints.RemoveAt(num2);
		}
		if (Damage <= 0f && Community != null)
		{
			Community.NeedsRepair.Remove(this);
		}
	}

	public int FindDamagePointNearestPos(Vector3 pos)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < DamagePoints.Count; i++)
		{
			float sqrMagnitude = (DamagePoints[i].Pos - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				result = i;
				num = sqrMagnitude;
			}
		}
		return result;
	}

	public override void AbandonBuilding()
	{
		base.AbandonBuilding();
		CaptureAmount = 0.5f;
		UnitySetupWindows();
	}

	public override bool CaptureBuilding(float amount, Character capturedBy)
	{
		CaptureAmount += amount;
		if (CaptureAmount >= 1f)
		{
			CaptureAmount = 1f;
			Community community = Community;
			SetCommunity(capturedBy.Community);
			capturedBy.Community.OnCompletedBuildingAddedToCommunity(capturedBy, this, community);
			if (capturedBy.Community.CommunityType == CommunityType.Player)
			{
				MarkInvestigated(capturedBy);
				GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
			}
			UnitySetupWindows();
			return true;
		}
		UnitySetupWindows();
		return false;
	}

	public override bool WantFlattenTerrain()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.WantFlattenTerrain;
	}

	public override bool WantClearGrass()
	{
		if (Prototype == null)
		{
			return false;
		}
		return Prototype.WantClearGrass;
	}

	public override void MarkInvestigated(Character investigator)
	{
		if (!Investigated)
		{
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Investigated, investigator, this);
			Investigated = true;
			if (Prototype != null && Prototype.OpenedPrefabs != null && Prototype.OpenedPrefabs.Count > 0)
			{
				UnityReinit();
			}
		}
		if (Prototype != null)
		{
			Prototype.MarkDiscovered();
		}
	}

	public override bool IsInvestigated()
	{
		return Investigated;
	}

	public override EquipmentContainer GetInventory()
	{
		return Inventory;
	}

	public override UnderConstructionInfo GetUnderConstructionInfo()
	{
		return UnderConstructionInfo;
	}

	public override void SetUnderConstructionInfo(UnderConstructionInfo underConstructionInfo)
	{
		bool flag = IsImpassableProp();
		bool num = HasUnityObject();
		bool flag2 = IsUnityObjectActive();
		if (flag2)
		{
			UnityDeactivate();
		}
		if (num)
		{
			UnityDelete();
		}
		if (UnderConstructionInfo != null && Community != null)
		{
			Community.UnderConstructionBuildings.Remove(this);
		}
		UnderConstructionInfo = underConstructionInfo;
		if (UnderConstructionInfo != null && Community != null && !Community.UnderConstructionBuildings.Contains(this))
		{
			Community.UnderConstructionBuildings.Add(this);
		}
		if (num)
		{
			UnityInit();
		}
		if (flag2)
		{
			UnityActivate();
		}
		UpdateWorldTransformAndBounds();
		if (InTerrain)
		{
			bool flag3 = IsImpassableProp();
			if (flag && !flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
			}
			else if (!flag && flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
		}
		if (Community != null && Community.CommunityType == CommunityType.Player && UnderConstructionInfo == null && Prototype != null)
		{
			Prototype.MarkDiscovered();
		}
		IconIndex = -1;
	}

	private void UnityApplyPosterVariations(GameObject obj, int salt)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				if (Prototype.HasPosterVariation(component.materials[i]) || component.gameObject.name.StartsWith("Poster"))
				{
					List<Resource<Material>> posterVariations = Prototype.PosterVariations;
					if (posterVariations != null && posterVariations.Count > 0)
					{
						int index = MathUtil.RandomInt(salt * 23 + i, posterVariations.Count);
						Material[] array = new Material[component.materials.Length];
						component.materials.CopyTo(array, 0);
						array[i] = posterVariations[index];
						component.materials = array;
					}
				}
			}
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			GameObject gameObject = obj.transform.GetChild(j).gameObject;
			UnityApplyPosterVariations(gameObject, salt * 37 + j);
		}
	}

	private void UnityApplyMaterialVariation(GameObject obj, int materialVariation)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				if (Prototype.HasMaterialVariation(component.materials[i]) || component.gameObject.name.StartsWith("Sign"))
				{
					Material[] array = new Material[component.materials.Length];
					component.materials.CopyTo(array, 0);
					array[i] = Prototype.MaterialVariations[materialVariation];
					component.materials = array;
				}
			}
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			GameObject gameObject = obj.transform.GetChild(j).gameObject;
			UnityApplyMaterialVariation(gameObject, materialVariation);
		}
	}

	public static void UnityApplyColorVariation(GameObject obj, string materialName, Color col)
	{
		string[] array = materialName.Split(',');
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int num = array[i].IndexOf(':');
			if (num != -1)
			{
				array2[i] = Shader.PropertyToID(array[i].Substring(num + 1));
				array[i] = array[i].Substring(0, num);
			}
		}
		UnityApplyColorVariationInternal(obj, array, array2, col);
	}

	public static void UnityApplyColorVariationInternal(GameObject obj, string[] materialNames, int[] paramNames, Color col)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				if (materialNames.Length == 0)
				{
					component.materials[i].color = col;
					continue;
				}
				for (int j = 0; j < materialNames.Length; j++)
				{
					if (component.materials[i].name.Replace(" (Instance)", string.Empty) == materialNames[j])
					{
						Material material = component.materials[i];
						if (paramNames[j] != 0)
						{
							material.SetColor(paramNames[j], col);
						}
						else
						{
							material.color = col;
						}
					}
				}
			}
		}
		for (int k = 0; k < obj.transform.childCount; k++)
		{
			UnityApplyColorVariationInternal(obj.transform.GetChild(k).gameObject, materialNames, paramNames, col);
		}
	}

	public static void UnitySetSnowFade(GameObject unityObj, float snowFade)
	{
		MeshRenderer component = unityObj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				component.materials[i].SetFloat(ShaderHash._SnowFade, snowFade);
			}
		}
		for (int j = 0; j < unityObj.transform.childCount; j++)
		{
			UnitySetSnowFade(unityObj.transform.GetChild(j).gameObject, snowFade);
		}
	}

	public static void UnitySetUnderConstruction(GameObject unityObj, float underConstruction)
	{
		if (underConstruction <= 0f)
		{
			underConstruction = -1f;
		}
		UnitySetUnderConstructionRecursive(unityObj, underConstruction);
	}

	private static void UnitySetUnderConstructionRecursive(GameObject unityObj, float underConstruction)
	{
		unityObj.layer = ((underConstruction < 1f) ? Character.IgnoreRaycastLayer : Character.DefaultLayer);
		MeshRenderer component = unityObj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				Material material = component.materials[i];
				material.SetFloat(ShaderHash._UnderConstruction, underConstruction);
				if (underConstruction < 1f)
				{
					material.EnableKeyword(CONSTRUCTION_PROGRESS_ENABLED);
				}
				else
				{
					material.DisableKeyword(CONSTRUCTION_PROGRESS_ENABLED);
				}
			}
		}
		for (int j = 0; j < unityObj.transform.childCount; j++)
		{
			UnitySetUnderConstructionRecursive(unityObj.transform.GetChild(j).gameObject, underConstruction);
		}
	}

	public static void UnitySetMoss(GameObject unityObj, float mossAmount, float mossOnSidesAmount, ref float oldMossAmount, ref float oldMossOnSidesAmount)
	{
		MeshRenderer component = unityObj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.materials.Length; i++)
			{
				Material material = component.materials[i];
				if (material.HasFloat(ShaderHash._MossAmount))
				{
					oldMossAmount = Math.Max(oldMossAmount, material.GetFloat(ShaderHash._MossAmount));
				}
				if (material.HasFloat(ShaderHash._MossOnSidesAmount))
				{
					oldMossOnSidesAmount = Math.Max(oldMossOnSidesAmount, material.GetFloat(ShaderHash._MossOnSidesAmount));
				}
				material.SetFloat(ShaderHash._MossAmount, mossAmount);
				material.SetFloat(ShaderHash._MossOnSidesAmount, mossOnSidesAmount);
			}
		}
		for (int j = 0; j < unityObj.transform.childCount; j++)
		{
			UnitySetMoss(unityObj.transform.GetChild(j).gameObject, mossAmount, mossOnSidesAmount, ref oldMossAmount, ref oldMossOnSidesAmount);
		}
	}

	public void SetMossCleanedAway(bool mossCleanedAway)
	{
		bool num = HasUnityObject();
		bool flag = IsUnityObjectActive();
		if (flag)
		{
			UnityDeactivate();
		}
		if (num)
		{
			UnityDelete();
		}
		MossCleanedAway = mossCleanedAway;
		if (num)
		{
			UnityInit();
		}
		if (flag)
		{
			UnityActivate();
		}
	}

	private void GetAllUnityRenderers(GameObject obj, List<Renderer> renderers)
	{
		Renderer component = obj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			renderers.Add(component);
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			GetAllUnityRenderers(gameObject, renderers);
		}
	}

	public override void UnityInit()
	{
		if (UnderConstructionInfo != null)
		{
			UnityConstructionCordon = UnderConstructionInfo.UnityInitCordon(this);
			UnityConstructionCordon.SetActive(value: false);
		}
		PrefabResource unityModel = GetUnityModel();
		if (unityModel != null && unityModel.GetAsset() != null && (UnderConstructionInfo == null || !(unityModel.GetAsset().GetComponent<MeshPostProcessSettings>() == null)))
		{
			UnityObj = UnityEngine.Object.Instantiate(unityModel.GetAsset(), (Id != 0) ? GameTerrain.Instance.UnityTerrainObj.transform : null, worldPositionStays: true);
		}
		if (UnityObj == null)
		{
			UnityObj = new GameObject();
		}
		if (Prototype != null && Prototype.VisibleFromFarAway && UnityObj.GetComponent<LODGroup>() == null)
		{
			LODGroup lODGroup = UnityObj.AddComponent<LODGroup>();
			lODGroup.fadeMode = LODFadeMode.CrossFade;
			lODGroup.animateCrossFading = true;
			List<Renderer> list = new List<Renderer>();
			GetAllUnityRenderers(UnityObj, list);
			lODGroup.SetLODs(new LOD[1]
			{
				new LOD(0.1f, list.ToArray())
			});
			lODGroup.RecalculateBounds();
		}
		if (Prototype != null && Prototype.MaterialVariationNames != null && Prototype.MaterialVariationNames.Count > 1)
		{
			int materialVariation = GetMaterialVariation();
			UnityApplyMaterialVariation(UnityObj, materialVariation);
		}
		if (Prototype != null && Prototype.PosterVariationNames != null && Prototype.PosterVariationNames.Count > 1)
		{
			UnityApplyPosterVariations(UnityObj, Id);
		}
		if (Prototype != null && Prototype.ColorVariations != null && Prototype.ColorVariations.Length != 0)
		{
			int colorVariation = GetColorVariation();
			UnityApplyColorVariation(UnityObj, Prototype.ColorVariationMaterialName, Prototype.ColorVariations[colorVariation]);
		}
		if (Prototype != null && Prototype.ColorVariations2 != null && Prototype.ColorVariations2.Length != 0)
		{
			int colorVariation2 = GetColorVariation2();
			UnityApplyColorVariation(UnityObj, Prototype.ColorVariationMaterial2Name, Prototype.ColorVariations2[colorVariation2]);
		}
		if (Prototype != null && Prototype.ColorVariations3 != null && Prototype.ColorVariations3.Length != 0)
		{
			int colorVariation3 = GetColorVariation3();
			UnityApplyColorVariation(UnityObj, Prototype.ColorVariationMaterial3Name, Prototype.ColorVariations3[colorVariation3]);
		}
		if (Prototype != null && Prototype.ColorVariations4 != null && Prototype.ColorVariations4.Length != 0)
		{
			int colorVariation4 = GetColorVariation4();
			UnityApplyColorVariation(UnityObj, Prototype.ColorVariationMaterial4Name, Prototype.ColorVariations4[colorVariation4]);
		}
		if (MossCleanedAway)
		{
			float oldMossAmount = 0f;
			float oldMossOnSidesAmount = 0f;
			UnitySetMoss(UnityObj, 0f, 0f, ref oldMossAmount, ref oldMossOnSidesAmount);
		}
		if (UnderConstructionInfo != null)
		{
			MeshPostProcessSettings component = UnityObj.GetComponent<MeshPostProcessSettings>();
			if (component != null && component.ExtraInfo == MeshExtraInfo.ConstructionProgressInfo)
			{
				float progress = UnderConstructionInfo.GetProgress();
				UnitySetUnderConstruction(UnityObj, progress);
			}
		}
		else
		{
			UnitySetupWindows();
		}
		UnityObj.name = GetDisplayNameString();
		UnityObj.SetActive(value: false);
		UpdateUnityTransform();
		UnitySetupIdBehaviour();
		if (Prototype != null && Prototype.ConnectedWirePoints != null && Prototype.ConnectedWirePoints.Length != 0 && !Destroyed && ConnectedWires != null)
		{
			LODGroup component2 = UnityObj.GetComponent<LODGroup>();
			foreach (Prop connectedWire in ConnectedWires)
			{
				if (connectedWire.Id > Id)
				{
					continue;
				}
				for (int i = 0; i < Prototype.ConnectedWirePoints.Length; i++)
				{
					Vector3 vector = World.MultiplyPoint(Prototype.ConnectedWirePoints[i]);
					Vector3 vector2 = connectedWire.World.MultiplyPoint((connectedWire.Prototype != null && connectedWire.Prototype.ConnectedWirePoints.Length != 0) ? connectedWire.Prototype.ConnectedWirePoints[Math.Min(i, connectedWire.Prototype.ConnectedWirePoints.Length - 1)] : Vector3.zero);
					Vector3 position = Vector3.Lerp(vector, vector2, 0.5f);
					Quaternion rotation = Quaternion.LookRotation(MathUtil.SafeNormalize(vector2 - vector, Vector3.right));
					GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)OverheadWirePrefab, position, rotation, UnityObj.transform);
					float magnitude = (vector2 - vector).magnitude;
					Vector3 localScale = UnityObj.transform.localScale;
					gameObject.transform.localScale = new Vector3(1f / localScale.x, 1f / localScale.y, magnitude / (40f * localScale.z));
					if (component2 != null)
					{
						LOD[] lODs = component2.GetLODs();
						for (int j = 0; j < lODs.Length; j++)
						{
							int num = lODs[j].renderers.Length;
							Renderer[] array = new Renderer[num + 1];
							lODs[j].renderers.CopyTo(array, 0);
							array[num] = gameObject.GetComponent<MeshRenderer>();
							lODs[j].renderers = array;
						}
						component2.SetLODs(lODs);
					}
				}
			}
		}
		if (IsUnityObjectAlwaysActive() || (InTerrain && Session.Instance != null && Session.Instance.IsInFocusArea(GetTileRect())))
		{
			UnityActivate();
		}
	}

	public override void UnityDelete()
	{
		base.UnityDelete();
		if (UnityWires != null)
		{
			UnityEngine.Object.Destroy(UnityWires);
			UnityWires = null;
		}
		if (UnityConstructionCordon != null)
		{
			UnityEngine.Object.Destroy(UnityConstructionCordon);
			UnityConstructionCordon = null;
		}
		if (UnityWindows != null)
		{
			UnityWindows.Clear();
		}
		if (UnityObj != null)
		{
			UnityEngine.Object.Destroy(UnityObj);
			UnityObj = null;
		}
	}

	public void UnitySetupWindows()
	{
		if (!(CaptureAmount > 0f) || !(UnityObj != null))
		{
			return;
		}
		PrefabResource unityModel = GetUnityModel();
		if (unityModel == null || unityModel.Windows.Count <= 0)
		{
			return;
		}
		int num = Mathf.FloorToInt(CaptureAmount * (float)unityModel.Windows.Count);
		while (UnityWindows != null && UnityWindows.Count > num)
		{
			int index = UnityWindows.Count - 1;
			UnityEngine.Object.Destroy(UnityWindows[index]);
			UnityWindows.RemoveAt(index);
		}
		for (int i = ((UnityWindows != null) ? UnityWindows.Count : 0); i < num; i++)
		{
			int num2 = Id * 1000 + i;
			GameObject original = BoardedWindowPrefabs[MathUtil.RandomInt(num2, BoardedWindowPrefabs.Length)];
			Matrix4x4 localToWorldMatrix = UnityObj.transform.localToWorldMatrix;
			Vector3 position = localToWorldMatrix.MultiplyPoint(unityModel.Windows[i].Pos);
			Quaternion rotation = localToWorldMatrix.rotation * unityModel.Windows[i].Rot * Quaternion.Euler(0f, MathUtil.RandomChoice(num2, 0.5f) ? 180f : 0f, 0f);
			GameObject gameObject = UnityEngine.Object.Instantiate(original, position, rotation, UnityObj.transform);
			gameObject.transform.localScale = Vector3.one * unityModel.Windows[i].Scale;
			if (UnityWindows == null)
			{
				UnityWindows = new List<GameObject>();
			}
			UnityWindows.Add(gameObject);
		}
	}

	public void AutoConnectWires()
	{
		if (UnderConstructionInfo != null || Prototype == null || Prototype.ConnectedWirePoints == null || Prototype.ConnectedWirePoints.Length == 0)
		{
			return;
		}
		List<TileObject> list = new List<TileObject>();
		List<Prop> list2 = new List<Prop>();
		int num = 64;
		GameTerrain.Instance.GetObjectsInRect(Tile - new TerrainCoord(num, num), Tile + new TerrainCoord(num, num), list);
		foreach (TileObject item2 in list)
		{
			PropPrototype propPrototype = item2.GetPropPrototype();
			if (propPrototype != null && propPrototype.ConnectedWirePoints != null && propPrototype.ConnectedWirePoints.Length != 0 && item2 != this && item2 is Prop item)
			{
				list2.Add(item);
			}
		}
		list2.Sort(delegate(Prop a, Prop b)
		{
			float distSquared = a.Tile.GetDistSquared(Tile);
			float distSquared2 = b.Tile.GetDistSquared(Tile);
			if (distSquared < distSquared2)
			{
				return -1;
			}
			return (distSquared2 < distSquared) ? 1 : 0;
		});
		ClearAllConnectedWires();
		for (int num2 = 0; num2 < Math.Min(2, list2.Count); num2++)
		{
			SetConnectedWireTo(list2[num2]);
		}
	}

	public override void OnStoryReloaded()
	{
		if (Prototype != null)
		{
			Prototype = GameImpl.Instance.FindPropPrototypeByNameHash(Prototype.NameHash);
		}
		UpdateWorldTransformAndBounds();
	}

	public void UpdateUnityTransform()
	{
		if (UnityConstructionCordon != null)
		{
			UnityConstructionCordon.transform.position = Pos;
			UnityConstructionCordon.transform.rotation = Quaternion.identity;
			UnityConstructionCordon.transform.localScale = Vector3.one;
		}
		if (UnityObj != null)
		{
			Matrix4x4 customModelTransform = GetCustomModelTransform();
			UnityObj.transform.position = customModelTransform.Translation();
			UnityObj.transform.rotation = Quaternion.LookRotation(customModelTransform.Forward(), customModelTransform.Up());
			UnityObj.transform.localScale = new Vector3(customModelTransform.Right().magnitude, customModelTransform.Up().magnitude, customModelTransform.Forward().magnitude);
		}
	}

	public virtual Matrix4x4 GetCustomModelTransform()
	{
		PrefabResource unityModel = GetUnityModel();
		if (unityModel == null || unityModel.GetAsset() == null)
		{
			return World;
		}
		Matrix4x4 mat = unityModel.LocalToWorldMatrix;
		MathUtil.SetTranslation(ref mat, ModelOffset);
		return World * mat;
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: true);
		}
		if (UnityConstructionCordon != null)
		{
			UnityConstructionCordon.SetActive(value: true);
		}
	}

	public override void UnityDeactivate()
	{
		if (UnityBurningEffect != null)
		{
			UnityEngine.Object.DestroyImmediate(UnityBurningEffect);
			UnityBurningEffect = null;
		}
		if (UnityDemolitionSmokeEffect != null)
		{
			UnityEngine.Object.DestroyImmediate(UnityDemolitionSmokeEffect);
			UnityDemolitionSmokeEffect = null;
		}
		if (UnityConstructionCordon != null)
		{
			UnityConstructionCordon.SetActive(value: false);
		}
		if (UnityObj != null)
		{
			UnityObj.SetActive(value: false);
		}
		base.UnityDeactivate();
	}

	public override bool HasUnityObject()
	{
		return UnityObj != null;
	}

	public override bool IsUnityObjectActive()
	{
		if (UnityObj != null)
		{
			return UnityObj.activeSelf;
		}
		return false;
	}

	public override GameObject GetUnityObject()
	{
		return UnityObj;
	}

	public virtual float GetOpenTransition()
	{
		return 0f;
	}

	public override void OnPostRender()
	{
		if (Character.DrawBoundingBoxes)
		{
			Bounds boundingBox = GetBoundingBox();
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawBox(boundingBox.center, boundingBox.extents, Color.blue);
			DebugGraphics.EndDrawLines();
		}
		if (Character.DrawHitBoxes)
		{
			DrawCollisionMesh(GetUnityModel(), GetCustomModelTransform(), GetOpenTransition());
		}
	}

	public static void DrawCollisionMesh(PrefabResource res, Matrix4x4 rootTransform, float gateOpenTransition)
	{
		if (res == null)
		{
			return;
		}
		foreach (Collidable collidable in res.Collidables)
		{
			Matrix4x4 matrix4x = rootTransform * collidable.WorldFromLocalTrans;
			float childAngle = Gate.GetChildAngle(collidable.Name, gateOpenTransition);
			matrix4x = MathUtil.CreateTranslation(matrix4x.Translation()) * MathUtil.CreateRotationY(childAngle) * MathUtil.CreateTranslation(-matrix4x.Translation()) * matrix4x;
			switch (collidable.CollidableType)
			{
			case CollidableType.Box:
				DebugGraphics.StartDrawLines(matrix4x);
				DebugGraphics.DrawBox(collidable.Box.center, collidable.Box.size * 0.5f, Color.red);
				DebugGraphics.EndDrawLines();
				break;
			case CollidableType.Sphere:
				DebugGraphics.StartDrawLines(matrix4x);
				DebugGraphics.DrawBox(collidable.Box.center, collidable.Box.size * 0.5f, Color.yellow);
				DebugGraphics.EndDrawLines();
				break;
			case CollidableType.Mesh:
			{
				DebugGraphics.StartDrawLines(matrix4x);
				for (int i = 0; i < collidable.Triangles.Length; i += 3)
				{
					Vector3 vector = collidable.Vertices[collidable.Triangles[i]];
					Vector3 vector2 = collidable.Vertices[collidable.Triangles[i + 1]];
					Vector3 vector3 = collidable.Vertices[collidable.Triangles[i + 2]];
					DebugGraphics.DrawLine(vector, vector2, Color.red);
					DebugGraphics.DrawLine(vector2, vector3, Color.red);
					DebugGraphics.DrawLine(vector3, vector, Color.red);
				}
				DebugGraphics.EndDrawLines();
				break;
			}
			}
		}
	}

	public bool WantStoreHere(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		if (StoragePolicies != null)
		{
			for (int i = 0; i < StoragePolicies.Count; i++)
			{
				if (StoragePolicies[i].Proto == proto && StoragePolicies[i].Liquid == liquid)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetStoragePolicy(EquipmentPrototype proto, LiquidPrototype liquid, bool on)
	{
		if (on && !WantStoreHere(proto, liquid))
		{
			if (StoragePolicies == null)
			{
				StoragePolicies = new List<StoragePolicy>();
			}
			StoragePolicies.Add(new StoragePolicy(proto, liquid));
		}
		else
		{
			if (on || !WantStoreHere(proto, liquid))
			{
				return;
			}
			for (int i = 0; i < StoragePolicies.Count; i++)
			{
				if (StoragePolicies[i].Proto == proto && StoragePolicies[i].Liquid == liquid)
				{
					StoragePolicies.RemoveAt(i);
					break;
				}
			}
		}
	}

	public void SetConnectedWireTo(Prop prop)
	{
		if (ConnectedWires == null)
		{
			ConnectedWires = new List<Prop>();
		}
		if (!ConnectedWires.Contains(prop))
		{
			ConnectedWires.Add(prop);
			UnityReinit();
			prop.SetConnectedWireTo(this);
		}
	}

	public void ClearConnectedWireTo(Prop prop)
	{
		if (ConnectedWires != null && ConnectedWires.Contains(prop))
		{
			ConnectedWires.Remove(prop);
			UnityReinit();
			prop.ClearConnectedWireTo(this);
		}
	}

	public void ClearAllConnectedWires()
	{
		while (ConnectedWires != null && ConnectedWires.Count > 0)
		{
			ClearConnectedWireTo(ConnectedWires[0]);
		}
	}

	public bool HasConnectedWires()
	{
		if (ConnectedWires != null)
		{
			return ConnectedWires.Count > 0;
		}
		return false;
	}

	public void UnityReInitConnectedWires()
	{
		if (!HasConnectedWires())
		{
			return;
		}
		UnityReinit();
		foreach (Prop connectedWire in ConnectedWires)
		{
			connectedWire.UnityReinit();
		}
	}
}

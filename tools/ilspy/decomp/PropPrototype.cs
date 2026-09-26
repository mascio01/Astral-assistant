using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class PropPrototype
{
	public static PrefabResource[] MiscPrefabs = new PrefabResource[53]
	{
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer1"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer2"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer2_With_Canopy"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer2_With_Canopy2"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer2_With_Canopy_Closed"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer3"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer3_With_Canopy"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer3_With_Canopy2"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Trailer3_With_Canopy_Closed"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Table"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Stool_Old"),
		new PrefabResource("Prefabs/Buildings/Trailer_Park_Pack/Stool_Old_2"),
		new PrefabResource("Prefabs/Buildings/Metal_Tower_Short"),
		new PrefabResource("Prefabs/Buildings/OldLogCabin"),
		new PrefabResource("Prefabs/Buildings/ChickenCoop"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart1"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart2"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart3"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart4"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart5"),
		new PrefabResource("Prefabs/Props/Crashed_planes/heliCrashPart6"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part1"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part2"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part3"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part4"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part5"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part6"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part7"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part8"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part9"),
		new PrefabResource("Prefabs/Props/Crashed_planes/jumbo_crash_part10"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part1"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part2"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part3"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part4"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part5"),
		new PrefabResource("Prefabs/Props/Crashed_planes/prop_crash_part6"),
		new PrefabResource("Prefabs/Props/WindTurbine"),
		new PrefabResource("Prefabs/Props/RadarTower"),
		new PrefabResource("Prefabs/Vehicles/Tank_Wrecked"),
		new PrefabResource("Prefabs/Props/Snowman/Snowman_01"),
		new PrefabResource("Prefabs/Props/Snowman/Snowman_02"),
		new PrefabResource("Prefabs/Props/Snowman/Snowman_03"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt1_01"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt1_02"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt1_03"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt2_01"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt2_02"),
		new PrefabResource("Prefabs/Props/Snowman/SnowmanMelt2_03"),
		new PrefabResource("Prefabs/Vehicles/ah6"),
		new PrefabResource("Prefabs/Vehicles/ah64"),
		new PrefabResource("Prefabs/Vehicles/uh60"),
		new PrefabResource("Prefabs/Vehicles/osprey")
	};

	public static Resource<Material>[] MiscMaterials = new Resource<Material>[8]
	{
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer1_Blue", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer1_Pink", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer1_Yellow", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer1_White", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer2_1", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer2_2", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer3_1", includeInList: true),
		new Resource<Material>("Materials/Buildings/Trailer_Park_Pack/Trailer3_2", includeInList: true)
	};

	public static PropPrototype Well;

	public static PropPrototype Campfire;

	public static PropPrototype Outhouse;

	public static PropPrototype WireFence;

	public static PropPrototype WoodFence;

	public static PropPrototype ConcreteWall;

	public static PropPrototype WireGate;

	public static PropPrototype WoodGate;

	public static PropPrototype WatchTower;

	public static PropPrototype ConcreteWatchTower;

	public static PropPrototype Pillbox;

	public static PropPrototype FlintProp;

	public static PropPrototype Grave;

	public static PropPrototype Shack;

	public static PropPrototype Boulder;

	public static PropPrototype Maize;

	public static PropPrototype PumpkinPlant;

	public static PropPrototype CucumberPlant;

	public static PropPrototype PepperPlant;

	public static List<PropPrototype> PlantableCropTypes = new List<PropPrototype>();

	[XmlIgnore]
	public string Name = "";

	public BaseObjectType TypeName = BaseObjectType.Prop;

	public string NativeName = "";

	public string Category = "";

	[XmlIgnore]
	public bool IsLooterProp;

	[XmlIgnore]
	public int NameHash;

	[DefaultValue(GenderType.Count)]
	public GenderType Gender;

	[DefaultValue(null)]
	public List<GenderInLanguage> GenderInLanguages;

	public List<string> PrefabNames = new List<string>();

	[XmlIgnore]
	public List<PrefabResource> Prefabs = new List<PrefabResource>();

	[XmlIgnore]
	public PrefabResource[] PrefabsAsArray;

	[OnlyVisibleIfHasInventory]
	[DefaultValue(null)]
	public List<string> OpenedPrefabNames;

	[XmlIgnore]
	public List<PrefabResource> OpenedPrefabs;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue(null)]
	public List<string> DeadPrefabNames;

	[XmlIgnore]
	public List<PrefabResource> DeadPrefabs;

	[OnlyVisibleForType(typeof(Grave), typeof(Snowman))]
	[DefaultValue(null)]
	public List<string> Stage1PrefabNames;

	[XmlIgnore]
	public List<PrefabResource> Stage1Prefabs;

	[OnlyVisibleForType(typeof(Grave), typeof(Snowman))]
	[DefaultValue(null)]
	public List<string> Stage2PrefabNames;

	[XmlIgnore]
	public List<PrefabResource> Stage2Prefabs;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(null)]
	public List<string> MaterialVariationNames;

	[XmlIgnore]
	public List<Resource<Material>> MaterialVariations;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(null)]
	public List<string> PosterVariationNames;

	[XmlIgnore]
	public List<Resource<Material>> PosterVariations;

	public List<Vector3> ModelOffset = new List<Vector3>();

	[OnlyVisibleForType(typeof(Flower), typeof(Bush))]
	public List<Vector2> MinMaxScale = new List<Vector2>();

	[DefaultValue(-1)]
	[OnlyVisibleIfMultiplePrefabs]
	public int PrefabIndexWhenBuiltByPlayer = -1;

	[OnlyVisibleForType(typeof(SingleTileProp))]
	[DefaultValue(false)]
	public bool TiltedWithGroundSlope;

	[DefaultValue(1f)]
	public float IconScale = 1f;

	[DefaultValue(0f)]
	[OnlyVisibleIfCanBeDestroyed]
	public float MaxDamage;

	public CoverType CoverType = CoverType.Full;

	[OnlyVisibleForType(typeof(Flower), typeof(Bush))]
	[DefaultValue(0f)]
	public float PlantCover;

	[NotVisibleForType(typeof(Campfire))]
	[DefaultValue(Flammability.Invulnerable)]
	public Flammability Flammability;

	[DefaultValue(ImpactSusceptibility.Invulnerable)]
	public ImpactSusceptibility ImpactSusceptibility = ImpactSusceptibility.Invulnerable;

	[DefaultValue(BulletHitEffect.Ricochet)]
	public BulletHitEffect BulletHitEffect = BulletHitEffect.Ricochet;

	[DefaultValue(true)]
	[OnlyVisibleIfCanBeDestroyed]
	public bool WantDebrisOnDemolition = true;

	[DefaultValue(true)]
	[OnlyVisibleIfCanBeDestroyed]
	public bool WantSoundOnDemolition = true;

	[OnlyVisibleForType(typeof(Prop))]
	public TerrainCoord ExtentsMin;

	[OnlyVisibleForType(typeof(Prop))]
	public TerrainCoord ExtentsMax;

	[OnlyVisibleForType(typeof(TiltedProp), typeof(TiltedBuilding))]
	public Vector2 WheelPos;

	[OnlyVisibleForType(typeof(TiltedProp), typeof(TiltedBuilding))]
	public Vector2 WheelOffset;

	[OnlyVisibleForType(typeof(TiltedProp), typeof(TiltedBuilding))]
	[DefaultValue(15f)]
	public float MaxYaw = 15f;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(false)]
	public bool VisibleFromFarAway;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(false)]
	public bool WantFlattenTerrain;

	[OnlyVisibleForType(typeof(Prop), typeof(SingleTileProp))]
	[DefaultValue(true)]
	public bool WantClearGrass = true;

	[OnlyVisibleForType(typeof(Prop), typeof(SingleTileProp))]
	[DefaultValue(true)]
	public bool SetAreaOwnership = true;

	[OnlyVisibleForType(typeof(Prop), typeof(SingleTileProp))]
	[DefaultValue(false)]
	public bool CanBeClearedForBuilding;

	[DefaultValue(false)]
	public bool CanBeDemolished;

	[OnlyVisibleForType(typeof(WorkBench))]
	[DefaultValue(false)]
	public bool CanRepairArmor;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(0f)]
	public float MaxInventoryWeight;

	[OnlyVisibleForType(typeof(Prop))]
	[OnlyVisibleIfHasInventory]
	[DefaultValue(null)]
	public string LootLocation;

	[OnlyVisibleIfHasInventory]
	[DefaultValue(true)]
	public bool CanAutoInvestigate = true;

	[OnlyVisibleForType(typeof(Prop))]
	[NotVisibleForType(typeof(Building))]
	[OnlyVisibleIfHasInventory]
	[DefaultValue(false)]
	public bool Lockable;

	[OnlyVisibleForType(typeof(Prop))]
	[OnlyVisibleIfHasInventory]
	[DefaultValue(false)]
	public bool CrouchToLoot;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(false)]
	public bool Tall;

	[OnlyVisibleForType(typeof(Prop), typeof(SingleTileProp))]
	[DefaultValue(false)]
	public bool Round;

	[OnlyVisibleForType(typeof(Building))]
	public InhabitantSlotDef[] Inhabitants;

	[OnlyVisibleForType(typeof(Building))]
	public EntranceDef[] Entrances;

	[OnlyVisibleForType(typeof(Building))]
	public bool HasGarageDoor;

	[OnlyVisibleForType(typeof(Building))]
	[OnlyVisibleIfHasGarageDoor]
	public TerrainCoord GarageDoorOffset;

	[OnlyVisibleForType(typeof(Building))]
	[OnlyVisibleIfHasGarageDoor]
	[DefaultValue(0f)]
	public float GarageDoorAngle;

	[OnlyVisibleForType(typeof(Building))]
	public Vector2[] Connectors;

	[OnlyVisibleForType(typeof(Building))]
	[OnlyVisibleIfHasConnectors]
	[DefaultValue(false)]
	public bool ConnectorsCanTerminate;

	[OnlyVisibleForCategory("Town/Buildings")]
	[DefaultValue("")]
	public string RoadPropType = "";

	[OnlyVisibleForCategory("Town/Buildings")]
	[DefaultValue("")]
	public string FrontPropType = "";

	[OnlyVisibleIfHasFrontProp]
	[DefaultValue(0f)]
	public float FrontPropDist;

	[OnlyVisibleForType(typeof(Building))]
	[DefaultValue(BuildingUsage.Camp)]
	public BuildingUsage Usage;

	[OnlyVisibleForCategory("Town/Buildings")]
	[DefaultValue(false)]
	public bool MustBeUnique;

	[OnlyVisibleForType(typeof(Building))]
	[DefaultValue(BaseObjectType.Invalid)]
	public BaseObjectType EnterableBySpecies;

	[OnlyVisibleIfCanBeDestroyed]
	[DefaultValue(RepairAnimType.Hammering)]
	public RepairAnimType RepairAnim;

	[OnlyVisibleIfCanBeDestroyed]
	[DefaultValue("")]
	public string RepairResourceType = "";

	[XmlIgnore]
	public EquipmentPrototype RepairResourceProto;

	[OnlyVisibleIfCanBeDestroyed]
	[OnlyVisibleIfCanBeRepaired]
	[DefaultValue(0f)]
	public float RepairResourceNeeded;

	[OnlyVisibleIfCanBeDestroyed]
	[OnlyVisibleIfCanBeRepaired]
	[DefaultValue(0)]
	public int RepairSkillNeeded;

	[DefaultValue("")]
	public string CaptureResourceType = "";

	[XmlIgnore]
	public EquipmentPrototype CaptureResourceProto;

	[OnlyVisibleIfCanBeCaptured]
	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(0f)]
	public float CaptureResourceNeeded;

	[OnlyVisibleIfCanBeCaptured]
	[DefaultValue(0)]
	public int CaptureSkillNeeded;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue("")]
	public string HarvestType = "";

	[XmlIgnore]
	public EquipmentPrototype HarvestPrototype;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue("")]
	public string HarvestSeedsType = "";

	[XmlIgnore]
	public EquipmentPrototype HarvestSeedsPrototype;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue(0)]
	public int MaxYield;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue(0)]
	public int MaxSeedsYield;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	[DefaultValue(0f)]
	public float WaterNeededPerDayInFlOz;

	[OnlyVisibleForType(typeof(PlantableCrop))]
	public Color32 Color;

	[OnlyVisibleForType(typeof(Rock), typeof(Boulder))]
	[DefaultValue(MineralType.None)]
	public MineralType MineralType;

	[OnlyVisibleForType(typeof(Rock), typeof(Boulder))]
	[DefaultValue(0)]
	public int MineralAmount;

	[OnlyVisibleForType(typeof(AnimalDrinkerProp), typeof(Outhouse), typeof(EnterableVehicle))]
	[DefaultValue(0f)]
	public float LiquidCapacity;

	[OnlyVisibleForType(typeof(AnimalDrinkerProp), typeof(Outhouse), typeof(EnterableVehicle))]
	[DefaultValue("")]
	public string LiquidType = "";

	[XmlIgnore]
	public LiquidPrototype LiquidPrototype;

	[DefaultValue("")]
	public string GrabbableEquipmentType = "";

	[XmlIgnore]
	public EquipmentPrototype GrabbableEquipmentPrototype;

	[DefaultValue("")]
	public string FallbackPropType = "";

	[XmlIgnore]
	public PropPrototype FallbackPropPrototype;

	[DefaultValue(1f)]
	public float SpawnProbabilityFactor = 1f;

	[OnlyVisibleForCategory("Town/Props/Shop")]
	[DefaultValue(0f)]
	public float SpawnWithBackAgainstBuilding;

	[OnlyVisibleForCategory("Town/Buildings")]
	[DefaultValue(0f)]
	public float SpawnMinDistanceFromRoad;

	[OnlyVisibleForType(typeof(Tripwire))]
	[DefaultValue(0f)]
	public float TrapDamage;

	[OnlyVisibleForType(typeof(Tripwire))]
	[DefaultValue(0f)]
	public float TrapDamageRadius;

	[DefaultValue(1500f)]
	public float Mass = 1500f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(0.35f)]
	public float VehicleWheelRadius = 0.35f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(1f)]
	public float VehicleSuspensionBounce = 1f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(0f)]
	public float ExtraFrontWheelSeparation;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(VehicleTractionType.FrontWheelDrive)]
	public VehicleTractionType VehicleTractionType;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(200f)]
	public float VehicleHP = 200f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(VehicleTractionType.FrontWheelDrive)]
	public VehicleTractionType VehicleBrakeType;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(1000000f)]
	public float VehicleBrakePower = 1000000f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(6500f)]
	public float VehicleMaxRPM = 6500f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(500f)]
	public float VehicleStartEngineRPM = 500f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(null)]
	public float[] VehicleGearRatios;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(30f)]
	public float VehicleMaxTurningAngle = 30f;

	[OnlyVisibleForType(typeof(EnterableVehicle))]
	[DefaultValue(true)]
	public bool VehicleCanHitTheRoad = true;

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(null)]
	public Vector3[] ConnectedWirePoints;

	[DefaultValue(null)]
	public Color32[] ColorVariations;

	[OnlyVisibleIfHasColorVariation]
	[DefaultValue("")]
	public string ColorVariationMaterialName = "";

	[OnlyVisibleIfHasColorVariation]
	[DefaultValue(null)]
	public Color32[] ColorVariations2;

	[OnlyVisibleIfHasColorVariation2]
	[DefaultValue("")]
	public string ColorVariationMaterial2Name = "";

	[OnlyVisibleIfHasColorVariation2]
	[DefaultValue(null)]
	public Color32[] ColorVariations3;

	[OnlyVisibleIfHasColorVariation3]
	[DefaultValue("")]
	public string ColorVariationMaterial3Name = "";

	[OnlyVisibleIfHasColorVariation3]
	[DefaultValue(null)]
	public Color32[] ColorVariations4;

	[OnlyVisibleIfHasColorVariation4]
	[DefaultValue("")]
	public string ColorVariationMaterial4Name = "";

	[OnlyVisibleForType(typeof(Prop))]
	[DefaultValue(null)]
	public ChildPropDef[] ChildProps;

	[XmlIgnore]
	public Resource<Texture2D> Tex;

	[XmlIgnore]
	public TileObject ProtoInstance;

	[XmlIgnore]
	public bool Discovered;

	private static string LooterPropsCatName = "LooterProps";

	public const string MaterialInstanceStr = " (Instance)";

	public string IconPath => "Icons/" + Name + ".png";

	public static void CachePropPrototypeRefs()
	{
		GameImpl instance = GameImpl.Instance;
		Well = instance.FindPropPrototypeByName("Well");
		Campfire = instance.FindPropPrototypeByName("Campfire");
		Outhouse = instance.FindPropPrototypeByName("Outhouse");
		WireFence = instance.FindPropPrototypeByName("WireFence");
		WoodFence = instance.FindPropPrototypeByName("WoodFence");
		ConcreteWall = instance.FindPropPrototypeByName("ConcreteWall");
		WireGate = instance.FindPropPrototypeByName("WireGate");
		WoodGate = instance.FindPropPrototypeByName("WoodGate");
		WatchTower = instance.FindPropPrototypeByName("WatchTower");
		ConcreteWatchTower = instance.FindPropPrototypeByName("ConcreteWatchTower");
		Pillbox = instance.FindPropPrototypeByName("Pillbox");
		FlintProp = instance.FindPropPrototypeByName("FlintProp");
		Grave = instance.FindPropPrototypeByName("Grave");
		Shack = instance.FindPropPrototypeByName("Shack");
		Boulder = instance.FindPropPrototypeByName("Boulder");
		Maize = instance.FindPropPrototypeByName("Maize");
		PumpkinPlant = instance.FindPropPrototypeByName("PumpkinPlant");
		CucumberPlant = instance.FindPropPrototypeByName("CucumberPlant");
		PepperPlant = instance.FindPropPrototypeByName("PepperPlant");
	}

	public static void CachePlantableCropTypes()
	{
		PlantableCropTypes.Clear();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.ProtoInstance is PlantableCrop)
			{
				PlantableCropTypes.Add(item.Value);
				EquipmentPrototype harvestPrototype = item.Value.HarvestPrototype;
				if (harvestPrototype != null)
				{
					harvestPrototype.HarvestedFromPlantProto = item.Value;
				}
			}
		}
	}

	public static List<PropPrototype> GetAccomodationTypes(string cat)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.Category.StartsWith(cat) && item.Value.ProtoInstance.IsAccommodation())
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public static List<PropPrototype> GetTypesWithCategory(string cat)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.Category.StartsWith(cat))
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public static List<PropPrototype> GetTypesWithCategory(string cat, List<PropPrototype> types)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (PropPrototype type in types)
		{
			if (type.Category.StartsWith(cat))
			{
				list.Add(type);
			}
		}
		return list;
	}

	public static List<PropPrototype> GetTypesWithClass(Type type)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.ProtoInstance.GetType().IsA(type))
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public static List<PropPrototype> GetTypesEnterableBySpecies(BaseObjectType species)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.EnterableBySpecies == species)
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public static List<PropPrototype> GetTypesWithMineralType(MineralType mineralType, List<PropPrototype> types)
	{
		List<PropPrototype> list = new List<PropPrototype>();
		foreach (PropPrototype type in types)
		{
			if (type.MineralType == mineralType)
			{
				list.Add(type);
			}
		}
		return list;
	}

	public static PropPrototype PickRandom(CustomRandom rand, List<PropPrototype> prototypes)
	{
		if (prototypes.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (PropPrototype prototype in prototypes)
		{
			num += prototype.SpawnProbabilityFactor;
		}
		float num2 = rand.RandomFloat() * num;
		int i;
		for (i = 0; i < prototypes.Count - 1 && !(num2 < prototypes[i].SpawnProbabilityFactor); i++)
		{
			num2 -= prototypes[i].SpawnProbabilityFactor;
		}
		return prototypes[i];
	}

	public void CalcMinMaxTile(TerrainCoord tile, Prop.OrientationType orientation, out TerrainCoord minTile, out TerrainCoord maxTile)
	{
		switch (orientation)
		{
		default:
			minTile = ExtentsMin;
			maxTile = ExtentsMax;
			break;
		case Prop.OrientationType.Deg90:
			minTile = new TerrainCoord(ExtentsMin.y, -ExtentsMax.x);
			maxTile = new TerrainCoord(ExtentsMax.y, -ExtentsMin.x);
			break;
		case Prop.OrientationType.Deg180:
			minTile = -ExtentsMax;
			maxTile = -ExtentsMin;
			break;
		case Prop.OrientationType.Deg270:
			minTile = new TerrainCoord(-ExtentsMax.y, ExtentsMin.x);
			maxTile = new TerrainCoord(-ExtentsMin.y, ExtentsMax.x);
			break;
		}
		minTile += tile;
		maxTile += tile;
	}

	public PrefabResource[] GetPrefabsAsArray()
	{
		if (PrefabsAsArray == null)
		{
			PrefabsAsArray = Prefabs.ToArray();
		}
		return PrefabsAsArray;
	}

	public string GetNameKey()
	{
		return "PROP_" + Name;
	}

	public Texture2D GetIconResource()
	{
		if (Tex == null)
		{
			return null;
		}
		return Tex.GetAsset();
	}

	public float GetHarvestWeight()
	{
		if (HarvestPrototype == null)
		{
			return 1f;
		}
		return HarvestPrototype.Weight;
	}

	public float GetHarvestSeedsWeight()
	{
		if (HarvestSeedsPrototype == null)
		{
			return 1f;
		}
		return HarvestSeedsPrototype.Weight;
	}

	public int GetNumColorVariations()
	{
		if (ColorVariations == null)
		{
			return 0;
		}
		return ColorVariations.Length;
	}

	public int GetNumColorVariations2()
	{
		if (ColorVariations2 == null)
		{
			return 0;
		}
		return ColorVariations2.Length;
	}

	public int GetNumColorVariations3()
	{
		if (ColorVariations3 == null)
		{
			return 0;
		}
		return ColorVariations3.Length;
	}

	public int GetNumColorVariations4()
	{
		if (ColorVariations4 == null)
		{
			return 0;
		}
		return ColorVariations4.Length;
	}

	public int GetNumMaterialVariations()
	{
		if (MaterialVariations == null)
		{
			return 0;
		}
		return MaterialVariations.Count;
	}

	public int GetNumPosterVariations()
	{
		if (PosterVariations == null)
		{
			return 0;
		}
		return PosterVariations.Count;
	}

	public int GetNumInhabitants()
	{
		if (Inhabitants == null)
		{
			return 0;
		}
		return Inhabitants.Length;
	}

	public bool NeedToolboxToRepair()
	{
		if (RepairAnim != RepairAnimType.Hammering)
		{
			return TypeName == BaseObjectType.EnterableVehicle;
		}
		return true;
	}

	public void CacheStuff()
	{
		ProtoInstance = BaseObjectManager.Create(TypeName) as TileObject;
		if (ProtoInstance is Prop prop)
		{
			prop.Prototype = this;
		}
		if (ProtoInstance is SingleTileProp singleTileProp)
		{
			singleTileProp.Prototype = this;
		}
		if (ProtoInstance is Building)
		{
			if (Entrances == null || Entrances.Length == 0)
			{
				Entrances = new EntranceDef[1];
			}
			if (Inhabitants == null || Inhabitants.Length == 0)
			{
				Inhabitants = new InhabitantSlotDef[1];
			}
		}
		else
		{
			Inhabitants = null;
			Entrances = null;
		}
		RepairResourceProto = GameImpl.Instance.FindEquipmentPrototypeByName(RepairResourceType);
		CaptureResourceProto = GameImpl.Instance.FindEquipmentPrototypeByName(CaptureResourceType);
		HarvestPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(HarvestType);
		HarvestSeedsPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(HarvestSeedsType);
		GrabbableEquipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(GrabbableEquipmentType);
		LiquidPrototype = GameImpl.Instance.FindLiquidPrototypeByName(LiquidType);
		FallbackPropPrototype = GameImpl.Instance.FindPropPrototypeByName(FallbackPropType);
		IsLooterProp = Category.Contains(LooterPropsCatName);
		PrefabsAsArray = null;
		Prefabs.Clear();
		foreach (string prefabName in PrefabNames)
		{
			if (Resource<GameObject>.FindResourceByPath(prefabName) is PrefabResource item)
			{
				Prefabs.Add(item);
			}
		}
		PrefabsAsArray = Prefabs.ToArray();
		if (OpenedPrefabNames != null && OpenedPrefabNames.Count > 0)
		{
			OpenedPrefabs = new List<PrefabResource>();
			foreach (string openedPrefabName in OpenedPrefabNames)
			{
				if (Resource<GameObject>.FindResourceByPath(openedPrefabName) is PrefabResource item2)
				{
					OpenedPrefabs.Add(item2);
				}
			}
		}
		else
		{
			OpenedPrefabs = null;
			OpenedPrefabNames = null;
		}
		if (DeadPrefabNames != null && DeadPrefabNames.Count > 0)
		{
			DeadPrefabs = new List<PrefabResource>();
			foreach (string deadPrefabName in DeadPrefabNames)
			{
				if (Resource<GameObject>.FindResourceByPath(deadPrefabName) is PrefabResource item3)
				{
					DeadPrefabs.Add(item3);
				}
			}
		}
		else
		{
			DeadPrefabs = null;
			DeadPrefabNames = null;
		}
		if (Stage1PrefabNames != null && Stage1PrefabNames.Count > 0)
		{
			Stage1Prefabs = new List<PrefabResource>();
			foreach (string stage1PrefabName in Stage1PrefabNames)
			{
				if (Resource<GameObject>.FindResourceByPath(stage1PrefabName) is PrefabResource item4)
				{
					Stage1Prefabs.Add(item4);
				}
			}
		}
		else
		{
			Stage1Prefabs = null;
			Stage1PrefabNames = null;
		}
		if (Stage2PrefabNames != null && Stage2PrefabNames.Count > 0)
		{
			Stage2Prefabs = new List<PrefabResource>();
			foreach (string stage2PrefabName in Stage2PrefabNames)
			{
				if (Resource<GameObject>.FindResourceByPath(stage2PrefabName) is PrefabResource item5)
				{
					Stage2Prefabs.Add(item5);
				}
			}
		}
		else
		{
			Stage2Prefabs = null;
			Stage2PrefabNames = null;
		}
		if (MaterialVariationNames != null && MaterialVariationNames.Count > 0)
		{
			MaterialVariations = new List<Resource<Material>>();
			foreach (string materialVariationName in MaterialVariationNames)
			{
				Resource<Material> resource = Resource<Material>.FindResourceByPath(materialVariationName);
				if (resource != null)
				{
					MaterialVariations.Add(resource);
				}
			}
		}
		else
		{
			MaterialVariations = null;
			MaterialVariationNames = null;
		}
		if (PosterVariationNames != null && PosterVariationNames.Count > 0)
		{
			PosterVariations = new List<Resource<Material>>();
			foreach (string posterVariationName in PosterVariationNames)
			{
				Resource<Material> resource2 = Resource<Material>.FindResourceByPath(posterVariationName);
				if (resource2 != null)
				{
					PosterVariations.Add(resource2);
				}
			}
		}
		else
		{
			PosterVariations = null;
			PosterVariationNames = null;
		}
		if (Entrances != null && Entrances.Length != 0)
		{
			for (int i = 0; i < Entrances.Length; i++)
			{
				Entrances[i].NameHash = ((!string.IsNullOrEmpty(Entrances[i].NativeName)) ? StringUtil.JenkinsHash("ENTRANCE_" + Entrances[i].NativeName.Replace(" ", "")) : 0);
			}
		}
		else
		{
			Entrances = null;
		}
		if (Inhabitants != null && Inhabitants.Length != 0)
		{
			for (int j = 0; j < Inhabitants.Length; j++)
			{
				Inhabitants[j].NameHash = ((!string.IsNullOrEmpty(Inhabitants[j].NativeName)) ? StringUtil.JenkinsHash("SLOT_" + Inhabitants[j].NativeName.Replace(" ", "")) : 0);
				if (!Inhabitants[j].External)
				{
					int num = ExtentsMax.x + 1 - ExtentsMin.x;
					int num2 = ExtentsMax.y + 1 - ExtentsMin.y;
					float num3 = (float)num / (float)num2;
					int num4 = Math.Max(1, Mathf.RoundToInt(Mathf.Sqrt((float)Inhabitants.Length / num3)));
					int num5 = Math.Max(1, Mathf.RoundToInt((float)Inhabitants.Length / (float)num4));
					if (TypeName == BaseObjectType.EnterableVehicle)
					{
						num5 = 2;
						num4 = Mathf.CeilToInt((float)Inhabitants.Length / (float)num5);
					}
					int num6 = j % num5;
					int num7 = j / num5;
					float x = Mathf.Lerp(ExtentsMin.x, (float)ExtentsMax.x + 1f, (0.5f + (float)num6) / (float)num5);
					float z = Mathf.Lerp(ExtentsMin.y, (float)ExtentsMax.y + 1f, (0.5f + (float)num7) / (float)num4);
					Inhabitants[j].Pos = new Vector3(x, 0f, z);
				}
			}
		}
		else
		{
			Inhabitants = null;
		}
		if (ConnectedWirePoints != null && ConnectedWirePoints.Length == 0)
		{
			ConnectedWirePoints = null;
		}
		if (ProtoInstance is EnterableVehicle)
		{
			if (VehicleGearRatios == null || VehicleGearRatios.Length < 3)
			{
				VehicleGearRatios = new float[7] { 4f, 3.2f, 2f, 1.3f, 1f, 0.75f, 3f };
			}
		}
		else if (VehicleGearRatios != null && VehicleGearRatios.Length == 0)
		{
			VehicleGearRatios = null;
		}
		if (GenderInLanguages != null && GenderInLanguages.Count == 0)
		{
			GenderInLanguages = null;
		}
	}

	public void UnloadIcon()
	{
		if (Tex != null)
		{
			Tex.UnloadResource();
			Tex = null;
		}
	}

	public static PropPrototype LoadFromFile(string fileName, Translation englishTranslation)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		if (string.IsNullOrEmpty(fileNameWithoutExtension))
		{
			return null;
		}
		try
		{
			if (File.Exists(fileName))
			{
				using (StreamReader textReader = new StreamReader(fileName))
				{
					PropPrototype propPrototype = (PropPrototype)new XmlSerializer(typeof(PropPrototype)).Deserialize(textReader);
					propPrototype.Name = fileNameWithoutExtension;
					propPrototype.NameHash = StringUtil.JenkinsHash(propPrototype.GetNameKey());
					propPrototype.Tex = Resource<Texture2D>.CreateIfFileExists(Path.GetDirectoryName(fileName) + "/" + propPrototype.IconPath);
					englishTranslation.Keys[propPrototype.NameHash] = propPrototype.NativeName;
					return propPrototype;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return null;
	}

	public bool SaveToFile(string fileName)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
				PropPrototype obj = new PropPrototype();
				FieldInfo[] fields = GetType().GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!fieldInfo.IsStatic)
					{
						object value = fieldInfo.GetValue(this);
						object value2 = fieldInfo.GetValue(obj);
						if (value != null && value.Equals(value2))
						{
							XmlAttributes xmlAttributes = new XmlAttributes();
							xmlAttributes.XmlIgnore = true;
							xmlAttributeOverrides.Add(typeof(PropPrototype), fieldInfo.Name, xmlAttributes);
						}
					}
				}
				new XmlSerializer(typeof(PropPrototype), xmlAttributeOverrides).Serialize(stream, this);
			}
			GameImpl.Instance.GetCurrentlyEditingStory().BuildTSVFile();
			Script.CopyFileBackToUnityFolder(fileName);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save '" + fileName + "': " + ex.ToString());
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_FileSaveFailed").Replace("%1", ex.Message));
			return false;
		}
	}

	public void CopyFrom(PropPrototype other)
	{
		FieldInfo[] fields = typeof(PropPrototype).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}

	public GenderType GetGenderInLanguage(Language language)
	{
		if (GenderInLanguages != null)
		{
			for (int i = 0; i < GenderInLanguages.Count; i++)
			{
				if (GenderInLanguages[i].Language == language)
				{
					return GenderInLanguages[i].Gender;
				}
			}
		}
		return Gender;
	}

	public void SetGenderInLanguage(Language language, GenderType gender)
	{
		if (GenderInLanguages == null)
		{
			GenderInLanguages = new List<GenderInLanguage>();
		}
		for (int i = 0; i < GenderInLanguages.Count; i++)
		{
			if (GenderInLanguages[i].Language == language)
			{
				GenderInLanguage genderInLanguage = GenderInLanguages[i];
				genderInLanguage.Gender = gender;
				GenderInLanguages[i] = genderInLanguage;
				return;
			}
		}
		GenderInLanguage genderInLanguage2 = new GenderInLanguage();
		genderInLanguage2.Language = language;
		genderInLanguage2.Gender = gender;
		GenderInLanguages.Add(genderInLanguage2);
	}

	public bool IsCorner()
	{
		if (Connectors != null)
		{
			if (Connectors.Length >= 3)
			{
				return true;
			}
			if (Connectors.Length == 2)
			{
				return Vector2.Dot(MathUtil.SafeNormalize(Connectors[0], Vector2.zero), MathUtil.SafeNormalize(Connectors[1], Vector2.zero)) > -0.5f;
			}
		}
		return false;
	}

	public bool IsEnterableBySpecies(BaseObjectType species)
	{
		if (EnterableBySpecies != BaseObjectType.Invalid)
		{
			return EnterableBySpecies == species;
		}
		return true;
	}

	public bool HasMaterialVariation(Material mat)
	{
		string text = mat.name.Replace(" (Instance)", string.Empty);
		foreach (Resource<Material> materialVariation in MaterialVariations)
		{
			if (materialVariation.GetAsset().name == text)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPosterVariation(Material mat)
	{
		string text = mat.name.Replace(" (Instance)", string.Empty);
		foreach (Resource<Material> posterVariation in PosterVariations)
		{
			if (posterVariation.GetAsset().name == text)
			{
				return true;
			}
		}
		return false;
	}

	public void MarkDiscovered()
	{
		if (!Discovered)
		{
			Discovered = true;
			if (Session.Instance.State == SessionState.Started)
			{
				NotificationManager.Instance.ShowRecipeDiscoveredNotifications();
			}
		}
	}
}

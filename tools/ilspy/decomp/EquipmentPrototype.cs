using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class EquipmentPrototype
{
	public static EquipmentPrototype Gold;

	public static EquipmentPrototype Bandage;

	public static EquipmentPrototype Antigen_Green;

	public static EquipmentPrototype Antigen_Blue;

	public static EquipmentPrototype Antigen_Red;

	public static EquipmentPrototype SpikedBaseballBat;

	public static EquipmentPrototype Axe;

	public static EquipmentPrototype HuntingKnife;

	public static EquipmentPrototype Shotgun;

	public static EquipmentPrototype AssaultRifle;

	public static EquipmentPrototype Bow;

	public static EquipmentPrototype Pistol;

	public static EquipmentPrototype MolotovCocktail;

	public static EquipmentPrototype Carrot;

	public static EquipmentPrototype Wood;

	public static EquipmentPrototype Stone;

	public static EquipmentPrototype LeadOre;

	public static EquipmentPrototype IronOre;

	public static EquipmentPrototype Match;

	public static EquipmentPrototype Flint;

	public static EquipmentPrototype SkinnedRabbit;

	public static EquipmentPrototype SkinnedChicken;

	public static EquipmentPrototype Arrow;

	public static EquipmentPrototype Pot;

	public static EquipmentPrototype FryingPan;

	public static EquipmentPrototype SealedContainer;

	public static EquipmentPrototype PlasticBottle;

	public static EquipmentPrototype DeadRabbit;

	public static EquipmentPrototype WateringCan;

	public static EquipmentPrototype Radio;

	public static EquipmentPrototype HumanMeat;

	public static EquipmentPrototype RancidHumanMeat;

	public static EquipmentPrototype PipeBomb;

	public static EquipmentPrototype WorldMap;

	public static EquipmentPrototype Pickaxe;

	public static EquipmentPrototype Snowball;

	public static EquipmentPrototype Venison;

	public static EquipmentPrototype Egg;

	public static EquipmentPrototype FertilizedEgg;

	public static EquipmentPrototype CornCookie;

	public static EquipmentPrototype ArmorPiercingARAmmo;

	public static EquipmentPrototype ArmorPiercingSniperAmmo;

	public static EquipmentPrototype VodkaBottle;

	public static EquipmentPrototype[] MiningResources;

	public static EquipmentPrototype[] Meat;

	public static List<EquipmentPrototype> HuntingKnives = new List<EquipmentPrototype>();

	public static List<EquipmentPrototype> Toolboxes = new List<EquipmentPrototype>();

	public static List<EquipmentPrototype> Shovels = new List<EquipmentPrototype>();

	public static List<EquipmentPrototype> AllAmmoTypes = new List<EquipmentPrototype>();

	public static bool[] ClothingTypesThatHaveInsulation = new bool[9];

	[XmlIgnore]
	[DefaultValue("")]
	public string Name = "";

	public BaseObjectType TypeName = BaseObjectType.Equipment;

	[DefaultValue("")]
	public string NativeName = "";

	[DefaultValue("")]
	public string NativeDescription = "";

	[DefaultValue("")]
	public string Category = "";

	[XmlIgnore]
	public int NameHash;

	[XmlIgnore]
	public int DescriptionHash;

	[XmlIgnore]
	public Resource<Texture2D> Tex;

	[DefaultValue(GenderType.Count)]
	public GenderType Gender = GenderType.Count;

	[DefaultValue(null)]
	public List<GenderInLanguage> GenderInLanguages;

	[DefaultValue(false)]
	public bool IsProperNoun;

	[DefaultValue(false)]
	public bool UsePluralIndefiniteArticle;

	[DefaultValue(false)]
	public bool Uncountable;

	[DefaultValue(0f)]
	public float Weight;

	[DefaultValue(0f)]
	public float BasePrice;

	[DefaultValue(false)]
	public bool CanBeEquipped;

	[DefaultValue(true)]
	public bool CanBeCombined = true;

	[DefaultValue(true)]
	public bool CanBeDestroyed = true;

	[DefaultValue(false)]
	public bool CanSurviveBuildingCollapse;

	[DefaultValue(true)]
	public bool CanBeAutoDeposited = true;

	[DefaultValue(false)]
	public bool CanMelt;

	[DefaultValue(true)]
	public bool CanHitTheRoad = true;

	[DefaultValue(false)]
	public bool HasCustomUseAction;

	[DefaultValue(false)]
	public bool Special;

	[DefaultValue(0)]
	public int SightRangeModifier;

	[DefaultValue(0f)]
	public float Nutrition;

	[OnlyVisibleForFood]
	[DefaultValue(0f)]
	public float Caffeine;

	[DefaultValue(0f)]
	public float Water;

	[OnlyVisibleForFood]
	[DefaultValue(0f)]
	public float Alcohol;

	[OnlyVisibleForFood]
	[DefaultValue(0f)]
	public float Tastiness;

	[OnlyVisibleForFood]
	[DefaultValue(false)]
	public bool Spicy;

	[OnlyVisibleForFood]
	[DefaultValue(false)]
	public bool ContainsHumanMeat;

	[DefaultValue(0f)]
	public float LiquidCapacity;

	[XmlIgnore]
	public LiquidPrototype DefaultLiquidPrototype;

	[OnlyVisibleForLiquidContainer]
	[DefaultValue(null)]
	public string DefaultLiquidType;

	[OnlyVisibleForLiquidContainer]
	[DefaultValue(100f)]
	public float DefaultLiquidFilledMax = 100f;

	[OnlyVisibleForLiquidContainer]
	[DefaultValue(100f)]
	public float DefaultLiquidFilledMin = 100f;

	[OnlyVisibleForFoodOrLiquidContainerOrGlassBottle]
	[DefaultValue(InfectionType.None)]
	public InfectionType DefaultInfectionTypeMin;

	[OnlyVisibleForFoodOrLiquidContainerOrGlassBottle]
	[DefaultValue(InfectionType.None)]
	public InfectionType DefaultInfectionTypeMax;

	[DefaultValue(-1)]
	public int BandageLevel = -1;

	[DefaultValue(InfectionType.None)]
	public InfectionType AntigenType;

	[OnlyVisibleForAntigen]
	[DefaultValue(false)]
	public bool Fake;

	[DefaultValue("")]
	public string SeedForPlantType = "";

	[XmlIgnore]
	public PropPrototype SeedForPlantTypeProto;

	[XmlIgnore]
	public PropPrototype HarvestedFromPlantProto;

	public string EquippedModelName = "";

	[XmlIgnore]
	public EquippedModelProperties EquippedModelProperties;

	[DefaultValue(ClothingType.Invalid)]
	public ClothingType ClothingType = ClothingType.Invalid;

	[DefaultValue(LootScarcity.None)]
	public LootScarcity Scarcity;

	[DefaultValue(0)]
	public int MaxSpawnable;

	[DefaultValue(null)]
	public List<LootLocation> LootLocations;

	[DefaultValue(null)]
	public List<LootableFrom> LootableFromLocations;

	public int TypicalLootAmount = 1;

	[DefaultValue(null)]
	public List<string> GiftFor;

	[DefaultValue(null)]
	public List<string> BadGiftFor;

	[OnlyVisibleForGifts]
	[DefaultValue(1)]
	public int GiftAmount = 1;

	[OnlyVisibleForGifts]
	[DefaultValue(0)]
	public int GiftLimit;

	[DefaultValue(null)]
	public List<BaseObjectType> FoodForAnimal;

	[DefaultValue(TradeBehaviour.Default)]
	public TradeBehaviour TradeBehaviour;

	[DefaultValue(null)]
	public Color32[] ColorVariations;

	[OnlyVisibleForClothing]
	[DefaultValue(null)]
	public Color32[] ColorVariations2;

	[OnlyVisibleForClothing]
	[DefaultValue(null)]
	public Color32[] ColorVariations3;

	[OnlyVisibleForClothing]
	[DefaultValue(null)]
	public string[] MaterialVariations;

	[OnlyVisibleForClothing]
	[DefaultValue(null)]
	public string MaleMeshName;

	[OnlyVisibleForClothing]
	[DefaultValue(null)]
	public string FemaleMeshName;

	[OnlyVisibleForClothing]
	[DefaultValue(0)]
	public int Insulation;

	public const int MaxInsulation = 4;

	public const int MaxTotalInsulation = 15;

	public float DamageAbsorption;

	[OnlyVisibleForClothing]
	[DefaultValue(0f)]
	public float CarryWeight;

	[OnlyVisibleForClothing]
	[DefaultValue(0f)]
	public float CarryWeightBonusPerSkillLevel;

	[DefaultValue(SkillType.Invalid)]
	public SkillType SkillBonusType = SkillType.Invalid;

	[OnlyVisibleForSkillBook]
	[DefaultValue(0)]
	public int SkillBonus;

	[OnlyVisibleForFood]
	[DefaultValue(SkillType.Invalid)]
	public SkillType SkillOnConsumptionType = SkillType.Invalid;

	[OnlyVisibleForFood]
	[DefaultValue(0)]
	public float SkillOnConsumptionProgression;

	[OnlyVisibleForType(typeof(Ammo))]
	[DefaultValue(0f)]
	public float ArmorPiercing;

	[OnlyVisibleForType(typeof(Ammo))]
	[DefaultValue(1f)]
	public float DamageModifier = 1f;

	[OnlyVisibleForType(typeof(Ammo))]
	[DefaultValue(0)]
	public int DefaultCarryAmount;

	[OnlyVisibleForType(typeof(Arrow))]
	[DefaultValue(25f)]
	public float ChanceOfArrowBreaking = 25f;

	[OnlyVisibleForType(typeof(Ammo))]
	public List<string> ExtraAmmoTypeForWeapons;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(InjuryType.Invalid)]
	public InjuryType InjuryType = InjuryType.Invalid;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float Damage;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float DamageBonusPerSkillLevel;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float Range;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float RangeBonusPerSkillLevel;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float AttackSoundRadius;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float Fuel;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float FatiguePenaltyWhenAiming;

	[OnlyVisibleForType(typeof(Weapon))]
	[DefaultValue(0f)]
	public float FatiguePenaltyWhenAttacking;

	[OnlyVisibleForType(typeof(RangedWeapon))]
	[DefaultValue(0f)]
	public float AccurateRange;

	[OnlyVisibleForType(typeof(RangedWeapon))]
	[DefaultValue(0f)]
	public float AccurateRangeBonusPerSkillLevel;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MinAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MaxAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MinHeadAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MaxHeadAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MinLegsAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MaxLegsAimTime;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(1f)]
	public float MinReloadSpeed = 1f;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(1f)]
	public float MaxReloadSpeed = 1f;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float Recoil;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float AimPenaltyWhenMoving;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0f)]
	public float MinTimeBetweenFiring;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue("")]
	public string AmmoType = "";

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	public List<string> AmmoTypes;

	[XmlIgnore]
	public List<EquipmentPrototype> AmmoPrototypes;

	[XmlIgnore]
	public List<EquipmentPrototype> AmmoForWeaponPrototypes;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(0)]
	public int MaxAmmo;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(1)]
	public int NumPelletsPerShot = 1;

	[OnlyVisibleForType(typeof(Throwable))]
	[DefaultValue(0f)]
	public float DamageRadius;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(null)]
	public List<string> FireSounds;

	[XmlIgnore]
	public List<Resource<AudioClip>> FireSoundResources;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(null)]
	public List<string> ReloadReleaseSounds;

	[XmlIgnore]
	public List<Resource<AudioClip>> ReloadReleaseSoundResources;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(null)]
	public List<string> ReloadInsertSounds;

	[XmlIgnore]
	public List<Resource<AudioClip>> ReloadInsertSoundResources;

	[OnlyVisibleForType(typeof(AmmoWeapon))]
	[DefaultValue(null)]
	public List<string> ReloadSlideSounds;

	[XmlIgnore]
	public List<Resource<AudioClip>> ReloadSlideSoundResources;

	[OnlyVisibleForType(typeof(Throwable))]
	[DefaultValue(null)]
	public List<string> ProjectileHitSounds;

	[XmlIgnore]
	public List<Resource<AudioClip>> ProjectileHitSoundResources;

	[OnlyVisibleForType(typeof(Throwable))]
	[DefaultValue(null)]
	public string HitEffectName;

	[XmlIgnore]
	public bool Discovered;

	[XmlIgnore]
	public int Counter;

	public string IconPath => "Icons/" + Name + ".png";

	public static void CacheEquipmentPrototypeRefs()
	{
		GameImpl instance = GameImpl.Instance;
		Gold = instance.FindEquipmentPrototypeByName("Gold");
		Bandage = instance.FindEquipmentPrototypeByName("Bandage");
		Antigen_Green = instance.FindEquipmentPrototypeByName("Antigen_Green");
		Antigen_Blue = instance.FindEquipmentPrototypeByName("Antigen_Blue");
		Antigen_Red = instance.FindEquipmentPrototypeByName("Antigen_Red");
		SpikedBaseballBat = instance.FindEquipmentPrototypeByName("SpikedBaseballBat");
		Axe = instance.FindEquipmentPrototypeByName("Axe");
		Shotgun = instance.FindEquipmentPrototypeByName("Shotgun");
		AssaultRifle = instance.FindEquipmentPrototypeByName("AssaultRifle");
		Bow = instance.FindEquipmentPrototypeByName("Bow");
		Pistol = instance.FindEquipmentPrototypeByName("Pistol");
		MolotovCocktail = instance.FindEquipmentPrototypeByName("MolotovCocktail");
		Carrot = instance.FindEquipmentPrototypeByName("Carrot");
		Wood = instance.FindEquipmentPrototypeByName("Wood");
		Stone = instance.FindEquipmentPrototypeByName("Stone");
		LeadOre = instance.FindEquipmentPrototypeByName("LeadOre");
		IronOre = instance.FindEquipmentPrototypeByName("IronOre");
		Match = instance.FindEquipmentPrototypeByName("Match");
		Flint = instance.FindEquipmentPrototypeByName("Flint");
		SkinnedRabbit = instance.FindEquipmentPrototypeByName("SkinnedRabbit");
		SkinnedChicken = instance.FindEquipmentPrototypeByName("SkinnedChicken");
		Arrow = instance.FindEquipmentPrototypeByName("Arrow");
		Pot = instance.FindEquipmentPrototypeByName("Pot");
		FryingPan = instance.FindEquipmentPrototypeByName("FryingPan");
		SealedContainer = instance.FindEquipmentPrototypeByName("SealedContainer");
		PlasticBottle = instance.FindEquipmentPrototypeByName("PlasticBottle");
		DeadRabbit = instance.FindEquipmentPrototypeByName("DeadRabbit");
		WateringCan = instance.FindEquipmentPrototypeByName("WateringCan");
		Radio = instance.FindEquipmentPrototypeByName("Radio");
		HumanMeat = instance.FindEquipmentPrototypeByName("HumanMeat");
		RancidHumanMeat = instance.FindEquipmentPrototypeByName("RancidHumanMeat");
		PipeBomb = instance.FindEquipmentPrototypeByName("PipeBomb");
		WorldMap = instance.FindEquipmentPrototypeByName("WorldMap");
		Pickaxe = instance.FindEquipmentPrototypeByName("Pickaxe");
		Snowball = instance.FindEquipmentPrototypeByName("Snowball");
		Venison = instance.FindEquipmentPrototypeByName("Venison");
		Egg = instance.FindEquipmentPrototypeByName("Egg");
		FertilizedEgg = instance.FindEquipmentPrototypeByName("FertilizedEgg");
		CornCookie = instance.FindEquipmentPrototypeByName("CornCookie");
		ArmorPiercingARAmmo = instance.FindEquipmentPrototypeByName("ArmorPiercingARAmmo");
		ArmorPiercingSniperAmmo = instance.FindEquipmentPrototypeByName("ArmorPiercingSniperAmmo");
		VodkaBottle = instance.FindEquipmentPrototypeByName("VodkaBottle");
		MiningResources = new EquipmentPrototype[4] { Stone, Flint, LeadOre, IronOre };
		Meat = new EquipmentPrototype[5] { SkinnedRabbit, SkinnedChicken, HumanMeat, RancidHumanMeat, Venison };
		AllAmmoTypes.Clear();
		Toolboxes.Clear();
		Shovels.Clear();
		HuntingKnives.Clear();
		foreach (KeyValuePair<string, EquipmentPrototype> item in instance.CurrentEquipmentPrototypesDeterministic)
		{
			item.Value.AmmoPrototypes = null;
			item.Value.AmmoForWeaponPrototypes = null;
		}
		foreach (KeyValuePair<string, EquipmentPrototype> item2 in instance.CurrentEquipmentPrototypesDeterministic)
		{
			if (item2.Value.AmmoTypes != null)
			{
				foreach (string ammoType in item2.Value.AmmoTypes)
				{
					EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(ammoType);
					if (equipmentPrototype != null)
					{
						item2.Value.AddAmmoPrototype(equipmentPrototype);
					}
				}
			}
			if (item2.Value.ExtraAmmoTypeForWeapons != null)
			{
				foreach (string extraAmmoTypeForWeapon in item2.Value.ExtraAmmoTypeForWeapons)
				{
					GameImpl.Instance.FindEquipmentPrototypeByName(extraAmmoTypeForWeapon)?.AddAmmoPrototype(item2.Value);
				}
			}
			ClothingType clothingType = item2.Value.ClothingType;
			if (clothingType != ClothingType.Invalid && item2.Value.Insulation != 0)
			{
				ClothingTypesThatHaveInsulation[(int)clothingType] = true;
			}
			switch (item2.Value.TypeName)
			{
			case BaseObjectType.Toolbox:
				Toolboxes.Add(item2.Value);
				break;
			case BaseObjectType.Shovel:
				Shovels.Add(item2.Value);
				break;
			case BaseObjectType.HuntingKnife:
				HuntingKnives.Add(item2.Value);
				break;
			}
		}
	}

	public void AddAmmoPrototype(EquipmentPrototype ammoPrototype)
	{
		if (!AllAmmoTypes.Contains(ammoPrototype))
		{
			AllAmmoTypes.Add(ammoPrototype);
		}
		if (AmmoPrototypes == null)
		{
			AmmoPrototypes = new List<EquipmentPrototype>();
		}
		AmmoPrototypes.Add(ammoPrototype);
		if (ammoPrototype.AmmoForWeaponPrototypes == null)
		{
			ammoPrototype.AmmoForWeaponPrototypes = new List<EquipmentPrototype>();
		}
		ammoPrototype.AmmoForWeaponPrototypes.Add(this);
	}

	public MineralType GetMineralType()
	{
		return (MineralType)Array.IndexOf(MiningResources, this);
	}

	public float GetNutrition()
	{
		return Nutrition * Sun.DayLengthSecs;
	}

	public float GetCaffeine()
	{
		return Caffeine * Sun.DayLengthSecs;
	}

	public float GetWater()
	{
		return Water * Sun.DayLengthSecs;
	}

	public PropPrototype GetSeedForPlantType()
	{
		if (SeedForPlantTypeProto == null && !string.IsNullOrEmpty(SeedForPlantType))
		{
			SeedForPlantTypeProto = GameImpl.Instance.FindPropPrototypeByName(SeedForPlantType);
		}
		return SeedForPlantTypeProto;
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

	public int GetNumMaterialVariations()
	{
		if (MaterialVariations == null)
		{
			return 0;
		}
		return MaterialVariations.Length;
	}

	public EquipmentPrototype GetDefaultAmmoPrototype()
	{
		if (AmmoPrototypes == null || AmmoPrototypes.Count <= 0)
		{
			return null;
		}
		return AmmoPrototypes[0];
	}

	public string GetNameKey()
	{
		return "EQUIPMENT_" + Name;
	}

	public string GetDescriptionKey()
	{
		return "DESC_" + Name;
	}

	public static EquipmentPrototype LoadFromFile(string fileName, Translation englishTranslation)
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
					EquipmentPrototype equipmentPrototype = (EquipmentPrototype)new XmlSerializer(typeof(EquipmentPrototype)).Deserialize(textReader);
					equipmentPrototype.Name = fileNameWithoutExtension;
					equipmentPrototype.Tex = Resource<Texture2D>.CreateIfFileExists(Path.GetDirectoryName(fileName) + "/" + equipmentPrototype.IconPath);
					equipmentPrototype.NameHash = StringUtil.JenkinsHash(equipmentPrototype.GetNameKey());
					equipmentPrototype.DescriptionHash = StringUtil.JenkinsHash(equipmentPrototype.GetDescriptionKey());
					englishTranslation.Keys[equipmentPrototype.NameHash] = equipmentPrototype.NativeName;
					englishTranslation.Keys[equipmentPrototype.DescriptionHash] = equipmentPrototype.NativeDescription;
					equipmentPrototype.EquippedModelProperties = ((equipmentPrototype.EquippedModelName.Length > 0) ? Equipment.GetEquippedModel(equipmentPrototype.EquippedModelName) : null);
					equipmentPrototype.DefaultLiquidPrototype = ((!string.IsNullOrEmpty(equipmentPrototype.DefaultLiquidType)) ? GameImpl.Instance.FindLiquidPrototypeByName(equipmentPrototype.DefaultLiquidType) : null);
					equipmentPrototype.Weight = Math.Max(equipmentPrototype.Weight, 0.001f);
					equipmentPrototype.Scarcity = (LootScarcity)MathUtil.Clamp((int)equipmentPrototype.Scarcity, 0, 5);
					if (equipmentPrototype.FatiguePenaltyWhenAttacking == 0f && BaseObjectManager.PrototypeGameObjects[(int)equipmentPrototype.TypeName] is MeleeWeapon)
					{
						equipmentPrototype.FatiguePenaltyWhenAttacking = 0.1f;
					}
					if (equipmentPrototype.LootLocations != null && equipmentPrototype.LootLocations.Count > 0 && (equipmentPrototype.LootableFromLocations == null || equipmentPrototype.LootableFromLocations.Count == 0))
					{
						equipmentPrototype.LootableFromLocations = new List<LootableFrom>();
						for (int i = 0; i < equipmentPrototype.LootLocations.Count; i++)
						{
							LootableFrom lootableFrom = new LootableFrom();
							lootableFrom.Name = equipmentPrototype.LootLocations[i].ToString();
							equipmentPrototype.LootableFromLocations.Add(lootableFrom);
						}
						equipmentPrototype.LootLocations = null;
					}
					if ((equipmentPrototype.AmmoTypes == null || equipmentPrototype.AmmoTypes.Count == 0) && !string.IsNullOrEmpty(equipmentPrototype.AmmoType))
					{
						equipmentPrototype.AmmoTypes = new List<string>();
						equipmentPrototype.AmmoTypes.Add(equipmentPrototype.AmmoType);
					}
					equipmentPrototype.AmmoType = string.Empty;
					CacheSounds(ref equipmentPrototype.FireSounds, ref equipmentPrototype.FireSoundResources);
					CacheSounds(ref equipmentPrototype.ReloadReleaseSounds, ref equipmentPrototype.ReloadReleaseSoundResources);
					CacheSounds(ref equipmentPrototype.ReloadInsertSounds, ref equipmentPrototype.ReloadInsertSoundResources);
					CacheSounds(ref equipmentPrototype.ReloadSlideSounds, ref equipmentPrototype.ReloadSlideSoundResources);
					CacheSounds(ref equipmentPrototype.ProjectileHitSounds, ref equipmentPrototype.ProjectileHitSoundResources);
					if (equipmentPrototype.GiftFor != null && equipmentPrototype.GiftFor.Count == 0)
					{
						equipmentPrototype.GiftFor = null;
					}
					if (equipmentPrototype.BadGiftFor != null && equipmentPrototype.BadGiftFor.Count == 0)
					{
						equipmentPrototype.BadGiftFor = null;
					}
					if (equipmentPrototype.FoodForAnimal != null && equipmentPrototype.FoodForAnimal.Count == 0)
					{
						equipmentPrototype.FoodForAnimal = null;
					}
					if (equipmentPrototype.LootableFromLocations != null && equipmentPrototype.LootableFromLocations.Count == 0)
					{
						equipmentPrototype.LootableFromLocations = null;
					}
					if (equipmentPrototype.LootLocations != null && equipmentPrototype.LootLocations.Count == 0)
					{
						equipmentPrototype.LootLocations = null;
					}
					if (equipmentPrototype.AmmoTypes != null && equipmentPrototype.AmmoTypes.Count == 0)
					{
						equipmentPrototype.AmmoTypes = null;
					}
					if (equipmentPrototype.ExtraAmmoTypeForWeapons != null && equipmentPrototype.ExtraAmmoTypeForWeapons.Count == 0)
					{
						equipmentPrototype.ExtraAmmoTypeForWeapons = null;
					}
					if (equipmentPrototype.GenderInLanguages != null && equipmentPrototype.GenderInLanguages.Count == 0)
					{
						equipmentPrototype.GenderInLanguages = null;
					}
					return equipmentPrototype;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return null;
	}

	private static void CacheSounds(ref List<string> sounds, ref List<Resource<AudioClip>> resources)
	{
		if (sounds != null && sounds.Count > 0)
		{
			resources = new List<Resource<AudioClip>>();
			{
				foreach (string sound in sounds)
				{
					Resource<AudioClip> resource = Resource<AudioClip>.FindResourceByPath(sound);
					if (resource != null)
					{
						resources.Add(resource);
					}
				}
				return;
			}
		}
		sounds = null;
	}

	public void CacheStuff()
	{
	}

	public bool SaveToFile(string fileName)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
				EquipmentPrototype obj = new EquipmentPrototype();
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
							xmlAttributeOverrides.Add(typeof(EquipmentPrototype), fieldInfo.Name, xmlAttributes);
						}
					}
				}
				new XmlSerializer(typeof(EquipmentPrototype), xmlAttributeOverrides).Serialize(stream, this);
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

	public void CopyFrom(EquipmentPrototype other)
	{
		FieldInfo[] fields = typeof(EquipmentPrototype).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}

	public LootScarcity GetLootScarcityForLocation(string name)
	{
		if (LootableFromLocations != null)
		{
			foreach (LootableFrom lootableFromLocation in LootableFromLocations)
			{
				if (lootableFromLocation.Name == name)
				{
					return (lootableFromLocation.OverrideScarcity != LootScarcity.None) ? lootableFromLocation.OverrideScarcity : Scarcity;
				}
			}
		}
		return LootScarcity.None;
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

	public bool IsClothingCompatible(EquipmentPrototype other)
	{
		if ((ClothingType == ClothingType.Glasses && other.TypeName == BaseObjectType.Balaklava) || (TypeName == BaseObjectType.Balaklava && other.ClothingType == ClothingType.Glasses))
		{
			return false;
		}
		return ClothingType != other.ClothingType;
	}

	public bool IsSuitableContainer(LiquidPrototype liquid)
	{
		if (LiquidCapacity > 0f)
		{
			if (liquid.CanPourIntoBottles)
			{
				return true;
			}
			if (BaseObjectManager.PrototypeGameObjects[(int)TypeName] is Equipment equipment && equipment.IsBottle())
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBeGift()
	{
		if (GiftFor != null)
		{
			return GiftFor.Count > 0;
		}
		return false;
	}

	public bool IsBookInSameSet(EquipmentPrototype other)
	{
		if (TypeName == BaseObjectType.Book && GiftFor != null && other.GiftFor != null)
		{
			return GiftFor.SequenceEqual(other.GiftFor);
		}
		return false;
	}

	public bool IsAmmo()
	{
		if (AmmoForWeaponPrototypes != null)
		{
			return AmmoForWeaponPrototypes.Count > 0;
		}
		return false;
	}

	public Color32 GetSpecialIconColor()
	{
		return EquipmentIconBehaviour.SpecialIconCol;
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

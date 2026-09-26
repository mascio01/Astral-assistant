using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Equipment : BaseObject
{
	public static EquipmentSettings EquipmentSettings;

	public static string SpoonProperties = "Prefabs/Equipment/Spoon";

	public static string HammerProperties = "Models/Equipment/Melee Weapon Pack/Hammer/Wooden_Hammer";

	protected EquipmentPrototype Prototype;

	public int ClothingIconIndex = -1;

	public int ColorVariation;

	public int ColorVariation2;

	public int ColorVariation3;

	public int MaterialVariation;

	public TileObject InventoryOwner;

	private int Amount;

	private int TradedAmount;

	private int GatheredAmount;

	public bool Gifted;

	public bool Concealed;

	public LiquidPrototype DesignatedLiquid;

	private LiquidPrototype LiquidContentsType;

	private float LiquidContentsAmount;

	private float Dampness;

	protected TimeSpan LastRareUpdateTime;

	public InfectionType InfectedWith;

	public static float SnowballMeltTemp = 5f;

	public override bool Deleted => InventoryOwner == null;

	public static EquippedModelProperties GetEquippedModel(string path)
	{
		for (int num = GameImpl.Instance.CurrentStories.Count - 1; num >= 0; num--)
		{
			Story story = GameImpl.Instance.CurrentStories[num];
			if (story.PrefabSettings != null)
			{
				foreach (EquippedModelProperties equippedModel in story.PrefabSettings.EquippedModels)
				{
					if (equippedModel.PrefabPath == path)
					{
						return equippedModel;
					}
				}
			}
		}
		if (EquipmentSettings != null)
		{
			EquippedModelProperties[] equippedModels = EquipmentSettings.EquippedModels;
			foreach (EquippedModelProperties equippedModelProperties in equippedModels)
			{
				if (equippedModelProperties.PrefabPath == path)
				{
					return equippedModelProperties;
				}
			}
		}
		return null;
	}

	public static void LoadContent()
	{
		EquipmentSettings = GameObject.Find("EquipmentSettings").GetComponent<EquipmentSettings>();
	}

	public virtual void Infect(InfectionType infection)
	{
		InfectedWith = infection;
	}

	public void SetInventoryOwner(TileObject owner)
	{
		InventoryOwner = owner;
		CheckWantRareUpdate();
	}

	private void CheckWantRareUpdate()
	{
		if (LastRareUpdateTime == TimeSpan.Zero && WantRareUpdate())
		{
			LastRareUpdateTime = Session.Instance.PlayTime;
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
		else if (LastRareUpdateTime != TimeSpan.Zero && !WantRareUpdate())
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			LastRareUpdateTime = TimeSpan.Zero;
		}
	}

	public void MarkDiscovered()
	{
		Prototype.MarkDiscovered();
		if (LiquidContentsType != null)
		{
			LiquidContentsType.MarkDiscovered();
		}
	}

	public float GetDampness()
	{
		return Dampness;
	}

	public void SetDampness(float dampness)
	{
		Dampness = dampness;
		CheckWantRareUpdate();
	}

	public static Equipment Create(EquipmentPrototype prototype)
	{
		Type type = BaseObjectManager.BaseObjectTypes[(int)prototype.TypeName];
		if (type == null)
		{
			type = typeof(Equipment);
		}
		Equipment obj = Activator.CreateInstance(type) as Equipment;
		obj.Prototype = prototype;
		return obj;
	}

	public static Equipment CreateCopy(Equipment item)
	{
		if ((int)item.GetBaseObjectType() >= BaseObjectManager.BaseObjectTypes.Length)
		{
			return Create(item.GetPrototype());
		}
		Type type = BaseObjectManager.BaseObjectTypes[(int)item.GetBaseObjectType()];
		if (type == null)
		{
			type = typeof(Equipment);
		}
		Equipment obj = Activator.CreateInstance(type) as Equipment;
		obj.Prototype = item.GetPrototype();
		return obj;
	}

	public static Equipment Spawn(EquipmentPrototype prototype)
	{
		return Spawn(prototype, 1);
	}

	public static Equipment Spawn(EquipmentPrototype prototype, int amount)
	{
		if (prototype == null)
		{
			return null;
		}
		if (amount <= 0)
		{
			Debug.LogError("Spawning equipment with 0 amount: " + prototype.Name + ", " + amount);
		}
		Equipment equipment = Create(prototype);
		equipment.Amount = amount;
		equipment.OnSpawn();
		return equipment;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		Session.Instance.OnSpawnedEquipment(Prototype, Amount);
	}

	public override void Init()
	{
		base.Init();
		if (WantRareUpdate())
		{
			if (LastRareUpdateTime == TimeSpan.Zero)
			{
				LastRareUpdateTime = Session.Instance.PlayTime;
			}
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
	}

	public override void Delete()
	{
		if (WantRareUpdate())
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
			LastRareUpdateTime = TimeSpan.Zero;
		}
		base.Delete();
	}

	public virtual bool WantRareUpdate()
	{
		if (Dampness > 0f && InventoryOwner is Prop)
		{
			return true;
		}
		if (LiquidContentsType == LiquidPrototype.Snow && LiquidContentsType != null)
		{
			return true;
		}
		if (Prototype != null)
		{
			return Prototype.CanMelt;
		}
		return false;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		if (LiquidContentsType == LiquidPrototype.Snow && Session.Instance.Weather.TemperatureInCelsius >= SnowballMeltTemp)
		{
			LiquidContentsType = LiquidPrototype.Water;
		}
		if (Dampness > 0f)
		{
			TimeSpan timeSpan = Session.Instance.PlayTime - LastRareUpdateTime;
			Dampness = Math.Max(0f, Dampness - (float)timeSpan.TotalSeconds * Character.IndoorEvaporationRate);
		}
		LastRareUpdateTime = Session.Instance.PlayTime;
		stillNeedUpdating = WantRareUpdate();
	}

	public override bool PropWantDelete()
	{
		if (Prototype.CanMelt && Session.Instance.Weather.TemperatureInCelsius > SnowballMeltTemp)
		{
			return true;
		}
		return false;
	}

	public static Equipment SpawnRandomVariation(EquipmentPrototype prototype, CustomRandom rand)
	{
		Equipment equipment = Spawn(prototype, 1);
		equipment.RandomiseVariations(rand);
		return equipment;
	}

	public void RandomiseVariations(CustomRandom rand)
	{
		if (Prototype.ColorVariations != null)
		{
			ColorVariation = ((Prototype.ColorVariations.Length != 0) ? rand.Next(Prototype.ColorVariations.Length) : 0);
		}
		if (Prototype.ColorVariations2 != null)
		{
			ColorVariation2 = ((Prototype.ColorVariations2.Length != 0) ? rand.Next(Prototype.ColorVariations2.Length) : 0);
		}
		if (Prototype.ColorVariations3 != null)
		{
			ColorVariation3 = ((Prototype.ColorVariations3.Length != 0) ? rand.Next(Prototype.ColorVariations3.Length) : 0);
		}
		if (Prototype.MaterialVariations != null && GetColor().IsEqual(MathUtil.White) && GetColor3().IsEqual(MathUtil.White))
		{
			MaterialVariation = ((Prototype.MaterialVariations.Length != 0) ? rand.Next(Prototype.MaterialVariations.Length) : 0);
		}
	}

	public Equipment()
	{
		Amount = 1;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Equipment;
	}

	public EquipmentPrototype GetPrototype()
	{
		return Prototype;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		if (LiquidContentsType != null)
		{
			return LiquidContentsType.GetGenderInLanguage(language);
		}
		if (Prototype == null)
		{
			return base.GetGender(language);
		}
		return Prototype.GetGenderInLanguage(language);
	}

	public override bool IsPlural()
	{
		return Prototype.UsePluralIndefiniteArticle;
	}

	public override bool IsUncountable()
	{
		if (!Prototype.Uncountable)
		{
			return LiquidContentsType != null;
		}
		return true;
	}

	public override bool IsMany()
	{
		return GetAmount() >= 5;
	}

	public override bool IsZero()
	{
		return GetAmount() == 0;
	}

	public override void OnStoryReloaded()
	{
		if (Prototype != null)
		{
			Prototype = GameImpl.Instance.FindEquipmentPrototypeByName(Prototype.Name);
		}
	}

	public void AddPrototypeCounters()
	{
		if (Prototype != null)
		{
			Prototype.Counter += Amount;
		}
		if (LiquidContentsType != null)
		{
			LiquidContentsType.Counter += LiquidContentsAmount;
		}
	}

	public int GetClothingVariationHash()
	{
		return (int)((uint)Prototype.NameHash ^ MathUtil.WangHash((uint)MaterialVariation) ^ MathUtil.WangHash((uint)(ColorVariation + 1000)) ^ MathUtil.WangHash((uint)(ColorVariation2 + 2000)) ^ MathUtil.WangHash((uint)(ColorVariation3 + 3000)));
	}

	public override Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		col = Color.white;
		if (Prototype.ClothingType != ClothingType.Invalid)
		{
			if ((Prototype.ColorVariations != null && Prototype.ColorVariations.Length > 1) || (Prototype.ColorVariations2 != null && Prototype.ColorVariations2.Length > 1) || (Prototype.ColorVariations3 != null && Prototype.ColorVariations3.Length > 1) || (Prototype.MaterialVariations != null && Prototype.MaterialVariations.Length > 1))
			{
				int clothingVariationHash = GetClothingVariationHash();
				Texture2D texture2D = IconGenerator.Instance.ClothingIconPool.GetTexture(ClothingIconIndex, clothingVariationHash);
				if (texture2D == null)
				{
					texture2D = IconGenerator.Instance.ClothingIconPool.FindSharedTexture(clothingVariationHash, out ClothingIconIndex);
				}
				if (texture2D == null)
				{
					IconGenerator.Instance.Queue.Push(IconGenerationRequest.Clothing(this));
				}
				col = ((texture2D != null) ? Color.white : MathUtil.TransparentBlackCol);
				return texture2D;
			}
		}
		else if (Prototype.GetNumColorVariations() > 0)
		{
			col = Prototype.ColorVariations[ColorVariation % Prototype.ColorVariations.Length];
		}
		if (Prototype.Tex == null)
		{
			return null;
		}
		return Prototype.Tex.GetAsset();
	}

	public Texture2D GetIconWithoutGenerating(out Color col)
	{
		col = Color.white;
		if (Prototype.GetNumColorVariations() > 0 && Prototype.ClothingType == ClothingType.Invalid)
		{
			col = Prototype.ColorVariations[ColorVariation % Prototype.ColorVariations.Length];
		}
		if (Prototype.Tex == null)
		{
			return null;
		}
		return Prototype.Tex.GetAsset();
	}

	public virtual void SetupClothingIconCam(Camera cam, Character iconGimp)
	{
		float orthographicSize = 0.5f;
		Vector3 vector = Vector3.zero;
		Vector3 forward = iconGimp.Forward;
		switch (GetClothingType())
		{
		case ClothingType.Hat:
			vector = new Vector3(-0.05f, 1.9f, 0f) * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.2f;
			iconGimp.SetFacingAngle(-MathF.PI / 2f);
			break;
		case ClothingType.Glasses:
			vector = Vector3.up * 1.85f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.1f;
			break;
		case ClothingType.Top:
			vector = Vector3.up * 1.4f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.4f;
			break;
		case ClothingType.Bottom:
			vector = Vector3.up * 0.65f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.5f;
			break;
		case ClothingType.Shoes:
			vector = new Vector3(-0.02f, 0.1f, 0f) * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.25f;
			iconGimp.SetFacingAngle(-MathF.PI / 2f);
			break;
		case ClothingType.Backpack:
			vector = Vector3.up * 1.4f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.4f;
			iconGimp.SetFacingAngle(MathF.PI);
			break;
		case ClothingType.BodyArmor:
			vector = Vector3.up * 1.4f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.4f;
			break;
		case ClothingType.LegArmor:
			vector = Vector3.up * 0.5f * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.4f;
			break;
		case ClothingType.Gloves:
			vector = new Vector3(0f, 1f, 0f) * iconGimp.Appearance.OverallScale;
			orthographicSize = 0.25f;
			iconGimp.SetFacingAngle(-MathF.PI / 2f);
			break;
		}
		Vector3 vector2 = iconGimp.Pos + vector;
		cam.orthographicSize = orthographicSize;
		cam.transform.position = vector2 + forward;
		cam.transform.LookAt(vector2, Vector3.up);
	}

	public int GetAmount()
	{
		return Amount;
	}

	public int GetTradedAmount()
	{
		return TradedAmount;
	}

	public int GetGatheredAmount()
	{
		return GatheredAmount;
	}

	public bool WasGifted()
	{
		return Gifted;
	}

	public void IncrementAmount(int inc)
	{
		if (inc < 0)
		{
			TradedAmount = Math.Max(0, TradedAmount + inc);
			GatheredAmount = Math.Max(0, GatheredAmount + inc);
		}
		Amount += inc;
		if (Prototype.SkillBonusType != SkillType.Invalid && InventoryOwner is Character character)
		{
			character.ClearCachedSkillLevelWithEffects(Prototype.SkillBonusType);
		}
	}

	public void CombineAmount(Equipment with)
	{
		Amount += with.Amount;
		TradedAmount += with.TradedAmount;
		IncrementGatheredAmount(with.GatheredAmount);
		if (Prototype.SkillBonusType != SkillType.Invalid && InventoryOwner is Character character)
		{
			character.ClearCachedSkillLevelWithEffects(Prototype.SkillBonusType);
		}
	}

	public void SetNewAmount(int amount)
	{
		Amount = amount;
		TradedAmount = Math.Min(TradedAmount, Amount);
		GatheredAmount = Math.Min(GatheredAmount, Amount);
		if (Prototype.SkillBonusType != SkillType.Invalid && InventoryOwner is Character character)
		{
			character.ClearCachedSkillLevelWithEffects(Prototype.SkillBonusType);
		}
	}

	public void SetTraded(out int amountMadeTraded)
	{
		amountMadeTraded = Amount - TradedAmount;
		TradedAmount = Amount;
	}

	public void ForceSetTradedAmount(int tradedAmount)
	{
		TradedAmount = Math.Min(Amount, tradedAmount);
	}

	public void ForceSetGatheredAmount(int gatheredAmount)
	{
		GatheredAmount = Math.Min(Amount, gatheredAmount);
	}

	public void SetGatheredAmount(int gatheredAmount)
	{
		GatheredAmount = Math.Min(Amount, gatheredAmount);
		if (GatheredAmount <= 0)
		{
			return;
		}
		if (InventoryOwner is Character character)
		{
			float targetAmountToCarryIncludingAmmo = character.GetTargetAmountToCarryIncludingAmmo(Prototype, LiquidContentsType, InfectedWith);
			if (LiquidContentsType != null)
			{
				GatheredAmount = Math.Min(Amount, gatheredAmount);
				if (targetAmountToCarryIncludingAmmo > 0f)
				{
					float totalLiquid = character.Inventory.GetTotalLiquid(LiquidContentsType, InfectedWith);
					float totalGatheredLiquid = character.Inventory.GetTotalGatheredLiquid(LiquidContentsType, InfectedWith);
					if (totalLiquid - totalGatheredLiquid < targetAmountToCarryIncludingAmmo)
					{
						GatheredAmount = 0;
					}
				}
			}
			else if (CanBeCombined())
			{
				GatheredAmount = Math.Min(Math.Max(0, Amount - (int)targetAmountToCarryIncludingAmmo), GatheredAmount);
			}
			else
			{
				int num = character.Inventory.CountItemsOfType(Prototype, InfectedWith);
				int num2 = character.Inventory.CountGatheredItemsOfType(Prototype, InfectedWith, this);
				if ((float)(num - num2) < targetAmountToCarryIncludingAmmo)
				{
					GatheredAmount = 0;
				}
			}
		}
		else
		{
			GatheredAmount = 0;
		}
	}

	public void IncrementGatheredAmount(int gatheredAmount)
	{
		SetGatheredAmount(GatheredAmount + gatheredAmount);
	}

	public void SetGathered()
	{
		SetGatheredAmount(Amount);
	}

	public void ClearGathered()
	{
		SetGatheredAmount(0);
	}

	public LiquidPrototype GetLiquidContentsType()
	{
		return LiquidContentsType;
	}

	public float GetLiquidContentsAmount()
	{
		return LiquidContentsAmount;
	}

	public void SetLiquid(LiquidPrototype liquidType, float amount)
	{
		LiquidContentsType = liquidType;
		LiquidContentsAmount = amount;
		if (LiquidContentsType != null && InventoryOwner != null)
		{
			Community community = InventoryOwner.GetCommunity();
			if (community != null && community.CommunityType == CommunityType.Player)
			{
				LiquidContentsType.MarkDiscovered();
			}
		}
		CheckWantRareUpdate();
	}

	public void FillLiquid(LiquidPrototype liquidType, float maxAmount, InfectionType infectedWith)
	{
		if (LiquidContentsType != null && LiquidContentsType != liquidType)
		{
			DrainLiquid(LiquidContentsAmount);
		}
		SetLiquid(liquidType, Math.Min(GetLiquidCapacity(), LiquidContentsAmount + maxAmount));
		InfectedWith = (InfectionType)Math.Max((int)InfectedWith, (int)infectedWith);
	}

	public float DrainLiquid(float maxAmount)
	{
		if (maxAmount < 0f)
		{
			Debug.LogWarning(GetDisplayNameString() + " draining negative amount: " + maxAmount);
			maxAmount = 0f;
		}
		float num = Math.Min(LiquidContentsAmount, maxAmount);
		LiquidContentsAmount -= num;
		if (LiquidContentsAmount <= 0.0001f)
		{
			SetLiquid(null, 0f);
			InfectedWith = InfectionType.None;
			Gifted = false;
		}
		return num;
	}

	public void SetDesignatedLiquid(LiquidPrototype liquid)
	{
		DesignatedLiquid = liquid;
	}

	public bool CanBeGiftItem()
	{
		if (WasGifted())
		{
			return false;
		}
		if (Prototype.CanBeGift())
		{
			return true;
		}
		if (LiquidContentsType != null && LiquidContentsType.CanBeGift())
		{
			return true;
		}
		return false;
	}

	public float CalcGiftUnits()
	{
		if (Prototype.CanBeGift())
		{
			return (float)GetAmount() / (float)Prototype.GiftAmount;
		}
		if (LiquidContentsType != null && LiquidContentsType.CanBeGift())
		{
			return GetLiquidContentsAmount() / LiquidContentsType.GiftAmount;
		}
		return 0f;
	}

	public virtual float GetPickpocketNoticeability(Character pickpocket)
	{
		float num = pickpocket.GetSkillLevelWithEffects(SkillType.Stealth);
		return (1f + GetWeight() * 10f + GetBasePrice() * 0.5f) * Mathf.Lerp(10f, 1f, num / 5f);
	}

	public string GetCategory()
	{
		if (LiquidContentsType == null)
		{
			return Prototype.Category + "/" + Prototype.Name;
		}
		return LiquidContentsType.Category + "/" + LiquidContentsType.Name;
	}

	public float GetWeight()
	{
		return Prototype.Weight;
	}

	public float GetBasePrice()
	{
		return Prototype.BasePrice + GetLiquidContentsPrice();
	}

	public float GetLiquidContentsPrice()
	{
		if (LiquidContentsType == null)
		{
			return 0f;
		}
		return LiquidContentsType.BasePricePerFlOz * LiquidContentsAmount;
	}

	public bool CanBeEquipped()
	{
		return Prototype.CanBeEquipped;
	}

	public bool CanBeCombined()
	{
		return Prototype.CanBeCombined;
	}

	public virtual bool CanBeCombinedWith(Equipment other)
	{
		if (other.Prototype == Prototype && Prototype.CanBeCombined && other.InfectedWith == InfectedWith)
		{
			return other.Gifted == Gifted;
		}
		return false;
	}

	public bool CanBeDestroyed()
	{
		return Prototype.CanBeDestroyed;
	}

	public bool CanSurviveBuildingCollapse()
	{
		return Prototype.CanSurviveBuildingCollapse;
	}

	public int GetSightRangeModifierWhenEquipped()
	{
		return Prototype.SightRangeModifier;
	}

	public float GetNutrition()
	{
		if (LiquidContentsType == null)
		{
			return Prototype.GetNutrition();
		}
		return LiquidContentsType.GetNutritionPerFlOz() * LiquidContentsAmount;
	}

	public float GetTastiness()
	{
		if (LiquidContentsType == null)
		{
			return Prototype.Tastiness;
		}
		return LiquidContentsType.Tastiness;
	}

	public float GetAlcoholContent()
	{
		if (LiquidContentsType == null)
		{
			return 0f;
		}
		return LiquidContentsType.AlcoholContent;
	}

	public float GetLiquidCapacity()
	{
		return Prototype.LiquidCapacity;
	}

	public virtual bool IsBottle()
	{
		return false;
	}

	public bool IsEdible()
	{
		if (!(Prototype.GetNutrition() > 0f))
		{
			if (LiquidContentsType != null)
			{
				return LiquidContentsType.Edible;
			}
			return false;
		}
		return true;
	}

	public bool IsDrinkable()
	{
		if (Prototype.GetWater() == 0f)
		{
			if (LiquidContentsType != null)
			{
				return LiquidContentsType.Drinkable;
			}
			return false;
		}
		return true;
	}

	public int GetBandageLevel()
	{
		return Prototype.BandageLevel;
	}

	public int GetInsulation()
	{
		return Prototype.Insulation;
	}

	public int GetInsulationIncludingDampness()
	{
		return (int)((float)Prototype.Insulation * (1f - Dampness) + 0.5f);
	}

	public InfectionType GetAntigenType()
	{
		return Prototype.AntigenType;
	}

	public bool IsFake()
	{
		return Prototype.Fake;
	}

	public PropPrototype GetSeedForPlantType()
	{
		return Prototype.GetSeedForPlantType();
	}

	public PropPrototype GetHarvestedFromPlantProto()
	{
		return Prototype.HarvestedFromPlantProto;
	}

	public ClothingType GetClothingType()
	{
		return Prototype.ClothingType;
	}

	public Color32 GetColor()
	{
		if (Prototype.ColorVariations == null || ColorVariation >= Prototype.ColorVariations.Length)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		return Prototype.ColorVariations[ColorVariation];
	}

	public Color32 GetColor2()
	{
		if (Prototype.ColorVariations2 == null || ColorVariation2 >= Prototype.ColorVariations2.Length)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		return Prototype.ColorVariations2[ColorVariation2];
	}

	public Color32 GetColor3()
	{
		if (Prototype.ColorVariations3 == null || ColorVariation3 >= Prototype.ColorVariations3.Length)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		return Prototype.ColorVariations3[ColorVariation3];
	}

	public string GetMaterialName()
	{
		if (Prototype.MaterialVariations == null || MaterialVariation >= Prototype.MaterialVariations.Length)
		{
			return null;
		}
		return Prototype.MaterialVariations[MaterialVariation];
	}

	public string GetMeshName(GenderType gender)
	{
		if (gender != GenderType.Male)
		{
			return Prototype.FemaleMeshName;
		}
		return Prototype.MaleMeshName;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (Prototype.NameHash != 0)
		{
			sb.Append(GameImpl.Translate(Prototype.NameHash, englishOnly));
		}
	}

	public void BuildDescriptionString(StringBuilder sb)
	{
		if (Prototype.DescriptionHash != 0)
		{
			sb.Append(GameImpl.Translate(Prototype.DescriptionHash));
		}
	}

	public GameObject GetEquippedModel()
	{
		if (Prototype.EquippedModelProperties == null || Prototype.EquippedModelProperties.Prefab == null)
		{
			return null;
		}
		return Prototype.EquippedModelProperties.Prefab.GetAsset();
	}

	public GameObject GetLoadedAmmoModel()
	{
		if (Prototype.EquippedModelProperties == null || Prototype.EquippedModelProperties.LoadedAmmoPrefab == null)
		{
			return null;
		}
		return Prototype.EquippedModelProperties.LoadedAmmoPrefab.GetAsset();
	}

	public virtual bool ShowLoadedAmmo()
	{
		return false;
	}

	public Bone GetEquippedBone()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Bone.Invalid;
		}
		return Prototype.EquippedModelProperties.EquippedBone;
	}

	public Bone GetLoadedAmmoBone()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Bone.Invalid;
		}
		return Prototype.EquippedModelProperties.LoadedAmmoBone;
	}

	public ActionAnim GetEquipAction()
	{
		if (Prototype.EquippedModelProperties != null)
		{
			switch (Prototype.EquippedModelProperties.EquippedAnim)
			{
			case EquippedAnim.OneHanded:
				return ActionAnim.EquipMelee;
			case EquippedAnim.Pistol:
				return ActionAnim.EquipPistol;
			case EquippedAnim.Rifle:
				return ActionAnim.EquipRifle;
			case EquippedAnim.Throwable:
				return ActionAnim.EquipThrowable;
			case EquippedAnim.Bow:
				return ActionAnim.EquipBow;
			}
		}
		return ActionAnim.EquipUnarmed;
	}

	public Vector3 GetEquippedLocalPos()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Vector3.zero;
		}
		return Prototype.EquippedModelProperties.LocalPos;
	}

	public Vector3 GetEquippedLocalRotation()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Vector3.zero;
		}
		return Prototype.EquippedModelProperties.LocalRotation;
	}

	public Vector3 GetEquippedLocalScale()
	{
		return Vector3.one * ((Prototype.EquippedModelProperties != null) ? Prototype.EquippedModelProperties.LocalScale : 1f);
	}

	public Vector3 GetMuzzleLocalPos()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Vector3.zero;
		}
		return Prototype.EquippedModelProperties.MuzzleLocalPos;
	}

	public Vector3 GetLoadedAmmoLocalPos()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Vector3.zero;
		}
		return Prototype.EquippedModelProperties.LoadedAmmoLocalPos;
	}

	public Vector3 GetLoadedAmmoLocalRotation()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return Vector3.zero;
		}
		return Prototype.EquippedModelProperties.LoadedAmmoLocalRotation;
	}

	public Vector3 GetLoadedAmmoLocalScale()
	{
		return Vector3.one * ((Prototype.EquippedModelProperties != null) ? Prototype.EquippedModelProperties.LoadedAmmoLocalScale : 1f);
	}

	public EquippedAnim GetEquippedAnim()
	{
		if (Prototype.EquippedModelProperties == null)
		{
			return EquippedAnim.None;
		}
		return Prototype.EquippedModelProperties.EquippedAnim;
	}

	public float GetFatiguePenaltyWhenAiming()
	{
		return Prototype.FatiguePenaltyWhenAiming;
	}

	public float GetFatiguePenaltyWhenAttacking()
	{
		return Prototype.FatiguePenaltyWhenAttacking;
	}

	public int GetNumPelletsPerShot()
	{
		return Prototype.NumPelletsPerShot;
	}

	public float GetAttackSoundRadius()
	{
		return Prototype.AttackSoundRadius;
	}

	public virtual bool AllowLockOn(Character character)
	{
		return false;
	}

	public virtual float GetBaseDamage()
	{
		return Prototype.Damage;
	}

	public virtual InjuryType GetInjuryType()
	{
		return Prototype.InjuryType;
	}

	public virtual SkillType GetRangeSkillType()
	{
		return SkillType.Invalid;
	}

	public virtual SkillType GetDamageSkillType()
	{
		return SkillType.Invalid;
	}

	public virtual int GetCurrentAmmo()
	{
		return 0;
	}

	public virtual EquipmentPrototype GetCurrentAmmoType()
	{
		return null;
	}

	public virtual int GetMapQuadrant()
	{
		return 0;
	}

	public virtual float GetArmorProtection()
	{
		return 0f;
	}

	public bool MatchesIngredientInfectionState(IngredientInfectionState ingredientInfectionState)
	{
		return ingredientInfectionState switch
		{
			IngredientInfectionState.Any => true, 
			IngredientInfectionState.CantBeInfected => InfectedWith == InfectionType.None, 
			IngredientInfectionState.MustBeInfected => InfectedWith != InfectionType.None, 
			_ => true, 
		};
	}

	public override Community GetCommunity()
	{
		if (InventoryOwner != null)
		{
			return InventoryOwner.GetCommunity();
		}
		return base.GetCommunity();
	}

	public override Character GetAsCharacter()
	{
		return InventoryOwner as Character;
	}

	public override TerrainCoord GetTile()
	{
		if (InventoryOwner == null)
		{
			return base.GetTile();
		}
		return InventoryOwner.GetTile();
	}

	public virtual float GetDamageIncludingEffects(Character character, EquipmentPrototype overrideAmmoType = null)
	{
		float num = ((character != null) ? ((float)character.GetSkillLevelWithEffects(GetDamageSkillType()) * Prototype.DamageBonusPerSkillLevel) : 0f);
		return GetBaseDamage() + num;
	}

	public virtual float GetRangeIncludingEffects(Character character, TileObject targetObj, out float accurateRange, bool includingBuildingEffects, out float accurateRangeAtLowestSkill)
	{
		int num = character?.GetSkillLevelWithEffects(GetRangeSkillType()) ?? 0;
		accurateRange = Prototype.AccurateRange + (float)num * Prototype.AccurateRangeBonusPerSkillLevel;
		accurateRangeAtLowestSkill = Prototype.AccurateRange;
		return Prototype.Range + (float)num * Prototype.RangeBonusPerSkillLevel;
	}

	public float GetRangeIncludingEffects(Character character, TileObject targetObj, out float accurateRange)
	{
		float accurateRangeAtLowestSkill;
		return GetRangeIncludingEffects(character, targetObj, out accurateRange, includingBuildingEffects: true, out accurateRangeAtLowestSkill);
	}

	public float GetRangeIncludingEffects(Character character, TileObject targetObj)
	{
		float accurateRange;
		return GetRangeIncludingEffects(character, targetObj, out accurateRange);
	}

	public float GetMinTimeBetweenFiring()
	{
		return Prototype.MinTimeBetweenFiring;
	}

	public virtual bool CanBeReloaded(Character character, bool checkIfAllowedToUseAmmo)
	{
		return false;
	}

	public Equipment TransferFrom(TileObject from, int amount, bool triggeredByLocalPlayer)
	{
		Equipment equipment = TakeMe(amount);
		if (equipment == this)
		{
			from.GetInventory().Remove(from, this);
		}
		from.GetInventory().CacheEncumbered(from);
		InfoScreen instance = InfoScreen.Instance;
		if (instance.Active && triggeredByLocalPlayer)
		{
			instance.WantRepopulate = true;
			InfoPage infoPage = instance.GetCurrentPage() as InfoPage;
			EquipmentBehaviour equipmentBehaviour = infoPage.FindUnityItem(this, null);
			if (equipmentBehaviour != null && equipmentBehaviour.ParentInventoryBehaviour != null && infoPage.UnityEventSystem != null)
			{
				if (equipment == this)
				{
					instance.DesiredEquipmentToSelect = equipmentBehaviour.ParentInventoryBehaviour.GetNextEquipmentToSelect(equipmentBehaviour);
					if (instance.DesiredEquipmentToSelect == null)
					{
						instance.DesiredEquipmentToSelect = equipment;
					}
				}
				else
				{
					instance.DesiredEquipmentToSelect = this;
				}
			}
		}
		return equipment;
	}

	public int GetMaxTransferrableTo(TileObject to)
	{
		return Math.Min(GetAmount(), (int)((to.GetMaxInventoryWeightIncludingBackpack(this) - to.GetInventory().GetWeight(to)) / GetWeight()));
	}

	public void OnUseDepletableEquipment(TileObject carrier, bool triggeredByLocalPlayer)
	{
		TransferFrom(carrier, 1, triggeredByLocalPlayer).Delete();
	}

	public virtual void OnUsedFromInfoScreen(PlayerRecord playerRecord, Character character, TileObject carrier)
	{
		bool flag = false;
		if (GetPrototype().HasCustomUseAction)
		{
			if (playerRecord.IsLocal)
			{
				InfoScreen.Instance.CloseInfoScreen();
			}
			BaseObject obj = this;
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, ref obj, SpeechSituation.CustomUseAction, default(MemoryParam));
			if (speechForSituation != null)
			{
				character.CommandSpeakTo(playerRecord, null, obj, default(MemoryParam), speechForSituation, null, null, default(MemoryParam), isDoubleClick: false);
			}
		}
		else if (LiquidContentsType != null && LiquidContentsType.DrinkableOrEdible)
		{
			character.ConsumeLiquid(this, playSound: true, fromInfoScreen: true);
		}
		else if (IsEdible() || IsDrinkable())
		{
			character.Eat(this, playSound: true, fromInfoScreen: true, null);
			flag = true;
		}
		if (GetBandageLevel() > -1)
		{
			character.ApplyBandage(character, this, playSound: true, callingCodeSupportsPrediction: false);
			flag = true;
		}
		if (GetAntigenType() != InfectionType.None)
		{
			character.InjectAntigen(character, GetAntigenType(), InfectedWith, this, callingCodeSupportsPrediction: false);
			flag = true;
		}
		if (flag)
		{
			OnUseDepletableEquipment(carrier, playerRecord.IsLocal);
		}
	}

	public void Equip(Character character)
	{
		if (CanBeEquipped())
		{
			character.EquippedItem = this;
			character.DesiredEquippedItem = this;
			if (this is Gun)
			{
				SoundManager.PlayMenuSound(SoundManager.GunCockSound);
			}
		}
	}

	public void Unequip(Character character)
	{
		if (character.EquippedItem == this)
		{
			character.EquippedItem = null;
			character.DesiredEquippedItem = null;
		}
	}

	public void Wear(Character character)
	{
		if (GetClothingType() == ClothingType.Invalid)
		{
			return;
		}
		SkillType skillBonusType = Prototype.SkillBonusType;
		if (skillBonusType != SkillType.Invalid)
		{
			character.ClearCachedSkillLevelWithEffects(skillBonusType);
		}
		for (int i = 0; i < character.Clothes.Length; i++)
		{
			if (character.Clothes[i] != null && !Prototype.IsClothingCompatible(character.Clothes[i].Prototype))
			{
				character.Clothes[i] = null;
			}
		}
		character.Clothes[(int)GetClothingType()] = this;
		character.UnityOnChangedAppearance();
		character.Inventory.CacheEncumbered(character);
		if (character.Id != 0)
		{
			character.MarkPlayerControlled();
		}
	}

	public virtual void Strip(Character character)
	{
		if (GetClothingType() != ClothingType.Invalid && character.Clothes[(int)GetClothingType()] == this)
		{
			SkillType skillBonusType = Prototype.SkillBonusType;
			if (skillBonusType != SkillType.Invalid)
			{
				character.ClearCachedSkillLevelWithEffects(skillBonusType);
			}
			character.Clothes[(int)GetClothingType()] = null;
			character.UnityOnChangedAppearance();
			character.Inventory.CacheEncumbered(character);
			if (character.Id != 0)
			{
				character.MarkPlayerControlled();
			}
		}
	}

	public bool IsEquippedOrWorn(TileObject carrier)
	{
		if (carrier is Character character)
		{
			if (character.EquippedItem == this)
			{
				return true;
			}
			if (GetClothingType() != ClothingType.Invalid && character.Clothes[(int)GetClothingType()] == this)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWorn(TileObject carrier)
	{
		if (GetClothingType() != ClothingType.Invalid)
		{
			if (carrier is Character character)
			{
				return character.Clothes[(int)GetClothingType()] == this;
			}
			return false;
		}
		return false;
	}

	public bool IsWornOrRemovedForSparring(TileObject carrier)
	{
		if (GetClothingType() != ClothingType.Invalid && carrier is Character character)
		{
			if (character.Clothes[(int)GetClothingType()] == this)
			{
				return true;
			}
			if (character.SparringRemovedArmorIds != null && character.SparringRemovedArmorIds.Contains(Id))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsIncludedInTakeAll(TileObject carrier)
	{
		return IsIncludedInTakeAll(carrier, showCampfirePot: false);
	}

	public bool IsIncludedInTakeAll(TileObject carrier, bool showCampfirePot)
	{
		if (IsWorn(carrier) && !(this is Armor) && Prototype.ClothingType != ClothingType.Backpack)
		{
			return false;
		}
		if (Concealed && carrier is Prop)
		{
			return false;
		}
		if (Prototype == EquipmentPrototype.Pot && carrier is Campfire && !showCampfirePot)
		{
			return false;
		}
		if (Prototype == EquipmentPrototype.FryingPan && carrier is Campfire && !showCampfirePot)
		{
			return false;
		}
		return true;
	}

	public virtual Equipment TakeMe(int amount)
	{
		if (amount <= 0)
		{
			Debug.LogError("Take amount " + amount + " (" + GetDisplayNameString() + ": " + Id + ")");
		}
		if (amount >= Amount)
		{
			return this;
		}
		int tradedAmount = TradedAmount;
		IncrementAmount(-amount);
		Equipment equipment = Spawn(Prototype);
		equipment.Amount = amount;
		equipment.InfectedWith = InfectedWith;
		equipment.TradedAmount = tradedAmount - TradedAmount;
		equipment.Init();
		return equipment;
	}

	public virtual int GetAmmoCountOfType(EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		if (Prototype != ammoType || InfectedWith != infectedWith)
		{
			return 0;
		}
		return GetAmount();
	}

	public virtual bool HasAmmoOfType(EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		if (Prototype == ammoType)
		{
			return InfectedWith == infectedWith;
		}
		return false;
	}

	public virtual bool HasAmmoOfType(EquipmentPrototype ammoType)
	{
		return Prototype == ammoType;
	}

	public virtual bool HasAmmoForWeapon(AmmoWeapon weapon, Character checkIfCharacterAllowedToUseIt = null)
	{
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				if (Prototype == item && (checkIfCharacterAllowedToUseIt == null || checkIfCharacterAllowedToUseIt.IsActionAllowedForItem(this, EquipmentPolicyAction.CanUse)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual int TakeBestAmmoForWeapon(Character checkIfCharacterIsAllowed, AmmoWeapon weapon, ref int requiredAmount, ref EquipmentPrototype takenAmmoType, ref InfectionType infectedWith, out bool deleteMe)
	{
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				if (Prototype == item && (takenAmmoType == null || takenAmmoType == item) && (infectedWith == InfectionType.Count || InfectedWith == infectedWith) && (checkIfCharacterIsAllowed == null || checkIfCharacterIsAllowed.IsActionAllowedForItem(item, null, InfectedWith, EquipmentPolicyAction.CanUse)))
				{
					int num = Math.Min(GetAmount(), requiredAmount);
					requiredAmount -= num;
					takenAmmoType = item;
					infectedWith = InfectedWith;
					deleteMe = GetAmount() == num;
					IncrementAmount(-num);
					return num;
				}
			}
		}
		deleteMe = false;
		return 0;
	}

	public virtual void GetSkillEffects(Character character, List<SkillEffect> skillEffects, int amount)
	{
		if ((GetClothingType() == ClothingType.Invalid || character.Clothes[(int)GetClothingType()] == this) && Prototype.SkillBonusType != SkillType.Invalid && Prototype.SkillBonus != 0)
		{
			skillEffects.Add(new SkillEffect(Prototype.SkillBonusType, Prototype.SkillBonus * amount));
		}
	}

	public virtual float GetCarryWeightEffect(int fitness)
	{
		return Prototype.CarryWeight + Prototype.CarryWeightBonusPerSkillLevel * (float)fitness;
	}

	public virtual bool GetLiquidAmountBar(out float amount, out Color col)
	{
		if (LiquidContentsType != null)
		{
			amount = LiquidContentsAmount / GetLiquidCapacity();
			col = LiquidContentsType.Col;
			return true;
		}
		if (Dampness > 0f && LiquidPrototype.Water != null)
		{
			amount = Dampness;
			col = LiquidPrototype.Water.Col;
			return true;
		}
		amount = 0f;
		col = Color.white;
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Prototype);
		if (Prototype == null && reflector.IsDeserialising && reflector is CustomBinaryReader customBinaryReader)
		{
			Debug.LogWarning("Equipment " + Id + " has null prototype! " + customBinaryReader.LastReadName);
		}
		reflector.Add(ref ColorVariation);
		reflector.Add(ref ColorVariation2);
		reflector.Add(ref ColorVariation3);
		reflector.Add(ref MaterialVariation);
		if (!reflector.IsDoingSavedCharacter)
		{
			reflector.AddAfter(ref InventoryOwner, 147);
		}
		reflector.Add(ref Amount);
		reflector.Add(ref TradedAmount);
		reflector.AddAfter(ref GatheredAmount, 522);
		reflector.AddAfter(ref DesignatedLiquid, 440);
		reflector.Add(ref LiquidContentsType);
		reflector.Add(ref LiquidContentsAmount);
		reflector.Add(ref Dampness);
		reflector.AddAfter(ref LastRareUpdateTime, 436);
		reflector.AddAfter(ref Gifted, 91);
		reflector.AddAfter(ref Concealed, 291);
		reflector.AddAfter(ref InfectedWith, 150);
		if (Amount == 0 && Prototype != null)
		{
			Debug.LogWarning("Serialising " + GetDisplayNameString() + " with amount == 0, Id: " + Id);
		}
		if (reflector.IsDeserialising)
		{
			if (reflector.Version < 233 && Id == 0 && Prototype != null && BaseObjectManager.PrototypeGameObjects[(int)Prototype.TypeName] is AmmoWeapon && !(this is AmmoWeapon))
			{
				int value = 0;
				reflector.Add(ref value);
			}
			Amount = Math.Max(1, Amount);
			if (Prototype != null && LiquidContentsAmount > Prototype.LiquidCapacity)
			{
				Debug.LogWarning(GetDisplayNameString() + " (" + Id + ") is overfilled: " + LiquidContentsAmount + "/" + Prototype.LiquidCapacity);
				LiquidContentsAmount = Prototype.LiquidCapacity;
			}
		}
	}
}

using System;
using System.Text;
using UnityEngine;

public class PlantableCrop : SingleTileProp
{
	public static float CriticalDehydrationLevel = -1f;

	public float Moisture;

	public float Frost;

	public float GrowthStage;

	public float Decay;

	public byte FarmerSkillLevel;

	public byte HarvestableAmount;

	public byte HarvestableSeedsAmount;

	public TimeSpan LastUpdate;

	public static float GrowthStageTime = Sun.DayLengthSecs;

	public static float FrostTempToDieInOneDay = -10f;

	private static int PROP_Ripe = StringUtil.JenkinsHash("PROP_Ripe");

	private static int PROP_Ripe_Female = StringUtil.JenkinsHash("PROP_Ripe_Female");

	private static int PROP_Dead = StringUtil.JenkinsHash("PROP_Dead");

	private static int PROP_Dead_Female = StringUtil.JenkinsHash("PROP_Dead_Female");

	public override Color32 MapColor => GameTerrain.MinimapSettings.CropsCol;

	public virtual Color CropPatchColor
	{
		get
		{
			if (Prototype == null)
			{
				return Color.white;
			}
			return Prototype.Color;
		}
	}

	public TimeSpan GrowthTime => TimeSpan.FromSeconds(GrowthStageTime * (float)GetMaxStage());

	public float GrowthTimeInSeconds => (float)GrowthTime.TotalSeconds;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PlantableCrop;
	}

	public float GetHarvestWeight()
	{
		if (GetHarvestPrototype() == null)
		{
			return 1f;
		}
		return GetHarvestPrototype().Weight;
	}

	public float GetHarvestSeedsWeight()
	{
		if (GetHarvestSeedsPrototype() == null)
		{
			return 1f;
		}
		return GetHarvestSeedsPrototype().Weight;
	}

	public virtual EquipmentPrototype GetHarvestPrototype()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.HarvestPrototype;
	}

	public virtual EquipmentPrototype GetHarvestSeedsPrototype()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.HarvestSeedsPrototype;
	}

	public virtual int GetNumStages()
	{
		if (Prototype == null)
		{
			return 0;
		}
		return Prototype.Prefabs.Count;
	}

	public virtual int GetMaxYield()
	{
		if (Prototype == null)
		{
			return 0;
		}
		return Prototype.MaxYield;
	}

	public virtual int GetMaxSeedsYield()
	{
		if (Prototype == null)
		{
			return 0;
		}
		return Prototype.MaxSeedsYield;
	}

	public virtual float GetWaterNeededPerDayInFlOz()
	{
		if (Prototype == null)
		{
			return 0f;
		}
		return Prototype.WaterNeededPerDayInFlOz;
	}

	public int GetMaxStage()
	{
		return GetNumStages() - 1;
	}

	public int GetStage()
	{
		return (int)GrowthStage;
	}

	public float GetTimeTillGrownInSeconds()
	{
		return (1f - GrowthStage / (float)GetMaxStage()) * GrowthTimeInSeconds;
	}

	public bool CanBeHarvested()
	{
		if (IsRipe())
		{
			if (HarvestableAmount <= 0)
			{
				return HarvestableSeedsAmount > 0;
			}
			return true;
		}
		return false;
	}

	public bool IsRipe()
	{
		return GetStage() == GetMaxStage();
	}

	public bool IsDead()
	{
		return Decay > 0f;
	}

	public bool IsDecayed()
	{
		if (!(Decay >= 1f))
		{
			if (GrowthStage >= (float)GetMaxStage() && HarvestableAmount == 0)
			{
				return HarvestableSeedsAmount == 0;
			}
			return false;
		}
		return true;
	}

	public override bool IsDestroyedNotIncludingDeadCrops()
	{
		if (IsDead())
		{
			return false;
		}
		return IsDestroyed();
	}

	public float GetMoisture()
	{
		return Math.Max(Moisture, CriticalDehydrationLevel);
	}

	public override bool IsTargetable()
	{
		if (Session.Instance.Editor || PropEditor.AllowTargetingAllProps)
		{
			return true;
		}
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		return GameTerrain.Instance.FogOfWar.IsTileExplored(Tile.x, Tile.y);
	}

	public override Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		EquipmentPrototype harvestPrototype = GetHarvestPrototype();
		if (harvestPrototype != null && harvestPrototype.Tex != null && harvestPrototype.Tex.GetAsset() != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			col = Color.white;
			return harvestPrototype.Tex;
		}
		return base.GetIcon(out mat, out col, highlighted);
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Moisture);
		reflector.Add(ref Frost);
		reflector.Add(ref GrowthStage);
		reflector.Add(ref Decay);
		if (reflector.Version < 19)
		{
			reflector.Add(ref CommunityId);
		}
		reflector.Add(ref FarmerSkillLevel);
		reflector.Add(ref HarvestableAmount);
		reflector.Add(ref HarvestableSeedsAmount);
		reflector.Add(ref LastUpdate);
		if (reflector.Version < 100 && IsRipe() && !IsDead())
		{
			Ripen();
		}
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.CropsManager.Add(this);
	}

	public override void Delete()
	{
		Session.Instance.CropsManager.Remove(this);
		base.Delete();
	}

	public void Update()
	{
		float num = (float)(Session.Instance.PlayTime - LastUpdate).TotalSeconds;
		LastUpdate = Session.Instance.PlayTime;
		PrefabResource unityModel = GetUnityModel();
		bool flag = IsDead();
		if (!flag)
		{
			Moisture = Math.Min(Moisture + Session.Instance.Weather.PrecipitationAmount * num / GetWaterNeededPerDayInFlOz(), 1f);
			Frost = Mathf.Clamp01(Frost + Session.Instance.Weather.TemperatureInCelsius / FrostTempToDieInOneDay * num / Sun.DayLengthSecs);
			if (Frost == 0f)
			{
				float num2 = num / Sun.DayLengthSecs;
				float num3 = num2 * 0.25f;
				if (Moisture > 0f)
				{
					Moisture -= num2;
					int maxStage = GetMaxStage();
					if (GrowthStage < (float)maxStage)
					{
						GrowthStage += num / GrowthStageTime;
						if (GrowthStage >= (float)maxStage)
						{
							GrowthStage = maxStage;
							Ripen();
						}
					}
				}
				else
				{
					Moisture -= num3;
				}
			}
			if (Moisture <= CriticalDehydrationLevel)
			{
				flag = true;
			}
			else if (Frost >= 1f)
			{
				if (CommunityId == Session.Instance.CommunityManager.PlayerCommunity.Id && HintManager.Instance.Hints[26].CanShowHint())
				{
					HintManager.Instance.Hints[26].ShowAndMarkPerformed(GameImpl.Translate(HintManager.HINT_CropsDiedOfFrost));
				}
				flag = true;
			}
		}
		if (flag)
		{
			Decay += num / Sun.DayLengthSecs;
		}
		if (UnityObj != null && unityModel != GetUnityModel())
		{
			unityModel.DeletePrefab(UnityObj);
			UnityObj = null;
			MarkCachedWorldTransDirty();
			UnityInit();
			UnityActivate();
		}
	}

	public void Ripen()
	{
		HarvestableAmount = (byte)Mathf.CeilToInt((float)(GetMaxYield() * FarmerSkillLevel) / 5f);
		HarvestableSeedsAmount = (byte)Mathf.RoundToInt((float)(GetMaxSeedsYield() * FarmerSkillLevel) / 5f);
		if (HarvestableAmount == 0 && HarvestableSeedsAmount == 0)
		{
			if (GetMaxSeedsYield() == 0 || MathUtil.RandomChoice(Id, 0.5f))
			{
				HarvestableAmount = 1;
				HarvestableSeedsAmount = 0;
			}
			else
			{
				HarvestableAmount = 0;
				HarvestableSeedsAmount = 1;
			}
		}
	}

	public float GetPredictedYield()
	{
		if (IsDead())
		{
			return 0f;
		}
		if (IsRipe())
		{
			return (int)HarvestableAmount;
		}
		if (Prototype == null)
		{
			return 0f;
		}
		return GetPredictedYieldForCropType(Prototype, (int)FarmerSkillLevel);
	}

	public static float GetPredictedYieldForCropType(PropPrototype proto, float farmerSkillLevel)
	{
		float num = Mathf.CeilToInt((float)proto.MaxYield * farmerSkillLevel / 5f);
		if (num == 0f)
		{
			num = ((proto.MaxSeedsYield <= 0) ? 1f : 0.5f);
		}
		return num;
	}

	public float GetPredictedSeedYield()
	{
		if (IsDead())
		{
			return 0f;
		}
		if (GetMaxSeedsYield() == 0)
		{
			return GetPredictedYield();
		}
		if (IsRipe())
		{
			return (int)HarvestableSeedsAmount;
		}
		float num = Mathf.RoundToInt((float)(GetMaxSeedsYield() * FarmerSkillLevel) / 5f);
		if (num == 0f)
		{
			num = 0.5f;
		}
		return num;
	}

	public bool Harvest(Character character, bool ignoreWeight, bool isGathering, out float price)
	{
		bool result = false;
		price = 0f;
		if (HarvestableAmount > 0)
		{
			int num = Math.Min(HarvestableAmount, (int)(character.GetAvailableInventorySpace() / GetHarvestWeight()));
			if (ignoreWeight)
			{
				num = Math.Max(num, 1);
				ignoreWeight = false;
			}
			if (GetHarvestPrototype() != null && num > 0)
			{
				Equipment equipment = Equipment.Spawn(GetHarvestPrototype(), num);
				price += equipment.GetBasePrice() * (float)num;
				equipment = character.Inventory.Add(character, equipment);
				equipment.IncrementGatheredAmount(num);
				NotificationManager.Instance.AddEquipmentNotification(this, character, equipment, num);
				result = true;
			}
			HarvestableAmount -= (byte)num;
		}
		if (HarvestableSeedsAmount > 0)
		{
			int num2 = Math.Min(HarvestableSeedsAmount, (int)(character.GetAvailableInventorySpace() / GetHarvestSeedsWeight()));
			if (ignoreWeight)
			{
				num2 = Math.Max(num2, 1);
				ignoreWeight = false;
			}
			if (GetHarvestSeedsPrototype() != null && num2 > 0)
			{
				Equipment equipment2 = Equipment.Spawn(GetHarvestSeedsPrototype(), num2);
				price += equipment2.GetBasePrice() * (float)num2;
				equipment2 = character.Inventory.Add(character, equipment2);
				equipment2.IncrementGatheredAmount(num2);
				NotificationManager.Instance.AddEquipmentNotification(this, character, equipment2, num2);
				result = true;
			}
			HarvestableSeedsAmount -= (byte)num2;
		}
		if (HarvestableAmount == 0 && HarvestableSeedsAmount == 0)
		{
			Delete();
			result = true;
		}
		return result;
	}

	public float GetNeededWaterInFlOz()
	{
		return GetWaterNeededPerDayInFlOz() * Math.Min(1f, 1f - Moisture);
	}

	public void Water(Character character, float amount)
	{
		if (!IsDead())
		{
			float moisture = Moisture;
			Moisture = Math.Min(Moisture + amount / GetWaterNeededPerDayInFlOz(), 1f);
			character.Skillset.AddProgress(character, SkillType.Farming, Moisture - moisture);
		}
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		return (options & 0x40) != 0;
	}

	public static PlantableCrop Spawn(PropPrototype proto, TerrainCoord tile, float growthStage, Community community, int skill)
	{
		PlantableCrop obj = (PlantableCrop)BaseObjectManager.Create(proto.TypeName);
		obj.Prototype = proto;
		obj.Tile = tile;
		obj.GrowthStage = growthStage;
		obj.FarmerSkillLevel = (byte)skill;
		obj.CommunityId = community?.Id ?? 0;
		obj.OnSpawn();
		return obj;
	}

	public static PlantableCrop Spawn(PropPrototype proto, TerrainCoord tile, Community community, int skill, CustomRandom rand)
	{
		PlantableCrop plantableCrop = (PlantableCrop)BaseObjectManager.Create(proto.TypeName);
		plantableCrop.Prototype = proto;
		plantableCrop.Tile = tile;
		plantableCrop.GrowthStage = rand.Next(plantableCrop.GetNumStages());
		plantableCrop.FarmerSkillLevel = (byte)skill;
		plantableCrop.CommunityId = community?.Id ?? 0;
		plantableCrop.OnSpawn();
		return plantableCrop;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		LastUpdate = Session.Instance.PlayTime;
		if (GrowthStage >= (float)GetMaxStage())
		{
			Ripen();
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		base.BuildDisplayName(sb, noStrangers, englishOnly);
		if (IsDead())
		{
			sb.Append(' ');
			sb.Append('(');
			sb.Append(GameImpl.Translate(PROP_Dead, PROP_Dead_Female, GetGender(GameImpl.Instance.Settings.Language), englishOnly));
			sb.Append(')');
		}
		else if (IsRipe())
		{
			sb.Append(' ');
			sb.Append('(');
			sb.Append(GameImpl.Translate(PROP_Ripe, PROP_Ripe_Female, GetGender(GameImpl.Instance.Settings.Language), englishOnly));
			sb.Append(')');
		}
	}

	public override PrefabResource GetUnityModel()
	{
		if (Prototype != null)
		{
			int stage = GetStage();
			if (IsDead() && Prototype.DeadPrefabs != null && stage < Prototype.DeadPrefabs.Count)
			{
				return Prototype.DeadPrefabs[stage];
			}
			if (stage >= Prototype.Prefabs.Count)
			{
				return null;
			}
			return Prototype.Prefabs[stage];
		}
		return null;
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if ((flags & 0x100000) != 0 && GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
		{
			return distance;
		}
		return base.Raycast(ray, length, flags, ref normal, ref bone, ref hitPosInBoneSpace, ref plantCover, source, target);
	}
}

using System;
using UnityEngine;

public class LooterSpawnPoint : AmbientEnemySpawnPoint
{
	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/LooterSpawnPoint", 10);

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return GameTerrain.MinimapSettings.EnemyCol;
		}
	}

	public static LooterSpawnPoint Spawn(TerrainCoord tile)
	{
		LooterSpawnPoint looterSpawnPoint = new LooterSpawnPoint();
		looterSpawnPoint.Tile = tile;
		looterSpawnPoint.Init();
		return looterSpawnPoint;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.LooterSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public static Community SpawnAmbientLooters(TerrainCoord centreTile, int minCount, int maxCount, float meleeWeapon, float ammoWeapon, float molotov, float bodyArmor, float helmet, float legArmor, LooterSpawnPoint spawnpoint, CommunityType communityType)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Community community = Community.Spawn(communityType);
		community.SpawnPoint = spawnpoint;
		int num = deterministicRand.Next(minCount, maxCount + 1);
		for (int i = 0; i < num; i++)
		{
			TerrainCoord tile = centreTile;
			if (i != 0)
			{
				tile += new TerrainCoord(deterministicRand.Next(-5, 5), deterministicRand.Next(-5, 5));
			}
			if (GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, null, null) || GameTerrain.Instance.IsTileEnclosed(tile.x, tile.y))
			{
				num = Math.Min(num + 1, maxCount + 10);
				continue;
			}
			float num2;
			switch (communityType)
			{
			default:
				num2 = 0.5f;
				break;
			case CommunityType.RovingRefugee:
				num2 = 0.4f;
				break;
			case CommunityType.AmbientLooter:
			case CommunityType.HunterLooter:
				num2 = 0.8f;
				break;
			}
			float probability = num2;
			HumanAppearance humanAppearance = new HumanAppearance((!deterministicRand.RandomChoice(probability)) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(deterministicRand));
			humanAppearance.Randomize(InfectionType.None, deterministicRand);
			Character character = Human.Spawn(tile, deterministicRand.RandomFloat() * (MathF.PI * 2f), humanAppearance, InfectionType.None);
			community.AddMember(character);
			character.InitialCommunity = community;
			if (community.Members.Count == 1)
			{
				character.SetRank(Rank.Leader);
				if (communityType == CommunityType.RovingTrader)
				{
					character.AddRole(new RoleInfo(Role.Trader));
				}
			}
			SetupSurvivor(character, PersonalityGroup.LooterFaction, meleeWeapon, ammoWeapon, molotov, bodyArmor, helmet, legArmor, deterministicRand);
			bool flag = character.HasRole(Role.Trader);
			if (EquipmentPrototype.Gold != null)
			{
				int num3 = 0;
				switch (communityType)
				{
				case CommunityType.RovingRefugee:
					num3 = 3;
					break;
				case CommunityType.RovingTrader:
					num3 = (flag ? 100 : 10);
					break;
				case CommunityType.AmbientLooter:
				case CommunityType.HunterLooter:
				case CommunityType.HunterMercenary:
					num3 = 1;
					break;
				}
				if (num3 > 0)
				{
					int amount = deterministicRand.Next(num3 + 1);
					character.SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.Gold, amount, fillLiquidContainers: true);
				}
			}
			int lootCount = 1;
			if (communityType == CommunityType.RovingTrader)
			{
				character.SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.PlasticBottle, 1, fillLiquidContainers: true);
				lootCount = 2;
				if (flag)
				{
					lootCount = 32;
				}
				if (flag && Session.Instance.DifficultySettings.SaveTokensRequired && Session.Instance.DifficultySettings.TradersHaveSaveTokens)
				{
					EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomItemOfClass(typeof(SavegameToken), deterministicRand);
					if (equipmentPrototype != null)
					{
						int num4 = deterministicRand.Next(Character.TraderSaveTokenMin, Character.TraderSaveTokenMax + 1);
						if (num4 > 0)
						{
							character.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype, num4, fillLiquidContainers: true);
						}
					}
				}
			}
			GameTerrain.GenerateLoot(character, lootCount, deterministicRand, includeClothing: false, fillLiquidContainers: true);
			character.SetGoal(new SurvivorGoal());
		}
		if (community.Members.Count == 0)
		{
			community.Delete();
			community = null;
		}
		community?.RandomizeRelationships(deterministicRand);
		return community;
	}

	public static void SetupSurvivor(Character character, string faction, float meleeWeapon, float ammoWeapon, float molotov, float bodyArmor, float helmet, float legArmor, CustomRandom rand)
	{
		character.LastUpdateTime = (character.LastThinkTime = Session.Instance.PlayTime);
		if (string.IsNullOrEmpty(character.FirstName))
		{
			character.RandomizeName(rand);
		}
		if (character.Skillset.StrengthCap == 0)
		{
			character.Skillset.Randomize(character, rand);
		}
		if (rand.RandomChoice(meleeWeapon) && character.Inventory.FindItemOfClass(typeof(MeleeWeapon)) == null && !character.Inventory.GeneratedLoot)
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomItemOfClass(character.GetLootLocation(), typeof(MeleeWeapon), rand, mustPickSomething: true);
			if (equipmentPrototype != null)
			{
				character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype));
			}
		}
		if (rand.RandomChoice(ammoWeapon) && character.Inventory.FindItemOfClass(typeof(AmmoWeapon)) == null && !character.Inventory.GeneratedLoot)
		{
			EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.PickRandomItemOfClass(character.GetLootLocation(), typeof(AmmoWeapon), rand, mustPickSomething: true);
			if (equipmentPrototype2 != null && character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype2)) is AmmoWeapon ammoWeapon2 && equipmentPrototype2.AmmoPrototypes != null && equipmentPrototype2.AmmoPrototypes.Count > 0)
			{
				EquipmentPrototype equipmentPrototype3 = GameImpl.Instance.PickAmmoForWeapon(character.GetLootLocation(), equipmentPrototype2, rand, mustPickSomething: true);
				if (equipmentPrototype3 != null)
				{
					ammoWeapon2.CurrentAmmoType = equipmentPrototype3;
					character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype3, Math.Max(equipmentPrototype2.MaxAmmo, 1)));
				}
			}
		}
		if (rand.RandomChoice(molotov) && EquipmentPrototype.MolotovCocktail != null && character.Inventory.FindItemOfType(EquipmentPrototype.MolotovCocktail) == null && !character.Inventory.GeneratedLoot)
		{
			character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.MolotovCocktail, 1));
		}
		bool flag = character.Inventory.GetWeight(character) >= character.GetMaxInventoryWeight();
		for (int i = 0; i < character.Roles.Count; i++)
		{
			switch (character.Roles[i].Role)
			{
			case Role.Farmer:
			case Role.Gatherer:
			case Role.Builder:
			case Role.Lumberjack:
			case Role.Cook:
			case Role.Trader:
			case Role.Trapper:
				flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
				break;
			}
		}
		bool mustHaveBodyArmor = rand.RandomChoice(bodyArmor);
		bool mustHaveHelmet = rand.RandomChoice(helmet);
		bool mustHaveLegArmor = rand.RandomChoice(legArmor);
		if ((character.Clothes[2] == null && character.Clothes[3] == null && character.Clothes[4] == null) || (flag && character.Clothes[5] == null))
		{
			character.RandomizeClothing(rand, seasonallyAppropriate: true, isPortrait: false, flag, mustHaveBodyArmor, mustHaveHelmet, mustHaveLegArmor);
		}
		if (character.Personality.Count == 0)
		{
			character.RandomizePersonality(rand, faction);
		}
		character.RandomizeSurvivalFactors(rand);
		if (!Session.Instance.Editor && rand.RandomChoice(Session.Instance.DifficultySettings.InvisibleStrainPercentage / 100f))
		{
			switch (rand.Next() % 2)
			{
			case 0:
				character.InvisibleStrain = InvisibleStrainType.Excitable;
				character.HadInvisibleStrainFromStart = true;
				break;
			case 1:
				character.InvisibleStrain = InvisibleStrainType.Subtle;
				character.HadInvisibleStrainFromStart = true;
				break;
			default:
				character.InvisibleStrain = InvisibleStrainType.None;
				character.HadInvisibleStrainFromStart = false;
				break;
			}
		}
	}

	public override Community SpawnEnemyGroup()
	{
		int num = Mathf.RoundToInt(4f * (Session.Instance.DifficultySettings.LootDensity / 100f));
		Community community = SpawnAmbientLooters(Tile, num / 2, num, 0.75f, 0.5f, 0.25f, 0.25f, 0.25f, 0.25f, this, CommunityType.AmbientLooter);
		OnSpawned(community);
		return community;
	}
}

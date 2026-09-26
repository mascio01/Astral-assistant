using System;
using UnityEngine;

public abstract class ZombieSpawnPoint : AmbientEnemySpawnPoint
{
	public abstract InfectionType Infection { get; }

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return GameTerrain.MinimapSettings.GetInfectionCol(Infection);
		}
	}

	public static Community SpawnAmbientZombies(TerrainCoord centreTile, int minCount, int maxCount, InfectionType infection, ZombieSpawnPoint spawnpoint)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Community community = Community.Spawn((spawnpoint == null) ? CommunityType.HunterZombie : CommunityType.AmbientZombie);
		community.SpawnPoint = spawnpoint;
		int num = deterministicRand.Next(minCount, maxCount + 1);
		for (int i = 0; i < num; i++)
		{
			TerrainCoord tile = centreTile;
			if (i != 0)
			{
				tile += new TerrainCoord(deterministicRand.Next(-5, 5), deterministicRand.Next(-5, 5));
			}
			if (GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, null, null))
			{
				num = Math.Min(num + 1, maxCount + 10);
				continue;
			}
			HumanAppearance humanAppearance = new HumanAppearance((!deterministicRand.RandomChoice(0.5f)) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(deterministicRand));
			humanAppearance.Randomize(infection, deterministicRand);
			Character character = Human.Spawn(tile, deterministicRand.RandomFloat() * (MathF.PI * 2f), humanAppearance, (infection == InfectionType.None) ? ((InfectionType)deterministicRand.Next(1, 5)) : infection);
			character.Rotten = true;
			character.PlayDead = Session.Instance.CommunityManager.GetTownForTile(tile, 0f) != null && deterministicRand.RandomChoice(0.5f);
			SetupZombie(character, deterministicRand);
			if (spawnpoint != null && spawnpoint.LastDiedTime == 0f && deterministicRand.RandomChoice(Session.Instance.DifficultySettings.ZombieCrippledPercentage / 100f))
			{
				bool flag = deterministicRand.RandomChoice(0.5f);
				character.AddInjury(new Injury(character, InjuryType.SharpObject, absorbedByVest: false, flag ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg, InfectionType.None, flag ? Bone.RightLeg : Bone.LeftLeg, Vector3.zero, character.GetMaxLimbDamage(), TimeSpan.Zero, null, assailantUnknown: false, intentional: true));
			}
			GameTerrain.GenerateLoot(character, deterministicRand.RandomChoice(Session.Instance.DifficultySettings.LootDensity / 200f) ? 1 : 0, deterministicRand, includeClothing: false, fillLiquidContainers: true);
			community.AddMember(character);
			character.SetGoal(new ZombieGoal());
		}
		if (community.Members.Count == 0)
		{
			community.Delete();
			community = null;
		}
		return community;
	}

	public static void SetupZombie(Character character, CustomRandom rand)
	{
		character.LastUpdateTime = (character.LastThinkTime = Session.Instance.PlayTime);
		character.RandomizeClothing(rand, seasonallyAppropriate: false);
	}

	public override Community SpawnEnemyGroup()
	{
		Community community = SpawnAmbientZombies(Tile, 1, 1, Infection, this);
		OnSpawned(community);
		return community;
	}
}

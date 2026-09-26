using System;
using UnityEngine;

public class RabbitSpawnPoint : AmbientEnemySpawnPoint
{
	public static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/RabbitSpawnPoint", 10);

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return GameTerrain.MinimapSettings.NeutralCol;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RabbitSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public override Community SpawnEnemyGroup()
	{
		TerrainCoord tile = Tile;
		if (GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, null, null))
		{
			return null;
		}
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Community community = Community.Spawn(CommunityType.AmbientAnimal);
		community.SpawnPoint = this;
		RabbitAppearance appearance = new RabbitAppearance((!deterministicRand.RandomChoice(0.5f)) ? GenderType.Female : GenderType.Male, RabbitAppearance.PickRandomAge(deterministicRand));
		Rabbit rabbit = Rabbit.Spawn(Tile, deterministicRand.RandomFloat() * (MathF.PI * 2f), appearance);
		rabbit.RandomizeSurvivalFactors(deterministicRand);
		community.AddMember(rabbit);
		rabbit.SetGoal(new AnimalGoal());
		OnSpawned(community);
		return community;
	}
}

using System;
using UnityEngine;

public struct StayInRangeParams : IReflectable
{
	public StayInRangeOf StayInRangeOf;

	public float StayInRangeDist;

	public TerrainCoord StayInRangeTile;

	public static StayInRangeParams OfSquadLeader()
	{
		return new StayInRangeParams
		{
			StayInRangeOf = StayInRangeOf.SquadLeader
		};
	}

	public static StayInRangeParams OfTile(TerrainCoord tile, float dist)
	{
		return new StayInRangeParams
		{
			StayInRangeOf = StayInRangeOf.Tile,
			StayInRangeTile = tile,
			StayInRangeDist = dist
		};
	}

	public static StayInRangeParams OfSquadLeaderOrBase(Character character)
	{
		if (character.Community != null && (character.Community.CommunityType == CommunityType.Normal || character.Community.CommunityType == CommunityType.Looter) && character.Community.GetBaseCentre(out var centre, out var radius) && character.Tile.GetDist(centre) <= radius + 32f)
		{
			return OfTile(centre, radius + 16f);
		}
		return OfSquadLeader();
	}

	public void Reflect(Reflector reflector)
	{
		reflector.AddAfter(ref StayInRangeDist, 175);
		reflector.AddAfter(ref StayInRangeOf, 175);
		reflector.AddAfter(ref StayInRangeTile, 175);
	}

	public void CalcStayInRangeDistAndTile(Character character, out float resultDist, out TerrainCoord resultTile)
	{
		resultTile = character.Tile;
		resultDist = float.MaxValue;
		switch (StayInRangeOf)
		{
		case StayInRangeOf.Tile:
			resultDist = StayInRangeDist;
			resultTile = StayInRangeTile;
			break;
		case StayInRangeOf.SquadLeader:
			if (character.SquadLeader != null && character.SquadLeader.WantSquadToStayInRange() && character.SparringType != SparringType.Feuding && character.SparringType != SparringType.FightToTheDeath)
			{
				resultDist = Mathf.Sqrt(25f * (1f + (float)character.SquadLeader.Followers.Count) / MathF.PI);
				resultTile = character.SquadLeader.Tile;
			}
			break;
		}
	}
}

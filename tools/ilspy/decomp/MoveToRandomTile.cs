using System.Collections.Generic;
using UnityEngine;

internal class MoveToRandomTile : Goal
{
	private static List<TerrainCoord> Somewheres = new List<TerrainCoord>();

	private static List<TerrainCoord> SomewheresBehindMe = new List<TerrainCoord>();

	private static List<TileObject> Temp = new List<TileObject>();

	private static float SoundProbability = 0.25f;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToRandomTile;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Somewheres.Clear();
		SomewheresBehindMe.Clear();
		Somewheres.Add(new TerrainCoord(-1, 0));
		Somewheres.Add(new TerrainCoord(1, 0));
		Somewheres.Add(new TerrainCoord(0, -1));
		Somewheres.Add(new TerrainCoord(0, 1));
		Somewheres.Add(new TerrainCoord(-1, -1));
		Somewheres.Add(new TerrainCoord(-1, 1));
		Somewheres.Add(new TerrainCoord(1, -1));
		Somewheres.Add(new TerrainCoord(1, 1));
		Somewheres.Add(new TerrainCoord(-2, 0));
		Somewheres.Add(new TerrainCoord(2, 0));
		Somewheres.Add(new TerrainCoord(0, -2));
		Somewheres.Add(new TerrainCoord(0, 2));
		Somewheres.Add(new TerrainCoord(-2, -2));
		Somewheres.Add(new TerrainCoord(-2, 2));
		Somewheres.Add(new TerrainCoord(2, -2));
		Somewheres.Add(new TerrainCoord(2, 2));
		TerrainCoord tile = character.Tile;
		for (int num = Somewheres.Count - 1; num >= 0; num--)
		{
			if (Vector2.Dot(Somewheres[num].ToVector2(), MathUtil.ToXZ(character.Forward)) < Mathf.Cos(0.87266463f))
			{
				SomewheresBehindMe.Add(Somewheres[num]);
				Somewheres.RemoveAt(num);
			}
		}
		List<TerrainCoord> list = Somewheres;
		while (list.Count > 0)
		{
			int index = Session.Instance.DeterministicRand.Next() % list.Count;
			TerrainCoord terrainCoord = tile + list[index];
			if (IsPassable(character, tile, terrainCoord))
			{
				if (character is Animal && list == Somewheres)
				{
					bool flag = false;
					GameTerrain.Instance.GetObjectsInRect(terrainCoord + new TerrainCoord(-1, -1), terrainCoord + new TerrainCoord(1, 1), Temp);
					foreach (TileObject item in Temp)
					{
						if (item.IsBurning())
						{
							flag = true;
							break;
						}
					}
					Temp.Clear();
					if (flag)
					{
						SomewheresBehindMe.Add(Somewheres[index]);
						Somewheres.RemoveAt(index);
						if (list.Count == 0)
						{
							list = SomewheresBehindMe;
						}
						continue;
					}
				}
				if (!character.HasMovementZone() || !character.MovementZone.Contains(character.Tile) || character.MovementZone.Contains(terrainCoord))
				{
					if (character.Zombie && Session.Instance.DeterministicRand.RandomChoice(SoundProbability))
					{
						character.PlayVoiceSoundFromList(SoundManager.ZombieIdleSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
					}
					character.MoveToTile(MovementType.Walk, terrainCoord);
					return;
				}
			}
			list.RemoveAt(index);
			if (list.Count == 0)
			{
				if (list != Somewheres)
				{
					break;
				}
				list = SomewheresBehindMe;
			}
		}
		Finished = true;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.HaltMovement();
		base.OnDeactivate(character, parent);
	}

	private bool IsPassable(Character character, TerrainCoord cur, TerrainCoord dest)
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord terrainCoord = new TerrainCoord((dest.x < cur.x) ? (-1) : ((dest.x > cur.x) ? 1 : 0), (dest.y < cur.y) ? (-1) : ((dest.y > cur.y) ? 1 : 0));
		bool flag = dest.x != cur.x && dest.y != cur.y;
		while (cur != dest)
		{
			if (instance.IsImpassable(cur.x + terrainCoord.x, cur.y + terrainCoord.y, 2051, character, null))
			{
				return false;
			}
			if (flag)
			{
				if (instance.IsImpassable(cur.x + terrainCoord.x, cur.y, 2051, character, null))
				{
					return false;
				}
				if (instance.IsImpassable(cur.x, cur.y + terrainCoord.y, 2051, character, null))
				{
					return false;
				}
			}
			cur += terrainCoord;
		}
		return true;
	}

	public override void OnCollisionWithCharacter(Character character, Goal parent, Character other)
	{
		StopMoving(character);
	}

	public override void Update(Character character, Goal parent)
	{
		if (character.IsFinishedRoute() || character.CarriedBy != null)
		{
			StopMoving(character);
		}
		base.Update(character, parent);
	}

	private void StopMoving(Character character)
	{
		character.HaltMovement();
		Finished = true;
	}
}

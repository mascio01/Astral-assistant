using System;
using System.Collections.Generic;
using UnityEngine;

public class FleeFromAllEnemies : MoveTo
{
	public float MinRange;

	public float MaxRange;

	public List<TerrainCoord> Dangers = new List<TerrainCoord>();

	public FleeFromAllEnemies()
	{
	}

	public FleeFromAllEnemies(MovementType movementType, float minRange, float maxRange, bool dontOpenOurGates, bool avoidHostileBases)
		: base(movementType)
	{
		MinRange = minRange;
		MaxRange = maxRange;
		_dontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MinRange);
		reflector.Add(ref MaxRange);
		reflector.Add(ref Dangers);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FleeFromAllEnemies;
	}

	public static bool HasAnyThreats(Character character)
	{
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAnyThreatsInRange(Character character, float range)
	{
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target) && MathUtil.ToXZ(target.LastKnownPosition - character.Pos).sqrMagnitude < range * range)
			{
				return true;
			}
		}
		return false;
	}

	public static int CountThreats(Character character)
	{
		int num = 0;
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				num++;
			}
		}
		return num;
	}

	public static Target GetNearestThreat(Character character)
	{
		Target result = null;
		float num = float.MaxValue;
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				float sqrMagnitude = MathUtil.ToXZ(target.LastKnownPosition - character.Pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = target;
				}
			}
		}
		return result;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		return HasAnyThreats(character);
	}

	public static bool IsAThreat(Character character, Target target)
	{
		if (target.Object is EnterableVehicle { IsMoving: not false })
		{
			return true;
		}
		if (target.Object is Character { Deleted: false } character2 && !target.GetFlag(TargetFlags.KnockedOut) && !target.GetFlag(TargetFlags.Dead))
		{
			return character.IsEnemy(character2);
		}
		return false;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		Dangers.Clear();
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				Dangers.Add(GameTerrain.Instance.GetTileCoordForPos(target.LastKnownPosition));
			}
		}
		_requester.StartFleeFromAllEnemiesRequest(GetStartTile(character), Dangers, MinRange, MaxRange, character, null, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public bool IsOutOfRangeOfTarget(Character character)
	{
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target) && Vector2.SqrMagnitude(MathUtil.ToXZ(target.LastKnownPosition) - MathUtil.ToXZ(character.Position)) < MaxRange * MaxRange)
			{
				return false;
			}
		}
		return true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished && SubGoal == null && IsOutOfRangeOfTarget(character))
		{
			Finished = true;
			Success = true;
		}
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (IsOutOfRangeOfTarget(character))
		{
			return MoveToResult.Success;
		}
		int num = 0;
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				if (num >= Dangers.Count)
				{
					return MoveToResult.Working;
				}
				if (GameTerrain.Instance.GetTileCoordForPos(target.LastKnownPosition).GetDist(Dangers[num]) >= Math.Max(character.Tile.GetDist(Dangers[num]) * 0.5f, 1f))
				{
					return MoveToResult.Working;
				}
				num++;
			}
		}
		if (num != Dangers.Count)
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Success;
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		if (_requester != null)
		{
			return false;
		}
		TerrainCoord routeDestination = character.GetRouteDestination();
		if (GameTerrain.Instance.IsImpassable(routeDestination.x, routeDestination.y, 2049, character, null))
		{
			return true;
		}
		if (Dangers.Count < CountThreats(character))
		{
			return true;
		}
		int num = 0;
		foreach (Target target in character.Targets)
		{
			if (IsAThreat(character, target))
			{
				if (num >= Dangers.Count)
				{
					return true;
				}
				if (GameTerrain.Instance.GetTileCoordForPos(target.LastKnownPosition).GetDist(Dangers[num]) >= Math.Max(character.Tile.GetDist(Dangers[num]) * 0.5f, 1f))
				{
					return true;
				}
				num++;
			}
		}
		return false;
	}

	public override int GetBuildingExitIndex(Character character)
	{
		float num = float.MinValue;
		int result = 0;
		for (int i = 0; i < character.InsideBuilding.GetEntranceDefs().Length; i++)
		{
			GameTerrain.Instance.GetTileCoordForPos(character.InsideBuilding.GetEntrancePos(i));
			float num2 = float.MaxValue;
			foreach (Target target in character.Targets)
			{
				if (IsAThreat(character, target))
				{
					float b = Vector2.SqrMagnitude(GameTerrain.Instance.GetTileCentreXZ(DestTile) - MathUtil.ToXZ(character.Position));
					num2 = Mathf.Min(num2, b);
				}
			}
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}
}

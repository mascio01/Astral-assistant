using UnityEngine;

public class MoveAsCloseAsPossibleToTarget : MoveToTarget
{
	public float MaxRange = float.MaxValue;

	public float FinishWhenInRange;

	public MoveAsCloseAsPossibleToTarget()
	{
	}

	public MoveAsCloseAsPossibleToTarget(MovementType movementType)
		: base(movementType)
	{
	}

	public MoveAsCloseAsPossibleToTarget(MovementType movementType, bool dontOpenOurGates)
		: base(movementType)
	{
		_dontOpenOurGates = dontOpenOurGates;
	}

	public MoveAsCloseAsPossibleToTarget(MovementType movementType, float maxRange)
		: base(movementType)
	{
		MaxRange = maxRange;
	}

	public MoveAsCloseAsPossibleToTarget(MovementType movementType, float maxRange, bool dontOpenOurGates)
		: base(movementType)
	{
		MaxRange = maxRange;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public MoveAsCloseAsPossibleToTarget(Character character, TileObject targetObj, MovementType movementType)
		: base(movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveAsCloseAsPossibleToTarget(Character character, TileObject targetObj, MovementType movementType, float maxRange)
		: base(movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MaxRange = maxRange;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MaxRange);
		reflector.AddAfter(ref FinishWhenInRange, 501);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveAsCloseAsPossibleToTarget;
	}

	public override void UpdateDestination(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			if (targetCharacter.InsideBuilding != null)
			{
				DestTile = targetCharacter.InsideBuilding.GetNearestPassableTileTo(character.Tile, 2049, character, null);
			}
			else
			{
				if (AlwaysTrackTarget)
				{
					Target.ForceVisible(character);
				}
				DestTile = GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition);
				if (UseFollowOffset)
				{
					Vector2 pos = FollowGoal.CalcFollowingPosXZ(character.SquadLeader, character);
					DestTile = GameTerrain.Instance.GetTileCoordForPosXZ(pos);
				}
			}
		}
		else if (!IsTargetDeleted())
		{
			DestTile = Target.Object.GetNearestPassableTileTo(character.Tile, 2049, character, null);
		}
		DestTile = GameTerrain.Instance.ClampTileWithinBounds(DestTile);
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartAsCloseAsPossibleRequest(GetStartTile(character), DestTile, character, Target.Object, MaxRange, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.Tile.GetDistSquared(DestTile) <= FinishWhenInRange * FinishWhenInRange && SubGoal == null)
		{
			Finished = true;
			Success = true;
		}
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (_requestedDestTile != DestTile)
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Success;
	}
}

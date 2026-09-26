using System;
using UnityEngine;

public class MoveToTarget : MoveTo
{
	public bool AlwaysTrackTarget;

	public bool UseFollowOffset;

	public MoveToTarget()
	{
	}

	public MoveToTarget(MovementType movementType)
		: base(movementType)
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref AlwaysTrackTarget);
		reflector.Add(ref UseFollowOffset);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToTarget;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void UpdateDestination(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			if (targetCharacter.InsideBuilding != null)
			{
				DestTile = targetCharacter.InsideBuilding.GetNearestTileTo(character.Tile);
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
			DestTile = Target.Object.GetNearestTileTo(character.Tile);
		}
		DestTile = GameTerrain.Instance.ClampTileWithinBounds(DestTile);
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartRequest(GetStartTile(character), DestTile, character, Target.Object, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		return DestTile.GetDistSquared(_requestedDestTile) >= Math.Max(character.Tile.GetDistSquared(_requestedDestTile), 1f);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		TerrainCoord tile = character.Tile;
		if (tile != DestTile)
		{
			return MoveToResult.Working;
		}
		if (!tile.IsWithinBounds(Target.Object.GetMinTile(), Target.Object.GetMaxTile()))
		{
			return MoveToResult.Fail;
		}
		return MoveToResult.Success;
	}

	public override int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetClosestEntranceTo(Target.Object.GetCentreTile(), character, mustBeUnblocked: true);
	}
}

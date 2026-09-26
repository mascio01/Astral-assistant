using System;

public class SitAroundFireGoal : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public float MaxRange = 2.25f;

	public float MinRange = 1.75f;

	public float WaitTime = 2f;

	public SitAroundFireGoal()
	{
	}

	public SitAroundFireGoal(Character character, TileObject campfire, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(campfire));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SitAroundFireGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DirectControlledCrouching = false;
		character.IssueCommandToFollowers(FollowCommand.SitAroundFire, GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition), null);
		if (character.Sitting && MathUtil.ToXZ(character.Pos - Target.LastKnownPosition).sqrMagnitude <= MaxRange * MaxRange)
		{
			SetSubGoal(character, parent, new WaitAndFaceTarget(TimeSpan.FromSeconds(WaitTime)));
		}
		else
		{
			SetSubGoal(character, parent, new MoveWithinRangeOfTarget(MovementType, aiming: false, MinRange, MaxRange, dontOpenOurGates: false));
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinRangeOfTarget { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			return new SitGoal();
		}
		if (SubGoal is SitGoal)
		{
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(WaitTime));
		}
		if (SubGoal is WaitAndFaceTarget)
		{
			if (parent is ObeyLeaderGoal obeyLeaderGoal && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player)
			{
				character.SetHangoutLocation(character.Tile);
			}
			Success = true;
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}

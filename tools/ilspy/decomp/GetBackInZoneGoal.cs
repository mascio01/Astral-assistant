using System;

public class GetBackInZoneGoal : StateMachineGoal
{
	private TimeSpan LastFailedTime;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref LastFailedTime);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GetBackInZoneGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_GetBackInZone;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.HasMovementZone() && !character.MovementZone.Contains(character.Tile) && !character.DirectControlledMajorAIDisabled && Session.Instance.PlayTime - LastFailedTime >= TimeSpan.FromSeconds(10.0))
		{
			if (character.IsInsideBuildingUnderConstruction(out var building) && building.GetTileRect().Overlaps(character.MovementZone))
			{
				return false;
			}
			if (character.InsideBuilding != null && character.InsideBuilding.GetTileRect().Overlaps(character.MovementZone))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new MoveWithinBounds(MovementType.Run, character.MovementZone.min, character.MovementZone.max));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinBounds moveWithinBounds)
		{
			if (moveWithinBounds.Success)
			{
				LastFailedTime = TimeSpan.Zero;
			}
			else
			{
				LastFailedTime = Session.Instance.PlayTime;
			}
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}

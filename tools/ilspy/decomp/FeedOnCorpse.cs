using System;
using UnityEngine;

public class FeedOnCorpse : StateMachineGoal
{
	private static GameProfiler CalcBestTargetTimer = new GameProfiler("ZombieFeedOnCorpseCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.FeedOnCorpse;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is EatTargetCorpse)
		{
			return null;
		}
		if (Target == null)
		{
			return null;
		}
		return Target.GetAlertIcon();
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		MovementType movementType = (character.ShouldLimp() ? MovementType.Walk : MovementType.Run);
		FoodProp targetFood = GetTargetFood();
		if (targetFood != null && targetFood.FoodProto == EquipmentPrototype.RancidHumanMeat)
		{
			movementType = MovementType.Walk;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.Rotten)
		{
			movementType = MovementType.Walk;
		}
		SetSubGoal(character, parent, new MoveWithinRangeOfTarget(movementType, aiming: false, 0f, 1f, dontOpenOurGates: false));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget)
		{
			if (moveWithinRangeOfTarget.Success && !IsTargetDeletedOrDisappeared())
			{
				return new EatTargetCorpse();
			}
			return new ZombieFrustrationGoal();
		}
		if (SubGoal is ZombieFrustrationGoal)
		{
			Target.MarkInaccessible();
		}
		return null;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeletedOrDisappeared())
		{
			return false;
		}
		if (Target.Object.IsBeingCarried())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !targetCharacter.Rotten && character.Infection != InfectionType.Invisible)
		{
			return GoalPriority.Zombie_FeedOnFreshMeat;
		}
		FoodProp targetFood = GetTargetFood();
		if (targetFood != null)
		{
			if (targetFood.FoodProto != EquipmentPrototype.RancidHumanMeat)
			{
				return GoalPriority.Zombie_FeedOnFreshMeat;
			}
			return GoalPriority.Zombie_FeedOnRottenMeat;
		}
		return GoalPriority.Zombie_FeedOnCorpse;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (HasUserTarget)
			{
				return Target;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (!(target.Object is Character character2))
				{
					if (!(target.Object is FoodProp))
					{
						continue;
					}
				}
				else if (!target.HasAnyFlag((TargetFlags)8390656) || character2.Rotten || character2.Disappeared || character2.IsPlayingDead())
				{
					continue;
				}
				if (target.Object.GetConsumedAmount() >= 1f || (target.IsInaccessible() && target.TimeSinceInaccessible < TimeSpan.FromSeconds(60.0)))
				{
					continue;
				}
				TerrainCoord centreTile = target.Object.GetCentreTile();
				if (!GameTerrain.Instance.IsImpassable(centreTile.x, centreTile.y, 2049, character, target.Object))
				{
					float num2 = MathUtil.ToXZ(character.Pos - target.LastKnownPosition).magnitude;
					if (target == Target)
					{
						num2 *= 0.5f;
					}
					if (!(num2 > 900f) && num2 < num)
					{
						result = target;
						num = num2;
					}
				}
			}
			return result;
		}
	}
}

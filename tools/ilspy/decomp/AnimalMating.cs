using System;
using UnityEngine;

public class AnimalMating : StateMachineGoal
{
	private TimeSpan LastAttemptedTime;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("AnimalMatingCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalMating;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref LastAttemptedTime);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!Active || !(SubGoal is AnimationGoal))
		{
			return GoalPriority.Animal_Mating;
		}
		return GoalPriority.Animal_Mating_Active;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		SetSubGoal(character, parent, new MoveAdjacentToTarget(MovementType.Walk, canBeOnTile: false));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentToTarget moveAdjacentToTarget)
		{
			if (moveAdjacentToTarget.Success)
			{
				return new TurnToTarget();
			}
			LastAttemptedTime = Session.Instance.PlayTime;
		}
		if (SubGoal is TurnToTarget)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.IsOutdoors() && targetCharacter.MovementType == MovementType.None && Mathf.Abs((targetCharacter.PosXZ - character.PosXZ).magnitude - 1f) < 0.1f && targetCharacter.OnMatingStarted(character))
			{
				return new AnimationGoal(ActionAnim.MaleMating);
			}
			LastAttemptedTime = Session.Instance.PlayTime;
		}
		if (SubGoal is AnimationGoal)
		{
			LastAttemptedTime = Session.Instance.PlayTime;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Mate)
		{
			if (GetTargetCharacter() is Chicken chicken)
			{
				chicken.LastFertilizedTime = Session.Instance.PlayTime;
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (Session.Instance.PlayTime - LastAttemptedTime < TimeSpan.FromSeconds(10.0))
		{
			return false;
		}
		if (character.Community != null && character.Community.IsAISettlement() && character.Community.GetChickenCountIncludingEggs() >= Math.Min(character.Community.InitialChickenCount, character.Community.GetChickenAccommodation()))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (Active)
			{
				return Target;
			}
			if (!character.IsOutdoors())
			{
				return null;
			}
			if (character.IsYouth())
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object != null && !target.Object.Deleted && !target.GetFlag(TargetFlags.Lost) && !target.Object.IsDestroyed() && target.Object.GetBaseObjectType() == character.GetBaseObjectType() && target.Object.GetAuthoritativeOrElseThis() is Character character2 && character2.IsOutdoors() && character.Community == character2.Community && character.IsAttractedTo(character2.GetGender()) && !character2.IsYouth() && target.FullyTracked && (!(character2 is Chicken chicken) || (!(Session.Instance.PlayTime - chicken.LastFertilizedTime < TimeSpan.FromSeconds(300.0)) && !chicken.IsBroody())))
				{
					float num2 = character.Get2DDistToTargetLastKnownPos(target) / character.GetWalkSpeed();
					float num3 = (float)target.TimeSinceLastDetected.TotalSeconds + num2;
					if (target == Target && Active)
					{
						num3 *= 0.75f;
					}
					if (num3 < num)
					{
						result = target;
						num = num3;
					}
				}
			}
			return result;
		}
	}
}

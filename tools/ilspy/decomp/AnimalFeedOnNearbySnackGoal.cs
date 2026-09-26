using System;
using UnityEngine;

public class AnimalFeedOnNearbySnackGoal : StateMachineGoal
{
	private const float EatCarrotFromHandRange = 1.5f;

	private static float MinWaitTime = 2f;

	private static float MaxWaitTime = 6f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("RabbitFeedOnCarrotCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalFeedOnNearbySnackGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Animal_FeedOnCarrot;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is EatCarrot)
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
		if (GetTargetProp() is RabbitTrap rabbitTrap)
		{
			SetSubGoal(character, parent, new MoveTo(MovementType.Walk, rabbitTrap.GetEntranceTile()));
		}
		else if (character.InsideBuilding != null)
		{
			SetSubGoal(character, parent, new LeaveBuilding());
		}
		else if (GetTargetCharacter() != null)
		{
			SetSubGoal(character, parent, GetWaitGoal());
		}
		else
		{
			SetSubGoal(character, parent, new MoveToTarget(MovementType.Walk));
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is EatCarrot && Target != null && MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).magnitude > 1.5f)
		{
			SetSubGoal(character, parent, GetWaitGoal());
		}
	}

	private Goal GetWaitGoal()
	{
		if (Session.Instance.DeterministicRand.RandomChoice(0.5f))
		{
			return new AnimationGoal(ActionAnim.LookAround);
		}
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(MinWaitTime, MaxWaitTime, Session.Instance.DeterministicRand.RandomFloat())));
	}

	public void FeedFromHand(Character character, Goal parent, Character feeder)
	{
		if (!(SubGoal is EatCarrot))
		{
			SetSubGoal(character, parent, new EatCarrot(character, feeder, ActionAnim.LookAround));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is LeaveBuilding)
		{
			if (GetTargetCharacter() != null)
			{
				return GetWaitGoal();
			}
			return new MoveToTarget(MovementType.Walk);
		}
		if (SubGoal is EatCarrot)
		{
			if (IsTargetDeleted() || GetTargetObject() is Human)
			{
				return null;
			}
			return new EatCarrot(ActionAnim.Eat);
		}
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget)
		{
			if (moveWithinRangeOfTarget.Success)
			{
				return GetWaitGoal();
			}
			Target.MarkInaccessible();
		}
		if (SubGoal is AnimationGoal)
		{
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(MinWaitTime, MaxWaitTime, Session.Instance.DeterministicRand.RandomFloat())));
		}
		if (SubGoal is WaitAndFaceTarget)
		{
			float magnitude = MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).magnitude;
			if (magnitude < 1.5f)
			{
				return new TurnToTarget();
			}
			return new MoveWithinRangeOfTarget(MovementType.Walk, aiming: false, 0f, Math.Max(1.25f, magnitude - 1f), dontOpenOurGates: true);
		}
		if (SubGoal is MoveTo moveTo)
		{
			if (moveTo.Success)
			{
				return new TurnToTarget();
			}
			Target.MarkInaccessible();
		}
		if (SubGoal is TurnToTarget)
		{
			if (GetTargetProp() is RabbitTrap rabbitTrap)
			{
				if (rabbitTrap.IsOpen)
				{
					rabbitTrap.TriggerTrap(character);
					character.MarkForDeletion();
				}
				return null;
			}
			return new EatCarrot(ActionAnim.Eat);
		}
		return null;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && SubGoal is EatCarrot)
		{
			return true;
		}
		if (IsTargetDeleted())
		{
			return false;
		}
		if (Target.Object is RabbitTrap { IsOpen: false })
		{
			return false;
		}
		return base.IsPossible(character, parent);
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
				RabbitTrap rabbitTrap = target.Object as RabbitTrap;
				Human human = target.Object as Human;
				if (rabbitTrap != null)
				{
					if (!rabbitTrap.IsOpen || character.GetBaseObjectType() != BaseObjectType.Rabbit)
					{
						continue;
					}
				}
				else if (!(target.Object is FoodProp) && (human == null || !target.GetFlag(TargetFlags.HasCarrot) || (!human.DirectControlled && (!Active || Target != target))))
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

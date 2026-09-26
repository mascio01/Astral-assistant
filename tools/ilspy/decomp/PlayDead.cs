using System;

public class PlayDead : StateMachineGoal
{
	public override GoalType GetGoalType()
	{
		return GoalType.PlayDead;
	}

	public override bool CanShowDialogOptions(Character character)
	{
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new AnimationGoal(ActionAnim.PlayDead));
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.PlayDead = false;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.CurrentActionAnim == ActionAnim.PlayDead)
		{
			if (!character.PlayDead)
			{
				WakeUp(character, parent);
			}
			else
			{
				if (!character.Zombie)
				{
					return;
				}
				foreach (Target target in character.Targets)
				{
					if (target.Object != null && !target.Object.Deleted && (!(target.Camouflage > 0f) || !(target.TimeSinceLastHeard >= TimeSpan.FromSeconds(2.0))) && target.Object is Human { Zombie: false })
					{
						WakeUp(character, parent);
						break;
					}
				}
			}
		}
		else if (!character.IsGettingUp())
		{
			character.PlayDead = false;
			Finished = true;
		}
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
		WakeUp(character, parent);
	}

	public void WakeUp(Character character, Goal parent)
	{
		if (character.CurrentActionAnim == ActionAnim.PlayDead)
		{
			SetSubGoal(character, parent, new AnimationGoal(character.AnimWrapper.PlayDeadOnFront ? ActionAnim.GetUp_Front : ActionAnim.GetUp_Back));
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.PlayDead)
		{
			return Active;
		}
		return true;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		character.PlayDead = false;
		return base.GetNextSubGoal(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!character.Zombie)
		{
			return GoalPriority.Survivor_PlayDead;
		}
		return GoalPriority.Zombie_PlayDead;
	}
}

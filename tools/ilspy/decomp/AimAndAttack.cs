public class AimAndAttack : StateMachineGoal
{
	private bool Assassinate;

	private SecrecyMode Secret;

	public AimAndAttack()
	{
	}

	public AimAndAttack(bool crouching, bool assassinate, SecrecyMode secret)
	{
		Crouching = crouching;
		Assassinate = assassinate;
		Secret = secret;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Assassinate);
		reflector.Add(ref Secret);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AimAndAttack;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.Sitting)
		{
			SetSubGoal(character, parent, new StopSittingGoal());
		}
		else if (character.CrouchingTransition > 0f && !character.IsCrouching())
		{
			SetSubGoal(character, parent, new UncrouchAnim());
		}
		else if (character.CrouchingTransition < 1f && character.IsCrouching())
		{
			SetSubGoal(character, parent, new CrouchAnim());
		}
		else
		{
			SetSubGoal(character, parent, new AimAnim(Assassinate));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		SetSubGoal(character, parent, null);
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is CrouchAnim || SubGoal is UncrouchAnim)
		{
			return new AimAnim(Assassinate);
		}
		if (SubGoal is AimAnim { Success: not false })
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && character.IsTargetSurrendering(targetCharacter))
			{
				return null;
			}
			return new AttackAnim(Assassinate, Secret);
		}
		UpdateUnsuccessfulShotCount(character, parent);
		return null;
	}

	private void UpdateUnsuccessfulShotCount(Character character, Goal parent)
	{
		if (SubGoal is AttackAnim attackAnim && parent is RangedAttack rangedAttack)
		{
			if (attackAnim.Success)
			{
				rangedAttack.UnsuccessfulShotCount = 0;
			}
			else
			{
				rangedAttack.UnsuccessfulShotCount++;
			}
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		AttackAnim attackAnim = SubGoal as AttackAnim;
		if ((attackAnim == null || !attackAnim.Success) && RangedAttack.IsAttackBlockedByAllies(character, Target, Assassinate))
		{
			Finished = true;
		}
		if (attackAnim != null && attackAnim.CanInterrupt(character) && character.IsLongEnoughSinceWeLastFired())
		{
			UpdateUnsuccessfulShotCount(character, parent);
			Finished = true;
		}
	}
}

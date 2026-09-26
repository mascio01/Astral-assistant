internal class PanicGoal : AnimationGoal
{
	public PanicGoal()
		: base(ActionAnim.Panic)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.PanicGoal;
	}

	public override bool IsHighAlert(Character character)
	{
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.IsBurning())
		{
			if (character.DirectControlled)
			{
				return false;
			}
			if (character.InsideBuilding != null && character.IsControllableByPlayer())
			{
				if (character.HangOutLocation == character.InsideBuilding.Tile)
				{
					return false;
				}
				if (character.SquadLeader != null && character.SquadLeader.DirectControlled)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!character.Zombie)
		{
			return GoalPriority.Survivor_Panic;
		}
		return GoalPriority.Zombie_Panic;
	}

	public override void OnPreActivate(Character character, Goal parent)
	{
		base.OnPreActivate(character, parent);
		character.AboutToBeInCombat = true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		if (character.InsideBuilding != null)
		{
			character.InsideBuilding.OnCharacterLeave(character, 0, fromBuildingDestroyed: false, fromRagdolled: false);
		}
		base.OnActivate(character, parent);
		character.AboutToBeInCombat = false;
		character.InCombat = true;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.InCombat = false;
		base.OnDeactivate(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.GrrArgh)
		{
			if (character.Zombie && !character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				character.PlayVoiceSoundFromList(SoundManager.ZombiePainSounds, VoiceSoundType.ZombieSnarl);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

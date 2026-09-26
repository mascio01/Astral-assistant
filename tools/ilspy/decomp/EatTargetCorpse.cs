public class EatTargetCorpse : AnimationGoal
{
	public static float CorpseConsumeRate = 5E-05f;

	public EatTargetCorpse()
		: base(ActionAnim.ZombieEatCorpse)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.EatTargetCorpse;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeletedOrDisappeared())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.PlayVoiceSoundFromList(SoundManager.ZombieEatSounds, VoiceSoundType.Eating);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.StopVoiceSound();
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!character.IsAnyVoiceSoundPlaying())
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieEatSounds, VoiceSoundType.Eating);
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.EatCorpse && !IsTargetDeleted())
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter == null || (!targetCharacter.Disappeared && !targetCharacter.IsAwake))
			{
				character.Heal(0.05f);
				Target.Object.Consume(character, CorpseConsumeRate, character.Infection);
				if (Target.Object.GetConsumedAmount() >= 1f && Target.Object.DeleteWhenSkinned())
				{
					Target.Object.DeleteOrDisappear(fromGoal: false);
				}
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

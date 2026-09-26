using System;

internal class ReadGoal : StateMachineGoal
{
	public Equipment GiftBook;

	public Equipment PreviouslyEquipped;

	public bool Success;

	public ReadGoal()
	{
	}

	public ReadGoal(Equipment giftBook)
	{
		GiftBook = giftBook;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref GiftBook);
		reflector.AddAfter(ref PreviouslyEquipped, 53);
		reflector.AddAfter(ref Success, 53);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ReadGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.EquippedItem != GiftBook)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(GiftBook));
		}
		else
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.ReadStart));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.EquippedItem = null;
		if (character.Speaking != null && character.Speaking.Situation == SpeechSituation.Reading)
		{
			character.OnSpeechFinished(interrupted: true);
		}
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is AnimationGoal animationGoal && animationGoal.GetAnim() == ActionAnim.ReadLoop && (character.Speaking == null || character.Speaking.Situation != SpeechSituation.Reading) && ((animationGoal.AnimState == SpeechAnimState.Started && character.GetActionAnimTime() >= TimeSpan.FromSeconds(10.0)) || animationGoal.AnimState == SpeechAnimState.Finished))
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.ReadFinish));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip equip && equip.Equipment == GiftBook && !Success)
		{
			return new AnimationGoal(ActionAnim.ReadStart);
		}
		if (SubGoal is AnimationGoal animationGoal)
		{
			switch (animationGoal.GetAnim())
			{
			case ActionAnim.ReadStart:
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, GiftBook, SpeechSituation.Reading);
				character.Speak(speechForSituation, null, GiftBook);
				return new AnimationGoal(ActionAnim.ReadLoop);
			}
			case ActionAnim.ReadFinish:
				if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
				{
					return new Equip(PreviouslyEquipped);
				}
				return null;
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		if (speech.Situation == SpeechSituation.Reading)
		{
			if (interrupted)
			{
				Finished = true;
			}
			else
			{
				SetSubGoal(character, parent, new AnimationGoal(ActionAnim.ReadFinish));
			}
		}
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishReading && GiftBook != null)
		{
			character.OnReadBook(GiftBook.GetPrototype());
			character.Inventory.UseItem(character, GiftBook, 1);
			Success = true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

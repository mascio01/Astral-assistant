using System;

public class MoveToAndSkin : StateMachineGoal
{
	public bool Success;

	public bool DontOpenOurGates;

	public bool IsGathering;

	public Equipment Meat;

	public MovementType MovementType = MovementType.Walk;

	public Equipment PreviouslyEquipped;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndSkin;
	}

	public MoveToAndSkin()
	{
	}

	public MoveToAndSkin(Character character, Character targetCharacter, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetCharacter));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.AddAfter(ref IsGathering, 353);
		reflector.Add(ref MovementType);
		reflector.AddAfter(ref Meat, 324);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.SkinnedAmount >= 1f)
		{
			Finished = true;
			return;
		}
		if (character.EquippedItem is HuntingKnife)
		{
			SetSubGoal(character, parent, new MoveWithinRangeOfTarget(MovementType, aiming: false, 0.75f, 1.5f, DontOpenOurGates));
			return;
		}
		HuntingKnife huntingKnife = character.Inventory.GetHuntingKnife();
		if (huntingKnife != null)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(huntingKnife));
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Success)
		{
			return true;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return false;
		}
		if (targetCharacter.Alive)
		{
			return false;
		}
		if (targetCharacter.SkinnedAmount >= 1f)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip && character.EquippedItem is HuntingKnife && !Success)
		{
			return new MoveWithinRangeOfTarget(MovementType, aiming: false, 0.75f, 1.5f, DontOpenOurGates);
		}
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget)
		{
			if (moveWithinRangeOfTarget.Success)
			{
				return new AnimationGoal(ActionAnim.Skin);
			}
			return new MoveAdjacentToTarget(MovementType, DontOpenOurGates, canBeOnTile: true);
		}
		if (SubGoal is MoveAdjacentToTarget { Success: not false })
		{
			return new AnimationGoal(ActionAnim.Skin);
		}
		if (SubGoal is AnimationGoal && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			return new Equip(PreviouslyEquipped);
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		AnimationEventType eventType = animEvent.EventType;
		if (eventType != AnimationEventType.StartSkinning && eventType == AnimationEventType.FinishSkinning)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.SkinnedAmount < 1f)
			{
				bool flag = targetCharacter.DeleteWhenSkinned();
				if (flag)
				{
					while (targetCharacter.Inventory.Count > 0)
					{
						Equipment item = targetCharacter.Inventory.GetItem(0);
						int amount = item.GetAmount();
						item = targetCharacter.Inventory.Take(targetCharacter, item, amount);
						item = character.Inventory.Add(character, item);
						if (IsGathering)
						{
							item.IncrementGatheredAmount(amount);
						}
					}
				}
				int meatAmount = targetCharacter.GetMeatAmount(character.GetSkillLevelWithEffects(SkillType.Cooking));
				if (meatAmount > 0)
				{
					Equipment equipment = Equipment.Spawn(targetCharacter.GetMeatType(), meatAmount);
					equipment.InfectedWith = ((targetCharacter.Infection == InfectionType.None) ? targetCharacter.GetWorstInfectionTypeInProgression() : targetCharacter.Infection);
					if (targetCharacter is Human && character.EquippedItem is HuntingKnife huntingKnife)
					{
						equipment.InfectedWith = (InfectionType)Math.Max((int)equipment.InfectedWith, (int)huntingKnife.InfectedWith);
						huntingKnife.DosesRemaining = Math.Max(0, huntingKnife.DosesRemaining - 1);
					}
					Meat = character.Inventory.Add(character, equipment);
					if (IsGathering)
					{
						Meat.IncrementGatheredAmount(meatAmount);
					}
					NotificationManager.Instance.AddEquipmentNotification(null, character, equipment, meatAmount);
				}
				character.Skillset.AddProgress(character, SkillType.Cooking, 1f);
				targetCharacter.SkinnedAmount = 1f;
				if (character.IsAuthoritative())
				{
					StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Skinned, character, targetCharacter);
				}
				if (flag)
				{
					targetCharacter.DeleteOrDisappear(fromGoal: false);
				}
				if (targetCharacter is Human)
				{
					Memory.OnMemorableEvent(MemoryPrototype.DesecratedCorpse, character, targetCharacter, 1f, secret: false);
				}
				Success = true;
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

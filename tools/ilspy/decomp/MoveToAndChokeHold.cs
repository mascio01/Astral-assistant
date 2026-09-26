using System;

public class MoveToAndChokeHold : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool Assassinate;

	public SecrecyMode Secrecy = SecrecyMode.IgnoredByObjectCommunity;

	public HoldType HoldType;

	public Equipment PreviouslyEquipped;

	public TimeSpan StartedTime;

	private const float MaxRange = 1.25f;

	public MoveToAndChokeHold()
	{
	}

	public MoveToAndChokeHold(HoldType holdType)
	{
		HoldType = holdType;
	}

	public MoveToAndChokeHold(Character character, TileObject targetObj, MovementType movementType, HoldType holdType, bool assassinate)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		HoldType = holdType;
		Assassinate = assassinate;
	}

	public MoveToAndChokeHold(Character character, TileObject targetObj, MovementType movementType, HoldType holdType, bool assassinate, SecrecyMode secrecy)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		HoldType = holdType;
		Assassinate = assassinate;
		Secrecy = secrecy;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndChokeHold;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref Assassinate, 402);
		reflector.AddAfter(ref Secrecy, 613);
		if (reflector.Version < 345)
		{
			bool value = false;
			reflector.Add(ref value);
			HoldType = (value ? HoldType.SlitThroat : HoldType.ChokeHold);
		}
		else
		{
			reflector.Add(ref HoldType);
		}
		reflector.AddAfter(ref PreviouslyEquipped, 118);
		reflector.AddAfter(ref StartedTime, 556);
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (!character.IsChokingSomeone() && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartedTime <= TimeSpan.FromSeconds(2.0) && GameCursor.CanChokeHold(character, GetTargetCharacter()))
		{
			return AIOverridesControlReason.Animation;
		}
		if (SubGoal is StartChokeHold)
		{
			return AIOverridesControlReason.Animation;
		}
		if (SubGoal is FinishChokeHold)
		{
			return AIOverridesControlReason.Animation;
		}
		return AIOverridesControlReason.None;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (HoldType == HoldType.SlitThroat && character.Inventory.GetHuntingKnife() == null)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		StartedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (GetTargetObject() == null)
		{
			Finished = true;
			Success = true;
		}
		else
		{
			MoveTo goal = new MoveWithinRangeOfTarget(MovementType, aiming: false, 0f, 1.25f, DontOpenOurGates);
			SetSubGoal(character, parent, goal);
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (SubGoal is StartChokeHold || SubGoal is LoopChokeHold)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.InteractionObject == character && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChokeHoldCancelled(character);
			}
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
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || character.IsBeingBittenOrChoked() || character.IsParrying() || character.IsDodging() || character.IsRagdollOrProneOrRecovering() || character.IsInDamageReactionAnim() || character.IsGettingUp())
		{
			Finished = true;
		}
		else
		{
			if (!(SubGoal is StartChokeHold) && !(SubGoal is LoopChokeHold))
			{
				return;
			}
			if (!targetCharacter.IsAwake || targetCharacter.InteractionObject != character)
			{
				SetSubGoal(character, parent, new FinishChokeHold(HoldType));
			}
			else if (HoldType != HoldType.Restrain && targetCharacter.IsFacing(character.PosXZ, MathF.PI / 2f))
			{
				if (parent is InvisibleStrainSpreadGoal)
				{
					targetCharacter.SetFacingAngle(character.FacingAngle);
					return;
				}
				character.OnCancelChokeHold();
				Finished = true;
			}
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip { Equipment: null } && !Success)
		{
			return new MoveWithinRangeOfTarget(MovementType, aiming: false, 0f, 1.25f, DontOpenOurGates);
		}
		if (SubGoal is MoveTo { Success: not false } && GetTargetObject() != null)
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget && GetTargetObject() != null && !IsTargetDeleted())
		{
			Character targetCharacter = GetTargetCharacter();
			if (character.IsPredicted() == targetCharacter.IsPredicted() && !targetCharacter.OnChokeHoldStarted(character, HoldType))
			{
				return null;
			}
			if (character.IsAuthoritative())
			{
				if (HoldType == HoldType.Restrain)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, targetCharacter.SparringPartner, SpeechSituation.Restrain);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, targetCharacter, targetCharacter.SparringPartner);
					}
				}
				else
				{
					Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Choke, targetCharacter.Pos, 0f, character.GetMaxSoundVisibilityRange(), character, character, targetCharacter, targetCharacter));
				}
			}
			if (HoldType != HoldType.SlitThroat && character.EquippedItem != null)
			{
				PreviouslyEquipped = character.EquippedItem;
				character.DesiredEquippedItem = (character.EquippedItem = null);
			}
			if (HoldType == HoldType.SlitThroat && !(character.EquippedItem is HuntingKnife))
			{
				PreviouslyEquipped = character.EquippedItem;
				character.DesiredEquippedItem = (character.EquippedItem = character.Inventory.GetHuntingKnife());
			}
			return new StartChokeHold(HoldType);
		}
		if (SubGoal is StartChokeHold startChokeHold)
		{
			return new LoopChokeHold(HoldType, startChokeHold.PosWhenAnimStarted);
		}
		if (SubGoal is FinishChokeHold && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			SetCrouching(character, parent, HoldType != HoldType.Restrain);
			return new Equip(PreviouslyEquipped);
		}
		return null;
	}

	public override void OnChokeSucceeded(Character character, Goal parent, Character target)
	{
		if (target.BloodLoss >= 0.95f && target.WillActivateInvisibleStrain())
		{
			character.OnCancelChokeHold();
			Finished = true;
		}
		else
		{
			SetSubGoal(character, parent, new FinishChokeHold(HoldType));
			Success = true;
		}
	}

	public override void OnTargetStruggledFree(Character character, Goal parent, Character target)
	{
		if (Target.Object == target && (SubGoal is LoopChokeHold || SubGoal is StartChokeHold))
		{
			character.OnCancelChokeHold();
			Finished = true;
		}
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}
}

using System;
using System.Text;

public class MoveToAndInteractGoal : StateMachineGoal
{
	public static string[] InteractionTypeNames = StringUtil.GetEnumNames<InteractionType>("Invalid");

	private MovementType _movementType = MovementType.Walk;

	private InteractionType _interactionType;

	private EquipmentPrototype Prototype;

	private int Price;

	private Speech _speechOnFinish;

	private BaseObject SpeechOnFinishReferringTo;

	public Equipment Item;

	public int AmountToGive;

	public float AmountToEat;

	public TileObject CarriedObject;

	public bool DontOpenOurGates;

	public bool CanUnlockGatesFromInsideWithKey;

	public bool Success;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndInteractGoal;
	}

	public InteractionType GetInteractionType()
	{
		return _interactionType;
	}

	public MoveToAndInteractGoal()
	{
	}

	public MoveToAndInteractGoal(InteractionType interactionType, int price, Speech speechOnFinish, EquipmentPrototype proto, BaseObject speechOnFinishReferringTo)
	{
		_interactionType = interactionType;
		Prototype = proto;
		Price = price;
		_speechOnFinish = speechOnFinish;
		SpeechOnFinishReferringTo = speechOnFinishReferringTo;
	}

	public MoveToAndInteractGoal(Character character, TileObject targetObj, InteractionType interactionType, int price, Speech speechOnFinish, EquipmentPrototype proto, BaseObject speechOnFinishReferringTo, int amountToGive = 1)
	{
		_interactionType = interactionType;
		Prototype = proto;
		Price = price;
		AmountToGive = amountToGive;
		_speechOnFinish = speechOnFinish;
		SpeechOnFinishReferringTo = speechOnFinishReferringTo;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveToAndInteractGoal(Character character, TileObject targetObj, InteractionType interactionType, Equipment item, MovementType movementType)
	{
		_interactionType = interactionType;
		_movementType = movementType;
		Item = item;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveToAndInteractGoal(Character character, TileObject targetObj, InteractionType interactionType, Equipment item, int amountToGive, MovementType movementType)
	{
		_interactionType = interactionType;
		_movementType = movementType;
		Item = item;
		AmountToGive = amountToGive;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveToAndInteractGoal(Character character, TileObject targetObj, InteractionType interactionType, Equipment item, float amountToEat, MovementType movementType)
	{
		_interactionType = interactionType;
		_movementType = movementType;
		Item = item;
		AmountToEat = amountToEat;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveToAndInteractGoal(Character character, TileObject targetObj, InteractionType interactionType, TileObject carriedObject)
	{
		_interactionType = interactionType;
		CarriedObject = carriedObject;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public MoveToAndInteractGoal(InteractionType interactionType, Equipment item, MovementType movementType)
	{
		_interactionType = interactionType;
		_movementType = movementType;
		Item = item;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref _interactionType);
		reflector.Add(ref Prototype);
		reflector.AddAfter(ref Item, 369);
		reflector.AddAfter(ref AmountToGive, 531);
		reflector.AddAfter(ref AmountToEat, 541);
		reflector.Add(ref Price);
		reflector.Add(ref _speechOnFinish);
		reflector.AddAfter(ref SpeechOnFinishReferringTo, 260);
		reflector.AddAfter(ref CarriedObject, 26);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.AddAfter(ref CanUnlockGatesFromInsideWithKey, 549);
		reflector.AddAfter(ref Success, 32);
	}

	public override void BuildDebugExtraInfoString(Character character, Goal parent, StringBuilder str)
	{
		base.BuildDebugExtraInfoString(character, parent, str);
		str.Append(' ');
		str.Append('(');
		str.Append(InteractionTypeNames[(int)_interactionType]);
		str.Append(')');
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (_interactionType == InteractionType.KillUnconscious && !(character.EquippedItem is HuntingKnife))
		{
			HuntingKnife huntingKnife = character.Inventory.GetHuntingKnife();
			if (huntingKnife != null)
			{
				SetSubGoal(character, parent, new Equip(huntingKnife));
				return;
			}
		}
		if (_interactionType == InteractionType.EatFromPot && character.InsideBuilding != null && character.InsideBuilding == GetTargetBuilding())
		{
			SetSubGoal(character, parent, new InteractAnim(_interactionType, null, Price, AmountToGive, AmountToEat, Prototype, Item));
		}
		else
		{
			SetSubGoal(character, parent, GetMoveToGoal(character));
		}
	}

	private Goal GetMoveToGoal(Character character)
	{
		Prop targetProp = GetTargetProp();
		if (targetProp != null && targetProp.GetMaxTile() != targetProp.GetMinTile())
		{
			return new MoveAdjacentToTarget(_movementType, DontOpenOurGates, canBeOnTile: false)
			{
				CanUnlockGatesFromInsideWithKey = CanUnlockGatesFromInsideWithKey
			};
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.InsideBuilding != null)
		{
			return new MoveAdjacentToTarget(character, targetCharacter.InsideBuilding, _movementType, DontOpenOurGates, canBeOnTile: false)
			{
				CanUnlockGatesFromInsideWithKey = CanUnlockGatesFromInsideWithKey
			};
		}
		float maxRange = 1.25f;
		InteractionType interactionType = _interactionType;
		if ((uint)(interactionType - 4) <= 1u || interactionType == InteractionType.HugThenTalk)
		{
			maxRange = HugTarget.MaxStartHugDist - 0.01f;
		}
		return new MoveWithinRangeOfTarget(_movementType, aiming: false, 0f, maxRange, DontOpenOurGates)
		{
			CanUnlockGatesFromInsideWithKey = CanUnlockGatesFromInsideWithKey
		};
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (_interactionType == InteractionType.Bury)
		{
			TileObject targetObject = GetTargetObject();
			Grave grave = targetObject as Grave;
			if ((character.CarryingObject != CarriedObject || targetObject == null || (grave != null && grave.Corpse != null)) && !(SubGoal is InteractAnim { Success: not false }))
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is Equip)
		{
			return GetMoveToGoal(character);
		}
		if (SubGoal is MoveWithinRangeOfTarget { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is MoveAdjacentToTarget { Success: not false } moveAdjacentToTarget)
		{
			if (targetCharacter != null && moveAdjacentToTarget.GetTargetBuilding() != null)
			{
				if (targetCharacter.InsideBuilding != null)
				{
					return null;
				}
				return GetMoveToGoal(character);
			}
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			switch (_interactionType)
			{
			case InteractionType.Hug:
			case InteractionType.HugThenTalk:
				return new HugTarget(TimeSpan.FromSeconds(2.0), shoot: false);
			case InteractionType.HugShoot:
				return new HugTarget(TimeSpan.FromSeconds(2.0), shoot: true);
			case InteractionType.RepairVehicleWithItem:
				if (character.EquippedItem != null)
				{
					return new Equip(null);
				}
				return new AnimationGoal(ActionAnim.CraftStart);
			default:
				return new InteractAnim(_interactionType, targetCharacter, Price, AmountToGive, AmountToEat, Prototype, Item);
			}
		}
		if (SubGoal is Equip && _interactionType == InteractionType.RepairVehicleWithItem)
		{
			return new AnimationGoal(ActionAnim.CraftStart);
		}
		if (SubGoal is InteractAnim { Success: not false })
		{
			Success = true;
			if (_speechOnFinish != null)
			{
				if (_interactionType == InteractionType.RepairVehicleWithItem)
				{
					return new AnimationGoal(ActionAnim.CraftEnd);
				}
				targetCharacter?.PushQueuedSpeech(_speechOnFinish, character, SpeechOnFinishReferringTo, default(MemoryParam));
			}
		}
		if (SubGoal is HugTarget { Success: not false })
		{
			Success = true;
			if (_speechOnFinish != null && targetCharacter != null)
			{
				if (_interactionType == InteractionType.HugShoot || _interactionType == InteractionType.HugThenTalk)
				{
					character.PushQueuedSpeech(_speechOnFinish, targetCharacter, SpeechOnFinishReferringTo, default(MemoryParam));
				}
				else
				{
					targetCharacter.PushQueuedSpeech(_speechOnFinish, character, SpeechOnFinishReferringTo, default(MemoryParam));
				}
			}
		}
		if (SubGoal is AnimationGoal animationGoal)
		{
			switch (animationGoal.GetAnim())
			{
			case ActionAnim.CraftStart:
				return new InteractAnim(_interactionType, GetTargetObject(), Price, AmountToGive, AmountToEat, Prototype, Item);
			case ActionAnim.CraftEnd:
				if (_interactionType == InteractionType.RepairVehicleWithItem)
				{
					if (character.Inventory.UseItemOfType(character, character, Prototype, AmountToGive, null, out var _) > 0)
					{
						GetTargetVehicle()?.SetDriveable(driveable: true);
					}
					if (_speechOnFinish != null)
					{
						character.PushQueuedSpeech(_speechOnFinish, SpeechOnFinishReferringTo as Character, GetTargetObject(), default(MemoryParam));
					}
				}
				return null;
			}
		}
		return null;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		_movementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return _movementType;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (_interactionType != InteractionType.HugShoot)
		{
			return AIOverridesControlReason.None;
		}
		return AIOverridesControlReason.Animation;
	}
}

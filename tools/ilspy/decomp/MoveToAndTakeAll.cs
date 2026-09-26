public class MoveToAndTakeAll : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndTakeAll;
	}

	public override bool IsCollectingSomethingFrom(TileObject obj)
	{
		return GetTargetObject() == obj;
	}

	public MoveToAndTakeAll()
	{
	}

	public MoveToAndTakeAll(Character character, TileObject targetObject, MovementType movementType)
	{
		_movementType = movementType;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new MoveAdjacentToTarget(_movementType, Target.Object is Character));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.IsAwake)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentToTarget { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			return new AnimationGoal(GetTargetObject()?.GetTakeAnim() ?? ActionAnim.Scavenge);
		}
		return null;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take)
		{
			character.RemoveFailedFindAttempt(Target.Object);
			bool flag = character.IsControllableByPlayer();
			if (flag)
			{
				Target.Object.MarkInvestigated(character);
			}
			EquipmentContainer equipmentContainer = ((!IsTargetDeleted()) ? Target.Object.GetInventory() : null);
			if (equipmentContainer != null)
			{
				int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Strength);
				Equipment bestBackpack = equipmentContainer.GetBestBackpack(skillLevelWithEffects);
				Equipment bestBackpack2 = character.Inventory.GetBestBackpack(skillLevelWithEffects);
				if (bestBackpack != null && (bestBackpack2 == null || bestBackpack.GetCarryWeightEffect(skillLevelWithEffects) > bestBackpack2.GetCarryWeightEffect(skillLevelWithEffects)))
				{
					bestBackpack = equipmentContainer.Take(Target.Object, bestBackpack, bestBackpack.GetAmount());
					if (bestBackpack != null)
					{
						NotificationManager.Instance.AddEquipmentNotification(Target.Object, character, bestBackpack, bestBackpack.GetAmount());
					}
					character.Inventory.Add(character, bestBackpack);
				}
				PlayerRecord playerControllingMe = character.GetPlayerControllingMe();
				float num = 0f;
				while (!equipmentContainer.IsEmptyExceptForWornItems(Target.Object))
				{
					Equipment equipment = equipmentContainer.FindLightestTakeableItem(Target.Object);
					if (equipment == null)
					{
						break;
					}
					TileObject tileObject = character;
					int amountOfEquipmentThatCanBeStored = character.GetAmountOfEquipmentThatCanBeStored(equipment);
					if (amountOfEquipmentThatCanBeStored <= 0)
					{
						if (flag)
						{
							break;
						}
						tileObject = character.Community.GetNearestMemberOrBuildingWithInventorySpaceFor(character.Tile, character, BaseObjectType.Human, float.MaxValue, equipment.GetWeight());
						if (tileObject == null)
						{
							break;
						}
						amountOfEquipmentThatCanBeStored = tileObject.GetAmountOfEquipmentThatCanBeStored(equipment);
						if (amountOfEquipmentThatCanBeStored <= 0)
						{
							break;
						}
					}
					bool num2 = amountOfEquipmentThatCanBeStored >= equipment.GetAmount();
					equipment = equipmentContainer.Take(Target.Object, equipment, amountOfEquipmentThatCanBeStored);
					if (equipment != null)
					{
						int amount = equipment.GetAmount();
						NotificationManager.Instance.AddEquipmentNotification(Target.Object, tileObject, equipment, amount);
						num += equipment.GetBasePrice() * (float)amount;
						equipment = tileObject.GetInventory().Add(tileObject, equipment);
						if (!flag && tileObject is Character character2 && character2.GetGatherGoal() != null)
						{
							equipment.IncrementGatheredAmount(amount);
							character2.AddRole(new RoleInfo(Role.Organizer));
						}
					}
					if (playerControllingMe != null)
					{
						playerControllingMe.HasScavengedAnyEquipment = true;
					}
					if (!num2)
					{
						break;
					}
				}
				if (Target.Object.GetCommunity() != null && Target.Object.GetCommunity() != character.Community)
				{
					character.OnStoleSomething(Target.Object.GetCommunity(), GetTargetCharacter(), num);
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
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
}

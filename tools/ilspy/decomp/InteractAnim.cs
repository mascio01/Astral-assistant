using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractAnim : AnimationGoal
{
	private InteractionType _interactionType;

	private EquipmentPrototype Prototype;

	private Equipment Item;

	private int Price;

	public int AmountToGive;

	public float AmountToEat;

	public bool Success;

	public InteractAnim()
	{
	}

	private static ActionAnim PickAnim(InteractionType interactionType, TileObject targetObj)
	{
		switch (interactionType)
		{
		case InteractionType.BludgeonUnconscious:
			return ActionAnim.BludgeonUnconscious;
		case InteractionType.KillUnconscious:
			return ActionAnim.KillUnconscious;
		case InteractionType.Bury:
			return ActionAnim.Drop;
		case InteractionType.Knock:
			return ActionAnim.Knock;
		case InteractionType.RepairVehicleWithItem:
			return ActionAnim.CraftNonLooped;
		default:
			if (!(targetObj is Character) || !((Character)targetObj).IsRagdollOrProneOrRecovering())
			{
				return ActionAnim.Scavenge;
			}
			return ActionAnim.ScavengeCorpse;
		}
	}

	public InteractAnim(InteractionType interactionType, TileObject targetObj, int price, int amountToGive, float amountToEat, EquipmentPrototype proto, Equipment item)
		: base(PickAnim(interactionType, targetObj))
	{
		_interactionType = interactionType;
		Price = price;
		Prototype = proto;
		Item = item;
		AmountToGive = amountToGive;
		AmountToEat = amountToEat;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _interactionType);
		reflector.Add(ref Item);
		reflector.Add(ref Prototype);
		reflector.Add(ref Price);
		reflector.AddAfter(ref AmountToGive, 531);
		reflector.AddAfter(ref AmountToEat, 541);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.InteractAnim;
	}

	public override bool IsUsingEquippedItem()
	{
		if (_interactionType != InteractionType.ApplyBandage && _interactionType != InteractionType.ApplyBandageAndSpeak && _interactionType != InteractionType.GiveAntigen)
		{
			return _interactionType == InteractionType.Feed;
		}
		return true;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		Character targetCharacter = GetTargetCharacter();
		switch (animEvent.EventType)
		{
		case AnimationEventType.Take:
			switch (_interactionType)
			{
			case InteractionType.ApplyBandage:
			case InteractionType.ApplyBandageAndSpeak:
				if (targetCharacter == null)
				{
					break;
				}
				while (targetCharacter.Inventory.GetGoldAmount() >= Price)
				{
					Equipment equipment5 = character.Inventory.FindItemWithHighestBandageLevel();
					if (equipment5 == null)
					{
						break;
					}
					int bandageLevel = equipment5.GetBandageLevel();
					int num5 = Mathf.Min(character.GetSkillLevelWithEffects(SkillType.Medicine), bandageLevel);
					if (!targetCharacter.HasUnbandagedInjury(num5))
					{
						break;
					}
					if (!character.HasInfiniteBandages(equipment5, targetCharacter))
					{
						character.Inventory.UseItem(character, equipment5, 1);
					}
					if (Price > 0)
					{
						Equipment equipment6 = targetCharacter.Inventory.Take(targetCharacter, targetCharacter.Inventory.GetGold(), Price);
						NotificationManager.Instance.AddEquipmentNotification(targetCharacter, character, equipment6, equipment6.GetAmount());
						character.Inventory.Add(character, equipment6);
					}
					targetCharacter.ApplyBandage(character, equipment5, playSound: true, Goal.WantPrediction(GetGoalType()));
					if (targetCharacter.AliveAndNotZombie)
					{
						Memory.OnMemorableEvent((Price == 0) ? MemoryPrototype.BandagedForFree : MemoryPrototype.BandagedForGold, character, targetCharacter, (float)num5 / 5f, secret: false);
					}
					if (_interactionType == InteractionType.ApplyBandageAndSpeak && character.Speaking == null)
					{
						Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, Item, SpeechSituation.ApplyBandage);
						if (speechForSituation4 != null)
						{
							character.Speak(speechForSituation4, targetCharacter, Item);
						}
					}
				}
				Success = true;
				break;
			case InteractionType.GiveAntigen:
			{
				if (targetCharacter == null || targetCharacter.Inventory.GetGoldAmount() < Price)
				{
					break;
				}
				if (Prototype == null && Item != null)
				{
					Prototype = Item.GetPrototype();
				}
				if (character.Inventory.UseItemOfType(character, character, Prototype, 1, null, out var infectionType) == 1)
				{
					if (Price > 0)
					{
						Equipment equipment7 = targetCharacter.Inventory.Take(targetCharacter, targetCharacter.Inventory.GetGold(), Price);
						NotificationManager.Instance.AddEquipmentNotification(targetCharacter, character, equipment7, equipment7.GetAmount());
						character.Inventory.Add(character, equipment7);
					}
					targetCharacter.InjectAntigen(character, Prototype.AntigenType, infectionType, Item, Goal.WantPrediction(GetGoalType()));
					Memory.OnMemorableEvent((Price == 0) ? MemoryPrototype.BandagedForFree : MemoryPrototype.BandagedForGold, character, targetCharacter, 1f, secret: false);
					if (!targetCharacter.IsAwake && character.Speaking == null)
					{
						Speech speechForSituation5 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, Item, SpeechSituation.GiveAntigen);
						if (speechForSituation5 != null)
						{
							character.Speak(speechForSituation5, targetCharacter, Item);
						}
					}
				}
				Success = true;
				break;
			}
			case InteractionType.GiveAllGold:
			{
				if (targetCharacter == null)
				{
					break;
				}
				Equipment gold = character.Inventory.GetGold();
				if (gold != null)
				{
					int num = Mathf.Min(gold.GetAmount(), targetCharacter.GetAmountOfEquipmentThatCanBeStored(gold));
					if (num > 0)
					{
						Equipment equipment = character.Inventory.Take(character, gold, num);
						if (equipment != null)
						{
							NotificationManager.Instance.AddEquipmentNotification(character, targetCharacter, equipment, equipment.GetAmount());
							targetCharacter.Inventory.Add(targetCharacter, equipment);
						}
					}
				}
				Success = true;
				break;
			}
			case InteractionType.GiveAllSupplies:
				if (targetCharacter == null)
				{
					break;
				}
				foreach (Equipment item in character.Inventory.GetEquipmentSortedByPriceDescending())
				{
					int num2 = Math.Min(item.GetAmount(), targetCharacter.GetAmountOfEquipmentThatCanBeStored(item));
					if (num2 > 0)
					{
						Equipment equipment2 = character.Inventory.Take(character, item, num2);
						if (equipment2 != null)
						{
							NotificationManager.Instance.AddEquipmentNotification(character, targetCharacter, equipment2, equipment2.GetAmount());
							targetCharacter.Inventory.Add(targetCharacter, equipment2);
						}
					}
				}
				Success = true;
				break;
			case InteractionType.GiveGift:
			{
				if (targetCharacter == null || !character.InventoryContains(Item))
				{
					break;
				}
				if (AmountToGive == 0)
				{
					AmountToGive = 1;
				}
				Equipment equipment4 = character.Inventory.Take(character, Item, AmountToGive);
				equipment4.Gifted = true;
				NotificationManager.Instance.AddEquipmentNotification(character, targetCharacter, equipment4, equipment4.GetAmount());
				float quantityFactor2 = equipment4.CalcGiftUnits();
				targetCharacter.Inventory.Add(targetCharacter, equipment4);
				if (equipment4.GetPrototype() == EquipmentPrototype.Gold)
				{
					character.ReservedGoldAmount = Math.Max(0, character.ReservedGoldAmount - equipment4.GetAmount());
				}
				if (!equipment4.Gifted)
				{
					break;
				}
				SpeechSituation speechSituation = SpeechSituation.None;
				List<string> list = new List<string>();
				if (targetCharacter.LikesGift(Item, list))
				{
					Memory.OnMemorableEvent(MemoryPrototype.GaveThoughtfulGift, character, targetCharacter, quantityFactor2, secret: false);
					speechSituation = SpeechSituation.ReceiveGiftLike;
				}
				else if (targetCharacter.DislikesGift(Item, list))
				{
					Memory.OnMemorableEvent(MemoryPrototype.GaveInappropriateGift, character, targetCharacter, quantityFactor2, secret: false);
					speechSituation = SpeechSituation.ReceiveGiftDislike;
				}
				else
				{
					Memory.OnMemorableEvent(MemoryPrototype.GaveGift, character, targetCharacter, quantityFactor2, secret: false);
					speechSituation = SpeechSituation.ReceiveGiftNeutral;
				}
				foreach (string item2 in list)
				{
					targetCharacter.SetPersonalityKnown(item2);
				}
				if (speechSituation != SpeechSituation.None && _interactionType != InteractionType.GiveGiftNoReply)
				{
					Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(targetCharacter, character, Item, speechSituation);
					targetCharacter.Speak(speechForSituation3, character, Item);
				}
				Success = true;
				break;
			}
			case InteractionType.EatFromPot:
			{
				if (!(GetTargetObject() is Prop prop) || Item == null || !prop.Inventory.Contains(Item))
				{
					break;
				}
				LiquidPrototype liquidContentsType = Item.GetLiquidContentsType();
				if (liquidContentsType != null && liquidContentsType.DrinkableOrEdible)
				{
					float num3 = character.ConsumeLiquid(Item, playSound: true, fromInfoScreen: false, (AmountToEat == 0f) ? float.MaxValue : AmountToEat);
					character.OnStoleSomething(prop.GetCommunity(), null, num3 * liquidContentsType.BasePricePerFlOz);
					Success = true;
				}
				else if (Item.IsEdible())
				{
					Equipment equipment3 = prop.Inventory.Take(prop, Item, (AmountToEat == 0f) ? 1 : ((int)AmountToEat));
					if (equipment3 != null)
					{
						float basePrice = Item.GetBasePrice();
						character.Eat(equipment3, playSound: true, fromInfoScreen: false, null);
						character.OnStoleSomething(prop.GetCommunity(), null, basePrice);
						equipment3.Delete();
						Success = true;
					}
				}
				break;
			}
			case InteractionType.FillFromPot:
				if (GetTargetObject() is Campfire campfire && Item != null && campfire.Inventory.Contains(Item) && Item.GetPrototype() == EquipmentPrototype.Pot && Item.GetLiquidContentsType() != null)
				{
					Equipment bestLiquidContainerToFill = character.Inventory.GetBestLiquidContainerToFill(Item.GetLiquidContentsType());
					if (bestLiquidContainerToFill != null)
					{
						float maxAmount = Math.Min((AmountToEat == 0f) ? float.MaxValue : AmountToEat, bestLiquidContainerToFill.GetLiquidCapacity() - bestLiquidContainerToFill.GetLiquidContentsAmount() + 0.001f);
						bestLiquidContainerToFill.FillLiquid(Item.GetLiquidContentsType(), Item.DrainLiquid(maxAmount), Item.InfectedWith);
						Success = true;
					}
				}
				break;
			case InteractionType.Feed:
				if (targetCharacter == null || !targetCharacter.AliveAndNotZombie || Item == null)
				{
					break;
				}
				if (targetCharacter is Animal && targetCharacter.IsAwake && targetCharacter.LikesFood(Item.GetPrototype()))
				{
					if (targetCharacter.GetGoal() is AnimalGoal animalGoal)
					{
						targetCharacter.Eat(Item, playSound: true, fromInfoScreen: false, character);
						character.Inventory.UseItem(character, Item, 1);
						animalGoal.FeedFromHand(targetCharacter, null, character);
						StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.FeedAnimal, character, targetCharacter);
						Success = true;
					}
				}
				else
				{
					if (targetCharacter.IsAwake)
					{
						break;
					}
					float num4 = 0f;
					if (character.Speaking == null)
					{
						Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, Item, SpeechSituation.Feed);
						if (speechForSituation2 != null)
						{
							character.Speak(speechForSituation2, targetCharacter, Item);
						}
					}
					if (Item.GetLiquidContentsType() != null)
					{
						num4 = targetCharacter.ConsumeLiquid(Item, playSound: true, fromInfoScreen: false) / Sun.DayLengthSecs;
					}
					else if (targetCharacter.GetHunger() >= Item.GetNutrition())
					{
						targetCharacter.Eat(Item, playSound: true, fromInfoScreen: false, character);
						character.Inventory.UseItem(character, Item, 1);
						num4 = Item.GetNutrition() / Sun.DayLengthSecs;
					}
					if (num4 > 0f)
					{
						Memory.OnMemorableEvent(MemoryPrototype.LookedAfter, character, targetCharacter, num4, secret: false);
						Success = true;
					}
				}
				break;
			case InteractionType.GiveWater:
			{
				if (targetCharacter == null || targetCharacter.IsAwake || !targetCharacter.AliveAndNotZombie || Item == null || Item.GetLiquidContentsType() == null)
				{
					break;
				}
				float quantityFactor = targetCharacter.ConsumeLiquid(Item, playSound: true, fromInfoScreen: false) / Character.WaterNeededPerDayInFlOz;
				Memory.OnMemorableEvent(MemoryPrototype.LookedAfter, character, targetCharacter, quantityFactor, secret: false);
				if (character.Speaking == null)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, Item, SpeechSituation.GiveWater);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, targetCharacter, Item);
					}
				}
				Success = true;
				break;
			}
			case InteractionType.Unlock:
				if (GetTargetObject() is Gate gate2 && character.Inventory.FindItemOfType(gate2.KeyProto) != null)
				{
					gate2.Unlock();
				}
				Success = true;
				break;
			}
			break;
		case AnimationEventType.BludgeonUnconscious:
			character.OnMeleeAttack(targetCharacter, AttackType.BludgeonUnconscious, InjuryType.Punch, Bone.RightHand, Character.PunchDamage * character.GetUnarmedDamageModifier(), assassinate: false, stealthy: true);
			Success = true;
			break;
		case AnimationEventType.KillUnconscious:
			character.OnMeleeAttack(targetCharacter, AttackType.KillUnconscious, InjuryType.SharpObject, Bone.RightHand, 1f, assassinate: true, stealthy: true);
			Success = true;
			break;
		case AnimationEventType.Drop:
		{
			Character character2 = character.CarryingObject as Character;
			if (GetTargetObject() is Grave grave && character2 != null)
			{
				grave.Bury(character2);
				Success = true;
			}
			Building targetBuilding = GetTargetBuilding();
			if (targetBuilding != null && character2 != null)
			{
				character.DropAuthoritative(targetBuilding);
				Vector3 pos = character2.Pos;
				bool back = true;
				float facingAngle = character2.FacingAngle;
				if (!Session.Instance.IsInMultiplayerGame())
				{
					character2.GetPosFromRagdoll(out pos, out back, out facingAngle);
				}
				character2.OnRagdollStopMoving(MathUtil.ToXZ(pos), back, facingAngle);
				character2.UnityDeactivate();
				character2.SetStanding();
				targetBuilding.PlayEnterSound(character2);
				if (character2 is Chicken { IsConscious: not false } chicken)
				{
					SoundManager.PlaySound3DFromList((chicken.GetChickenModel() == ChickenModel.Chick) ? SoundManager.ChickPeepSounds : SoundManager.ChickenFleeSounds, pos);
				}
				targetBuilding.OnCharacterEnter(character2, wasOrderedInsideBuilding: true, forceEnter: false, character);
				Success = true;
			}
			break;
		}
		case AnimationEventType.Knock:
		{
			Gate gate = GetTargetObject() as Gate;
			character.Knock(gate);
			break;
		}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override void OnAnimationFinished(Character character, Goal parent)
	{
		base.OnAnimationFinished(character, parent);
		if (_interactionType == InteractionType.RepairVehicleWithItem)
		{
			Success = true;
		}
	}
}

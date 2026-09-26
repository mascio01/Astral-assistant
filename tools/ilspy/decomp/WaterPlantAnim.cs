using System;

public class WaterPlantAnim : AnimationGoal
{
	private TimeSpan _lastWaterTime;

	private bool _startedWatering;

	public bool PourOnto;

	public bool Success;

	public WaterPlantAnim()
		: base(ActionAnim.WaterCrops)
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastWaterTime);
		reflector.Add(ref _startedWatering);
		reflector.AddAfter(ref PourOnto, 468);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.WaterPlantAnim;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.EquippedItem == null)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.WaterPlantsStart:
			_startedWatering = true;
			_lastWaterTime = Session.Instance.PlayTime;
			character.PlaySoundUsingFootstepAudioSourceFromList(SoundManager.PouringSounds, 1f);
			return true;
		case AnimationEventType.WaterPlantsEnd:
			PourWater(character, end: true);
			_startedWatering = false;
			Success = true;
			if (GetTargetObject() is Campfire campfire)
			{
				campfire.PutOutFire(character);
			}
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}

	private void PourWater(Character character, bool end)
	{
		TimeSpan playTime = Session.Instance.PlayTime;
		Equipment equippedItem = character.EquippedItem;
		if (equippedItem != null && equippedItem.GetLiquidContentsType() != null)
		{
			PlantableCrop plantableCrop = GetTargetObject() as PlantableCrop;
			LiquidPrototype liquidContentsType = equippedItem.GetLiquidContentsType();
			float num = equippedItem.GetLiquidCapacity();
			if (plantableCrop != null && liquidContentsType == LiquidPrototype.Water)
			{
				num = Math.Min(num, plantableCrop.GetWaterNeededPerDayInFlOz());
			}
			else if (!PourOnto)
			{
				Prop targetProp = GetTargetProp();
				if (targetProp != null && targetProp.GetLiquidType() == liquidContentsType)
				{
					num = Math.Min(num, targetProp.GetLiquidCapacity());
				}
			}
			float num2 = (float)(playTime - _lastWaterTime).TotalSeconds * num;
			float num3 = 0f;
			if (plantableCrop != null && liquidContentsType == LiquidPrototype.Water)
			{
				num2 = Math.Min(num2, plantableCrop.GetNeededWaterInFlOz());
				if (num2 > 0f)
				{
					num3 = equippedItem.DrainLiquid(num2);
					plantableCrop.Water(character, num3);
				}
			}
			else if (PourOnto)
			{
				num3 = equippedItem.DrainLiquid(num2);
				TileObject targetObject = GetTargetObject();
				if (targetObject != null && liquidContentsType.Flammable)
				{
					targetObject.AddFuel(character, num3);
					Community community = targetObject.GetCommunity();
					if (community != null && community != character.Community && Session.Instance.CommunityManager.GetRelationship(community, character.Community) != CommunityRelationshipType.Hostile && community.IsCharacterVisibleToAnyMember(character, out var closestMember, notIncludingCaptives: true))
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(closestMember, character, SpeechSituation.StopVandal);
						closestMember.Speak(speechForSituation);
						Session.Instance.CommunityManager.SetRelationship(character.Community, community, CommunityRelationshipType.Hostile);
					}
				}
			}
			else
			{
				Prop targetProp2 = GetTargetProp();
				if (targetProp2 != null && targetProp2.GetLiquidType() == liquidContentsType)
				{
					num2 = Math.Min(num2, targetProp2.GetLiquidCapacity() - targetProp2.GetLiquidAmount());
					if (num2 > 0f)
					{
						num3 = equippedItem.DrainLiquid(num2);
						targetProp2.FillWithLiquid(num3);
					}
				}
			}
			if (num3 == 0f && num2 > 0f)
			{
				equippedItem.DrainLiquid(num2);
			}
		}
		_lastWaterTime = playTime;
	}

	public override void Update(Character character, Goal parent)
	{
		if (_startedWatering)
		{
			PourWater(character, end: false);
		}
		base.Update(character, parent);
	}
}

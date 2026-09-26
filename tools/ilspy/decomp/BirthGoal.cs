using System;

public class BirthGoal : StateMachineGoal
{
	private float LaborTime;

	private TimeSpan LastFailedTime = Target.Never;

	public override GoalType GetGoalType()
	{
		return GoalType.BirthGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!Active)
		{
			if (character.GetPregnancyProgression() >= 1f)
			{
				return Session.Instance.PlayTime - LastFailedTime > TimeSpan.FromSeconds(10.0);
			}
			return false;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_Birth;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		return AIOverridesControlReason.InLabor;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref LaborTime, 485);
		reflector.AddAfter(ref LastFailedTime, 485);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.Community == null || character.Community.IsAnyMemberInCombat())
		{
			LastFailedTime = Session.Instance.PlayTime;
			Finished = true;
			return;
		}
		character.Community.GivingBirth.Add(character);
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.WaterBroke);
		if (speechForSituation != null)
		{
			SetSubGoal(character, parent, new Conversation(character, null, null, speechForSituation, controlledByPlayer: false));
			return;
		}
		Building building = PickBuilding(character);
		if (building != null)
		{
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, MovementType.Run));
			return;
		}
		LastFailedTime = Session.Instance.PlayTime;
		Finished = true;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.Community.GivingBirth.Remove(character);
		base.OnDeactivate(character, parent);
	}

	private Building PickBuilding(Character character)
	{
		TerrainCoord tile = character.Tile;
		float num = float.MaxValue;
		Building result = null;
		foreach (Prop allProp in Session.Instance.PropManager.AllProps)
		{
			if (!(allProp is Building building) || !building.HasAnyInternalSlots() || building.Inhabitants.Length < 2)
			{
				continue;
			}
			if (building == character.InsideBuilding)
			{
				return building;
			}
			int closestEntranceTo = building.GetClosestEntranceTo(tile);
			if (!character.HasFailedFindAttempt(building, closestEntranceTo, 0, FindType.Birth, null, null, null, Sun.DayLength, out var _, out var _))
			{
				float num2 = building.GetEntranceTile(closestEntranceTo).GetDist(tile);
				if (building.Community == character.Community)
				{
					num2 -= 30f;
				}
				if (num2 < num)
				{
					result = building;
					num = num2;
				}
			}
		}
		return result;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Conversation conversation)
		{
			Building building = PickBuilding(character);
			if (conversation.Success && building != null)
			{
				return new MoveToAndEnterBuilding(character, building, MovementType.Run);
			}
			LastFailedTime = Session.Instance.PlayTime;
			return null;
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				return new Idle();
			}
			character.AddFailedFindAttempt(moveToAndEnterBuilding.GetTargetBuilding(), moveToAndEnterBuilding.EntranceIndex, -1, FindType.Birth, null, null, null);
			Building building2 = PickBuilding(character);
			if (building2 != null)
			{
				return new MoveToAndEnterBuilding(character, building2, MovementType.Run);
			}
			LastFailedTime = Session.Instance.PlayTime;
		}
		if (SubGoal is LeaveBuilding)
		{
			return new Wait(TimeSpan.FromSeconds(10.0));
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		if (SubGoal is Idle)
		{
			if (character.InsideBuilding == null)
			{
				Building building = PickBuilding(character);
				if (building != null)
				{
					SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, MovementType.Run));
				}
				else
				{
					LastFailedTime = Session.Instance.PlayTime;
				}
			}
			else
			{
				float num = (float)(Session.Instance.PlayTime - character.LastThinkTime).TotalSeconds;
				LaborTime += num;
				if (LaborTime >= Human.InLaborTime)
				{
					if (character is Human human)
					{
						human.SetPregnant(v: false);
					}
					Memory.OnMemorableEvent(MemoryPrototype.Miscarriage, null, character, 1f, secret: false);
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.Miscarriage);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, null, null, default(MemoryParam));
					}
					SetSubGoal(character, parent, new LeaveBuilding(0));
				}
			}
		}
		if (SubGoal == null)
		{
			Finished = true;
		}
		base.Update(character, parent);
	}
}

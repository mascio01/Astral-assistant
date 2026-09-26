public class FleeAndDisappearGoal : StateMachineGoal
{
	public string TriggerUniqueID;

	public Character TriggerActor;

	public Character TriggerTarget;

	public BaseObject TriggerObject;

	public MemoryParam TriggerParam;

	public bool Triggered;

	public override GoalType GetGoalType()
	{
		return GoalType.FleeAndDisappearGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.Version >= 533)
		{
			reflector.Add(ref TriggerUniqueID);
			reflector.Add(ref TriggerActor);
			reflector.Add(ref TriggerTarget);
			reflector.Add(ref TriggerObject);
			TriggerParam.Reflect(reflector);
			reflector.AddAfter(ref Triggered, 544);
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetHunger(0f);
		character.SetThirst(0f);
		character.SetSleepDeprivation(0f);
		character.SetFatigue(0f);
		float num = float.MaxValue;
		Character character2 = null;
		foreach (Character member in Session.Instance.CommunityManager.PlayerCommunity.Members)
		{
			if (member != character)
			{
				float sqrMagnitude = (member.PosXZ - character.PosXZ).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					character2 = member;
				}
			}
		}
		if (character2 != null)
		{
			SetSubGoal(character, parent, new FleeFromTarget(character, character2, MovementType.Run, 1f, 30f));
		}
		else
		{
			Disappear(character);
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		Disappear(character);
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!GameTerrain.Instance.FogOfWar.IsAnyTileInRectVisible(character.MinTile, character.MaxTile))
		{
			Disappear(character);
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Disappear(character);
		return base.GetNextSubGoal(character, parent);
	}

	private void Disappear(Character character)
	{
		if (!string.IsNullOrEmpty(TriggerUniqueID))
		{
			if (!Triggered)
			{
				StoryManager.Instance.OneShotTrigger(TriggerUniqueID, TriggerActor, TriggerTarget, TriggerObject, TriggerParam);
				Triggered = true;
			}
		}
		else if (!character.Disappeared)
		{
			character.Disappear(fromGoal: true);
		}
	}
}

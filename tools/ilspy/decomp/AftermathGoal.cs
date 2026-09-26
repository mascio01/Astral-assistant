using System;

internal class AftermathGoal : StateMachineGoal
{
	public static TimeSpan MaxTimeSinceActivity = TimeSpan.FromSeconds(5.0);

	public static TimeSpan MaxTimeSinceTalkTo = TimeSpan.FromSeconds(2.0);

	public static TimeSpan MaxTimeSinceEnteredBuilding = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.AftermathGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		switch (character.RecentActivityType)
		{
		case RecentActivityType.Combat:
		case RecentActivityType.Scared:
		case RecentActivityType.HighAlert:
		case RecentActivityType.Escape:
		case RecentActivityType.Chase:
		{
			SpeechSituation speechSituation = SpeechSituation.None;
			if (character.GetBaseObjectType() == BaseObjectType.Human)
			{
				switch (character.RecentActivityType)
				{
				case RecentActivityType.Combat:
					speechSituation = SpeechSituation.RecentKill;
					break;
				case RecentActivityType.Escape:
					speechSituation = SpeechSituation.RecentEscape;
					break;
				case RecentActivityType.Scared:
					speechSituation = SpeechSituation.RecentlyScared;
					break;
				case RecentActivityType.HighAlert:
					speechSituation = SpeechSituation.RecentHighAlert;
					break;
				case RecentActivityType.Chase:
					speechSituation = SpeechSituation.RecentChase;
					break;
				}
				if (speechSituation != SpeechSituation.None)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, character.RecentActivityWith, speechSituation);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, character.RecentActivityWith);
					}
				}
			}
			SetSubGoal(character, parent, new WaitAndFaceTarget(character, character.RecentActivityWith, TimeSpan.FromSeconds(5.0)));
			character.ClearRecentActivity();
			break;
		}
		case RecentActivityType.Conversation:
			SetSubGoal(character, parent, new WaitAndFaceTarget(character, character.RecentActivityWith, TimeSpan.FromSeconds(5.0)));
			break;
		case RecentActivityType.Doorbell:
			if (character.InsideBuilding != null)
			{
				int entranceIndex = ((character.RecentActivityWith != null) ? character.InsideBuilding.GetClosestEntranceTo(character.RecentActivityWith.Tile) : 0);
				SetSubGoal(character, parent, new LeaveBuilding(entranceIndex));
			}
			break;
		case RecentActivityType.SpeakToSelf:
			SetSubGoal(character, parent, new Wait(TimeSpan.FromSeconds(5.0)));
			break;
		case RecentActivityType.EnteredBuilding:
			SetSubGoal(character, parent, new Wait(MaxTimeSinceEnteredBuilding));
			break;
		case RecentActivityType.TalkTo:
			SetSubGoal(character, parent, new WaitAndFaceTarget(character, character.RecentActivityWith, TimeSpan.FromSeconds(2.0)));
			break;
		case RecentActivityType.Blame:
		case RecentActivityType.ThrownAt:
		case RecentActivityType.Feuding:
		case RecentActivityType.CaredFor:
			SetSubGoal(character, parent, new WaitAndFaceTarget(character, character.RecentActivityWith, TimeSpan.FromSeconds(5.0)));
			break;
		case RecentActivityType.Assassination:
			SetSubGoal(character, parent, new MoveWithinRangeAndSightOfTarget(character, character.RecentActivityWith, MovementType.Run, aiming: false, 2f, 6f, dontOpenOurGates: false, default(StayInRangeParams), 0f));
			break;
		case RecentActivityType.Interesting:
			SetSubGoal(character, parent, new WaitAndFace(GameTerrain.Instance.GetTileCoordForPos(character.RecentActivityPos), TimeSpan.FromSeconds(5.0)));
			break;
		default:
			Finished = true;
			break;
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetPlayerControllingMe() != null && character.HasBeenPlayerControlledRecently(critical: true, extraCritical: false))
		{
			return false;
		}
		if (character.RecentActivityType == RecentActivityType.Interesting && character.GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: false).Urgent)
		{
			return false;
		}
		if (!Active)
		{
			switch (character.RecentActivityType)
			{
			case RecentActivityType.Combat:
				if (character.RecentActivityWith != null && Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity && !character.RecentActivityWith.Alive && character.UnderAttackRefCount == 0)
				{
					return character.Speaking == null;
				}
				return false;
			case RecentActivityType.Scared:
			case RecentActivityType.HighAlert:
			case RecentActivityType.Escape:
			case RecentActivityType.Chase:
				if (character.RecentActivityWith != null && Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity && character.UnderAttackRefCount == 0)
				{
					return character.Speaking == null;
				}
				return false;
			case RecentActivityType.Conversation:
			case RecentActivityType.ThrownAt:
				if (character.RecentActivityWith != null)
				{
					return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity;
				}
				return false;
			case RecentActivityType.Doorbell:
				if (character.RecentActivityWith != null && Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity)
				{
					return character.InsideBuilding != null;
				}
				return false;
			case RecentActivityType.SpeakToSelf:
				return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity;
			case RecentActivityType.TalkTo:
				if (character.RecentActivityWith != null)
				{
					return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceTalkTo;
				}
				return false;
			case RecentActivityType.Blame:
			case RecentActivityType.Feuding:
			case RecentActivityType.CaredFor:
			case RecentActivityType.Assassination:
				if (character.RecentActivityWith != null)
				{
					return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity;
				}
				return false;
			case RecentActivityType.EnteredBuilding:
				if (character.RecentActivityWith != null)
				{
					return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceEnteredBuilding;
				}
				return false;
			case RecentActivityType.Interesting:
				return Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity;
			default:
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (character.GetBaseObjectType() == BaseObjectType.Human)
		{
			if (!(Session.Instance.PlayTime - character.RecentActivityTime < MaxTimeSinceTalkTo))
			{
				return GoalPriority.Survivor_Aftermath;
			}
			return GoalPriority.Survivor_Aftermath_Recent;
		}
		return GoalPriority.Animal_Aftermath;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		switch (character.RecentActivityType)
		{
		case RecentActivityType.Doorbell:
			if (SubGoal is LeaveBuilding)
			{
				Character recentActivityWith = character.RecentActivityWith;
				character.ClearRecentActivity();
				if (recentActivityWith != null && recentActivityWith != character)
				{
					return new WaitAndFaceTarget(character, recentActivityWith, TimeSpan.FromSeconds(5.0));
				}
			}
			break;
		case RecentActivityType.Assassination:
			if (SubGoal is MoveWithinRangeAndSightOfTarget || SubGoal is StopSittingGoal || SubGoal is LeaveBuilding)
			{
				if (character.IsSitting())
				{
					return new StopSittingGoal();
				}
				if (!character.IsOutdoors())
				{
					return new LeaveBuilding();
				}
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, character.RecentActivityWith, SpeechSituation.Assassinated);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation, character.RecentActivityWith);
				}
				return new WaitAndFaceTarget(character, character.RecentActivityWith, TimeSpan.FromSeconds(15.0));
			}
			break;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.RecentActivityType == RecentActivityType.TalkTo && Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceTalkTo && SubGoal is WaitAndFaceTarget waitAndFaceTarget)
		{
			waitAndFaceTarget.StartTime = Session.Instance.PlayTime;
		}
		if (character.RecentActivityType != RecentActivityType.Conversation || !(Session.Instance.PlayTime - character.RecentActivityTime <= MaxTimeSinceActivity))
		{
			return;
		}
		Character recentActivityWith = character.RecentActivityWith;
		if (recentActivityWith != null && recentActivityWith.GetLeaderCommand() is DrinkGoal)
		{
			if (SubGoal is WaitAndFaceTarget waitAndFaceTarget2)
			{
				waitAndFaceTarget2.StartTime = Session.Instance.PlayTime;
			}
			character.RecentActivityTime = Session.Instance.PlayTime;
		}
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
		Character targetCharacter = GetTargetCharacter();
		if (cont != null)
		{
			character.Speak(cont, listener, continueObject, continueParam);
			if (targetCharacter != null)
			{
				character.SetRecentActivity(RecentActivityType.Conversation, targetCharacter);
			}
		}
	}
}

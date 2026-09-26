public class TakeUIGoal : StateMachineGoal
{
	private PlayerID ControllingPlayerID;

	public bool Pickpocketing;

	public bool Caught;

	public static string[] SwappingSuppliesModeNames = StringUtil.GetEnumNames<SwappingSuppliesMode>();

	public TakeUIGoal()
	{
	}

	public TakeUIGoal(PlayerID controllingPlayerID)
	{
		ControllingPlayerID = controllingPlayerID;
	}

	public TakeUIGoal(PlayerID controllingPlayerID, bool pickpocketing)
	{
		ControllingPlayerID = controllingPlayerID;
		Pickpocketing = pickpocketing;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeUIGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		ActionAnim anim = (((GetTargetObject()?.GetTakeAnim() ?? ActionAnim.Scavenge) == ActionAnim.ScavengeCorpse) ? ActionAnim.ScavengeCorpseStart : ActionAnim.ScavengeStart);
		SetSubGoal(character, parent, new AnimationGoal(anim));
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref ControllingPlayerID);
		reflector.AddAfter(ref Pickpocketing, 624);
		reflector.AddAfter(ref Caught, 625);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		Finished = true;
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is AnimationGoal animationGoal)
		{
			ActionAnim anim = animationGoal.GetAnim();
			if (anim == ActionAnim.ScavengeStart || anim == ActionAnim.ScavengeCorpseStart)
			{
				TileObject targetObject = GetTargetObject();
				if (targetObject != null)
				{
					PlayerRecord playerRecord = Session.Instance.GetPlayerControllingCharacter(character);
					if (playerRecord == null)
					{
						playerRecord = ((!ControllingPlayerID.IsValid()) ? Session.Instance.GetPartyLeaderRecord() : Session.Instance.GetPlayerRecord(ControllingPlayerID));
					}
					else
					{
						ControllingPlayerID = playerRecord.PlayerID;
					}
					targetObject.MarkInvestigated(character);
					StartSwappingSupplies(playerRecord, character, targetObject, Pickpocketing ? SwappingSuppliesMode.Pickpocketing : SwappingSuppliesMode.Stealing);
				}
				return new AnimationGoal((animationGoal.GetAnim() == ActionAnim.ScavengeCorpseStart) ? ActionAnim.ScavengeCorpseLoop : ActionAnim.ScavengeLoop);
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public static void StartSwappingSupplies(PlayerRecord playerRecord, Character character, TileObject targetObject, SwappingSuppliesMode mode)
	{
		if (playerRecord == null || playerRecord.PlayerCharacter != character || playerRecord.SyncedIsInInfoScreen || playerRecord.SyncedSwappingSuppliesWith != null)
		{
			return;
		}
		Character character2 = targetObject as Character;
		if (character2 != null && character2.IsControllableByPlayer() && mode == SwappingSuppliesMode.Trading)
		{
			return;
		}
		playerRecord.SyncedSwappingSupplies = character;
		playerRecord.SyncedSwappingSuppliesWith = targetObject;
		playerRecord.SyncedSwappingSuppliesMode = mode;
		_ = 2;
		InfoScreen instance = InfoScreen.Instance;
		if (playerRecord.IsLocal)
		{
			instance.SentScavengingFinished = false;
			if (!instance.Active)
			{
				if (mode == SwappingSuppliesMode.Trading)
				{
					instance.ActivateTrade(targetObject as Character, character, mode);
				}
				else
				{
					instance.ActivateTake(targetObject, character, mode);
				}
			}
		}
		if (character2 != null && character2.IsControllableByPlayer())
		{
			PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(character2);
			if (playerControllingCharacter != null && playerControllingCharacter.IsLocal)
			{
				instance.SentScavengingFinished = false;
				if (!instance.Active)
				{
					instance.ActivateTake(character, character2, mode);
				}
			}
		}
		character.RemoveFailedFindAttempt(targetObject);
		targetObject.MarkInvestigated(character);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!(SubGoal is AnimationGoal animationGoal))
		{
			return;
		}
		ActionAnim anim = animationGoal.GetAnim();
		if (anim != ActionAnim.ScavengeLoop && anim != ActionAnim.ScavengeCorpseLoop)
		{
			return;
		}
		if (Pickpocketing && !Caught)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && character.OnStoleSomething(targetCharacter.Community, targetCharacter, 0f, seenByThiefCommunity: false))
			{
				Caught = true;
			}
		}
		PlayerRecord playerRecord = Session.Instance.GetPlayerRecord(ControllingPlayerID);
		if (playerRecord == null || playerRecord.SyncedSwappingSuppliesWith == null)
		{
			SetSubGoal(character, parent, new AnimationGoal((animationGoal.GetAnim() == ActionAnim.ScavengeCorpseLoop) ? ActionAnim.ScavengeCorpseFinish : ActionAnim.ScavengeFinish));
		}
	}

	public override bool IsSubstantiallyFinished(Character character)
	{
		if (SubGoal is AnimationGoal animationGoal && (animationGoal.GetAnim() == ActionAnim.ScavengeCorpseFinish || animationGoal.GetAnim() == ActionAnim.ScavengeFinish))
		{
			return true;
		}
		return base.IsSubstantiallyFinished(character);
	}
}

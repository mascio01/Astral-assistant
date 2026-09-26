using System;

public class TargetedGoal : FaceTarget
{
	private TimeSpan StartTime;

	private SpeechAnimState SpeakingState;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("TargetedCalcBestTarget");

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartTime);
		reflector.Add(ref SpeakingState);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TargetedGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_Targeted;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && SpeakingState == SpeechAnimState.Started)
		{
			return true;
		}
		return !IsTargetDeleted();
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (!character.IsOutdoors())
			{
				return null;
			}
			Session instance = Session.Instance;
			Target result = null;
			float num = 1E+38f;
			foreach (PlayerRecord playerRecord in instance.PlayerRecords)
			{
				if (playerRecord.FlyMode || !playerRecord.WantLockOnTarget || playerRecord.TargetObject != character || playerRecord.PlayerMode != PlayerMode.Controlling || playerRecord.PlayerCharacter == null || playerRecord.PlayerCharacter.Community == null || playerRecord.PlayerCharacter.SparringPartner == character || !(playerRecord.PlayerCharacter.EquippedItem is RangedWeapon) || instance.CommunityManager.GetRelationship(playerRecord.PlayerCharacter.Community, character.Community) == CommunityRelationshipType.Hostile)
				{
					continue;
				}
				Target target = character.GetTarget(playerRecord.PlayerCharacter);
				if (target != null && target.Camouflage == 0f)
				{
					float num2 = MathUtil.ToXZ(target.LastKnownPosition - character.Pos).magnitude;
					if (target == Target && Active)
					{
						num2 *= 0.75f;
					}
					if (num2 < num)
					{
						result = target;
						num = num2;
					}
				}
			}
			return result;
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		SpeakingState = SpeechAnimState.NotStarted;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		TimeSpan timeSpan = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime;
		if (SpeakingState != SpeechAnimState.NotStarted || !(timeSpan >= TimeSpan.FromSeconds(1.0)) || targetCharacter.EquippedItem == null || !(targetCharacter.EquippedItem.GetPrototype().Damage > 0f))
		{
			return;
		}
		if (character.IsAuthoritative())
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Targeted);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter);
			}
		}
		SpeakingState = SpeechAnimState.Started;
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
		if (SpeakingState == SpeechAnimState.Started && speech.Situation == SpeechSituation.Targeted)
		{
			SpeakingState = SpeechAnimState.Finished;
		}
		if (SpeakingState != SpeechAnimState.Finished)
		{
			return;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsAuthoritative())
		{
			PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(targetCharacter);
			if (playerControllingCharacter != null && playerControllingCharacter.WantLockOnTarget && playerControllingCharacter.TargetObject == character && character.Community != null && targetCharacter.Community != null)
			{
				Memory.OnMemorableEvent(MemoryPrototype.DeclaredWar, targetCharacter, character, 1f, secret: false);
				Session.Instance.CommunityManager.SetRelationship(targetCharacter.Community, character.Community, CommunityRelationshipType.Hostile);
			}
		}
		Finished = true;
	}
}

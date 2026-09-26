using System;
using UnityEngine;

public class WatchFisticuffs : StateMachineGoal
{
	private TimeSpan NextCheerTime;

	private TimeSpan LastFailedTime;

	private static TimeSpan TimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("WatchFisticuffsCalcBestTarget");

	private static float MinDist = 3f;

	private static float MaxDist = 9f;

	private static float MinTimeBetweenCheers = 4f;

	private static float MaxTimeBetweenCheers = 16f;

	public override GoalType GetGoalType()
	{
		return GoalType.WatchFisticuffs;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_WatchFisticuffs;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref NextCheerTime);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (character.DirectControlledMajorAIDisabled)
		{
			return false;
		}
		if (character.IsCrouching())
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastFailedTime <= TimeBetweenAttempts)
		{
			return false;
		}
		if (character.GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: false).Urgent)
		{
			return false;
		}
		return true;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (character.SparringPartner != null)
			{
				return null;
			}
			if (!character.IsOutdoors())
			{
				return null;
			}
			if (character.GetRank() == Rank.Captive)
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object != null && !target.Object.Deleted && target.Object.GetAuthoritativeOrElseThis() is Character { SparringPartner: not null, AliveAndNotZombie: not false } && !(target.TimeSinceLastDetected >= TimeSpan.FromSeconds(60.0)))
				{
					float num2 = character.Get2DDistToTargetLastKnownPos(target) / Character.WalkSpeed;
					float num3 = (float)target.TimeSinceLastDetected.TotalSeconds + num2;
					if (target == Target && Active)
					{
						num3 *= 0.75f;
					}
					if (num3 < num)
					{
						result = target;
						num = num3;
					}
				}
			}
			return result;
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DisableFistsAiming = true;
		SetupNextCheerTime(character);
		if (character.EquippedItem == null)
		{
			SetSubGoal(character, parent, new FaceTarget());
		}
		else
		{
			SetSubGoal(character, parent, new Equip(null));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.DisableFistsAiming = false;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return;
		}
		float magnitude = MathUtil.ToXZ(character.Pos - targetCharacter.Pos).magnitude;
		if (!(SubGoal is FaceTarget))
		{
			return;
		}
		if (magnitude <= MinDist || magnitude >= MaxDist)
		{
			SetSubGoal(character, parent, new MoveWithinRangeAndSightOfTarget(MovementType.Jog, aiming: true, MinDist, MaxDist, dontOpenOurGates: true, default(StayInRangeParams), 0f));
		}
		else if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) >= NextCheerTime)
		{
			character.CalcApprovalRating(targetCharacter, out var approval, out var _);
			character.CalcApprovalRating(targetCharacter.SparringPartner, out var approval2, out var _);
			if (targetCharacter.Community == character.Community)
			{
				approval += 10f;
			}
			if (targetCharacter.SparringPartner.Community == character.Community)
			{
				approval2 += 10f;
			}
			SpeechSituation sit = ((approval >= approval2) ? SpeechSituation.CheerFisticuffs : SpeechSituation.BooFisticuffs);
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, targetCharacter.SparringPartner, sit);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter, targetCharacter.SparringPartner);
			}
			SetupNextCheerTime(character);
		}
	}

	private void SetupNextCheerTime(Character character)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		NextCheerTime = currentTime + TimeSpan.FromSeconds(Mathf.Lerp(MinTimeBetweenCheers, MaxTimeBetweenCheers, MathUtil.RandomFloat((float)(character.Id * 100) + (float)currentTime.Seconds)));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinRangeAndSightOfTarget { Success: false })
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter == null)
			{
				return null;
			}
			if (MathUtil.ToXZ(character.Pos - targetCharacter.Pos).magnitude >= MaxDist)
			{
				LastFailedTime = Session.Instance.PlayTime;
				return null;
			}
		}
		return new FaceTarget();
	}
}

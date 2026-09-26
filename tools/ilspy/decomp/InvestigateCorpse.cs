using System;
using UnityEngine;

public class InvestigateCorpse : StateMachineGoal
{
	private static GameProfiler CalcBestTargetTimer = new GameProfiler("InvestigateCorpseCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.InvestigateCorpse;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_InvestigateCorpse;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.AlertIcon;
	}

	public override bool IsLowAlert(Character character)
	{
		return true;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		if (Target.InvestigateCorpseCount == 0)
		{
			if (character.IsGuarding())
			{
				SetSubGoal(character, parent, GetShoutGoal(character));
			}
			else
			{
				SetSubGoal(character, parent, new MoveAsCloseAsPossibleToTarget(MovementType.Run, AlertGoal.CalcDontOpenOurGates(character)));
			}
		}
		else
		{
			SetSubGoal(character, parent, GetNextInvestigateGoal(character, parent));
		}
	}

	private Goal GetShoutGoal(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (character.IsAuthoritative() && targetCharacter != null)
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.SeeCorpse);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter);
			}
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.FoundBody, targetCharacter.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, character, targetCharacter, targetCharacter));
		}
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(2f, 4f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).TotalSeconds + (float)character.Id))));
	}

	private bool IsUrgent(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.GetCommunity() == character.GetCommunity() && character.GetCommunity() != null && (Target.GetFlag(TargetFlags.KnockedOut) || (Target.GetFlag(TargetFlags.Dead) && Session.Instance.PlayTime - targetCharacter.TimeOfDeath < TimeSpan.FromSeconds(60.0))))
		{
			return true;
		}
		return false;
	}

	private Goal GetNextInvestigateGoal(Character character, Goal parent)
	{
		GameTerrain instance = GameTerrain.Instance;
		Vector2 vector = MathUtil.ToXZ(Target.LastKnownPosition);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(vector + MathUtil.RandomVec2InCircle((float)currentTime.TotalSeconds + (float)character.Id) * Math.Min(5, Target.InvestigateCorpseCount) * 8f);
		tileCoordForPosXZ = GameTerrain.Instance.ClampTileWithinBounds(tileCoordForPosXZ);
		if (character.IsGuarding() || character.HasPersonality(CachedPersonalityType.Nervous))
		{
			return new WaitAndFace(tileCoordForPosXZ, TimeSpan.FromSeconds(Mathf.Lerp(2f, 8f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
		}
		return new MoveAsCloseAsPossibleTo((!IsUrgent(character)) ? MovementType.Walk : MovementType.Run, tileCoordForPosXZ, float.MaxValue, AlertGoal.CalcDontOpenOurGates(character));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return null;
		}
		if (SubGoal is MoveAsCloseAsPossibleToTarget)
		{
			return GetShoutGoal(character);
		}
		Target.InvestigateCorpseCount++;
		if (Target.InvestigateCorpseCount >= (IsUrgent(character) ? 15 : 5))
		{
			character.SetRecentActivity(RecentActivityType.HighAlert, null);
			Target.SetFlag(TargetFlags.HaveInvestigatedBody, on: true);
			if (character.IsAuthoritative() && character.Community != null && !IsTargetDeleted())
			{
				foreach (Character member in character.Community.Members)
				{
					if (member.AliveAndNotZombie && !member.IsLowAlert() && Target.Object != member)
					{
						Target orCreateTarget = member.GetOrCreateTarget(Target.Object);
						orCreateTarget.SetFlag(TargetFlags.HaveInvestigatedBody, on: true);
						orCreateTarget.UpdateLastKnownInfo(member);
					}
				}
			}
			return null;
		}
		return GetNextInvestigateGoal(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.IsControllableByPlayer())
		{
			return false;
		}
		if (Active)
		{
			return true;
		}
		return !IsTargetDeleted();
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (character.IsControllableByPlayer() || character.Rank == Rank.Captive)
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object != null && !target.Object.Deleted && ((target == Target && Active) || target.GetFlag((TargetFlags)80) || target.GetFlag((TargetFlags)48)) && !target.GetFlag(TargetFlags.HaveInvestigatedBody) && !target.GetFlag(TargetFlags.HaveAssignedBlameForAttack))
				{
					float num2 = character.Get2DDistToTargetLastKnownPos(target) / Character.WalkSpeed;
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
}

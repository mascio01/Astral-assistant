using System;
using UnityEngine;

public class MedicGoal : RoleGoal
{
	private TimeSpan LastAttemptedTime = TimeSpan.FromDays(-365.0);

	public Character CurrentPersonToHeal;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.MedicGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorBandages;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MovementType);
		reflector.Add(ref LastAttemptedTime);
		reflector.AddAfter(ref CurrentPersonToHeal, 612);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active)
		{
			MoveToAndInteractGoal moveToAndInteractGoal = SubGoal as MoveToAndInteractGoal;
			MoveToAndDeposit moveToAndDeposit = SubGoal as MoveToAndDeposit;
			if ((moveToAndInteractGoal != null && moveToAndInteractGoal.SubGoal is AnimationGoal) || (moveToAndDeposit != null && moveToAndDeposit.SubGoal is AnimationGoal))
			{
				return GoalPriority.Survivor_Role_Animation;
			}
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Medic));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Medic))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Medic))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.Medic), inProgress: true);
		Goal firstSubGoal = GetFirstSubGoal(character, parent);
		if (firstSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, firstSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal)
		{
			if (!moveToAndInteractGoal.Success)
			{
				OnFailed(character);
				return null;
			}
			character.OnRoleSucceeded();
		}
		return GetFirstSubGoal(character, parent);
	}

	private Goal GetFirstSubGoal(Character character, Goal parent)
	{
		_ = CurrentPersonToHeal;
		CurrentPersonToHeal = GetBestPersonToHeal(character);
		if (CurrentPersonToHeal != null)
		{
			if (SubGoal is FindGoal { Success: false })
			{
				OnFailed(character);
				return null;
			}
			int injuryBandageLevel = CurrentPersonToHeal.GetInjuryBandageLevel();
			if (injuryBandageLevel == int.MaxValue)
			{
				OnFailed(character);
				return null;
			}
			int num = injuryBandageLevel + 1;
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel(num);
			if (equipment != null)
			{
				return new MoveToAndInteractGoal(character, CurrentPersonToHeal, InteractionType.ApplyBandageAndSpeak, equipment, MovementType)
				{
					DontOpenOurGates = FindGoal.CalcDontOpenOurGates(character)
				};
			}
			return new FindGoal((num <= 0) ? FindType.Bandage : FindType.GoodBandage, MovementType, num == 0);
		}
		OnFailed(character);
		return null;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (GetBestPersonToHeal(character) != CurrentPersonToHeal)
		{
			Goal firstSubGoal = GetFirstSubGoal(character, parent);
			if (firstSubGoal != null)
			{
				SetSubGoal(character, parent, firstSubGoal);
			}
			else
			{
				Finished = true;
			}
		}
	}

	private Character GetBestPersonToHeal(Character character)
	{
		int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Medicine);
		if (CurrentPersonToHeal != null && CurrentPersonToHeal.AliveAndNotZombie)
		{
			if (CurrentPersonToHeal.GetInjuryBandageLevel() < skillLevelWithEffects && (!character.HasMovementZone() || character.MovementZone.Contains(CurrentPersonToHeal.Tile)))
			{
				return CurrentPersonToHeal;
			}
			if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal && moveToAndInteractGoal.SubGoal is AnimationGoal)
			{
				return CurrentPersonToHeal;
			}
		}
		int val = character.Community.FindHighestBandageLevel(includeDead: true);
		skillLevelWithEffects = Math.Min(skillLevelWithEffects, val);
		if (skillLevelWithEffects == -1)
		{
			return null;
		}
		float num = float.MaxValue;
		Character result = null;
		foreach (Character member in character.Community.Members)
		{
			if (member != character && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.HasUnbandagedInjury(skillLevelWithEffects) && (!character.HasMovementZone() || character.MovementZone.Contains(member.Tile)))
			{
				float num2 = (float)member.GetInjuryBandageLevel() * 1000f + MathUtil.ToXZ(member.Position - character.Position).magnitude;
				if (num2 < num && (!member.IsConscious || (Math.Min(member.GetSkillLevelWithEffects(SkillType.Medicine), member.Inventory.FindHighestBandageLevel()) <= skillLevelWithEffects && member.FindActiveGoal(GoalType.BandageSelfGoal) == null)) && !character.Community.IsAnyMemberHealing(member))
				{
					num = num2;
					result = member;
				}
			}
		}
		return result;
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Medic));
		LastAttemptedTime = Session.Instance.PlayTime;
	}

	public void ResetLastAttemptedTime()
	{
		LastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Medic);
	}
}

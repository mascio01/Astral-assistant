using System;
using UnityEngine;

public class BandageSelfGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is BandageSelfAnim)
		{
			return null;
		}
		return GameCursor.CursorBandages;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BandageSelfGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && !character.DirectControlled && SubGoal is BandageSelfAnim)
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered())
		{
			return false;
		}
		if (character.Inventory.FindItemWithHighestBandageLevel() != null)
		{
			return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
		}
		if (character.HasBeenPlayerControlledRecently(critical: true, extraCritical: true))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - _lastSearchedTime < (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && SubGoal is BandageSelfAnim)
		{
			return GoalPriority.Survivor_BandageOrMedicate_Animation;
		}
		if (!character.HasUnbandagedInjury(0))
		{
			if (character.HasRunningRole(Role.Medic) && character.HasUnbandagedInjury(character.GetSkillLevelWithEffects(SkillType.Medicine)))
			{
				return GoalPriority.Survivor_Bandage_NotCritical;
			}
			return GoalPriority.Impossible;
		}
		if (character.Inventory.FindItemWithHighestBandageLevel() != null)
		{
			return GoalPriority.Survivor_Bandage_Pocket;
		}
		if (!character.IsControllableByPlayer() && !character.CanFollowPlayer)
		{
			return GoalPriority.Survivor_AI_Bandage_Critical;
		}
		return GoalPriority.Survivor_Player_Bandage_Critical;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		int num = character.GetInjuryBandageLevel() + 1;
		Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel(num);
		if (equipment != null)
		{
			SetSubGoal(character, parent, new BandageSelfAnim(equipment));
			return;
		}
		SetSubGoal(character, parent, new FindGoal((num > 0) ? FindType.GoodBandage : FindType.Bandage, MovementType.Run, critical: true));
		if (SubGoal.Finished && !((FindGoal)SubGoal).Success)
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FindGoal { Success: not false })
		{
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel();
			if (equipment != null && character.HasUnbandagedInjury(Math.Min(character.GetSkillLevelWithEffects(SkillType.Medicine), equipment.GetBandageLevel())))
			{
				return new BandageSelfAnim(equipment);
			}
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is BandageSelfAnim)
		{
			_lastSearchFailed = false;
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}

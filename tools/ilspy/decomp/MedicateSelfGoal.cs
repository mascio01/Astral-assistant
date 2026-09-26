using System;
using UnityEngine;

public class MedicateSelfGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MedicateSelfGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is MedicateSelfAnim)
		{
			return null;
		}
		return GameCursor.CursorMedication;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && !character.DirectControlled && SubGoal is MedicateSelfAnim)
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered())
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < character.Injuries.Count; i++)
		{
			InfectionType infectionType = character.Injuries[i].InfectionType;
			if (infectionType != InfectionType.None)
			{
				if (character.Inventory.GetAntigen(infectionType) != null)
				{
					return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
				}
				flag = flag || infectionType == InfectionType.Green;
				flag2 = flag2 || infectionType == InfectionType.Blue;
				flag3 = flag3 || infectionType == InfectionType.Red;
			}
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
		if (Active && SubGoal is MedicateSelfAnim)
		{
			return GoalPriority.Survivor_BandageOrMedicate_Animation;
		}
		if (character.GetInfectionProgression() <= 0f)
		{
			return GoalPriority.Impossible;
		}
		if (character.Infection == InfectionType.White)
		{
			return GoalPriority.Impossible;
		}
		float num = 0f;
		int constitution = -1;
		for (int i = 0; i < character.Injuries.Count; i++)
		{
			InfectionType infectionType = character.Injuries[i].InfectionType;
			if (infectionType != InfectionType.None && character.Inventory.GetAntigen(infectionType) != null)
			{
				return GoalPriority.Survivor_Antigen_Pocket;
			}
			num = Math.Max(num, character.Injuries[i].GetInfectionProgressionRate(character, ref constitution));
		}
		if (character.GetInfectionProgression() + num * 60f >= 1f)
		{
			if (!character.IsControllableByOrFollowingPlayer())
			{
				return GoalPriority.Survivor_AI_Antigen_Critical;
			}
			return GoalPriority.Survivor_Player_Antigen_Critical;
		}
		return GoalPriority.Survivor_Antigen;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		InfectionType worstInfectionTypeInProgression = character.GetWorstInfectionTypeInProgression();
		Equipment antigen = character.Inventory.GetAntigen(worstInfectionTypeInProgression);
		if (antigen != null)
		{
			SetSubGoal(character, parent, new MedicateSelfAnim(antigen));
			return;
		}
		bool flag = worstInfectionTypeInProgression != InfectionType.Green || character.InfectionProgression >= 0.5f;
		FindGoal findGoal = new FindGoal(FindType.Antigen, (!flag) ? MovementType.Walk : MovementType.Run, flag);
		findGoal.CanTravelFar = !character.IsControllableByPlayer();
		SetSubGoal(character, parent, findGoal);
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
			for (int i = 0; i < character.Injuries.Count; i++)
			{
				InfectionType infectionType = character.Injuries[i].InfectionType;
				if (infectionType > InfectionType.None)
				{
					Equipment antigen = character.Inventory.GetAntigen(infectionType);
					if (antigen != null)
					{
						return new MedicateSelfAnim(antigen);
					}
				}
			}
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is MedicateSelfAnim)
		{
			_lastSearchFailed = false;
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}

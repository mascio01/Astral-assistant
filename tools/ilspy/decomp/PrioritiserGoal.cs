using System.Collections.Generic;

public abstract class PrioritiserGoal : StateMachineGoal
{
	private class SortGoalsByPriorityAscending : IComparer<Goal>
	{
		int IComparer<Goal>.Compare(Goal a, Goal b)
		{
			if (a.Priority > b.Priority)
			{
				return 1;
			}
			if (a.Priority < b.Priority)
			{
				return -1;
			}
			return 0;
		}
	}

	public List<Goal> SubGoals = new List<Goal>();

	private static SortGoalsByPriorityAscending GoalSorter = new SortGoalsByPriorityAscending();

	public PrioritiserGoal()
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.IsDoingPrediction)
		{
			return;
		}
		int value = SubGoals.Count;
		reflector.Add(ref value);
		for (int i = 0; i < value; i++)
		{
			Goal goal = null;
			if (reflector.IsSerialising)
			{
				goal = ((SubGoals[i] == SubGoal) ? null : SubGoals[i]);
			}
			reflector.Add(ref goal, character);
			if (reflector.IsDeserialising)
			{
				SubGoals.Add((goal != null) ? goal : SubGoal);
			}
		}
	}

	public void AddSubGoal(Goal subGoal)
	{
		SubGoals.Add(subGoal);
	}

	protected Goal GetBestSubGoal(Character character, Goal parent)
	{
		foreach (Goal subGoal in SubGoals)
		{
			subGoal.Priority = (subGoal.IsPossible(character, this) ? subGoal.CalcPriority(character, this) : GoalPriority.Impossible);
		}
		SubGoals.InsertionSort(GoalSorter);
		Goal goal = SubGoals[SubGoals.Count - 1];
		if (goal.Priority == GoalPriority.Impossible)
		{
			goal = null;
		}
		return goal;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (character.IsPredicted())
		{
			return null;
		}
		foreach (Goal subGoal in SubGoals)
		{
			subGoal.SetTarget(character, this, subGoal.CalcBestTarget(character, this));
		}
		return GetBestSubGoal(character, parent);
	}

	public Goal FindSubGoalByType(GoalType type)
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal.GetGoalType() == type)
			{
				return subGoal;
			}
		}
		return null;
	}

	public override void PreUpdate(Character character, Goal parent)
	{
		foreach (Goal subGoal in SubGoals)
		{
			subGoal.SetTarget(character, this, subGoal.CalcBestTarget(character, this));
		}
	}

	public override void Update(Character character, Goal parent)
	{
		CheckForImpossible(character, parent);
		if (!Finished && !character.IsPredicted())
		{
			SetSubGoal(character, parent, GetBestSubGoal(character, parent), hasCalculatedTargetAlready: true);
		}
		CheckForSubGoalFinished(character, parent, hasCalculatedTargetAlready: true);
	}

	public override void PostUpdate(Character character, Goal parent)
	{
		foreach (Goal subGoal in SubGoals)
		{
			subGoal.ClearTargetIfInactive(character, parent);
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		foreach (Goal subGoal in SubGoals)
		{
			subGoal.ClearTargetIfInactive(character, parent);
			subGoal.OnPrioritiserDeactivated(character, parent);
		}
		SubGoals.Clear();
		base.OnDeactivate(character, parent);
	}

	public override Goal GetLeaderCommand()
	{
		if (!(SubGoal is ObeyLeaderGoal obeyLeaderGoal))
		{
			return null;
		}
		return obeyLeaderGoal.GetCommand();
	}

	public override Goal GetLeaderCommandEvenIfItIsInactive()
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal.GetGoalType() == GoalType.ObeyLeaderGoal)
			{
				return (subGoal as ObeyLeaderGoal).GetCommand();
			}
		}
		return null;
	}

	public override bool IsLeaderCommandFinished(Character character)
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal is ObeyLeaderGoal obeyLeaderGoal)
			{
				return obeyLeaderGoal.GetCommand() == null || obeyLeaderGoal.GetCommand().CalcPriority(character, obeyLeaderGoal) == GoalPriority.Impossible;
			}
		}
		return true;
	}

	public override bool WasLeaderCommandSuccessful()
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal is ObeyLeaderGoal obeyLeaderGoal)
			{
				return obeyLeaderGoal.WasCommandSuccessful;
			}
		}
		return false;
	}

	public override FollowGoal GetFollowGoal()
	{
		foreach (Goal subGoal in SubGoals)
		{
			if (subGoal.GetGoalType() == GoalType.FollowGoal)
			{
				return subGoal as FollowGoal;
			}
		}
		return null;
	}
}

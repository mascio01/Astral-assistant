using System;
using UnityEngine;

internal class Wander : StateMachineGoal
{
	private static float MinTimeBetweenWanders = 1f;

	private static float MaxTimeBetweenWanders = 5f;

	public override GoalType GetGoalType()
	{
		return GoalType.Wander;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Zombie_Wander;
	}

	public override bool IsBored(Character character)
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, GetNextSubGoal(character, parent));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (character.IsSitting())
		{
			return new StopSittingGoal();
		}
		bool flag = false;
		foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
		{
			if (playerRecord.IsPlayerControllable() && playerRecord.PlayerCharacter.IsSneakingUpOn(character))
			{
				flag = true;
				break;
			}
		}
		if (SubGoal is Wait && !flag)
		{
			return new MoveToRandomTile();
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(MinTimeBetweenWanders, MaxTimeBetweenWanders, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
	}
}

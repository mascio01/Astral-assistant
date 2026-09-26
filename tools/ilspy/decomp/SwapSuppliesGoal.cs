using UnityEngine;

internal class SwapSuppliesGoal : FaceTarget
{
	private static GameProfiler CalcBestTargetTimer = new GameProfiler("SwapSuppliesCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.SwapSuppliesGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorTake;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_SwapSupplies;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.SyncedSwappingSupplies == character)
				{
					return character.GetTarget(playerRecord.SyncedSwappingSuppliesWith);
				}
				if (playerRecord.SyncedSwappingSuppliesWith == character && playerRecord.SyncedSwappingSuppliesMode != SwappingSuppliesMode.Pickpocketing)
				{
					return character.GetTarget(playerRecord.SyncedSwappingSupplies);
				}
			}
			return null;
		}
	}
}

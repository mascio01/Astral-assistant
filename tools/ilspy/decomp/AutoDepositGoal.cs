using System;
using UnityEngine;

public class AutoDepositGoal : StateMachineGoal
{
	private static TimeSpan RememberFailedAttemptTime = TimeSpan.FromSeconds(60.0);

	private static float CostOfFailure = 10f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("AutoDepositCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.AutoDepositGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorTake;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (character.HasBeenPlayerControlledRecently(critical: true, extraCritical: true))
		{
			return false;
		}
		if (!Active)
		{
			if (character.DirectControlledCrouching)
			{
				return false;
			}
			if (character.CarryingObject != null)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_AutoDeposit;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (!character.IsControllableByPlayer())
			{
				return null;
			}
			if (Active && character.CurrentActionAnim == ActionAnim.Scavenge)
			{
				return Target;
			}
			Equipment bestItem;
			int bestAmountToDeposit;
			Prop bestPropToDepositIn = GetBestPropToDepositIn(character, Active ? GetTargetProp() : null, out bestItem, out bestAmountToDeposit);
			if (bestPropToDepositIn != null)
			{
				return character.GetOrCreateTarget(bestPropToDepositIn);
			}
			return null;
		}
	}

	private Prop GetBestPropToDepositIn(Character character, Prop prefer, out Equipment bestItem, out int bestAmountToDeposit)
	{
		character.GetGatherGoal();
		bool flag = character.HasRunningRoleWhichWillDepositGatheredItems();
		float num = 48f;
		Prop result = null;
		bestItem = null;
		bestAmountToDeposit = 0;
		foreach (Equipment content in character.Inventory.Contents)
		{
			if (!character.IsActionAllowedForItem(content, EquipmentPolicyAction.AutoDeposit) || !FindGoal.CanTransferFromCharacter(character, content, FindType.AutoDeposit, out var amountToTransfer) || (flag && content.GetGatheredAmount() > 0))
			{
				continue;
			}
			Prop buildingToStoreSuppliesIn = GatherGoal.GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(content));
			if (buildingToStoreSuppliesIn == null || (character.HasMovementZone() && !character.MovementZone.Overlaps(buildingToStoreSuppliesIn.GetTileRect())))
			{
				continue;
			}
			if (buildingToStoreSuppliesIn == prefer)
			{
				bestItem = content;
				bestAmountToDeposit = amountToTransfer;
				return prefer;
			}
			float num2 = (buildingToStoreSuppliesIn.PosXZ - character.PosXZ).magnitude;
			if (!(num2 >= num))
			{
				if (character.HasFailedFindAttempt(buildingToStoreSuppliesIn, 0, 0, FindType.AutoDeposit, null, null, null, RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
				{
					num2 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
				}
				if (!(num2 >= num))
				{
					result = buildingToStoreSuppliesIn;
					bestItem = content;
					bestAmountToDeposit = amountToTransfer;
					num = num2;
				}
			}
		}
		return result;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Equipment bestItem;
		int bestAmountToDeposit;
		Prop bestPropToDepositIn = GetBestPropToDepositIn(character, null, out bestItem, out bestAmountToDeposit);
		if (bestPropToDepositIn != null)
		{
			SetSubGoal(character, parent, new MoveToAndDeposit(character, bestPropToDepositIn, bestItem, bestAmountToDeposit, MovementType.Walk));
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: false })
		{
			character.AddFailedFindAttempt(Target.Object, 0, 0, FindType.AutoDeposit, null, null, null);
		}
		return base.GetNextSubGoal(character, parent);
	}
}

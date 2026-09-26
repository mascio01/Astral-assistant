using System;
using UnityEngine;

public class AutoCollectGoal : StateMachineGoal
{
	private static TimeSpan RememberFailedAttemptTime = TimeSpan.FromSeconds(60.0);

	private static float CostOfFailure = 10f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("AutoCollectCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.AutoCollectGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorTake;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			if (!Active)
			{
				return false;
			}
			if (character.CurrentActionAnim != ActionAnim.Scavenge && character.CurrentActionAnim != ActionAnim.ScavengeCorpse)
			{
				return false;
			}
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
		return GoalPriority.Survivor_AutoCollect;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (character.Community == null)
			{
				return null;
			}
			if (character.EquipmentPolicies.Count == 0 && character.Community.EquipmentPolicies.Count == 0)
			{
				return null;
			}
			if (Active && (character.CurrentActionAnim == ActionAnim.Scavenge || character.CurrentActionAnim == ActionAnim.ScavengeCorpse || GetAutoCollectableItem(character, Target) != null || (Target != null && Target.Object != null && Target.Object.GetGrabbableEquipmentType() != null)))
			{
				return Target;
			}
			Target result = null;
			float num = 48f;
			foreach (Target target in character.Targets)
			{
				if (target.Object == null || target.Object.Deleted || !target.FullyTracked)
				{
					continue;
				}
				if (!target.HasAnyFlag((TargetFlags)8390660))
				{
					if (target.Object is Character && character.IsEnemy(target.Object))
					{
						return null;
					}
				}
				else
				{
					if (character.HasMovementZone() && !character.MovementZone.Contains(target.Object.GetTile()))
					{
						continue;
					}
					float num2 = character.Get2DDistToTargetLastKnownPos(target);
					if (target == Target && Active)
					{
						num2 *= 0.75f;
					}
					if (num2 >= num || target.Object.GetInventory() == null || GetAutoCollectableItem(character, target) == null)
					{
						continue;
					}
					Community community = target.Object.GetCommunity();
					if (community == null || !community.HasAnyLivingNonZombieMembers() || community.GetRelationship(character.Community) == CommunityRelationshipType.Hostile)
					{
						if (character.HasFailedFindAttempt(target.Object, 0, 0, FindType.AutoCollect, null, null, null, RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
						{
							num2 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
						}
						if (!(num2 >= num) && !character.Community.IsAnyMemberCollectingSomethingFrom(target.Object, character))
						{
							result = target;
							num = num2;
						}
					}
				}
			}
			ArrowProp arrowProp = FindNearbyArrowProp(character, num);
			if (arrowProp != null)
			{
				result = character.GetOrCreateTarget(arrowProp);
			}
			return result;
		}
	}

	private Equipment GetAutoCollectableItem(Character character, Target target)
	{
		EquipmentContainer equipmentContainer = ((target != null && target.Object != null) ? target.Object.GetInventory() : null);
		if (equipmentContainer == null)
		{
			return null;
		}
		foreach (Equipment content in equipmentContainer.Contents)
		{
			if (character.IsActionAllowedForItem(content, EquipmentPolicyAction.AutoCollect) && !character.Community.HasReachedCraftingLimitForItem(content) && character.HasInventorySpaceFor(content.GetWeight()))
			{
				return content;
			}
		}
		return null;
	}

	private ArrowProp FindNearbyArrowProp(Character character, float bestTargetPriority)
	{
		float inventorySpace = character.GetAvailableInventorySpace();
		ArrowProp best = null;
		if (character.IsActionAllowedForType(BaseObjectType.Arrow, EquipmentPolicyAction.AutoCollect))
		{
			GameTerrain.Instance.ArrowMapWho.GetNearestObject(character.Tile, Mathf.CeilToInt(bestTargetPriority), delegate(ArrowProp arrow)
			{
				EquipmentPrototype grabbableEquipmentType = arrow.GetGrabbableEquipmentType();
				if (grabbableEquipmentType == null || grabbableEquipmentType.Weight > inventorySpace)
				{
					return false;
				}
				float num = character.Tile.GetDist(arrow.Tile);
				if (num >= bestTargetPriority)
				{
					return false;
				}
				if (character.HasMovementZone() && !character.MovementZone.Contains(arrow.Tile))
				{
					return false;
				}
				if (character.HasFailedFindAttempt(arrow, 0, 0, FindType.AutoCollect, null, null, null, RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
				{
					num += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
				}
				if (num >= bestTargetPriority)
				{
					return false;
				}
				if (character.Community.IsAnyMemberCollectingSomethingFrom(arrow, character))
				{
					return false;
				}
				best = arrow;
				bestTargetPriority = num;
				return true;
			});
		}
		return best;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		if (Target.Object is ArrowProp)
		{
			SetSubGoal(character, parent, new MoveToAndGrab(character, GetTargetObject(), MovementType.Walk));
			return;
		}
		Equipment autoCollectableItem = GetAutoCollectableItem(character, Target);
		if (autoCollectableItem != null)
		{
			MoveToAndTake moveToAndTake = new MoveToAndTake(character, GetTargetObject(), autoCollectableItem, Math.Min(autoCollectableItem.GetAmount(), Math.Max(1, (int)(character.GetAvailableInventorySpace() / autoCollectableItem.GetWeight()))), ignoreWeight: false, MovementType.Walk, null);
			moveToAndTake.IsGathering = true;
			SetSubGoal(character, parent, moveToAndTake);
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndTake moveToAndTake)
		{
			if (moveToAndTake.AmountRetrieved > 0f)
			{
				Equipment autoCollectableItem = GetAutoCollectableItem(character, Target);
				if (autoCollectableItem != null)
				{
					return new MoveToAndTake(character, GetTargetObject(), autoCollectableItem, Math.Min(autoCollectableItem.GetAmount(), Math.Max(1, (int)(character.GetAvailableInventorySpace() / autoCollectableItem.GetWeight()))), ignoreWeight: false, MovementType.Walk, null)
					{
						IsGathering = true
					};
				}
			}
			else if (moveToAndTake.GetTargetEquipment() != null)
			{
				character.AddFailedFindAttempt(Target.Object, 0, 0, FindType.AutoCollect, null, null, null);
			}
		}
		if (SubGoal is MoveToAndGrab { Success: false })
		{
			character.AddFailedFindAttempt(Target.Object, 0, 0, FindType.AutoCollect, null, null, null);
		}
		return base.GetNextSubGoal(character, parent);
	}
}

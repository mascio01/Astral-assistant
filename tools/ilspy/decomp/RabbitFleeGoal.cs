using System;
using UnityEngine;

public class RabbitFleeGoal : StateMachineGoal
{
	private static float FleeDist = 40f;

	public static float IgnoreThreatIfCloseToCarrotDist = 16f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("RabbitFleeCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.RabbitFleeGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Animal_Flee;
	}

	public override bool IsHighAlert(Character character)
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.IsAuthoritative())
		{
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Fled, GetTargetCharacter(), character);
		}
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		if (!character.IsVoiceSoundPlaying(VoiceSoundType.AnimalFleeing) && character.CheckFrontmostPrediction(PredictedEventType.FleeSound))
		{
			((Animal)character).PlayFleeSound();
		}
		_ = SubGoal;
		SetSubGoal(character, parent, new FleeFromAllEnemies(MovementType.Run, 1f, FleeDist, dontOpenOurGates: false, avoidHostileBases: false));
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		if (parent is AnimalGoal animalGoal)
		{
			animalGoal.LastFleeTime = Session.Instance.PlayTime;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsTargetDeleted() && !FleeFromAllEnemies.HasAnyThreats(character))
		{
			return null;
		}
		if (SubGoal is WaitAndFaceTarget && !FleeFromAllEnemies.HasAnyThreatsInRange(character, FleeDist))
		{
			return null;
		}
		if (SubGoal is Wait && FleeFromAllEnemies.HasAnyThreats(character))
		{
			return new FleeFromAllEnemies(MovementType.Run, 1f, FleeDist, dontOpenOurGates: false, avoidHostileBases: false);
		}
		TimeSpan waitTime = TimeSpan.FromSeconds(Mathf.Lerp(0f, 2f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).TotalSeconds + (float)character.Id)));
		if (SubGoal is FleeFromAllEnemies { Success: not false })
		{
			Target nearestThreat = FleeFromAllEnemies.GetNearestThreat(character);
			if (nearestThreat != null)
			{
				return new WaitAndFaceTarget(character, nearestThreat.Object, waitTime);
			}
		}
		return new Wait(waitTime);
	}

	public static Vector2? GetNearestCarrotSensedXZ(Character character)
	{
		Vector2? result = null;
		foreach (Target target in character.Targets)
		{
			if (target.Object != null && !target.Object.Deleted && !target.GetFlag(TargetFlags.Lost) && !target.Object.IsDestroyed())
			{
				if (target.Object.GetGrabbableEquipmentType() != null && character.LikesFood(target.Object.GetGrabbableEquipmentType()))
				{
					result = MathUtil.ToXZ(target.LastKnownPosition);
				}
				else if (target.GetFlag(TargetFlags.HasCarrot))
				{
					result = MathUtil.ToXZ(target.LastKnownPosition);
				}
			}
		}
		return result;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (!character.IsOutdoors())
			{
				return null;
			}
			Vector2? nearestCarrotSensedXZ = GetNearestCarrotSensedXZ(character);
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object == null || target.Object.Deleted || target.GetFlag(TargetFlags.Lost) || target.Object.IsDestroyed())
				{
					continue;
				}
				TileObject authoritativeOrElseThis = target.Object.GetAuthoritativeOrElseThis();
				if (!(authoritativeOrElseThis is Character character2))
				{
					if (!(target.Object is EnterableVehicle))
					{
						continue;
					}
				}
				else if (!character2.IsOutdoors())
				{
					continue;
				}
				if ((nearestCarrotSensedXZ.HasValue && !target.GetFlag(TargetFlags.AlarmedBy) && target.GetFlag(TargetFlags.Crouching) && (MathUtil.ToXZ(target.LastKnownPosition) - nearestCarrotSensedXZ.Value).magnitude < IgnoreThreatIfCloseToCarrotDist) || target.GetFlag(TargetFlags.TamedBy) || !character.IsEnemy(authoritativeOrElseThis))
				{
					continue;
				}
				if (Active)
				{
					if (target.TimeSinceLastDetected >= TimeSpan.FromSeconds(10.0))
					{
						continue;
					}
				}
				else if (target.Camouflage > 0f && target.TimeSinceLastHeardAttack >= TimeSpan.FromSeconds(10.0))
				{
					continue;
				}
				float num2 = character.Get2DDistToTargetLastKnownPos(target) / character.GetWalkSpeed();
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
			return result;
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active)
		{
			return true;
		}
		if (!IsTargetDeleted())
		{
			return FleeFromAllEnemies.HasAnyThreats(character);
		}
		return false;
	}
}

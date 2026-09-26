using System;
using UnityEngine;

public class RabbitAlertGoal : StateMachineGoal
{
	public bool HighAlert;

	private static float RunDistFromTarget = 8f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("RabbitAlertCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.RabbitAlertGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Target != null && (Target.TimeSinceLastHeard < TimeSpan.FromSeconds(4.0) || Target.Camouflage < Target.AlertCamouflageThreshold))
		{
			return GoalPriority.Animal_HighAlert;
		}
		if (HighAlert)
		{
			return GoalPriority.Animal_HighAlert;
		}
		return GoalPriority.Animal_Alert;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (Target == null)
		{
			return null;
		}
		return Target.GetAlertIcon();
	}

	public override bool IsHighAlert(Character character)
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Animal animal = (Animal)character;
		animal.CurrentPose = AnimalPose.Alert;
		float magnitude = MathUtil.ToXZ(Target.LastKnownPosition - character.Position).magnitude;
		HighAlert = magnitude <= RunDistFromTarget;
		if (HighAlert)
		{
			if (!character.IsVoiceSoundPlaying(VoiceSoundType.AnimalFleeing) && character.CheckFrontmostPrediction(PredictedEventType.FleeSound))
			{
				animal.PlayFleeSound();
			}
		}
		else if (!character.IsVoiceSoundPlaying(VoiceSoundType.AnimalAlert) && !character.IsVoiceSoundPlaying(VoiceSoundType.AnimalFleeing) && character.CheckFrontmostPrediction(PredictedEventType.AlertSound))
		{
			animal.PlayAlertSound();
		}
		SetSubGoal(character, parent, new TurnToTarget());
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		((Animal)character).CurrentPose = AnimalPose.Normal;
		base.OnDeactivate(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref HighAlert, 158);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!HighAlert && MathUtil.ToXZ(Target.LastKnownPosition - character.Position).magnitude <= RunDistFromTarget)
		{
			HighAlert = true;
			if (!character.IsVoiceSoundPlaying(VoiceSoundType.AnimalFleeing) && character.CheckFrontmostPrediction(PredictedEventType.AlertSound))
			{
				((Animal)character).PlayFleeSound();
			}
		}
		if (SubGoal is FleeFromTarget { MovementType: MovementType.Walk } && HighAlert)
		{
			SubGoal.SetMovementType(character, MovementType.Run);
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return null;
		}
		if (SubGoal is TurnToTarget || SubGoal is WaitAndFaceTarget)
		{
			if (Target.TimeSinceLastHeard >= TimeSpan.FromSeconds(2.0) && Target.TimeSinceLastSmelled >= TimeSpan.FromSeconds(2.0) && Target.Camouflage >= Target.AlertCamouflageThreshold)
			{
				return null;
			}
			return new AnimationGoal(ActionAnim.LookAround);
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (SubGoal is AnimationGoal)
		{
			((Animal)character).CurrentPose = AnimalPose.Normal;
			float magnitude = MathUtil.ToXZ(Target.LastKnownPosition - character.Position).magnitude;
			HighAlert = magnitude <= RunDistFromTarget;
			float minRange = magnitude + (character.IsSmallAnimal() ? 1f : 2f);
			float maxRange = magnitude + Mathf.Lerp(character.IsSmallAnimal() ? 2f : 4f, character.IsSmallAnimal() ? 4f : 8f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id));
			return new FleeFromTarget((!HighAlert) ? MovementType.Walk : (character.IsSmallAnimal() ? MovementType.Run : MovementType.Jog), minRange, maxRange, dontOpenOurGates: false);
		}
		((Animal)character).CurrentPose = AnimalPose.Alert;
		if (SubGoal is FleeFromTarget { Success: not false })
		{
			return new TurnToTarget();
		}
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(0f, 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (!character.IsOutdoors())
			{
				return null;
			}
			Vector2? nearestCarrotSensedXZ = RabbitFleeGoal.GetNearestCarrotSensedXZ(character);
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object == null || target.Object.Deleted || target.GetFlag(TargetFlags.Lost) || target.Object.IsDestroyed())
				{
					continue;
				}
				TileObject authoritativeOrElseThis = target.Object.GetAuthoritativeOrElseThis();
				if (authoritativeOrElseThis is Character character2 && character2.IsOutdoors() && (!nearestCarrotSensedXZ.HasValue || target.GetFlag(TargetFlags.AlarmedBy) || !target.GetFlag(TargetFlags.Crouching) || !((MathUtil.ToXZ(target.LastKnownPosition) - nearestCarrotSensedXZ.Value).magnitude < RabbitFleeGoal.IgnoreThreatIfCloseToCarrotDist)) && !target.GetFlag(TargetFlags.TamedBy) && character.IsEnemy(authoritativeOrElseThis) && (Active || !(target.TimeSinceLastHeard >= TimeSpan.FromSeconds(2.0)) || !(target.TimeSinceLastSmelled >= TimeSpan.FromSeconds(2.0)) || !(target.Camouflage >= Target.AlertCamouflageThreshold)))
				{
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
			}
			return result;
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		return !IsTargetDeleted();
	}
}

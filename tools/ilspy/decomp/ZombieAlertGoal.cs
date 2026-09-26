using System;
using UnityEngine;

public class ZombieAlertGoal : StateMachineGoal
{
	private TimeSpan StartedMovingTime;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("ZombieAlertCalcBestTarget");

	private static float RecentAttackTime = 30f;

	public override GoalType GetGoalType()
	{
		return GoalType.ZombieAlertGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Target != null && Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(60.0))
		{
			return GoalPriority.Zombie_AlertAttacker;
		}
		if (Target != null && Target.LastAttackedUsTime == Target.Never && Target.Camouflage >= 1f && Target.TimeSinceLastHeard >= TimeSpan.FromSeconds(10.0) && Target.TimeSinceLastVisible >= TimeSpan.FromSeconds(10.0) && Target.TimeSinceLastFullyVisible >= TimeSpan.FromSeconds(30.0))
		{
			return GoalPriority.Zombie_LowAlert;
		}
		return GoalPriority.Zombie_Alert;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (Target == null)
		{
			return null;
		}
		return Target.GetAlertIcon();
	}

	public override bool IsLowAlert(Character character)
	{
		return true;
	}

	public override bool IsHighAlert(Character character)
	{
		if (Target != null)
		{
			return Target.TimeSinceLastHeardAttack < TimeSpan.FromSeconds(RecentAttackTime);
		}
		return false;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref StartedMovingTime, 57);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieAlertSounds, VoiceSoundType.ZombieSnarl);
		}
		if (IsHighAlert(character))
		{
			SetSubGoal(character, parent, new TurnToTarget());
		}
		else
		{
			SetSubGoal(character, parent, new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(1f, 2f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).TotalSeconds + (float)character.Id)))));
		}
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (!character.IsOutdoors())
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object != null && !target.Object.Deleted && target.Object.GetAuthoritativeOrElseThis() is Character character2 && !target.HasAnyFlag((TargetFlags)8421440) && (Active || ShouldNoticeTarget(target)) && !character.ShouldIgnoreInvisibleStrainCharacter(character2) && !character2.IsPlayingDead())
				{
					float num2 = character.Get2DDistToTargetLastKnownPos(target) / Character.ZombieWalkSpeed;
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
		if (Active)
		{
			return true;
		}
		return !IsTargetDeleted();
	}

	public bool ShouldNoticeTarget(Target target)
	{
		if (target == null)
		{
			return false;
		}
		if (!(target.TimeSinceLastHeard < TimeSpan.FromSeconds(20.0)) && !(target.TimeSinceLastSmelled < TimeSpan.FromSeconds(2.0)) && !(target.TimeSinceLastHeardAttack < TimeSpan.FromSeconds(RecentAttackTime)))
		{
			return target.Camouflage < Target.AlertCamouflageThreshold;
		}
		return true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = SubGoal as MoveAsCloseAsPossibleTo;
		if (Target != null)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition);
			if (moveAsCloseAsPossibleTo != null && moveAsCloseAsPossibleTo.OriginalDestTile.GetDist(tileCoordForPos) >= 2f && Target.LastHeardTime > StartedMovingTime)
			{
				SetSubGoal(character, parent, new MoveAsCloseAsPossibleTo((!IsHighAlert(character) || character.ShouldLimp()) ? MovementType.Walk : MovementType.Run, tileCoordForPos));
			}
		}
		if (moveAsCloseAsPossibleTo != null && moveAsCloseAsPossibleTo.MovementType == MovementType.Walk && IsHighAlert(character) && !character.ShouldLimp())
		{
			moveAsCloseAsPossibleTo.SetMovementType(character, MovementType.Run);
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is TurnToTarget)
		{
			return new ZombieFrustrationGoal(character);
		}
		if (!ShouldNoticeTarget(Target))
		{
			return null;
		}
		if (character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieAlertSounds, VoiceSoundType.ZombieSnarl);
		}
		if (SubGoal is AnimationGoal || SubGoal is Wait || SubGoal is WaitAndFaceTarget)
		{
			GameTerrain instance = GameTerrain.Instance;
			Vector2 vector = MathUtil.ToXZ(Target.LastKnownPosition);
			float val = Math.Max(8f, (int)(vector - character.PosXZ).magnitude);
			if (Target.LastVisibility > 0.5f)
			{
				val = Math.Min(val, (float)Target.TimeSinceLastVisible.TotalSeconds * Character.CrouchRunSpeed);
			}
			val = Math.Min(val, (float)Target.TimeSinceLastHeard.TotalSeconds * Character.CrouchRunSpeed);
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(vector + MathUtil.RandomVec2InCircle((float)currentTime.TotalSeconds + (float)character.Id) * val);
			tileCoordForPosXZ = GameTerrain.Instance.ClampTileWithinBounds(tileCoordForPosXZ);
			StartedMovingTime = currentTime;
			return new MoveAsCloseAsPossibleTo((!IsHighAlert(character) || character.ShouldLimp()) ? MovementType.Walk : MovementType.Run, tileCoordForPosXZ);
		}
		if (IsHighAlert(character))
		{
			return new ZombieFrustrationGoal(character)
			{
				EnableFaceTarget = false
			};
		}
		return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(1f, 2f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).TotalSeconds + (float)character.Id))));
	}
}

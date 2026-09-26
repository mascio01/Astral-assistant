using System;
using UnityEngine;

public class AlertGoal : StateMachineGoal
{
	private bool HighAlert;

	private TimeSpan StartedMovingTime;

	private TimeSpan TakeCoverFailedTime = Target.Never;

	private TimeSpan MoveToBuildingFailedTime = Target.Never;

	private TimeSpan LastAlertSpeechTime = Target.Never;

	private TerrainCoord TakenCoverFrom = TerrainCoord.Invalid;

	private StayInRangeParams StayInRangeParams;

	public static float HiddenAttackerAlertTime = 600f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("AlertCalcBestTarget");

	public static float RecentAttackTime = 30f;

	public override GoalType GetGoalType()
	{
		return GoalType.AlertGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Target != null && Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(10.0))
		{
			return GoalPriority.Survivor_HighAlert;
		}
		if (!ShouldSearchForTarget(character))
		{
			if (character.IsGuarding())
			{
				return GoalPriority.Survivor_GuardWatchThreat;
			}
			return GoalPriority.Survivor_WatchThreat;
		}
		return GoalPriority.Survivor_Alert;
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
		return HighAlert;
	}

	public override bool IsLowAlert(Character character)
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref HighAlert, 57);
		reflector.AddAfter(ref StartedMovingTime, 57);
		reflector.AddAfter(ref TakeCoverFailedTime, 78);
		reflector.AddAfter(ref MoveToBuildingFailedTime, 85);
		reflector.AddAfter(ref LastAlertSpeechTime, 423);
		reflector.AddAfter(ref TakenCoverFrom, 78);
		if (reflector.Version >= 176)
		{
			StayInRangeParams.Reflect(reflector);
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		if (parent is SurvivorGoal)
		{
			StayInRangeParams = StayInRangeParams.OfSquadLeaderOrBase(character);
		}
		base.OnActivate(character, parent);
		HighAlert = CalcIsHighAlert();
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		Character targetCharacter = GetTargetCharacter();
		SpeechSituation speechSituation = (HighAlert ? SpeechSituation.HighAlert : ((Target.Camouflage > 0f && targetCharacter.IsControllableByOrFollowingPlayer()) ? SpeechSituation.Scared : SpeechSituation.Alert));
		if (character.IsAuthoritative() && targetCharacter != null && currentTime - LastAlertSpeechTime >= TimeSpan.FromSeconds((speechSituation == SpeechSituation.Alert) ? 60f : 20f) && !Target.GetFlag(TargetFlags.Inaccessible))
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, Target.GetFlag(TargetFlags.Lost) ? SpeechSituation.TargetLost : speechSituation);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter);
				LastAlertSpeechTime = currentTime;
			}
		}
		SetSubGoal(character, parent, new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(HighAlert ? 0.5f : 1f, HighAlert ? 1f : 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id)))));
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (character.InsideBuilding != null && !character.IsOutdoors() && !character.InsideBuilding.IsBurning() && AttackFallbackGoal.WouldBeMoreVulnerableIfExitingBuilding(character))
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (target.Object == null || target.Object.Deleted || target.Object.GetBaseObjectType() != BaseObjectType.Human || !(target.Object.GetAuthoritativeOrElseThis() is Character character2) || target.HasAnyFlag((TargetFlags)8423424) || character2.IsPlayingDead() || character2.Community == character.Community || (character.Community != null && (character.Community.CachedAllies.Contains(character2.Community) || (character.Community.IsFEMA && character2.Community != null && character2.Community.IsFEMA))))
				{
					continue;
				}
				if (target.LastAttackedUsTime == Target.Never)
				{
					if (!character.IsEnemy(character2))
					{
						continue;
					}
				}
				else if (character.ShouldIgnoreInvisibleStrainCharacter(character2))
				{
					continue;
				}
				if (character.IsTargetSurrendering(character2) || (character.Rank == Rank.Captive && character2.IsControllableByPlayer() && target.LastAttackedMeTime == Target.Never))
				{
					continue;
				}
				if (target != Target || !Active)
				{
					if (target.GetFlag(TargetFlags.EscapedFromTarget) || Attack.ShouldForgetInaccessibleTarget(character, target))
					{
						continue;
					}
					if (target.GetFlag(TargetFlags.KnownAssailant) || character.IsEnemy(character2))
					{
						if (!ShouldNoticeTarget(target))
						{
							continue;
						}
					}
					else if (target.TimeSinceLastHeardAttack >= TimeSpan.FromSeconds(RecentAttackTime))
					{
						continue;
					}
				}
				float num2 = character.Get2DDistToTargetLastKnownPos(target) / Character.WalkSpeed;
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

	public bool ShouldNoticeTarget(Target target)
	{
		if (target == null)
		{
			return false;
		}
		if (!(target.TimeSinceLastHeard < TimeSpan.FromSeconds(20.0)) && !(target.TimeSinceLastHeardAttack < TimeSpan.FromSeconds(RecentAttackTime)))
		{
			return target.Camouflage < Target.AlertCamouflageThreshold;
		}
		return true;
	}

	public bool ShouldSearchForTarget(Character character)
	{
		if (character.IsControllableByOrFollowingPlayer())
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.Zombie)
		{
			return false;
		}
		if (CalcDontOpenOurGates(character))
		{
			return false;
		}
		return true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.SquadLeader != null)
		{
			SetCrouching(character, parent, FollowGoal.WantCrouching(character));
			character.DirectControlledCrouching = Crouching;
		}
		MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = SubGoal as MoveAsCloseAsPossibleTo;
		if (Target != null)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition);
			if (moveAsCloseAsPossibleTo != null && moveAsCloseAsPossibleTo.DestTile.GetDist(tileCoordForPos) >= 2f && Target.LastHeardTime > StartedMovingTime)
			{
				StartedMovingTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
				SetSubGoal(character, parent, new MoveAsCloseAsPossibleTo((!HighAlert) ? MovementType.Walk : MovementType.Run, tileCoordForPos, float.MaxValue, CalcDontOpenOurGates(character)));
			}
			if (Target.FullyTracked && !character.IsEnemy(Target.Object) && !character.IsControllableByPlayer())
			{
				if (character.Community != null && Target.Object.GetCommunity() != null && character.IsAuthoritative())
				{
					Session.Instance.CommunityManager.SetRelationship(character.Community, Target.Object.GetCommunity(), CommunityRelationshipType.Hostile);
					Memory.OnMemorableEvent(MemoryPrototype.DeclaredWar, character, Target.Object, 1f, secret: false);
				}
				Character targetCharacter = GetTargetCharacter();
				if (targetCharacter != null && character.IsAuthoritative())
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Attacking);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, targetCharacter);
					}
				}
			}
		}
		if (!HighAlert && CalcIsHighAlert())
		{
			HighAlert = true;
			if (moveAsCloseAsPossibleTo != null && moveAsCloseAsPossibleTo.MovementType == MovementType.Walk)
			{
				moveAsCloseAsPossibleTo.SetMovementType(character, MovementType.Run);
			}
		}
	}

	public bool CalcIsHighAlert()
	{
		if (Target == null)
		{
			return false;
		}
		if (Target.GetFlag(TargetFlags.Lost) || Target.TimeSinceLastHeardAttack < TimeSpan.FromSeconds(RecentAttackTime) || Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(RecentAttackTime))
		{
			return true;
		}
		return false;
	}

	public static bool CalcDontOpenOurGates(Character character)
	{
		if (character.IsControllableByPlayer())
		{
			return true;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.Community.IsAISettlement())
		{
			return false;
		}
		if (!character.IsInEnclosedArea())
		{
			return false;
		}
		if (character.Community.HasRecentlyDeadMembers(2, PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - Sun.DayLength))
		{
			return true;
		}
		return false;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.HasBeenPlayerControlledRecently(critical: true, extraCritical: true))
		{
			return false;
		}
		if (Active)
		{
			return true;
		}
		return !IsTargetDeleted();
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (SubGoal is MoveToAndEnterBuilding { Success: false })
		{
			MoveToBuildingFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (character.IsGuarding() && character.Community != null)
		{
			Character targetCharacter = GetTargetCharacter();
			if ((targetCharacter == null || character.Inventory.GetBestWeapon(character, targetCharacter, wantRanged: true, bluntOnly: false) == null) && character.Community.DoesAnyOtherMemberWantToUseGuardPostInsteadOfMe(character, targetCharacter))
			{
				return new LeaveBuilding();
			}
		}
		if (!ShouldNoticeTarget(Target))
		{
			Character targetCharacter2 = GetTargetCharacter();
			if (targetCharacter2 != null)
			{
				if (character.IsAuthoritative())
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter2, SpeechSituation.TargetLost);
					if (speechForSituation != null)
					{
						character.Speak(speechForSituation, targetCharacter2);
					}
				}
				if (HighAlert)
				{
					character.SetRecentActivity(RecentActivityType.HighAlert, targetCharacter2);
				}
				else if (targetCharacter2.IsControllableByOrFollowingPlayer())
				{
					character.SetRecentActivity(RecentActivityType.Scared, targetCharacter2);
				}
			}
			if (Target != null)
			{
				Target.ForgetUnprovenAttacks();
			}
			return null;
		}
		Goal moveWithinRangeOfSquadLeaderGoal = Attack.GetMoveWithinRangeOfSquadLeaderGoal(character, CalcDontOpenOurGates(character), StayInRangeParams);
		if (moveWithinRangeOfSquadLeaderGoal != null)
		{
			return moveWithinRangeOfSquadLeaderGoal;
		}
		TakeCoverFromTarget takeCoverFromTarget = SubGoal as TakeCoverFromTarget;
		if (takeCoverFromTarget != null)
		{
			if (takeCoverFromTarget.Success)
			{
				if (TakeCoverGoal.CheckCover(character, Target, out var crouching))
				{
					TakenCoverFrom = GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition);
					TakeCoverFailedTime = Target.Never;
					if (crouching)
					{
						SetCrouching(character, parent, crouching: true);
						return new CrouchAnim();
					}
				}
			}
			else
			{
				TakeCoverFailedTime = currentTime;
			}
		}
		if (character.InsideBuilding == null && Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(4.0))
		{
			Building building = FindBuildingToShootFrom(character, fallback: false);
			if (building != null)
			{
				return new MoveToAndEnterBuilding(character, building, MovementType.Run, dontOpenOurGates: true);
			}
			if (currentTime - TakeCoverFailedTime >= TimeSpan.FromSeconds(4.0))
			{
				if (TakenCoverFrom == TerrainCoord.Invalid || TakenCoverFrom.GetDist(GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition)) >= 2f)
				{
					SetCrouching(character, parent, crouching: false);
					return new TakeCoverFromTarget(MovementType.Run, CalcDontOpenOurGates(character));
				}
				return new WaitAndFaceTarget(TimeSpan.FromSeconds(Mathf.Lerp(1f, 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id))));
			}
		}
		bool flag = ShouldSearchForTarget(character) && (character.SquadLeader == null || !character.SquadLeader.WantSquadToStayInRange()) && !character.IsGuarding() && !character.HasPersonality(CachedPersonalityType.Nervous);
		if ((SubGoal is WaitAndFaceTarget || SubGoal is Wait || takeCoverFromTarget != null) && flag)
		{
			StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
			GameTerrain instance = GameTerrain.Instance;
			Vector2 vector = MathUtil.ToXZ(Target.LastKnownPosition);
			float val = Math.Max(8f, (int)(vector - character.PosXZ).magnitude);
			if (Target.LastVisibility > 0.5f)
			{
				val = Math.Min(val, (float)Target.TimeSinceLastVisible.TotalSeconds * Character.CrouchRunSpeed);
			}
			val = Math.Min(val, (float)Target.TimeSinceLastHeard.TotalSeconds * Character.CrouchRunSpeed);
			Vector2 vector2 = vector + MathUtil.RandomVec2InCircle((float)currentTime.TotalSeconds + (float)character.Id) * val;
			if (resultTile != TerrainCoord.Invalid && resultDist < float.MaxValue)
			{
				Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(resultTile);
				float magnitude = (vector2 - tileCentreXZ).magnitude;
				if (magnitude > resultDist)
				{
					vector2 += (tileCentreXZ - vector2) * (magnitude - resultDist) / magnitude;
				}
			}
			TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(vector2);
			tileCoordForPosXZ = GameTerrain.Instance.ClampTileWithinBounds(tileCoordForPosXZ);
			StartedMovingTime = currentTime;
			return new MoveAsCloseAsPossibleTo((!HighAlert) ? MovementType.Walk : MovementType.Run, tileCoordForPosXZ, float.MaxValue, CalcDontOpenOurGates(character));
		}
		TimeSpan waitTime = TimeSpan.FromSeconds(Mathf.Lerp(HighAlert ? 0.5f : 1f, HighAlert ? 1f : 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)character.Id)));
		if (flag)
		{
			return new Wait(waitTime);
		}
		return new WaitAndFaceTarget(waitTime, aiming: false, character.IsGuarding());
	}

	private Building FindBuildingToShootFrom(Character character, bool fallback)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return null;
		}
		Equipment bestWeapon = character.Inventory.GetBestWeapon(character, targetCharacter, wantRanged: true, bluntOnly: false);
		if (bestWeapon == null)
		{
			return null;
		}
		return RangedAttack.FindBuildingToShootFrom(character, GetTargetCharacter(), bestWeapon, fallback, MoveToBuildingFailedTime, StayInRangeParams);
	}
}

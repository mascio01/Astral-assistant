using System;
using System.Text;
using UnityEngine;

public abstract class Goal
{
	public bool Active;

	public bool Finished;

	public GoalPriority Priority;

	public Target Target;

	public bool HasUserTarget;

	public bool Aiming;

	public bool Crouching;

	private static string[] GoalNames = StringUtil.GetEnumNames<GoalType>();

	private static Type[] GoalTypes = StringUtil.GetEnumTypes<GoalType>();

	private static string deleted = "deleted";

	private static string dead = "dead";

	~Goal()
	{
		if (Target != null)
		{
			Target.RefCount--;
			Target = null;
		}
	}

	public abstract GoalType GetGoalType();

	public static Goal Create(GoalType goalType)
	{
		return (Goal)Activator.CreateInstance(GoalTypes[(int)goalType]);
	}

	public static bool WantPrediction(GoalType goalType)
	{
		switch (goalType)
		{
		case GoalType.AnimationGoal:
		case GoalType.Idle:
		case GoalType.Wait:
		case GoalType.TurnTo:
		case GoalType.TurnToAngle:
		case GoalType.TurnToTarget:
		case GoalType.FaceTarget:
		case GoalType.WatchTarget:
		case GoalType.WaitAndFaceTarget:
		case GoalType.MoveTo:
		case GoalType.MoveToTarget:
		case GoalType.MoveAdjacentToTarget:
		case GoalType.MoveAsCloseAsPossibleToTarget:
		case GoalType.MoveWithinRangeAndSightOfTarget:
		case GoalType.MoveAdjacentTo:
		case GoalType.MoveWithinBounds:
		case GoalType.MoveAsCloseAsPossibleTo:
		case GoalType.MoveAsCloseAsPossibleAndWait:
		case GoalType.MoveWithinRangeOfTarget:
		case GoalType.TakeCoverFromTarget:
		case GoalType.TakeCoverAndReload:
		case GoalType.TakeCoverFromTargetWhileFiring:
		case GoalType.FleeFromTarget:
		case GoalType.EnterBuilding:
		case GoalType.MoveToAndEnterBuilding:
		case GoalType.LeaveBuilding:
		case GoalType.LeaveBuildingAndMoveTo:
		case GoalType.ObeyLeaderGoal:
		case GoalType.SurvivorGoal:
		case GoalType.ZombieGoal:
		case GoalType.Attack:
		case GoalType.AttackDefences:
		case GoalType.AttackAnim:
		case GoalType.MeleeAttackAnim:
		case GoalType.MeleeAttack:
		case GoalType.RangedAttack:
		case GoalType.AimAnim:
		case GoalType.AimAndAttack:
		case GoalType.ReloadAnim:
		case GoalType.Equip:
		case GoalType.EquipAnim:
		case GoalType.UnequipAnim:
		case GoalType.FleeGoal:
		case GoalType.FeedOnLiving:
		case GoalType.ZombieFrustrationGoal:
		case GoalType.ChargeTarget:
		case GoalType.MoveDirectlyToTarget:
		case GoalType.BiteTarget:
		case GoalType.PrepareBiteTarget:
		case GoalType.StartBiteTarget:
		case GoalType.LoopBiteTarget:
		case GoalType.FinishBiteTarget:
		case GoalType.FailBiteTarget:
		case GoalType.ZombieJumpGoal:
		case GoalType.FlankTarget:
		case GoalType.VaultWaistHighWall:
		case GoalType.MoveToAndVaultWaistHighWall:
		case GoalType.BandageSelfAnim:
		case GoalType.MedicateSelfAnim:
		case GoalType.OpenGateGoal:
		case GoalType.CloseGateGoal:
		case GoalType.LockGateGoal:
		case GoalType.ZombieAlertGoal:
		case GoalType.AlertGoal:
		case GoalType.PanicGoal:
		case GoalType.WaitAndFace:
		case GoalType.MoveToAndChokeHold:
		case GoalType.StartChokeHold:
		case GoalType.LoopChokeHold:
		case GoalType.FinishChokeHold:
		case GoalType.TargetedGoal:
		case GoalType.StopSittingGoal:
		case GoalType.MoveWithinRangeOfSquadLeader:
		case GoalType.AttackFallbackGoal:
		case GoalType.TakeCoverGoal:
		case GoalType.CrouchAnim:
		case GoalType.UncrouchAnim:
		case GoalType.MoveWithinRange:
		case GoalType.RetreatToBaseGoal:
			return true;
		default:
			return false;
		}
	}

	public Goal()
	{
	}

	public virtual void Reflect(Reflector reflector, Character character)
	{
		reflector.Add(ref Active);
		reflector.Add(ref Finished);
		reflector.Add(ref Priority);
		reflector.Add(ref Aiming);
		reflector.Add(ref Crouching);
		reflector.Add(ref HasUserTarget);
		reflector.Add(ref Target, character);
		if (reflector.IsDeserialising && Target != null)
		{
			Target.RefCount++;
		}
	}

	public virtual void OnActivate(Character character, Goal parent)
	{
		Active = true;
		Finished = false;
		if (Target != null)
		{
			OnActivateTarget(character, parent);
		}
		if (Aiming)
		{
			character.GoalAimingRefCount++;
		}
		if (Crouching)
		{
			character.GoalCrouchingRefCount++;
		}
		if (WantDisableSleep())
		{
			character.DisableSleep++;
		}
	}

	public virtual void OnDeactivate(Character character, Goal parent)
	{
		if (WantDisableSleep())
		{
			character.DisableSleep--;
		}
		if (Crouching)
		{
			character.GoalCrouchingRefCount--;
		}
		if (Aiming)
		{
			character.GoalAimingRefCount--;
		}
		if (Target != null)
		{
			OnDeactivateTarget(character, parent);
		}
		Active = false;
	}

	public virtual void PreUpdate(Character character, Goal parent)
	{
	}

	public virtual void Update(Character character, Goal parent)
	{
		CheckForImpossible(character, parent);
	}

	protected void CheckForImpossible(Character character, Goal parent)
	{
		if (!IsPossible(character, parent))
		{
			Finished = true;
		}
	}

	public virtual void PostUpdate(Character character, Goal parent)
	{
	}

	public virtual bool IsPossible(Character character, Goal parent)
	{
		return true;
	}

	public virtual GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Default;
	}

	public void SetAiming(Character character, Goal parent, bool aiming)
	{
		if (aiming != Aiming)
		{
			if (Aiming && Active)
			{
				character.GoalAimingRefCount--;
			}
			Aiming = aiming;
			if (Aiming && Active)
			{
				character.GoalAimingRefCount++;
			}
		}
	}

	public void SetCrouching(Character character, Goal parent, bool crouching)
	{
		if (crouching != Crouching)
		{
			if (Crouching && Active)
			{
				character.GoalCrouchingRefCount--;
			}
			Crouching = crouching;
			if (Crouching && Active)
			{
				character.GoalCrouchingRefCount++;
			}
		}
	}

	public virtual Target CalcBestTarget(Character character, Goal parent)
	{
		if (!HasUserTarget)
		{
			return parent?.Target;
		}
		return Target;
	}

	public void SetTarget(Character character, Goal parent, Target target)
	{
		if (target == Target)
		{
			return;
		}
		if (Target != null)
		{
			if (Active)
			{
				OnDeactivateTarget(character, parent);
			}
			Target.RefCount--;
		}
		Target = target;
		if (Target != null)
		{
			Target.RefCount++;
			if (Active)
			{
				OnActivateTarget(character, parent);
			}
		}
	}

	public void ClearTargetIfInactive(Character character, Goal parent)
	{
		if (!Active)
		{
			SetTarget(character, parent, null);
		}
	}

	public virtual void OnPrioritiserDeactivated(Character character, Goal parent)
	{
	}

	protected virtual void OnActivateTarget(Character character, Goal parent)
	{
	}

	protected virtual void OnDeactivateTarget(Character character, Goal parent)
	{
	}

	public TileObject GetTargetObject()
	{
		if (Target == null || Target.Object == null || Target.Object.Deleted)
		{
			return null;
		}
		return Target.Object;
	}

	public Character GetTargetCharacter()
	{
		return GetTargetObject() as Character;
	}

	public Human GetTargetHuman()
	{
		return GetTargetObject() as Human;
	}

	public Prop GetTargetProp()
	{
		return GetTargetObject() as Prop;
	}

	public Building GetTargetBuilding()
	{
		return GetTargetObject() as Building;
	}

	public TreeProp GetTargetTree()
	{
		return GetTargetObject() as TreeProp;
	}

	public FallenTreeProp GetTargetFallenTree()
	{
		return GetTargetObject() as FallenTreeProp;
	}

	public Bush GetTargetBush()
	{
		return GetTargetObject() as Bush;
	}

	public FoodProp GetTargetFood()
	{
		return GetTargetObject() as FoodProp;
	}

	public EnterableVehicle GetTargetVehicle()
	{
		return GetTargetObject() as EnterableVehicle;
	}

	public bool IsTargetMovingVehicle()
	{
		return GetTargetVehicle()?.IsMoving ?? false;
	}

	public bool IsTargetDeleted()
	{
		if (Target != null && Target.Object != null)
		{
			return Target.Object.Deleted;
		}
		return true;
	}

	public bool IsTargetDeletedOrDisappeared()
	{
		if (Target != null && Target.Object != null && !Target.Object.Deleted)
		{
			return Target.Object.IsDisappeared();
		}
		return true;
	}

	public bool IsTargetDeletedButNotNull()
	{
		if (Target != null && Target.Object != null)
		{
			return Target.Object.Deleted;
		}
		return false;
	}

	public virtual TileObject GetCurrentTarget(Character character)
	{
		return GetTargetObject();
	}

	public virtual TargettableBodyLocation GetCurrentTargetBodyLocation(Character character)
	{
		return TargettableBodyLocation.Torso;
	}

	public virtual void BuildDebugString(Character character, Goal parent, StringBuilder str)
	{
		int goalType = (int)GetGoalType();
		if (goalType < GoalNames.Length)
		{
			str.Append(GoalNames[goalType]);
		}
		else
		{
			str.AppendWithoutGarbage(goalType);
		}
		BuildDebugExtraInfoString(character, parent, str);
	}

	public virtual void BuildDebugExtraInfoString(Character character, Goal parent, StringBuilder str)
	{
		if (Target == null || (parent != null && Target == parent.Target))
		{
			return;
		}
		str.Append(' ');
		str.Append('<');
		if (Target.Object == null || Target.Object.Deleted)
		{
			str.Append(deleted);
		}
		else
		{
			Target.Object.BuildDisplayName(str, noStrangers: true, englishOnly: true);
			if (Target.Object.IsDestroyed())
			{
				str.Append(' ');
				str.Append(dead);
			}
		}
		str.Append('>');
	}

	public virtual bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		return false;
	}

	public virtual void OnActionAnimFinished(Character character, Goal parent, ActionAnim anim)
	{
	}

	public virtual void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
	}

	public virtual void OnSpokenToStarted(Character character, Goal parent, Character speaker, Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyParam)
	{
	}

	public virtual bool OnSpokenToFinished(Character character, Goal parent, Character speaker, Speech speech, bool interrupted, Speech cont, Speech specialBehaviourTriggered)
	{
		return false;
	}

	public virtual void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
	}

	public virtual void OnEncounterGate(Character character, Goal parent, Gate gate)
	{
	}

	public virtual void OnEncounterWaistHighWall(Character character, Goal parent, TileObject prop)
	{
	}

	public virtual void OnCollisionWithCharacter(Character character, Goal parent, Character other)
	{
	}

	public virtual void OnEquipmentPolicyChanged(Character character, Goal parent)
	{
	}

	public virtual void OnTargetStruggledFree(Character character, Goal parent, Character target)
	{
	}

	public virtual void OnChokeSucceeded(Character character, Goal parent, Character target)
	{
	}

	public virtual bool OnCraftingFinished(Character character, Goal parent, TileObject obj)
	{
		return false;
	}

	public virtual void PredictedFixup(Character character, Goal parent)
	{
	}

	public virtual void SetLeaderCommand(Character character, Goal parent, Goal command, TileObject target, ObeyLeaderGoal.SourceType source, PlayerRecord playerRecord)
	{
	}

	public virtual void OnPreActivate(Character character, Goal parent)
	{
	}

	public virtual Goal GetLeaderCommand()
	{
		return null;
	}

	public virtual Goal GetLeaderCommandEvenIfItIsInactive()
	{
		return null;
	}

	public virtual bool IsLeaderCommandFinished(Character character)
	{
		return true;
	}

	public virtual bool WasLeaderCommandSuccessful()
	{
		return false;
	}

	public virtual bool IsSubstantiallyFinished(Character character)
	{
		return Finished;
	}

	public virtual FollowGoal GetFollowGoal()
	{
		return null;
	}

	public virtual MovementType GetMovementType()
	{
		return MovementType.None;
	}

	public virtual void SetMovementType(Character character, MovementType movementType)
	{
	}

	public virtual Texture2D GetOverheadActionIcon(Character character)
	{
		return null;
	}

	public virtual bool CanShowDialogOptions(Character character)
	{
		return true;
	}

	public virtual bool IsDoingSomethingTerriblyImportant()
	{
		return false;
	}

	public virtual AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		return AIOverridesControlReason.None;
	}

	public virtual float IsWateringOrHarvestingPlant(PlantableCrop plant)
	{
		return float.MaxValue;
	}

	public virtual bool IsSatisfyingNeeds()
	{
		return false;
	}

	public virtual bool IsUsingEquippedItem()
	{
		return false;
	}

	public virtual TileObject GetCombatTarget(out TimeSpan combatStartTime)
	{
		combatStartTime = TimeSpan.Zero;
		return null;
	}

	public virtual bool IsFleeing()
	{
		return false;
	}

	public virtual bool WantSquadToStayInRange(Character character)
	{
		return character.IsControllableByPlayer();
	}

	public virtual bool IsHighAlert(Character character)
	{
		return false;
	}

	public virtual bool IsLowAlert(Character character)
	{
		return IsHighAlert(character);
	}

	public virtual bool IsBored(Character character)
	{
		return false;
	}

	public virtual bool IsCollectingSomethingFrom(TileObject obj)
	{
		return false;
	}

	public virtual bool WasSuccessful()
	{
		return false;
	}

	public virtual bool WantDisableSleep()
	{
		return false;
	}

	public virtual int CheckDisableSleepRefCount()
	{
		if (!WantDisableSleep())
		{
			return 0;
		}
		return 1;
	}
}

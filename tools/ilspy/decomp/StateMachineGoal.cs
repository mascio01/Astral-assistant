using System;
using System.Text;
using UnityEngine;

public abstract class StateMachineGoal : Goal
{
	public Goal SubGoal;

	private static string colon = " : ";

	public StateMachineGoal()
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref SubGoal, character);
	}

	public void SetSubGoal(Character character, Goal parent, Goal goal, bool hasCalculatedTargetAlready = false)
	{
		if (SubGoal == goal)
		{
			return;
		}
		_ = SubGoal;
		goal?.OnPreActivate(character, parent);
		if (SubGoal != null)
		{
			SubGoal.OnDeactivate(character, this);
			if (SubGoal != goal)
			{
				SubGoal.SetTarget(character, this, null);
			}
		}
		SubGoal = goal;
		if (SubGoal != null)
		{
			_ = SubGoal.Target;
			if (!hasCalculatedTargetAlready)
			{
				SubGoal.SetTarget(character, this, SubGoal.CalcBestTarget(character, this));
			}
			SubGoal.OnActivate(character, this);
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		SetSubGoal(character, parent, null);
	}

	public override void PreUpdate(Character character, Goal parent)
	{
		if (SubGoal != null)
		{
			SubGoal.SetTarget(character, this, SubGoal.CalcBestTarget(character, this));
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		CheckForSubGoalFinished(character, parent, hasCalculatedTargetAlready: false);
	}

	protected void CheckForSubGoalFinished(Character character, Goal parent, bool hasCalculatedTargetAlready)
	{
		if (SubGoal == null || Finished)
		{
			return;
		}
		SubGoal.PreUpdate(character, this);
		SubGoal.Update(character, this);
		if (SubGoal == null)
		{
			Debug.LogWarning("SubGoal was set to null during update: " + character.GetDisplayNameString() + " (" + character.GetGoalDebugString() + ")");
			return;
		}
		SubGoal.PostUpdate(character, this);
		if (!SubGoal.Finished)
		{
			return;
		}
		Goal subGoal = SubGoal;
		SubGoal.OnDeactivate(character, this);
		SubGoal = GetNextSubGoal(character, parent);
		if (subGoal != SubGoal)
		{
			subGoal.SetTarget(character, this, null);
		}
		if (SubGoal != null)
		{
			if (!hasCalculatedTargetAlready)
			{
				SubGoal.SetTarget(character, this, SubGoal.CalcBestTarget(character, this));
			}
			SubGoal.OnActivate(character, this);
			if (!SubGoal.Finished && !SubGoal.IsPossible(character, this))
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				subGoal.BuildDebugString(character, this, stringBuilder);
				SubGoal.BuildDebugString(character, this, stringBuilder2);
				Debug.LogError(character.GetDisplayNameString() + " (" + character.GetGoalDebugString() + ") activating a SubGoal that is Not Possible (old = " + stringBuilder.ToString() + ", new = " + stringBuilder2.ToString() + ")");
			}
		}
		else if (FinishOnNullSubGoal())
		{
			Finished = true;
		}
	}

	public override void PostUpdate(Character character, Goal parent)
	{
	}

	protected virtual bool FinishOnNullSubGoal()
	{
		return true;
	}

	protected virtual Goal GetNextSubGoal(Character character, Goal parent)
	{
		return null;
	}

	public override void BuildDebugString(Character character, Goal parent, StringBuilder str)
	{
		base.BuildDebugString(character, parent, str);
		if (SubGoal != null)
		{
			str.Append(colon);
			SubGoal.BuildDebugString(character, this, str);
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.OnAnimationEvent(character, this, animEvent);
	}

	public override void OnActionAnimFinished(Character character, Goal parent, ActionAnim anim)
	{
		if (SubGoal != null)
		{
			SubGoal.OnActionAnimFinished(character, parent, anim);
		}
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		if (SubGoal != null)
		{
			SubGoal.OnSpeechFinished(character, this, speech, listener, interrupted, cont, continueObject, continueParam);
		}
	}

	public override void OnSpokenToStarted(Character character, Goal parent, Character speaker, Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyParam)
	{
		if (SubGoal != null)
		{
			SubGoal.OnSpokenToStarted(character, this, speaker, speech, speechObject, speechParam, replyTo, replyReferringTo, replyParam);
		}
	}

	public override bool OnSpokenToFinished(Character character, Goal parent, Character speaker, Speech speech, bool interrupted, Speech cont, Speech specialBehaviourTriggered)
	{
		if (SubGoal != null)
		{
			return SubGoal.OnSpokenToFinished(character, this, speaker, speech, interrupted, cont, specialBehaviourTriggered);
		}
		return false;
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		if (SubGoal != null)
		{
			SubGoal.OnDamaged(character, this, source, injuryLocation, absorbedByVest);
		}
	}

	public override void OnEncounterWaistHighWall(Character character, Goal parent, TileObject prop)
	{
		if (SubGoal != null)
		{
			SubGoal.OnEncounterWaistHighWall(character, this, prop);
		}
	}

	public override void OnEncounterGate(Character character, Goal parent, Gate gate)
	{
		if (SubGoal != null)
		{
			SubGoal.OnEncounterGate(character, this, gate);
		}
	}

	public override void OnCollisionWithCharacter(Character character, Goal parent, Character other)
	{
		if (SubGoal != null)
		{
			SubGoal.OnCollisionWithCharacter(character, this, other);
		}
	}

	public override void OnEquipmentPolicyChanged(Character character, Goal parent)
	{
		if (SubGoal != null)
		{
			SubGoal.OnEquipmentPolicyChanged(character, this);
		}
	}

	public override void OnTargetStruggledFree(Character character, Goal parent, Character target)
	{
		if (SubGoal != null)
		{
			SubGoal.OnTargetStruggledFree(character, this, target);
		}
	}

	public override void OnChokeSucceeded(Character character, Goal parent, Character target)
	{
		if (SubGoal != null)
		{
			SubGoal.OnChokeSucceeded(character, this, target);
		}
	}

	public override void PredictedFixup(Character character, Goal parent)
	{
		if (SubGoal != null)
		{
			SubGoal.PredictedFixup(character, this);
		}
	}

	public override TileObject GetCurrentTarget(Character character)
	{
		if (SubGoal == null)
		{
			return GetTargetObject();
		}
		return SubGoal.GetCurrentTarget(character);
	}

	public override TargettableBodyLocation GetCurrentTargetBodyLocation(Character character)
	{
		if (SubGoal == null)
		{
			return TargettableBodyLocation.Torso;
		}
		return SubGoal.GetCurrentTargetBodyLocation(character);
	}

	public override MovementType GetMovementType()
	{
		if (SubGoal == null)
		{
			return MovementType.None;
		}
		return SubGoal.GetMovementType();
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		if (SubGoal != null)
		{
			SubGoal.SetMovementType(character, movementType);
		}
	}

	public override bool OnCraftingFinished(Character character, Goal parent, TileObject obj)
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.OnCraftingFinished(character, this, obj);
	}

	public override int CheckDisableSleepRefCount()
	{
		int num = base.CheckDisableSleepRefCount();
		if (SubGoal != null)
		{
			num += SubGoal.CheckDisableSleepRefCount();
		}
		return num;
	}

	public override bool CanShowDialogOptions(Character character)
	{
		if (SubGoal == null)
		{
			return true;
		}
		return SubGoal.CanShowDialogOptions(character);
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsDoingSomethingTerriblyImportant();
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (SubGoal == null)
		{
			return AIOverridesControlReason.None;
		}
		return SubGoal.AIOverridesControl(character, this);
	}

	public override float IsWateringOrHarvestingPlant(PlantableCrop plant)
	{
		if (SubGoal == null)
		{
			return float.MaxValue;
		}
		return SubGoal.IsWateringOrHarvestingPlant(plant);
	}

	public override bool IsSatisfyingNeeds()
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsSatisfyingNeeds();
	}

	public override bool IsUsingEquippedItem()
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsUsingEquippedItem();
	}

	public override TileObject GetCombatTarget(out TimeSpan combatStartTime)
	{
		if (SubGoal != null)
		{
			return SubGoal.GetCombatTarget(out combatStartTime);
		}
		combatStartTime = TimeSpan.Zero;
		return null;
	}

	public override bool IsFleeing()
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsFleeing();
	}

	public override bool WantSquadToStayInRange(Character character)
	{
		if (SubGoal == null)
		{
			return base.WantSquadToStayInRange(character);
		}
		return SubGoal.WantSquadToStayInRange(character);
	}

	public override bool IsHighAlert(Character character)
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsHighAlert(character);
	}

	public override bool IsLowAlert(Character character)
	{
		if (!IsHighAlert(character))
		{
			if (SubGoal == null)
			{
				return false;
			}
			return SubGoal.IsLowAlert(character);
		}
		return true;
	}

	public override bool IsBored(Character character)
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsBored(character);
	}

	public override bool IsCollectingSomethingFrom(TileObject obj)
	{
		if (SubGoal == null)
		{
			return false;
		}
		return SubGoal.IsCollectingSomethingFrom(obj);
	}
}

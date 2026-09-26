using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimWrapper
{
	public ActionAnim Action;

	public string ClipName;

	public AnimationClip Clip;

	public float ClipLength;

	public string BaseStateName;

	public string UpperBodyStateName;

	public int BaseStateNameHash;

	public int UpperBodyStateNameHash;

	public bool UpperBodyIsAdditive;

	public int EquipmentStateNameHash;

	public List<AnimEvent> Events = new List<AnimEvent>();

	public bool HasRootMotion;

	public bool CanTurnDuringAnim = true;

	public Resource<SavedRootMotion> RootMotion;

	public bool Looped;

	public bool Aiming;

	public bool Crouching;

	public bool Sitting;

	public bool Zombie;

	public bool Youth;

	public bool PlayDeadOnFront;

	public BaseObjectType Species = BaseObjectType.Human;

	public bool OnlyAgainstDirectControlled;

	public bool NotAgainstDirectControlled;

	public bool IsInteractionWithObject;

	public Type EquippedType;

	public Type NotEquippedSubType;

	public TargettableBodyLocation TargetBodyLocation;

	public float TargetRange;

	public bool TargetOnGround;

	public GenderType Gender = GenderType.Count;

	public GenderType TargetGender = GenderType.Count;

	public float Speed = 1f;

	public float TransitionInTime = 0.1f;

	public float InterruptibleFrac = 1f;

	public float VaultLoopStartFrac;

	public float VaultLoopEndFrac = 1f;

	public PipAnimView PipView;

	public bool DontRestartIfAlreadyPLaying;

	public TimeSpan GetDuration()
	{
		return TimeSpan.FromSeconds(ClipLength / Speed);
	}

	public TimeSpan GetDurationSpeedIndependant()
	{
		return TimeSpan.FromSeconds(ClipLength);
	}

	private void TriggerEventsBetween(Character character, TimeSpan start, TimeSpan end)
	{
		foreach (AnimEvent @event in Events)
		{
			if (@event.Time >= start && @event.Time < end)
			{
				character.OnAnimationEvent(@event);
			}
		}
		if (HasRootMotion && Action != ActionAnim.ZombieBiteStart && Action != ActionAnim.ZombieJumpBiteStart)
		{
			float overallScale = character.Appearance.OverallScale;
			float time = (float)start.TotalSeconds * Speed;
			float time2 = (float)end.TotalSeconds * Speed;
			SavedRootMotion asset = RootMotion.GetAsset();
			float num = asset.Curve.Evaluate(time) * overallScale;
			float num2 = asset.Curve.Evaluate(time2) * overallScale;
			float num3 = ((asset.CurveX != null) ? (asset.CurveX.Evaluate(time) * overallScale) : 0f);
			float num4 = ((asset.CurveX != null) ? (asset.CurveX.Evaluate(time2) * overallScale) : 0f);
			character.AddVelocityXZ(MathUtil.ToXZ(character.Forward) * (num2 - num) + MathUtil.ToXZ(character.Right) * (num4 - num3), setFollowMeDir: false);
			if (Action == ActionAnim.Vault)
			{
				float animHeight = ((asset.CurveY != null) ? (asset.CurveY.Evaluate(time2) * overallScale) : 0f);
				character.AnimHeight = animHeight;
			}
		}
	}

	public void TriggerEvents(Character character, TimeSpan start, TimeSpan dt)
	{
		if (Looped)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(Clip.length / Speed);
			start = TimeSpan.FromTicks(start.Ticks % timeSpan.Ticks);
			TimeSpan timeSpan2 = start + dt;
			while (timeSpan2 >= timeSpan)
			{
				TriggerEventsBetween(character, start, timeSpan);
				dt -= timeSpan - start;
				start = new TimeSpan(0L);
				timeSpan2 = start + dt;
			}
			TriggerEventsBetween(character, start, timeSpan2);
		}
		else
		{
			TriggerEventsBetween(character, start, start + dt);
		}
	}

	public TimeSpan GetTimeOfLastEvent()
	{
		float num = 0f;
		foreach (AnimEvent @event in Events)
		{
			num = Math.Max((float)@event.Time.TotalSeconds, num);
		}
		return TimeSpan.FromSeconds(num);
	}

	public TimeSpan GetTimeOfEvent(AnimationEventType eventType)
	{
		foreach (AnimEvent @event in Events)
		{
			if (@event.EventType == eventType)
			{
				return TimeSpan.FromSeconds(@event.Time.TotalSeconds);
			}
		}
		Debug.LogError("Event not found: " + ClipName + ", " + eventType);
		return TimeSpan.Zero;
	}

	public bool CanBeParried(Character attacker, Character defender, float animSpeed, out ActionAnim parryAction, out bool enabled, out float score)
	{
		enabled = false;
		score = float.MinValue;
		parryAction = ActionAnim.None;
		AttackType attackType = AttackType.Invalid;
		using (List<AnimEvent>.Enumerator enumerator = Events.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.EventType)
				{
				case AnimationEventType.ChanceToParryHigh:
					attackType = AttackType.High;
					break;
				case AnimationEventType.ChanceToParryMiddle:
					attackType = AttackType.Middle;
					break;
				case AnimationEventType.ChanceToParryLow:
					attackType = AttackType.Low;
					break;
				case AnimationEventType.ChanceToDodgeKick:
					attackType = AttackType.Kick;
					break;
				case AnimationEventType.ChanceToBlockZombieBite:
					attackType = AttackType.ZombieBite;
					break;
				case AnimationEventType.ChanceToBlockZombieJump:
					attackType = AttackType.ZombieJump;
					break;
				case AnimationEventType.ChanceToBlockPunch:
					attackType = attacker.GetCurrentTargetBodyLocation() switch
					{
						TargettableBodyLocation.Head => AttackType.PunchHigh, 
						TargettableBodyLocation.Legs => AttackType.PunchLow, 
						_ => AttackType.Punch, 
					};
					break;
				}
			}
		}
		bool isFacingAttacker = false;
		if (attackType != AttackType.Invalid && defender.CanParry(attacker, attackType, out enabled, out parryAction, out isFacingAttacker, out score))
		{
			switch (attackType)
			{
			case AttackType.ZombieBite:
				return true;
			case AttackType.ZombieJump:
				return Vector2.Dot(defender.PosXZ - attacker.PosXZ, attacker.Forward) >= 0f;
			default:
			{
				TimeSpan timeSpan = TimeSpan.Zero;
				foreach (AnimEvent @event in Events)
				{
					AnimationEventType eventType = @event.EventType;
					if ((uint)(eventType - 4) <= 5u)
					{
						timeSpan = TimeSpan.FromSeconds(Math.Max((float)@event.Time.TotalSeconds / (Speed * animSpeed), (float)timeSpan.TotalSeconds));
					}
				}
				return attacker.GetActionAnimTime() < timeSpan;
			}
			}
		}
		return false;
	}
}

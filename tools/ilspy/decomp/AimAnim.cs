using System;
using UnityEngine;

public class AimAnim : Goal
{
	private bool Assassinate;

	private TimeSpan StartAimingTime = Target.Never;

	public bool Success;

	private static float FatigueAimTime = 0.75f;

	public AimAnim()
	{
	}

	public AimAnim(bool assassinate)
	{
		Assassinate = assassinate;
		Aiming = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Assassinate);
		reflector.Add(ref Success);
		reflector.AddAfter(ref StartAimingTime, 459);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AimAnim;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.GoalTarget = Target;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		AmmoWeapon ammoWeapon = character.EquippedItem as AmmoWeapon;
		if (ammoWeapon != null && ammoWeapon.GetFatiguePenaltyWhenAiming() != 0f)
		{
			if (character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel && StartAimingTime == Target.Never)
			{
				SetAiming(character, parent, aiming: false);
				return;
			}
			if (!Aiming && character.GetFatigueMinusAdrenaline() < Character.ExhaustedFatigueLevel - ammoWeapon.GetFatiguePenaltyWhenAiming() * FatigueAimTime * 1.1f)
			{
				SetAiming(character, parent, aiming: true);
				StartAimingTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			}
		}
		if (!(MathUtil.AngleDiff(character.FacingAngle, character.DesiredFacingAngle) < MathF.PI / 100f) || !character.IsLongEnoughSinceWeLastFired() || (character.GetCurrentActionPriority() >= ActionPriority.Attack && !character.IsActionAnimFinished()))
		{
			return;
		}
		if (ammoWeapon == null)
		{
			Finished = true;
			Success = true;
		}
		else if ((ammoWeapon.GetFatiguePenaltyWhenAttacking() == 0f || !(character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel) || Aiming) && !(PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartAimingTime < TimeSpan.FromSeconds(FatigueAimTime)))
		{
			float accurateRange;
			float rangeIncludingEffects = ammoWeapon.GetRangeIncludingEffects(character, GetTargetObject(), out accurateRange);
			float magnitude = MathUtil.ToXZ(Target.Object.Pos - character.Pos).magnitude;
			float num = 1f - Mathf.Clamp01((magnitude - accurateRange) / (rangeIncludingEffects - accurateRange));
			float val = 0.25f + 0.5f * ((float)character.GetSkillLevelWithEffects(ammoWeapon.GetRangeSkillType()) / 5f);
			val = Math.Min(val, Math.Max(0f, num - 0.25f));
			if (Assassinate || character.GetAccuracy() >= val)
			{
				Finished = true;
				Success = true;
			}
			if (magnitude > rangeIncludingEffects || !Target.HasLineOfSightIgnoringPlantCover)
			{
				Finished = true;
			}
		}
	}
}

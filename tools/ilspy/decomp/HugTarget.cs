using System;
using UnityEngine;

public class HugTarget : StateMachineGoal
{
	private TimeSpan Timeout;

	private bool Shoot;

	public bool Success;

	public static float MaxStartHugDist = 0.8f;

	public HugTarget()
	{
	}

	public HugTarget(TimeSpan timeout, bool shoot)
	{
		Timeout = timeout;
		Shoot = shoot;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Timeout);
		reflector.Add(ref Shoot);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.HugTarget;
	}

	public static Vector3 GetPosForHuggingAnimation(Character character, Character targetCharacter)
	{
		Vector3 result = targetCharacter.Position - MathUtil.ToX0Y(MathUtil.GetDirFromAngle(character.DesiredFacingAngle)) * MaxStartHugDist;
		result.y = targetCharacter.Position.y;
		return result;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.InsideBuilding != null)
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || IsTargetDeleted())
		{
			return false;
		}
		if (targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (!Active)
		{
			if ((MathUtil.ToXZ(targetCharacter.Position) - MathUtil.ToXZ(character.Position)).sqrMagnitude > MaxStartHugDist * MaxStartHugDist)
			{
				return false;
			}
			if (!targetCharacter.IsAwake)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.HaltMovement();
		character.DirectControlledCrouching = false;
		if (character.EquippedItem != null)
		{
			SetSubGoal(character, parent, new UnequipAnim());
		}
		else
		{
			SetSubGoal(character, parent, new StartHugTarget());
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (character.IsPredicted() == targetCharacter.IsPredicted())
		{
			targetCharacter.OnHugFinished(character);
		}
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is UnequipAnim)
		{
			return new StartHugTarget();
		}
		if (SubGoal is StartHugTarget)
		{
			return new LoopHugTarget(Timeout);
		}
		if (SubGoal is LoopHugTarget loopHugTarget)
		{
			if (Shoot)
			{
				return new HugShootTarget();
			}
			Character targetCharacter = GetTargetCharacter();
			if (character.IsPredicted() == targetCharacter.IsPredicted())
			{
				targetCharacter.OnHugFinished(character);
			}
			Success = loopHugTarget.Success;
			return new FinishHugTarget();
		}
		if (SubGoal is HugShootTarget hugShootTarget)
		{
			Success = hugShootTarget.Success;
			return null;
		}
		return null;
	}
}

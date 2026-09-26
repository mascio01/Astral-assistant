using UnityEngine;

public class AnimationGoal : Goal
{
	protected ActionAnim Anim;

	public SpeechAnimState AnimState;

	public bool EnableFaceTarget = true;

	public Vector3 PosWhenAnimStarted;

	public float AnimSpeed = 1f;

	public AnimationGoal()
	{
	}

	public AnimationGoal(ActionAnim anim)
	{
		Anim = anim;
		AnimSpeed = 1f;
	}

	public AnimationGoal(ActionAnim anim, float animSpeed)
	{
		Anim = anim;
		AnimSpeed = animSpeed;
	}

	public AnimationGoal(Character character, TileObject targetObj, ActionAnim anim)
	{
		Anim = anim;
		AnimSpeed = 1f;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public AnimationGoal(Character character, TileObject targetObj, ActionAnim anim, float animSpeed)
	{
		Anim = anim;
		AnimSpeed = animSpeed;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public AnimationGoal(Character character, TileObject targetObj, ActionAnim anim, bool enableFaceTarget)
	{
		Anim = anim;
		EnableFaceTarget = enableFaceTarget;
		AnimSpeed = 1f;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public AnimationGoal(Character character, TileObject targetObj, ActionAnim anim, bool enableFaceTarget, float animSpeed)
	{
		Anim = anim;
		EnableFaceTarget = enableFaceTarget;
		AnimSpeed = animSpeed;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public AnimationGoal(ActionAnim anim, bool enableFaceTarget)
	{
		Anim = anim;
		EnableFaceTarget = enableFaceTarget;
		AnimSpeed = 1f;
	}

	public AnimationGoal(ActionAnim anim, bool enableFaceTarget, float animSpeed)
	{
		Anim = anim;
		EnableFaceTarget = enableFaceTarget;
		AnimSpeed = animSpeed;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AnimationGoal;
	}

	public ActionAnim GetAnim()
	{
		return Anim;
	}

	public bool CanInterrupt(Character character)
	{
		if (AnimState >= SpeechAnimState.Started && character.CurrentActionAnim == Anim)
		{
			return character.IsActionAnimInterruptible();
		}
		return false;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Anim);
		reflector.Add(ref AnimState);
		reflector.Add(ref EnableFaceTarget);
		reflector.Add(ref PosWhenAnimStarted);
		reflector.AddAfter(ref AnimSpeed, 600);
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DirectControlled = false;
		if (CanStartAnimation(character, parent) && character.TryStartActionAnimFromGoal(Anim, GetTargetObject(), AnimSpeed))
		{
			if (WantFaceTarget(character))
			{
				character.GoalTarget = Target;
			}
			AnimState = SpeechAnimState.Started;
			PosWhenAnimStarted = character.Position;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (AnimState == SpeechAnimState.Started && WantStopAnimOnExit())
		{
			character.StopActionAnim(Anim);
		}
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished)
		{
			if (character.CurrentActionAnim != Anim)
			{
				if (WantBailOnFail())
				{
					Finished = true;
				}
				else
				{
					AnimState = SpeechAnimState.NotStarted;
					character.GoalTarget = null;
				}
			}
			if (AnimState == SpeechAnimState.NotStarted && CanStartAnimation(character, parent) && character.TryStartActionAnimFromGoal(Anim, GetTargetObject(), AnimSpeed))
			{
				AnimState = SpeechAnimState.Started;
				PosWhenAnimStarted = character.Position;
			}
			if (AnimState == SpeechAnimState.Started && WantFaceTarget(character))
			{
				character.GoalTarget = Target;
			}
		}
		if (AnimState == SpeechAnimState.NotStarted || !WantPosInterpolation(character, out var destPos))
		{
			return;
		}
		if (character.AnimWrapper != null)
		{
			if (character.AnimWrapper.HasRootMotion)
			{
				float time = (float)character.GetActionAnimTime().TotalSeconds * character.AnimWrapper.Speed * character.AnimSpeed;
				SavedRootMotion savedRootMotion = character.AnimWrapper.RootMotion;
				float num = savedRootMotion.Curve.Evaluate(time) - savedRootMotion.Curve.Evaluate(0f);
				float num2 = ((savedRootMotion.CurveX != null) ? (savedRootMotion.CurveX.Evaluate(time) - savedRootMotion.CurveX.Evaluate(0f)) : 0f);
				float t = num / savedRootMotion.GetDist();
				Vector3 position = Vector3.Lerp(PosWhenAnimStarted, destPos, t) + MathUtil.ToXZY(MathUtil.RightNormal(MathUtil.ToXZ((destPos - PosWhenAnimStarted).normalized)) * num2, 0f);
				character.SetPosition(position);
			}
			else
			{
				Vector3 position2 = Vector3.Lerp(PosWhenAnimStarted, destPos, character.GetActionAnimPlayedFrac());
				character.SetPosition(position2);
			}
		}
		else
		{
			character.SetPosition(destPos);
		}
	}

	public override void OnActionAnimFinished(Character character, Goal parent, ActionAnim anim)
	{
		base.OnActionAnimFinished(character, parent, anim);
		if (AnimState == SpeechAnimState.Started && anim == Anim)
		{
			OnAnimationFinished(character, parent);
			AnimState = SpeechAnimState.Finished;
			Finished = true;
		}
	}

	public virtual void OnAnimationFinished(Character character, Goal parent)
	{
	}

	public virtual bool WantFaceTarget(Character character)
	{
		if (!EnableFaceTarget)
		{
			return false;
		}
		if (character.Sitting)
		{
			return false;
		}
		int num = OnlyFaceTargetIfInRange();
		if (num == 0)
		{
			return true;
		}
		if (Target == null || Target.Object == null)
		{
			return true;
		}
		return (character.PosXZ - Target.Object.PosXZ).sqrMagnitude <= (float)(num * num);
	}

	public virtual int OnlyFaceTargetIfInRange()
	{
		return 0;
	}

	public virtual bool WantPosInterpolation(Character character, out Vector3 destPos)
	{
		destPos = Vector3.zero;
		return false;
	}

	public virtual bool WantBailOnFail()
	{
		return false;
	}

	public virtual bool WantStopAnimOnExit()
	{
		return true;
	}

	public virtual bool CanStartAnimation(Character character, Goal parent)
	{
		return !character.IsRagdollOrProneOrRecovering();
	}

	public void SetAnimSpeed(Character character, float animSpeed)
	{
		AnimSpeed = animSpeed;
		if (AnimState == SpeechAnimState.Started)
		{
			character.SetAnimSpeed(animSpeed);
			if (character.Predicted != null && RoleGoal.CanActionAnimSpeechChange(character.Predicted.CurrentActionAnim))
			{
				character.Predicted.SetAnimSpeed(animSpeed);
			}
		}
	}
}

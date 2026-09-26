using System;
using UnityEngine;

public class ZombieJumpGoal : AnimationGoal
{
	private Vector2 Direction;

	public bool Success;

	public static float BiteDist = 0.75f;

	public static float DodgeTime = 1f;

	public ZombieJumpGoal()
	{
	}

	public ZombieJumpGoal(Vector2 direction)
		: base(ActionAnim.ZombieJump)
	{
		Direction = direction;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Direction);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ZombieJumpGoal;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		return null;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.HaltMovement();
		character.DesiredFacingAngle = MathUtil.GetAngleFromDir(Direction, character.DesiredFacingAngle);
		if (!character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
		}
		Character targetCharacter = parent.GetTargetCharacter();
		if (character.IsAuthoritative() && !character.IsBeingChoked() && targetCharacter != null)
		{
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, targetCharacter, character, character));
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.CurrentActionAnim != ActionAnim.ZombieJump)
		{
			Success = false;
			Finished = true;
			return;
		}
		Character targetCharacter = parent.GetTargetCharacter();
		if (AnimState == SpeechAnimState.NotStarted || !(character.GetActionAnimTimeSpeedIndependant().TotalSeconds < (double)DodgeTime) || targetCharacter == null || !((targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= BiteDist * BiteDist) || !(Vector2.Dot(targetCharacter.PosXZ - character.PosXZ, character.Forward) >= 0f) || targetCharacter.IsBeingBittenOrChoked() || targetCharacter.IsDodging() || !targetCharacter.IsAwake)
		{
			return;
		}
		if (targetCharacter.CurrentActionAnim == ActionAnim.AttackJumpingZombie && targetCharacter.EquippedItem != null && targetCharacter.IsFacing(character.PosXZ, MathF.PI / 4f))
		{
			if (character.IsPredicted() == targetCharacter.IsPredicted() && targetCharacter.CanAttackJumpingZombie != CanAttackState.HasAttacked)
			{
				targetCharacter.CanAttackJumpingZombie = CanAttackState.CanAttack;
			}
		}
		else
		{
			Success = true;
			Finished = true;
		}
	}

	public override void OnActionAnimFinished(Character character, Goal parent, ActionAnim anim)
	{
		base.OnActionAnimFinished(character, parent, anim);
		character.Ragdollify(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, fromStumble: true, retainVelocity: true);
	}
}

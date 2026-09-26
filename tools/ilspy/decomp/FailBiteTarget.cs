using UnityEngine;

public class FailBiteTarget : AnimationGoal
{
	private Vector3 StartPos;

	private bool FromJumping;

	public FailBiteTarget()
	{
	}

	public FailBiteTarget(bool fromJumping)
		: base(fromJumping ? ActionAnim.ZombieJumpBiteFail : ActionAnim.ZombieBiteFail)
	{
		FromJumping = fromJumping;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartPos);
		reflector.Add(ref FromJumping);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FailBiteTarget;
	}

	public override bool WantBailOnFail()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		StartPos = character.Position;
		base.OnActivate(character, parent);
	}

	public override bool WantFaceTarget(Character character)
	{
		return false;
	}

	public override void OnActionAnimFinished(Character character, Goal parent, ActionAnim anim)
	{
		base.OnActionAnimFinished(character, parent, anim);
		if (FromJumping)
		{
			character.Ragdollify(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, fromStumble: true, retainVelocity: true);
		}
	}
}

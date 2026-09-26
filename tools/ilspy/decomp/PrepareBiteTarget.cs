using UnityEngine;

public class PrepareBiteTarget : AnimationGoal
{
	public Vector3 StartPos;

	public bool FromJumping;

	public PrepareBiteTarget()
	{
	}

	public PrepareBiteTarget(Vector3 startPos, bool fromJumping)
		: base(fromJumping ? ActionAnim.ZombieJumpBitePrepare : ActionAnim.ZombieBitePrepare)
	{
		StartPos = startPos;
		FromJumping = fromJumping;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FromJumping);
		reflector.Add(ref StartPos);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.PrepareBiteTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		character.SetPosition(BiteTarget.GetPosForBitingAnimation(character, GetTargetCharacter(), FromJumping, StartPos));
	}

	public override bool WantFaceTarget(Character character)
	{
		if (FromJumping)
		{
			return false;
		}
		return base.WantFaceTarget(character);
	}
}

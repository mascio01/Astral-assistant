using UnityEngine;

public class FinishBiteTarget : AnimationGoal
{
	private Vector3 StartPos;

	private bool FromJumping;

	public FinishBiteTarget()
	{
	}

	public FinishBiteTarget(bool fromJumping)
		: base(fromJumping ? ActionAnim.ZombieJumpBiteFinish : ActionAnim.ZombieBiteFinish)
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
		return GoalType.FinishBiteTarget;
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

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		character.SetPosition(Vector3.Lerp(StartPos, GameTerrain.Instance.ClampPosToSurface(character.Position), character.GetActionAnimPlayedFrac()));
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

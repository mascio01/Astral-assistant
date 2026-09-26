using UnityEngine;

public class LoopChokeHold : AnimationGoal
{
	public Vector3 StartPos;

	public static float ChokeDist = 0.35f;

	public LoopChokeHold()
	{
	}

	public LoopChokeHold(HoldType holdType, Vector3 startPos)
		: base(holdType switch
		{
			HoldType.SlitThroat => ActionAnim.SlitThroatLoop, 
			HoldType.Restrain => ActionAnim.RestrainLoop, 
			_ => ActionAnim.ChokeHoldLoop, 
		})
	{
		StartPos = startPos;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartPos);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LoopChokeHold;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
	}

	public static Vector3 GetPosForChokeHoldAnimation(Character character, Character targetCharacter, Vector3 startPos)
	{
		Vector3 vector = MathUtil.ToX0Y(MathUtil.SafeNormalize(MathUtil.ToXZ(startPos) - targetCharacter.PosXZ, -MathUtil.GetDirFromAngle(character.DesiredFacingAngle)));
		return targetCharacter.Position + vector * ChokeDist;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Anim != ActionAnim.RestrainLoop)
		{
			character.SetPosition(GetPosForChokeHoldAnimation(character, GetTargetCharacter(), StartPos));
		}
	}
}

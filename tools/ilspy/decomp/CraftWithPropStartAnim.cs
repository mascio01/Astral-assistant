using UnityEngine;

public class CraftWithPropStartAnim : AnimationGoal
{
	public static float PotStirDist = 0.7f;

	public CraftWithPropStartAnim()
	{
	}

	public CraftWithPropStartAnim(Character character, TileObject targetObj, ActionAnim anim)
		: base(character, targetObj, anim, 1f)
	{
	}

	public CraftWithPropStartAnim(Character character, TileObject targetObj, ActionAnim anim, float animSpeed)
		: base(character, targetObj, anim, animSpeed)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.CraftWithPropStartAnim;
	}

	public override bool WantPosInterpolation(Character character, out Vector3 destPos)
	{
		if (GetTargetObject() is CraftingProp craftingProp)
		{
			Vector3 vector = MathUtil.ToX0Y(MathUtil.SafeNormalize(MathUtil.ToXZ(PosWhenAnimStarted) - craftingProp.PosXZ, -MathUtil.GetDirFromAngle(character.DesiredFacingAngle)));
			destPos = craftingProp.Pos + vector * PotStirDist;
		}
		else
		{
			destPos = character.Pos;
		}
		return true;
	}
}

using UnityEngine;

public class FinishHugTarget : AnimationGoal
{
	private Vector3 _startPos;

	public FinishHugTarget()
		: base(ActionAnim.HugFinish)
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _startPos);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FinishHugTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		_startPos = character.Position;
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		character.SetPosition(Vector3.Lerp(_startPos, GameTerrain.Instance.ClampPosToSurface(character.Position), character.GetActionAnimPlayedFrac()));
	}
}

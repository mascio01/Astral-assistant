using UnityEngine;

public class FinishChokeHold : AnimationGoal
{
	private Vector3 StartPos;

	public FinishChokeHold()
	{
	}

	public FinishChokeHold(HoldType holdType)
		: base(holdType switch
		{
			HoldType.SlitThroat => ActionAnim.SlitThroatFinish, 
			HoldType.Restrain => ActionAnim.RestrainFinish, 
			_ => ActionAnim.ChokeHoldFinish, 
		})
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartPos);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FinishChokeHold;
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
}

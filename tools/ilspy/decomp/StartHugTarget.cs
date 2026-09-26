using UnityEngine;

public class StartHugTarget : AnimationGoal
{
	private Vector3 _startPos;

	public bool TriggeredHuggee;

	public StartHugTarget()
		: base(ActionAnim.HugStart)
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _startPos);
		reflector.AddAfter(ref TriggeredHuggee, 123);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.StartHugTarget;
	}

	public override bool WantStopAnimOnExit()
	{
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		_startPos = character.Position;
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		float num = ((AnimState == SpeechAnimState.Finished) ? 1f : character.GetActionAnimPlayedFrac());
		character.SetPosition(_startPos * (1f - num) + HugTarget.GetPosForHuggingAnimation(character, targetCharacter) * num);
		if (!targetCharacter.IsAwake)
		{
			Finished = true;
		}
		else if (!TriggeredHuggee && num >= 0.5f)
		{
			TriggeredHuggee = true;
			if (character.IsPredicted() == targetCharacter.IsPredicted() && !targetCharacter.OnHugStarted(character))
			{
				Finished = true;
			}
		}
	}
}

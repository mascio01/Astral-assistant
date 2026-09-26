using System;

public class RepairArmorAnim : AnimationGoal
{
	public TimeSpan StartTime;

	public RepairArmorAnim()
		: base(ActionAnim.CraftTableLoop)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.RepairArmorAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartTime);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTime = Session.Instance.PlayTime;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (AnimState == SpeechAnimState.Started && character.GetActionAnimTime() >= character.GetActionAnimDuration())
		{
			Finished = true;
		}
	}
}

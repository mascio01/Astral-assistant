using UnityEngine;

public class AttackAnim : AnimationGoal
{
	private bool Assassinate;

	private SecrecyMode Secret;

	public bool Success;

	public AttackAnim()
		: base(ActionAnim.Fire)
	{
	}

	public AttackAnim(bool assassinate, SecrecyMode secret)
		: base(ActionAnim.Fire)
	{
		Assassinate = assassinate;
		Secret = secret;
		Aiming = true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Anim = character.GetFireAction(out AnimSpeed);
		base.OnActivate(character, parent);
		Target.ClearInaccessible();
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Assassinate);
		reflector.Add(ref Secret);
		reflector.AddAfter(ref Success, 61);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AttackAnim;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Fire)
		{
			RangedWeapon rangedWeapon = character.EquippedItem as RangedWeapon;
			TargettableBodyLocation currentTargetBodyLocation = character.GetCurrentTargetBodyLocation();
			if (rangedWeapon != null)
			{
				Success = rangedWeapon.OnFired(character, Target, currentTargetBodyLocation, Vector3.zero, 0f, 0f, Assassinate, Secret, fromAI: true);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

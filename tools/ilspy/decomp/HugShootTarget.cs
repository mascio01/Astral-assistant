using UnityEngine;

public class HugShootTarget : AnimationGoal
{
	private Vector3 _startPos;

	public bool Success;

	public HugShootTarget()
		: base(ActionAnim.HugShoot)
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _startPos);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.HugShootTarget;
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

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.Equip:
			character.EquippedItem = (character.DesiredEquippedItem = character.Inventory.FindItemOfType(EquipmentPrototype.Pistol));
			return true;
		case AnimationEventType.Fire:
			if (character.EquippedItem is AmmoWeapon ammoWeapon)
			{
				ammoWeapon.OnFired(character, Target, TargettableBodyLocation.Torso, Vector3.zero, 0f, 0f, assassinate: true, SecrecyMode.Public, fromAI: true);
			}
			Success = true;
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}
}

using System;

public class ReloadAnim : AnimationGoal
{
	public EquipmentPrototype AmmoType;

	public InfectionType InfectedWith;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref AmmoType, 433);
		reflector.AddAfter(ref InfectedWith, 450);
	}

	public ReloadAnim()
		: base(ActionAnim.Reload)
	{
	}

	public ReloadAnim(bool aiming)
		: base(ActionAnim.Reload)
	{
		Aiming = aiming;
	}

	public ReloadAnim(bool aiming, bool crouching)
		: base(ActionAnim.Reload)
	{
		Aiming = aiming;
		Crouching = crouching;
	}

	public ReloadAnim(bool aiming, bool crouching, EquipmentPrototype ammoType, InfectionType infectedWith)
		: base(ActionAnim.Reload)
	{
		Aiming = aiming;
		Crouching = crouching;
		AmmoType = ammoType;
		InfectedWith = infectedWith;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		AnimSpeed = (character.EquippedItem as AmmoWeapon)?.GetReloadSpeed(character) ?? 1f;
		base.OnActivate(character, parent);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ReloadAnim;
	}

	public override bool WantBailOnFail()
	{
		return true;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (AmmoType != null && animEvent.EventType == AnimationEventType.Reload)
		{
			if (character.IsAuthoritative() && character.EquippedItem is AmmoWeapon ammoWeapon)
			{
				int num = ((ammoWeapon.CurrentAmmoType != AmmoType) ? ammoWeapon.GetMaxAmmo() : (ammoWeapon.GetMaxAmmo() - ammoWeapon.CurrentAmmo));
				if (num > 0)
				{
					if (character.HasInfiniteAmmo(ammoWeapon))
					{
						InfectionType infectionType = InfectionType.None;
						if (ammoWeapon.CurrentAmmoType == AmmoType)
						{
							infectionType = (InfectionType)Math.Max((int)infectionType, (int)ammoWeapon.InfectedWith);
						}
						Equipment equipment = character.Inventory.FindItemOfType(AmmoType);
						if (equipment != null)
						{
							infectionType = (InfectionType)Math.Max((int)infectionType, (int)equipment.InfectedWith);
						}
						ammoWeapon.OnReload(character, num, AmmoType, infectionType);
					}
					else
					{
						Equipment equipment2 = character.Inventory.TakeAmmoOfType(character, AmmoType, InfectedWith, num);
						if (equipment2 != null)
						{
							ammoWeapon.OnReload(character, equipment2.GetAmount(), AmmoType, equipment2.InfectedWith);
							equipment2.Delete();
						}
					}
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

public class UnequipAnim : AnimationGoal
{
	public bool IsSettingDesiredEquipment;

	public UnequipAnim()
		: base(ActionAnim.Unequip)
	{
	}

	public UnequipAnim(bool isSettingDesiredEquipment)
		: base(ActionAnim.Unequip)
	{
		IsSettingDesiredEquipment = isSettingDesiredEquipment;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.UnequipAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref IsSettingDesiredEquipment, 172);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.EquippedItem == null || character.EquippedItem.GetEquippedModel() == null)
		{
			character.EquippedItem = null;
			Finished = true;
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Unequip)
		{
			if (character.EquippedItem is Gun && character.CheckFrontmostPrediction(PredictedEventType.GunUnequipSound))
			{
				character.PlaySoundOneShot(SoundManager.GunUnequipSound);
			}
			character.EquippedItem = null;
			if (!IsSettingDesiredEquipment)
			{
				character.DesiredEquippedItem = null;
			}
			SetAiming(character, parent, aiming: false);
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

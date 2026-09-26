public class EquipAnim : AnimationGoal
{
	public Equipment Equipment;

	public bool IsSettingDesiredEquipment;

	public EquipAnim()
	{
	}

	public EquipAnim(Equipment equipment, bool isSettingDesiredEquipment)
		: base(equipment?.GetEquipAction() ?? ActionAnim.EquipUnarmed)
	{
		Equipment = equipment;
		IsSettingDesiredEquipment = isSettingDesiredEquipment;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Equipment);
		reflector.AddAfter(ref IsSettingDesiredEquipment, 172);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.EquipAnim;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Equip)
		{
			character.EquippedItem = Equipment;
			if (!IsSettingDesiredEquipment)
			{
				character.DesiredEquippedItem = Equipment;
			}
			if (Equipment is Gun && character.CheckFrontmostPrediction(PredictedEventType.GunCockSound))
			{
				character.PlaySoundOneShot(SoundManager.GunCockSound);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

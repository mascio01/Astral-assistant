public class AddMaterialToFireAnim : AnimationGoal
{
	public Equipment Item;

	public AddMaterialToFireAnim()
	{
	}

	public AddMaterialToFireAnim(Character character, TileObject targetObj, Equipment item)
		: base(character, targetObj, ActionAnim.AddMaterialToFire)
	{
		Item = item;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AddMaterialToFireAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Item);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.AddMaterialToFire && GetTargetObject() is Campfire { WoodRemaining: <1f } campfire)
		{
			int num = character.Inventory.UseItem(character, Item, 1);
			campfire.AddWood(character, num);
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

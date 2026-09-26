public class FillLiquidContainerFromWellAnim : AnimationGoal
{
	public bool Success;

	public FillLiquidContainerFromWellAnim()
	{
	}

	public FillLiquidContainerFromWellAnim(Character character, TileObject obj)
		: base(ActionAnim.FillLiquidContainerFromWell)
	{
		SetTarget(character, null, character.GetOrCreateTarget(obj));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FillLiquidContainerFromWellAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref Success, 65);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FillLiquidContainer)
		{
			Equipment equippedItem = character.EquippedItem;
			if (equippedItem != null && equippedItem.GetLiquidCapacity() > 0f)
			{
				if (character.IsAuthoritative())
				{
					Prop targetProp = GetTargetProp();
					if (targetProp != null && targetProp.GetLiquidType() != null && targetProp.GetLiquidCapacity() > 0f)
					{
						float maxAmount = targetProp.ExtractLiquid(equippedItem.GetLiquidCapacity() - equippedItem.GetLiquidContentsAmount());
						equippedItem.FillLiquid(targetProp.GetLiquidType(), maxAmount, InfectionType.None);
					}
					else
					{
						equippedItem.FillLiquid(LiquidPrototype.Water, equippedItem.GetLiquidCapacity(), InfectionType.None);
					}
				}
				Success = true;
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

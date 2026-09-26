public class FillLiquidContainerFromRiverAnim : AnimationGoal
{
	public bool Success;

	public TerrainCoord Tile;

	public FillLiquidContainerFromRiverAnim()
	{
	}

	public FillLiquidContainerFromRiverAnim(TerrainCoord tile)
		: base(ActionAnim.FillLiquidContainerFromRiver)
	{
		Tile = tile;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FillLiquidContainerFromRiverAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref Success, 65);
		reflector.AddAfter(ref Tile, 65);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FillLiquidContainer)
		{
			Equipment equippedItem = character.EquippedItem;
			if (equippedItem != null && equippedItem.GetLiquidCapacity() > 0f)
			{
				if (GameTerrain.Instance.IsTileRiver(Tile.x, Tile.y))
				{
					if (character.IsAuthoritative())
					{
						equippedItem.FillLiquid(LiquidPrototype.Water, equippedItem.GetLiquidCapacity(), InfectionType.None);
					}
					Success = true;
				}
				else if (Session.Instance.Weather.SnowOnGroundAmount >= Weather.ScoopableSnowOnGroundAmount)
				{
					if (character.IsAuthoritative())
					{
						equippedItem.FillLiquid(LiquidPrototype.Snow, equippedItem.GetLiquidCapacity(), InfectionType.None);
					}
					Success = true;
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

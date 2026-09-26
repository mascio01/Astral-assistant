public class HarvestCropsAnim : AnimationGoal
{
	private TerrainCoord _tile;

	private bool _ignoreWeight;

	private bool IsGathering;

	public bool Success;

	public HarvestCropsAnim()
	{
	}

	public HarvestCropsAnim(TerrainCoord tile, bool ignoreWeight, bool isGathering)
		: base(ActionAnim.HarvestCrops)
	{
		_tile = tile;
		_ignoreWeight = ignoreWeight;
		IsGathering = isGathering;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _tile);
		reflector.AddAfter(ref _ignoreWeight, 4);
		reflector.AddAfter(ref IsGathering, 353);
		reflector.AddAfter(ref Success, 74);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.HarvestCropsAnim;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (GameTerrain.Instance.GetPlant(_tile.x, _tile.y) == null && !Success)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.HarvestCrops)
		{
			PlantableCrop plant = GameTerrain.Instance.GetPlant(_tile.x, _tile.y);
			if (plant != null)
			{
				Community community = plant.GetCommunity();
				character.PlaySoundUsingFootstepAudioSourceFromList(SoundManager.HarvestingSounds, 1f);
				if (plant.Harvest(character, _ignoreWeight, IsGathering, out var price))
				{
					character.OnStoleSomething(community, null, price);
					Success = true;
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

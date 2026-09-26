public class PlantCropsAnim : AnimationGoal
{
	private TerrainCoord _tile;

	private Equipment Seeds;

	public bool Success;

	public PlantCropsAnim()
	{
	}

	public PlantCropsAnim(TerrainCoord tile, Equipment seeds)
		: base(ActionAnim.PlantCrops)
	{
		_tile = tile;
		Seeds = seeds;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _tile);
		reflector.Add(ref Seeds);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.PlantCropsAnim;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.PlantCrops)
		{
			GameTerrain instance = GameTerrain.Instance;
			if (character.EquippedItem == Seeds && Seeds != null && Seeds.GetSeedForPlantType() != null && GameTerrain.CanPlantCropsOnTerrainType(instance.GetTileTerrainType(_tile)) && !instance.IsImpassable(_tile.x, _tile.y, 68, character, null))
			{
				Seeds.IncrementAmount(-1);
				character.Inventory.CacheEncumbered(character);
				if (Seeds.GetAmount() == 0)
				{
					character.Inventory.Remove(character, Seeds);
					Seeds.Delete();
				}
				TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(_tile.x, _tile.y);
				if (fixedObjectOnTile != null && fixedObjectOnTile.CanBeClearedForBuilding(character.Community, null))
				{
					fixedObjectOnTile.Delete();
				}
				int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Farming);
				PlantableCrop.Spawn(Seeds.GetSeedForPlantType(), _tile, 0f, character.Community, skillLevelWithEffects);
				character.Skillset.AddProgress(character, SkillType.Farming, 2f);
				character.PlaySoundUsingFootstepAudioSourceFromList(SoundManager.PlantingSounds, 1f);
				instance.ClearAllGrassInRect(_tile, _tile);
				instance.BuildMinimap(_tile, _tile);
				Success = true;
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

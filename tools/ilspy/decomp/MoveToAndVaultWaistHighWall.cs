public class MoveToAndVaultWaistHighWall : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public TerrainCoord PropTile;

	private static TerrainCoord[] AdjacentTiles = new TerrainCoord[4]
	{
		new TerrainCoord(1, 0),
		new TerrainCoord(-1, 0),
		new TerrainCoord(0, 1),
		new TerrainCoord(0, -1)
	};

	public MoveToAndVaultWaistHighWall()
	{
	}

	public MoveToAndVaultWaistHighWall(Character character, TileObject targetObj, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndVaultWaistHighWall;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (SubGoal is VaultWaistHighWall)
		{
			return SubGoal.AIOverridesControl(character, this);
		}
		return AIOverridesControlReason.Animation;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.Add(ref PropTile);
	}

	public static bool CanVaultOverProp(Character controlledCharacter, TileObject prop, out TerrainCoord startTile, out TerrainCoord propTile)
	{
		propTile = prop.GetNearestTileTo(controlledCharacter.Tile);
		TerrainCoord terrainCoord = (startTile = controlledCharacter.Tile);
		if (propTile == terrainCoord)
		{
			return false;
		}
		if (!startTile.IsAdjacent(propTile))
		{
			float num = float.MaxValue;
			for (int i = 0; i < AdjacentTiles.Length; i++)
			{
				float distSquared = terrainCoord.GetDistSquared(propTile + AdjacentTiles[i]);
				if (distSquared < num)
				{
					num = distSquared;
					startTile = propTile + AdjacentTiles[i];
				}
			}
		}
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord terrainCoord2 = startTile;
		TerrainCoord terrainCoord3 = propTile - startTile;
		while (true)
		{
			if (terrainCoord2 != terrainCoord && instance.IsImpassable(terrainCoord2.x, terrainCoord2.y, 2561, controlledCharacter, prop))
			{
				return false;
			}
			if (terrainCoord2 != startTile)
			{
				TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(terrainCoord2.x, terrainCoord2.y);
				if (fixedObjectOnTile == null || fixedObjectOnTile.CoverType != CoverType.WaistHigh)
				{
					break;
				}
			}
			terrainCoord2 += terrainCoord3;
		}
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		TileObject targetObject = GetTargetObject();
		if (targetObject == null || targetObject.CoverType != CoverType.WaistHigh)
		{
			Finished = true;
			Success = true;
			return;
		}
		if (!CanVaultOverProp(character, targetObject, out var startTile, out PropTile))
		{
			Finished = true;
			Success = true;
			return;
		}
		character.GoalTarget = Target;
		MoveTo moveTo = new MoveTo(MovementType, startTile);
		moveTo._dontOpenOurGates = DontOpenOurGates;
		moveTo.MoveToCentreOfTile = true;
		SetSubGoal(character, parent, moveTo);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false } && GetTargetObject() != null)
		{
			return new TurnTo(PropTile);
		}
		if (SubGoal is TurnTo)
		{
			TileObject targetObject = GetTargetObject();
			if (targetObject != null && !IsTargetDeleted() && targetObject.CoverType == CoverType.WaistHigh)
			{
				return new VaultWaistHighWall();
			}
		}
		if (SubGoal is VaultWaistHighWall)
		{
			Success = true;
		}
		return null;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}
}

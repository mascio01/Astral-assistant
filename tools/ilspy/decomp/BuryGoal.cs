public class BuryGoal : StateMachineGoal
{
	public Character Corpse;

	public BuryGoal()
	{
	}

	public BuryGoal(Character corpse)
	{
		Corpse = corpse;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BuryGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Corpse);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Corpse == null || Corpse.Deleted || Corpse.Alive)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.Community == null || Corpse == null || Corpse.Deleted || Corpse.Alive)
		{
			Finished = true;
			return;
		}
		if (Corpse.Disappeared)
		{
			SetSubGoal(character, parent, new MoveAsCloseAsPossibleToTarget(character, Corpse, MovementType.Walk));
			return;
		}
		Grave grave = FindBestGrave(character, includeUnderConstruction: true);
		if (grave == null || grave.GetUnderConstructionInfo() != null)
		{
			SetSubGoal(character, parent, new FindGoal(FindType.Shovel, MovementType.Walk, critical: false));
		}
		else if (Corpse.Inventory.FindLightestTakeableItem(Corpse) != null)
		{
			SetSubGoal(character, parent, new MoveToAndTakeAll(character, Corpse, MovementType.Walk));
		}
		else
		{
			SetSubGoal(character, parent, new MoveToAndPickUp(character, Corpse, MovementType.Walk));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (character.CarryingObject != null)
		{
			character.DropAuthoritative();
		}
		base.OnDeactivate(character, parent);
	}

	private Grave FindBestGrave(Character character, bool includeUnderConstruction)
	{
		Grave result = null;
		float num = float.MaxValue;
		foreach (Prop building in character.Community.Buildings)
		{
			if (building is Grave { GraveState: GraveState.Open } grave && (includeUnderConstruction || grave.GetUnderConstructionInfo() == null))
			{
				float magnitude = (grave.PosXZ - Corpse.PosXZ).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					result = grave;
				}
			}
		}
		return result;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FindGoal { Success: not false })
		{
			Grave grave = FindBestGrave(character, includeUnderConstruction: true);
			if (grave == null)
			{
				CustomRandom deterministicRand = Session.Instance.DeterministicRand;
				Recipe recipe = GameImpl.Instance.FindRecipeByProduct(PropPrototype.Grave, null);
				if (recipe != null)
				{
					bool flag = false;
					TerrainCoord tile = Corpse.Tile;
					Prop.OrientationType orientationType = Prop.OrientationType.Deg0;
					Grave grave2 = new Grave();
					for (int i = 0; i < 100; i++)
					{
						int num = 1 + i / 10;
						tile += deterministicRand.RandomTile(new TerrainCoord(-num, -num), new TerrainCoord(num, num));
						orientationType = (Prop.OrientationType)deterministicRand.Next(4);
						grave2.SetTileGhost(tile);
						grave2.SetOrientationType(orientationType);
						if (GameCursor.CanBuildHere(character, grave2, checkOtherCharacters: true, checkCropPatches: true, character.Community) == CursorActionDisabledReason.Enabled && !GameTerrain.Instance.HasObstructions(tile, 2, ignoreIfPassable: false) && !GameTerrain.Instance.IsTileEnclosed(tile.x, tile.y) && GameTerrain.Instance.IsTileSpawnable(tile.x, tile.y))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						grave = Session.Instance.PlaceBuildingDuringGameplay(character, recipe, tile, orientationType) as Grave;
					}
				}
			}
			if (grave != null)
			{
				if (grave.GetUnderConstructionInfo() != null)
				{
					BuildGoal buildGoal = new BuildGoal();
					buildGoal.SetCurrentBuilding(character, this, grave);
					return buildGoal;
				}
				if (Corpse.Inventory.FindLightestTakeableItem(Corpse) != null)
				{
					return new MoveToAndTakeAll(character, Corpse, MovementType.Walk);
				}
				return new MoveToAndPickUp(character, Corpse, MovementType.Walk);
			}
		}
		if (SubGoal is BuildGoal buildGoal2 && buildGoal2.WasSuccessful())
		{
			if (Corpse.Inventory.FindLightestTakeableItem(Corpse) != null)
			{
				return new MoveToAndTakeAll(character, Corpse, MovementType.Walk);
			}
			return new MoveToAndPickUp(character, Corpse, MovementType.Walk);
		}
		if (SubGoal is MoveToAndTakeAll)
		{
			return new MoveToAndPickUp(character, Corpse, MovementType.Walk);
		}
		if (SubGoal is MoveToAndPickUp { Success: not false } && character.CarryingObject == Corpse)
		{
			Grave grave3 = FindBestGrave(character, includeUnderConstruction: false);
			if (grave3 != null)
			{
				return new MoveToAndInteractGoal(character, grave3, InteractionType.Bury, Corpse);
			}
		}
		MoveToAndInteractGoal moveToAndInteractGoal = SubGoal as MoveToAndInteractGoal;
		MoveAsCloseAsPossibleToTarget moveAsCloseAsPossibleToTarget = SubGoal as MoveAsCloseAsPossibleToTarget;
		if ((moveToAndInteractGoal != null && moveToAndInteractGoal.Success) || (moveAsCloseAsPossibleToTarget != null && moveAsCloseAsPossibleToTarget.Success))
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, Corpse, null, SpeechSituation.Eulogy);
			if (speechForSituation != null)
			{
				return new Conversation(character, Corpse, null, speechForSituation, controlledByPlayer: false);
			}
		}
		character.Community.AddFailedToBury(Corpse);
		return null;
	}
}

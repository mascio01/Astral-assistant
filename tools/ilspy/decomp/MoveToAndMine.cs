using System.Collections.Generic;

public class MoveToAndMine : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool IsGathering;

	public TerrainCoord MineTile = TerrainCoord.Invalid;

	public MineralType MineralType = MineralType.None;

	public int EntranceIndex = -1;

	public Equipment PreviouslyEquipped;

	public static int ProgressNeededToExtractRichDeposits = 1;

	public static int ProgressNeededToExtractPoorDeposits = 120;

	public MoveToAndMine()
	{
	}

	public MoveToAndMine(Character character, TileObject targetObj, MineralType mineralType, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		MineralType = mineralType;
	}

	public MoveToAndMine(Character character, TileObject targetObj, MineralType mineralType, MovementType movementType, bool dontOpenOurGates)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		MineralType = mineralType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndMine;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref IsGathering, 353);
		reflector.Add(ref MineTile);
		reflector.AddAfter(ref MineralType, 178);
		reflector.AddAfter(ref EntranceIndex, 215);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active)
		{
			if (SubGoal is AnimationGoal animationGoal && (animationGoal.GetAnim() == ActionAnim.MineRockEnd || animationGoal.GetAnim() == ActionAnim.MineBoulderEnd))
			{
				return true;
			}
			if (SubGoal is Equip && Success)
			{
				return true;
			}
		}
		if (character.Inventory.GetPickaxe() == null)
		{
			return false;
		}
		return !IsTargetDeleted();
	}

	private void CheckTile(Character character, TerrainCoord tile, TerrainCoord targetTile, List<TerrainCoord> candidates, List<TerrainCoord> targetTiles)
	{
		if (!GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2049, character, null) && (GameTerrain.Instance.IsTileSpawnable(tile.x, tile.y) || GameTerrain.Instance.IsTileEnclosed(tile.x, tile.y)))
		{
			candidates.Add(tile);
			targetTiles.Add(targetTile);
		}
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		TileObject targetObject = GetTargetObject();
		if (targetObject != null)
		{
			if (targetObject is Mine)
			{
				SetSubGoal(character, parent, new MoveToAndEnterBuilding(EntranceIndex, MovementType, DontOpenOurGates));
				return;
			}
			List<TerrainCoord> list = new List<TerrainCoord>();
			List<TerrainCoord> list2 = new List<TerrainCoord>();
			for (int i = targetObject.GetMinTile().x; i <= targetObject.GetMaxTile().x; i++)
			{
				CheckTile(character, new TerrainCoord(i, targetObject.GetMinTile().y - 1), new TerrainCoord(i, targetObject.GetMinTile().y), list, list2);
			}
			for (int j = targetObject.GetMinTile().x; j <= targetObject.GetMaxTile().x; j++)
			{
				CheckTile(character, new TerrainCoord(j, targetObject.GetMaxTile().y + 1), new TerrainCoord(j, targetObject.GetMaxTile().y), list, list2);
			}
			for (int k = targetObject.GetMinTile().y; k <= targetObject.GetMaxTile().y; k++)
			{
				CheckTile(character, new TerrainCoord(targetObject.GetMinTile().x - 1, k), new TerrainCoord(targetObject.GetMinTile().x, k), list, list2);
			}
			for (int l = targetObject.GetMinTile().y; l <= targetObject.GetMaxTile().y; l++)
			{
				CheckTile(character, new TerrainCoord(targetObject.GetMaxTile().x + 1, l), new TerrainCoord(targetObject.GetMaxTile().x, l), list, list2);
			}
			if (list.Count > 0)
			{
				int index = MathUtil.RandomInt((int)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).Ticks, list.Count);
				MineTile = list2[index];
				TerrainCoord destTile = list[index];
				MoveTo moveTo = new MoveTo(MovementType, destTile, DontOpenOurGates);
				moveTo.MoveToCentreOfTile = true;
				SetSubGoal(character, parent, moveTo);
			}
			else
			{
				Finished = true;
				Success = false;
			}
		}
		else
		{
			Finished = true;
			Success = false;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		AnimationGoal animationGoal = SubGoal as AnimationGoal;
		if (SubGoal is Equip && Success)
		{
			return null;
		}
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.MineRockEnd || animationGoal.GetAnim() == ActionAnim.MineBoulderEnd))
		{
			Success = true;
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
			{
				return new Equip(PreviouslyEquipped);
			}
			return null;
		}
		TileObject targetObject = GetTargetObject();
		if (targetObject != null && (targetObject.GetMiningResourceRemaining(MineralType) > 0 || targetObject is Mine))
		{
			MoveTo moveTo = SubGoal as MoveTo;
			MoveToAndEnterBuilding moveToAndEnterBuilding = SubGoal as MoveToAndEnterBuilding;
			if (moveToAndEnterBuilding != null)
			{
				EntranceIndex = moveToAndEnterBuilding.EntranceIndex;
			}
			if ((moveTo != null && moveTo.Success) || (moveToAndEnterBuilding != null && moveToAndEnterBuilding.Success))
			{
				character.IssueCommandToFollowers(FollowCommand.MineRock, targetObject.GetCentreTile(), EquipmentPrototype.MiningResources[(int)MineralType]);
				if (!(character.EquippedItem is Pickaxe))
				{
					Pickaxe pickaxe = character.Inventory.GetPickaxe();
					if (pickaxe == null)
					{
						return null;
					}
					PreviouslyEquipped = character.EquippedItem;
					return new Equip(pickaxe);
				}
				if (moveToAndEnterBuilding != null)
				{
					return new AnimationGoal(ActionAnim.MineBoulderStart, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new TurnTo(MineTile);
			}
			if (SubGoal is Equip)
			{
				return new TurnTo(MineTile);
			}
			if (SubGoal is TurnTo)
			{
				return new AnimationGoal((targetObject is Rock) ? ActionAnim.MineRockStart : ActionAnim.MineBoulderStart, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
			if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.MineRockStart || animationGoal.GetAnim() == ActionAnim.MineBoulderStart || animationGoal.GetAnim() == ActionAnim.MineRockLoop || animationGoal.GetAnim() == ActionAnim.MineBoulderLoop))
			{
				if (targetObject.GetMiningProgress(MineralType) < targetObject.GetMiningProgressNeededToExtract(MineralType))
				{
					if (parent is FollowGoal && character.SquadLeader != null && character.SquadLeader.FindActiveGoal(GoalType.MoveToAndMine) == null)
					{
						return null;
					}
					return new AnimationGoal((targetObject is Rock) ? ActionAnim.MineRockLoop : ActionAnim.MineBoulderLoop, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new AnimationGoal((targetObject is Rock) ? ActionAnim.MineRockEnd : ActionAnim.MineBoulderEnd, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		return null;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		if (SubGoal is AnimationGoal animationGoal)
		{
			animationGoal.SetAnimSpeed(character, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.BreakRock:
		{
			if (!character.IsAuthoritative())
			{
				break;
			}
			TileObject targetObject2 = GetTargetObject();
			if (targetObject2 != null)
			{
				character.RemoveFailedFindAttempt(targetObject2);
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 16f, 16f, character, character, null, null));
				int num = 1;
				targetObject2.IncrementMiningProgress(MineralType, num);
				character.Skillset.AddProgress(character, SkillType.Strength, num);
				Community communityThatOwnsThisArea2 = targetObject2.GetCommunityThatOwnsThisArea();
				if (communityThatOwnsThisArea2 != null && (!communityThatOwnsThisArea2.GrantedMiningRightsToPlayer || !character.IsControllableByPlayer()) && communityThatOwnsThisArea2.GetRelationship(character.Community) != CommunityRelationshipType.Allied)
				{
					character.OnStoleSomething(communityThatOwnsThisArea2, null, 0f);
				}
			}
			break;
		}
		case AnimationEventType.BreakRockFinish:
		{
			if (!character.IsAuthoritative())
			{
				break;
			}
			TileObject targetObject = GetTargetObject();
			if (targetObject == null || !targetObject.Mine(MineralType))
			{
				break;
			}
			character.RemoveFailedFindAttempt(targetObject);
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 16f, 16f, character, character, null, null));
			EquipmentPrototype equipmentPrototype = ((MineralType != MineralType.None) ? EquipmentPrototype.MiningResources[(int)MineralType] : null);
			if (equipmentPrototype != null)
			{
				Equipment equipment = character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype));
				if (IsGathering)
				{
					equipment.IncrementGatheredAmount(1);
				}
				NotificationManager.Instance.AddEquipmentNotification(null, character, equipment, 1);
				character.Skillset.AddProgress(character, SkillType.Construction, 1f);
				Community communityThatOwnsThisArea = targetObject.GetCommunityThatOwnsThisArea();
				if (communityThatOwnsThisArea != null && (!communityThatOwnsThisArea.GrantedMiningRightsToPlayer || !character.IsControllableByPlayer()) && communityThatOwnsThisArea.GetRelationship(character.Community) != CommunityRelationshipType.Allied)
				{
					character.OnStoleSomething(communityThatOwnsThisArea, null, equipmentPrototype.BasePrice);
				}
			}
			break;
		}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

using System.Collections.Generic;

public class MoveToAndChop : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool WantClearStump;

	public bool IsGathering;

	public TerrainCoord ChopTile = TerrainCoord.Invalid;

	public Equipment PreviouslyEquipped;

	public MoveToAndChop()
	{
	}

	public MoveToAndChop(Character character, TileObject targetObj, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public MoveToAndChop(Character character, TileObject targetObj, MovementType movementType, bool dontOpenOurGates)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndChop;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref WantClearStump, 282);
		reflector.AddAfter(ref IsGathering, 353);
		reflector.Add(ref ChopTile);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active)
		{
			if (SubGoal is AnimationGoal animationGoal && (animationGoal.GetAnim() == ActionAnim.ChopLogEnd || animationGoal.GetAnim() == ActionAnim.ChopTreeEnd))
			{
				return true;
			}
			if (SubGoal is Equip && Success)
			{
				return true;
			}
		}
		if (character.Inventory.GetAxe() == null)
		{
			return false;
		}
		return !IsTargetDeleted();
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		TreeProp targetTree = GetTargetTree();
		FallenTreeProp targetFallenTree = GetTargetFallenTree();
		Bush targetBush = GetTargetBush();
		WantClearStump = targetFallenTree?.IsStump() ?? false;
		if (targetTree != null || targetBush != null || WantClearStump)
		{
			ChopTile = GetTargetObject().GetCentreTile();
			MoveTo goal = new MoveAdjacentToTarget(MovementType, DontOpenOurGates, canBeOnTile: false);
			SetSubGoal(character, parent, goal);
		}
		else if (targetFallenTree != null)
		{
			Goal moveToFallenTreeGoal = GetMoveToFallenTreeGoal(character, targetFallenTree);
			if (moveToFallenTreeGoal != null)
			{
				SetSubGoal(character, parent, moveToFallenTreeGoal);
				return;
			}
			Finished = true;
			Success = false;
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

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!WantClearStump)
		{
			FallenTreeProp targetFallenTree = GetTargetFallenTree();
			if (targetFallenTree != null && targetFallenTree.IsStump())
			{
				Finished = true;
				Success = true;
			}
		}
	}

	private Goal GetMoveToFallenTreeGoal(Character character, FallenTreeProp fallenTree)
	{
		List<TerrainCoord> list = new List<TerrainCoord>();
		List<TerrainCoord> list2 = new List<TerrainCoord>();
		int num = fallenTree.CalcFallenLength();
		TerrainCoord dirFromOrientationType = Prop.GetDirFromOrientationType(fallenTree.FallDirection);
		TerrainCoord dirFromOrientationType2 = Prop.GetDirFromOrientationType((Prop.OrientationType)((int)(fallenTree.FallDirection - 1 + 4) % 4));
		TerrainCoord dirFromOrientationType3 = Prop.GetDirFromOrientationType((Prop.OrientationType)((int)(fallenTree.FallDirection + 1) % 4));
		for (int i = 1; i < num; i++)
		{
			TerrainCoord terrainCoord = fallenTree.Tile + dirFromOrientationType * i;
			if (!GameTerrain.Instance.IsImpassable(terrainCoord.x + dirFromOrientationType2.x, terrainCoord.y + dirFromOrientationType2.y, 2049, character, fallenTree) && (!character.HasMovementZone() || character.MovementZone.Contains(terrainCoord + dirFromOrientationType2)))
			{
				list.Add(terrainCoord + dirFromOrientationType2);
				list2.Add(terrainCoord);
			}
			if (!GameTerrain.Instance.IsImpassable(terrainCoord.x + dirFromOrientationType3.x, terrainCoord.y + dirFromOrientationType3.y, 2049, character, fallenTree) && (!character.HasMovementZone() || character.MovementZone.Contains(terrainCoord + dirFromOrientationType3)))
			{
				list.Add(terrainCoord + dirFromOrientationType3);
				list2.Add(terrainCoord);
			}
		}
		if (list.Count == 0)
		{
			TerrainCoord nearestPassableTileTo = fallenTree.GetNearestPassableTileTo(character.Tile, 2049, character, fallenTree);
			if (nearestPassableTileTo != TerrainCoord.Invalid)
			{
				TerrainCoord adjacentTileInDir = nearestPassableTileTo.GetAdjacentTileInDir(fallenTree.PosXZ - character.PosXZ);
				list.Add(nearestPassableTileTo);
				list2.Add(adjacentTileInDir);
			}
		}
		if (list.Count > 0)
		{
			int index = MathUtil.RandomInt((int)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).Ticks, list.Count);
			ChopTile = list2[index];
			TerrainCoord destTile = list[index];
			return new MoveTo(MovementType, destTile, DontOpenOurGates)
			{
				MoveToCentreOfTile = true
			};
		}
		return null;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		AnimationGoal animationGoal = SubGoal as AnimationGoal;
		if (SubGoal is Equip && Success)
		{
			return null;
		}
		FallenTreeProp targetFallenTree = GetTargetFallenTree();
		if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.ChopTreeEnd)
		{
			Success = true;
			if (targetFallenTree != null)
			{
				Goal moveToFallenTreeGoal = GetMoveToFallenTreeGoal(character, targetFallenTree);
				if (moveToFallenTreeGoal != null)
				{
					return moveToFallenTreeGoal;
				}
			}
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped) && parent is ObeyLeaderGoal)
			{
				return new Equip(PreviouslyEquipped);
			}
			return null;
		}
		if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.ChopLogEnd)
		{
			Success = true;
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped) && parent is ObeyLeaderGoal)
			{
				return new Equip(PreviouslyEquipped);
			}
			return null;
		}
		TreeProp targetTree = GetTargetTree();
		if (targetTree != null)
		{
			if (SubGoal is MoveTo { Success: not false })
			{
				character.IssueCommandToFollowers(FollowCommand.ChopWood, targetTree.GetCentreTile(), null);
				if (!(character.EquippedItem is Axe))
				{
					Axe axe = character.Inventory.GetAxe();
					if (axe == null)
					{
						return null;
					}
					PreviouslyEquipped = character.EquippedItem;
					return new Equip(axe);
				}
				return new TurnToTarget();
			}
			if (SubGoal is Equip)
			{
				return new TurnToTarget();
			}
			if (SubGoal is TurnToTarget)
			{
				return new AnimationGoal(ActionAnim.ChopTreeStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
			if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.ChopTreeStart || animationGoal.GetAnim() == ActionAnim.ChopTreeLoop))
			{
				if (targetTree.ChoppedDownProgress < targetTree.GetChopsNeededToCutDown())
				{
					if (parent is FollowGoal && character.SquadLeader != null && character.SquadLeader.FindActiveGoal(GoalType.MoveToAndChop) == null)
					{
						return null;
					}
					return new AnimationGoal(ActionAnim.ChopTreeLoop, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new AnimationGoal(ActionAnim.ChopTreeEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		if (targetFallenTree != null && targetFallenTree.WoodRemaining > 0)
		{
			if (SubGoal is MoveTo moveTo2 && (moveTo2.Success || MoveAdjacentToTarget.IsAdjacentToTarget(character, targetFallenTree)))
			{
				character.IssueCommandToFollowers(FollowCommand.ChopWood, targetFallenTree.GetCentreTile(), null);
				if (!(character.EquippedItem is Axe))
				{
					Axe axe2 = character.Inventory.GetAxe();
					if (axe2 == null)
					{
						return null;
					}
					PreviouslyEquipped = character.EquippedItem;
					return new Equip(axe2);
				}
				return new TurnTo(ChopTile);
			}
			if (SubGoal is Equip)
			{
				return new TurnTo(ChopTile);
			}
			if (SubGoal is TurnTo)
			{
				return new AnimationGoal(ActionAnim.ChopLogStart, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
			if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.ChopLogStart || animationGoal.GetAnim() == ActionAnim.ChopLogLoop))
			{
				if (targetFallenTree.ChoppedLogProgress < targetFallenTree.GetChopsNeededToHarvestWood())
				{
					if (parent is FollowGoal && character.SquadLeader != null && character.SquadLeader.FindActiveGoal(GoalType.MoveToAndChop) == null)
					{
						return null;
					}
					return new AnimationGoal(ActionAnim.ChopLogLoop, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new AnimationGoal(ActionAnim.ChopLogEnd, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		Bush targetBush = GetTargetBush();
		if ((targetFallenTree != null && targetFallenTree.IsStump()) || targetBush != null)
		{
			if (SubGoal is MoveTo { Success: not false })
			{
				if (!(character.EquippedItem is Axe))
				{
					Axe axe3 = character.Inventory.GetAxe();
					if (axe3 == null)
					{
						return null;
					}
					PreviouslyEquipped = character.EquippedItem;
					return new Equip(axe3);
				}
				return new TurnTo(ChopTile);
			}
			if (SubGoal is Equip)
			{
				return new TurnTo(ChopTile);
			}
			if (SubGoal is TurnTo)
			{
				return new AnimationGoal(ActionAnim.ChopLogStart, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
			if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.ChopLogStart)
			{
				return new AnimationGoal(ActionAnim.ChopLogLoop, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
			if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.ChopLogLoop)
			{
				return new AnimationGoal(ActionAnim.ChopLogEnd, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
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
		case AnimationEventType.ChopTree:
		{
			if (!character.IsAuthoritative())
			{
				break;
			}
			TreeProp targetTree = GetTargetTree();
			if (targetTree == null)
			{
				break;
			}
			character.RemoveFailedFindAttempt(targetTree);
			if (targetTree.ChoppedDownProgress < targetTree.GetChopsNeededToCutDown())
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 16f, 16f, character, character, null, null));
				targetTree.ChoppedDownProgress++;
				character.Skillset.AddProgress(character, SkillType.Strength, 1f);
			}
			else
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Suspicious, character.Pos, 32f, 32f, character, character, null, null));
				FallenTreeProp fallenTreeProp = FallenTreeProp.Spawn(targetTree);
				fallenTreeProp.StartFalling(MathUtil.SafeNormalize(MathUtil.ToXZ(targetTree.Pos - character.Position), MathUtil.ToXZ(character.Forward)), character);
				Target orCreateTarget = character.GetOrCreateTarget(fallenTreeProp);
				SetTarget(character, parent, orCreateTarget);
				if (parent is ObeyLeaderGoal { Target: not null } obeyLeaderGoal && obeyLeaderGoal.Target.Object == targetTree)
				{
					obeyLeaderGoal.SetTarget(character, null, orCreateTarget);
				}
				targetTree.Delete();
			}
			Community communityThatOwnsThisArea = targetTree.GetCommunityThatOwnsThisArea();
			if (communityThatOwnsThisArea != null && !character.Community.CachedAllies.Contains(communityThatOwnsThisArea))
			{
				character.OnStoleSomething(communityThatOwnsThisArea, null, EquipmentPrototype.Wood.BasePrice);
			}
			break;
		}
		case AnimationEventType.ChopLog:
			if (character.IsAuthoritative())
			{
				GetTargetFallenTree()?.ChopWood(character);
			}
			break;
		case AnimationEventType.ChopLogFinish:
		{
			if (!character.IsAuthoritative())
			{
				break;
			}
			FallenTreeProp targetFallenTree = GetTargetFallenTree();
			Bush targetBush = GetTargetBush();
			if (targetFallenTree != null)
			{
				character.RemoveFailedFindAttempt(targetFallenTree);
				if (WantClearStump)
				{
					SoundManager.PlaySound3D(SoundManager.StumpFallSound, targetFallenTree.Pos);
					targetFallenTree.Delete();
				}
				else
				{
					targetFallenTree.HarvestWood(character, IsGathering);
				}
			}
			else if (targetBush != null)
			{
				character.RemoveFailedFindAttempt(targetBush);
				SoundManager.PlaySound3D(SoundManager.BushFallSound, targetBush.Pos);
				targetBush.Delete();
			}
			break;
		}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

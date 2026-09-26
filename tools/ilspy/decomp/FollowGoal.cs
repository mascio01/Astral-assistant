using System;
using UnityEngine;

public class FollowGoal : StateMachineGoal
{
	public FollowCommand FollowCommand;

	private Character FollowCommandIssuer;

	private TimeSpan FollowCommandIssuedTime = TimeSpan.Zero;

	private TerrainCoord FollowCommandDest = TerrainCoord.Invalid;

	private TileObject FollowCommandTarget;

	public EquipmentPrototype FollowCommandResourceType;

	public LiquidPrototype FollowCommandResourceLiquid;

	public float FollowCommandResourceAmount;

	public TimeSpan ExtraSuperStealthyTime = TimeSpan.Zero;

	public bool WantCatchUpWithPlayer;

	public bool IsObeyingFollowCommand;

	public static int OutsideZoneWarningDist = 20;

	public static float CaughtUpWithPlayerDist = 20f;

	private static float RunDist = 2f;

	private static float StartMovingDist = 1f;

	private static float ObeyFollowCommandDist = 20f;

	private static float CancelFollowCommandDist = 30f;

	private static float CrouchDist = 50f;

	private static float LayerDist = 2f;

	public static bool CanFollowCommandBeIssuedTo(Character leader, Character follower, FollowCommand command)
	{
		if (leader.Community == follower.Community)
		{
			return true;
		}
		if (command == FollowCommand.FillWaterBottle || command == FollowCommand.SitAroundFire || (uint)(command - 12) <= 1u)
		{
			return true;
		}
		return false;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		switch (FollowCommand)
		{
		case FollowCommand.Normal:
		{
			Texture2D texture2D = ((character.SquadLeader != null) ? character.SquadLeader.GetOverheadActionIcon() : null);
			if (!(texture2D == (Texture2D)GameCursor.CursorGather))
			{
				return null;
			}
			return texture2D;
		}
		case FollowCommand.GatherItems:
		case FollowCommand.StoreItems:
			return GameCursor.CursorGather;
		case FollowCommand.FillWaterBottle:
			return GameCursor.CursorWaterBottle;
		case FollowCommand.ChopWood:
		case FollowCommand.StoreChoppedWood:
			return GameCursor.CursorAxe;
		case FollowCommand.MineRock:
		case FollowCommand.StoreMinedRock:
			return GameCursor.CursorPickaxe;
		case FollowCommand.ResetTrap:
		case FollowCommand.StoreTrappedAnimals:
			return GameCursor.TrapperIcon;
		case FollowCommand.Assassinate:
			return GameCursor.CursorKnife;
		case FollowCommand.ChokdHold:
			return GameCursor.CursorKnockOut;
		default:
			return null;
		}
	}

	public override bool IsBored(Character character)
	{
		if (character.SquadLeader != null && character.SquadLeader.IsBored())
		{
			return (character.PosXZ - character.SquadLeader.PosXZ).magnitude < Character.JogIfTiredDist;
		}
		return false;
	}

	public void OnSquadLeaderChanged(Character character, Goal parent)
	{
		ClearFollowCommand(character, parent);
		if (Active && !(SubGoal is Idle))
		{
			SetSubGoal(character, parent, new Idle());
		}
	}

	public void SetFollowCommand(Character character, Goal parent, FollowCommand followCommand, TerrainCoord tile, EquipmentPrototype resourcePrototype, LiquidPrototype liquid = null, float amount = 0f, TileObject target = null)
	{
		if (FollowCommand != FollowCommand.EatAroundFire || followCommand != FollowCommand.SitAroundFire || !(tile == FollowCommandDest))
		{
			ClearFollowCommand(character, parent);
			FollowCommand = followCommand;
			FollowCommandIssuer = character.SquadLeader;
			FollowCommandIssuedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			FollowCommandDest = tile;
			FollowCommandResourceType = resourcePrototype;
			FollowCommandResourceLiquid = liquid;
			FollowCommandResourceAmount = amount;
			FollowCommandTarget = target;
		}
	}

	public void ClearFollowCommand(Character character, Goal parent)
	{
		if (IsObeyingFollowCommand)
		{
			SetSubGoal(character, parent, new Idle());
		}
		ClearFollowCommandInternal();
	}

	private void ClearFollowCommandInternal()
	{
		FollowCommand = FollowCommand.Normal;
		FollowCommandIssuer = null;
		FollowCommandIssuedTime = TimeSpan.Zero;
		FollowCommandDest = TerrainCoord.Invalid;
		FollowCommandResourceType = null;
		FollowCommandResourceLiquid = null;
		FollowCommandResourceAmount = 0f;
		FollowCommandTarget = null;
		IsObeyingFollowCommand = false;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FollowCommand);
		reflector.Add(ref FollowCommandIssuer);
		reflector.Add(ref FollowCommandIssuedTime);
		reflector.Add(ref FollowCommandDest);
		reflector.Add(ref FollowCommandResourceType);
		reflector.AddAfter(ref FollowCommandResourceLiquid, 531);
		reflector.AddAfter(ref FollowCommandResourceAmount, 542);
		reflector.AddAfter(ref FollowCommandTarget, 598);
		reflector.AddAfter(ref ExtraSuperStealthyTime, 599);
		reflector.Add(ref IsObeyingFollowCommand);
		reflector.AddAfter(ref WantCatchUpWithPlayer, 371);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FollowGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = WantCrouching(character);
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new Idle());
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		IsObeyingFollowCommand = false;
	}

	public static bool WantCrouching(Character character)
	{
		if (character.SquadLeader.IsCrouching())
		{
			return (character.SquadLeader.PosXZ - character.PosXZ).sqrMagnitude <= CrouchDist * CrouchDist;
		}
		return false;
	}

	public void OnStartFollowingPlayer(Character character)
	{
		Vector2 vector = CalcFollowingPosXZ(character.SquadLeader, character);
		float sqrMagnitude = (character.PosXZ - vector).sqrMagnitude;
		WantCatchUpWithPlayer = sqrMagnitude > CaughtUpWithPlayerDist * CaughtUpWithPlayerDist;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Finished)
		{
			return;
		}
		SetCrouching(character, parent, WantCrouching(character));
		character.DirectControlledCrouching = Crouching;
		Vector2 vector = CalcFollowingPosXZ(character.SquadLeader, character);
		float sqrMagnitude = (character.PosXZ - vector).sqrMagnitude;
		MovementType movementType = MovementType.Walk;
		if (character.GetMovementType() != MovementType.None)
		{
			movementType = character.GetMovementType();
		}
		if (sqrMagnitude <= CaughtUpWithPlayerDist * CaughtUpWithPlayerDist)
		{
			WantCatchUpWithPlayer = false;
		}
		if (sqrMagnitude >= RunDist * RunDist)
		{
			movementType = MovementType.Run;
		}
		else if (character.SquadLeader.DirectControlled)
		{
			float num = Character.JogSpeed;
			float num2 = character.GetRunSpeed();
			if (movementType >= MovementType.Jog)
			{
				num = Character.WalkSpeed;
			}
			if (movementType >= MovementType.Run)
			{
				num2 = Character.JogSpeed;
			}
			num *= 1f / 60f;
			num2 *= 1f / 60f;
			if (character.SquadLeader.VelocityXZ.sqrMagnitude >= num * num)
			{
				movementType = MovementType.Jog;
			}
			if (character.SquadLeader.VelocityXZ.sqrMagnitude >= num2 * num2)
			{
				movementType = MovementType.Run;
			}
		}
		else
		{
			switch (character.SquadLeader.GetMovementType())
			{
			case MovementType.None:
			case MovementType.Walk:
				movementType = MovementType.Walk;
				break;
			case MovementType.Jog:
				movementType = MovementType.Jog;
				break;
			case MovementType.Run:
				movementType = MovementType.Run;
				break;
			}
		}
		if (character.SquadLeader.DirectControlled)
		{
			character.MarkFollowingPlayerControlled(character.SquadLeader);
			if (character.SquadLeader == Hud.Instance.LocalControlledCharacter && character.IsAuthoritative() && character.HasMovementZone() && !character.MovementZone.Expand(OutsideZoneWarningDist).Contains(character.SquadLeader.Tile) && !HudBehaviour.Instance.IsShowingRecentStatusBarMsg())
			{
				HudBehaviour.Instance.ShowOutsideZoneMsg(character);
			}
		}
		if (IsObeyingFollowCommand)
		{
			if (sqrMagnitude >= CancelFollowCommandDist * CancelFollowCommandDist)
			{
				ClearFollowCommand(character, parent);
			}
		}
		else if (sqrMagnitude < ObeyFollowCommandDist * ObeyFollowCommandDist)
		{
			GameTerrain instance = GameTerrain.Instance;
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			switch (FollowCommand)
			{
			case FollowCommand.GatherItems:
			{
				if (!(FollowCommandDest != TerrainCoord.Invalid) || !(currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0)))
				{
					break;
				}
				Prop prop = instance.GetProp(FollowCommandDest.x, FollowCommandDest.y);
				if (prop != null)
				{
					Equipment equipment5 = prop.Inventory.FindItemOfType(FollowCommandResourceType);
					if (equipment5 != null && character.HasInventorySpaceFor(equipment5.GetWeight()))
					{
						MoveToAndTake moveToAndTake = new MoveToAndTake(character, prop, equipment5, equipment5.GetAmount(), ignoreWeight: false);
						moveToAndTake.IsGathering = true;
						SetSubGoal(character, parent, moveToAndTake);
						IsObeyingFollowCommand = true;
					}
				}
				break;
			}
			case FollowCommand.StoreItems:
			case FollowCommand.StoreChoppedWood:
			case FollowCommand.StoreMinedRock:
			case FollowCommand.StoreTrappedAnimals:
				if (FollowCommandDest != TerrainCoord.Invalid && currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0))
				{
					Goal storeGatheredItemsGoal = GetStoreGatheredItemsGoal(character, movementType);
					if (storeGatheredItemsGoal != null)
					{
						SetSubGoal(character, parent, storeGatheredItemsGoal);
						IsObeyingFollowCommand = true;
					}
				}
				break;
			case FollowCommand.FillWaterBottle:
			{
				if (!(FollowCommandDest != TerrainCoord.Invalid) || !(currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(5.0)))
				{
					break;
				}
				TerrainCoord terrainCoord = TerrainCoord.Invalid;
				Well well = instance.GetProp(FollowCommandDest.x, FollowCommandDest.y) as Well;
				LiquidPrototype liquidPrototype = null;
				if (well != null)
				{
					terrainCoord = well.Tile;
					liquidPrototype = LiquidPrototype.Water;
				}
				else if (instance.IsTileRiver(FollowCommandDest.x, FollowCommandDest.y))
				{
					terrainCoord = instance.GetNearestTileOnPath(character.PosXZ, river: true, 32f);
					liquidPrototype = LiquidPrototype.Water;
				}
				else if (Session.Instance.Weather.SnowOnGroundAmount >= Weather.ScoopableSnowOnGroundAmount)
				{
					terrainCoord = FollowCommandDest;
					liquidPrototype = LiquidPrototype.Snow;
				}
				if (terrainCoord != TerrainCoord.Invalid)
				{
					Equipment bestLiquidContainerToFill = character.Inventory.GetBestLiquidContainerToFill(liquidPrototype);
					if (bestLiquidContainerToFill != null)
					{
						SetSubGoal(character, parent, new FillLiquidContainer(character, FollowCommandDest, -1, bestLiquidContainerToFill, movementType));
						IsObeyingFollowCommand = true;
					}
					else if (well == null && liquidPrototype == LiquidPrototype.Water)
					{
						SetSubGoal(character, parent, new MoveToAndDrinkFromRiver(character, terrainCoord, -1, movementType));
						IsObeyingFollowCommand = true;
					}
				}
				break;
			}
			case FollowCommand.ChopWood:
				if (FollowCommandDest != TerrainCoord.Invalid && currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0))
				{
					TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(FollowCommandDest.x, FollowCommandDest.y);
					if (LumberjackGoal.IsChoppableTree(fixedObjectOnTile) && character.Inventory.GetAxe() != null && (fixedObjectOnTile is TreeProp || character.HasInventorySpaceFor(EquipmentPrototype.Wood.Weight)))
					{
						MoveToAndChop moveToAndChop = new MoveToAndChop(character, fixedObjectOnTile, movementType);
						moveToAndChop.IsGathering = true;
						SetSubGoal(character, parent, moveToAndChop);
						IsObeyingFollowCommand = true;
					}
				}
				break;
			case FollowCommand.MineRock:
				if (FollowCommandDest != TerrainCoord.Invalid && currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0) && FollowCommandResourceType != null)
				{
					TileObject fixedObjectOnTile2 = instance.GetFixedObjectOnTile(FollowCommandDest.x, FollowCommandDest.y);
					if (fixedObjectOnTile2 != null && character.Inventory.GetPickaxe() != null && character.HasInventorySpaceFor(FollowCommandResourceType.Weight))
					{
						MoveToAndMine moveToAndMine = new MoveToAndMine(character, fixedObjectOnTile2, FollowCommandResourceType.GetMineralType(), movementType);
						moveToAndMine.IsGathering = true;
						SetSubGoal(character, parent, moveToAndMine);
						IsObeyingFollowCommand = true;
					}
				}
				break;
			case FollowCommand.ResetTrap:
			{
				if (!(FollowCommandDest != TerrainCoord.Invalid) || !(currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0)))
				{
					break;
				}
				TileObject fixedObjectOnTile3 = instance.GetFixedObjectOnTile(FollowCommandDest.x, FollowCommandDest.y);
				if (fixedObjectOnTile3 is ITrap trap && trap.CanResetTrap())
				{
					EquipmentPrototype equipmentNeededForReset = trap.GetEquipmentNeededForReset();
					if (equipmentNeededForReset == null || character.Inventory.FindItemOfType(equipmentNeededForReset) != null)
					{
						MoveToAndResetTrap moveToAndResetTrap = new MoveToAndResetTrap(character, fixedObjectOnTile3, movementType);
						moveToAndResetTrap.IsGathering = true;
						SetSubGoal(character, parent, moveToAndResetTrap);
						IsObeyingFollowCommand = true;
					}
				}
				break;
			}
			case FollowCommand.SitAroundFire:
				if (FollowCommandDest != TerrainCoord.Invalid && currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(10.0))
				{
					TileObject fixedObjectOnTile4 = instance.GetFixedObjectOnTile(FollowCommandDest.x, FollowCommandDest.y);
					if (fixedObjectOnTile4 != null)
					{
						SetSubGoal(character, parent, new SitAroundFireGoal(character, fixedObjectOnTile4, MovementType.Walk));
						IsObeyingFollowCommand = true;
					}
				}
				break;
			case FollowCommand.Assassinate:
			case FollowCommand.ChokdHold:
				if (FollowCommandTarget != null && currentTime - FollowCommandIssuedTime <= TimeSpan.FromSeconds(1.0) && FollowCommandTarget is Character { Deleted: false, IsConscious: not false } character2 && character.IsEnemy(character2))
				{
					SetSubGoal(character, parent, new MoveToAndChokeHold(character, character2, MovementType.Run, (FollowCommand == FollowCommand.Assassinate) ? HoldType.SlitThroat : HoldType.ChokeHold, assassinate: true));
					IsObeyingFollowCommand = true;
				}
				break;
			case FollowCommand.EatAroundFire:
				if (!(FollowCommandDest != TerrainCoord.Invalid) || !(instance.GetFixedObjectOnTile(FollowCommandDest.x, FollowCommandDest.y) is CraftingProp craftingProp))
				{
					break;
				}
				if (FollowCommandResourceAmount > 0f)
				{
					if (FollowCommandResourceLiquid != null)
					{
						Equipment equipment = craftingProp.Inventory.FindFullestItemOfTypeWithLiquid(craftingProp, FollowCommandResourceType, FollowCommandResourceLiquid, InfectionType.None);
						if (equipment != null)
						{
							if (character.Inventory.GetBestLiquidContainerToFill(FollowCommandResourceLiquid) != null)
							{
								SetSubGoal(character, parent, new MoveToAndInteractGoal(character, craftingProp, InteractionType.FillFromPot, equipment, FollowCommandResourceAmount, MovementType.Walk));
								IsObeyingFollowCommand = true;
							}
							else
							{
								SetSubGoal(character, parent, new MoveToAndInteractGoal(character, craftingProp, InteractionType.EatFromPot, equipment, FollowCommandResourceAmount, MovementType.Walk));
								IsObeyingFollowCommand = true;
							}
						}
					}
					else
					{
						Equipment equipment2 = craftingProp.Inventory.FindItemOfType(FollowCommandResourceType, InfectionType.None);
						if (equipment2 != null)
						{
							SetSubGoal(character, parent, new MoveToAndTake(character, craftingProp, equipment2, (int)FollowCommandResourceAmount, ignoreWeight: true, MovementType.Walk));
							IsObeyingFollowCommand = true;
						}
					}
				}
				else if (!character.IsSitting())
				{
					SetSubGoal(character, parent, new SitAroundFireGoal(character, craftingProp, MovementType.Walk));
					IsObeyingFollowCommand = true;
				}
				else
				{
					if (!(character.GetHunger() > 0f))
					{
						break;
					}
					if (FollowCommandResourceLiquid != null)
					{
						Equipment equipment3 = character.Inventory.FindBestItemWithLiquid(FollowCommandResourceLiquid, InfectionType.None);
						if (equipment3 != null)
						{
							SetSubGoal(character, parent, new EatGoal(equipment3));
							IsObeyingFollowCommand = true;
						}
					}
					else if (FollowCommandResourceType != null)
					{
						Equipment equipment4 = character.Inventory.FindItemOfType(FollowCommandResourceType, InfectionType.None);
						if (equipment4 != null)
						{
							SetSubGoal(character, parent, new EatGoal(equipment4));
							IsObeyingFollowCommand = true;
						}
					}
				}
				break;
			}
			if (FollowCommand != FollowCommand.Normal && !IsObeyingFollowCommand)
			{
				ClearFollowCommand(character, parent);
			}
		}
		if (!IsObeyingFollowCommand)
		{
			if (character.SquadLeader.InsideBuilding != null)
			{
				if (character.InsideBuilding == character.SquadLeader.InsideBuilding)
				{
					if (!(SubGoal is Idle))
					{
						SetSubGoal(character, parent, new Idle());
					}
				}
				else if (character.InsideBuilding != null)
				{
					if (FollowSquadLeaderToAdjoiningBuildingIfPossible(character))
					{
						if (!(SubGoal is Idle))
						{
							SetSubGoal(character, parent, new Idle());
						}
					}
					else if (!(SubGoal is LeaveBuilding))
					{
						int closestEntranceTo = character.InsideBuilding.GetClosestEntranceTo(character.SquadLeader.Tile);
						SetSubGoal(character, parent, new LeaveBuilding(closestEntranceTo));
					}
				}
				else if ((!(SubGoal is MoveToAndEnterBuilding) || (SubGoal as MoveToAndEnterBuilding).GetTargetBuilding() != character.SquadLeader.InsideBuilding) && (!(SubGoal is MoveAsCloseAsPossibleToTarget) || (SubGoal as MoveAsCloseAsPossibleToTarget).UseFollowOffset) && !(SubGoal is Wait) && character.SquadLeader.InsideBuilding.CanEnter(character))
				{
					SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, character.SquadLeader.InsideBuilding, movementType));
				}
			}
			else if (character.InsideBuilding != null && !(SubGoal is LeaveBuilding))
			{
				int closestEntranceTo2 = character.InsideBuilding.GetClosestEntranceTo(character.SquadLeader.Tile);
				SetSubGoal(character, parent, new LeaveBuilding(closestEntranceTo2));
			}
			if (SubGoal is Idle && !(SubGoal is LeaveBuilding))
			{
				float num3 = StartMovingDist;
				if (character.SquadLeader.MovementSpeed == 0f)
				{
					num3 = (CalcMaxFollowerDist(character.SquadLeader) + 1f) * 2f;
				}
				EnterableVehicle enterableVehicle = character.SquadLeader.InsideBuilding as EnterableVehicle;
				if ((character.SquadLeader.InsideBuilding == null && sqrMagnitude >= num3 * num3) || (enterableVehicle != null && enterableVehicle.HasEverMoved && sqrMagnitude >= MathUtil.Squared(num3 + MathUtil.ToXZ(enterableVehicle.GetBoundingBox().extents).magnitude)))
				{
					MoveAsCloseAsPossibleToTarget moveAsCloseAsPossibleToTarget = new MoveAsCloseAsPossibleToTarget(character, character.SquadLeader, movementType);
					moveAsCloseAsPossibleToTarget.AlwaysTrackTarget = true;
					moveAsCloseAsPossibleToTarget.UseFollowOffset = true;
					SetSubGoal(character, parent, moveAsCloseAsPossibleToTarget);
				}
			}
			if (character.SquadLeader.InsideBuilding != null && SubGoal is MoveAsCloseAsPossibleToTarget && !(SubGoal as MoveAsCloseAsPossibleToTarget).UseFollowOffset && character.Tile.IsWithinBounds(character.SquadLeader.InsideBuilding.MinTile - new TerrainCoord(1, 1), character.SquadLeader.InsideBuilding.MaxTile + new TerrainCoord(1, 1)))
			{
				SetSubGoal(character, parent, new Wait(TimeSpan.FromSeconds(2.0)));
			}
		}
		if (SubGoal is MoveTo moveTo)
		{
			moveTo.SetMovementType(character, movementType);
		}
		if (SubGoal is MoveToAndDeposit moveToAndDeposit)
		{
			moveToAndDeposit.SetMovementType(character, movementType);
		}
		if (SubGoal is MoveToAndTake moveToAndTake2)
		{
			moveToAndTake2.SetMovementType(character, movementType);
		}
	}

	public static bool FollowSquadLeaderToAdjoiningBuildingIfPossible(Character character)
	{
		if (character.InsideBuilding != null && character.SquadLeader.InsideBuilding != null && character.IsAuthoritative())
		{
			EntranceDef[] entranceDefs = character.InsideBuilding.GetEntranceDefs();
			for (int i = 0; i < entranceDefs.Length; i++)
			{
				TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(character.InsideBuilding.GetEntrancePos(i));
				Building building = GameTerrain.Instance.GetBuilding(tileCoordForPos.x, tileCoordForPos.y);
				if (building == character.SquadLeader.InsideBuilding)
				{
					if (building.CanEnter(character))
					{
						character.InsideBuilding.OnCharacterLeave(character, i, fromBuildingDestroyed: false, fromRagdolled: false);
						building.OnCharacterEnter(character, wasOrderedInsideBuilding: true);
					}
					return true;
				}
			}
		}
		return false;
	}

	public Goal GetStoreGatheredItemsGoal(Character character, MovementType movementType)
	{
		Prop prop = GameTerrain.Instance.GetProp(FollowCommandDest.x, FollowCommandDest.y);
		if (prop != null && character.GetGatherGoal() != null)
		{
			Prop resultProp = null;
			int resultAmountToDeposit;
			Equipment bestItemToDump = GatherGoal.GetBestItemToDump(character, wantToFreeSpace: true, needToFreeSpace: false, ref resultProp, out resultAmountToDeposit);
			if (bestItemToDump != null && GatherGoal.CanStoreSuppliesIn(character, prop, bestItemToDump.GetWeight()))
			{
				return new MoveToAndDeposit(character, prop, bestItemToDump, resultAmountToDeposit, movementType)
				{
					WasGathered = (bestItemToDump.GetGatheredAmount() > 0)
				};
			}
		}
		return null;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsObeyingFollowCommand)
		{
			switch (FollowCommand)
			{
			case FollowCommand.StoreItems:
			case FollowCommand.StoreChoppedWood:
			case FollowCommand.StoreMinedRock:
			case FollowCommand.StoreTrappedAnimals:
			{
				Goal storeGatheredItemsGoal = GetStoreGatheredItemsGoal(character, MovementType.Walk);
				if (storeGatheredItemsGoal != null)
				{
					return storeGatheredItemsGoal;
				}
				ClearFollowCommandInternal();
				return new Idle();
			}
			case FollowCommand.GatherItems:
			case FollowCommand.FillWaterBottle:
			case FollowCommand.ChopWood:
			case FollowCommand.MineRock:
			case FollowCommand.ResetTrap:
			case FollowCommand.SitAroundFire:
				ClearFollowCommandInternal();
				return new Idle();
			case FollowCommand.Assassinate:
			case FollowCommand.ChokdHold:
				ExtraSuperStealthyTime = Session.Instance.PlayTime + TimeSpan.FromSeconds(4.0);
				ClearFollowCommandInternal();
				return new Idle();
			case FollowCommand.EatAroundFire:
				if (SubGoal is MoveToAndTake moveToAndTake)
				{
					FollowCommandResourceAmount = 0f;
					return new SitAroundFireGoal(character, moveToAndTake.GetTargetObject(), MovementType.Walk);
				}
				if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal)
				{
					FollowCommandResourceAmount = 0f;
					return new SitAroundFireGoal(character, moveToAndInteractGoal.GetTargetObject(), MovementType.Walk);
				}
				if (SubGoal is SitAroundFireGoal && character.GetHunger() > 0f)
				{
					if (FollowCommandResourceLiquid != null)
					{
						Equipment equipment = character.Inventory.FindBestItemWithLiquid(FollowCommandResourceLiquid, InfectionType.None);
						if (equipment != null)
						{
							return new EatGoal(equipment);
						}
					}
					else if (FollowCommandResourceType != null)
					{
						Equipment equipment2 = character.Inventory.FindItemOfType(FollowCommandResourceType, InfectionType.None);
						if (equipment2 != null)
						{
							return new EatGoal(equipment2);
						}
					}
				}
				ClearFollowCommandInternal();
				return new Idle();
			}
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				return new Idle();
			}
			return new MoveAsCloseAsPossibleToTarget(character, character.SquadLeader, moveToAndEnterBuilding.MovementType)
			{
				AlwaysTrackTarget = true,
				UseFollowOffset = false
			};
		}
		if (SubGoal is MoveAsCloseAsPossibleToTarget moveAsCloseAsPossibleToTarget)
		{
			if (character.SquadLeader.InsideBuilding != null && character.SquadLeader.InsideBuilding.CanEnter(character))
			{
				if (moveAsCloseAsPossibleToTarget.UseFollowOffset)
				{
					return new MoveToAndEnterBuilding(character, character.SquadLeader.InsideBuilding, moveAsCloseAsPossibleToTarget.MovementType);
				}
				return new Wait(TimeSpan.FromSeconds(2.0));
			}
			if (character.Tile == moveAsCloseAsPossibleToTarget.DestTile)
			{
				return new Idle();
			}
			return new Wait(TimeSpan.FromSeconds(2.0));
		}
		if (SubGoal is Wait || SubGoal is LeaveBuilding || SubGoal is MoveToAndEnterBuilding)
		{
			return new Idle();
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.SquadLeader == null)
		{
			return false;
		}
		if (character.IsControllableByOrFollowingPlayer() && character.IsTooDepressedToFollowOrders())
		{
			return false;
		}
		if (!character.SquadLeader.IsConscious && !character.IsControllableByPlayer())
		{
			FindGoal.BuildListOfEscapedFromTargets(character);
			bool num = FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, character.SquadLeader.Pos);
			FindGoal.ClearListOfEscapedFromTargets();
			if (num)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (character.Zombie)
		{
			return GoalPriority.Zombie_Follow;
		}
		if (character.DirectControlledMajorAIDisabled)
		{
			return GoalPriority.Impossible;
		}
		if (!character.IsControllableByPlayer() && character.SquadLeader.IsControllableByPlayer() && (character.GetHunger() >= Character.HungerExtraCriticalTime || character.GetThirst() >= Character.ThirstExtraCriticalTime || character.GetSleepDeprivation() >= (character.IsSleeping() ? (Character.SleepDeprivationExtraCriticalTime - Sun.DayLengthSecs) : Character.SleepDeprivationExtraCriticalTime) || character.GetBodyTemperatureInCelsius() < ((character.TemperatureInsulationDelta > 0f) ? Character.BodyTemperatureInCelsiusModerateHypothermia : Character.BodyTemperatureInCelsiusWakeUp)))
		{
			return GoalPriority.Survivor_Follow_LeaderIdle;
		}
		if (FollowCommand == FollowCommand.EatAroundFire)
		{
			return GoalPriority.Survivor_Follow_EatTreat;
		}
		if ((FollowCommand == FollowCommand.Normal || FollowCommand == FollowCommand.SitAroundFire) && !character.SquadLeader.DirectControlled && !character.SquadLeader.HasBeenPlayerControlledRecently(critical: false, extraCritical: false))
		{
			Squad squad = character.GetSquad();
			if (squad != null)
			{
				if (squad.Action == SquadAction.StopForWarmth || squad.Action == SquadAction.HangAround)
				{
					return GoalPriority.Survivor_Follow;
				}
				foreach (Character member in squad.Members)
				{
					if (member.SparringPartner != null)
					{
						return GoalPriority.Survivor_Follow_LeaderIdle;
					}
				}
			}
			if (character.SquadLeader.SparringPartner != null)
			{
				return GoalPriority.Survivor_Follow_LeaderIdle;
			}
			if (character.SquadLeader.IsSatisfyingNeeds() && !WantCatchUpWithPlayer)
			{
				bool flag = false;
				if (character.SquadLeader.FindActiveGoal(GoalType.WarmGoal) is WarmGoal warmGoal)
				{
					flag |= warmGoal.SubGoal is CraftGoal;
					flag |= warmGoal.SubGoal is LightFireGoal;
				}
				if (!flag)
				{
					return GoalPriority.Survivor_Follow_LeaderIdle;
				}
			}
			if (!character.IsControllableByOrFollowingPlayer())
			{
				if (character.SquadLeader.IsBored())
				{
					return GoalPriority.Survivor_Follow_LeaderIdle;
				}
				if (squad == null || (squad.Action != SquadAction.TalkToAmbush && squad.Action != SquadAction.FindTarget && squad.Action != SquadAction.GoToHighPrio))
				{
					if (character.SquadLeader.FindActiveGoal(GoalType.Conversation) is Conversation conversation && (conversation.OpeningSpeech == null || (conversation.OpeningSpeech.Situation != SpeechSituation.Eulogy && conversation.OpeningSpeech.Importance < Importance.Quest)))
					{
						return GoalPriority.Survivor_Follow_LeaderIdle;
					}
					if (character.SquadLeader.FindActiveGoal(GoalType.AftermathGoal) != null)
					{
						RecentActivityType recentActivityType = character.SquadLeader.RecentActivityType;
						if ((uint)(recentActivityType - 1) <= 1u || (uint)(recentActivityType - 4) <= 1u || recentActivityType == RecentActivityType.ThrownAt)
						{
							return GoalPriority.Survivor_Follow_LeaderIdle;
						}
					}
				}
				if (character.SquadLeader.FindActiveGoal(GoalType.SwapSuppliesGoal) != null)
				{
					return GoalPriority.Survivor_Follow_LeaderIdle;
				}
			}
		}
		return GoalPriority.Survivor_Follow;
	}

	public static Vector2 CalcFollowingPosXZ(Character squadLeader, Character follower)
	{
		CalcFollowAngleAndDist(squadLeader, follower, out var angle, out var dist);
		return squadLeader.FollowMePosXZ + MathUtil.GetDirFromAngle(squadLeader.FollowMeDirAngle + MathF.PI + angle) * dist;
	}

	public static void CalcFollowAngleAndDist(Character squadLeader, Character follower, out float angle, out float dist)
	{
		angle = 0f;
		dist = 0f;
		if (squadLeader.InsideBuilding != null)
		{
			return;
		}
		int num = 0;
		int num2 = 3;
		int num3 = 0;
		if (squadLeader.Followers == null)
		{
			return;
		}
		foreach (Character follower2 in squadLeader.Followers)
		{
			if (follower2 == follower)
			{
				if (num == 0)
				{
					num3 = num2 - 1 - num3;
				}
				dist = (float)(num + 1) * LayerDist;
				angle = MathF.PI / 2f * (float)((num3 + 1) / 2) * ((num3 % 2 == 0) ? 1f : (-1f)) / (float)(num2 / 2);
				break;
			}
			num3++;
			if (num3 >= num2)
			{
				num++;
				num2 += 2;
				num3 = 0;
			}
		}
	}

	public static float CalcMaxFollowerDist(Character squadLeader)
	{
		int num = 0;
		int num2 = 3;
		int num3 = 0;
		if (squadLeader.Followers != null)
		{
			for (int i = 0; i < squadLeader.Followers.Count; i++)
			{
				num3++;
				if (num3 >= num2)
				{
					num++;
					num2 += 2;
					num3 = 0;
				}
			}
		}
		return (float)(num + ((num3 > 0) ? 1 : 0)) * LayerDist;
	}
}

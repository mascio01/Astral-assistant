using System;
using UnityEngine;

public class MoveToAndEnterBuilding : StateMachineGoal
{
	public bool Success;

	private MovementType _movementType = MovementType.Walk;

	public bool _dontOpenOurGates;

	public int EntranceIndex = -1;

	public TerrainCoord RequestedEntranceTile = TerrainCoord.Invalid;

	public bool StayThere;

	public bool ForceEnter;

	public bool AvoidHostileBases;

	public MovementType MovementType => _movementType;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndEnterBuilding;
	}

	public MoveToAndEnterBuilding()
	{
	}

	public MoveToAndEnterBuilding(Character character, Building targetBuilding)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetBuilding));
		HasUserTarget = true;
	}

	public MoveToAndEnterBuilding(Character character, Building targetBuilding, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetBuilding));
		HasUserTarget = true;
		_movementType = movementType;
	}

	public MoveToAndEnterBuilding(Character character, Building targetBuilding, int entranceIndex, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetBuilding));
		HasUserTarget = true;
		_movementType = movementType;
		EntranceIndex = entranceIndex;
	}

	public MoveToAndEnterBuilding(Character character, Building targetBuilding, MovementType movementType, bool dontOpenOurGates)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetBuilding));
		HasUserTarget = true;
		_movementType = movementType;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public MoveToAndEnterBuilding(int entranceIndex, MovementType movementType, bool dontOpenOurGates)
	{
		EntranceIndex = entranceIndex;
		_movementType = movementType;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref Success);
		reflector.Add(ref _dontOpenOurGates);
		reflector.AddAfter(ref EntranceIndex, 90);
		reflector.AddAfter(ref RequestedEntranceTile, 506);
		reflector.AddAfter(ref StayThere, 261);
		reflector.AddAfter(ref ForceEnter, 504);
		reflector.AddAfter(ref AvoidHostileBases, 517);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		Building targetBuilding = GetTargetBuilding();
		if (targetBuilding == null || character.InsideBuilding == targetBuilding)
		{
			Finished = true;
			Success = true;
			EntranceIndex = 0;
			return;
		}
		if (EntranceIndex == -1)
		{
			EntranceIndex = targetBuilding.GetClosestEntranceTo(character.Tile);
		}
		Vector3 entrancePos = targetBuilding.GetEntrancePos(EntranceIndex);
		MoveTo moveTo = new MoveTo(destTile: RequestedEntranceTile = GameTerrain.Instance.GetTileCoordForPos(entrancePos), movementType: _movementType);
		moveTo._dontOpenOurGates = _dontOpenOurGates;
		moveTo.AvoidHostileBases = AvoidHostileBases;
		SetSubGoal(character, parent, moveTo);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		MoveTo moveTo = SubGoal as MoveTo;
		if (SubGoal is MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo)
		{
			if (moveAsCloseAsPossibleTo.Success)
			{
				Building targetBuilding = GetTargetBuilding();
				if (targetBuilding != null)
				{
					float angle = MathUtil.WrapAngle(targetBuilding.GetEntranceAngle(EntranceIndex) + MathF.PI);
					return new TurnTo(GameTerrain.Instance.GetTileCoordForPos(character.Position + MathUtil.ToX0Y(MathUtil.GetDirFromAngle(angle))));
				}
			}
		}
		else if (moveTo != null)
		{
			Building targetBuilding2 = GetTargetBuilding();
			if (targetBuilding2 != null)
			{
				if (moveTo.Success)
				{
					float angle2 = MathUtil.WrapAngle(targetBuilding2.GetEntranceAngle(EntranceIndex) + MathF.PI);
					return new TurnTo(GameTerrain.Instance.GetTileCoordForPos(character.Position + MathUtil.ToX0Y(MathUtil.GetDirFromAngle(angle2))));
				}
				if (targetBuilding2.IsDriveableVehicle())
				{
					return new MoveAsCloseAsPossibleTo(_movementType, RequestedEntranceTile, 3.1f, _dontOpenOurGates);
				}
				return new CheckPathToBuildingEntrance(EntranceIndex, _dontOpenOurGates, AvoidHostileBases);
			}
		}
		if (SubGoal is CheckPathToBuildingEntrance { Success: not false })
		{
			return new MoveAsCloseAsPossibleTo(_movementType, RequestedEntranceTile, 3.1f, _dontOpenOurGates);
		}
		if (SubGoal is TurnTo)
		{
			Building targetBuilding3 = GetTargetBuilding();
			if (targetBuilding3 != null && !IsTargetDeleted() && (targetBuilding3.CanEnter(character) || ForceEnter))
			{
				return new EnterBuilding(ForceEnter);
			}
		}
		if (SubGoal is EnterBuilding enterBuilding)
		{
			if (StayThere && enterBuilding.Success)
			{
				return new Idle();
			}
			if (parent is ObeyLeaderGoal obeyLeaderGoal && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player)
			{
				character.SetRecentActivity(RecentActivityType.EnteredBuilding, null);
			}
			Success = enterBuilding.Success;
		}
		return null;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Building targetBuilding = GetTargetBuilding();
		if (targetBuilding == null)
		{
			return;
		}
		Vector3 entrancePos = targetBuilding.GetEntrancePos(EntranceIndex);
		EnterableVehicle enterableVehicle = targetBuilding as EnterableVehicle;
		if (enterableVehicle != null)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(entrancePos);
			if (RequestedEntranceTile != tileCoordForPos && RequestedEntranceTile.GetDist(tileCoordForPos) >= Math.Min(10f, Math.Min(RequestedEntranceTile.GetDist(character.Tile), tileCoordForPos.GetDist(character.Tile)) - 2f) && SubGoal is MoveTo)
			{
				RequestedEntranceTile = tileCoordForPos;
				MoveTo moveTo = new MoveTo(_movementType, tileCoordForPos);
				moveTo._dontOpenOurGates = _dontOpenOurGates;
				moveTo.AvoidHostileBases = AvoidHostileBases;
				SetSubGoal(character, parent, moveTo);
			}
		}
		bool flag = enterableVehicle != null && enterableVehicle.GearState != GearState.Off;
		if (!(character.GetAuthoritativeOrElseThisCharacter().UnderAttackRefCount > 0 || flag) || StayThere || character.InsideBuilding != null || character.InteractionObject != null || !(MathUtil.ToXZ(entrancePos - character.Pos).sqrMagnitude <= MathUtil.Squared(1f)))
		{
			return;
		}
		targetBuilding.PlayEnterSound(character);
		if (!character.IsAuthoritative())
		{
			return;
		}
		if (character.CarryingObject != null)
		{
			character.DropAuthoritative();
			return;
		}
		ObeyLeaderGoal obeyLeaderGoal = parent as ObeyLeaderGoal;
		bool wasOrderedInsideBuilding = obeyLeaderGoal != null && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player;
		if (targetBuilding.OnCharacterEnter(character, wasOrderedInsideBuilding, ForceEnter))
		{
			if (obeyLeaderGoal != null && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player)
			{
				character.SetRecentActivity(RecentActivityType.EnteredBuilding, null);
			}
			Success = true;
			Finished = true;
		}
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		_movementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return _movementType;
	}
}

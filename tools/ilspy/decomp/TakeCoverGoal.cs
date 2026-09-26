using System;
using UnityEngine;

public class TakeCoverGoal : StateMachineGoal
{
	public bool Success;

	public bool _dontOpenOurGates;

	public StayInRangeParams StayInRangeParams;

	public static float CrouchRaycastYOffset = -0.2f;

	public TakeCoverGoal()
	{
	}

	public TakeCoverGoal(bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref _dontOpenOurGates);
		StayInRangeParams.Reflect(reflector);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeCoverGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new TakeCoverFromTargetWhileFiring(_dontOpenOurGates, StayInRangeParams));
	}

	public static bool IsCover(CoverType coverType, TerrainCoord tile, Target target, float h)
	{
		switch (coverType)
		{
		case CoverType.Full:
		{
			Vector3 v = target.LastKnownPosition + new Vector3(0f, HumanAppearance.MaleDefaultGunHeight, 0f);
			Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(tile);
			return v.y - h < AStar.MaxCoverGradient * (MathUtil.ToXZ(v) - tileCentreXZ).magnitude;
		}
		case CoverType.WaistHigh:
		{
			Vector3 lastKnownPosition = target.LastKnownPosition;
			Vector3 tileCentrePos = GameTerrain.Instance.GetTileCentrePos(tile);
			return lastKnownPosition.y - tileCentrePos.y < AStar.MaxWaistHighCoverGradient * MathUtil.ToXZ(lastKnownPosition - tileCentrePos).magnitude;
		}
		default:
			return false;
		}
	}

	public static bool CheckCover(Character character, Target target, out bool crouching)
	{
		if (target == null || !(target.Object is Character) || !character.IsOutdoors())
		{
			crouching = false;
			return false;
		}
		TerrainCoord tile = character.Tile;
		if ((character.PosXZ - GameTerrain.Instance.GetTileCentreXZ(tile)).sqrMagnitude >= 0.010000001f)
		{
			crouching = false;
			return false;
		}
		Vector2 dirXZ = MathUtil.ToXZ(target.LastKnownPosition - character.Pos);
		TerrainCoord adjacentTileInDir = tile.GetAdjacentTileInDir(dirXZ);
		TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(adjacentTileInDir.x, adjacentTileInDir.y);
		crouching = fixedObjectOnTile != null && fixedObjectOnTile.CoverType == CoverType.WaistHigh;
		if (fixedObjectOnTile is Building building && building.IsGuardPost())
		{
			Character firstInhabitant = building.GetFirstInhabitant();
			if (character.IsEnemy(firstInhabitant))
			{
				return false;
			}
		}
		if (fixedObjectOnTile != null)
		{
			return IsCover(fixedObjectOnTile.CoverType, adjacentTileInDir, target, fixedObjectOnTile.GetBoundingBox().max.y);
		}
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is TakeCoverFromTargetWhileFiring { Success: not false } && GetTargetCharacter() != null)
		{
			bool crouching = false;
			if (CheckCover(character, Target, out crouching))
			{
				Success = true;
				if (crouching)
				{
					SetCrouching(character, parent, crouching: true);
					return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.6666666865348816), aiming: true, crouching: true);
				}
				return null;
			}
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}

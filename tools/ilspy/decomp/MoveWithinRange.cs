using System;

public class MoveWithinRange : MoveTo
{
	public float MinRange;

	public float MaxRange;

	public StayInRangeParams StayInRangeParams;

	public MoveWithinRange()
	{
	}

	public MoveWithinRange(MovementType movementType, TerrainCoord destTile, bool aiming, float minRange, float maxRange, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
		: base(movementType, destTile)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		MinRange = minRange;
		MaxRange = maxRange;
		Aiming = aiming;
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MinRange);
		reflector.Add(ref MaxRange);
		StayInRangeParams.Reflect(reflector);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveWithinRange;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		_requester.StartWithinRangeRequest(GetStartTile(character), GameTerrain.Instance.GetTileCentrePos(DestTile), MinRange, MaxRange, character, null, AStarPriority, _dontOpenOurGates, resultTile, resultDist, 0f, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override void UpdateDestination(Character character, Goal parent)
	{
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		float dist = character.Tile.GetDist(DestTile);
		if (!(dist >= MinRange) || !(dist <= MaxRange))
		{
			return MoveToResult.Fail;
		}
		return MoveToResult.Success;
	}

	public override int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetClosestEntranceTo(DestTile, character, mustBeUnblocked: true);
	}
}

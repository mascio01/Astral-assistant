public class MoveAsCloseAsPossibleTo : MoveTo
{
	public float MaxRange = float.MaxValue;

	public TerrainCoord OriginalDestTile;

	public bool IgnoreFlammableDefences;

	public bool IgnoreExplodableDefences;

	public MoveAsCloseAsPossibleTo()
	{
	}

	public MoveAsCloseAsPossibleTo(MovementType movementType, TerrainCoord destTile)
		: base(movementType, destTile)
	{
		OriginalDestTile = destTile;
	}

	public MoveAsCloseAsPossibleTo(MovementType movementType, TerrainCoord destTile, float maxRange)
		: base(movementType, destTile)
	{
		MaxRange = maxRange;
		OriginalDestTile = destTile;
	}

	public MoveAsCloseAsPossibleTo(MovementType movementType, TerrainCoord destTile, float maxRange, bool dontOpenOurGates)
		: base(movementType, destTile)
	{
		MaxRange = maxRange;
		OriginalDestTile = destTile;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MaxRange);
		reflector.AddAfter(ref OriginalDestTile, 197);
		if (reflector.IsDeserialising && reflector.Version < 197)
		{
			OriginalDestTile = DestTile;
		}
		reflector.AddAfter(ref IgnoreFlammableDefences, 590);
		reflector.AddAfter(ref IgnoreExplodableDefences, 590);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveAsCloseAsPossibleTo;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartAsCloseAsPossibleRequest(GetStartTile(character), DestTile, character, null, MaxRange, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId, canUnlockGatesFromInsideWithKey: false, IgnoreFlammableDefences, IgnoreExplodableDefences);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		return MoveToResult.Success;
	}
}

using System;

public class MoveAsCloseAsPossibleAndWait : StateMachineGoal
{
	private MovementType _movementType;

	private TerrainCoord _destTile;

	private TimeSpan _waitTime;

	public TimeSpan WaitTimeIfFailed;

	public float MaxRange = float.MaxValue;

	public bool IgnoreFlammableDefences;

	public bool IgnoreExplodableDefences;

	public bool AvoidHostileBases;

	public int DontAvoidCommunityId;

	public MoveAsCloseAsPossibleAndWait()
	{
	}

	public MoveAsCloseAsPossibleAndWait(MovementType movementType, TerrainCoord destTile, TimeSpan waitTime, float maxRange)
	{
		_movementType = movementType;
		_destTile = destTile;
		_waitTime = waitTime;
		MaxRange = maxRange;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref _destTile);
		reflector.Add(ref _waitTime);
		reflector.AddAfter(ref WaitTimeIfFailed, 583);
		reflector.AddAfter(ref IgnoreFlammableDefences, 590);
		reflector.AddAfter(ref IgnoreExplodableDefences, 590);
		reflector.AddAfter(ref AvoidHostileBases, 411);
		reflector.AddAfter(ref DontAvoidCommunityId, 411);
		reflector.AddAfter(ref MaxRange, 555);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveAsCloseAsPossibleAndWait;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = new MoveAsCloseAsPossibleTo(_movementType, _destTile, MaxRange);
		moveAsCloseAsPossibleTo.IgnoreFlammableDefences = IgnoreFlammableDefences;
		moveAsCloseAsPossibleTo.IgnoreExplodableDefences = IgnoreExplodableDefences;
		moveAsCloseAsPossibleTo.AvoidHostileBases = AvoidHostileBases;
		moveAsCloseAsPossibleTo.DontAvoidCommunityId = DontAvoidCommunityId;
		SetSubGoal(character, parent, moveAsCloseAsPossibleTo);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo && (moveAsCloseAsPossibleTo.Success || WaitTimeIfFailed.Ticks > 0))
		{
			return new Wait(moveAsCloseAsPossibleTo.Success ? _waitTime : WaitTimeIfFailed);
		}
		return base.GetNextSubGoal(character, parent);
	}
}

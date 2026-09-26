using UnityEngine;

public class MoveWithinRangeOfSquadLeader : MoveTo
{
	private float Range;

	private Vector3 RequestedDestPos;

	public MoveWithinRangeOfSquadLeader()
	{
	}

	public MoveWithinRangeOfSquadLeader(MovementType movementType, bool aiming, float range, bool dontOpenOurGates)
		: base(movementType)
	{
		Aiming = aiming;
		Range = range;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveWithinRangeOfSquadLeader;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Range);
		reflector.Add(ref RequestedDestPos);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.SquadLeader == null)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void UpdateDestination(Character character, Goal parent)
	{
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		Vector3 goalPos = (RequestedDestPos = ((character.SquadLeader != null) ? character.SquadLeader.Pos : character.Pos));
		_requester.StartWithinRangeRequest(GetStartTile(character), goalPos, 0f, Range, character, null, AStarPriority, _dontOpenOurGates, TerrainCoord.Invalid, float.MaxValue, 0f, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if ((character.PosXZ - character.SquadLeader.PosXZ).sqrMagnitude <= Range * Range)
		{
			return MoveToResult.Success;
		}
		if ((character.SquadLeader.PosXZ - MathUtil.ToXZ(RequestedDestPos)).magnitude >= 2f)
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Fail;
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		if (_requester == null && character.GetRouteCount() > 0)
		{
			TerrainCoord routeDestination = character.GetRouteDestination();
			if (GameTerrain.Instance.IsImpassable(routeDestination.x, routeDestination.y, 2049, character, null))
			{
				return true;
			}
		}
		if ((character.SquadLeader.PosXZ - MathUtil.ToXZ(RequestedDestPos)).magnitude >= 4f)
		{
			return true;
		}
		return false;
	}
}

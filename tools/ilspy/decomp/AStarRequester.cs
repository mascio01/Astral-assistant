using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AStarRequester
{
	public TerrainCoord Start;

	public TerrainCoord Goal;

	public Vector3 GoalPos;

	public bool TargetIsCrouching;

	public List<TerrainCoord> Route;

	public List<TerrainCoord> Dangers;

	private volatile AStarResult _result;

	public int TotalRemovedCountAtFinish;

	public int StartInputFrame = -1;

	public volatile int CancelledInputFrame = -1;

	public bool Discarded;

	public AStarReachedGoalCondition ReachedGoalCondition;

	public Character Requester;

	public InfectionType RequesterInfectionType;

	public Community RequesterCommunity;

	public CanOpenGates RequesterCanOpenPlayerGates;

	public bool RequesterIsFollowingDirectControlledPlayer;

	public bool RequesterInCombat;

	public bool SeparatedByWalls;

	public List<EquipmentPrototype> RequesterKeys;

	public TileObject IgnoreObject;

	public Building IgnoreGuardBuilding;

	public bool IncludeWireFences;

	public bool IgnoreFlammableDefences;

	public bool IgnoreExplodableDefences;

	public bool IgnoreCharacters;

	public bool DontOpenOurGates;

	public bool CanUnlockGatesFromInsideWithKey;

	public bool CanVaultWaistHighWalls;

	public bool AvoidHostileBases;

	public int DontAvoidCommunityId;

	public TerrainCoord Min;

	public TerrainCoord Max;

	public TerrainCoord BoundsMin;

	public TerrainCoord BoundsMax;

	public float MinRange;

	public float MaxRange;

	public float _minRangeSquared;

	public float _maxRangeSquared;

	public TerrainRect MovementZone = TerrainRect.Invalid;

	public TerrainCoord SquadLeaderTile = TerrainCoord.Invalid;

	public float StayInRangeOfSquadLeaderTile = float.MaxValue;

	public float MinDistFromStart;

	public AStarPriority Priority;

	public static bool DrawLineOfSightRaycasts;

	private static string HasReachedGoalStr = "HasReachedGoal";

	public AStarResult Result
	{
		get
		{
			if (Session.Instance.IsInMultiplayerGame())
			{
				while (!GameTerrain.Instance.AStar.HasPassedDeterministicTime(AStar.InputFrameToDeterministicTimeSpan(Session.Instance.InputFrame)))
				{
					Thread.Sleep(1);
				}
				AStarResult aStarResult = AStarResult.None;
				if (GameTerrain.Instance.AStar.PopCompletedRequest(this))
				{
					return _result;
				}
				return AStarResult.Processing;
			}
			return _result;
		}
	}

	public AStarRequester()
	{
		Route = new List<TerrainCoord>();
		RequesterKeys = new List<EquipmentPrototype>();
	}

	public void StartRequest(TerrainCoord start, TerrainCoord goal, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, goal, AStarReachedGoalCondition.Exact, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartAdjacentToRequest(TerrainCoord start, TerrainCoord goal, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool canBeOnTile, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, goal, canBeOnTile ? AStarReachedGoalCondition.AdjacentTo : AStarReachedGoalCondition.AdjacentToButNotOn, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartWithinBoundsRequest(TerrainCoord start, TerrainCoord boundsMin, TerrainCoord boundsMax, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		BoundsMin = boundsMin;
		BoundsMax = boundsMax;
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, instance.ClampTileWithinBounds((boundsMin + boundsMax) / 2), AStarReachedGoalCondition.WithinBounds, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartAsCloseAsPossibleRequest(TerrainCoord start, TerrainCoord goal, Character requester, TileObject ignore, float maxRange, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false, bool ignoreFlammableDefences = false, bool ignoreExplodableDefences = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		MaxRange = maxRange;
		_maxRangeSquared = ((maxRange == float.MaxValue) ? float.MaxValue : (maxRange * maxRange));
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = ignoreFlammableDefences;
		IgnoreExplodableDefences = ignoreExplodableDefences;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, goal, AStarReachedGoalCondition.AsCloseAsPossible, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartWithinRangeRequest(TerrainCoord start, Vector3 goalPos, float minRange, float maxRange, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, TerrainCoord squadLeaderTile, float stayInRangeOfSquadLeaderTile, float minDistFromStart, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		MinRange = minRange;
		MaxRange = maxRange;
		_minRangeSquared = minRange * minRange;
		_maxRangeSquared = maxRange * maxRange;
		SquadLeaderTile = squadLeaderTile;
		StayInRangeOfSquadLeaderTile = stayInRangeOfSquadLeaderTile;
		MinDistFromStart = minDistFromStart;
		GoalPos = goalPos;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, instance.GetTileCoordForPos(goalPos), AStarReachedGoalCondition.WithinRange, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartVisibleAndWithinRangeRequest(TerrainCoord start, Vector3 goalPos, bool targetIsCrouching, float minRange, float maxRange, Character requester, TileObject ignore, AStarPriority priority, bool ignoreFlammableDefences, bool ignoreExplodableDefences, bool dontOpenOurGates, TerrainCoord squadLeaderTile, float stayInRangeOfSquadLeaderTile, float minDistFromStart, bool includeWireFences, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		MinRange = minRange;
		MaxRange = maxRange;
		_minRangeSquared = minRange * minRange;
		_maxRangeSquared = maxRange * maxRange;
		SquadLeaderTile = squadLeaderTile;
		StayInRangeOfSquadLeaderTile = stayInRangeOfSquadLeaderTile;
		MinDistFromStart = minDistFromStart;
		GoalPos = goalPos;
		TargetIsCrouching = targetIsCrouching;
		IgnoreFlammableDefences = ignoreFlammableDefences;
		IgnoreExplodableDefences = ignoreExplodableDefences;
		IncludeWireFences = includeWireFences;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		IgnoreGuardBuilding = ((requester.InsideBuilding != null && requester.InTerrain) ? requester.InsideBuilding : null);
		StartRequest(start, instance.GetTileCoordForPos(goalPos), AStarReachedGoalCondition.VisibleAndWithinRange, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartTakeCoverRequest(TerrainCoord start, Vector3 danger, int maxRangeFromMyCurrentPos, Character requester, TileObject targetObj, AStarPriority priority, bool dontOpenOurGates, TerrainCoord squadLeaderTile, float stayInRangeOfSquadLeaderTile, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = TerrainCoord.Max(start - new TerrainCoord(maxRangeFromMyCurrentPos, maxRangeFromMyCurrentPos), new TerrainCoord(0, 0));
		Max = TerrainCoord.Min(start + new TerrainCoord(maxRangeFromMyCurrentPos, maxRangeFromMyCurrentPos), new TerrainCoord(instance.Size - 1, instance.Size - 1));
		SquadLeaderTile = squadLeaderTile;
		StayInRangeOfSquadLeaderTile = stayInRangeOfSquadLeaderTile;
		GoalPos = danger;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		if (targetObj is Character character)
		{
			IgnoreGuardBuilding = ((character.InsideBuilding != null && character.InTerrain) ? character.InsideBuilding : null);
		}
		StartRequest(start, instance.GetTileCoordForPos(danger), AStarReachedGoalCondition.TakeCover, requester, targetObj, priority, respectMovementZone);
	}

	public void StartFleeRequest(TerrainCoord start, TerrainCoord danger, float minRange, float maxRange, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		MinRange = minRange;
		MaxRange = maxRange;
		_minRangeSquared = minRange * minRange;
		_maxRangeSquared = maxRange * maxRange;
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		StartRequest(start, danger, AStarReachedGoalCondition.Flee, requester, ignore, priority, respectMovementZone);
	}

	public void StartFleeFromAllEnemiesRequest(TerrainCoord start, List<TerrainCoord> dangers, float minRange, float maxRange, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		MinRange = minRange;
		MaxRange = maxRange;
		_minRangeSquared = minRange * minRange;
		_maxRangeSquared = maxRange * maxRange;
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		Dangers = dangers;
		StartRequest(start, start, AStarReachedGoalCondition.FleeMultipleDangers, requester, ignore, priority, respectMovementZone);
	}

	public void StartBuildingEntrancesRequest(TerrainCoord start, TerrainCoord goal, List<TerrainCoord> entrances, Character requester, TileObject ignore, AStarPriority priority, bool dontOpenOurGates, bool respectMovementZone, bool avoidHostileBases, int dontAvoidCommunityId, bool canUnlockGatesFromInsideWithKey = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		IgnoreCharacters = true;
		DontOpenOurGates = dontOpenOurGates;
		AvoidHostileBases = avoidHostileBases;
		DontAvoidCommunityId = dontAvoidCommunityId;
		Dangers = entrances;
		StartRequest(start, goal, AStarReachedGoalCondition.BuildingEntrances, requester, ignore, priority, respectMovementZone, canUnlockGatesFromInsideWithKey);
	}

	public void StartTeleportOffSlopeRequest(TerrainCoord start, Character requester, AStarPriority priority, bool respectMovementZone)
	{
		GameTerrain instance = GameTerrain.Instance;
		Min = new TerrainCoord(0, 0);
		Max = new TerrainCoord(instance.Size - 1, instance.Size - 1);
		SquadLeaderTile = TerrainCoord.Invalid;
		StayInRangeOfSquadLeaderTile = float.MaxValue;
		TargetIsCrouching = false;
		IgnoreFlammableDefences = false;
		IgnoreExplodableDefences = false;
		IncludeWireFences = false;
		DontOpenOurGates = false;
		StartRequest(start, start, AStarReachedGoalCondition.TeleportOffSlope, requester, null, priority, respectMovementZone);
	}

	private void StartRequest(TerrainCoord start, TerrainCoord goal, AStarReachedGoalCondition reachedGoalCondition, Character requester, TileObject ignore, AStarPriority priority, bool respectMovementZone, bool canUnlockGatesFromInsideWithKey = false)
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		Start = start;
		Goal = goal;
		Route.Clear();
		ReachedGoalCondition = reachedGoalCondition;
		_result = AStarResult.Processing;
		StartInputFrame = instance.InputFrame;
		Requester = requester;
		IgnoreObject = ignore;
		Priority = ((priority != AStarPriority.Unknown) ? priority : ((instance.GetPlayerControllingCharacter(requester) != null) ? AStarPriority.High : (requester.InCombat ? AStarPriority.Medium : AStarPriority.Low)));
		CanVaultWaistHighWalls = requester is Human && requester.CarryingObject == null;
		CanUnlockGatesFromInsideWithKey = canUnlockGatesFromInsideWithKey;
		MovementZone = new TerrainRect(new TerrainCoord(0, 0), new TerrainCoord(instance2.Size - 1, instance2.Size - 1));
		if (respectMovementZone && requester.HasMovementZone())
		{
			if (requester.MovementZone.Contains(start))
			{
				Min = TerrainCoord.Max(Min, requester.MovementZone.min);
				Max = TerrainCoord.Min(Max, requester.MovementZone.max);
			}
			else
			{
				MovementZone = requester.MovementZone;
			}
		}
		RequesterInfectionType = Requester.Infection;
		RequesterCommunity = Requester.Community;
		RequesterCanOpenPlayerGates = Requester.CanOpenPlayerGates;
		RequesterIsFollowingDirectControlledPlayer = Requester.SquadLeader != null && Requester.SquadLeader.DirectControlled;
		RequesterInCombat = Requester.InCombat;
		SeparatedByWalls = instance2.IsTileEnclosed(Goal.x, Goal.y) != instance2.IsTileEnclosed(Start.x, Start.y);
		RequesterKeys.Clear();
		foreach (Equipment content in requester.Inventory.Contents)
		{
			if (content.GetPrototype().TypeName == BaseObjectType.GateKey)
			{
				RequesterKeys.Add(content.GetPrototype());
			}
		}
		instance2.AStar.AddRequest(this);
	}

	public void CancelRequest()
	{
		CancelledInputFrame = Session.Instance.InputFrame;
		Discarded = true;
	}

	public void FinishRequest(bool success)
	{
		_result = (success ? AStarResult.Success : AStarResult.Fail);
	}

	public bool IsProcessing()
	{
		return _result == AStarResult.Processing;
	}

	public bool HasReachedGoal(CompressedTerrainCoord tile)
	{
		using (new UnityProfileMarker(HasReachedGoalStr))
		{
			switch (ReachedGoalCondition)
			{
			case AStarReachedGoalCondition.Exact:
				return tile == Goal;
			case AStarReachedGoalCondition.AdjacentTo:
				return tile.IsAdjacent(Goal) && (!(tile == Start) || GameTerrain.Instance.AStar.IsTileImpassable(tile.x, tile.y, CompressedTerrainCoord.Invalid) != AStar.Passableness.Impassable) && !GameTerrain.Instance.AStar.IsWaistHighWallTile(tile.x, tile.y);
			case AStarReachedGoalCondition.AdjacentToButNotOn:
				return tile.IsAdjacent(Goal) && tile != Goal && (!(tile == Start) || GameTerrain.Instance.AStar.IsTileImpassable(tile.x, tile.y, CompressedTerrainCoord.Invalid) != AStar.Passableness.Impassable) && !GameTerrain.Instance.AStar.IsWaistHighWallTile(tile.x, tile.y);
			case AStarReachedGoalCondition.WithinBounds:
				return tile.IsWithinBounds(BoundsMin, BoundsMax) && !GameTerrain.Instance.AStar.IsWaistHighWallTile(tile.x, tile.y);
			case AStarReachedGoalCondition.WithinRange:
			{
				if (!MovementZone.Contains(tile.ToUncompressed()))
				{
					return false;
				}
				if (StayInRangeOfSquadLeaderTile < float.MaxValue && tile.GetDistSquared(SquadLeaderTile) >= StayInRangeOfSquadLeaderTile * StayInRangeOfSquadLeaderTile)
				{
					return false;
				}
				float sqrMagnitude = (GameTerrain.Instance.GetTileCentreXZ(tile.ToUncompressed()) - MathUtil.ToXZ(GoalPos)).sqrMagnitude;
				return sqrMagnitude <= _maxRangeSquared && sqrMagnitude >= _minRangeSquared;
			}
			case AStarReachedGoalCondition.AsCloseAsPossible:
				return tile == Goal;
			case AStarReachedGoalCondition.VisibleAndWithinRange:
			{
				if (!MovementZone.Contains(tile.ToUncompressed()))
				{
					return false;
				}
				if (StayInRangeOfSquadLeaderTile < float.MaxValue && tile.GetDistSquared(SquadLeaderTile) >= StayInRangeOfSquadLeaderTile * StayInRangeOfSquadLeaderTile)
				{
					return false;
				}
				if (MinDistFromStart > 0f && tile.GetDistSquared(Start) < MinDistFromStart * MinDistFromStart)
				{
					return false;
				}
				float sqrMagnitude2 = (GameTerrain.Instance.GetTileCentreXZ(tile.ToUncompressed()) - MathUtil.ToXZ(GoalPos)).sqrMagnitude;
				if (sqrMagnitude2 > _maxRangeSquared || sqrMagnitude2 < _minRangeSquared)
				{
					return false;
				}
				GameTerrain instance2 = GameTerrain.Instance;
				Vector3 vector = GoalPos + new Vector3(0f, TargetIsCrouching ? HumanAppearance.MaleDefaultCrouchingGunHeight : HumanAppearance.MaleDefaultGunHeight, 0f);
				Vector3 vector2 = instance2.GetTileCentrePos(tile.ToUncompressed()) + new Vector3(0f, HumanAppearance.MaleDefaultGunHeight, 0f) - vector;
				float magnitude = vector2.magnitude;
				if (magnitude < 0.1f)
				{
					return true;
				}
				int num = 16384;
				if (IncludeWireFences)
				{
					num |= 0x1000;
				}
				if (IgnoreObject is SingleTileObject)
				{
					num |= 0x40000;
				}
				RaycastResult raycastResult = instance2.RayCastFromAStarThread(new Ray(vector, vector2 / magnitude), magnitude, num, IgnoreGuardBuilding, null, IgnoreObject);
				if (DrawLineOfSightRaycasts)
				{
					DebugGraphics.AddPersistentLine(vector, vector + vector2 * raycastResult.HitDist / magnitude, Color.red);
				}
				return raycastResult.HitObject == null;
			}
			case AStarReachedGoalCondition.TakeCover:
			{
				if (!MovementZone.Contains(tile.ToUncompressed()))
				{
					return false;
				}
				if (StayInRangeOfSquadLeaderTile < float.MaxValue && tile.GetDistSquared(SquadLeaderTile) >= StayInRangeOfSquadLeaderTile * StayInRangeOfSquadLeaderTile)
				{
					return false;
				}
				if (tile == Goal)
				{
					return false;
				}
				GameTerrain instance = GameTerrain.Instance;
				Vector2 dirXZ = instance.GetTileCentreXZ(Goal) - instance.GetTileCentreXZ(tile.ToUncompressed());
				TerrainCoord adjacentTileInDir = tile.ToUncompressed().GetAdjacentTileInDir(dirXZ);
				return instance.AStar.IsCoverTile(adjacentTileInDir.x, adjacentTileInDir.y);
			}
			case AStarReachedGoalCondition.Flee:
				return tile.GetDistSquared(Goal) > _maxRangeSquared;
			case AStarReachedGoalCondition.FleeMultipleDangers:
			{
				for (int l = 0; l < Dangers.Count; l++)
				{
					if (tile.GetDistSquared(Dangers[l]) < _maxRangeSquared)
					{
						return false;
					}
				}
				return true;
			}
			case AStarReachedGoalCondition.BuildingEntrances:
			{
				for (int k = 0; k < Dangers.Count; k++)
				{
					if (tile == Dangers[k])
					{
						return true;
					}
				}
				return tile == Goal;
			}
			case AStarReachedGoalCondition.TeleportOffSlope:
			{
				for (int i = tile.x - 1; i <= tile.x + 1; i++)
				{
					for (int j = tile.y - 1; j <= tile.y + 1; j++)
					{
						if (GameTerrain.Instance.IsSlope(i, j))
						{
							return false;
						}
					}
				}
				return true;
			}
			default:
				return true;
			}
		}
	}

	public float GetMinDistSqFromDangers(TerrainCoord tile)
	{
		float num = float.MaxValue;
		for (int i = 0; i < Dangers.Count; i++)
		{
			num = Mathf.Min(num, tile.GetDistSquared(Dangers[i]));
		}
		return num;
	}
}

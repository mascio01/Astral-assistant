using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public class AStar
{
	public enum Passableness
	{
		Passable,
		Slow,
		WantToAvoid,
		Impassable
	}

	private GameTerrain _terrain;

	public static float[,] VerticesOnThread;

	public static AStarTile[,] Tiles;

	public static AStarLookupSquare[,] LookupSquares;

	public AStarTileContentsManager AStarTileContentsManager;

	private AStarOpenSetItem[] OpenSet = new AStarOpenSetItem[1024];

	private int OpenSetCount;

	private int ClosedSetCount;

	private int MaxOpenSetCount;

	private int OpenSetMask;

	private int ClosedSetMask;

	private int PassableMask;

	private int PassableButSlowMask;

	private int PassableButWantToAvoidMask;

	private int ImpassableMask;

	private int TotalAddSortCost;

	private int MaxAddSortCost;

	private int TotalAddCount;

	private int TotalRemoveSortCost;

	private int MaxRemoveSortCost;

	private int TotalRemoveCount;

	private int TotalUpdateSortCost;

	private int MaxUpdateSortCost;

	private int TotalUpdateCount;

	private Stopwatch _stopwatch = new Stopwatch();

	public CustomRandom AStarRand = new CustomRandom(42);

	private CompressedTerrainCoord[] Neighbours;

	private float[] NeighbourDist;

	private CompressedTerrainCoord LowestHScoreTile;

	private float LowestHScore;

	private int TilesAddedSinceLowestHScore;

	private TimeSpan PoppedTimer;

	private AStarRequester _currentRequest;

	private List<AStarChange> _requestQueue = new List<AStarChange>();

	private List<AStarRequester> _completedRequests = new List<AStarRequester>();

	private AutoResetEvent _event = new AutoResetEvent(initialState: false);

	private Thread _thread;

	private bool _wantQuit;

	public List<PropPrototype> FixedPropPrototypes = new List<PropPrototype>();

	private static string CalcPathStr = "CalcPath";

	public static float ExtraScoreForVaulting = 2f;

	public static float ExtraScoreForWantToAvoidArea = 100f;

	private static string IsTileImpassableInternalStr = "IsTileImpassableInternal";

	public static float MaxCoverGradient = 0.35f;

	public static float MaxWaistHighCoverGradient = 0.35f;

	private bool StartedPlaying;

	private List<OldCommunityRelationship> CachedCommunityRelationships = new List<OldCommunityRelationship>();

	private List<int> CachedSurrenderingCommunityIds = new List<int>();

	private List<int> CachedAllDeadOrUnconsciousCommunityIds = new List<int>();

	private List<int> CachedWarnedAboutTraps = new List<int>();

	private List<int> CachedWarnedAboutTripwires = new List<int>();

	private static float FleeAversionDist = 16f;

	private static float FleeAversionFactor = 16f;

	private static float D2 = (float)Math.Sqrt(2.0);

	private static string CancelledStr = "Cancelled";

	private static string DiscardedStr = "Discarded";

	private static string QueuedHeaderStr = "============================ Queued ================================\n";

	private static string CompletedHeaderStr = "============================ Completed ================================\n";

	private static string ApplyChangeStr = "ApplyChange";

	public AStar(GameTerrain terrain)
	{
		_terrain = terrain;
		AStarTileContentsManager = new AStarTileContentsManager();
		Neighbours = new CompressedTerrainCoord[8];
		Neighbours[0] = new CompressedTerrainCoord(-1, 0);
		Neighbours[1] = new CompressedTerrainCoord(1, 0);
		Neighbours[2] = new CompressedTerrainCoord(0, -1);
		Neighbours[3] = new CompressedTerrainCoord(0, 1);
		Neighbours[4] = new CompressedTerrainCoord(-1, -1);
		Neighbours[5] = new CompressedTerrainCoord(1, -1);
		Neighbours[6] = new CompressedTerrainCoord(-1, 1);
		Neighbours[7] = new CompressedTerrainCoord(1, 1);
		NeighbourDist = new float[8];
		for (int i = 0; i < 8; i++)
		{
			Vector2 vector = new Vector2(Neighbours[i].x, Neighbours[i].y);
			NeighbourDist[i] = vector.magnitude;
		}
	}

	public void InitSize(int size)
	{
	}

	public void Init()
	{
		Array.Copy(GameTerrain.Vertices, VerticesOnThread, VerticesOnThread.Length);
		_thread = new Thread(ThreadFunc);
		_thread.IsBackground = true;
		_thread.Start();
	}

	public void Unload()
	{
		_wantQuit = true;
		_event.Set();
		if (_thread != null)
		{
			_thread.Join();
		}
		Array.Clear(Tiles, 0, Tiles.Length);
		Array.Clear(LookupSquares, 0, LookupSquares.Length);
		Array.Clear(VerticesOnThread, 0, VerticesOnThread.Length);
		AStarTileContentsManager.ClearAllContents();
		AStarTileContentsManager = null;
	}

	public bool CalcPath()
	{
		using (new UnityProfileMarker(CalcPathStr))
		{
			_stopwatch.Reset();
			_stopwatch.Start();
			CompressedTerrainCoord compressedTerrainCoord = _currentRequest.Start.ToCompressed();
			CompressedTerrainCoord compressedTerrainCoord2 = _currentRequest.Goal.ToCompressed();
			OpenSetCount = 0;
			MaxOpenSetCount = 0;
			ClosedSetCount = 0;
			TotalAddSortCost = 0;
			MaxAddSortCost = 0;
			TotalAddCount = 0;
			TotalRemoveSortCost = 0;
			MaxRemoveSortCost = 0;
			TotalRemoveCount = 0;
			TotalUpdateSortCost = 0;
			MaxUpdateSortCost = 0;
			TotalUpdateCount = 0;
			OpenSetMask++;
			ClosedSetMask = OpenSetMask | int.MinValue;
			PassableMask++;
			PassableButWantToAvoidMask = PassableMask | 0x20000000;
			PassableButSlowMask = PassableMask | 0x40000000;
			ImpassableMask = PassableMask | int.MinValue;
			AStarRand.Seed = (uint)(_currentRequest.Start.x | (_currentRequest.Start.y << 16));
			if (_currentRequest.ReachedGoalCondition == AStarReachedGoalCondition.Exact && (_terrain.IsTileOutsideBounds(_currentRequest.Goal.x, _currentRequest.Goal.y) || IsTileImpassable(_currentRequest.Goal.x, _currentRequest.Goal.y, CompressedTerrainCoord.Invalid) == Passableness.Impassable))
			{
				CleanUp(success: false);
				return false;
			}
			float distHeuristic = GetDistHeuristic(compressedTerrainCoord, compressedTerrainCoord2);
			float gScore = 0f;
			AddToOpenSet(compressedTerrainCoord, distHeuristic, gScore, CompressedTerrainCoord.Invalid);
			LowestHScore = distHeuristic;
			LowestHScoreTile = compressedTerrainCoord;
			TilesAddedSinceLowestHScore = 0;
			float distTo = compressedTerrainCoord.GetDistTo(compressedTerrainCoord2);
			while (OpenSetCount > 0)
			{
				AStarOpenSetItem cur = PopFromOpenSet();
				lock (this)
				{
					if (_wantQuit)
					{
						CleanUp(success: false);
						return false;
					}
					_currentRequest.TotalRemovedCountAtFinish = TotalRemoveCount;
					if (_currentRequest.CancelledInputFrame != -1 && (!Session.Instance.IsInMultiplayerGame() || HasPassedDeterministicTime(InputFrameToDeterministicTimeSpan(_currentRequest.CancelledInputFrame))))
					{
						CleanUp(success: false);
						return false;
					}
				}
				while (Session.Instance.DebugPaused && Session.Instance.WantFinish != WantFinishState.None)
				{
					Thread.Sleep(10);
				}
				if (_currentRequest.HasReachedGoal(cur.Tile))
				{
					ReconstructPath(cur.Tile, dontEndOnVault: false);
					CleanUp(success: true);
					return true;
				}
				for (int i = 0; i < 8; i++)
				{
					CompressedTerrainCoord compressedTerrainCoord3 = cur.Tile + Neighbours[i];
					if (compressedTerrainCoord3.x < _currentRequest.Min.x || compressedTerrainCoord3.x > _currentRequest.Max.x || compressedTerrainCoord3.y < _currentRequest.Min.y || compressedTerrainCoord3.y > _currentRequest.Max.y)
					{
						continue;
					}
					int setMask = Tiles[compressedTerrainCoord3.x, compressedTerrainCoord3.y].SetMask;
					if (setMask == ClosedSetMask)
					{
						continue;
					}
					Passableness passableness = IsTileImpassable(compressedTerrainCoord3.x, compressedTerrainCoord3.y, cur.Tile);
					if (passableness == Passableness.Impassable)
					{
						continue;
					}
					if (i >= 4)
					{
						passableness = (Passableness)Math.Max((int)passableness, (int)IsTileImpassable(cur.Tile.x + Neighbours[i].x, cur.Tile.y, cur.Tile));
						if (passableness == Passableness.Impassable)
						{
							continue;
						}
						passableness = (Passableness)Math.Max((int)passableness, (int)IsTileImpassable(cur.Tile.x, cur.Tile.y + Neighbours[i].y, cur.Tile));
						if (passableness == Passableness.Impassable)
						{
							continue;
						}
					}
					if (setMask == OpenSetMask)
					{
						gScore = CalcGScore(cur, i, passableness);
						if (gScore < OpenSet[Tiles[compressedTerrainCoord3.x, compressedTerrainCoord3.y].OpenSetIndex].GScore)
						{
							UpdateGScore(compressedTerrainCoord3, gScore, cur.Tile);
						}
						continue;
					}
					gScore = CalcGScore(cur, i, passableness);
					distHeuristic = GetDistHeuristic(compressedTerrainCoord3, compressedTerrainCoord2);
					AddToOpenSet(compressedTerrainCoord3, distHeuristic, gScore, cur.Tile);
					if (distHeuristic < LowestHScore)
					{
						LowestHScoreTile = compressedTerrainCoord3;
						LowestHScore = distHeuristic;
						TilesAddedSinceLowestHScore = 0;
						continue;
					}
					TilesAddedSinceLowestHScore++;
					float num = 50f;
					if (_currentRequest.RequesterInfectionType != InfectionType.None)
					{
						num /= 5f;
						if (_currentRequest.SeparatedByWalls)
						{
							num /= 10f;
						}
					}
					else if (_currentRequest.RequesterCommunity != null && _currentRequest.RequesterCommunity.CommunityType == CommunityType.Player && _terrain.ComplexPathfinding)
					{
						num = 200f;
					}
					if (!((float)TilesAddedSinceLowestHScore >= Math.Max((int)distTo * 200, (float)_terrain.Size * num)))
					{
						continue;
					}
					return Fail();
				}
			}
			return Fail();
		}
	}

	private bool Fail()
	{
		switch (_currentRequest.ReachedGoalCondition)
		{
		case AStarReachedGoalCondition.AsCloseAsPossible:
			if (LowestHScoreTile.GetDistSquared(_currentRequest.Goal) <= _currentRequest._maxRangeSquared)
			{
				ReconstructPath(LowestHScoreTile, dontEndOnVault: true);
				CleanUp(success: true);
				return true;
			}
			break;
		case AStarReachedGoalCondition.Flee:
			if (LowestHScoreTile.GetDistSquared(_currentRequest.Goal) >= _currentRequest._minRangeSquared)
			{
				ReconstructPath(LowestHScoreTile, dontEndOnVault: true);
				CleanUp(success: true);
				return true;
			}
			break;
		case AStarReachedGoalCondition.FleeMultipleDangers:
			if (_currentRequest.GetMinDistSqFromDangers(LowestHScoreTile.ToUncompressed()) >= _currentRequest._minRangeSquared)
			{
				ReconstructPath(LowestHScoreTile, dontEndOnVault: true);
				CleanUp(success: true);
				return true;
			}
			break;
		}
		CleanUp(success: false);
		return false;
	}

	public Passableness IsTileImpassable(int x, int y, CompressedTerrainCoord from)
	{
		int impassableMask = Tiles[x, y].ImpassableMask;
		if (impassableMask == PassableMask)
		{
			return Passableness.Passable;
		}
		if (impassableMask == ImpassableMask)
		{
			return Passableness.Impassable;
		}
		if (impassableMask == PassableButSlowMask)
		{
			return Passableness.Slow;
		}
		if (impassableMask == PassableButWantToAvoidMask)
		{
			return Passableness.WantToAvoid;
		}
		bool canCacheMe = true;
		Passableness passableness = IsTileImpassableInternal(x, y, from, ref canCacheMe);
		if (canCacheMe)
		{
			switch (passableness)
			{
			case Passableness.Passable:
				Tiles[x, y].ImpassableMask = PassableMask;
				break;
			case Passableness.Slow:
				Tiles[x, y].ImpassableMask = PassableButSlowMask;
				break;
			case Passableness.WantToAvoid:
				Tiles[x, y].ImpassableMask = PassableButWantToAvoidMask;
				break;
			case Passableness.Impassable:
				Tiles[x, y].ImpassableMask = ImpassableMask;
				break;
			}
		}
		return passableness;
	}

	private Passableness IsTileImpassableInternal(int x, int y, CompressedTerrainCoord from, ref bool canCacheMe)
	{
		using (new UnityProfileMarker(IsTileImpassableInternalStr))
		{
			if (_terrain.IsTileOutsideBounds(x, y))
			{
				return Passableness.Impassable;
			}
			if (_currentRequest.ReachedGoalCondition == AStarReachedGoalCondition.TeleportOffSlope)
			{
				if (_terrain.IsImpassableRaw(x, y))
				{
					return Passableness.Impassable;
				}
			}
			else if (_terrain.IsSlopeOrImpassableRaw(x, y))
			{
				return Passableness.Impassable;
			}
			if (!_currentRequest.Requester.CanWalkInRivers() && _terrain.IsTileRiver(x, y))
			{
				return Passableness.Impassable;
			}
			Passableness passableness = Passableness.Passable;
			if (_currentRequest.AvoidHostileBases)
			{
				int ownerCommunityId = LookupSquares[x / 8, y / 8].OwnerCommunityId;
				if (ownerCommunityId != 0 && ownerCommunityId != _currentRequest.DontAvoidCommunityId && GetCachedRelationship(ownerCommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile && !AreAllies(ownerCommunityId, _currentRequest.DontAvoidCommunityId))
				{
					passableness = Passableness.WantToAvoid;
				}
			}
			BaseObjectType fixedObstacleType = (BaseObjectType)Tiles[x, y].FixedObstacleType;
			if (fixedObstacleType != BaseObjectType.Invalid)
			{
				PropPrototype propPrototype = ((Tiles[x, y].FixedObstaclePropProtoIndex != 0) ? GameTerrain.Instance.AStar.FixedPropPrototypes[Tiles[x, y].FixedObstaclePropProtoIndex - 1] : null);
				TileObject tileObject = ((propPrototype != null) ? propPrototype.ProtoInstance : (BaseObjectManager.PrototypeGameObjects[(int)fixedObstacleType] as TileObject));
				if (fixedObstacleType == BaseObjectType.PitTrap)
				{
					if (Tiles[x, y].FixedObstacleModelIndex == 0)
					{
						if (_currentRequest.IgnoreFlammableDefences && tileObject.IsFlammable() && GetCachedRelationship(Tiles[x, y].FixedObstacleCommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
						{
							return passableness;
						}
						if (_currentRequest.IgnoreExplodableDefences && !tileObject.IsExplosionProof() && GetCachedRelationship(Tiles[x, y].FixedObstacleCommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
						{
							return passableness;
						}
						return Passableness.Impassable;
					}
					if (IsAwareOfTrapOnThread(Tiles[x, y].FixedObstacleCommunityId, tripwire: false))
					{
						return Passableness.WantToAvoid;
					}
					return passableness;
				}
				if (_currentRequest.CanVaultWaistHighWalls && tileObject.CoverType == CoverType.WaistHigh)
				{
					return (Passableness)Math.Max((int)passableness, 1);
				}
				if (_currentRequest.IgnoreFlammableDefences && tileObject.IsFlammable() && !tileObject.IsLooterProp() && GetCachedRelationship(Tiles[x, y].FixedObstacleCommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
				{
					return passableness;
				}
				if (_currentRequest.IgnoreExplodableDefences && !tileObject.IsExplosionProof() && !tileObject.IsLooterProp() && GetCachedRelationship(Tiles[x, y].FixedObstacleCommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
				{
					return passableness;
				}
				return Passableness.Impassable;
			}
			List<AStarMoveableObstacle> moveableObstacles = Tiles[x, y].MoveableObstacles;
			if (moveableObstacles != null)
			{
				int num = ((_currentRequest.RequesterCommunity != null) ? _currentRequest.RequesterCommunity.Id : 0);
				foreach (AStarMoveableObstacle item in moveableObstacles)
				{
					if (item.Object == _currentRequest.Requester || item.Object == _currentRequest.IgnoreObject)
					{
						continue;
					}
					if (item.Object is Character)
					{
						if (item.CharacterIsAwake && item.CharacterIsStationary && !_currentRequest.IgnoreCharacters)
						{
							return Passableness.Impassable;
						}
						continue;
					}
					if (item.Object is Campfire)
					{
						return Passableness.Impassable;
					}
					if (item.Object is Tripwire)
					{
						if (IsAwareOfTrapOnThread(item.CommunityId, tripwire: true))
						{
							return Passableness.Impassable;
						}
						continue;
					}
					if (_currentRequest.IgnoreFlammableDefences && item.Object.IsFlammable() && !item.Object.IsLooterProp() && GetCachedRelationship(item.CommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
					{
						return passableness;
					}
					if (_currentRequest.IgnoreExplodableDefences && !item.Object.IsExplosionProof() && !item.Object.IsLooterProp() && GetCachedRelationship(item.CommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile)
					{
						return passableness;
					}
					if (item.Object is Gate gate)
					{
						if (item.GateState == GateState.Open)
						{
							bool flag = false;
							if (_currentRequest.Requester is Animal)
							{
								flag = !item.GateBlockAnimals;
							}
							else
							{
								flag |= _currentRequest.RequesterInfectionType != InfectionType.None;
								flag |= _currentRequest.RequesterIsFollowingDirectControlledPlayer;
								flag |= GetCachedRelationship(item.CommunityId, _currentRequest.RequesterCommunity) == CommunityRelationshipType.Hostile;
							}
							if (flag && new TerrainRect(item.MinTile, item.MaxTile).GetClosestDistSqTo(_currentRequest.Start) <= 256f)
							{
								continue;
							}
						}
						bool flag2 = gate.KeyProto != null && _currentRequest.RequesterKeys.Contains(gate.KeyProto);
						if (item.GateState == GateState.Locked)
						{
							bool flag3 = flag2;
							if (!_currentRequest.CanUnlockGatesFromInsideWithKey)
							{
								flag3 &= gate.IsInFrontOfGate(_terrain.GetTileCentreXZ(from.ToUncompressed()));
							}
							if (!flag3)
							{
								return Passableness.Impassable;
							}
						}
						if (_currentRequest.RequesterInfectionType != InfectionType.None)
						{
							return Passableness.Impassable;
						}
						if (_currentRequest.Requester is Animal)
						{
							return Passableness.Impassable;
						}
						if (gate.AINeverUseMe)
						{
							return Passableness.Impassable;
						}
						if (item.GatePolicy == GatePolicy.NeverOpenable)
						{
							return Passableness.Impassable;
						}
						if (item.GatePolicy == GatePolicy.CompletelyOpen)
						{
							return passableness;
						}
						int communityId = item.CommunityId;
						if (num == communityId && communityId != 0 && (item.GatePolicy == GatePolicy.OpenableByFriendsEvenWhenUnderAttack || item.GatePolicy == GatePolicy.OpenableByCommunityEvenWhenUnderAttack))
						{
							return passableness;
						}
						canCacheMe = false;
						if (communityId != 0 && communityId != num && !CachedAllDeadOrUnconsciousCommunityIds.Contains(communityId) && num != 0 && gate.IsInFrontOfGate(_terrain.GetTileCentreXZ(from.ToUncompressed())))
						{
							if (item.CommunityId == Session.Instance.CommunityManager.PlayerCommunity.Id && _currentRequest.RequesterCommunity != Session.Instance.CommunityManager.PlayerCommunity)
							{
								if (_currentRequest.RequesterCanOpenPlayerGates == CanOpenGates.No)
								{
									return Passableness.Impassable;
								}
								if (_currentRequest.RequesterCanOpenPlayerGates == CanOpenGates.Unknown && _currentRequest.RequesterInCombat)
								{
									return Passableness.Impassable;
								}
							}
							CommunityRelationshipType cachedRelationship = GetCachedRelationship(item.CommunityId, _currentRequest.RequesterCommunity);
							if (cachedRelationship == CommunityRelationshipType.Hostile && !flag2 && !CachedSurrenderingCommunityIds.Contains(communityId))
							{
								return Passableness.Impassable;
							}
							if (num == Session.Instance.CommunityManager.PlayerCommunity.Id && !flag2 && cachedRelationship <= CommunityRelationshipType.Introducing)
							{
								return Passableness.Impassable;
							}
							if (item.GatePolicy == GatePolicy.OpenableByCommunityEvenWhenUnderAttack || item.GatePolicy == GatePolicy.OpenableByCommunityExceptWhenInsideAndUnderAttack)
							{
								return Passableness.Impassable;
							}
						}
						if (_currentRequest.DontOpenOurGates && communityId != 0 && communityId == num)
						{
							switch (gate.Orientation)
							{
							case Prop.OrientationType.Deg0:
								return (from.x < x) ? Passableness.Impassable : passableness;
							case Prop.OrientationType.Deg180:
								return (from.x > x) ? Passableness.Impassable : passableness;
							case Prop.OrientationType.Deg90:
								return (from.y > y) ? Passableness.Impassable : passableness;
							case Prop.OrientationType.Deg270:
								return (from.y < y) ? Passableness.Impassable : passableness;
							}
						}
						continue;
					}
					if (_currentRequest.CanVaultWaistHighWalls && item.Object.CoverType == CoverType.WaistHigh)
					{
						return (Passableness)Math.Max((int)passableness, 1);
					}
					return Passableness.Impassable;
				}
			}
			return passableness;
		}
	}

	public bool IsAwareOfTrapOnThread(int trapCommunityId, bool tripwire)
	{
		if (_currentRequest.RequesterCommunity == null)
		{
			return false;
		}
		if (_currentRequest.RequesterInfectionType > InfectionType.None && _currentRequest.RequesterInfectionType < InfectionType.Invisible)
		{
			return false;
		}
		if (trapCommunityId == _currentRequest.RequesterCommunity.Id)
		{
			return false;
		}
		if (tripwire)
		{
			if (CachedWarnedAboutTripwires.Contains(_currentRequest.RequesterCommunity.Id))
			{
				return true;
			}
		}
		else if (CachedWarnedAboutTraps.Contains(_currentRequest.RequesterCommunity.Id))
		{
			return true;
		}
		return false;
	}

	public bool IsWaistHighWallTile(int x, int y)
	{
		if (_terrain.IsTileOutsideBounds(x, y))
		{
			return true;
		}
		if (Tiles[x, y].FixedObstaclePropProtoIndex != 0)
		{
			PropPrototype propPrototype = GameTerrain.Instance.AStar.FixedPropPrototypes[Tiles[x, y].FixedObstaclePropProtoIndex - 1];
			if (propPrototype != null && propPrototype.CoverType == CoverType.WaistHigh)
			{
				return true;
			}
		}
		List<AStarMoveableObstacle> moveableObstacles = Tiles[x, y].MoveableObstacles;
		if (moveableObstacles != null)
		{
			foreach (AStarMoveableObstacle item in moveableObstacles)
			{
				if (item.Object.CoverType == CoverType.WaistHigh)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCover(CoverType coverType, TerrainCoord tile, float h)
	{
		switch (coverType)
		{
		case CoverType.Full:
		{
			Vector3 v = _currentRequest.GoalPos + new Vector3(0f, HumanAppearance.MaleDefaultGunHeight, 0f);
			Vector2 tileCentreXZ = _terrain.GetTileCentreXZ(tile);
			return v.y - h < MaxCoverGradient * (MathUtil.ToXZ(v) - tileCentreXZ).magnitude;
		}
		case CoverType.WaistHigh:
		{
			Vector3 goalPos = _currentRequest.GoalPos;
			Vector3 aStarTileCentrePos = _terrain.GetAStarTileCentrePos(tile);
			return goalPos.y - aStarTileCentrePos.y < MaxWaistHighCoverGradient * MathUtil.ToXZ(goalPos - aStarTileCentrePos).magnitude;
		}
		default:
			return false;
		}
	}

	public bool IsCoverTile(int x, int y)
	{
		if (!_terrain.IsTileOutsideBounds(x, y))
		{
			BaseObjectType fixedObstacleType = (BaseObjectType)Tiles[x, y].FixedObstacleType;
			if (fixedObstacleType != BaseObjectType.Invalid)
			{
				PropPrototype propPrototype = ((Tiles[x, y].FixedObstaclePropProtoIndex != 0) ? GameTerrain.Instance.AStar.FixedPropPrototypes[Tiles[x, y].FixedObstaclePropProtoIndex - 1] : null);
				if (((propPrototype != null) ? propPrototype.ProtoInstance : BaseObjectManager.PrototypeGameObjects[(int)fixedObstacleType]) is TileObject { CoverType: not CoverType.None } tileObject)
				{
					PrefabResource unityModel = tileObject.GetUnityModel();
					if (IsCover(tileObject.CoverType, new TerrainCoord(x, y), GameTerrain.Instance.GetAStarTileHeightAtPos(x, y) + (unityModel?.Bounds.max.y ?? 0f)))
					{
						return true;
					}
				}
			}
			List<AStarMoveableObstacle> moveableObstacles = Tiles[x, y].MoveableObstacles;
			if (moveableObstacles != null)
			{
				foreach (AStarMoveableObstacle item in moveableObstacles)
				{
					if ((item.OccupiedByCommunityId == 0 || GetCachedRelationship(item.OccupiedByCommunityId, _currentRequest.RequesterCommunity) != CommunityRelationshipType.Hostile) && IsCover(item.Object.CoverType, new TerrainCoord(x, y), item.PropBoundingBox.max.y))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void OnStartPlaying()
	{
		OldCommunityRelationship item = default(OldCommunityRelationship);
		foreach (Community community in Session.Instance.CommunityManager.Communities)
		{
			community.CachedSurrendering = community.IsSurrendering();
			community.CachedAllDeadOrUnconscious = !community.IsAnyoneConscious(includeDrunk: true);
			if (community.CachedSurrendering)
			{
				CachedSurrenderingCommunityIds.Add(community.Id);
			}
			if (community.CachedAllDeadOrUnconscious)
			{
				CachedAllDeadOrUnconsciousCommunityIds.Add(community.Id);
			}
			if (community.WarnedAboutTraps)
			{
				CachedWarnedAboutTraps.Add(community.Id);
			}
			if (community.WarnedAboutTripwires)
			{
				CachedWarnedAboutTripwires.Add(community.Id);
			}
			for (int i = 0; i < community.CommunityRelationships.Count; i++)
			{
				if (community.CommunityRelationships[i].Initiator)
				{
					item.Community1Id = community.Id;
					item.Community2Id = community.CommunityRelationships[i].OtherCommunityId;
					item.RelationshipType = community.CommunityRelationships[i].RelationshipType;
					CachedCommunityRelationships.Add(item);
				}
			}
		}
		StartedPlaying = true;
	}

	public CommunityRelationshipType GetCachedRelationship(int communityId1, Community community2)
	{
		if (community2 == null || communityId1 == 0)
		{
			return CommunityRelationshipType.Unknown;
		}
		if (community2.IsAlwaysHostileCommunity())
		{
			return CommunityRelationshipType.Hostile;
		}
		for (int i = 0; i < CachedCommunityRelationships.Count; i++)
		{
			if (CachedCommunityRelationships[i].IsBetween(communityId1, community2.Id))
			{
				return CachedCommunityRelationships[i].RelationshipType;
			}
		}
		return CommunityRelationshipType.Unknown;
	}

	public bool AreAllies(int communityId1, int communityId2)
	{
		if (communityId2 == 0 || communityId1 == 0)
		{
			return false;
		}
		for (int i = 0; i < CachedCommunityRelationships.Count; i++)
		{
			if (CachedCommunityRelationships[i].IsBetween(communityId1, communityId2))
			{
				return CachedCommunityRelationships[i].RelationshipType == CommunityRelationshipType.Allied;
			}
		}
		return false;
	}

	private void CleanUp(bool success)
	{
		_stopwatch.Stop();
	}

	public float CalcGScore(AStarOpenSetItem cur, int neighbourIndex, Passableness passableness)
	{
		float num = cur.GScore + NeighbourDist[neighbourIndex];
		switch (passableness)
		{
		case Passableness.Slow:
			num += ExtraScoreForVaulting;
			break;
		case Passableness.WantToAvoid:
			num += ExtraScoreForWantToAvoidArea;
			break;
		}
		switch (_currentRequest.ReachedGoalCondition)
		{
		case AStarReachedGoalCondition.TeleportOffSlope:
		{
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord terrainCoord = cur.Tile.ToUncompressed();
			TerrainCoord terrainCoord2 = Neighbours[neighbourIndex].ToUncompressed();
			Vector2 tileCentreXZ = instance.GetTileCentreXZ(terrainCoord);
			Vector2 tileCentreXZ2 = instance.GetTileCentreXZ(terrainCoord + terrainCoord2);
			float aStarTileHeightAtPos = instance.GetAStarTileHeightAtPos(tileCentreXZ.x, tileCentreXZ.y);
			float aStarTileHeightAtPos2 = instance.GetAStarTileHeightAtPos(tileCentreXZ2.x, tileCentreXZ2.y);
			num += Math.Max(0f, aStarTileHeightAtPos2 - aStarTileHeightAtPos) * 10f;
			break;
		}
		case AStarReachedGoalCondition.Flee:
		{
			TerrainCoord other = cur.Tile.ToUncompressed() + Neighbours[neighbourIndex].ToUncompressed();
			num += MathUtil.Squared(1f - Mathf.Clamp01(Mathf.Sqrt(_currentRequest.Goal.GetDistSquared(other)) / FleeAversionDist)) * FleeAversionFactor;
			break;
		}
		case AStarReachedGoalCondition.FleeMultipleDangers:
		{
			TerrainCoord tile = cur.Tile.ToUncompressed() + Neighbours[neighbourIndex].ToUncompressed();
			num += MathUtil.Squared(1f - Mathf.Clamp01(Mathf.Sqrt(_currentRequest.GetMinDistSqFromDangers(tile)) / FleeAversionDist)) * FleeAversionFactor;
			break;
		}
		}
		return num;
	}

	public static float GetChessKingDist(TerrainCoord a, TerrainCoord b)
	{
		float num = Math.Abs(a.x - b.x);
		float num2 = Math.Abs(a.y - b.y);
		float num3 = Math.Min(num, num2);
		float num4 = num + num2;
		return D2 * num3 + (num4 - 2f * num3);
	}

	public float GetDistHeuristic(CompressedTerrainCoord tile, CompressedTerrainCoord goal)
	{
		if (_currentRequest.ReachedGoalCondition == AStarReachedGoalCondition.FleeMultipleDangers)
		{
			return 0f - Mathf.Sqrt(_currentRequest.GetMinDistSqFromDangers(tile.ToUncompressed()));
		}
		int num = Math.Abs(goal.x - tile.x);
		int num2 = Math.Abs(goal.y - tile.y);
		int num3 = Math.Min(num, num2);
		int num4 = num + num2;
		float num5 = D2 * (float)num3 + (float)(num4 - 2 * num3);
		switch (_currentRequest.ReachedGoalCondition)
		{
		case AStarReachedGoalCondition.WithinRange:
		case AStarReachedGoalCondition.VisibleAndWithinRange:
			num5 = Math.Abs(num5 - _currentRequest.MinRange);
			if (_currentRequest.StayInRangeOfSquadLeaderTile < float.MaxValue)
			{
				num5 += Math.Max(0f, tile.GetDistTo(_currentRequest.SquadLeaderTile.ToCompressed()) - _currentRequest.StayInRangeOfSquadLeaderTile);
			}
			break;
		case AStarReachedGoalCondition.TakeCover:
			num5 = GetChessKingDist(_currentRequest.Start, tile.ToUncompressed()) - num5 * 0.1f;
			if (_currentRequest.StayInRangeOfSquadLeaderTile < float.MaxValue)
			{
				num5 += Math.Max(0f, tile.GetDistTo(_currentRequest.SquadLeaderTile.ToCompressed()) - _currentRequest.StayInRangeOfSquadLeaderTile);
			}
			break;
		case AStarReachedGoalCondition.Flee:
			num5 = 0f - num5;
			break;
		}
		return num5;
	}

	private bool AreInSameRow(TerrainCoord a, TerrainCoord b, TerrainCoord c)
	{
		if (a.x == b.x && b.x == c.x)
		{
			return true;
		}
		if (a.y == b.y && b.y == c.y)
		{
			return true;
		}
		if (a.x - b.x == a.y - b.y && c.x - b.x == c.y - b.y)
		{
			return true;
		}
		if (b.x - a.x == a.y - b.y && b.x - c.x == c.y - b.y)
		{
			return true;
		}
		return false;
	}

	public void ReconstructPath(CompressedTerrainCoord i, bool dontEndOnVault)
	{
		List<TerrainCoord> route = _currentRequest.Route;
		while (i != CompressedTerrainCoord.Invalid)
		{
			CompressedTerrainCoord cameFrom = Tiles[i.x, i.y].CameFrom;
			if (dontEndOnVault)
			{
				if (Tiles[i.x, i.y].ImpassableMask == PassableButSlowMask)
				{
					i = cameFrom;
					continue;
				}
				dontEndOnVault = false;
			}
			if (cameFrom != CompressedTerrainCoord.Invalid && (route.Count == 0 || !AreInSameRow(route[route.Count - 1], i.ToUncompressed(), cameFrom.ToUncompressed())))
			{
				route.Add(i.ToUncompressed());
			}
			i = cameFrom;
		}
		route.Reverse();
	}

	private void SwitchElements(int i, int j)
	{
		AStarOpenSetItem aStarOpenSetItem = OpenSet[i];
		OpenSet[i] = OpenSet[j];
		OpenSet[j] = aStarOpenSetItem;
		CompressedTerrainCoord tile = OpenSet[i].Tile;
		CompressedTerrainCoord tile2 = OpenSet[j].Tile;
		Tiles[tile.x, tile.y].OpenSetIndex = i;
		Tiles[tile2.x, tile2.y].OpenSetIndex = j;
	}

	public void AddToOpenSet(CompressedTerrainCoord tile, float hScore, float gScore, CompressedTerrainCoord cameFrom)
	{
		float fScore = gScore + hScore;
		int num = OpenSetCount;
		Tiles[tile.x, tile.y].SetMask = OpenSetMask;
		Tiles[tile.x, tile.y].OpenSetIndex = num;
		Tiles[tile.x, tile.y].CameFrom = cameFrom;
		if (OpenSet.Length <= num)
		{
			AStarOpenSetItem[] array = new AStarOpenSetItem[OpenSet.Length + 1024];
			OpenSet.CopyTo(array, 0);
			OpenSet = array;
		}
		OpenSet[num] = new AStarOpenSetItem(fScore, gScore, tile);
		OpenSetCount++;
		MaxOpenSetCount = Math.Max(OpenSetCount, MaxOpenSetCount);
		int num2 = 0;
		while (num != 0)
		{
			int num3 = (num - 1) / 2;
			if (!(OpenSet[num].FScore < OpenSet[num3].FScore))
			{
				break;
			}
			SwitchElements(num, num3);
			num = num3;
			num2++;
		}
		TotalAddCount++;
		TotalAddSortCost += num2;
		MaxAddSortCost = Math.Max(MaxAddSortCost, num2);
	}

	public AStarOpenSetItem PopFromOpenSet()
	{
		AStarOpenSetItem result = OpenSet[0];
		CompressedTerrainCoord tile = result.Tile;
		Tiles[tile.x, tile.y].SetMask = ClosedSetMask;
		OpenSetCount--;
		ClosedSetCount++;
		OpenSet[0] = OpenSet[OpenSetCount];
		CompressedTerrainCoord tile2 = OpenSet[0].Tile;
		Tiles[tile2.x, tile2.y].OpenSetIndex = 0;
		int num = 0;
		int num2 = 0;
		while (true)
		{
			int num3 = num2;
			int num4 = 2 * num2 + 1;
			int num5 = 2 * num2 + 2;
			if (OpenSetCount > num4 && OpenSet[num2].FScore > OpenSet[num4].FScore)
			{
				num2 = num4;
			}
			if (OpenSetCount > num5 && OpenSet[num2].FScore > OpenSet[num5].FScore)
			{
				num2 = num5;
			}
			if (num2 == num3)
			{
				break;
			}
			SwitchElements(num2, num3);
			num++;
		}
		TotalRemoveCount++;
		TotalRemoveSortCost += num;
		MaxRemoveSortCost = Math.Max(MaxRemoveSortCost, num);
		return result;
	}

	public void UpdateGScore(CompressedTerrainCoord tile, float gScore, CompressedTerrainCoord cameFrom)
	{
		Tiles[tile.x, tile.y].CameFrom = cameFrom;
		int num = Tiles[tile.x, tile.y].OpenSetIndex;
		OpenSet[num].FScore += gScore - OpenSet[num].GScore;
		OpenSet[num].GScore = gScore;
		int num2 = 0;
		while (num != 0)
		{
			int num3 = (num - 1) / 2;
			if (!(OpenSet[num].FScore < OpenSet[num3].FScore))
			{
				break;
			}
			SwitchElements(num, num3);
			num = num3;
			num2++;
		}
		TotalUpdateCount++;
		TotalUpdateSortCost += num2;
		MaxUpdateSortCost = Math.Max(MaxUpdateSortCost, num2);
	}

	public void PrintRequest(StringBuilder sb, AStarRequester request)
	{
		sb.AppendWithoutGarbage(request.Requester.Id);
		sb.Append(':');
		sb.Append(' ');
		request.Requester.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
		if (request.Requester.GetGoal() != null)
		{
			sb.Append(' ');
			sb.Append('-');
			sb.Append(' ');
			request.Requester.GetGoal().BuildDebugString(request.Requester, null, sb);
		}
		sb.Append(',');
		sb.Append(' ');
		sb.AppendWithoutGarbage(request.TotalRemovedCountAtFinish);
		if (request.Discarded)
		{
			sb.Append(',');
			sb.Append(' ');
			sb.Append(DiscardedStr);
		}
		if (request.CancelledInputFrame != -1)
		{
			sb.Append(',');
			sb.Append(' ');
			sb.Append(CancelledStr);
		}
		sb.Append('\n');
	}

	public int PrintRequestQueue(StringBuilder sb)
	{
		lock (this)
		{
			int num = 0;
			if (_currentRequest != null)
			{
				PrintRequest(sb, _currentRequest);
				num++;
			}
			sb.Append(QueuedHeaderStr);
			num++;
			for (int i = 0; i < _requestQueue.Count; i++)
			{
				if (_requestQueue[i].ChangeType == AStarChange.Type.Request)
				{
					PrintRequest(sb, _requestQueue[i].Requester);
					num++;
				}
			}
			if (Session.Instance.IsInMultiplayerGame())
			{
				sb.Append(CompletedHeaderStr);
				num++;
				TimeSpan deterministicTimer = PoppedTimer;
				for (int j = 0; j < _completedRequests.Count; j++)
				{
					AStarRequester aStarRequester = _completedRequests[j];
					AddDeterministicTime(aStarRequester, ref deterministicTimer);
					if (!aStarRequester.Discarded)
					{
						sb.AppendWithoutGarbage((float)deterministicTimer.TotalSeconds, 1);
						sb.Append(' ');
						if (deterministicTimer >= InputFrameToDeterministicTimeSpan(Session.Instance.InputFrame))
						{
							sb.Append('!');
							sb.Append('!');
							sb.Append('!');
						}
						PrintRequest(sb, aStarRequester);
						num++;
					}
				}
			}
			return num;
		}
	}

	public void AddChange(AStarChange change)
	{
		if (Session.Instance.Editor)
		{
			return;
		}
		if (!StartedPlaying)
		{
			ApplyChange(ref change);
			return;
		}
		lock (this)
		{
			_requestQueue.Add(change);
			_event.Set();
		}
	}

	public void AddRequest(AStarRequester request)
	{
		AStarChange item = AStarChange.Request(request);
		lock (this)
		{
			_requestQueue.Add(item);
			_event.Set();
		}
	}

	public void FinishRequest(bool result)
	{
		lock (this)
		{
			if (Session.Instance.IsInMultiplayerGame())
			{
				_completedRequests.Add(_currentRequest);
			}
			_currentRequest.TotalRemovedCountAtFinish = TotalRemoveCount;
			_currentRequest.FinishRequest(result);
			_currentRequest = null;
		}
	}

	private void AddDeterministicTime(AStarRequester request, ref TimeSpan deterministicTimer)
	{
		deterministicTimer = MathUtil.Max(deterministicTimer, InputFrameToDeterministicTimeSpan(request.StartInputFrame));
		TimeSpan timeSpan = RemovedCountToDeterministicTimeSpan(request.TotalRemovedCountAtFinish);
		if (request.CancelledInputFrame != -1)
		{
			deterministicTimer = MathUtil.Min(deterministicTimer + timeSpan, MathUtil.Max(deterministicTimer, InputFrameToDeterministicTimeSpan(request.CancelledInputFrame)));
		}
		else
		{
			deterministicTimer += timeSpan;
		}
	}

	public bool HasPassedDeterministicTime(TimeSpan testTime)
	{
		lock (this)
		{
			TimeSpan deterministicTimer = PoppedTimer;
			for (int i = 0; i < _completedRequests.Count; i++)
			{
				AStarRequester request = _completedRequests[i];
				AddDeterministicTime(request, ref deterministicTimer);
			}
			if (_currentRequest != null)
			{
				AddDeterministicTime(_currentRequest, ref deterministicTimer);
			}
			else if (_requestQueue.Count == 0)
			{
				return true;
			}
			return deterministicTimer >= testTime;
		}
	}

	public bool PopCompletedRequest(AStarRequester request)
	{
		lock (this)
		{
			bool flag = true;
			TimeSpan deterministicTimer = PoppedTimer;
			for (int i = 0; i < _completedRequests.Count; i++)
			{
				AStarRequester aStarRequester = _completedRequests[i];
				AddDeterministicTime(aStarRequester, ref deterministicTimer);
				if (aStarRequester == request)
				{
					if (deterministicTimer >= InputFrameToDeterministicTimeSpan(Session.Instance.InputFrame))
					{
						return false;
					}
					_completedRequests[i].Discarded = true;
					if (flag)
					{
						PoppedTimer = deterministicTimer;
						_completedRequests.RemoveRange(0, i + 1);
					}
					return true;
				}
				if (!_completedRequests[i].Discarded)
				{
					flag = false;
				}
			}
			return false;
		}
	}

	public bool GetNextRequest()
	{
		lock (this)
		{
			while (_requestQueue.Count > 0)
			{
				if (Session.Instance.IsInMultiplayerGame())
				{
					AStarChange change = _requestQueue[0];
					_requestQueue.RemoveAt(0);
					if (ApplyChange(ref change))
					{
						return true;
					}
					continue;
				}
				for (int i = 0; i < _requestQueue.Count; i++)
				{
					if (_requestQueue[i].ChangeType != AStarChange.Type.Request)
					{
						AStarChange change2 = _requestQueue[i];
						_requestQueue.RemoveAt(i);
						i--;
						ApplyChange(ref change2);
					}
				}
				int num = -1;
				for (int j = 0; j < _requestQueue.Count; j++)
				{
					if (num == -1 || _requestQueue[j].Requester.Priority > _requestQueue[num].Requester.Priority)
					{
						num = j;
					}
				}
				if (num != -1)
				{
					AStarChange change3 = _requestQueue[num];
					_requestQueue.RemoveAt(num);
					ApplyChange(ref change3);
					return true;
				}
			}
			return false;
		}
	}

	public static TimeSpan InputFrameToDeterministicTimeSpan(int inputFrame)
	{
		return TimeSpan.FromTicks(166666L * (long)inputFrame);
	}

	public static TimeSpan RemovedCountToDeterministicTimeSpan(int removedCount)
	{
		long num = 100000L;
		if (GameTerrain.Instance.Size > 1024)
		{
			num *= 4;
		}
		return TimeSpan.FromTicks(removedCount * (10000000 / num));
	}

	private bool ApplyChange(ref AStarChange change)
	{
		using (new UnityProfileMarker(ApplyChangeStr))
		{
			switch (change.ChangeType)
			{
			case AStarChange.Type.Request:
				_currentRequest = change.Requester;
				TotalRemoveCount = 0;
				return true;
			case AStarChange.Type.AddedFixedContents:
				if (change.Object is MultiTileObject)
				{
					AddMoveableObstacle(ref change);
				}
				else
				{
					for (int m = change.MinTile.x; m <= change.MaxTile.x; m++)
					{
						for (int n = change.MinTile.y; n <= change.MaxTile.y; n++)
						{
							Tiles[m, n].FixedObstacleType = (byte)change.Object.GetBaseObjectType();
							Tiles[m, n].FixedObstacleCommunityId = change.CommunityId;
							Tiles[m, n].FixedObstacleModelIndex = change.ModelIndex;
							Tiles[m, n].FixedObstaclePropProtoIndex = change.FixedObstaclePropProtoIndex;
						}
					}
				}
				return false;
			case AStarChange.Type.RemovedFixedContents:
				if (change.Object is MultiTileObject)
				{
					RemoveMoveableObstacle(ref change);
				}
				else
				{
					for (int k = change.MinTile.x; k <= change.MaxTile.x; k++)
					{
						for (int l = change.MinTile.y; l <= change.MaxTile.y; l++)
						{
							Tiles[k, l].FixedObstacleType = 0;
							Tiles[k, l].FixedObstacleCommunityId = 0;
							Tiles[k, l].FixedObstacleModelIndex = 0;
							Tiles[k, l].FixedObstaclePropProtoIndex = 0;
						}
					}
				}
				return false;
			case AStarChange.Type.AddCharacter:
				AddMoveableObstacle(ref change);
				return false;
			case AStarChange.Type.RemoveCharacter:
				RemoveMoveableObstacle(ref change);
				return false;
			case AStarChange.Type.SetCommunityRelationship:
			{
				CommunityManager.ApplyRelationship(CachedCommunityRelationships, change.Relationship, out var _);
				return false;
			}
			case AStarChange.Type.SetCommunityAllDeadOrUnconscious:
				if (change.Val)
				{
					if (!CachedAllDeadOrUnconsciousCommunityIds.Contains(change.CommunityId))
					{
						CachedAllDeadOrUnconsciousCommunityIds.Add(change.CommunityId);
					}
				}
				else
				{
					CachedAllDeadOrUnconsciousCommunityIds.Remove(change.CommunityId);
				}
				return false;
			case AStarChange.Type.SetCommunitySurrendering:
				if (change.Val)
				{
					if (!CachedSurrenderingCommunityIds.Contains(change.CommunityId))
					{
						CachedSurrenderingCommunityIds.Add(change.CommunityId);
					}
				}
				else
				{
					CachedSurrenderingCommunityIds.Remove(change.CommunityId);
				}
				return false;
			case AStarChange.Type.WarnAboutTraps:
				CachedWarnedAboutTraps.Add(change.CommunityId);
				return false;
			case AStarChange.Type.WarnAboutTripwires:
				CachedWarnedAboutTripwires.Add(change.CommunityId);
				return false;
			case AStarChange.Type.FlattenTerrain:
				GameTerrain.Instance.ApplyFlattenPatch(change.Bounds, change.MinTile, change.MaxTile, change.FlattenHeight, change.FlattenBorder, change.BuildingId, VerticesOnThread, onThread: true);
				return false;
			case AStarChange.Type.AddDitch:
				GameTerrain.Instance.ApplyDitch(change.Bounds, change.MinTile, change.MaxTile, change.DitchHeight, VerticesOnThread);
				return false;
			case AStarChange.Type.TerrainHeightChange:
			{
				for (int i = change.MinTile.x; i <= change.MaxTile.x + 1; i++)
				{
					for (int j = change.MinTile.y; j <= change.MaxTile.y + 1; j++)
					{
						VerticesOnThread[i, j] = change.TerrrainPatch[i - change.MinTile.x, j - change.MinTile.y];
					}
				}
				return false;
			}
			case AStarChange.Type.SetLookupSquareOwnerCommunityId:
				LookupSquares[change.MinTile.x, change.MinTile.y].OwnerCommunityId = change.OccupiedByCommunityId;
				return false;
			default:
				return false;
			}
		}
	}

	private void AddMoveableObstacle(ref AStarChange change)
	{
		bool flag = false;
		AStarTileContentsManager aStarTileContentsManager = GameTerrain.Instance.AStar.AStarTileContentsManager;
		if (aStarTileContentsManager.AllMoveableObstacles.TryGetValue(change.Object.Id, out var value))
		{
			if (value.MinTile != change.MinTile || value.MaxTile != change.MaxTile)
			{
				for (int i = value.MinTile.x; i <= value.MaxTile.x; i++)
				{
					for (int j = value.MinTile.y; j <= value.MaxTile.y; j++)
					{
						if (!_terrain.IsTileOutsideBounds(i, j))
						{
							Tiles[i, j].RemoveMoveableObstacle(value);
						}
					}
				}
				flag = true;
			}
		}
		else
		{
			value = new AStarMoveableObstacle();
			value.Object = change.Object;
			aStarTileContentsManager.AllMoveableObstacles[change.Object.Id] = value;
			flag = true;
		}
		value.CommunityId = change.CommunityId;
		value.OccupiedByCommunityId = change.OccupiedByCommunityId;
		value.PropWorldMatrix = change.PropWorldMatrix;
		value.PropBoundingBox = change.PropBoundingBox;
		value.PropModel = change.PropModel;
		value.GateState = change.GateState;
		value.GatePolicy = change.GatePolicy;
		value.GateBlockAnimals = change.GateBlockAnimals;
		value.CharacterIsStationary = change.CharacterIsStationary;
		value.CharacterIsAwake = change.CharacterIsAwake;
		value.MinTile = change.MinTile;
		value.MaxTile = change.MaxTile;
		if (!flag)
		{
			return;
		}
		for (int k = value.MinTile.x; k <= value.MaxTile.x; k++)
		{
			for (int l = value.MinTile.y; l <= value.MaxTile.y; l++)
			{
				if (!_terrain.IsTileOutsideBounds(k, l))
				{
					Tiles[k, l].AddMoveableObstacle(value);
				}
			}
		}
	}

	private void RemoveMoveableObstacle(ref AStarChange change)
	{
		AStarTileContentsManager aStarTileContentsManager = GameTerrain.Instance.AStar.AStarTileContentsManager;
		if (!aStarTileContentsManager.AllMoveableObstacles.TryGetValue(change.Object.Id, out var value))
		{
			return;
		}
		for (int i = value.MinTile.x; i <= value.MaxTile.x; i++)
		{
			for (int j = value.MinTile.y; j <= value.MaxTile.y; j++)
			{
				if (!_terrain.IsTileOutsideBounds(i, j))
				{
					Tiles[i, j].RemoveMoveableObstacle(value);
				}
			}
		}
		aStarTileContentsManager.AllMoveableObstacles.Remove(change.Object.Id);
	}

	public void ThreadFunc()
	{
		try
		{
			Util.SetFloatingPointControl();
			Util.CheckFloatingPointControl();
			while (!_wantQuit)
			{
				_event.WaitOne();
				while (GetNextRequest())
				{
					Util.SetFloatingPointControl();
					Util.CheckFloatingPointControl();
					FinishRequest(CalcPath());
					Util.CheckFloatingPointControl();
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.Message + ex.StackTrace);
		}
		Profiler.EndThreadProfiling();
	}
}

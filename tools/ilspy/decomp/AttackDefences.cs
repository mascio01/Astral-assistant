using System;
using System.Collections.Generic;

public class AttackDefences : StateMachineGoal
{
	private float MinRange;

	private float MaxRange;

	private MovementType _movementTypeWhenNotInDanger = MovementType.Jog;

	public bool Success;

	private AStarRequester _requester;

	private StayInRangeParams StayInRangeParams;

	public override GoalType GetGoalType()
	{
		return GoalType.AttackDefences;
	}

	public AttackDefences()
	{
	}

	public AttackDefences(float minRange, float maxRange, MovementType movementTypeWhenNotInDanger, StayInRangeParams stayInRangeParams)
	{
		MinRange = minRange;
		MaxRange = maxRange;
		MinRange = Math.Max(0f, Math.Min(MinRange, MaxRange - 1f));
		_movementTypeWhenNotInDanger = movementTypeWhenNotInDanger;
		StayInRangeParams = stayInRangeParams;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MinRange);
		reflector.Add(ref MaxRange);
		reflector.Add(ref _movementTypeWhenNotInDanger);
		reflector.Add(ref Success);
		StayInRangeParams.Reflect(reflector);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new FaceTarget(aiming: true));
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (_requester != null)
		{
			_requester.CancelRequest();
			_requester = null;
		}
		base.OnDeactivate(character, parent);
	}

	private TileObject GetFirstDefenceOnRoute(Character character)
	{
		GameTerrain instance = GameTerrain.Instance;
		bool flag = character.HasMolotovCocktail();
		bool flag2 = character.HasExplosives();
		TerrainCoord tile = character.Tile;
		for (int i = 0; i < _requester.Route.Count; i++)
		{
			TerrainCoord terrainCoord = _requester.Route[i];
			while (tile != terrainCoord)
			{
				tile += tile.GetDirTo(terrainCoord);
				if (instance.IsTileOutsideBounds(tile.x, tile.y))
				{
					continue;
				}
				List<TileObject> objectsInLookupSquare = instance.GetObjectsInLookupSquare(tile.x, tile.y);
				if (objectsInLookupSquare == null)
				{
					continue;
				}
				foreach (TileObject item in objectsInLookupSquare)
				{
					if (item is Character || Target.Object == null || !tile.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
					{
						continue;
					}
					Community community = item.GetCommunity();
					if (character.Community != community && (character.Community == null || !character.Community.CachedAllies.Contains(community)) && !(item is Campfire) && !(item is TreeProp) && item.IsImpassable(character, 512, tile) && (community == null || character.Community == null || character.Community.IsLooterCommunity() || character.IsAttackableEnemy(item) || !community.HasAnyLivingNonZombieMembers()))
					{
						if (flag && item.IsFlammable())
						{
							return item;
						}
						if (flag2 && !item.IsExplosionProof())
						{
							return item;
						}
					}
				}
			}
		}
		return null;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (_requester != null)
		{
			switch (_requester.Result)
			{
			case AStarResult.Success:
			{
				TileObject firstDefenceOnRoute = GetFirstDefenceOnRoute(character);
				_requester = null;
				if (firstDefenceOnRoute != null)
				{
					SetSubGoal(character, parent, new RangedAttack(character, firstDefenceOnRoute, _movementTypeWhenNotInDanger, StayInRangeParams));
				}
				else
				{
					Finished = true;
				}
				break;
			}
			case AStarResult.Fail:
				_requester = null;
				Finished = true;
				break;
			}
		}
		else if (SubGoal is FaceTarget)
		{
			bool flag = character.HasMolotovCocktail();
			bool flag2 = character.HasExplosives();
			if (flag || flag2)
			{
				if (character.IsAuthoritative())
				{
					StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
					if (resultDist < float.MaxValue)
					{
						resultDist += MaxRange;
					}
					TerrainCoord start = character.Tile;
					if (character.InsideBuilding != null)
					{
						int closestEntranceTo = character.InsideBuilding.GetClosestEntranceTo(GameTerrain.Instance.GetTileCoordForPos(Target.LastKnownPosition));
						start = character.GetBuildingExitPos(closestEntranceTo);
					}
					_requester = new AStarRequester();
					_requester.StartVisibleAndWithinRangeRequest(start, Target.LastKnownPosition, Target.GetFlag(TargetFlags.Crouching), MinRange, MaxRange, character, Target.Object, AStarPriority.Unknown, flag || flag2, flag2, dontOpenOurGates: true, resultTile, resultDist, 0f, includeWireFences: true, respectMovementZone: true, avoidHostileBases: false, 0);
				}
			}
			else
			{
				Finished = true;
			}
		}
		else if (SubGoal is RangedAttack && (SubGoal.Target == null || SubGoal.Target.Object == null || SubGoal.Target.Object.Deleted || SubGoal.Target.Object.IsDestroyed() || SubGoal.Target.Object.IsBurningEnoughToDestroy()))
		{
			Success = true;
			Finished = true;
		}
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		if (source == Target.Object)
		{
			Finished = true;
		}
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		return base.GetNextSubGoal(character, parent);
	}
}

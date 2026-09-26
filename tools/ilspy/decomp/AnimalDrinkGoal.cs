using System;
using UnityEngine;

public class AnimalDrinkGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	private int RiverPathIndex = -1;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(5.0);

	private static TimeSpan RememberFailedAttemptTime = TimeSpan.FromSeconds(60.0);

	public static GameProfiler _Timer = new GameProfiler("Update.AnimalDrinkGoalSearch");

	private const float DrinkAmount = 2.5f;

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalDrinkGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!(character.Thirst >= Character.ThirstCriticalTime))
		{
			if (!Active)
			{
				return GoalPriority.Animal_Drink;
			}
			return GoalPriority.Animal_SatisfyNeeds_Active;
		}
		if (!Active)
		{
			return GoalPriority.Animal_Drink_Critical;
		}
		return GoalPriority.Animal_SatisfyNeeds_Critical_Active;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorWaterBottle;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
		reflector.AddAfter(ref RiverPathIndex, 319);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (!Active)
		{
			if (character.GetThirst() < Character.ThirstyTime)
			{
				return false;
			}
			if (Session.Instance.PlayTime - _lastSearchedTime < (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
			{
				return false;
			}
			if (_lastSearchFailed && character.IsSatisfyingNeeds())
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		RiverPathIndex = -1;
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		Vector2 posXZ = character.PosXZ;
		bool flag = character.IsOnPlayersTeam();
		float num = float.MaxValue;
		TileObject tileObject = null;
		TerrainCoord destTile = TerrainCoord.Invalid;
		int num2 = -1;
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (allProp is AnimalDrinkerProp animalDrinkerProp && !(animalDrinkerProp.GetLiquidAmount() <= 0f))
				{
					float num3 = allProp.Tile.GetDist(tile) / Character.WalkSpeed;
					if ((allProp.GetCommunity() == character.Community || !(num3 > 64f / Character.WalkSpeed)) && !(num3 >= num) && (!character.HasMovementZone() || character.MovementZone.Overlaps(allProp.GetTileRect())) && (!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(allProp.MinTile, allProp.MaxTile)))
					{
						tileObject = allProp;
						num = num3;
						destTile = allProp.Tile;
						num2 = -1;
					}
				}
			}
			foreach (TerrainPath path in instance2.Paths)
			{
				if (!path.IsRiver)
				{
					continue;
				}
				float closestDistSq = float.MaxValue;
				Vector3 closestPointOnPath = Vector3.zero;
				Vector2 closestDirXZOnPath = Vector2.zero;
				float closestPathIndex = 0f;
				if (!path.GetClosestPointOnPathToPos(instance2, posXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex, mustBeInWater: true))
				{
					continue;
				}
				float num4 = Mathf.Sqrt(closestDistSq) / Character.WalkSpeed + 64f;
				if (num4 < num)
				{
					TerrainCoord tileCoordForPos = instance2.GetTileCoordForPos(closestPointOnPath);
					if ((!character.HasMovementZone() || character.MovementZone.Contains(tileCoordForPos)) && !character.HasFailedFindAttempt(instance2, 0, path.Index, FindType.Drink, null, null, null, RememberFailedAttemptTime, out var _, out var _))
					{
						num = num4;
						tileObject = null;
						destTile = tileCoordForPos;
						num2 = path.Index;
					}
				}
			}
		}
		if (num2 != -1)
		{
			RiverPathIndex = num2;
			SetSubGoal(character, parent, new MoveAsCloseAsPossibleTo(MovementType.Walk, destTile, instance2.Paths[num2].DefaultWidth + 2f));
		}
		else if (tileObject != null)
		{
			SetSubGoal(character, parent, new MoveAdjacentToTarget(character, tileObject, MovementType.Walk, canBeOnTile: false));
		}
		else
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo)
		{
			if (moveAsCloseAsPossibleTo.Success)
			{
				return new AnimationGoal(ActionAnim.Eat);
			}
			character.AddFailedFindAttempt(GameTerrain.Instance, 0, RiverPathIndex, FindType.Drink, null, null, null);
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = true;
		}
		if (SubGoal is MoveAdjacentToTarget moveAdjacentToTarget)
		{
			if (moveAdjacentToTarget.Success)
			{
				return new AnimationGoal(character, moveAdjacentToTarget.GetTargetObject(), ActionAnim.Eat);
			}
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = true;
		}
		if (SubGoal is AnimationGoal)
		{
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = false;
			if (character.GetThirst() * Character.WaterNeededPerDayInFlOz / Sun.DayLengthSecs > 2.5f * character.GetNutritionFactor() && SubGoal.GetTargetObject() is AnimalDrinkerProp animalDrinkerProp && animalDrinkerProp.GetLiquidAmount() > 0f)
			{
				return new AnimationGoal(character, animalDrinkerProp, ActionAnim.Eat);
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishEat)
		{
			if (RiverPathIndex != -1)
			{
				character.ConsumeLiquid(LiquidPrototype.Water, 2.5f, playSound: false, fromInfoScreen: false, InfectionType.None);
			}
			else if (SubGoal != null && SubGoal.GetTargetObject() is AnimalDrinkerProp animalDrinkerProp && animalDrinkerProp.GetLiquidAmount() > 0f)
			{
				float amount = animalDrinkerProp.ExtractLiquid(2.5f);
				character.ConsumeLiquid(LiquidPrototype.Water, amount, playSound: false, fromInfoScreen: false, InfectionType.None);
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

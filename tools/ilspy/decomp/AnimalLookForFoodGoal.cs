using System;
using UnityEngine;

public class AnimalLookForFoodGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(5.0);

	public static GameProfiler _Timer = new GameProfiler("Update.AnimalLookForFoodGoalSearch");

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalLookForFoodGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!(character.Hunger >= Character.HungerCriticalTime))
		{
			if (!Active)
			{
				return GoalPriority.Animal_Feed;
			}
			return GoalPriority.Animal_SatisfyNeeds_Active;
		}
		if (!Active)
		{
			return GoalPriority.Animal_Feed_Critical;
		}
		return GoalPriority.Animal_SatisfyNeeds_Critical_Active;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorEat;
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
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (!Active)
		{
			if (character.GetHunger() < Character.HungryTime)
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
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsOnPlayersTeam();
		float num = float.MaxValue;
		TileObject tileObject = null;
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (allProp is AnimalFeederProp animalFeederProp && animalFeederProp.Inventory.GetFoodForAnimal(character.GetBaseObjectType(), null) != null)
				{
					float dist = allProp.Tile.GetDist(tile);
					if ((allProp.GetCommunity() == character.Community || !(dist > 64f)) && !(dist >= num) && (!character.HasMovementZone() || character.MovementZone.Overlaps(allProp.GetTileRect())) && (!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(allProp.MinTile, allProp.MaxTile)))
					{
						tileObject = allProp;
						num = dist;
					}
				}
			}
		}
		if (tileObject != null)
		{
			SetSubGoal(character, parent, new MoveAdjacentToTarget(character, tileObject, MovementType.Walk, canBeOnTile: false));
			return;
		}
		_lastSearchedTime = instance.PlayTime;
		_lastSearchFailed = true;
		Finished = true;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
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
			TileObject targetObject = SubGoal.GetTargetObject();
			if (targetObject != null && targetObject.GetInventory() != null)
			{
				Equipment foodForAnimal = targetObject.GetInventory().GetFoodForAnimal(character.GetBaseObjectType(), null);
				if (foodForAnimal != null && character.GetHunger() >= foodForAnimal.GetNutrition() * character.GetNutritionFactor())
				{
					return new AnimationGoal(character, targetObject, ActionAnim.Eat);
				}
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishEat && SubGoal != null)
		{
			TileObject targetObject = SubGoal.GetTargetObject();
			if (targetObject != null && targetObject.GetInventory() != null)
			{
				Equipment foodForAnimal = targetObject.GetInventory().GetFoodForAnimal(character.GetBaseObjectType(), null);
				if (foodForAnimal != null)
				{
					Equipment equipment = targetObject.GetInventory().Take(targetObject, foodForAnimal, 1);
					if (equipment != null)
					{
						AnimalFeederProp animalFeederProp = targetObject as AnimalFeederProp;
						if (equipment.GetPrototype().ContainsHumanMeat && animalFeederProp != null && animalFeederProp.LastFilledBy != null)
						{
							Memory.OnMemorableEvent(MemoryPrototype.FedHumanMeatTo, animalFeederProp.LastFilledBy, character, 1f, secret: false);
						}
						character.Eat(equipment, playSound: false, fromInfoScreen: false, null);
						equipment.Delete();
					}
				}
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

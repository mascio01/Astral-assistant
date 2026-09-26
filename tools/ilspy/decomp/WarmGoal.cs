using System;
using UnityEngine;

public class WarmGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	private TimeSpan LastSearchedForMaterialFailedTime;

	public Campfire ReplenishCampfire;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static TimeSpan RememberFindFailedTime = TimeSpan.FromSeconds(300.0);

	private static float BuildFireDist = 64f;

	public static GameProfiler _Timer = new GameProfiler("Update.WarmGoalSearch");

	public static float ReplenishWoodAmount = 0.5f;

	public override GoalType GetGoalType()
	{
		return GoalType.WarmGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is Idle)
		{
			return null;
		}
		if (SubGoal is MoveToAndEnterBuilding)
		{
			return GameCursor.CursorShelter;
		}
		return GameCursor.CursorCampfire;
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
		reflector.AddAfter(ref LastSearchedForMaterialFailedTime, 40);
		reflector.AddAfter(ref ReplenishCampfire, 410);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!(parent is PrioritiserGoal))
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (character.Sitting && character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusNormal && character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) != null)
		{
			return true;
		}
		if (character.HasBeenPlayerControlledRecently(character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusMildHypothermia, character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia))
		{
			return false;
		}
		if (!Active && character.DirectControlledCrouching && character.GetBodyTemperatureInCelsius() > Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			return false;
		}
		if (!Active && character.CarryingObject != null && character.GetBodyTemperatureInCelsius() > Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			return false;
		}
		if (!Active && character.GetBodyTemperatureInCelsius() > Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			Squad squad = character.GetSquad();
			if (squad != null && squad.Action != SquadAction.Wait && squad.Action != SquadAction.Trade)
			{
				return false;
			}
		}
		if (!Active)
		{
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

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!(parent is PrioritiserGoal))
		{
			if (!Active)
			{
				return GoalPriority.Survivor_Warm;
			}
			return GoalPriority.Survivor_SatisfyNeeds_Active;
		}
		if ((Active || (!character.IsOutdoors() && character.Community != null && character.Community.IsAmbientCommunity()) || (character.Sitting && character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) != null)) && !Finished)
		{
			ActionAnim currentActionAnim = character.CurrentActionAnim;
			if ((uint)(currentActionAnim - 102) <= 1u)
			{
				return GoalPriority.Survivor_SatisfyNeeds_Critical_Active;
			}
			if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusShivering)
			{
				return GoalPriority.Survivor_Warm_Critical;
			}
			if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusNormal || (Active && !(SubGoal is Idle)))
			{
				return GoalPriority.Survivor_SatisfyNeeds_Active;
			}
			return GoalPriority.Impossible;
		}
		if (character.GetBodyTemperatureInCelsius() > Character.BodyTemperatureInCelsiusShivering)
		{
			return GoalPriority.Impossible;
		}
		if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusMildHypothermia)
		{
			return GoalPriority.Survivor_Warm_Critical;
		}
		if (character.ShouldRoleTakePriorityOverNeeds())
		{
			return GoalPriority.Impossible;
		}
		return GoalPriority.Survivor_Warm;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.Sitting && character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) != null)
		{
			SetSubGoal(character, parent, new Idle());
			return;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsControllableByPlayer();
		Squad squad = character.GetSquad();
		float num = (flag ? 80f : 200f);
		TileObject tileObject = null;
		int entranceIndex = -1;
		bool flag2 = false;
		if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			num = float.MaxValue;
			flag2 = true;
		}
		bool flag3 = character.Inventory.FindItemOfType(EquipmentPrototype.Match) != null;
		flag3 |= character.Inventory.FindItemOfClass(typeof(HuntingKnife)) != null && character.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null;
		float num2 = float.MaxValue;
		bool flag4 = character.Community != null && character.Community.IsAmbientCommunity();
		FindGoal.BuildListOfEscapedFromTargets(character);
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in instance.PropManager.AllProps)
			{
				if (allProp is Building building)
				{
					if ((building != character.InsideBuilding && !building.CanEnter(character)) || !building.HasAnyInternalSlots() || (character.HasMovementZone() && !character.MovementZone.Overlaps(building.GetTileRect())) || (flag && !instance2.FogOfWar.IsAnyTileInRectCornersExplored(building.MinTile, building.MaxTile)) || building is EnterableVehicle { GearState: not GearState.Off })
					{
						continue;
					}
					for (int i = 0; i < building.GetEntranceDefs().Length; i++)
					{
						float num3 = MathUtil.ToXZ(building.GetEntrancePos(i) - character.Pos).magnitude;
						if (building.Community != character.Community)
						{
							if (!flag4)
							{
								if (!flag2)
								{
									continue;
								}
								num3 = ((building == character.InsideBuilding) ? (num3 + 100f) : (num3 + 250f));
							}
						}
						else if (allProp.GetBaseObjectType() == BaseObjectType.Outhouse)
						{
							num3 += 30f;
						}
						if (!(num3 >= num) && (building == character.InsideBuilding || !character.HasFailedFindAttempt(building, 0, 0, FindType.Warmth, null, null, null, RememberFindFailedTime, out var _, out var _)) && !FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, building.Pos))
						{
							tileObject = building;
							entranceIndex = i;
							num = num3;
						}
					}
				}
				if (!(allProp is Campfire campfire))
				{
					continue;
				}
				float num4 = campfire.Tile.GetDist(tile);
				if (character.HasRoleWithTargetLocation(Role.Cook, campfire.Tile) || character.HasRoleWithTargetLocation(Role.Crafter, campfire.Tile))
				{
					num4 -= 100f;
				}
				else if (campfire.State != CampfireState.Burning)
				{
					if (campfire.Community == null || campfire.Community != character.Community)
					{
						continue;
					}
					if (!flag3)
					{
						if (num2 == float.MaxValue)
						{
							num2 = EstimateCostToFindLighter(character);
						}
						num4 += num2;
					}
					num4 -= 30f;
				}
				else
				{
					num4 -= 50f;
				}
				if (campfire.Community != character.Community)
				{
					if (character.WantToAvoidCommunity(campfire.GetCommunity()))
					{
						continue;
					}
					num4 += 50f;
				}
				if (squad != null)
				{
					SquadAction action = squad.Action;
					if (action == SquadAction.FindTarget || action == SquadAction.GoToHighPrio)
					{
						int ownerCommunityIdForTile = instance2.GetOwnerCommunityIdForTile(squad.GoalTile.x, squad.GoalTile.y);
						if (ownerCommunityIdForTile != 0 && campfire.GetCommunityId() == ownerCommunityIdForTile)
						{
							num4 -= 200f;
						}
					}
				}
				if (!(num4 >= num) && (!character.HasMovementZone() || character.MovementZone.Contains(campfire.Tile)) && (!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(campfire.MinTile, campfire.MaxTile)) && !character.HasFailedFindAttempt(campfire, 0, 0, FindType.Warmth, null, null, null, RememberFindFailedTime, out var _, out var _) && !FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, campfire.Pos))
				{
					tileObject = campfire;
					entranceIndex = -1;
					num = num4;
				}
			}
		}
		FindGoal.ClearListOfEscapedFromTargets();
		if ((tileObject == null || tileObject is Building || character.Tile.GetDist(tileObject.GetCentreTile()) >= BuildFireDist) && !character.IsControllableByPlayer() && character.GetCommunityThatOwnsThisArea() == null && !character.HasFailedFindAttempt(GameTerrain.Instance, -1, 0, FindType.BuildCampfire, null, null, null, Sun.DayLength, out var _, out var _) && ((character.DoIOrAnyoneInSquadHaveEquipmentOfType(EquipmentPrototype.Flint) && character.DoIOrAnyoneInSquadHaveEquipmentOfClass(typeof(HuntingKnife))) || character.DoIOrAnyoneInSquadHaveEquipmentOfType(EquipmentPrototype.Match)))
		{
			Recipe recipe = GameImpl.Instance.FindRecipeByProduct(PropPrototype.Campfire, null);
			if (recipe != null && (character.DoIOrAnyoneInSquadHaveAllIngredients(recipe) || (character.DoIOrAnyoneInSquadHaveEquipmentOfClass(typeof(Axe)) && recipe.IsIngredient(EquipmentPrototype.Wood) && recipe.Ingredients.Count == 1)))
			{
				Campfire campfire2 = new Campfire();
				campfire2.SetTile(character.Tile);
				if (GameCursor.CanBuildHere(character, campfire2, checkOtherCharacters: false, checkCropPatches: true, character.Community) == CursorActionDisabledReason.Enabled)
				{
					CraftGoal craftGoal = new CraftGoal();
					craftGoal.MaxDistToTravel = BuildFireDist;
					craftGoal.StartRecipe(character, recipe, null, 1, character.Tile, Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: false, urgent: false);
					SetSubGoal(character, parent, craftGoal);
					return;
				}
			}
		}
		if (tileObject != null)
		{
			if (tileObject is Campfire campfire3)
			{
				if (campfire3.State != CampfireState.Burning && (character.Community == null || !character.Community.IsAnyMemberLightingFire(campfire3)))
				{
					character.DirectControlledCrouching = false;
					ReplenishCampfire = campfire3;
					LightFireGoal lightFireGoal = new LightFireGoal(character, null, campfire3, MovementType.Walk, canAddMaterialToFire: true);
					lightFireGoal.Critical = true;
					SetSubGoal(character, parent, lightFireGoal);
				}
				else
				{
					if (character.IsControllableByPlayer() && StoryManager.Instance.GetMostInterestingSpeaker() == null)
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, (campfire3.Community == character.Community && campfire3.Community != null) ? SpeechSituation.GoingToSitByFire : SpeechSituation.SearchingForFire);
						if (speechForSituation != null)
						{
							character.Speak(speechForSituation);
						}
					}
					SetSubGoal(character, parent, new SitAroundFireGoal(character, campfire3, MovementType.Walk));
				}
			}
			if (!(tileObject is Building building2))
			{
				return;
			}
			if (character.IsControllableByPlayer() && StoryManager.Instance.GetMostInterestingSpeaker() == null)
			{
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, (building2.Community == character.Community && building2.Community != null) ? SpeechSituation.TakingShelter : SpeechSituation.SearchingForShelter);
				if (speechForSituation2 != null)
				{
					character.Speak(speechForSituation2);
				}
			}
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building2, entranceIndex, MovementType.Walk));
		}
		else
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	public static bool DoesInventoryHaveLighter(EquipmentContainer inventory, bool alreadyHaveHuntingKnife, bool alreadyHaveFlint)
	{
		if (inventory.FindItemOfType(EquipmentPrototype.Match) != null)
		{
			return true;
		}
		if (!alreadyHaveHuntingKnife && inventory.FindItemOfClass(typeof(HuntingKnife)) == null)
		{
			return false;
		}
		if (!alreadyHaveFlint && inventory.FindItemOfType(EquipmentPrototype.Flint) != null)
		{
			return false;
		}
		return true;
	}

	public static float EstimateCostToFindLighter(Character character)
	{
		Session instance = Session.Instance;
		float num = 100f;
		bool alreadyHaveHuntingKnife = character.Inventory.FindItemOfClass(typeof(HuntingKnife)) != null;
		bool alreadyHaveFlint = character.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null;
		foreach (Character character2 in instance.CharacterManager.Characters)
		{
			if (!character2.Alive || character2.Community == character.Community)
			{
				float magnitude = (character2.PosXZ - character.PosXZ).magnitude;
				if (magnitude < num && DoesInventoryHaveLighter(character2.Inventory, alreadyHaveHuntingKnife, alreadyHaveFlint))
				{
					num = magnitude;
				}
			}
		}
		foreach (Prop allProp in instance.PropManager.AllProps)
		{
			float magnitude2 = (allProp.PosXZ - character.PosXZ).magnitude;
			if (magnitude2 < num && (allProp.Community == null || allProp.Community == character.Community || !allProp.Community.HasAnyActiveMembers()) && DoesInventoryHaveLighter(allProp.Inventory, alreadyHaveHuntingKnife, alreadyHaveFlint))
			{
				num = magnitude2;
			}
		}
		return num;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is Idle && character.IsOutdoors())
		{
			Campfire nearestBurningCampfire = character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange);
			if (nearestBurningCampfire == null)
			{
				Finished = true;
				ReplenishCampfire = null;
			}
			else if (nearestBurningCampfire.WoodRemaining < ReplenishWoodAmount && character.GetBodyTemperatureInCelsius() >= Character.BodyTemperatureInCelsiusNormal - 0.01f && character.HasInventorySpaceFor(EquipmentPrototype.Wood.Weight) && Session.Instance.PlayTime - LastSearchedForMaterialFailedTime >= TimeSpan.FromSeconds(30.0) && (character.Community == null || !character.Community.IsAnyMemberAddingMaterialToFire(nearestBurningCampfire)))
			{
				ReplenishCampfire = nearestBurningCampfire;
				SetSubGoal(character, parent, new MoveToAndAddMaterialToFire(character, nearestBurningCampfire.Tile, null, MovementType.Walk, critical: false));
			}
			else
			{
				ReplenishCampfire = null;
			}
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is CraftGoal craftGoal)
		{
			if (GameTerrain.Instance.GetCraftingPropOnTile(craftGoal.DestTile.x, craftGoal.DestTile.y) is Campfire campfire)
			{
				if (!character.IsControllableByPlayer())
				{
					campfire.SetTemporaryFire(isTemporaryFire: true);
				}
				ReplenishCampfire = campfire;
				return new LightFireGoal(character, null, campfire, MovementType.Walk, canAddMaterialToFire: true)
				{
					Critical = true
				};
			}
			character.AddFailedFindAttempt(GameTerrain.Instance, -1, 0, FindType.BuildCampfire, null, null, null);
		}
		if (SubGoal is LightFireGoal lightFireGoal && lightFireGoal.GetTargetObject() is Campfire campfire2)
		{
			if (ReplenishCampfire == campfire2)
			{
				ReplenishCampfire = null;
			}
			if (lightFireGoal.Success && campfire2.IsBurning())
			{
				return new SitAroundFireGoal(character, campfire2, MovementType.Walk);
			}
			character.AddFailedFindAttempt(campfire2, 0, 0, FindType.Warmth, null, null, null);
		}
		if (SubGoal is SitAroundFireGoal sitAroundFireGoal)
		{
			if (sitAroundFireGoal.Success)
			{
				_lastSearchFailed = false;
				return new Idle();
			}
			character.AddFailedFindAttempt(sitAroundFireGoal.GetTargetObject(), 0, 0, FindType.Warmth, null, null, null);
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				_lastSearchFailed = false;
				return new Idle();
			}
			character.AddFailedFindAttempt(moveToAndEnterBuilding.GetTargetObject(), moveToAndEnterBuilding.EntranceIndex, 0, FindType.Warmth, null, null, null);
		}
		if (SubGoal is MoveToAndAddMaterialToFire moveToAndAddMaterialToFire)
		{
			Campfire campfire3 = GameTerrain.Instance.GetCraftingPropOnTile(moveToAndAddMaterialToFire.Tile.x, moveToAndAddMaterialToFire.Tile.y) as Campfire;
			if (ReplenishCampfire == campfire3)
			{
				ReplenishCampfire = null;
			}
			if (moveToAndAddMaterialToFire.Success)
			{
				if (campfire3 != null)
				{
					return new SitAroundFireGoal(character, campfire3, MovementType.Walk);
				}
			}
			else
			{
				LastSearchedForMaterialFailedTime = Session.Instance.PlayTime;
				if (moveToAndAddMaterialToFire.Item != null && campfire3 != null)
				{
					character.AddFailedFindAttempt(campfire3, 0, 0, FindType.Warmth, null, null, null);
				}
			}
		}
		return base.GetNextSubGoal(character, parent);
	}
}

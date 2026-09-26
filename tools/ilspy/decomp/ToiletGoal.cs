using System;
using System.Collections.Generic;
using UnityEngine;

public class ToiletGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	private Equipment Trousers;

	private Equipment PreviouslyEquipped;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	private static List<TileObject> NearbyObjects = new List<TileObject>();

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref _lastSearchedTime, 80);
		reflector.AddAfter(ref _lastSearchFailed, 80);
		reflector.AddAfter(ref Trousers, 80);
		reflector.AddAfter(ref PreviouslyEquipped, 82);
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is AnimationGoal || SubGoal is LeaveBuilding)
		{
			return null;
		}
		return GameCursor.CursorToilet;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ToiletGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && !character.DirectControlled && (SubGoal is AnimationGoal || SubGoal is LeaveBuilding || SubGoal is Equip))
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (character.HasBeenPlayerControlledRecently(critical: false, extraCritical: false))
		{
			return false;
		}
		if (!Active)
		{
			if (character.DirectControlledCrouching)
			{
				return false;
			}
			if (character.CarryingObject != null)
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
		if (Active && !Finished)
		{
			if (SubGoal is AnimationGoal || SubGoal is LeaveBuilding || SubGoal is Equip)
			{
				return GoalPriority.Survivor_SatisfyNeeds_Animation;
			}
			if (character.GetToilet() >= Character.WetPantsTime)
			{
				return GoalPriority.Survivor_Toilet_Critical;
			}
			return GoalPriority.Survivor_SatisfyNeeds_Active;
		}
		if (character.GetToilet() < Character.NeedToiletTime)
		{
			return GoalPriority.Impossible;
		}
		if (character.ShouldRoleTakePriorityOverNeeds())
		{
			return GoalPriority.Impossible;
		}
		if (character.GetToilet() >= Character.WetPantsTime)
		{
			return GoalPriority.Survivor_Toilet_Critical;
		}
		return GoalPriority.Survivor_Toilet;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		PreviouslyEquipped = null;
		float num = 64f;
		Outhouse outhouse = null;
		if (character.Community != null)
		{
			foreach (Prop building in character.Community.Buildings)
			{
				if (building is Outhouse outhouse2 && outhouse2.GetUnderConstructionInfo() == null)
				{
					float dist = outhouse2.Tile.GetDist(character.Tile);
					if (dist < num)
					{
						num = dist;
						outhouse = outhouse2;
					}
				}
			}
		}
		if (outhouse != null)
		{
			if (outhouse.GetInhabitantCount() > 0 || character.Community.IsAnyMemberMovingToEnterBuilding(outhouse))
			{
				_lastSearchedTime = Session.Instance.PlayTime;
				_lastSearchFailed = true;
				Finished = true;
			}
			else
			{
				MoveToAndEnterBuilding moveToAndEnterBuilding = new MoveToAndEnterBuilding(character, outhouse, MovementType.Walk);
				moveToAndEnterBuilding._dontOpenOurGates = FindGoal.CalcDontOpenOurGates(character);
				SetSubGoal(character, parent, moveToAndEnterBuilding);
			}
			return;
		}
		TileObject tileObject = null;
		GameTerrain.Instance.GetObjectsInRect(character.Tile - new TerrainCoord(32, 32), character.Tile + new TerrainCoord(32, 32), NearbyObjects);
		bool flag = false;
		foreach (TileObject nearbyObject in NearbyObjects)
		{
			if (nearbyObject is TreeProp || nearbyObject is Bush)
			{
				float dist2 = nearbyObject.GetCentreTile().GetDist(character.Tile);
				if (dist2 < num)
				{
					num = dist2;
					tileObject = nearbyObject;
				}
			}
			if (nearbyObject is Character)
			{
				float dist3 = nearbyObject.GetCentreTile().GetDist(character.Tile);
				flag |= dist3 < (nearbyObject.IsDestroyed() ? 4f : 8f);
			}
		}
		NearbyObjects.Clear();
		if (tileObject != null)
		{
			SetSubGoal(character, parent, new MoveAdjacentTo(MovementType.Walk, tileObject.GetCentreTile(), canBeOnTile: false, FindGoal.CalcDontOpenOurGates(character)));
			return;
		}
		if (!flag)
		{
			SetSubGoal(character, parent, GetAnimationGoal(character));
			return;
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		Finished = true;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		if (Trousers != null && character.InventoryContains(Trousers))
		{
			Trousers.Wear(character);
			Trousers = null;
		}
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				return new AnimationGoal(ActionAnim.Pee);
			}
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = true;
		}
		if (SubGoal is MoveAdjacentTo moveAdjacentTo)
		{
			if (moveAdjacentTo.Success)
			{
				return new TurnTo((character.Appearance.Gender == GenderType.Male) ? moveAdjacentTo.DestTile : (character.Tile + (character.Tile - moveAdjacentTo.DestTile)));
			}
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = true;
		}
		if (SubGoal is TurnTo)
		{
			return GetAnimationGoal(character);
		}
		if (SubGoal is Equip { Equipment: null })
		{
			return GetAnimationGoal(character);
		}
		if (SubGoal is AnimationGoal)
		{
			if (Trousers != null && character.InventoryContains(Trousers))
			{
				Trousers.Wear(character);
				Trousers = null;
			}
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
			{
				return new Equip(PreviouslyEquipped);
			}
			if (character.InsideBuilding is Outhouse)
			{
				return new LeaveBuilding();
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	private Goal GetAnimationGoal(Character character)
	{
		if (character.EquippedItem != null && character.InsideBuilding == null)
		{
			PreviouslyEquipped = character.EquippedItem;
			return new Equip(null);
		}
		if (character.Appearance.Gender == GenderType.Female)
		{
			Trousers = character.Clothes[3];
			if (Trousers != null)
			{
				Trousers.Strip(character);
			}
		}
		return new AnimationGoal(ActionAnim.Pee);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.FinishPeeing:
			if (character.InsideBuilding is Outhouse outhouse)
			{
				outhouse.FillWithLiquid(character.Toilet * (Character.WaterNeededPerDayInFlOz / Sun.DayLengthSecs) * 0.5f);
			}
			character.Toilet = 0f;
			break;
		case AnimationEventType.PeeingSound:
			if (character == Hud.Instance.LocalControlledCharacter)
			{
				SoundManager.PlaySound3DFromList(SoundManager.PeeingSounds, character.Pos);
			}
			break;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}

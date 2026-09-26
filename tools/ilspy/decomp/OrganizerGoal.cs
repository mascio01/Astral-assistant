using System;
using System.Collections.Generic;
using UnityEngine;

public class OrganizerGoal : RoleGoal
{
	private TimeSpan LastAttemptedTime = TimeSpan.FromDays(-365.0);

	private Prop PlaceToStore;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.OrganizerGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorOrganize;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref LastAttemptedTime, 379);
		reflector.AddAfter(ref MovementType, 379);
		reflector.AddAfter(ref PlaceToStore, 379);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Organizer))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Organizer))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && SubGoal is MoveToAndDeposit moveToAndDeposit && moveToAndDeposit.SubGoal is AnimationGoal)
		{
			return GoalPriority.Survivor_Role_Animation;
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Organizer));
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.Organizer), inProgress: true);
		Goal firstSubGoal = GetFirstSubGoal(character, parent);
		if (firstSubGoal != null)
		{
			SetSubGoal(character, parent, firstSubGoal);
			return;
		}
		OnFailed(character);
		Finished = true;
	}

	private List<StoragePolicy> BuildListOfStoragePolicies(Character character)
	{
		List<StoragePolicy> list = new List<StoragePolicy>();
		foreach (Prop building in character.Community.Buildings)
		{
			if (building.StoragePolicies == null || building.GetUnderConstructionInfo() != null)
			{
				continue;
			}
			for (int i = 0; i < building.StoragePolicies.Count; i++)
			{
				if (!list.Contains(building.StoragePolicies[i]))
				{
					list.Add(building.StoragePolicies[i]);
				}
			}
		}
		return list;
	}

	private Goal GetFirstSubGoal(Character character, Goal parent)
	{
		if (!character.IsControllableByPlayer() && !character.Inventory.HasAnyGatheredItems())
		{
			character.CancelRole(new RoleInfo(Role.Organizer));
			return null;
		}
		bool failed;
		Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, float.MaxValue, float.MaxValue, MovementType, out failed);
		if (moveToAndDepositGoal != null)
		{
			return moveToAndDepositGoal;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		List<StoragePolicy> storagePolicies = BuildListOfStoragePolicies(character);
		TerrainCoord tile = character.Tile;
		Prop prop = null;
		Prop placeToStore = null;
		Equipment targetEquipment = null;
		float num = float.MaxValue;
		foreach (Prop building in character.Community.Buildings)
		{
			float dist = building.Tile.GetDist(tile);
			if (dist >= num || (character.HasMovementZone() && !character.MovementZone.Overlaps(building.GetTileRect())))
			{
				continue;
			}
			switch (building.GetBaseObjectType())
			{
			case BaseObjectType.Campfire:
			case BaseObjectType.Grave:
			case BaseObjectType.RabbitTrap:
			case BaseObjectType.AnimalFeederProp:
				continue;
			}
			if (building is CraftingProp craftingProp && craftingProp.IsCrafting())
			{
				continue;
			}
			foreach (Equipment content in building.Inventory.Contents)
			{
				if (IsItemInStoragePolicies(storagePolicies, content) && !IsItemInStoragePolicies(building.StoragePolicies, content) && character.HasInventorySpaceFor(content.GetWeight()))
				{
					Prop buildingToStoreSuppliesIn = GatherGoal.GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(content), building);
					if (buildingToStoreSuppliesIn != building && buildingToStoreSuppliesIn != null && (!character.HasMovementZone() || character.MovementZone.Overlaps(buildingToStoreSuppliesIn.GetTileRect())))
					{
						prop = building;
						placeToStore = buildingToStoreSuppliesIn;
						targetEquipment = content;
						num = dist;
						break;
					}
				}
			}
		}
		if (prop != null)
		{
			PlaceToStore = placeToStore;
			return new MoveToAndTake(character, prop, targetEquipment, int.MaxValue, ignoreWeight: false, MovementType)
			{
				IsGathering = true,
				DontOpenOurGates = FindGoal.CalcDontOpenOurGates(character)
			};
		}
		return null;
	}

	public static bool IsItemInStoragePolicies(List<StoragePolicy> storagePolicies, Equipment item)
	{
		if (storagePolicies == null)
		{
			return false;
		}
		GatheredItem gatheredItem = GatheredItem.Create(item);
		for (int i = 0; i < storagePolicies.Count; i++)
		{
			if (storagePolicies[i].Matches(gatheredItem))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsGatheredItemInStoragePolicies(List<StoragePolicy> storagePolicies, GatheredItem gatheredItem)
	{
		if (storagePolicies == null)
		{
			return false;
		}
		for (int i = 0; i < storagePolicies.Count; i++)
		{
			if (storagePolicies[i].Matches(gatheredItem))
			{
				return true;
			}
		}
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndDeposit moveToAndDeposit)
		{
			if (moveToAndDeposit.SuccessfullyReachedDestination)
			{
				return GetFirstSubGoal(character, parent);
			}
			OnFailed(character);
			return null;
		}
		if (SubGoal is MoveToAndTake moveToAndTake)
		{
			if (moveToAndTake.SuccessfullyReachedDestination && moveToAndTake.AmountRetrieved > 0f)
			{
				return GetFirstSubGoal(character, parent);
			}
			OnFailed(character);
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		MoveToAndTake moveToAndTake = SubGoal as MoveToAndTake;
		if (animEvent.EventType == AnimationEventType.Take && moveToAndTake != null)
		{
			bool result = base.OnAnimationEvent(character, parent, animEvent);
			OnFirstItemTaken(character, moveToAndTake.GetTargetProp());
			return result;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public void OnFirstItemTaken(Character character, Prop prop)
	{
		if (prop == null || PlaceToStore == null || PlaceToStore.Destroyed || PlaceToStore.Deleted)
		{
			return;
		}
		List<StoragePolicy> storagePolicies = BuildListOfStoragePolicies(character);
		for (int i = 0; i < prop.Inventory.Contents.Count; i++)
		{
			Equipment equipment = prop.Inventory.Contents[i];
			if (!IsItemInStoragePolicies(storagePolicies, equipment) || IsItemInStoragePolicies(prop.StoragePolicies, equipment) || !character.HasInventorySpaceFor(equipment.GetWeight()) || GatherGoal.GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(equipment), prop) != PlaceToStore)
			{
				continue;
			}
			int num = Math.Min(equipment.GetAmount(), Mathf.FloorToInt(character.GetAvailableInventorySpace() / equipment.GetWeight()));
			if (num > 0)
			{
				Equipment equipment2 = prop.Inventory.Take(prop, equipment, num);
				if (equipment2 == equipment)
				{
					i--;
				}
				equipment2 = character.Inventory.Add(character, equipment2, prop);
				equipment2.IncrementGatheredAmount(num);
			}
		}
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Organizer));
		LastAttemptedTime = Session.Instance.PlayTime;
	}

	public void ResetLastAttemptedTime()
	{
		LastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Organizer);
	}
}

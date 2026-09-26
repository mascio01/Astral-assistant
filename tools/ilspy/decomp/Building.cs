using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : Prop
{
	private static PrefabResource[] Models = new PrefabResource[39]
	{
		new PrefabResource("Prefabs/Buildings/House1"),
		new PrefabResource("Prefabs/Buildings/House1_Var1"),
		new PrefabResource("Prefabs/Buildings/House1_Var2"),
		new PrefabResource("Prefabs/Buildings/House2"),
		new PrefabResource("Prefabs/Buildings/House2_Var1"),
		new PrefabResource("Prefabs/Buildings/House2_Var2"),
		new PrefabResource("Prefabs/Buildings/House3"),
		new PrefabResource("Prefabs/Buildings/House3_Var1"),
		new PrefabResource("Prefabs/Buildings/House3_Var2"),
		new PrefabResource("Prefabs/Buildings/ConvenienceStore"),
		new PrefabResource("Prefabs/Buildings/ConvenienceStore_Var1"),
		new PrefabResource("Prefabs/Buildings/Restaurant"),
		new PrefabResource("Prefabs/Buildings/Store Front/burger pizza store"),
		new PrefabResource("Prefabs/Buildings/Store Front/footwear store"),
		new PrefabResource("Prefabs/Buildings/Store Front/gold loan store"),
		new PrefabResource("Prefabs/Buildings/Store Front/mobile store"),
		new PrefabResource("Prefabs/Buildings/Store Front/SportsStore"),
		new PrefabResource("Prefabs/Buildings/Store Front/Bank"),
		new PrefabResource("Prefabs/Buildings/Store Front/BookStore"),
		new PrefabResource("Prefabs/Buildings/Store Front/CornerCafe"),
		new PrefabResource("Prefabs/Buildings/Store Front/JewelryStore"),
		new PrefabResource("Prefabs/Buildings/Store Front/Cinema"),
		new PrefabResource("Prefabs/Buildings/Store Front/Supermarket"),
		new PrefabResource("Prefabs/Buildings/Store Front/Clinic"),
		new PrefabResource("Prefabs/Buildings/Store Front/PhotoStudio"),
		new PrefabResource("Prefabs/Buildings/Store Front/GardenShop"),
		new PrefabResource("Prefabs/Buildings/Security_Booth"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_Black"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_Blue"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_Orange"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_Rusted"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_White"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Abandoned_Yellow"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_Black"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_Blue"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_Orange"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_Rusted"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_White"),
		new PrefabResource("Prefabs/Vehicles/Excavator_Anchored_Yellow")
	};

	private static Resource<Material>[] Materials = new Resource<Material>[20]
	{
		new Resource<Material>("Materials/Signs/Vegan", includeInList: true),
		new Resource<Material>("Materials/Signs/Steakery", includeInList: true),
		new Resource<Material>("Materials/Signs/Shrimp", includeInList: true),
		new Resource<Material>("Materials/Signs/Burger", includeInList: true),
		new Resource<Material>("Materials/Signs/Fresh", includeInList: true),
		new Resource<Material>("Materials/Signs/MiniMart", includeInList: true),
		new Resource<Material>("Materials/Signs/Convenience", includeInList: true),
		new Resource<Material>("Materials/Signs/Latte", includeInList: true),
		new Resource<Material>("Materials/Signs/Guns", includeInList: true),
		new Resource<Material>("Materials/Signs/PPE", includeInList: true),
		new Resource<Material>("Materials/Signs/Peace", includeInList: true),
		new Resource<Material>("Materials/Signs/Tools", includeInList: true),
		new Resource<Material>("Materials/Signs/Dirt", includeInList: true),
		new Resource<Material>("Materials/Signs/DIY", includeInList: true),
		new Resource<Material>("Materials/Signs/Plant", includeInList: true),
		new Resource<Material>("Materials/Signs/Mall", includeInList: true),
		new Resource<Material>("Materials/Billboards/Billboard_BunnyBackpack", includeInList: true),
		new Resource<Material>("Materials/Billboards/Billboard_McCoys", includeInList: true),
		new Resource<Material>("Materials/Billboards/Billboard_FatNeils", includeInList: true),
		new Resource<Material>("Materials/Billboards/Billboard_Pharma", includeInList: true)
	};

	private static Resource<Material>[] Adverts = new Resource<Material>[14]
	{
		new Resource<Material>("Materials/Signs/Sale", includeInList: true),
		new Resource<Material>("Materials/Signs/Coffee", includeInList: true),
		new Resource<Material>("Materials/Signs/Frapp", includeInList: true),
		new Resource<Material>("Materials/Signs/Iceman", includeInList: true),
		new Resource<Material>("Materials/Signs/Snackbar", includeInList: true),
		new Resource<Material>("Materials/Signs/Chips", includeInList: true),
		new Resource<Material>("Materials/Signs/America", includeInList: true),
		new Resource<Material>("Materials/Signs/BuySellTrade", includeInList: true),
		new Resource<Material>("Materials/Signs/Chainsaws", includeInList: true),
		new Resource<Material>("Materials/Signs/Hibiscus", includeInList: true),
		new Resource<Material>("Materials/Signs/Hunting", includeInList: true),
		new Resource<Material>("Materials/Signs/SelectedItems", includeInList: true),
		new Resource<Material>("Materials/Signs/Sunflower", includeInList: true),
		new Resource<Material>("Materials/Signs/Workers", includeInList: true)
	};

	public Character[] Inhabitants;

	public bool UnlockedToPlayer;

	public bool InaccessibleToPlayer;

	public bool IgnoreForQuests;

	public static float DestroyedHitForce = 5000f;

	public const float MaxTarmacDist = 4f;

	public virtual bool HasGarageDoor
	{
		get
		{
			if (Prototype == null)
			{
				return GarageDoorOffset != TerrainCoord.Invalid;
			}
			return Prototype.HasGarageDoor;
		}
	}

	public virtual TerrainCoord GarageDoorOffset
	{
		get
		{
			if (Prototype == null)
			{
				return TerrainCoord.Invalid;
			}
			return Prototype.GarageDoorOffset;
		}
	}

	public virtual float GarageDoorAngle
	{
		get
		{
			if (Prototype == null)
			{
				return 0f;
			}
			return Prototype.GarageDoorAngle;
		}
	}

	public override float TarmacDist
	{
		get
		{
			if (GetBuildingUsage() != BuildingUsage.Commercial)
			{
				return 0f;
			}
			return 4f;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Building;
	}

	public virtual BuildingUsage GetBuildingUsage()
	{
		if (Prototype == null)
		{
			return BuildingUsage.Camp;
		}
		return Prototype.Usage;
	}

	public override void Init()
	{
		base.Init();
		if (Inhabitants == null)
		{
			Inhabitants = new Character[GetInhabitantSlotDefs().Length];
		}
	}

	public override void Delete()
	{
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				OnCharacterLeave(Inhabitants[i], 0, fromBuildingDestroyed: true, fromRagdolled: false);
			}
		}
		base.Delete();
	}

	public override void OnDestroyed(Character source, bool from_impact, Vector3 hitPos)
	{
		base.OnDestroyed(source, from_impact, hitPos);
		for (int i = 0; i < GetInhabitantSlotDefs().Length; i++)
		{
			if (GetInhabitantSlotDefs()[i].External && Inhabitants[i] != null)
			{
				Character character = Inhabitants[i];
				Vector3 boundingBoxCentre = character.GetBoundingBoxCentre();
				Vector3 hitForce = MathUtil.SafeNormalize(boundingBoxCentre - hitPos, Vector3.zero) * DestroyedHitForce;
				if (character.GetPredicted() != null)
				{
					character.GetPredictedOrElseThisCharacter().Ragdollify(0f, boundingBoxCentre, hitForce, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: true);
				}
				character.Ragdollify(0f, boundingBoxCentre, hitForce, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: true);
			}
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.IsDeserialising)
		{
			Inhabitants = new Character[(Prototype != null) ? GetInhabitantSlotDefs().Length : 0];
		}
		int value = Inhabitants.Length;
		if (reflector.Version >= 216)
		{
			reflector.Add(ref value);
		}
		else
		{
			value = GetOldInhabitantsCount();
		}
		for (int i = 0; i < value; i++)
		{
			Character obj = ((i < Inhabitants.Length) ? Inhabitants[i] : null);
			reflector.Add(ref obj);
			if (reflector.IsDeserialising && i < Inhabitants.Length)
			{
				Inhabitants[i] = obj;
			}
		}
		reflector.AddAfter(ref InaccessibleToPlayer, 577);
		reflector.AddAfter(ref UnlockedToPlayer, 269);
		reflector.AddAfter(ref IgnoreForQuests, 629);
	}

	protected virtual int GetOldInhabitantsCount()
	{
		return GetInhabitantSlotDefs().Length;
	}

	public override void SetCommunity(Community community)
	{
		base.SetCommunity(community);
		if (Inhabitants == null)
		{
			return;
		}
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && !CouldEnterIfNotFull(character))
			{
				OnCharacterLeave(character, 0, fromBuildingDestroyed: false, fromRagdolled: false);
			}
		}
	}

	public override void ApplyDamage(Character source, float damage, bool from_impact, Vector3 hitPos, bool burning, float damageRadius)
	{
		base.ApplyDamage(source, damage, from_impact, hitPos, burning, damageRadius);
		if (!burning)
		{
			return;
		}
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && !character.IsFriendlyFire(source, character, itsATrap: false))
			{
				TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
				int num = character.Id * 1000 + (int)currentTime.TotalSeconds;
				InjuryLocation injuryLocation = (InjuryLocation)MathUtil.RandomInt(num, 6);
				character.PickRandomHitPos(injuryLocation, num + 1, GetBoundingBoxCentre(), out var bone, out var hitPosInBoneSpace);
				character.Fuel = Math.Max(character.Fuel, TileObject.FuelBurningRateFlOzPerSecond);
				float damage2 = damage * 0.1f;
				character.OnDamaged(source, character, InjuryType.Fire, InjuryLocation.Torso, InfectionType.None, null, SkillType.Construction, ref damage2, 0f, Vector3.zero, Vector3.zero, bone, hitPosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
			}
		}
	}

	public virtual InhabitantSlotDef[] GetInhabitantSlotDefs()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.Inhabitants;
	}

	public virtual EntranceDef[] GetEntranceDefs()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.Entrances;
	}

	public TerrainCoord GetEntranceTile(int i)
	{
		return GameTerrain.Instance.GetTileCoordForPos(GetEntrancePos(i));
	}

	public virtual Vector3 GetEntrancePos(int i)
	{
		TerrainCoord entranceOffset = GetEntranceDefs()[i].EntranceOffset;
		return World.MultiplyPoint(new Vector3(entranceOffset.x, 0f, entranceOffset.y));
	}

	public virtual float GetEntranceAngle(int i)
	{
		float entranceAngle = GetEntranceDefs()[i].EntranceAngle;
		return MathUtil.WrapAngle(GetFacingAngleRad() + entranceAngle);
	}

	public Vector3 GetGarageDoorPos()
	{
		return World.MultiplyPoint(new Vector3(GarageDoorOffset.x, 0f, GarageDoorOffset.y));
	}

	public float GetGarageDoorAngle()
	{
		return MathUtil.WrapAngle(GetFacingAngleRad() + GarageDoorAngle);
	}

	public void PlayEnterSound(Character character)
	{
		if (character.CheckFrontmostPrediction(PredictedEventType.EnterBuildingSound))
		{
			if (this is EnterableVehicle)
			{
				SoundManager.PlaySound3D(SoundManager.EnterVehicleSound, character.Pos, 0.5f);
			}
			else if (HasAnyInternalSlots() && !(this is TiltedBuilding))
			{
				SoundManager.PlaySound3D(SoundManager.EnterBuildingSound, character.Pos, 0.5f);
			}
		}
	}

	public int GetClosestEntranceTo(TerrainCoord tile)
	{
		return GetClosestEntranceTo(tile, null, mustBeUnblocked: true);
	}

	public int GetClosestEntranceTo(TerrainCoord tile, Character checkFailedExitAttemptsBy, bool mustBeUnblocked)
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < GetEntranceDefs().Length; i++)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(GetEntrancePos(i));
			float distSquared = tile.GetDistSquared(tileCoordForPos);
			if (distSquared < num && (checkFailedExitAttemptsBy == null || !checkFailedExitAttemptsBy.HasFailedExitBuildingAttempt(this, i)) && (!mustBeUnblocked || !GameTerrain.Instance.IsImpassable(tileCoordForPos.x, tileCoordForPos.y, 1024, null, null)))
			{
				num = distSquared;
				num2 = i;
			}
		}
		if (num2 == -1 && checkFailedExitAttemptsBy != null)
		{
			return GetClosestEntranceTo(tile, null, mustBeUnblocked);
		}
		if (num2 == -1 && mustBeUnblocked)
		{
			return GetClosestEntranceTo(tile, checkFailedExitAttemptsBy, mustBeUnblocked: false);
		}
		return num2;
	}

	public int GetFurthestEntranceFrom(TerrainCoord tile)
	{
		return GetFurthestEntranceFrom(tile, null);
	}

	public int GetFurthestEntranceFrom(TerrainCoord tile, Character checkFailedExitAttemptsBy)
	{
		float num = float.MinValue;
		int num2 = -1;
		for (int i = 0; i < GetEntranceDefs().Length; i++)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(GetEntrancePos(i));
			float distSquared = tile.GetDistSquared(tileCoordForPos);
			if (distSquared > num && (checkFailedExitAttemptsBy == null || !checkFailedExitAttemptsBy.HasFailedExitBuildingAttempt(this, i)))
			{
				num = distSquared;
				num2 = i;
			}
		}
		if (num2 == -1 && checkFailedExitAttemptsBy != null)
		{
			return GetFurthestEntranceFrom(tile, null);
		}
		return num2;
	}

	public virtual Vector3 GetSlotPos(int i)
	{
		return World.MultiplyPoint(GetInhabitantSlotDefs()[i].Pos);
	}

	public virtual float GetSlotAngleRad(int i)
	{
		float defaultAngle = GetInhabitantSlotDefs()[i].DefaultAngle;
		return MathUtil.WrapAngle(GetFacingAngleRad() + defaultAngle * (MathF.PI / 180f));
	}

	public float GetInhabitantSlotAngleRad(Character inhabitant)
	{
		InhabitantSlotDef inhabitantSlotDef = GetInhabitantSlotDef(inhabitant);
		return MathUtil.WrapAngle(GetFacingAngleRad() + inhabitantSlotDef.DefaultAngle * (MathF.PI / 180f));
	}

	public bool HasAnyInternalSlots()
	{
		InhabitantSlotDef[] inhabitantSlotDefs = GetInhabitantSlotDefs();
		for (int i = 0; i < inhabitantSlotDefs.Length; i++)
		{
			if (!inhabitantSlotDefs[i].External)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMultipleExternalSlots()
	{
		int num = 0;
		InhabitantSlotDef[] inhabitantSlotDefs = GetInhabitantSlotDefs();
		for (int i = 0; i < inhabitantSlotDefs.Length; i++)
		{
			if (inhabitantSlotDefs[i].External)
			{
				num++;
			}
		}
		return num >= 2;
	}

	public bool IsFull()
	{
		return GetInhabitantCount() >= Inhabitants.Length;
	}

	public override bool IsAccommodation()
	{
		if (IsGuardPost())
		{
			return false;
		}
		BaseObjectType baseObjectType = GetBaseObjectType();
		if (baseObjectType == BaseObjectType.Outhouse || baseObjectType == BaseObjectType.Mine || baseObjectType == BaseObjectType.EnterableVehicle)
		{
			return false;
		}
		if (Prototype.EnterableBySpecies != BaseObjectType.Invalid)
		{
			return Prototype.EnterableBySpecies == BaseObjectType.Human;
		}
		return true;
	}

	public override bool IsGuardPost()
	{
		return !HasAnyInternalSlots();
	}

	public override bool CanSetStoragePolicy()
	{
		if (!HasAnyInternalSlots())
		{
			return HasMultipleExternalSlots();
		}
		return true;
	}

	public override bool IsLockable()
	{
		return true;
	}

	public bool CanEnter(Character character, Character stashedBy = null)
	{
		return CanEnterReason(character, stashedBy) == CursorActionDisabledReason.Enabled;
	}

	public CursorActionDisabledReason CanEnterReason(Character character, Character stashedBy = null)
	{
		CursorActionDisabledReason cursorActionDisabledReason = CouldEnterIfNotFullReason(character, stashedBy);
		if (cursorActionDisabledReason != CursorActionDisabledReason.Enabled)
		{
			return cursorActionDisabledReason;
		}
		if (!IsFull())
		{
			return CursorActionDisabledReason.Enabled;
		}
		return CursorActionDisabledReason.BuildingFull;
	}

	public bool CouldEnterIfNotFull(Character character, Character stashedBy = null)
	{
		return CouldEnterIfNotFullReason(character, stashedBy) == CursorActionDisabledReason.Enabled;
	}

	public CursorActionDisabledReason CouldEnterIfNotFullReason(Character character, Character stashedBy = null)
	{
		if (UnderConstructionInfo != null)
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (Destroyed)
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (!Prototype.IsEnterableBySpecies(character.GetBaseObjectType()))
		{
			return CursorActionDisabledReason.Disabled;
		}
		Character character2 = ((stashedBy != null) ? stashedBy : character);
		if (UnlockedToPlayer && character2.IsControllableByPlayer())
		{
			return CursorActionDisabledReason.Enabled;
		}
		if (Community != null && Community.GrantedMiningRightsToPlayer && character2.IsControllableByPlayer() && this is Mine)
		{
			return CursorActionDisabledReason.Enabled;
		}
		if (Session.Instance.CommunityManager.GetRelationship(Community, character2.Community) == CommunityRelationshipType.Allied)
		{
			return CursorActionDisabledReason.Enabled;
		}
		if (character.CanFollowPlayer && Community != null && Community.CommunityType == CommunityType.Player)
		{
			return CursorActionDisabledReason.Enabled;
		}
		int num = 0;
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				num++;
				if (Inhabitants[i].IsConscious && Inhabitants[i].GetBaseObjectType() == BaseObjectType.Human && !Inhabitants[i].IsInMyCommunityOrAlly(character2) && (!character.CanFollowPlayer || !Inhabitants[i].IsControllableByOrFollowingPlayer()) && (!Inhabitants[i].CanFollowPlayer || !character.IsControllableByOrFollowingPlayer()))
				{
					return CursorActionDisabledReason.OccupiedByOtherCommunity;
				}
			}
		}
		if (Community != null && Community != character2.Community && (Community.HasAnyActiveMembers() || Community.BuildingsCantBeCaptured))
		{
			return CursorActionDisabledReason.OwnedByOtherCommunity;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public bool OnCharacterEnter(Character character, bool wasOrderedInsideBuilding, bool forceEnter = false, Character stashedBy = null)
	{
		if (!character.IsAuthoritative())
		{
			return false;
		}
		if (!CanEnter(character, stashedBy))
		{
			if (!forceEnter)
			{
				return false;
			}
			for (int i = 0; i < Inhabitants.Length; i++)
			{
				if (Inhabitants[i] != null)
				{
					OnCharacterLeave(Inhabitants[i], 0, fromBuildingDestroyed: false, fromRagdolled: false);
				}
			}
		}
		int num = -1;
		for (int j = 0; j < Inhabitants.Length; j++)
		{
			if (Inhabitants[j] == null)
			{
				num = j;
				break;
			}
		}
		if (num == -1)
		{
			return false;
		}
		Inhabitants[num] = character;
		character.OnEnterBuilding(this, wasOrderedInsideBuilding);
		if (character.IsControllableByPlayer() && character.IsConscious)
		{
			MarkInvestigated(character);
		}
		if (IsImpassableProp() && IsGuardPost())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		return true;
	}

	public void OnCharacterLeave(Character character, int entranceIndex, bool fromBuildingDestroyed, bool fromRagdolled)
	{
		character.OnLeaveBuilding(entranceIndex, fromBuildingDestroyed, fromRagdolled);
		if (character.IsAuthoritative())
		{
			int inhabitantIndex = GetInhabitantIndex(character);
			if (inhabitantIndex >= 0)
			{
				Inhabitants[inhabitantIndex] = null;
			}
			if (IsImpassableProp() && IsGuardPost())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
		}
	}

	public void OnCharacterSlotChange(Character character, int newSlot)
	{
		int inhabitantIndex = GetInhabitantIndex(character);
		if (inhabitantIndex != newSlot)
		{
			Character character2 = Inhabitants[newSlot];
			character2?.OnChangeBuildingSlot(newSlot, inhabitantIndex);
			character.OnChangeBuildingSlot(inhabitantIndex, newSlot);
			Inhabitants[inhabitantIndex] = character2;
			Inhabitants[newSlot] = character;
		}
	}

	public override void OnHearSound(AISound sound)
	{
		if (!GetBoundingBox().Intersects(new BoundingSphere(sound.Pos, sound.SoundRadius)) || sound.Type == AISoundType.Choke)
		{
			return;
		}
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && character.Consciousness < Consciousness.Unconscious && sound.Source != character && !character.InTerrain)
			{
				character.OnHearSound(sound);
			}
		}
	}

	public int GetInhabitantIndex(Character character)
	{
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] == character)
			{
				return i;
			}
		}
		return -1;
	}

	public Character GetFirstInhabitant()
	{
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				return Inhabitants[i];
			}
		}
		return null;
	}

	public InhabitantSlotDef GetInhabitantSlotDef(Character character)
	{
		if (character.IsPredicted())
		{
			character = character.Authoritative;
		}
		int num = GetInhabitantIndex(character);
		if (num == -1)
		{
			num = 0;
		}
		return GetInhabitantSlotDefs()[num];
	}

	public int GetInhabitantCount()
	{
		int num = 0;
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetExternalInhabitantCount()
	{
		int num = 0;
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null && GetInhabitantSlotDefs()[i].External)
			{
				num++;
			}
		}
		return num;
	}

	public virtual bool AddTalkToInhabitantActions(Character controlledCharacter, List<AvailableAction> actions)
	{
		bool flag = false;
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && (!character.IsConscious || !controlledCharacter.IsEnemy(character)))
			{
				CursorActionDisabledReason cursorActionDisabledReason = CursorActionDisabledReason.Disabled;
				if (controlledCharacter.IsInMyCommunityOrAlly(character) || !Session.Instance.GameCamera.FlyCam)
				{
					cursorActionDisabledReason = CursorActionDisabledReason.Enabled;
				}
				if (character.GetPregnancyProgression() >= 1f)
				{
					cursorActionDisabledReason = CursorActionDisabledReason.InLabor;
				}
				actions.Add(new AvailableAction(CursorAction.TalkToInhabitant, controlledCharacter, character, this, cursorActionDisabledReason));
				flag = flag || cursorActionDisabledReason == CursorActionDisabledReason.Enabled;
			}
		}
		return flag;
	}

	public bool HasInhabitantsWhoCanFollowMe(Character controlledCharacter, out bool allAreFollowingMe, out bool allAreDepressed, out bool allAreBusy)
	{
		bool result = false;
		allAreFollowingMe = true;
		allAreDepressed = true;
		allAreBusy = true;
		Character[] inhabitants = Inhabitants;
		foreach (Character character in inhabitants)
		{
			if (character != null && character != controlledCharacter && character.CanFollowPlayerIncludeAllies())
			{
				result = true;
				if (!character.IsInSameSquad(controlledCharacter))
				{
					allAreFollowingMe = false;
					allAreDepressed &= character.IsTooDepressedToFollowOrders();
					allAreBusy &= character.SquadId != 0;
				}
			}
		}
		return result;
	}

	public bool HasInhabitantsWhoBlockMeFromTakingThings(Community community)
	{
		if (community != null)
		{
			for (int i = 0; i < Inhabitants.Length; i++)
			{
				Character character = Inhabitants[i];
				if (character != null && character.ConsciousAndNotZombie && character.GetBaseObjectType() == BaseObjectType.Human && community.GetRelationship(character.Community) == CommunityRelationshipType.Hostile)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Community GetOccupiedByCommunity()
	{
		for (int i = 0; i < Inhabitants.Length; i++)
		{
			if (Inhabitants[i] != null)
			{
				return Inhabitants[i].Community;
			}
		}
		return null;
	}

	public override bool IsUnityObjectAlwaysActive()
	{
		return true;
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (PropEditor.ShowEntrances)
		{
			for (int i = 0; i < GetEntranceDefs().Length; i++)
			{
				GameCursor.DrawExitArrow(this, i, 1f);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecord : IReflectable
{
	public PlayerID PlayerID;

	public string PlayerName = string.Empty;

	public TimeSpan LastUsedTime;

	public Character PlayerCharacter;

	public PlayerMode PlayerMode;

	public bool HasCreatedCharacter;

	public bool IsLocal;

	public bool IsPartyLeader;

	public bool FlyMode;

	public Vector2 SyncedCamFocusPosXZ;

	public float SyncedCamAngle;

	public float SyncedFlyModeZoomDist = GameCamera.ZoomedOutDistMax;

	public TileObject SyncedPipObj;

	public bool SyncedIsNavigatingMenus;

	public bool SyncedIsInInfoScreen;

	public bool JustSetRoleToUrgent;

	public Character SyncedSwappingSupplies;

	public TileObject SyncedSwappingSuppliesWith;

	public SwappingSuppliesMode SyncedSwappingSuppliesMode;

	public bool WantLockOnTarget;

	public TileObject TargetObject;

	public Vector3 TargetPos;

	public float ThrowAngle;

	public float ThrowSpeed;

	public TargettableBodyLocation TargetBodyLocationToAimFor;

	public bool MustLetGoToFireAgain;

	public SortBy SyncedInventorySortBy;

	public SortOrder SyncedInventorySortOrder;

	public SortCharactersBy SyncedCharactersSortBy;

	public SortOrder SyncedCharactersSortOrder;

	public QuestInstance SyncedCurrentQuest;

	public MineralType SyncedGeologicalMap = MineralType.None;

	public Vector3 SyncedMapCamPosition;

	public Hint[] SyncedHint = new Hint[39];

	public bool HasScavengedAnyEquipment;

	public TimeSpan LastPowerNapTime = Target.Never;

	public List<Character> DeterministicVisibleCharacters = new List<Character>();

	public List<Character> SelectedCharacters = new List<Character>();

	public List<ShortcutGroup> ShortcutGroups = new List<ShortcutGroup>();

	public List<MapMarkerLocation> MapMarkerLocations = new List<MapMarkerLocation>();

	public static float SwitchTargetAimingPenalty = 10f;

	public static float HeadAimingPenalty = 10f;

	public bool IsPlayerControllableIgnoringAIOverridesControl()
	{
		if (PlayerMode == PlayerMode.Controlling && PlayerCharacter != null)
		{
			return PlayerCharacter.IsControllableByPlayer();
		}
		return false;
	}

	public bool IsPlayerControllableAndPredicted()
	{
		if (IsPlayerControllable())
		{
			return PlayerCharacter.IsPredicted();
		}
		return false;
	}

	public bool IsPlayerControllable()
	{
		if (IsPlayerControllableIgnoringAIOverridesControl())
		{
			return PlayerCharacter.AIOverridesControl() == AIOverridesControlReason.None;
		}
		return false;
	}

	public bool IsPlayerMoveableAndPredicted()
	{
		if (IsPlayerMoveable())
		{
			return PlayerCharacter.IsPredicted();
		}
		return false;
	}

	public bool IsPlayerMoveable()
	{
		if (IsPlayerControllableIgnoringAIOverridesControl())
		{
			AIOverridesControlReason aIOverridesControlReason = PlayerCharacter.AIOverridesControl();
			if (aIOverridesControlReason == AIOverridesControlReason.None || aIOverridesControlReason == AIOverridesControlReason.Vaulting)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSelectedCharacterCommandable(Character character)
	{
		if (character != PlayerCharacter)
		{
			return character.IsControllableByPlayer();
		}
		return false;
	}

	public bool IsSelectedCharacterControllable(Character character)
	{
		if (character != PlayerCharacter && character.IsControllableByPlayer() && FlyMode)
		{
			return character.AIOverridesControl() == AIOverridesControlReason.None;
		}
		return false;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref PlayerID);
		reflector.Add(ref PlayerName);
		reflector.Add(ref LastUsedTime);
		reflector.Add(ref PlayerCharacter);
		reflector.Add(ref PlayerMode);
		reflector.AddAfter(ref HasCreatedCharacter, 141);
		reflector.Add(ref FlyMode);
		reflector.Add(ref SyncedCamFocusPosXZ);
		reflector.Add(ref SyncedCamAngle);
		reflector.AddAfter(ref SyncedFlyModeZoomDist, 121);
		reflector.Add(ref SyncedPipObj);
		if (reflector.IsDoingNetworkChecksum && reflector.Version >= 220)
		{
			reflector.AddGameObjectRefList(ref DeterministicVisibleCharacters);
		}
		if (reflector.Version < 199)
		{
			bool value = false;
			reflector.Add(ref value);
		}
		reflector.Add(ref SyncedInventorySortBy);
		reflector.Add(ref SyncedInventorySortOrder);
		reflector.AddAfter(ref SyncedCurrentQuest, 34);
		if (reflector.Version < 578)
		{
			TerrainCoord value2 = TerrainCoord.Invalid;
			reflector.AddAfter(ref value2, 136);
			MapMarkerLocations.Add(new MapMarkerLocation(MapMarkerType.Red, value2));
		}
		else
		{
			reflector.Add(ref MapMarkerLocations);
		}
		reflector.AddAfter(ref SyncedMapCamPosition, 173);
		if (reflector.Version >= 190)
		{
			reflector.AddStructArray(ref SyncedHint);
		}
		if (reflector.Version >= 127)
		{
			reflector.AddGameObjectRefList(ref SelectedCharacters);
		}
		reflector.AddAfter(ref HasScavengedAnyEquipment, 196);
		reflector.AddAfter(ref LastPowerNapTime, 412);
		if (reflector.Version >= 237)
		{
			reflector.Add(ref ShortcutGroups);
		}
	}

	public void CopyPlayerRecordSettings(PlayerRecord other)
	{
		PlayerCharacter = other.PlayerCharacter;
		FlyMode = other.FlyMode;
		SyncedCamFocusPosXZ = other.SyncedCamFocusPosXZ;
		SyncedCamAngle = other.SyncedCamAngle;
		SyncedFlyModeZoomDist = other.SyncedFlyModeZoomDist;
		SyncedPipObj = other.SyncedPipObj;
		SyncedInventorySortBy = other.SyncedInventorySortBy;
		SyncedInventorySortOrder = other.SyncedInventorySortOrder;
		SyncedCurrentQuest = other.SyncedCurrentQuest;
		SyncedMapCamPosition = other.SyncedMapCamPosition;
		for (int i = 0; i < 39; i++)
		{
			SyncedHint[i] = other.SyncedHint[i];
		}
		other.MapMarkerLocations.CopyToList(MapMarkerLocations);
	}

	public TileObject GetPlayerCharacterOrBuildingTheyAreIn()
	{
		if (PlayerCharacter != null && PlayerCharacter.InsideBuilding != null)
		{
			return PlayerCharacter.InsideBuilding;
		}
		return PlayerCharacter;
	}

	public void SetTargetObject(TileObject targetObject, TargettableBodyLocation targettableBodyLocation)
	{
		TargetObject = targetObject;
		TargetBodyLocationToAimFor = targettableBodyLocation;
		PlayerCharacter.ApplyAimingPenalty(SwitchTargetAimingPenalty);
	}

	public void SetTargetBodyLocationToAimFor(TargettableBodyLocation targettableBodyLocation)
	{
		TargetBodyLocationToAimFor = targettableBodyLocation;
		if (TargetBodyLocationToAimFor == TargettableBodyLocation.Head)
		{
			PlayerCharacter.ApplyAimingPenalty(HeadAimingPenalty);
		}
	}

	public ShortcutGroup GetShortcutGroup(int group)
	{
		foreach (ShortcutGroup shortcutGroup in ShortcutGroups)
		{
			if (shortcutGroup.Group == group)
			{
				return shortcutGroup;
			}
		}
		return null;
	}

	public void SetShortcutGroup(int group, Character controlledCharacter, List<Character> selectedCharacters)
	{
		ShortcutGroup shortcutGroup = GetShortcutGroup(group);
		if (shortcutGroup == null)
		{
			shortcutGroup = new ShortcutGroup();
			shortcutGroup.Group = group;
			ShortcutGroups.Add(shortcutGroup);
		}
		shortcutGroup.ControlledCharacter = controlledCharacter;
		shortcutGroup.SelectedCharacters = selectedCharacters;
	}

	public bool IsMapMarkerTypeSet(MapMarkerType markerType)
	{
		for (int i = 0; i < MapMarkerLocations.Count; i++)
		{
			if (MapMarkerLocations[i].Type == markerType)
			{
				return true;
			}
		}
		return false;
	}
}

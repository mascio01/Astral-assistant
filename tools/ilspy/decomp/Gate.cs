using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Gate : Prop
{
	private static PrefabResource[] GateModels = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Fences/WoodFenceGate"),
		new PrefabResource("Prefabs/Fences/WireFenceGate"),
		new PrefabResource("Prefabs/Fences/PicketFenceGate")
	};

	public GateState State = GateState.Closed;

	public GatePolicy GatePolicy;

	public bool OverrideDefaultGatePolicy;

	public bool BlockAnimals;

	public bool AINeverUseMe;

	private float _openTransitionStartTime = float.MinValue;

	private bool _startedLocked;

	public EquipmentPrototype KeyProto;

	public TimeSpan DontCloseUntilTime = Target.Never;

	public static int HUD_OpenableByFriendsExceptWhenInsideAndUnderAttack = StringUtil.JenkinsHash("HUD_OpenableByFriendsExceptWhenInsideAndUnderAttack");

	public static int HUD_OpenableByFriendsEvenWhenUnderAttack = StringUtil.JenkinsHash("HUD_OpenableByFriendsEvenWhenUnderAttack");

	public static int HUD_OpenableByCommunityExceptWhenInsideAndUnderAttack = StringUtil.JenkinsHash("HUD_OpenableByCommunityExceptWhenInsideAndUnderAttack");

	public static int HUD_OpenableByCommunityEvenWhenUnderAttack = StringUtil.JenkinsHash("HUD_OpenableByCommunityEvenWhenUnderAttack");

	public static int HUD_NeverOpenable = StringUtil.JenkinsHash("HUD_NeverOpenable");

	public static int HUD_CompletelyOpen = StringUtil.JenkinsHash("HUD_CompletelyOpen");

	private static List<TileObject> _blockingObjects = new List<TileObject>();

	public static string[] GateStateNames = StringUtil.GetEnumNames<GateState>();

	private static float OpenTime = 1f;

	private List<GameObject> UnityGate1 = new List<GameObject>();

	private List<GameObject> UnityGate2 = new List<GameObject>();

	private static string UnityUpdateStr = "Gate.UnityUpdate";

	public static string Gate1Str = "Gate1";

	public static string Gate2Str = "Gate2";

	public override Color32 MapColor => GameTerrain.MinimapSettings.PropCol;

	public GateState GateState
	{
		get
		{
			return State;
		}
		private set
		{
			State = value;
			GameTerrain.Instance.LastChangedTime = Session.Instance.PlayTime;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Gate;
	}

	public static int GetGatePolicyStringHash(GatePolicy gatePolicy)
	{
		return gatePolicy switch
		{
			GatePolicy.OpenableByFriendsExceptWhenInsideAndUnderAttack => HUD_OpenableByFriendsExceptWhenInsideAndUnderAttack, 
			GatePolicy.OpenableByFriendsEvenWhenUnderAttack => HUD_OpenableByFriendsEvenWhenUnderAttack, 
			GatePolicy.OpenableByCommunityExceptWhenInsideAndUnderAttack => HUD_OpenableByCommunityExceptWhenInsideAndUnderAttack, 
			GatePolicy.OpenableByCommunityEvenWhenUnderAttack => HUD_OpenableByCommunityEvenWhenUnderAttack, 
			GatePolicy.NeverOpenable => HUD_NeverOpenable, 
			GatePolicy.CompletelyOpen => HUD_CompletelyOpen, 
			_ => 0, 
		};
	}

	public void SetGatePolicy(GatePolicy gatePolicy, bool overrideDefault)
	{
		GatePolicy gatePolicy2 = GatePolicy;
		GatePolicy = gatePolicy;
		OverrideDefaultGatePolicy = overrideDefault;
		if (gatePolicy2 != gatePolicy)
		{
			GameTerrain.Instance.LastChangedTime = Session.Instance.PlayTime;
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			StoryManager.Instance.SetConditionsDirty();
			if (GatePolicy == GatePolicy.CompletelyOpen)
			{
				Open(null);
			}
			else if (GatePolicy == GatePolicy.NeverOpenable || gatePolicy2 == GatePolicy.CompletelyOpen)
			{
				Close(null);
			}
		}
	}

	public void SetBlockAnimals(bool blockAnimals)
	{
		if (BlockAnimals != blockAnimals)
		{
			BlockAnimals = blockAnimals;
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref State);
		reflector.AddAfter(ref GatePolicy, 128);
		reflector.AddAfter(ref OverrideDefaultGatePolicy, 381);
		reflector.AddAfter(ref BlockAnimals, 381);
		reflector.AddAfter(ref KeyProto, 497);
		reflector.Add(ref _openTransitionStartTime);
		reflector.Add(ref _startedLocked);
		reflector.Add(ref AINeverUseMe);
		reflector.AddAfter(ref DontCloseUntilTime, 521);
	}

	public override void Init()
	{
		base.Init();
		if (!Session.Instance.Editor && State == GateState.Open)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		if (Session.Instance.Frame == 0)
		{
			_startedLocked = State == GateState.Locked;
		}
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		RecalcFenceNeighbours();
	}

	public override void SetCommunity(Community community)
	{
		base.SetCommunity(community);
		if (Community != null)
		{
			SetGatePolicy(Community.DefaultGatePolicy, overrideDefault: false);
		}
	}

	private bool WantsToClose()
	{
		if (State == GateState.Open && GatePolicy != GatePolicy.CompletelyOpen && Community != null)
		{
			return Community.HasAnyLivingNonZombieMembers();
		}
		return false;
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		if (!WantsToClose())
		{
			return;
		}
		bool flag = false;
		if (Session.Instance.PlayTime < DontCloseUntilTime)
		{
			flag = true;
		}
		else
		{
			TerrainCoord terrainCoord = MinTile - new TerrainCoord(2, 2);
			TerrainCoord terrainCoord2 = MaxTile + new TerrainCoord(2, 2);
			GameTerrain.Instance.GetObjectsInRect(terrainCoord, terrainCoord2, _blockingObjects);
			for (int i = 0; i < _blockingObjects.Count; i++)
			{
				if (_blockingObjects[i] is Character { Tile: var tile } character && tile.IsWithinBounds(terrainCoord, terrainCoord2) && character.AliveAndNotZombie && character.InsideBuilding == null)
				{
					if (character.Tile.IsWithinBounds(MinTile, MaxTile))
					{
						flag = true;
						break;
					}
					if (!character.IsSitting() && character.IsConscious && (!(character.HangOutLocation == character.Tile) || !character.IsBored()))
					{
						flag = true;
						break;
					}
				}
				if (_blockingObjects[i] is EnterableVehicle enterableVehicle && enterableVehicle.GetTileRect().Overlaps(enterableVehicle.IsMoving ? new TerrainRect(terrainCoord, terrainCoord2) : GetTileRect()))
				{
					flag = true;
				}
			}
			_blockingObjects.Clear();
		}
		if (flag)
		{
			stillNeedUpdating = true;
		}
		else
		{
			CloseOrLock();
		}
	}

	public void IncrementGateState()
	{
		if (State == GateState.Open)
		{
			Close(null);
		}
		else if (State == GateState.Closed)
		{
			Lock();
		}
		else
		{
			Open(null);
		}
	}

	public void DecrementGateState()
	{
		if (State == GateState.Open)
		{
			Lock();
		}
		else if (State == GateState.Closed)
		{
			Open(null);
		}
		else
		{
			Close(null);
		}
	}

	public void BuildGateStateString(ref StringBuilder value)
	{
		value.Append(GateStateNames[(int)State]);
	}

	public bool GetAINeverUseMe()
	{
		return AINeverUseMe;
	}

	public void SetAINeverUseMe(bool on)
	{
		AINeverUseMe = on;
	}

	public bool CanOpenMe(Character character, bool fromAI, bool dontOpenOurGates)
	{
		return CanOpenMeReason(character, fromAI, dontOpenOurGates) == CursorActionDisabledReason.Enabled;
	}

	public CursorActionDisabledReason CanOpenMeReason(Character character, bool fromAI, bool dontOpenOurGates, bool canUnlockGatesFromInsideWithKey = false)
	{
		bool flag = KeyProto != null && character.Inventory.FindItemOfType(KeyProto) != null;
		if (State == GateState.Locked)
		{
			bool flag2 = flag;
			if (fromAI && !canUnlockGatesFromInsideWithKey)
			{
				flag2 &= IsInFrontOfGate(character.PosXZ);
			}
			if (!flag2)
			{
				return CursorActionDisabledReason.GateLocked;
			}
		}
		if (!character.CanOpenGates())
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (fromAI && AINeverUseMe)
		{
			return CursorActionDisabledReason.Disabled;
		}
		if (GatePolicy == GatePolicy.NeverOpenable)
		{
			return CursorActionDisabledReason.GatePolicyIsAlwaysClosed;
		}
		if (GatePolicy == GatePolicy.CompletelyOpen)
		{
			return CursorActionDisabledReason.Enabled;
		}
		if (Community == character.Community && Community != null && (GatePolicy == GatePolicy.OpenableByFriendsEvenWhenUnderAttack || GatePolicy == GatePolicy.OpenableByCommunityEvenWhenUnderAttack))
		{
			return CursorActionDisabledReason.Enabled;
		}
		if (Community != null && Community != character.Community && Community.IsAnyoneConscious(includeDrunk: true) && character.Community != null && IsInFrontOfGate(character.PosXZ))
		{
			if (Community == Session.Instance.CommunityManager.PlayerCommunity && character.Community != Session.Instance.CommunityManager.PlayerCommunity)
			{
				if (character.CanOpenPlayerGates == CanOpenGates.No)
				{
					return CursorActionDisabledReason.Disabled;
				}
				if (character.CanOpenPlayerGates == CanOpenGates.Unknown && character.InCombat)
				{
					return CursorActionDisabledReason.Disabled;
				}
			}
			CommunityRelationshipType relationship = Session.Instance.CommunityManager.GetRelationship(Community, character.Community);
			if (relationship == CommunityRelationshipType.Hostile && !flag && !Community.IsSurrendering() && Community.IsAnyMemberInsidePerimeter())
			{
				return CursorActionDisabledReason.CantOpenEnemyGate;
			}
			if (character.IsControllableByPlayer() && !flag && relationship <= CommunityRelationshipType.Introducing)
			{
				return CursorActionDisabledReason.CantOpenUnintroducedGate;
			}
			if (GatePolicy == GatePolicy.OpenableByCommunityEvenWhenUnderAttack || GatePolicy == GatePolicy.OpenableByCommunityExceptWhenInsideAndUnderAttack)
			{
				return CursorActionDisabledReason.Disabled;
			}
		}
		if (dontOpenOurGates && Community != null && Community == character.Community && !IsInFrontOfGate(character.PosXZ))
		{
			return CursorActionDisabledReason.Disabled;
		}
		return CursorActionDisabledReason.Enabled;
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		float num = base.OnVehicleCollision(other, relativeVelocity, contactPoint, predicted);
		if (num <= 0f)
		{
			Character character = ((other is EnterableVehicle enterableVehicle) ? enterableVehicle.GetDriver() : null);
			if (character != null && CanOpenMe(character, fromAI: false, dontOpenOurGates: false))
			{
				Open(character);
			}
		}
		return num;
	}

	public bool IsInFrontOfGate(Vector2 posXZ)
	{
		return Vector2.Dot(posXZ - PosXZ, MathUtil.ToXZ(base.Right)) >= 0f;
	}

	public TerrainCoord GetTileInsideGate()
	{
		return Orientation switch
		{
			OrientationType.Deg0 => Tile + new TerrainCoord(-1, 0), 
			OrientationType.Deg180 => Tile + new TerrainCoord(1, 0), 
			OrientationType.Deg90 => Tile + new TerrainCoord(0, 1), 
			OrientationType.Deg270 => Tile + new TerrainCoord(0, -1), 
			_ => Tile, 
		};
	}

	public TerrainCoord GetTileOutsideGate()
	{
		return Orientation switch
		{
			OrientationType.Deg0 => Tile + new TerrainCoord(1, 0), 
			OrientationType.Deg180 => Tile + new TerrainCoord(-1, 0), 
			OrientationType.Deg90 => Tile + new TerrainCoord(0, -1), 
			OrientationType.Deg270 => Tile + new TerrainCoord(0, 1), 
			_ => Tile, 
		};
	}

	public void Open(Character opener)
	{
		if (State == GateState.Open)
		{
			return;
		}
		GateState = GateState.Open;
		if (!Session.Instance.Editor && InTerrain)
		{
			_openTransitionStartTime = (float)Session.Instance.PlayTime.TotalSeconds;
			DontCloseUntilTime = Session.Instance.PlayTime + TimeSpan.FromSeconds(2.0);
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			StoryManager.Instance.SetConditionsDirty();
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
			if (opener != null)
			{
				opener.PlaySoundUsingFootstepAudioSource(SoundManager.OpenGateSound, 1f);
			}
			else
			{
				SoundManager.PlaySound3D(SoundManager.OpenGateSound, Pos);
			}
		}
	}

	public void Close(Character closer)
	{
		if (State != GateState.Open)
		{
			return;
		}
		GateState = GateState.Closed;
		if (!Session.Instance.Editor && InTerrain)
		{
			_openTransitionStartTime = (float)Session.Instance.PlayTime.TotalSeconds;
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			StoryManager.Instance.SetConditionsDirty();
			if (closer != null)
			{
				closer.PlaySoundUsingFootstepAudioSource(SoundManager.CloseGateSound, 1f);
			}
			else
			{
				SoundManager.PlaySound3D(SoundManager.CloseGateSound, Pos);
			}
		}
	}

	public void CloseOrLock()
	{
		if (_startedLocked && Community != null)
		{
			Lock();
		}
		else
		{
			Close(null);
		}
	}

	public void Lock()
	{
		Close(null);
		GateState = GateState.Locked;
		if (!Session.Instance.Editor && InTerrain)
		{
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public void Unlock()
	{
		if (State != GateState.Locked)
		{
			return;
		}
		GateState = GateState.Closed;
		_startedLocked = false;
		if (!Session.Instance.Editor && InTerrain)
		{
			if (IsImpassableProp())
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public override bool IsTargetable()
	{
		if (Destroyed)
		{
			return false;
		}
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		return GameTerrain.Instance.FogOfWar.IsAnyTileInRectExplored(MinTile, MaxTile);
	}

	public override float GetOpenTransition()
	{
		if (Session.Instance != null)
		{
			float num = (float)Session.Instance.PlayTime.TotalSeconds - _openTransitionStartTime;
			if (num < OpenTime)
			{
				if (State != GateState.Open)
				{
					return 1f - num / OpenTime;
				}
				return num / OpenTime;
			}
		}
		if (State != GateState.Open)
		{
			return 0f;
		}
		return 1f;
	}

	public override void UnityInit()
	{
		base.UnityInit();
		for (int i = 0; i < UnityObj.transform.childCount; i++)
		{
			GameObject gameObject = UnityObj.transform.GetChild(i).gameObject;
			string name = gameObject.name;
			if (name.IndexOf(Gate1Str) != -1)
			{
				UnityGate1.Add(gameObject);
			}
			else if (name.IndexOf(Gate2Str) != -1)
			{
				UnityGate2.Add(gameObject);
			}
		}
	}

	public override void UnityDelete()
	{
		UnityGate1.Clear();
		UnityGate2.Clear();
		base.UnityDelete();
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		if (InTerrain)
		{
			Session.Instance.StaticUnityObjectsThatNeedUpdate.Add(this);
		}
		UnityUpdateDoors();
	}

	public override void UnityDeactivate()
	{
		if (InTerrain)
		{
			Session.Instance.StaticUnityObjectsThatNeedUpdate.Remove(this);
		}
		base.UnityDeactivate();
	}

	public override void UnityUpdate()
	{
		using (new UnityProfileMarker(UnityUpdateStr))
		{
			base.UnityUpdate();
			Session instance = Session.Instance;
			if (instance != null && (float)instance.PlayTime.TotalSeconds - _openTransitionStartTime <= OpenTime + 1f)
			{
				UnityUpdateDoors();
			}
		}
	}

	private void UnityUpdateDoors()
	{
		if (!(UnityObj != null) || UnderConstructionInfo != null)
		{
			return;
		}
		float openTransition = GetOpenTransition();
		float childAngle = GetChildAngle(Gate1Str, openTransition);
		float childAngle2 = GetChildAngle(Gate2Str, openTransition);
		foreach (GameObject item in UnityGate1)
		{
			item.transform.localEulerAngles = new Vector3(item.transform.localEulerAngles.x, childAngle * 57.29578f, item.transform.localEulerAngles.z);
		}
		foreach (GameObject item2 in UnityGate2)
		{
			item2.transform.localEulerAngles = new Vector3(item2.transform.localEulerAngles.x, childAngle2 * 57.29578f, item2.transform.localEulerAngles.z);
		}
	}

	public static float GetChildAngle(string childName, float openTransition)
	{
		if (childName.IndexOf(Gate1Str) != -1)
		{
			return openTransition * (MathF.PI / 2f);
		}
		if (childName.IndexOf(Gate2Str) != -1)
		{
			return (0f - openTransition) * (MathF.PI / 2f);
		}
		return 0f;
	}

	public override void Delete()
	{
		base.Delete();
		RecalcFenceNeighbours();
	}

	public override void SetOrientationType(OrientationType orientation)
	{
		base.SetOrientationType(orientation);
		if (InTerrain)
		{
			RecalcFenceNeighbours();
		}
	}

	public override void SetTile(TerrainCoord tile)
	{
		base.SetTile(tile);
		if (InTerrain)
		{
			RecalcFenceNeighbours();
		}
	}

	public void RecalcFenceNeighbours()
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord tile = Tile;
		TerrainCoord tl = tile - new TerrainCoord(3, 3);
		TerrainCoord br = tile + new TerrainCoord(3, 3);
		for (int i = tl.x - 3; i <= br.x + 3; i++)
		{
			for (int j = tl.y - 3; j <= br.y + 3; j++)
			{
				instance.RecalcFence(i, j);
			}
		}
		if (instance.UnityTerrainObj != null)
		{
			instance.BuildMinimap(tl, br);
		}
	}

	public override bool IsImpassableProp()
	{
		if (UnderConstructionInfo != null && !UnderConstructionInfo.IsCompleted())
		{
			return false;
		}
		return base.IsImpassableProp();
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 4) != 0)
		{
			return true;
		}
		if (UnderConstructionInfo != null && !UnderConstructionInfo.IsCompleted())
		{
			return false;
		}
		if (State == GateState.Open && (Community == null || Community.CommunityType != CommunityType.Player || GatePolicy == GatePolicy.CompletelyOpen || requester == null || requester.Community == null || requester.Community.CommunityType == CommunityType.Player || requester.CanOpenPlayerGates == CanOpenGates.Yes || requester.GetBaseObjectType() != BaseObjectType.Human || requester.Zombie || (requester.SquadLeader != null && requester.SquadLeader.DirectControlled) || !IsInFrontOfGate(GameTerrain.Instance.GetTileCentreXZ(requester.Tile)) || GetTileRect().Contains(requester.Tile) || Session.Instance.CommunityManager.GetRelationship(Community, requester.Community) == CommunityRelationshipType.Hostile) && (requester == null || requester.GetBaseObjectType() == BaseObjectType.Human || !BlockAnimals))
		{
			return false;
		}
		if (requester == null)
		{
			return true;
		}
		if ((options & 0x200) != 0 && CoverType == CoverType.WaistHigh)
		{
			return false;
		}
		if ((options & 0x10) != 0 && IsFlammable() && !requester.IsInMyCommunityOrAlly(this))
		{
			return false;
		}
		if ((options & 0x20) != 0 && !IsExplosionProof() && !requester.IsInMyCommunityOrAlly(this))
		{
			return false;
		}
		return true;
	}

	public override bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		if (builderCommunity != null && builderCommunity.CommunityType == CommunityType.Player && Prototype.CanBeClearedForBuilding)
		{
			Community community = GetCommunity();
			if (!(newBuilding is Gate gate))
			{
				return false;
			}
			if (gate.Orientation != Orientation || gate.Tile != Tile)
			{
				return false;
			}
			if (newBuilding.GetPropPrototype() == Prototype)
			{
				return false;
			}
			if (community == builderCommunity)
			{
				return true;
			}
			if (community != null && !community.HasAnyActiveMembers())
			{
				return true;
			}
		}
		return false;
	}

	protected override Vector3 CalcWorldPos()
	{
		Vector3 result = base.CalcWorldPos();
		CalcMinMaxTile(Tile, Orientation, out var minTile, out var maxTile);
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				if (!GameTerrain.Instance.IsTileOutsideBounds(i, j))
				{
					result.y = Math.Max(result.y, GameTerrain.Instance.GetTileMaxHeight(i, j, includeRiver: true));
				}
			}
		}
		if (Destroyed)
		{
			result += new Vector3(DemolitionShakeOffset.x, DemolitionShakeOffset.y - DemolitionTransition, DemolitionShakeOffset.z);
		}
		return result;
	}
}

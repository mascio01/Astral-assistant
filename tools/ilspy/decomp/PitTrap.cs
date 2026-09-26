using System.Collections.Generic;
using UnityEngine;

public class PitTrap : SingleTileProp, ITrap
{
	private static float DitchDepth = 1.5f;

	private static PrefabResource[] UnityTrapSpikes = new PrefabResource[3]
	{
		new PrefabResource("Prefabs\\Props\\Trap\\TrapSpikes1", 20),
		new PrefabResource("Prefabs\\Props\\Trap\\TrapSpikes2", 20),
		new PrefabResource("Prefabs\\Props\\Trap\\TrapSpikes3", 20)
	};

	private static PrefabResource[] UnityTrapCovers = new PrefabResource[1]
	{
		new PrefabResource("Prefabs\\Props\\Trap\\TrapCover1", 20)
	};

	public bool IsJoiner;

	public bool IsCovered;

	public int FreeResets;

	public int TerrainModifiedPatchId;

	public GameObject UnityObjCover;

	public GameObject UnityObjScaffold;

	private static string TrapCoverName = "TrapCover";

	public static int MaxFreeResets = 3;

	public override Vector3 Pos => GameTerrain.Instance.GetTileCentrePos(Tile, ignoreIce: true, ignoreRoadCamber: true);

	public override Color32 MapColor
	{
		get
		{
			if (!IsJoiner)
			{
				return GameTerrain.MinimapSettings.TrapCol;
			}
			return MathUtil.TransparentBlack;
		}
	}

	public static PitTrap Spawn(TerrainCoord tile, bool joiner)
	{
		if (GameTerrain.Instance.GetPitTrapOnTile(tile.x, tile.y) != null)
		{
			TerrainCoord terrainCoord = tile;
			Debug.Log("Existing pit trap on tile " + terrainCoord.ToString());
		}
		PitTrap obj = new PitTrap
		{
			Tile = tile,
			IsJoiner = joiner,
			IsCovered = true
		};
		GameTerrain.Instance.SetFlag(tile.x, tile.y, TileFlags.Trap, on: true);
		obj.OnSpawn();
		GameTerrain.Instance.TellObjectsThatHeightChanged(tile, tile);
		return obj;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref IsJoiner);
		reflector.Add(ref IsCovered);
		reflector.AddAfter(ref FreeResets, 182);
		reflector.AddAfter(ref TerrainModifiedPatchId, 425);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PitTrap;
	}

	public override Texture2D GetIconResource()
	{
		return Prop.PitTrapIcon;
	}

	public override PrefabResource GetUnityModel()
	{
		if (!IsJoiner)
		{
			if (Prototype == null)
			{
				return UnityTrapSpikes[MathUtil.RandomInt(Id, UnityTrapSpikes.Length)];
			}
			return base.GetUnityModel();
		}
		return null;
	}

	public PrefabResource GetUnityModelScaffold()
	{
		return UnityTrapCovers[MathUtil.RandomInt(Id * 234, UnityTrapCovers.Length)];
	}

	public override bool IsImpassableSingleTileProp()
	{
		if (!IsJoiner && !IsCovered)
		{
			return UnderConstructionInfo == null;
		}
		return false;
	}

	public override bool HasUnityObject()
	{
		if (!IsCovered)
		{
			return base.HasUnityObject();
		}
		return true;
	}

	public override bool IsUnityObjectActive()
	{
		if (!(UnityObjCover != null) || !UnityObjCover.activeSelf)
		{
			return base.IsUnityObjectActive();
		}
		return true;
	}

	public override void Init()
	{
		base.Init();
		if (IsCovered)
		{
			GameTerrain.Instance.SetFlag(Tile.x, Tile.y, TileFlags.Trap, on: true);
		}
		else
		{
			GameTerrain.Instance.TrapSignMapWho.AddToMapWho(this, Tile);
		}
		if (!IsJoiner)
		{
			GetCommunity()?.PitTraps.Add(this);
			if (UnderConstructionInfo == null)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
		}
	}

	public override void Delete()
	{
		if (!IsJoiner)
		{
			if (UnderConstructionInfo == null)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
			}
			GetCommunity()?.PitTraps.Remove(this);
		}
		if (IsCovered)
		{
			if (!GameTerrain.Instance.IsAnyCoveredTrapOnTile(Tile.x, Tile.y, this))
			{
				GameTerrain.Instance.SetFlag(Tile.x, Tile.y, TileFlags.Trap, on: false);
			}
		}
		else
		{
			GameTerrain.Instance.TrapSignMapWho.RemoveFromMapWho(this, Tile);
		}
		if (TerrainModifiedPatchId != 0)
		{
			GameTerrain.Instance.RemoveModifiedPatch(TerrainModifiedPatchId);
			TerrainModifiedPatchId = 0;
		}
		base.Delete();
		if (!IsJoiner)
		{
			UpdateNearbyJoiners(deletingMe: true);
		}
		if (IsCovered && Session.Instance.State == SessionState.Started)
		{
			GameTerrain.Instance.TellObjectsThatHeightChanged(Tile, Tile);
		}
	}

	public override void SetCommunity(Community community)
	{
		if (!IsJoiner)
		{
			GetCommunity()?.PitTraps.Remove(this);
		}
		base.SetCommunity(community);
		if (!IsJoiner)
		{
			community?.PitTraps.Add(this);
		}
	}

	public override void SetUnderConstructionInfo(UnderConstructionInfo underConstructionInfo)
	{
		if (underConstructionInfo == null)
		{
			List<BaseObject> list = new List<BaseObject>();
			GameTerrain.Instance.FindDuplicatePitTraps(this, list);
			foreach (BaseObject item in list)
			{
				item.Delete();
			}
			IsCovered = true;
			GameTerrain.Instance.SetFlag(Tile.x, Tile.y, TileFlags.Trap, on: true);
			GameTerrain.Instance.TrapSignMapWho.RemoveFromMapWho(this, Tile);
			GameTerrain.Instance.TellObjectsThatHeightChanged(Tile, Tile);
			if (!IsJoiner)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
			}
			GameTerrain instance = GameTerrain.Instance;
			TerrainModifiedPatchId = instance.AddModifiedPatch(new ModifiedPatch
			{
				Type = TerrainModificationType.PitTrap,
				MinTile = Tile,
				MaxTile = Tile,
				DitchHeight = DitchDepth
			});
			CreateUnityCover();
		}
		base.SetUnderConstructionInfo(underConstructionInfo);
		if (underConstructionInfo != null)
		{
			return;
		}
		UpdateNearbyJoiners(deletingMe: false);
		for (int i = Tile.x - 1; i <= Tile.x + 1; i++)
		{
			for (int j = Tile.y - 1; j <= Tile.y + 1; j++)
			{
				if (i == Tile.x && j == Tile.y)
				{
					continue;
				}
				PitTrap pitTrapOnTile = GameTerrain.Instance.GetPitTrapOnTile(i, j);
				if (pitTrapOnTile != null && pitTrapOnTile.GetUnderConstructionInfo() == null && pitTrapOnTile.UnityObjCover != null)
				{
					bool num = pitTrapOnTile.IsUnityObjectActive();
					if (num)
					{
						pitTrapOnTile.UnityDeactivate();
					}
					pitTrapOnTile.UnityDelete();
					pitTrapOnTile.UnityInit();
					if (num)
					{
						pitTrapOnTile.UnityActivate();
					}
				}
			}
		}
	}

	public void UpdateNearbyJoiners(bool deletingMe)
	{
		for (int i = Tile.x - 1; i <= Tile.x + 1; i++)
		{
			for (int j = Tile.y - 1; j <= Tile.y + 1; j++)
			{
				TerrainCoord terrainCoord = new TerrainCoord(i, j);
				if (terrainCoord == Tile)
				{
					if (deletingMe && WantJoinerOnTile(terrainCoord) && GameTerrain.Instance.GetPitTrapOnTile(i, j) == null)
					{
						Session.Instance.ClearTrashForBuilding(new TerrainCoord(i, j), new TerrainCoord(i, j), null, null);
						Spawn(new TerrainCoord(i, j), joiner: true);
					}
					continue;
				}
				PitTrap pitTrapOnTile = GameTerrain.Instance.GetPitTrapOnTile(i, j);
				if (pitTrapOnTile != null && pitTrapOnTile.IsJoiner && (!IsCovered || deletingMe))
				{
					if (!WantJoinerOnTile(pitTrapOnTile.Tile))
					{
						pitTrapOnTile.Delete();
					}
				}
				else if (pitTrapOnTile == null && IsCovered && !deletingMe && WantJoinerOnTile(terrainCoord))
				{
					Session.Instance.ClearTrashForBuilding(new TerrainCoord(i, j), new TerrainCoord(i, j), null, null);
					Spawn(new TerrainCoord(i, j), joiner: true);
				}
			}
		}
	}

	public static bool WantJoinerOnTile(TerrainCoord tile)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = tile.x - 1; i <= tile.x + 1; i++)
		{
			for (int j = tile.y - 1; j <= tile.y + 1; j++)
			{
				if (new TerrainCoord(i, j) == tile)
				{
					continue;
				}
				PitTrap pitTrapOnTile = GameTerrain.Instance.GetPitTrapOnTile(i, j);
				if (pitTrapOnTile == null || pitTrapOnTile.IsJoiner || pitTrapOnTile.GetUnderConstructionInfo() != null)
				{
					continue;
				}
				if (pitTrapOnTile.Tile.IsAdjacent(tile))
				{
					if (pitTrapOnTile.IsCovered)
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
				if (pitTrapOnTile.IsCovered)
				{
					num3++;
				}
			}
		}
		if (num2 > num)
		{
			return false;
		}
		return num3 > 0;
	}

	private void CreateUnityCover()
	{
		if (UnityObjCover != null)
		{
			Debug.LogWarning("Pit trap already has a cover!");
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		Vector3 vector = GameTerrain.Instance.GetOriginalTileCentrePos(Tile) - new Vector3(0f, DitchDepth, 0f);
		float z = instance.GetOriginalTileMinHeight(Tile.x, Tile.y) - 0.3f;
		Vector3 vector2 = instance.GetOriginalVertexPos(Tile.x, Tile.y) - vector;
		Vector3 vector3 = instance.GetOriginalVertexPos(Tile.x + 1, Tile.y) - vector;
		Vector3 vector4 = instance.GetOriginalVertexPos(Tile.x + 1, Tile.y + 1) - vector;
		Vector3 vector5 = instance.GetOriginalVertexPos(Tile.x, Tile.y + 1) - vector;
		Vector3 vector6 = MathUtil.ToXZY(instance.GetVertexPosXZ(Tile.x, Tile.y), z) - vector;
		Vector3 vector7 = MathUtil.ToXZY(instance.GetVertexPosXZ(Tile.x + 1, Tile.y), z) - vector;
		Vector3 vector8 = MathUtil.ToXZY(instance.GetVertexPosXZ(Tile.x + 1, Tile.y + 1), z) - vector;
		Vector3 vector9 = MathUtil.ToXZY(instance.GetVertexPosXZ(Tile.x, Tile.y + 1), z) - vector;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[24]
		{
			vector2, vector3, vector4, vector5, vector6, vector7, vector3, vector2, vector8, vector9,
			vector4, vector5, vector2, vector5, vector9, vector6, vector3, vector4, vector8, vector7,
			vector6, vector7, vector8, vector9
		};
		mesh.normals = new Vector3[24]
		{
			instance.GetSmoothNormalAtVertex(Tile.x, Tile.y),
			instance.GetSmoothNormalAtVertex(Tile.x + 1, Tile.y),
			instance.GetSmoothNormalAtVertex(Tile.x + 1, Tile.y + 1),
			instance.GetSmoothNormalAtVertex(Tile.x, Tile.y + 1),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0f, -1f, 0f)
		};
		mesh.triangles = new int[36]
		{
			0, 2, 1, 0, 3, 2, 4, 6, 5, 4,
			7, 6, 11, 9, 8, 8, 10, 11, 12, 14,
			13, 12, 15, 14, 16, 17, 18, 16, 18, 19,
			20, 21, 22, 20, 22, 23
		};
		mesh.uv = new Vector2[24]
		{
			MathUtil.ToXZ(instance.GetOriginalVertexPos(Tile.x, Tile.y)) / 4f,
			MathUtil.ToXZ(instance.GetOriginalVertexPos(Tile.x + 1, Tile.y)) / 4f,
			MathUtil.ToXZ(instance.GetOriginalVertexPos(Tile.x + 1, Tile.y + 1)) / 4f,
			MathUtil.ToXZ(instance.GetOriginalVertexPos(Tile.x, Tile.y + 1)) / 4f,
			MathUtil.ToXY(vector6) / 4f,
			MathUtil.ToXY(vector7) / 4f,
			MathUtil.ToXY(vector3) / 4f,
			MathUtil.ToXY(vector2) / 4f,
			MathUtil.ToXY(vector8) / 4f,
			MathUtil.ToXY(vector9) / 4f,
			MathUtil.ToXY(vector4) / 4f,
			MathUtil.ToXY(vector5) / 4f,
			MathUtil.ToZY(vector2) / 4f,
			MathUtil.ToZY(vector5) / 4f,
			MathUtil.ToZY(vector9) / 4f,
			MathUtil.ToZY(vector6) / 4f,
			MathUtil.ToZY(vector3) / 4f,
			MathUtil.ToZY(vector4) / 4f,
			MathUtil.ToZY(vector8) / 4f,
			MathUtil.ToZY(vector7) / 4f,
			MathUtil.ToXZ(vector6) / 4f,
			MathUtil.ToXZ(vector7) / 4f,
			MathUtil.ToXZ(vector8) / 4f,
			MathUtil.ToXZ(vector9) / 4f
		};
		UnityObjCover = new GameObject(TrapCoverName);
		UnityObjCover.SetActive(value: false);
		UnityObjCover.AddComponent<MeshFilter>().mesh = mesh;
		UnityObjCover.AddComponent<MeshRenderer>().material = Prop.MudMaterial;
		UnityObjCover.AddComponent<MeshCollider>().sharedMesh = mesh;
		UnityObjCover.transform.position = vector;
	}

	private void DeleteUnityCover()
	{
		if (UnityObjCover != null)
		{
			Object.Destroy(UnityObjCover);
			UnityObjCover = null;
		}
	}

	public override void UnityInit()
	{
		if (UnderConstructionInfo != null)
		{
			UnityObj = UnderConstructionInfo.UnityInitCordon(this);
			return;
		}
		if (IsCovered)
		{
			CreateUnityCover();
		}
		if (Session.Instance != null && Session.Instance.IsInFocusArea(GetTileRect()))
		{
			UnityActivate();
		}
	}

	public override void UnityDelete()
	{
		if (IsCovered)
		{
			DeleteUnityCover();
		}
		base.UnityDelete();
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		if (IsCovered)
		{
			if (UnityObjScaffold == null)
			{
				UnityObjScaffold = GetUnityModelScaffold().InstantiatePrefab((Id != 0) ? GameTerrain.Instance.UnityTerrainObj.transform : null);
			}
			UnityObjScaffold.SetActive(value: true);
			UnityObjCover.SetActive(value: true);
			UpdateUnityTransform();
		}
	}

	public override void UnityDeactivate()
	{
		if (UnityObjCover != null)
		{
			UnityObjCover.SetActive(value: false);
		}
		if (UnityObjScaffold != null)
		{
			UnityObjScaffold.SetActive(value: false);
			GetUnityModelScaffold().DeletePrefab(UnityObjScaffold);
			UnityObjScaffold = null;
		}
		base.UnityDeactivate();
	}

	public override void UpdateUnityTransform()
	{
		if (UnderConstructionInfo != null || IsGhost())
		{
			if (UnityObj != null)
			{
				Vector3 position = ((GameTerrain.Instance != null) ? GameTerrain.Instance.GetOriginalTileCentrePos(Tile) : Vector3.zero);
				position.y = Mathf.Max(position.y, (GameTerrain.Instance != null) ? GameTerrain.Instance.GetTileCentrePos(Tile).y : 0f);
				UnityObj.transform.position = position;
			}
			return;
		}
		base.UpdateUnityTransform();
		if (UnityObjCover != null)
		{
			UnityObjCover.transform.position = GameTerrain.Instance.GetOriginalTileCentrePos(Tile) - new Vector3(0f, DitchDepth, 0f);
		}
		if (UnityObjScaffold != null)
		{
			UnityObjScaffold.transform.position = GameTerrain.Instance.GetOriginalTileCentrePos(Tile) - new Vector3(0f, DitchDepth, 0f);
			UnityObjScaffold.transform.rotation = Quaternion.Euler(0f, (float)MathUtil.RandomInt(Id * 73, 4) * 90f, 0f);
		}
	}

	public void TriggerTrap(Character character)
	{
		if (IsCovered)
		{
			character.SetPosition(GameTerrain.Instance.GetTileCentrePos(Tile, ignoreIce: false, ignoreRoadCamber: false));
			IsCovered = false;
			GameTerrain.Instance.SetFlag(Tile.x, Tile.y, TileFlags.Trap, on: false);
			GameTerrain.Instance.TellObjectsThatHeightChanged(Tile, Tile);
			GameTerrain.Instance.TrapSignMapWho.AddToMapWho(this, Tile);
			DeleteUnityCover();
			if (!IsJoiner)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
				UpdateNearbyJoiners(deletingMe: false);
			}
			if (IsUnityObjectActive())
			{
				UnityDeactivate();
				UnityActivate();
			}
			SoundManager.PlaySound3DFromList(SoundManager.PitTrapTriggerSounds, Pos);
			character.WantKilledByPitTrap = true;
		}
	}

	public void ResetTrap(Character character, bool isGathering)
	{
		if (CanResetTrap())
		{
			if (FreeResets >= MaxFreeResets)
			{
				FreeResets = 0;
			}
			else
			{
				FreeResets++;
			}
			IsCovered = true;
			GameTerrain.Instance.SetFlag(Tile.x, Tile.y, TileFlags.Trap, on: true);
			GameTerrain.Instance.TellObjectsThatHeightChanged(Tile, Tile);
			GameTerrain.Instance.TrapSignMapWho.RemoveFromMapWho(this, Tile);
			CreateUnityCover();
			if (!IsJoiner)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
				UpdateNearbyJoiners(deletingMe: false);
			}
			if (IsUnityObjectActive())
			{
				UnityDeactivate();
				UnityActivate();
			}
		}
	}

	public bool CanTriggerTrap(Character character)
	{
		if (IsCovered)
		{
			return !IsJoiner;
		}
		return false;
	}

	public bool CanResetTrap()
	{
		if (UnderConstructionInfo == null && !IsJoiner)
		{
			return !IsCovered;
		}
		return false;
	}

	public EquipmentPrototype GetEquipmentNeededForReset()
	{
		if (FreeResets < MaxFreeResets)
		{
			return null;
		}
		return EquipmentPrototype.Wood;
	}

	public override float? RaycastThreadSafe(Ray ray, float length, AStarMoveableObstacle obstacle, TerrainCoord tile, byte modelIndex, bool includeWireFences)
	{
		return null;
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x80) != 0)
		{
			return true;
		}
		if ((options & 0x400) != 0)
		{
			return false;
		}
		if (requester != null && requester.IsRagdoll())
		{
			return false;
		}
		return base.IsImpassable(requester, options, tile);
	}

	public override bool IsTargetable()
	{
		if (IsJoiner)
		{
			return false;
		}
		if (Session.Instance.Editor || PropEditor.AllowTargetingAllProps)
		{
			return true;
		}
		return CommunityId == Session.Instance.CommunityManager.PlayerCommunity.Id;
	}

	public override bool WantDebrisOnDemolition()
	{
		return false;
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (PropEditor.ShowPitTrapJoiners)
		{
			Bounds boundingBox = GetBoundingBox();
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawBox(boundingBox.center, boundingBox.extents, IsJoiner ? Color.yellow : Color.red);
			DebugGraphics.EndDrawLines();
		}
	}
}

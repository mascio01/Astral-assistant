using System;
using UnityEngine;

public class BaseFence : SingleTileProp
{
	public static PrefabResource[] WoodFenceModels = new PrefabResource[15]
	{
		new PrefabResource("Prefabs\\Fences\\WoodFence_Post", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NS", 100),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE", 50),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NES", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NESW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_N_SE", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_N_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_N_E_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_N_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE_SE", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE_SW", 50),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE_SE_SW_NW", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_N_S_Slope", 10),
		new PrefabResource("Prefabs\\Fences\\WoodFence_NE_SW_Slope", 10)
	};

	public static PrefabResource[] WireFenceModels = new PrefabResource[15]
	{
		new PrefabResource("Prefabs\\Fences\\WireFence_Post", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NS", 100),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE", 50),
		new PrefabResource("Prefabs\\Fences\\WireFence_NES", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NESW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_N_SE", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_N_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_N_E_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_N_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE_SE", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE_SW", 50),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE_SE_SW_NW", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NS_Slope", 10),
		new PrefabResource("Prefabs\\Fences\\WireFence_NE_SW_Slope", 10)
	};

	public static PrefabResource[] PicketFenceModels = new PrefabResource[15]
	{
		new PrefabResource("Prefabs\\Fences\\PicketFence_Post", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NS", 100),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE", 50),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NES", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NESW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_N_SE", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_N_SW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_N_E_SW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_N_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE_SE", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE_SW", 50),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE_SE_SW_NW", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NS_Slope", 10),
		new PrefabResource("Prefabs\\Fences\\PicketFence_NE_SW_Slope", 10)
	};

	public static PrefabResource[] ConcreteWallModels = new PrefabResource[15]
	{
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_Post", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NS", 100),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE", 50),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NES", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NESW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_N_SE", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_N_SW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_N_E_SW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_N_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE_SE", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE_SW", 50),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE_SE_SW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE_SE_SW_NW", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NS_Slope", 10),
		new PrefabResource("Prefabs\\Fences\\ConcreteWall_NE_SW_Slope", 10)
	};

	public FenceModelType _modelType;

	private float _angle;

	private bool SurroundedByHigherFences;

	private bool ForceInvulnerable;

	private const float LevelStep = 0.5f;

	private const float MeshId = 0.99215686f;

	private static string FenceRaycastStr = "FenceRaycast";

	public byte ModelIndex
	{
		get
		{
			FenceModelType num = ((UnderConstructionInfo != null) ? FenceModelType.Count : _modelType);
			int num2 = ((_angle != 0f) ? ((_angle == MathF.PI / 2f) ? 1 : ((_angle == MathF.PI) ? 2 : 3)) : 0);
			int num3 = (SurroundedByHigherFences ? 1 : 0);
			return (byte)((uint)num | (uint)(num2 << 4) | (uint)(num3 << 6));
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BaseFence;
	}

	public override Flammability GetFlammability()
	{
		if (!ForceInvulnerable)
		{
			return base.GetFlammability();
		}
		return Flammability.Invulnerable;
	}

	public override ImpactSusceptibility GetImpactSusceptibility()
	{
		if (!ForceInvulnerable)
		{
			return base.GetImpactSusceptibility();
		}
		return ImpactSusceptibility.CompletelyInvulnerable;
	}

	public override bool IsForcedInvulnerable()
	{
		return ForceInvulnerable;
	}

	public override void SetForceInvulnerable(bool v)
	{
		ForceInvulnerable = v;
	}

	public void RecalcNeighbours()
	{
		GameTerrain instance = GameTerrain.Instance;
		instance.RecalcFence(Tile.x, Tile.y + 1);
		instance.RecalcFence(Tile.x + 1, Tile.y);
		instance.RecalcFence(Tile.x, Tile.y - 1);
		instance.RecalcFence(Tile.x - 1, Tile.y);
		instance.RecalcFence(Tile.x + 1, Tile.y + 1);
		instance.RecalcFence(Tile.x + 1, Tile.y - 1);
		instance.RecalcFence(Tile.x - 1, Tile.y - 1);
		instance.RecalcFence(Tile.x - 1, Tile.y + 1);
		if (GameTerrain.Instance.UnityTerrainObj != null)
		{
			instance.BuildMinimap(Tile, Tile);
		}
	}

	public override void OnSpawn()
	{
		UpdateModelAndAngle(inited: false);
		base.OnSpawn();
		RecalcNeighbours();
	}

	public override void Delete()
	{
		base.Delete();
		RecalcNeighbours();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref _modelType);
		reflector.Add(ref _angle);
		reflector.AddAfter(ref SurroundedByHigherFences, 422);
		reflector.AddAfter(ref ForceInvulnerable, 519);
		if (reflector.Version < 19)
		{
			reflector.Add(ref CommunityId);
		}
	}

	public static float CalcLevel(TerrainCoord tile)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (instance == null || instance.IsTileOutsideBounds(tile.x, tile.y))
		{
			return 0f;
		}
		return (float)Math.Floor(instance.GetTileMaxHeight(tile.x, tile.y, includeRiver: true) / 0.5f) * 0.5f;
	}

	public float CalcLevel()
	{
		return CalcLevel(Tile);
	}

	public void UpdateModelAndAngle(bool inited)
	{
		GameTerrain instance = GameTerrain.Instance;
		bool flag = instance.IsFence(Tile.x, Tile.y - 1, vert: true, horiz: false);
		bool flag2 = instance.IsFence(Tile.x - 1, Tile.y, vert: false, horiz: true);
		bool flag3 = instance.IsFence(Tile.x, Tile.y + 1, vert: true, horiz: false);
		bool flag4 = instance.IsFence(Tile.x + 1, Tile.y, vert: false, horiz: true);
		bool flag5 = instance.IsFence(Tile.x - 1, Tile.y - 1, vert: false, horiz: false);
		bool flag6 = instance.IsFence(Tile.x - 1, Tile.y + 1, vert: false, horiz: false);
		bool flag7 = instance.IsFence(Tile.x + 1, Tile.y + 1, vert: false, horiz: false);
		bool flag8 = instance.IsFence(Tile.x + 1, Tile.y - 1, vert: false, horiz: false);
		float num = CalcLevel();
		bool flag9 = flag && CalcLevel(new TerrainCoord(Tile.x, Tile.y - 1)) > num;
		bool flag10 = flag2 && CalcLevel(new TerrainCoord(Tile.x - 1, Tile.y)) > num;
		bool flag11 = flag3 && CalcLevel(new TerrainCoord(Tile.x, Tile.y + 1)) > num;
		bool flag12 = flag4 && CalcLevel(new TerrainCoord(Tile.x + 1, Tile.y)) > num;
		bool flag13 = flag5 && CalcLevel(new TerrainCoord(Tile.x - 1, Tile.y - 1)) > num;
		bool flag14 = flag6 && CalcLevel(new TerrainCoord(Tile.x - 1, Tile.y + 1)) > num;
		bool flag15 = flag7 && CalcLevel(new TerrainCoord(Tile.x + 1, Tile.y + 1)) > num;
		bool flag16 = flag8 && CalcLevel(new TerrainCoord(Tile.x + 1, Tile.y - 1)) > num;
		_angle = 0f;
		PrefabResource unityModel = GetUnityModel();
		if (flag && flag2 && flag3 && flag4)
		{
			_modelType = FenceModelType.Fence_N_E_S_W;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag10 && flag11 && flag12;
		}
		else if (flag && flag2 && flag3)
		{
			_modelType = FenceModelType.Fence_N_E_S;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag10 && flag11;
		}
		else if (flag && flag2 && flag4)
		{
			_modelType = FenceModelType.Fence_N_E_S;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag9 && flag10 && flag12;
		}
		else if (flag && flag4 && flag3)
		{
			_modelType = FenceModelType.Fence_N_E_S;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag9 && flag12 && flag11;
		}
		else if (flag4 && flag3 && flag2)
		{
			_modelType = FenceModelType.Fence_N_E_S;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag12 && flag11 && flag10;
		}
		else if (flag && flag2 && flag7)
		{
			_modelType = FenceModelType.Fence_N_E_SW;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag10 && flag15;
		}
		else if (flag && flag4 && flag6)
		{
			_modelType = FenceModelType.Fence_N_E_SW;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag9 && flag12 && flag14;
		}
		else if (flag4 && flag3 && flag5)
		{
			_modelType = FenceModelType.Fence_N_E_SW;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag12 && flag11 && flag13;
		}
		else if (flag3 && flag2 && flag8)
		{
			_modelType = FenceModelType.Fence_N_E_SW;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag11 && flag10 && flag16;
		}
		else if (flag && flag2)
		{
			_modelType = FenceModelType.Fence_N_E;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag10;
		}
		else if (flag && flag4)
		{
			_modelType = FenceModelType.Fence_N_E;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag9 && flag12;
		}
		else if (flag4 && flag3)
		{
			_modelType = FenceModelType.Fence_N_E;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag12 && flag11;
		}
		else if (flag3 && flag2)
		{
			_modelType = FenceModelType.Fence_N_E;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag11 && flag10;
		}
		else if (flag && flag3)
		{
			if (flag9 && flag11)
			{
				_modelType = FenceModelType.Fence_N_S;
				_angle = 0f;
				SurroundedByHigherFences = true;
			}
			else if (flag9)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
				_angle = 0f;
				SurroundedByHigherFences = false;
			}
			else if (flag11)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
				_angle = MathF.PI;
				SurroundedByHigherFences = false;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
				_angle = 0f;
				SurroundedByHigherFences = false;
			}
		}
		else if (flag4 && flag2)
		{
			if (flag12 && flag10)
			{
				_modelType = FenceModelType.Fence_N_S;
				_angle = -MathF.PI / 2f;
				SurroundedByHigherFences = true;
			}
			else if (flag12)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
				_angle = -MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
			else if (flag10)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
				_angle = MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
				_angle = -MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
		}
		else if (flag && flag6 && flag7)
		{
			_modelType = FenceModelType.Fence_N_SE_SW;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag14 && flag15;
		}
		else if (flag4 && flag5 && flag6)
		{
			_modelType = FenceModelType.Fence_N_SE_SW;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag12 && flag13 && flag14;
		}
		else if (flag3 && flag8 && flag5)
		{
			_modelType = FenceModelType.Fence_N_SE_SW;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag11 && flag16 && flag13;
		}
		else if (flag2 && flag7 && flag8)
		{
			_modelType = FenceModelType.Fence_N_SE_SW;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag10 && flag15 && flag16;
		}
		else if (flag && flag7)
		{
			_modelType = FenceModelType.Fence_N_SW;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag15;
		}
		else if (flag4 && flag6)
		{
			_modelType = FenceModelType.Fence_N_SW;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag12 && flag14;
		}
		else if (flag3 && flag5)
		{
			_modelType = FenceModelType.Fence_N_SW;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag11 && flag13;
		}
		else if (flag2 && flag8)
		{
			_modelType = FenceModelType.Fence_N_SW;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag10 && flag16;
		}
		else if (flag && flag6)
		{
			_modelType = FenceModelType.Fence_N_SE;
			_angle = 0f;
			SurroundedByHigherFences = flag9 && flag14;
		}
		else if (flag4 && flag5)
		{
			_modelType = FenceModelType.Fence_N_SE;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag12 && flag13;
		}
		else if (flag3 && flag8)
		{
			_modelType = FenceModelType.Fence_N_SE;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag11 && flag16;
		}
		else if (flag2 && flag7)
		{
			_modelType = FenceModelType.Fence_N_SE;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag10 && flag15;
		}
		else if (flag)
		{
			if (flag9)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
			}
			_angle = 0f;
			SurroundedByHigherFences = false;
		}
		else if (flag4)
		{
			if (flag12)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
			}
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = false;
		}
		else if (flag3)
		{
			if (flag11)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
			}
			_angle = MathF.PI;
			SurroundedByHigherFences = false;
		}
		else if (flag2)
		{
			if (flag10)
			{
				_modelType = FenceModelType.Fence_N_S_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_N_S;
			}
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = false;
		}
		else if (flag5 && flag6 && flag7 && flag8)
		{
			_modelType = FenceModelType.Fence_NE_SE_SW_NW;
			_angle = 0f;
			SurroundedByHigherFences = flag13 && flag14 && flag15 && flag16;
		}
		else if (flag5 && flag6 && flag7)
		{
			_modelType = FenceModelType.Fence_NE_SE_SW;
			_angle = 0f;
			SurroundedByHigherFences = flag13 && flag14 && flag15;
		}
		else if (flag8 && flag5 && flag6)
		{
			_modelType = FenceModelType.Fence_NE_SE_SW;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag16 && flag13 && flag14;
		}
		else if (flag7 && flag8 && flag5)
		{
			_modelType = FenceModelType.Fence_NE_SE_SW;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag15 && flag16 && flag13;
		}
		else if (flag6 && flag7 && flag8)
		{
			_modelType = FenceModelType.Fence_NE_SE_SW;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag14 && flag15 && flag16;
		}
		else if (flag5 && flag6)
		{
			_modelType = FenceModelType.Fence_NE_SE;
			_angle = 0f;
			SurroundedByHigherFences = flag13 && flag14;
		}
		else if (flag8 && flag5)
		{
			_modelType = FenceModelType.Fence_NE_SE;
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = flag16 && flag13;
		}
		else if (flag7 && flag8)
		{
			_modelType = FenceModelType.Fence_NE_SE;
			_angle = MathF.PI;
			SurroundedByHigherFences = flag15 && flag16;
		}
		else if (flag6 && flag7)
		{
			_modelType = FenceModelType.Fence_NE_SE;
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = flag14 && flag15;
		}
		else if (flag5 && flag7)
		{
			if (flag13 && flag15)
			{
				_modelType = FenceModelType.Fence_NE_SW;
				_angle = 0f;
				SurroundedByHigherFences = true;
			}
			else if (flag13)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
				_angle = 0f;
				SurroundedByHigherFences = false;
			}
			else if (flag15)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
				_angle = MathF.PI;
				SurroundedByHigherFences = false;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
				_angle = 0f;
				SurroundedByHigherFences = false;
			}
		}
		else if (flag6 && flag8)
		{
			if (flag14 && flag16)
			{
				_modelType = FenceModelType.Fence_NE_SW;
				_angle = MathF.PI / 2f;
				SurroundedByHigherFences = true;
			}
			else if (flag14)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
				_angle = MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
			else if (flag16)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
				_angle = -MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
				_angle = MathF.PI / 2f;
				SurroundedByHigherFences = false;
			}
		}
		else if (flag5)
		{
			if (flag13)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
			}
			_angle = 0f;
			SurroundedByHigherFences = false;
		}
		else if (flag8)
		{
			if (flag16)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
			}
			_angle = -MathF.PI / 2f;
			SurroundedByHigherFences = false;
		}
		else if (flag7)
		{
			if (flag15)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
			}
			_angle = MathF.PI;
			SurroundedByHigherFences = false;
		}
		else if (flag6)
		{
			if (flag14)
			{
				_modelType = FenceModelType.Fence_NE_SW_Slope;
			}
			else
			{
				_modelType = FenceModelType.Fence_NE_SW;
			}
			_angle = MathF.PI / 2f;
			SurroundedByHigherFences = false;
		}
		else
		{
			_modelType = FenceModelType.FencePost;
			_angle = 0f;
			SurroundedByHigherFences = false;
		}
		MarkCachedWorldTransDirty();
		if (UnderConstructionInfo == null && UnityObj != null && unityModel != GetUnityModel())
		{
			unityModel.DeletePrefab(UnityObj);
			UnityObj = null;
			UnityInit();
			UnityActivate();
		}
		UpdateUnityTransform();
		if (inited && !Session.Instance.Editor && IsImpassableSingleTileProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return CalcModelWorldTransform(GetUnityModel(), Tile, _angle, SurroundedByHigherFences, ModelOffset);
	}

	public static Matrix4x4 CalcModelWorldTransform(PrefabResource model, TerrainCoord tile, float angle, bool surroundedByHigherFences, Vector3 modelOffset)
	{
		GameTerrain instance = GameTerrain.Instance;
		Matrix4x4 mat = model?.LocalToWorldMatrix ?? Matrix4x4.identity;
		MathUtil.SetTranslation(ref mat, modelOffset);
		float num = CalcLevel(tile);
		if (surroundedByHigherFences)
		{
			num += 0.5f;
		}
		float x = ((instance != null) ? ((float)tile.x - instance.HalfSize + 0.5f) : 0f);
		float z = ((instance != null) ? ((float)tile.y - instance.HalfSize + 0.5f) : 0f);
		return MathUtil.CreateTranslation(new Vector3(x, num, z)) * MathUtil.CreateRotationY(angle) * mat;
	}

	public override void OnTerrainHeightChanged()
	{
		UpdateModelAndAngle(inited: true);
	}

	public override void SetTile(TerrainCoord tile)
	{
		UnregisterWithTerrain();
		RecalcNeighbours();
		GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		Tile = tile;
		GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		RegisterWithTerrain();
		RecalcNeighbours();
		UpdateModelAndAngle(inited: true);
	}

	public override void SetTileGhost(TerrainCoord tile)
	{
		base.SetTileGhost(tile);
		UpdateModelAndAngle(inited: false);
	}

	public override bool CanEncloseAnArea()
	{
		if (UnderConstructionInfo != null)
		{
			return UnderConstructionInfo.IsBuiltEnoughToBeAnObstacle();
		}
		return true;
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x10) != 0 && IsFlammable() && !requester.IsInMyCommunityOrAlly(this))
		{
			return false;
		}
		if ((options & 0x20) != 0 && !IsExplosionProof() && !requester.IsInMyCommunityOrAlly(this))
		{
			return false;
		}
		return base.IsImpassable(requester, options, tile);
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		using (new UnityProfileMarker(FenceRaycastStr))
		{
			if ((flags & 0x40) != 0 && UnderConstructionInfo != null && GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
			{
				return distance;
			}
			if ((flags & 0x10) != 0 && GetBoundingBox().IntersectRay(ray, out var distance2) && distance2 < length)
			{
				return distance2;
			}
			if ((flags & 0x20) != 0 && GetBoundingBox().IntersectRay(ray, out var distance3) && distance3 < length)
			{
				return Prop.RaycastAgainstModel(GetUnityModel(), GetWorldTrans(), ray, length, 0, out normal, 0f, (flags & 0x1000) != 0);
			}
		}
		return null;
	}

	public override float? RaycastThreadSafe(Ray ray, float length, AStarMoveableObstacle obstacle, TerrainCoord tile, byte modelIndex, bool includeWireFences)
	{
		FenceModelType fenceModelType = (FenceModelType)(modelIndex & 0xF);
		if (fenceModelType != FenceModelType.Count)
		{
			Vector3 aStarTileCentrePos = GameTerrain.Instance.GetAStarTileCentrePos(tile);
			if ((int)fenceModelType >= GetFenceModels().Length)
			{
				return null;
			}
			PrefabResource prefabResource = GetFenceModels()[(int)fenceModelType];
			float y = prefabResource.Height * 0.5f;
			if (MathUtil.CreateBoundsCentreExtents(aStarTileCentrePos + new Vector3(0f, y, 0f), new Vector3(0.5f, y, 0.5f)).IntersectRay(ray, out var distance) && distance < length)
			{
				float angle = 0f;
				switch ((modelIndex >> 4) & 3)
				{
				case 0:
					angle = 0f;
					break;
				case 1:
					angle = MathF.PI / 2f;
					break;
				case 2:
					angle = MathF.PI;
					break;
				case 3:
					angle = -MathF.PI / 2f;
					break;
				}
				bool surroundedByHigherFences = modelIndex >> 6 != 0;
				Matrix4x4 rootTransform = CalcModelWorldTransform(prefabResource, tile, angle, surroundedByHigherFences, ModelOffset);
				Vector3 hitNormal = Vector3.zero;
				return Prop.RaycastAgainstModel(prefabResource, rootTransform, ray, length, 0, out hitNormal, 0f, includeWireFences);
			}
		}
		return null;
	}

	public virtual PrefabResource[] GetFenceModels()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.GetPrefabsAsArray();
	}

	public override PrefabResource GetUnityModel()
	{
		if ((int)_modelType >= GetFenceModels().Length)
		{
			return null;
		}
		return GetFenceModels()[(int)_modelType];
	}

	public override bool CanBeClearedForBuilding(Community builderCommunity, TileObject newBuilding)
	{
		if (builderCommunity != null && builderCommunity.CommunityType == CommunityType.Player && Prototype.CanBeClearedForBuilding)
		{
			Community community = GetCommunity();
			if (!(newBuilding is BaseFence) && !(newBuilding is Gate))
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
}

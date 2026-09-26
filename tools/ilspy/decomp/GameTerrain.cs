using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class GameTerrain : BaseObject
{
	public delegate bool FilterFunc(TileObject obj);

	private struct Span
	{
		public short x0;

		public short x1;

		public short y;

		public static Span Invalid = new Span(-1, -1, -1);

		public Span(short _x0, short _x1, short _y)
		{
			x0 = _x0;
			x1 = _x1;
			y = _y;
		}
	}

	private struct BiomeAmount
	{
		public BiomeType BiomeType;

		public float Amount;

		public BiomeAmount(BiomeType type, float amount)
		{
			BiomeType = type;
			Amount = amount;
		}
	}

	private struct InfluenceOfPathOnVertex
	{
		public TerrainPath Path;

		public float DistFactor;

		public float DistFromEdgeOfPath;

		public float DesiredHeight;

		public float CamberOrRiverDepth;
	}

	private struct ConnectorCandidate
	{
		public Vector2 Dir;

		public int Index;

		public Prop.OrientationType Orientation;

		public float Score;
	}

	private struct MineralScore
	{
		public MineralType Type;

		public float Score;
	}

	public class PlayerStartCandidate : IComparable<PlayerStartCandidate>
	{
		public ControlPoint ControlPoint;

		public float Score;

		public int RoadIndex;

		public float ControlPointIndex;

		public int DirAlongRoad;

		public int CompareTo(PlayerStartCandidate other)
		{
			if (Score > other.Score)
			{
				return -1;
			}
			if (Score < other.Score)
			{
				return 1;
			}
			return 0;
		}
	}

	public class SortTownsByDistFromPlayerAscending : IComparer<Town>
	{
		public TerrainCoord Tile;

		int IComparer<Town>.Compare(Town a, Town b)
		{
			float dist = a.Tile.GetDist(Tile);
			float dist2 = b.Tile.GetDist(Tile);
			if (dist < dist2)
			{
				return -1;
			}
			if (dist > dist2)
			{
				return 1;
			}
			return 0;
		}
	}

	public struct GenerationStep
	{
		public string Name;

		public Stopwatch Stopwatch;

		public GenerationStep(string name)
		{
			Name = name;
			Stopwatch = new Stopwatch();
		}
	}

	public class SortCommunitiesByLeaderDistAscending : IComparer<Community>
	{
		private Vector2 FromPosXZ;

		public SortCommunitiesByLeaderDistAscending(Vector2 fromPosXZ)
		{
			FromPosXZ = fromPosXZ;
		}

		int IComparer<Community>.Compare(Community a, Community b)
		{
			float magnitude = (a.Leader.PosXZ - FromPosXZ).magnitude;
			float magnitude2 = (b.Leader.PosXZ - FromPosXZ).magnitude;
			if (magnitude < magnitude2)
			{
				return -1;
			}
			if (magnitude > magnitude2)
			{
				return 1;
			}
			return 0;
		}
	}

	public static int MaxSize = 1024;

	public static GameTerrain Instance;

	public static string[] TerrainTexNames = StringUtil.GetEnumNames<TerrainTex>();

	public static string[] TreeTypeNames = StringUtil.GetEnumNames<TreeType>();

	public static string[] GrassTypeNames = StringUtil.GetEnumNames<GrassType>();

	public const int LookupSquareBlockLevel = 3;

	public const int LookupSquareSize = 8;

	public const int MineralSquareSize = 4;

	public const float SlopeHeight = 0.75f;

	public const float CliffHeight = 1.25f;

	public const float HeightRange = 64f;

	public int Size;

	public float HalfSize;

	public static byte[,] Tiles;

	public static byte[,] Tiles2;

	public static byte[,] TilesThreadCopy;

	public static float[,] Vertices;

	public static float[,] OriginalVertices;

	public static byte[,,] TextureWeights;

	public static byte[,,] MineralWeights;

	public static TerrainLookupSquare[,] LookupSquares;

	public bool UseFixedTerrain;

	public bool ComplexPathfinding;

	public GameObject UnityTerrainObj;

	public Terrain UnityTerrain;

	public AudioSource UnityAudioSource;

	public Material UnityWaterMaterial;

	public List<TreeProp> TreeProps = new List<TreeProp>();

	public List<ModifiedPatch> ModifiedPatches = new List<ModifiedPatch>();

	public int NextModifiedPatchId = 1;

	public TileContentsManager TileContentsManager = new TileContentsManager();

	public MapWho<Character> CharacterMapWho;

	public MapWho<TileObject> BurningMapWho;

	public MapWho<Prop> FoodMapWho;

	public MapWho<TreeProp> TreeMapWho;

	public MapWho<SingleTileObject> RockMapWho;

	public MapWho<SingleTileProp> TrapSignMapWho;

	public MapWho<RadioProp> RadioMapWho;

	public MapWho<ArrowProp> ArrowMapWho;

	public AStar AStar;

	public FogOfWar FogOfWar;

	public GrassMap GrassMap;

	public BirdSongManager BirdSongManager;

	private static ColMap MinimapCol = new ColMap();

	public Texture2D MinimapTex;

	private bool MinimapTexDirty;

	public Texture2D[] GeologicalTex = new Texture2D[4];

	private bool[] GeologicalTexDirty = new bool[4];

	private static ColMap HeightMapCol = new ColMap();

	public Texture2D HeightMapTex;

	private bool HeightMapTexDirty;

	public TimeSpan LastChangedTime = Target.Never;

	public int LastCalcEnclosedAreasFrame = -1;

	public int LastEnclosedAreasChangedFrame;

	public bool WantCalcEnclosedAreasOnThread;

	private List<TerrainCoord> EnclosedAreaSeedPoints = new List<TerrainCoord>();

	public List<TerrainCoord> RecentSnowScoops = new List<TerrainCoord>();

	public List<int> DisabledEntrancePathIndices = new List<int>();

	public List<int> DisabledHitTheRoadPathIndices = new List<int>();

	public const int OwnershipLookupRange = 2;

	public static Resource<Material> CustomTerrainMaterial;

	public static Resource<Material> CustomWaterMaterial;

	public static Resource<Material> RoadMaterial;

	public static Resource<Texture2D> SnowTex;

	public static Resource<Texture2D> SnowNormals;

	public static Resource<Texture2D> SnowNoiseCover;

	public static Resource<Texture2D> MossTex;

	public static Resource<Texture2D> MossNormals;

	public static Resource<Texture2D> MossRoughness;

	public static Resource<Texture2D> MossAO;

	public static Resource<Texture2D>[] Textures = new Resource<Texture2D>[8];

	public static Resource<Texture2D>[] NormalMaps = new Resource<Texture2D>[8];

	public static PrefabResource[] Trees = new PrefabResource[18];

	public static MinimapSettings MinimapSettings;

	public static string[] TreeNames = new string[18]
	{
		"Conifer1", "Conifer2", "Conifer3", "Conifer4", "Conifer5", "Conifer6", "Conifer7", "Conifer8", "PineTree01", "PineTree02",
		"PineTree03", "Tree01", "Tree02", "Tree03", "Tree04", "Tree05", "Tree06", "Tree08"
	};

	private static List<float[,]> UnityHeightMapBuiltOnThread;

	private static List<float[,,]> UnityAlphaMapBuiltOnThread;

	public const float DetailDist = 80f;

	public const float DefaultTexTileSize = 4f;

	public static float RiverHeightOffset = -0.25f;

	public static float IceCoverSoftness = 0.5f;

	private static string GetObjectsInRectStr = "GetObjectsInRect";

	private static List<TileObject> Temp = new List<TileObject>();

	private static string GetUnityObjectsInRectStr = "GetUnityObjectsInRect";

	public static int FlattenPatchExtra = 2;

	private TerrainRayCastTracer RaycastTracer;

	private TerrainRayCastTracer RaycastTracerForAStarThread;

	private TerrainRayCastTracer RaycastTracerForVisibleTilesThread;

	private TerrainRayCastTracer RaycastTracerForHandleInputThread;

	private TerrainMaxHeightTracer MaxHeightTracer;

	private TerrainIsPassableTracer IsPassableTracer;

	private TerrainCollisionTracer CollisionTracer;

	private FindImpassableObjectTracer FindImpassableObjectTracer;

	private static string RayCastStr = "RayCast";

	private static string RayCastFromAStarThreadStr = "RayCastFromAStarThread";

	private static string RayCastFromVisibleTilesThreadStr = "RayCastFromVisibleTilesThread";

	private const string CopyTilesForThreadStr = "CopyTilesForThread";

	private const string CopyTilesFromThreadStr = "CopyTilesFromThread";

	private static List<Span> Stack = new List<Span>();

	private static string UpdateMinimapTexStr = "UpdateMinimapTex";

	private static string UpdateGeoTexStr = "UpdateGeoTex";

	private static string UpdateHeightMapTexStr = "UpdateHeightMapTex";

	private static List<TerrainPath> PathsToUpdate = new List<TerrainPath>();

	private static float ButteTopFalloff = 0.75f;

	public const float SmoothingFactor = 3f;

	public const float CamberHeight = 0.2f;

	public const float MaxRiverDepth = 1f;

	public static float Hilliness = 1f;

	private const float MaxRoadGradient = 0.25f;

	public const float MinRoadOrRiverWidth = 5f;

	public const float MaxRoadOrRiverWidth = 10f;

	public const int Border = 32;

	public static int WoodLimitPerCampfire = 5;

	public List<TerrainPath> Paths = new List<TerrainPath>();

	public List<TerrainPathJunction> Junctions = new List<TerrainPathJunction>();

	private static TerrainCoord[] Directions = new TerrainCoord[4]
	{
		new TerrainCoord(-1, 0),
		new TerrainCoord(1, 0),
		new TerrainCoord(0, -1),
		new TerrainCoord(0, 1)
	};

	public static float MineralThreshold = 0.5f;

	private static List<MineralType> TempMineralTypes = new List<MineralType>();

	public bool Generating;

	public static int CurGenerationStep = 0;

	public static GenerationStep[] GenerationSteps = new GenerationStep[19]
	{
		new GenerationStep("Height Gen"),
		new GenerationStep("Mineral Gen"),
		new GenerationStep("Roads And Rivers"),
		new GenerationStep("Buildings"),
		new GenerationStep("Apply Height Map"),
		new GenerationStep("Map Edge Props"),
		new GenerationStep("Communities"),
		new GenerationStep("Countryside"),
		new GenerationStep("Boulders"),
		new GenerationStep("Rock Biomes"),
		new GenerationStep("Forest Biomes"),
		new GenerationStep("Ground Texture"),
		new GenerationStep("Grass"),
		new GenerationStep("Vehicles"),
		new GenerationStep("Town Props"),
		new GenerationStep("Player Spawn Points"),
		new GenerationStep("Spawn Points"),
		new GenerationStep("Deer Road Signs"),
		new GenerationStep("Loot")
	};

	private static bool[,] grid;

	private const float ImpassableEdgeMountainHeight = 0.125f;

	private static string GetNearestPointOnPathStr = "GetNearestPointOnPath";

	public static float MaxLootFilledAmount = 0.75f;

	private const int SpawnPointGridTileSize = 16;

	public int TilesSize => Size;

	public int VerticesSize => Size + 1;

	private static Stopwatch CurStopwatch => GenerationSteps[CurGenerationStep].Stopwatch;

	public static int GetSizeFromMapSize(MapSize mapSize)
	{
		int result = 1024;
		switch (mapSize)
		{
		case MapSize.Tiny:
			result = 128;
			break;
		case MapSize.Small:
			result = 256;
			break;
		case MapSize.Medium:
			result = 512;
			break;
		case MapSize.Large:
			result = 1024;
			break;
		case MapSize.Huge:
			result = 2048;
			break;
		}
		return result;
	}

	public static MapSize GetMapSizeFromSize(int size)
	{
		for (int i = 0; i < 5; i++)
		{
			if (size == GetSizeFromMapSize((MapSize)i))
			{
				return (MapSize)i;
			}
		}
		return MapSize.Large;
	}

	public static bool IsValidSize(int size)
	{
		for (int i = 0; i < 5; i++)
		{
			if (size == GetSizeFromMapSize((MapSize)i))
			{
				return true;
			}
		}
		return false;
	}

	public GameTerrain()
	{
		Instance = this;
		RaycastTracer = new TerrainRayCastTracer(this);
		RaycastTracerForHandleInputThread = new TerrainRayCastTracer(this);
		RaycastTracerForAStarThread = new TerrainRayCastTracer(this);
		RaycastTracerForVisibleTilesThread = new TerrainRayCastTracer(this);
		MaxHeightTracer = new TerrainMaxHeightTracer(this);
		IsPassableTracer = new TerrainIsPassableTracer(this);
		CollisionTracer = new TerrainCollisionTracer(this);
		FindImpassableObjectTracer = new FindImpassableObjectTracer(this);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GameTerrain;
	}

	public static GameTerrain Spawn(int size)
	{
		GameTerrain gameTerrain = new GameTerrain();
		gameTerrain.InitSize(size);
		for (int i = 0; i <= size; i++)
		{
			for (int j = 0; j <= size; j++)
			{
				Vertices[i, j] = 16f;
			}
		}
		Array.Copy(Vertices, OriginalVertices, OriginalVertices.Length);
		gameTerrain.OnSpawn();
		return gameTerrain;
	}

	public void InitSize(int size)
	{
		if (MaxSize < size && IsValidSize(size))
		{
			MaxSize = size;
			InitTerrainArrays();
		}
		Size = size;
		HalfSize = size / 2;
		AStar = new AStar(this);
		CharacterMapWho = new MapWho<Character>(16);
		BurningMapWho = new MapWho<TileObject>(32);
		TreeMapWho = new MapWho<TreeProp>(16);
		RockMapWho = new MapWho<SingleTileObject>(16);
		FoodMapWho = new MapWho<Prop>(32);
		TrapSignMapWho = new MapWho<SingleTileProp>(32);
		RadioMapWho = new MapWho<RadioProp>(32);
		ArrowMapWho = new MapWho<ArrowProp>(32);
		FogOfWar = new FogOfWar(this);
		GrassMap = new GrassMap(this);
		BirdSongManager = new BirdSongManager();
		AStar.InitSize(size);
		CharacterMapWho.InitSize(size);
		BurningMapWho.InitSize(size);
		TreeMapWho.InitSize(size);
		RockMapWho.InitSize(size);
		FoodMapWho.InitSize(size);
		TrapSignMapWho.InitSize(size);
		RadioMapWho.InitSize(size);
		ArrowMapWho.InitSize(size);
		FogOfWar.InitSize(size);
		GrassMap.InitSize(size);
		BirdSongManager.InitSize(size);
		HeightMapCol.InitSize(size);
		MinimapCol.InitSize(size);
		OnHeightChanged(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
	}

	public void Unload()
	{
		AStar.Unload();
		CharacterMapWho.Unload();
		BurningMapWho.Unload();
		TreeMapWho.Unload();
		RockMapWho.Unload();
		FoodMapWho.Unload();
		TrapSignMapWho.Unload();
		RadioMapWho.Unload();
		ArrowMapWho.Unload();
		FogOfWar.Unload();
		GrassMap.Unload();
		BirdSongManager.Unload();
		Array.Clear(Tiles, 0, Tiles.Length);
		Array.Clear(Tiles2, 0, Tiles2.Length);
		Array.Clear(TilesThreadCopy, 0, TilesThreadCopy.Length);
		Array.Clear(LookupSquares, 0, LookupSquares.Length);
		Array.Clear(Vertices, 0, Vertices.Length);
		Array.Clear(TextureWeights, 0, TextureWeights.Length);
		Array.Clear(MineralWeights, 0, MineralWeights.Length);
		Array.Clear(HeightMapCol.GetColMap(), 0, HeightMapCol.GetColMap().Length);
		Array.Clear(MinimapCol.GetColMap(), 0, MinimapCol.GetColMap().Length);
		TileContentsManager.ClearAllContents();
		TileContentsManager = null;
		Instance = null;
	}

	public override void Init()
	{
		base.Init();
		AStar.Init();
		FogOfWar.Init();
		GrassMap.Init();
	}

	public override void Delete()
	{
		base.Delete();
	}

	public void SetFlag(int x, int y, TileFlags flag, bool on)
	{
		if (on)
		{
			Tiles[x, y] |= (byte)flag;
		}
		else
		{
			Tiles[x, y] &= (byte)(~(int)flag);
		}
	}

	public void SetFlag2(int x, int y, TileFlags2 flag, bool on)
	{
		if (on)
		{
			Tiles2[x, y] |= (byte)flag;
		}
		else
		{
			Tiles2[x, y] &= (byte)(~(int)flag);
		}
	}

	public void ReflectFixedTerrain(Reflector reflector)
	{
		int value = Size;
		reflector.Add(ref value);
		if (Size == MaxSize)
		{
			reflector.Add(Vertices, 4 * VerticesSize * VerticesSize, VerticesSize);
		}
		else
		{
			for (int i = 0; i < VerticesSize; i++)
			{
				for (int j = 0; j < VerticesSize; j++)
				{
					reflector.Add(ref Vertices[i, j]);
				}
			}
		}
		if (reflector.IsDeserialising)
		{
			Array.Copy(Vertices, OriginalVertices, OriginalVertices.Length);
		}
		if (Size == MaxSize)
		{
			reflector.Add(TextureWeights, 8 * Size * Size, Size);
		}
		else
		{
			for (int k = 0; k < Size; k++)
			{
				for (int l = 0; l < Size; l++)
				{
					for (int m = 0; m < 8; m++)
					{
						reflector.Add(ref TextureWeights[k, l, m]);
					}
				}
			}
		}
		if (reflector.Version >= 177)
		{
			int num = Size / 4;
			if (Size == MaxSize)
			{
				reflector.Add(MineralWeights, 4 * MathUtil.Squared(num), num);
			}
			else
			{
				for (int n = 0; n < num; n++)
				{
					for (int num2 = 0; num2 < num; num2++)
					{
						for (int num3 = 0; num3 < 4; num3++)
						{
							reflector.Add(ref MineralWeights[n, num2, num3]);
						}
					}
				}
			}
			if (reflector.IsDeserialising)
			{
				for (int num4 = 0; num4 < 4; num4++)
				{
					GeologicalTexDirty[num4] = true;
				}
			}
		}
		else
		{
			List<MineralDeposit> mineralDeposits = new List<MineralDeposit>();
			GenerateGeologicalMap(new CustomRandom(42), mineralDeposits);
		}
		GrassMap.Reflect(reflector, value);
		for (int num5 = 0; num5 < value; num5++)
		{
			for (int num6 = 0; num6 < value; num6++)
			{
				reflector.Add(ref Tiles[num5, num6]);
			}
		}
		if (reflector.Version >= 159)
		{
			for (int num7 = 0; num7 < value; num7++)
			{
				for (int num8 = 0; num8 < value; num8++)
				{
					reflector.Add(ref Tiles2[num7, num8]);
				}
			}
		}
		if (reflector.IsSerialising && Session.Instance.Editor)
		{
			using (new StopWatchMarker("BuildCommunityAreaOwner"))
			{
				BuildCommunityAreaOwner(new TerrainRect(new TerrainCoord(0, 0), new TerrainCoord(Size, Size)));
			}
		}
		for (int num9 = 0; num9 < value / 8; num9++)
		{
			for (int num10 = 0; num10 < value / 8; num10++)
			{
				reflector.Add(ref LookupSquares[num9, num10].OwnerCommunityId);
			}
		}
		reflector.Add(ref Paths);
		reflector.Add(ref Junctions);
		reflector.AddAfter(ref ComplexPathfinding, 502);
		if (reflector.Version < 226)
		{
			foreach (TerrainPath path in Paths)
			{
				path.BuildPoints(this);
			}
			RecalcRiverTiles(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
		}
		if (reflector.Version < 602)
		{
			SetupRoadSkillCapBonus();
		}
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Size);
		if (reflector.IsDeserialising)
		{
			InitSize(Size);
		}
		reflector.Add(ref UseFixedTerrain);
		if (UseFixedTerrain || reflector.Version >= 132)
		{
			if (reflector.IsDeserialising)
			{
				GameImpl instance = GameImpl.Instance;
				string text = null;
				bool flag = false;
				bool inSaveData = false;
				if (UseFixedTerrain)
				{
					text = instance.CurrentStory.Path + "/FixedTerrain.mapx";
					if (!File.Exists(text))
					{
						text = instance.CurrentStory.Path + "/FixedTerrain.map";
					}
				}
				else if (Session.Instance.TerrainHash.IsValid())
				{
					text = OnlineParty.GetTerrainCacheFileName(Session.Instance.TerrainHash);
					flag = OnlineParty.IsTerrainCacheCompressed();
					inSaveData = true;
					OnlineParty.Instance.SetTerrainCacheLastUsedTime(Session.Instance.TerrainHash, DateTime.Now);
					if (reflector.AltTerrainFolder != null && !SaveGameManager.FileExists(text))
					{
						text = reflector.AltTerrainFolder + "/" + Session.Instance.TerrainHash.ToString() + ".map";
						inSaveData = false;
					}
				}
				if (text != null && SaveGameManager.FileExists(text, inSaveData))
				{
					Stream stream;
					if (flag)
					{
						byte[] array = SaveGameManager.FileReadAllBytes(text, inSaveData);
						stream = new MemoryStream(Compression.Decompress(array, 0, array.Length));
					}
					else
					{
						stream = SaveGameManager.FileOpenRead(text, inSaveData);
					}
					using (stream)
					{
						using (CustomBinaryReader customBinaryReader = new CustomBinaryReader(stream))
						{
							instance.SetCurrentTerrainReader(customBinaryReader);
							customBinaryReader.Read(out customBinaryReader.Version);
							if (customBinaryReader.Version > 629)
							{
								throw new Exception(GameImpl.Translate("MENU_SaveGameFromFuture").Replace("%1", customBinaryReader.Version.ToString()).Replace("%2", 629.ToString()));
							}
							ReflectFixedTerrain(customBinaryReader);
							instance.SetCurrentTerrainReader(null);
						}
						stream.Close();
					}
				}
			}
			reflector.Add(ref ModifiedPatches);
			reflector.AddAfter(ref NextModifiedPatchId, 425);
			if (reflector.IsDeserialising)
			{
				TerrainRect bounds = new TerrainRect(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
				for (int i = 0; i < ModifiedPatches.Count; i++)
				{
					ApplyModifiedPatch(ModifiedPatches[i], bounds, applyTextureWeightsAndGrass: true);
				}
			}
		}
		else
		{
			ReflectFixedTerrain(reflector);
			if (reflector.IsDeserialising)
			{
				SaveToTerrainCache(out Session.Instance.TerrainHash);
			}
		}
		if (Session.Instance.Frame > 0 && !reflector.IsTextDumping && !reflector.IsFastPath)
		{
			FogOfWar.Reflect(reflector, Size);
		}
		BirdSongManager.Reflect(reflector, Size);
		reflector.AddAfter(ref RecentSnowScoops, 242);
		if (reflector.Version >= 486)
		{
			reflector.AddIntList(ref DisabledEntrancePathIndices);
		}
		if (reflector.Version >= 487)
		{
			reflector.AddIntList(ref DisabledHitTheRoadPathIndices);
		}
		reflector.Add(ref LastChangedTime);
	}

	public bool SaveToTerrainCache(out MD5Hash hash)
	{
		using CustomBinaryWriter customBinaryWriter = new CustomBinaryWriterToMemory(Size * Size * 16);
		customBinaryWriter.Write(629);
		ReflectFixedTerrain(customBinaryWriter);
		hash = new MD5Hash(customBinaryWriter._buffer, customBinaryWriter._index);
		return OnlineParty.Instance.SaveToTerrainCache(hash, customBinaryWriter._buffer, customBinaryWriter._index, isCompressed: false);
	}

	public static bool SaveFixedToTerrainCache(out MD5Hash hash)
	{
		GameImpl instance = GameImpl.Instance;
		hash = default(MD5Hash);
		try
		{
			string path = instance.CurrentStory.Path + "/FixedTerrain.mapx";
			if (!File.Exists(path))
			{
				path = instance.CurrentStory.Path + "/FixedTerrain.map";
			}
			if (!File.Exists(path))
			{
				return false;
			}
			using Stream stream = File.OpenRead(path);
			byte[] array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);
			hash = new MD5Hash(array, 0);
			return OnlineParty.Instance.SaveToTerrainCache(hash, array, array.Length, isCompressed: false);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning("Error saving fixed terrain to cache: " + ex.ToString());
			return false;
		}
	}

	public void AddRecentSnowScoop(TerrainCoord tile)
	{
		if (!RecentSnowScoops.Contains(tile))
		{
			if (RecentSnowScoops.Count >= 32)
			{
				RecentSnowScoops.RemoveAt(0);
			}
			RecentSnowScoops.Add(tile);
		}
	}

	public void BuildCommunityAreaOwner(TerrainRect tileRect)
	{
		BuildCommunityAreaOwnerLookupRect(new TerrainRect(new TerrainCoord(tileRect.min.x / 8, tileRect.min.y / 8), new TerrainCoord(tileRect.max.x / 8, tileRect.max.y / 8)));
	}

	public void BuildCommunityAreaOwnerLookupRect(TerrainRect lookupRect)
	{
		lookupRect.min.x = Math.Max(0, lookupRect.min.x - 2);
		lookupRect.min.y = Math.Max(0, lookupRect.min.y - 2);
		lookupRect.max.x = Math.Min(Size / 8 - 1, lookupRect.max.x + 2);
		lookupRect.max.y = Math.Min(Size / 8 - 1, lookupRect.max.y + 2);
		for (int i = lookupRect.min.x; i <= lookupRect.max.x; i++)
		{
			for (int j = lookupRect.min.y; j <= lookupRect.max.y; j++)
			{
				if ((LookupSquares[i, j].OwnerCommunityId & 0x80000000u) != 0L)
				{
					int id = LookupSquares[i, j].OwnerCommunityId & 0x7FFFFFFF;
					if (BaseObjectManager.Instance.FindBaseObjectByID(id) is Community community && community.HasAnyActiveMembers())
					{
						continue;
					}
				}
				TerrainRect terrainRect = new TerrainRect(new TerrainCoord(i * 8, j * 8), new TerrainCoord((i + 1) * 8 - 1, (j + 1) * 8 - 1));
				Community community2 = null;
				float num = float.MaxValue;
				for (int k = Math.Max(0, i - 2); k <= Math.Min(Size / 8 - 1, i + 2); k++)
				{
					for (int l = Math.Max(0, j - 2); l <= Math.Min(Size / 8 - 1, j + 2); l++)
					{
						List<TileObject> objects = LookupSquares[k, l].Objects;
						if (objects == null)
						{
							continue;
						}
						foreach (TileObject item in objects)
						{
							if (item is Character || item is Campfire || item is Grave || item.GetGrabbableEquipmentType() != null)
							{
								continue;
							}
							PropPrototype propPrototype = item.GetPropPrototype();
							if (propPrototype == null || !propPrototype.SetAreaOwnership)
							{
								continue;
							}
							Community community3 = item.GetCommunity();
							if (community3 != null && !community3.IsAmbientCommunity() && community3.HasAnyActiveMembers())
							{
								float num2 = Mathf.Sqrt(terrainRect.GetClosestDistSqToRect(item.GetTileRect()));
								if (community3.CommunityType == CommunityType.Player)
								{
									num2 += 1000f;
								}
								if (num2 < num)
								{
									num = num2;
									community2 = community3;
								}
							}
						}
					}
				}
				int num3 = community2?.Id ?? 0;
				LookupSquares[i, j].OwnerCommunityId = num3;
				Instance.AStar.AddChange(AStarChange.SetLookupSquareOwnerCommunityId(new TerrainCoord(i, j), num3));
			}
		}
	}

	public static void LoadContent()
	{
		CustomTerrainMaterial = new Resource<Material>("Materials/CustomTerrain");
		CustomWaterMaterial = new Resource<Material>("Materials/CustomWater");
		RoadMaterial = new Resource<Material>("Materials/Road");
		SnowTex = new Resource<Texture2D>("Textures/Terrain/Snow Cover_BC");
		SnowNormals = new Resource<Texture2D>("Textures/Terrain/Snow Cover_N_v1");
		SnowNoiseCover = new Resource<Texture2D>("Textures/Terrain/Snow Cover Noise");
		MossTex = new Resource<Texture2D>("Textures/Moss/TexturesCom_Nature_Moss2_1K_albedo");
		MossNormals = new Resource<Texture2D>("Textures/Moss/TexturesCom_Nature_Moss2_2K_normal");
		MossRoughness = new Resource<Texture2D>("Textures/Moss/TexturesCom_Nature_Moss2_1K_roughness");
		MossAO = new Resource<Texture2D>("Textures/Moss/TexturesCom_Nature_Moss2_1K_ao");
		Textures[0] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Grass_1_BC_R");
		NormalMaps[0] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Grass_1_N");
		Textures[1] = new Resource<Texture2D>("Textures/Terrain/Grass_07_Diffuse");
		NormalMaps[1] = new Resource<Texture2D>("Textures/Terrain/Grass_07_Normal");
		Textures[2] = new Resource<Texture2D>("Textures/Terrain/ground12_Diffuse");
		NormalMaps[2] = new Resource<Texture2D>("Textures/Terrain/ground12_Normal");
		Textures[3] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Mud_3_BC_R");
		NormalMaps[3] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Mud_3_N");
		Textures[4] = new Resource<Texture2D>("Textures/Terrain/Cracked_Road_D");
		NormalMaps[4] = new Resource<Texture2D>("Textures/Terrain/Cracked_Road_H");
		Textures[5] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Rock_6_BC_R");
		NormalMaps[5] = new Resource<Texture2D>("Textures/Terrain/T_Ground_Rock_6_N");
		Textures[6] = new Resource<Texture2D>("Textures/Terrain/rock_01_dif");
		NormalMaps[6] = new Resource<Texture2D>("Textures/Terrain/rock_01_nm");
		Textures[7] = new Resource<Texture2D>("Textures/Terrain/TexturesCom_GravelwithRubble_2048_albedo");
		NormalMaps[7] = new Resource<Texture2D>("Textures/Terrain/TexturesCom_GravelwithRubble_2048_normal");
		for (int i = 0; i < TreeNames.Length; i++)
		{
			Trees[i] = new PrefabResource("Prefabs/Nature Package/" + TreeNames[i]);
		}
		MinimapSettings = GameObject.Find("Minimap Settings").GetComponent<MinimapSettings>();
		InitTerrainArrays();
	}

	public static void InitTerrainArrays()
	{
		BirdSongManager.AggroTiles = new AggroTile[MaxSize / 16, MaxSize / 16];
		AStar.VerticesOnThread = new float[MaxSize + 1, MaxSize + 1];
		AStar.Tiles = new AStarTile[MaxSize, MaxSize];
		AStar.LookupSquares = new AStarLookupSquare[MaxSize / 8, MaxSize / 8];
		FogOfWar.Explored = new byte[MaxSize * MaxSize];
		FogOfWar.NewExplored = new byte[MaxSize * MaxSize];
		FogOfWar.TileRefs = new int[MaxSize * MaxSize];
		FogOfWar.NewTileRefs = new int[MaxSize * MaxSize];
		Tiles = new byte[MaxSize, MaxSize];
		Tiles2 = new byte[MaxSize, MaxSize];
		TilesThreadCopy = new byte[MaxSize, MaxSize];
		Vertices = new float[MaxSize + 1, MaxSize + 1];
		OriginalVertices = new float[MaxSize + 1, MaxSize + 1];
		TextureWeights = new byte[MaxSize, MaxSize, 8];
		MineralWeights = new byte[MaxSize / 4, MaxSize / 4, 4];
		LookupSquares = new TerrainLookupSquare[MaxSize / 8, MaxSize / 8];
		GrassMap.MaxDensityForPatch = new uint[MaxSize / 8, MaxSize / 8];
	}

	public void FixupBuiltOnTiles(TerrainRect rect)
	{
		for (int i = rect.min.x; i <= rect.max.x; i++)
		{
			for (int j = rect.min.y; j <= rect.max.y; j++)
			{
				if (GetAreaEnclosingObjectOnTile(i, j) != null)
				{
					Tiles[i, j] |= 4;
				}
				else
				{
					Tiles[i, j] &= 251;
				}
			}
		}
		LastEnclosedAreasChangedFrame = Session.Instance.Frame;
	}

	public void CalcEnclosedAreasBlocking()
	{
		LastCalcEnclosedAreasFrame = -1;
		CopyTilesForThread();
		CalcEnclosedAreas();
		CopyTilesFromThread();
	}

	public void InitOnThread()
	{
		FogOfWar.InitOnThread();
		CalcEnclosedAreasBlocking();
		using (new StopWatchMarker("Build minimap"))
		{
			BuildEntireMinimap();
		}
		CustomRandom customRandom = new CustomRandom();
		Color32[] colMap = HeightMapCol.GetColMap();
		for (int i = 0; i < colMap.Length; i++)
		{
			colMap[i].b = (byte)customRandom.Next(256);
		}
		BuildHeightMapTexCol(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
		MapSize mapSizeFromSize = GetMapSizeFromSize(Size);
		if (UnityHeightMapBuiltOnThread == null)
		{
			UnityHeightMapBuiltOnThread = new List<float[,]>();
			for (int j = 0; j < 5; j++)
			{
				UnityHeightMapBuiltOnThread.Add(null);
			}
		}
		if (UnityHeightMapBuiltOnThread[(int)mapSizeFromSize] == null)
		{
			UnityHeightMapBuiltOnThread[(int)mapSizeFromSize] = new float[Size + 1, Size + 1];
		}
		if (UnityAlphaMapBuiltOnThread == null)
		{
			UnityAlphaMapBuiltOnThread = new List<float[,,]>();
			for (int k = 0; k < 5; k++)
			{
				UnityAlphaMapBuiltOnThread.Add(null);
			}
		}
		if (UnityAlphaMapBuiltOnThread[(int)mapSizeFromSize] == null)
		{
			UnityAlphaMapBuiltOnThread[(int)mapSizeFromSize] = new float[Size, Size, 8];
		}
		BuildUnityHeightMap(new TerrainCoord(0, 0), new TerrainCoord(Size, Size), UnityHeightMapBuiltOnThread[(int)mapSizeFromSize]);
		BuildUnityAlphaMaps(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1), UnityAlphaMapBuiltOnThread[(int)mapSizeFromSize]);
	}

	public static void UnityOnQualityLevelChanged()
	{
		if (QualitySettings.GetQualityLevel() == 0)
		{
			Shader.EnableKeyword("LOW_QUALITY");
		}
		else
		{
			Shader.DisableKeyword("LOW_QUALITY");
		}
	}

	public static void UnityInitSnowShaderParams()
	{
		Shader.SetGlobalFloat(ShaderHash._SnowAmount, 0f);
		Shader.SetGlobalTexture(ShaderHash._SnowTex, (Texture2D)SnowTex);
		Shader.SetGlobalTexture(ShaderHash._SnowNormals, (Texture2D)SnowNormals);
		Shader.SetGlobalTexture(ShaderHash._SnowNoiseCover, (Texture2D)SnowNoiseCover);
		Shader.SetGlobalTexture(ShaderHash._MossTex, (Texture2D)MossTex);
		Shader.SetGlobalTexture(ShaderHash._MossNormals, (Texture2D)MossNormals);
		Shader.SetGlobalTexture(ShaderHash._MossRoughness, (Texture2D)MossRoughness);
		Shader.SetGlobalTexture(ShaderHash._MossAO, (Texture2D)MossAO);
	}

	public override void UnityInit()
	{
		base.UnityInit();
		using (new StopWatchMarker("Unity Init Terrain"))
		{
			UnityInitSnowShaderParams();
			UnityOnQualityLevelChanged();
			FogOfWar.UnityInit();
			using (new StopWatchMarker("Init grass"))
			{
				GrassMap.UnityInit();
			}
			MinimapTex = new Texture2D(Size, Size, TextureFormat.RGBA32, mipChain: false);
			MinimapTex.wrapMode = TextureWrapMode.Clamp;
			for (int i = 0; i < 4; i++)
			{
				GeologicalTex[i] = new Texture2D(Size / 4, Size / 4, TextureFormat.ARGB32, mipChain: false);
				GeologicalTex[i].wrapMode = TextureWrapMode.Clamp;
			}
			HeightMapTex = new Texture2D(Size, Size, TextureFormat.ARGB32, mipChain: false);
			HeightMapTex.wrapMode = TextureWrapMode.Clamp;
			TerrainData terrainData = new TerrainData();
			terrainData.heightmapResolution = Size + 1;
			terrainData.alphamapResolution = Size;
			terrainData.size = new Vector3(Size, 64f, Size);
			terrainData.baseMapResolution = Size;
			terrainData.SetDetailResolution(Size, 8);
			TerrainLayer[] array = new TerrainLayer[8];
			for (int j = 0; j < 8; j++)
			{
				TerrainLayer terrainLayer = new TerrainLayer();
				terrainLayer.diffuseTexture = Textures[j];
				terrainLayer.normalMapTexture = NormalMaps[j];
				if ((byte)j == 1)
				{
					terrainLayer.tileSize = new Vector2(2f, 2f);
				}
				else
				{
					terrainLayer.tileSize = new Vector2(4f, 4f);
				}
				array[j] = terrainLayer;
			}
			terrainData.terrainLayers = array;
			TreePrototype[] array2 = new TreePrototype[18];
			for (int k = 0; k < 18; k++)
			{
				TreePrototype treePrototype = new TreePrototype();
				treePrototype.prefab = Trees[k];
				array2[k] = treePrototype;
			}
			terrainData.treePrototypes = array2;
			MapSize mapSizeFromSize = GetMapSizeFromSize(Size);
			using (new StopWatchMarker("Apply textures"))
			{
				terrainData.SetAlphamaps(0, 0, UnityAlphaMapBuiltOnThread[(int)mapSizeFromSize]);
			}
			using (new StopWatchMarker("Apply heights"))
			{
				terrainData.SetHeights(0, 0, UnityHeightMapBuiltOnThread[(int)mapSizeFromSize]);
			}
			using (new StopWatchMarker("Apply trees"))
			{
				ApplyTreesToUnityTerrainData(terrainData);
			}
			using (new StopWatchMarker("Create object"))
			{
				UnityTerrainObj = Terrain.CreateTerrainGameObject(terrainData);
				UnityTerrainObj.name = "GameTerrain";
				UnityTerrainObj.transform.position = new Vector3((float)(-Size) * 0.5f, 0f, (float)(-Size) * 0.5f);
				UnityTerrain = UnityTerrainObj.GetComponent<Terrain>();
				UnityTerrain.materialTemplate = UnityEngine.Object.Instantiate(CustomTerrainMaterial.GetAsset());
				UnityTerrain.treeDistance = 256f;
				UnityTerrain.treeLODBiasMultiplier = GameImpl.Instance.Settings.TreeQuality;
				UnityTerrain.detailObjectDistance = 80f;
				UnityTerrain.materialTemplate.SetFloat(ShaderHash._SnowTiling, Weather.SnowTiling * (float)Size / 1024f);
				UnityTerrain.materialTemplate.SetFloat(ShaderHash._SnowNoiseTiling, Weather.SnowNoiseTiling * (float)Size / 1024f);
				Shader.SetGlobalFloat(ShaderHash._GrassDensityFactor, GameImpl.Instance.Settings.GrassDensity);
				UnityAudioSource = UnityTerrainObj.AddComponent<AudioSource>();
				UnityTerrainObj.AddComponent<IdBehaviour>().Id = Id;
			}
			using (new StopWatchMarker("Build rivers"))
			{
				UnityWaterMaterial = UnityEngine.Object.Instantiate(CustomWaterMaterial.GetAsset());
				UnityWaterMaterial.SetTexture("_ReflectionTex", GameImpl.Instance.UnityReflectionRenderTexture);
				UnityWaterMaterial.SetTexture("_RefractionTex", GameImpl.Instance.UnityRefractionRenderTexture);
				foreach (TerrainPath path in Paths)
				{
					path.UnityInitPath();
				}
			}
		}
	}

	public bool GetDebugShowTerrain()
	{
		if (!(UnityTerrain != null))
		{
			return false;
		}
		return UnityTerrain.drawHeightmap;
	}

	public bool GetDebugShowTrees()
	{
		if (!(UnityTerrain != null))
		{
			return false;
		}
		return UnityTerrain.drawTreesAndFoliage;
	}

	public float GetTreeLODBias()
	{
		if (!(UnityTerrain != null))
		{
			return 1f;
		}
		return UnityTerrain.treeLODBiasMultiplier;
	}

	public void SetDebugShowTerrain(bool v)
	{
		if (UnityTerrain != null)
		{
			UnityTerrain.drawHeightmap = v;
		}
	}

	public void SetDebugShowTrees(bool v)
	{
		if (UnityTerrain != null)
		{
			UnityTerrain.drawTreesAndFoliage = v;
		}
	}

	public void SetTreeLODBias(float v)
	{
		if (UnityTerrain != null)
		{
			UnityTerrain.treeLODBiasMultiplier = v;
		}
	}

	public override void UnityDelete()
	{
		foreach (TerrainPath path in Paths)
		{
			path.UnityDeletePath();
		}
		TerrainData terrainData = UnityTerrain.terrainData;
		UnityEngine.Object.Destroy(UnityTerrainObj);
		UnityEngine.Object.Destroy(UnityTerrain.materialTemplate);
		UnityEngine.Object.Destroy(UnityTerrain.normalmapTexture);
		UnityEngine.Object.Destroy(UnityWaterMaterial);
		for (int i = 0; i < terrainData.alphamapTextures.Length; i++)
		{
			UnityEngine.Object.Destroy(terrainData.alphamapTextures[i]);
		}
		UnityEngine.Object.Destroy(terrainData);
		UnityEngine.Object.Destroy(MinimapTex);
		UnityEngine.Object.Destroy(HeightMapTex);
		for (int j = 0; j < 4; j++)
		{
			UnityEngine.Object.Destroy(GeologicalTex[j]);
			GeologicalTex[j] = null;
		}
		FogOfWar.UnityDelete();
		GrassMap.UnityDelete();
		base.UnityDelete();
	}

	public void UnityOnTemperatureChanged(float oldTemp, float newTemp)
	{
		bool num = oldTemp < Weather.IceCollisionTemperatureInCelsius;
		bool flag = newTemp < Weather.IceCollisionTemperatureInCelsius;
		if (num == flag)
		{
			return;
		}
		foreach (TerrainPath path in Paths)
		{
			if (!path.IsRiver)
			{
				continue;
			}
			foreach (MeshCollider unityPathObject in path.UnityPathObjects)
			{
				unityPathObject.enabled = flag;
			}
		}
	}

	private float[,,] BuildUnityAlphaMaps(TerrainCoord tl, TerrainCoord br, float[,,] map)
	{
		if (map == null)
		{
			map = new float[br.y + 1 - tl.y, br.x + 1 - tl.x, 8];
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				int num = i - tl.x;
				int num2 = j - tl.y;
				for (int k = 0; k < 8; k++)
				{
					map[num2, num, k] = (float)(int)TextureWeights[i, j, k] / 255f;
				}
			}
		}
		return map;
	}

	private void ApplyTexturesToUnityTerrainData(TerrainData terrainData, TerrainCoord tl, TerrainCoord br)
	{
		ClampMinMaxTileWithinBounds(ref tl, ref br);
		float[,,] map = BuildUnityAlphaMaps(tl, br, null);
		terrainData.SetAlphamaps(tl.x, tl.y, map);
	}

	private float[,] BuildUnityHeightMap(TerrainCoord tl, TerrainCoord br, float[,] heights)
	{
		if (heights == null)
		{
			heights = new float[br.y + 1 - tl.y, br.x + 1 - tl.x];
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				heights[j - tl.y, i - tl.x] = Vertices[i, j] / 64f;
			}
		}
		return heights;
	}

	private void ApplyHeightsToUnityTerrainData(TerrainData terrainData, TerrainCoord tl, TerrainCoord br)
	{
		ClampMinMaxVertexWithinBounds(ref tl, ref br);
		float[,] heights = BuildUnityHeightMap(tl, br, null);
		terrainData.SetHeights(tl.x, tl.y, heights);
		BuildHeightMapTexCol(tl, br);
	}

	private void BuildHeightMapTexCol(TerrainCoord tl, TerrainCoord br)
	{
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		Color32[] colMap = HeightMapCol.GetColMap();
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				int num = i + j * Size;
				float num2 = Vertices[i, j];
				if ((IsTileRoad(i, j) || IsTileRoad(Math.Max(0, i - 1), j) || IsTileRoad(i, Math.Max(0, j - 1)) || IsTileRoad(Math.Max(0, i - 1), Math.Max(0, j - 1))) && GetAmountOnPath(GetVertexPosXZ(i, j), out var H, wantRiver: false) < 1f)
				{
					num2 = H;
				}
				float num3 = Mathf.Clamp01(num2 / 64f) * 65535f;
				colMap[num].r = (byte)(num3 / 256f);
				colMap[num].g = (byte)(num3 - (float)(colMap[num].r * 256));
				colMap[num].a = (byte)(255f * num2 / 64f);
			}
		}
		HeightMapTexDirty = true;
	}

	private void ApplyTreesToUnityTerrainData(TerrainData terrainData)
	{
		TreeInstance[] array = new TreeInstance[TreeProps.Count];
		for (int i = 0; i < TreeProps.Count; i++)
		{
			TreeProp treeProp = TreeProps[i];
			Vector3 pos = treeProp.Pos;
			TreeInstance treeInstance = default(TreeInstance);
			treeInstance.prototypeIndex = (int)treeProp.GetTreeTypeIncludingGrowth();
			treeInstance.position = new Vector3((pos.x + HalfSize) / (float)Size, pos.y / 64f, (pos.z + HalfSize) / (float)Size);
			treeInstance.rotation = treeProp.Rotation;
			treeInstance.color = Color.white;
			treeInstance.lightmapColor = Color.white;
			treeInstance.heightScale = (treeInstance.widthScale = treeProp.GetScaleIncludingGrowth());
			array[i] = treeInstance;
		}
		terrainData.treeInstances = array;
	}

	public TreeProp GetTreeFromCollisionPoint(Vector3 contactPoint)
	{
		TreeProp treeProp = null;
		TerrainCoord tileCoordForPos = GetTileCoordForPos(contactPoint);
		TreeMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(1, 1), tileCoordForPos + new TerrainCoord(1, 1), Temp);
		foreach (TileObject item in Temp)
		{
			if (MathUtil.ToXZ(item.Pos - contactPoint).sqrMagnitude < MathUtil.Squared(TreeProp.Radius + 0.1f))
			{
				treeProp = item as TreeProp;
				if (treeProp != null)
				{
					break;
				}
			}
		}
		Temp.Clear();
		return treeProp;
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		if (other is EnterableVehicle enterableVehicle)
		{
			TreeProp treeFromCollisionPoint = GetTreeFromCollisionPoint(contactPoint);
			if (treeFromCollisionPoint != null)
			{
				FallenTreeProp.Spawn(treeFromCollisionPoint).StartFalling(MathUtil.SafeNormalize(MathUtil.ToXZ(relativeVelocity), MathUtil.ToXZ(enterableVehicle.Forward)), enterableVehicle);
				treeFromCollisionPoint.Delete();
			}
		}
		return 0f;
	}

	public bool IsVertexOutsideBounds(int x, int y)
	{
		if (x >= 0 && x <= Size && y >= 0)
		{
			return y > Size;
		}
		return true;
	}

	public bool IsVertexOutsideTileRect(int vertX, int vertY, TerrainRect tileRect)
	{
		if (vertX >= tileRect.min.x && vertX <= tileRect.max.x + 1 && vertY >= tileRect.min.y)
		{
			return vertY > tileRect.max.y + 1;
		}
		return true;
	}

	public bool IsTileOutsideBounds(int x, int y)
	{
		if (x >= 0 && x < Size && y >= 0)
		{
			return y >= Size;
		}
		return true;
	}

	public bool IsTileOnEdge(int x, int y)
	{
		if (x > 0 && x < Size - 1 && y > 0)
		{
			return y >= Size - 1;
		}
		return true;
	}

	public bool IsTileRectWithinBounds(TerrainCoord tl, TerrainCoord br)
	{
		if (tl.x >= 0 && tl.y >= 0 && br.x < Size)
		{
			return br.y < Size;
		}
		return false;
	}

	public bool IsPosXZOutsideBounds(Vector2 posXZ)
	{
		if (!(posXZ.x < 0f - HalfSize) && !(posXZ.x >= HalfSize) && !(posXZ.y < 0f - HalfSize))
		{
			return posXZ.y >= HalfSize;
		}
		return true;
	}

	public bool IsLookupSquareOutsideBounds(int x, int y)
	{
		if (x >= 0 && x < Size / 8 && y >= 0)
		{
			return y >= Size / 8;
		}
		return true;
	}

	public bool IsMineralSquareOutsideBounds(int x, int y)
	{
		if (x >= 0 && x < Size / 4 && y >= 0)
		{
			return y >= Size / 4;
		}
		return true;
	}

	public Vector2 GetVertexPosXZ(int x, int y)
	{
		return new Vector2((float)x - HalfSize, (float)y - HalfSize);
	}

	public Vector3 GetVertexPos(int x, int y)
	{
		return new Vector3((float)x - HalfSize, Vertices[x, y], (float)y - HalfSize);
	}

	public Vector3 GetVertexPosSafe(int x, int y)
	{
		return new Vector3((float)x - HalfSize, IsVertexOutsideBounds(x, y) ? 0f : Vertices[x, y], (float)y - HalfSize);
	}

	public Vector3 GetOriginalVertexPos(int x, int y)
	{
		return new Vector3((float)x - HalfSize, OriginalVertices[x, y], (float)y - HalfSize);
	}

	public Vector3 GetAStarVertexPos(int x, int y)
	{
		return new Vector3((float)x - HalfSize, AStar.VerticesOnThread[x, y], (float)y - HalfSize);
	}

	public bool IsTriFlipped(int x, int y)
	{
		return (Tiles[x, y] & 1) != 0;
	}

	public bool IsImpassable(int x, int y, int options, Character requester, TileObject ignore)
	{
		bool isRound;
		return IsImpassable(x, y, options, requester, ignore, out isRound);
	}

	public bool IsImpassable(int x, int y, int options, Character requester, TileObject ignore, out bool isRound)
	{
		isRound = false;
		if ((options & 0x100) != 0)
		{
			if (IsImpassableRaw(x, y))
			{
				return true;
			}
		}
		else if (IsSlopeOrImpassableRaw(x, y))
		{
			return true;
		}
		if (requester != null && !requester.CanWalkInRivers() && IsTileRiver(x, y))
		{
			return true;
		}
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			TerrainCoord tile = new TerrainCoord(x, y);
			foreach (TileObject item in objects)
			{
				if (item != requester && (requester == null || (item != requester.InteractionObject && item != requester.CarryingObject)) && item != ignore && tile.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()) && item.IsImpassable(requester, options, tile) && ((options & 0x1000) == 0 || requester == null || !item.CanBeClearedForBuilding(requester.Community, ignore)))
				{
					if (!item.IsRound())
					{
						return true;
					}
					isRound = true;
				}
			}
		}
		return isRound;
	}

	public bool IsImpassableRaw(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return true;
		}
		return (Tiles[x, y] & 2) != 0;
	}

	public bool IsSlope(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return true;
		}
		return (Tiles[x, y] & 0x40) != 0;
	}

	public bool IsSlopeWithinInBounds(TerrainCoord tile)
	{
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return false;
		}
		return (Tiles[tile.x, tile.y] & 0x40) != 0;
	}

	public bool IsSlopeOrImpassableRaw(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return true;
		}
		return (Tiles[x, y] & 0x42) != 0;
	}

	public bool IsAnyTileInRectSlopeOrImpassableRaw(TerrainCoord minTile, TerrainCoord maxTile)
	{
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				if (IsSlopeOrImpassableRaw(i, j))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool DoesRectContainBiomeType(TerrainCoord minTile, TerrainCoord maxTile, BiomeType biomeType, BiomeType[,] biome)
	{
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				if (biome[i, j] == biomeType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetTileMaxHeight(int x, int y)
	{
		return GetTileMaxHeight(x, y, includeRiver: false);
	}

	public float GetTileMaxHeight(int x, int y, bool includeRiver)
	{
		float num = MathUtil.Max(Vertices[x, y], Vertices[x + 1, y], Vertices[x, y + 1], Vertices[x + 1, y + 1]);
		if (includeRiver)
		{
			if (IsTileRiver(x, y))
			{
				GetAmountOnPath(new Vector2((float)x - HalfSize, (float)y - HalfSize), out var H, wantRiver: true);
				GetAmountOnPath(new Vector2((float)(x + 1) - HalfSize, (float)y - HalfSize), out var H2, wantRiver: true);
				GetAmountOnPath(new Vector2((float)x - HalfSize, (float)(y + 1) - HalfSize), out var H3, wantRiver: true);
				GetAmountOnPath(new Vector2((float)(x + 1) - HalfSize, (float)(y + 1) - HalfSize), out var H4, wantRiver: true);
				num = Math.Max(num, MathUtil.Max(H, H2, H3, H4));
			}
			if (IsTileRoad(x, y))
			{
				GetAmountOnPath(new Vector2((float)x - HalfSize, (float)y - HalfSize), out var H5, wantRiver: false);
				GetAmountOnPath(new Vector2((float)(x + 1) - HalfSize, (float)y - HalfSize), out var H6, wantRiver: false);
				GetAmountOnPath(new Vector2((float)x - HalfSize, (float)(y + 1) - HalfSize), out var H7, wantRiver: false);
				GetAmountOnPath(new Vector2((float)(x + 1) - HalfSize, (float)(y + 1) - HalfSize), out var H8, wantRiver: false);
				num = Math.Max(num, MathUtil.Max(H5, H6, H7, H8));
			}
			if (IsTileTrap(x, y))
			{
				num = Math.Max(num, MathUtil.Max(OriginalVertices[x, y], OriginalVertices[x + 1, y], OriginalVertices[x, y + 1], OriginalVertices[x + 1, y + 1]));
			}
		}
		return num;
	}

	public float GetTileMaxHeightNotIncludingRiver(int x, int y)
	{
		return MathUtil.Max(Vertices[x, y], Vertices[x + 1, y], Vertices[x, y + 1], Vertices[x + 1, y + 1]);
	}

	public float GetTileMinHeight(int x, int y)
	{
		return MathUtil.Min(Vertices[x, y], Vertices[x + 1, y], Vertices[x, y + 1], Vertices[x + 1, y + 1]);
	}

	public float GetOriginalTileMinHeight(int x, int y)
	{
		return MathUtil.Min(OriginalVertices[x, y], OriginalVertices[x + 1, y], OriginalVertices[x, y + 1], OriginalVertices[x + 1, y + 1]);
	}

	public TerrainCoord GetTileCoordForPos(Vector3 pos)
	{
		return new TerrainCoord((int)(HalfSize + pos.x), (int)(HalfSize + pos.z));
	}

	public TerrainCoord GetTileCoordForPosXZ(Vector2 pos)
	{
		return new TerrainCoord((int)(HalfSize + pos.x), (int)(HalfSize + pos.y));
	}

	public TerrainCoord GetVertexCoordForPosXZ(Vector2 pos)
	{
		return new TerrainCoord((int)(HalfSize + pos.x + 0.5f), (int)(HalfSize + pos.y + 0.5f));
	}

	public void GetTileRectXZ(int x, int y, out Vector2 minXZ, out Vector2 maxXZ)
	{
		minXZ = new Vector2((float)x - HalfSize, (float)y - HalfSize);
		maxXZ = new Vector2((float)(x + 1) - HalfSize, (float)(y + 1) - HalfSize);
	}

	public void GetTileBox(TerrainCoord tile, out Vector3 centre, out Vector3 extents)
	{
		Vector2 tileCentreXZ = GetTileCentreXZ(tile);
		float tileMaxHeight = GetTileMaxHeight(tile.x, tile.y);
		float tileMinHeight = GetTileMinHeight(tile.x, tile.y);
		centre = new Vector3(tileCentreXZ.x, Mathf.Lerp(tileMinHeight, tileMaxHeight, 0.5f), tileCentreXZ.y);
		extents = new Vector3(0.5f, (tileMaxHeight - tileMinHeight) * 0.5f, 0.5f);
	}

	public float GetMaxHeightDiffInRect(TerrainCoord minVert, TerrainCoord maxVert)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		for (int i = minVert.x; i <= maxVert.x; i++)
		{
			for (int j = minVert.y; j <= maxVert.y; j++)
			{
				num = Math.Min(num, Vertices[i, j]);
				num2 = Math.Max(num2, Vertices[i, j]);
			}
		}
		return num2 - num;
	}

	public static TerrainType GetTerrainTypeForTex(TerrainTex tex)
	{
		switch (tex)
		{
		case TerrainTex.Grass:
			return TerrainType.Grass;
		case TerrainTex.Mud:
			return TerrainType.Mud;
		case TerrainTex.ConiferFloor:
		case TerrainTex.DeciduousFloor:
			return TerrainType.ForestFloor;
		case TerrainTex.Rock:
		case TerrainTex.RockCliff:
			return TerrainType.Rock;
		case TerrainTex.Road:
			return TerrainType.Road;
		case TerrainTex.Debris:
			return TerrainType.Debris;
		default:
			return TerrainType.Invalid;
		}
	}

	public Color32 GetMapColorForTile(TerrainCoord tile)
	{
		if (IsTileRiver(tile.x, tile.y))
		{
			return MinimapSettings.RiverCol;
		}
		if (IsTileRoad(tile.x, tile.y))
		{
			return MinimapSettings.RoadCol;
		}
		switch (GetTileTerrainTex(tile))
		{
		case TerrainTex.Grass:
		case TerrainTex.Mud:
			return MinimapSettings.GrassCol;
		case TerrainTex.ConiferFloor:
		case TerrainTex.DeciduousFloor:
			return MinimapSettings.ForestCol;
		case TerrainTex.Rock:
		case TerrainTex.RockCliff:
			return MinimapSettings.RockCol;
		case TerrainTex.Road:
		case TerrainTex.Debris:
			return MinimapSettings.RoadCol;
		default:
			return new Color32(0, 0, 0, byte.MaxValue);
		}
	}

	public override BulletHitEffect GetBulletHitEffect(Vector3 nondeterministicHitPos)
	{
		TerrainType tileTerrainType = GetTileTerrainType(GetTileCoordForPos(nondeterministicHitPos));
		if ((uint)(tileTerrainType - 2) <= 1u)
		{
			return BulletHitEffect.Ricochet;
		}
		if (!(Session.Instance.Weather.SnowOnGroundAmount >= 0.9f))
		{
			return BulletHitEffect.Smoke;
		}
		return BulletHitEffect.SnowPuff;
	}

	public TerrainType GetTileTerrainType(TerrainCoord tile)
	{
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return TerrainType.Invalid;
		}
		TerrainType result = TerrainType.Grass;
		float num = 0f;
		for (int i = 0; i < 8; i++)
		{
			if ((float)(int)TextureWeights[tile.x, tile.y, i] > num)
			{
				result = GetTerrainTypeForTex((TerrainTex)i);
				num = (int)TextureWeights[tile.x, tile.y, i];
			}
		}
		return result;
	}

	public TerrainTex GetTileTerrainTex(TerrainCoord tile)
	{
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return TerrainTex.Count;
		}
		TerrainTex result = TerrainTex.Grass;
		float num = 0f;
		for (int i = 0; i < 8; i++)
		{
			if ((float)(int)TextureWeights[tile.x, tile.y, i] > num)
			{
				result = (TerrainTex)i;
				num = (int)TextureWeights[tile.x, tile.y, i];
			}
		}
		return result;
	}

	public Vector3 GetNormalAtPos(float x, float z)
	{
		return GetPlaneAtPos(x, z).normal;
	}

	public Vector3 GetSmoothNormalAtVertex(int x, int y)
	{
		Vector3 vertexPos = GetVertexPos(Math.Max(0, x - 1), y);
		Vector3 vertexPos2 = GetVertexPos(Math.Min(Size, x + 1), y);
		Vector3 vertexPos3 = GetVertexPos(x, Math.Max(0, y - 1));
		return -Vector3.Normalize(Vector3.Cross(rhs: GetVertexPos(x, Math.Min(Size, y + 1)) - vertexPos3, lhs: vertexPos2 - vertexPos));
	}

	public Vector3 GetInterpolatedNormalAtPos(Vector2 posXZ)
	{
		return UnityTerrain.terrainData.GetInterpolatedNormal(Mathf.Clamp01((posXZ.x + HalfSize) / (float)Size), Mathf.Clamp01((posXZ.y + HalfSize) / (float)Size));
	}

	public Plane GetPlaneAtPos(float x, float z)
	{
		TerrainCoord tile = new TerrainCoord((int)(HalfSize + x), (int)(HalfSize + z));
		tile = ClampTileWithinBounds(tile);
		Vector3 vertexPos = GetVertexPos(tile.x, tile.y);
		Vector3 vertexPos2 = GetVertexPos(tile.x, tile.y + 1);
		Vector3 vertexPos3 = GetVertexPos(tile.x + 1, tile.y + 1);
		Vector3 vertexPos4 = GetVertexPos(tile.x + 1, tile.y);
		float num = x - vertexPos.x;
		float num2 = z - vertexPos.z;
		if (!IsTriFlipped(tile.x, tile.y))
		{
			if (num2 > num)
			{
				return new Plane(vertexPos, vertexPos2, vertexPos3);
			}
			return new Plane(vertexPos3, vertexPos4, vertexPos);
		}
		if (num2 < 1f - num)
		{
			return new Plane(vertexPos, vertexPos2, vertexPos4);
		}
		return new Plane(vertexPos3, vertexPos4, vertexPos2);
	}

	public float GetAmountOnPath(Vector2 posXZ, out float H, bool wantRiver)
	{
		float num = 1f;
		float num2 = 0f;
		foreach (TerrainPath path in Paths)
		{
			if (path.IsRiver == wantRiver)
			{
				float h;
				float amountOnPath = path.GetAmountOnPath(this, posXZ, out h);
				if (amountOnPath < num)
				{
					num2 = ((!path.IsRiver) ? Math.Max(num2, h) : h);
					num = amountOnPath;
				}
			}
		}
		H = num2;
		return num;
	}

	public float GetTileHeightAtPos(Vector2 posXZ, bool ignoreIce, bool ignoreRoadCamber)
	{
		return GetTileHeightAtPos(posXZ.x, posXZ.y, ignoreIce, ignoreRoadCamber);
	}

	public float GetTileHeightAtPos(float x, float z)
	{
		return GetTileHeightAtPos(x, z, ignoreIce: false, ignoreRoadCamber: false);
	}

	public float GetTileHeightAtPos(float x, float z, bool ignoreIce, bool ignoreRoadCamber)
	{
		float num = Mathf.Clamp(x + HalfSize, 0f, Size);
		float num2 = Mathf.Clamp(z + HalfSize, 0f, Size);
		int num3 = (int)num;
		int num4 = (int)num2;
		float t = num % 1f;
		float t2 = num2 % 1f;
		float num5 = Mathf.Lerp(Mathf.Lerp(Vertices[num3, num4], Vertices[Math.Min(Size, num3 + 1), num4], t), Mathf.Lerp(Vertices[num3, Math.Min(Size, num4 + 1)], Vertices[Math.Min(Size, num3 + 1), Math.Min(Size, num4 + 1)], t), t2);
		int x2 = Math.Min(num3, Size - 1);
		int y = Math.Min(num4, Size - 1);
		if (!ignoreIce && IsTileRiver(x2, y) && Session.Instance.Weather.TemperatureInCelsius < 0f)
		{
			float H;
			float amountOnPath = GetAmountOnPath(new Vector2(x, z), out H, wantRiver: true);
			if (amountOnPath < 1f)
			{
				float t3 = Mathf.Clamp01((Mathf.Clamp01(Session.Instance.Weather.TemperatureInCelsius / Weather.WaterCompletelyFrozenTemperatureInCelsius) * (1f + IceCoverSoftness * 2f) + IceCoverSoftness - (1f - Mathf.Abs(amountOnPath))) / IceCoverSoftness);
				return Mathf.Lerp(num5, Math.Max(num5, H + RiverHeightOffset), t3);
			}
		}
		if (!ignoreRoadCamber)
		{
			if (IsTileRoad(x2, y) && GetAmountOnPath(new Vector2(x, z), out var H2, wantRiver: false) < 1f)
			{
				return Math.Max(num5, H2);
			}
			if (IsTileTrap(x2, y))
			{
				float val = Mathf.Lerp(Mathf.Lerp(OriginalVertices[num3, num4], OriginalVertices[Math.Min(Size, num3 + 1), num4], t), Mathf.Lerp(OriginalVertices[num3, Math.Min(Size, num4 + 1)], OriginalVertices[Math.Min(Size, num3 + 1), Math.Min(Size, num4 + 1)], t), t2);
				num5 = Math.Max(num5, val);
			}
		}
		return num5;
	}

	public float GetOriginalTileHeightAtPos(float x, float z)
	{
		float num = Mathf.Clamp(x + HalfSize, 0f, Size);
		float num2 = Mathf.Clamp(z + HalfSize, 0f, Size);
		int num3 = (int)num;
		int num4 = (int)num2;
		float t = num % 1f;
		float t2 = num2 % 1f;
		return Mathf.Lerp(Mathf.Lerp(OriginalVertices[num3, num4], OriginalVertices[Math.Min(Size, num3 + 1), num4], t), Mathf.Lerp(OriginalVertices[num3, Math.Min(Size, num4 + 1)], OriginalVertices[Math.Min(Size, num3 + 1), Math.Min(Size, num4 + 1)], t), t2);
	}

	public float GetAStarTileHeightAtPos(float x, float z)
	{
		float num = Mathf.Clamp(x + HalfSize, 0f, Size);
		float num2 = Mathf.Clamp(z + HalfSize, 0f, Size);
		int num3 = (int)num;
		int num4 = (int)num2;
		float t = num % 1f;
		float t2 = num2 % 1f;
		return Mathf.Lerp(Mathf.Lerp(AStar.VerticesOnThread[num3, num4], AStar.VerticesOnThread[Math.Min(Size, num3 + 1), num4], t), Mathf.Lerp(AStar.VerticesOnThread[num3, Math.Min(Size, num4 + 1)], AStar.VerticesOnThread[Math.Min(Size, num3 + 1), Math.Min(Size, num4 + 1)], t), t2);
	}

	public Vector3 ClampPosToSurface(Vector3 pos)
	{
		pos.y = GetTileHeightAtPos(pos.x, pos.z);
		return pos;
	}

	public Vector3 ClampPosAboveSurface(Vector3 pos, float minHeightAbove, bool ignoreIce, bool ignoreRoadCamber)
	{
		pos.y = Math.Max(pos.y, GetTileHeightAtPos(pos.x, pos.z, ignoreIce, ignoreRoadCamber) + minHeightAbove);
		return pos;
	}

	public Vector3 GetSurfaceOffset(float x, float yOffset, float z)
	{
		return new Vector3(x, GetTileHeightAtPos(x, z) + yOffset, z);
	}

	public Vector3 GetTileCentrePos(TerrainCoord tile)
	{
		return GetTileCentrePos(tile, ignoreIce: false, ignoreRoadCamber: false);
	}

	public Vector3 GetTileCentrePos(TerrainCoord tile, bool ignoreIce, bool ignoreRoadCamber)
	{
		float x = (float)tile.x - HalfSize + 0.5f;
		float z = (float)tile.y - HalfSize + 0.5f;
		float tileHeightAtPos = GetTileHeightAtPos(x, z, ignoreIce, ignoreRoadCamber);
		return new Vector3(x, tileHeightAtPos, z);
	}

	public Vector3 GetOriginalTileCentrePos(TerrainCoord tile)
	{
		float x = (float)tile.x - HalfSize + 0.5f;
		float z = (float)tile.y - HalfSize + 0.5f;
		float originalTileHeightAtPos = GetOriginalTileHeightAtPos(x, z);
		return new Vector3(x, originalTileHeightAtPos, z);
	}

	public Vector3 GetAStarTileCentrePos(TerrainCoord tile)
	{
		float x = (float)tile.x - HalfSize + 0.5f;
		float z = (float)tile.y - HalfSize + 0.5f;
		float aStarTileHeightAtPos = GetAStarTileHeightAtPos(x, z);
		return new Vector3(x, aStarTileHeightAtPos, z);
	}

	public Vector2 GetTileCentreXZ(TerrainCoord tile)
	{
		float x = (float)tile.x - HalfSize + 0.5f;
		float y = (float)tile.y - HalfSize + 0.5f;
		return new Vector2(x, y);
	}

	public Vector3 GetTileCentreMinHeightPos(TerrainCoord tile)
	{
		float x = (float)tile.x - HalfSize + 0.5f;
		float z = (float)tile.y - HalfSize + 0.5f;
		float tileMinHeight = GetTileMinHeight(tile.x, tile.y);
		return new Vector3(x, tileMinHeight, z);
	}

	public bool ClampMinMaxTileWithinBounds(ref TerrainCoord tl, ref TerrainCoord br)
	{
		tl.x = Math.Max(tl.x, 0);
		tl.y = Math.Max(tl.y, 0);
		br.x = Math.Min(br.x, Size - 1);
		br.y = Math.Min(br.y, Size - 1);
		if (br.x >= tl.x)
		{
			return br.y >= tl.y;
		}
		return false;
	}

	public bool ClampMinMaxVertexWithinBounds(ref TerrainCoord tl, ref TerrainCoord br)
	{
		tl.x = Math.Max(tl.x, 0);
		tl.y = Math.Max(tl.y, 0);
		br.x = Math.Min(br.x, Size);
		br.y = Math.Min(br.y, Size);
		if (br.x >= tl.x)
		{
			return br.y >= tl.y;
		}
		return false;
	}

	public TerrainCoord ClampTileWithinBounds(TerrainCoord tile)
	{
		tile.x = Math.Max(tile.x, 0);
		tile.y = Math.Max(tile.y, 0);
		tile.x = Math.Min(tile.x, Size - 1);
		tile.y = Math.Min(tile.y, Size - 1);
		return tile;
	}

	public TerrainCoord ClampVertexWithinBounds(TerrainCoord vert)
	{
		vert.x = Math.Max(vert.x, 0);
		vert.y = Math.Max(vert.y, 0);
		vert.x = Math.Min(vert.x, Size);
		vert.y = Math.Min(vert.y, Size);
		return vert;
	}

	public Vector3 ClampPosWithinBounds(Vector3 pos, float bufferXZ)
	{
		pos.x = Mathf.Clamp(pos.x, 0f - HalfSize + bufferXZ, HalfSize - bufferXZ);
		pos.y = Mathf.Clamp(pos.y, 0f, 64f);
		pos.z = Mathf.Clamp(pos.z, 0f - HalfSize + bufferXZ, HalfSize - bufferXZ);
		return pos;
	}

	public void AddSingleTileObject(int x, int y, TileObject obj)
	{
		LookupSquares[x / 8, y / 8].AddObject(obj);
		if (obj.CanEncloseAnArea())
		{
			Tiles[x, y] |= 4;
			LastEnclosedAreasChangedFrame = Session.Instance.Frame;
		}
	}

	public void AddMultiTileObject(TerrainCoord minTile, TerrainCoord maxTile, TileObject obj)
	{
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				LookupSquares[i, j].AddObject(obj);
			}
		}
		if (!obj.CanEncloseAnArea())
		{
			return;
		}
		for (int k = minTile.x; k <= maxTile.x; k++)
		{
			for (int l = minTile.y; l <= maxTile.y; l++)
			{
				Tiles[k, l] |= 4;
			}
		}
		LastEnclosedAreasChangedFrame = Session.Instance.Frame;
	}

	public void RemoveSingleTileObject(int x, int y, TileObject obj)
	{
		LookupSquares[x / 8, y / 8].RemoveObject(obj);
		if (obj.CanEncloseAnArea() && GetAreaEnclosingObjectOnTile(x, y) == null)
		{
			Tiles[x, y] &= 251;
			LastEnclosedAreasChangedFrame = Session.Instance.Frame;
		}
	}

	public void RemoveMultiTileObject(TerrainCoord minTile, TerrainCoord maxTile, TileObject obj)
	{
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				LookupSquares[i, j].RemoveObject(obj);
			}
		}
		if (!obj.CanEncloseAnArea())
		{
			return;
		}
		for (int k = minTile.x; k <= maxTile.x; k++)
		{
			for (int l = minTile.y; l <= maxTile.y; l++)
			{
				if (GetAreaEnclosingObjectOnTile(k, l) == null)
				{
					Tiles[k, l] &= 251;
				}
			}
		}
		LastEnclosedAreasChangedFrame = Session.Instance.Frame;
	}

	public List<TileObject> GetObjectsInLookupSquare(int x, int y)
	{
		return LookupSquares[x / 8, y / 8].Objects;
	}

	public List<TileObject> GetObjectsInLookupSquareUsingLookupCoords(int x, int y)
	{
		return LookupSquares[x, y].Objects;
	}

	public int CountObjectsOfTypeInLookupSquare(int x, int y, BaseObjectType type)
	{
		int num = 0;
		List<TileObject> objectsInLookupSquare = GetObjectsInLookupSquare(x, y);
		if (objectsInLookupSquare != null)
		{
			foreach (TileObject item in objectsInLookupSquare)
			{
				if (item.GetBaseObjectType() == type)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int CountObjectsOfClassInLookupSquare(int x, int y, Type type)
	{
		int num = 0;
		List<TileObject> objectsInLookupSquare = GetObjectsInLookupSquare(x, y);
		if (objectsInLookupSquare != null)
		{
			foreach (TileObject item in objectsInLookupSquare)
			{
				if (item.GetType().IsA(type))
				{
					num++;
				}
			}
		}
		return num;
	}

	public int GetOwnerCommunityIdForTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return 0;
		}
		return LookupSquares[x / 8, y / 8].OwnerCommunityId & 0x7FFFFFFF;
	}

	public int GetOwnerCommunityIdForTileNoBoundsCheck(int x, int y)
	{
		return LookupSquares[x / 8, y / 8].OwnerCommunityId & 0x7FFFFFFF;
	}

	public int GetOwnerCommunityIdForLookupSquare(int x, int y)
	{
		return LookupSquares[x, y].OwnerCommunityId & 0x7FFFFFFF;
	}

	public void SetOwnerCommunityIdForLookupTileManually(int x, int y, int id)
	{
		LookupSquares[x / 8, y / 8].OwnerCommunityId = id | int.MinValue;
		Instance.AStar.AddChange(AStarChange.SetLookupSquareOwnerCommunityId(new TerrainCoord(x / 8, y / 8), id | int.MinValue));
	}

	public void GetObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea, bool clearList = true)
	{
		using (new UnityProfileMarker(GetObjectsInRectStr))
		{
			if (clearList)
			{
				objectsInArea.Clear();
			}
			ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
			for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
			{
				for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
				{
					List<TileObject> objects = LookupSquares[i, j].Objects;
					if (objects == null)
					{
						continue;
					}
					for (int k = 0; k < objects.Count; k++)
					{
						if (TerrainCoord.Overlaps(objects[k].GetMinTile(), objects[k].GetMaxTile(), minTile, maxTile))
						{
							objectsInArea.Add(objects[k]);
						}
					}
				}
			}
			TileObjectsSorter.RemoveDuplicates(objectsInArea, wantSort: false);
		}
	}

	public void GetObjectsInSphere(BoundingSphere sphere, List<TileObject> objectsInArea)
	{
		TerrainCoord tileCoordForPos = GetTileCoordForPos(sphere.position);
		int num = (int)Math.Ceiling(sphere.radius);
		TerrainCoord minTile = tileCoordForPos - new TerrainCoord(num, num);
		TerrainCoord maxTile = tileCoordForPos + new TerrainCoord(num, num);
		GetObjectsInRect(minTile, maxTile, objectsInArea);
		int num2 = 0;
		for (int i = 0; i < objectsInArea.Count; i++)
		{
			TileObject tileObject = objectsInArea[i];
			if (tileObject.GetBoundingBox().SqrDistance(sphere.position) <= sphere.radius * sphere.radius)
			{
				objectsInArea[num2] = tileObject;
				num2++;
			}
		}
		objectsInArea.RemoveRange(num2, objectsInArea.Count - num2);
	}

	public TileObject GetClosestObjectInRange(TerrainCoord tile, int range, FilterFunc filterFunc)
	{
		GetObjectsInRect(tile - new TerrainCoord(range, range), tile + new TerrainCoord(range, range), Temp);
		float num = float.MaxValue;
		TileObject result = null;
		foreach (TileObject item in Temp)
		{
			float distSquared = tile.GetDistSquared(item.GetCentreTile());
			if (distSquared < num && filterFunc(item))
			{
				num = distSquared;
				result = item;
			}
		}
		Temp.Clear();
		return result;
	}

	public void GetFilteredObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea, FilterFunc filterFunc)
	{
		objectsInArea.Clear();
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects == null)
				{
					continue;
				}
				for (int k = 0; k < objects.Count; k++)
				{
					if (filterFunc(objects[k]) && TerrainCoord.Overlaps(objects[k].GetMinTile(), objects[k].GetMaxTile(), minTile, maxTile))
					{
						objectsInArea.Add(objects[k]);
					}
				}
			}
		}
		TileObjectsSorter.RemoveDuplicates(objectsInArea, wantSort: false);
	}

	public void GetObjectsOfTypeInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea, Type type)
	{
		objectsInArea.Clear();
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects == null)
				{
					continue;
				}
				for (int k = 0; k < objects.Count; k++)
				{
					if (type.IsInstanceOfType(objects[k]) && TerrainCoord.Overlaps(objects[k].GetMinTile(), objects[k].GetMaxTile(), minTile, maxTile))
					{
						objectsInArea.Add(objects[k]);
					}
				}
			}
		}
		TileObjectsSorter.RemoveDuplicates(objectsInArea, wantSort: false);
	}

	public void GetObjectsOfBaseTypeInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea, BaseObjectType type)
	{
		objectsInArea.Clear();
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects == null)
				{
					continue;
				}
				for (int k = 0; k < objects.Count; k++)
				{
					if (objects[k].GetBaseObjectType() == type && TerrainCoord.Overlaps(objects[k].GetMinTile(), objects[k].GetMaxTile(), minTile, maxTile))
					{
						objectsInArea.Add(objects[k]);
					}
				}
			}
		}
		TileObjectsSorter.RemoveDuplicates(objectsInArea, wantSort: false);
	}

	public void GetTriggerZonesOnTile(TerrainCoord tile, List<Zone> triggerZones)
	{
		List<TileObject> objects = LookupSquares[tile.x / 8, tile.y / 8].Objects;
		if (objects == null)
		{
			return;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			if (objects[i] is Zone zone && tile.IsWithinBounds(zone.GetMinTile(), zone.GetMaxTile()))
			{
				triggerZones.Add(zone);
			}
		}
	}

	public void GetUnityObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea)
	{
		using (new UnityProfileMarker(GetUnityObjectsInRectStr))
		{
			ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
			TerrainCoord terrainCoord = minTile / 8;
			TerrainCoord terrainCoord2 = maxTile / 8;
			for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
			{
				for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
				{
					List<TileObject> objects = LookupSquares[i, j].Objects;
					if (objects == null)
					{
						continue;
					}
					for (int k = 0; k < objects.Count; k++)
					{
						if (objects[k].CanUnityObjectBeActivated())
						{
							objectsInArea.Add(objects[k]);
						}
					}
				}
			}
		}
	}

	public void GetUnityObjectsInRectApprox(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInArea)
	{
		using (new UnityProfileMarker(GetUnityObjectsInRectStr))
		{
			ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
			TerrainCoord terrainCoord = minTile / 8;
			TerrainCoord terrainCoord2 = maxTile / 8;
			for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
			{
				for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
				{
					List<TileObject> objects = LookupSquares[i, j].Objects;
					if (objects != null)
					{
						for (int k = 0; k < objects.Count; k++)
						{
							objectsInArea.Add(objects[k]);
						}
					}
				}
			}
		}
	}

	public static bool IsFixedObject(TileObject obj)
	{
		if (obj is Character)
		{
			return false;
		}
		if (obj is PitTrap { IsJoiner: not false })
		{
			return false;
		}
		if (obj is SpawnPoint)
		{
			return false;
		}
		if (obj is Zone)
		{
			return false;
		}
		return true;
	}

	public bool IsAnyFixedObjectOnRect(TerrainCoord minTile, TerrainCoord maxTile)
	{
		ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / 8; i <= maxTile.x / 8; i++)
		{
			for (int j = minTile.y / 8; j <= maxTile.y / 8; j++)
			{
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects == null)
				{
					continue;
				}
				for (int k = 0; k < objects.Count; k++)
				{
					if (IsFixedObject(objects[k]) && TerrainCoord.Overlaps(objects[k].GetMinTile(), objects[k].GetMaxTile(), minTile, maxTile))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public TileObject GetAreaEnclosingObjectOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item.CanEncloseAnArea() && terrainCoord.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
				{
					return item;
				}
			}
		}
		return null;
	}

	public CraftingProp GetCraftingPropOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is CraftingProp result && terrainCoord.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
				{
					return result;
				}
			}
		}
		return null;
	}

	public TileObject GetFixedObjectOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		TileObject result = null;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (IsFixedObject(item) && terrainCoord.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
				{
					if (item.GetBaseObjectType() != BaseObjectType.EnterableVehicle)
					{
						return item;
					}
					result = item;
				}
			}
		}
		return result;
	}

	public TileObject GetUnderConstructionObjectOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (IsFixedObject(item) && terrainCoord.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()) && item.GetUnderConstructionInfo() != null)
				{
					return item;
				}
			}
		}
		return null;
	}

	public TileObject IsUpgradingFence(TileObject newBuilding)
	{
		TerrainCoord terrainCoord = newBuilding.GetMinTile() / 8;
		TerrainCoord terrainCoord2 = newBuilding.GetMaxTile() / 8;
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				if (IsLookupSquareOutsideBounds(i, j))
				{
					continue;
				}
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects == null)
				{
					continue;
				}
				foreach (TileObject item in objects)
				{
					if (item != newBuilding && (item is BaseFence || item is Gate) && newBuilding.GetTileRect().Contains(item.GetTile()))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	public PitTrap GetPitTrapOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is PitTrap pitTrap && pitTrap.Tile.x == x && pitTrap.Tile.y == y)
				{
					return pitTrap;
				}
			}
		}
		return null;
	}

	public bool IsAnyCoveredTrapOnTile(int x, int y, PitTrap ignore)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return false;
		}
		new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is PitTrap pitTrap && pitTrap != ignore && pitTrap.Tile.x == x && pitTrap.Tile.y == y && pitTrap.IsCovered)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void FindDuplicatePitTraps(PitTrap pitTrap, List<BaseObject> deleteList)
	{
		if (IsTileOutsideBounds(pitTrap.Tile.x, pitTrap.Tile.y))
		{
			return;
		}
		List<TileObject> objects = LookupSquares[pitTrap.Tile.x / 8, pitTrap.Tile.y / 8].Objects;
		if (objects == null)
		{
			return;
		}
		foreach (TileObject item in objects)
		{
			if (item is PitTrap pitTrap2 && pitTrap2 != pitTrap && pitTrap2.Tile == pitTrap.Tile && pitTrap2.IsJoiner)
			{
				deleteList.Add(pitTrap2);
			}
		}
	}

	public TileObject GetTreePropOnTile(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is TreeProp treeProp && treeProp.Tile == terrainCoord)
				{
					return item;
				}
			}
		}
		return null;
	}

	public Prop GetProp(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is Prop prop && terrainCoord.IsWithinBounds(prop.MinTile, prop.MaxTile))
				{
					return prop;
				}
			}
		}
		return null;
	}

	public Building GetBuilding(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is Building building && terrainCoord.IsWithinBounds(building.MinTile, building.MaxTile))
				{
					return building;
				}
			}
		}
		return null;
	}

	public PlantableCrop GetPlant(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return null;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			foreach (TileObject item in objects)
			{
				if (item is PlantableCrop plantableCrop && plantableCrop.Tile == terrainCoord)
				{
					return plantableCrop;
				}
			}
		}
		return null;
	}

	public bool IsFence(int x, int y, bool vert, bool horiz)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return false;
		}
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects != null)
		{
			TerrainCoord terrainCoord = new TerrainCoord(x, y);
			foreach (TileObject item in objects)
			{
				if (item.GetUnderConstructionInfo() != null)
				{
					continue;
				}
				if (item is BaseFence)
				{
					if (!((item as BaseFence).Tile != terrainCoord))
					{
						return true;
					}
				}
				else if (item is Gate gate && terrainCoord.IsWithinBounds(gate.MinTile, gate.MaxTile))
				{
					if (vert)
					{
						return gate.Orientation == Prop.OrientationType.Deg0 || gate.Orientation == Prop.OrientationType.Deg180;
					}
					if (horiz)
					{
						return gate.Orientation == Prop.OrientationType.Deg90 || gate.Orientation == Prop.OrientationType.Deg270;
					}
					return false;
				}
			}
		}
		return false;
	}

	public void RecalcFence(int x, int y)
	{
		if (IsTileOutsideBounds(x, y))
		{
			return;
		}
		List<TileObject> objects = LookupSquares[x / 8, y / 8].Objects;
		if (objects == null)
		{
			return;
		}
		TerrainCoord terrainCoord = new TerrainCoord(x, y);
		foreach (TileObject item in objects)
		{
			if (item is BaseFence baseFence && baseFence.Tile == terrainCoord)
			{
				baseFence.UpdateModelAndAngle(inited: true);
			}
		}
	}

	public void AdjustHeight(Vector2 posXZ, float radius, float amount, bool lockRivers, bool lockRoads, bool apply)
	{
		TerrainCoord tl = GetTileCoordForPosXZ(posXZ - new Vector2(radius, radius));
		TerrainCoord br = GetTileCoordForPosXZ(posXZ + new Vector2(radius, radius)) + new TerrainCoord(1, 1);
		if (!ClampMinMaxVertexWithinBounds(ref tl, ref br))
		{
			return;
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				if (IsVertexOutsideBounds(i, j) || (lockRivers && IsVertexRiver(i, j)) || (lockRoads && IsVertexRoad(i, j)))
				{
					continue;
				}
				float magnitude = (GetVertexPosXZ(i, j) - posXZ).magnitude;
				if (!(magnitude <= radius))
				{
					continue;
				}
				float num = amount * (1f - magnitude / radius);
				Vertices[i, j] = Mathf.Clamp(Vertices[i, j] + num, 0f, 64f);
				if (GetTerrainTypeGrassFactor(GetTileTerrainType(new TerrainCoord(i, j))) == 0f)
				{
					for (int k = 0; k < 8; k++)
					{
						GrassMap.SetGrassAmount(i, j, (GrassType)k, 0);
					}
				}
			}
		}
		if (apply)
		{
			GrassMap.ApplyGrassChangesIfNeeded(tl, br - new TerrainCoord(1, 1));
			OnHeightChanged(tl - new TerrainCoord(1, 1), br);
		}
	}

	private void SetTextureWeight(int x, int y, TerrainTex tex, float amount)
	{
		float num = 0f;
		for (int i = 0; i < 8; i++)
		{
			if (GetTerrainTypeForTex((TerrainTex)i) == TerrainType.Rock)
			{
				num = Math.Min(num + (float)(int)TextureWeights[x, y, i] / 255f, 1f);
			}
		}
		float num2 = 1f - num;
		float num3 = Mathf.Clamp((float)(int)TextureWeights[x, y, (uint)tex] / 255f + amount, 0f, num2);
		TextureWeights[x, y, (uint)tex] = (byte)(num3 * 255f);
		float num4 = 0f;
		for (int j = 0; j < 8; j++)
		{
			if (j != (int)tex && GetTerrainTypeForTex((TerrainTex)j) != TerrainType.Rock)
			{
				num4 += (float)(int)TextureWeights[x, y, j] / 255f;
			}
		}
		if (num4 > 0f)
		{
			for (int k = 0; k < 8; k++)
			{
				if (k != (int)tex && GetTerrainTypeForTex((TerrainTex)k) != TerrainType.Rock)
				{
					float num5 = (float)(int)TextureWeights[x, y, k] / 255f;
					float num6 = (num2 - num3) * num5 / num4;
					TextureWeights[x, y, k] = (byte)(num6 * 255f);
				}
			}
		}
		if (GetTerrainTypeGrassFactor(GetTileTerrainType(new TerrainCoord(x, y))) == 0f)
		{
			for (int l = 0; l < 8; l++)
			{
				GrassMap.SetGrassAmount(x, y, (GrassType)l, 0);
			}
		}
	}

	public void SetTerrainTexture(TerrainTex tex, Vector2 posXZ, float radius, float amount, float limit, bool apply)
	{
		TerrainCoord tl = GetTileCoordForPosXZ(posXZ - new Vector2(radius, radius));
		TerrainCoord br = GetTileCoordForPosXZ(posXZ + new Vector2(radius, radius));
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		using (new StopWatchMarker("Set Terrain Texture"))
		{
			for (int i = tl.x; i <= br.x; i++)
			{
				for (int j = tl.y; j <= br.y; j++)
				{
					if (!IsTileOutsideBounds(i, j))
					{
						float magnitude = (GetTileCentreXZ(new TerrainCoord(i, j)) - posXZ).magnitude;
						if (magnitude <= radius)
						{
							SetTextureWeight(i, j, tex, amount * (1f - magnitude / radius));
						}
					}
				}
			}
			if (apply)
			{
				GrassMap.ApplyGrassChangesIfNeeded(tl, br);
				using (new StopWatchMarker("Apply Textures"))
				{
					ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, tl, br);
				}
				using (new StopWatchMarker("Build Minimap"))
				{
					BuildMinimap(tl, br);
					return;
				}
			}
		}
	}

	public void SetBiome(ManualBiomeType biomeType, TerrainCoord centreTile, float radius, float border, float flowersPerlinScale, float grassPerlinScale, float dts)
	{
		List<PropPrototype> typesWithClass = PropPrototype.GetTypesWithClass(typeof(Bush));
		List<PropPrototype> typesWithCategory = PropPrototype.GetTypesWithCategory("Nature/Flowers/Meadow");
		List<PropPrototype> typesWithCategory2 = PropPrototype.GetTypesWithCategory("Nature/Flowers/Forest");
		TerrainTex terrainTex = TerrainTex.Count;
		switch (biomeType)
		{
		case ManualBiomeType.Meadow:
			terrainTex = TerrainTex.Grass;
			break;
		case ManualBiomeType.ConiferForest:
			terrainTex = TerrainTex.ConiferFloor;
			break;
		case ManualBiomeType.PineForest:
			terrainTex = TerrainTex.ConiferFloor;
			break;
		case ManualBiomeType.DeciduousForest:
			terrainTex = TerrainTex.DeciduousFloor;
			break;
		}
		int num = Mathf.CeilToInt(radius + border);
		TerrainCoord tl = centreTile - new TerrainCoord(num, num);
		TerrainCoord br = centreTile + new TerrainCoord(num, num);
		ClampMinMaxTileWithinBounds(ref tl, ref br);
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				TerrainCoord tile = new TerrainCoord(i, j);
				TerrainType tileTerrainType = GetTileTerrainType(tile);
				if (biomeType != ManualBiomeType.Clear && (tileTerrainType == TerrainType.Grass || tileTerrainType == TerrainType.ForestFloor) && terrainTex != TerrainTex.Count)
				{
					float magnitude = (GetTileCentreXZ(tile) - GetTileCentreXZ(centreTile)).magnitude;
					SetTextureWeight(i, j, terrainTex, (1f - Mathf.Clamp01((magnitude - radius) / border)) * dts);
				}
				if (tile.GetDist(centreTile) > radius)
				{
					continue;
				}
				TileObject fixedObjectOnTile = GetFixedObjectOnTile(i, j);
				if (fixedObjectOnTile != null && !(fixedObjectOnTile is TreeProp) && !(fixedObjectOnTile is Flower) && !(fixedObjectOnTile is Bush))
				{
					continue;
				}
				if (tileTerrainType == TerrainType.Grass || tileTerrainType == TerrainType.ForestFloor || biomeType == ManualBiomeType.Clear)
				{
					int num2 = i | (j << 10);
					switch (biomeType)
					{
					case ManualBiomeType.ConiferForest:
					case ManualBiomeType.PineForest:
					case ManualBiomeType.DeciduousForest:
					{
						float num3 = 1f / flowersPerlinScale;
						if (MathUtil.RandomInt(num2 + 987323, 16) == 0)
						{
							TreeType treeType = TreeType.Count;
							switch (biomeType)
							{
							case ManualBiomeType.DeciduousForest:
								treeType = (TreeType)MathUtil.RandomInt(num2 + 2348397, 11, 18);
								break;
							case ManualBiomeType.ConiferForest:
								treeType = (TreeType)MathUtil.RandomInt(num2 + 523435, 0, 5);
								break;
							case ManualBiomeType.PineForest:
								treeType = (TreeType)MathUtil.RandomInt(num2 + 10874356, 8, 11);
								break;
							}
							if (treeType != TreeType.Count && (!(fixedObjectOnTile is TreeProp treeProp) || treeProp.TreeType != treeType))
							{
								fixedObjectOnTile?.Delete();
								CustomRandom rand = new CustomRandom(num2 + 36478);
								TreeProp.Spawn(treeType, tile, 1f, rand);
							}
						}
						else if (Mathf.PerlinNoise((float)i * num3 + 62934.348f, (float)j * num3 + 10345.094f) >= 0.5f && MathUtil.RandomInt(num2 + 39723, 2) == 0)
						{
							PropPrototype propPrototype = typesWithClass[MathUtil.RandomInt(num2, typesWithClass.Count)];
							if (!(fixedObjectOnTile is Bush bush) || bush.GetPropPrototype() != propPrototype)
							{
								fixedObjectOnTile?.Delete();
								TileObject.SpawnProp(propPrototype, tile, Prop.OrientationType.Deg0);
							}
						}
						else if (Mathf.PerlinNoise((float)i * num3 + 123.456f, (float)j * num3 + 987.654f) >= 0.65f)
						{
							int index = MathUtil.Clamp(Mathf.FloorToInt(Mathf.PerlinNoise((float)i * num3 + 862.0345f, (float)j * num3 + 2104.193f) * (float)typesWithCategory2.Count), 0, typesWithCategory2.Count - 1);
							PropPrototype propPrototype2 = typesWithCategory2[index];
							if (!(fixedObjectOnTile is Flower flower) || flower.GetPropPrototype() != propPrototype2)
							{
								fixedObjectOnTile?.Delete();
								TileObject.SpawnProp(propPrototype2, tile, Prop.OrientationType.Deg0);
							}
						}
						break;
					}
					case ManualBiomeType.Meadow:
					{
						float num4 = 1f / flowersPerlinScale;
						if (Mathf.PerlinNoise((float)i * num4 + 854.753f, (float)j * num4 + 8943.936f) >= 0.75f)
						{
							int index2 = MathUtil.Clamp(Mathf.FloorToInt(Mathf.PerlinNoise((float)i * num4 + 325.1325f, (float)j * num4 + 7553.5415f) * (float)typesWithCategory.Count), 0, typesWithCategory.Count - 1);
							PropPrototype propPrototype3 = typesWithCategory[index2];
							if (!(fixedObjectOnTile is Flower flower2) || flower2.GetPropPrototype() != propPrototype3)
							{
								fixedObjectOnTile?.Delete();
								TileObject.SpawnProp(propPrototype3, tile, Prop.OrientationType.Deg0);
							}
						}
						break;
					}
					case ManualBiomeType.Clear:
						fixedObjectOnTile?.Delete();
						break;
					}
				}
				GrassType grassType = GrassType.Grass1;
				while ((int)grassType < 8)
				{
					float num5 = ((biomeType == ManualBiomeType.Meadow) ? GetTerrainTypeGrassFactor(tileTerrainType) : 0f);
					float num6 = GrassRenderer.GetMaxDensityForGrassType(grassType);
					float num7 = 1f / grassPerlinScale;
					float num8 = Mathf.PerlinNoise((float)i * num7 + 2563.0344f + (float)(int)grassType * 3434.8967f, (float)j * num7 + 1073.543f + (float)(int)grassType * 10239.649f);
					num8 = Mathf.Clamp01((num8 - 0.25f) * 1.5f);
					if ((int)grassType >= 3)
					{
						num8 = Mathf.Clamp01((num8 - 0.5f) * 2f);
					}
					GrassMap.SetGrassAmount(i, j, grassType, Mathf.CeilToInt(num8 * num5 * num6));
					grassType++;
				}
			}
		}
		GrassMap.ApplyGrassChangesIfNeeded(tl, br);
		using (new StopWatchMarker("Apply Textures"))
		{
			ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, tl, br);
		}
		using (new StopWatchMarker("Build Minimap"))
		{
			BuildMinimap(tl, br);
		}
	}

	private Vector2 GetTileSlideVector(TerrainCoord tile)
	{
		tile = ClampTileWithinBounds(tile);
		float num = Vertices[tile.x, tile.y];
		float num2 = Vertices[tile.x, tile.y + 1];
		float num3 = Vertices[tile.x + 1, tile.y + 1];
		float num4 = Vertices[tile.x + 1, tile.y];
		float num5 = Math.Max(num, Math.Max(num2, Math.Max(num3, num4)));
		return new Vector2(-0.5f, -0.5f) * (num5 - num) + new Vector2(-0.5f, 0.5f) * (num5 - num2) + new Vector2(0.5f, 0.5f) * (num5 - num3) + new Vector2(0.5f, -0.5f) * (num5 - num4);
	}

	private void OnHeightChanged(TerrainCoord tl, TerrainCoord br)
	{
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				float num = Vertices[i, j];
				float num2 = Vertices[i, j + 1];
				float num3 = Vertices[i + 1, j + 1];
				float num4 = Vertices[i + 1, j];
				SetFlag(i, j, TileFlags.TriFlipped, Math.Abs(num - num3) <= Math.Abs(num2 - num4));
				float num5 = MathUtil.Max(num, num2, num3, num4);
				float num6 = MathUtil.Min(num, num2, num3, num4);
				bool flag = num5 - num6 > 0.75f;
				bool flag2 = num5 - num6 > 1.25f;
				SetFlag(i, j, TileFlags.Impassable, on: false);
				SetFlag(i, j, TileFlags.Slope, flag);
				TextureWeights[i, j, 5] = (byte)((flag && !flag2) ? 255u : 0u);
				TextureWeights[i, j, 6] = (byte)((flag && flag2) ? 255u : 0u);
				if (flag)
				{
					for (int k = 0; k < 8; k++)
					{
						if (k != 5 && k != 6)
						{
							TextureWeights[i, j, k] = 0;
						}
					}
				}
				else
				{
					int num7 = 0;
					for (int l = 0; l < 8; l++)
					{
						num7 += TextureWeights[i, j, l];
					}
					if (num7 < 255)
					{
						TextureWeights[i, j, 0] += (byte)(255 - num7);
					}
				}
				if (AStar.VerticesOnThread != null)
				{
					AStar.VerticesOnThread[i, j] = Vertices[i, j];
				}
			}
		}
		RebuildPathsInRect(tl, br);
		RecalcRiverTiles(tl, br);
		TellObjectsThatHeightChanged(tl, br);
		if (UnityTerrain != null)
		{
			ApplyHeightsToUnityTerrainData(UnityTerrain.terrainData, tl, br + new TerrainCoord(1, 1));
			ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, tl, br);
			UnityTerrain.Flush();
			BuildMinimap(tl, br);
		}
	}

	public void TellObjectsThatHeightChanged(TerrainCoord tl, TerrainCoord br)
	{
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		for (int i = tl.x / 8; i <= br.x / 8; i++)
		{
			for (int j = tl.y / 8; j <= br.y / 8; j++)
			{
				List<TileObject> objects = LookupSquares[i, j].Objects;
				if (objects != null)
				{
					for (int k = 0; k < objects.Count; k++)
					{
						objects[k].OnTerrainHeightChanged();
					}
				}
			}
		}
	}

	public static float GetTerrainTypeGrassFactor(TerrainType terrainType)
	{
		return terrainType switch
		{
			TerrainType.Grass => 1f, 
			TerrainType.ForestFloor => 0.5f, 
			TerrainType.Mud => 1f, 
			_ => 0f, 
		};
	}

	public static bool CanPlantCropsOnTerrainType(TerrainType terrainType)
	{
		if (terrainType == TerrainType.Grass || (uint)(terrainType - 4) <= 2u)
		{
			return true;
		}
		return false;
	}

	public void AddOrRemoveGrass(GrassType grassType, Vector2 posXZ, float radius, float amount, bool remove, bool apply)
	{
		TerrainCoord tl = GetTileCoordForPosXZ(posXZ - new Vector2(radius, radius));
		TerrainCoord br = GetTileCoordForPosXZ(posXZ + new Vector2(radius, radius));
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				float terrainTypeGrassFactor = GetTerrainTypeGrassFactor(GetTileTerrainType(new TerrainCoord(i, j)));
				if (!(terrainTypeGrassFactor > 0f || remove))
				{
					continue;
				}
				float magnitude = (GetTileCentreXZ(new TerrainCoord(i, j)) - posXZ).magnitude;
				if (magnitude <= radius)
				{
					if (remove)
					{
						float num = magnitude / radius;
						float val = amount * terrainTypeGrassFactor * num;
						float num2 = Math.Min(GrassMap.GetGrassAmount(i, j, grassType), val);
						GrassMap.SetGrassAmount(i, j, grassType, (int)num2);
					}
					else
					{
						float num3 = 1f - magnitude / radius;
						float val2 = amount * terrainTypeGrassFactor * num3;
						float num4 = Math.Max(GrassMap.GetGrassAmount(i, j, grassType), val2);
						GrassMap.SetGrassAmount(i, j, grassType, (int)num4);
					}
				}
			}
		}
		if (UnityTerrain != null)
		{
			GrassMap.ApplyGrassChangesIfNeeded(tl, br);
		}
	}

	public void ClearAllGrassInRect(TerrainCoord tl, TerrainCoord br)
	{
		if (!ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			return;
		}
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					GrassMap.SetGrassAmount(i, j, (GrassType)k, 0);
				}
			}
		}
		if (UnityTerrain != null)
		{
			GrassMap.ApplyGrassChangesIfNeeded(tl, br);
		}
	}

	public void AddDestroyedPatch(TerrainCoord minTile, TerrainCoord maxTile)
	{
		AddModifiedPatch(new ModifiedPatch
		{
			Type = TerrainModificationType.Debris,
			MinTile = minTile,
			MaxTile = maxTile
		});
	}

	public int AddModifiedPatch(ModifiedPatch patch)
	{
		if (!Generating && !Session.Instance.Editor)
		{
			patch.ModifiedPatchId = NextModifiedPatchId++;
			ModifiedPatches.Add(patch);
		}
		ApplyModifiedPatch(patch, new TerrainRect(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1)), applyTextureWeightsAndGrass: true);
		return patch.ModifiedPatchId;
	}

	public void RemoveModifiedPatch(int id)
	{
		for (int i = 0; i < ModifiedPatches.Count; i++)
		{
			if (ModifiedPatches[i].ModifiedPatchId != id)
			{
				continue;
			}
			ModifiedPatch modifiedPatch = ModifiedPatches[i];
			ModifiedPatches.RemoveAt(i);
			TerrainRect bounds = new TerrainRect(modifiedPatch.MinTile, modifiedPatch.MaxTile).Expand(FlattenPatchExtra);
			if (!ClampMinMaxTileWithinBounds(ref bounds.min, ref bounds.max))
			{
				break;
			}
			for (int j = bounds.min.x; j <= bounds.max.x + 1; j++)
			{
				for (int k = bounds.min.y; k <= bounds.max.y + 1; k++)
				{
					Vertices[j, k] = OriginalVertices[j, k];
				}
			}
			if (UnityTerrain != null)
			{
				ApplyHeightsToUnityTerrainData(UnityTerrain.terrainData, bounds.min, bounds.max + new TerrainCoord(1, 1));
			}
			TellObjectsThatHeightChanged(bounds.min, bounds.max);
			for (int l = bounds.min.x; l <= bounds.max.x; l++)
			{
				for (int m = bounds.min.y; m <= bounds.max.y; m++)
				{
					SetFlag2(l, m, TileFlags2.Pit, on: false);
				}
			}
			for (int n = 0; n < ModifiedPatches.Count; n++)
			{
				ApplyModifiedPatch(ModifiedPatches[n], bounds, applyTextureWeightsAndGrass: false);
			}
			float[,] array = new float[bounds.TilesSizeX + 1, bounds.TilesSizeY + 1];
			for (int num = bounds.min.x; num <= bounds.max.x + 1; num++)
			{
				for (int num2 = bounds.min.y; num2 <= bounds.max.y + 1; num2++)
				{
					array[num - bounds.min.x, num2 - bounds.min.y] = Vertices[num, num2];
				}
			}
			AStar.AddChange(AStarChange.TerrainHeightChange(bounds.min, bounds.max, array));
			break;
		}
	}

	private void ApplyModifiedPatch(ModifiedPatch patch, TerrainRect bounds, bool applyTextureWeightsAndGrass)
	{
		TerrainRect result;
		switch (patch.Type)
		{
		case TerrainModificationType.Debris:
		{
			if (!applyTextureWeightsAndGrass || !bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile), out result))
			{
				break;
			}
			for (int num6 = result.min.x; num6 <= result.max.x; num6++)
			{
				for (int num7 = result.min.y; num7 <= result.max.y; num7++)
				{
					if (MathUtil.RandomInt(patch.MinTile.x | (patch.MinTile.y << 16), 8) != 0 && !IsTileOutsideBounds(num6, num7))
					{
						for (int num8 = 0; num8 < 8; num8++)
						{
							TextureWeights[num6, num7, num8] = (byte)((num8 == 7) ? 255u : 0u);
						}
					}
				}
			}
			ClearAllGrassInRect(result.min, result.max);
			if (UnityTerrainObj != null)
			{
				using (new StopWatchMarker("Apply Debris Textures"))
				{
					ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, result.min, result.max);
				}
				using (new StopWatchMarker("Build Minimap"))
				{
					BuildMinimap(result.min - new TerrainCoord(1, 1), result.max + new TerrainCoord(1, 1));
					break;
				}
			}
			break;
		}
		case TerrainModificationType.Flatten:
		{
			if (!bounds.Overlaps(new TerrainRect(patch.MinTile, patch.MaxTile).Expand(1)))
			{
				break;
			}
			ApplyFlattenPatch(bounds, patch.MinTile, patch.MaxTile, patch.FlattenHeight, patch.FlattenBorder, patch.BuildingId, Vertices, onThread: false);
			bool flag = bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile), out result);
			if (flag && applyTextureWeightsAndGrass)
			{
				Vector2 vector4 = Vector2.Lerp(GetVertexPosXZ(patch.MinTile.x, patch.MinTile.y), GetVertexPosXZ(patch.MaxTile.x + 1, patch.MaxTile.y + 1), 0.5f);
				for (int num4 = result.min.x; num4 <= result.max.x; num4++)
				{
					for (int num5 = result.min.y; num5 <= result.max.y; num5++)
					{
						if (!IsTileOutsideBounds(num4, num5))
						{
							Vector2 tileCentreXZ2 = GetTileCentreXZ(new TerrainCoord(num4, num5));
							Vector2 vector5 = tileCentreXZ2 - vector4;
							Vector2 vector6 = ((!(Math.Abs(vector5.x) > Math.Abs(vector5.y))) ? ((vector5.y < 0f) ? GetVertexPosXZ(num4, patch.MinTile.y) : GetVertexPosXZ(num4, patch.MaxTile.y + 1)) : ((vector5.x < 0f) ? GetVertexPosXZ(patch.MinTile.x, num5) : GetVertexPosXZ(patch.MaxTile.x + 1, num5)));
							float amount2 = Mathf.Clamp01((tileCentreXZ2 - vector6).magnitude / patch.FlattenBorder);
							SetTextureWeight(num4, num5, IsTileRoad(num4, num5) ? TerrainTex.Debris : TerrainTex.Mud, amount2);
						}
					}
				}
			}
			if (flag && applyTextureWeightsAndGrass)
			{
				ClearAllGrassInRect(result.min, result.max);
			}
			if (flag)
			{
				TellObjectsThatHeightChanged(result.min, result.max);
			}
			if (!Session.Instance.Editor)
			{
				AStar.AddChange(AStarChange.FlattenTerrain(bounds, patch.MinTile, patch.MaxTile, patch.FlattenHeight, patch.FlattenBorder, patch.BuildingId, patch.Type));
			}
			if (UnityTerrainObj != null && flag)
			{
				ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, result.min, result.max);
				ApplyHeightsToUnityTerrainData(UnityTerrain.terrainData, result.min, result.max + new TerrainCoord(1, 1));
				UnityTerrain.Flush();
				BuildMinimap(result.min - new TerrainCoord(1, 1), result.max + new TerrainCoord(1, 1));
			}
			break;
		}
		case TerrainModificationType.PitTrap:
		{
			if (!bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile).Expand(FlattenPatchExtra), out result))
			{
				break;
			}
			ApplyDitch(bounds, patch.MinTile, patch.MaxTile, patch.DitchHeight, Vertices);
			TerrainRect result2 = TerrainRect.Invalid;
			if (bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile).Expand(1), out result2))
			{
				for (int k = result2.min.x; k <= result2.max.x; k++)
				{
					for (int l = result2.min.y; l <= result2.max.y; l++)
					{
						if (!IsTileOutsideBounds(k, l))
						{
							SetFlag2(k, l, TileFlags2.Pit, on: true);
						}
					}
				}
			}
			if (applyTextureWeightsAndGrass)
			{
				for (int m = result.min.x; m <= result.max.x; m++)
				{
					for (int n = result.min.y; n <= result.max.y; n++)
					{
						if (!IsTileOutsideBounds(m, n))
						{
							SetTextureWeight(m, n, TerrainTex.Mud, 1f);
						}
					}
				}
				ClearAllGrassInRect(result2.min, result2.max);
			}
			TellObjectsThatHeightChanged(result2.min, result2.max);
			if (!Session.Instance.Editor)
			{
				AStar.AddChange(AStarChange.AddDitch(bounds, patch.MinTile, patch.MaxTile, patch.DitchHeight, patch.Type));
			}
			if (UnityTerrainObj != null)
			{
				ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, result.min, result.max);
				ApplyHeightsToUnityTerrainData(UnityTerrain.terrainData, patch.MinTile, patch.MaxTile + new TerrainCoord(1, 1));
				UnityTerrain.Flush();
				BuildMinimap(result.min, result.max);
			}
			break;
		}
		case TerrainModificationType.ApplyTexture:
		{
			if (!applyTextureWeightsAndGrass || !bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile), out result))
			{
				break;
			}
			Vector2 vector = Vector2.Lerp(GetVertexPosXZ(patch.MinTile.x, patch.MinTile.y), GetVertexPosXZ(patch.MaxTile.x + 1, patch.MaxTile.y + 1), 0.5f);
			for (int num2 = result.min.x; num2 <= result.max.x; num2++)
			{
				for (int num3 = result.min.y; num3 <= result.max.y; num3++)
				{
					if (!IsTileOutsideBounds(num2, num3))
					{
						Vector2 tileCentreXZ = GetTileCentreXZ(new TerrainCoord(num2, num3));
						Vector2 vector2 = tileCentreXZ - vector;
						Vector2 vector3 = ((!(Math.Abs(vector2.x) > Math.Abs(vector2.y))) ? ((vector2.y < 0f) ? GetVertexPosXZ(num2, patch.MinTile.y) : GetVertexPosXZ(num2, patch.MaxTile.y + 1)) : ((vector2.x < 0f) ? GetVertexPosXZ(patch.MinTile.x, num3) : GetVertexPosXZ(patch.MaxTile.x + 1, num3)));
						float amount = Mathf.Clamp01((tileCentreXZ - vector3).magnitude / patch.FlattenBorder);
						SetTextureWeight(num2, num3, patch.Tex, amount);
					}
				}
			}
			if (UnityTerrain != null)
			{
				ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, result.min, result.max);
			}
			break;
		}
		case TerrainModificationType.ApplyGrass:
		{
			if (!applyTextureWeightsAndGrass || !bounds.Intersect(new TerrainRect(patch.MinTile, patch.MaxTile), out result))
			{
				break;
			}
			float num = GrassRenderer.GetMaxDensityForGrassType(patch.GrassType);
			for (int i = result.min.x; i <= result.max.x; i++)
			{
				for (int j = result.min.y; j <= result.max.y; j++)
				{
					if (!IsTileOutsideBounds(i, j))
					{
						float f = num * MathUtil.RandomFloat(i * 69 + j * 42);
						GrassMap.SetGrassAmount(i, j, patch.GrassType, Mathf.CeilToInt(f));
					}
				}
			}
			if (UnityTerrain != null)
			{
				GrassMap.ApplyGrassChangesIfNeeded(result.min, result.max);
			}
			break;
		}
		}
	}

	public void ApplyFlattenPatch(TerrainRect bounds, TerrainCoord minTile, TerrainCoord maxTile, float flattenHeight, float flattenBorder, int buildingId, float[,] vertices, bool onThread)
	{
		Vector2 vertexPosXZ = GetVertexPosXZ(minTile.x, minTile.y);
		Vector2 vertexPosXZ2 = GetVertexPosXZ(maxTile.x + 1, maxTile.y + 1);
		Vector2 vector = Vector2.Lerp(vertexPosXZ, vertexPosXZ2, 0.5f);
		Vector2 vector2 = (vertexPosXZ2 - vertexPosXZ) * 0.5f;
		for (int i = minTile.x; i <= maxTile.x + 1; i++)
		{
			for (int j = minTile.y; j <= maxTile.y + 1; j++)
			{
				if (IsVertexOutsideTileRect(i, j, bounds) || IsVertexPit(i, j))
				{
					continue;
				}
				Vector2 vertexPosXZ3 = GetVertexPosXZ(i, j);
				Vector2 vector3 = (vertexPosXZ3 - vector) / vector2;
				Vector2 vector4 = ((!(Math.Abs(vector3.x) > Math.Abs(vector3.y))) ? ((vector3.y < 0f) ? GetVertexPosXZ(i, minTile.y) : GetVertexPosXZ(i, maxTile.y + 1)) : ((vector3.x < 0f) ? GetVertexPosXZ(minTile.x, j) : GetVertexPosXZ(maxTile.x + 1, j)));
				float num = ((flattenBorder > 0f) ? Mathf.Clamp01((vertexPosXZ3 - vector4).magnitude / flattenBorder) : 1f);
				float num2 = Mathf.Lerp(vertices[i, j], flattenHeight, num);
				if (!(num < 1f) || !IsBuildingThatFlattensTerrainOnVertex(new TerrainCoord(i, j), buildingId, onThread))
				{
					if (!Generating && !Session.Instance.Editor && IsVertexRoadOrRiver(i, j))
					{
						vertices[i, j] = Math.Max(OriginalVertices[i, j], num2);
					}
					else
					{
						vertices[i, j] = num2;
					}
				}
			}
		}
	}

	public void ApplyDitch(TerrainRect bounds, TerrainCoord minTile, TerrainCoord maxTile, float ditchHeight, float[,] vertices)
	{
		for (int i = minTile.x; i <= maxTile.x + 1; i++)
		{
			for (int j = minTile.y; j <= maxTile.y + 1; j++)
			{
				if (!IsVertexOutsideTileRect(i, j, bounds))
				{
					vertices[i, j] = OriginalVertices[i, j] - ditchHeight;
				}
			}
		}
	}

	public void SetDebris(TerrainCoord minTile, TerrainCoord maxTile)
	{
		AddDestroyedPatch(minTile, maxTile);
		LastChangedTime = Session.Instance.PlayTime;
	}

	public void AddTreeProp(TreeProp treeProp)
	{
		TreeProps.Add(treeProp);
		if (UnityTerrain != null)
		{
			ApplyTreesToUnityTerrainData(UnityTerrain.terrainData);
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = false;
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = true;
			UnityTerrain.Flush();
		}
	}

	public void RemoveTreeProp(TreeProp treeProp)
	{
		TreeProps.Remove(treeProp);
		if (UnityTerrain != null)
		{
			ApplyTreesToUnityTerrainData(UnityTerrain.terrainData);
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = false;
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = true;
			UnityTerrain.Flush();
		}
	}

	public void RebuildTreeProps()
	{
		if (UnityTerrain != null)
		{
			ApplyTreesToUnityTerrainData(UnityTerrain.terrainData);
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = false;
			UnityTerrainObj.GetComponent<TerrainCollider>().enabled = true;
			UnityTerrain.Flush();
		}
	}

	public float? RayCastAgainstTile(TerrainCoord tile, Ray ray, float length, out Vector3 normal, bool aStar)
	{
		normal = -ray.direction;
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return null;
		}
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		Vector3 vector4;
		if (aStar)
		{
			vector = GetAStarVertexPos(tile.x, tile.y);
			vector2 = GetAStarVertexPos(tile.x, tile.y + 1);
			vector3 = GetAStarVertexPos(tile.x + 1, tile.y + 1);
			vector4 = GetAStarVertexPos(tile.x + 1, tile.y);
		}
		else if (IsTileTrap(tile.x, tile.y))
		{
			vector = GetOriginalVertexPos(tile.x, tile.y);
			vector2 = GetOriginalVertexPos(tile.x, tile.y + 1);
			vector3 = GetOriginalVertexPos(tile.x + 1, tile.y + 1);
			vector4 = GetOriginalVertexPos(tile.x + 1, tile.y);
		}
		else
		{
			vector = GetVertexPos(tile.x, tile.y);
			vector2 = GetVertexPos(tile.x, tile.y + 1);
			vector3 = GetVertexPos(tile.x + 1, tile.y + 1);
			vector4 = GetVertexPos(tile.x + 1, tile.y);
		}
		float num = length;
		float? num2;
		Vector3 normal2;
		float? num3;
		Vector3 normal3;
		if (!IsTriFlipped(tile.x, tile.y))
		{
			num2 = RayCastAgainstTriangle(vector, vector2, vector3, ray, out normal2);
			num3 = RayCastAgainstTriangle(vector3, vector4, vector, ray, out normal3);
		}
		else
		{
			num2 = RayCastAgainstTriangle(vector, vector2, vector4, ray, out normal2);
			num3 = RayCastAgainstTriangle(vector3, vector4, vector2, ray, out normal3);
		}
		if (num2.HasValue && num2.Value < num)
		{
			num = num2.Value;
			normal = normal2;
		}
		if (num3.HasValue && num3.Value < num)
		{
			num = num3.Value;
			normal = normal3;
		}
		if (num < length)
		{
			return num;
		}
		return null;
	}

	private static float? RayCastAgainstTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Ray ray, out Vector3 normal)
	{
		Plane plane = new Plane(p0, p1, p2);
		normal = plane.normal;
		if (!plane.Raycast(ray, out var enter))
		{
			return null;
		}
		Vector3 vector = ray.origin + enter * ray.direction;
		float num = (p1.z - p2.z) * (p0.x - p2.x) + (p2.x - p1.x) * (p0.z - p2.z);
		float num2 = ((p1.z - p2.z) * (vector.x - p2.x) + (p2.x - p1.x) * (vector.z - p2.z)) / num;
		float num3 = ((p2.z - p0.z) * (vector.x - p2.x) + (p0.x - p2.x) * (vector.z - p2.z)) / num;
		float num4 = 1f - num2 - num3;
		if (-0.01f <= num2 && num2 <= 1.01f && -0.01f <= num3 && num3 <= 1.01f && -0.01f <= num4 && num4 <= 1.01f)
		{
			return enter;
		}
		return null;
	}

	public RaycastResult RayCast(Vector3 start, Vector3 end, int flags)
	{
		return RayCast(start, end, flags, null, null, null, predicted: false);
	}

	public RaycastResult RayCast(Vector3 start, Vector3 end, int flags, TileObject ignore)
	{
		return RayCast(start, end, flags, ignore, null, null, predicted: false);
	}

	public RaycastResult RayCast(Vector3 start, Vector3 end, int flags, TileObject ignore, Character source, TileObject target, bool predicted)
	{
		Vector3 direction = end - start;
		float magnitude = direction.magnitude;
		if (magnitude > 0.0001f)
		{
			direction /= magnitude;
			return RayCast(new Ray(start, direction), magnitude, flags, ignore, source, target, predicted);
		}
		return default(RaycastResult);
	}

	public RaycastResult RayCast(Ray ray, float length, int flags)
	{
		return RayCast(ray, length, flags, null, null, null, predicted: false);
	}

	public RaycastResult RayCast(Ray ray, float length, int flags, TileObject ignore)
	{
		return RayCast(ray, length, flags, ignore, null, null, predicted: false);
	}

	public RaycastResult RayCast(Ray ray, float length, int flags, TileObject ignore, Character source, TileObject target, bool predicted)
	{
		using (new UnityProfileMarker(RayCastStr))
		{
			if (Util.AmIOnMainThread())
			{
				RaycastTracer.Predicted = predicted;
				RaycastTracer.Flags = flags;
				RaycastTracer.Ignore = ignore;
				RaycastTracer.Source = source;
				RaycastTracer.Target = target;
				RaycastTracer.TraceLine(ray, length);
				RaycastTracer.CleanUp();
				return RaycastTracer.Result;
			}
			RaycastTracerForHandleInputThread.Predicted = predicted;
			RaycastTracerForHandleInputThread.Flags = flags;
			RaycastTracerForHandleInputThread.Ignore = ignore;
			RaycastTracerForHandleInputThread.Source = source;
			RaycastTracerForHandleInputThread.Target = target;
			RaycastTracerForHandleInputThread.TraceLine(ray, length);
			RaycastTracerForHandleInputThread.CleanUp();
			return RaycastTracerForHandleInputThread.Result;
		}
	}

	public RaycastResult RayCastFromAStarThread(Ray ray, float length, int flags, TileObject ignore, Character source, TileObject target)
	{
		using (new UnityProfileMarker(RayCastFromAStarThreadStr))
		{
			RaycastTracerForAStarThread.Flags = flags;
			RaycastTracerForAStarThread.Ignore = ignore;
			RaycastTracerForAStarThread.Source = source;
			RaycastTracerForAStarThread.Target = target;
			RaycastTracerForAStarThread.TraceLine(ray, length);
			RaycastTracerForAStarThread.CleanUp();
			return RaycastTracerForAStarThread.Result;
		}
	}

	public RaycastResult RayCastFromVisibleTilesThread(Ray ray, float length, int flags, TileObject ignore, Character source)
	{
		using (new UnityProfileMarker(RayCastFromVisibleTilesThreadStr))
		{
			lock (RaycastTracerForVisibleTilesThread)
			{
				RaycastTracerForVisibleTilesThread.Flags = flags;
				RaycastTracerForVisibleTilesThread.Ignore = ignore;
				RaycastTracerForVisibleTilesThread.Source = source;
				RaycastTracerForVisibleTilesThread.TraceLine(ray, length);
				RaycastTracerForVisibleTilesThread.CleanUp();
				return RaycastTracerForVisibleTilesThread.Result;
			}
		}
	}

	public float GetMaxHeight(Ray ray, float length)
	{
		MaxHeightTracer.TraceLine(ray, length);
		if (!MaxHeightTracer.InBounds)
		{
			return 0f;
		}
		return MaxHeightTracer.Result;
	}

	public bool IsPassable(Ray ray, float length, int options, Character requester, TileObject ignore, bool predicted)
	{
		return IsPassable(ray, length, options, requester, ignore, TerrainCoord.Invalid, predicted);
	}

	public bool IsPassable(Ray ray, float length, int options, Character requester, TileObject ignore, TerrainCoord ignoreTile, bool predicted)
	{
		IsPassableTracer.Predicted = predicted;
		IsPassableTracer.Options = options;
		IsPassableTracer.Requester = requester;
		IsPassableTracer.Ignore = ignore;
		IsPassableTracer.IgnoreTile = ignoreTile;
		IsPassableTracer.TraceLine(ray, length);
		return IsPassableTracer.Passable;
	}

	public Vector3 TraceCollisions(Ray ray, float length, int options, Character requester, TileObject ignore, out TileObject hitObject)
	{
		CollisionTracer.Options = options;
		CollisionTracer.Requester = requester;
		CollisionTracer.Ignore = ignore;
		CollisionTracer.TraceLine(ray, length);
		hitObject = CollisionTracer.HitObject;
		return CollisionTracer.Result;
	}

	public TileObject FindImpassableObject(Ray ray, float length, TileObject ignore)
	{
		FindImpassableObjectTracer.Ignore = ignore;
		FindImpassableObjectTracer.TraceLine(ray, length);
		return FindImpassableObjectTracer.FoundObject;
	}

	public float GetMaxHeight(TerrainCoord minTile, TerrainCoord maxTile)
	{
		float num = float.MinValue;
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				num = Math.Max(num, GetTileMaxHeight(i, j));
			}
		}
		return num;
	}

	public void FillImpassable(CustomRandom rand)
	{
		TerrainCoord item = new TerrainCoord(Size / 2, Size / 2);
		int num = 0;
		while (IsSlopeOrImpassableRaw(item.x, item.y))
		{
			item = rand.RandomTile(new TerrainCoord(Size / 4, Size / 4), new TerrainCoord(3 * (Size / 4), 3 * (Size / 4)));
			num++;
			if (num >= 1000)
			{
				return;
			}
		}
		bool[,] array = new bool[Size, Size];
		int val = 0;
		List<TerrainCoord> list = new List<TerrainCoord>();
		using (new StopWatchMarker("FloodFill Passable Tiles"))
		{
			list.Clear();
			list.Add(item);
			while (list.Count > 0)
			{
				val = Math.Max(val, list.Count);
				TerrainCoord terrainCoord = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				if (array[terrainCoord.x, terrainCoord.y])
				{
					continue;
				}
				TerrainCoord terrainCoord2 = terrainCoord;
				TerrainCoord terrainCoord3 = terrainCoord;
				while (terrainCoord2.x > 0 && !IsSlopeOrImpassableRaw(terrainCoord2.x - 1, terrainCoord2.y))
				{
					terrainCoord2.x--;
				}
				while (terrainCoord3.x < Size - 1 && !IsSlopeOrImpassableRaw(terrainCoord3.x + 1, terrainCoord3.y))
				{
					terrainCoord3.x++;
				}
				for (int i = terrainCoord2.x; i <= terrainCoord3.x; i++)
				{
					array[i, terrainCoord.y] = true;
					if (terrainCoord.y > 0 && !IsSlopeOrImpassableRaw(i, terrainCoord.y - 1) && !array[i, terrainCoord.y - 1])
					{
						list.Add(new TerrainCoord(i, terrainCoord.y - 1));
					}
					if (terrainCoord.y < Size - 1 && !IsSlopeOrImpassableRaw(i, terrainCoord.y + 1) && !array[i, terrainCoord.y + 1])
					{
						list.Add(new TerrainCoord(i, terrainCoord.y + 1));
					}
				}
			}
		}
		using (new StopWatchMarker("Loop Through All Tiles"))
		{
			for (int j = 0; j < Size; j++)
			{
				for (int k = 0; k < Size; k++)
				{
					SetFlag(j, k, TileFlags.Impassable, !array[j, k]);
				}
			}
		}
		UnityEngine.Debug.Log("maxStackSize: " + val);
	}

	public bool IsTileSpawnable(int x, int y)
	{
		return (Tiles[x, y] & 8) != 0;
	}

	public bool IsTileEnclosed(int x, int y)
	{
		return (Tiles[x, y] & 0x10) != 0;
	}

	public bool IsTileEnclosed(TerrainCoord tile)
	{
		return (Tiles[tile.x, tile.y] & 0x10) != 0;
	}

	public bool IsTileBuiltOn(int x, int y)
	{
		return (Tiles[x, y] & 4) != 0;
	}

	public bool IsTileEnclosedOrBuiltOn(TerrainCoord tile)
	{
		return (Tiles[tile.x, tile.y] & 0x14) != 0;
	}

	public bool IsTileSpawnableOrEnclosed(TerrainCoord tile)
	{
		return (Tiles[tile.x, tile.y] & 0x18) != 0;
	}

	public bool IsTileRiver(int x, int y)
	{
		return (Tiles[x, y] & 0x20) != 0;
	}

	public bool IsTileRoad(int x, int y)
	{
		return (Tiles2[x, y] & 2) != 0;
	}

	public bool IsTileTrap(int x, int y)
	{
		return (Tiles[x, y] & 0x80) != 0;
	}

	public bool IsTilePit(int x, int y)
	{
		return (Tiles2[x, y] & 1) != 0;
	}

	public bool IsTileIllegal(int x, int y)
	{
		return (Tiles2[x, y] & 4) != 0;
	}

	public bool IsTileRoadOrRiver(int x, int y)
	{
		x = MathUtil.Clamp(0, x, Size - 1);
		y = MathUtil.Clamp(0, y, Size - 1);
		if ((Tiles2[x, y] & 2) == 0)
		{
			return (Tiles[x, y] & 0x20) != 0;
		}
		return true;
	}

	public bool IsVertexRoadOrRiver(int x, int y)
	{
		if (x <= 0 || y <= 0 || x >= Size || y >= Size)
		{
			return false;
		}
		if (!IsTileRoadOrRiver(x, y) && !IsTileRoadOrRiver(x - 1, y) && !IsTileRoadOrRiver(x - 1, y - 1))
		{
			return IsTileRoadOrRiver(x, y - 1);
		}
		return true;
	}

	public bool IsVertexRoad(int x, int y)
	{
		if (x <= 0 || y <= 0 || x >= Size || y >= Size)
		{
			return false;
		}
		if (!IsTileRoad(x, y) && !IsTileRoad(x - 1, y) && !IsTileRoad(x - 1, y - 1))
		{
			return IsTileRoad(x, y - 1);
		}
		return true;
	}

	public bool IsVertexRiver(int x, int y)
	{
		if (x <= 0 || y <= 0 || x >= Size || y >= Size)
		{
			return false;
		}
		if (!IsTileRiver(x, y) && !IsTileRiver(x - 1, y) && !IsTileRiver(x - 1, y - 1))
		{
			return IsTileRiver(x, y - 1);
		}
		return true;
	}

	public bool IsVertexPit(int x, int y)
	{
		if (x <= 0 || y <= 0 || x >= Size || y >= Size)
		{
			return false;
		}
		if (!IsTilePit(x, y) && !IsTilePit(x - 1, y) && !IsTilePit(x - 1, y - 1))
		{
			return IsTilePit(x, y - 1);
		}
		return true;
	}

	public bool IsAnySurroundingTileSpawnable(TerrainRect rect)
	{
		for (int i = Math.Max(0, rect.min.x - 1); i <= Math.Min(Size - 1, rect.max.x + 1); i++)
		{
			for (int j = Math.Max(0, rect.min.y - 1); j <= Math.Min(Size - 1, rect.max.y + 1); j++)
			{
				if (IsTileSpawnable(i, j))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsBuildingThatFlattensTerrainOnVertex(TerrainCoord vert, int ignoreId, bool onThread)
	{
		TerrainCoord tl = new TerrainCoord(vert.x - 1, vert.y - 1);
		TerrainCoord br = new TerrainCoord(vert.x, vert.y);
		if (ClampMinMaxTileWithinBounds(ref tl, ref br))
		{
			if (onThread)
			{
				for (int i = tl.x; i <= br.x; i++)
				{
					for (int j = tl.y; j <= br.y; j++)
					{
						List<AStarMoveableObstacle> moveableObstacles = AStar.Tiles[i, j].MoveableObstacles;
						if (moveableObstacles == null)
						{
							continue;
						}
						for (int k = 0; k < moveableObstacles.Count; k++)
						{
							if (moveableObstacles[k].Object.Id != ignoreId)
							{
								PropPrototype propPrototype = moveableObstacles[k].Object.GetPropPrototype();
								if (propPrototype != null && propPrototype.WantFlattenTerrain)
								{
									return true;
								}
							}
						}
					}
				}
			}
			else
			{
				for (int l = tl.x / 8; l <= br.x / 8; l++)
				{
					for (int m = tl.y / 8; m <= br.y / 8; m++)
					{
						List<TileObject> objects = LookupSquares[l, m].Objects;
						if (objects == null)
						{
							continue;
						}
						for (int n = 0; n < objects.Count; n++)
						{
							if (objects[n].Id != ignoreId)
							{
								PropPrototype propPrototype2 = objects[n].GetPropPrototype();
								if (propPrototype2 != null && propPrototype2.WantFlattenTerrain && TerrainCoord.Overlaps(objects[n].GetMinTile(), objects[n].GetMaxTile(), tl, br))
								{
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	public void CopyTilesForThread()
	{
		if (LastCalcEnclosedAreasFrame >= LastEnclosedAreasChangedFrame)
		{
			return;
		}
		LastCalcEnclosedAreasFrame = Session.Instance.Frame;
		EnclosedAreaSeedPoints.Clear();
		using (new StopWatchMarker("CopyTilesForThread"))
		{
			Buffer.BlockCopy(Tiles, 0, TilesThreadCopy, 0, MaxSize * MaxSize);
		}
		foreach (Prop allProp in Session.Instance.PropManager.AllProps)
		{
			if (allProp is Building { InaccessibleToPlayer: false } building)
			{
				for (int i = 0; i < building.GetEntranceDefs().Length; i++)
				{
					TerrainCoord tileCoordForPos = GetTileCoordForPos(building.GetEntrancePos(i));
					if (!IsTileOutsideBounds(tileCoordForPos.x, tileCoordForPos.y))
					{
						EnclosedAreaSeedPoints.Add(tileCoordForPos);
					}
				}
			}
			if (allProp is Gate gate && (!gate.ForceInvulnerable || gate.KeyProto != null || gate.State != GateState.Locked))
			{
				TerrainCoord tileInsideGate = gate.GetTileInsideGate();
				TerrainCoord tileOutsideGate = gate.GetTileOutsideGate();
				if (!IsTileOutsideBounds(tileInsideGate.x, tileInsideGate.y))
				{
					EnclosedAreaSeedPoints.Add(tileInsideGate);
				}
				if (!IsTileOutsideBounds(tileOutsideGate.x, tileOutsideGate.y))
				{
					EnclosedAreaSeedPoints.Add(tileOutsideGate);
				}
			}
		}
		EnclosedAreaSeedPoints.Add(new TerrainCoord(Size / 2, Size / 2));
		WantCalcEnclosedAreasOnThread = true;
	}

	public void CopyTilesFromThread()
	{
		if (!WantCalcEnclosedAreasOnThread)
		{
			return;
		}
		using (new StopWatchMarker("CopyTilesFromThread"))
		{
			byte b = 24;
			byte b2 = 231;
			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					Tiles[i, j] &= b2;
					Tiles[i, j] |= (byte)(TilesThreadCopy[i, j] & b);
				}
			}
		}
		WantCalcEnclosedAreasOnThread = false;
	}

	public bool CalcEnclosedAreas()
	{
		if (!WantCalcEnclosedAreasOnThread)
		{
			return false;
		}
		using (new StopWatchMarker("CalcEnclosedAreas Total"))
		{
			using (new StopWatchMarker("Clear Flags"))
			{
				byte b = 231;
				for (int i = 0; i < Size; i++)
				{
					for (int j = 0; j < Size; j++)
					{
						TilesThreadCopy[i, j] &= b;
					}
				}
			}
			using (new StopWatchMarker("Loop Through Spawn Points"))
			{
				CommunityManager communityManager = Session.Instance.CommunityManager;
				for (int k = 0; k < communityManager.AmbientEnemySpawnPoints.Count; k++)
				{
					AmbientEnemySpawnPoint ambientEnemySpawnPoint = communityManager.AmbientEnemySpawnPoints[k];
					CalcEnclosedAreas_ProcessTile(ambientEnemySpawnPoint.Tile);
				}
				for (int l = 0; l < EnclosedAreaSeedPoints.Count; l++)
				{
					CalcEnclosedAreas_ProcessTile(EnclosedAreaSeedPoints[l]);
				}
			}
		}
		return true;
	}

	private void CalcEnclosedAreas_ProcessTile(TerrainCoord tile)
	{
		byte b = 24;
		if ((TilesThreadCopy[tile.x, tile.y] & b) == 0)
		{
			int num = FillEnclosed(tile, TileFlags.Spawnable);
			int num2 = ((Size <= 128) ? 8000 : ((Size <= 256) ? 30000 : 100000));
			if (num <= num2)
			{
				FillEnclosed(tile, TileFlags.Enclosed);
			}
		}
	}

	private bool CalcSpanForTile(TerrainCoord tile, out Span span)
	{
		byte b = 70;
		if ((TilesThreadCopy[tile.x, tile.y] & b) != 0)
		{
			span = Span.Invalid;
			return false;
		}
		span = new Span((short)tile.x, (short)tile.x, (short)tile.y);
		while (span.x0 > 0 && (TilesThreadCopy[span.x0 - 1, span.y] & b) == 0)
		{
			span.x0--;
		}
		while (span.x1 < Size - 1 && (TilesThreadCopy[span.x1 + 1, span.y] & b) == 0)
		{
			span.x1++;
		}
		return true;
	}

	private int FillEnclosed(TerrainCoord start, TileFlags flag)
	{
		int num = 0;
		int val = 0;
		using (new StopWatchMarker("FillEnclosed"))
		{
			if (!CalcSpanForTile(start, out var span))
			{
				return 0;
			}
			Stack.Add(span);
			while (Stack.Count > 0)
			{
				val = Math.Max(val, Stack.Count);
				span = Stack[Stack.Count - 1];
				Stack.RemoveAt(Stack.Count - 1);
				if (((uint)TilesThreadCopy[span.x0, span.y] & (uint)flag) != 0)
				{
					continue;
				}
				Span span2 = Span.Invalid;
				Span span3 = Span.Invalid;
				for (int i = span.x0; i <= span.x1; i++)
				{
					TilesThreadCopy[i, span.y] |= (byte)flag;
					num++;
					if (i > span2.x1 && span.y > 0 && ((uint)TilesThreadCopy[i, span.y - 1] & (uint)flag) == 0 && CalcSpanForTile(new TerrainCoord(i, span.y - 1), out var span4))
					{
						Stack.Add(span4);
						span2 = span4;
					}
					if (i > span3.x1 && span.y < Size - 1 && ((uint)TilesThreadCopy[i, span.y + 1] & (uint)flag) == 0 && CalcSpanForTile(new TerrainCoord(i, span.y + 1), out var span5))
					{
						Stack.Add(span5);
						span3 = span5;
					}
				}
			}
			return num;
		}
	}

	public void CalcIllegalAreas()
	{
		for (int i = 0; i < Size; i++)
		{
			CalcIllegalAreas_ProcessTile(new TerrainCoord(i, 0));
			CalcIllegalAreas_ProcessTile(new TerrainCoord(i, Size - 1));
		}
		for (int j = 1; j < Size - 1; j++)
		{
			CalcIllegalAreas_ProcessTile(new TerrainCoord(0, j));
			CalcIllegalAreas_ProcessTile(new TerrainCoord(Size - 1, j));
		}
	}

	private void CalcIllegalAreas_ProcessTile(TerrainCoord start)
	{
		if (!CalcSpanForIllegalTile(start, out var span))
		{
			return;
		}
		using (new StopWatchMarker("FillIllegal"))
		{
			Stack.Add(span);
			while (Stack.Count > 0)
			{
				span = Stack[Stack.Count - 1];
				Stack.RemoveAt(Stack.Count - 1);
				if ((Tiles2[span.x0, span.y] & 4) != 0)
				{
					continue;
				}
				Span span2 = Span.Invalid;
				Span span3 = Span.Invalid;
				for (int i = span.x0; i <= span.x1; i++)
				{
					Tiles2[i, span.y] |= 4;
					if (i > span2.x1 && span.y > 0 && (Tiles2[i, span.y - 1] & 4) == 0 && CalcSpanForIllegalTile(new TerrainCoord(i, span.y - 1), out var span4))
					{
						Stack.Add(span4);
						span2 = span4;
					}
					if (i > span3.x1 && span.y < Size - 1 && (Tiles2[i, span.y + 1] & 4) == 0 && CalcSpanForIllegalTile(new TerrainCoord(i, span.y + 1), out var span5))
					{
						Stack.Add(span5);
						span3 = span5;
					}
				}
			}
		}
	}

	private bool CalcSpanForIllegalTile(TerrainCoord tile, out Span span)
	{
		byte b = 94;
		if ((Tiles[tile.x, tile.y] & b) != 0 || (Tiles2[tile.x, tile.y] & 4) != 0)
		{
			span = Span.Invalid;
			return false;
		}
		span = new Span((short)tile.x, (short)tile.x, (short)tile.y);
		while (span.x0 > 0 && (Tiles[span.x0 - 1, span.y] & b) == 0 && (Tiles2[tile.x, tile.y] & 4) == 0)
		{
			span.x0--;
		}
		while (span.x1 < Size - 1 && (Tiles[span.x1 + 1, span.y] & b) == 0 && (Tiles2[tile.x, tile.y] & 4) == 0)
		{
			span.x1++;
		}
		return true;
	}

	public void BuildEntireMinimap()
	{
		BuildMinimap(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
	}

	public void BuildMinimap(TerrainCoord tl, TerrainCoord br)
	{
		tl = TerrainCoord.Max(tl, new TerrainCoord(0, 0));
		br = TerrainCoord.Min(br, new TerrainCoord(Size - 1, Size - 1));
		Color32[] colMap = MinimapCol.GetColMap();
		for (int i = tl.x; i <= br.x; i++)
		{
			for (int j = tl.y; j <= br.y; j++)
			{
				int num = i + j * Size;
				colMap[num] = GetMapColorForTile(new TerrainCoord(i, j));
				if (TerrainEditor.ShowImpassable)
				{
					if (IsTileEnclosed(i, j))
					{
						colMap[num] = Color.green;
					}
					else if (IsTileBuiltOn(i, j))
					{
						colMap[num] = Color.yellow;
					}
					else if (IsTileSpawnable(i, j))
					{
						colMap[num] = Color.magenta;
					}
					else if (IsTileIllegal(i, j))
					{
						colMap[num] = Color.cyan;
					}
				}
				List<TileObject> objects = LookupSquares[i / 8, j / 8].Objects;
				if (objects == null)
				{
					continue;
				}
				TerrainCoord terrainCoord = new TerrainCoord(i, j);
				foreach (TileObject item in objects)
				{
					if (!(item is Character) && terrainCoord.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
					{
						Color32 mapColor = item.MapColor;
						if (mapColor.a != 0)
						{
							colMap[i + j * Size] = mapColor;
							break;
						}
					}
				}
			}
		}
		MinimapTexDirty = true;
	}

	public void UpdateMinimapTexIfNeeded()
	{
		if (MinimapTexDirty)
		{
			using (new UnityProfileMarker(UpdateMinimapTexStr))
			{
				MinimapTex.SetPixelData(MinimapCol.GetColMap(), 0);
				MinimapTex.Apply();
				MinimapTexDirty = false;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			if (!GeologicalTexDirty[i])
			{
				continue;
			}
			using (new UnityProfileMarker(UpdateGeoTexStr))
			{
				int num = Size / 4;
				Color32[] array = new Color32[MathUtil.Squared(num)];
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num; k++)
					{
						byte b = MineralWeights[j, k, i];
						array[j + k * num] = new Color32(b, b, b, byte.MaxValue);
					}
				}
				GeologicalTex[i].SetPixels32(array);
				GeologicalTex[i].Apply();
				GeologicalTexDirty[i] = false;
			}
		}
	}

	public void UpdateHeightMapTexIfNeeded()
	{
		if (HeightMapTexDirty)
		{
			using (new UnityProfileMarker(UpdateHeightMapTexStr))
			{
				HeightMapTex.SetPixels32(HeightMapCol.GetColMap());
				HeightMapTex.Apply();
				HeightMapTexDirty = false;
			}
		}
	}

	public void RecalcRiverTiles(TerrainCoord min, TerrainCoord max)
	{
		for (int i = min.x; i <= max.x; i++)
		{
			for (int j = min.y; j <= max.y; j++)
			{
				SetFlag(i, j, TileFlags.River, on: false);
				SetFlag2(i, j, TileFlags2.Road, on: false);
			}
		}
		foreach (TerrainPath path in Paths)
		{
			if (path.LocalBounds == null)
			{
				path.BuildBoundingBoxes(this);
			}
			for (int k = 0; k < path.LocalBounds.Count; k++)
			{
				if (!path.LocalBounds[k].Intersect(new TerrainRect(min, max), out var result))
				{
					continue;
				}
				for (int l = result.min.x; l <= result.max.x; l++)
				{
					for (int m = result.min.y; m <= result.max.y; m++)
					{
						Vector3 tileCentrePos = GetTileCentrePos(new TerrainCoord(l, m));
						if (path.GetAmountOnPath(this, MathUtil.ToXZ(tileCentrePos), out var h) < 1f && h >= tileCentrePos.y)
						{
							if (path.IsRiver)
							{
								SetFlag(l, m, TileFlags.River, on: true);
							}
							else
							{
								SetFlag2(l, m, TileFlags2.Road, on: true);
							}
						}
					}
				}
			}
		}
	}

	public void RebuildPathsInRect(TerrainCoord min, TerrainCoord max)
	{
		if (UnityTerrainObj == null)
		{
			return;
		}
		PathsToUpdate.Clear();
		foreach (TerrainPath path in Paths)
		{
			if (path.LocalBounds == null)
			{
				path.BuildBoundingBoxes(this);
			}
			for (int i = 0; i < path.LocalBounds.Count; i++)
			{
				if (path.LocalBounds[i].Overlaps(new TerrainRect(min, max)))
				{
					PathsToUpdate.Add(path);
					break;
				}
			}
		}
		foreach (TerrainPath item in PathsToUpdate)
		{
			item.Rebuild(this);
		}
		PathsToUpdate.Clear();
	}

	private void RemoveGrassFromRocksInRadius(TerrainCoord tile, float radius, float innerRadius)
	{
		int num = (int)Math.Ceiling(innerRadius + radius);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord terrainCoord2 = new TerrainCoord(tile.x + num, tile.y + num);
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				if (!IsTileOutsideBounds(i, j) && !(Math.Max(new Vector2((float)i - ((float)tile.x + 0.5f), (float)j - ((float)tile.y + 0.5f)).magnitude - innerRadius, 0f) > radius) && GetTerrainTypeGrassFactor(GetTileTerrainType(new TerrainCoord(i, j))) == 0f)
				{
					for (int k = 0; k < 8; k++)
					{
						GrassMap.SetGrassAmount(i, j, (GrassType)k, 0);
					}
				}
			}
		}
	}

	public void DrawRock(TerrainCoord tile, float baseHeight, float radius, float scale, bool lockRivers, bool lockRoads)
	{
		int num = (int)Math.Ceiling(radius);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord br = new TerrainCoord(tile.x + num + 1, tile.y + num + 1);
		for (int i = terrainCoord.x; i <= br.x; i++)
		{
			for (int j = terrainCoord.y; j <= br.y; j++)
			{
				if (!IsVertexOutsideBounds(i, j) && (!lockRivers || !IsVertexRiver(i, j)) && (!lockRoads || !IsVertexRoad(i, j)))
				{
					float magnitude = new Vector2((float)i - ((float)tile.x + 0.5f), (float)j - ((float)tile.y + 0.5f)).magnitude;
					if (magnitude <= radius)
					{
						Vertices[i, j] = Math.Max(Vertices[i, j], baseHeight + (radius - magnitude) * scale * (0.5f + 0.5f * (float)MathUtil.NonDeterministicRand.NextDouble()));
					}
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius, 0f);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, br);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), br);
	}

	public void DrawButte(TerrainCoord tile, float baseHeight, float plateauRadius, float radius, float scale, bool lockRivers, bool lockRoads)
	{
		int num = (int)Math.Ceiling(plateauRadius + radius);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord br = new TerrainCoord(tile.x + num + 1, tile.y + num + 1);
		for (int i = terrainCoord.x; i <= br.x; i++)
		{
			for (int j = terrainCoord.y; j <= br.y; j++)
			{
				if (!IsVertexOutsideBounds(i, j) && (!lockRivers || !IsVertexRiver(i, j)) && (!lockRoads || !IsVertexRoad(i, j)))
				{
					float num2 = Math.Max(new Vector2((float)i - ((float)tile.x + 0.5f), (float)j - ((float)tile.y + 0.5f)).magnitude - plateauRadius, 0f);
					if (num2 <= radius * 0.25f)
					{
						Vertices[i, j] = Math.Max(Vertices[i, j], baseHeight + radius * scale - num2 * scale * ButteTopFalloff);
					}
					else if (num2 <= radius)
					{
						Vertices[i, j] = Math.Max(Vertices[i, j], baseHeight + (radius - num2) * scale * 0.75f);
					}
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius, plateauRadius);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, br);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), br);
	}

	public void DrawCanyon(TerrainCoord tile, float baseHeight, float canyonRadius, float radius, float scale, bool lockRivers, bool lockRoads)
	{
		int num = (int)Math.Ceiling(canyonRadius + radius + 2f);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord br = new TerrainCoord(tile.x + num + 1, tile.y + num + 1);
		for (int i = terrainCoord.x; i <= br.x; i++)
		{
			for (int j = terrainCoord.y; j <= br.y; j++)
			{
				if (IsVertexOutsideBounds(i, j) || (lockRivers && IsVertexRiver(i, j)) || (lockRoads && IsVertexRoad(i, j)))
				{
					continue;
				}
				float num2 = Math.Max(new Vector2((float)i - ((float)tile.x + 0.5f), (float)j - ((float)tile.y + 0.5f)).magnitude - canyonRadius, 0f);
				if (num2 <= radius)
				{
					if (num2 >= radius * 0.75f)
					{
						Vertices[i, j] = Math.Min(Vertices[i, j], baseHeight - (radius - num2) * scale * ButteTopFalloff);
					}
					else
					{
						Vertices[i, j] = Math.Min(Vertices[i, j], baseHeight - radius * scale + num2 * scale * 0.75f);
					}
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius, canyonRadius);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, br);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), br);
	}

	public void Roughen(TerrainCoord tile, float radius, float amount, bool ignoreImpassable, bool lockRivers, bool lockRoads)
	{
		int num = (int)Math.Ceiling(radius);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord terrainCoord2 = new TerrainCoord(tile.x + num + 1, tile.y + num + 1);
		bool[,] array = new bool[Size + 1, Size + 1];
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				if (!IsVertexOutsideBounds(i, j) && (!ignoreImpassable || !IsImpassable(i, j, 0, null, null)) && (!lockRivers || !IsVertexRiver(i, j)) && (!lockRoads || !IsVertexRoad(i, j)) && new Vector2((float)i - (float)tile.x, (float)j - (float)tile.y).magnitude <= radius)
				{
					Vertices[i, j] += (2f * MathUtil.NonDeterministicRand.RandomFloat() - 1f) * amount;
					array[i, j] = true;
				}
			}
		}
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), terrainCoord2 + new TerrainCoord(1, 1));
		Smooth(tile, radius + 2f, 2f, 1f, 1f, ignoreImpassable: false, lockRivers, lockRoads, array);
	}

	public void DrawDune(TerrainCoord tile, float baseHeight, float radius, float amount, bool lockRivers, bool lockRoads)
	{
		float num = radius * 0.5f;
		int num2 = (int)Math.Ceiling(radius + num);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num2, tile.y - num2);
		TerrainCoord terrainCoord2 = new TerrainCoord(tile.x + num2 + 1, tile.y + num2 + 1);
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				if (IsVertexOutsideBounds(i, j) || (lockRivers && IsVertexRiver(i, j)) || (lockRoads && IsVertexRoad(i, j)))
				{
					continue;
				}
				float magnitude = new Vector2((float)i - (float)tile.x, (float)j - (float)tile.y).magnitude;
				if (magnitude <= radius + num)
				{
					float num3 = baseHeight;
					num3 = ((!(magnitude <= radius)) ? (num3 + amount * (1f - magnitude / radius)) : (num3 + amount * Mathf.SmoothStep(0f, 1f, 1f - magnitude / radius)));
					if (!(Vertices[i, j] >= num3))
					{
						Vertices[i, j] = num3;
					}
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius + num, 0f);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, terrainCoord2);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), terrainCoord2 + new TerrainCoord(1, 1));
	}

	public void DrawCamber(TerrainCoord tile, float baseHeight, float radius, float amount, bool lockRivers, bool lockRoads)
	{
		float num = radius * 0.5f;
		int num2 = (int)Math.Ceiling(radius + num);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num2, tile.y - num2);
		TerrainCoord terrainCoord2 = new TerrainCoord(tile.x + num2 + 1, tile.y + num2 + 1);
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				if (IsVertexOutsideBounds(i, j) || (lockRivers && IsVertexRiver(i, j)) || (lockRoads && IsVertexRoad(i, j)))
				{
					continue;
				}
				float magnitude = new Vector2((float)i - (float)tile.x, (float)j - (float)tile.y).magnitude;
				if (magnitude <= radius + num)
				{
					float num3 = baseHeight;
					if (magnitude <= radius)
					{
						num3 += amount * Mathf.Cos(magnitude / radius * (MathF.PI / 2f));
					}
					if (!(Vertices[i, j] >= num3))
					{
						Vertices[i, j] = num3;
					}
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius + num, 0f);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, terrainCoord2);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), terrainCoord2 + new TerrainCoord(1, 1));
	}

	public float GetSmoothedHeight(TerrainCoord vert, float[,] gaussinBlurKernel, int kernelRadius)
	{
		TerrainCoord terrainCoord = new TerrainCoord(vert.x - kernelRadius, vert.y - kernelRadius);
		TerrainCoord terrainCoord2 = new TerrainCoord(vert.x + kernelRadius, vert.y + kernelRadius);
		float num = 0f;
		for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
		{
			for (int j = terrainCoord.y; j <= terrainCoord2.y; j++)
			{
				TerrainCoord terrainCoord3 = ClampVertexWithinBounds(new TerrainCoord(i, j));
				num += Vertices[terrainCoord3.x, terrainCoord3.y] * gaussinBlurKernel[i - vert.x + kernelRadius, j - vert.y + kernelRadius];
			}
		}
		return num;
	}

	public void Smooth(TerrainCoord tile, float radius, float smoothRadius, float sigma, float amount, bool ignoreImpassable, bool lockRivers, bool lockRoads, bool[,] vertices)
	{
		int num = (int)Math.Ceiling(smoothRadius);
		float[,] array = new float[num * 2 + 1, num * 2 + 1];
		float num2 = 0f;
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				num2 += (array[i + num, j + num] = MathUtil.Gaussian(new Vector2(i, j).magnitude, sigma));
			}
		}
		for (int k = -num; k <= num; k++)
		{
			for (int l = -num; l <= num; l++)
			{
				array[k + num, l + num] /= num2;
			}
		}
		int num3 = (int)Math.Ceiling(radius + smoothRadius);
		float[,] array2 = new float[num3 * 2 + 1, num3 * 2 + 1];
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num3, tile.y - num3);
		TerrainCoord terrainCoord2 = new TerrainCoord(tile.x + num3, tile.y + num3);
		for (int m = terrainCoord.x; m <= terrainCoord2.x; m++)
		{
			for (int n = terrainCoord.y; n <= terrainCoord2.y; n++)
			{
				if (!IsVertexOutsideBounds(m, n) && (!ignoreImpassable || !IsImpassable(m, n, 0, null, null)) && (!lockRivers || !IsVertexRiver(m, n)) && (!lockRoads || !IsVertexRoad(m, n)) && (vertices == null || vertices[m, n]) && new Vector2((float)m - (float)tile.x, (float)n - (float)tile.y).magnitude <= radius)
				{
					array2[m - tile.x + num3, n - tile.y + num3] = GetSmoothedHeight(new TerrainCoord(m, n), array, num);
				}
			}
		}
		for (int num4 = terrainCoord.x; num4 <= terrainCoord2.x; num4++)
		{
			for (int num5 = terrainCoord.y; num5 <= terrainCoord2.y; num5++)
			{
				if (!IsVertexOutsideBounds(num4, num5) && (!ignoreImpassable || !IsImpassable(num4, num5, 0, null, null)) && (!lockRivers || !IsVertexRiver(num4, num5)) && (!lockRoads || !IsVertexRoad(num4, num5)) && (vertices == null || vertices[num4, num5]) && new Vector2((float)num4 - (float)tile.x, (float)num5 - (float)tile.y).magnitude <= radius)
				{
					Vertices[num4, num5] = Mathf.Lerp(Vertices[num4, num5], array2[num4 - tile.x + num3, num5 - tile.y + num3], amount);
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius + smoothRadius, 0f);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, terrainCoord2);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), terrainCoord2 + new TerrainCoord(1, 1));
	}

	public void Flatten(TerrainCoord tile, float baseHeight, float radius, bool lockRivers, bool lockRoads)
	{
		int num = (int)Math.Ceiling(radius);
		TerrainCoord terrainCoord = new TerrainCoord(tile.x - num, tile.y - num);
		TerrainCoord br = new TerrainCoord(tile.x + num + 1, tile.y + num + 1);
		for (int i = terrainCoord.x; i <= br.x; i++)
		{
			for (int j = terrainCoord.y; j <= br.y; j++)
			{
				if (!IsVertexOutsideBounds(i, j) && (!lockRivers || !IsVertexRiver(i, j)) && (!lockRoads || !IsVertexRoad(i, j)) && new Vector2((float)i - ((float)tile.x + 0.5f), (float)j - ((float)tile.y + 0.5f)).magnitude <= radius)
				{
					Vertices[i, j] = baseHeight;
				}
			}
		}
		RemoveGrassFromRocksInRadius(tile, radius, 0f);
		GrassMap.ApplyGrassChangesIfNeeded(terrainCoord, br);
		OnHeightChanged(terrainCoord - new TerrainCoord(1, 1), br);
	}

	public void CarveRiverBed(TerrainPath path, int node)
	{
		for (int i = 0; i < path.ControlPoints.Count; i++)
		{
			if (node == i || node == -1)
			{
				ControlPoint value = path.ControlPoints[i];
				value.RiverDepth = 1f;
				path.ControlPoints[i] = value;
			}
		}
		int num = 4;
		TerrainRect terrainRect = path.TotalBounds.Expand(num);
		if (node != -1)
		{
			terrainRect = TerrainRect.Invalid;
			for (int j = 0; j < path.Points.Count; j++)
			{
				if (Mathf.Abs(path.Points[j].ControlPointIndex - (float)node) <= 1f)
				{
					TerrainCoord tileCoordForPos = GetTileCoordForPos(path.Points[j].Pos);
					int num2 = Mathf.CeilToInt(path.Points[j].Width * 0.5f);
					TerrainRect terrainRect2 = new TerrainRect(tileCoordForPos - new TerrainCoord(num2, num2), tileCoordForPos + new TerrainCoord(num2, num2));
					terrainRect = ((terrainRect == TerrainRect.Invalid) ? terrainRect2 : terrainRect.Include(terrainRect2));
				}
			}
			if (terrainRect == TerrainRect.Invalid)
			{
				return;
			}
			terrainRect = terrainRect.Expand(num);
		}
		ClampMinMaxTileWithinBounds(ref terrainRect.min, ref terrainRect.max);
		terrainRect.max += new TerrainCoord(1, 1);
		for (int k = terrainRect.min.x; k <= terrainRect.max.x; k++)
		{
			for (int l = terrainRect.min.y; l <= terrainRect.max.y; l++)
			{
				if (IsVertexOutsideBounds(k, l))
				{
					continue;
				}
				Vector2 vertexPosXZ = GetVertexPosXZ(k, l);
				float closestDistSq = float.MaxValue;
				float closestPathIndex = 0f;
				Vector3 closestPointOnPath = Vector3.zero;
				Vector2 closestDirXZOnPath = Vector2.zero;
				if (!path.GetClosestPointOnPathToPos(this, vertexPosXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex))
				{
					continue;
				}
				float num3 = Mathf.Sqrt(closestDistSq);
				float t = closestPathIndex - Mathf.Floor(closestPathIndex);
				int index = MathUtil.Clamp(Mathf.FloorToInt(closestPathIndex), 0, path.Points.Count - 1);
				int index2 = MathUtil.Clamp(Mathf.CeilToInt(closestPathIndex), 0, path.Points.Count - 1);
				float num4 = 1f;
				if (node != -1)
				{
					num4 = 1f - Mathf.Clamp01(Math.Min(Mathf.Abs(path.Points[index].ControlPointIndex - (float)node), Mathf.Abs(path.Points[index2].ControlPointIndex - (float)node)) - 0.5f);
					if (num4 <= 0f)
					{
						continue;
					}
				}
				float num5 = Mathf.Lerp(path.Points[index].Width, path.Points[index2].Width, t) * 0.5f;
				if (num3 < num5)
				{
					float num6 = (path.IsRiver ? (Math.Max(0f, Mathf.Cos(MathF.PI / 2f * num3 / num5)) * 1f) : 0f);
					Vertices[k, l] = Mathf.Lerp(Vertices[k, l], closestPointOnPath.y - num6, num4);
				}
				else if (num3 - num5 < (float)num)
				{
					Vertices[k, l] = Mathf.Lerp(Vertices[k, l], closestPointOnPath.y, num4 * (1f - (num3 - num5) / (float)num));
				}
			}
		}
		GrassMap.ApplyGrassChangesIfNeeded(terrainRect.min, terrainRect.max);
		OnHeightChanged(terrainRect.min - new TerrainCoord(1, 1), terrainRect.max);
	}

	public void ApplyPathTexture(TerrainPath path, int node)
	{
		int num = 2;
		TerrainRect terrainRect = path.TotalBounds.Expand(num);
		if (node != -1)
		{
			terrainRect = TerrainRect.Invalid;
			for (int i = 0; i < path.Points.Count; i++)
			{
				if (Mathf.Abs(path.Points[i].ControlPointIndex - (float)node) <= 1f)
				{
					TerrainCoord tileCoordForPos = GetTileCoordForPos(path.Points[i].Pos);
					int num2 = Mathf.CeilToInt(path.Points[i].Width * 0.5f);
					TerrainRect terrainRect2 = new TerrainRect(tileCoordForPos - new TerrainCoord(num2, num2), tileCoordForPos + new TerrainCoord(num2, num2));
					terrainRect = ((terrainRect == TerrainRect.Invalid) ? terrainRect2 : terrainRect.Include(terrainRect2));
				}
			}
			if (terrainRect == TerrainRect.Invalid)
			{
				return;
			}
			terrainRect = terrainRect.Expand(num);
		}
		ClampMinMaxTileWithinBounds(ref terrainRect.min, ref terrainRect.max);
		for (int j = terrainRect.min.x; j <= terrainRect.max.x; j++)
		{
			for (int k = terrainRect.min.y; k <= terrainRect.max.y; k++)
			{
				if (IsTileOutsideBounds(j, k))
				{
					continue;
				}
				Vector2 vertexPosXZ = GetVertexPosXZ(j, k);
				float closestDistSq = float.MaxValue;
				float closestPathIndex = 0f;
				Vector3 closestPointOnPath = Vector3.zero;
				Vector2 closestDirXZOnPath = Vector2.zero;
				if (!path.GetClosestPointOnPathToPos(this, vertexPosXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex))
				{
					continue;
				}
				float num3 = Mathf.Sqrt(closestDistSq);
				float t = closestPathIndex - Mathf.Floor(closestPathIndex);
				int index = MathUtil.Clamp(Mathf.FloorToInt(closestPathIndex), 0, path.Points.Count - 1);
				int index2 = MathUtil.Clamp(Mathf.CeilToInt(closestPathIndex), 0, path.Points.Count - 1);
				float num4 = 1f;
				if (node != -1)
				{
					num4 = 1f - Mathf.Clamp01(Math.Min(Mathf.Abs(path.Points[index].ControlPointIndex - (float)node), Mathf.Abs(path.Points[index2].ControlPointIndex - (float)node)) - 0.5f);
					if (num4 <= 0f)
					{
						continue;
					}
				}
				float num5 = Mathf.Lerp(path.Points[index].Width, path.Points[index2].Width, t) * 0.5f;
				int num6 = j * 3843 + k;
				TerrainTex terrainTex = (path.IsRiver ? TerrainTex.Mud : ((!MathUtil.RandomChoice(num6, 0.1f)) ? TerrainTex.Road : TerrainTex.Grass));
				SetTextureWeight(j, k, terrainTex, num4 * (1f - Mathf.Clamp01((num3 - num5) / (float)num)));
				if (terrainTex == TerrainTex.Grass)
				{
					GrassType grassType = (GrassType)MathUtil.RandomInt(num6, 3);
					float num7 = GrassRenderer.GetMaxDensityForGrassType(grassType);
					GrassMap.SetGrassAmount(j, k, grassType, Mathf.CeilToInt(num7 * MathUtil.RandomFloat(num6)));
				}
			}
		}
		GrassMap.ApplyGrassChangesIfNeeded(terrainRect.min, terrainRect.max);
		using (new StopWatchMarker("Apply Textures"))
		{
			ApplyTexturesToUnityTerrainData(UnityTerrain.terrainData, terrainRect.min, terrainRect.max);
		}
		using (new StopWatchMarker("Build Minimap"))
		{
			BuildMinimap(terrainRect.min, terrainRect.max);
		}
	}

	public void Reset()
	{
		for (int i = 0; i <= Size; i++)
		{
			for (int j = 0; j <= Size; j++)
			{
				Vertices[i, j] = 16f;
				TextureWeights[i, j, 0] = byte.MaxValue;
				for (int k = 1; k < 8; k++)
				{
					TextureWeights[i, j, k] = 0;
				}
			}
		}
		GrassMap.ApplyGrassChangesIfNeeded(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
		OnHeightChanged(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
	}

	private TerrainTex GetTexForBiome(BiomeType biomeType, int x, int y)
	{
		switch (biomeType)
		{
		case BiomeType.Meadow:
			return TerrainTex.Grass;
		case BiomeType.DeciduousForest:
			return TerrainTex.DeciduousFloor;
		case BiomeType.ConiferForest:
			return TerrainTex.ConiferFloor;
		case BiomeType.PineForest:
			return TerrainTex.ConiferFloor;
		case BiomeType.Road:
			return TerrainTex.Road;
		case BiomeType.River:
			return TerrainTex.Mud;
		case BiomeType.Rock:
			if (!(GetTileMaxHeight(x, y) - GetTileMinHeight(x, y) >= 1.25f))
			{
				return TerrainTex.Rock;
			}
			return TerrainTex.RockCliff;
		default:
			return TerrainTex.Mud;
		}
	}

	private float SampleHeightMap(float[,] heightMap, Vector2 pos)
	{
		float num = Mathf.Clamp(pos.x + HalfSize, 0f, Size);
		float num2 = Mathf.Clamp(pos.y + HalfSize, 0f, Size);
		int num3 = (int)num;
		int num4 = (int)num2;
		float t = num % 1f;
		float t2 = num2 % 1f;
		return Mathf.Lerp(Mathf.Lerp(heightMap[num3, num4], heightMap[Math.Min(Size, num3 + 1), num4], t), Mathf.Lerp(heightMap[num3, Math.Min(Size, num4 + 1)], heightMap[Math.Min(Size, num3 + 1), Math.Min(Size, num4 + 1)], t), t2);
	}

	public static CubicBezier2D GetBezierFromPathPoints(ControlPoint cur, ControlPoint next, out float len)
	{
		len = (next.Pos - cur.Pos).magnitude;
		CubicBezier2D result = default(CubicBezier2D);
		result.p0 = cur.Pos;
		result.p1 = cur.Pos + cur.Dir * len * 0.25f;
		result.p2 = next.Pos - next.Dir * len * 0.25f;
		result.p3 = next.Pos;
		return result;
	}

	private TerrainPath GetClosestPathToPos(Vector2 posXZ, ref float closestDistSq, ref Vector3 closestPointOnPath, ref Vector2 closestDirXZOnPath, ref float closestPointIndex)
	{
		TerrainPath result = null;
		foreach (TerrainPath path in Paths)
		{
			if (path.GetClosestPointOnPathToPos(this, posXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPointIndex))
			{
				result = path;
			}
		}
		return result;
	}

	private float GetHeightForNextPathPoint(bool isRiver, float[,] originalHeight, List<TerrainPathPoint> points, Vector2 pathPosXZ)
	{
		float num = ((!isRiver) ? SampleHeightMap(originalHeight, pathPosXZ) : GetTileHeightAtPos(pathPosXZ.x, pathPosXZ.y));
		if (points.Count > 0)
		{
			Vector3 pos = points[points.Count - 1].Pos;
			if (isRiver)
			{
				num = Math.Min(num, pos.y);
			}
			else
			{
				float num2 = (pathPosXZ - MathUtil.ToXZ(pos)).magnitude * 0.25f;
				num = Mathf.Clamp(num, pos.y - num2, pos.y + num2);
			}
		}
		return num;
	}

	private bool AddCurveToPath(bool isRiver, float[,] originalHeight, List<ControlPoint> controlPoints, List<TerrainPathPoint> points, ref ControlPoint next, bool allowCrossing, bool final, ref bool failed, ref float width, List<TerrainPathJunction> newJunctions, ref TerrainPath lastCrossedPath)
	{
		ControlPoint cur = controlPoints[controlPoints.Count - 1];
		float len;
		CubicBezier2D bezierFromPathPoints = GetBezierFromPathPoints(cur, next, out len);
		int num = Mathf.CeilToInt(len * TerrainPath.SampleDensity);
		int num2 = num + (final ? 1 : 0);
		int count = points.Count;
		for (int i = 0; i < num2; i++)
		{
			float t = (float)i / (float)num;
			Vector2 vector = bezierFromPathPoints.EvaluatePos(t);
			float heightForNextPathPoint = GetHeightForNextPathPoint(isRiver, originalHeight, points, vector);
			points.Add(new TerrainPathPoint
			{
				Pos = MathUtil.ToXZY(vector, heightForNextPathPoint),
				DirXZ = bezierFromPathPoints.EvaluateDir(t),
				Width = Mathf.Lerp(cur.Width, next.Width, t)
			});
			float closestDistSq = 576f;
			float closestPointIndex = 0f;
			Vector3 closestPointOnPath = Vector3.zero;
			Vector2 closestDirXZOnPath = Vector2.zero;
			TerrainPath closestPathToPos = GetClosestPathToPos(vector, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPointIndex);
			if (closestPathToPos == null)
			{
				continue;
			}
			if (!allowCrossing)
			{
				UnityEngine.Debug.Log("Abandoned path because it crossed another path near the beginning or end");
				failed = true;
				lastCrossedPath = null;
				return false;
			}
			if (closestPathToPos == lastCrossedPath)
			{
				UnityEngine.Debug.Log("Abandoned path because it criss-crossed another path too quickly");
				failed = true;
				return false;
			}
			Vector2 vector2 = vector;
			Vector2 vector3 = bezierFromPathPoints.EvaluateDir(t);
			Vector2 vector4 = MathUtil.ToXZ(closestPointOnPath);
			Vector2 vector5 = MathUtil.SafeNormalize(vector4 - vector2, vector3);
			float num3 = closestPointIndex;
			float num4 = 1f;
			while (Vector2.Dot(vector3, vector5) < 0.707f)
			{
				float num5 = num3 + num4;
				TerrainPathPoint point = closestPathToPos.GetPoint(num5);
				vector4 = MathUtil.ToXZ(point.Pos);
				vector5 = MathUtil.SafeNormalize(vector4 - vector2, vector3);
				closestPointOnPath = point.Pos;
				closestDirXZOnPath = point.DirXZ;
				closestPointIndex = num5;
				closestDistSq = (vector4 - vector2).sqrMagnitude;
				num4 = ((!(num4 > 0f)) ? (0f - num4 + 1f) : (0f - num4));
				if (Mathf.Abs(num4) > 10f)
				{
					UnityEngine.Debug.Log("Abandoned path because it crossed another path at too sharp an angle, couldn't find good crossing place");
					failed = true;
					lastCrossedPath = null;
					return false;
				}
			}
			if (isRiver && closestPathToPos.IsRiver && closestPointOnPath.y > heightForNextPathPoint)
			{
				UnityEngine.Debug.Log("Abandoned river because it joined another river that was higher than itself");
				failed = true;
				lastCrossedPath = null;
				return false;
			}
			if (closestPathToPos.IsRiver)
			{
				Vector3 pos = closestPathToPos.Points[MathUtil.Clamp(Mathf.FloorToInt(closestPointIndex - 0.5f), 0, closestPathToPos.Points.Count - 1)].Pos;
				Vector3 pos2 = closestPathToPos.Points[MathUtil.Clamp(Mathf.CeilToInt(closestPointIndex + 0.5f), 0, closestPathToPos.Points.Count - 1)].Pos;
				if (Mathf.Abs(pos2.y - pos.y) > MathUtil.ToXZ(pos2 - pos).magnitude * 0.35f)
				{
					UnityEngine.Debug.Log("Abandoned path because it tried to join or cross a river at a waterfall or steep section");
					failed = true;
					return false;
				}
			}
			ControlPoint controlPoint = new ControlPoint
			{
				Pos = vector2,
				Dir = vector3,
				RiverDepth = Mathf.Lerp(cur.RiverDepth, next.RiverDepth, t),
				Width = Mathf.Lerp(cur.Width, next.Width, t)
			};
			controlPoints.Add(controlPoint);
			ControlPoint controlPoint2 = new ControlPoint
			{
				Pos = vector4,
				Dir = vector5,
				RiverDepth = controlPoint.RiverDepth,
				Width = controlPoint.Width
			};
			if (!isRiver && closestPathToPos.IsRiver)
			{
				Vector2 vector6 = MathUtil.RightNormal(closestDirXZOnPath);
				if (Vector2.Dot(vector6, controlPoint2.Dir) < 0f)
				{
					vector6 = -vector6;
				}
				float angleFromNormalizedDir = MathUtil.GetAngleFromNormalizedDir(vector6);
				angleFromNormalizedDir = Prop.GetAngleFromOrientationType(Prop.GetClosestOrientationTypeToAngle(angleFromNormalizedDir));
				controlPoint2.Dir = MathUtil.GetDirFromAngle(angleFromNormalizedDir);
			}
			controlPoints.Add(controlPoint2);
			next = controlPoint2;
			float num6 = 24f / (24f - closestPathToPos.DefaultWidth) - 1f;
			float len2;
			CubicBezier2D bezierFromPathPoints2 = GetBezierFromPathPoints(controlPoint, controlPoint2, out len2);
			int num7 = Mathf.CeilToInt(len2 * TerrainPath.SampleDensity);
			for (int j = 1; j < num7; j++)
			{
				float num8 = (float)j / (float)num7;
				Vector2 v = bezierFromPathPoints2.EvaluatePos(num8);
				float z = Mathf.Lerp(heightForNextPathPoint, closestPointOnPath.y, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num8 * (1f + num6))));
				points.Add(new TerrainPathPoint
				{
					Pos = MathUtil.ToXZY(v, z),
					DirXZ = bezierFromPathPoints2.EvaluateDir(num8),
					Width = Mathf.Lerp(controlPoint.Width, controlPoint2.Width, num8)
				});
			}
			if (!isRiver && Mathf.Abs(closestPointOnPath.y - heightForNextPathPoint) > Mathf.Sqrt(closestDistSq) * 0.25f)
			{
				float magnitude = MathUtil.ToXZ(closestPointOnPath - points[count].Pos).magnitude;
				if (Mathf.Abs(closestPointOnPath.y - points[count].Pos.y) > magnitude * 0.25f)
				{
					UnityEngine.Debug.Log("Abandoned road because it joined another path that at too steep a gradient");
					failed = true;
					lastCrossedPath = null;
					return false;
				}
				float num9 = magnitude / (magnitude - closestPathToPos.DefaultWidth) - 1f;
				for (int k = count; k < points.Count; k++)
				{
					float num10 = (float)(k - count) / (float)(points.Count - count);
					TerrainPathPoint value = points[k];
					value.Pos.y = Mathf.Lerp(points[count].Pos.y, closestPointOnPath.y, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num10 * (1f + num9))));
					points[k] = value;
				}
			}
			if (isRiver == closestPathToPos.IsRiver)
			{
				TerrainPathJunction terrainPathJunction = new TerrainPathJunction();
				terrainPathJunction.PathIndex0 = closestPathToPos.Index;
				terrainPathJunction.PathIndex1 = Paths.Count;
				terrainPathJunction.PointIndex0 = closestPointIndex;
				terrainPathJunction.PointIndex1 = points.Count;
				terrainPathJunction.Pos = closestPointOnPath;
				newJunctions.Add(terrainPathJunction);
				if (isRiver && closestPathToPos.IsRiver)
				{
					width = Math.Max(Math.Min(width, closestPathToPos.DefaultWidth - 1f), 5f);
				}
				lastCrossedPath = null;
				return false;
			}
			ControlPoint controlPoint3 = new ControlPoint
			{
				Pos = controlPoint2.Pos + controlPoint2.Dir * 32f,
				Dir = controlPoint2.Dir,
				RiverDepth = controlPoint2.RiverDepth,
				Width = controlPoint2.Width
			};
			controlPoints.Add(controlPoint3);
			next = controlPoint3;
			float closestDistSq2 = 576f;
			float closestPointIndex2 = 0f;
			Vector3 closestPointOnPath2 = Vector3.zero;
			Vector2 closestDirXZOnPath2 = Vector2.zero;
			if (GetClosestPathToPos(controlPoint3.Pos, ref closestDistSq2, ref closestPointOnPath2, ref closestDirXZOnPath2, ref closestPointIndex2) != null)
			{
				UnityEngine.Debug.Log("Abandoned road because it criss-crossed one path soon after another");
				failed = true;
				lastCrossedPath = null;
				return false;
			}
			TerrainPathJunction terrainPathJunction2 = new TerrainPathJunction();
			terrainPathJunction2.PathIndex0 = closestPathToPos.Index;
			terrainPathJunction2.PathIndex1 = Paths.Count;
			terrainPathJunction2.PointIndex0 = closestPointIndex;
			terrainPathJunction2.PointIndex1 = points.Count;
			terrainPathJunction2.Pos = closestPointOnPath;
			newJunctions.Add(terrainPathJunction2);
			float heightForNextPathPoint2 = GetHeightForNextPathPoint(isRiver, originalHeight, points, controlPoint3.Pos);
			float len3;
			CubicBezier2D bezierFromPathPoints3 = GetBezierFromPathPoints(controlPoint2, controlPoint3, out len3);
			int num11 = Mathf.CeilToInt(len3 * TerrainPath.SampleDensity);
			for (int l = 0; l < num11; l++)
			{
				float num12 = (float)l / (float)num11;
				Vector2 v2 = bezierFromPathPoints3.EvaluatePos(num12);
				float z2 = Mathf.Lerp(closestPointOnPath.y, heightForNextPathPoint2, Mathf.SmoothStep(0f, 1f, num12 * (1f + num6) - num6));
				points.Add(new TerrainPathPoint
				{
					Pos = MathUtil.ToXZY(v2, z2),
					DirXZ = bezierFromPathPoints3.EvaluateDir(num12),
					Width = Mathf.Lerp(controlPoint2.Width, controlPoint3.Width, num12)
				});
			}
			lastCrossedPath = closestPathToPos;
			return true;
		}
		controlPoints.Add(next);
		lastCrossedPath = null;
		return true;
	}

	private float GetDistSqToClosestPathOnEdgeOfTerrain(Vector2 posXZ)
	{
		float num = float.MaxValue;
		foreach (TerrainPath path in Paths)
		{
			Vector2 vector = MathUtil.ToXZ(path.Points[0].Pos);
			Vector2 vector2 = MathUtil.ToXZ(path.Points[path.Points.Count - 1].Pos);
			float sqrMagnitude = (posXZ - vector).sqrMagnitude;
			float sqrMagnitude2 = (posXZ - vector2).sqrMagnitude;
			num = Math.Min(sqrMagnitude, num);
			num = Math.Min(sqrMagnitude2, num);
		}
		return num;
	}

	private TerrainPath LayoutPath(bool isRiver, float[,] originalHeight, float[,] closestPathDistFactor, float[,] camberOrRiverBed, BiomeType[,] biome, CustomRandom rand, float minRiverStartHeight)
	{
		float width = Mathf.Lerp(5f, 10f, rand.RandomFloat());
		bool failed = false;
		List<TerrainPathPoint> list = new List<TerrainPathPoint>();
		TerrainPath lastCrossedPath = null;
		ControlPoint next = new ControlPoint
		{
			RiverDepth = 1f,
			Width = width
		};
		int num = 0;
		do
		{
			num++;
			if (num >= 100)
			{
				return null;
			}
			switch (rand.Next(4))
			{
			case 0:
				next.Pos = new Vector2(Mathf.Lerp(0f - HalfSize + 32f, HalfSize - 32f, rand.RandomFloat()), 0f - HalfSize);
				next.Dir = new Vector2(0f, 1f);
				break;
			case 1:
				next.Pos = new Vector2(Mathf.Lerp(0f - HalfSize + 32f, HalfSize - 32f, rand.RandomFloat()), HalfSize);
				next.Dir = new Vector2(0f, -1f);
				break;
			case 2:
				next.Pos = new Vector2(0f - HalfSize, Mathf.Lerp(0f - HalfSize + 32f, HalfSize - 32f, rand.RandomFloat()));
				next.Dir = new Vector2(1f, 0f);
				break;
			case 3:
				next.Pos = new Vector2(HalfSize, Mathf.Lerp(0f - HalfSize + 32f, HalfSize - 32f, rand.RandomFloat()));
				next.Dir = new Vector2(-1f, 0f);
				break;
			}
		}
		while (GetDistSqToClosestPathOnEdgeOfTerrain(next.Pos) < 1024f || (isRiver && SampleHeightMap(originalHeight, next.Pos) < minRiverStartHeight));
		List<ControlPoint> list2 = new List<ControlPoint>();
		List<TerrainPathJunction> list3 = new List<TerrainPathJunction>();
		list2.Add(next);
		next.Pos += next.Dir * 32f;
		AddCurveToPath(isRiver, originalHeight, list2, list, ref next, allowCrossing: false, final: false, ref failed, ref width, list3, ref lastCrossedPath);
		bool flag = false;
		while (!flag)
		{
			ControlPoint controlPoint = default(ControlPoint);
			if (!isRiver && rand.RandomChoice(0.3f))
			{
				controlPoint.Pos = next.Pos + next.Dir * Mathf.Lerp(32f, 128f, rand.RandomFloat());
			}
			else
			{
				controlPoint.Pos = next.Pos + next.Dir * Mathf.Lerp(32f, 128f, rand.RandomFloat()) + MathUtil.RightNormal(next.Dir) * Mathf.Lerp(-32f, 32f, rand.RandomFloat());
			}
			controlPoint.Dir = MathUtil.GetDirFromAngle(MathUtil.GetAngleFromDir(controlPoint.Pos - next.Pos, 0f) + Mathf.Lerp(-MathF.PI / 4f, MathF.PI / 4f, rand.RandomFloat()));
			controlPoint.RiverDepth = next.RiverDepth;
			controlPoint.Width = next.Width;
			if (controlPoint.Pos.x < 0f - HalfSize + 32f)
			{
				controlPoint.Pos.x = 0f - HalfSize + 32f;
				controlPoint.Pos.y = Mathf.Clamp(controlPoint.Pos.y, 0f - HalfSize + 40f, HalfSize - 40f);
				controlPoint.Dir = new Vector2(-1f, 0f);
				flag = true;
			}
			else if (controlPoint.Pos.x > HalfSize - 32f)
			{
				controlPoint.Pos.x = HalfSize - 32f;
				controlPoint.Pos.y = Mathf.Clamp(controlPoint.Pos.y, 0f - HalfSize + 40f, HalfSize - 40f);
				controlPoint.Dir = new Vector2(1f, 0f);
				flag = true;
			}
			else if (controlPoint.Pos.y < 0f - HalfSize + 32f)
			{
				controlPoint.Pos.y = 0f - HalfSize + 32f;
				controlPoint.Pos.x = Mathf.Clamp(controlPoint.Pos.x, 0f - HalfSize + 40f, HalfSize - 40f);
				controlPoint.Dir = new Vector2(0f, -1f);
				flag = true;
			}
			else if (controlPoint.Pos.y > HalfSize - 32f)
			{
				controlPoint.Pos.y = HalfSize - 32f;
				controlPoint.Pos.x = Mathf.Clamp(controlPoint.Pos.x, 0f - HalfSize + 40f, HalfSize - 40f);
				controlPoint.Dir = new Vector2(0f, 1f);
				flag = true;
			}
			if (flag && (controlPoint.Pos - next.Pos).magnitude < 16f)
			{
				next.Dir = controlPoint.Dir;
				break;
			}
			next = controlPoint;
			if (!AddCurveToPath(isRiver, originalHeight, list2, list, ref next, !flag, final: false, ref failed, ref width, list3, ref lastCrossedPath))
			{
				flag = false;
				break;
			}
			if (list2.Count >= 100)
			{
				if (isRiver)
				{
					return null;
				}
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Vector2? rayRectIntersectionPoint = MathUtil.GetRayRectIntersectionPoint(next.Pos, next.Dir, new Vector2(0f - HalfSize, 0f - HalfSize), new Vector2(HalfSize, HalfSize));
			if (!rayRectIntersectionPoint.HasValue)
			{
				return null;
			}
			next.Pos = rayRectIntersectionPoint.Value;
			AddCurveToPath(isRiver, originalHeight, list2, list, ref next, allowCrossing: false, final: true, ref failed, ref width, list3, ref lastCrossedPath);
		}
		for (int i = 0; i < list2.Count - 1; i++)
		{
			if (GetTileCoordForPosXZ(list2[i].Pos) == GetTileCoordForPosXZ(list2[i + 1].Pos))
			{
				UnityEngine.Debug.Log("Abandoned degenerate path");
				failed = true;
				break;
			}
		}
		for (int j = 0; j < list.Count - 1; j++)
		{
			Vector2 a = MathUtil.ToXZ(list[j].Pos);
			Vector2 a2 = MathUtil.ToXZ(list[j + 1].Pos);
			for (int k = 0; k < j - 1; k++)
			{
				Vector2 b = MathUtil.ToXZ(list[k].Pos);
				Vector2 b2 = MathUtil.ToXZ(list[k + 1].Pos);
				if (MathUtil.DoLinesIntersect(a, a2, b, b2))
				{
					UnityEngine.Debug.Log("Abandoned path because of self intersection");
					return null;
				}
			}
		}
		if (failed)
		{
			return null;
		}
		TerrainPath terrainPath = new TerrainPath();
		terrainPath.Index = Paths.Count;
		terrainPath.IsRiver = isRiver;
		terrainPath.ControlPoints = list2;
		terrainPath.Points = list;
		terrainPath.DefaultWidth = width;
		terrainPath.BuildBoundingBoxes(this);
		if (terrainPath.ControlPoints.Count >= 3)
		{
			TerrainCoord terrainCoord = ClampTileWithinBounds(GetTileCoordForPosXZ(terrainPath.ControlPoints[0].Pos));
			if (IsTileOnEdge(terrainCoord.x, terrainCoord.y))
			{
				terrainPath.EntrancePointIndex = 1;
				terrainPath.EntranceRoadTraits = RoadDestinationTraits.CreateRandomized(rand);
			}
			TerrainCoord terrainCoord2 = ClampTileWithinBounds(GetTileCoordForPosXZ(terrainPath.ControlPoints[terrainPath.ControlPoints.Count - 1].Pos));
			if (IsTileOnEdge(terrainCoord2.x, terrainCoord2.y))
			{
				terrainPath.ExitPointIndex = terrainPath.ControlPoints.Count - 2;
				terrainPath.ExitRoadTraits = RoadDestinationTraits.CreateRandomized(rand);
			}
		}
		for (int l = 0; l <= Size; l++)
		{
			for (int m = 0; m <= Size; m++)
			{
				InfluenceOfPathOnVertex? influenceOfPathOnVertex = GetInfluenceOfPathOnVertex(new TerrainCoord(l, m), terrainPath);
				if (influenceOfPathOnVertex.HasValue)
				{
					ApplyInfluence(l, m, closestPathDistFactor, influenceOfPathOnVertex.Value.DesiredHeight, influenceOfPathOnVertex.Value.DistFactor);
					int num2 = MathUtil.Clamp(l - 1, 0, Size - 1);
					int num3 = MathUtil.Clamp(l, 0, Size - 1);
					int num4 = MathUtil.Clamp(m - 1, 0, Size - 1);
					int num5 = MathUtil.Clamp(m, 0, Size - 1);
					if (influenceOfPathOnVertex.Value.CamberOrRiverDepth > 0f)
					{
						camberOrRiverBed[l, m] = Math.Max(camberOrRiverBed[l, m], influenceOfPathOnVertex.Value.CamberOrRiverDepth);
						biome[num2, num4] = (biome[num2, num5] = (biome[num3, num4] = (biome[num3, num5] = BiomeType.Road)));
						SetFlag(num2, num4, TileFlags.River, on: false);
						SetFlag(num2, num5, TileFlags.River, on: false);
						SetFlag(num3, num4, TileFlags.River, on: false);
						SetFlag(num3, num5, TileFlags.River, on: false);
						SetFlag2(num2, num4, TileFlags2.Road, on: true);
						SetFlag2(num2, num5, TileFlags2.Road, on: true);
						SetFlag2(num3, num4, TileFlags2.Road, on: true);
						SetFlag2(num3, num5, TileFlags2.Road, on: true);
					}
					else if (influenceOfPathOnVertex.Value.CamberOrRiverDepth < 0f)
					{
						camberOrRiverBed[l, m] = Math.Min(camberOrRiverBed[l, m], influenceOfPathOnVertex.Value.CamberOrRiverDepth);
						biome[num2, num4] = (biome[num2, num5] = (biome[num3, num4] = (biome[num3, num5] = BiomeType.River)));
						SetFlag(num2, num4, TileFlags.River, on: true);
						SetFlag(num2, num5, TileFlags.River, on: true);
						SetFlag(num3, num4, TileFlags.River, on: true);
						SetFlag(num3, num5, TileFlags.River, on: true);
					}
					else if (camberOrRiverBed[l, m] < 0f && !isRiver && influenceOfPathOnVertex.Value.DistFromEdgeOfPath < 2f)
					{
						float t = (2f - influenceOfPathOnVertex.Value.DistFromEdgeOfPath) / 2f;
						float val = Mathf.Lerp(-1f, 0f, Mathf.SmoothStep(0f, 1f, t));
						camberOrRiverBed[l, m] = Math.Max(camberOrRiverBed[l, m], val);
					}
				}
			}
		}
		Paths.Add(terrainPath);
		Junctions.AddRange(list3);
		return terrainPath;
	}

	private void ApplyInfluence(int x, int y, float[,] closestPathDistFactor, float desiredHeight, float distFactor)
	{
		if (closestPathDistFactor[x, y] < float.MaxValue)
		{
			float num = closestPathDistFactor[x, y] + distFactor;
			if (num > 0f)
			{
				Vertices[x, y] = Mathf.Lerp(desiredHeight, Vertices[x, y], distFactor / num);
			}
			else
			{
				Vertices[x, y] = Mathf.Lerp(desiredHeight, Vertices[x, y], 0.5f);
			}
		}
		else
		{
			Vertices[x, y] = desiredHeight;
		}
		Vertices[x, y] = Mathf.Clamp(Vertices[x, y], 1f, 64f);
		closestPathDistFactor[x, y] = Math.Min(closestPathDistFactor[x, y], distFactor);
	}

	private InfluenceOfPathOnVertex? GetInfluenceOfPathOnVertex(TerrainCoord vert, TerrainPath path)
	{
		Vector2 vertexPosXZ = GetVertexPosXZ(vert.x, vert.y);
		float num = path.DefaultWidth * 0.5f;
		float num2 = path.DefaultWidth * 4f;
		float num3 = num + num2;
		float closestDistSq = num3 * num3;
		Vector3 closestPointOnPath = Vector3.zero;
		Vector2 closestDirXZOnPath = Vector2.zero;
		float closestPathIndex = 0f;
		if (path.GetClosestPointOnPathToPos(this, vertexPosXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex))
		{
			InfluenceOfPathOnVertex value = default(InfluenceOfPathOnVertex);
			value.Path = path;
			float y = closestPointOnPath.y;
			float num4 = Mathf.Sqrt(closestDistSq);
			if (num4 <= num)
			{
				value.CamberOrRiverDepth = Math.Max(0f, Mathf.Cos(MathF.PI / 2f * num4 / num)) * (path.IsRiver ? (-1f) : 0.2f);
				value.DesiredHeight = y;
				value.DistFromEdgeOfPath = 0f;
				value.DistFactor = 0f;
			}
			else
			{
				float num5 = num4 - num;
				float num6 = path.DefaultWidth * 2f;
				if (path.IsRiver)
				{
					num6 = Mathf.Lerp(num2, num6, y / 64f);
				}
				value.DesiredHeight = Mathf.Lerp(y, Vertices[vert.x, vert.y], Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num5 / num6)));
				value.DistFromEdgeOfPath = num5;
				value.DistFactor = Mathf.Clamp01(num5 / num2);
				value.CamberOrRiverDepth = 0f;
			}
			return value;
		}
		return null;
	}

	private static bool HasAnyOfLootLocation<T>(List<T> objects, string LootLocation) where T : TileObject
	{
		foreach (T @object in objects)
		{
			if (@object.GetLootLocation() == LootLocation)
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasAnyOfType<T>(List<T> objects, PropPrototype type) where T : TileObject
	{
		foreach (T @object in objects)
		{
			if (@object.GetPropPrototype() == type)
			{
				return true;
			}
		}
		return false;
	}

	private static int CountType<T>(List<T> objects, BaseObjectType type) where T : BaseObject
	{
		int num = 0;
		foreach (T @object in objects)
		{
			if (@object.GetBaseObjectType() == type)
			{
				num++;
			}
		}
		return num;
	}

	private void FlattenBuildingGround(TileObject building, float height, float surroundingDist, float[,] closestPathDistFactor, BiomeType[,] biome)
	{
		float num = ((building is Building) ? (building as Building).TarmacDist : 0f);
		int num2 = Mathf.CeilToInt(num + surroundingDist);
		TerrainCoord terrainCoord = new TerrainCoord(Mathf.CeilToInt(num), Mathf.CeilToInt(num));
		TerrainRect terrainRect = new TerrainRect(building.GetMinTile() - terrainCoord, building.GetMaxTile() + terrainCoord + new TerrainCoord(1, 1));
		for (int i = building.GetMinTile().x - num2; i <= building.GetMaxTile().x + num2; i++)
		{
			for (int j = building.GetMinTile().y - num2; j <= building.GetMaxTile().y + num2; j++)
			{
				if (IsVertexOutsideBounds(i, j))
				{
					continue;
				}
				TerrainCoord terrainCoord2 = new TerrainCoord(i, j);
				float num3 = 0f;
				if (terrainRect.Contains(terrainCoord2))
				{
					if (num > 0f)
					{
						biome[i, j] = BiomeType.Road;
					}
				}
				else
				{
					num3 = terrainRect.GetNearestCoordWithinRectTo(terrainCoord2).GetDist(terrainCoord2);
				}
				float desiredHeight = Mathf.Lerp(height, Vertices[i, j], Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num3 / surroundingDist)));
				float distFactor = Mathf.Clamp01(num3 / surroundingDist);
				ApplyInfluence(i, j, closestPathDistFactor, desiredHeight, distFactor);
			}
		}
		building.OnTerrainHeightChanged();
	}

	private void GenerateDriveway(TileObject frontBuilding, int xMin, int xMax, float distFromRoad, float housePosY, TerrainPath road, float[,] closestPathDistFactor, BiomeType[,] biome)
	{
		distFromRoad -= frontBuilding.TarmacDist;
		for (int i = xMin; i <= xMax; i++)
		{
			Vector2 vector = MathUtil.ToXZ(frontBuilding.GetWorld().MultiplyPoint(new Vector3(i, 0f, (float)frontBuilding.GetPropPrototype().ExtentsMax.y + frontBuilding.TarmacDist)));
			road.GetEarliestIntersectionOfLineWithPath(this, vector, vector + MathUtil.GetDirFromAngle(Prop.GetAngleFromOrientationType(frontBuilding.GetOrientationType())) * distFromRoad, out var intersection);
			for (int j = 0; j <= Mathf.CeilToInt(distFromRoad); j++)
			{
				vector = MathUtil.ToXZ(frontBuilding.GetWorld().MultiplyPoint(new Vector3(i, 0f, (float)(j + frontBuilding.GetPropPrototype().ExtentsMax.y) + frontBuilding.TarmacDist)));
				TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(vector);
				biome[tileCoordForPosXZ.x, tileCoordForPosXZ.y] = BiomeType.Road;
				float desiredHeight = Mathf.Lerp(housePosY, intersection.y, (float)j / distFromRoad);
				ApplyInfluence(tileCoordForPosXZ.x, tileCoordForPosXZ.y, closestPathDistFactor, desiredHeight, 0f);
			}
		}
	}

	private bool IsBuildingBlocked(TileObject building, float housePosY, BiomeType[,] biome)
	{
		int num = Mathf.CeilToInt(building.TarmacDist);
		int num2 = 32 + num;
		if (!IsTileRectWithinBounds(building.GetMinTile() - new TerrainCoord(num2, num2), building.GetMaxTile() + new TerrainCoord(num2, num2)))
		{
			return true;
		}
		int num3 = num + Mathf.CeilToInt(4f) + 20;
		List<TileObject> list = new List<TileObject>();
		GetObjectsInRect(building.GetMinTile() - new TerrainCoord(num3, num3), building.GetMaxTile() + new TerrainCoord(num3, num3), list);
		foreach (TileObject item in list)
		{
			int num4 = Mathf.CeilToInt((item is Building) ? (item as Building).TarmacDist : 0f);
			int num5 = num + num4 + 4;
			float num6 = Mathf.Abs(item.Pos.y - housePosY);
			num5 += (int)num6;
			if (TerrainCoord.Overlaps(building.GetMinTile() - new TerrainCoord(num5, num5), building.GetMaxTile() + new TerrainCoord(num5, num5), item.GetMinTile(), item.GetMaxTile()))
			{
				return true;
			}
		}
		TerrainCoord terrainCoord = new TerrainCoord(Mathf.CeilToInt(num), Mathf.CeilToInt(num));
		int num7 = Mathf.CeilToInt(num) + 4;
		for (int i = building.GetMinTile().x - num7; i <= building.GetMaxTile().x + num7; i++)
		{
			for (int j = building.GetMinTile().y - num7; j <= building.GetMaxTile().y + num7; j++)
			{
				if (!IsTileOutsideBounds(i, j))
				{
					if (biome[i, j] == BiomeType.River)
					{
						return true;
					}
					TerrainCoord other = new TerrainCoord(i, j);
					if (other.IsWithinBounds(building.GetMinTile() - terrainCoord, building.GetMaxTile() + terrainCoord) && biome[i, j] != BiomeType.Meadow)
					{
						return true;
					}
					float num8 = 2f;
					if (biome[i, j] == BiomeType.Road)
					{
						num8 = other.GetDist(new TerrainRect(building.GetMinTile() - terrainCoord, building.GetMaxTile() + terrainCoord).GetNearestCoordWithinRectTo(other)) / 4f;
					}
					if (GetTileMaxHeight(i, j) >= housePosY + num8 || GetTileMinHeight(i, j) <= housePosY - num8)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool GenerateBuilding(TerrainPath road, int centrePt, ref int offset, ref int last, ref bool stopped, int dir, int side, List<Building> buildings, List<TileObject> props, int desiredNumBuildings, ref int attempts, float[,] closestPathDistFactor, BiomeType[,] biome, CustomRandom rand)
	{
		if (stopped)
		{
			return true;
		}
		List<PropPrototype> allowedBuildings = PropPrototype.GetTypesWithCategory("Town/Buildings");
		int i;
		for (i = allowedBuildings.Count - 1; i >= 0; i--)
		{
			if (allowedBuildings[i].Usage == BuildingUsage.Commercial)
			{
				if (HasAnyOfType(buildings, allowedBuildings[i]))
				{
					allowedBuildings.RemoveAt(i);
					continue;
				}
				if (HasAnyOfLootLocation(buildings, allowedBuildings[i].LootLocation))
				{
					allowedBuildings.RemoveAt(i);
					continue;
				}
			}
			if (allowedBuildings[i].MustBeUnique && Session.Instance.PropManager.AllProps.Exists((Prop prop5) => prop5.Prototype == allowedBuildings[i]))
			{
				allowedBuildings.RemoveAt(i);
			}
		}
		int num = offset;
		int num2 = 0;
		ConnectorCandidate item = default(ConnectorCandidate);
		do
		{
			TileObject tileObject = TileObject.CreateProp(PropPrototype.PickRandom(rand, allowedBuildings));
			offset = num;
			while (true)
			{
				if (offset - num < 32)
				{
					Prop prop = tileObject as Prop;
					offset += dir;
					if (Math.Abs(offset - last) >= 32)
					{
						break;
					}
					int num3 = centrePt + offset;
					if (num3 >= 0 && num3 < road.Points.Count)
					{
						float val = Mathf.Lerp(3f, 16f, rand.RandomFloat()) + (float)(prop?.ExtentsMax.y ?? 0);
						val = Math.Max(val, tileObject.GetPropPrototype().SpawnMinDistanceFromRoad);
						Vector2 vector = MathUtil.RightNormal(road.Points[num3].DirXZ);
						Vector2 pos = MathUtil.ToXZ(road.Points[num3].Pos) + vector * (road.DefaultWidth * 0.5f + val) * side;
						float y = road.Points[num3].Pos.y;
						TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(pos);
						if (Session.Instance.CommunityManager.GetTownForTile(tileCoordForPosXZ, 0f) != null)
						{
							stopped = true;
							return true;
						}
						tileObject.SetTileGhost(tileCoordForPosXZ);
						tileObject.SetOrientationType(Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir(-vector * side)));
						List<TileObject> list = new List<TileObject>();
						list.Add(tileObject);
						int num4 = 0;
						if (tileObject.GetPropPrototype().Connectors != null && tileObject.GetPropPrototype().Connectors.Length != 0)
						{
							int num5 = 0;
							while (true)
							{
								List<PropPrototype> list2 = new List<PropPrototype>();
								foreach (PropPrototype proto in allowedBuildings)
								{
									if (proto.Category == tileObject.Category && proto != tileObject.GetPropPrototype() && proto.Connectors != null && proto.Connectors.Length != 0 && (!proto.MustBeUnique || (!list.Exists((TileObject obj) => obj.GetPropPrototype() == proto) && !Session.Instance.PropManager.AllProps.Exists((Prop prop5) => prop5.Prototype == proto))) && (list.Count >= 3 || proto.Connectors.Length != 1) && (list.Count < 7 || proto.Connectors.Length == 1) && (num4 < 2 || !proto.IsCorner()))
									{
										list2.Add(proto);
									}
								}
								if (list2.Count == 0)
								{
									break;
								}
								TileObject tileObject2 = list[list.Count - 1];
								if (tileObject2.GetPropPrototype().ConnectorsCanTerminate && list.Count >= 3)
								{
									list2.Add(null);
								}
								PropPrototype propPrototype = list2[rand.Next(list2.Count)];
								if (propPrototype == null)
								{
									break;
								}
								TileObject tileObject3 = TileObject.CreateProp(propPrototype);
								Vector2[] connectors = tileObject2.GetConnectors();
								Vector2[] connectors2 = tileObject3.GetConnectors();
								Vector2 vector2 = MathUtil.ToXZ(tileObject2.GetWorld().MultiplyVector(MathUtil.ToX0Y(connectors[num5])));
								List<ConnectorCandidate> list3 = new List<ConnectorCandidate>();
								for (int num6 = 0; num6 < connectors2.Length; num6++)
								{
									for (int num7 = 0; num7 < 4; num7++)
									{
										tileObject3.SetOrientationType((Prop.OrientationType)num7);
										Vector2 dir2 = MathUtil.ToXZ(tileObject3.GetWorld().MultiplyVector(MathUtil.ToX0Y(connectors2[num6])));
										if (!(Vector2.Dot(vector2.normalized, dir2.normalized) <= -0.99f))
										{
											continue;
										}
										float num8 = 0f;
										if (tileObject3 is Building && tileObject2 is Building)
										{
											float entranceAngle = ((Building)tileObject3).GetEntranceAngle(0);
											float entranceAngle2 = ((Building)tileObject2).GetEntranceAngle(0);
											float num9 = MathF.PI / 20f;
											if (Prop.IsDiagonalOrientation(entranceAngle))
											{
												num9 += MathF.PI / 4f;
											}
											if (Prop.IsDiagonalOrientation(entranceAngle2))
											{
												num9 += MathF.PI / 4f;
											}
											if (MathUtil.AngleDiff(entranceAngle, entranceAngle2) < num9)
											{
												num8 += 1f;
											}
										}
										if (list3.Count > 0)
										{
											if (list3[0].Score < num8)
											{
												list3.Clear();
											}
											else if (list3[0].Score > num8)
											{
												continue;
											}
										}
										item.Orientation = (Prop.OrientationType)num7;
										item.Dir = dir2;
										item.Index = num6;
										item.Score = num8;
										list3.Add(item);
									}
								}
								if (list3.Count == 0)
								{
									break;
								}
								int index = rand.Next(list3.Count);
								Vector2 dir3 = list3[index].Dir;
								int index2 = list3[index].Index;
								Prop.OrientationType orientation = list3[index].Orientation;
								tileObject3.SetOrientationType(orientation);
								Vector2 pos2 = tileObject2.PosXZ + vector2 - dir3;
								tileObject3.SetTile(GetTileCoordForPosXZ(pos2));
								list.Add(tileObject3);
								if (tileObject3.GetPropPrototype().IsCorner())
								{
									num4++;
								}
								if (connectors2.Length <= 1)
								{
									break;
								}
								num5 = (index2 + 1) % connectors2.Length;
							}
							if (list.Count < 3)
							{
								allowedBuildings.Remove(tileObject.GetPropPrototype());
								tileObject = TileObject.CreateProp(PropPrototype.PickRandom(rand, allowedBuildings));
								continue;
							}
						}
						PropPrototype propPrototype2 = tileObject.GetPropPrototype();
						if (prop != null && (list.Count > 1 || propPrototype2.FrontPropType != null || propPrototype2.RoadPropType != null))
						{
							int colorVariation = ((propPrototype2.GetNumColorVariations() > 0) ? rand.Next(propPrototype2.GetNumColorVariations()) : (-1));
							int colorVariation2 = ((propPrototype2.GetNumColorVariations2() > 0) ? rand.Next(propPrototype2.GetNumColorVariations2()) : (-1));
							int colorVariation3 = ((propPrototype2.GetNumColorVariations3() > 0) ? rand.Next(propPrototype2.GetNumColorVariations3()) : (-1));
							int colorVariation4 = ((propPrototype2.GetNumColorVariations4() > 0) ? rand.Next(propPrototype2.GetNumColorVariations4()) : (-1));
							int materialVariation = ((propPrototype2.GetNumMaterialVariations() > 0) ? rand.Next(propPrototype2.GetNumMaterialVariations()) : (-1));
							foreach (TileObject item2 in list)
							{
								if (item2 is Prop prop2)
								{
									prop2.ColorVariation = colorVariation;
									prop2.ColorVariation2 = colorVariation2;
									prop2.ColorVariation3 = colorVariation3;
									prop2.ColorVariation4 = colorVariation4;
									prop2.MaterialVariation = materialVariation;
								}
							}
						}
						bool flag = false;
						foreach (TileObject item3 in list)
						{
							if (IsBuildingBlocked(item3, y, biome))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							continue;
						}
						last = offset;
						foreach (TileObject item4 in list)
						{
							item4.OnSpawn();
							FlattenBuildingGround(item4, y, 16f, closestPathDistFactor, biome);
							if (!(item4 is Building building))
							{
								continue;
							}
							buildings.Add(building);
							if (building.GetBuildingUsage() != BuildingUsage.Commercial)
							{
								continue;
							}
							int num10 = 4;
							for (int num11 = building.ExtentsMin.x - num10; num11 <= building.ExtentsMax.x + num10; num11++)
							{
								for (int num12 = building.ExtentsMax.y - num10; num12 <= building.ExtentsMax.y + num10; num12++)
								{
									Vector2 pos3 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(num11, 0f, num12)));
									TerrainCoord tileCoordForPosXZ2 = GetTileCoordForPosXZ(pos3);
									biome[tileCoordForPosXZ2.x, tileCoordForPosXZ2.y] = BiomeType.Road;
								}
							}
						}
						if (propPrototype2.Usage == BuildingUsage.Commercial)
						{
							GenerateDriveway(tileObject, propPrototype2.ExtentsMin.x, propPrototype2.ExtentsMax.x, val, y, road, closestPathDistFactor, biome);
						}
						else if (propPrototype2.GarageDoorOffset != TerrainCoord.Invalid && propPrototype2.GarageDoorAngle == 0f)
						{
							GenerateDriveway(tileObject, propPrototype2.GarageDoorOffset.x - 1, propPrototype2.GarageDoorOffset.x + 1, val, y, road, closestPathDistFactor, biome);
						}
						if (propPrototype2.FrontPropType != null)
						{
							PropPrototype propPrototype3 = GameImpl.Instance.FindPropPrototypeByName(propPrototype2.FrontPropType);
							if (propPrototype3 != null)
							{
								Vector2 pos4 = MathUtil.ToXZ(tileObject.GetWorld().MultiplyPoint(new Vector3(0f, 0f, (float)propPrototype2.ExtentsMax.y - 1f + ((propPrototype2.FrontPropDist != 0f) ? propPrototype2.FrontPropDist : (val * 0.5f)))));
								TerrainCoord tileCoordForPosXZ3 = GetTileCoordForPosXZ(pos4);
								TileObject tileObject4 = TileObject.SpawnProp(propPrototype3, tileCoordForPosXZ3, tileObject.GetOrientationType());
								Prop prop3 = tileObject4 as Prop;
								if (prop3 != null && prop != null)
								{
									prop3.ColorVariation = prop.ColorVariation;
									prop3.ColorVariation2 = prop.ColorVariation2;
									prop3.ColorVariation3 = prop.ColorVariation3;
									prop3.ColorVariation4 = prop.ColorVariation4;
									prop3.MaterialVariation = prop.MaterialVariation;
								}
								props.Add(tileObject4);
								FlattenBuildingGround(prop3, prop3.Pos.y, val * 0.5f, closestPathDistFactor, biome);
							}
						}
						if (propPrototype2.RoadPropType != null)
						{
							PropPrototype propPrototype4 = GameImpl.Instance.FindPropPrototypeByName(propPrototype2.RoadPropType);
							if (propPrototype4 != null)
							{
								int num13 = Math.Max(propPrototype4.ExtentsMax.x - propPrototype4.ExtentsMin.x, propPrototype4.ExtentsMax.y - propPrototype4.ExtentsMin.y) / 2;
								Vector2 pos5 = MathUtil.ToXZ(road.Points[num3].Pos) + vector * (road.DefaultWidth * 0.5f + 1f + (float)num13) * side;
								TerrainCoord tileCoordForPosXZ4 = GetTileCoordForPosXZ(pos5);
								TileObject tileObject5 = TileObject.SpawnProp(propPrototype4, tileCoordForPosXZ4, tileObject.GetOrientationType());
								if (tileObject5 is Prop prop4 && prop != null)
								{
									prop4.ColorVariation = prop.ColorVariation;
									prop4.ColorVariation2 = prop.ColorVariation2;
									prop4.ColorVariation3 = prop.ColorVariation3;
									prop4.ColorVariation4 = prop.ColorVariation4;
									prop4.MaterialVariation = prop.MaterialVariation;
								}
								props.Add(tileObject5);
							}
						}
					}
				}
				attempts++;
				if (buildings.Count < desiredNumBuildings)
				{
					return attempts < desiredNumBuildings * 10;
				}
				return false;
			}
			allowedBuildings.Remove(tileObject.GetPropPrototype());
			num2++;
		}
		while (num2 < 10 && allowedBuildings.Count != 0);
		stopped = true;
		return true;
	}

	private void GenerateTown(TerrainPath road, float pointIndex, float[,] closestPathDistFactor, BiomeType[,] biome, CustomRandom rand)
	{
		int num = (int)pointIndex;
		TerrainCoord tileCoordForPos = GetTileCoordForPos(road.Points[num].Pos);
		if (IsTileOutsideBounds(tileCoordForPos.x, tileCoordForPos.y))
		{
			return;
		}
		int[] array = new int[4];
		int[] array2 = new int[2];
		bool[] array3 = new bool[4];
		int desiredNumBuildings = rand.Next(6, 12);
		List<Building> list = new List<Building>();
		List<TileObject> list2 = new List<TileObject>();
		int attempts = 0;
		while ((!array3[0] || !array3[1] || !array3[2] || !array3[3]) && GenerateBuilding(road, num, ref array[0], ref array2[0], ref array3[0], 1, 1, list, list2, desiredNumBuildings, ref attempts, closestPathDistFactor, biome, rand) && GenerateBuilding(road, num, ref array[1], ref array2[0], ref array3[1], 1, -1, list, list2, desiredNumBuildings, ref attempts, closestPathDistFactor, biome, rand) && GenerateBuilding(road, num, ref array[2], ref array2[1], ref array3[2], -1, 1, list, list2, desiredNumBuildings, ref attempts, closestPathDistFactor, biome, rand) && GenerateBuilding(road, num, ref array[3], ref array2[1], ref array3[3], -1, -1, list, list2, desiredNumBuildings, ref attempts, closestPathDistFactor, biome, rand))
		{
		}
		List<PropPrototype> typesWithClass = PropPrototype.GetTypesWithClass(typeof(WorkBench));
		if (typesWithClass.Count > 0)
		{
			int num2 = 0;
			int num3 = 1;
			List<Building> list3 = new List<Building>();
			list.CopyToList(list3);
			while (list3.Count > 0 && num2 < num3)
			{
				int index = rand.Next() % list3.Count;
				Building building = list3[index];
				list3.RemoveAt(index);
				WorkBench workBench = null;
				if (building.Prototype.Name == "House1")
				{
					Vector2 pos = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(building.ExtentsMax.x + 1, 0f, 0f)));
					TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(pos);
					workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ, building.Orientation) as WorkBench;
				}
				if (building.Prototype.Name == "House2")
				{
					if (building.UnityModelIndexFromPrototype == 0)
					{
						Vector2 pos2 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(building.ExtentsMax.x + 1, 0f, 0f)));
						TerrainCoord tileCoordForPosXZ2 = GetTileCoordForPosXZ(pos2);
						workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ2, building.Orientation) as WorkBench;
					}
					else
					{
						Vector2 pos3 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(building.ExtentsMin.x - 1, 0f, -3f)));
						TerrainCoord tileCoordForPosXZ3 = GetTileCoordForPosXZ(pos3);
						workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ3, (Prop.OrientationType)((int)(building.Orientation + 2) % 4)) as WorkBench;
					}
				}
				if (building.Prototype.Name == "House3")
				{
					Vector2 pos4 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(2f, 0f, -building.ExtentsMax.y - 1)));
					TerrainCoord tileCoordForPosXZ4 = GetTileCoordForPosXZ(pos4);
					workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ4, (Prop.OrientationType)((int)(building.Orientation + 1) % 4)) as WorkBench;
				}
				if (building.Prototype.Name == "GasStation")
				{
					Vector2 pos5 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(-building.ExtentsMax.x - 1, 0f, 0f)));
					TerrainCoord tileCoordForPosXZ5 = GetTileCoordForPosXZ(pos5);
					workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ5, (Prop.OrientationType)((int)(building.Orientation + 2) % 4)) as WorkBench;
				}
				if (building.Prototype.Name == "MiniMart")
				{
					Vector2 pos6 = MathUtil.ToXZ(building.World.MultiplyPoint(new Vector3(-2f, 0f, -building.ExtentsMax.y - 1)));
					TerrainCoord tileCoordForPosXZ6 = GetTileCoordForPosXZ(pos6);
					workBench = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithClass), tileCoordForPosXZ6, (Prop.OrientationType)((int)(building.Orientation + 1) % 4)) as WorkBench;
				}
				if (workBench != null)
				{
					workBench.Variation = 1;
					list2.Add(workBench);
					num2++;
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		TerrainRect terrainRect = list[0].GetTileRect();
		for (int i = 1; i < list.Count; i++)
		{
			terrainRect = terrainRect.Include(list[i].GetMinTile()).Include(list[i].GetMaxTile());
		}
		Town town = Town.Spawn(ClampTileWithinBounds(terrainRect.Centre));
		town.TownName.Randomise(rand, unique: true, town);
		town.RoadIndex = road.Index;
		town.RoadPointIndex = pointIndex;
		foreach (Building item in list)
		{
			item.SetTown(town);
		}
		foreach (TileObject item2 in list2)
		{
			if (item2 is Prop)
			{
				((Prop)item2).SetTown(town);
			}
		}
		UnityEngine.Debug.Log("Created a town called " + town.GetDisplayNameString() + " with " + list.Count + "/" + desiredNumBuildings + " buildings, " + list2.Count + " props");
	}

	public int GeneratePropCluster(TerrainCoord tile, CustomRandom rand, List<PropPrototype> types, int clusterSize, bool notOnRoads, int notBlockingDoors, BiomeType[,] biome)
	{
		int num = 0;
		for (int i = 0; i < clusterSize; i++)
		{
			PropPrototype propPrototype = PropPrototype.PickRandom(rand, types);
			if (propPrototype != null && propPrototype.MineralType != MineralType.None)
			{
				List<PropPrototype> typesWithMineralType = PropPrototype.GetTypesWithMineralType(PickMineralForTile(tile, rand), types);
				propPrototype = PropPrototype.PickRandom(rand, typesWithMineralType);
			}
			if (propPrototype != null)
			{
				if (propPrototype.ProtoInstance is Prop prop)
				{
					Prop.OrientationType orientation = rand.RandomOrientationType();
					prop.CalcMinMaxTile(tile, orientation, out var minTile, out var maxTile);
					if (!IsAnyTileInRectSlopeOrImpassableRaw(minTile, maxTile) && !IsAnyFixedObjectOnRect(minTile, maxTile) && (!notOnRoads || !DoesRectContainBiomeType(minTile, maxTile, BiomeType.Road, biome)) && (notBlockingDoors <= 0 || !IsBlockingGate(minTile, maxTile, notBlockingDoors)))
					{
						Prop.Spawn(propPrototype, tile, orientation);
						num++;
					}
				}
				else if (!IsSlopeOrImpassableRaw(tile.x, tile.y) && GetFixedObjectOnTile(tile.x, tile.y) == null && (!notOnRoads || biome[tile.x, tile.y] != BiomeType.Road) && (notBlockingDoors <= 0 || !IsBlockingGate(tile, tile, notBlockingDoors)))
				{
					SingleTileProp.Spawn(propPrototype, tile);
					num++;
				}
			}
			switch (rand.Next(4))
			{
			case 0:
				tile.x++;
				break;
			case 1:
				tile.x--;
				break;
			case 2:
				tile.y++;
				break;
			case 3:
				tile.y--;
				break;
			}
		}
		return num;
	}

	private TileObject SpawnRoadSign(TerrainPath path, int i, bool onRight, List<PropPrototype> signTypes, BiomeType[,] biome, CustomRandom rand)
	{
		PropPrototype propPrototype = PropPrototype.PickRandom(rand, signTypes);
		if (propPrototype != null)
		{
			float num = path.Points[i].Width * 0.5f + 2f;
			Vector2 pos = MathUtil.ToXZ(path.Points[i].Pos) + (onRight ? 1f : (-1f)) * num * MathUtil.RightNormal(path.Points[i].DirXZ);
			TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(pos);
			Prop.OrientationType closestOrientationTypeToAngle = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir((onRight ? (-1f) : 1f) * path.Points[i].DirXZ) + (onRight ? 1f : (-1f)) * (MathF.PI / 8f));
			if (!HasObstructions(tileCoordForPosXZ, closestOrientationTypeToAngle, propPrototype, 0, canBeNextToFence: true, notOnRoads: true, notOnRivers: true, biome, notOnSlopeOrImpassableTerrain: false))
			{
				TileObject tileObject = TileObject.CreateProp(propPrototype);
				tileObject.SetOrientationType(closestOrientationTypeToAngle);
				tileObject.SetTileGhost(tileCoordForPosXZ);
				Session.Instance.ClearTrashForBuilding(tileObject.GetMinTile(), tileObject.GetMaxTile(), null, tileObject);
				return TileObject.SpawnProp(tileObject.GetPropPrototype(), tileObject.GetTile(), tileObject.GetOrientationType());
			}
		}
		return null;
	}

	public TileObject SpawnTiltedPropIfNotBlocked(TileObject dummy, bool checkSlope = true)
	{
		if (IsTileRectWithinBounds(dummy.GetMinTile(), dummy.GetMaxTile()))
		{
			if (dummy is TiltedProp tiltedProp)
			{
				TiltedProp.CalcPitchAndRoll(tiltedProp.WheelOffset, tiltedProp.WheelPos, tiltedProp.ExtraFrontWheelSeparation, tiltedProp.Tile, tiltedProp.MaxYaw, tiltedProp.World, tiltedProp.ModelOffset, inTerrain: true, out var _, out var pitch, out var roll, out var _);
				bool flag = Math.Abs(pitch) >= MathF.PI / 6f || Math.Abs(roll) >= MathF.PI / 6f;
				if (flag)
				{
					for (int i = dummy.GetMinTile().x; i <= dummy.GetMaxTile().x; i++)
					{
						for (int j = dummy.GetMinTile().y; j <= dummy.GetMaxTile().y; j++)
						{
							if (IsTileRiver(i, j))
							{
								flag = false;
								break;
							}
						}
					}
				}
				if (flag)
				{
					return null;
				}
			}
			else if (checkSlope && GetMaxHeightDiffInRect(dummy.GetMinTile(), dummy.GetMaxTile()) >= 0.25f)
			{
				return null;
			}
			List<TileObject> list = new List<TileObject>();
			GetObjectsInRect(dummy.GetMinTile(), dummy.GetMaxTile(), list);
			bool flag2 = false;
			for (int k = 0; k < list.Count; k++)
			{
				TileObject tileObject = list[k];
				if (!(tileObject is Flower) && !(tileObject is Trash))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				return null;
			}
			if (IsBlockingGate(dummy.GetMinTile(), dummy.GetMaxTile(), 1))
			{
				return null;
			}
			for (int l = 0; l < list.Count; l++)
			{
				TileObject tileObject2 = list[l];
				if (tileObject2 is Flower)
				{
					tileObject2.Delete();
				}
				if (tileObject2 is Trash)
				{
					tileObject2.Delete();
				}
			}
			if (dummy is Prop prop)
			{
				return Prop.Spawn(prop.Prototype, prop.Tile, prop.Orientation);
			}
			if (dummy is SingleTileProp singleTileProp)
			{
				return SingleTileProp.Spawn(singleTileProp.Prototype, singleTileProp.Tile);
			}
		}
		return null;
	}

	private TileObject SpawnPlaygroundPropIfNotBlocked(TileObject dummy, BiomeType[,] biome)
	{
		if (IsTileRectWithinBounds(dummy.GetMinTile(), dummy.GetMaxTile()))
		{
			List<TileObject> list = new List<TileObject>();
			GetObjectsInRect(dummy.GetMinTile(), dummy.GetMaxTile(), list);
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].CanBeClearedForBuilding(null, dummy))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return null;
			}
			for (int j = dummy.GetMinTile().x - 1; j <= dummy.GetMaxTile().x + 1; j++)
			{
				for (int k = dummy.GetMinTile().y - 1; k <= dummy.GetMaxTile().y + 1; k++)
				{
					if (IsTileOutsideBounds(j, k) || biome[j, k] != BiomeType.Meadow)
					{
						return null;
					}
				}
			}
			if (GetMaxHeightDiffInRect(dummy.GetMinTile(), dummy.GetMaxTile()) >= 0.5f)
			{
				return null;
			}
			Session.Instance.ClearTrashForBuilding(dummy.GetMinTile(), dummy.GetMaxTile(), null, dummy);
			return TileObject.SpawnProp(dummy.GetPropPrototype(), dummy.GetTile(), dummy.GetOrientationType());
		}
		return null;
	}

	public bool HasObstructions(TerrainCoord tile, int extents, bool ignoreIfPassable)
	{
		for (int i = tile.x - extents; i <= tile.x + extents; i++)
		{
			for (int j = tile.y - extents; j <= tile.y + extents; j++)
			{
				if (IsSlopeOrImpassableRaw(i, j))
				{
					return true;
				}
				TileObject fixedObjectOnTile = GetFixedObjectOnTile(i, j);
				if (fixedObjectOnTile != null && (!ignoreIfPassable || fixedObjectOnTile.IsImpassable(null, 5, new TerrainCoord(i, j))))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasObstructions(TerrainCoord tile, Prop.OrientationType orientation, PropPrototype typeToSpawn)
	{
		return HasObstructions(tile, orientation, typeToSpawn, 0, canBeNextToFence: false, notOnRoads: false, notOnRivers: false, null);
	}

	public bool HasObstructions(TerrainCoord tile, Prop.OrientationType orientation, PropPrototype typeToSpawn, int extra, bool canBeNextToFence, bool notOnRoads, bool notOnRivers, BiomeType[,] biome, bool notOnSlopeOrImpassableTerrain = true)
	{
		Prop prop = ((typeToSpawn != null) ? (typeToSpawn.ProtoInstance as Prop) : (BaseObjectManager.PrototypeGameObjects[207] as Prop));
		TerrainCoord minTile;
		TerrainCoord maxTile;
		if (prop != null)
		{
			prop.CalcMinMaxTile(tile, orientation, out minTile, out maxTile);
		}
		else
		{
			minTile = (maxTile = tile);
		}
		for (int i = minTile.x - extra; i <= maxTile.x + extra; i++)
		{
			for (int j = minTile.y - extra; j <= maxTile.y + extra; j++)
			{
				if (notOnSlopeOrImpassableTerrain ? IsSlopeOrImpassableRaw(i, j) : IsTileOutsideBounds(i, j))
				{
					return true;
				}
				if (notOnRivers && biome[i, j] == BiomeType.River)
				{
					return true;
				}
				if (notOnRoads && biome[i, j] == BiomeType.Road)
				{
					return true;
				}
				TileObject fixedObjectOnTile = GetFixedObjectOnTile(i, j);
				if (fixedObjectOnTile != null && (!canBeNextToFence || !(fixedObjectOnTile is BaseFence) || (i >= minTile.x && i <= maxTile.x && j >= minTile.y && j <= maxTile.y)))
				{
					return true;
				}
			}
		}
		if (typeToSpawn != null && typeToSpawn.ChildProps != null)
		{
			for (int k = 0; k < typeToSpawn.ChildProps.Length; k++)
			{
				ChildPropDef childPropDef = typeToSpawn.ChildProps[k];
				if (HasObstructions(tile + Prop.RotateByOrientation(childPropDef.Offset, orientation), orientation, null, extra, canBeNextToFence, notOnRoads, notOnRivers, biome))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsBlockingGate(TerrainCoord minTile, TerrainCoord maxTile, int extents)
	{
		for (int i = minTile.x - extents; i <= maxTile.x + extents; i++)
		{
			for (int j = minTile.y - extents; j <= maxTile.y + extents; j++)
			{
				TileObject fixedObjectOnTile = GetFixedObjectOnTile(i, j);
				if (fixedObjectOnTile is Gate)
				{
					return true;
				}
				if (fixedObjectOnTile is Well)
				{
					return true;
				}
			}
		}
		foreach (Prop allProp in Session.Instance.PropManager.AllProps)
		{
			if (!(allProp is Building building))
			{
				continue;
			}
			for (int k = 0; k < building.GetEntranceDefs().Length; k++)
			{
				if (GetTileCoordForPos(building.GetEntrancePos(k)).IsWithinBounds(minTile - new TerrainCoord(extents, extents), maxTile + new TerrainCoord(extents, extents)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsCloseToCampfire(List<Campfire> campfires, TerrainCoord tile, float minDist)
	{
		foreach (Campfire campfire in campfires)
		{
			if (tile.GetDistSquared(campfire.Tile) <= minDist * minDist)
			{
				return true;
			}
		}
		return false;
	}

	private TileObject GenerateProp(TerrainCoord tile, Prop.OrientationType orientation, PropPrototype buildingType, bool notOnRoads, bool notOnRivers, BiomeType[,] biome)
	{
		if (buildingType != null && !HasObstructions(tile, orientation, buildingType, 0, canBeNextToFence: false, notOnRoads, notOnRivers, biome))
		{
			TileObject tileObject = TileObject.SpawnProp(buildingType, tile, orientation);
			if (tileObject != null)
			{
				return tileObject;
			}
		}
		return null;
	}

	private TileObject GenerateCommunityBuilding(TerrainCoord tile, Prop.OrientationType orientation, PropPrototype buildingType, Community community)
	{
		if (buildingType != null && !HasObstructions(tile, orientation, buildingType))
		{
			TileObject tileObject = TileObject.SpawnProp(buildingType, tile, orientation);
			if (tileObject != null)
			{
				tileObject.SetCommunity(community);
				return tileObject;
			}
		}
		return null;
	}

	private BaseFence GenerateFence(TerrainCoord tile, PropPrototype fenceType, Community community, bool invulnerable = false)
	{
		if (fenceType != null && !HasObstructions(tile, 0, ignoreIfPassable: false) && SingleTileProp.Spawn(fenceType, tile) is BaseFence baseFence)
		{
			baseFence.SetCommunity(community);
			baseFence.SetForceInvulnerable(invulnerable);
			return baseFence;
		}
		return null;
	}

	public static Character GenerateCharacter(Community community, TerrainCoord tile, CustomRandom rand)
	{
		HumanAppearance humanAppearance = new HumanAppearance(rand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(rand));
		humanAppearance.Randomize(InfectionType.None, rand);
		Human human = Human.Spawn(tile, rand.RandomFloat() * (MathF.PI * 2f), humanAppearance, InfectionType.None);
		human.SetCommunity(community);
		human.InitialCommunity = community;
		human.Boxer = community.IsAISettlement() && community.GetBoxerCount() < 3 && rand.RandomChoice(0.25f);
		return human;
	}

	public static Chicken GenerateChicken(Community community, TerrainCoord tile, CustomRandom rand)
	{
		ChickenAppearance chickenAppearance = new ChickenAppearance((community.GetLivingNonZombieMemberCountBySpeciesGender(BaseObjectType.Chicken, GenderType.Female) == 0 || rand.RandomChoice(0.75f)) ? GenderType.Female : GenderType.Male, ChickenAppearance.PickRandomAge(rand));
		chickenAppearance.Randomize(rand);
		Chicken chicken = Chicken.Spawn(tile, rand.RandomFloat() * (MathF.PI * 2f), chickenAppearance);
		chicken.RandomizeName(rand);
		chicken.SetCommunity(community);
		chicken.InitialCommunity = community;
		return chicken;
	}

	public static void GenerateCharacterEquipment(Character character, string faction, CustomRandom rand)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < character.Roles.Count; i++)
		{
			switch (character.Roles[i].Role)
			{
			case Role.Lumberjack:
			case Role.Trapper:
				if (character.Inventory.FindItemOfClass(typeof(Axe)) == null)
				{
					Equipment equipment2 = Equipment.Spawn(GameImpl.Instance.PickRandomItemOfClass(typeof(Axe), rand));
					if (equipment2 != null)
					{
						character.Inventory.Add(character, equipment2);
					}
				}
				break;
			case Role.Farmer:
				if (character.Inventory.FindItemOfClass(typeof(WateringCan)) == null)
				{
					Equipment equipment = Equipment.Spawn(GameImpl.Instance.PickRandomItemOfClass(typeof(WateringCan), rand));
					if (equipment != null)
					{
						character.Inventory.Add(character, equipment);
					}
				}
				break;
			case Role.Cook:
			{
				flag4 = true;
				if (character.Inventory.FindItemOfType(EquipmentPrototype.Pot) == null)
				{
					character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.Pot));
				}
				int num = rand.Next(1, 4);
				for (int j = 0; j < num; j++)
				{
					character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.SealedContainer));
				}
				if (character.Inventory.FindItemOfClass(typeof(Axe)) == null)
				{
					EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomItemOfClass(typeof(Axe), rand);
					if (equipmentPrototype != null && character.HasInventorySpaceFor(equipmentPrototype.Weight))
					{
						character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype));
					}
				}
				break;
			}
			case Role.Guard:
				flag = true;
				break;
			case Role.Enforcer:
				flag2 = true;
				break;
			case Role.Trader:
				flag3 = true;
				break;
			}
		}
		if (character.Inventory.FindItemOfType(EquipmentPrototype.PlasticBottle) == null)
		{
			character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.PlasticBottle));
		}
		if (character.Inventory.FindItemOfType(EquipmentPrototype.Flint) == null && character.Inventory.FindItemOfType(EquipmentPrototype.Match) == null && character.Inventory.FindItemOfClass(typeof(HuntingKnife)) == null)
		{
			if (flag4 || rand.RandomChoice(0.5f))
			{
				character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.Flint));
				if (EquipmentPrototype.HuntingKnives.Count > 0)
				{
					character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.HuntingKnives[rand.Next(EquipmentPrototype.HuntingKnives.Count)]));
				}
			}
			else if (rand.RandomChoice(0.5f))
			{
				character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.Match, rand.Next(5, 11)));
			}
		}
		if (character.Inventory.FindItemOfType(EquipmentPrototype.Bandage) == null && rand.RandomChoice(0.75f))
		{
			character.Inventory.Add(character, Equipment.Spawn(EquipmentPrototype.Bandage, rand.Next(1, 3)));
		}
		LooterSpawnPoint.SetupSurvivor(character, faction, 0.75f, (flag || flag2) ? 1f : 0.5f, 0.25f, flag2 ? 0.95f : (flag ? 0.75f : (character.IsLooter() ? 0.25f : 0.1f)), flag2 ? 0.8f : (flag ? 0.65f : (character.IsLooter() ? 0.1f : 0f)), flag2 ? 0.95f : (flag ? 0.75f : (character.IsLooter() ? 0.25f : 0.1f)), rand);
		if (EquipmentPrototype.Gold != null && character.Inventory.FindItemOfType(EquipmentPrototype.Gold) == null)
		{
			int amount = rand.Next(flag3 ? 50 : 0, flag3 ? 100 : 10);
			character.SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.Gold, amount, fillLiquidContainers: true);
		}
		if (!flag3 || !Session.Instance.DifficultySettings.SaveTokensRequired || !Session.Instance.DifficultySettings.TradersHaveSaveTokens || character.Inventory.FindItemOfClass(typeof(SavegameToken)) != null)
		{
			return;
		}
		EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.PickRandomItemOfClass(typeof(SavegameToken), rand);
		if (equipmentPrototype2 != null)
		{
			int num2 = rand.Next(Character.TraderSaveTokenMin, Character.TraderSaveTokenMax + 1);
			if (num2 > 0)
			{
				character.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype2, num2, fillLiquidContainers: true);
			}
		}
	}

	private static int CountMineralDepositsOfType(List<MineralDeposit> mineralDeposits, MineralType mineralType, bool communitiesOnly)
	{
		int num = 0;
		for (int i = 0; i < mineralDeposits.Count; i++)
		{
			if (mineralDeposits[i].Type == mineralType && (!communitiesOnly || mineralDeposits[i].Community != null))
			{
				num++;
			}
		}
		return num;
	}

	private TerrainCoord GetNextPerimeterFencePostTile(TerrainCoord cur, TerrainCoord end)
	{
		float num = MathUtil.GetAngleToXZ(GetTileCentreXZ(cur), GetTileCentreXZ(end), 0f) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		if (num < 22.5f)
		{
			return cur + new TerrainCoord(0, 1);
		}
		if (num < 67.5f)
		{
			return cur + new TerrainCoord(1, 1);
		}
		if (num < 112.5f)
		{
			return cur + new TerrainCoord(1, 0);
		}
		if (num < 157.5f)
		{
			return cur + new TerrainCoord(1, -1);
		}
		if (num < 202.5f)
		{
			return cur + new TerrainCoord(0, -1);
		}
		if (num < 247.5f)
		{
			return cur + new TerrainCoord(-1, -1);
		}
		if (num < 292.5f)
		{
			return cur + new TerrainCoord(-1, 0);
		}
		if (num < 337.5f)
		{
			return cur + new TerrainCoord(-1, 1);
		}
		return cur + new TerrainCoord(0, 1);
	}

	private TerrainCoord PickRandomTileOnPerimeterEdge(CustomRandom rand, List<TerrainCoord> perimeter, float perimeterLength, float distFromPerimeter, out Prop.OrientationType orientation)
	{
		int num = 0;
		float num2 = rand.RandomFloat() * perimeterLength;
		float num3 = 0f;
		float t = 0f;
		for (int i = 0; i < perimeter.Count; i++)
		{
			Vector2 vector = perimeter[i].AsVector2();
			Vector2 vector2 = perimeter[(i + 1) % perimeter.Count].AsVector2();
			float magnitude = (vector - vector2).magnitude;
			if (num3 + magnitude >= num2)
			{
				num = i;
				t = (num2 - num3) / magnitude;
				break;
			}
			num3 += magnitude;
		}
		Vector2 tileCentreXZ = GetTileCentreXZ(perimeter[num]);
		Vector2 tileCentreXZ2 = GetTileCentreXZ(perimeter[(num + 1) % perimeter.Count]);
		Vector2 vector3 = Vector2.Lerp(tileCentreXZ, tileCentreXZ2, t);
		Vector2 v = MathUtil.SafeNormalize(tileCentreXZ2 - tileCentreXZ, Vector2.zero);
		Vector2 pos = vector3 + MathUtil.RightNormal(v) * distFromPerimeter;
		orientation = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromDir(MathUtil.LeftNormal(v), 0f));
		return GetTileCoordForPosXZ(pos);
	}

	private TerrainCoord PickRandomTileOnStraightPerimeterEdge(CustomRandom rand, List<TerrainCoord> perimeter, float distFromPerimeter, out Prop.OrientationType orientation)
	{
		int num = 0;
		for (int i = 0; i < perimeter.Count; i++)
		{
			TerrainCoord terrainCoord = perimeter[i];
			TerrainCoord terrainCoord2 = perimeter[(i + 1) % perimeter.Count];
			if (terrainCoord.x == terrainCoord2.x || terrainCoord.y == terrainCoord2.y)
			{
				num++;
			}
		}
		int num2 = rand.Next(num);
		int num3 = 0;
		for (int j = 0; j < perimeter.Count; j++)
		{
			TerrainCoord terrainCoord3 = perimeter[j];
			TerrainCoord terrainCoord4 = perimeter[(j + 1) % perimeter.Count];
			if (terrainCoord3.x == terrainCoord4.x || terrainCoord3.y == terrainCoord4.y)
			{
				if (num2 == 0)
				{
					num3 = j;
					break;
				}
				num2--;
			}
		}
		Vector2 tileCentreXZ = GetTileCentreXZ(perimeter[num3]);
		Vector2 tileCentreXZ2 = GetTileCentreXZ(perimeter[(num3 + 1) % perimeter.Count]);
		Vector2 vector = Vector2.Lerp(tileCentreXZ, tileCentreXZ2, 0.5f);
		Vector2 v = MathUtil.SafeNormalize(tileCentreXZ2 - tileCentreXZ, Vector2.zero);
		Vector2 pos = vector + MathUtil.RightNormal(v) * distFromPerimeter;
		orientation = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromDir(MathUtil.LeftNormal(v), 0f));
		return GetTileCoordForPosXZ(pos);
	}

	public void GenerateCommunity(CustomRandom rand, BiomeType[,] biome, int maxCommunities, int desiredNumNPCs, int iter, List<MineralDeposit> mineralDeposits)
	{
		Session instance = Session.Instance;
		CommunityManager communityManager = instance.CommunityManager;
		CharacterManager characterManager = instance.CharacterManager;
		Weather weather = instance.Weather;
		int amount = ((Size <= 256 || iter >= 500) ? 16 : 32);
		int num = ((Size <= 256) ? 16 : 32);
		bool flag = communityManager.Communities.Count == 0 && maxCommunities > 1 && iter < 500 && instance.DifficultySettings.SurvivorCampLooterPercentage > 0f && Size >= GetSizeFromMapSize(MapSize.Medium);
		int num2 = 25;
		int num3 = 10;
		if (iter < 500 && Size > 256 && characterManager.GetCharacterCountBySpecies(BaseObjectType.Human) < desiredNumNPCs / 2)
		{
			num3 = 20;
		}
		if (flag)
		{
			num3 = 25;
			num2 = 40;
		}
		TerrainCoord terrainCoord = new TerrainCoord(rand.Next(num3, num2 + 1), rand.Next(num3, num2 + 1));
		if (flag)
		{
			while (terrainCoord.x * terrainCoord.y < 900)
			{
				if (rand.RandomChoice(0.5f))
				{
					terrainCoord.x++;
				}
				else
				{
					terrainCoord.y++;
				}
			}
			while (terrainCoord.x * terrainCoord.y > 1000)
			{
				if (rand.RandomChoice(0.5f))
				{
					terrainCoord.x--;
				}
				else
				{
					terrainCoord.y--;
				}
			}
		}
		TerrainCoord terrainCoord2 = rand.RandomTile(new TerrainCoord(32 + num, 32 + num), new TerrainCoord(Size - 32 - num, Size - 32 - num));
		TerrainRect terrainRect = new TerrainRect(terrainCoord2 - terrainCoord, terrainCoord2 + terrainCoord);
		List<TerrainCoord> list = new List<TerrainCoord>();
		int num4 = Math.Min(terrainCoord.x, terrainCoord.y) / 2;
		TerrainRect terrainRect2 = terrainRect.Expand(num4);
		TerrainRect terrainRect3 = terrainRect.Expand(-terrainCoord / 2);
		TerrainCoord terrainCoord3 = rand.RandomTileOnOutsideEdge(terrainRect.min, terrainRect.max, num4);
		list.Add(terrainCoord3);
		Vector2 tileCentreXZ = GetTileCentreXZ(terrainCoord2);
		float magnitude = (tileCentreXZ - GetTileCentreXZ(terrainCoord3)).magnitude;
		float num5 = 0f;
		int num6 = 0;
		float num7 = (rand.RandomFloat() - 0.5f) * 2f * (MathF.PI / 4f);
		while (true)
		{
			num6++;
			if (num6 >= 1000)
			{
				return;
			}
			Vector2 tileCentreXZ2 = GetTileCentreXZ(terrainCoord3);
			float angleToXZ = MathUtil.GetAngleToXZ(tileCentreXZ, tileCentreXZ2, 0f);
			float num8 = Mathf.Lerp(4f, 16f, rand.RandomFloat());
			float num9 = Mathf.Clamp(num7 + (rand.RandomFloat() - 0.5f) * (MathF.PI / 4f), -MathF.PI / 4f, MathF.PI / 4f);
			Vector2 vector = tileCentreXZ2 + MathUtil.GetDirFromAngle(angleToXZ + MathF.PI / 2f + num9) * num8;
			while (terrainRect3.Contains(GetTileCoordForPosXZ(vector)) && num9 > -MathF.PI / 4f)
			{
				num9 -= rand.RandomFloat() * 0.25f * (MathF.PI / 4f);
				num9 = Math.Max(num9, -MathF.PI / 4f);
				num8 = Mathf.Lerp(4f, 16f, rand.RandomFloat());
				vector = tileCentreXZ2 + MathUtil.GetDirFromAngle(angleToXZ + MathF.PI / 2f + num9) * num8;
			}
			while (!terrainRect2.Contains(GetTileCoordForPosXZ(vector)) && num9 < MathF.PI / 4f)
			{
				num9 += rand.RandomFloat() * 0.25f * (MathF.PI / 4f);
				num9 = Math.Min(num9, MathF.PI / 4f);
				num8 = Mathf.Lerp(4f, 16f, rand.RandomFloat());
				vector = tileCentreXZ2 + MathUtil.GetDirFromAngle(angleToXZ + MathF.PI / 2f + num9) * num8;
			}
			float magnitude2 = (tileCentreXZ - vector).magnitude;
			magnitude2 = Mathf.Lerp(magnitude2, magnitude, Mathf.Clamp01((num5 - 4.712389f) / (MathF.PI / 2f)));
			vector = tileCentreXZ + MathUtil.SafeNormalize(vector - tileCentreXZ, Vector2.zero) * magnitude2;
			TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(vector);
			if (!(tileCoordForPosXZ == terrainCoord3))
			{
				float num10 = MathUtil.AngleDiff(angleToXZ, MathUtil.GetAngleToXZ(tileCentreXZ, GetTileCentreXZ(tileCoordForPosXZ), 0f));
				num5 += num10;
				if (num5 >= MathF.PI * 2f)
				{
					break;
				}
				num7 = num9;
				terrainCoord3 = tileCoordForPosXZ;
				list.Add(terrainCoord3);
			}
		}
		List<int> list2 = new List<int>();
		int num11 = rand.Next(3, Math.Min(7, list.Count / 3));
		int num12 = 0;
		while (num12 < num11 * 10)
		{
			num12++;
			int num13 = rand.Next(list.Count);
			if (list2.Contains(num13) || list2.Contains(num13 - 1) || list2.Contains(num13 + 1))
			{
				continue;
			}
			TerrainCoord terrainCoord4 = list[num13];
			TerrainCoord terrainCoord5 = list[(num13 + 1) % list.Count];
			if (Math.Abs(terrainCoord5.x - terrainCoord4.x) < 4 && Math.Abs(terrainCoord5.y - terrainCoord4.y) < 4)
			{
				continue;
			}
			list2.Add(num13);
			if (list2.Count < num11)
			{
				continue;
			}
			float num14 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				TerrainCoord terrainCoord6 = (list[i] + list[(i + 1) % list.Count]) / 2;
				float num15 = float.MaxValue;
				for (int j = 0; j < list2.Count; j++)
				{
					TerrainCoord other = (list[list2[j]] + list[(list2[j] + 1) % list.Count]) / 2;
					num15 = Math.Min(num15, terrainCoord6.GetDist(other));
				}
				num14 = Math.Max(num14, num15);
			}
			if (num14 <= 24f)
			{
				break;
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			int num16 = list2[k];
			TerrainCoord value = list[num16];
			TerrainCoord value2 = list[(num16 + 1) % list.Count];
			if (Math.Abs(value2.x - value.x) >= Math.Abs(value2.y - value.y))
			{
				value2.y = (value.y = (value2.y + value.y) / 2);
			}
			else
			{
				value2.x = (value.x = (value2.x + value.x) / 2);
			}
			list[num16] = value;
			list[(num16 + 1) % list.Count] = value2;
		}
		for (int l = 0; l < list.Count; l++)
		{
			TerrainCoord terrainCoord7 = list[l];
			TerrainCoord terrainCoord8 = list[(l + 1) % list.Count];
			if (terrainCoord7.x == terrainCoord8.x || terrainCoord7.y == terrainCoord8.y || Math.Abs(terrainCoord8.x - terrainCoord7.x) == Math.Abs(terrainCoord8.y - terrainCoord7.y))
			{
				continue;
			}
			TerrainCoord item;
			if (Math.Abs(terrainCoord8.x - terrainCoord7.x) > Math.Abs(terrainCoord8.y - terrainCoord7.y))
			{
				int num17 = Math.Sign(terrainCoord8.x - terrainCoord7.x) * Math.Abs(terrainCoord8.y - terrainCoord7.y);
				item = ((!rand.RandomChoice(0.5f)) ? new TerrainCoord(terrainCoord7.x + num17, terrainCoord8.y) : new TerrainCoord(terrainCoord8.x - num17, terrainCoord7.y));
			}
			else
			{
				int num18 = Math.Sign(terrainCoord8.y - terrainCoord7.y) * Math.Abs(terrainCoord8.x - terrainCoord7.x);
				item = ((!rand.RandomChoice(0.5f)) ? new TerrainCoord(terrainCoord8.x, terrainCoord7.y + num18) : new TerrainCoord(terrainCoord7.x, terrainCoord8.y - num18));
			}
			for (int m = 0; m < list2.Count; m++)
			{
				if (list2[m] > l)
				{
					list2[m]++;
				}
			}
			list.Insert(l + 1, item);
			l++;
		}
		for (int n = 0; n < list.Count; n++)
		{
			Vector2 tileCentreXZ3 = GetTileCentreXZ(list[n]);
			Vector2 tileCentreXZ4 = GetTileCentreXZ(list[(n + list.Count - 1) % list.Count]);
			if (MathUtil.GetAngleBetweenNormalizedDirs(b: MathUtil.SafeNormalize(GetTileCentreXZ(list[(n + list.Count + 1) % list.Count]) - tileCentreXZ3, Vector2.zero), a: MathUtil.SafeNormalize(tileCentreXZ4 - tileCentreXZ3, Vector2.zero)) < MathF.PI / 3f)
			{
				return;
			}
		}
		for (int num19 = 0; num19 < list.Count; num19++)
		{
			Vector2 tileCentreXZ5 = GetTileCentreXZ(list[num19]);
			Vector2 tileCentreXZ6 = GetTileCentreXZ(list[(num19 + 1) % list.Count]);
			for (int num20 = 0; num20 < list.Count; num20++)
			{
				if (num19 != num20)
				{
					Vector2 tileCentreXZ7 = GetTileCentreXZ(list[num20]);
					Vector2 tileCentreXZ8 = GetTileCentreXZ(list[(num20 + 1) % list.Count]);
					if (MathUtil.GetLineIntersection(tileCentreXZ5, tileCentreXZ6, tileCentreXZ7, tileCentreXZ8, out var a, out var b) && a > 0.001f && a < 0.999f && b > 0.001f && b < 0.999f)
					{
						return;
					}
				}
			}
		}
		terrainRect = new TerrainRect(terrainCoord2, terrainCoord2);
		float num21 = 0f;
		for (int num22 = 0; num22 < list.Count; num22++)
		{
			terrainRect = terrainRect.Include(list[num22]);
			num21 += (list[(num22 + 1) % list.Count].AsVector2() - list[num22].AsVector2()).magnitude;
		}
		terrainCoord2 = terrainRect.Centre;
		terrainCoord = (terrainRect.max + new TerrainCoord(1, 1) - terrainRect.min) / 2;
		if (terrainCoord.x < num3 || terrainCoord.x > num2 || terrainCoord.y < num3 || terrainCoord.y > num2)
		{
			return;
		}
		foreach (Town town in communityManager.Towns)
		{
			if (terrainRect.Overlaps(town.GetTownRect().Expand(amount)))
			{
				return;
			}
		}
		foreach (Community community2 in communityManager.Communities)
		{
			if (terrainRect.Overlaps(community2.BaseRect.Expand(num)))
			{
				return;
			}
		}
		for (int num23 = 0; num23 < list.Count; num23++)
		{
			TerrainCoord terrainCoord9 = list[num23];
			TerrainCoord terrainCoord10 = list[(num23 + 1) % list.Count];
			while (terrainCoord9 != terrainCoord10)
			{
				if (IsSlopeOrImpassableRaw(terrainCoord9.x, terrainCoord9.y))
				{
					return;
				}
				terrainCoord9 = GetNextPerimeterFencePostTile(terrainCoord9, terrainCoord10);
			}
		}
		int num24 = 0;
		for (int num25 = terrainRect.min.x; num25 <= terrainRect.max.x; num25++)
		{
			for (int num26 = terrainRect.min.y; num26 <= terrainRect.max.y; num26++)
			{
				if (IsTileRiver(num25, num26))
				{
					return;
				}
				if (IsSlopeOrImpassableRaw(num25, num26))
				{
					num24++;
				}
			}
		}
		if (num24 >= terrainRect.TilesArea / 8)
		{
			return;
		}
		Community community = Community.Spawn((!flag && !rand.RandomChoice(instance.DifficultySettings.SurvivorCampLooterPercentage / 100f)) ? CommunityType.Normal : CommunityType.Looter);
		community.Nemesis = flag;
		community.CommunityName.Randomise(rand, unique: true, community);
		community.BaseRect = terrainRect;
		community.Perimeter = list;
		float num27 = ((float)(terrainCoord.x + terrainCoord.y) * 0.5f - (float)num3) / (float)(num2 - num3);
		PropPrototype propPrototype = ((flag || rand.RandomChoice(num27 * num27)) ? PropPrototype.ConcreteWall : (rand.RandomChoice(0.5f) ? PropPrototype.WireFence : PropPrototype.WoodFence));
		PropPrototype buildingType = ((propPrototype == PropPrototype.WoodFence) ? PropPrototype.WoodGate : PropPrototype.WireGate);
		bool flag2 = false;
		for (int num28 = 0; num28 < list2.Count; num28++)
		{
			int num29 = list2[num28];
			TerrainCoord terrainCoord11 = list[num29];
			TerrainCoord terrainCoord12 = list[(num29 + 1) % list.Count];
			TerrainCoord tile = (terrainCoord11 + terrainCoord12) / 2;
			if (GenerateCommunityBuilding(orientation: (Math.Abs(terrainCoord12.x - terrainCoord11.x) < Math.Abs(terrainCoord12.y - terrainCoord11.y)) ? (MathUtil.IsTileInPolygon(new TerrainCoord(tile.x + 1, tile.y), list) ? Prop.OrientationType.Deg180 : Prop.OrientationType.Deg0) : (MathUtil.IsTileInPolygon(new TerrainCoord(tile.x, tile.y + 1), list) ? Prop.OrientationType.Deg90 : Prop.OrientationType.Deg270), tile: tile, buildingType: buildingType, community: community) is Gate gate)
			{
				gate.SetBlockAnimals(blockAnimals: true);
				flag2 = true;
			}
		}
		if (!flag2)
		{
			community.Delete();
			return;
		}
		MineralType mineralType = MineralType.None;
		List<MineralScore> list3 = new List<MineralScore>();
		list3.Add(new MineralScore
		{
			Type = MineralType.None,
			Score = 0.4f
		});
		for (int num30 = 1; num30 < 4; num30++)
		{
			list3.Add(new MineralScore
			{
				Type = (MineralType)num30,
				Score = list3[list3.Count - 1].Score + 0.2f * instance.DifficultySettings.GetMineralDensity((MineralType)num30)
			});
		}
		float num31 = rand.RandomFloat() * list3[list3.Count - 1].Score;
		for (int num32 = 0; num32 < list3.Count; num32++)
		{
			if (num31 < list3[num32].Score)
			{
				mineralType = list3[num32].Type;
				break;
			}
		}
		for (int num33 = 1; num33 < 4; num33++)
		{
			if (CountMineralDepositsOfType(mineralDeposits, (MineralType)num33, communitiesOnly: true) == 0 && instance.DifficultySettings.GetMineralDensity((MineralType)num33) > 0f)
			{
				mineralType = (MineralType)num33;
				break;
			}
		}
		for (int num34 = 0; num34 < list.Count; num34++)
		{
			TerrainCoord terrainCoord13 = list[num34];
			TerrainCoord terrainCoord14 = list[(num34 + 1) % list.Count];
			while (terrainCoord13 != terrainCoord14)
			{
				GenerateFence(terrainCoord13, propPrototype, community);
				terrainCoord13 = GetNextPerimeterFencePostTile(terrainCoord13, terrainCoord14);
			}
		}
		int num35 = Math.Max(2, (terrainCoord.x + terrainCoord.y) / 8);
		int num36 = Math.Max(1, num35 / 2);
		if (propPrototype != PropPrototype.WireFence)
		{
			num36 = 4;
			num35 = Math.Max(num35, num36);
		}
		int num37 = rand.Next(num36, num35 + 1);
		int num38 = 0;
		int num39 = 0;
		while (num38 < num37 && num39 < 1000)
		{
			num39++;
			int num40 = 1;
			PropPrototype propPrototype2 = null;
			switch (rand.Next((propPrototype == PropPrototype.WireFence) ? 3 : 2))
			{
			case 0:
				propPrototype2 = PropPrototype.WatchTower;
				break;
			case 1:
				propPrototype2 = PropPrototype.ConcreteWatchTower;
				break;
			case 2:
				propPrototype2 = PropPrototype.Pillbox;
				num40 = 2;
				break;
			}
			Prop.OrientationType orientation;
			TerrainCoord tile2 = PickRandomTileOnPerimeterEdge(rand, list, num21, num40, out orientation);
			if (HasObstructions(tile2, orientation, propPrototype2, 1, canBeNextToFence: true, notOnRoads: false, notOnRivers: true, biome))
			{
				continue;
			}
			propPrototype2.CalcMinMaxTile(tile2, orientation, out var minTile, out var maxTile);
			if (!IsBlockingGate(minTile, maxTile, 1))
			{
				TileObject.SpawnProp(propPrototype2, tile2, orientation).SetCommunity(community);
				num38++;
				if (IsFence(minTile.x - 1, minTile.y, vert: false, horiz: false) && IsFence(minTile.x, minTile.y - 1, vert: false, horiz: false))
				{
					GenerateFence(new TerrainCoord(minTile.x - 1, minTile.y - 1), propPrototype, community);
				}
				if (IsFence(minTile.x - 1, maxTile.y, vert: false, horiz: false) && IsFence(minTile.x, maxTile.y + 1, vert: false, horiz: false))
				{
					GenerateFence(new TerrainCoord(minTile.x - 1, maxTile.y + 1), propPrototype, community);
				}
				if (IsFence(maxTile.x + 1, maxTile.y, vert: false, horiz: false) && IsFence(maxTile.x, maxTile.y + 1, vert: false, horiz: false))
				{
					GenerateFence(new TerrainCoord(maxTile.x + 1, maxTile.y + 1), propPrototype, community);
				}
				if (IsFence(maxTile.x + 1, minTile.y, vert: false, horiz: false) && IsFence(maxTile.x, minTile.y - 1, vert: false, horiz: false))
				{
					GenerateFence(new TerrainCoord(maxTile.x + 1, minTile.y - 1), propPrototype, community);
				}
			}
		}
		int num41 = terrainCoord.x * terrainCoord.y / 25;
		int minValue = Math.Max(4, flag ? num41 : (num41 / 2));
		int num42 = rand.Next(minValue, num41 + 1);
		int num43 = rand.Next(1, 2 + num42 / 10);
		int num44 = rand.Next(1, 2 + num42 / 15);
		int num45 = 0;
		if (rand.RandomChoice(0.5f) || characterManager.GetCharacterCountBySpecies(BaseObjectType.Chicken) == 0)
		{
			num45 = ((num42 >= 8) ? rand.Next(num42 / 8, num42 / 4 + 1) : 0);
		}
		List<PropPrototype> list4 = new List<PropPrototype>();
		foreach (PropPrototype plantableCropType in PropPrototype.PlantableCropTypes)
		{
			if (plantableCropType.HarvestPrototype != null && plantableCropType.HarvestPrototype.Nutrition > 0f)
			{
				LootScarcity lootScarcity = ((plantableCropType.HarvestSeedsPrototype != null) ? plantableCropType.HarvestSeedsPrototype.Scarcity : plantableCropType.HarvestPrototype.Scarcity);
				for (int num46 = 0; num46 < (int)lootScarcity; num46++)
				{
					list4.Add(plantableCropType);
				}
			}
		}
		List<PropPrototype> accomodationTypes = PropPrototype.GetAccomodationTypes("Camp/Buildings");
		int num47 = 0;
		List<Campfire> list5 = new List<Campfire>();
		List<Building> list6 = new List<Building>();
		while (num47 < 2000)
		{
			TerrainCoord terrainCoord15 = terrainCoord;
			num47++;
			bool flag3 = false;
			PropPrototype propPrototype3 = null;
			if (mineralType != MineralType.None && community.GetNumCompletedBuildingsOfClass(typeof(Mine)) < 1)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(Mine)));
				terrainCoord15 = terrainCoord / 2;
			}
			else if (community.GetNumCompletedBuildingsOfClass(typeof(Well)) < num44)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(Well)));
			}
			else if (community.GetNumCompletedBuildingsOfClass(typeof(Campfire)) < num42 / 8 + 1)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(Campfire)));
			}
			else if (community.GetAccommodation() < num42)
			{
				propPrototype3 = accomodationTypes[rand.Next(accomodationTypes.Count)];
			}
			else if (community.GetNumOuthouses() < Math.Max(1, Community.GetDesiredNumOuthouses(num42)))
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(Outhouse)));
			}
			else if (community.GetNumCompletedBuildingsOfClass(typeof(WorkBench)) < num43)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(WorkBench)));
				flag3 = true;
			}
			else if (num45 > 0 && community.GetChickenAccommodation() < num45)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesEnterableBySpecies(BaseObjectType.Chicken));
			}
			else if (community.GetNumCompletedBuildingsOfClass(typeof(AnimalFeederProp)) < (num45 + 3) / 4)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(AnimalFeederProp)));
			}
			else if (community.GetNumCompletedBuildingsOfClass(typeof(AnimalDrinkerProp)) < (num45 + 3) / 4)
			{
				propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithClass(typeof(AnimalDrinkerProp)));
			}
			if (propPrototype3 == null)
			{
				break;
			}
			TerrainCoord terrainCoord16 = rand.RandomTile(terrainCoord2 - new TerrainCoord(terrainCoord15.x - 1, terrainCoord15.y - 1), terrainCoord2 + new TerrainCoord(terrainCoord15.x - 1, terrainCoord15.y - 1));
			Prop.OrientationType orientation2 = (Prop.OrientationType)rand.Next(4);
			bool canBeNextToFence = false;
			if (flag3)
			{
				terrainCoord16 = PickRandomTileOnStraightPerimeterEdge(rand, list, 1f, out orientation2);
				orientation2 = (Prop.OrientationType)((int)(orientation2 + 1) % 4);
				canBeNextToFence = true;
			}
			else if (!MathUtil.IsTileInPolygon(terrainCoord16, list))
			{
				continue;
			}
			if (HasObstructions(terrainCoord16, orientation2, propPrototype3, 2, canBeNextToFence, notOnRoads: false, notOnRivers: true, biome))
			{
				continue;
			}
			TileObject tileObject = TileObject.SpawnProp(propPrototype3, terrainCoord16, orientation2);
			tileObject.SetCommunity(community);
			if (tileObject is WorkBench workBench)
			{
				workBench.SetVariation(0);
			}
			if (tileObject is Campfire campfire)
			{
				campfire.LightFire(null);
				list5.Add(campfire);
				for (int num48 = terrainCoord16.x - Prop.FlattenTerrainBorder; num48 <= terrainCoord16.x + Prop.FlattenTerrainBorder; num48++)
				{
					for (int num49 = terrainCoord16.y - Prop.FlattenTerrainBorder; num49 <= terrainCoord16.y + Prop.FlattenTerrainBorder; num49++)
					{
						if (!(terrainCoord16.GetDist(new TerrainCoord(num48, num49)) > (float)Prop.FlattenTerrainBorder + 0.1f))
						{
							biome[num48, num49] = BiomeType.Footpath;
						}
					}
				}
			}
			if (tileObject is Building building && building.IsAccommodation())
			{
				list6.Add(building);
			}
			if (tileObject is Mine mine)
			{
				GenerateMineralDeposit(mineralType, community, mine.Tile, community.BaseRect.TilesArea / 4, rand, mineralDeposits);
			}
		}
		community.SetCraftingLimit(EquipmentPrototype.Wood, list5.Count * WoodLimitPerCampfire);
		int num50 = 0;
		while (community.Members.Count < num42 && num50 < 1000)
		{
			num50++;
			TerrainCoord terrainCoord17 = rand.RandomTile(terrainCoord2 - new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1), terrainCoord2 + new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1));
			if (MathUtil.IsTileInPolygon(terrainCoord17, list) && !HasObstructions(terrainCoord17, 1, ignoreIfPassable: true))
			{
				GenerateCharacter(community, terrainCoord17, rand);
			}
		}
		if (community.Members.Count > 0)
		{
			int index = rand.Next(community.Members.Count);
			community.Members[index].SetRank(Rank.Leader);
		}
		int num51 = 0;
		int count = community.Members.Count;
		while (count > 0 && community.Members.Count - count < num45 && num51 < 1000)
		{
			num51++;
			TerrainCoord terrainCoord18 = rand.RandomTile(terrainCoord2 - new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1), terrainCoord2 + new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1));
			if (MathUtil.IsTileInPolygon(terrainCoord18, list) && !HasObstructions(terrainCoord18, 1, ignoreIfPassable: true))
			{
				GenerateChicken(community, terrainCoord18, rand);
			}
		}
		int[] array = new int[18];
		community.CalcDesiredNumberOfMembersAssignedToRoles(array, canAddTraders: true);
		int num52 = 0;
		int num53 = 0;
		int num54 = 0;
		int num55 = 0;
		int num56 = 0;
		int num57 = 0;
		List<Character> list7 = new List<Character>();
		foreach (Character member in community.Members)
		{
			if (member.GetBaseObjectType() == BaseObjectType.Human)
			{
				if (num55 < array[8])
				{
					member.AddRole(new RoleInfo(Role.Lumberjack));
					num55++;
				}
				else if (list7.Count < array[1])
				{
					member.AddRole(new RoleInfo(Role.Farmer));
					list7.Add(member);
				}
				else if (num56 < array[9])
				{
					member.AddRole(new RoleInfo(Role.Cook, list5[num56].Tile));
					num56++;
				}
				else if (num57 < array[14])
				{
					member.AddRole(new RoleInfo(Role.AnimalFeeder));
					num57++;
				}
				else if (num54 < array[10])
				{
					member.AddRole(new RoleInfo(Role.Trader));
					num54++;
				}
				else if (num52 < array[3])
				{
					member.AddRole(new RoleInfo(Role.Guard));
					num52++;
				}
				else if (num53 < array[15])
				{
					member.AddRole(new RoleInfo(Role.Enforcer));
					num53++;
				}
				GenerateCharacterEquipment(member, community.IsLooterCommunity() ? PersonalityGroup.LooterFaction : PersonalityGroup.NormalFaction, rand);
				if (member.HasRole(Role.Farmer) && member.Skillset.Farming == 0)
				{
					UnityEngine.Debug.LogError("wtf?");
				}
			}
		}
		int num58 = 0;
		List<PlantableCrop> list8 = new List<PlantableCrop>();
		float num59 = ((float)num42 + (float)num45 / Chicken.NutritionFactor) * Community.RecommendedPlantedNutritionDays * Sun.DayLengthSecs * 2f;
		float num60 = 0f;
		while (num60 < num59 && num58 < 1000 && list4.Count > 0)
		{
			num58++;
			TerrainRect terrainRect4 = new TerrainRect(terrainCoord2 - new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1), terrainCoord2 + new TerrainCoord(terrainCoord.x - 1, terrainCoord.y - 1));
			TerrainCoord terrainCoord19 = rand.RandomTile(terrainRect4.min, terrainRect4.max);
			if (!MathUtil.IsTileInPolygon(terrainCoord19, list))
			{
				continue;
			}
			int num61 = rand.Next(3, 7);
			PropPrototype propPrototype4 = list4[rand.Next() % list4.Count];
			for (int num62 = -num61 / 2; num62 <= num61; num62++)
			{
				for (int num63 = -num61 / 2; num63 <= num61; num63++)
				{
					TerrainCoord terrainCoord20 = terrainCoord19 + new TerrainCoord(num62, num63);
					if (terrainRect4.Contains(terrainCoord20) && MathUtil.IsTileInPolygon(terrainCoord20, list) && !IsTileOutsideBounds(terrainCoord20.x, terrainCoord20.y) && !IsTileRoadOrRiver(terrainCoord20.x, terrainCoord20.y) && !IsBlockingGate(terrainCoord20, terrainCoord20, (num58 >= 500) ? 1 : 4) && !HasObstructions(terrainCoord20, 0, ignoreIfPassable: false) && !IsCloseToCampfire(list5, terrainCoord20, (num58 < 500) ? TileObject.MaxFireHeatRange : (TileObject.MaxFireHeatRange * 0.5f)))
					{
						int minAIFarmerSkill = Skillset.MinAIFarmerSkill;
						PlantableCrop plantableCrop = PlantableCrop.Spawn(propPrototype4, terrainCoord20, community, minAIFarmerSkill, rand);
						list8.Add(plantableCrop);
						instance.CropsManager.SetPlantableCropType(community.Id, terrainCoord20, propPrototype4);
						float predictedYield = plantableCrop.GetPredictedYield();
						EquipmentPrototype harvestPrototype = plantableCrop.GetHarvestPrototype();
						if (harvestPrototype != null)
						{
							num60 += harvestPrototype.GetNutrition() * predictedYield;
						}
					}
				}
			}
		}
		if (num60 < num59)
		{
			UnityEngine.Debug.LogWarning(community.GetDisplayNameString() + ": Not enough planted nutrition: " + num60 + " / " + num59);
		}
		int buildingIndex = 0;
		float spawnedWeight = 0f;
		if (list6.Count > 0)
		{
			int num64 = rand.Next(1, 11);
			for (int num65 = 0; num65 < num64; num65++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, EquipmentPrototype.SealedContainer);
			}
			int num66 = rand.Next(1, 11);
			for (int num67 = 0; num67 < num66; num67++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, EquipmentPrototype.PlasticBottle);
			}
			int num68 = rand.Next(1, 4);
			for (int num69 = 0; num69 < num68; num69++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, EquipmentPrototype.Pot);
			}
			int num70 = rand.Next(1, 3);
			for (int num71 = 0; num71 < num70; num71++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, GameImpl.Instance.PickRandomItemOfClass(typeof(Shovel), rand));
			}
			int num72 = rand.Next(1, 3);
			for (int num73 = 0; num73 < num72; num73++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, GameImpl.Instance.PickRandomItemOfClass(typeof(Toolbox), rand));
			}
			int num74 = rand.Next(1, 3);
			for (int num75 = 0; num75 < num74; num75++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, GameImpl.Instance.PickLargeBackpack(rand));
			}
			if (list8.Count > 0)
			{
				float num76 = Weather.CalcNutritionNeededToStoreForWinterPerPerson(instance.DayOfYear, rampUpOverPlantingSeason: true) * (float)community.Members.Count;
				foreach (CropPatch patch in instance.CropsManager.Patches)
				{
					if (patch.CommunityId == community.Id && patch.CropsInPatch.Count > 0)
					{
						int num77 = rand.Next(Math.Max(patch.CropsInPatch.Count / 2, 1), patch.CropsInPatch.Count * 2);
						if (weather.TemperatureInCelsius <= 0f)
						{
							num77 += patch.CropsInPatch.Count;
						}
						EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindSeedPrototypeForPlantType(patch.CropType);
						if (equipmentPrototype != null && num77 > 0)
						{
							SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, equipmentPrototype, num77);
						}
					}
				}
				int num78 = 0;
				while (num76 > 0f && num78 < 10000 && buildingIndex < list6.Count)
				{
					num78++;
					PlantableCrop plantableCrop2 = list8[rand.Next(list8.Count)];
					if (plantableCrop2.GetHarvestPrototype() != null)
					{
						num76 -= SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, plantableCrop2.GetHarvestPrototype());
					}
					else
					{
						UnityEngine.Debug.Log("Harvest prototype not found for plant: " + plantableCrop2.GetDisplayNameString());
					}
				}
			}
			buildingIndex = Math.Min(buildingIndex + 1, list6.Count - 1);
			int num79 = rand.Next(1, community.Members.Count * 2);
			for (int num80 = 0; num80 < num79; num80++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, EquipmentPrototype.Bandage);
			}
			buildingIndex = Math.Min(buildingIndex + 1, list6.Count - 1);
			int num81 = rand.Next(community.Members.Count, community.Members.Count * 2);
			for (int num82 = 0; num82 < num81; num82++)
			{
				if (buildingIndex >= list6.Count)
				{
					break;
				}
				float num83 = rand.RandomFloat() * 100f;
				ClothingType clothingType = ((num83 < 40f) ? ClothingType.Bottom : ((num83 < 80f) ? ClothingType.Top : ((!(num83 < 85f)) ? ((num83 < 90f) ? ClothingType.Shoes : ((num83 < 95f) ? ClothingType.Backpack : ((num83 < 98f) ? ClothingType.BodyArmor : ClothingType.LegArmor))) : ClothingType.Hat)));
				EquipmentPrototype proto = GameImpl.Instance.PickRandomClothing(LootLocationDef.Survivor, clothingType, mustPickSomething: true, rand);
				SpawnCommunitySupplies(list6, ref buildingIndex, ref spawnedWeight, proto);
			}
		}
		if (weather.TemperatureInCelsius <= 0f)
		{
			foreach (PlantableCrop item2 in list8)
			{
				item2.Delete();
			}
		}
		community.RandomizeRelationships(rand);
		if (community.CommunityType != CommunityType.Looter)
		{
			return;
		}
		int num84 = Mathf.CeilToInt(num21 * Mathf.Lerp(0.125f, 0.25f, rand.RandomFloat()));
		int num85 = 0;
		int num86 = 0;
		while (num85 < num84 && num86 < num84 * 10)
		{
			Prop.OrientationType orientation3;
			TerrainCoord tile3 = PickRandomTileOnPerimeterEdge(rand, list, num21, 0f - Mathf.Lerp(4f, 16f, rand.RandomFloat()), out orientation3);
			orientation3 = (Prop.OrientationType)((int)(orientation3 + 2) % 4);
			Prop.GetDirFromOrientationType(orientation3);
			PropPrototype propPrototype5 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithCategory("Camp/LooterProps"));
			if (propPrototype5 != null)
			{
				TileObject tileObject2 = GenerateProp(tile3, orientation3, propPrototype5, notOnRoads: true, notOnRivers: true, biome);
				if (tileObject2 != null)
				{
					tileObject2.SetCommunity(community);
					num85++;
				}
			}
			num86++;
		}
	}

	private float SpawnCommunitySupplies(List<Building> buildings, ref int buildingIndex, ref float spawnedWeight, EquipmentPrototype proto)
	{
		return SpawnCommunitySupplies(buildings, ref buildingIndex, ref spawnedWeight, proto, 1);
	}

	private float SpawnCommunitySupplies(List<Building> buildings, ref int buildingIndex, ref float spawnedWeight, EquipmentPrototype proto, int amount)
	{
		if (proto == null)
		{
			return 0f;
		}
		if (buildingIndex >= buildings.Count)
		{
			return 0f;
		}
		Building building = buildings[buildingIndex];
		if (spawnedWeight + proto.Weight * (float)amount > building.GetMaxInventoryWeight() * 0.75f)
		{
			buildingIndex++;
			spawnedWeight = 0f;
			if (buildingIndex >= buildings.Count)
			{
				return 0f;
			}
		}
		Equipment equipment = Equipment.Spawn(proto, amount);
		spawnedWeight += equipment.GetWeight();
		float result = equipment.GetNutrition() * (float)amount;
		building.Inventory.Add(building, equipment);
		return result;
	}

	private void GenerateMapEdgeWall(TerrainPath path, PropPrototype fenceType, TerrainCoord startTile, TerrainCoord wallDir)
	{
		int num = Mathf.CeilToInt(path.DefaultWidth);
		TerrainCoord tile = startTile;
		int num2 = 0;
		while (num2 < num && !IsSlopeOrImpassableRaw(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			num2++;
			tile += wallDir;
		}
		tile = startTile - wallDir;
		int num3 = 1;
		while (num3 < num && !IsSlopeOrImpassableRaw(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			num3++;
			tile -= wallDir;
		}
	}

	private void GenerateMapEdgeInnerWall(TerrainPath path, PropPrototype fenceType, PropPrototype gateType, TerrainCoord startTile, TerrainCoord wallDir, TerrainCoord pathDir, float[,] edgeMountainAmount)
	{
		int num = Mathf.CeilToInt(path.DefaultWidth * 0.5f + 16f);
		Gate gate = null;
		if (gateType != null)
		{
			Prop.OrientationType closestOrientationTypeToAngle = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromDir(new Vector2(-pathDir.y, pathDir.x), 0f));
			gate = GenerateProp(startTile, closestOrientationTypeToAngle, gateType, notOnRoads: false, notOnRivers: false, null) as Gate;
			gate?.Lock();
		}
		TerrainCoord tile = startTile + wallDir * ((gate != null) ? 2 : 0);
		int num2 = 0;
		while (num2 < num && !IsSlopeOrImpassableRaw(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			num2++;
			tile += wallDir;
		}
		while (!IsSlopeOrImpassableRaw(tile.x, tile.y) && !IsTileOutsideBounds(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			tile -= pathDir;
		}
		tile = startTile - wallDir * ((gate == null) ? 1 : 2);
		int num3 = 1;
		while (num3 < num && !IsSlopeOrImpassableRaw(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			num3++;
			tile -= wallDir;
		}
		while (!IsSlopeOrImpassableRaw(tile.x, tile.y) && !IsTileOutsideBounds(tile.x, tile.y))
		{
			GenerateFence(tile, fenceType, null, invulnerable: true);
			tile -= pathDir;
		}
		for (int i = -num3 - 4; i <= num2 + 4; i++)
		{
			tile = startTile + wallDir * i;
			TerrainCoord terrainCoord = tile - ((pathDir.x < 0 || pathDir.y < 0) ? pathDir : TerrainCoord.Zero);
			while (!IsTileOutsideBounds(terrainCoord.x, terrainCoord.y))
			{
				if ((i >= -num3 && i <= num2) || edgeMountainAmount[terrainCoord.x, terrainCoord.y] + edgeMountainAmount[terrainCoord.x + 1, terrainCoord.y] + edgeMountainAmount[terrainCoord.x, terrainCoord.y + 1] + edgeMountainAmount[terrainCoord.x + 1, terrainCoord.y + 1] != 0f)
				{
					edgeMountainAmount[terrainCoord.x, terrainCoord.y] = 1f;
					edgeMountainAmount[terrainCoord.x + 1, terrainCoord.y] = 1f;
					edgeMountainAmount[terrainCoord.x, terrainCoord.y + 1] = 1f;
					edgeMountainAmount[terrainCoord.x + 1, terrainCoord.y + 1] = 1f;
				}
				terrainCoord -= pathDir;
			}
		}
	}

	private void GenerateMapEdgeTrees(TerrainPath path, TerrainCoord startTile, TerrainCoord wallDir, TerrainCoord pathDir, CustomRandom rand)
	{
		TreeType treeType = (TreeType)rand.Next(5, 8);
		int num = Mathf.CeilToInt(path.DefaultWidth * 0.5f + 8f);
		int num2 = num * 2;
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2 && num4 < num2 * 10)
		{
			TerrainCoord tile = startTile + pathDir * (1 + rand.Next(8)) + wallDir * rand.Next(-num, num + 1);
			if (!IsTileOutsideBounds(tile.x, tile.y) && GetFixedObjectOnTile(tile.x, tile.y) == null)
			{
				TreeProp.Spawn(treeType, tile, 1f, rand);
				num3++;
			}
			num4++;
		}
	}

	private void GenerateMapEdgeBoulders(TerrainPath path, TerrainCoord startTile, TerrainCoord wallDir, TerrainCoord pathDir, CustomRandom rand)
	{
		int num = Mathf.CeilToInt(path.DefaultWidth * 0.5f + 8f);
		int num2 = num * 2;
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2 && num4 < num2 * 10)
		{
			float num5 = rand.RandomFloat() * rand.RandomFloat();
			TerrainCoord terrainCoord = startTile + pathDir * (1 + (int)(num5 * 8f)) + wallDir * rand.Next(-num, num + 1);
			PropPrototype boulder = PropPrototype.Boulder;
			if (boulder == null)
			{
				break;
			}
			Prop.OrientationType orientation = rand.RandomOrientationType();
			TerrainCoord minTile;
			TerrainCoord maxTile;
			if (boulder.ProtoInstance is Prop prop)
			{
				prop.CalcMinMaxTile(terrainCoord, orientation, out minTile, out maxTile);
			}
			else
			{
				minTile = (maxTile = terrainCoord);
			}
			if (!IsAnyTileInRectSlopeOrImpassableRaw(minTile, maxTile) && !IsAnyFixedObjectOnRect(minTile, maxTile))
			{
				TileObject.SpawnProp(boulder, terrainCoord, orientation);
				num3++;
			}
			num4++;
		}
	}

	private void GenerateMapEdgeProps(TerrainPath path, TerrainCoord tile0, TerrainCoord tile1, CustomRandom rand, float[,] edgeMountainAmount)
	{
		if (IsTileOnEdge(tile0.x, tile0.y) && !(tile0 == tile1))
		{
			TerrainCoord terrainCoord = tile1 - tile0;
			TerrainCoord wallDir = ((Math.Abs(terrainCoord.x) >= Math.Abs(terrainCoord.y)) ? new TerrainCoord(0, 1) : new TerrainCoord(1, 0));
			terrainCoord.Normalize();
			tile1 = tile0 + terrainCoord * 12;
			if (path.IsRiver)
			{
				GenerateMapEdgeWall(path, PropPrototype.WireFence, tile0, wallDir);
				GenerateMapEdgeInnerWall(path, PropPrototype.WireFence, null, tile1, wallDir, terrainCoord, edgeMountainAmount);
				GenerateMapEdgeBoulders(path, tile0, wallDir, terrainCoord, rand);
			}
			else
			{
				GenerateMapEdgeWall(path, PropPrototype.ConcreteWall, tile0, wallDir);
				GenerateMapEdgeInnerWall(path, PropPrototype.ConcreteWall, PropPrototype.WireGate, tile1, wallDir, terrainCoord, edgeMountainAmount);
			}
			GenerateMapEdgeTrees(path, tile0, wallDir, terrainCoord, rand);
		}
	}

	public void InitBlank()
	{
		for (int i = 0; i < Size; i++)
		{
			for (int j = 0; j < Size; j++)
			{
				TextureWeights[i, j, 0] = byte.MaxValue;
			}
		}
	}

	public void LoadHeightMap(string path)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(path);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			Texture2D texture2D = new Texture2D(Size, Size, TextureFormat.RFloat, mipChain: false, linear: true);
			texture2D.LoadImage(array);
			for (int i = 0; i <= Size; i++)
			{
				for (int j = 0; j <= Size; j++)
				{
					Color pixel = texture2D.GetPixel(Math.Min(i, Size - 1), Math.Min(j, Size - 1));
					Vertices[Size - i, Size - j] = pixel.r * 64f;
				}
			}
			GrassMap.ApplyGrassChangesIfNeeded(new TerrainCoord(0, 0), new TerrainCoord(Size, Size));
			OnHeightChanged(new TerrainCoord(0, 0), new TerrainCoord(Size, Size));
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			UnityEngine.Debug.Log("Error loading " + path + ": " + ex.ToString());
		}
	}

	public void SaveHeightMap(string path)
	{
		try
		{
			Texture2D texture2D = new Texture2D(Size, Size, TextureFormat.RFloat, mipChain: false, linear: true);
			for (int i = 0; i < Size; i++)
			{
				for (int j = 0; j < Size; j++)
				{
					float num = Vertices[Size - i, Size - j] / 64f;
					texture2D.SetPixel(i, j, new Color(num, num, num, 1f));
				}
			}
			byte[] array = texture2D.EncodeToPNG();
			using FileStream fileStream = File.OpenWrite(path);
			fileStream.Write(array, 0, array.Length);
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			UnityEngine.Debug.Log("Error saving " + path + ": " + ex.ToString());
		}
	}

	public void GenerateMineralDeposit(MineralType mineralType, Community community, CustomRandom rand, List<MineralDeposit> mineralDeposits)
	{
		TerrainCoord centre = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
		int area = rand.Next(1000, 2500);
		GenerateMineralDeposit(mineralType, community, centre, area, rand, mineralDeposits);
	}

	public void GenerateMineralDeposit(MineralType mineralType, Community community, TerrainCoord centre, int area, CustomRandom rand, List<MineralDeposit> mineralDeposits)
	{
		int num = rand.Next(area, area * 2) / 16;
		List<TerrainCoord> list = new List<TerrainCoord>();
		list.Add(centre / 4);
		for (int i = 0; i < num; i++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			int index = rand.Next(list.Count);
			TerrainCoord terrainCoord = list[index];
			list.RemoveAt(index);
			MineralWeights[terrainCoord.x, terrainCoord.y, (int)mineralType] = byte.MaxValue;
			for (int j = 0; j < Directions.Length; j++)
			{
				TerrainCoord item = terrainCoord + Directions[j];
				if (!IsTileOutsideBounds(item.x * 4, item.y * 4) && MineralWeights[item.x, item.y, (int)mineralType] == 0 && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		mineralDeposits.Add(new MineralDeposit(centre, area, mineralType, community));
	}

	public void GenerateGeologicalMap(CustomRandom rand, List<MineralDeposit> mineralDeposits)
	{
		for (int i = 0; i < 4; i++)
		{
			if (i == 0)
			{
				float num = 1f / 32f;
				Vector2 vector = rand.RandomVec2() * 1000f;
				for (int j = 0; j < Size / 4; j++)
				{
					for (int k = 0; k < Size / 4; k++)
					{
						float value = Mathf.PerlinNoise(vector.x + (float)j * num, vector.y + (float)k * num) * 0.5f + Mathf.PerlinNoise(vector.x + (float)j * num * 2f, vector.y + (float)k * num * 2f) * 0.25f + Mathf.PerlinNoise(vector.x + (float)j * num * 4f, vector.y + (float)k * num * 4f) * 0.125f + Mathf.PerlinNoise(vector.x + (float)j * num * 8f, vector.y + (float)k * num * 8f) * 0.0625f + Mathf.PerlinNoise(vector.x + (float)j * num * 16f, vector.y + (float)k * num * 16f) * (1f / 32f);
						MineralWeights[j, k, i] = (byte)(Mathf.Clamp01(value) * 255f);
					}
				}
			}
			else
			{
				float mineralDensity = Session.Instance.DifficultySettings.GetMineralDensity((MineralType)i);
				int num2 = rand.Next(Mathf.FloorToInt(Mathf.Max(0f, mineralDensity - 1f) * (float)Size / 128f), Mathf.CeilToInt(mineralDensity * (float)Size / 128f));
				for (int l = 0; l < num2; l++)
				{
					GenerateMineralDeposit((MineralType)i, null, rand, mineralDeposits);
				}
			}
			GeologicalTexDirty[i] = true;
		}
	}

	public void LoadGeologicalMap(MineralType mineralType, string path)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(path);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, array.Length);
			Texture2D texture2D = new Texture2D(Size / 4, Size / 4, TextureFormat.ARGB32, mipChain: false, linear: true);
			texture2D.LoadImage(array);
			for (int i = 0; i < Size / 4; i++)
			{
				for (int j = 0; j < Size / 4; j++)
				{
					Color32 color = texture2D.GetPixel(i, j);
					MineralWeights[i, j, (int)mineralType] = color.r;
				}
			}
			GeologicalTexDirty[(int)mineralType] = true;
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			UnityEngine.Debug.Log("Error loading " + path + ": " + ex.ToString());
		}
	}

	public void SaveGeologicalMap(MineralType mineralType, string path)
	{
		try
		{
			Texture2D texture2D = new Texture2D(Size / 4, Size / 4, TextureFormat.ARGB32, mipChain: false, linear: true);
			for (int i = 0; i < Size / 4; i++)
			{
				for (int j = 0; j < Size / 4; j++)
				{
					byte b = MineralWeights[i, j, (int)mineralType];
					texture2D.SetPixel(i, j, new Color32(b, b, b, byte.MaxValue));
				}
			}
			byte[] array = texture2D.EncodeToPNG();
			using FileStream fileStream = File.OpenWrite(path);
			fileStream.Write(array, 0, array.Length);
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			UnityEngine.Debug.Log("Error saving " + path + ": " + ex.ToString());
		}
	}

	public MineralType PickMineralForTile(TerrainCoord tile, CustomRandom rand)
	{
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return MineralType.None;
		}
		TempMineralTypes.Clear();
		for (int i = 1; i < 4; i++)
		{
			if ((float)(int)MineralWeights[tile.x / 4, tile.y / 4, i] / 255f > MineralThreshold)
			{
				TempMineralTypes.Add((MineralType)i);
			}
		}
		if (TempMineralTypes.Count <= 0 || !rand.RandomChoice(0.25f))
		{
			return MineralType.Stone;
		}
		return TempMineralTypes[rand.Next() % TempMineralTypes.Count];
	}

	public bool HasRichMineralDeposits(TerrainCoord tile, MineralType mineralType)
	{
		if (IsTileOutsideBounds(tile.x, tile.y))
		{
			return false;
		}
		return (float)(int)MineralWeights[tile.x / 4, tile.y / 4, (int)mineralType] / 255f > MineralThreshold;
	}

	public void SetMineralType(TerrainCoord tile, MineralType mineralType, byte weight)
	{
		if (!IsTileOutsideBounds(tile.x, tile.y))
		{
			MineralWeights[tile.x / 4, tile.y / 4, (int)mineralType] = weight;
		}
	}

	public void Generate(CustomRandom rand)
	{
		Session instance = Session.Instance;
		float num = 1f / 128f;
		BiomeType[,] array = new BiomeType[Size, Size];
		float[,] array2 = new float[Size + 1, Size + 1];
		float[,] array3 = new float[Size + 1, Size + 1];
		float[,] array4 = new float[Size + 1, Size + 1];
		float[,] array5 = new float[Size + 1, Size + 1];
		Generating = true;
		CurGenerationStep = 0;
		using (new StopWatchScope(CurStopwatch))
		{
			new TerrainCoord(Size / 2, Size / 2);
			Vector2 vector = rand.RandomVec2() * 1000f;
			float num2 = 56.6f;
			for (int i = 0; i <= Size; i++)
			{
				for (int j = 0; j <= Size; j++)
				{
					float f = Mathf.PerlinNoise(vector.x + (float)i * num, vector.y + (float)j * num) * 0.5f + Mathf.PerlinNoise(vector.x + (float)i * num * 2f, vector.y + (float)j * num * 2f) * 0.25f + Mathf.PerlinNoise(vector.x + (float)i * num * 4f, vector.y + (float)j * num * 4f) * 0.125f + Mathf.PerlinNoise(vector.x + (float)i * num * 8f, vector.y + (float)j * num * 8f) * 0.0625f + Mathf.PerlinNoise(vector.x + (float)i * num * 16f, vector.y + (float)j * num * 16f) * (1f / 32f);
					f = (array2[i, j] = 1f + num2 * Hilliness * Mathf.Pow(f, 3f));
					TerrainCoord terrainCoord = new TerrainCoord(i, (j >= Size / 2) ? Size : 0);
					TerrainCoord terrainCoord2 = new TerrainCoord((i >= Size / 2) ? Size : 0, j);
					float num3 = Mathf.Abs(j - terrainCoord.y);
					float num4 = Mathf.Abs(i - terrainCoord2.x);
					float t = Mathf.PerlinNoise(vector.x + (float)terrainCoord.x * num * 8f, vector.y + (float)terrainCoord.y * num * 8f);
					float t2 = Mathf.PerlinNoise(vector.x + (float)terrainCoord2.x * num * 8f, vector.y + (float)terrainCoord2.y * num * 8f);
					float num5 = Mathf.Lerp(1f, 32f, t);
					float num6 = Mathf.Lerp(1f, 32f, t2);
					float val = Mathf.Clamp01((num5 - num3) / 8f);
					float val2 = Mathf.Clamp01((num6 - num4) / 8f);
					array3[i, j] = Mathf.SmoothStep(0f, 1f, Math.Max(val, val2));
					f += 6.4f * array3[i, j];
					Vertices[i, j] = Mathf.Clamp(f, 1f, 64f);
					array4[i, j] = float.MaxValue;
				}
			}
		}
		UnityEngine.Debug.Log("Height Gen: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		int num7 = 0;
		int num8 = 0;
		Stopwatch stopwatch = new Stopwatch();
		Stopwatch stopwatch2 = new Stopwatch();
		using (new StopWatchScope(CurStopwatch))
		{
			List<float> list = new List<float>();
			for (int k = 0; k <= Size; k++)
			{
				list.Add(array2[k, 0]);
				list.Add(array2[k, Size]);
				list.Add(array2[0, k]);
				list.Add(array2[Size, k]);
			}
			list.Sort();
			float val3 = list[list.Count * 3 / 4];
			val3 = Math.Max(val3, 1f);
			using (new StopWatchScope(stopwatch))
			{
				int num9 = Mathf.CeilToInt((float)Math.Max(1, Size / 128) * (Session.Instance.DifficultySettings.RiverDensity / 100f));
				int num10 = rand.Next(num9 / 2, num9 + 1);
				int num11 = 0;
				while (Paths.Count < num10 && num11 < num10 * 10)
				{
					if (LayoutPath(isRiver: true, array2, array4, array5, array, rand, val3) != null)
					{
						num7++;
					}
					num11++;
				}
			}
			using (new StopWatchScope(stopwatch2))
			{
				int num12 = Mathf.CeilToInt((float)Math.Max(1, Size / 128) * (Session.Instance.DifficultySettings.RoadDensity / 100f));
				int num13 = rand.Next(num12 / 2, num12 + 1);
				int num14 = 0;
				while (num8 < num13 && num14 < num13 * 10)
				{
					if (LayoutPath(isRiver: false, array2, array4, array5, array, rand, val3) != null)
					{
						num8++;
					}
					num14++;
				}
			}
			for (int l = 0; l <= Size; l++)
			{
				for (int m = 0; m <= Size; m++)
				{
					Vertices[l, m] += array5[l, m];
					Vertices[l, m] = Mathf.Clamp(Vertices[l, m], 0f, 64f);
					float val4 = Math.Max(0.0625f, (Vertices[l, m] - array2[l, m]) / 6.4f);
					array3[l, m] = Math.Min(array3[l, m], val4);
				}
			}
		}
		UnityEngine.Debug.Log("Roads and Rivers: " + CurStopwatch.ElapsedMilliseconds + "ms (River paths: " + stopwatch.ElapsedMilliseconds + "ms, Road paths: " + stopwatch2.ElapsedMilliseconds + "ms, Number of rivers: " + num7 + ", Number of roads: " + num8 + ")");
		CurGenerationStep++;
		CommunityManager communityManager = Session.Instance.CommunityManager;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithClass = PropPrototype.GetTypesWithClass(typeof(BridgeWall));
			for (int n = 0; n < Junctions.Count; n++)
			{
				TerrainPathJunction terrainPathJunction = Junctions[n];
				TerrainPath terrainPath = Paths[terrainPathJunction.PathIndex0];
				TerrainPath terrainPath2 = Paths[terrainPathJunction.PathIndex1];
				if (terrainPath.IsRiver && !terrainPath2.IsRiver)
				{
					TerrainPathPoint point = terrainPath.GetPoint(terrainPathJunction.PointIndex0);
					TerrainPathPoint point2 = terrainPath2.GetPoint(terrainPathJunction.PointIndex1);
					float angleFromNormalizedDir = MathUtil.GetAngleFromNormalizedDir(point2.DirXZ);
					float angleFromNormalizedDir2 = MathUtil.GetAngleFromNormalizedDir(point.DirXZ);
					Prop.OrientationType closestOrientationTypeToAngle = Prop.GetClosestOrientationTypeToAngle(angleFromNormalizedDir);
					float num15 = MathUtil.AngleDiff(angleFromNormalizedDir + MathF.PI / 2f, angleFromNormalizedDir2);
					if (num15 >= MathF.PI / 2f)
					{
						num15 = MathF.PI - num15;
					}
					float num16 = terrainPath2.DefaultWidth * 0.5f / Mathf.Cos(Math.Min(num15, MathF.PI / 4f));
					Vector2 pos = MathUtil.ToXZ(point2.Pos) - point.DirXZ * num16;
					Vector2 pos2 = MathUtil.ToXZ(point2.Pos) + point.DirXZ * num16;
					PropPrototype propPrototype = PropPrototype.PickRandom(rand, typesWithClass);
					if (propPrototype != null)
					{
						BridgeWall.Spawn(propPrototype, GetTileCoordForPosXZ(pos), closestOrientationTypeToAngle, n);
						BridgeWall.Spawn(propPrototype, GetTileCoordForPosXZ(pos2), closestOrientationTypeToAngle, n);
					}
				}
			}
			if (num8 > 0)
			{
				int num17 = Mathf.CeilToInt((float)Math.Max(1, 2 * Size / 128) * (instance.DifficultySettings.TownDensity / 100f));
				int num18 = rand.Next(Math.Max(1, num17 / 2), num17 + 1);
				foreach (TerrainPathJunction junction in Junctions)
				{
					if (!Paths[junction.PathIndex0].IsRiver && !Paths[junction.PathIndex1].IsRiver)
					{
						TerrainCoord tileCoordForPos = GetTileCoordForPos(junction.Pos);
						if (communityManager.Towns.Count < num18 && communityManager.GetTownForTile(tileCoordForPos, 32f) == null && communityManager.GetClosestTownDistSq(tileCoordForPos) >= 4096f)
						{
							GenerateTown(Paths[junction.PathIndex0], junction.PointIndex0, array4, array, rand);
						}
					}
				}
				int num19 = 0;
				while (communityManager.Towns.Count < num18 && num19 < num18 * 100)
				{
					TerrainPath terrainPath3 = Paths[num7 + rand.Next(num8)];
					int num20 = rand.Next(terrainPath3.Points.Count);
					TerrainCoord tileCoordForPos2 = GetTileCoordForPos(terrainPath3.Points[num20].Pos);
					if (communityManager.GetTownForTile(tileCoordForPos2, 32f) == null && communityManager.GetClosestTownDistSq(tileCoordForPos2) >= 4096f)
					{
						GenerateTown(terrainPath3, num20, array4, array, rand);
					}
					num19++;
				}
				UnityEngine.Debug.Log("Buildings: " + CurStopwatch.ElapsedMilliseconds + "ms, Number of Towns: " + communityManager.Towns.Count + "/" + num18);
			}
		}
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			Array.Copy(Vertices, OriginalVertices, OriginalVertices.Length);
			OnHeightChanged(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
		}
		UnityEngine.Debug.Log("Apply Height Map: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			foreach (TerrainPath path in Paths)
			{
				if (path.ControlPoints.Count >= 2)
				{
					if (!path.IsRiver)
					{
						GenerateMapEdgeProps(path, ClampTileWithinBounds(GetTileCoordForPosXZ(path.ControlPoints[0].Pos)), ClampTileWithinBounds(GetTileCoordForPosXZ(path.ControlPoints[1].Pos)), rand, array3);
					}
					GenerateMapEdgeProps(path, ClampTileWithinBounds(GetTileCoordForPosXZ(path.ControlPoints[path.ControlPoints.Count - 1].Pos)), ClampTileWithinBounds(GetTileCoordForPosXZ(path.ControlPoints[path.ControlPoints.Count - 2].Pos)), rand, array3);
				}
			}
		}
		UnityEngine.Debug.Log("Map Edge Props: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		List<MineralDeposit> list2 = new List<MineralDeposit>();
		using (new StopWatchScope(CurStopwatch))
		{
			GenerateGeologicalMap(rand, list2);
		}
		UnityEngine.Debug.Log("Mineral Gen: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			CharacterManager characterManager = Session.Instance.CharacterManager;
			int num21 = Mathf.CeilToInt((float)Math.Max(2, MathUtil.Squared(Size / 256)) * (instance.DifficultySettings.SurvivorCampDensity / 100f));
			int num22 = Mathf.CeilToInt((float)(Size / 8) * (instance.DifficultySettings.SurvivorCampDensity / 100f));
			int num23 = 0;
			while (num23 < 2000 && communityManager.Communities.Count < num21 && characterManager.GetCharacterCountBySpecies(BaseObjectType.Human) < num22)
			{
				num23++;
				GenerateCommunity(rand, array, num21, num22, num23, list2);
			}
			for (int num24 = 1; num24 < 4; num24++)
			{
				if (CountMineralDepositsOfType(list2, (MineralType)num24, communitiesOnly: false) == 0 && Session.Instance.DifficultySettings.GetMineralDensity((MineralType)num24) > 0f)
				{
					GenerateMineralDeposit((MineralType)num24, null, rand, list2);
				}
			}
			int num25 = 0;
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				if (character.InvisibleStrain != InvisibleStrainType.None)
				{
					num25++;
				}
			}
			UnityEngine.Debug.Log("Communities: " + communityManager.Communities.Count + "/" + num21 + ", NPCS: " + Session.Instance.CharacterManager.GetCharacterCountBySpecies(BaseObjectType.Human) + "/" + num22 + ", Invisible Strain: " + num25 + ", time taken: " + CurStopwatch.ElapsedMilliseconds + "ms, iter: " + num23);
			CurGenerationStep++;
		}
		int num26 = 0;
		int num27 = 0;
		int num28 = 0;
		int num29 = 0;
		int num30 = 0;
		int num31 = 0;
		int num32 = 0;
		int num33 = 0;
		int num34 = 0;
		int num35 = Mathf.CeilToInt((float)rand.Next(MathUtil.Squared(Size / 512), MathUtil.Squared(Size / 256)) * (instance.DifficultySettings.CountrysidePropDensity / 100f));
		using (new StopWatchScope(CurStopwatch))
		{
			for (int num36 = 0; num36 < num35 * 10; num36++)
			{
				if (num26 >= num35)
				{
					break;
				}
				TerrainCoord terrainCoord3 = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
				int num37 = rand.Next(8);
				float closestDistSq = MathUtil.Squared(8f);
				Vector3 closestPointOnPath = Vector3.zero;
				Vector2 closestDirXZOnPath = Vector2.zero;
				float closestPointIndex = 0f;
				switch (num37)
				{
				case 0:
					closestDistSq = MathUtil.Squared(32f);
					if (GetClosestPathToPos(GetTileCentreXZ(terrainCoord3), ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPointIndex) == null && communityManager.GetClosestTownDistSq(terrainCoord3) > MathUtil.Squared(64f))
					{
						PropPrototype propPrototype3 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithCategory("Countryside/FireTower"));
						if (propPrototype3 != null && GenerateProp(terrainCoord3, rand.RandomOrientationType(), propPrototype3, notOnRoads: true, notOnRivers: true, array) != null)
						{
							num26++;
							num27++;
						}
					}
					break;
				case 1:
					if (GetClosestPathToPos(GetTileCentreXZ(terrainCoord3), ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPointIndex) == null && communityManager.GetClosestTownDistSq(terrainCoord3) > MathUtil.Squared(32f))
					{
						PropPrototype propPrototype5 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithCategory("Countryside/WeatherRadar"));
						if (propPrototype5 != null && GenerateProp(terrainCoord3, rand.RandomOrientationType(), propPrototype5, notOnRoads: true, notOnRivers: true, array) != null)
						{
							num26++;
							num28++;
						}
					}
					break;
				case 4:
					if (num31 > 0)
					{
						break;
					}
					goto case 2;
				case 2:
				case 3:
				{
					List<PropPrototype> typesWithCategory2 = PropPrototype.GetTypesWithCategory(num37 switch
					{
						3 => "Countryside/PropCrash", 
						2 => "Countryside/HeliCrash", 
						_ => "Countryside/JumboCrash", 
					});
					int num44 = 0;
					foreach (PropPrototype item in typesWithCategory2)
					{
						num44 = Math.Max(num44, item.ExtentsMin.GetDistManhattan(item.ExtentsMax) * 2);
					}
					List<TileObject> list3 = new List<TileObject>();
					foreach (PropPrototype item2 in typesWithCategory2)
					{
						if (!rand.RandomChoice(item2.SpawnProbabilityFactor))
						{
							continue;
						}
						for (int num45 = 0; num45 < 40; num45++)
						{
							TileObject tileObject = GenerateProp(terrainCoord3 + rand.RandomTile(-new TerrainCoord(num44, num44), new TerrainCoord(num44, num44)), rand.RandomOrientationType(), item2, notOnRoads: false, notOnRivers: false, array);
							if (tileObject != null)
							{
								list3.Add(tileObject);
								break;
							}
							num44++;
						}
					}
					if (list3.Count <= 0)
					{
						break;
					}
					num26++;
					if (num37 == 2)
					{
						num29++;
					}
					if (num37 == 3)
					{
						num30++;
					}
					if (num37 == 4)
					{
						num31++;
					}
					int colorVariation = ((typesWithCategory2[0].GetNumColorVariations() > 0) ? rand.Next(typesWithCategory2[0].GetNumColorVariations()) : (-1));
					int colorVariation2 = ((typesWithCategory2[0].GetNumColorVariations2() > 0) ? rand.Next(typesWithCategory2[0].GetNumColorVariations2()) : (-1));
					int colorVariation3 = ((typesWithCategory2[0].GetNumColorVariations3() > 0) ? rand.Next(typesWithCategory2[0].GetNumColorVariations3()) : (-1));
					int colorVariation4 = ((typesWithCategory2[0].GetNumColorVariations4() > 0) ? rand.Next(typesWithCategory2[0].GetNumColorVariations4()) : (-1));
					int materialVariation = ((typesWithCategory2[0].GetNumMaterialVariations() > 0) ? rand.Next(typesWithCategory2[0].GetNumMaterialVariations()) : (-1));
					foreach (TileObject item3 in list3)
					{
						if (item3 is Prop prop)
						{
							prop.ColorVariation = colorVariation;
							prop.ColorVariation2 = colorVariation2;
							prop.ColorVariation3 = colorVariation3;
							prop.ColorVariation4 = colorVariation4;
							prop.MaterialVariation = materialVariation;
						}
					}
					break;
				}
				case 5:
				{
					if (!(communityManager.GetClosestTownDistSq(terrainCoord3) > MathUtil.Squared(32f)))
					{
						break;
					}
					List<PropPrototype> typesWithCategory = PropPrototype.GetTypesWithCategory("Countryside/WindFarm");
					if (typesWithCategory.Count <= 0)
					{
						break;
					}
					bool flag = false;
					int num38 = rand.Next(3);
					int num39 = rand.Next(3);
					int num40 = rand.Next(16, 25);
					int num41 = rand.Next(3);
					for (int num42 = -num38; num42 <= num38; num42++)
					{
						for (int num43 = -num39; num43 <= num39; num43++)
						{
							if (GenerateProp(terrainCoord3 + new TerrainCoord(num42, num43) * num40 + rand.RandomTile(new TerrainCoord(-num41, -num41), new TerrainCoord(num41, num41)), rand.RandomOrientationType(), PropPrototype.PickRandom(rand, typesWithCategory), notOnRoads: true, notOnRivers: true, array) != null)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						num26++;
						num32++;
					}
					break;
				}
				case 6:
					if (GetClosestPathToPos(GetTileCentreXZ(terrainCoord3), ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPointIndex) == null && communityManager.GetClosestTownDistSq(terrainCoord3) > MathUtil.Squared(32f))
					{
						PropPrototype propPrototype4 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithCategory("Countryside/LogCabin"));
						if (propPrototype4 != null && GenerateProp(terrainCoord3, rand.RandomOrientationType(), propPrototype4, notOnRoads: true, notOnRivers: true, array) != null)
						{
							num26++;
							num33++;
						}
					}
					break;
				case 7:
				{
					PropPrototype propPrototype2 = PropPrototype.PickRandom(rand, PropPrototype.GetTypesWithCategory("Countryside/Tank"));
					if (propPrototype2 != null && GenerateProp(terrainCoord3, rand.RandomOrientationType(), propPrototype2, notOnRoads: false, notOnRivers: false, array) != null)
					{
						num26++;
						num34++;
					}
					break;
				}
				}
			}
		}
		UnityEngine.Debug.Log("Countryside scenes: " + num26 + "/" + num35 + ", time taken: " + CurStopwatch.ElapsedMilliseconds + "ms (" + num27 + " fire towers, " + num28 + " radars, " + num29 + " helis, " + num30 + " prop planes, " + num31 + " jumbos, " + num33 + " cabins, " + num34 + " tanks, " + num32 + " windfarms)");
		CurGenerationStep++;
		int num46 = 0;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithCategory3 = PropPrototype.GetTypesWithCategory("Nature/Rocks");
			for (int num47 = 0; num47 < list2.Count; num47++)
			{
				TerrainCoord centre = list2[num47].Centre;
				int area = list2[num47].Area;
				int num48 = (int)Mathf.Sqrt(area);
				int num49 = Math.Max(1, rand.Next(area / 200, area / 100));
				for (int num50 = 0; num50 < num49; num50++)
				{
					TerrainCoord tile = centre + rand.RandomTile(new TerrainCoord(-num48, -num48), new TerrainCoord(num48, num48));
					if (GeneratePropCluster(tile, rand, typesWithCategory3, rand.Next(4, 8), notOnRoads: true, 2, array) > 0)
					{
						num46++;
					}
				}
			}
			int num51 = MathUtil.Squared(Size / 64);
			for (int num52 = 0; num52 < num51 * 10; num52++)
			{
				if (num46 >= num51)
				{
					break;
				}
				TerrainCoord tile2 = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
				if (GeneratePropCluster(tile2, rand, typesWithCategory3, rand.Next(4, 8), notOnRoads: true, 2, array) > 0)
				{
					num46++;
				}
			}
		}
		UnityEngine.Debug.Log("Boulder patches: " + num46 + ", time taken: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		int num53 = 0;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithClass2 = PropPrototype.GetTypesWithClass(typeof(Rock));
			for (int num54 = 0; num54 < Size; num54++)
			{
				for (int num55 = 0; num55 < Size; num55++)
				{
					if (array[num54, num55] == BiomeType.River)
					{
						SetFlag(num54, num55, TileFlags.Impassable, on: false);
						SetFlag(num54, num55, TileFlags.Slope, on: false);
					}
					if (IsSlopeOrImpassableRaw(num54, num55))
					{
						array[num54, num55] = BiomeType.Rock;
						if (rand.RandomChoice(0.25f))
						{
							float num56 = Mathf.Lerp(0.1f, 0.9f, rand.RandomFloat());
							Vector2 tileCentreXZ = GetTileCentreXZ(new TerrainCoord(num54, num55));
							Vector2 zero = Vector2.zero;
							int num57 = 0;
							while (!IsPosXZOutsideBounds(tileCentreXZ) && num57 < 50)
							{
								TerrainCoord tileCoordForPosXZ = GetTileCoordForPosXZ(tileCentreXZ);
								if (!IsSlopeOrImpassableRaw(tileCoordForPosXZ.x, tileCoordForPosXZ.y) && zero.sqrMagnitude < 0.010000001f)
								{
									if (GetFixedObjectOnTile(tileCoordForPosXZ.x, tileCoordForPosXZ.y) == null && array[tileCoordForPosXZ.x, tileCoordForPosXZ.y] != BiomeType.Road && CountObjectsOfClassInLookupSquare(tileCoordForPosXZ.x, tileCoordForPosXZ.y, typeof(Rock)) < 4)
									{
										List<PropPrototype> typesWithMineralType = PropPrototype.GetTypesWithMineralType(PickMineralForTile(tileCoordForPosXZ, rand), typesWithClass2);
										SingleTileProp.Spawn(PropPrototype.PickRandom(rand, typesWithMineralType), tileCoordForPosXZ);
									}
									break;
								}
								zero += GetTileSlideVector(tileCoordForPosXZ);
								tileCentreXZ += zero;
								zero *= num56;
								num57++;
							}
						}
					}
					if (array[num54, num55] != BiomeType.Meadow)
					{
						num53++;
					}
					if (array3[num54, num55] >= 0.125f && array3[num54, num55 + 1] >= 0.125f && array3[num54 + 1, num55] >= 0.125f && array3[num54 + 1, num55 + 1] >= 0.125f)
					{
						SetFlag(num54, num55, TileFlags.Impassable, on: true);
					}
				}
			}
		}
		UnityEngine.Debug.Log("Rock Biome: " + CurStopwatch.ElapsedMilliseconds + "ms, Number of Rocks: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.Rock));
		CurGenerationStep++;
		List<PropPrototype> typesWithCategory4 = PropPrototype.GetTypesWithCategory("Town/Props/Trash");
		List<PropPrototype> typesWithCategory5 = PropPrototype.GetTypesWithCategory("Nature/Flowers/Meadow");
		List<PropPrototype> typesWithCategory6 = PropPrototype.GetTypesWithCategory("Nature/Flowers/Forest");
		List<PropPrototype> typesWithClass3 = PropPrototype.GetTypesWithClass(typeof(Bush));
		List<TerrainCoord> list4 = new List<TerrainCoord>();
		using (new StopWatchScope(CurStopwatch))
		{
			Stopwatch stopwatch3 = new Stopwatch();
			Stopwatch stopwatch4 = new Stopwatch();
			Stopwatch stopwatch5 = new Stopwatch();
			Stopwatch stopwatch6 = new Stopwatch();
			int num58 = Size * Size / 4;
			List<TerrainCoord> list5 = new List<TerrainCoord>();
			List<TerrainCoord> list6 = new List<TerrainCoord>();
			while (num53 < num58)
			{
				BiomeType biomeType = (BiomeType)rand.Next(1, 4);
				int num59 = rand.Next(64, 512);
				TerrainCoord terrainCoord4;
				using (new StopWatchScope(stopwatch3))
				{
					int num60 = 0;
					while (true)
					{
						terrainCoord4 = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
						num60++;
						if (num60 >= 100)
						{
							break;
						}
						if (array[terrainCoord4.x, terrainCoord4.y] != BiomeType.Meadow)
						{
							continue;
						}
						goto IL_1b5f;
					}
				}
				break;
				IL_1b5f:
				using (new StopWatchScope(stopwatch4))
				{
					list4.Add(terrainCoord4);
					list5.Clear();
					list6.Clear();
					list6.Add(terrainCoord4);
					while (list6.Count > 0 && num59 > 0)
					{
						int index = rand.Next(list6.Count);
						terrainCoord4 = list6[index];
						list6.RemoveAt(index);
						if (!IsTileOutsideBounds(terrainCoord4.x, terrainCoord4.y) && array[terrainCoord4.x, terrainCoord4.y] == BiomeType.Meadow)
						{
							array[terrainCoord4.x, terrainCoord4.y] = biomeType;
							list5.Add(terrainCoord4);
							list6.Add(new TerrainCoord(terrainCoord4.x + 1, terrainCoord4.y));
							list6.Add(new TerrainCoord(terrainCoord4.x - 1, terrainCoord4.y));
							list6.Add(new TerrainCoord(terrainCoord4.x, terrainCoord4.y + 1));
							list6.Add(new TerrainCoord(terrainCoord4.x, terrainCoord4.y - 1));
							num59--;
							num53++;
						}
					}
				}
				using (new StopWatchScope(stopwatch5))
				{
					for (int num61 = 0; num61 < list5.Count; num61 += rand.Next(8, 16))
					{
						terrainCoord4 = list5[num61];
						if (GetFixedObjectOnTile(terrainCoord4.x, terrainCoord4.y) == null)
						{
							TreeType treeType = TreeType.Conifer1;
							switch (biomeType)
							{
							case BiomeType.ConiferForest:
								treeType = (TreeType)rand.Next(0, 5);
								break;
							case BiomeType.PineForest:
								treeType = (TreeType)rand.Next(8, 11);
								break;
							case BiomeType.DeciduousForest:
								treeType = (TreeType)rand.Next(11, 18);
								break;
							}
							TreeProp.Spawn(treeType, terrainCoord4, 1f, rand);
						}
					}
				}
				using (new StopWatchScope(stopwatch6))
				{
					for (int num62 = list5.Count - 1; num62 >= 0; num62 -= rand.Next(5, 10))
					{
						terrainCoord4 = list5[num62];
						bool flag2 = rand.RandomChoice(0.25f);
						int num63 = rand.Next(1, 8);
						for (int num64 = 0; num64 < num63; num64++)
						{
							if (!IsSlopeOrImpassableRaw(terrainCoord4.x, terrainCoord4.y) && GetFixedObjectOnTile(terrainCoord4.x, terrainCoord4.y) == null)
							{
								if (flag2)
								{
									PropPrototype propPrototype6 = PropPrototype.PickRandom(rand, typesWithCategory6);
									if (propPrototype6 != null)
									{
										TileObject.SpawnProp(propPrototype6, terrainCoord4, rand.RandomOrientationType());
									}
								}
								else
								{
									PropPrototype propPrototype7 = PropPrototype.PickRandom(rand, typesWithClass3);
									if (propPrototype7 != null)
									{
										TileObject.SpawnProp(propPrototype7, terrainCoord4, rand.RandomOrientationType());
									}
								}
							}
							switch (rand.Next(4))
							{
							case 0:
								terrainCoord4.x++;
								break;
							case 1:
								terrainCoord4.x--;
								break;
							case 2:
								terrainCoord4.y++;
								break;
							case 3:
								terrainCoord4.y--;
								break;
							}
						}
					}
				}
			}
			UnityEngine.Debug.Log("Find Empty Tile: " + stopwatch3.ElapsedMilliseconds + "ms");
			UnityEngine.Debug.Log("Fill Biome: " + stopwatch4.ElapsedMilliseconds + "ms");
			UnityEngine.Debug.Log("Trees: " + stopwatch5.ElapsedMilliseconds + "ms, Number of trees: " + TreeProps.Count);
			UnityEngine.Debug.Log("Bushes: " + stopwatch6.ElapsedMilliseconds + "ms, Number of Bushes: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.Bush));
		}
		UnityEngine.Debug.Log("Add Forest Biomes Total: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		int num65 = 0;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithCategory7 = PropPrototype.GetTypesWithCategory("Vehicles");
			List<PropPrototype> typesWithCategory8 = PropPrototype.GetTypesWithCategory("Town/Props/Billboards");
			List<PropPrototype> typesWithCategory9 = PropPrototype.GetTypesWithCategory("Town/Props/TelegraphPoles");
			List<PropPrototype> typesWithCategory10 = PropPrototype.GetTypesWithCategory("Nature/Riverside");
			List<PropPrototype> typesWithCategory11 = PropPrototype.GetTypesWithCategory("Town/Props/RoadSigns/Stop");
			List<PropPrototype> typesWithCategory12 = PropPrototype.GetTypesWithCategory("Town/Props/RoadSigns/OpenRoad");
			foreach (TerrainPathJunction junction2 in Junctions)
			{
				TerrainPath terrainPath4 = Paths[junction2.PathIndex0];
				TerrainPath terrainPath5 = Paths[junction2.PathIndex1];
				if (!terrainPath4.IsRiver && !terrainPath5.IsRiver)
				{
					TerrainPath terrainPath6 = ((junction2.PointIndex0 >= (float)terrainPath4.Points.Count) ? terrainPath4 : terrainPath5);
					int num66 = terrainPath6.Points.Count - 1;
					while (num66 >= 0 && SpawnRoadSign(terrainPath6, num66, onRight: true, typesWithCategory11, array, rand) == null)
					{
						num66--;
					}
				}
			}
			foreach (TerrainPath path2 in Paths)
			{
				if (path2.IsRiver)
				{
					for (int num67 = 0; num67 < path2.Points.Count; num67++)
					{
						float probability = 0.025f;
						if (rand.RandomChoice(probability))
						{
							bool flag3 = rand.RandomChoice(0.5f);
							float num68 = path2.DefaultWidth + rand.RandomFloat() * path2.DefaultWidth * 2f;
							Vector2 pos3 = MathUtil.ToXZ(path2.Points[num67].Pos) + (flag3 ? (-1f) : 1f) * num68 * MathUtil.RightNormal(path2.Points[num67].DirXZ);
							TerrainCoord tileCoordForPosXZ2 = GetTileCoordForPosXZ(pos3);
							PropPrototype propPrototype8 = PropPrototype.PickRandom(rand, typesWithCategory10);
							if (propPrototype8 != null)
							{
								GenerateProp(tileCoordForPosXZ2, rand.RandomOrientationType(), propPrototype8, notOnRoads: true, notOnRivers: true, array);
							}
						}
					}
					continue;
				}
				bool flag4 = rand.RandomChoice(0.5f);
				bool flag5 = rand.RandomChoice(0.5f);
				TileObject tileObject2 = null;
				for (int num69 = 0; num69 < path2.Points.Count; num69++)
				{
					if (flag4)
					{
						PropPrototype propPrototype9 = PropPrototype.PickRandom(rand, typesWithCategory9);
						if (propPrototype9 != null)
						{
							float num70 = path2.DefaultWidth * 0.5f + 2f;
							Vector2 vector2 = MathUtil.ToXZ(path2.Points[num69].Pos) + (flag5 ? (-1f) : 1f) * num70 * MathUtil.RightNormal(path2.Points[num69].DirXZ);
							TerrainCoord tileCoordForPosXZ3 = GetTileCoordForPosXZ(vector2);
							Prop.OrientationType closestOrientationTypeToAngle2 = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir((flag5 ? 1f : (-1f)) * path2.Points[num69].DirXZ));
							float num71 = ((tileObject2 == null) ? 0f : (tileObject2.PosXZ - vector2).magnitude);
							if (tileObject2 == null || num71 >= 16f)
							{
								if (!HasObstructions(tileCoordForPosXZ3, closestOrientationTypeToAngle2, propPrototype9, 0, canBeNextToFence: true, notOnRoads: true, notOnRivers: true, array, notOnSlopeOrImpassableTerrain: false))
								{
									TileObject tileObject3 = TileObject.CreateProp(propPrototype9);
									tileObject3.SetOrientationType(closestOrientationTypeToAngle2);
									tileObject3.SetTileGhost(tileCoordForPosXZ3);
									Session.Instance.ClearTrashForBuilding(tileObject3.GetMinTile(), tileObject3.GetMaxTile(), null, tileObject3);
									TileObject tileObject4 = TileObject.SpawnProp(tileObject3.GetPropPrototype(), tileObject3.GetTile(), tileObject3.GetOrientationType());
									if (tileObject4 is Prop prop2 && tileObject2 is Prop)
									{
										prop2.SetConnectedWireTo((Prop)tileObject2);
									}
									tileObject2 = tileObject4;
									num65++;
								}
								else if (num71 >= 32f)
								{
									flag4 = false;
								}
							}
						}
					}
					float probability2 = 0.2f * (instance.DifficultySettings.VehicleDensity / 100f);
					if (rand.RandomChoice(probability2))
					{
						bool flag6 = rand.RandomChoice(0.5f);
						float num72 = path2.DefaultWidth * 2f * MathUtil.Squared(rand.RandomFloat());
						Vector2 pos4 = MathUtil.ToXZ(path2.Points[num69].Pos) + (flag6 ? (-1f) : 1f) * num72 * MathUtil.RightNormal(path2.Points[num69].DirXZ);
						TerrainCoord tileCoordForPosXZ4 = GetTileCoordForPosXZ(pos4);
						Prop.OrientationType closestOrientationTypeToAngle3 = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir((flag6 ? 1f : (-1f)) * path2.Points[num69].DirXZ));
						PropPrototype propPrototype10 = PropPrototype.PickRandom(rand, typesWithCategory7);
						if (propPrototype10 != null)
						{
							TileObject tileObject5 = TileObject.CreateProp(propPrototype10);
							tileObject5.SetTileGhost(tileCoordForPosXZ4);
							tileObject5.SetOrientationType(closestOrientationTypeToAngle3);
							TileObject tileObject6 = SpawnTiltedPropIfNotBlocked(tileObject5);
							if (tileObject6 != null)
							{
								int num73 = rand.Next(3);
								for (int num74 = 0; num74 < num73; num74++)
								{
									TerrainCoord tile3 = rand.RandomTileOnOutsideEdge(tileObject6.GetMinTile(), tileObject6.GetMaxTile(), 1);
									GeneratePropCluster(tile3, rand, typesWithCategory4, rand.Next(1, 8), notOnRoads: false, 0, array);
								}
							}
						}
					}
					float probability3 = 0.03f;
					if (rand.RandomChoice(probability3))
					{
						PropPrototype propPrototype11 = PropPrototype.PickRandom(rand, typesWithCategory8);
						if (propPrototype11 != null)
						{
							bool flag7 = rand.RandomChoice(0.5f);
							float num75 = path2.DefaultWidth * 0.5f + 4f;
							Vector2 pos5 = MathUtil.ToXZ(path2.Points[num69].Pos) + (flag7 ? (-1f) : 1f) * num75 * MathUtil.RightNormal(path2.Points[num69].DirXZ);
							TerrainCoord tileCoordForPosXZ5 = GetTileCoordForPosXZ(pos5);
							Prop.OrientationType closestOrientationTypeToAngle4 = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir((flag7 ? 1f : (-1f)) * MathUtil.RightNormal(path2.Points[num69].DirXZ)));
							TileObject tileObject7 = TileObject.CreateProp(propPrototype11);
							tileObject7.SetOrientationType(closestOrientationTypeToAngle4);
							tileObject7.SetTileGhost(tileCoordForPosXZ5);
							SpawnPlaygroundPropIfNotBlocked(tileObject7, array);
						}
					}
					if (rand.RandomChoice(0.02f))
					{
						SpawnRoadSign(path2, num69, rand.RandomChoice(0.5f), typesWithCategory12, array, rand);
					}
				}
			}
		}
		UnityEngine.Debug.Log("Vehicles: " + CurStopwatch.ElapsedMilliseconds + "ms, Number of Vehicles: " + BaseObjectManager.Instance.CountObjectsOfType(typeof(EnterableVehicle)) + ", Number of Telgraph Poles: " + num65);
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			BiomeAmount[] array6 = new BiomeAmount[8];
			for (int num76 = 0; num76 < Size; num76++)
			{
				for (int num77 = 0; num77 < Size; num77++)
				{
					for (int num78 = 0; num78 < 8; num78++)
					{
						array6[num78].BiomeType = (BiomeType)num78;
						array6[num78].Amount = 0f;
					}
					for (int num79 = Math.Max(num76 - 2, 0); num79 < Math.Min(num76 + 2, Size - 1); num79++)
					{
						for (int num80 = Math.Max(num77 - 2, 0); num80 < Math.Min(num77 + 2, Size - 1); num80++)
						{
							BiomeType val5 = array[num79, num80];
							float magnitude = (GetTileCentreXZ(new TerrainCoord(num79, num80)) - GetVertexPosXZ(num76, num77)).magnitude;
							array6[Math.Max(0, (int)val5)].Amount += Mathf.Max(0f, (2f - magnitude) / 2f);
						}
					}
					if (array6[4].Amount > 0f && rand.RandomChoice(0.1f))
					{
						array6[4].Amount = 0f;
						array6[0].Amount = 1f;
						array[num76, num77] = BiomeType.Meadow;
					}
					float num81 = 0f;
					for (int num82 = 0; num82 < 8; num82++)
					{
						num81 += array6[num82].Amount;
					}
					for (int num83 = 0; num83 < 8; num83++)
					{
						TextureWeights[num76, num77, num83] = 0;
					}
					for (int num84 = 0; num84 < 8; num84++)
					{
						TerrainTex texForBiome = GetTexForBiome(array6[num84].BiomeType, num76, num77);
						float num85 = (float)(int)TextureWeights[num76, num77, (uint)texForBiome] / 255f;
						TextureWeights[num76, num77, (uint)texForBiome] = (byte)(255f * Math.Min(1f, num85 + array6[num84].Amount / num81));
					}
				}
			}
		}
		UnityEngine.Debug.Log("Ground Tex: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			Vector2[] array7 = new Vector2[8];
			for (int num86 = 0; num86 < 8; num86++)
			{
				array7[num86] = rand.RandomVec2();
			}
			float num87 = 0.1f;
			for (int num88 = 0; num88 < Size; num88++)
			{
				for (int num89 = 0; num89 < Size; num89++)
				{
					TileObject fixedObjectOnTile = GetFixedObjectOnTile(num88, num89);
					if (fixedObjectOnTile != null && (fixedObjectOnTile.WantClearGrass() || fixedObjectOnTile.WantFlattenTerrain()))
					{
						continue;
					}
					if (array[num88, num89] == BiomeType.Meadow && rand.RandomChoice(0.01f))
					{
						TerrainCoord tile4 = new TerrainCoord(num88, num89);
						GeneratePropCluster(tile4, rand, typesWithCategory5, rand.Next(1, 8), notOnRoads: true, 0, array);
					}
					if (array[num88, num89] == BiomeType.Road && rand.RandomChoice(0.01f))
					{
						TerrainCoord tile5 = new TerrainCoord(num88, num89);
						GeneratePropCluster(tile5, rand, typesWithCategory4, rand.Next(1, 8), notOnRoads: false, 0, array);
					}
					for (int num90 = 0; num90 < 8; num90++)
					{
						GrassType grassType = (GrassType)num90;
						float num91 = 0f;
						float num92 = GrassRenderer.GetMaxDensityForGrassType(grassType);
						switch (grassType)
						{
						case GrassType.Grass1:
							num91 = 0.75f;
							break;
						case GrassType.Grass2:
							num91 = 0.5f;
							break;
						case GrassType.Grass3:
							num91 = 0.75f;
							break;
						case GrassType.Flowers1:
							num91 = 0.25f;
							break;
						case GrassType.Flowers2:
							num91 = 0.25f;
							break;
						case GrassType.Flowers3:
							num91 = 0.25f;
							break;
						case GrassType.Flowers4:
							num91 = 0.25f;
							break;
						case GrassType.Flowers5:
							num91 = 0.25f;
							break;
						}
						switch (array[num88, num89])
						{
						case BiomeType.Footpath:
							num91 *= 0.1f;
							break;
						case BiomeType.Rock:
							num91 *= 0f;
							break;
						case BiomeType.Road:
							num91 *= 0f;
							break;
						case BiomeType.Meadow:
							num91 *= 1f;
							break;
						case BiomeType.ConiferForest:
							num91 *= 0.6f;
							break;
						case BiomeType.PineForest:
							num91 *= 0.6f;
							break;
						case BiomeType.DeciduousForest:
							num91 *= 0.75f;
							break;
						case BiomeType.River:
						{
							float H;
							float amountOnPath = GetAmountOnPath(GetTileCentreXZ(new TerrainCoord(num88, num89)), out H, wantRiver: true);
							num91 *= ((amountOnPath < 1f && H + RiverHeightOffset > GetTileMaxHeight(num88, num89)) ? 0f : 1f);
							break;
						}
						}
						if (num91 != 0f)
						{
							float num93 = Mathf.Clamp01((Mathf.PerlinNoise((float)num88 * num87 + array7[num90].x, (float)num89 * num87 + array7[num90].y) - 1f + num91) / num91) * num92;
							if (num93 > 0f)
							{
								GrassMap.SetGrassAmount(num88, num89, grassType, Math.Max((int)num93, 1));
							}
						}
					}
				}
			}
		}
		UnityEngine.Debug.Log("Grass: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		int num94 = 0;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithCategory13 = PropPrototype.GetTypesWithCategory("Town/Props/Bins");
			List<PropPrototype> typesWithCategory14 = PropPrototype.GetTypesWithCategory("Town/Props/Shop");
			List<PropPrototype> typesWithCategory15 = PropPrototype.GetTypesWithCategory("Town/Props/Playground");
			List<PropPrototype> typesWithCategory16 = PropPrototype.GetTypesWithCategory("Town/Props/Roadside");
			foreach (Town town in communityManager.Towns)
			{
				for (int num95 = 0; num95 < town.Buildings.Count; num95++)
				{
					Prop prop3 = town.Buildings[num95];
					if (town.Buildings[num95] is Building building)
					{
						TerrainCoord[] array8 = new TerrainCoord[building.GetEntranceDefs().Length];
						for (int num96 = 0; num96 < array8.Length; num96++)
						{
							array8[num96] = GetTileCoordForPos(building.GetEntrancePos(num96));
						}
						TerrainCoord terrainCoord5 = (building.HasGarageDoor ? GetTileCoordForPos(building.GetGarageDoorPos()) : TerrainCoord.Invalid);
						int num97 = rand.Next(3);
						int num98 = 0;
						int num99 = 0;
						while (num98 < num97 && num99 < num97 * 20)
						{
							num99++;
							TerrainCoord tile6 = rand.RandomTileOnOutsideEdge(building.GetMinTile(), building.GetMaxTile(), 1);
							bool flag8 = false;
							for (int num100 = 0; num100 < array8.Length; num100++)
							{
								if (tile6.IsWithinBounds(array8[num100] - new TerrainCoord(2, 2), array8[num100] + new TerrainCoord(2, 2)))
								{
									flag8 = true;
									break;
								}
							}
							if (terrainCoord5 != TerrainCoord.Invalid && tile6.IsWithinBounds(terrainCoord5 - new TerrainCoord(3, 3), terrainCoord5 + new TerrainCoord(3, 3)))
							{
								flag8 = true;
								break;
							}
							if (!flag8)
							{
								num98 += GeneratePropCluster(tile6, rand, typesWithCategory13, rand.Next(1, 3), notOnRoads: false, 1, array);
							}
						}
						if (building.GetBuildingUsage() == BuildingUsage.Commercial)
						{
							int num101 = rand.Next(4);
							int num102 = 0;
							int num103 = 0;
							while (num102 < num101 && num103 < num101 * 100)
							{
								num103++;
								PropPrototype propPrototype12 = typesWithCategory14[rand.Next(typesWithCategory14.Count)];
								TileObject tileObject8 = TileObject.CreateProp(propPrototype12);
								Prop prop4 = tileObject8 as Prop;
								Prop.OrientationType orientationType2;
								if (rand.RandomChoice(propPrototype12.SpawnWithBackAgainstBuilding))
								{
									List<Prop.OrientationType> list7 = new List<Prop.OrientationType>();
									for (Prop.OrientationType orientationType = Prop.OrientationType.Deg0; orientationType < Prop.OrientationType.Count; orientationType++)
									{
										float angleFromOrientationType = Prop.GetAngleFromOrientationType(orientationType);
										float entranceAngle = building.GetEntranceAngle(0);
										if (MathUtil.AngleDiff(angleFromOrientationType, entranceAngle) > 0.9424779f)
										{
											list7.Add(orientationType);
										}
									}
									orientationType2 = list7[rand.Next(list7.Count)];
									switch (orientationType2)
									{
									case Prop.OrientationType.Deg0:
										tileObject8.SetTileGhost(new TerrainCoord(rand.Next(building.GetMinTile().x, building.GetMaxTile().x), building.GetMinTile().y + 1));
										break;
									case Prop.OrientationType.Deg180:
										tileObject8.SetTileGhost(new TerrainCoord(rand.Next(building.GetMinTile().x, building.GetMaxTile().x), building.GetMinTile().y - 1));
										break;
									case Prop.OrientationType.Deg90:
										tileObject8.SetTileGhost(new TerrainCoord(building.GetMinTile().x + 1, rand.Next(building.GetMinTile().y, building.GetMaxTile().y)));
										break;
									case Prop.OrientationType.Deg270:
										tileObject8.SetTileGhost(new TerrainCoord(building.GetMinTile().x - 1, rand.Next(building.GetMinTile().y, building.GetMaxTile().y)));
										break;
									}
								}
								else
								{
									orientationType2 = rand.RandomOrientationType();
									int range = ((!rand.RandomChoice(0.5f)) ? 1 : 8);
									tileObject8.SetTileGhost(rand.RandomTileOnOutsideEdge(building.GetMinTile(), building.GetMaxTile(), range));
								}
								prop4?.SetOrientationType(orientationType2);
								bool flag9 = false;
								for (int num104 = 0; num104 < array8.Length; num104++)
								{
									if (TerrainCoord.Overlaps(tileObject8.GetMinTile(), tileObject8.GetMaxTile(), array8[num104] - new TerrainCoord(1, 1), array8[num104] + new TerrainCoord(1, 1)))
									{
										flag9 = true;
										break;
									}
								}
								if (terrainCoord5 != TerrainCoord.Invalid && TerrainCoord.Overlaps(tileObject8.GetMinTile(), tileObject8.GetMaxTile(), terrainCoord5 - new TerrainCoord(2, 2), terrainCoord5 + new TerrainCoord(2, 2)))
								{
									flag9 = true;
									break;
								}
								if (!flag9)
								{
									TileObject tileObject9 = SpawnTiltedPropIfNotBlocked(tileObject8);
									if (tileObject9 is Prop)
									{
										town.AddBuilding((Prop)tileObject9);
										num102++;
									}
								}
							}
						}
						else if (building.GetBuildingUsage() == BuildingUsage.Domestic)
						{
							int num105 = rand.Next(5);
							int num106 = 0;
							int num107 = 0;
							while (num106 < num105 && num107 < num105 * 100)
							{
								num107++;
								PropPrototype propPrototype13 = typesWithCategory15[rand.Next(typesWithCategory15.Count)];
								if (HasAnyOfType(town.Buildings, propPrototype13))
								{
									continue;
								}
								TileObject tileObject10 = TileObject.CreateProp(propPrototype13);
								tileObject10.SetOrientationType(rand.RandomOrientationType());
								tileObject10.SetTileGhost(rand.RandomTileOnOutsideEdge(building.GetMinTile(), building.GetMaxTile(), 16));
								TileObject tileObject11 = SpawnPlaygroundPropIfNotBlocked(tileObject10, array);
								if (tileObject11 != null)
								{
									num106++;
									if (tileObject11 is Prop)
									{
										town.AddBuilding((Prop)tileObject11);
									}
								}
							}
						}
					}
					int outsideEdgeLength = prop3.GetOutsideEdgeLength();
					int num108 = rand.Next(outsideEdgeLength / 2) + 1;
					for (int num109 = 0; num109 < num108; num109++)
					{
						TerrainCoord tile7 = rand.RandomTileOnOutsideEdge(prop3.GetMinTile(), prop3.GetMaxTile(), 1);
						GeneratePropCluster(tile7, rand, typesWithCategory4, rand.Next(1, 8), notOnRoads: false, 0, array);
					}
				}
				if (town.RoadIndex != -1)
				{
					TerrainPath terrainPath7 = Paths[town.RoadIndex];
					int num110 = (int)town.RoadPointIndex;
					int num111;
					for (num111 = (int)town.RoadPointIndex; num111 + 1 < terrainPath7.Points.Count && !((MathUtil.ToXZ(terrainPath7.Points[num111 + 1].Pos) - town.PosXZ).sqrMagnitude > town.Radius * town.Radius); num111++)
					{
					}
					while (num110 - 1 >= 0 && !((MathUtil.ToXZ(terrainPath7.Points[num110 - 1].Pos) - town.PosXZ).sqrMagnitude > town.Radius * town.Radius))
					{
						num110--;
					}
					int num112 = Mathf.CeilToInt(town.Radius / 4f);
					int num113 = rand.Next(num112 / 2, num112 + 1);
					int num114 = 0;
					int num115 = 0;
					while (num114 < num113 && num115 < num113 * 10)
					{
						num115++;
						int index2 = rand.Next(num110, num111);
						int num116 = ((!rand.RandomChoice(0.5f)) ? 1 : (-1));
						Vector2 vector3 = MathUtil.RightNormal(terrainPath7.Points[index2].DirXZ);
						Vector2 pos6 = MathUtil.ToXZ(terrainPath7.Points[index2].Pos) + vector3 * (terrainPath7.DefaultWidth * 0.5f + 2f) * num116;
						TerrainCoord tileCoordForPosXZ6 = GetTileCoordForPosXZ(pos6);
						Prop.OrientationType closestOrientationTypeToAngle5 = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir(vector3 * num116));
						if (!IsSlopeOrImpassableRaw(tileCoordForPosXZ6.x, tileCoordForPosXZ6.y) && !IsTileRoadOrRiver(tileCoordForPosXZ6.x, tileCoordForPosXZ6.y) && GetFixedObjectOnTile(tileCoordForPosXZ6.x, tileCoordForPosXZ6.y) == null && typesWithCategory16.Count > 0)
						{
							TileObject tileObject12 = TileObject.SpawnProp(PropPrototype.PickRandom(rand, typesWithCategory16), tileCoordForPosXZ6, closestOrientationTypeToAngle5);
							if (tileObject12 is Prop)
							{
								town.AddBuilding((Prop)tileObject12);
							}
							num114++;
							num94++;
						}
					}
				}
				float num117 = MathF.PI * MathUtil.Squared(town.Radius);
				int num118 = rand.Next(Mathf.CeilToInt(num117 * 0.02f));
				for (int num119 = 0; num119 < num118; num119++)
				{
					TerrainCoord tileCoordForPosXZ7 = GetTileCoordForPosXZ(town.PosXZ + rand.RandomVec2() * town.Radius);
					GeneratePropCluster(tileCoordForPosXZ7, rand, typesWithCategory4, rand.Next(1, 8), notOnRoads: false, 0, array);
				}
			}
		}
		UnityEngine.Debug.Log("Number of Flowers: " + BaseObjectManager.Instance.CountObjectsOfType(typeof(Flower)));
		UnityEngine.Debug.Log("Number of Trash: " + BaseObjectManager.Instance.CountObjectsOfType(typeof(Trash)));
		UnityEngine.Debug.Log("Town Props: " + CurStopwatch.ElapsedMilliseconds + "ms, Num roadside town props: " + num94);
		CurGenerationStep++;
		PlayerStartPoint playerStartPoint = null;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PlayerStartCandidate> list8 = new List<PlayerStartCandidate>();
			for (int num120 = 0; num120 < Paths.Count; num120++)
			{
				TerrainPath terrainPath8 = Paths[num120];
				if (!terrainPath8.IsRiver && terrainPath8.ControlPoints.Count >= 3)
				{
					if (terrainPath8.EntrancePointIndex != -1)
					{
						PlayerStartCandidate playerStartCandidate = new PlayerStartCandidate();
						playerStartCandidate.RoadIndex = num120;
						playerStartCandidate.DirAlongRoad = 1;
						playerStartCandidate.ControlPointIndex = terrainPath8.EntrancePointIndex;
						playerStartCandidate.ControlPoint = terrainPath8.ControlPoints[terrainPath8.EntrancePointIndex];
						list8.Add(playerStartCandidate);
					}
					if (terrainPath8.ExitPointIndex != -1)
					{
						PlayerStartCandidate playerStartCandidate2 = new PlayerStartCandidate();
						playerStartCandidate2.RoadIndex = num120;
						playerStartCandidate2.DirAlongRoad = -1;
						playerStartCandidate2.ControlPointIndex = terrainPath8.ExitPointIndex;
						playerStartCandidate2.ControlPoint = terrainPath8.ControlPoints[terrainPath8.ExitPointIndex];
						playerStartCandidate2.ControlPoint.Dir = -playerStartCandidate2.ControlPoint.Dir;
						list8.Add(playerStartCandidate2);
					}
				}
			}
			if (list8.Count == 0)
			{
				PlayerStartCandidate playerStartCandidate3 = new PlayerStartCandidate();
				playerStartCandidate3.ControlPoint.Dir = Vector2.one;
				playerStartCandidate3.RoadIndex = -1;
				playerStartCandidate3.DirAlongRoad = 0;
				list8.Add(playerStartCandidate3);
			}
			foreach (PlayerStartCandidate item4 in list8)
			{
				TerrainCoord tileCoordForPosXZ8 = GetTileCoordForPosXZ(item4.ControlPoint.Pos);
				Town closestTown = communityManager.GetClosestTown(tileCoordForPosXZ8);
				if (closestTown != null)
				{
					float num121 = tileCoordForPosXZ8.GetDist(closestTown.Tile) - closestTown.Radius;
					if (num121 < 64f)
					{
						item4.Score = num121;
					}
					else
					{
						item4.Score = 64f;
						if (item4.RoadIndex != -1)
						{
							item4.Score += Paths[item4.RoadIndex].CalcPathLength();
						}
					}
				}
				int num122 = 0;
				foreach (Town town2 in communityManager.Towns)
				{
					if (town2.RoadIndex == item4.RoadIndex)
					{
						num122++;
					}
				}
				if (num122 == 0)
				{
					item4.Score = 0f;
				}
			}
			list8.Sort();
			int num123 = 0;
			for (int num124 = 0; num124 < list8.Count; num124++)
			{
				if (playerStartPoint != null)
				{
					break;
				}
				PlayerStartCandidate playerStartCandidate4 = list8[num124];
				while (true)
				{
					TerrainCoord tileCoordForPosXZ9 = GetTileCoordForPosXZ(playerStartCandidate4.ControlPoint.Pos);
					float angleFromDir = MathUtil.GetAngleFromDir(playerStartCandidate4.ControlPoint.Dir, 0f);
					if (IsImpassable(tileCoordForPosXZ9.x, tileCoordForPosXZ9.y, 0, null, null) && num123 < 1000)
					{
						if (playerStartCandidate4.RoadIndex != -1 && playerStartCandidate4.DirAlongRoad != 0 && playerStartCandidate4.ControlPointIndex >= 1f && playerStartCandidate4.ControlPointIndex <= (float)(Paths[playerStartCandidate4.RoadIndex].ControlPoints.Count - 2))
						{
							TerrainPath terrainPath9 = Paths[playerStartCandidate4.RoadIndex];
							playerStartCandidate4.ControlPointIndex += 0.1f * (float)playerStartCandidate4.DirAlongRoad;
							float t3 = playerStartCandidate4.ControlPointIndex - Mathf.Floor(playerStartCandidate4.ControlPointIndex);
							playerStartCandidate4.ControlPoint.Pos = Vector2.Lerp(terrainPath9.ControlPoints[Mathf.FloorToInt(playerStartCandidate4.ControlPointIndex)].Pos, terrainPath9.ControlPoints[Mathf.CeilToInt(playerStartCandidate4.ControlPointIndex)].Pos, t3);
							playerStartCandidate4.ControlPoint.Dir = Vector2.Lerp(terrainPath9.ControlPoints[Mathf.FloorToInt(playerStartCandidate4.ControlPointIndex)].Dir, terrainPath9.ControlPoints[Mathf.CeilToInt(playerStartCandidate4.ControlPointIndex)].Dir, t3);
							if (playerStartCandidate4.ControlPointIndex >= (float)(terrainPath9.ControlPoints.Count / 2))
							{
								playerStartCandidate4.ControlPoint.Dir = -playerStartCandidate4.ControlPoint.Dir;
							}
						}
						else
						{
							if (num124 < list8.Count - 1)
							{
								break;
							}
							playerStartCandidate4.ControlPoint.Pos = rand.RandomVec2() * Size - new Vector2(HalfSize, HalfSize);
							num123++;
						}
						continue;
					}
					playerStartPoint = PlayerStartPoint.Spawn(tileCoordForPosXZ9, angleFromDir);
					break;
				}
			}
		}
		UnityEngine.Debug.Log("Player Spawnpoints: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			int num125 = Size / 16;
			if (grid == null || grid.GetLength(0) != MaxSize / 16)
			{
				grid = new bool[MaxSize / 16, MaxSize / 16];
			}
			Array.Clear(grid, 0, grid.Length);
			SortTownsByDistFromPlayerAscending sortTownsByDistFromPlayerAscending = new SortTownsByDistFromPlayerAscending();
			sortTownsByDistFromPlayerAscending.Tile = playerStartPoint.Tile;
			List<Town> list9 = new List<Town>();
			communityManager.Towns.CopyToList(list9);
			list9.Sort(sortTownsByDistFromPlayerAscending);
			float num126 = instance.DifficultySettings.GetTotalZombieDensity() / 100f;
			float num127 = instance.DifficultySettings.RabbitDensity / 100f;
			float num128 = instance.DifficultySettings.DeerDensity / 100f;
			for (int num129 = 0; num129 < list9.Count; num129++)
			{
				float t4 = (float)num129 / (float)(list9.Count - 1) + Mathf.Lerp(-1f, 1f, rand.RandomFloat()) * 0.5f;
				list9[num129].Infection = instance.DifficultySettings.PickStrain(t4);
			}
			foreach (Town town3 in communityManager.Towns)
			{
				if (town3.Infection != InfectionType.None)
				{
					BaseObjectType spawnPointType = (BaseObjectType)(103 + town3.Infection);
					GenerateSpawnPoints(grid, town3.Tile, Mathf.CeilToInt(16f * num126), Mathf.CeilToInt(32f * num126), spawnPointType, rand, playerStartPoint, 0);
				}
			}
			int num130 = 0;
			int num131 = Mathf.CeilToInt(num126 * (float)num125 * (float)num125 / 8f);
			if (instance.DifficultySettings.GreenStrainDensity + instance.DifficultySettings.BlueStrainDensity + instance.DifficultySettings.RedStrainDensity + instance.DifficultySettings.WhiteStrainDensity > 0f)
			{
				while (communityManager.AmbientEnemySpawnPoints.Count < num131 && num130 < num131 * 10)
				{
					num130++;
					TerrainCoord terrainCoord6 = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
					Town closestTown2 = communityManager.GetClosestTown(terrainCoord6);
					InfectionType infectionType = InfectionType.None;
					if (closestTown2 != null)
					{
						infectionType = closestTown2.Infection;
					}
					if (infectionType == InfectionType.None)
					{
						float t5 = playerStartPoint.Tile.GetDist(terrainCoord6) / (float)Size + Mathf.Lerp(-1f, 1f, rand.RandomFloat()) * 0.5f;
						infectionType = instance.DifficultySettings.PickStrain(t5);
					}
					if (infectionType != InfectionType.None)
					{
						int minCount = 1;
						int maxCount = 4;
						BaseObjectType spawnPointType2 = (BaseObjectType)(103 + infectionType);
						GenerateSpawnPoints(grid, terrainCoord6, minCount, maxCount, spawnPointType2, rand, playerStartPoint, 0);
					}
				}
			}
			int num132 = 0;
			int num133 = Mathf.CeilToInt(num128 * (float)num125 * (float)num125 / 256f);
			num131 = rand.Next(num133 / 2, num133 + 1);
			int count = Session.Instance.CommunityManager.AmbientEnemySpawnPoints.Count;
			while (communityManager.AmbientEnemySpawnPoints.Count - count < num133 && num132 < num133 * 10)
			{
				num132++;
				GenerateSpawnPoints(startTile: (num132 >= num133 * 5 || list4.Count <= 0) ? rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1)) : list4[rand.Next(list4.Count)], grid: grid, minCount: 1, maxCount: 1, spawnPointType: BaseObjectType.DeerSpawnPoint, rand: rand, playerStartPoint: playerStartPoint, gridRange: 1);
			}
			num132 = 0;
			int num134 = Mathf.CeilToInt(num127 * (float)num125 * (float)num125 / 32f);
			count = Session.Instance.CommunityManager.AmbientEnemySpawnPoints.Count;
			while (communityManager.AmbientEnemySpawnPoints.Count - count < num134 && num132 < num134 * 10)
			{
				num132++;
				TerrainCoord startTile = rand.RandomTile(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
				GenerateSpawnPoints(grid, startTile, 1, 4, BaseObjectType.RabbitSpawnPoint, rand, playerStartPoint, 0);
			}
		}
		UnityEngine.Debug.Log("Number of Looter SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.LooterSpawnPoint));
		UnityEngine.Debug.Log("Number of Green Strain SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.GreenStrainSpawnPoint));
		UnityEngine.Debug.Log("Number of Blue Strain SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.BlueStrainSpawnPoint));
		UnityEngine.Debug.Log("Number of Red Strain SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.RedStrainSpawnPoint));
		UnityEngine.Debug.Log("Number of White Strain SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.WhiteStrainSpawnPoint));
		UnityEngine.Debug.Log("Number of Rabbit SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.RabbitSpawnPoint));
		UnityEngine.Debug.Log("Number of Deer SpawnPoints: " + BaseObjectManager.Instance.CountObjectsOfType(BaseObjectType.DeerSpawnPoint));
		UnityEngine.Debug.Log("Spawnpoints: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			List<PropPrototype> typesWithCategory17 = PropPrototype.GetTypesWithCategory("Town/Props/RoadSigns/Deer");
			float num135 = MathUtil.Squared(64f);
			foreach (TerrainPath path3 in Paths)
			{
				if (path3.IsRiver)
				{
					continue;
				}
				bool flag10 = false;
				for (int num136 = 0; num136 < path3.Points.Count; num136++)
				{
					if (communityManager.GetClosestDeerDistSq(GetTileCoordForPosXZ(MathUtil.ToXZ(path3.Points[num136].Pos))) < num135)
					{
						if (!flag10 && SpawnRoadSign(path3, num136, onRight: true, typesWithCategory17, array, rand) != null)
						{
							flag10 = true;
						}
					}
					else
					{
						flag10 = false;
					}
				}
				flag10 = false;
				for (int num137 = path3.Points.Count - 1; num137 >= 0; num137--)
				{
					if (communityManager.GetClosestDeerDistSq(GetTileCoordForPosXZ(MathUtil.ToXZ(path3.Points[num137].Pos))) < num135)
					{
						if (!flag10 && SpawnRoadSign(path3, num137, onRight: false, typesWithCategory17, array, rand) != null)
						{
							flag10 = true;
						}
					}
					else
					{
						flag10 = false;
					}
				}
			}
		}
		UnityEngine.Debug.Log("Deer Roadsigns: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		using (new StopWatchScope(CurStopwatch))
		{
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (allProp.GetBaseObjectType() != BaseObjectType.Mine)
				{
					GenerateLoot(allProp, rand);
				}
			}
			foreach (Character character2 in Session.Instance.CharacterManager.Characters)
			{
				GenerateLoot(character2, rand);
			}
		}
		UnityEngine.Debug.Log("Loot: " + CurStopwatch.ElapsedMilliseconds + "ms");
		CurGenerationStep++;
		int num138 = 0;
		foreach (Community community in communityManager.Communities)
		{
			if (community.CommunityType != CommunityType.Looter && community.CommunityType != CommunityType.Normal)
			{
				continue;
			}
			List<Community> list10 = new List<Community>();
			foreach (Community community2 in communityManager.Communities)
			{
				if (community2 != community && (community.CommunityType != CommunityType.Normal || community2.CommunityType != CommunityType.Normal) && (community2.CommunityType == CommunityType.Looter || community2.CommunityType == CommunityType.Normal))
				{
					list10.Add(community2);
				}
			}
			SortCommunitiesByLeaderDistAscending comparer = new SortCommunitiesByLeaderDistAscending(community.Leader.PosXZ);
			list10.Sort(comparer);
			int num139 = 0;
			int num140 = rand.Next(0, Math.Max(2, list10.Count / 4));
			while (list10.Count > 0 && num139 < num140)
			{
				int index3 = rand.Next(Math.Min(list10.Count, 3));
				communityManager.SetRelationship(community, list10[index3], CommunityRelationshipType.Hostile);
				list10.RemoveAt(index3);
				num139++;
				num138++;
			}
		}
		foreach (Community community3 in communityManager.Communities)
		{
			if (!community3.IsAISettlement())
			{
				continue;
			}
			foreach (Community community4 in communityManager.Communities)
			{
				if (community3 != community4 && community4.IsAISettlement())
				{
					if (communityManager.GetRelationship(community3, community4) == CommunityRelationshipType.Unknown)
					{
						communityManager.SetRelationship(community3, community4, CommunityRelationshipType.Known);
					}
					if (community3.CommunityType == CommunityType.Looter && community3.GetLivingNonZombieMemberCount() > community4.GetLivingNonZombieMemberCount())
					{
						StoryManager.Instance.IncrementVariable("Extorted", community3, community4, 1f);
					}
				}
			}
		}
		UnityEngine.Debug.Log("Wars: " + num138);
		communityManager.NumFreebieInvaders = rand.Next(communityManager.GetMaxLivingHunterGroups() / 2, communityManager.GetMaxLivingHunterGroups() + 1);
		Array.Copy(Vertices, OriginalVertices, OriginalVertices.Length);
		Generating = false;
	}

	public TreeProp GetNearestTree(TerrainCoord tile)
	{
		TreeProp result = null;
		float num = float.MaxValue;
		foreach (TreeProp treeProp in TreeProps)
		{
			float distSquared = treeProp.GetCentreTile().GetDistSquared(tile);
			if (distSquared < num)
			{
				result = treeProp;
				num = distSquared;
			}
		}
		return result;
	}

	public TerrainCoord GetNearestTileOnPath(Vector2 myPosXZ, bool river, float maxDist)
	{
		TerrainPath nearestPath;
		Vector2 bestDirXZ;
		float pathIndex;
		Vector3? nearestPointOnPath = GetNearestPointOnPath(myPosXZ, river, maxDist, out nearestPath, out bestDirXZ, out pathIndex);
		if (nearestPointOnPath.HasValue)
		{
			return GetTileCoordForPos(nearestPointOnPath.Value);
		}
		return TerrainCoord.Invalid;
	}

	public Vector3? GetNearestPointOnPath(Vector2 myPosXZ, bool river, float maxDist, out TerrainPath nearestPath, out Vector2 bestDirXZ, out float pathIndex)
	{
		using (new UnityProfileMarker(GetNearestPointOnPathStr))
		{
			float num = maxDist * maxDist;
			Vector3? result = null;
			pathIndex = 0f;
			bestDirXZ = Vector2.zero;
			nearestPath = null;
			foreach (TerrainPath path in Paths)
			{
				if (path.IsRiver == river)
				{
					float closestDistSq = num;
					Vector3 closestPointOnPath = Vector3.zero;
					Vector2 closestDirXZOnPath = Vector2.zero;
					float closestPathIndex = 0f;
					if (path.GetClosestPointOnPathToPos(this, myPosXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex) && closestDistSq < num)
					{
						result = closestPointOnPath;
						bestDirXZ = closestDirXZOnPath;
						num = closestDistSq;
						pathIndex = closestPathIndex;
						nearestPath = path;
					}
				}
			}
			return result;
		}
	}

	public TerrainPath GetNearestHitTheRoadPoint(Vector2 posXZ, float maxDist, out RoadDestinationTraits traits)
	{
		float num = maxDist * maxDist;
		TerrainPath result = null;
		RoadDestinationTraits roadDestinationTraits = default(RoadDestinationTraits);
		foreach (TerrainPath path in Paths)
		{
			if (path.IsRiver || DisabledHitTheRoadPathIndices.Contains(path.Index))
			{
				continue;
			}
			if (path.EntrancePointIndex != -1)
			{
				float sqrMagnitude = (path.ControlPoints[path.EntrancePointIndex].Pos - posXZ).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = path;
					roadDestinationTraits = path.EntranceRoadTraits;
					num = sqrMagnitude;
				}
			}
			if (path.ExitPointIndex != -1)
			{
				float sqrMagnitude2 = (path.ControlPoints[path.ExitPointIndex].Pos - posXZ).sqrMagnitude;
				if (sqrMagnitude2 < num)
				{
					result = path;
					roadDestinationTraits = path.ExitRoadTraits;
					num = sqrMagnitude2;
				}
			}
		}
		traits = roadDestinationTraits;
		return result;
	}

	public int GetPathIndexByName(string name)
	{
		for (int i = 0; i < Paths.Count; i++)
		{
			if (Paths[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsRoadEntranceEnabled(int pathIndex)
	{
		return !DisabledEntrancePathIndices.Contains(pathIndex);
	}

	public bool IsHitTheRoadEnabled(int pathIndex)
	{
		return !DisabledHitTheRoadPathIndices.Contains(pathIndex);
	}

	public void SetRoadEntranceEnabled(int pathIndex, bool enabled)
	{
		if (!enabled)
		{
			DisabledEntrancePathIndices.Add(pathIndex);
		}
		else
		{
			DisabledEntrancePathIndices.Remove(pathIndex);
		}
	}

	public void SetHitTheRoadEnabled(int pathIndex, bool enabled)
	{
		if (!enabled)
		{
			DisabledHitTheRoadPathIndices.Add(pathIndex);
		}
		else
		{
			DisabledHitTheRoadPathIndices.Remove(pathIndex);
		}
	}

	public void DeletePath(TerrainPath path)
	{
		path.UnityDeletePath();
		Paths.Remove(path);
		SetupPathIndices();
		Junctions.Clear();
		for (int i = 0; i < DisabledEntrancePathIndices.Count; i++)
		{
			if (DisabledEntrancePathIndices[i] == path.Index)
			{
				DisabledEntrancePathIndices.RemoveAt(i);
				break;
			}
			if (DisabledEntrancePathIndices[i] > path.Index)
			{
				DisabledEntrancePathIndices[i]--;
			}
		}
		for (int j = 0; j < DisabledHitTheRoadPathIndices.Count; j++)
		{
			if (DisabledHitTheRoadPathIndices[j] == path.Index)
			{
				DisabledHitTheRoadPathIndices.RemoveAt(j);
				break;
			}
			if (DisabledHitTheRoadPathIndices[j] > path.Index)
			{
				DisabledHitTheRoadPathIndices[j]--;
			}
		}
	}

	public void SetupPathIndices()
	{
		for (int i = 0; i < Paths.Count; i++)
		{
			Paths[i].Index = i;
		}
	}

	public void SetupRoadSkillCapBonus()
	{
		Session instance = Session.Instance;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < Paths.Count; i++)
		{
			if (Paths[i].IsRiver)
			{
				continue;
			}
			RoadDestinationTraits entranceRoadTraits = Paths[i].EntranceRoadTraits;
			RoadDestinationTraits exitRoadTraits = Paths[i].ExitRoadTraits;
			if (!UseFixedTerrain)
			{
				if (!flag)
				{
					entranceRoadTraits.Type = RoadDestinationType.KeepCurrentSettings;
					flag = true;
				}
				else if (!flag2)
				{
					entranceRoadTraits.Type = RoadDestinationType.HarderSettings;
					flag2 = true;
				}
			}
			entranceRoadTraits.CalcSkillCapBonus(instance.DifficultySettings);
			exitRoadTraits.CalcSkillCapBonus(instance.DifficultySettings);
			Paths[i].EntranceRoadTraits = entranceRoadTraits;
			Paths[i].ExitRoadTraits = exitRoadTraits;
		}
	}

	public void OnEditorSave()
	{
		SetupPathIndices();
	}

	public static void GenerateLoot(TileObject obj, CustomRandom rand)
	{
		int num = Mathf.CeilToInt((float)MathUtil.Clamp((int)obj.GetMaxInventoryWeight() / 200, 2, 20) * (Session.Instance.DifficultySettings.LootDensity / 100f));
		bool includeClothing = false;
		bool fillLiquidContainers = false;
		LootLocationDef lootLocationDef = GameImpl.Instance.FindLootLocation(obj.GetLootLocation());
		if (lootLocationDef != null)
		{
			includeClothing = rand.RandomChoice(lootLocationDef.ClothingProbability);
			fillLiquidContainers = !lootLocationDef.EmptyLiquidContainers;
		}
		else
		{
			num = 0;
		}
		if (obj is Character character && character.HasRole(Role.Trader))
		{
			num = 32;
			includeClothing = rand.RandomChoice(Character.TraderStockClothingProbability);
		}
		int lootCount = rand.Next(num / 2, num + 1);
		GenerateLoot(obj, lootCount, rand, includeClothing, fillLiquidContainers);
	}

	public static void GenerateLoot(TileObject obj, int lootCount, CustomRandom rand, bool includeClothing, bool fillLiquidContainers)
	{
		EquipmentContainer inventory = obj.GetInventory();
		if (inventory == null)
		{
			return;
		}
		float num = MaxLootFilledAmount * obj.GetMaxInventoryWeight();
		Character character = obj as Character;
		int traderLevel = character?.TraderLevel ?? 0;
		if (character != null && character.Community != null && (character.HasRole(Role.Trader) || rand.RandomChoice(0.25f)))
		{
			foreach (Prop building in character.Community.Buildings)
			{
				if (building is Mine mine)
				{
					for (int i = 1; i < 4; i++)
					{
						EquipmentPrototype equipmentPrototype = EquipmentPrototype.MiningResources[i];
						if (equipmentPrototype != null && mine.HasRichDeposits((MineralType)i))
						{
							int val = rand.Next(lootCount / 2, lootCount);
							val = Math.Min(val, (int)((num - inventory.GetWeight(obj)) / equipmentPrototype.Weight));
							val -= character.Inventory.CountItemsOfType(equipmentPrototype);
							if (val > 0)
							{
								obj.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype, val, fillLiquidContainers);
							}
						}
					}
					break;
				}
				if (!(building is Still) || !character.HasRole(Role.Trader))
				{
					continue;
				}
				foreach (KeyValuePair<string, LiquidPrototype> item in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
				{
					LiquidPrototype value = item.Value;
					if (!(value.AlcoholContent > 0f))
					{
						continue;
					}
					EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.FindDefaultContainerForLiquidType(value);
					if (equipmentPrototype2 != null)
					{
						while (inventory.GetTotalLiquid(value) < equipmentPrototype2.LiquidCapacity * (equipmentPrototype2.DefaultLiquidFilledMin / 100f) * 3f && obj.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype2, 1, fillLiquidContainers: true) != 0 && inventory.CountItemsOfType(equipmentPrototype2) <= 3)
						{
						}
					}
				}
			}
		}
		for (int j = 0; j < lootCount; j++)
		{
			EquipmentPrototype equipmentPrototype3 = GameImpl.Instance.PickRandomItem(obj.GetLootLocation(), rand, includeClothing, traderLevel);
			if (equipmentPrototype3 != null)
			{
				int val2 = rand.Next(Math.Max(1, equipmentPrototype3.TypicalLootAmount / 2), Math.Max(2, equipmentPrototype3.TypicalLootAmount + 1));
				val2 = Math.Min(val2, (int)((num - inventory.GetWeight(obj)) / equipmentPrototype3.Weight));
				if (val2 > 0)
				{
					obj.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype3, val2, fillLiquidContainers);
				}
			}
		}
		inventory.GeneratedLoot = true;
	}

	private bool IsGridTileProcessed(bool[,] grid, TerrainCoord gridTile, int gridRange)
	{
		for (int i = gridTile.x - gridRange; i <= gridTile.x + gridRange; i++)
		{
			for (int j = gridTile.y - gridRange; j <= gridTile.y + gridRange; j++)
			{
				if (grid[i, j])
				{
					return true;
				}
			}
		}
		return false;
	}

	private void GenerateSpawnPoints(bool[,] grid, TerrainCoord startTile, int minCount, int maxCount, BaseObjectType spawnPointType, CustomRandom rand, PlayerStartPoint playerStartPoint, int gridRange)
	{
		List<TerrainCoord> list = new List<TerrainCoord>();
		list.Add(new TerrainCoord(startTile.x / 16, startTile.y / 16));
		int num = 2;
		int num2 = rand.Next(minCount, maxCount + 1);
		int num3 = 0;
		while (list.Count > 0 && num3 < num2)
		{
			int index = rand.Next(list.Count);
			TerrainCoord gridTile = list[index];
			list.RemoveAt(index);
			int num4 = Size / 16;
			if (gridTile.x < num || gridTile.y < num || gridTile.x >= num4 - num || gridTile.y >= num4 - num || IsGridTileProcessed(grid, gridTile, gridRange))
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				TerrainCoord terrainCoord = new TerrainCoord(gridTile.x * 16 + rand.Next() % 16, gridTile.y * 16 + rand.Next() % 16);
				if (!IsSlopeOrImpassableRaw(terrainCoord.x, terrainCoord.y) && GetFixedObjectOnTile(terrainCoord.x, terrainCoord.y) == null && GetOwnerCommunityIdForTile(terrainCoord.x, terrainCoord.y) == 0 && !IsTileRiver(terrainCoord.x, terrainCoord.y))
				{
					if ((spawnPointType == BaseObjectType.RedStrainSpawnPoint || spawnPointType == BaseObjectType.WhiteStrainSpawnPoint) && Session.Instance.DifficultySettings.GreenStrainDensity > 10f && playerStartPoint.Tile.GetDist(terrainCoord) < (float)(Size / 4))
					{
						spawnPointType = BaseObjectType.GreenStrainSpawnPoint;
					}
					SingleTileProp.Spawn(spawnPointType, terrainCoord);
					list.Add(new TerrainCoord(gridTile.x + 1, gridTile.y));
					list.Add(new TerrainCoord(gridTile.x - 1, gridTile.y));
					list.Add(new TerrainCoord(gridTile.x, gridTile.y + 1));
					list.Add(new TerrainCoord(gridTile.x, gridTile.y - 1));
					num3++;
					break;
				}
			}
			grid[gridTile.x, gridTile.y] = true;
		}
	}
}

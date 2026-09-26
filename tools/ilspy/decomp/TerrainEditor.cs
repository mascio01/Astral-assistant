public class TerrainEditor : DebugMenu
{
	public static bool ShowImpassable;

	public static bool ShowOwnership;

	public static int ShowOwnershipForCommunityId;

	public static bool ShowPaths;

	public static bool GetShowImpassable()
	{
		return ShowImpassable;
	}

	public static void SetShowImpassable(bool v)
	{
		ShowImpassable = v;
		ShowOwnership = false;
		if (GameTerrain.Instance != null)
		{
			GameTerrain.Instance.BuildEntireMinimap();
		}
	}

	public static bool GetShowOwnership()
	{
		return ShowOwnership;
	}

	public static void SetShowOwnership(bool v)
	{
		ShowOwnership = v;
		ShowImpassable = false;
		if (GameTerrain.Instance != null)
		{
			GameTerrain.Instance.BuildEntireMinimap();
		}
	}

	public static bool GetShowPaths()
	{
		return ShowPaths;
	}

	public static void SetShowPaths(bool on)
	{
		ShowPaths = on;
	}

	public TerrainEditor()
		: base(GameImpl.Translate("DEBUG_TerrainEditor"))
	{
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CoordinateFinder"), typeof(TerrainCoordinateFinder)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_TreeEditor"), typeof(TreePropEditor)));
		if (Session.Instance.Editor)
		{
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_HeightEditor"), typeof(TerrainHeightEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_TextureEditor"), typeof(TerrainTextureEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_GrassEditor"), typeof(GrassEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CamberEditor"), typeof(TerrainCamberEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_ButteEditor"), typeof(TerrainButteEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CanyonEditor"), typeof(TerrainCanyonEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_DuneEditor"), typeof(TerrainDuneEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_RockEditor"), typeof(TerrainRockEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Roughener"), typeof(TerrainRoughener)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Smoothener"), typeof(TerrainSmoothener)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Flattener"), typeof(TerrainFlattener)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_GeologicalMapEditor"), typeof(GeologicalMapEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_MineralEditor"), typeof(MineralEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_RockSpawner"), typeof(TerrainRockSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_VehicleSpawner"), typeof(VehicleSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_BiomeEditor"), typeof(BiomeEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_PathEditor"), typeof(PathEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_PassabilityEditor"), typeof(PassabilityEditor)));
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ComplexPathfinding"), () => GameTerrain.Instance.ComplexPathfinding, delegate(bool v)
			{
				GameTerrain.Instance.ComplexPathfinding = v;
			}));
		}
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowImpassable"), GetShowImpassable, SetShowImpassable));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowOwnership"), GetShowOwnership, SetShowOwnership));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowPaths"), GetShowPaths, SetShowPaths));
	}

	private void Treeify()
	{
		GameTerrain instance = GameTerrain.Instance;
		for (int i = 0; i < instance.Size; i += 8)
		{
			for (int j = 0; j < instance.Size; j += 8)
			{
				TreeProp.Spawn(TreeType.Conifer1, new TerrainCoord(i, j), 1f, MathUtil.NonDeterministicRand);
			}
		}
	}
}

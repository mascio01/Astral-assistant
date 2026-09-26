public class TerrainCamberEditor : DebugMenu
{
	public float BaseHeight;

	public float Radius = 5f;

	public float Height = 0.2f;

	private bool Drawing;

	private TerrainCoord LastDrawTile;

	public TerrainCamberEditor()
		: base(GameImpl.Translate("DEBUG_CamberEditor"))
	{
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_BaseHeight"), 0f, 64f, this, "BaseHeight"));
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_Radius"), 0f, 10f, this, "Radius"));
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_Height"), 0f, 10f, this, "Height"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRivers"), typeof(TerrainHeightEditor), "LockRivers"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRoads"), typeof(TerrainHeightEditor), "LockRoads"));
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (instance3.IsPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				if (!Drawing || LastDrawTile != raycastResult.Tile)
				{
					instance2.DrawCamber(raycastResult.Tile, BaseHeight, Radius, Height, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
					LastDrawTile = raycastResult.Tile;
				}
				Drawing = true;
				Session.Instance.AchievementsEnabled = false;
			}
		}
		else
		{
			Drawing = false;
		}
		if (instance3.IsJustPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				BaseHeight = raycastResult2.GetHitPosition().y;
			}
		}
	}
}

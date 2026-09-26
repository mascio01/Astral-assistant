public class TerrainSmoothener : DebugMenu
{
	private float Radius = 4f;

	private float SmoothRadius = 2f;

	private float Amount = 1f;

	private void SetRadius(float radius)
	{
		Radius = radius;
	}

	private void SetSmoothRadius(float radius)
	{
		SmoothRadius = radius;
	}

	private float GetRadius()
	{
		return Radius;
	}

	private float GetSmoothRadius()
	{
		return SmoothRadius;
	}

	public TerrainSmoothener()
		: base(GameImpl.Translate("DEBUG_TerrainSmoothener"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 100f, GetRadius, SetRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_SmoothRadius"), 1f, 100f, GetSmoothRadius, SetSmoothRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), 0f, 10f, () => Amount, delegate(float v)
		{
			Amount = v;
		}));
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
				instance2.Smooth(instance2.GetTileCoordForPos(raycastResult.GetHitPosition()), Radius, SmoothRadius, 1f, Amount, ignoreImpassable: false, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads, null);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

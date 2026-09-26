public class TerrainRoughener : DebugMenu
{
	private float Radius = 4f;

	private float Amount = 0.125f;

	private void SetRadius(float radius)
	{
		Radius = radius;
	}

	private void SetAmount(float amount)
	{
		Amount = amount;
	}

	private float GetRadius()
	{
		return Radius;
	}

	private float GetAmount()
	{
		return Amount;
	}

	public TerrainRoughener()
		: base(GameImpl.Translate("DEBUG_TerrainRoughener"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 10f, GetRadius, SetRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), 0f, 1f, GetAmount, SetAmount));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_WholeTerrain"), RoughenWholeTerrain));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRivers"), typeof(TerrainHeightEditor), "LockRivers"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRoads"), typeof(TerrainHeightEditor), "LockRoads"));
	}

	private void RoughenWholeTerrain()
	{
		GameTerrain.Instance.Roughen(new TerrainCoord(512, 512), 512f, Amount, ignoreImpassable: true, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
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
				instance2.Roughen(instance2.GetTileCoordForPos(raycastResult.GetHitPosition()), Radius, Amount, ignoreImpassable: true, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

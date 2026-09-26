public class TerrainHeightEditor : DebugMenu
{
	private float Radius = 1f;

	private float Amount = 1f;

	public static bool LockRivers;

	public static bool LockRoads;

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

	public TerrainHeightEditor()
		: base(GameImpl.Translate("DEBUG_HeightEditor"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 100f, GetRadius, SetRadius));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), -10f, 10f, GetAmount, SetAmount));
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
				instance2.AdjustHeight(MathUtil.ToXZ(raycastResult.GetHitPosition()), Radius, Amount * GameImpl.UnscaledDeltaTime, LockRivers, LockRoads, apply: true);
				Session.Instance.AchievementsEnabled = false;
			}
			instance2.UnityTerrain.detailObjectDistance = 0f;
		}
		else if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance2.AdjustHeight(MathUtil.ToXZ(raycastResult2.GetHitPosition()), Radius, (0f - Amount) * GameImpl.UnscaledDeltaTime, LockRivers, LockRoads, apply: true);
				Session.Instance.AchievementsEnabled = false;
			}
			instance2.UnityTerrain.detailObjectDistance = 0f;
		}
		else
		{
			instance2.UnityTerrain.detailObjectDistance = 80f;
		}
	}
}

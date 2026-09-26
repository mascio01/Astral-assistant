public class PassabilityEditor : DebugMenu
{
	private enum FlagType
	{
		Impassable,
		Slope,
		River
	}

	private FlagType CurrentFlagType;

	private bool OldShowImpassable;

	public PassabilityEditor()
		: base(GameImpl.Translate("DEBUG_PassabilityEditor"))
	{
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_Slope"), () => CurrentFlagType == FlagType.Slope, delegate(bool v)
		{
			CurrentFlagType = (v ? FlagType.Slope : FlagType.Impassable);
		}));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_IsRiver"), () => CurrentFlagType == FlagType.River, delegate(bool v)
		{
			CurrentFlagType = (v ? FlagType.River : FlagType.Impassable);
		}));
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		OldShowImpassable = TerrainEditor.ShowImpassable;
		TerrainEditor.ShowImpassable = true;
	}

	public override void DeactivateImpl()
	{
		TerrainEditor.ShowImpassable = OldShowImpassable;
		base.DeactivateImpl();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		TileFlags flag = TileFlags.Impassable;
		switch (CurrentFlagType)
		{
		case FlagType.Slope:
			flag = TileFlags.Slope;
			break;
		case FlagType.River:
			flag = TileFlags.River;
			break;
		}
		if (instance3.IsPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				instance2.SetFlag(raycastResult.Tile.x, raycastResult.Tile.y, flag, on: true);
				Session.Instance.AchievementsEnabled = false;
			}
		}
		if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance2.SetFlag(raycastResult2.Tile.x, raycastResult2.Tile.y, flag, on: false);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

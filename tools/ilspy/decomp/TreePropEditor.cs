public class TreePropEditor : DebugMenu
{
	private TreeType CurrentTreeType;

	public TreePropEditor()
		: base(GameImpl.Translate("DEBUG_TreeEditor"))
	{
		for (int i = 0; i < 18; i++)
		{
			TreeType treeType = (TreeType)i;
			Items.Add(new DebugMenuItemToggle(GameTerrain.TreeTypeNames[i], () => CurrentTreeType == treeType, delegate
			{
				CurrentTreeType = treeType;
			}));
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (!instance3.IsJustPressed(InputFunction.MainAction))
		{
			return;
		}
		RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
		if (raycastResult.HitObject == instance2)
		{
			TileObject fixedObjectOnTile = instance2.GetFixedObjectOnTile(raycastResult.Tile.x, raycastResult.Tile.y);
			if (fixedObjectOnTile == null || fixedObjectOnTile is Zone)
			{
				TreeProp.Spawn(CurrentTreeType, raycastResult.Tile, 1f, MathUtil.NonDeterministicRand);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

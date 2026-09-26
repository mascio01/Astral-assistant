public class CommunityAreaOwnershipEditor : DebugMenu
{
	private Community _community;

	public CommunityAreaOwnershipEditor(Community community)
		: base(GameImpl.Translate("DEBUG_AreaOwnershipEditor"))
	{
		_community = community;
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		TerrainEditor.ShowOwnership = true;
		TerrainEditor.ShowOwnershipForCommunityId = _community.Id;
	}

	public override void DeactivateImpl()
	{
		TerrainEditor.ShowOwnershipForCommunityId = 0;
		TerrainEditor.ShowOwnership = false;
		base.DeactivateImpl();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (instance3.IsJustPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				instance2.SetOwnerCommunityIdForLookupTileManually(raycastResult.Tile.x, raycastResult.Tile.y, _community.Id);
				Session.Instance.AchievementsEnabled = false;
			}
		}
		else if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance2.SetOwnerCommunityIdForLookupTileManually(raycastResult2.Tile.x, raycastResult2.Tile.y, 0);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

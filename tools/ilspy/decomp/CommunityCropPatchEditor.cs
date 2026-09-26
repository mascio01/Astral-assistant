public class CommunityCropPatchEditor : DebugMenu
{
	private Community _community;

	private PropPrototype CropType;

	private bool WantRepopulate;

	public CommunityCropPatchEditor(Community community)
		: base(GameImpl.Translate("DEBUG_CropPatchEditor"))
	{
		_community = community;
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		GameCursor.DrawAllCropPatches = true;
		WantRepopulate = true;
	}

	public override void DeactivateImpl()
	{
		GameCursor.DrawAllCropPatches = false;
		base.DeactivateImpl();
	}

	private void Populate()
	{
		Items.Clear();
		foreach (PropPrototype proto in PropPrototype.PlantableCropTypes)
		{
			int num = _community.CountInventoryItemsOfType(proto.HarvestSeedsPrototype);
			int communityTilesCountForCropType = Session.Instance.CropsManager.GetCommunityTilesCountForCropType(_community.Id, proto);
			float num2 = ((communityTilesCountForCropType > 0) ? ((float)num / (float)communityTilesCountForCropType) : 0f);
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate(proto.NameHash) + " " + num + "/" + communityTilesCountForCropType + "=" + num2, () => CropType == proto, delegate(bool v)
			{
				CropType = (v ? proto : null);
			}));
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		if (WantRepopulate)
		{
			Populate();
			WantRepopulate = false;
		}
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (instance3.IsPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				instance.CropsManager.SetPlantableCropType(_community.Id, raycastResult.Tile, CropType);
				WantRepopulate = true;
				Session.Instance.AchievementsEnabled = false;
			}
		}
		else if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance.CropsManager.SetPlantableCropType(_community.Id, raycastResult2.Tile, null);
				WantRepopulate = true;
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

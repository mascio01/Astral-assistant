public class CharacterTeleporter : DebugMenu
{
	public CharacterTeleporter()
		: base(GameImpl.Translate("DEBUG_Teleporter"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		instance.GameCamera.SetFlyCamMode(on: true, snap: false, inputFrame);
		Character currentCharacter = CharacterEditor.GetCurrentCharacter();
		if (currentCharacter != null && instance3.IsPressed(InputFunction.MainAction))
		{
			instance.AchievementsEnabled = false;
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				TerrainCoord tileCoordForPos = instance2.GetTileCoordForPos(raycastResult.GetHitPosition());
				currentCharacter.SetTile(tileCoordForPos);
			}
		}
	}
}

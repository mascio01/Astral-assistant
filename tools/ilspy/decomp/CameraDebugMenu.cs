public class CameraDebugMenu : DebugMenu
{
	public CameraDebugMenu()
		: base(GameImpl.Translate("DEBUG_CameraDebug"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_FocusPointHeight"), 0f, 2f, GetFocusPointHeight, SetFocusPointHeight));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DirectControlDistUnarmed"), 0f, 16f, GetZoomedInDistUnarmed, SetZoomedInDistUnarmed));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DirectControlDistMelee"), 0f, 16f, GetZoomedInDistMelee, SetZoomedInDistMelee));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DirectControlDistRanged"), 0f, 16f, GetZoomedInDistRanged, SetZoomedInDistRanged));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DirectControlDistForest"), 0f, 16f, GetZoomedInDistForest, SetZoomedInDistForest));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_CommandModeDistMin"), 0f, 64f, GetZoomedOutDistMin, SetZoomedOutDistMin));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_CommandModeDistMax"), 0f, 64f, GetZoomedOutDistMax, SetZoomedOutDistMax));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DirectControlPitch"), -89f, 89f, GetDirectControlPitchOnFlatGround, SetDirectControlPitchOnFlatGround));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_CommandModePitch"), -89f, 89f, GetFlyCamPitch, SetFlyCamPitch));
	}

	public static float GetFocusPointHeight()
	{
		return GameCamera.FocusPointHeight;
	}

	public static void SetFocusPointHeight(float h)
	{
		GameCamera.FocusPointHeight = h;
	}

	public float GetZoomedInDistUnarmed()
	{
		return GameCamera.ZoomedInDistUnarmed;
	}

	public void SetZoomedInDistUnarmed(float v)
	{
		GameCamera.ZoomedInDistUnarmed = v;
	}

	public float GetZoomedInDistMelee()
	{
		return GameCamera.ZoomedInDistMelee;
	}

	public void SetZoomedInDistMelee(float v)
	{
		GameCamera.ZoomedInDistMelee = v;
	}

	public float GetZoomedInDistRanged()
	{
		return GameCamera.ZoomedInDistRanged;
	}

	public void SetZoomedInDistRanged(float v)
	{
		GameCamera.ZoomedInDistRanged = v;
	}

	public float GetZoomedInDistForest()
	{
		return GameCamera.ZoomedInDistForest;
	}

	public void SetZoomedInDistForest(float v)
	{
		GameCamera.ZoomedInDistForest = v;
	}

	public float GetZoomedOutDistMax()
	{
		return GameCamera.ZoomedOutDistMax;
	}

	public void SetZoomedOutDistMax(float v)
	{
		GameCamera.ZoomedOutDistMax = v;
	}

	public float GetZoomedOutDistMin()
	{
		return GameCamera.ZoomedOutDistMin;
	}

	public void SetZoomedOutDistMin(float v)
	{
		GameCamera.ZoomedOutDistMin = v;
	}

	public float GetDirectControlPitchOnFlatGround()
	{
		return GameCamera.DirectControlPitchOnFlatGround;
	}

	public void SetDirectControlPitchOnFlatGround(float v)
	{
		GameCamera.DirectControlPitchOnFlatGround = v;
	}

	public float GetFlyCamPitch()
	{
		return GameCamera.FlyCamPitch;
	}

	public void SetFlyCamPitch(float v)
	{
		GameCamera.FlyCamPitch = v;
	}
}

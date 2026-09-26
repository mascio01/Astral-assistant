public class BiomeEditor : DebugMenu
{
	private ManualBiomeType CurrentBiomeType;

	private float Radius = 10f;

	private float Border = 2f;

	private float GrassPerlinScale = 32f;

	private float FlowersPerlinScale = 4f;

	public BiomeEditor()
		: base(GameImpl.Translate("DEBUG_BiomeEditor"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 1f, 10f, () => Radius, delegate(float v)
		{
			Radius = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster("Border", 1f, 8f, () => Border, delegate(float v)
		{
			Border = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster("Grass Perlin Scale", 1f, 128f, () => GrassPerlinScale, delegate(float v)
		{
			GrassPerlinScale = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster("Flowers Perlin Scale", 1f, 32f, () => FlowersPerlinScale, delegate(float v)
		{
			FlowersPerlinScale = v;
		}));
		for (int num = 0; num < 5; num++)
		{
			ManualBiomeType biomeType = (ManualBiomeType)num;
			Items.Add(new DebugMenuItemToggle(biomeType.ToString(), () => CurrentBiomeType == biomeType, delegate
			{
				CurrentBiomeType = biomeType;
			}));
		}
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
				instance2.SetBiome(CurrentBiomeType, raycastResult.Tile, Radius, Border, FlowersPerlinScale, GrassPerlinScale, GameImpl.UnscaledDeltaTime);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}

using System.Text;

public class SunFiddler : DebugMenu
{
	private Sun Sun;

	public static float HourAdjustAmount = 0.25f;

	public static float AngleAdjustAmount = 1f;

	public static bool FullBright = false;

	public static bool SunLightEnabled = true;

	public static bool MoonLightEnabled = true;

	public static bool FillLightEnabled = true;

	public static bool BackLightEnabled = true;

	public static float BackLightAngle = 90f;

	public static bool AmbientLightEnabled = true;

	public SunFiddler()
		: base(GameImpl.Translate("DEBUG_SunFiddler"))
	{
		Sun = GameImpl.Instance.Sun;
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_StartHourOfDay"), DecrementStartHourOfDay, IncrementStartHourOfDay, BuildEastWestAngleString, affectsGameState: true));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_SunNorthSouthAngle"), DecrementNorthSouthAngle, IncrementNorthSouthAngle, BuildNorthSouthAngleString));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_MoonEastWestAngle"), DecrementMoonEastWestAngle, IncrementMoonEastWestAngle, BuildMoonEastWestAngleString));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_MoonNorthSouthAngle"), DecrementMoonNorthSouthAngle, IncrementMoonNorthSouthAngle, BuildMoonNorthSouthAngleString));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_FullBright"), GetFullBright, SetFullBright));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_SunLightEnabled"), GetSunLightEnabled, SetSunLightEnabled));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_MoonLightEnabled"), GetMoonLightEnabled, SetMoonLightEnabled));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_FillLightEnabled"), GetFillLightEnabled, SetFillLightEnabled));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_BackLightEnabled"), GetBackLightEnabled, SetBackLightEnabled));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_BackLightAngle"), 0f, 180f, GetBackLightAngle, SetBackLightAngle));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_AmbientLightEnabled"), GetAmbientLightEnabled, SetAmbientLightEnabled));
		Items.Add(new DebugMenuItemToggleField("Pip Command Buffers", typeof(GraphicsDebugMenu), "PipCommandBuffers"));
		Items.Add(new DebugMenuItemToggleField("Pip Spherical Harmonics", typeof(GraphicsDebugMenu), "PipSphericalHarmonics"));
		Items.Add(new DebugMenuFloatFieldAdjuster("SHFillLightIntensity", 0f, 10f, typeof(PipCameraBehaviour), "SHFillLightIntensity"));
		Items.Add(new DebugMenuFloatFieldAdjuster("SHBackLightIntensity", 0f, 10f, typeof(PipCameraBehaviour), "SHBackLightIntensity"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Boost", 0f, 10f, typeof(PipCameraBehaviour), "LightingBoostFactor"));
	}

	public void IncrementStartHourOfDay()
	{
		Session.Instance.Weather.StartHourOfDay += HourAdjustAmount;
		Session.Instance.Weather.LastSetWeatherGlobalsTime = Target.Never;
	}

	public void DecrementStartHourOfDay()
	{
		Session.Instance.Weather.StartHourOfDay -= HourAdjustAmount;
		Session.Instance.Weather.LastSetWeatherGlobalsTime = Target.Never;
	}

	public void BuildEastWestAngleString(ref StringBuilder value)
	{
		value.AppendWithoutGarbage(Session.Instance.Weather.StartHourOfDay, 3);
	}

	public void IncrementNorthSouthAngle()
	{
		Sun.NorthSouthAngle += AngleAdjustAmount;
	}

	public void DecrementNorthSouthAngle()
	{
		Sun.NorthSouthAngle -= AngleAdjustAmount;
	}

	public void BuildNorthSouthAngleString(ref StringBuilder value)
	{
		value.AppendWithoutGarbage(Sun.NorthSouthAngle, 1);
	}

	public void IncrementMoonNorthSouthAngle()
	{
		Sun.MoonNorthSouthAngle += AngleAdjustAmount;
	}

	public void DecrementMoonNorthSouthAngle()
	{
		Sun.MoonNorthSouthAngle -= AngleAdjustAmount;
	}

	public void BuildMoonNorthSouthAngleString(ref StringBuilder value)
	{
		value.AppendWithoutGarbage(Sun.MoonNorthSouthAngle, 1);
	}

	public void IncrementMoonEastWestAngle()
	{
		Sun.MoonEastWestAngle += AngleAdjustAmount;
	}

	public void DecrementMoonEastWestAngle()
	{
		Sun.MoonEastWestAngle -= AngleAdjustAmount;
	}

	public void BuildMoonEastWestAngleString(ref StringBuilder value)
	{
		value.AppendWithoutGarbage(Sun.MoonEastWestAngle, 1);
	}

	public bool GetFullBright()
	{
		return FullBright;
	}

	public bool GetSunLightEnabled()
	{
		return SunLightEnabled;
	}

	public bool GetMoonLightEnabled()
	{
		return MoonLightEnabled;
	}

	public bool GetFillLightEnabled()
	{
		return FillLightEnabled;
	}

	public bool GetBackLightEnabled()
	{
		return BackLightEnabled;
	}

	public bool GetAmbientLightEnabled()
	{
		return AmbientLightEnabled;
	}

	public void SetFullBright(bool v)
	{
		FullBright = v;
	}

	public void SetSunLightEnabled(bool v)
	{
		SunLightEnabled = v;
	}

	public void SetMoonLightEnabled(bool v)
	{
		MoonLightEnabled = v;
	}

	public void SetFillLightEnabled(bool v)
	{
		FillLightEnabled = v;
	}

	public void SetBackLightEnabled(bool v)
	{
		BackLightEnabled = v;
	}

	public void SetAmbientLightEnabled(bool v)
	{
		AmbientLightEnabled = v;
	}

	public void SetBackLightAngle(float v)
	{
		BackLightAngle = v;
	}

	public float GetBackLightAngle()
	{
		return BackLightAngle;
	}
}

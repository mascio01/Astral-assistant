public struct GlobalLightingSettingsForSunAngle
{
	public float SunAngle;

	public GlobalLightingSettings LightingSettings;

	public GlobalLightingSettingsForSunAngle(float sun_angle, GlobalLightingSettings lighting_settings)
	{
		SunAngle = sun_angle;
		LightingSettings = lighting_settings;
	}
}

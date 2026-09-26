public class WeatherFiddler : DebugMenu
{
	private Weather Weather;

	private DebugMenuItemText AvgTempTimeText;

	private DebugMenuItemText ThreadCalcTimeText;

	public WeatherFiddler()
		: base(GameImpl.Translate("DEBUG_Weather"))
	{
		Weather = Session.Instance.Weather;
		AvgTempTimeText = new DebugMenuItemText(GameImpl.Translate("DEBUG_AvgTemperature"), Weather.CalcAverageTemperatureInCelsius() + "°C");
		Items.Add(AvgTempTimeText);
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_StartDayOfYear"), 0, Weather.DaysInAYear - 1, Weather.GetStartDayOfYear, Weather.SetStartDayOfYear, affectsGameState: true));
		for (int i = 0; i < 3; i++)
		{
			WeatherType weatherType = (WeatherType)i;
			Items.Add(new DebugMenuItemToggle(Weather.WeatherTypeNames[i], () => Session.Instance.Weather.CurrentWeatherType == weatherType, delegate
			{
				Session.Instance.Weather.StartFront(weatherType, MathUtil.NonDeterministicRand);
			}, affectsGameState: true));
		}
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_FrontTransition"), 0f, 1f, Weather.GetFrontTransition, Weather.SetFrontTransition, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Temperature"), Weather.AverageDaytimeWinterTemperatureInCelsius - Weather.TemperatureRangeInCelsius - Weather.DiurnalTemperatureVariationInCelsius, Weather.AverageDaytimeSummerTemperatureInCelsius + Weather.TemperatureRangeInCelsius + Weather.DiurnalTemperatureVariationInCelsius, Weather.GetTemperatureInCelsius, Weather.SetTemperatureInCelsius, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Cloudiness"), 0f, 1f, Weather.GetCloudiness, Weather.SetCloudiness, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Precipitation"), 0f, 1f, Weather.GetPrecipitationAmount, Weather.SetPrecipitationAmount, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_SnowOnGround"), 0f, 1f, Weather.GetSnowOnGroundAmount, Weather.SetSnowOnGroundAmount, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_GroundMuddiness"), 0f, 1f, Weather.GetGroundMuddiness, Weather.SetGroundMuddiness, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WindAngle"), -180f, 180f, Weather.GetWindAngleDeg, Weather.SetWindAngleDeg, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WindStrength"), 0f, 1f, Weather.GetWindStrength, Weather.SetWindStrength, affectsGameState: true));
		ThreadCalcTimeText = new DebugMenuItemText(GameImpl.Translate("DEBUG_ParticleThreadCalcTime"), "");
		Items.Add(ThreadCalcTimeText);
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		AvgTempTimeText.Text = Weather.CalcAverageTemperatureInCelsius() + "°C";
		ThreadCalcTimeText.Text = WeatherParticleSystem.Instance.ThreadCalcTime.TotalMilliseconds + "ms";
	}
}

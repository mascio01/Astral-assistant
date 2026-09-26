using System;
using System.Threading;
using UnityEngine;

public class Weather : IReflectable
{
	public static float AltitudeInMetres = 2500f;

	public static float AverageDaytimeSummerTemperatureInCelsius = 25f;

	public static float AverageDaytimeWinterTemperatureInCelsius = -5f;

	public static float DiurnalTemperatureVariationInCelsius = 5f;

	public static float TemperatureRangeInCelsius = 10f;

	public static float SnowOnGroundBuildupRate = 1f / 60f;

	public static float SnowOnGroundMeltRate = 0.0016666667f;

	public static float MudOnGroundBuildupRate = 0.1f;

	public static float MudOnGroundDryRate = 0.0016666667f;

	public static float MudFromSnowRatio = 4f;

	public static float WaterCompletelyFrozenTemperatureInCelsius = -5f;

	public static float RiversDrinkableTemperatureInCelsius = -4f;

	public static float IceCollisionTemperatureInCelsius = -1f;

	public static float ScoopableSnowOnGroundAmount = 0.75f;

	public static float DefaultWindStrength = 0.25f;

	public static float WindInGrassVolume = 0.25f;

	public static float WindInLeavesVolume = 1.5f;

	public static int DaysInAMonth = 7;

	public static int MonthsInAYear = 12;

	public static int DaysInAYear = DaysInAMonth * MonthsInAYear;

	public static string[] WeatherTypeNames = StringUtil.GetEnumNames<WeatherType>();

	public int StartDayOfYear;

	public float StartHourOfDay = 9f;

	public WeatherType CurrentWeatherType;

	public float FrontDurationSecs;

	public float FrontStrength;

	public float FrontTransition;

	public float TemperatureInCelsius;

	public float TemperatureInCelsiusNotIncludingDiurnalVariation;

	public float TemperatureInCelsiusWhenFrontStarted;

	public float Cloudiness = 0.5f;

	public float CloudinessWhenFrontStarted;

	public float PrecipitationAmount;

	public float PrecipitationWhenFrontStarted;

	public float GroundMuddiness;

	public float SnowOnGroundAmount;

	public float WindAngleDeg;

	public float WindAngleDegWhenFrontStarted;

	public float WindStrength = DefaultWindStrength;

	public float WindStrengthWhenFrontStarted = DefaultWindStrength;

	public WeatherBehaviour WeatherSettings;

	public WindZone UnityWindZone;

	public Vector2 CloudPosXZ;

	public float WindTime;

	public Vector4 PlayerPosSpeedTerrainSize;

	public int LastPlayerId;

	public static float SnowSettleTempCelcius = 2f;

	public const float SnowTemp = 0f;

	public const float SleetTemp = 2f;

	public const float RainTemp = 5f;

	public static float SnowTiling = 446f;

	public static float SnowNoiseTiling = 646f;

	private float RiverSoundMaxDist;

	private float WindInLeavesSoundMaxDist;

	private float Waterfallness;

	private Vector3? RiverPos;

	private TreeProp NearestTree;

	private TaskFunc WeatherUpdateTaskFunc;

	public ManualResetEvent WeatherUpdateFinishedEvent = new ManualResetEvent(initialState: true);

	private static string FinishWeatherUpdateThreadStr = "FinishWeatherUpdateThread";

	public TimeSpan LastSetWeatherGlobalsTime = Target.Never;

	private static TimeSpan TimeBetweenSetGlobals = TimeSpan.FromSeconds(1.0);

	private static float WaterfallMinGradient = 0.25f;

	private static float WaterfallMaxGradient = 0.5f;

	private static float WaterfallDistCheck = 16f;

	public int GetStartDayOfYear()
	{
		return StartDayOfYear;
	}

	public float GetFrontTransition()
	{
		return FrontTransition;
	}

	public float GetTemperatureInCelsius()
	{
		return TemperatureInCelsius;
	}

	public float GetCloudiness()
	{
		return Cloudiness;
	}

	public float GetPrecipitationAmount()
	{
		return PrecipitationAmount;
	}

	public float GetSnowOnGroundAmount()
	{
		return SnowOnGroundAmount;
	}

	public float GetGroundMuddiness()
	{
		return GroundMuddiness;
	}

	public float GetWindAngleDeg()
	{
		return WindAngleDeg;
	}

	public float GetWindStrength()
	{
		return WindStrength;
	}

	public void SetStartDayOfYear(int v)
	{
		StartDayOfYear = v;
	}

	public void SetFrontTransition(float v)
	{
		FrontTransition = v;
	}

	public void SetTemperatureInCelsius(float v)
	{
		float temperatureInCelsius = TemperatureInCelsius;
		TemperatureInCelsiusNotIncludingDiurnalVariation += v - TemperatureInCelsius;
		TemperatureInCelsius = v;
		StartFront(WeatherType.Clear, MathUtil.NonDeterministicRand);
		GameTerrain.Instance.UnityOnTemperatureChanged(temperatureInCelsius, TemperatureInCelsius);
	}

	public void SetInitialTemperatureInCelsius(CustomRandom rand)
	{
		TemperatureInCelsiusNotIncludingDiurnalVariation = CalcAverageTemperatureInCelsius();
		CalcTemperatureIncludingDiurnalVariation();
		if (TemperatureInCelsius < 0f)
		{
			SnowOnGroundAmount = Mathf.Clamp01(rand.RandomFloat() * (1f + (0f - TemperatureInCelsius) * 5f));
		}
		StartFront(WeatherType.Clear, rand);
	}

	public void SetCloudiness(float v)
	{
		Cloudiness = v;
		StartFront(WeatherType.Clear, MathUtil.NonDeterministicRand);
	}

	public void SetPrecipitationAmount(float v)
	{
		PrecipitationAmount = v;
		StartFront(WeatherType.Clear, MathUtil.NonDeterministicRand);
	}

	public void SetSnowOnGroundAmount(float v)
	{
		SnowOnGroundAmount = v;
	}

	public void SetGroundMuddiness(float v)
	{
		GroundMuddiness = v;
	}

	public void SetWindAngleDeg(float v)
	{
		WindAngleDeg = v;
		StartFront(WeatherType.Clear, MathUtil.NonDeterministicRand);
	}

	public void SetWindStrength(float v)
	{
		WindStrength = v;
		StartFront(WeatherType.Clear, MathUtil.NonDeterministicRand);
	}

	public void OnStart()
	{
		GameImpl instance = GameImpl.Instance;
		WeatherSettings = GameObject.Find("Weather").GetComponent<WeatherBehaviour>();
		UnityWindZone = GameObject.Find("Weather").GetComponent<WindZone>();
		WeatherUpdateTaskFunc = WeatherUpdateOnThread;
		RiverSoundMaxDist = instance.UnityRiverSound.maxDistance;
		WindInLeavesSoundMaxDist = instance.UnityWindInLeavesSound.maxDistance;
	}

	public float CalcDaysSinceStartFromTime(TimeSpan time)
	{
		return (float)(time.TotalSeconds / Sun.DayLength.TotalSeconds) + StartHourOfDay / 24f;
	}

	public float CalcDayOfYearFromTime(TimeSpan time)
	{
		return (CalcDaysSinceStartFromTime(time) + (float)StartDayOfYear) % (float)DaysInAYear;
	}

	public static float CalcSeasonFromDayOfYear(float dayOfYear)
	{
		return (dayOfYear + (float)DaysInAMonth) % (float)DaysInAYear / ((float)DaysInAYear / 4f);
	}

	public static float CalcWinterinessFromDayOfYear(float dayOfYear)
	{
		return Mathf.Clamp01(Mathf.Abs(2.5f - CalcSeasonFromDayOfYear(dayOfYear)) - 0.5f);
	}

	public static float CalcAverageTemperatureInCelsiusFromDayOfYear(float dayOfYear)
	{
		return Mathf.Lerp(AverageDaytimeSummerTemperatureInCelsius, AverageDaytimeWinterTemperatureInCelsius, CalcWinterinessFromDayOfYear(dayOfYear)) - DiurnalTemperatureVariationInCelsius * 0.5f;
	}

	public float CalcSeason()
	{
		return CalcSeasonFromDayOfYear(Session.Instance.DayOfYear);
	}

	public float CalcWinteriness()
	{
		return CalcWinterinessFromDayOfYear(Session.Instance.DayOfYear);
	}

	public float CalcAverageTemperatureInCelsius()
	{
		return CalcAverageTemperatureInCelsiusFromDayOfYear(Session.Instance.DayOfYear);
	}

	public float CalcProbabilityOfWarmFront()
	{
		return Mathf.Clamp01((CalcAverageTemperatureInCelsius() + TemperatureRangeInCelsius - TemperatureInCelsius) / (TemperatureRangeInCelsius * 2f));
	}

	public bool IsSafeForPlantingCrops()
	{
		if (TemperatureInCelsius <= 0f)
		{
			return false;
		}
		if (CalcAverageTemperatureInCelsius() <= 0f)
		{
			return false;
		}
		if (CurrentWeatherType == WeatherType.ColdFront && TemperatureInCelsiusWhenFrontStarted + FrontStrength <= 0f)
		{
			return false;
		}
		return true;
	}

	public bool IsSafeForPlantingCropsAI()
	{
		if (!IsSafeForPlantingCrops())
		{
			return false;
		}
		float dayOfYear = Session.Instance.DayOfYear;
		for (int i = 0; i <= 7; i++)
		{
			if (CalcAverageTemperatureInCelsiusFromDayOfYear(dayOfYear + (float)i) - DiurnalTemperatureVariationInCelsius / 2f <= 0f)
			{
				return false;
			}
		}
		return true;
	}

	public static float CalcNutritionNeededToStoreForWinterPerPerson(float dayOfYear, bool rampUpOverPlantingSeason)
	{
		float num = (0f - (AverageDaytimeWinterTemperatureInCelsius - DiurnalTemperatureVariationInCelsius * 0.5f)) / (AverageDaytimeSummerTemperatureInCelsius - AverageDaytimeWinterTemperatureInCelsius) * (float)(DaysInAMonth * 3);
		float num2 = (float)(DaysInAMonth * 11) - num;
		float num3 = (float)(DaysInAMonth * 2) + num;
		float num4 = (float)(3 * DaysInAMonth) + num * 2f;
		float num5 = 0f;
		num5 = ((dayOfYear >= num3 && dayOfYear <= num2) ? (rampUpOverPlantingSeason ? ((dayOfYear - num3) / (num2 - num3)) : 1f) : ((!(dayOfYear >= num2)) ? (1f - (dayOfYear + (float)DaysInAMonth + num) / num4) : (1f - (dayOfYear - num2) / num4)));
		float num6 = num5 * num4 * Sun.DayLengthSecs;
		float num7 = ((!(dayOfYear >= num3) || !(dayOfYear <= num2)) ? 1f : ((dayOfYear - num3) / (num2 - num3)));
		float num8 = num3 + Community.RecommendedPlantedNutritionDays * 2f;
		if (dayOfYear >= num3 && dayOfYear <= num8)
		{
			num7 += (num8 - dayOfYear) / (num8 - num3);
		}
		return num6 + num7 * Community.RecommendedPlantedNutritionDays * Sun.DayLengthSecs * 2f;
	}

	public void StartFront(WeatherType weatherType, CustomRandom rand)
	{
		CurrentWeatherType = weatherType;
		if (CurrentWeatherType == WeatherType.Clear)
		{
			float num = 1f - CalcWinteriness();
			FrontStrength = 0f;
			FrontDurationSecs = Mathf.Lerp(0f, 0.5f + num * num * 2f, (float)rand.NextDouble()) * Sun.DayLengthSecs;
		}
		else
		{
			float num2 = CalcProbabilityOfWarmFront();
			FrontStrength = (float)rand.NextDouble() * ((CurrentWeatherType == WeatherType.WarmFront) ? num2 : (1f - num2)) * (TemperatureRangeInCelsius * 2f);
			float a = ((CurrentWeatherType == WeatherType.WarmFront) ? 0.5f : 0.25f);
			float b = ((CurrentWeatherType == WeatherType.WarmFront) ? 1f : 0.5f);
			FrontDurationSecs = Mathf.Lerp(a, b, (float)rand.NextDouble()) * Sun.DayLengthSecs;
		}
		FrontTransition = 0f;
		TemperatureInCelsiusWhenFrontStarted = TemperatureInCelsiusNotIncludingDiurnalVariation;
		CloudinessWhenFrontStarted = Cloudiness;
		PrecipitationWhenFrontStarted = PrecipitationAmount;
		WindAngleDegWhenFrontStarted = WindAngleDeg;
		WindStrengthWhenFrontStarted = WindStrength;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref StartDayOfYear);
		reflector.Add(ref StartHourOfDay);
		reflector.Add(ref CurrentWeatherType);
		reflector.Add(ref FrontDurationSecs);
		reflector.Add(ref FrontStrength);
		reflector.Add(ref FrontTransition);
		reflector.Add(ref TemperatureInCelsius);
		reflector.AddAfter(ref TemperatureInCelsiusNotIncludingDiurnalVariation, 48);
		reflector.Add(ref TemperatureInCelsiusWhenFrontStarted);
		reflector.Add(ref Cloudiness);
		reflector.Add(ref CloudinessWhenFrontStarted);
		reflector.Add(ref PrecipitationAmount);
		reflector.Add(ref PrecipitationWhenFrontStarted);
		reflector.Add(ref SnowOnGroundAmount);
		reflector.AddAfter(ref GroundMuddiness, 395);
		reflector.Add(ref WindAngleDeg);
		reflector.Add(ref WindStrength);
	}

	private void CalcTemperatureIncludingDiurnalVariation()
	{
		TemperatureInCelsius = TemperatureInCelsiusNotIncludingDiurnalVariation - (1f - Sun.GetSunIntensity(Session.Instance.DaysSinceStart)) * DiurnalTemperatureVariationInCelsius;
	}

	public void DeterministicUpdate(TimeSpan dt)
	{
		Session instance = Session.Instance;
		FrontTransition += (float)dt.TotalSeconds / FrontDurationSecs;
		switch (CurrentWeatherType)
		{
		case WeatherType.Clear:
			TemperatureInCelsiusNotIncludingDiurnalVariation = TemperatureInCelsiusWhenFrontStarted;
			break;
		case WeatherType.WarmFront:
			TemperatureInCelsiusNotIncludingDiurnalVariation = TemperatureInCelsiusWhenFrontStarted + WeatherSettings.WarmFrontTemperature.Evaluate(FrontTransition) * FrontStrength;
			Cloudiness = Mathf.Lerp(CloudinessWhenFrontStarted, WeatherSettings.WarmFrontCloudiness.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			PrecipitationAmount = Mathf.Lerp(PrecipitationWhenFrontStarted, WeatherSettings.WarmFrontPrecipitation.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			WindAngleDeg = MathUtil.AngleLerp(WindAngleDegWhenFrontStarted * (MathF.PI / 180f), WeatherSettings.WarmFrontWindDirection.Evaluate(FrontTransition) * (MathF.PI / 180f), Mathf.Clamp01(FrontTransition / 0.125f)) * 57.29578f;
			WindStrength = Mathf.Lerp(WindStrengthWhenFrontStarted, WeatherSettings.WarmFrontWindStrength.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			break;
		case WeatherType.ColdFront:
			TemperatureInCelsiusNotIncludingDiurnalVariation = TemperatureInCelsiusWhenFrontStarted + WeatherSettings.ColdFrontTemperature.Evaluate(FrontTransition) * FrontStrength;
			Cloudiness = Mathf.Lerp(CloudinessWhenFrontStarted, WeatherSettings.ColdFrontCloudiness.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			PrecipitationAmount = Mathf.Lerp(PrecipitationWhenFrontStarted, WeatherSettings.ColdFrontPrecipitation.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			WindAngleDeg = MathUtil.AngleLerp(WindAngleDegWhenFrontStarted * (MathF.PI / 180f), WeatherSettings.ColdFrontWindDirection.Evaluate(FrontTransition) * (MathF.PI / 180f), Mathf.Clamp01(FrontTransition / 0.125f)) * 57.29578f;
			WindStrength = Mathf.Lerp(WindStrengthWhenFrontStarted, WeatherSettings.ColdFrontWindStrength.Evaluate(FrontTransition), Mathf.Clamp01(FrontTransition / 0.125f));
			break;
		}
		float temperatureInCelsius = TemperatureInCelsius;
		CalcTemperatureIncludingDiurnalVariation();
		GameTerrain.Instance.UnityOnTemperatureChanged(temperatureInCelsius, TemperatureInCelsius);
		if (FrontTransition >= 1f)
		{
			if (CurrentWeatherType != WeatherType.Clear)
			{
				StartFront(WeatherType.Clear, instance.DeterministicRand);
			}
			else
			{
				float probability = CalcProbabilityOfWarmFront();
				StartFront(instance.DeterministicRand.RandomChoice(probability) ? WeatherType.WarmFront : WeatherType.ColdFront, instance.DeterministicRand);
			}
		}
		if (TemperatureInCelsius <= SnowSettleTempCelcius)
		{
			SnowOnGroundAmount = Mathf.Min(1f, SnowOnGroundAmount + PrecipitationAmount * SnowOnGroundBuildupRate * (float)dt.TotalSeconds);
		}
		if (TemperatureInCelsius >= 0f)
		{
			float snowOnGroundAmount = SnowOnGroundAmount;
			SnowOnGroundAmount = Mathf.Max(0f, SnowOnGroundAmount - TemperatureInCelsius * SnowOnGroundMeltRate * (float)dt.TotalSeconds);
			float num = (snowOnGroundAmount - SnowOnGroundAmount) * MudFromSnowRatio;
			GroundMuddiness = Mathf.Min(1f, GroundMuddiness + PrecipitationAmount * MudOnGroundBuildupRate * (float)dt.TotalSeconds + num);
		}
		GroundMuddiness = Mathf.Max(0f, GroundMuddiness - Math.Max(0f, TemperatureInCelsius) * MudOnGroundDryRate * (float)dt.TotalSeconds);
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		if (localControlledCharacter == null)
		{
			return;
		}
		float num2 = Mathf.Min(localControlledCharacter.MovementSpeed / Character.JogSpeed, 1f);
		if (LastPlayerId != localControlledCharacter.Id)
		{
			if (PlayerPosSpeedTerrainSize.z > 0.01f)
			{
				num2 = 0f;
			}
			else
			{
				LastPlayerId = localControlledCharacter.Id;
			}
		}
		PlayerPosSpeedTerrainSize.x = localControlledCharacter.Pos.x;
		PlayerPosSpeedTerrainSize.y = localControlledCharacter.Pos.z;
		PlayerPosSpeedTerrainSize.z += (num2 - PlayerPosSpeedTerrainSize.z) * 0.1f;
		PlayerPosSpeedTerrainSize.w = GameTerrain.Instance.HalfSize;
	}

	public void StartWeatherUpdateTask()
	{
		WeatherUpdateFinishedEvent.Reset();
		GameImpl.Instance.UpdateThreadPool.AddTask(WeatherUpdateTaskFunc, null, null, TaskPriority.QuiteHigh);
	}

	public void WeatherUpdateOnThread(BaseTaskData data)
	{
		try
		{
			Hud instance = Hud.Instance;
			GameTerrain instance2 = GameTerrain.Instance;
			Character localControlledCharacter = instance.LocalControlledCharacter;
			TerrainPath nearestPath = null;
			float pathIndex = 0f;
			RiverPos = ((localControlledCharacter != null) ? GameTerrain.Instance.GetNearestPointOnPath(localControlledCharacter.PosXZ, river: true, RiverSoundMaxDist, out nearestPath, out var _, out pathIndex) : ((Vector3?)null));
			if (RiverPos.HasValue)
			{
				Waterfallness = Math.Max(GetWaterfallness(localControlledCharacter.PosXZ, nearestPath, pathIndex, -1), GetWaterfallness(localControlledCharacter.PosXZ, nearestPath, pathIndex, 1));
			}
			else
			{
				Waterfallness = 0f;
			}
			NearestTree = ((instance.LocalControlledCharacter != null) ? instance2.TreeMapWho.GetNearestObjectMoreAccurate(instance.LocalControlledCharacter.PosXZ, WindInLeavesSoundMaxDist) : null);
		}
		catch (Exception ex)
		{
			Debug.Log("Error in WeatherUpdateOnThread: " + ex.Message + ex.StackTrace);
		}
		WeatherUpdateFinishedEvent.Set();
	}

	public void FinishWeatherUpdateThread()
	{
		using (new UnityProfileMarker(FinishWeatherUpdateThreadStr))
		{
			WeatherUpdateFinishedEvent.WaitOne();
		}
	}

	public void NonDeterministicUpdate(float dts)
	{
		GameImpl instance = GameImpl.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		Session instance3 = Session.Instance;
		_ = Hud.Instance;
		float sunIntensity = Sun.GetSunIntensity(instance3.DaysSinceStart);
		float num = CalcWinteriness();
		float snowToSleetTransition = Mathf.Clamp01((TemperatureInCelsius - 0f) / 2f);
		float num2 = Mathf.Clamp01((TemperatureInCelsius - 2f) / 3f);
		Vector2 dirFromAngle = MathUtil.GetDirFromAngle(WindAngleDeg * (MathF.PI / 180f));
		WindTime += dts * WindStrength;
		CloudPosXZ -= dts * WindStrength * dirFromAngle;
		instance2.UnityTerrain.materialTemplate.SetFloat(ShaderHash._SnowAmount, SnowOnGroundAmount);
		Shader.SetGlobalFloat(ShaderHash._SnowAmount, SnowOnGroundAmount);
		if (LastSetWeatherGlobalsTime < instance3.PlayTime - TimeBetweenSetGlobals)
		{
			Shader.SetGlobalFloat(ShaderHash._Season, CalcSeason());
			Shader.SetGlobalFloat(ShaderHash._GrassDryness, num);
			instance.Sun.SetupSkyShaderGlobals();
			LastSetWeatherGlobalsTime = instance3.PlayTime;
		}
		Shader.SetGlobalVector(ShaderHash._WindDirStrength, new Vector4(dirFromAngle.x, 0f, dirFromAngle.y, WindStrength));
		UnityWindZone.transform.eulerAngles = new Vector3(0f, WindAngleDeg, 0f);
		UnityWindZone.windMain = WindStrength;
		instance2.UnityTerrain.terrainData.wavingGrassStrength = WindStrength;
		Shader.SetGlobalVector(ShaderHash._CloudPosXZDensityTime, new Vector4(CloudPosXZ.x, CloudPosXZ.y, Cloudiness, WindTime));
		Shader.SetGlobalVector(ShaderHash._PlayerPosSpeedTerrainSize, PlayerPosSpeedTerrainSize);
		SetAmbientSoundVolume(instance.UnityRainSound, num2 * PrecipitationAmount);
		SetAmbientSoundVolume(instance.UnityCricketsSound, 0.75f * Mathf.Clamp01(1f - sunIntensity) * (1f - num) * Mathf.Clamp01(TemperatureInCelsius / 2f) * Mathf.Clamp01(1f - PrecipitationAmount * 2f));
		float num3 = Mathf.Clamp01((WindStrength - DefaultWindStrength) / (1f - DefaultWindStrength));
		SetAmbientSoundVolume(instance.UnityWindInGrassSound, num3 * num3 * WindInGrassVolume);
		FinishWeatherUpdateThread();
		if (RiverPos.HasValue)
		{
			float num4 = Mathf.Clamp01(Session.Instance.Weather.TemperatureInCelsius / WaterCompletelyFrozenTemperatureInCelsius);
			SetAmbientSoundVolume(instance.UnityRiverSound, Mathf.Sqrt(1f - Waterfallness) * (1f - num4));
			SetAmbientSoundVolume(instance.UnityWaterfallSound, Mathf.Sqrt(Waterfallness) * (1f - num4));
			instance.UnityRiverSound.transform.position = RiverPos.Value;
			instance.UnityWaterfallSound.transform.position = RiverPos.Value;
		}
		else
		{
			SetAmbientSoundVolume(instance.UnityRiverSound, 0f);
			SetAmbientSoundVolume(instance.UnityWaterfallSound, 0f);
		}
		if (NearestTree != null)
		{
			instance.UnityWindInLeavesSound.transform.position = NearestTree.Pos + NearestTree.Height * 0.5f * Vector3.up;
			NearestTree = null;
		}
		SetAmbientSoundVolume(instance.UnityWindInLeavesSound, WindStrength * WindStrength * WindInLeavesVolume);
		if (!GameImpl.Instance.IsMenuOpen())
		{
			WeatherParticleSystem.Instance.Update(PrecipitationAmount, dirFromAngle, WindStrength, snowToSleetTransition, num2);
		}
	}

	private float GetWaterfallness(Vector2 myPosXZ, TerrainPath path, float start, int dir)
	{
		int num = Mathf.FloorToInt(start);
		float num2 = 0f;
		float num3 = 0f;
		bool flag = true;
		while (num2 < WaterfallDistCheck && num >= 0 && num < path.Points.Count - 1)
		{
			Vector3 pos = path.Points[num].Pos;
			Vector3 pos2 = path.Points[num + 1].Pos;
			Vector3 vector = ((dir > 0) ? pos2 : pos);
			float magnitude = MathUtil.ToXZ(pos - pos2).magnitude;
			float num4 = (pos.y - pos2.y) / magnitude;
			float val = Mathf.Clamp01(1f - num2 / WaterfallDistCheck) * Mathf.Clamp01((num4 - WaterfallMinGradient) / (WaterfallMaxGradient - WaterfallMinGradient));
			num3 = Math.Max(num3, val);
			if (flag)
			{
				Vector3 vector2 = Vector3.Lerp(pos, pos2, start - (float)num);
				num2 += (vector - vector2).magnitude;
			}
			else
			{
				num2 += magnitude;
			}
			num += dir;
			flag = false;
		}
		return num3;
	}

	public static void SetAmbientSoundVolume(AudioSource unityAmbientSound, float volume)
	{
		float num = volume * SoundManager.BackgroundSoundVolume;
		if (num != unityAmbientSound.volume)
		{
			unityAmbientSound.volume = num;
			if (unityAmbientSound.volume > 0f && !unityAmbientSound.isPlaying && !GameImpl.Instance.IsMenuOpen())
			{
				unityAmbientSound.Play();
			}
			else if (unityAmbientSound.volume == 0f && unityAmbientSound.isPlaying)
			{
				unityAmbientSound.Stop();
			}
		}
	}
}

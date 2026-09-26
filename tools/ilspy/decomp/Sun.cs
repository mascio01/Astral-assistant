using System;
using BeautifyEffect;
using UnityEngine;
using UnityEngine.Rendering;

public class Sun
{
	public Light UnitySunLight;

	public Light UnityMoonLight;

	public Light UnityFillLight;

	public Light[] UnityBackLight = new Light[2];

	public SunSettings SunSettings;

	public ReflectionProbe UnityReflectionProbe;

	public ReflectionProbe UnityMiddayReflectionProbe;

	public int ReflectionRenderID = -1;

	public int MiddayReflectionRenderID = -1;

	public float EastWestAngle = -45f;

	public float NorthSouthAngle = 45f;

	public float MoonEastWestAngle = -30f;

	public float MoonNorthSouthAngle = 45f;

	public static TimeSpan DayLength = TimeSpan.FromMinutes(30.0);

	public static float DayLengthSecs = (float)DayLength.TotalSeconds;

	public static float HorizonFadeAngle = 1f;

	public static float IntensityFactor = 1f;

	public static Vector3 IconGenerationPos = new Vector3(10000f, 0f, 0f);

	public GlobalLightingSettingsForSunAngle[] LightCycle;

	public GlobalLightingSettings LightingSettings;

	private static string SetupLightForTimeStr = "SetupLightForTime";

	public void Init()
	{
		int cullingMask = (1 << Character.CharactersLayer) | (1 << Character.IconGimpLayer) | (1 << Character.PreviewGimpLayer) | (1 << Character.PipLayer);
		GameObject gameObject = GameObject.Find("Sun Light");
		gameObject.layer = Character.SunLayer;
		UnitySunLight = gameObject.GetComponent<Light>();
		UnitySunLight.type = LightType.Directional;
		UnitySunLight.shadows = LightShadows.Soft;
		UnitySunLight.shadowBias = 0.01f;
		if (UseBeautify())
		{
			UnitySunLight.flare = null;
		}
		gameObject = GameObject.Find("Sun Settings");
		SunSettings = gameObject.GetComponent<SunSettings>();
		LightingSettings = new GlobalLightingSettings(SunSettings.DayLight);
		LightCycle = new GlobalLightingSettingsForSunAngle[10]
		{
			new GlobalLightingSettingsForSunAngle(float.MinValue, SunSettings.MoonLight),
			new GlobalLightingSettingsForSunAngle(-95f, SunSettings.MoonLight),
			new GlobalLightingSettingsForSunAngle(-90f, SunSettings.SwitchOverLight),
			new GlobalLightingSettingsForSunAngle(-85f, SunSettings.DuskDawnLight),
			new GlobalLightingSettingsForSunAngle(-70f, SunSettings.DayLight),
			new GlobalLightingSettingsForSunAngle(70f, SunSettings.DayLight),
			new GlobalLightingSettingsForSunAngle(85f, SunSettings.DuskDawnLight),
			new GlobalLightingSettingsForSunAngle(90f, SunSettings.SwitchOverLight),
			new GlobalLightingSettingsForSunAngle(95f, SunSettings.MoonLight),
			new GlobalLightingSettingsForSunAngle(float.MaxValue, SunSettings.MoonLight)
		};
		gameObject = new GameObject();
		gameObject.name = "Moon Light";
		gameObject.layer = Character.SunLayer;
		UnityMoonLight = gameObject.AddComponent<Light>();
		UnityMoonLight.type = LightType.Directional;
		UnityMoonLight.shadows = LightShadows.Soft;
		UnityMoonLight.shadowBias = 0.01f;
		UnityMoonLight.cullingMask = UnitySunLight.cullingMask;
		gameObject = new GameObject();
		gameObject.name = "Fill Light";
		UnityFillLight = gameObject.AddComponent<Light>();
		UnityFillLight.type = LightType.Directional;
		UnityFillLight.shadows = LightShadows.None;
		UnityFillLight.cullingMask = cullingMask;
		for (int i = 0; i < 2; i++)
		{
			gameObject = new GameObject();
			gameObject.name = "Back Light" + i;
			UnityBackLight[i] = gameObject.AddComponent<Light>();
			UnityBackLight[i].type = LightType.Directional;
			UnityBackLight[i].shadows = LightShadows.None;
			UnityBackLight[i].cullingMask = cullingMask;
			UnityBackLight[i].enabled = false;
			UnityBackLight[i].intensity = 0.5f * IntensityFactor;
		}
		UnityReflectionProbe = GameObject.Find("ReflectionProbe").GetComponent<ReflectionProbe>();
		UnityMiddayReflectionProbe = GameObject.Find("MiddayReflectionProbe").GetComponent<ReflectionProbe>();
		UnityMiddayReflectionProbe.transform.position = IconGenerationPos;
		SetupLightForTime(0.5f, useMiddayReflectionTex: false);
		UnityMiddayReflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
		UnityMiddayReflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
		MiddayReflectionRenderID = UnityMiddayReflectionProbe.RenderProbe();
	}

	public static bool UseBeautify()
	{
		return false;
	}

	public void StartRenderingMainReflectionProbe()
	{
		UnityReflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
		UnityReflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
		ReflectionRenderID = UnityReflectionProbe.RenderProbe();
	}

	public void Update()
	{
		if (MiddayReflectionRenderID != -1 && UnityMiddayReflectionProbe.IsFinishedRendering(MiddayReflectionRenderID))
		{
			MiddayReflectionRenderID = -1;
			UnityMiddayReflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
		}
		if (ReflectionRenderID != -1 && UnityReflectionProbe.IsFinishedRendering(ReflectionRenderID))
		{
			ReflectionRenderID = -1;
			UnityReflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
			UnityReflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
		}
	}

	public void Unload()
	{
		UnityEngine.Object.Destroy(UnityFillLight.gameObject);
		for (int i = 0; i < 2; i++)
		{
			UnityEngine.Object.Destroy(UnityBackLight[i].gameObject);
		}
	}

	private static Quaternion CalcSunLightAngle(float eastWestAngle, float northSouthAngle)
	{
		return Quaternion.AngleAxis(northSouthAngle, new Vector3(1f, 0f, 0f)) * Quaternion.AngleAxis(90f, new Vector3(0f, 1f, 0f)) * Quaternion.AngleAxis(eastWestAngle + 90f, new Vector3(1f, 0f, 0f));
	}

	public static float GetSunIntensity(float daysSinceStart)
	{
		float value = 57.29578f * MathUtil.WrapAngle(MathF.PI * 2f * (daysSinceStart - 0.5f));
		return Mathf.Clamp01((90f - Math.Abs(value)) / HorizonFadeAngle);
	}

	public void SetupLightForTime(float daysSinceStart, bool useMiddayReflectionTex)
	{
		using (new UnityProfileMarker(SetupLightForTimeStr))
		{
			EastWestAngle = 57.29578f * MathUtil.WrapAngle(MathF.PI * 2f * (daysSinceStart - 0.5f));
			float num = Mathf.Clamp01((90f - Math.Abs(EastWestAngle)) / HorizonFadeAngle);
			float num2 = 1f - num;
			for (int i = 0; i < LightCycle.Length - 1; i++)
			{
				float sunAngle = LightCycle[i].SunAngle;
				float sunAngle2 = LightCycle[i + 1].SunAngle;
				if (EastWestAngle >= sunAngle && EastWestAngle < sunAngle2)
				{
					GlobalLightingSettings lightingSettings = LightCycle[i].LightingSettings;
					GlobalLightingSettings lightingSettings2 = LightCycle[i + 1].LightingSettings;
					float t = (EastWestAngle - sunAngle) / (sunAngle2 - sunAngle);
					LightingSettings.Ambient = Color.Lerp(lightingSettings.Ambient, lightingSettings2.Ambient, t);
					LightingSettings.Diffuse = Color.Lerp(lightingSettings.Diffuse, lightingSettings2.Diffuse, t);
					LightingSettings.Fill = Color.Lerp(lightingSettings.Fill, lightingSettings2.Fill, t);
					LightingSettings.Back = Color.Lerp(lightingSettings.Back, lightingSettings2.Back, t);
					LightingSettings.FogTop = Color.Lerp(lightingSettings.FogTop, lightingSettings2.FogTop, t);
					LightingSettings.FogBottom = Color.Lerp(lightingSettings.FogBottom, lightingSettings2.FogBottom, t);
					break;
				}
			}
			UnitySunLight.enabled = SunFiddler.SunLightEnabled && num > 0f;
			UnitySunLight.color = (SunFiddler.FullBright ? Color.white : LightingSettings.Diffuse);
			UnitySunLight.intensity = num * (SunFiddler.FullBright ? 1f : LightingSettings.Diffuse.a) * IntensityFactor;
			UnitySunLight.shadowStrength = num;
			UnitySunLight.transform.localRotation = CalcSunLightAngle(EastWestAngle, NorthSouthAngle);
			UnityMoonLight.enabled = SunFiddler.MoonLightEnabled && num2 > 0f;
			UnityMoonLight.color = (SunFiddler.FullBright ? Color.white : SunSettings.MoonLight.Diffuse);
			UnityMoonLight.intensity = num2 * (SunFiddler.FullBright ? 1f : SunSettings.MoonLight.Diffuse.a) * IntensityFactor;
			UnityMoonLight.shadowStrength = num2;
			UnityMoonLight.transform.localRotation = CalcSunLightAngle(MoonEastWestAngle, MoonNorthSouthAngle);
			UnityFillLight.enabled = SunFiddler.FillLightEnabled;
			UnityFillLight.color = (SunFiddler.FullBright ? Color.white : LightingSettings.Fill);
			UnityFillLight.intensity = (SunFiddler.FullBright ? 1f : LightingSettings.Fill.a) * IntensityFactor;
			UnityFillLight.transform.localEulerAngles = new Vector3(0f, UnitySunLight.transform.localEulerAngles.y + 90f, 0f);
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = (SunFiddler.AmbientLightEnabled ? LightingSettings.Ambient : Color.black);
			RenderSettings.ambientIntensity = (SunFiddler.AmbientLightEnabled ? LightingSettings.Ambient.a : 1f) * IntensityFactor;
			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
			RenderSettings.customReflectionTexture = (useMiddayReflectionTex ? UnityMiddayReflectionProbe.texture : UnityReflectionProbe.texture);
			if (HudBehaviour.Instance != null && UseBeautify())
			{
				Beautify unityBeautifyBehaviour = HudBehaviour.Instance.UnityBeautifyBehaviour;
				Color color = (unityBeautifyBehaviour.sunFlaresTint = UnitySunLight.color);
				Color color3 = (unityBeautifyBehaviour.anamorphicFlaresTint = color);
				Color tintColor = (unityBeautifyBehaviour.bloomTint = color3);
				unityBeautifyBehaviour.tintColor = tintColor;
			}
		}
	}

	public void SetupSkyShaderGlobals()
	{
		Shader.SetGlobalColor(ShaderHash._FogColTop, LightingSettings.FogTop);
		Shader.SetGlobalColor(ShaderHash._FogColBottom, LightingSettings.FogBottom);
		Shader.SetGlobalVector(ShaderHash._SunColor, LightingSettings.Diffuse);
		Shader.SetGlobalVector(ShaderHash._MoonColor, SunSettings.MoonLight.Diffuse);
	}
}

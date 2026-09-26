using System.Collections.Generic;
using UnityEngine;

public class PortraitGallery
{
	public RenderTexture RenderTex = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);

	private List<Portrait> Portraits = new List<Portrait>();

	public static PortraitGallery Instance;

	private static Vector3 TreelineCamPos = new Vector3(0f, 8f, 20f);

	private static Vector3 TreelineHighCamPos = new Vector3(0f, 3f, 6f);

	private static float TreelineHalfLength = 14f;

	private static float TreelineWidth = 2f;

	private static Vector3 HeliPos = new Vector3(0f, 8f, -8f);

	private static Vector3 HeliRot = new Vector3(0f, 60f, 30f);

	private static Vector3 HeliCamPos = new Vector3(0f, 0f, 4f);

	private static Vector3 VehiclePos = new Vector3(0f, 0f, -4f);

	private static Vector3 VehicleFocusPos = new Vector3(0f, 2f, -4f);

	private static Vector3 VehicleRot = new Vector3(0f, -45f, 0f);

	private static Vector3 VehicleCamPos = new Vector3(0f, 0f, 1f);

	public void Init()
	{
		Instance = this;
		for (int i = 0; i < 6; i++)
		{
			Portraits.Add(new Portrait(1920, 1080));
		}
	}

	public void Unload()
	{
		Instance = null;
	}

	public Texture2D GetPortrait(TileObject sub, PortraitPose pose)
	{
		Portrait portrait = null;
		foreach (Portrait portrait2 in Portraits)
		{
			if (portrait2.Tex.width == RenderTex.width && portrait2.Tex.height == RenderTex.height)
			{
				if (portrait2.PortraitSubject == sub && portrait2.Pose == pose)
				{
					return portrait2.Ready ? portrait2.Tex : null;
				}
				if (portrait2.Ready && portrait2.LastUsedFrame < Time.frameCount - 1 && (portrait == null || portrait2.LastUsedFrame < portrait.LastUsedFrame))
				{
					portrait = portrait2;
				}
			}
		}
		if (portrait == null)
		{
			portrait = new Portrait(RenderTex.width, RenderTex.height);
			Portraits.Add(portrait);
		}
		portrait.PortraitSubject = sub;
		portrait.Pose = pose;
		portrait.Ready = false;
		portrait.LastUsedFrame = Time.frameCount;
		bool flag = PortraitGeneratorMenu.Instance.IsGeneratingAchievementIcons();
		CustomRandom customRandom = (flag ? PortraitGeneratorMenu.Instance.Rand : MathUtil.NonDeterministicRand);
		if (portrait.Pose == PortraitPose.Treeline || portrait.Pose == PortraitPose.TreelineHigh)
		{
			GameImpl instance = GameImpl.Instance;
			Session instance2 = Session.Instance;
			Camera component = instance.UnityPortraitCameraObj.GetComponent<Camera>();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < 64; i++)
			{
				float t = ((float)i + Mathf.Lerp(-0.25f, 0.25f, customRandom.RandomFloat())) / 63f;
				GameObject gameObject = Object.Instantiate((GameObject)GameTerrain.Trees[5 + customRandom.Next() % 3]);
				gameObject.transform.position = Sun.IconGenerationPos + new Vector3(Mathf.Lerp(0f - TreelineHalfLength, TreelineHalfLength, t), 0f, customRandom.RandomFloat() * TreelineWidth);
				gameObject.transform.rotation = Quaternion.Euler(0f, customRandom.RandomFloat() * 360f, 0f);
				gameObject.transform.localScale = Vector3.one * Mathf.Lerp(1.5f, 2.5f, customRandom.RandomFloat());
				Prop.ReplaceLayerRecursively(gameObject, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
				list.Add(gameObject);
			}
			instance.Sun.UnitySunLight.enabled = false;
			instance.Sun.UnityMoonLight.enabled = false;
			instance.Sun.UnityFillLight.enabled = false;
			instance.Sun.UnityBackLight[0].enabled = false;
			instance.Sun.UnityBackLight[1].enabled = false;
			instance.Sun.UnitySunLight.color = Color.black;
			instance.Sun.UnityMoonLight.color = Color.black;
			instance.Sun.UnityFillLight.color = Color.black;
			instance.Sun.UnityBackLight[0].color = Color.black;
			instance.Sun.UnityBackLight[1].color = Color.black;
			RenderSettings.ambientLight = Color.black;
			GameTerrain.UnityInitSnowShaderParams();
			Shader.SetGlobalFloat(ShaderHash._Season, 2f);
			Shader.SetGlobalVector(ShaderHash._CloudPosXZDensityTime, Vector4.zero);
			Shader.SetGlobalVector(ShaderHash._WindDirStrength, Vector4.zero);
			component.targetTexture = RenderTex;
			component.gameObject.SetActive(value: true);
			RenderTexture.active = RenderTex;
			component.transform.position = Sun.IconGenerationPos + ((portrait.Pose == PortraitPose.TreelineHigh) ? TreelineHighCamPos : TreelineCamPos);
			component.transform.LookAt(new Vector3(component.transform.position.x, component.transform.position.y, 0f), Vector3.up);
			component.Render();
			portrait.Tex.ReadPixels(new Rect(0f, 0f, RenderTex.width, RenderTex.height), 0, 0);
			portrait.Tex.Apply();
			if (instance2 != null)
			{
				instance.Sun.SetupLightForTime(instance2.DaysSinceStart, useMiddayReflectionTex: true);
			}
			foreach (GameObject item in list)
			{
				Object.DestroyImmediate(item);
			}
			portrait.Ready = true;
		}
		else if (portrait.Pose == PortraitPose.Helicopter || portrait.Pose == PortraitPose.BigHelicopter)
		{
			GameImpl instance3 = GameImpl.Instance;
			Session instance4 = Session.Instance;
			Camera component2 = instance3.UnityPortraitCameraObj.GetComponent<Camera>();
			PropPrototype propPrototype = instance3.FindPropPrototypeByName((portrait.Pose == PortraitPose.BigHelicopter) ? "Helicopter4" : "Helicopter3");
			if (propPrototype != null && propPrototype.Prefabs.Count > 0)
			{
				GameObject gameObject2 = Object.Instantiate((GameObject)propPrototype.Prefabs[customRandom.Next(propPrototype.Prefabs.Count)]);
				if (gameObject2 != null)
				{
					gameObject2.transform.position = Sun.IconGenerationPos + HeliPos;
					gameObject2.transform.rotation = Quaternion.Euler(HeliRot);
					Prop.ReplaceLayerRecursively(gameObject2, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
					GameTerrain.UnityInitSnowShaderParams();
					Shader.SetGlobalFloat(ShaderHash._Season, 2f);
					Shader.SetGlobalVector(ShaderHash._CloudPosXZDensityTime, Vector4.zero);
					Shader.SetGlobalVector(ShaderHash._WindDirStrength, Vector4.zero);
					instance3.Sun.SetupLightForTime(0.72f, useMiddayReflectionTex: true);
					component2.targetTexture = RenderTex;
					component2.gameObject.SetActive(value: true);
					RenderTexture.active = RenderTex;
					component2.transform.position = Sun.IconGenerationPos + HeliCamPos;
					component2.transform.LookAt(Sun.IconGenerationPos + HeliPos, Vector3.up);
					component2.Render();
					portrait.Tex.ReadPixels(new Rect(0f, 0f, RenderTex.width, RenderTex.height), 0, 0);
					portrait.Tex.Apply();
					if (instance4 != null)
					{
						instance3.Sun.SetupLightForTime(instance4.DaysSinceStart, useMiddayReflectionTex: false);
					}
					Object.DestroyImmediate(gameObject2);
				}
			}
			portrait.Ready = true;
		}
		else if (portrait.Pose == PortraitPose.MilitaryVehicle || portrait.Pose == PortraitPose.Sedan || portrait.Pose == PortraitPose.Taxi || portrait.Pose == PortraitPose.Ambulance || portrait.Pose == PortraitPose.Pickup || portrait.Pose == PortraitPose.Van || portrait.Pose == PortraitPose.DumpTruck || portrait.Pose == PortraitPose.Watchtower)
		{
			GameImpl instance5 = GameImpl.Instance;
			Session instance6 = Session.Instance;
			Camera component3 = instance5.UnityPortraitCameraObj.GetComponent<Camera>();
			GameObject gameObject3 = null;
			if (portrait.PortraitSubject != null && portrait.PortraitSubject.GetUnityModel() != null)
			{
				gameObject3 = Object.Instantiate((GameObject)portrait.PortraitSubject.GetUnityModel());
			}
			if (gameObject3 == null)
			{
				string name = string.Empty;
				switch (portrait.Pose)
				{
				case PortraitPose.MilitaryVehicle:
					name = "HumVee";
					break;
				case PortraitPose.Sedan:
					name = "Sedan";
					break;
				case PortraitPose.Taxi:
					name = "Taxi";
					break;
				case PortraitPose.Ambulance:
					name = "Ambulance";
					break;
				case PortraitPose.Pickup:
					name = "Pickup2";
					break;
				case PortraitPose.Van:
					name = "Van2";
					break;
				case PortraitPose.DumpTruck:
					name = "DumpTruck";
					break;
				case PortraitPose.Watchtower:
					name = "WatchTower";
					break;
				}
				PropPrototype propPrototype2 = instance5.FindPropPrototypeByName(name);
				if (propPrototype2 != null && propPrototype2.Prefabs.Count > 0)
				{
					gameObject3 = Object.Instantiate((GameObject)propPrototype2.Prefabs[customRandom.Next(propPrototype2.Prefabs.Count)]);
					if (propPrototype2.ColorVariations != null && propPrototype2.ColorVariations.Length != 0)
					{
						int num = customRandom.Next(propPrototype2.ColorVariations.Length);
						switch (portrait.Pose)
						{
						case PortraitPose.Taxi:
							num = 0;
							break;
						case PortraitPose.Van:
							num = 8;
							break;
						case PortraitPose.Pickup:
							num = 1;
							break;
						}
						Prop.UnityApplyColorVariation(gameObject3, propPrototype2.ColorVariationMaterialName, propPrototype2.ColorVariations[num]);
					}
				}
			}
			if (gameObject3 != null)
			{
				Vector3 euler = VehicleRot;
				Vector3 position = Sun.IconGenerationPos + VehiclePos;
				if (flag && portrait.Pose != PortraitPose.Watchtower && portrait.Pose != PortraitPose.DumpTruck)
				{
					int num2 = customRandom.Next(4);
					switch (portrait.Pose)
					{
					case PortraitPose.MilitaryVehicle:
						num2 = 2;
						break;
					case PortraitPose.Taxi:
						num2 = 2;
						break;
					case PortraitPose.Ambulance:
						num2 = 0;
						break;
					case PortraitPose.Pickup:
						num2 = 3;
						break;
					case PortraitPose.Van:
						num2 = 3;
						break;
					}
					switch (num2)
					{
					case 0:
						euler = new Vector3(0f, 145f, 20f);
						position.x += 0.1f;
						break;
					case 1:
						euler = new Vector3(0f, -145f, -20f);
						position.x -= 0.1f;
						break;
					case 2:
						euler = new Vector3(0f, 35f, 20f);
						position.x -= 0.1f;
						break;
					case 3:
						euler = new Vector3(0f, -35f, -20f);
						position.x += 0.1f;
						break;
					}
					position.y += 1f;
				}
				if (portrait.Pose == PortraitPose.DumpTruck)
				{
					euler = new Vector3(0f, -145f, -10f);
					position = Sun.IconGenerationPos + new Vector3(-0.5f, 1f, -14f);
				}
				gameObject3.transform.position = position;
				gameObject3.transform.rotation = Quaternion.Euler(euler);
				Prop.ReplaceLayerRecursively(gameObject3, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
				Prop.UnitySetSnowFade(gameObject3, 0f);
				GameTerrain.UnityInitSnowShaderParams();
				Shader.SetGlobalFloat(ShaderHash._Season, 2f);
				Shader.SetGlobalVector(ShaderHash._CloudPosXZDensityTime, Vector4.zero);
				Shader.SetGlobalVector(ShaderHash._WindDirStrength, Vector4.zero);
				instance5.Sun.SetupLightForTime((Mathf.Abs(euler.y) > 90f) ? 0.5f : ((euler.y < 0f) ? 0.72f : 0.28f), useMiddayReflectionTex: true);
				component3.targetTexture = RenderTex;
				component3.gameObject.SetActive(value: true);
				RenderTexture.active = RenderTex;
				component3.transform.position = Sun.IconGenerationPos + VehicleCamPos;
				component3.transform.LookAt(Sun.IconGenerationPos + VehicleFocusPos, Vector3.up);
				if (portrait.Pose == PortraitPose.Watchtower)
				{
					component3.transform.position = Sun.IconGenerationPos + new Vector3(0f, 1f, 3f);
					component3.transform.LookAt(Sun.IconGenerationPos + new Vector3(0f, 3f, -4f), Vector3.up);
				}
				component3.Render();
				portrait.Tex.ReadPixels(new Rect(0f, 0f, RenderTex.width, RenderTex.height), 0, 0);
				portrait.Tex.Apply();
				if (instance6 != null)
				{
					instance5.Sun.SetupLightForTime(instance6.DaysSinceStart, useMiddayReflectionTex: false);
				}
				Object.DestroyImmediate(gameObject3);
			}
			portrait.Ready = true;
		}
		else if (portrait.Pose == PortraitPose.Chicken)
		{
			GameImpl instance7 = GameImpl.Instance;
			Session instance8 = Session.Instance;
			Camera component4 = instance7.UnityPortraitCameraObj.GetComponent<Camera>();
			Chicken chicken = new Chicken();
			chicken.Appearance = new ChickenAppearance(GenderType.Female, 1f);
			chicken.UnityInit();
			chicken.UnityActivate();
			GameObject unityObject = chicken.GetUnityObject();
			if (unityObject != null)
			{
				unityObject.transform.position = Sun.IconGenerationPos;
				unityObject.transform.rotation = Quaternion.Euler(0f, -80f, 0f);
				Prop.ReplaceLayerRecursively(unityObject, Character.DefaultLayer, Character.IconGimpLayer, skipParticleEffects: false);
				Shader.SetGlobalFloat(ShaderHash._Season, 2f);
				Shader.SetGlobalFloat(ShaderHash._SnowAmount, 0f);
				Shader.SetGlobalVector(ShaderHash._CloudPosXZDensityTime, Vector4.zero);
				Shader.SetGlobalVector(ShaderHash._WindDirStrength, Vector4.zero);
				instance7.Sun.SetupLightForTime(0.72f, useMiddayReflectionTex: true);
				component4.targetTexture = RenderTex;
				component4.gameObject.SetActive(value: true);
				RenderTexture.active = RenderTex;
				component4.transform.position = Sun.IconGenerationPos + new Vector3(-0.2f, 0.5f, 0.4f);
				component4.transform.LookAt(Sun.IconGenerationPos + new Vector3(-0.2f, 0.5f, 0f), Vector3.up);
				component4.Render();
				portrait.Tex.ReadPixels(new Rect(0f, 0f, RenderTex.width, RenderTex.height), 0, 0);
				portrait.Tex.Apply();
				if (instance8 != null)
				{
					instance7.Sun.SetupLightForTime(instance8.DaysSinceStart, useMiddayReflectionTex: false);
				}
			}
			chicken.UnityDeactivate();
			chicken.UnityDelete();
			portrait.Ready = true;
		}
		else if (portrait.PortraitSubject is Character)
		{
			((Character)portrait.PortraitSubject).TryStartGeneratingIcon(IconType.Portrait, portrait.Pose);
		}
		return null;
	}

	public void OnPortraitGenerated(Character character, PortraitPose pose)
	{
		foreach (Portrait portrait in Portraits)
		{
			if (portrait.PortraitSubject == character && portrait.Pose == pose && portrait.Tex.width == RenderTex.width && portrait.Tex.height == RenderTex.height)
			{
				portrait.Tex.ReadPixels(new Rect(0f, 0f, RenderTex.width, RenderTex.height), 0, 0);
				portrait.Tex.Apply();
				portrait.Ready = true;
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PipCameraBehaviour : CameraBehaviour
{
	private static List<Light> UnityLights = new List<Light>();

	private static Vector4[] PointLightColor = new Vector4[8];

	private static float Fudge1 = 1f;

	private static float Fudge2 = 1f;

	private static float Fudge3 = 1f;

	private static string PipRenderWithoutCameraStr = "PipRenderWithoutCamera";

	private static float CmdBufEdgeIntensity = 0.75f;

	private static float CmdBufNormalThreshold = 0.5f;

	private static float SHFillLightIntensity = 1f;

	private static float SHBackLightIntensity = 1f;

	private static float LightingBoostFactor = 2f;

	private Vector4[] avCoeff = new Vector4[7];

	public static void BuildListOfRenderedObjects(List<TileObject> renderedObjects)
	{
		int unityActivationRangePipView = Session.UnityActivationRangePipView;
		GameTerrain instance = GameTerrain.Instance;
		TileObject focusObject = Hud.Instance.Pip.FocusObject;
		renderedObjects.Clear();
		if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
		{
			TerrainCoord terrainCoord = focusObject?.GetCentreTile() ?? instance.GetTileCoordForPos(Vector3.zero);
			instance.GetUnityObjectsInRect(terrainCoord - new TerrainCoord(unityActivationRangePipView, unityActivationRangePipView), terrainCoord + new TerrainCoord(unityActivationRangePipView, unityActivationRangePipView), renderedObjects);
		}
		else
		{
			if (focusObject == null || !focusObject.CanUnityObjectBeActivated())
			{
				return;
			}
			renderedObjects.Add(focusObject);
			if (focusObject is Character character)
			{
				if (character.InteractionObject is Character character2 && character2.CanUnityObjectBeActivated())
				{
					renderedObjects.Add(character2);
				}
				if (character.CarryingObject != null && character.CarryingObject.CanUnityObjectBeActivated())
				{
					renderedObjects.Add(character.CarryingObject);
				}
				if (character.CarriedBy != null && character.CarriedBy.CanUnityObjectBeActivated())
				{
					renderedObjects.Add(character.CarriedBy);
				}
			}
			if (!(focusObject is Building building))
			{
				return;
			}
			for (int i = 0; i < building.Inhabitants.Length; i++)
			{
				if (building.Inhabitants[i] != null && building.GetInhabitantSlotDefs()[i].External && building.Inhabitants[i].CanUnityObjectBeActivated())
				{
					renderedObjects.Add(building.Inhabitants[i]);
				}
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		FogOfWarBehaviour = GetComponent<FogOfWarBehaviour>();
	}

	public override void OnPreCull()
	{
		base.OnPreCull();
		FogOfWarBehaviour.PlayerPos = Hud.Instance.Pip.GetFocusPos();
		if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
		{
			foreach (TileObject item in Session.Instance.UnityObjectsVisibleInPip)
			{
				item.GetPredictedOrElseThis().UnityUpdateLayer(pip: true, base.transform.position, Camera.farClipPlane);
			}
			return;
		}
		foreach (TileObject item2 in Session.Instance.UnityObjectsVisibleInPip)
		{
			if (item2 is Character)
			{
				item2.GetPredictedOrElseThis().UnityUpdateLayer(pip: true, base.transform.position, Camera.farClipPlane);
				continue;
			}
			GameObject unityObject = item2.GetPredictedOrElseThis().GetUnityObject();
			if (unityObject != null)
			{
				Prop.ReplaceLayerRecursively(unityObject, Character.DefaultLayer, Character.PipLayer, skipParticleEffects: true);
			}
		}
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
		{
			return;
		}
		foreach (TileObject item in Session.Instance.UnityObjectsVisibleInPip)
		{
			GameObject unityObject = item.GetPredictedOrElseThis().GetUnityObject();
			if (unityObject != null)
			{
				Prop.ReplaceLayerRecursively(unityObject, Character.PipLayer, (item is Character) ? Character.CharactersLayer : Character.DefaultLayer, skipParticleEffects: true);
			}
			if (item is Character character && character.Unity.MuzzleFlash != null)
			{
				Prop.SetLayerRecursively(character.Unity.MuzzleFlash.gameObject, Character.DefaultLayer);
			}
		}
	}

	public void RenderWithoutCamera()
	{
		using (new UnityProfileMarker(PipRenderWithoutCameraStr))
		{
			GameImpl instance = GameImpl.Instance;
			Camera camera = Camera;
			float lightingBoost = 0f;
			foreach (TileObject item in Session.Instance.UnityObjectsVisibleInPip)
			{
				if (item is Human human)
				{
					Color color = human.GetAppearance().GetSkinColor(human);
					float num = 1f - (color.r + color.g + color.b) / 3f;
					lightingBoost = Mathf.Max(0f, (num - 0.5f) * 2f) * Mathf.Lerp(0f, LightingBoostFactor, instance.Sun.UnityMoonLight.intensity);
					break;
				}
			}
			EnableBackLights();
			UnityLights.Clear();
			Light light = null;
			light = ((!instance.Sun.UnitySunLight.enabled || (!(instance.Sun.UnitySunLight.intensity >= instance.Sun.UnityMoonLight.intensity) && instance.Sun.UnityMoonLight.enabled)) ? instance.Sun.UnityMoonLight : instance.Sun.UnitySunLight);
			if (GraphicsDebugMenu.PipSphericalHarmonics)
			{
				UnityLights.Add(light);
			}
			else
			{
				if (instance.Sun.UnitySunLight.enabled)
				{
					UnityLights.Add(instance.Sun.UnitySunLight);
				}
				if (instance.Sun.UnityMoonLight.enabled)
				{
					UnityLights.Add(instance.Sun.UnityMoonLight);
				}
				if (instance.Sun.UnityFillLight.enabled)
				{
					UnityLights.Add(instance.Sun.UnityFillLight);
				}
				for (int i = 0; i < instance.Sun.UnityBackLight.Length; i++)
				{
					if (instance.Sun.UnityBackLight[i].enabled)
					{
						UnityLights.Add(instance.Sun.UnityBackLight[i]);
					}
				}
			}
			Vector4[] array = CalcSphericalHarmonics(lightingBoost);
			Color value = array[0] * Fudge1;
			Color value2 = array[1] * Fudge1;
			Color value3 = array[2] * Fudge1;
			Color value4 = array[3] * Fudge2;
			Color value5 = array[4] * Fudge2;
			Color value6 = array[5] * Fudge2;
			Color value7 = array[6] * Fudge3;
			RenderTexture active = RenderTexture.active;
			RenderTexture temporary = RenderTexture.GetTemporary(camera.targetTexture.width, camera.targetTexture.height, camera.targetTexture.depth, camera.targetTexture.format);
			RenderTexture temporary2 = RenderTexture.GetTemporary(camera.targetTexture.width, camera.targetTexture.height, camera.targetTexture.depth, camera.targetTexture.format);
			RenderTexture.active = temporary;
			GL.Clear(clearDepth: true, clearColor: true, MathUtil.TransparentBlackCol);
			GL.PushMatrix();
			GL.LoadProjectionMatrix(camera.projectionMatrix);
			GL.modelview = camera.worldToCameraMatrix;
			Shader.SetGlobalVector(ShaderHash._WorldSpaceCameraPos, camera.transform.position);
			Shader.SetGlobalColor(ShaderHash.unity_SHAr, value);
			Shader.SetGlobalColor(ShaderHash.unity_SHAg, value2);
			Shader.SetGlobalColor(ShaderHash.unity_SHAb, value3);
			Shader.SetGlobalColor(ShaderHash.unity_SHBr, value4);
			Shader.SetGlobalColor(ShaderHash.unity_SHBg, value5);
			Shader.SetGlobalColor(ShaderHash.unity_SHBb, value6);
			Shader.SetGlobalColor(ShaderHash.unity_SHC, value7);
			Sun sun = GameImpl.Instance.Sun;
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube0_ProbePosition, sun.UnityReflectionProbe.transform.position);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube1_ProbePosition, sun.UnityReflectionProbe.transform.position);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube0_BoxMax, sun.UnityReflectionProbe.bounds.max);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube1_BoxMax, sun.UnityReflectionProbe.bounds.max);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube0_BoxMin, sun.UnityReflectionProbe.bounds.min);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube1_BoxMin, sun.UnityReflectionProbe.bounds.min);
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube0_HDR, new Vector4(1f, 1f, 0f, 0f));
			Shader.SetGlobalVector(ShaderHash.unity_SpecCube1_HDR, new Vector4(1f, 1f, 0f, 0f));
			Shader.SetGlobalTexture(ShaderHash.unity_SpecCube0, sun.UnityReflectionProbe.texture);
			Shader.SetGlobalTexture(ShaderHash.unity_SpecCube1, sun.UnityReflectionProbe.texture);
			foreach (TileObject item2 in Session.Instance.UnityObjectsVisibleInPip)
			{
				OutlineCameraBehaviour.DrawTileObjectToCommandBuffer(item2.GetPredictedOrElseThis(), UnityLights, toDepthBuffer: false, lightingBoost);
			}
			GL.PopMatrix();
			RenderTexture.active = temporary2;
			GL.Clear(clearDepth: true, clearColor: true, new Color32(127, 127, byte.MaxValue, byte.MaxValue));
			GL.PushMatrix();
			GL.LoadProjectionMatrix(camera.projectionMatrix);
			GL.modelview = camera.worldToCameraMatrix;
			Shader.SetGlobalVector(ShaderHash._WorldSpaceCameraPos, camera.transform.position);
			SetShaderGlobalsFromCamera(camera);
			foreach (TileObject item3 in Session.Instance.UnityObjectsVisibleInPip)
			{
				OutlineCameraBehaviour.DrawTileObjectToCommandBuffer(item3.GetPredictedOrElseThis(), UnityLights, toDepthBuffer: true, lightingBoost);
			}
			GL.PopMatrix();
			DisableBackLights();
			Shader.SetGlobalTexture(ShaderHash._CameraDepthNormalsTexture, temporary2);
			float normalThreshold = FogOfWarBehaviour.NormalThreshold;
			float edgeIntensity = FogOfWarBehaviour.EdgeIntensity;
			FogOfWarBehaviour.ConstantsDirty |= FogOfWarBehaviour.NormalThreshold != CmdBufNormalThreshold;
			FogOfWarBehaviour.ConstantsDirty |= FogOfWarBehaviour.EdgeIntensity != CmdBufEdgeIntensity;
			FogOfWarBehaviour.NormalThreshold = CmdBufNormalThreshold;
			FogOfWarBehaviour.EdgeIntensity = CmdBufEdgeIntensity;
			FogOfWarBehaviour.ApplyFogOfWar(temporary, camera.targetTexture);
			FogOfWarBehaviour.NormalThreshold = normalThreshold;
			FogOfWarBehaviour.EdgeIntensity = edgeIntensity;
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			SetShaderGlobalsFromCamera(HudBehaviour.Instance.UnityGameCamera);
		}
	}

	private void SetShaderGlobalsFromCamera(Camera unityCamera)
	{
		float nearClipPlane = unityCamera.nearClipPlane;
		float farClipPlane = unityCamera.farClipPlane;
		float num = unityCamera.targetTexture.width;
		float num2 = unityCamera.targetTexture.height;
		Shader.SetGlobalVector(ShaderHash._ProjectionParams, new Vector4(1f, nearClipPlane, farClipPlane, 1f / farClipPlane));
		Shader.SetGlobalVector(ShaderHash._ScreenParams, new Vector4(num, num2, 1f + 1f / num, 1f + 1f / num2));
		Shader.SetGlobalVector(ShaderHash._ZBufferParams, new Vector4(1f - farClipPlane / nearClipPlane, farClipPlane / nearClipPlane, (1f - farClipPlane / nearClipPlane) / farClipPlane, 1f / nearClipPlane));
	}

	private Vector4[] CalcSphericalHarmonics(float lightingBoost)
	{
		GameImpl instance = GameImpl.Instance;
		SphericalHarmonicsL2 sphericalHarmonicsL = default(SphericalHarmonicsL2);
		sphericalHarmonicsL.Clear();
		if (SunFiddler.AmbientLightEnabled)
		{
			Color ambient = instance.Sun.LightingSettings.Ambient;
			ambient += lightingBoost * ambient;
			sphericalHarmonicsL.AddAmbientLight(ambient);
		}
		if (GraphicsDebugMenu.PipSphericalHarmonics)
		{
			if (instance.Sun.UnitySunLight.enabled && instance.Sun.UnityMoonLight.enabled)
			{
				if (instance.Sun.UnitySunLight.intensity < instance.Sun.UnityMoonLight.intensity)
				{
					sphericalHarmonicsL.AddDirectionalLight(-instance.Sun.UnitySunLight.transform.forward, instance.Sun.UnitySunLight.color, instance.Sun.UnitySunLight.intensity * (1f + lightingBoost));
				}
				else
				{
					sphericalHarmonicsL.AddDirectionalLight(-instance.Sun.UnityMoonLight.transform.forward, instance.Sun.UnityMoonLight.color, instance.Sun.UnityMoonLight.intensity * (1f + lightingBoost));
				}
			}
			if (instance.Sun.UnityFillLight.enabled)
			{
				sphericalHarmonicsL.AddDirectionalLight(-instance.Sun.UnityFillLight.transform.forward, instance.Sun.UnityFillLight.color, instance.Sun.UnityFillLight.intensity * (SHFillLightIntensity + lightingBoost));
			}
			for (int i = 0; i < instance.Sun.UnityBackLight.Length; i++)
			{
				if (instance.Sun.UnityBackLight[i].enabled)
				{
					sphericalHarmonicsL.AddDirectionalLight(-instance.Sun.UnityBackLight[i].transform.forward, instance.Sun.UnityBackLight[i].color, instance.Sun.UnityBackLight[i].intensity * (SHBackLightIntensity + lightingBoost));
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			avCoeff[j].x = sphericalHarmonicsL[j, 3];
			avCoeff[j].y = sphericalHarmonicsL[j, 1];
			avCoeff[j].z = sphericalHarmonicsL[j, 2];
			avCoeff[j].w = sphericalHarmonicsL[j, 0] - sphericalHarmonicsL[j, 6];
		}
		for (int k = 0; k < 3; k++)
		{
			avCoeff[k + 3].x = sphericalHarmonicsL[k, 4];
			avCoeff[k + 3].y = sphericalHarmonicsL[k, 5];
			avCoeff[k + 3].z = 3f * sphericalHarmonicsL[k, 6];
			avCoeff[k + 3].w = sphericalHarmonicsL[k, 7];
		}
		avCoeff[6].x = sphericalHarmonicsL[0, 8];
		avCoeff[6].y = sphericalHarmonicsL[1, 8];
		avCoeff[6].z = sphericalHarmonicsL[2, 8];
		avCoeff[6].w = 1f;
		return avCoeff;
	}
}

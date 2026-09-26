using System;
using System.Collections.Generic;
using UnityEngine;

public class OutlineCameraBehaviour : IComparable<OutlineCameraBehaviour>
{
	public RenderTexture OutlineBufferTexture;

	public List<TileObject> ObjectsToOutline = new List<TileObject>();

	public Color OutlineCol = Color.white;

	public float CamZ;

	private static string DrawTileObjectToCommandBufferStr = "DrawTileObjectToCommandBuffer";

	private static string DrawGameObjectToCommandBufferRecursiveStr = "DrawGameObjectToCommandBufferRecursive";

	private static string DIRECTIONAL = "DIRECTIONAL";

	private static Matrix4x4[] WorldToShadow = new Matrix4x4[4];

	private static Vector4[] unity_ShadowSplitSpheres = new Vector4[4];

	private static Resource<Material> WriteDepthNormalsMaterialRes;

	private static Material WriteDepthNormalsMaterial;

	private static string DrawRendererToCommandBufferStr = "DrawRendererToCommandBuffer";

	private static string HairStr = "Hair";

	private static string LightStr = "Light";

	private static string DepthStr = "Depth";

	private static GraphicsBuffer UnityReflectionProbesBuffer;

	public void ClearObjectToOutline()
	{
		ObjectsToOutline.Clear();
	}

	public void SetObjectToOutline(TileObject obj, Color col)
	{
		if (obj != null && obj.GetPredictedOrElseThis().IsUnityObjectActive())
		{
			if (col.a == 0f)
			{
				col = Color.white;
			}
			ObjectsToOutline.Add(obj);
			OutlineCol = col;
			if (obj != null)
			{
				OutlineBehaviour.ActiveOutlineCameras.Add(this);
			}
		}
	}

	public int CompareTo(OutlineCameraBehaviour other)
	{
		if (CamZ < other.CamZ)
		{
			return 1;
		}
		if (CamZ > other.CamZ)
		{
			return -1;
		}
		return 0;
	}

	public OutlineCameraBehaviour(string name)
	{
	}

	public void DrawWithoutCameraOverhead()
	{
		if (ObjectsToOutline.Count > 0)
		{
			RenderTexture mainViewRenderTexture = HudBehaviour.Instance.MainViewRenderTexture;
			Vector2Int mainPanelSizeWithoutSuperSample = GameImpl.Instance.MainPanelSizeWithoutSuperSample;
			Camera unityGameCamera = HudBehaviour.Instance.UnityGameCamera;
			if (OutlineBufferTexture == null || OutlineBufferTexture.width != mainPanelSizeWithoutSuperSample.x || OutlineBufferTexture.height != mainPanelSizeWithoutSuperSample.y)
			{
				if (OutlineBufferTexture != null)
				{
					OutlineBufferTexture.Release();
				}
				OutlineBufferTexture = new RenderTexture(mainPanelSizeWithoutSuperSample.x, mainPanelSizeWithoutSuperSample.y, mainViewRenderTexture.depth, RenderTextureFormat.Depth);
				OutlineBufferTexture.Create();
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = OutlineBufferTexture;
			GL.Clear(clearDepth: true, clearColor: true, MathUtil.TransparentBlackCol);
			GL.PushMatrix();
			GL.LoadProjectionMatrix(unityGameCamera.projectionMatrix);
			GL.modelview = unityGameCamera.worldToCameraMatrix;
			foreach (TileObject item in ObjectsToOutline)
			{
				DrawTileObjectToCommandBuffer(item.GetPredictedOrElseThis(), null, toDepthBuffer: false, 0f);
			}
			GL.PopMatrix();
			RenderTexture.active = active;
		}
		ObjectsToOutline.Clear();
	}

	public static void DrawTileObjectToCommandBuffer(TileObject obj, List<Light> unityLights, bool toDepthBuffer, float lightingBoost)
	{
		using (new UnityProfileMarker(DrawTileObjectToCommandBufferStr))
		{
			if (obj is Character character)
			{
				if (character.GetBaseObjectType() != BaseObjectType.Human)
				{
					LODGroup component = character.Unity.Obj.GetComponent<LODGroup>();
					if (component != null && component.lodCount > 0)
					{
						Renderer[] renderers = component.GetLODs()[0].renderers;
						foreach (Renderer renderer in renderers)
						{
							if (renderer != null)
							{
								DrawRendererToCommandBuffer(renderer, unityLights, toDepthBuffer, lightingBoost);
							}
						}
						return;
					}
				}
				if (character.Unity.SkinnedMeshRenderers != null)
				{
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in character.Unity.SkinnedMeshRenderers)
					{
						DrawRendererToCommandBuffer(skinnedMeshRenderer, unityLights, toDepthBuffer, lightingBoost);
					}
				}
				if (character.Unity.MeshRenderers != null)
				{
					foreach (MeshRenderer meshRenderer in character.Unity.MeshRenderers)
					{
						DrawRendererToCommandBuffer(meshRenderer, unityLights, toDepthBuffer, lightingBoost);
					}
				}
				if (!(character.EquippedItem is Bow) || !(character.Unity.WeaponObj != null))
				{
					return;
				}
				for (int j = 0; j < character.Unity.WeaponObj.transform.childCount; j++)
				{
					SkinnedMeshRenderer component2 = character.Unity.WeaponObj.transform.GetChild(j).gameObject.GetComponent<SkinnedMeshRenderer>();
					if (component2 != null)
					{
						DrawRendererToCommandBuffer(component2, unityLights, toDepthBuffer, lightingBoost);
					}
				}
			}
			else
			{
				GameObject unityObject = obj.GetUnityObject();
				if (unityObject != null)
				{
					DrawGameObjectToCommandBufferRecursive(unityObject, unityLights, toDepthBuffer, lightingBoost);
				}
				if (obj is Prop prop && prop.UnityConstructionCordon != null)
				{
					DrawGameObjectToCommandBufferRecursive(prop.UnityConstructionCordon, unityLights, toDepthBuffer, lightingBoost);
				}
			}
		}
	}

	private static void DrawGameObjectToCommandBufferRecursive(GameObject go, List<Light> unityLights, bool toDepthBuffer, float lightingBoost)
	{
		using (new UnityProfileMarker(DrawGameObjectToCommandBufferRecursiveStr))
		{
			LODGroup component = go.GetComponent<LODGroup>();
			if (component != null && component.lodCount > 0)
			{
				Renderer[] renderers = component.GetLODs()[0].renderers;
				foreach (Renderer renderer in renderers)
				{
					if (renderer != null)
					{
						DrawRendererToCommandBuffer(renderer, unityLights, toDepthBuffer, lightingBoost);
					}
				}
				return;
			}
			MeshRenderer component2 = go.GetComponent<MeshRenderer>();
			if (component2 != null)
			{
				DrawRendererToCommandBuffer(component2, unityLights, toDepthBuffer, lightingBoost);
			}
			for (int j = 0; j < go.transform.childCount; j++)
			{
				GameObject gameObject = go.transform.GetChild(j).gameObject;
				if (gameObject.activeSelf)
				{
					DrawGameObjectToCommandBufferRecursive(gameObject, unityLights, toDepthBuffer, lightingBoost);
				}
			}
		}
	}

	public static void LoadContent()
	{
		WriteDepthNormalsMaterialRes = new Resource<Material>("Materials/PipDepthNormals");
	}

	public static Material CreateWriteDepthNormalsMaterial()
	{
		if (!WriteDepthNormalsMaterial)
		{
			WriteDepthNormalsMaterial = new Material(WriteDepthNormalsMaterialRes);
			WriteDepthNormalsMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		return WriteDepthNormalsMaterial;
	}

	private static void DrawRendererToCommandBuffer(Renderer renderer, List<Light> unityLights, bool toDepthBuffer, float lightingBoost)
	{
		using (new UnityProfileMarker(DrawRendererToCommandBufferStr))
		{
			RendererDataBehaviour rendererDataBehaviour = renderer.gameObject.GetComponent<RendererDataBehaviour>();
			if (rendererDataBehaviour == null)
			{
				rendererDataBehaviour = renderer.gameObject.AddComponent<RendererDataBehaviour>();
				rendererDataBehaviour.InitCommandBuffers(renderer);
			}
			else if (rendererDataBehaviour.RendererDataDirty)
			{
				rendererDataBehaviour.DestroyCommandBuffers();
				rendererDataBehaviour.InitCommandBuffers(renderer);
			}
			if (rendererDataBehaviour.HasGrabPass)
			{
				return;
			}
			Material[] cachedMaterials = rendererDataBehaviour.CachedMaterials;
			if (cachedMaterials == null)
			{
				return;
			}
			for (int i = 0; i < cachedMaterials.Length; i++)
			{
				Material material = cachedMaterials[i];
				if (!(material != null))
				{
					continue;
				}
				if (toDepthBuffer)
				{
					if (rendererDataBehaviour.Eyelash || material.shader.renderQueue >= 3000)
					{
						continue;
					}
					using (new UnityProfileMarker(DepthStr))
					{
						if (material.shader.renderQueue >= 2450)
						{
							WriteDepthNormalsMaterial.mainTexture = material.mainTexture;
						}
						if (rendererDataBehaviour.DepthBufferPass[i] != null)
						{
							Graphics.ExecuteCommandBuffer(rendererDataBehaviour.DepthBufferPass[i]);
						}
					}
					continue;
				}
				if (rendererDataBehaviour.Hair)
				{
					using (new UnityProfileMarker(HairStr))
					{
						if (rendererDataBehaviour.HairCutoutPass[i] != null)
						{
							Graphics.ExecuteCommandBuffer(rendererDataBehaviour.HairCutoutPass[i]);
						}
					}
				}
				if (unityLights != null)
				{
					material.EnableKeyword(DIRECTIONAL);
					for (int j = 0; j < unityLights.Count; j++)
					{
						using (new UnityProfileMarker(LightStr))
						{
							Light light = unityLights[j];
							Vector3 forward = light.transform.forward;
							Color value = light.color * (light.intensity + lightingBoost);
							Shader.SetGlobalVector(ShaderHash._WorldSpaceLightPos0, -new Vector4(forward.x, forward.y, forward.z, 0f));
							Shader.SetGlobalColor(ShaderHash._LightColor0, value);
							if (j == 0)
							{
								if (rendererDataBehaviour.ForwardBasePass[i] != null)
								{
									Graphics.ExecuteCommandBuffer(rendererDataBehaviour.ForwardBasePass[i]);
								}
							}
							else if (rendererDataBehaviour.ForwardAdditivePass[i] != null)
							{
								Graphics.ExecuteCommandBuffer(rendererDataBehaviour.ForwardAdditivePass[i]);
							}
						}
					}
				}
				else if (!rendererDataBehaviour.Hair && rendererDataBehaviour.ForwardBasePass[i] != null)
				{
					Graphics.ExecuteCommandBuffer(rendererDataBehaviour.ForwardBasePass[i]);
				}
			}
		}
	}
}

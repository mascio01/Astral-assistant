using UnityEngine;
using UnityEngine.Rendering;

public class RendererDataBehaviour : MonoBehaviour
{
	public bool RendererDataDirty = true;

	public Material[] CachedMaterials;

	public CommandBuffer[] ForwardBasePass;

	public CommandBuffer[] ForwardAdditivePass;

	public CommandBuffer[] DepthBufferPass;

	public CommandBuffer[] HairCutoutPass;

	public bool Hair;

	public bool Eyelash;

	public bool HasGrabPass;

	private static string FORWARD = "FORWARD";

	public static ShaderTagId GRABPASS;

	public static ShaderTagId LightMode;

	private void OnDestroy()
	{
		DestroyCommandBuffers();
	}

	public void InitCommandBuffers(Renderer renderer)
	{
		CachedMaterials = renderer.materials;
		Hair = renderer.gameObject.tag == Human.HairTag;
		Eyelash = renderer.gameObject.tag == Human.EyelashTag;
		if (CachedMaterials != null)
		{
			ForwardBasePass = new CommandBuffer[CachedMaterials.Length];
			ForwardAdditivePass = new CommandBuffer[CachedMaterials.Length];
			DepthBufferPass = new CommandBuffer[CachedMaterials.Length];
			if (Hair)
			{
				HairCutoutPass = new CommandBuffer[CachedMaterials.Length];
			}
			for (int i = 0; i < CachedMaterials.Length; i++)
			{
				Material material = CachedMaterials[i];
				if (!(material != null))
				{
					continue;
				}
				Shader shader = material.shader;
				if (shader != null)
				{
					for (int j = 0; j < shader.subshaderCount; j++)
					{
						int passCountInSubshader = shader.GetPassCountInSubshader(j);
						for (int k = 0; k < passCountInSubshader; k++)
						{
							if (shader.FindPassTagValue(j, k, LightMode) == GRABPASS)
							{
								HasGrabPass = true;
							}
						}
					}
				}
				for (int l = 0; l < material.passCount; l++)
				{
					if (material.GetPassName(l).Contains(FORWARD))
					{
						if (ForwardBasePass[i] != null)
						{
							ForwardAdditivePass[i] = new CommandBuffer();
							ForwardAdditivePass[i].DrawRenderer(renderer, material, i, l);
							break;
						}
						ForwardBasePass[i] = new CommandBuffer();
						ForwardBasePass[i].DrawRenderer(renderer, material, i, l);
					}
				}
				if (ForwardBasePass[i] == null)
				{
					ForwardBasePass[i] = new CommandBuffer();
					ForwardBasePass[i].DrawRenderer(renderer, material, i, 0);
				}
				if (Hair)
				{
					HairCutoutPass[i] = new CommandBuffer();
					HairCutoutPass[i].DrawRenderer(renderer, material, i, 0);
				}
				int shaderPass = 0;
				if (material.shader.renderQueue >= 2450)
				{
					shaderPass = 1;
				}
				DepthBufferPass[i] = new CommandBuffer();
				DepthBufferPass[i].DrawRenderer(renderer, OutlineCameraBehaviour.CreateWriteDepthNormalsMaterial(), i, shaderPass);
			}
		}
		RendererDataDirty = false;
	}

	public void DestroyCommandBuffers()
	{
		if (ForwardBasePass != null)
		{
			for (int i = 0; i < ForwardBasePass.Length; i++)
			{
				if (ForwardBasePass[i] != null)
				{
					ForwardBasePass[i].Dispose();
				}
			}
			for (int j = 0; j < ForwardAdditivePass.Length; j++)
			{
				if (ForwardAdditivePass[j] != null)
				{
					ForwardAdditivePass[j].Dispose();
				}
			}
			for (int k = 0; k < DepthBufferPass.Length; k++)
			{
				if (DepthBufferPass[k] != null)
				{
					DepthBufferPass[k].Dispose();
				}
			}
			ForwardBasePass = null;
			ForwardAdditivePass = null;
			DepthBufferPass = null;
		}
		if (HairCutoutPass != null)
		{
			for (int l = 0; l < HairCutoutPass.Length; l++)
			{
				if (HairCutoutPass[l] != null)
				{
					HairCutoutPass[l].Dispose();
				}
			}
			HairCutoutPass = null;
		}
		CachedMaterials = null;
		RendererDataDirty = true;
	}
}

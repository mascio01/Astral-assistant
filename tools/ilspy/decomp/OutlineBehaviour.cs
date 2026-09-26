using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutlineBehaviour : MonoBehaviour
{
	public float LineThickness = 4f;

	private CommandBuffer CommandBuffer;

	private static Resource<Material> OutlineMaterial;

	private Material OutlineMaterialCopy;

	public static List<OutlineCameraBehaviour> ActiveOutlineCameras = new List<OutlineCameraBehaviour>();

	public static void LoadContent()
	{
		OutlineMaterial = new Resource<Material>("Materials/Outline");
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (ActiveOutlineCameras.Count == 0)
		{
			Graphics.Blit(source, destination);
			return;
		}
		foreach (OutlineCameraBehaviour activeOutlineCamera in ActiveOutlineCameras)
		{
			float a = float.MaxValue;
			float num = float.MinValue;
			foreach (TileObject item in activeOutlineCamera.ObjectsToOutline)
			{
				float z = HudBehaviour.Instance.UnityGameCamera.WorldToScreenPoint(item.GetBoundingBoxCentre()).z;
				a = Mathf.Min(a, z);
				num = Mathf.Max(num, z);
			}
			activeOutlineCamera.CamZ = Mathf.Lerp(a, num, 0.5f);
		}
		ActiveOutlineCameras.Sort();
		RenderTexture renderTexture = null;
		for (int i = 0; i < ActiveOutlineCameras.Count; i++)
		{
			OutlineCameraBehaviour outlineCameraBehaviour = ActiveOutlineCameras[i];
			outlineCameraBehaviour.DrawWithoutCameraOverhead();
			RenderTexture renderTexture2 = null;
			if (i < ActiveOutlineCameras.Count - 1)
			{
				renderTexture2 = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			}
			RenderOutline((renderTexture != null) ? renderTexture : source, (renderTexture2 != null) ? renderTexture2 : destination, outlineCameraBehaviour);
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			renderTexture = renderTexture2;
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	private void RenderOutline(RenderTexture source, RenderTexture destination, OutlineCameraBehaviour outlineCameraBehaviour)
	{
		if (OutlineMaterialCopy == null)
		{
			OutlineMaterialCopy = Object.Instantiate(OutlineMaterial.GetAsset());
		}
		OutlineMaterialCopy.SetTexture(ShaderHash._OutlineSource, outlineCameraBehaviour.OutlineBufferTexture);
		OutlineMaterialCopy.SetColor(ShaderHash._LineColor, outlineCameraBehaviour.OutlineCol);
		OutlineMaterialCopy.SetFloat(ShaderHash._LineThicknessX, LineThickness / 1000f * (1f / (float)outlineCameraBehaviour.OutlineBufferTexture.width) * 1000f);
		OutlineMaterialCopy.SetFloat(ShaderHash._LineThicknessY, LineThickness / 1000f * (1f / (float)outlineCameraBehaviour.OutlineBufferTexture.height) * 1000f);
		Graphics.Blit(source, destination, OutlineMaterialCopy, 0);
	}
}

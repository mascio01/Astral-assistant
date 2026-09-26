using UnityEngine;

public class BFX_BloodDecalLayers : MonoBehaviour
{
	public enum DecalLayersProperty
	{
		DrawToSelectedLayers,
		IgnoreSelectedLayers
	}

	public enum DepthMode
	{
		FullScreen,
		HalfScreen,
		QuarterScreen
	}

	public LayerMask DecalLayers = 1;

	public DecalLayersProperty DecalRenderingMode;

	public DepthMode LayerDepthResoulution;

	private DepthTextureMode defaultMode;

	private RenderTexture rt;

	private Camera depthCamera;

	private void OnEnable()
	{
		Camera component = GetComponent<Camera>();
		defaultMode = component.depthTextureMode;
		if (component.renderingPath == RenderingPath.Forward)
		{
			component.depthTextureMode |= DepthTextureMode.Depth;
		}
		GameObject gameObject = new GameObject("DecalLayersCamera");
		gameObject.transform.parent = component.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		depthCamera = gameObject.AddComponent<Camera>();
		depthCamera.CopyFrom(component);
		depthCamera.renderingPath = RenderingPath.Forward;
		depthCamera.depth = component.depth - 1f;
		depthCamera.cullingMask = DecalLayers;
		CreateDepthTexture();
		depthCamera.targetTexture = rt;
		Shader.SetGlobalTexture("_LayerDecalDepthTexture", rt);
		Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS");
		if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers)
		{
			Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
		}
	}

	private void OnDisable()
	{
		GetComponent<Camera>().depthTextureMode = defaultMode;
		RenderTexture.ReleaseTemporary(rt);
		Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS");
		if (DecalRenderingMode == DecalLayersProperty.IgnoreSelectedLayers)
		{
			Shader.DisableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
		}
	}

	private void CreateDepthTexture()
	{
		switch (LayerDepthResoulution)
		{
		case DepthMode.FullScreen:
			rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
			break;
		case DepthMode.HalfScreen:
			rt = RenderTexture.GetTemporary((int)((float)Screen.width * 0.5f), (int)((float)Screen.height * 0.5f), 24, RenderTextureFormat.Depth);
			break;
		case DepthMode.QuarterScreen:
			rt = RenderTexture.GetTemporary((int)((float)Screen.width * 0.25f), (int)((float)Screen.height * 0.25f), 24, RenderTextureFormat.Depth);
			break;
		}
	}
}

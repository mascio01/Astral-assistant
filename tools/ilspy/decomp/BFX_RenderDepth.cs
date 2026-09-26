using UnityEngine;

public class BFX_RenderDepth : MonoBehaviour
{
	private DepthTextureMode defaultMode;

	private void OnEnable()
	{
		Camera component = GetComponent<Camera>();
		defaultMode = component.depthTextureMode;
		if (component.renderingPath == RenderingPath.Forward)
		{
			component.depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	private void OnDisable()
	{
		GetComponent<Camera>().depthTextureMode = defaultMode;
	}
}

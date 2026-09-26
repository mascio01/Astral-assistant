using UnityEngine;
using UnityEngine.Rendering;

public class MyPipeline : RenderPipeline
{
	private CommandBuffer cmd = new CommandBuffer();

	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		foreach (Camera camera in cameras)
		{
			if (camera.TryGetCullingParameters(out var cullingParameters))
			{
				context.Cull(ref cullingParameters);
				context.SetupCameraProperties(camera);
				cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.magenta);
				context.ExecuteCommandBuffer(cmd);
				cmd.Clear();
				ShaderTagId shaderPassName = new ShaderTagId("ForwardBase");
				SortingSettings sortingSettings = new SortingSettings(camera);
				sortingSettings.criteria = SortingCriteria.CommonOpaque;
				new DrawingSettings(shaderPassName, sortingSettings);
				FilteringSettings defaultValue = FilteringSettings.defaultValue;
				defaultValue.renderQueueRange = RenderQueueRange.opaque;
				context.Submit();
			}
		}
	}
}

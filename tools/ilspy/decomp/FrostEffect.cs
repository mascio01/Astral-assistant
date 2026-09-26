using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Frost")]
public class FrostEffect : MonoBehaviour
{
	public float FrostAmount = 0.5f;

	public float EdgeSharpness = 1f;

	public float minFrost;

	public float maxFrost = 1f;

	public float seethroughness = 0.2f;

	public float distortion = 0.1f;

	public Texture2D Frost;

	public Texture2D FrostNormals;

	public Shader Shader;

	private Material material;

	private void Awake()
	{
		material = new Material(Shader);
		material.SetTexture(ShaderHash._BlendTex, Frost);
		material.SetTexture(ShaderHash._BumpMap, FrostNormals);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!Application.isPlaying)
		{
			material.SetTexture(ShaderHash._BlendTex, Frost);
			material.SetTexture(ShaderHash._BumpMap, FrostNormals);
			EdgeSharpness = Mathf.Max(1f, EdgeSharpness);
		}
		material.SetFloat(ShaderHash._BlendAmount, Mathf.Clamp01(Mathf.Clamp01(FrostAmount) * (maxFrost - minFrost) + minFrost));
		material.SetFloat(ShaderHash._EdgeSharpness, EdgeSharpness);
		material.SetFloat(ShaderHash._SeeThroughness, seethroughness);
		material.SetFloat(ShaderHash._Distortion, distortion);
		Graphics.Blit(source, destination, material);
	}
}

using UnityEngine;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Fog Of War")]
public class FogOfWarBehaviour : PostEffectsBase
{
	public enum CameraUsage
	{
		Game,
		Pip,
		Icon,
		Portrait
	}

	private Camera Camera;

	private CameraBehaviour CameraBehaviour;

	public Shader FogOfWarShader;

	public Shader BlurShader;

	private Material FogOfWarMaterial;

	private Material BlurMaterial;

	public float FogStart = 24f;

	public float FogEnd = 32f;

	public float FarFogStart = 64f;

	public float FarFogEnd = 96f;

	public float HeightFogStart = 8f;

	public float HeightFogEnd = 2f;

	public float SkyHeightFogStart = 128f;

	public float SkyHeightFogEnd = 64f;

	public float FogAmount = 1f;

	public int BlurGridSize = 1;

	public float BlurSpread;

	public Vector3 PlayerPos;

	public float EdgeWidth = 0.75f;

	public float EdgeIntensity = 0.75f;

	public float NormalThreshold = 3f;

	public float DepthThreshold = 0.0001f;

	public float NormalSensitivity = 1f;

	public float DepthSensitivity = 100f;

	public bool FogEnabled = true;

	public bool FogOfWarEnabled = true;

	public CameraUsage Usage;

	private static string ApplyFogOfWarStr = "ApplyFogOfWar";

	private static string BlitStr = "Blit";

	private static string SetTextureStr = "SetTexture";

	private static string SetConstantsStr = "SetConstants";

	public bool ConstantsDirty = true;

	public float Foo = 0.65f;

	private void Awake()
	{
		Camera = GetComponent<Camera>();
		CameraBehaviour = GetComponent<CameraBehaviour>();
		Camera.depthTextureMode = DepthTextureMode.DepthNormals;
		CheckResources();
	}

	public override bool CheckResources()
	{
		CheckSupport(needDepth: true);
		FogOfWarMaterial = CheckShaderAndCreateMaterial(FogOfWarShader, FogOfWarMaterial);
		BlurMaterial = CheckShaderAndCreateMaterial(BlurShader, BlurMaterial);
		if (!isSupported)
		{
			ReportAutoDisable();
		}
		return isSupported;
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		ApplyFogOfWar(source, destination);
	}

	public void ApplyFogOfWar(RenderTexture source, RenderTexture destination)
	{
		using (new UnityProfileMarker(ApplyFogOfWarStr))
		{
			GameImpl instance = GameImpl.Instance;
			GameTerrain instance2 = GameTerrain.Instance;
			if (FogOfWarMaterial == null || BlurMaterial == null || instance == null || instance.Sun.UnitySunLight == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			Vector3 position = base.transform.position;
			Vector3 vector = base.transform.TransformVector(CameraBehaviour.FarFrustumCornersInCamSpace[0]);
			Vector3 vector2 = base.transform.TransformVector(CameraBehaviour.FarFrustumCornersInCamSpace[1]);
			Vector3 vector3 = base.transform.TransformVector(CameraBehaviour.FarFrustumCornersInCamSpace[2]);
			Vector3 vector4 = base.transform.TransformVector(CameraBehaviour.FarFrustumCornersInCamSpace[3]);
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetRow(0, vector);
			identity.SetRow(1, vector4);
			identity.SetRow(2, vector2);
			identity.SetRow(3, vector3);
			FogOfWarMaterial.SetMatrix(ShaderHash._FrustumFarCornersWS, identity);
			FogOfWarMaterial.SetVector(ShaderHash._CameraWS, position);
			using (new UnityProfileMarker(SetTextureStr))
			{
				if (FogOfWarEnabled && instance2 != null)
				{
					FogOfWarMaterial.SetTexture(ShaderHash._FogOfWarTex, instance2.FogOfWar.GetCurTex());
				}
				else
				{
					FogOfWarMaterial.SetTexture(ShaderHash._FogOfWarTex, FogOfWar.AllVisibleTex);
				}
			}
			FogOfWarMaterial.SetFloat(ShaderHash._FogStart, FogStart);
			FogOfWarMaterial.SetFloat(ShaderHash._FogEnd, FogEnd);
			FogOfWarMaterial.SetFloat(ShaderHash._FogAmount, FogAmount);
			FogOfWarMaterial.SetVector(ShaderHash._SunDir, instance.Sun.UnitySunLight.transform.forward);
			FogOfWarMaterial.SetVector(ShaderHash._PlayerPos, PlayerPos);
			using (new UnityProfileMarker(SetConstantsStr))
			{
				if (instance2 != null)
				{
					FogOfWarMaterial.SetVector(ShaderHash._TerrainHalfSize_HeightRange, new Vector4(instance2.HalfSize, 64f, 0f, 0f));
					FogOfWarMaterial.SetTexture(ShaderHash._HeightMapTex, instance2.HeightMapTex);
				}
				float num = Mathf.Pow(Camera.farClipPlane / 1024f, Foo);
				FogOfWarMaterial.SetFloat(ShaderHash._FarFogStart, FarFogStart);
				FogOfWarMaterial.SetFloat(ShaderHash._FarFogEnd, FarFogEnd);
				FogOfWarMaterial.SetFloat(ShaderHash._HeightFogStart, HeightFogStart);
				FogOfWarMaterial.SetFloat(ShaderHash._HeightFogEnd, HeightFogEnd);
				FogOfWarMaterial.SetFloat(ShaderHash._SkyHeightFogStart, SkyHeightFogStart * num);
				FogOfWarMaterial.SetFloat(ShaderHash._SkyHeightFogEnd, SkyHeightFogEnd * num);
				FogOfWarMaterial.SetFloat(ShaderHash._EdgeWidth, EdgeWidth);
				FogOfWarMaterial.SetFloat(ShaderHash._EdgeIntensity, EdgeIntensity);
				FogOfWarMaterial.SetFloat(ShaderHash._NormalThreshold, NormalThreshold);
				FogOfWarMaterial.SetFloat(ShaderHash._DepthThreshold, DepthThreshold);
				FogOfWarMaterial.SetFloat(ShaderHash._NormalSensitivity, NormalSensitivity);
				FogOfWarMaterial.SetFloat(ShaderHash._DepthSensitivity, DepthSensitivity);
			}
			using (new UnityProfileMarker(BlitStr))
			{
				if (GameImpl.SuperSample && Usage != CameraUsage.Icon && Usage != CameraUsage.Portrait)
				{
					Vector2Int vector2Int = new Vector2Int(source.width, source.height);
					RenderTexture temporary = RenderTexture.GetTemporary(vector2Int.x, vector2Int.y, 0, destination.format);
					RenderTexture temporary2 = RenderTexture.GetTemporary(vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RFloat);
					RenderTexture temporary3 = RenderTexture.GetTemporary(vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RFloat);
					RenderTexture temporary4 = RenderTexture.GetTemporary(vector2Int.x, vector2Int.y, 0, RenderTextureFormat.RFloat);
					Graphics.SetRenderTarget(new RenderBuffer[2] { temporary.colorBuffer, temporary2.colorBuffer }, temporary.depthBuffer);
					GL.Clear(clearDepth: true, clearColor: true, Color.clear);
					GL.PushMatrix();
					GL.LoadOrtho();
					FogOfWarMaterial.mainTexture = source;
					FogOfWarMaterial.SetPass(FogEnabled ? 3 : ((Usage == CameraUsage.Pip) ? 4 : 5));
					GL.Begin(7);
					GL.TexCoord2(0f, 0f);
					GL.Vertex3(0f, 0f, 0.1f);
					GL.TexCoord2(1f, 0f);
					GL.Vertex3(1f, 0f, 0.1f);
					GL.TexCoord2(1f, 1f);
					GL.Vertex3(1f, 1f, 0.1f);
					GL.TexCoord2(0f, 1f);
					GL.Vertex3(0f, 1f, 0.1f);
					GL.End();
					GL.PopMatrix();
					BlurMaterial.SetInteger(ShaderHash._GridSize, BlurGridSize);
					BlurMaterial.SetFloat(ShaderHash._Spread, BlurSpread);
					Graphics.Blit(temporary2, temporary3, BlurMaterial, 0);
					Graphics.Blit(temporary3, temporary4, BlurMaterial, 1);
					FogOfWarMaterial.SetTexture(ShaderHash._EdgeTex, temporary4);
					Graphics.Blit(temporary, destination, FogOfWarMaterial, 6);
					RenderTexture.ReleaseTemporary(temporary2);
					RenderTexture.ReleaseTemporary(temporary3);
					RenderTexture.ReleaseTemporary(temporary4);
					RenderTexture.ReleaseTemporary(temporary);
				}
				else
				{
					Graphics.Blit(source, destination, FogOfWarMaterial, (!FogEnabled) ? ((Usage == CameraUsage.Pip) ? 1 : 2) : 0);
				}
			}
		}
	}
}

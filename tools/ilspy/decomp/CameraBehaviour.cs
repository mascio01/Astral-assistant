using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
	public Camera Camera;

	public FogOfWarBehaviour FogOfWarBehaviour;

	public GrassCameraBehaviour GrassCameraBehaviour;

	public Vector3[] NearFrustumCornersInCamSpace = new Vector3[4];

	public Vector3[] FarFrustumCornersInCamSpace = new Vector3[4];

	public Vector3[] FogFrustumCornersInCamSpace = new Vector3[4];

	public Vector3[] GrassFrustumCornersInCamSpace = new Vector3[4];

	public Vector3[] NearFrustumCorners = new Vector3[4];

	public Vector3[] FogFrustumCorners = new Vector3[4];

	public Vector3[] GrassFrustumCorners = new Vector3[4];

	public Plane[] FrustumPlanes = new Plane[6];

	public Bounds FrustumBounds;

	public virtual void Awake()
	{
		Camera = GetComponent<Camera>();
		FogOfWarBehaviour = GetComponent<FogOfWarBehaviour>();
		GrassCameraBehaviour = GetComponent<GrassCameraBehaviour>();
	}

	public void UpdateFrustum()
	{
		MathUtil.CalculateFrustumPlanes(Camera, FrustumPlanes);
		float z = Mathf.Lerp((FogOfWarBehaviour != null) ? FogOfWarBehaviour.FogEnd : Camera.farClipPlane, Camera.farClipPlane, Session.Instance.GameCamera.FlyCamTransition);
		Camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), Camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, NearFrustumCornersInCamSpace);
		Camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), Camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, FarFrustumCornersInCamSpace);
		Camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), z, Camera.MonoOrStereoscopicEye.Mono, FogFrustumCornersInCamSpace);
		Camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), GrassCameraBehaviour.GrassRangeMax, Camera.MonoOrStereoscopicEye.Mono, GrassFrustumCornersInCamSpace);
		for (int i = 0; i < 4; i++)
		{
			NearFrustumCorners[i] = base.transform.TransformPoint(NearFrustumCornersInCamSpace[i]);
			FogFrustumCorners[i] = base.transform.TransformPoint(FogFrustumCornersInCamSpace[i]);
			GrassFrustumCorners[i] = base.transform.TransformPoint(GrassFrustumCornersInCamSpace[i]);
		}
		FrustumBounds = new Bounds(NearFrustumCorners[0], Vector3.zero);
		for (int j = 1; j < NearFrustumCorners.Length; j++)
		{
			FrustumBounds.Encapsulate(NearFrustumCorners[j]);
		}
		for (int k = 0; k < GrassFrustumCorners.Length; k++)
		{
			FrustumBounds.Encapsulate(GrassFrustumCorners[k]);
		}
	}

	public bool IsInFrustum(Bounds aabb)
	{
		return GeometryUtility.TestPlanesAABB(FrustumPlanes, aabb);
	}

	public virtual void OnPreCull()
	{
		if (GameImpl.Instance != null)
		{
			EnableBackLights();
		}
	}

	public void EnableBackLights()
	{
		for (int i = 0; i < 2; i++)
		{
			GameImpl.Instance.Sun.UnityBackLight[i].enabled = SunFiddler.BackLightEnabled;
			GameImpl.Instance.Sun.UnityBackLight[i].color = (SunFiddler.FullBright ? Color.white : GameImpl.Instance.Sun.LightingSettings.Back);
			GameImpl.Instance.Sun.UnityBackLight[i].intensity = (SunFiddler.FullBright ? 1f : GameImpl.Instance.Sun.LightingSettings.Back.a) * Sun.IntensityFactor * 0.5f;
			GameImpl.Instance.Sun.UnityBackLight[i].transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y + 180f + ((i == 0) ? (-1f) : 1f) * SunFiddler.BackLightAngle, 0f);
		}
	}

	public virtual void OnPostRender()
	{
		if (GameImpl.Instance != null)
		{
			DisableBackLights();
		}
	}

	public void DisableBackLights()
	{
		for (int i = 0; i < 2; i++)
		{
			GameImpl.Instance.Sun.UnityBackLight[i].enabled = false;
		}
	}
}

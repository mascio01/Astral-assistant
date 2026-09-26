using UnityEngine;

public class BFX_BloodSettings : MonoBehaviour
{
	public enum _DecalRenderinMode
	{
		Floor_XZ,
		AverageRayBetwenForwardAndFloor
	}

	public float AnimationSpeed = 1f;

	public float GroundHeight;

	[Range(0f, 1f)]
	public float LightIntensityMultiplier = 1f;

	public bool FreezeDecalDisappearance;

	public _DecalRenderinMode DecalRenderinMode;

	public bool ClampDecalSideSurface;

	private float StartTime;

	public static int BloodDecalsActive;

	private void OnEnable()
	{
		StartTime = Time.time;
		BloodDecalsActive++;
	}

	private void OnDisable()
	{
		BloodDecalsActive--;
	}

	private void Update()
	{
		if (Time.time >= StartTime + 20f / AnimationSpeed)
		{
			Object.Destroy(base.gameObject);
		}
	}
}

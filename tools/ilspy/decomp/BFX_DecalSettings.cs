using UnityEngine;

public class BFX_DecalSettings : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public Transform parent;

	public float TimeHeightMax = 3.1f;

	public float TimeHeightMin = -0.1f;

	[Space]
	public Vector3 TimeScaleMax = Vector3.one;

	public Vector3 TimeScaleMin = Vector3.one;

	[Space]
	public Vector3 TimeOffsetMax = Vector3.zero;

	public Vector3 TimeOffsetMin = Vector3.zero;

	[Space]
	public AnimationCurve TimeByHeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Vector3 startOffset;

	private Vector3 startScale;

	private float timeDelay;

	private Transform t;

	private Transform tParent;

	private BFX_ShaderProperies shaderProperies;

	private Vector3 averageRay;

	private bool isPositionInitialized;

	private Vector3 initializedPosition;

	private void Awake()
	{
		startOffset = base.transform.localPosition;
		startScale = base.transform.localScale;
		t = base.transform;
		tParent = parent.transform;
		shaderProperies = GetComponent<BFX_ShaderProperies>();
		shaderProperies.OnAnimationFinished += ShaderCurve_OnAnimationFinished;
	}

	private void ShaderCurve_OnAnimationFinished()
	{
		GetComponent<Renderer>().enabled = false;
	}

	private void Update()
	{
		if (!isPositionInitialized)
		{
			InitializePosition();
		}
		if (shaderProperies.enabled && initializedPosition.x < float.PositiveInfinity)
		{
			base.transform.position = initializedPosition;
		}
	}

	private void InitializePosition()
	{
		GetComponent<Renderer>().enabled = false;
		float y = parent.position.y;
		float groundHeight = BloodSettings.GroundHeight;
		float y2 = parent.localScale.y;
		float num = TimeHeightMax * y2;
		float num2 = TimeHeightMin * y2;
		if (y - groundHeight >= num || y - groundHeight <= num2)
		{
			GetComponent<MeshRenderer>().enabled = false;
		}
		else
		{
			GetComponent<MeshRenderer>().enabled = true;
		}
		float f = (tParent.position.y - groundHeight) / num;
		f = Mathf.Abs(f);
		Vector3 vector = Vector3.Lerp(TimeScaleMin, TimeScaleMax, f);
		t.localScale = new Vector3(vector.x * startScale.x, startScale.y, vector.z * startScale.z);
		Vector3 vector2 = Vector3.Lerp(TimeOffsetMin, TimeOffsetMax, f);
		t.localPosition = startOffset + vector2;
		t.position = new Vector3(t.position.x, groundHeight + 0.05f, t.position.z);
		timeDelay = TimeByHeight.Evaluate(f);
		shaderProperies.enabled = false;
		Invoke("EnableDecalAnimation", Mathf.Max(0f, timeDelay / BloodSettings.AnimationSpeed));
		if (BloodSettings.DecalRenderinMode == BFX_BloodSettings._DecalRenderinMode.AverageRayBetwenForwardAndFloor)
		{
			averageRay = GetAverageRay(tParent.position + tParent.right * 0.05f, tParent.right);
			float num3 = Mathf.Clamp(Vector3.Angle(Vector3.up, averageRay), -90f, 90f);
			Vector3 eulerAngles = t.localRotation.eulerAngles;
			t.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, (0f - num3) * 0.5f);
			float num4 = Mathf.Abs(num3) / 90f;
			Vector3 localScale = t.localScale;
			localScale.y = Mathf.Lerp(localScale.y, localScale.x * 1.5f, num4);
			t.localScale = localScale;
		}
		if (BloodSettings.ClampDecalSideSurface)
		{
			Shader.EnableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = true;
	}

	private void OnDisable()
	{
		if (BloodSettings.ClampDecalSideSurface)
		{
			Shader.DisableKeyword("CLAMP_SIDE_SURFACE");
		}
		isPositionInitialized = false;
		initializedPosition = Vector3.positiveInfinity;
	}

	private Vector3 GetAverageRay(Vector3 start, Vector3 forward)
	{
		if (Physics.Raycast(start, -forward, out var hitInfo))
		{
			return (hitInfo.normal + Vector3.up).normalized;
		}
		return Vector3.up;
	}

	private void EnableDecalAnimation()
	{
		shaderProperies.enabled = true;
		initializedPosition = base.transform.position;
	}

	private void OnDrawGizmos()
	{
		if (t == null)
		{
			t = base.transform;
		}
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.03f);
		Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.85f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}
}

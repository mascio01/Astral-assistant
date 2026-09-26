using UnityEngine;

public class BFX_ManualAnimationUpdate : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public AnimationCurve AnimationSpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float FramesCount = 99f;

	public float TimeLimit = 3f;

	public float OffsetFrames;

	private float currentTime;

	private Renderer rend;

	private MaterialPropertyBlock propertyBlock;

	private static string _UseCustomTime = "_UseCustomTime";

	private static string _TimeInFrames = "_TimeInFrames";

	private static string _LightIntencity = "_LightIntencity";

	private void Awake()
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		rend = GetComponent<Renderer>();
	}

	private void OnEnable()
	{
		rend.enabled = true;
		rend.GetPropertyBlock(propertyBlock);
		propertyBlock.SetFloat(_UseCustomTime, 1f);
		propertyBlock.SetFloat(_TimeInFrames, 0f);
		rend.SetPropertyBlock(propertyBlock);
		currentTime = 0f;
	}

	private void Update()
	{
		currentTime += Time.deltaTime * BloodSettings.AnimationSpeed;
		if ((double)(currentTime / TimeLimit) > 1.0)
		{
			if (rend.enabled)
			{
				rend.enabled = false;
			}
		}
		else
		{
			float value = Mathf.Ceil(0f - (AnimationSpeed.Evaluate(currentTime / TimeLimit) * FramesCount + OffsetFrames + 1.1f)) / (FramesCount + 1f) + 1f / (FramesCount + 1f);
			rend.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat(_LightIntencity, Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
			propertyBlock.SetFloat(_TimeInFrames, value);
			rend.SetPropertyBlock(propertyBlock);
		}
	}
}

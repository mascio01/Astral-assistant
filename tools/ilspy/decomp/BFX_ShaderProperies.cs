using System;
using UnityEngine;

public class BFX_ShaderProperies : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public float TimeDelay;

	private bool canUpdate;

	private bool isFrized;

	private float startTime;

	private int cutoutPropertyID;

	private int forwardDirPropertyID;

	private float timeLapsed;

	private MaterialPropertyBlock props;

	private Renderer rend;

	public event Action OnAnimationFinished;

	private void Awake()
	{
		props = new MaterialPropertyBlock();
		rend = GetComponent<Renderer>();
		cutoutPropertyID = Shader.PropertyToID("_Cutout");
		forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");
		OnEnable();
	}

	private void OnEnable()
	{
		startTime = Time.time + TimeDelay;
		canUpdate = true;
		GetComponent<Renderer>().enabled = true;
		rend.GetPropertyBlock(props);
		float value = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
		props.SetFloat(cutoutPropertyID, value);
		props.SetVector(forwardDirPropertyID, base.transform.up);
		rend.SetPropertyBlock(props);
	}

	private void OnDisable()
	{
		rend.GetPropertyBlock(props);
		float value = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
		props.SetFloat(cutoutPropertyID, value);
		rend.SetPropertyBlock(props);
		timeLapsed = 0f;
	}

	private void Update()
	{
		if (canUpdate)
		{
			rend.GetPropertyBlock(props);
			float num = ((BloodSettings == null) ? Time.deltaTime : (Time.deltaTime * BloodSettings.AnimationSpeed));
			if (!(BloodSettings != null) || !BloodSettings.FreezeDecalDisappearance || !(timeLapsed / GraphTimeMultiplier > 0.3f))
			{
				timeLapsed += num;
			}
			float value = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
			props.SetFloat(cutoutPropertyID, value);
			if (BloodSettings != null)
			{
				props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
			}
			if (timeLapsed >= GraphTimeMultiplier)
			{
				canUpdate = false;
				this.OnAnimationFinished?.Invoke();
			}
			props.SetVector(forwardDirPropertyID, base.transform.up);
			rend.SetPropertyBlock(props);
		}
	}
}

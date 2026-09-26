using UnityEngine;

public class FireFlicker : MonoBehaviour
{
	public float baseStart;

	public float amplitude = 1f;

	public float phase;

	public float frequency = 0.5f;

	private Color OriginalColor;

	private Light Light;

	private void Start()
	{
		Light = GetComponent<Light>();
		OriginalColor = Light.color;
	}

	private void Update()
	{
		Light.color = OriginalColor * EvalWave();
	}

	private float EvalWave()
	{
		Mathf.Floor((Time.time + phase) * frequency);
		return (2f - Random.value * 0.75f) * amplitude + baseStart;
	}
}

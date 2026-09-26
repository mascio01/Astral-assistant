using UnityEngine;

public class EmissionPulse : MonoBehaviour
{
	public float frequency = 1f;

	public float amplitude = 1f;

	public float baseMult = 0.75f;

	public Material material;

	public Color emissionColor;

	private void Start()
	{
		emissionColor = emissionColor * baseMult * amplitude;
	}

	private void Update()
	{
		float num = (2f + Mathf.Cos(Time.time * frequency)) * amplitude;
		material.SetColor("_EmissionColor", emissionColor * num);
	}
}

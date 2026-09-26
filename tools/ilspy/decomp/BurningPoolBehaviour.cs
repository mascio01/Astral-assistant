using UnityEngine;

public class BurningPoolBehaviour : MonoBehaviour
{
	private ParticleSystem UnityParticleSystem;

	private Light UnityLight;

	private void Start()
	{
		UnityParticleSystem = GetComponent<ParticleSystem>();
		UnityLight = GetComponent<Light>();
	}

	private void Update()
	{
		UnityLight.intensity = 4f * Mathf.Clamp01((float)UnityParticleSystem.particleCount / 16f);
		if (!UnityParticleSystem.IsAlive())
		{
			Object.Destroy(base.gameObject);
		}
	}
}

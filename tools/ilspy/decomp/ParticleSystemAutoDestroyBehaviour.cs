using UnityEngine;

public class ParticleSystemAutoDestroyBehaviour : MonoBehaviour
{
	private ParticleSystem UnityParticleSystem;

	public Character OwnerCharacter;

	private void Start()
	{
		UnityParticleSystem = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		if (!UnityParticleSystem.IsAlive())
		{
			Object.Destroy(base.gameObject);
		}
	}
}

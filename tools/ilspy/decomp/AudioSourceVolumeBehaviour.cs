using UnityEngine;

public class AudioSourceVolumeBehaviour : MonoBehaviour
{
	private AudioSource AudioSource;

	public float BaseVolume = 1f;

	private void Start()
	{
		AudioSource = GetComponent<AudioSource>();
		AudioSource.RealisticRolloff();
	}

	private void Update()
	{
		AudioSource.volume = SoundManager.WorldSoundVolume * BaseVolume;
	}
}

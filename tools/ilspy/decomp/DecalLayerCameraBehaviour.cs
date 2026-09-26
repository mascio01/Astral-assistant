using UnityEngine;

public class DecalLayerCameraBehaviour : MonoBehaviour
{
	private ShadowQuality StoredShadowQuality;

	private void OnPreRender()
	{
		StoredShadowQuality = QualitySettings.shadows;
		QualitySettings.shadows = ShadowQuality.Disable;
	}

	private void OnPostRender()
	{
		QualitySettings.shadows = StoredShadowQuality;
	}
}

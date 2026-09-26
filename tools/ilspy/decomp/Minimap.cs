using UnityEngine;

public class Minimap
{
	public float MaxHeight;

	public static float DefaultHeight = 40f;

	public void OnStart()
	{
		MaxHeight = GameTerrain.Instance.Size / 2;
		Camera unityMinimapCamera = HudBehaviour.Instance.UnityMinimapCamera;
		unityMinimapCamera.farClipPlane = DefaultHeight + 8f;
		unityMinimapCamera.transform.position = new Vector3(0f, DefaultHeight, 0f);
		unityMinimapCamera.transform.LookAt(Vector3.zero, Vector3.back);
	}

	public void Update()
	{
		Camera unityMinimapCamera = HudBehaviour.Instance.UnityMinimapCamera;
		Vector3 position = unityMinimapCamera.transform.position;
		GameCamera gameCamera = Session.Instance.GameCamera;
		position.x = gameCamera.Focus.x;
		position.z = gameCamera.Focus.z;
		position.y = DefaultHeight;
		position.y = Mathf.Clamp(position.y, 16f, MaxHeight);
		Vector3 worldUp = ((!GameImpl.Instance.Settings.MinimapRotationEnabled) ? (-Vector3.forward) : MathUtil.SafeNormalize(MathUtil.ToX0Y(MathUtil.ToXZ(gameCamera.GetForward())), Vector3.back));
		unityMinimapCamera.transform.position = position;
		unityMinimapCamera.transform.LookAt(MathUtil.ToX0Y(MathUtil.ToXZ(position)), worldUp);
	}
}

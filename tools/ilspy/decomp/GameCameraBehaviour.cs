using UnityEngine;

public class GameCameraBehaviour : CameraBehaviour
{
	public override void OnPreCull()
	{
		base.OnPreCull();
		if (Hud.Instance.LocalControlledCharacter != null)
		{
			Hud.Instance.LocalControlledCharacter.GetSightRange(out var fogStart, out var fogEnd);
			FogOfWarBehaviour.FogStart = fogStart;
			FogOfWarBehaviour.FogEnd = fogEnd;
		}
		FogOfWarBehaviour.PlayerPos = Session.Instance.GameCamera.Focus;
		FogOfWarBehaviour.FogAmount = (FogOfWar.DebugFogOfWarEnabled ? (1f - Mathf.Clamp01((Session.Instance.GameCamera.FlyCamTransition - 0.5f) * 2f)) : 0f);
		foreach (TileObject activeMovingUnityObject in Session.Instance.ActiveMovingUnityObjects)
		{
			activeMovingUnityObject.UnityUpdateLayer(pip: false, base.transform.position, Camera.farClipPlane);
		}
	}

	public override void OnPostRender()
	{
		base.OnPostRender();
		if (Session.Instance != null && Session.Instance.State == SessionState.Started)
		{
			Session.Instance.OnPostRender();
			if (InfoScreen.Instance.Transition == 0f)
			{
				HudBehaviour.Instance.UnityMinimapCameraBehaviour.RenderWithoutCamera();
			}
			if (Hud.Instance.Pip.FocusObject != null && !GameImpl.Instance.Settings.PiPBackgroundEnabled && GraphicsDebugMenu.PipCommandBuffers)
			{
				HudBehaviour.Instance.UnityPipCameraBehaviour.RenderWithoutCamera();
			}
		}
	}
}

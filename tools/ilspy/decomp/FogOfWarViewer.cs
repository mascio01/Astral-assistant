using UnityEngine;

public class FogOfWarViewer : DebugPage
{
	public FogOfWarViewer()
		: base(GameImpl.Translate("DEBUG_FogOfWarViewer"))
	{
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		int size = GameTerrain.Instance.Size;
		GUI.DrawTexture(new Rect(0f, 0f, size, size), GameTerrain.Instance.MinimapTex);
		GUI.DrawTexture(new Rect(0f, 0f, size, size), GameTerrain.Instance.FogOfWar.GetCurTex());
	}
}

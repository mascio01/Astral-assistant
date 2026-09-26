using UnityEngine;

public class GrassMapViewer : DebugPage
{
	public GrassMapViewer()
		: base(GameImpl.Translate("DEBUG_GrassMapViewer"))
	{
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		int size = GameTerrain.Instance.Size;
		GUI.DrawTexture(new Rect(0f, 0f, size, size), GameTerrain.Instance.GrassMap.Tex);
	}
}

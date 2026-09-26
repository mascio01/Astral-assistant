using UnityEngine;

public class HeightMapViewer : DebugPage
{
	public HeightMapViewer()
		: base(GameImpl.Translate("DEBUG_HeightMapViewer"))
	{
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		int size = GameTerrain.Instance.Size;
		GUI.DrawTexture(new Rect(0f, 0f, size, size), GameTerrain.Instance.HeightMapTex);
	}
}

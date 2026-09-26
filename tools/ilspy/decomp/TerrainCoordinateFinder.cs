using UnityEngine;

public class TerrainCoordinateFinder : DebugPage
{
	private TerrainCoord HitTile;

	private string LabelText = "Invalid";

	public TerrainCoordinateFinder()
		: base(GameImpl.Translate("DEBUG_CoordinateFinder"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (instance3.IsPressed(InputFunction.MainAction))
		{
			RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult.HitObject == instance2)
			{
				HitTile = raycastResult.Tile;
				LabelText = "(" + HitTile.x + ", " + HitTile.y + ")";
			}
		}
		if (!GameTerrain.Instance.IsTileOutsideBounds(HitTile.x, HitTile.y))
		{
			Debug.DrawLine(GameTerrain.Instance.GetTileCentrePos(HitTile), GameTerrain.Instance.GetTileCentrePos(HitTile) + new Vector3(0f, 10f, 0f), Color.red);
		}
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		GUI.Box(new Rect(10f, 10f, 300f, 50f), TitleString);
		GUI.Label(new Rect(30f, 30f, 200f, 30f), LabelText);
	}

	public override void OnPostRenderImpl()
	{
		GameTerrain instance = GameTerrain.Instance;
		if (!instance.IsTileOutsideBounds(HitTile.x, HitTile.y))
		{
			instance.GetTileBox(HitTile, out var centre, out var extents);
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawBox(centre, extents + new Vector3(0f, 1f, 0f), Color.red);
			DebugGraphics.EndDrawLines();
		}
	}

	public override void OnDrawGizmosImpl()
	{
		GameTerrain instance = GameTerrain.Instance;
		if (!instance.IsTileOutsideBounds(HitTile.x, HitTile.y))
		{
			instance.GetTileBox(HitTile, out var centre, out var extents);
			Gizmos.DrawWireCube(centre, extents + new Vector3(0f, 1f, 0f));
		}
	}
}

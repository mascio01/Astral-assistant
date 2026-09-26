using System.Text;
using UnityEngine;

public class TerrainFlattener : DebugMenu
{
	private bool _drawing;

	private TerrainCoord _lastDrawTile;

	private float _baseHeight;

	private float _radius = 4f;

	public TerrainFlattener()
		: base(GameImpl.Translate("DEBUG_Flattener"))
	{
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Radius"), 0f, 64f, () => _radius, delegate(float r)
		{
			_radius = r;
		}));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRivers"), typeof(TerrainHeightEditor), "LockRivers"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRoads"), typeof(TerrainHeightEditor), "LockRoads"));
	}

	private void BuildRadiusString(ref StringBuilder value)
	{
		value.Append(_radius);
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
				if (!_drawing)
				{
					Vector3 hitPosition = raycastResult.GetHitPosition();
					_baseHeight = instance2.GetTileHeightAtPos(hitPosition.x, hitPosition.z);
				}
				if (!_drawing || _lastDrawTile != raycastResult.Tile)
				{
					instance2.Flatten(raycastResult.Tile, _baseHeight, _radius, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
					_lastDrawTile = raycastResult.Tile;
				}
				Session.Instance.AchievementsEnabled = false;
				_drawing = true;
			}
		}
		else
		{
			_drawing = false;
		}
	}
}

using System.Text;
using UnityEngine;

public class TerrainDuneEditor : DebugMenu
{
	private bool _drawing;

	private TerrainCoord _lastDrawTile;

	private float _baseHeight;

	private float _radius = 10f;

	private float _height = 2f;

	public TerrainDuneEditor()
		: base(GameImpl.Translate("DEBUG_DuneEditor"))
	{
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Radius"), DecrementRadius, IncrementRadius, BuildRadiusString));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Height"), DecrementHeight, IncrementHeight, BuildHeightString));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRivers"), typeof(TerrainHeightEditor), "LockRivers"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRoads"), typeof(TerrainHeightEditor), "LockRoads"));
	}

	private void DecrementRadius()
	{
		if (_radius > 0f)
		{
			_radius -= 1f;
		}
	}

	private void IncrementRadius()
	{
		_radius += 1f;
	}

	private void BuildRadiusString(ref StringBuilder value)
	{
		value.Append(_radius);
	}

	private void DecrementHeight()
	{
		if (_height > 0f)
		{
			_height -= 0.125f;
		}
	}

	private void IncrementHeight()
	{
		_height += 0.125f;
	}

	private void BuildHeightString(ref StringBuilder value)
	{
		value.Append(_height);
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
					instance2.DrawDune(raycastResult.Tile, _baseHeight, _radius, _height, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
					_lastDrawTile = raycastResult.Tile;
				}
				_drawing = true;
				Session.Instance.AchievementsEnabled = false;
			}
		}
		else
		{
			_drawing = false;
		}
	}
}

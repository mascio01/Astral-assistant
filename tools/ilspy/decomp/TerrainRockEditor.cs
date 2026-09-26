using System.Text;
using UnityEngine;

public class TerrainRockEditor : DebugMenu
{
	private bool _drawing;

	private TerrainCoord _lastDrawTile;

	private float _baseHeight;

	private float _radius = 2f;

	private float _scale = 1f;

	public TerrainRockEditor()
		: base(GameImpl.Translate("DEBUG_RockEditor"))
	{
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Radius"), DecrementRadius, IncrementRadius, BuildRadiusString));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Scale"), DecrementScale, IncrementScale, BuildScaleString));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRivers"), typeof(TerrainHeightEditor), "LockRivers"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_LockRoads"), typeof(TerrainHeightEditor), "LockRoads"));
	}

	private void DecrementRadius()
	{
		if (_radius > 0f)
		{
			_radius -= 0.125f;
		}
	}

	private void IncrementRadius()
	{
		_radius += 0.125f;
	}

	private void BuildRadiusString(ref StringBuilder value)
	{
		value.Append(_radius);
	}

	private void DecrementScale()
	{
		if (_scale > 0f)
		{
			_scale -= 0.125f;
		}
	}

	private void IncrementScale()
	{
		_scale += 0.125f;
	}

	private void BuildScaleString(ref StringBuilder value)
	{
		value.Append(_scale);
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
					instance2.DrawRock(raycastResult.Tile, _baseHeight, _radius, _scale, TerrainHeightEditor.LockRivers, TerrainHeightEditor.LockRoads);
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

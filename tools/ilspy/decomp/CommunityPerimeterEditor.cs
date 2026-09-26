using System.Collections.Generic;
using UnityEngine;

public class CommunityPerimeterEditor : DebugMenu
{
	private Community _community;

	private bool WantRepopulate;

	private bool Dragging;

	private int SelectedIndex = -1;

	public CommunityPerimeterEditor(Community community)
		: base(GameImpl.Translate("DEBUG_PerimeterEditor"))
	{
		_community = community;
		WantRepopulate = true;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (WantRepopulate)
		{
			WantRepopulate = false;
			Populate();
		}
		if (Dragging)
		{
			if (instance3.IsPressed(InputFunction.MainAction))
			{
				RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
				if (raycastResult.HitObject == instance2 && SelectedIndex >= 0 && SelectedIndex < _community.Perimeter.Count)
				{
					_community.Perimeter[SelectedIndex] = raycastResult.Tile;
					OnPerimeterChanged();
				}
			}
			else
			{
				Dragging = false;
			}
		}
		else if (instance3.IsJustPressed(InputFunction.MainAction, capture: false))
		{
			instance3.Capture(InputFunction.MainAction, untilReleased: false);
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject != instance2)
			{
				return;
			}
			if (_community.Perimeter == null)
			{
				_community.Perimeter = new List<TerrainCoord>();
			}
			int num = _community.Perimeter.IndexOf(raycastResult2.Tile);
			if (num == -1)
			{
				if (SelectedIndex == -1)
				{
					SelectedIndex = _community.Perimeter.Count;
					_community.Perimeter.Add(raycastResult2.Tile);
				}
				else if (SelectedIndex < _community.Perimeter.Count)
				{
					_community.Perimeter.Insert(SelectedIndex + 1, raycastResult2.Tile);
					SelectedIndex++;
				}
				OnPerimeterChanged();
			}
			else
			{
				SelectedIndex = num;
				Dragging = true;
			}
		}
		else if (instance3.IsJustPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult3 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult3.HitObject == instance2 && _community.Perimeter != null)
			{
				DeleteVertex(_community.Perimeter.IndexOf(raycastResult3.Tile));
			}
		}
	}

	public void Populate()
	{
		Items.Clear();
		if (_community.Perimeter == null)
		{
			return;
		}
		for (int i = 0; i < _community.Perimeter.Count; i++)
		{
			int scopedIndex = i;
			Items.Add(new DebugMenuString(i.ToString(), () => (scopedIndex >= _community.Perimeter.Count) ? string.Empty : _community.Perimeter[scopedIndex].ToString(), delegate(string v)
			{
				if (scopedIndex < _community.Perimeter.Count)
				{
					_community.Perimeter[scopedIndex] = StringUtil.ParseTerrainCoord(v);
				}
				OnPerimeterChanged();
			}));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), delegate
			{
				DeleteVertex(scopedIndex);
			}));
		}
	}

	private void DeleteVertex(int i)
	{
		if (i >= 0 && i < _community.Perimeter.Count)
		{
			if (i == SelectedIndex)
			{
				SelectedIndex = -1;
			}
			else if (SelectedIndex > 1)
			{
				SelectedIndex--;
			}
			_community.Perimeter.RemoveAt(i);
			OnPerimeterChanged();
		}
	}

	private void OnPerimeterChanged()
	{
		WantRepopulate = true;
		_community.BaseRect = new TerrainRect(_community.Perimeter);
		Session.Instance.AchievementsEnabled = false;
	}

	public override void OnPostRenderImpl()
	{
		base.OnPostRenderImpl();
		DrawPerimeter(_community, SelectedIndex);
	}

	public static void DrawPerimeter(Community community, int selectedIndex)
	{
		if (community.BaseRect.VerticesArea <= 0 || !community.IsAISettlement())
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		Vector3 vector = Vector3.up * 0.1f;
		DebugGraphics.DrawRectOnGround(community.BaseRect, Color.yellow);
		DebugGraphics.DrawRectOnGround(community.PrisonRect, Color.black);
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		if (community.Perimeter != null && community.Perimeter.Count > 1)
		{
			for (int i = 0; i < community.Perimeter.Count; i++)
			{
				DebugGraphics.DrawBox(instance.GetTileCentrePos(community.Perimeter[i]) + vector, Vector3.one * 0.1f, (i == selectedIndex) ? Color.white : Color.red);
				DebugGraphics.DrawLine(instance.GetTileCentrePos(community.Perimeter[i]) + vector, instance.GetTileCentrePos(community.Perimeter[(i + 1) % community.Perimeter.Count]) + vector, Color.red);
			}
		}
		DebugGraphics.EndDrawLines();
	}
}

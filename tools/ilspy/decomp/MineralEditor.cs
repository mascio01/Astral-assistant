using UnityEngine;

public class MineralEditor : DebugMenu
{
	private MineralType CurrentMineralType;

	public MineralEditor()
		: base(GameImpl.Translate("DEBUG_MineralEditor"))
	{
		for (int i = 0; i < 4; i++)
		{
			MineralType mineralType = (MineralType)i;
			Items.Add(new DebugMenuItemToggle(mineralType.ToString(), () => CurrentMineralType == mineralType, delegate
			{
				CurrentMineralType = mineralType;
			}));
		}
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
				instance2.SetMineralType(raycastResult.Tile, CurrentMineralType, byte.MaxValue);
				Session.Instance.AchievementsEnabled = false;
			}
		}
		if (instance3.IsPressed(InputFunction.Clear))
		{
			RaycastResult raycastResult2 = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
			if (raycastResult2.HitObject == instance2)
			{
				instance2.SetMineralType(raycastResult2.Tile, CurrentMineralType, 0);
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	public override void OnPostRenderImpl()
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord terrainCoord = instance.GetTileCoordForPos(Session.Instance.GameCamera.Focus) / 4;
		Vector3 vector = new Vector3(0f, 0.1f, 0f);
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		for (int i = terrainCoord.x - 8; i < terrainCoord.x + 8; i++)
		{
			for (int j = terrainCoord.y - 8; j < terrainCoord.y + 8; j++)
			{
				if (instance.IsMineralSquareOutsideBounds(i, j))
				{
					continue;
				}
				bool flag = true;
				int num = (int)CurrentMineralType;
				while (num >= 0)
				{
					if (flag || num != (int)CurrentMineralType)
					{
						MineralType mineralType = (MineralType)num;
						if (instance.HasRichMineralDeposits(new TerrainCoord(i, j) * 4, mineralType))
						{
							TerrainCoord terrainCoord2 = new TerrainCoord(i * 4, j * 4);
							TerrainCoord terrainCoord3 = new TerrainCoord((i + 1) * 4, (j + 1) * 4);
							Color mineralCol = GameTerrain.MinimapSettings.GetMineralCol(mineralType);
							DebugGraphics.DrawLine(instance.GetVertexPos(terrainCoord2.x, terrainCoord2.y) + vector, instance.GetVertexPos(terrainCoord3.x, terrainCoord2.y) + vector, mineralCol);
							DebugGraphics.DrawLine(instance.GetVertexPos(terrainCoord2.x, terrainCoord2.y) + vector, instance.GetVertexPos(terrainCoord2.x, terrainCoord3.y) + vector, mineralCol);
							DebugGraphics.DrawLine(instance.GetVertexPos(terrainCoord2.x, terrainCoord3.y) + vector, instance.GetVertexPos(terrainCoord3.x, terrainCoord3.y) + vector, mineralCol);
							DebugGraphics.DrawLine(instance.GetVertexPos(terrainCoord3.x, terrainCoord2.y) + vector, instance.GetVertexPos(terrainCoord3.x, terrainCoord3.y) + vector, mineralCol);
							break;
						}
					}
					num = (flag ? 3 : (num - 1));
					flag = false;
				}
			}
		}
		DebugGraphics.EndDrawLines();
	}
}

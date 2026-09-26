using System;
using UnityEngine;

public class VisibleTilesCache
{
	private TerrainCoord Centre;

	private TerrainCoord NewCentre;

	private long[] VisibleTiles = new long[64];

	private long[] NewVisibleTiles = new long[64];

	private int FogStart;

	private int FogEnd;

	private int NewFogStart;

	private int NewFogEnd;

	public float Score;

	public int MarkedDirtyFrame;

	private static float VisRayDestHeight = HumanAppearance.MaleDefaultEyeHeight;

	private bool IsVisible(int offsetX, int offsetY)
	{
		return (VisibleTiles[offsetY + 32] & (1L << offsetX + 32)) != 0;
	}

	private int CalcAmountAtOffset(int offsetX, int offsetY)
	{
		float magnitude = new Vector2(offsetX, offsetY).magnitude;
		return (int)((1f - Mathf.Clamp01((magnitude - (float)FogStart) / (float)(FogEnd - FogStart))) * 255f);
	}

	public void RemoveRefs(Character character)
	{
		FogOfWar fogOfWar = GameTerrain.Instance.FogOfWar;
		fogOfWar.DirtyVisibleTilesCaches.Remove(character);
		fogOfWar.TryFinishThreadedCalc(force: true);
		lock (FogOfWar.TileCol)
		{
			for (int i = -FogEnd; i <= FogEnd; i++)
			{
				for (int j = -FogEnd; j <= FogEnd; j++)
				{
					if (IsVisible(i, j))
					{
						fogOfWar.RemoveRef(Centre.x + i, Centre.y + j, CalcAmountAtOffset(i, j));
					}
				}
			}
		}
		fogOfWar.NeedCopyRects.Add(new TerrainRect(Centre - new TerrainCoord(FogEnd, FogEnd), Centre + new TerrainCoord(FogEnd, FogEnd)));
		Array.Clear(VisibleTiles, 0, VisibleTiles.Length);
		FogStart = (FogEnd = 0);
	}

	public void StartThreadedReCalculate(TerrainCoord centre, int fogStart, int fogEnd)
	{
		NewCentre = centre;
		NewFogStart = Math.Min(fogStart, fogEnd - 1);
		NewFogEnd = fogEnd;
		GameImpl.Instance.UpdateThreadPool.AddTask(ReCalculateOnThread, null, null, TaskPriority.Medium);
	}

	public void SynchronousRecalculate(TerrainCoord centre, int fogStart, int fogEnd)
	{
		NewCentre = centre;
		NewFogStart = Math.Min(fogStart, fogEnd - 1);
		NewFogEnd = fogEnd;
		ReCalculate();
	}

	private void ReCalculate()
	{
		GameTerrain instance = GameTerrain.Instance;
		FogOfWar fogOfWar = instance.FogOfWar;
		TerrainRect newRect = new TerrainRect(Centre - new TerrainCoord(FogEnd, FogEnd), Centre + new TerrainCoord(FogEnd, FogEnd)).Include(new TerrainRect(NewCentre - new TerrainCoord(NewFogEnd, NewFogEnd), NewCentre + new TerrainCoord(NewFogEnd, NewFogEnd)));
		fogOfWar.UpdateNewTileRefs(newRect);
		float num = (float)NewFogEnd + 0.5f;
		Vector3 vector = instance.GetTileCentrePos(NewCentre) + new Vector3(0f, HumanAppearance.MaleDefaultEyeHeight, 0f);
		Array.Clear(NewVisibleTiles, 0, NewVisibleTiles.Length);
		for (int i = -NewFogEnd; i <= NewFogEnd; i++)
		{
			for (int j = -NewFogEnd; j <= NewFogEnd; j++)
			{
				TerrainCoord tile = NewCentre + new TerrainCoord(i, j);
				if (!instance.IsTileOutsideBounds(tile.x, tile.y))
				{
					Vector3 vector2 = instance.GetTileCentrePos(tile) + new Vector3(0f, VisRayDestHeight, 0f) - vector;
					float magnitude = vector2.magnitude;
					if (!(magnitude > num) && (!fogOfWar.WantRaycasts || magnitude < 0.5f || instance.RayCastFromVisibleTilesThread(new Ray(vector, vector2 / magnitude), magnitude, 0, null, null).HitObject == null))
					{
						NewVisibleTiles[j + 32] |= 1L << i + 32;
					}
				}
			}
		}
		lock (FogOfWar.TileCol)
		{
			for (int k = -FogEnd; k <= FogEnd; k++)
			{
				for (int l = -FogEnd; l <= FogEnd; l++)
				{
					if (IsVisible(k, l))
					{
						fogOfWar.RemoveRefOnThread(Centre.x + k, Centre.y + l, CalcAmountAtOffset(k, l));
					}
				}
			}
			Centre = NewCentre;
			FogStart = NewFogStart;
			FogEnd = NewFogEnd;
			Array.Copy(NewVisibleTiles, VisibleTiles, 64);
			for (int m = -FogEnd; m <= FogEnd; m++)
			{
				for (int n = -FogEnd; n <= FogEnd; n++)
				{
					if (IsVisible(m, n))
					{
						fogOfWar.AddRefOnThread(Centre.x + m, Centre.y + n, CalcAmountAtOffset(m, n));
					}
				}
			}
		}
	}

	private void ReCalculateOnThread(BaseTaskData data)
	{
		FogOfWar fogOfWar = GameTerrain.Instance.FogOfWar;
		ReCalculate();
		fogOfWar.FinishedEvent.Set();
	}
}

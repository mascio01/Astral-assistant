using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FogOfWar
{
	private class SortCharactersByScoreAscending : IComparer<Character>
	{
		int IComparer<Character>.Compare(Character a, Character b)
		{
			if (a.VisibleTilesCache.Score > b.VisibleTilesCache.Score)
			{
				return 1;
			}
			if (a.VisibleTilesCache.Score < b.VisibleTilesCache.Score)
			{
				return -1;
			}
			return 0;
		}
	}

	private const byte MaxExplored = 96;

	private int Size;

	public static byte[] Explored;

	public static byte[] NewExplored;

	public static int[] TileRefs;

	public static int[] NewTileRefs;

	public static ByteMap TileCol = new ByteMap();

	private int CurTex;

	private Texture2D[] Tex = new Texture2D[2];

	public static Texture2D AllVisibleTex;

	private bool Dirty;

	public VisibleTilesCache ThreadCalcInProgress;

	public int ThreadCalcFrame;

	public bool WantRaycasts;

	public List<Character> DirtyVisibleTilesCaches = new List<Character>();

	public List<TerrainRect> NeedCopyRects = new List<TerrainRect>();

	public ManualResetEvent FinishedEvent = new ManualResetEvent(initialState: true);

	private int HasWorldMapQuadrants;

	public bool HasRadio;

	public static bool NoFogOfWar = false;

	private static string UpdateTextureStr = "UpdateTexture";

	private static SortCharactersByScoreAscending DirtyVisibleTileCacheSorter = new SortCharactersByScoreAscending();

	public static bool DebugFogOfWarEnabled = true;

	public FogOfWar(GameTerrain terrain)
	{
		DebugFogOfWarEnabled = !Session.Instance.Editor && !NoFogOfWar;
		Dirty = true;
	}

	public void InitSize(int size)
	{
		Size = size;
		TileCol.InitSize(size);
		NeedCopyRects.Clear();
	}

	public void Init()
	{
	}

	public void Unload()
	{
		TryFinishThreadedCalc(force: true);
		DirtyVisibleTilesCaches.Clear();
		Array.Clear(Explored, 0, Explored.Length);
		Array.Clear(NewExplored, 0, NewExplored.Length);
		Array.Clear(TileRefs, 0, TileRefs.Length);
		Array.Clear(NewTileRefs, 0, NewTileRefs.Length);
		Array.Clear(TileCol.GetColMap(), 0, TileCol.GetColMap().Length);
	}

	public void UnityInit()
	{
		for (int i = 0; i < Tex.Length; i++)
		{
			Tex[i] = new Texture2D(Size, Size, TextureFormat.Alpha8, mipChain: false);
		}
	}

	public void UnityDelete()
	{
		for (int i = 0; i < Tex.Length; i++)
		{
			UnityEngine.Object.Destroy(Tex[i]);
		}
	}

	public static void LoadContent()
	{
		AllVisibleTex = new Texture2D(2, 2, TextureFormat.Alpha8, mipChain: false);
		Color32[] array = new Color32[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = new Color32(0, 0, 0, byte.MaxValue);
		}
		AllVisibleTex.SetPixels32(array);
		AllVisibleTex.Apply();
	}

	public static void UnloadContent()
	{
		UnityEngine.Object.Destroy(AllVisibleTex);
	}

	public void Reflect(Reflector reflector, int size)
	{
		for (int i = 0; i < size * size; i++)
		{
			reflector.Add(ref Explored[i]);
		}
		if (reflector.IsDeserialising)
		{
			Array.Copy(Explored, NewExplored, size * size);
		}
		if (reflector.Version < 271)
		{
			bool value = false;
			reflector.AddAfter(ref value, 4);
			HasWorldMapQuadrants = (value ? 15 : 0);
		}
		else
		{
			reflector.Add(ref HasWorldMapQuadrants);
		}
		reflector.AddAfter(ref HasRadio, 47);
		if (reflector.IsDeserialising)
		{
			byte[] colMap = TileCol.GetColMap();
			for (int j = 0; j < size * size; j++)
			{
				colMap[j] = (byte)(IsTileRevealedByMap(j) ? 96 : Explored[j]);
			}
		}
	}

	public bool IsTileExplored(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Size || y >= Size)
		{
			return false;
		}
		int num = x + y * Size;
		if (Explored[num] <= 0)
		{
			return IsTileRevealedByMap(num);
		}
		return true;
	}

	public bool IsTileExploredNotIncludingMap(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Size || y >= Size)
		{
			return false;
		}
		int num = x + y * Size;
		return Explored[num] > 0;
	}

	public bool IsAnyTileInRectExplored(TerrainCoord min, TerrainCoord max)
	{
		for (int i = min.x; i <= max.x; i++)
		{
			for (int j = min.y; j <= max.y; j++)
			{
				if (i >= 0 && j >= 0 && i < Size && j < Size)
				{
					int num = i + j * Size;
					if (Explored[num] > 0 || IsTileRevealedByMap(num))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsAnyTileInRectCornersExplored(TerrainCoord min, TerrainCoord max)
	{
		if (IsTileExplored(min.x, min.y))
		{
			return true;
		}
		if (IsTileExplored(max.x, min.y))
		{
			return true;
		}
		if (IsTileExplored(min.x, max.y))
		{
			return true;
		}
		if (IsTileExplored(max.x, max.y))
		{
			return true;
		}
		return false;
	}

	public bool IsTileVisible(int x, int y)
	{
		return TileRefs[x + y * Size] > 0;
	}

	public bool IsAnyTileInRectVisible(TerrainCoord min, TerrainCoord max)
	{
		for (int i = min.x; i <= max.x; i++)
		{
			for (int j = min.y; j <= max.y; j++)
			{
				if (TileRefs[i + j * Size] > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyTileInRectCornersVisible(TerrainCoord min, TerrainCoord max)
	{
		if (TileRefs[min.x + min.y * Size] > 0)
		{
			return true;
		}
		if (TileRefs[max.x + min.y * Size] > 0)
		{
			return true;
		}
		if (TileRefs[min.x + max.y * Size] > 0)
		{
			return true;
		}
		if (TileRefs[max.x + max.y * Size] > 0)
		{
			return true;
		}
		return false;
	}

	public void UpdateTexture()
	{
		if (!Dirty)
		{
			return;
		}
		using (new UnityProfileMarker(UpdateTextureStr))
		{
			if (Monitor.TryEnter(TileCol))
			{
				int num = (CurTex + 1) % Tex.Length;
				Tex[num].SetPixelData(TileCol.GetColMap(), 0);
				Tex[num].Apply();
				CurTex = num;
				Dirty = false;
				Monitor.Exit(TileCol);
			}
		}
	}

	public void UpdateHasWorldMap()
	{
		Community playerCommunity = Session.Instance.CommunityManager.PlayerCommunity;
		HasRadio = playerCommunity.HasAnyInventoryItemsOfClass(typeof(Radio));
		int num = playerCommunity?.GetWorldMapQuadrants() ?? 0;
		if (num == HasWorldMapQuadrants)
		{
			return;
		}
		HasWorldMapQuadrants = num;
		lock (TileCol)
		{
			for (int i = 0; i < Size * Size; i++)
			{
				UpdateTileCol(i);
			}
		}
	}

	public void AddRefOnThread(int x, int y, int amount)
	{
		int num = x + y * Size;
		NewTileRefs[num] += amount;
		NewExplored[num] = (byte)MathUtil.Clamp(NewTileRefs[num], NewExplored[num], 96);
		UpdateTileColOnThread(num);
	}

	public void RemoveRefOnThread(int x, int y, int amount)
	{
		int num = x + y * Size;
		if (amount > NewTileRefs[num])
		{
			Debug.LogError("RemoveRefOnThread error at x: " + x + ", y: " + y + ", was: " + NewTileRefs[num] + ", subtracting: " + amount);
			amount = NewTileRefs[num];
		}
		NewTileRefs[num] -= amount;
		UpdateTileColOnThread(num);
	}

	public void RemoveRef(int x, int y, int amount)
	{
		int num = x + y * Size;
		if (amount > TileRefs[num])
		{
			Debug.LogError("RemoveRef error at x: " + x + ", y: " + y + ", was: " + TileRefs[num] + ", subtracting: " + amount);
			amount = TileRefs[num];
		}
		TileRefs[num] -= amount;
		UpdateTileCol(num);
	}

	private bool IsTileRevealedByMap(int i)
	{
		int num = i % Size;
		int num2 = i / Size;
		num >>= 9;
		num2 >>= 9;
		int num3 = Size >> 9;
		return (HasWorldMapQuadrants & (1 << num + num2 * num3)) != 0;
	}

	private void UpdateTileColOnThread(int i)
	{
		TileCol.GetColMap()[i] = (byte)MathUtil.Clamp(NewTileRefs[i], IsTileRevealedByMap(i) ? 96 : NewExplored[i], 255);
		Dirty = true;
	}

	private void UpdateTileCol(int i)
	{
		TileCol.GetColMap()[i] = (byte)MathUtil.Clamp(TileRefs[i], IsTileRevealedByMap(i) ? 96 : Explored[i], 255);
		Dirty = true;
	}

	public void MarkVisibleTilesCacheDirty(Character character)
	{
		if (!DirtyVisibleTilesCaches.Contains(character))
		{
			DirtyVisibleTilesCaches.Add(character);
			character.VisibleTilesCache.MarkedDirtyFrame = Session.Instance.Frame;
		}
	}

	public void StartThreadedCalc(Character character)
	{
		DirtyVisibleTilesCaches.Remove(character);
		ThreadCalcInProgress = character.VisibleTilesCache;
		ThreadCalcFrame = 0;
		FinishedEvent.Reset();
		WantRaycasts = !Session.Instance.IsInMultiplayerGame() && DirtyVisibleTilesCaches.Count < 5;
		character.GetSightRange(out var fogStart, out var fogEnd);
		character.VisibleTilesCache.StartThreadedReCalculate(character.Tile, fogStart, fogEnd);
	}

	public void UpdateNewTileRefs(TerrainRect newRect)
	{
		for (int i = 0; i < NeedCopyRects.Count; i++)
		{
			TerrainRect terrainRect = NeedCopyRects[i];
			GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref terrainRect.min, ref terrainRect.max);
			for (int j = terrainRect.min.x; j <= terrainRect.max.x; j++)
			{
				for (int k = terrainRect.min.y; k <= terrainRect.max.y; k++)
				{
					int num = j + k * Size;
					NewTileRefs[num] = TileRefs[num];
					NewExplored[num] = Explored[num];
				}
			}
		}
		NeedCopyRects.Clear();
		NeedCopyRects.Add(newRect);
	}

	public bool TryFinishThreadedCalc(bool force)
	{
		if (ThreadCalcInProgress == null)
		{
			return true;
		}
		if (force)
		{
			FinishedEvent.WaitOne(-1);
		}
		else if (Session.Instance.IsInMultiplayerGame())
		{
			ThreadCalcFrame++;
			if (ThreadCalcFrame < 2)
			{
				return false;
			}
			FinishedEvent.WaitOne(-1);
		}
		else if (!FinishedEvent.WaitOne(0))
		{
			return false;
		}
		MathUtil.Swap(ref NewTileRefs, ref TileRefs);
		MathUtil.Swap(ref NewExplored, ref Explored);
		ThreadCalcInProgress = null;
		ThreadCalcFrame = 0;
		return true;
	}

	public void DeterministicUpdate()
	{
		if (!TryFinishThreadedCalc(force: false) || DirtyVisibleTilesCaches.Count == 0)
		{
			return;
		}
		Community playerCommunity = Session.Instance.CommunityManager.PlayerCommunity;
		foreach (Character dirtyVisibleTilesCache in DirtyVisibleTilesCaches)
		{
			dirtyVisibleTilesCache.VisibleTilesCache.Score = (dirtyVisibleTilesCache.DirectControlled ? (-30f) : float.MaxValue);
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling && playerRecord.PlayerCharacter != null)
				{
					dirtyVisibleTilesCache.VisibleTilesCache.Score = Math.Min(dirtyVisibleTilesCache.VisibleTilesCache.Score, MathUtil.ToXZ(playerRecord.PlayerCharacter.Position - dirtyVisibleTilesCache.Position).magnitude);
				}
			}
			dirtyVisibleTilesCache.VisibleTilesCache.Score -= Session.Instance.Frame - dirtyVisibleTilesCache.VisibleTilesCache.MarkedDirtyFrame;
			if (dirtyVisibleTilesCache.Community != playerCommunity)
			{
				dirtyVisibleTilesCache.VisibleTilesCache.Score += 1000f;
			}
		}
		DirtyVisibleTilesCaches.InsertionSort(DirtyVisibleTileCacheSorter);
		StartThreadedCalc(DirtyVisibleTilesCaches[0]);
	}

	public void InitOnThread()
	{
		using (new StopWatchMarker("Calc Visible Tiles", logTimes: true))
		{
			foreach (Character dirtyVisibleTilesCache in DirtyVisibleTilesCaches)
			{
				dirtyVisibleTilesCache.GetSightRange(out var fogStart, out var fogEnd);
				dirtyVisibleTilesCache.VisibleTilesCache.SynchronousRecalculate(dirtyVisibleTilesCache.Tile, fogStart, fogEnd);
				MathUtil.Swap(ref NewTileRefs, ref TileRefs);
				MathUtil.Swap(ref NewExplored, ref Explored);
			}
		}
	}

	public Texture2D GetCurTex()
	{
		if (!DebugFogOfWarEnabled)
		{
			return AllVisibleTex;
		}
		return Tex[CurTex];
	}

	public static bool GetDebugFogOfWarEnabled()
	{
		return DebugFogOfWarEnabled;
	}

	public static void SetDebugFogOfWarEnabled(bool on)
	{
		DebugFogOfWarEnabled = on;
	}
}

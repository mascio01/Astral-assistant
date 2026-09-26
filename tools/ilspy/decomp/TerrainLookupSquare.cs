using System.Collections.Generic;

public struct TerrainLookupSquare
{
	private int ContentsIndex;

	public int OwnerCommunityId;

	public List<TileObject> Objects
	{
		get
		{
			if (ContentsIndex == 0)
			{
				return null;
			}
			return GameTerrain.Instance.TileContentsManager.GetContents(ContentsIndex - 1).Objects;
		}
	}

	public void AddObject(TileObject obj)
	{
		TileContentsManager tileContentsManager = GameTerrain.Instance.TileContentsManager;
		if (ContentsIndex == 0)
		{
			ContentsIndex = tileContentsManager.CreateContents() + 1;
		}
		tileContentsManager.GetContents(ContentsIndex - 1).Objects.Add(obj);
	}

	public void RemoveObject(TileObject obj)
	{
		TileContentsManager tileContentsManager = GameTerrain.Instance.TileContentsManager;
		TileContents contents = tileContentsManager.GetContents(ContentsIndex - 1);
		contents.Objects.Remove(obj);
		if (contents.Objects.Count == 0)
		{
			tileContentsManager.DeleteContents(ContentsIndex - 1);
			ContentsIndex = 0;
		}
	}
}

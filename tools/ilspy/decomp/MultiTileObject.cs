using UnityEngine;

public abstract class MultiTileObject : TileObject
{
	public string UniqueID;

	public bool InTerrain;

	public TerrainCoord MinTile;

	public TerrainCoord MaxTile;

	public Bounds BoundingBox;

	public Matrix4x4 World = Matrix4x4.identity;

	public Vector3 Right => World.Right();

	public Vector3 Up => World.Up();

	public Vector3 Forward => World.Forward();

	public override string GetUniqueID()
	{
		return UniqueID;
	}

	public override void SetUniqueID(string id)
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		UniqueID = id;
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override void Init()
	{
		base.Init();
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override void Delete()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		base.Delete();
	}

	public override Bounds GetBoundingBox()
	{
		return BoundingBox;
	}

	public override TerrainCoord GetMinTile()
	{
		return MinTile;
	}

	public override TerrainCoord GetMaxTile()
	{
		return MaxTile;
	}

	public int GetArea()
	{
		TerrainCoord terrainCoord = GetMinTile() - GetMinTile();
		return terrainCoord.x * terrainCoord.y;
	}

	public int GetOutsideEdgeLength()
	{
		TerrainCoord terrainCoord = GetMinTile() - new TerrainCoord(1, 1);
		TerrainCoord terrainCoord2 = GetMaxTile() + new TerrainCoord(1, 1);
		int num = terrainCoord2.x + 1 - terrainCoord.x;
		int num2 = terrainCoord2.y + 1 - terrainCoord.y;
		return num * 2 + (num2 - 2) * 2;
	}

	public override Matrix4x4 GetWorld()
	{
		return World;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref UniqueID);
	}

	public virtual void AddToTerrain()
	{
		if (!InTerrain)
		{
			InTerrain = true;
			GameTerrain.Instance.AddMultiTileObject(MinTile, MaxTile, this);
		}
	}

	public virtual void RemoveFromTerrain()
	{
		if (InTerrain)
		{
			GameTerrain.Instance.RemoveMultiTileObject(MinTile, MaxTile, this);
			InTerrain = false;
		}
	}

	public void SetBoundingBox(Bounds newBoundingBox)
	{
		BoundingBox = newBoundingBox;
		GameTerrain instance = GameTerrain.Instance;
		if (instance == null)
		{
			return;
		}
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(newBoundingBox.min);
		TerrainCoord tileCoordForPos2 = instance.GetTileCoordForPos(newBoundingBox.max);
		if (!InTerrain)
		{
			MinTile = tileCoordForPos;
			MaxTile = tileCoordForPos2;
		}
		else if (tileCoordForPos != MinTile || tileCoordForPos2 != MaxTile)
		{
			if (tileCoordForPos / 8 != MinTile / 8 || tileCoordForPos2 / 8 != MaxTile / 8)
			{
				instance.AddMultiTileObject(tileCoordForPos, tileCoordForPos2, this);
				instance.RemoveMultiTileObject(MinTile, MaxTile, this);
			}
			TerrainCoord minTile = MinTile;
			TerrainCoord maxTile = MaxTile;
			MinTile = tileCoordForPos;
			MaxTile = tileCoordForPos2;
			OnMinMaxTilesChanged(minTile, maxTile, tileCoordForPos, tileCoordForPos2);
		}
	}

	protected virtual void OnMinMaxTilesChanged(TerrainCoord oldMinTile, TerrainCoord oldMaxTile, TerrainCoord newMinTile, TerrainCoord newMaxTile)
	{
	}
}

using System.Linq;
using UnityEngine;

public abstract class SingleTileObject : TileObject
{
	public TerrainCoord Tile;

	public override Vector3 Pos
	{
		get
		{
			GameTerrain instance = GameTerrain.Instance;
			if (instance == null)
			{
				return Vector3.zero;
			}
			if (!instance.IsTileOutsideBounds(Tile.x, Tile.y) && instance.IsTilePit(Tile.x, Tile.y))
			{
				instance.GetOriginalTileCentrePos(Tile);
			}
			return instance.GetTileCentrePos(Tile, ignoreIce: true, ignoreRoadCamber: false);
		}
	}

	public override Vector2 PosXZ => GameTerrain.Instance.GetTileCentreXZ(Tile);

	public override TerrainCoord GetTile()
	{
		return Tile;
	}

	public override int GetTileX()
	{
		return Tile.x;
	}

	public override int GetTileY()
	{
		return Tile.y;
	}

	public override void SetTileX(int x)
	{
		SetTile(new TerrainCoord(x, Tile.y));
	}

	public override void SetTileY(int y)
	{
		SetTile(new TerrainCoord(Tile.x, y));
	}

	public override void Init()
	{
		base.Init();
		RegisterWithTerrain();
	}

	public override void Delete()
	{
		UnregisterWithTerrain();
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Tile);
	}

	protected virtual void RegisterWithTerrain()
	{
		GameTerrain.Instance.AddSingleTileObject(Tile.x, Tile.y, this);
		EquipmentPrototype grabbableEquipmentType = GetGrabbableEquipmentType();
		if (grabbableEquipmentType != null && EquipmentPrototype.MiningResources.Contains(grabbableEquipmentType))
		{
			GameTerrain.Instance.RockMapWho.AddToMapWho(this, Tile);
		}
	}

	protected virtual void UnregisterWithTerrain()
	{
		EquipmentPrototype grabbableEquipmentType = GetGrabbableEquipmentType();
		if (grabbableEquipmentType != null && EquipmentPrototype.MiningResources.Contains(grabbableEquipmentType))
		{
			GameTerrain.Instance.RockMapWho.RemoveFromMapWho(this, Tile);
		}
		GameTerrain.Instance.RemoveSingleTileObject(Tile.x, Tile.y, this);
	}

	public override Bounds GetBoundingBox()
	{
		return MathUtil.CreateBoundsCentreExtents(Pos + new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
	}

	public override TerrainCoord GetMinTile()
	{
		return Tile;
	}

	public override TerrainCoord GetMaxTile()
	{
		return Tile;
	}

	public override void SetTile(TerrainCoord tile)
	{
		if (GameTerrain.Instance.IsTileOutsideBounds(tile.x, tile.y))
		{
			Debug.LogWarning("SingleTileObject.SetTile: tile " + tile.x + "," + tile.y + " is outside terrain bounds");
			return;
		}
		bool num = IsUnityObjectActive();
		if (num)
		{
			UnityDeactivate();
		}
		UnityDelete();
		UnregisterWithTerrain();
		if (IsImpassableSingleTileProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
		Tile = tile;
		if (IsImpassableSingleTileProp())
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		RegisterWithTerrain();
		UnityInit();
		if (num)
		{
			UnityActivate();
		}
	}

	public override void SetTileGhost(TerrainCoord tile)
	{
		Tile = tile;
	}

	public virtual bool IsImpassableSingleTileProp()
	{
		return false;
	}

	public override bool IsVisibleInFogOfWar()
	{
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		return !GameTerrain.Instance.FogOfWar.IsTileVisible(Tile.x, Tile.y);
	}
}

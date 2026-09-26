public class TrapsSign : SingleTileProp
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Trap/Wooden Sign");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TrapsSign;
	}

	public override void Init()
	{
		base.Init();
		GameTerrain.Instance.TrapSignMapWho.AddToMapWho(this, Tile);
	}

	public override void Delete()
	{
		GameTerrain.Instance.TrapSignMapWho.RemoveFromMapWho(this, Tile);
		base.Delete();
	}

	public override void SetTile(TerrainCoord tile)
	{
		GameTerrain.Instance.TrapSignMapWho.OnMoved(this, Tile, tile);
		base.SetTile(tile);
	}
}

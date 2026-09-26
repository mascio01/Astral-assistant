public class InvisibleWall : Prop
{
	private TerrainCoord Min;

	private TerrainCoord Max;

	public override TerrainCoord ExtentsMin => Min;

	public override TerrainCoord ExtentsMax => Max;

	public static InvisibleWall Spawn(TerrainCoord tile, OrientationType orientation, TerrainCoord extentsMin, TerrainCoord extentsMax)
	{
		InvisibleWall invisibleWall = new InvisibleWall();
		invisibleWall.Min = extentsMin;
		invisibleWall.Max = extentsMax;
		invisibleWall.Tile = tile;
		invisibleWall.Orientation = orientation;
		invisibleWall.OnSpawn();
		return invisibleWall;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.InvisibleWall;
	}
}

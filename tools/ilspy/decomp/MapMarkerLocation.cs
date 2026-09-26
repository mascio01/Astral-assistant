public struct MapMarkerLocation : IReflectable
{
	public MapMarkerType Type;

	public TerrainCoord Tile;

	public MapMarkerLocation(MapMarkerType type, TerrainCoord tile)
	{
		Type = type;
		Tile = tile;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref Tile);
	}
}

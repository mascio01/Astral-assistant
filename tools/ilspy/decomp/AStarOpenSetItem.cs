public struct AStarOpenSetItem
{
	public float FScore;

	public float GScore;

	public CompressedTerrainCoord Tile;

	public AStarOpenSetItem(float fScore, float gScore, CompressedTerrainCoord tile)
	{
		FScore = fScore;
		GScore = gScore;
		Tile = tile;
	}
}

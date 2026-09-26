public struct MineralDeposit
{
	public TerrainCoord Centre;

	public int Area;

	public MineralType Type;

	public Community Community;

	public MineralDeposit(TerrainCoord centre, int area, MineralType mineralType, Community community)
	{
		Centre = centre;
		Area = area;
		Type = mineralType;
		Community = community;
	}
}

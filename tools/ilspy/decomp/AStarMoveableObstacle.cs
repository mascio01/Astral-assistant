using UnityEngine;

public class AStarMoveableObstacle
{
	public TileObject Object;

	public int CommunityId;

	public int OccupiedByCommunityId;

	public TerrainCoord MinTile;

	public TerrainCoord MaxTile;

	public Matrix4x4 PropWorldMatrix;

	public Bounds PropBoundingBox;

	public PrefabResource PropModel;

	public GateState GateState;

	public GatePolicy GatePolicy;

	public bool GateBlockAnimals;

	public bool CharacterIsStationary;

	public bool CharacterIsAwake;
}

using UnityEngine;

public class BridgeWall : Prop
{
	public int JunctionIndex = -1;

	public float YOffset;

	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/BridgeWall");

	public static BridgeWall Spawn(PropPrototype proto, TerrainCoord tile, OrientationType orientation, int junctionIndex)
	{
		BridgeWall bridgeWall = new BridgeWall();
		bridgeWall.Prototype = proto;
		bridgeWall.Tile = tile;
		bridgeWall.Orientation = orientation;
		bridgeWall.JunctionIndex = junctionIndex;
		bridgeWall.OnSpawn();
		return bridgeWall;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref JunctionIndex);
		reflector.AddAfter(ref YOffset, 225);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BridgeWall;
	}

	protected override Vector3 CalcWorldPos()
	{
		GameTerrain instance = GameTerrain.Instance;
		if (JunctionIndex != -1 && JunctionIndex < instance.Junctions.Count)
		{
			TerrainPathJunction terrainPathJunction = instance.Junctions[JunctionIndex];
			if (terrainPathJunction.PathIndex1 < instance.Paths.Count)
			{
				TerrainPathPoint point = instance.Paths[terrainPathJunction.PathIndex1].GetPoint(terrainPathJunction.PointIndex1);
				return MathUtil.ToXZY(GameTerrain.Instance.GetTileCentreXZ(Tile), point.Pos.y + YOffset);
			}
		}
		return base.CalcWorldPos() + new Vector3(0f, YOffset, 0f);
	}
}

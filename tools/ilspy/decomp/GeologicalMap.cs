using System;

public class GeologicalMap : Equipment
{
	public int MapQuadrant = 15;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GeologicalMap;
	}

	public override int GetMapQuadrant()
	{
		return MapQuadrant;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		MapQuadrant = 1 << MathUtil.RandomInt(Id, Math.Max(1, MathUtil.Squared(GameTerrain.Instance.Size >> 9)));
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 271)
		{
			MapQuadrant = 15;
		}
		else
		{
			reflector.Add(ref MapQuadrant);
		}
	}
}

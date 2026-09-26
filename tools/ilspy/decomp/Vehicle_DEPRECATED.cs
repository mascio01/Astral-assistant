using UnityEngine;

public abstract class Vehicle_DEPRECATED : TiltedProp
{
	public override TerrainCoord ExtentsMin => new TerrainCoord(-1, -2);

	public override TerrainCoord ExtentsMax => new TerrainCoord(0, 2);

	public override Vector3 ModelOffset => new Vector3(-0.5f, 0f, 0f);

	public override CoverType CoverType => CoverType.Full;

	public override float GetMaxInventoryWeight()
	{
		return 200f;
	}

	public override bool IsUnityObjectAlwaysActive()
	{
		return true;
	}
}

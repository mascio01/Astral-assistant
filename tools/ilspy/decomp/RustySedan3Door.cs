using UnityEngine;

internal class RustySedan3Door : Vehicle_DEPRECATED
{
	private static int Name = StringUtil.JenkinsHash("PROP_BurnedOutCar");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.9f, 1.35f);

	public override Vector3 ModelOffset => new Vector3(-0.5f, 0f, 0.5f);

	public override TerrainCoord ExtentsMin => new TerrainCoord(-1, -1);

	public override TerrainCoord ExtentsMax => new TerrainCoord(0, 2);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RustySedan3Door;
	}

	public override PrefabResource GetUnityModel()
	{
		return Hatchback.Model;
	}
}

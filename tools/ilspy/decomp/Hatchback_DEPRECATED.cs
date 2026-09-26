using UnityEngine;

internal class Hatchback_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Car");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.7f, 1.3f);

	public override Vector3 ModelOffset => new Vector3(-0.5f, 0f, 0.5f);

	public override TerrainCoord ExtentsMin => new TerrainCoord(-1, -1);

	public override TerrainCoord ExtentsMax => new TerrainCoord(0, 2);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Hatchback_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Hatchback.Model;
	}
}

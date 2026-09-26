using UnityEngine;

internal class RustySedan5Door : Vehicle_DEPRECATED
{
	private static int Name = StringUtil.JenkinsHash("PROP_BurnedOutCar");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(1f, 1.6f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RustySedan5Door;
	}

	public override PrefabResource GetUnityModel()
	{
		return Hatchback.Model;
	}
}

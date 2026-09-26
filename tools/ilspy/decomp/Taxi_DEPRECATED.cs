using UnityEngine;

internal class Taxi_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Taxi");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.8f, 1.4f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Taxi_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Taxi.Model;
	}
}

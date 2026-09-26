using UnityEngine;

internal class Pickup2_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Pickup");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(1f, 1.7f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Pickup2_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Pickup2.Model;
	}
}

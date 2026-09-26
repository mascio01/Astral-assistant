using UnityEngine;

internal class HumVee_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_MilitaryVehicle");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(1.1f, 1.6f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.HumVee_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return HumVee.Model;
	}
}

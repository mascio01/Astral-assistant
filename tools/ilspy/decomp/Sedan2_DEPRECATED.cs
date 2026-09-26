using UnityEngine;

internal class Sedan2_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Car");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.8f, 1.4f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Sedan2_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Sedan2.Model;
	}
}

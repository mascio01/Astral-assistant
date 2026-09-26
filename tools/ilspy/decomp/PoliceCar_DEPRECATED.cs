using UnityEngine;

internal class PoliceCar_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_PoliceCar");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.9f, 1.4f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PoliceCar_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return PoliceCar.Model;
	}
}

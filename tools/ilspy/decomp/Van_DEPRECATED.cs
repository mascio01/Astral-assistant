using UnityEngine;

internal class Van_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Van");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(0.9f, 1.5f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Van_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Van.Model;
	}
}

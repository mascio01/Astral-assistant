using UnityEngine;

internal class Ambulance_DEPRECATED : Vehicle_DEPRECATED
{
	public static int Name = StringUtil.JenkinsHash("PROP_Ambulance");

	public override int NameHash => Name;

	public override Vector3 ModelOffset => new Vector3(-0.5f, 0f, -0.5f);

	public override Vector2 WheelPos => new Vector2(0.9f, 1.6f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Ambulance_DEPRECATED;
	}

	public override PrefabResource GetUnityModel()
	{
		return Ambulance.Model;
	}
}

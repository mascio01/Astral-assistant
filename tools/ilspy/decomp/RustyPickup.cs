using UnityEngine;

internal class RustyPickup : Vehicle_DEPRECATED
{
	private static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\RuinedCar");

	private static int Name = StringUtil.JenkinsHash("PROP_BurnedOutPickup");

	public override int NameHash => Name;

	public override Vector2 WheelPos => new Vector2(1f, 1.7f);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RustyPickup;
	}

	public override PrefabResource GetUnityModel()
	{
		return Pickup.Model;
	}
}

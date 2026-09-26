public class VendingMachineOverturned : TiltedProp
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropDrinkVendingMachineOverturned"),
		new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropDrinkVendingMachineOverturned2")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.VendingMachineOverturned;
	}
}

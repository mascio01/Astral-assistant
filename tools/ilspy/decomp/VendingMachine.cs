public class VendingMachine : Prop
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropDrinkVendingMachine"),
		new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropDrinkVendingMachine2")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.VendingMachine;
	}
}
